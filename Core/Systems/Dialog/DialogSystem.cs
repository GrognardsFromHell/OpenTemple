using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using SharpDX.DirectWrite;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.IO;
using SpicyTemple.Core.Logging;
using SpicyTemple.Core.Systems.AI;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.Systems.D20.Conditions;
using SpicyTemple.Core.Systems.ObjScript;
using SpicyTemple.Core.Systems.Script.Extensions;
using SpicyTemple.Core.Systems.TimeEvents;
using SpicyTemple.Core.TigSubsystems;

namespace SpicyTemple.Core.Systems.Dialog
{
    public class DialogSystem : IGameSystem
    {
        private static readonly ILogger Logger = new ConsoleLogger();

        private readonly DialogScripts _scripts = new DialogScripts();

        private readonly Dictionary<int, PlayerVoiceSet> _playerVoiceSetsByKey = new Dictionary<int, PlayerVoiceSet>();

        private GeneratedDialog _generatedDialog;

        [TempleDllLocation(0x108EC860)]
        private readonly List<PlayerVoiceSet> _playerVoiceSets = new List<PlayerVoiceSet>();

        [TempleDllLocation(0x10036040)]
        public DialogSystem()
        {
            _generatedDialog = new GeneratedDialog();

            LoadPlayerVoices();
        }

        [TempleDllLocation(0x10035660)]
        private void LoadPlayerVoices()
        {
            var playerVoiceFiles = Tig.FS.ReadMesFile("rules/pcvoice.mes");
            var playerVoiceNames = Tig.FS.ReadMesFile("mes/pcvoice.mes");

            foreach (var (key, filename) in playerVoiceFiles)
            {
                var name = playerVoiceNames[key];
                var voiceSet = new PlayerVoiceSet(key, name, filename);
                _playerVoiceSetsByKey[key] = voiceSet;
                _playerVoiceSets.Add(voiceSet);
            }
        }

        public void Dispose()
        {
        }

        [TempleDllLocation(0x10038470)]
        public void OnAfterTeleport(int targetMapId)
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x100372d0)]
        private void GetNpcVoiceLine(GameObjectBody speaker, GameObjectBody listener,
            out string text, out int soundId, int generatedLineFrom, int generatedLineTo, int dialogScriptLineId)
        {
            if (!GameSystems.AI.CanTalkTo(speaker, listener))
            {
                text = null;
                soundId = -1;
            }
            else
            {
                var dialogState = new DialogState(speaker, listener);

                sub_10036E00(out text, dialogScriptLineId, dialogState, generatedLineFrom, generatedLineTo);
                soundId = dialogState.speechId;
            }
        }

        [TempleDllLocation(0x10036e00)]
        private void sub_10036E00(out string textOut, int dialogScriptLine, DialogState state,
            int generatedLineFrom, int generatedLineTo)
        {
            if (!GetDialogScriptLine(state, out textOut, dialogScriptLine))
            {
                var line = _generatedDialog.GetNpcLine(state.npc, state.pc, generatedLineFrom, generatedLineTo);
                if (line != null)
                {
                    textOut = ResolveLineTokens(state, line);
                }

                state.speechId = -1;
            }
        }

        private bool UseFemaleResponseFor(GameObjectBody listener)
        {
            if (listener.IsCritter())
            {
                return listener.GetStat(Stat.gender) == (int) Gender.Female;
            }

            return false;
        }

        [TempleDllLocation(0x100369e0)]
        private bool GetDialogScriptLine(DialogState state, out string lineText, int lineNumber)
        {
            lineText = null;

            var script = state.npc.GetScript(obj_f.scripts_idx, (int) ObjScriptEvent.Dialog);

            if (script.scriptId == 0)
            {
                return false; // NPC has no associated dialog script
            }

            var dialogScript = _scripts.Get(script.scriptId);
            if (dialogScript == null)
            {
                return false;
            }

            if (!dialogScript.Lines.TryGetValue(lineNumber, out var dialogLine))
            {
                return false;
            }

            if (UseFemaleResponseFor(state.pc))
            {
                lineText = dialogLine.genderField;
            }
            else
            {
                lineText = dialogLine.txt;
            }

            lineText = ResolveLineTokens(state, lineText);
            state.speechId = ((script.scriptId & 0x7FFF) << 16) | (dialogLine.key & 0xFFFF);

            return true;
        }

        [TempleDllLocation(0x10034c20)]
        internal string ResolveLineTokens(DialogState state, string lineText)
        {
            if (!lineText.Contains('@'))
            {
                return lineText;
            }

            var result = new StringBuilder(lineText.Length);
            ReadOnlySpan<char> chars = lineText;

            for (var i = 0; i < chars.Length; i++)
            {
                if (chars[i] == '@')
                {
                    var rest = chars.Slice(i);
                    if (rest.StartsWith("@pcname@"))
                    {
                        result.Append(GameSystems.MapObject.GetDisplayName(state.pc));
                        continue;
                    }

                    if (rest.StartsWith("@npcname@"))
                    {
                        result.Append(GameSystems.MapObject.GetDisplayName(state.npc));
                        continue;
                    }
                }

                result.Append(chars[i]);
            }

            return result.ToString();
        }

        /// <summary>
        /// fetches a PC who is not identical to the object. For NPCs this will try to fetch their leader first.
        /// </summary>
        [TempleDllLocation(0x10034A40)]
        public GameObjectBody GetListeningPartyMember(GameObjectBody critter)
        {
            if (critter.IsNPC())
            {
                var leader = GameSystems.Critter.GetLeader(critter);
                if (leader != null)
                {
                    return leader;
                }
            }

            foreach (var otherPlayer in GameSystems.Party.PlayerCharacters)
            {
                if (otherPlayer != critter)
                {
                    return otherPlayer;
                }
            }

            return null;
        }

        [TempleDllLocation(0x100348c0)]
        private void GetPcVoiceLine(GameObjectBody speaker, GameObjectBody listener, out string text, out int soundId,
            PlayerVoiceLine line)
        {
            var v9 = speaker.GetInt32(obj_f.pc_voice_idx);

            if (!_playerVoiceSetsByKey.TryGetValue(v9, out var voiceSet))
            {
                text = null;
                soundId = -1;
                return;
            }

            voiceSet.PickLine(line, out text, out soundId);
        }

        [TempleDllLocation(0x100373c0)]
        public bool TryGetOkayVoiceLine(GameObjectBody speaker, GameObjectBody listener,
            out string text, out int soundId)
        {
            if (speaker.IsNPC())
            {
                GetNpcVoiceLine(speaker, listener, out text, out soundId, 2100, 2199, 12020);
            }
            else
            {
                GetPcVoiceLine(speaker, listener, out text, out soundId,
                    PlayerVoiceLine.Acknowledge);
            }

            return soundId != -1 || text != null;
        }

        [TempleDllLocation(0x10037c60)]
        public void GetDyingVoiceLine(GameObjectBody speaker, GameObjectBody listener, out string text, out int soundId)
        {
            if (speaker.IsNPC())
            {
                GetNpcVoiceLine(speaker, listener, out text, out soundId, 1500, 1599, 12014);
            }
            else
            {
                GetPcVoiceLine(speaker, listener, out text, out soundId, PlayerVoiceLine.DeathCry);
            }
        }

        [TempleDllLocation(0x10037e40)]
        public void GetLeaderDyingVoiceLine(GameObjectBody speaker, GameObjectBody listener, out string text,
            out int soundId)
        {
            text = null;
            soundId = -1;

            if (speaker.IsNPC())
            {
                GetNpcVoiceLine(speaker, listener, out text, out soundId, 3500, 3599, 12056);
            }
        }

        [TempleDllLocation(0x10037450)]
        public void GetFriendlyFireVoiceLine(GameObjectBody speaker, GameObjectBody listener, out string text,
            out int soundId)
        {
            if (speaker.IsNPC())
            {
                GetNpcVoiceLine(speaker, listener, out text, out soundId, 2300, 2399, 12022);
            }
            else
            {
                GetPcVoiceLine(speaker, listener, out text, out soundId,
                    PlayerVoiceLine.TakingFriendlyFire);
            }
        }

        [TempleDllLocation(0x10037be0)]
        public bool TryGetWontSellVoiceLine(GameObjectBody speaker, GameObjectBody listener, out string text,
            out int soundId)
        {
            GetNpcVoiceLine(speaker, listener, out text, out soundId, 1200, 1299, 12008);
            return text != null || soundId != -1;
        }

        [TempleDllLocation(0x10036120)]
        public bool PlayCritterVoiceLine(GameObjectBody obj, GameObjectBody objAddressed, string text, int soundId)
        {
            Stub.TODO();
            return false;
        }

        [TempleDllLocation(0x100374e0)]
        public void GetFleeVoiceLine(GameObjectBody speaker, GameObjectBody listener, out string text, out int soundId)
        {
            text = null;
            soundId = -1;
            if (speaker.IsNPC())
            {
                GetNpcVoiceLine(speaker, listener, out text, out soundId, 3000, 3099, 12029);
            }
        }

        [TempleDllLocation(0x10037eb0)]
        public void PlayTreasureLootingVoiceLine()
        {
            // Count how many followers would have a response to the treasure
            var followersWithLine = new List<GameObjectBody>();
            foreach (var npcFollower in GameSystems.Party.NPCFollowers)
            {
                var listener = GetListeningPartyMember(npcFollower);
                GetNpcVoiceLine(npcFollower, listener, out var text,
                    out var soundId, 3800, 3899, 12059);
                if (!string.IsNullOrEmpty(text))
                {
                    followersWithLine.Add(npcFollower);
                }
            }

            // If no followers have a response, we use a party member as the speaker
            // but we do prioritize the NPC followers
            GameObjectBody speaker;
            if (followersWithLine.Count == 0)
            {
                speaker = GameSystems.Random.PickRandom(new List<GameObjectBody>(GameSystems.Party.PlayerCharacters));
            }
            else
            {
                speaker = GameSystems.Random.PickRandom(followersWithLine);
            }

            if (speaker != null)
            {
                var listener = GetListeningPartyMember(speaker);
                string text;
                int soundId;
                if (speaker.IsNPC())
                {
                    GetNpcVoiceLine(speaker, listener, out text, out soundId, 3800, 3899, 12059);
                }
                else
                {
                    GetPcVoiceLine(speaker, listener, out text, out soundId,
                        PlayerVoiceLine.FoundLotsOfGold);
                }

                PlayCritterVoiceLine(speaker, listener, text, soundId);
            }
        }

        [TempleDllLocation(0x10037cf0)]
        public void GetOverburdenedVoiceLine(GameObjectBody speaker, GameObjectBody listener, out string text,
            out int soundId)
        {
            if (speaker.IsNPC())
            {
                GetNpcVoiceLine(speaker, listener, out text, out soundId, 3400, 3499, 12055);
            }
            else
            {
                GetPcVoiceLine(speaker, listener, out text, out soundId, PlayerVoiceLine.Encumbered);
            }
        }

        [TempleDllLocation(0x10037d80)]
        public void PlayOverburdenedVoiceLine(GameObjectBody critter)
        {
            if (GameSystems.Party.IsInParty(critter))
            {
                var listener = GetListeningPartyMember(critter);
                GetOverburdenedVoiceLine(critter, listener, out var text, out var soundId);
                PlayCritterVoiceLine(critter, listener, text, soundId);
                QueueComplainAgainTimer(critter);
            }
        }

        [TempleDllLocation(0x100355d0)]
        private void QueueComplainAgainTimer(GameObjectBody critter)
        {
            GameSystems.TimeEvent.Remove(TimeEventType.EncumberedComplain, evt => evt.arg1.handle == critter);
            if (critter.IsNPC())
            {
                var evt = new TimeEvent(TimeEventType.EncumberedComplain);
                evt.arg1.handle = critter;
                var delay = GameSystems.Random.GetInt(300000, 600000);
                GameSystems.TimeEvent.Schedule(evt, delay, out _);
            }
        }

        public bool TryLoadDialog(int scriptId, out DialogScript dialogScript)
        {
            return _scripts.TryGet(scriptId, out dialogScript);
        }

        [TempleDllLocation(0x108ed0c0)] [TempleDllLocation(0x108ec878)]
        private readonly List<ObjectId> _savedPartySelection = new List<ObjectId>();

        [TempleDllLocation(0x10038770)]
        public bool BeginDialog(DialogState state)
        {
            if (GameSystems.AI.GetCannotTalkReason(state.npc, state.pc) != AiSystem.CannotTalkCause.None)
            {
                return false;
            }

            GameSystems.Reaction.DialogReaction_10053FE0(state.npc, state.pc);
            GameSystems.Combat.CritterLeaveCombat(GameSystems.Party.GetConsciousLeader());
            if (GameSystems.Party.IsInParty(state.pc))
            {
                _savedPartySelection.Clear();
                foreach (var selectedCritter in GameSystems.Party.Selected)
                {
                    _savedPartySelection.Add(selectedCritter.id);
                }

                GameSystems.Party.ClearSelection();
                GameSystems.Party.AddToSelection(state.pc);
                GameSystems.Scroll.CenterOnSmooth(state.npc);
            }

            state.lineNumber = state.reqNpcLineId;
            state.actionType = 0;
            DialogGetLines(false, state);
            return true;
        }

        [TempleDllLocation(0x100385f0)]
        public void DialogGetLines(bool reuseRngSeed, DialogState state)
        {
            state.pcLineText = Array.Empty<string>();
            state.pcLineSkillUse = Array.Empty<int>();

            if (reuseRngSeed)
            {
                GameSystems.Random.SetSeed(state.rngSeed);
            }
            else
            {
                state.rngSeed = GameSystems.Random.SetRandomSeed();
            }

            if (DialogGetNpcLine(state, reuseRngSeed))
            {
                Span<int> answerKeys = stackalloc int[20];
                var answerCount = DialogGetPossibleAnswerLineIds(state, answerKeys);
                if (answerCount == 0)
                {
                    state.pcLineText = new string[1];
                    state.pcReplyOpcode = new int[1];
                    state.pcLineSkillUse = new int[1];
                    state.npcReplyIds = new int[1];
                    state.effectFields = new string[1];

                    state.pcLineText[0] = _generatedDialog.GetPcLine(state, 400, 499);
                    state.pcReplyOpcode[0] = 1;
                }
                else
                {
                    state.pcLineText = new string[answerCount];
                    state.pcReplyOpcode = new int[answerCount];
                    state.pcLineSkillUse = new int[answerCount];
                    state.npcReplyIds = new int[answerCount];
                    state.effectFields = new string[answerCount];
                    for (var i = 0; i < answerCount; i++)
                    {
                        DialogGetPcReplyLine(answerKeys[i], i, state);
                    }
                }
            }
        }

        [TempleDllLocation(0x100357d0)]
        private int DialogGetPossibleAnswerLineIds(DialogState state, Span<int> answers)
        {
            state.pcLineSkillUse = Array.Empty<int>();

            var intScore = state.pc.GetStat(Stat.intelligence);
            if (intScore > 7 && GameSystems.Critter.CritterIsLowInt(state.pc))
            {
                intScore = 1;
            }

            if (!state.dialogScript.Lines.TryGetValue(state.lineNumber, out var line))
            {
                return 0;
            }

            int answerCount = 0;
            int nextLineKey = line.nextResponseKey;
            while (nextLineKey != -1)
            {
                var responseLine = state.dialogScript.Lines[nextLineKey];
                nextLineKey = responseLine.nextResponseKey;
                answerCount++;

                if (!SatisfiesGenderRequirement(state.pc, ref responseLine))
                {
                    continue;
                }

                if (!SatisfiesIntRequirement(intScore, responseLine.minIq))
                {
                    continue;
                }

                if (!SatisfiesPrecondition(state, ref responseLine))
                {
                    continue;
                }

                answers[answerCount++] = responseLine.key;
            }

            if (answerCount > 5)
            {
                Debugger.Break();
            }

            return answerCount;
        }

        [TempleDllLocation(0x10035960)]
        private void DialogGetPcReplyLine(int answerLineId, int responseIdx, DialogState state)
        {
            var line = state.dialogScript.Lines[answerLineId];

            DialogGetOpcodeAndAnswer2(line.answerLineId,
                out state.npcReplyIds[responseIdx],
                out state.pcReplyOpcode[responseIdx]
            );

            if (line.txt.Equals("a:", StringComparison.OrdinalIgnoreCase))
            {
                state.pcLineText[responseIdx] = _generatedDialog.GetPcClassBasedLine(1000, state);
            }
            else if (line.txt.StartsWith("b:", StringComparison.OrdinalIgnoreCase))
            {
                var restOfLine = line.txt.Substring(2);
                if (restOfLine.Length > 0)
                {
                    state.pcLineText[responseIdx] = restOfLine;
                }
                else
                {
                    state.pcLineText[responseIdx] = _generatedDialog.GetPcLine(state, 300, 399);
                }

                // This always returned zero in vanilla, but what is opcode 26?
                if (false)
                {
                    state.pcReplyOpcode[responseIdx] = 26;
                    state.npcReplyIds[responseIdx] = line.answerLineId;
                }
                else
                {
                    state.pcReplyOpcode[responseIdx] = 3;
                    state.npcReplyIds[responseIdx] = line.answerLineId;
                }
            }
            else if (line.txt.StartsWith("c:", StringComparison.OrdinalIgnoreCase))
            {
                var storyState = GameSystems.Script.StoryState;
                state.pcLineText[responseIdx] = _generatedDialog.GetPcLine(state, 1700 + storyState, 1700 + storyState);
                state.pcReplyOpcode[responseIdx] = 24;
                state.npcReplyIds[responseIdx] = line.answerLineId;
            }
            else if (line.txt.Equals("e:", StringComparison.OrdinalIgnoreCase))
            {
                state.pcLineText[responseIdx] = _generatedDialog.GetPcLine(state, 400, 499);
            }
            else if (line.txt.Equals("f:", StringComparison.OrdinalIgnoreCase))
            {
                state.pcLineText[responseIdx] = _generatedDialog.GetPcLine(state, 800, 899);
            }
            else if (line.txt.Equals("k:", StringComparison.OrdinalIgnoreCase))
            {
                state.pcLineText[responseIdx] = _generatedDialog.GetPcLine(state, 1500, 1599);
            }
            else if (line.txt.Equals("n:", StringComparison.OrdinalIgnoreCase))
            {
                state.pcLineText[responseIdx] = _generatedDialog.GetPcLine(state, 100, 199);
            }
            else if (line.txt.StartsWith("q:", StringComparison.OrdinalIgnoreCase))
            {
                throw new NotSupportedException("q: was only implemented in Arkanum, not in ToEE.");
            }
            else if (line.txt.StartsWith("r:", StringComparison.OrdinalIgnoreCase))
            {
                state.pcLineText[responseIdx] = _generatedDialog.GetPcClassBasedLine(2000, state);
                state.pcReplyOpcode[responseIdx] = 8;
                state.npcReplyIds[responseIdx] = line.answerLineId;
            }
            else if (line.txt.Equals("s:", StringComparison.OrdinalIgnoreCase))
            {
                state.pcLineText[responseIdx] = _generatedDialog.GetPcLine(state, 200, 299);
            }
            else if (line.txt.Equals("y:", StringComparison.OrdinalIgnoreCase))
            {
                state.pcLineText[responseIdx] = _generatedDialog.GetPcLine(state, 1, 99);
            }
            else
            {
                state.pcLineText[responseIdx] = ResolveLineTokens(state, line.txt);
            }

            state.effectFields[responseIdx] = line.effectField;

            // This is a debug option
            if (Globals.Config.ShowDialogLineIds)
            {
                state.pcLineText[responseIdx] = $"[{line.key}] " + state.pcLineText[responseIdx];
            }
        }

        [TempleDllLocation(0x10034dc0)]
        private void DialogGetOpcodeAndAnswer2(int answerLineId, out int answerId2, out int opCode)
        {
            if (answerLineId < 0)
            {
                opCode = 2;
                answerId2 = -answerLineId;
            }
            else if (answerLineId > 0)
            {
                opCode = 0;
                answerId2 = answerLineId;
            }
            else
            {
                answerId2 = 0;
                opCode = 1;
            }
        }

        private bool SatisfiesGenderRequirement(GameObjectBody pc, ref DialogLine line)
        {
            // TODO Check required gender for response lines
            // -1 = any gender
            throw new NotImplementedException();
        }

        private bool SatisfiesIntRequirement(int pcIntScore, int lineRequirement)
        {
            if (lineRequirement == 0)
            {
                return true;
            }

            // Negative int requirements are maximums
            if (lineRequirement < 0)
            {
                return pcIntScore >= -lineRequirement;
            }

            // Positives are minimums
            return pcIntScore >= lineRequirement;
        }

        [TempleDllLocation(0x10034d50)]
        private bool SatisfiesPrecondition(DialogState state, ref DialogLine responseLine)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10038240)]
        public bool DialogGetNpcLine(DialogState state, bool rngSeed)
        {
            if (!state.dialogScript.Lines.TryGetValue(state.lineNumber, out var line))
            {
                state.npcLineText = " ";
                state.speechId = -1;
                return false;
            }

            int dlgScriptId = state.dialogScriptId;
            state.answerLineId = line.answerLineId;

            int line_and_script_packed;
            if (dlgScriptId != 0)
            {
                line_and_script_packed = ((dlgScriptId & 0x7FFF) << 16) | (line.key & 0xFFFF);
            }
            else
            {
                line_and_script_packed = -1;
            }

            state.speechId = line_and_script_packed;
            if (!rngSeed && line.effectField != null)
            {
                RunDialogAction /*0x100ae7a0*/(line.effectField, state, 0);
            }

            if (line.txt.StartsWith("g:", StringComparison.OrdinalIgnoreCase))
            {
                dlg_generic_greeting(state);
                if (state.actionType == 3)
                {
                    state.pcLineText = Array.Empty<string>();
                    state.pcReplyOpcode = Array.Empty<int>();
                    state.pcLineSkillUse = Array.Empty<int>();
                    state.npcReplyIds = Array.Empty<int>();
                    state.effectFields = Array.Empty<string>();
                    return false;
                }

                return true;
            }

            if (line.txt.StartsWith("m:", StringComparison.OrdinalIgnoreCase))
            {
                throw new NotSupportedException("m: is no longer supported since it was not used in ToEE");
            }

            if (line.txt.StartsWith("r:", StringComparison.OrdinalIgnoreCase))
            {
                DialogRumor_100370F0(line.answerLineId, 5, state);
                return false;
            }

            string lineTemplate;
            if (state.pc.GetGender() != Gender.Male)
            {
                lineTemplate = line.genderField;
            }
            else
            {
                lineTemplate = line.txt;
            }

            state.npcLineText = ResolveLineTokens(state, lineTemplate);
            return true;
        }

        [TempleDllLocation(0x10037550)]
        private void dlg_generic_greeting(DialogState state,
            bool disableFearsomeResponse = false,
            bool disableInvisibleResponse = false,
            bool disablePolymorphedResponse = false,
            bool disableMirrorImageResponse = false,
            bool disableNakedResponse = false,
            bool disableBarbarianArmorResponse = false,
            bool disableReputationResponse = false
        )
        {
            if (GameSystems.AI.HasSurrendered(state.npc, out _))
            {
                state.npcLineText = "";
                state.speechId = -1;

                if (state.npc.IsNPC())
                {
                    GameSystems.Dialog.GetNpcVoiceLine(state.npc, state.pc, out var text, out var speechId, 3000,
                        3099, 12029);
                    state.npcLineText = text;
                    state.speechId = speechId;
                }

                return;
            }

            if (!disableFearsomeResponse && GameSystems.Critter.HasFearsomeAssociates(state.pc))
            {
                state.npcLineText = GetClassBasedNpcLine(state, 18000);
                if (GameSystems.Critter.GetLeaderRecursive(state.npc) == null)
                {
                    state.actionType = 3;
                }

                return;
            }

            if (!disableInvisibleResponse &&
                GameSystems.D20.D20Query(state.pc, D20DispatcherKey.QUE_Critter_Is_Invisible))
            {
                state.npcLineText = GetClassBasedNpcLine(state, 22000);
                if (GameSystems.Critter.GetLeaderRecursive(state.npc) == null)
                {
                    state.actionType = 3;
                }

                return;
            }

            if (!disablePolymorphedResponse && GameSystems.D20.D20Query(state.pc, D20DispatcherKey.QUE_Polymorphed))
            {
                state.npcLineText = GetClassBasedNpcLine(state, 20000);
                if (GameSystems.Critter.GetLeaderRecursive(state.npc) == null)
                {
                    state.actionType = 3;
                }

                return;
            }

            if (!disableMirrorImageResponse && state.pc.HasCondition(SpellEffects.SpellMirrorImage))
            {
                state.npcLineText = GetClassBasedNpcLine(state, 21000);
                return;
            }

            var armor = GameSystems.Item.ItemWornAt(state.pc, EquipSlot.Armor);
            var robes = GameSystems.Item.ItemWornAt(state.pc, EquipSlot.Robes);
            if (!disableNakedResponse && armor == null && robes == null)
            {
                state.npcLineText = GetClassBasedNpcLine(state, 16000);
                return;
            }

            if (!disableBarbarianArmorResponse && armor != null && state.pc.GetStat(Stat.level_barbarian) > 0 &&
                armor.ProtoId == 6055)
            {
                state.npcLineText = GetClassBasedNpcLine(state, 17000);
                return;
            }

            // 20% chance that the NPC will remark on one of the player's reputations if it's affecting the NPC's
            // reaction towards the player positively.
            if (!disableReputationResponse
                && GameSystems.Random.GetInt(1, 100) <= 20
                && GameSystems.Reputation.TryGetReputationAffectingReaction(state.pc, state.npc,
                    out var reputationId)
                && GameSystems.Reputation.TryGetGreeting(state.pc, state.npc, reputationId, out var repGreeting)
            )
            {
                state.npcLineText = repGreeting;
                state.speechId = -1;
                return;
            }

            var reaction = GameSystems.Reaction.GetReaction(state.npc, state.pc);
            var reactionLevel = GameSystems.Reaction.GetReactionLevel(reaction);
            if (GameSystems.Reaction.HasMet(state.npc, state.pc))
            {
                switch (reactionLevel)
                {
                    // Further "good" greeting
                    case 0:
                    case 1:
                    case 2:
                        state.npcLineText = GetClassBasedNpcLine(state, 12000);
                        break;
                    // Further neutral greeting
                    case 3:
                        state.npcLineText = GetClassBasedNpcLine(state, 13000);
                        break;
                    case 4:
                    case 5:
                        state.npcLineText = GetRaceBasedNpcLine(state, 4000);
                        break;
                    case 6:
                        state.npcLineText = GetRaceBasedNpcLine(state, 5000);
                        break;
                    default:
                        return;
                }
            }
            else
            {
                switch (reactionLevel)
                {
                    case 0:
                    case 1:
                    case 2:
                        state.npcLineText = GetClassBasedNpcLine(state, 9000);
                        break;
                    case 3:
                        state.npcLineText = GetClassBasedNpcLine(state, 10000);
                        break;
                    case 4:
                    case 5:
                        state.npcLineText = GetRaceBasedNpcLine(state, 2000);
                        break;
                    case 6:
                        state.npcLineText = GetRaceBasedNpcLine(state, 3000);
                        break;
                    default:
                        return;
                }
            }
        }

        private void RunDialogAction(string lineEffectField, DialogState state, int i)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x100370f0)]
        public void DialogRumor_100370F0(int answerLineId, int a2, DialogState state)
        {
            DialogGetOpcodeAndAnswer2(answerLineId, out var npcReplyId, out var pcOpCode);

            if (!GameSystems.Rumor.TryFindRumor(state.pc, state.npc, out var rumorId))
            {
                state.npcLineText = GetClassBasedNpcLine(state, 7000);

                state.pcLineText = new string[1];
                state.pcReplyOpcode = new int[1];
                state.pcLineSkillUse = new int[1];
                state.npcReplyIds = new int[1];
                state.effectFields = new string[1];

                state.pcLineText[0] = _generatedDialog.GetPcLine(state, 600, 699);
                state.pcReplyOpcode[0] = pcOpCode;
                state.npcReplyIds[0] = npcReplyId;
                state.effectFields[0] = null;
            }
            else if (a2 <= 0)
            {
                sub_10035300(state, rumorId, pcOpCode, npcReplyId);
            }
            else
            {
                sub_10036FB0(a2, state, 9, rumorId, pcOpCode, npcReplyId);
            }
        }

        [TempleDllLocation(0x10035300)]
        public void sub_10035300(DialogState state, int rumorId, int pcOpCode, int npcReplyId)
        {
            GameSystems.Rumor.TryGetRumorNpcLine(state.pc, state.npc, rumorId, out var rumorText);
            state.npcLineText = GameSystems.Dialog.ResolveLineTokens(state, rumorText);

            state.speechId = -1;
            GameSystems.Rumor.Add(state.pc, rumorId);

            state.pcLineText = new string[1];
            state.pcReplyOpcode = new int[1];
            state.pcLineSkillUse = new int[1];
            state.npcReplyIds = new int[1];
            state.effectFields = new string[1];

            state.pcLineText[0] = _generatedDialog.GetPcClassBasedLine(1000, state);
            state.pcReplyOpcode[0] = pcOpCode;
            state.npcReplyIds[0] = npcReplyId;
            state.effectFields[0] = null;
        }

        [TempleDllLocation(0x10036fb0)]
        private void sub_10036FB0(int a1, DialogState state, int a3, int rumorId, int pcOpCode, int npcReplyId)
        {
            var rumorNpcReplyId = GameSystems.Reaction.sub_100541C0(state.npc, state.pc, a1);
            if (rumorNpcReplyId == 1)
            {
                rumorNpcReplyId = 2;
            }

            state.npcLineText = GetClassBasedNpcLine(state, 1000);

            state.pcLineText = new string[2];
            state.pcReplyOpcode = new int[2];
            state.pcLineSkillUse = new int[2];
            state.npcReplyIds = new int[2];
            state.effectFields = new string[2];

            state.pcLineText[0] = _generatedDialog.GetPcLine(state, 1, 99);
            state.npcReplyIds[0] = rumorNpcReplyId;
            state.pcReplyOpcode[0] = 4;
            state.effectFields[0] = null;

            state.pcLineText[1] = _generatedDialog.GetPcLine(state, 100, 199);
            state.pcReplyOpcode[1] = pcOpCode;
            state.effectFields[1] = null;
            state.npcReplyIds[1] = npcReplyId;

            // TODO: Check if the opcode used above makes use of these "hidden" fields
            // TODO state.pcReplyOpcode[2] = a3;
            // TODO state.npcReplyIds[2] = rumorId;
        }

        [TempleDllLocation(0x10036b20)]
        private string GetClassBasedNpcLine(DialogState state, int baseLine)
        {
            // First attempt to get an NPC specific line, before consulting the generic lines
            var overrideLineKey = 9999 + baseLine / 1000;
            if (GetDialogScriptLine(state, out var lineText, overrideLineKey))
            {
                return lineText;
            }

            lineText = _generatedDialog.GetNpcClassBasedLine(state.npc, state.pc, baseLine);
            lineText = ResolveLineTokens(state, lineText);
            state.speechId = -1;
            return lineText;
        }

        [TempleDllLocation(0x10036c80)]
        private string GetRaceBasedNpcLine(DialogState state, int baseLine)
        {
            // First attempt to get an NPC specific line, before consulting the generic lines
            var overrideLineKey = 10999 + baseLine / 1000;
            if (GetDialogScriptLine(state, out var lineText, overrideLineKey))
            {
                return lineText;
            }

            lineText = _generatedDialog.GetNpcRaceBasedLine(state.npc, state.pc, baseLine);
            lineText = ResolveLineTokens(state, lineText);
            state.speechId = -1;
            return lineText;
        }

        [TempleDllLocation(0x10034710)]
        public void EndDialog(DialogState state)
        {
            GameSystems.Reaction.NpcReactionUpdate(state.npc, state.pc);

            GameSystems.Party.ClearSelection();
            foreach (var selectedMember in _savedPartySelection)
            {
                var handle = GameSystems.Object.GetObject(selectedMember);
                if (handle != null)
                {
                    GameSystems.Party.AddToSelection(handle);
                }
            }

            _savedPartySelection.Clear();
        }

        [TempleDllLocation(0x100346f0)]
        public void Free(ref DialogScript dialogScript)
        {
            dialogScript = null;
        }

        [TempleDllLocation(0x10037af0)]
        public string Dialog_10037AF0(GameObjectBody npc, GameObjectBody pc)
        {
            if (GameSystems.AI.GetCannotTalkReason(npc, pc) != AiSystem.CannotTalkCause.None)
            {
                return null;
            }
            else
            {
                var dlgState = new DialogState(npc, pc);
                if (GameSystems.Party.IsInParty(npc))
                {
                    dlg_generic_greeting(
                        dlgState,
                        disableFearsomeResponse: true,
                        disableInvisibleResponse: true,
                        disablePolymorphedResponse: true,
                        disableMirrorImageResponse: true,
                        disableNakedResponse: true,
                        disableBarbarianArmorResponse: true
                    );
                }
                else
                {
                    dlg_generic_greeting(dlgState);
                }

                return dlgState.npcLineText;
            }
        }

        // Probably the stream id of the currently playing sound
        [TempleDllLocation(0x108ec85c)]
        private int sound_108EC85C = -1;

        [TempleDllLocation(0x100354a0)]
        public void PlayVoiceLine(GameObjectBody speaker, GameObjectBody listener, int soundId)
        {
            if (soundId != -1)
            {
                // Stop the currently playing voice line
                if (sound_108EC85C != -1)
                {
                    Tig.Sound.FreeStream(sound_108EC85C);
                    sound_108EC85C = -1;
                }

                string soundPath;
                if (speaker.IsPC())
                {
                    var voiceId = speaker.GetInt32(obj_f.pc_voice_idx);
                    soundPath = $"sound/speech/pcvoice/{voiceId:D2}/{soundId}.mp3";
                }
                else
                {
                    var dialogScriptId = (soundId >> 16) & 0x7FFF;
                    var lineId = soundId & 0xFFFF;
                    var genderSuffix = listener.GetGender() != Gender.Female ? 'm' : 'f';
                    soundPath = $"sound/speech/{dialogScriptId:D5}/v{lineId}_{genderSuffix}.mp3";
                    // Fall back to the male variant if the female variant does not exist
                    if (genderSuffix == 'f' && !Tig.FS.FileExists(soundPath))
                    {
                        soundPath = $"sound/speech/{dialogScriptId:D5}/v{lineId}_{'m'}.mp3";
                    }
                }

                if (Tig.FS.FileExists(soundPath))
                {
                    sound_108EC85C = GameSystems.SoundGame.PlaySpeechFile(soundPath, 0);
                }
            }
        }
    }
}
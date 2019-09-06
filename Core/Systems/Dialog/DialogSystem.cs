using System;
using System.Collections.Generic;
using System.Text;
using Community.CsharpSqlite;
using SharpDX.Multimedia;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.IO;
using SpicyTemple.Core.Logging;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.Systems.ObjScript;
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
        private string ResolveLineTokens(DialogState state, string lineText)
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
                GameSystems.Dialog.GetNpcVoiceLine(speaker, listener, out text, out soundId, 2100, 2199, 12020);
            }
            else
            {
                GameSystems.Dialog.GetPcVoiceLine(speaker, listener, out text, out soundId,
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
                GameSystems.Dialog.GetNpcVoiceLine(speaker, listener, out text, out soundId, 2300, 2399, 12022);
            }
            else
            {
                GameSystems.Dialog.GetPcVoiceLine(speaker, listener, out text, out soundId,
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
                var listener = GameSystems.Dialog.GetListeningPartyMember(npcFollower);
                GameSystems.Dialog.GetNpcVoiceLine(npcFollower, listener, out var text,
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
                var listener = GameSystems.Dialog.GetListeningPartyMember(speaker);
                string text;
                int soundId;
                if (speaker.IsNPC())
                {
                    GameSystems.Dialog.GetNpcVoiceLine(speaker, listener, out text, out soundId, 3800, 3899, 12059);
                }
                else
                {
                    GameSystems.Dialog.GetPcVoiceLine(speaker, listener, out text, out soundId,
                        PlayerVoiceLine.FoundLotsOfGold);
                }

                GameSystems.Dialog.PlayCritterVoiceLine(speaker, listener, text, soundId);
            }
        }

        [TempleDllLocation(0x10037cf0)]
        public void GetOverburdenedVoiceLine(GameObjectBody speaker, GameObjectBody listener, out string text,
            out int soundId)
        {
            if (speaker.IsNPC())
            {
                GameSystems.Dialog.GetNpcVoiceLine(speaker, listener, out text, out soundId, 3400, 3499, 12055);
            }
            else
            {
                GameSystems.Dialog.GetPcVoiceLine(speaker, listener, out text, out soundId, PlayerVoiceLine.Encumbered);
            }
        }

        [TempleDllLocation(0x10037d80)]
        public void PlayOverburdenedVoiceLine(GameObjectBody critter)
        {
            if (GameSystems.Party.IsInParty(critter))
            {
                var listener = GameSystems.Dialog.GetListeningPartyMember(critter);
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
    }
}
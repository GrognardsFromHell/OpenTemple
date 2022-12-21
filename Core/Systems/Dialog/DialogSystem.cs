using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.IO;
using OpenTemple.Core.Logging;
using OpenTemple.Core.Systems.AI;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Systems.D20.Conditions;
using OpenTemple.Core.Systems.ObjScript;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Systems.TimeEvents;
using OpenTemple.Core.TigSubsystems;

namespace OpenTemple.Core.Systems.Dialog;

public class DialogSystem : IGameSystem
{
    private static readonly ILogger Logger = LoggingSystem.CreateLogger();

    private readonly DialogScripts _scripts = new();

    private readonly Dictionary<int, PlayerVoiceSet> _playerVoiceSetsByKey = new();

    private GeneratedDialog _generatedDialog;

    [TempleDllLocation(0x108EC860)]
    private readonly List<PlayerVoiceSet> _playerVoiceSets = new();

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
    private void GetNpcVoiceLine(GameObject speaker, GameObject listener,
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
            soundId = dialogState.SpeechId;
        }
    }

    [TempleDllLocation(0x10036e00)]
    private void sub_10036E00(out string textOut, int dialogScriptLine, DialogState state,
        int generatedLineFrom, int generatedLineTo)
    {
        if (!GetDialogScriptLine(state, out textOut, dialogScriptLine))
        {
            var line = _generatedDialog.GetNpcLine(state.NPC, state.Pc, generatedLineFrom, generatedLineTo);
            if (line != null)
            {
                textOut = ResolveLineTokens(state, line);
            }

            state.SpeechId = -1;
        }
    }

    private bool UseFemaleResponseFor(GameObject listener)
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

        var script = state.NPC.GetScript(obj_f.scripts_idx, (int) ObjScriptEvent.Dialog);

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

        if (UseFemaleResponseFor(state.Pc))
        {
            lineText = dialogLine.genderField;
        }
        else
        {
            lineText = dialogLine.txt;
        }

        lineText = ResolveLineTokens(state, lineText);
        state.SpeechId = ((script.scriptId & 0x7FFF) << 16) | (dialogLine.key & 0xFFFF);

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
                    result.Append(GameSystems.MapObject.GetDisplayName(state.Pc));
                    i += "pcname@".Length;
                    continue;
                }

                if (rest.StartsWith("@npcname@"))
                {
                    result.Append(GameSystems.MapObject.GetDisplayName(state.NPC));
                    i += "npcname@".Length;
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
    public GameObject GetListeningPartyMember(GameObject critter)
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
    private void GetPcVoiceLine(GameObject speaker, GameObject listener, out string text, out int soundId,
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
    public bool TryGetOkayVoiceLine(GameObject speaker, GameObject listener,
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
    public void GetDyingVoiceLine(GameObject speaker, GameObject listener, out string text, out int soundId)
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
    public void GetLeaderDyingVoiceLine(GameObject speaker, GameObject listener, out string text,
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
    public void GetFriendlyFireVoiceLine(GameObject speaker, GameObject listener, out string text,
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
    public bool TryGetWontSellVoiceLine(GameObject speaker, GameObject listener, out string text,
        out int soundId)
    {
        GetNpcVoiceLine(speaker, listener, out text, out soundId, 1200, 1299, 12008);
        return text != null || soundId != -1;
    }

    [TempleDllLocation(0x10036120)]
    public bool PlayCritterVoiceLine(GameObject obj, GameObject objAddressed, string text, int soundId)
    {
        if (string.IsNullOrEmpty(text))
        {
            return false;
        }

        if (Globals.Config.PartyTextConfirmations)
        {
            GameSystems.TextBubble.FloatText_100A2E60(obj, text);
            GameSystems.TextBubble.SetDuration(obj, 3);
        }

        if (Globals.Config.PartyVoiceConfirmations)
        {
            GameSystems.Dialog.PlayVoiceLine(obj, objAddressed, soundId);
            return true;
        }
        else
        {
            return false;
        }
    }

    [TempleDllLocation(0x100374e0)]
    public void GetFleeVoiceLine(GameObject speaker, GameObject listener, out string text, out int soundId)
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
        var followersWithLine = new List<GameObject>();
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
        GameObject speaker;
        if (followersWithLine.Count == 0)
        {
            speaker = GameSystems.Random.PickRandom(new List<GameObject>(GameSystems.Party.PlayerCharacters));
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
    public void GetOverburdenedVoiceLine(GameObject speaker, GameObject listener, out string text,
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
    public void PlayOverburdenedVoiceLine(GameObject critter)
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
    private void QueueComplainAgainTimer(GameObject critter)
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
    private readonly List<ObjectId> _savedPartySelection = new();

    [TempleDllLocation(0x10038770)]
    public bool BeginDialog(DialogState state)
    {
        if (GameSystems.AI.GetCannotTalkReason(state.NPC, state.Pc) != AiSystem.CannotTalkCause.None)
        {
            return false;
        }

        GameSystems.Reaction.DialogReaction_10053FE0(state.NPC, state.Pc);
        GameSystems.Combat.CritterLeaveCombat(GameSystems.Party.GetConsciousLeader());
        if (GameSystems.Party.IsInParty(state.Pc))
        {
            _savedPartySelection.Clear();
            foreach (var selectedCritter in GameSystems.Party.Selected)
            {
                _savedPartySelection.Add(selectedCritter.id);
            }

            GameSystems.Party.ClearSelection();
            GameSystems.Party.AddToSelection(state.Pc);
            GameSystems.Scroll.CenterOnSmooth(state.NPC);
        }

        state.LineNumber = state.ReqNpcLineId;
        state.ActionType = 0;
        DialogGetLines(false, state);
        return true;
    }

    [TempleDllLocation(0x100385f0)]
    public void DialogGetLines(bool reuseRngSeed, DialogState state)
    {
        state.PcLineText = Array.Empty<string>();
        state.PcLineSkillUse = Array.Empty<DialogSkill>();

        if (reuseRngSeed)
        {
            GameSystems.Random.SetSeed(state.RngSeed);
        }
        else
        {
            state.RngSeed = GameSystems.Random.SetRandomSeed();
        }

        if (DialogGetNpcLine(state, reuseRngSeed))
        {
            Span<int> answerKeys = stackalloc int[20];
            var answerCount = DialogGetPossibleAnswerLineIds(state, answerKeys);
            if (answerCount == 0)
            {
                state.PcLineText = new string[1];
                state.PcReplyOpcode = new DialogReplyOpCode[1];
                state.PcLineSkillUse = new DialogSkill[1];
                state.NPCReplyIds = new int[1];
                state.EffectLineKey = new int[1];

                state.PcLineText[0] = _generatedDialog.GetPcLine(state, 400, 499);
                state.PcReplyOpcode[0] = DialogReplyOpCode.GoToLine;
            }
            else
            {
                state.PcLineText = new string[answerCount];
                state.PcReplyOpcode = new DialogReplyOpCode[answerCount];
                state.PcLineSkillUse = new DialogSkill[answerCount];
                state.NPCReplyIds = new int[answerCount];
                state.EffectLineKey = new int[answerCount];
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
        state.PcLineSkillUse = Array.Empty<DialogSkill>();

        var intScore = state.Pc.GetStat(Stat.intelligence);
        if (intScore > 7 && GameSystems.Critter.CritterIsLowInt(state.Pc))
        {
            intScore = 1;
        }

        if (!state.DialogScript.Lines.TryGetValue(state.LineNumber, out var line))
        {
            return 0;
        }

        int answerCount = 0;
        int nextLineKey = line.nextResponseKey;
        while (nextLineKey != -1)
        {
            var responseLine = state.DialogScript.Lines[nextLineKey];
            nextLineKey = responseLine.nextResponseKey;

            if (!SatisfiesIntRequirement(intScore, responseLine.minIq))
            {
                continue;
            }

            if (!SatisfiesPrecondition(state, ref responseLine, out var skillChecks))
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
        var line = state.DialogScript.Lines[answerLineId];

        DialogGetOpcodeAndAnswer2(line.answerLineId,
            out state.NPCReplyIds[responseIdx],
            out state.PcReplyOpcode[responseIdx]
        );

        if (line.txt.Equals("a:", StringComparison.OrdinalIgnoreCase))
        {
            state.PcLineText[responseIdx] = _generatedDialog.GetPcClassBasedLine(1000, state);
        }
        else if (line.txt.StartsWith("b:", StringComparison.OrdinalIgnoreCase))
        {
            var restOfLine = line.txt.Substring(2);
            if (restOfLine.Length > 0)
            {
                state.PcLineText[responseIdx] = restOfLine;
            }
            else
            {
                state.PcLineText[responseIdx] = _generatedDialog.GetPcLine(state, 300, 399);
            }

            // This always returned zero in vanilla, but what is opcode 26?
            if (false)
            {
                state.PcReplyOpcode[responseIdx] = DialogReplyOpCode.NpcSellOffThenBarter;
                state.NPCReplyIds[responseIdx] = line.answerLineId;
            }
            else
            {
                state.PcReplyOpcode[responseIdx] = DialogReplyOpCode.Barter;
                state.NPCReplyIds[responseIdx] = line.answerLineId;
            }
        }
        else if (line.txt.StartsWith("c:", StringComparison.OrdinalIgnoreCase))
        {
            throw new NotSupportedException(
                "The c: prefix is an Arcanum leftover using Files that were not updated for ToEE.");
        }
        else if (line.txt.Equals("e:", StringComparison.OrdinalIgnoreCase))
        {
            state.PcLineText[responseIdx] = _generatedDialog.GetPcLine(state, 400, 499);
        }
        else if (line.txt.Equals("f:", StringComparison.OrdinalIgnoreCase))
        {
            state.PcLineText[responseIdx] = _generatedDialog.GetPcLine(state, 800, 899);
        }
        else if (line.txt.Equals("k:", StringComparison.OrdinalIgnoreCase))
        {
            state.PcLineText[responseIdx] = _generatedDialog.GetPcLine(state, 1500, 1599);
        }
        else if (line.txt.Equals("n:", StringComparison.OrdinalIgnoreCase))
        {
            state.PcLineText[responseIdx] = _generatedDialog.GetPcLine(state, 100, 199);
        }
        else if (line.txt.StartsWith("q:", StringComparison.OrdinalIgnoreCase))
        {
            throw new NotSupportedException("q: was only implemented in Arkanum, not in ToEE.");
        }
        else if (line.txt.StartsWith("r:", StringComparison.OrdinalIgnoreCase))
        {
            state.PcLineText[responseIdx] = _generatedDialog.GetPcClassBasedLine(2000, state);
            state.PcReplyOpcode[responseIdx] = DialogReplyOpCode.OfferRumor;
            state.NPCReplyIds[responseIdx] = line.answerLineId;
        }
        else if (line.txt.Equals("s:", StringComparison.OrdinalIgnoreCase))
        {
            state.PcLineText[responseIdx] = _generatedDialog.GetPcLine(state, 200, 299);
        }
        else if (line.txt.Equals("y:", StringComparison.OrdinalIgnoreCase))
        {
            state.PcLineText[responseIdx] = _generatedDialog.GetPcLine(state, 1, 99);
        }
        else
        {
            state.PcLineText[responseIdx] = ResolveLineTokens(state, line.txt);
        }

        state.EffectLineKey[responseIdx] = line.key;

        // This is a debug option
        if (Globals.Config.ShowDialogLineIds)
        {
            state.PcLineText[responseIdx] = $"[{line.key}] " + state.PcLineText[responseIdx];
        }
    }

    [TempleDllLocation(0x10034dc0)]
    private void DialogGetOpcodeAndAnswer2(int answerLineId, out int answerId2, out DialogReplyOpCode opCode)
    {
        if (answerLineId < 0)
        {
            opCode = DialogReplyOpCode.GoToLineWithoutEffect;
            answerId2 = -answerLineId;
        }
        else if (answerLineId > 0)
        {
            opCode = DialogReplyOpCode.GoToLine;
            answerId2 = answerLineId;
        }
        else
        {
            answerId2 = 0;
            opCode = DialogReplyOpCode.ExitDialog;
        }
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
            return pcIntScore <= -lineRequirement;
        }

        // Positives are minimums
        return pcIntScore >= lineRequirement;
    }

    [TempleDllLocation(0x10038240)]
    public bool DialogGetNpcLine(DialogState state, bool rngSeed)
    {
        if (!state.DialogScript.Lines.TryGetValue(state.LineNumber, out var line))
        {
            state.NPCLineText = " ";
            state.SpeechId = -1;
            return false;
        }

        int dlgScriptId = state.DialogScriptId;
        state.AnswerLineId = line.answerLineId;

        int line_and_script_packed;
        if (dlgScriptId != 0)
        {
            line_and_script_packed = ((dlgScriptId & 0x7FFF) << 16) | (line.key & 0xFFFF);
        }
        else
        {
            line_and_script_packed = -1;
        }

        state.SpeechId = line_and_script_packed;
        if (!rngSeed && line.effectField != null)
        {
            RunDialogAction(line.key, state, 0);
        }

        if (line.txt.StartsWith("g:", StringComparison.OrdinalIgnoreCase))
        {
            dlg_generic_greeting(state);
            if (state.ActionType == 3)
            {
                state.PcLineText = Array.Empty<string>();
                state.PcReplyOpcode = Array.Empty<DialogReplyOpCode>();
                state.PcLineSkillUse = Array.Empty<DialogSkill>();
                state.NPCReplyIds = Array.Empty<int>();
                state.EffectLineKey = Array.Empty<int>();
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
            OfferRumor(line.answerLineId, 5, state);
            return false;
        }

        string lineTemplate;
        if (state.Pc.GetGender() != Gender.Male)
        {
            lineTemplate = line.genderField;
        }
        else
        {
            lineTemplate = line.txt;
        }

        state.NPCLineText = ResolveLineTokens(state, lineTemplate);
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
        if (GameSystems.AI.HasSurrendered(state.NPC, out _))
        {
            state.NPCLineText = "";
            state.SpeechId = -1;

            if (state.NPC.IsNPC())
            {
                GetNpcVoiceLine(state.NPC, state.Pc, out var text, out var speechId, 3000,
                    3099, 12029);
                state.NPCLineText = text;
                state.SpeechId = speechId;
            }

            return;
        }

        if (!disableFearsomeResponse && GameSystems.Critter.HasFearsomeAssociates(state.Pc))
        {
            state.NPCLineText = GetClassBasedNpcLine(state, 18000);
            if (GameSystems.Critter.GetLeaderRecursive(state.NPC) == null)
            {
                state.ActionType = 3;
            }

            return;
        }

        if (!disableInvisibleResponse &&
            GameSystems.D20.D20Query(state.Pc, D20DispatcherKey.QUE_Critter_Is_Invisible))
        {
            state.NPCLineText = GetClassBasedNpcLine(state, 22000);
            if (GameSystems.Critter.GetLeaderRecursive(state.NPC) == null)
            {
                state.ActionType = 3;
            }

            return;
        }

        if (!disablePolymorphedResponse && GameSystems.D20.D20Query(state.Pc, D20DispatcherKey.QUE_Polymorphed))
        {
            state.NPCLineText = GetClassBasedNpcLine(state, 20000);
            if (GameSystems.Critter.GetLeaderRecursive(state.NPC) == null)
            {
                state.ActionType = 3;
            }

            return;
        }

        if (!disableMirrorImageResponse && state.Pc.HasCondition(SpellEffects.SpellMirrorImage))
        {
            state.NPCLineText = GetClassBasedNpcLine(state, 21000);
            return;
        }

        var armor = GameSystems.Item.ItemWornAt(state.Pc, EquipSlot.Armor);
        var robes = GameSystems.Item.ItemWornAt(state.Pc, EquipSlot.Robes);
        if (!disableNakedResponse && armor == null && robes == null)
        {
            state.NPCLineText = GetClassBasedNpcLine(state, 16000);
            return;
        }

        if (!disableBarbarianArmorResponse && armor != null && state.Pc.GetStat(Stat.level_barbarian) > 0 &&
            armor.ProtoId == 6055)
        {
            state.NPCLineText = GetClassBasedNpcLine(state, 17000);
            return;
        }

        // 20% chance that the NPC will remark on one of the player's reputations if it's affecting the NPC's
        // reaction towards the player positively.
        if (!disableReputationResponse
            && GameSystems.Random.GetInt(1, 100) <= 20
            && GameSystems.Reputation.TryGetReputationAffectingReaction(state.Pc, state.NPC,
                out var reputationId)
            && GameSystems.Reputation.TryGetGreeting(state.Pc, state.NPC, reputationId, out var repGreeting)
           )
        {
            state.NPCLineText = repGreeting;
            state.SpeechId = -1;
            return;
        }

        var reaction = GameSystems.Reaction.GetReaction(state.NPC, state.Pc);
        var reactionLevel = GameSystems.Reaction.GetReactionLevel(reaction);
        if (GameSystems.Reaction.HasMet(state.NPC, state.Pc))
        {
            switch (reactionLevel)
            {
                // Further "good" greeting
                case 0:
                case 1:
                case 2:
                    state.NPCLineText = GetClassBasedNpcLine(state, 12000);
                    break;
                // Further neutral greeting
                case 3:
                    state.NPCLineText = GetClassBasedNpcLine(state, 13000);
                    break;
                case 4:
                case 5:
                    state.NPCLineText = GetRaceBasedNpcLine(state, 4000);
                    break;
                case 6:
                    state.NPCLineText = GetRaceBasedNpcLine(state, 5000);
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
                    state.NPCLineText = GetClassBasedNpcLine(state, 9000);
                    break;
                case 3:
                    state.NPCLineText = GetClassBasedNpcLine(state, 10000);
                    break;
                case 4:
                case 5:
                    state.NPCLineText = GetRaceBasedNpcLine(state, 2000);
                    break;
                case 6:
                    state.NPCLineText = GetRaceBasedNpcLine(state, 3000);
                    break;
                default:
                    return;
            }
        }
    }

    [TempleDllLocation(0x100ae7a0)]
    private void RunDialogAction(int lineKey, DialogState state, int chosenIndex)
    {
        if (lineKey <= 0)
        {
            return; // No action to run
        }

        if (!GameSystems.Script.TryGetDialogScript(state.DialogScriptId, out var dialogScript))
        {
            Logger.Warn("Dialog script {0} is missing.", state.DialogScriptId);
            return;
        }

        var currentEffect = state.DialogScript.Lines[lineKey].effectField;
        dialogScript.ApplySideEffect(state.NPC, state.Pc, lineKey, out var originalEffect);
        if (currentEffect != originalEffect)
        {
            Logger.Warn("Dialog script {0} line {1} has effect '{2}', but C# scripts were converted from '{3}'",
                state.DialogScript.Path, lineKey, currentEffect, originalEffect);
        }
    }

    [TempleDllLocation(0x10034d50)]
    private bool SatisfiesPrecondition(DialogState state, ref DialogLine responseLine,
        out DialogSkillChecks skillChecks)
    {
        if (!GameSystems.Script.TryGetDialogScript(state.DialogScriptId, out var dialogScript))
        {
            Logger.Warn("Dialog script {0} is missing.", state.DialogScriptId);
            skillChecks = default;
            return true;
        }

        if (!dialogScript.TryGetSkillChecks(responseLine.key, out skillChecks))
        {
            skillChecks = default;
        }

        var currentTest = responseLine.testField;
        var result = dialogScript.CheckPrecondition(state.NPC, state.Pc, responseLine.key, out var originalTest);

        if (currentTest != originalTest)
        {
            Logger.Warn("Dialog script {0} line {1} has effect '{2}', but C# scripts were converted from '{3}'",
                state.DialogScript.Path, responseLine.key, currentTest, originalTest);
        }

        return result;
    }

    [TempleDllLocation(0x100370f0)]
    private void OfferRumor(int answerLineId, int rumorCostInGold, DialogState state)
    {
        DialogGetOpcodeAndAnswer2(answerLineId, out var npcReplyId, out var pcOpCode);

        if (!GameSystems.Rumor.TryFindRumor(state.Pc, state.NPC, out var rumorId))
        {
            state.NPCLineText = GetClassBasedNpcLine(state, 7000);

            state.PcLineText = new string[1];
            state.PcReplyOpcode = new DialogReplyOpCode[1];
            state.PcLineSkillUse = new DialogSkill[1];
            state.NPCReplyIds = new int[1];
            state.EffectLineKey = new int[1];

            state.PcLineText[0] = _generatedDialog.GetPcLine(state, 600, 699);
            state.PcReplyOpcode[0] = pcOpCode;
            state.NPCReplyIds[0] = npcReplyId;
        }
        else if (rumorCostInGold <= 0)
        {
            SayRumor(state, rumorId, pcOpCode, npcReplyId);
        }
        else
        {
            var successOp = new ReplyOp(DialogReplyOpCode.GiveRumor, rumorId);
            AskForMoney(rumorCostInGold, state, successOp, pcOpCode, npcReplyId);
        }
    }

    [TempleDllLocation(0x10035300)]
    public void SayRumor(DialogState state, int rumorId, DialogReplyOpCode pcOpCode, int npcReplyId)
    {
        GameSystems.Rumor.TryGetRumorNpcLine(state.Pc, state.NPC, rumorId, out var rumorText);
        state.NPCLineText = ResolveLineTokens(state, rumorText);

        state.SpeechId = -1;
        GameSystems.Rumor.Add(state.Pc, rumorId);

        state.PcLineText = new string[1];
        state.PcReplyOpcode = new DialogReplyOpCode[1];
        state.PcLineSkillUse = new DialogSkill[1];
        state.NPCReplyIds = new int[1];
        state.EffectLineKey = new int[1];

        state.PcLineText[0] = _generatedDialog.GetPcClassBasedLine(1000, state);
        state.PcReplyOpcode[0] = pcOpCode;
        state.NPCReplyIds[0] = npcReplyId;
    }

    [TempleDllLocation(0x10036fb0)]
    private void AskForMoney(int goldPieces, DialogState state, ReplyOp successOp, DialogReplyOpCode pcOpCode,
        int npcReplyId)
    {
        // Is this how much money the NPC is asking for?
        var moneyAmount = GameSystems.Reaction.AdjustBuyPrice(state.NPC, state.Pc, goldPieces);

        // Asking for money
        state.NPCLineText = GetClassBasedNpcLine(state, 1000);

        state.PcLineText = new string[2];
        state.PcReplyOpcode = new DialogReplyOpCode[2];
        state.PcLineSkillUse = new DialogSkill[2];
        state.NPCReplyIds = new int[2];
        state.EffectLineKey = new int[2];

        // Yes
        state.PcLineText[0] = _generatedDialog.GetPcLine(state, 1, 99);
        state.NPCReplyIds[0] = moneyAmount;
        state.PcReplyOpcode[0] = DialogReplyOpCode.AskForMoney;

        // No
        state.PcLineText[1] = _generatedDialog.GetPcLine(state, 100, 199);
        state.PcReplyOpcode[1] = pcOpCode;
        state.NPCReplyIds[1] = npcReplyId;

        state.AskForMoneyOp = successOp;
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

        lineText = _generatedDialog.GetNpcClassBasedLine(state.NPC, state.Pc, baseLine);
        lineText = ResolveLineTokens(state, lineText);
        state.SpeechId = -1;
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

        lineText = _generatedDialog.GetNpcRaceBasedLine(state.NPC, state.Pc, baseLine);
        lineText = ResolveLineTokens(state, lineText);
        state.SpeechId = -1;
        return lineText;
    }

    [TempleDllLocation(0x10034710)]
    public void EndDialog(DialogState state)
    {
        GameSystems.Reaction.NpcReactionUpdate(state.NPC, state.Pc);

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
    public string Dialog_10037AF0(GameObject npc, GameObject pc)
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

            return dlgState.NPCLineText;
        }
    }

    // Probably the stream id of the currently playing sound
    [TempleDllLocation(0x108ec85c)]
    private int _currentVoiceLineStream = -1;

    [TempleDllLocation(0x100354a0)]
    public void PlayVoiceLine(GameObject speaker, GameObject listener, int soundId)
    {
        if (soundId != -1)
        {
            StopCurrentVoiceLine();

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
                _currentVoiceLineStream = GameSystems.SoundGame.PlaySpeechFile(soundPath, 0);
            }
        }
    }

    [TempleDllLocation(0x10034ab0)]
    public void StopCurrentVoiceLine()
    {
        // Stop the currently playing voice line if any
        if (_currentVoiceLineStream != -1)
        {
            Tig.Sound.FreeStream(_currentVoiceLineStream);
            _currentVoiceLineStream = -1;
        }
    }

    private static SkillId GetSkillForDialogSkill(DialogSkill dialogSkill)
    {
        return dialogSkill switch
        {
            DialogSkill.Bluff => SkillId.bluff,
            DialogSkill.Diplomacy => SkillId.diplomacy,
            DialogSkill.Intimidate => SkillId.intimidate,
            DialogSkill.SenseMotive => SkillId.sense_motive,
            DialogSkill.GatherInformation => SkillId.gather_information,
            _ => throw new ArgumentOutOfRangeException(nameof(dialogSkill), dialogSkill, null)
        };
    }

    [TempleDllLocation(0x10038a50)]
    public void DialogChoiceParse(DialogState state, int responseIdx)
    {
        var actionType = state.ActionType;
        if (actionType != 3 && actionType != 4 && actionType != 6 && actionType != 5 && actionType != 7 &&
            actionType != 8)
        {
            if (GameSystems.AI.GetCannotTalkReason(state.NPC, state.Pc) != AiSystem.CannotTalkCause.None)
            {
                state.ActionType = 1;
            }
            else
            {
                var dialogSkill = state.PcLineSkillUse[responseIdx];
                if (dialogSkill != DialogSkill.None)
                {
                    var skill = GetSkillForDialogSkill(dialogSkill);
                    GameUiBridge.RecordSkillUse(state.Pc, skill);
                }

                if (actionType == 2)
                {
                    DialogGetOpcodeAndAnswer2(state.LineNumber, out var answerId, out var opCode);
                    DialogGetNpcReply(opCode, answerId, state);
                }
                else
                {
                    if (actionType != 1)
                    {
                        var effectField = state.EffectLineKey[responseIdx];
                        if (effectField != null)
                        {
                            RunDialogAction(effectField, state, responseIdx);
                        }
                    }

                    DialogGetNpcReply(state.PcReplyOpcode[responseIdx], state.NPCReplyIds[responseIdx], state);
                }
            }
        }
    }

    [TempleDllLocation(0x10038900)]
    private void DialogGetNpcReply(DialogReplyOpCode replyOpCode, int lineNumber, DialogState state)
    {
        switch (replyOpCode)
        {
            case DialogReplyOpCode.GoToLine:
                state.LineNumber = lineNumber;
                state.ActionType = 0;
                GameSystems.Dialog.DialogGetLines(false, state);
                return;
            case DialogReplyOpCode.ExitDialog:
                state.ActionType = 1;
                return;
            case DialogReplyOpCode.Barter:
                state.LineNumber = lineNumber;
                state.ActionType = 2;
                return;
            case DialogReplyOpCode.AskForMoney:
                AskedForMoneyConfirmed(lineNumber, state);
                return;
            case DialogReplyOpCode.OfferRumor:
                GameSystems.Dialog.OfferRumor(lineNumber, 5, state);
                return;
            case DialogReplyOpCode.GiveRumor:
                GameSystems.Dialog.SayRumor(state, lineNumber, state.PcReplyOpcode[1], state.NPCReplyIds[1]);
                return;
            case DialogReplyOpCode.NpcSellOffThenBarter:
                DialogDoNpcSelloff_0(state, lineNumber);
                return;
            default:
                return;
        }
    }

    [TempleDllLocation(0x100389e0)]
    private void AskedForMoneyConfirmed(int goldPieces, DialogState state)
    {
        if (GameSystems.Item.GetTotalCurrencyAmount(state.Pc) < goldPieces)
        {
            // Not enough money
            SayNotEnoughMoney(2000, state, state.PcReplyOpcode[1], state.NPCReplyIds[1]);
        }
        else
        {
            GameSystems.Party.RemovePartyMoney(0, goldPieces, 0, 0);
            DialogGetNpcReply(state.AskForMoneyOp.OpCode, state.AskForMoneyOp.Argument, state);
        }
    }

    [TempleDllLocation(0x10037090)]
    public void SayNotEnoughMoney(int a1, DialogState state, DialogReplyOpCode nextOpCode, int nextNpcReply)
    {
        GetClassBasedNpcLine(state, a1);

        state.PcLineText = new string[1];
        state.PcReplyOpcode = new DialogReplyOpCode[1];
        state.PcLineSkillUse = new DialogSkill[1];
        state.NPCReplyIds = new int[1];
        state.EffectLineKey = new int[1];

        state.PcLineText[0] = _generatedDialog.GetPcLine(state, 600, 699); // "continue"
        state.PcReplyOpcode[0] = nextOpCode;
        state.NPCReplyIds[0] = nextNpcReply;
    }

    [TempleDllLocation(0x10037240)]
    public void DialogDoNpcSelloff_0(DialogState state, int a2)
    {
        // TODO NpcsSellItems/*0x1006c930*/(state.pc, state.npc);
        // TODO The DLL Fix used in GoG stubs this method out and opcode 26 is NEVER used as a result
        GetClassBasedNpcLine(state, 7000); // "No rumors available"
        // NPC barters with followers first
        sub_10036E00(out var npcLine, 12012, state, 4400, 4499);
        if (npcLine != null)
        {
            state.NPCLineText = npcLine;
        }

        state.PcLineText = new string[1];
        state.PcReplyOpcode = new DialogReplyOpCode[1];
        state.PcLineSkillUse = new DialogSkill[1];
        state.NPCReplyIds = new int[1];
        state.EffectLineKey = new int[1];

        state.PcLineText[0] = GameSystems.Dialog._generatedDialog.GetPcLine(state, 600, 699);
        state.PcReplyOpcode[0] = DialogReplyOpCode.Barter;
        state.NPCReplyIds[0] = a2;
    }
}
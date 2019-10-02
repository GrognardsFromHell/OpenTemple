
using System;
using System.Collections.Generic;
using System.Diagnostics;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Systems;
using SpicyTemple.Core.Systems.Dialog;
using SpicyTemple.Core.Systems.Feats;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.Systems.Script;
using SpicyTemple.Core.Systems.Spells;
using SpicyTemple.Core.Systems.GameObjects;
using SpicyTemple.Core.Systems.D20.Conditions;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Systems.ObjScript;
using SpicyTemple.Core.Ui;
using System.Linq;
using SpicyTemple.Core.Systems.Script.Extensions;
using SpicyTemple.Core.Utils;
using static SpicyTemple.Core.Systems.Script.ScriptUtilities;

namespace Scripts.Dialog
{
    [DialogScript(150)]
    public class WerewolfMirrorDialog : WerewolfMirror, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 2:
                case 4:
                    Trace.Assert(originalScript == "game.party_alignment != LAWFUL_EVIL and game.party_alignment != NEUTRAL_EVIL and game.party_alignment != CHAOTIC_EVIL");
                    return PartyAlignment != Alignment.LAWFUL_EVIL && PartyAlignment != Alignment.NEUTRAL_EVIL && PartyAlignment != Alignment.CHAOTIC_EVIL;
                case 3:
                case 5:
                    Trace.Assert(originalScript == "game.party_alignment == LAWFUL_EVIL or game.party_alignment == NEUTRAL_EVIL or game.party_alignment == CHAOTIC_EVIL or game.leader.reputation_has(32) == 1 or game.leader.reputation_has(30) == 1 or game.leader.reputation_has(29) == 1");
                    return PartyAlignment == Alignment.LAWFUL_EVIL || PartyAlignment == Alignment.NEUTRAL_EVIL || PartyAlignment == Alignment.CHAOTIC_EVIL || SelectedPartyLeader.HasReputation(32) || SelectedPartyLeader.HasReputation(30) || SelectedPartyLeader.HasReputation(29);
                case 25:
                case 26:
                case 71:
                case 72:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc, skill_sense_motive) >= 10");
                    return pc.GetSkillLevel(npc, SkillId.sense_motive) >= 10;
                default:
                    Trace.Assert(originalScript == null);
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 1:
                    Trace.Assert(originalScript == "game.global_flags[154] = 1");
                    SetGlobalFlag(154, true);
                    break;
                default:
                    Trace.Assert(originalScript == null);
                    return;
            }
        }
        public bool TryGetSkillCheck(int lineNumber, out DialogSkillChecks skillChecks)
        {
            switch (lineNumber)
            {
                case 25:
                case 26:
                case 71:
                case 72:
                    skillChecks = new DialogSkillChecks(SkillId.sense_motive, 10);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}

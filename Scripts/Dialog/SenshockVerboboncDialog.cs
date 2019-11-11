
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
    [DialogScript(372)]
    public class SenshockVerboboncDialog : SenshockVerbobonc, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 2:
                case 3:
                    originalScript = "not npc.has_met( pc )";
                    return !npc.HasMet(pc);
                case 4:
                    originalScript = "npc.has_met( pc )";
                    return npc.HasMet(pc);
                case 12:
                    originalScript = "(game.party_alignment == LAWFUL_GOOD or game.party_alignment == NEUTRAL_GOOD or game.party_alignment == CHAOTIC_GOOD) and pc.stat_level_get(stat_level_paladin) == 0";
                    return (PartyAlignment == Alignment.LAWFUL_GOOD || PartyAlignment == Alignment.NEUTRAL_GOOD || PartyAlignment == Alignment.CHAOTIC_GOOD) && pc.GetStat(Stat.level_paladin) == 0;
                case 13:
                    originalScript = "(game.party_alignment == LAWFUL_NEUTRAL or game.party_alignment == TRUE_NEUTRAL or game.party_alignment == CHAOTIC_NEUTRAL)";
                    return (PartyAlignment == Alignment.LAWFUL_NEUTRAL || PartyAlignment == Alignment.NEUTRAL || PartyAlignment == Alignment.CHAOTIC_NEUTRAL);
                case 14:
                    originalScript = "(game.party_alignment == LAWFUL_EVIL or game.party_alignment == CHAOTIC_EVIL or game.party_alignment == NEUTRAL_EVIL)";
                    return (PartyAlignment == Alignment.LAWFUL_EVIL || PartyAlignment == Alignment.CHAOTIC_EVIL || PartyAlignment == Alignment.NEUTRAL_EVIL);
                case 15:
                    originalScript = "pc.stat_level_get(stat_level_paladin) >= 1";
                    return pc.GetStat(Stat.level_paladin) >= 1;
                case 22:
                case 52:
                    originalScript = "game.quests[78].state == qs_accepted";
                    return GetQuestState(78) == QuestState.Accepted;
                case 61:
                    originalScript = "not npc_get(npc,1)";
                    return !ScriptDaemon.npc_get(npc, 1);
                case 62:
                    originalScript = "not npc_get(npc,2)";
                    return !ScriptDaemon.npc_get(npc, 2);
                case 63:
                    originalScript = "not npc_get(npc,3)";
                    return !ScriptDaemon.npc_get(npc, 3);
                case 64:
                    originalScript = "not npc_get(npc,4)";
                    return !ScriptDaemon.npc_get(npc, 4);
                case 72:
                    originalScript = "not npc_get(npc,5)";
                    return !ScriptDaemon.npc_get(npc, 5);
                case 73:
                    originalScript = "not npc_get(npc,6)";
                    return !ScriptDaemon.npc_get(npc, 6);
                case 74:
                    originalScript = "not npc_get(npc,7)";
                    return !ScriptDaemon.npc_get(npc, 7);
                case 75:
                    originalScript = "(npc_get(npc,1) or npc_get(npc,4)) and not npc_get(npc,8)";
                    return (ScriptDaemon.npc_get(npc, 1) || ScriptDaemon.npc_get(npc, 4)) && !ScriptDaemon.npc_get(npc, 8);
                default:
                    originalScript = null;
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 11:
                case 15:
                case 32:
                case 42:
                    originalScript = "npc.attack( pc )";
                    npc.Attack(pc);
                    break;
                case 61:
                    originalScript = "npc_set(npc,1)";
                    ScriptDaemon.npc_set(npc, 1);
                    break;
                case 62:
                    originalScript = "npc_set(npc,2)";
                    ScriptDaemon.npc_set(npc, 2);
                    break;
                case 63:
                    originalScript = "npc_set(npc,3)";
                    ScriptDaemon.npc_set(npc, 3);
                    break;
                case 64:
                    originalScript = "npc_set(npc,4)";
                    ScriptDaemon.npc_set(npc, 4);
                    break;
                case 72:
                    originalScript = "npc_set(npc,5)";
                    ScriptDaemon.npc_set(npc, 5);
                    break;
                case 73:
                    originalScript = "npc_set(npc,6)";
                    ScriptDaemon.npc_set(npc, 6);
                    break;
                case 74:
                    originalScript = "npc_set(npc,7)";
                    ScriptDaemon.npc_set(npc, 7);
                    break;
                case 75:
                    originalScript = "npc_set(npc,8)";
                    ScriptDaemon.npc_set(npc, 8);
                    break;
                default:
                    originalScript = null;
                    return;
            }
        }
        public bool TryGetSkillChecks(int lineNumber, out DialogSkillChecks skillChecks)
        {
            switch (lineNumber)
            {
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}

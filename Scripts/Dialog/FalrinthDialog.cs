
using System;
using System.Collections.Generic;
using System.Diagnostics;
using OpenTemple.Core.GameObject;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.Dialog;
using OpenTemple.Core.Systems.Feats;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Systems.Script;
using OpenTemple.Core.Systems.Spells;
using OpenTemple.Core.Systems.GameObjects;
using OpenTemple.Core.Systems.D20.Conditions;
using OpenTemple.Core.Location;
using OpenTemple.Core.Systems.ObjScript;
using OpenTemple.Core.Ui;
using System.Linq;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Utils;
using static OpenTemple.Core.Systems.Script.ScriptUtilities;

namespace Scripts.Dialog
{
    [DialogScript(155)]
    public class FalrinthDialog : Falrinth, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 2:
                case 3:
                case 51:
                case 52:
                case 131:
                case 132:
                    originalScript = "game.quests[54].state == qs_accepted";
                    return GetQuestState(54) == QuestState.Accepted;
                case 21:
                case 22:
                case 61:
                case 62:
                case 91:
                case 92:
                case 111:
                case 112:
                    originalScript = "pc.skill_level_get(npc, skill_intimidate) >= 10";
                    return pc.GetSkillLevel(npc, SkillId.intimidate) >= 10;
                case 23:
                case 24:
                case 53:
                case 54:
                    originalScript = "pc.skill_level_get(npc, skill_bluff) >= 10";
                    return pc.GetSkillLevel(npc, SkillId.bluff) >= 10;
                case 72:
                case 73:
                case 82:
                case 83:
                case 171:
                case 172:
                    originalScript = "anyone( pc.group_list(), \"has_item\", 5808 )";
                    return pc.GetPartyMembers().Any(o => o.HasItemByName(5808));
                case 74:
                case 75:
                case 84:
                case 85:
                case 173:
                case 174:
                    originalScript = "anyone( pc.group_list(), \"has_item\", 5809 )";
                    return pc.GetPartyMembers().Any(o => o.HasItemByName(5809));
                case 76:
                case 77:
                case 86:
                case 87:
                case 175:
                case 176:
                    originalScript = "anyone( pc.group_list(), \"has_item\", 5810 )";
                    return pc.GetPartyMembers().Any(o => o.HasItemByName(5810));
                case 151:
                case 152:
                    originalScript = "pc.skill_level_get(npc, skill_sense_motive) >= 10";
                    return pc.GetSkillLevel(npc, SkillId.sense_motive) >= 10;
                default:
                    originalScript = null;
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 1:
                    originalScript = "game.global_flags[167] = 1";
                    SetGlobalFlag(167, true);
                    break;
                case 40:
                case 200:
                    originalScript = "game.global_flags[164] = 1";
                    SetGlobalFlag(164, true);
                    break;
                case 41:
                    originalScript = "game.particles( 'sp-Dimension Door', npc ); game.sound( 4018 ); falrinth_escape(npc,pc)";
                    AttachParticles("sp-Dimension Door", npc);
                    Sound(4018);
                    falrinth_escape(npc, pc);
                    ;
                    break;
                case 101:
                case 102:
                case 115:
                case 116:
                case 121:
                case 193:
                case 194:
                    originalScript = "npc.attack(pc)";
                    npc.Attack(pc);
                    break;
                case 130:
                    originalScript = "game.global_vars[901] = 2";
                    SetGlobalVar(901, 2);
                    break;
                case 201:
                    originalScript = "game.particles( 'sp-Dimension Door', npc ); game.sound( 4018 ); falrinth_well( npc,pc ); falrinth_escape(npc,pc)";
                    AttachParticles("sp-Dimension Door", npc);
                    Sound(4018);
                    falrinth_well(npc, pc);
                    falrinth_escape(npc, pc);
                    ;
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
                case 21:
                case 22:
                case 61:
                case 62:
                case 91:
                case 92:
                case 111:
                case 112:
                    skillChecks = new DialogSkillChecks(SkillId.intimidate, 10);
                    return true;
                case 23:
                case 24:
                case 53:
                case 54:
                    skillChecks = new DialogSkillChecks(SkillId.bluff, 10);
                    return true;
                case 151:
                case 152:
                    skillChecks = new DialogSkillChecks(SkillId.sense_motive, 10);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}

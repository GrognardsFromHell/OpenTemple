
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
    [DialogScript(141)]
    public class SmigmalDialog : Smigmal, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 5:
                case 6:
                case 45:
                case 46:
                    Trace.Assert(originalScript == "game.party_alignment & ALIGNMENT_EVIL != 0 and pc.stat_level_get(stat_alignment) & ALIGNMENT_EVIL != 0");
                    return PartyAlignment.IsEvil() && pc.GetAlignment().IsEvil();
                case 24:
                case 25:
                    Trace.Assert(originalScript == "game.global_flags[425] == 1");
                    return GetGlobalFlag(425);
                case 41:
                case 42:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc, skill_sense_motive) >= 10");
                    return pc.GetSkillLevel(npc, SkillId.sense_motive) >= 10;
                case 51:
                    Trace.Assert(originalScript == "game.global_flags[7] == 0");
                    return !GetGlobalFlag(7);
                case 52:
                    Trace.Assert(originalScript == "game.global_flags[7] == 1");
                    return GetGlobalFlag(7);
                default:
                    Trace.Assert(originalScript == null);
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 60:
                    Trace.Assert(originalScript == "smigmal_well( npc,pc )");
                    smigmal_well(npc, pc);
                    break;
                case 61:
                    Trace.Assert(originalScript == "game.particles( 'sp-invisibility', npc ); game.sound( 4031 ); smigmal_escape(npc,pc)");
                    AttachParticles("sp-invisibility", npc);
                    Sound(4031);
                    smigmal_escape(npc, pc);
                    ;
                    break;
                case 110:
                    Trace.Assert(originalScript == "smig_backup(npc,pc); game.global_flags[996] = 1");
                    smig_backup(npc, pc);
                    SetGlobalFlag(996, true);
                    ;
                    break;
                case 111:
                case 121:
                case 131:
                    Trace.Assert(originalScript == "npc.attack(pc)");
                    npc.Attack(pc);
                    break;
                case 120:
                    Trace.Assert(originalScript == "smig_backup_2(npc,pc)");
                    smig_backup_2(npc, pc);
                    break;
                case 130:
                    Trace.Assert(originalScript == "smig_backup(npc,pc)");
                    smig_backup(npc, pc);
                    break;
                default:
                    Trace.Assert(originalScript == null);
                    return;
            }
        }
        public bool TryGetSkillChecks(int lineNumber, out DialogSkillChecks skillChecks)
        {
            switch (lineNumber)
            {
                case 41:
                case 42:
                    skillChecks = new DialogSkillChecks(SkillId.sense_motive, 10);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}

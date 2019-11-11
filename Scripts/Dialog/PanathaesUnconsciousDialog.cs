
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
    [DialogScript(464)]
    public class PanathaesUnconsciousDialog : PanathaesUnconscious, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 211:
                    originalScript = "game.global_vars[561] == 0 and game.global_flags[543] == 0";
                    return GetGlobalVar(561) == 0 && !GetGlobalFlag(543);
                case 212:
                    originalScript = "game.global_vars[561] == 1 and game.global_flags[543] == 0";
                    return GetGlobalVar(561) == 1 && !GetGlobalFlag(543);
                case 213:
                    originalScript = "game.global_vars[561] == 2 and game.global_flags[543] == 0";
                    return GetGlobalVar(561) == 2 && !GetGlobalFlag(543);
                case 214:
                    originalScript = "game.global_vars[561] == 0 and game.global_flags[543] == 1";
                    return GetGlobalVar(561) == 0 && GetGlobalFlag(543);
                case 221:
                    originalScript = "game.global_flags[530] == 0";
                    return !GetGlobalFlag(530);
                case 222:
                case 231:
                case 321:
                case 331:
                case 341:
                    originalScript = "pc.skill_level_get(npc,skill_sense_motive) >= 30";
                    return pc.GetSkillLevel(npc, SkillId.sense_motive) >= 30;
                case 223:
                case 232:
                case 284:
                case 294:
                case 304:
                case 314:
                case 322:
                case 332:
                case 342:
                    originalScript = "pc.skill_level_get(npc,skill_sense_motive) <= 29";
                    return pc.GetSkillLevel(npc, SkillId.sense_motive) <= 29;
                case 241:
                    originalScript = "game.global_vars[540] == 1 and game.global_vars[541] == 1 and pc.skill_level_get(npc,skill_sense_motive) >= 30";
                    return GetGlobalVar(540) == 1 && GetGlobalVar(541) == 1 && pc.GetSkillLevel(npc, SkillId.sense_motive) >= 30;
                case 242:
                    originalScript = "game.global_vars[540] == 2 and game.global_vars[541] == 2 and pc.skill_level_get(npc,skill_sense_motive) >= 30";
                    return GetGlobalVar(540) == 2 && GetGlobalVar(541) == 2 && pc.GetSkillLevel(npc, SkillId.sense_motive) >= 30;
                case 243:
                    originalScript = "game.global_vars[540] == 3 and game.global_vars[541] == 3 and pc.skill_level_get(npc,skill_sense_motive) >= 30";
                    return GetGlobalVar(540) == 3 && GetGlobalVar(541) == 3 && pc.GetSkillLevel(npc, SkillId.sense_motive) >= 30;
                case 244:
                    originalScript = "game.global_vars[540] == 4 and game.global_vars[541] == 4 and pc.skill_level_get(npc,skill_sense_motive) >= 30";
                    return GetGlobalVar(540) == 4 && GetGlobalVar(541) == 4 && pc.GetSkillLevel(npc, SkillId.sense_motive) >= 30;
                case 251:
                    originalScript = "pc.skill_level_get(npc,skill_diplomacy) >= 19";
                    return pc.GetSkillLevel(npc, SkillId.diplomacy) >= 19;
                case 252:
                    originalScript = "pc.skill_level_get(npc,skill_bluff) >= 19";
                    return pc.GetSkillLevel(npc, SkillId.bluff) >= 19;
                case 253:
                    originalScript = "pc.skill_level_get(npc,skill_intimidate) >= 19";
                    return pc.GetSkillLevel(npc, SkillId.intimidate) >= 19;
                case 254:
                    originalScript = "pc.skill_level_get(npc,skill_diplomacy) <= 18";
                    return pc.GetSkillLevel(npc, SkillId.diplomacy) <= 18;
                case 255:
                    originalScript = "pc.skill_level_get(npc,skill_bluff) <= 18";
                    return pc.GetSkillLevel(npc, SkillId.bluff) <= 18;
                case 256:
                    originalScript = "pc.skill_level_get(npc,skill_intimidate) <= 18";
                    return pc.GetSkillLevel(npc, SkillId.intimidate) <= 18;
                case 261:
                    originalScript = "game.global_vars[540] == 1 and game.global_vars[541] == 1 and pc.skill_level_get(npc,skill_diplomacy) >= 20";
                    return GetGlobalVar(540) == 1 && GetGlobalVar(541) == 1 && pc.GetSkillLevel(npc, SkillId.diplomacy) >= 20;
                case 262:
                    originalScript = "game.global_vars[540] == 2 and game.global_vars[541] == 2 and pc.skill_level_get(npc,skill_diplomacy) >= 20";
                    return GetGlobalVar(540) == 2 && GetGlobalVar(541) == 2 && pc.GetSkillLevel(npc, SkillId.diplomacy) >= 20;
                case 263:
                    originalScript = "game.global_vars[540] == 3 and game.global_vars[541] == 3 and pc.skill_level_get(npc,skill_diplomacy) >= 20";
                    return GetGlobalVar(540) == 3 && GetGlobalVar(541) == 3 && pc.GetSkillLevel(npc, SkillId.diplomacy) >= 20;
                case 264:
                    originalScript = "game.global_vars[540] == 4 and game.global_vars[541] == 4 and pc.skill_level_get(npc,skill_diplomacy) >= 20";
                    return GetGlobalVar(540) == 4 && GetGlobalVar(541) == 4 && pc.GetSkillLevel(npc, SkillId.diplomacy) >= 20;
                case 265:
                    originalScript = "game.global_vars[540] == 1 and game.global_vars[541] == 1 and pc.skill_level_get(npc,skill_bluff) >= 20";
                    return GetGlobalVar(540) == 1 && GetGlobalVar(541) == 1 && pc.GetSkillLevel(npc, SkillId.bluff) >= 20;
                case 266:
                    originalScript = "game.global_vars[540] == 2 and game.global_vars[541] == 2 and pc.skill_level_get(npc,skill_bluff) >= 20";
                    return GetGlobalVar(540) == 2 && GetGlobalVar(541) == 2 && pc.GetSkillLevel(npc, SkillId.bluff) >= 20;
                case 267:
                    originalScript = "game.global_vars[540] == 3 and game.global_vars[541] == 3 and pc.skill_level_get(npc,skill_bluff) >= 20";
                    return GetGlobalVar(540) == 3 && GetGlobalVar(541) == 3 && pc.GetSkillLevel(npc, SkillId.bluff) >= 20;
                case 268:
                    originalScript = "game.global_vars[540] == 4 and game.global_vars[541] == 4 and pc.skill_level_get(npc,skill_bluff) >= 20";
                    return GetGlobalVar(540) == 4 && GetGlobalVar(541) == 4 && pc.GetSkillLevel(npc, SkillId.bluff) >= 20;
                case 269:
                    originalScript = "game.global_vars[540] == 1 and game.global_vars[541] == 1 and pc.skill_level_get(npc,skill_intimidate) >= 20";
                    return GetGlobalVar(540) == 1 && GetGlobalVar(541) == 1 && pc.GetSkillLevel(npc, SkillId.intimidate) >= 20;
                case 270:
                    originalScript = "game.global_vars[540] == 2 and game.global_vars[541] == 2 and pc.skill_level_get(npc,skill_intimidate) >= 20";
                    return GetGlobalVar(540) == 2 && GetGlobalVar(541) == 2 && pc.GetSkillLevel(npc, SkillId.intimidate) >= 20;
                case 271:
                    originalScript = "game.global_vars[540] == 3 and game.global_vars[541] == 3 and pc.skill_level_get(npc,skill_intimidate) >= 20";
                    return GetGlobalVar(540) == 3 && GetGlobalVar(541) == 3 && pc.GetSkillLevel(npc, SkillId.intimidate) >= 20;
                case 272:
                    originalScript = "game.global_vars[540] == 4 and game.global_vars[541] == 4 and pc.skill_level_get(npc,skill_intimidate) >= 20";
                    return GetGlobalVar(540) == 4 && GetGlobalVar(541) == 4 && pc.GetSkillLevel(npc, SkillId.intimidate) >= 20;
                case 273:
                    originalScript = "pc.skill_level_get(npc,skill_diplomacy) <= 19";
                    return pc.GetSkillLevel(npc, SkillId.diplomacy) <= 19;
                case 274:
                    originalScript = "pc.skill_level_get(npc,skill_bluff) <= 19";
                    return pc.GetSkillLevel(npc, SkillId.bluff) <= 19;
                case 275:
                    originalScript = "pc.skill_level_get(npc,skill_intimidate) <= 19";
                    return pc.GetSkillLevel(npc, SkillId.intimidate) <= 19;
                case 281:
                case 291:
                case 301:
                case 311:
                    originalScript = "game.global_vars[542] == 1 and pc.skill_level_get(npc,skill_sense_motive) >= 30";
                    return GetGlobalVar(542) == 1 && pc.GetSkillLevel(npc, SkillId.sense_motive) >= 30;
                case 282:
                case 292:
                case 302:
                case 312:
                    originalScript = "game.global_vars[542] == 2 and pc.skill_level_get(npc,skill_sense_motive) >= 30";
                    return GetGlobalVar(542) == 2 && pc.GetSkillLevel(npc, SkillId.sense_motive) >= 30;
                case 283:
                case 293:
                case 303:
                case 313:
                    originalScript = "game.global_vars[542] == 3 and pc.skill_level_get(npc,skill_sense_motive) >= 30";
                    return GetGlobalVar(542) == 3 && pc.GetSkillLevel(npc, SkillId.sense_motive) >= 30;
                case 351:
                    originalScript = "game.global_vars[542] == 1 and pc.skill_level_get(npc,skill_diplomacy) >= 21";
                    return GetGlobalVar(542) == 1 && pc.GetSkillLevel(npc, SkillId.diplomacy) >= 21;
                case 352:
                    originalScript = "game.global_vars[542] == 2 and pc.skill_level_get(npc,skill_diplomacy) >= 21";
                    return GetGlobalVar(542) == 2 && pc.GetSkillLevel(npc, SkillId.diplomacy) >= 21;
                case 353:
                    originalScript = "game.global_vars[542] == 3 and pc.skill_level_get(npc,skill_diplomacy) >= 21";
                    return GetGlobalVar(542) == 3 && pc.GetSkillLevel(npc, SkillId.diplomacy) >= 21;
                case 354:
                    originalScript = "game.global_vars[542] == 1 and pc.skill_level_get(npc,skill_bluff) >= 21";
                    return GetGlobalVar(542) == 1 && pc.GetSkillLevel(npc, SkillId.bluff) >= 21;
                case 355:
                    originalScript = "game.global_vars[542] == 2 and pc.skill_level_get(npc,skill_bluff) >= 21";
                    return GetGlobalVar(542) == 2 && pc.GetSkillLevel(npc, SkillId.bluff) >= 21;
                case 356:
                    originalScript = "game.global_vars[542] == 3 and pc.skill_level_get(npc,skill_bluff) >= 21";
                    return GetGlobalVar(542) == 3 && pc.GetSkillLevel(npc, SkillId.bluff) >= 21;
                case 357:
                    originalScript = "game.global_vars[542] == 1 and pc.skill_level_get(npc,skill_intimidate) >= 21";
                    return GetGlobalVar(542) == 1 && pc.GetSkillLevel(npc, SkillId.intimidate) >= 21;
                case 358:
                    originalScript = "game.global_vars[542] == 2 and pc.skill_level_get(npc,skill_intimidate) >= 21";
                    return GetGlobalVar(542) == 2 && pc.GetSkillLevel(npc, SkillId.intimidate) >= 21;
                case 359:
                    originalScript = "game.global_vars[542] == 3 and pc.skill_level_get(npc,skill_intimidate) >= 21";
                    return GetGlobalVar(542) == 3 && pc.GetSkillLevel(npc, SkillId.intimidate) >= 21;
                case 360:
                    originalScript = "pc.skill_level_get(npc,skill_diplomacy) <= 20";
                    return pc.GetSkillLevel(npc, SkillId.diplomacy) <= 20;
                case 361:
                    originalScript = "pc.skill_level_get(npc,skill_bluff) <= 20";
                    return pc.GetSkillLevel(npc, SkillId.bluff) <= 20;
                case 362:
                    originalScript = "pc.skill_level_get(npc,skill_intimidate) <= 20";
                    return pc.GetSkillLevel(npc, SkillId.intimidate) <= 20;
                case 372:
                case 382:
                case 392:
                    originalScript = "game.global_vars[551] == 0 and pc.follower_atmax() == 0";
                    return GetGlobalVar(551) == 0 && !pc.HasMaxFollowers();
                default:
                    originalScript = null;
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 200:
                    originalScript = "game.global_flags[533] = 1";
                    SetGlobalFlag(533, true);
                    break;
                case 230:
                    originalScript = "game.global_flags[530] = 1";
                    SetGlobalFlag(530, true);
                    break;
                case 281:
                    originalScript = "game.global_vars[561] = 2";
                    SetGlobalVar(561, 2);
                    break;
                case 282:
                    originalScript = "game.global_flags[543] = 1";
                    SetGlobalFlag(543, true);
                    break;
                case 283:
                    originalScript = "game.global_vars[561] = 1";
                    SetGlobalVar(561, 1);
                    break;
                case 320:
                    originalScript = "increment_var_544(npc,pc); increment_var_556(npc,pc); check_evidence_rep_bor(npc,pc)";
                    increment_var_544(npc, pc);
                    increment_var_556(npc, pc);
                    check_evidence_rep_bor(npc, pc);
                    ;
                    break;
                case 330:
                    originalScript = "increment_var_543(npc,pc); check_evidence_rep_pan(npc,pc)";
                    increment_var_543(npc, pc);
                    check_evidence_rep_pan(npc, pc);
                    ;
                    break;
                case 340:
                    originalScript = "increment_var_545(npc,pc); increment_var_557(npc,pc); check_evidence_rep_rak(npc,pc)";
                    increment_var_545(npc, pc);
                    increment_var_557(npc, pc);
                    check_evidence_rep_rak(npc, pc);
                    ;
                    break;
                case 372:
                case 382:
                case 392:
                    originalScript = "gather_panathaes(npc,pc)";
                    gather_panathaes(npc, pc);
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
                case 222:
                case 231:
                case 241:
                case 242:
                case 243:
                case 244:
                case 281:
                case 282:
                case 283:
                case 291:
                case 292:
                case 293:
                case 301:
                case 302:
                case 303:
                case 311:
                case 312:
                case 313:
                case 321:
                case 331:
                case 341:
                    skillChecks = new DialogSkillChecks(SkillId.sense_motive, 30);
                    return true;
                case 251:
                    skillChecks = new DialogSkillChecks(SkillId.diplomacy, 19);
                    return true;
                case 252:
                    skillChecks = new DialogSkillChecks(SkillId.bluff, 19);
                    return true;
                case 253:
                    skillChecks = new DialogSkillChecks(SkillId.intimidate, 19);
                    return true;
                case 261:
                case 262:
                case 263:
                case 264:
                    skillChecks = new DialogSkillChecks(SkillId.diplomacy, 20);
                    return true;
                case 265:
                case 266:
                case 267:
                case 268:
                    skillChecks = new DialogSkillChecks(SkillId.bluff, 20);
                    return true;
                case 269:
                case 270:
                case 271:
                case 272:
                    skillChecks = new DialogSkillChecks(SkillId.intimidate, 20);
                    return true;
                case 351:
                case 352:
                case 353:
                    skillChecks = new DialogSkillChecks(SkillId.diplomacy, 21);
                    return true;
                case 354:
                case 355:
                case 356:
                    skillChecks = new DialogSkillChecks(SkillId.bluff, 21);
                    return true;
                case 357:
                case 358:
                case 359:
                    skillChecks = new DialogSkillChecks(SkillId.intimidate, 21);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}

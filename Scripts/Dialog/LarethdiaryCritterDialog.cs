
using System;
using System.Collections.Generic;
using System.Diagnostics;
using OpenTemple.Core.GameObjects;
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
    [DialogScript(221)]
    public class LarethdiaryCritterDialog : LarethdiaryCritter, IDialogScript
    {
        public bool CheckPrecondition(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 201:
                    originalScript = "pc.skill_level_get(npc,skill_open_lock) == 10 and game.global_vars[701] == 0";
                    throw new NotSupportedException("Conversion failed.");
                case 202:
                    originalScript = "pc.skill_level_get(npc,skill_open_lock) >= 11 and game.global_vars[701] == 0";
                    throw new NotSupportedException("Conversion failed.");
                case 203:
                    originalScript = "pc.skill_level_get(npc,skill_open_lock) <= 9 and game.global_vars[701] == 0";
                    throw new NotSupportedException("Conversion failed.");
                case 204:
                    originalScript = "pc.skill_level_get(npc,skill_open_lock) == 10 and game.global_vars[701] == 1";
                    throw new NotSupportedException("Conversion failed.");
                case 205:
                    originalScript = "pc.skill_level_get(npc,skill_open_lock) >= 11 and game.global_vars[701] == 1";
                    throw new NotSupportedException("Conversion failed.");
                case 206:
                    originalScript = "pc.skill_level_get(npc,skill_open_lock) <= 9 and game.global_vars[701] == 1";
                    throw new NotSupportedException("Conversion failed.");
                case 220:
                    originalScript = "pc.skill_level_get(npc,skill_search) == 8 and game.global_vars[701] == 0";
                    throw new NotSupportedException("Conversion failed.");
                case 221:
                    originalScript = "pc.skill_level_get(npc,skill_search) >= 9 and game.global_vars[701] == 0";
                    throw new NotSupportedException("Conversion failed.");
                case 222:
                    originalScript = "pc.skill_level_get(npc,skill_search) <= 7 and game.global_vars[701] == 0";
                    throw new NotSupportedException("Conversion failed.");
                case 411:
                case 506:
                case 516:
                case 530:
                case 606:
                case 616:
                case 625:
                case 651:
                    originalScript = "pc.skill_level_get(npc,skill_open_lock) == 10";
                    throw new NotSupportedException("Conversion failed.");
                case 412:
                case 422:
                    originalScript = "pc.skill_level_get(npc,skill_search) == 8";
                    throw new NotSupportedException("Conversion failed.");
                case 413:
                case 423:
                    originalScript = "pc.skill_level_get(npc,skill_search) >= 9";
                    throw new NotSupportedException("Conversion failed.");
                case 414:
                case 424:
                    originalScript = "pc.skill_level_get(npc,skill_search) <= 7";
                    throw new NotSupportedException("Conversion failed.");
                case 421:
                case 508:
                case 518:
                case 532:
                case 608:
                case 618:
                case 627:
                case 653:
                    originalScript = "pc.skill_level_get(npc,skill_open_lock) <= 9";
                    throw new NotSupportedException("Conversion failed.");
                case 501:
                case 621:
                    originalScript = "pc.skill_level_get(skill_disable_device) == 8";
                    throw new NotSupportedException("Conversion failed.");
                case 502:
                case 622:
                    originalScript = "pc.skill_level_get(skill_disable_device) >= 9";
                    throw new NotSupportedException("Conversion failed.");
                case 503:
                case 623:
                    originalScript = "pc.skill_level_get(skill_disable_device) <= 7 and pc.skill_level_get(skill_disable_device) >= 5";
                    throw new NotSupportedException("Conversion failed.");
                case 505:
                case 624:
                    originalScript = "pc.skill_level_get(skill_disable_device) <= 4";
                    throw new NotSupportedException("Conversion failed.");
                case 507:
                case 517:
                case 531:
                case 607:
                case 617:
                case 626:
                case 652:
                    originalScript = "pc.skill_level_get(npc,skill_open_lock) >= 11";
                    throw new NotSupportedException("Conversion failed.");
                case 611:
                    originalScript = "pc.skill_level_get(skill_disable_device) == 7";
                    throw new NotSupportedException("Conversion failed.");
                case 613:
                    originalScript = "pc.skill_level_get(skill_disable_device) <= 6";
                    throw new NotSupportedException("Conversion failed.");
                default:
                    originalScript = null;
                    return true;
            }
        }
        public void ApplySideEffect(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 1:
                    originalScript = "game.areas[3] = 1; game.story_state = 3; game.global_vars[701] = 2";
                    MakeAreaKnown(3);
                    StoryState = 3;
                    SetGlobalVar(701, 2);
                    ;
                    break;
                case 6:
                case 7:
                case 13:
                case 23:
                case 32:
                case 43:
                case 53:
                case 63:
                case 73:
                case 82:
                case 93:
                case 103:
                case 113:
                case 122:
                case 133:
                case 143:
                case 153:
                case 162:
                case 175:
                case 230:
                case 315:
                case 325:
                case 415:
                case 425:
                case 509:
                case 519:
                case 533:
                case 609:
                case 619:
                case 654:
                    originalScript = "npc.destroy()";
                    npc.Destroy();
                    break;
                case 401:
                case 621:
                case 622:
                case 623:
                case 624:
                case 625:
                case 626:
                case 627:
                    originalScript = "pc.condition_add_with_args(\"Poisoned\",11,0)";
                    pc.AddCondition("Poisoned", 11, 0);
                    break;
                case 628:
                    originalScript = "pc.condition_add_with_args(\"Poisoned\",11,0); npc.destroy()";
                    pc.AddCondition("Poisoned", 11, 0);
                    npc.Destroy();
                    ;
                    break;
                case 650:
                    originalScript = "game.global_vars[701] = 1";
                    SetGlobalVar(701, 1);
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

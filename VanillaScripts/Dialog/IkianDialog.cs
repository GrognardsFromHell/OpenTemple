
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

namespace VanillaScripts.Dialog
{
    [DialogScript(201)]
    public class IkianDialog : Ikian, IDialogScript
    {
        public bool CheckPrecondition(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 33:
                case 34:
                    originalScript = "game.story_state == 1";
                    return StoryState == 1;
                case 35:
                case 36:
                    originalScript = "game.story_state == 2";
                    return StoryState == 2;
                case 37:
                case 38:
                    originalScript = "game.story_state == 3";
                    return StoryState == 3;
                case 39:
                case 40:
                    originalScript = "game.story_state == 4";
                    return StoryState == 4;
                case 41:
                case 42:
                    originalScript = "game.story_state == 5";
                    return StoryState == 5;
                case 43:
                case 44:
                    originalScript = "game.story_state == 6";
                    return StoryState == 6;
                default:
                    originalScript = null;
                    return true;
            }
        }
        public void ApplySideEffect(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 53:
                    originalScript = "all_run_off(npc,pc)";
                    all_run_off(npc, pc);
                    break;
                case 81:
                    originalScript = "npc.attack(pc)";
                    npc.Attack(pc);
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


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

namespace VanillaScripts.Dialog
{
    [DialogScript(128)]
    public class JufferDialog : Juffer, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 2:
                case 3:
                    originalScript = "game.party_alignment != CHAOTIC_EVIL and game.party_alignment != NEUTRAL_EVIL";
                    return PartyAlignment != Alignment.CHAOTIC_EVIL && PartyAlignment != Alignment.NEUTRAL_EVIL;
                default:
                    originalScript = null;
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 2:
                case 31:
                    originalScript = "game.global_flags[127] = 1";
                    SetGlobalFlag(127, true);
                    break;
                case 4:
                case 5:
                    originalScript = "jufferlaugh(npc,pc,150)";
                    jufferlaugh(npc, pc, 150);
                    break;
                case 21:
                case 22:
                    originalScript = "jufferhelp(npc,pc,160)";
                    jufferhelp(npc, pc, 160);
                    break;
                case 51:
                case 52:
                case 61:
                case 62:
                    originalScript = "run_off( npc, pc )";
                    run_off(npc, pc);
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

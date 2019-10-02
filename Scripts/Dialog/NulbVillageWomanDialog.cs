
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
    [DialogScript(323)]
    public class NulbVillageWomanDialog : NulbVillageWoman, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 1001:
                    Trace.Assert(originalScript == "game.leader.reputation_has(25) == 0");
                    return !SelectedPartyLeader.HasReputation(25);
                case 1002:
                    Trace.Assert(originalScript == "game.leader.reputation_has(25) == 1");
                    return SelectedPartyLeader.HasReputation(25);
                default:
                    Trace.Assert(originalScript == null);
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 1000:
                    Trace.Assert(originalScript == "game.global_vars[958] = 2; get_sick(npc,pc)");
                    SetGlobalVar(958, 2);
                    get_sick(npc, pc);
                    ;
                    break;
                case 1011:
                case 1021:
                    Trace.Assert(originalScript == "game.fade_and_teleport(0,0,0,5090,487,484); game.fade(28800,0,0,12)");
                    FadeAndTeleport(0, 0, 0, 5090, 487, 484);
                    Fade(28800, 0, 0, 12);
                    ;
                    break;
                case 2000:
                    Trace.Assert(originalScript == "game.global_vars[958] = 10");
                    SetGlobalVar(958, 10);
                    break;
                case 2011:
                case 5011:
                    Trace.Assert(originalScript == "game.fade(28800,0,0,12)");
                    Fade(28800, 0, 0, 12);
                    break;
                case 3000:
                    Trace.Assert(originalScript == "game.particles( 'sp-Blink', npc ); game.global_vars[958] = 4");
                    AttachParticles("sp-Blink", npc);
                    SetGlobalVar(958, 4);
                    ;
                    break;
                case 3011:
                    Trace.Assert(originalScript == "all_die(npc,pc)");
                    all_die(npc, pc);
                    break;
                case 3020:
                    Trace.Assert(originalScript == "game.particles( 'sp-Blink', npc ); game.particles( 'sp-Wind Wall', npc )");
                    AttachParticles("sp-Blink", npc);
                    AttachParticles("sp-Wind Wall", npc);
                    ;
                    break;
                case 3021:
                    Trace.Assert(originalScript == "game.fade_and_teleport(0,0,0,5051,481,445)");
                    FadeAndTeleport(0, 0, 0, 5051, 481, 445);
                    break;
                case 4000:
                    Trace.Assert(originalScript == "game.global_vars[958] = 5");
                    SetGlobalVar(958, 5);
                    break;
                case 4030:
                    Trace.Assert(originalScript == "stop_watch(npc,pc)");
                    stop_watch(npc, pc);
                    break;
                case 4031:
                    Trace.Assert(originalScript == "npc.destroy()");
                    npc.Destroy();
                    break;
                case 5000:
                    Trace.Assert(originalScript == "game.global_vars[958] = 3");
                    SetGlobalVar(958, 3);
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
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}

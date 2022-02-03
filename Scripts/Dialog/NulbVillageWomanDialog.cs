
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

namespace Scripts.Dialog;

[DialogScript(323)]
public class NulbVillageWomanDialog : NulbVillageWoman, IDialogScript
{
    public bool CheckPrecondition(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
    {
        switch (lineNumber)
        {
            case 1001:
                originalScript = "game.leader.reputation_has(25) == 0";
                return !SelectedPartyLeader.HasReputation(25);
            case 1002:
                originalScript = "game.leader.reputation_has(25) == 1";
                return SelectedPartyLeader.HasReputation(25);
            default:
                originalScript = null;
                return true;
        }
    }
    public void ApplySideEffect(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
    {
        switch (lineNumber)
        {
            case 1000:
                originalScript = "game.global_vars[958] = 2; get_sick(npc,pc)";
                SetGlobalVar(958, 2);
                get_sick(npc, pc);
                ;
                break;
            case 1011:
            case 1021:
                originalScript = "game.fade_and_teleport(0,0,0,5090,487,484); game.fade(28800,0,0,12)";
                FadeAndTeleport(0, 0, 0, 5090, 487, 484);
                Fade(28800, 0, 0, 12);
                ;
                break;
            case 2000:
                originalScript = "game.global_vars[958] = 10";
                SetGlobalVar(958, 10);
                break;
            case 2011:
            case 5011:
                originalScript = "game.fade(28800,0,0,12)";
                Fade(28800, 0, 0, 12);
                break;
            case 3000:
                originalScript = "game.particles( 'sp-Blink', npc ); game.global_vars[958] = 4";
                AttachParticles("sp-Blink", npc);
                SetGlobalVar(958, 4);
                ;
                break;
            case 3011:
                originalScript = "all_die(npc,pc)";
                all_die(npc, pc);
                break;
            case 3020:
                originalScript = "game.particles( 'sp-Blink', npc ); game.particles( 'sp-Wind Wall', npc )";
                AttachParticles("sp-Blink", npc);
                AttachParticles("sp-Wind Wall", npc);
                ;
                break;
            case 3021:
                originalScript = "game.fade_and_teleport(0,0,0,5051,481,445)";
                FadeAndTeleport(0, 0, 0, 5051, 481, 445);
                break;
            case 4000:
                originalScript = "game.global_vars[958] = 5";
                SetGlobalVar(958, 5);
                break;
            case 4030:
                originalScript = "stop_watch(npc,pc)";
                stop_watch(npc, pc);
                break;
            case 4031:
                originalScript = "npc.destroy()";
                npc.Destroy();
                break;
            case 5000:
                originalScript = "game.global_vars[958] = 3";
                SetGlobalVar(958, 3);
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
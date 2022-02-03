
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

[DialogScript(284)]
public class OrboftheMoonsDialog : OrboftheMoons, IDialogScript
{
    public bool CheckPrecondition(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
    {
        switch (lineNumber)
        {
            case 12:
                originalScript = "game.global_flags[94] == 1";
                return GetGlobalFlag(94);
            case 13:
                originalScript = "game.global_flags[94] == 0";
                return !GetGlobalFlag(94);
            default:
                originalScript = null;
                return true;
        }
    }
    public void ApplySideEffect(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
    {
        switch (lineNumber)
        {
            case 11:
                originalScript = "game.particles( \"sp-Dimension Door\", pc ); game.fade_and_teleport(0,0,0,5008,465,481); npc.destroy()";
                AttachParticles("sp-Dimension Door", pc);
                FadeAndTeleport(0, 0, 0, 5008, 465, 481);
                npc.Destroy();
                ;
                break;
            case 12:
                originalScript = "game.particles( \"sp-Dimension Door\", pc ); game.fade_and_teleport(0,0,0,5085,480,480); npc.destroy()";
                AttachParticles("sp-Dimension Door", pc);
                FadeAndTeleport(0, 0, 0, 5085, 480, 480);
                npc.Destroy();
                ;
                break;
            case 13:
                originalScript = "game.particles( \"sp-Dimension Door\", pc ); game.fade_and_teleport(0,0,0,5051,506,360); npc.destroy()";
                AttachParticles("sp-Dimension Door", pc);
                FadeAndTeleport(0, 0, 0, 5051, 506, 360);
                npc.Destroy();
                ;
                break;
            case 14:
                originalScript = "game.particles( \"sp-Dimension Door\", pc ); game.fade_and_teleport(0,0,0,5080,479,590); npc.destroy()";
                AttachParticles("sp-Dimension Door", pc);
                FadeAndTeleport(0, 0, 0, 5080, 479, 590);
                npc.Destroy();
                ;
                break;
            case 15:
                originalScript = "npc.destroy()";
                npc.Destroy();
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
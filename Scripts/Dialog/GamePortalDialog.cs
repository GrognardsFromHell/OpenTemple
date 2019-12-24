
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
    [DialogScript(406)]
    public class GamePortalDialog : GamePortal, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 2:
                case 21:
                    originalScript = "(game.party_alignment == LAWFUL_GOOD)";
                    return (PartyAlignment == Alignment.LAWFUL_GOOD);
                case 3:
                case 22:
                    originalScript = "(game.party_alignment == NEUTRAL_GOOD)";
                    return (PartyAlignment == Alignment.NEUTRAL_GOOD);
                case 4:
                case 23:
                    originalScript = "(game.party_alignment == CHAOTIC_GOOD)";
                    return (PartyAlignment == Alignment.CHAOTIC_GOOD);
                case 5:
                case 24:
                    originalScript = "(game.party_alignment == LAWFUL_NEUTRAL)";
                    return (PartyAlignment == Alignment.LAWFUL_NEUTRAL);
                case 6:
                case 25:
                    originalScript = "(game.party_alignment == TRUE_NEUTRAL)";
                    return (PartyAlignment == Alignment.NEUTRAL);
                case 7:
                case 26:
                    originalScript = "(game.party_alignment == CHAOTIC_NEUTRAL)";
                    return (PartyAlignment == Alignment.CHAOTIC_NEUTRAL);
                case 8:
                case 27:
                    originalScript = "(game.party_alignment == LAWFUL_EVIL)";
                    return (PartyAlignment == Alignment.LAWFUL_EVIL);
                case 9:
                case 28:
                    originalScript = "(game.party_alignment == NEUTRAL_EVIL)";
                    return (PartyAlignment == Alignment.NEUTRAL_EVIL);
                case 10:
                case 29:
                    originalScript = "(game.party_alignment == CHAOTIC_EVIL)";
                    return (PartyAlignment == Alignment.CHAOTIC_EVIL);
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
                    originalScript = "game.particles( 'ef-NODE-Air portal', npc ); game.sound( 4008 ); game.global_flags[601] = 1";
                    AttachParticles("ef-NODE-Air portal", npc);
                    Sound(4008);
                    SetGlobalFlag(601, true);
                    ;
                    break;
                case 2:
                case 21:
                    originalScript = "intro_movie_setup(npc,pc); game.fade_and_teleport( 0, 0, 0, 5096, 488, 484 )";
                    intro_movie_setup(npc, pc);
                    FadeAndTeleport(0, 0, 0, 5096, 488, 484);
                    ;
                    break;
                case 3:
                case 22:
                    originalScript = "intro_movie_setup(npc,pc); game.fade_and_teleport( 0, 0, 0, 5101, 505, 488 )";
                    intro_movie_setup(npc, pc);
                    FadeAndTeleport(0, 0, 0, 5101, 505, 488);
                    ;
                    break;
                case 4:
                case 23:
                    originalScript = "intro_movie_setup(npc,pc); game.fade_and_teleport(0, 0, 0, 5097, 484, 477 )";
                    intro_movie_setup(npc, pc);
                    FadeAndTeleport(0, 0, 0, 5097, 484, 477);
                    ;
                    break;
                case 5:
                case 24:
                    originalScript = "intro_movie_setup(npc,pc); game.fade_and_teleport(0, 0, 0, 5103, 500, 483)";
                    intro_movie_setup(npc, pc);
                    FadeAndTeleport(0, 0, 0, 5103, 500, 483);
                    ;
                    break;
                case 6:
                case 25:
                    originalScript = "intro_movie_setup(npc,pc); game.fade_and_teleport( 0, 0, 0, 5100, 495, 533 )";
                    intro_movie_setup(npc, pc);
                    FadeAndTeleport(0, 0, 0, 5100, 495, 533);
                    ;
                    break;
                case 7:
                case 26:
                    originalScript = "intro_movie_setup(npc,pc); game.fade_and_teleport(0, 0, 0, 5104, 510, 492 )";
                    intro_movie_setup(npc, pc);
                    FadeAndTeleport(0, 0, 0, 5104, 510, 492);
                    ;
                    break;
                case 8:
                case 27:
                    originalScript = "intro_movie_setup(npc,pc); game.fade_and_teleport(0, 0, 0, 5098, 492, 489 )";
                    intro_movie_setup(npc, pc);
                    FadeAndTeleport(0, 0, 0, 5098, 492, 489);
                    ;
                    break;
                case 9:
                case 28:
                    originalScript = "intro_movie_setup(npc,pc); game.fade_and_teleport(0, 0, 0, 5102, 481, 505 )";
                    intro_movie_setup(npc, pc);
                    FadeAndTeleport(0, 0, 0, 5102, 481, 505);
                    ;
                    break;
                case 10:
                case 29:
                    originalScript = "intro_movie_setup(npc,pc); game.fade_and_teleport(0, 0, 0, 5099, 492, 496 )";
                    intro_movie_setup(npc, pc);
                    FadeAndTeleport(0, 0, 0, 5099, 492, 496);
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
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}

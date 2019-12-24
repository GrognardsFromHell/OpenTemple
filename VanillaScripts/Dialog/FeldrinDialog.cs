
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

namespace VanillaScripts.Dialog
{
    [DialogScript(167)]
    public class FeldrinDialog : Feldrin, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 6:
                    originalScript = "game.global_flags[174] == 1";
                    return GetGlobalFlag(174);
                case 7:
                case 8:
                    originalScript = "game.global_flags[175] == 0";
                    return !GetGlobalFlag(175);
                case 11:
                case 12:
                    originalScript = "game.party_alignment != LAWFUL_GOOD";
                    return PartyAlignment != Alignment.LAWFUL_GOOD;
                case 13:
                case 14:
                    originalScript = "( game.party_alignment == LAWFUL_GOOD or game.party_alignment == NEUTRAL_GOOD or game.party_alignment == CHAOTIC_GOOD )";
                    return (PartyAlignment == Alignment.LAWFUL_GOOD || PartyAlignment == Alignment.NEUTRAL_GOOD || PartyAlignment == Alignment.CHAOTIC_GOOD);
                case 17:
                case 18:
                    originalScript = "game.party_alignment == CHAOTIC_EVIL or game.party_alignment == NEUTRAL_EVIL";
                    return PartyAlignment == Alignment.CHAOTIC_EVIL || PartyAlignment == Alignment.NEUTRAL_EVIL;
                case 19:
                    originalScript = "game.party_alignment == TRUE_NEUTRAL";
                    return PartyAlignment == Alignment.NEUTRAL;
                case 23:
                case 24:
                    originalScript = "game.global_flags[174] == 0 and game.global_flags[178] == 1";
                    return !GetGlobalFlag(174) && GetGlobalFlag(178);
                case 51:
                case 52:
                case 83:
                case 84:
                    originalScript = "game.story_state >= 5";
                    return StoryState >= 5;
                case 55:
                case 56:
                case 92:
                case 93:
                    originalScript = "game.party_alignment == CHAOTIC_EVIL or game.party_alignment == NEUTRAL_EVIL or game.party_alignment == LAWFUL_EVIL or game.party_alignment == CHAOTIC_NEUTRAL or game.party_alignment == CHAOTIC_GOOD";
                    return PartyAlignment == Alignment.CHAOTIC_EVIL || PartyAlignment == Alignment.NEUTRAL_EVIL || PartyAlignment == Alignment.LAWFUL_EVIL || PartyAlignment == Alignment.CHAOTIC_NEUTRAL || PartyAlignment == Alignment.CHAOTIC_GOOD;
                case 81:
                case 82:
                    originalScript = "game.story_state < 5";
                    return StoryState < 5;
                case 161:
                case 162:
                    originalScript = "pc.stat_level_get(stat_race) != race_half_orc";
                    return pc.GetRace() != RaceId.half_orc;
                case 166:
                case 167:
                    originalScript = "pc.stat_level_get(stat_race) == race_half_orc";
                    return pc.GetRace() == RaceId.half_orc;
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
                case 3:
                case 21:
                case 22:
                case 31:
                case 161:
                case 162:
                case 166:
                case 167:
                    originalScript = "npc.attack( pc )";
                    npc.Attack(pc);
                    break;
                case 27:
                case 28:
                case 59:
                case 60:
                case 132:
                case 133:
                    originalScript = "game.global_flags[176] = 1; run_off(npc,pc)";
                    SetGlobalFlag(176, true);
                    run_off(npc, pc);
                    ;
                    break;
                case 81:
                case 82:
                    originalScript = "game.story_state = 5";
                    StoryState = 5;
                    break;
                case 130:
                case 140:
                    originalScript = "game.map_flags( 5067, 1, 1 ); game.map_flags( 5067, 3, 1 ); game.map_flags( 5067, 4, 1 )";
                    // FIXME: map_flags;
                    // FIXME: map_flags;
                    // FIXME: map_flags;
                    ;
                    break;
                case 150:
                    originalScript = "game.map_flags( 5067, 2, 1 )";
                    // FIXME: map_flags;
                    break;
                case 163:
                case 164:
                case 165:
                    originalScript = "run_off(npc,pc)";
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

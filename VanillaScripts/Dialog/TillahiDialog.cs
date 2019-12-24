
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
    [DialogScript(127)]
    public class TillahiDialog : Tillahi, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 2:
                case 3:
                    originalScript = "game.party_alignment == LAWFUL_NEUTRAL or game.party_alignment == LAWFUL_EVIL or game.party_alignment == LAWFUL_GOOD or pc.stat_level_get(stat_race) == race_elf or game.party_alignment == TRUE_NEUTRAL";
                    return PartyAlignment == Alignment.LAWFUL_NEUTRAL || PartyAlignment == Alignment.LAWFUL_EVIL || PartyAlignment == Alignment.LAWFUL_GOOD || pc.GetRace() == RaceId.aquatic_elf || PartyAlignment == Alignment.NEUTRAL;
                case 4:
                case 5:
                    originalScript = "game.party_alignment == CHAOTIC_GOOD or game.party_alignment == NEUTRAL_GOOD or game.party_alignment == LAWFUL_GOOD";
                    return PartyAlignment == Alignment.CHAOTIC_GOOD || PartyAlignment == Alignment.NEUTRAL_GOOD || PartyAlignment == Alignment.LAWFUL_GOOD;
                case 6:
                case 7:
                    originalScript = "game.party_alignment == CHAOTIC_EVIL or game.party_alignment == NEUTRAL_EVIL or game.party_alignment == LAWFUL_EVIL or game.party_alignment == CHAOTIC_NEUTRAL";
                    return PartyAlignment == Alignment.CHAOTIC_EVIL || PartyAlignment == Alignment.NEUTRAL_EVIL || PartyAlignment == Alignment.LAWFUL_EVIL || PartyAlignment == Alignment.CHAOTIC_NEUTRAL;
                case 21:
                case 22:
                case 33:
                case 34:
                case 163:
                case 164:
                case 172:
                case 173:
                    originalScript = "game.party_alignment == CHAOTIC_GOOD";
                    return PartyAlignment == Alignment.CHAOTIC_GOOD;
                case 25:
                case 26:
                    originalScript = "game.quests[3].state == qs_completed or game.quests[4].state == qs_completed";
                    return GetQuestState(3) == QuestState.Completed || GetQuestState(4) == QuestState.Completed;
                case 27:
                case 28:
                    originalScript = "game.party_alignment == CHAOTIC_NEUTRAL";
                    return PartyAlignment == Alignment.CHAOTIC_NEUTRAL;
                case 31:
                case 32:
                case 91:
                case 161:
                case 162:
                case 171:
                    originalScript = "game.party_alignment != CHAOTIC_GOOD";
                    return PartyAlignment != Alignment.CHAOTIC_GOOD;
                case 81:
                    originalScript = "game.party_alignment != LAWFUL_GOOD and game.global_flags[287] == 0";
                    return PartyAlignment != Alignment.LAWFUL_GOOD && !GetGlobalFlag(287);
                case 82:
                case 121:
                case 122:
                    originalScript = "game.party_alignment != LAWFUL_GOOD";
                    return PartyAlignment != Alignment.LAWFUL_GOOD;
                case 83:
                case 84:
                case 123:
                case 124:
                    originalScript = "game.global_flags[287] == 1";
                    return GetGlobalFlag(287);
                case 85:
                case 86:
                    originalScript = "game.party_alignment == LAWFUL_GOOD and game.global_flags[287] == 0";
                    return PartyAlignment == Alignment.LAWFUL_GOOD && !GetGlobalFlag(287);
                case 92:
                    originalScript = "game.global_flags[20] == 0";
                    return !GetGlobalFlag(20);
                case 93:
                case 94:
                    originalScript = "game.party_alignment == CHAOTIC_GOOD and game.global_flags[20] == 1";
                    return PartyAlignment == Alignment.CHAOTIC_GOOD && GetGlobalFlag(20);
                case 125:
                case 126:
                    originalScript = "game.party_alignment == LAWFUL_GOOD";
                    return PartyAlignment == Alignment.LAWFUL_GOOD;
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
                case 4:
                case 5:
                case 27:
                case 28:
                case 101:
                case 102:
                case 131:
                case 132:
                    originalScript = "game.global_flags[127] = 1; get_rep( npc, pc )";
                    SetGlobalFlag(127, true);
                    get_rep(npc, pc);
                    ;
                    break;
                case 65:
                case 66:
                case 141:
                    originalScript = "run_off( npc, pc )";
                    run_off(npc, pc);
                    break;
                case 81:
                case 85:
                case 86:
                case 121:
                case 125:
                case 126:
                    originalScript = "game.global_flags[20] = 1";
                    SetGlobalFlag(20, true);
                    break;
                case 91:
                case 171:
                    originalScript = "set_reward( npc, pc )";
                    set_reward(npc, pc);
                    break;
                case 92:
                    originalScript = "run_off( npc, pc ); game.quests[24].state = qs_botched";
                    run_off(npc, pc);
                    SetQuestState(24, QuestState.Botched);
                    ;
                    break;
                case 93:
                case 94:
                case 172:
                case 173:
                    originalScript = "game.quests[24].state = qs_completed; set_reward( npc, pc )";
                    SetQuestState(24, QuestState.Completed);
                    set_reward(npc, pc);
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

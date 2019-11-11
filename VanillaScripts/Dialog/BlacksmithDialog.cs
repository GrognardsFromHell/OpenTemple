
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
    [DialogScript(59)]
    public class BlacksmithDialog : Blacksmith, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 2:
                case 3:
                    originalScript = "not npc.has_met( pc )";
                    return !npc.HasMet(pc);
                case 4:
                case 5:
                    originalScript = "npc.has_met( pc )";
                    return npc.HasMet(pc);
                case 41:
                case 42:
                    originalScript = "game.areas[3] = 0 and game.story_state < 3";
                    throw new NotSupportedException("Conversion failed.");
                case 43:
                case 44:
                    originalScript = "game.global_flags[36] == 0";
                    return !GetGlobalFlag(36);
                case 45:
                case 46:
                case 47:
                    originalScript = "game.global_flags[36] == 1";
                    return GetGlobalFlag(36);
                case 111:
                case 113:
                    originalScript = "pc.money_get() >= 10000";
                    return pc.GetMoney() >= 10000;
                case 112:
                case 114:
                    originalScript = "pc.money_get() < 10000";
                    return pc.GetMoney() < 10000;
                default:
                    originalScript = null;
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 10:
                    originalScript = "game.areas[3] = 1; game.story_state = 3";
                    MakeAreaKnown(3);
                    StoryState = 3;
                    ;
                    break;
                case 12:
                case 13:
                    originalScript = "game.worldmap_travel_by_dialog(3)";
                    // FIXME: worldmap_travel_by_dialog;
                    break;
                case 70:
                case 80:
                    originalScript = "game.global_flags[36] = 1";
                    SetGlobalFlag(36, true);
                    break;
                case 90:
                    originalScript = "game.picker( npc, spell_cure_light_wounds, should_heal_hp_on, [ 100, 20, 110 ] )";
                    // FIXME: picker;
                    break;
                case 111:
                case 113:
                    originalScript = "pc.money_adj(-10000); npc.cast_spell( spell_cure_light_wounds, picker_obj )";
                    pc.AdjustMoney(-10000);
                    npc.CastSpell(WellKnownSpells.CureLightWounds, PickedObject);
                    ;
                    break;
                case 120:
                    originalScript = "npc.spells_pending_to_memorized()";
                    npc.PendingSpellsToMemorized();
                    break;
                case 161:
                    originalScript = "npc.reaction_adj( pc,-5)";
                    npc.AdjustReaction(pc, -5);
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

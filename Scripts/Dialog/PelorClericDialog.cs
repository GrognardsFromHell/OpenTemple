
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
    [DialogScript(361)]
    public class PelorClericDialog : PelorCleric, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 81:
                    originalScript = "pc.money_get() >= 8000";
                    return pc.GetMoney() >= 8000;
                case 91:
                    originalScript = "pc.money_get() >= 34000";
                    return pc.GetMoney() >= 34000;
                case 101:
                case 121:
                case 131:
                    originalScript = "pc.money_get() >= 80000";
                    return pc.GetMoney() >= 80000;
                case 111:
                    originalScript = "pc.money_get() >= 146000";
                    return pc.GetMoney() >= 146000;
                default:
                    originalScript = null;
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 20:
                    originalScript = "game.picker( npc, spell_cure_light_wounds, should_heal_hp_on, [ 140, 10, 80 ] )";
                    // FIXME: picker;
                    break;
                case 30:
                    originalScript = "game.picker( npc, spell_cure_moderate_wounds, should_heal_hp_on, [ 140, 10, 90 ] )";
                    // FIXME: picker;
                    break;
                case 40:
                    originalScript = "game.picker( npc, spell_cure_serious_wounds, should_heal_hp_on, [ 140, 10, 100 ] )";
                    // FIXME: picker;
                    break;
                case 50:
                    originalScript = "game.picker( npc, spell_cure_critical_wounds, should_heal_hp_on, [ 140, 10, 110 ] )";
                    // FIXME: picker;
                    break;
                case 60:
                    originalScript = "game.picker( npc, spell_remove_disease, should_heal_disease_on, [ 150, 10, 120 ] )";
                    // FIXME: picker;
                    break;
                case 70:
                    originalScript = "game.picker( npc, spell_neutralize_poison, should_heal_poison_on, [ 150, 10, 130 ] )";
                    // FIXME: picker;
                    break;
                case 81:
                    originalScript = "pc.money_adj(-8000); npc.cast_spell( spell_cure_light_wounds, picker_obj )";
                    pc.AdjustMoney(-8000);
                    npc.CastSpell(WellKnownSpells.CureLightWounds, PickedObject);
                    ;
                    break;
                case 91:
                    originalScript = "pc.money_adj(-34000); npc.cast_spell( spell_cure_moderate_wounds, picker_obj )";
                    pc.AdjustMoney(-34000);
                    npc.CastSpell(WellKnownSpells.CureModerateWounds, PickedObject);
                    ;
                    break;
                case 101:
                    originalScript = "pc.money_adj(-80000); npc.cast_spell( spell_cure_serious_wounds, picker_obj )";
                    pc.AdjustMoney(-80000);
                    npc.CastSpell(WellKnownSpells.CureSeriousWounds, PickedObject);
                    ;
                    break;
                case 111:
                    originalScript = "pc.money_adj(-146000); npc.cast_spell( spell_cure_critical_wounds, picker_obj )";
                    pc.AdjustMoney(-146000);
                    npc.CastSpell(WellKnownSpells.CureCriticalWounds, PickedObject);
                    ;
                    break;
                case 121:
                    originalScript = "pc.money_adj(-80000); npc.cast_spell( spell_remove_disease, picker_obj )";
                    pc.AdjustMoney(-80000);
                    npc.CastSpell(WellKnownSpells.RemoveDisease, PickedObject);
                    ;
                    break;
                case 131:
                    originalScript = "pc.money_adj(-80000); npc.cast_spell( spell_neutralize_poison, picker_obj )";
                    pc.AdjustMoney(-80000);
                    npc.CastSpell(WellKnownSpells.NeutralizePoison, PickedObject);
                    ;
                    break;
                case 160:
                    originalScript = "npc.spells_pending_to_memorized()";
                    npc.PendingSpellsToMemorized();
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

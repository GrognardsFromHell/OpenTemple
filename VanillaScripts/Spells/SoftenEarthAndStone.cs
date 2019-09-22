
using System;
using System.Collections.Generic;
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
using SpicyTemple.Core.Ui;
using System.Linq;
using SpicyTemple.Core.Systems.Script.Extensions;
using SpicyTemple.Core.Utils;
using static SpicyTemple.Core.Systems.Script.ScriptUtilities;

namespace VanillaScripts.Spells
{
    [SpellScript(442)]
    public class SoftenEarthAndStone : BaseSpellScript
    {

        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Soften Earth and Stone OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-transmutation-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Soften Earth and Stone OnSpellEffect");
            spell.duration = 1;

            var spell_obj = GameSystems.MapObject.CreateObject(OBJECT_SPELL_GENERIC, spell.aoeCenter);

            var caster_init_value = spell.caster.GetInitiative();

            spell_obj.InitD20Status();
            spell_obj.SetInitiative(caster_init_value);
            var spell_obj_partsys_id = AttachParticles("sp-Soften Earth and Stone", spell_obj);

            spell_obj.AddCondition("sp-Soften Earth and Stone", spell.spellId, spell.duration, 0, spell_obj_partsys_id);
        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Soften Earth and Stone OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Soften Earth and Stone OnEndSpellCast");
        }
        public override void OnAreaOfEffectHit(SpellPacketBody spell)
        {
            Logger.Info("Soften Earth and Stone OnAreaOfEffectHit");
        }
        public override void OnSpellStruck(SpellPacketBody spell)
        {
            Logger.Info("Soften Earth and Stone OnSpellStruck");
        }


    }
}

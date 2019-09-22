
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
    [SpellScript(79)]
    public class ControlPlants : BaseSpellScript
    {

        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Control Plants OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-transmutation-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Control Plants OnSpellEffect");
            var remove_list = new List<GameObjectBody>();

            spell.duration = 60 * spell.casterLevel;

            var control_plants_obj = GameSystems.MapObject.CreateObject(OBJECT_SPELL_GENERIC, spell.aoeCenter);

            var caster_init_value = spell.caster.GetInitiative();

            control_plants_obj.InitD20Status();
            control_plants_obj.SetInitiative(caster_init_value);
            var control_plants_partsys_id = AttachParticles("sp-Control Plants", control_plants_obj);

            control_plants_obj.AddCondition("sp-Control Plants", spell.spellId, spell.duration, 0, control_plants_partsys_id);
            spell.caster.AddCondition("sp-Control Plants Tracking", spell.spellId, spell.duration, 0);
        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Control Plants OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Control Plants OnEndSpellCast");
        }
        public override void OnAreaOfEffectHit(SpellPacketBody spell)
        {
            Logger.Info("Control Plants OnAreaOfEffectHit");
        }
        public override void OnSpellStruck(SpellPacketBody spell)
        {
            Logger.Info("Control Plants OnSpellStruck");
        }


    }
}

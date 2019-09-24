
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
using SpicyTemple.Core.Systems.ObjScript;
using SpicyTemple.Core.Ui;
using System.Linq;
using SpicyTemple.Core.Systems.Script.Extensions;
using SpicyTemple.Core.Utils;
using static SpicyTemple.Core.Systems.Script.ScriptUtilities;

namespace Scripts.Spells
{
    [SpellScript(404)]
    public class RighteousMight : BaseSpellScript
    {
        private static readonly string RM_KEY = "Sp404_RighteousMight_Activelist";
        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Righteous Might OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-transmutation-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Righteous Might OnSpellEffect");
            spell.duration = 1 * spell.casterLevel;
            var target_item = spell.Targets[0];
            // size mod
            // print "Size:" + str(target_item.obj.obj_get_int(obj_f_size))
            // print "Reach:" + str(target_item.obj.obj_get_int(obj_f_critter_reach))
            SizeUtils.IncSizeCategory(target_item.Object);
            // save target_list
            Co8PersistentData.AddToSpellActiveList(RM_KEY, spell.spellId, target_item.Object);

            // print "new Size:" + str(target_item.obj.obj_get_int(obj_f_size))
            // print "new Reach:" + str(target_item.obj.obj_get_int(obj_f_critter_reach))
            target_item.Object.AddCondition("sp-Righteous Might", spell.spellId, spell.duration, 0);
            target_item.ParticleSystem = AttachParticles("sp-Righteous Might", target_item.Object);
        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Righteous Might OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Righteous Might OnEndSpellCast");
            Co8PersistentData.CleanupActiveSpellTargets(RM_KEY, spell.spellId, SizeUtils.ResetSizeCategory);
        }

    }
}

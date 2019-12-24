
using System;
using System.Collections.Generic;
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

namespace Scripts.Spells
{
    [SpellScript(430)]
    public class Shillelagh : BaseSpellScript
    {
        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Shillelagh OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-transmutation-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Shillelagh OnSpellEffect");
            spell.duration = 10 * spell.casterLevel;
            var target_item = spell.Targets[0];
            var holy_water_proto_id = 4223;
            AttachParticles("sp-Shillelagh", target_item.Object);
            var item_obj = GameSystems.MapObject.CreateObject(holy_water_proto_id, spell.aoeCenter);
            item_obj.InitD20Status();
            // save item obj in target_list
            spell.Targets[0].Object = item_obj;
            spell.caster.GetItem(item_obj);
            spell.caster.AddCondition("sp-Shillelagh", spell.spellId, spell.duration, 0);
        }
        // add magic_stone condition to stones
        // item_obj.condition_add_with_args( 'sp-Shillelagh', spell.id, spell.duration, 0 )
        // item_obj.set_initiative( spell.caster.get_initiative() )
        // spell.target_list.remove_target( target_item.obj )
        // spell.spell_end( spell.id )

        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Shillelagh OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Shillelagh OnEndSpellCast");
        }

    }
}

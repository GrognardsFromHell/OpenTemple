
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
            spell.Targets[0].Object = item_obj;

            spell.caster.GetItem(item_obj);
            spell.caster.AddCondition("sp-Shillelagh", spell.spellId, spell.duration, 0);
        }
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

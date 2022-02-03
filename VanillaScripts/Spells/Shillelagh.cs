
using System;
using System.Collections.Generic;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.Dialog;
using OpenTemple.Core.Systems.Feats;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Systems.Script;
using OpenTemple.Core.Systems.Spells;
using OpenTemple.Core.Systems.GameObjects;
using OpenTemple.Core.Systems.D20.Conditions;
using OpenTemple.Core.Location;
using OpenTemple.Core.Ui;
using System.Linq;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Utils;
using static OpenTemple.Core.Systems.Script.ScriptUtilities;

namespace VanillaScripts.Spells;

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
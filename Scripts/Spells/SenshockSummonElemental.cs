
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
using OpenTemple.Core.Systems.ObjScript;
using OpenTemple.Core.Ui;
using System.Linq;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Utils;
using static OpenTemple.Core.Systems.Script.ScriptUtilities;

namespace Scripts.Spells;

[SpellScript(605)]
public class SenshockSummonElemental : BaseSpellScript
{
    public override void OnBeginSpellCast(SpellPacketBody spell)
    {
        Logger.Info("senshock summon elemental OnBeginSpellCast");
        Logger.Info("spell.target_list={0}", spell.Targets);
        Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
        AttachParticles("sp-conjuration-conjure", spell.caster);
    }
    public override void OnSpellEffect(SpellPacketBody spell)
    {
        Logger.Info("senshock summon elemental OnSpellEffect");
        spell.duration = 50;
        // what elementals will we get?
        var dice = Dice.D4;
        var proto = dice.Roll();
        // set the proto_id for this monster
        int proto_id;
        if (proto == 1)
        {
            // proto_id = 14292
            proto_id = 14624;
        }
        else if (proto == 2)
        {
            // proto_id = 14296
            proto_id = 14625;
        }
        else if (proto == 3)
        {
            // proto_id = 14298
            proto_id = 14626;
        }
        else
        {
            // proto_id = 14302
            proto_id = 14627;
        }

        // create monster, monster should be added to target_list
        // monster should disappear when duration is over, apply "TIMED_DISAPPEAR" condition happens auto now...
        // just be sure to set the spell.duration
        spell.SummonMonsters(true, proto_id);
        spell.EndSpell();
    }
    public override void OnBeginRound(SpellPacketBody spell)
    {
        Logger.Info("senshock summon elemental OnBeginRound");
    }
    public override void OnEndSpellCast(SpellPacketBody spell)
    {
        Logger.Info("senshock summon elemental OnEndSpellCast");
    }

}
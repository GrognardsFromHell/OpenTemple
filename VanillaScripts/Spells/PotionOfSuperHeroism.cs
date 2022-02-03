
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

[SpellScript(710)]
public class PotionOfSuperHeroism : BaseSpellScript
{

    public override void OnBeginSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Potion of super-heroism OnBeginSpellCast");
        Logger.Info("spell.target_list={0}", spell.Targets);
        Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
        AttachParticles("sp-abjuration-conjure", spell.caster);
    }
    public override void OnSpellEffect(SpellPacketBody spell)
    {
        Logger.Info("Potion of super-heroism OnSpellEffect");
        spell.duration = 600;

        var target = spell.Targets[0];

        target.Object.AddCondition("sp-Potion of super-heroism", spell.spellId, spell.duration, 0);
        int temp_hit_points; // DECL_PULL_UP
        if ((spell.casterLevel < 20))
        {
            temp_hit_points = spell.casterLevel;

        }
        else
        {
            temp_hit_points = 20;

        }

        target.Object.AddCondition("Temporary_Hit_Points", spell.spellId, spell.duration, temp_hit_points);
    }
    public override void OnBeginRound(SpellPacketBody spell)
    {
        Logger.Info("Potion of super-heroism OnBeginRound");
    }
    public override void OnEndSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Potion of super-heroism OnEndSpellCast");
    }


}
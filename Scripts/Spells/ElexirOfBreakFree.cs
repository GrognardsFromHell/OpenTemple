
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

[SpellScript(734)]
public class ElexirOfBreakFree : BaseSpellScript
{
    public override void OnBeginSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Elixir of break free OnBeginSpellCast");
        Logger.Info("spell.target_list={0}", spell.Targets);
        Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
    }
    public override void OnSpellEffect(SpellPacketBody spell)
    {
        Logger.Info("Elixir of break free OnSpellEffect");
        var target = spell.Targets[0];
        spell.duration = 0;
        // target.obj.condition_add_with_args( 'Elixer Timed Skill Bonus', 2, 0, 0 )
        if (target.Object.D20Query(D20DispatcherKey.QUE_Is_BreakFree_Possible))
        {
            SetGlobalVar(756, target.Object.GetStat(Stat.strength));
            target.Object.SetBaseStat(Stat.strength, 50);
            target.Object.D20SendSignal(D20DispatcherKey.SIG_BreakFree);
            target.Object.SetBaseStat(Stat.strength, GetGlobalVar(756));
        }

    }
    public override void OnBeginRound(SpellPacketBody spell)
    {
        Logger.Info("Elixir of break free OnBeginRound");
    }
    public override void OnEndSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Elixir of break free OnEndSpellCast");
    }

}
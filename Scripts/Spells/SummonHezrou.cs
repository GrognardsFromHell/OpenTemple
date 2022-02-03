
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

[SpellScript(728)]
public class SummonHezrou : BaseSpellScript
{
    public override void OnBeginSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Summon Hezrou OnBeginSpellCast");
        Logger.Info("spell.target_list={0}", spell.Targets);
        Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
        AttachParticles("sp-conjuration-conjure", spell.caster);
    }
    public override void OnSpellEffect(SpellPacketBody spell)
    {
        Logger.Info("Summon Hezrou OnSpellEffect");
        spell.duration = 5;
        // get the proto_id for this monster
        var monster_proto_id = 14259;
        // create monster
        spell.SummonMonsters(true, monster_proto_id);
        // Gets handle on monster, and sets a flag so that it won't be mistaken for a new summoned monster
        var monster_obj = Co8.GetCritterHandle(spell, monster_proto_id);
        AttachParticles("Orb-Summon-Hezrou", monster_obj);
        // add monster to follower list for spell_caster
        spell.caster.AddAIFollower(monster_obj);
        // add monster_obj to d20initiative, and set initiative to spell_caster's
        var caster_init_value = spell.caster.GetInitiative();
        monster_obj.AddToInitiative();
        monster_obj.SetInitiative(caster_init_value);
        UiSystems.Combat.Initiative.UpdateIfNeeded();
        // monster should disappear when duration is over, apply "TIMED_DISAPPEAR" condition
        monster_obj.AddCondition("sp-Summoned", spell.spellId, spell.duration, 0);
        // add monster to target list
        spell.ClearTargets();
        spell.AddTarget(monster_obj);
        spell.EndSpell();
    }
    public override void OnBeginRound(SpellPacketBody spell)
    {
        Logger.Info("Summon Hezrou OnBeginRound");
    }
    public override void OnEndSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Summon Hezrou OnEndSpellCast");
    }

}
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.Script;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Systems.Spells;

namespace Scripts.Spells;

public abstract class SummonMonsterBase : BaseSpellScript
{

    protected abstract string SpellName { get; }

    protected abstract string ParticleSystemId { get; }

    // Key from spells_radial_menu_options.mes
    protected abstract int SpellOptionsKey { get; }

    protected virtual void ModifySummonedProtoId(SpellPacketBody spell, ref int protoId)
    {
    }

    public override void OnBeginSpellCast(SpellPacketBody spell)
    {
        Logger.Info($"{SpellName} OnBeginSpellCast");
        Logger.Info("spell.target_list={0}", spell.Targets);
        Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
        ScriptUtilities.AttachParticles("sp-conjuration-conjure", spell.caster);
    }
    public override void OnSpellEffect(SpellPacketBody spell)
    {
        Logger.Info($"{SpellName} OnSpellEffect");
        spell.duration = 1 * spell.casterLevel;
        // Solves Radial menu problem for Wands/NPCs
        var options = SummonMonsterTools.GetSpellOptions(SpellOptionsKey);
        var protoId = spell.GetMenuArg(RadialMenuParam.MinSetting);
        if (!options.Contains(protoId))
        {
            protoId = GameSystems.Random.PickRandom(options);
        }

        ModifySummonedProtoId(spell, ref protoId);

        // create monster, monster should be added to target_list
        spell.SummonMonsters(true, protoId);
        var target_item = spell.Targets[0];
        ScriptUtilities.AttachParticles(ParticleSystemId, target_item.Object);
        SummonMonsterTools.SummonMonster_Rectify_Initiative(spell, protoId); // Added by S.A. - sets iniative to caster's initiative -1, so that it gets to act in the same round
        spell.EndSpell();
    }
    public override void OnBeginRound(SpellPacketBody spell)
    {
        Logger.Info($"{SpellName} OnBeginRound");
    }
    public override void OnEndSpellCast(SpellPacketBody spell)
    {
        Logger.Info($"{SpellName} OnEndSpellCast");
    }

}
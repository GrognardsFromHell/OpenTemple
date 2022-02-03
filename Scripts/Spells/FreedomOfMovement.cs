
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

[SpellScript(188)]
public class FreedomOfMovement : BaseSpellScript
{
    public override void OnBeginSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Freedom of Movement OnBeginSpellCast");
        Logger.Info("spell.target_list={0}", spell.Targets);
        Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
        AttachParticles("sp-abjuration-conjure", spell.caster);
    }
    public override void OnSpellEffect(SpellPacketBody spell)
    {
        Logger.Info("Freedom of Movement OnSpellEffect");
        if (spell.spellClass == 14)
        {
            if (spell.spellKnownSlotLevel < 4) // added to check for proper ranger slot level (darmagon)
            {
                spell.caster.FloatMesFileLine("mes/spell.mes", 16008);
                return;
            }

        }

        spell.duration = 100 * spell.casterLevel;
        var npc = spell.caster; // added so NPC's can pre-buff
        if (npc.type != ObjectType.pc && npc.GetLeader() == null && !GameSystems.Combat.IsCombatActive())
        {
            spell.duration = 2000 * spell.casterLevel;
        }

        var target = spell.Targets[0];
        target.Object.AddCondition("sp-Freedom of Movement", spell.spellId, spell.duration, 0);
        target.ParticleSystem = AttachParticles("sp-Freedom of Movement", target.Object);
    }
    public override void OnBeginRound(SpellPacketBody spell)
    {
        Logger.Info("Freedom of Movement OnBeginRound");
    }
    public override void OnEndSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Freedom of Movement OnEndSpellCast");
    }

}
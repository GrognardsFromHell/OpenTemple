
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
    [SpellScript(400)]
    public class ResistElements : BaseSpellScript
    {
        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Resist Elements OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-abjuration-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Resist Elements OnSpellEffect");
            var spell_arg = spell.GetMenuArg(RadialMenuParam.MinSetting);
            // Solves Radial menu problem for Wands/NPCs
            spell_arg = spell.GetMenuArg(RadialMenuParam.MinSetting);
            if (spell_arg != 1 && spell_arg != 2 && spell_arg != 3 && spell_arg != 4 && spell_arg != 5)
            {
                spell_arg = RandomRange(1, 5);
            }

            var npc = spell.caster;
            if (npc.GetNameId() == 8047) // Alrrem
            {
                spell_arg = 4;
            }

            if (npc.GetNameId() == 8035) // added so NPC's can cast spell
            {
                spell_arg = 3;
            }

            if (npc.GetNameId() == 14336) // added so NPC's can cast spell
            {
                spell_arg = 4;
            }

            if (spell.Targets[0].Object.GetNameId() == 14195 || spell.Targets[0].Object.GetNameId() == 14224) // Oohlgrist or Aern; they already has a magic ring against fire
            {
                spell_arg = 2;
            }

            if (spell_arg == 1)
            {
                var element_type = SpellDescriptor.ACID;
                var partsys_type = "sp-Resist Elements-acid";
            }
            else if (spell_arg == 2)
            {
                var element_type = SpellDescriptor.COLD;
                var partsys_type = "sp-Resist Elements-cold";
            }
            else if (spell_arg == 3)
            {
                var element_type = SpellDescriptor.ELECTRICITY;
                var partsys_type = "sp-Resist Elements-water";
            }
            else if (spell_arg == 4)
            {
                var element_type = SpellDescriptor.FIRE;
                var partsys_type = "sp-Resist Elements-fire";
            }
            else if (spell_arg == 5)
            {
                var element_type = SpellDescriptor.SONIC;
                var partsys_type = "sp-Resist Elements-sonic";
            }

            spell.duration = 100 * spell.casterLevel;
            npc = spell.caster; // added so NPC's can pre-buff
            if (npc.type != ObjectType.pc && npc.GetLeader() == null && !GameSystems.Combat.IsCombatActive())
            {
                spell.duration = 2000 * spell.casterLevel;
            }

            var target_item = spell.Targets[0];
            if (target_item.Object.AddCondition("sp-Resist Elements", spell.spellId, element_type, spell.duration))
            {
                target_item.ParticleSystem = AttachParticles(partsys_type, target_item.Object);
            }

        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Resist Elements OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Resist Elements OnEndSpellCast");
        }

    }
}

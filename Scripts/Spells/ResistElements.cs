
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

            SpellDescriptor element_type;
            string partsys_type;
            if (spell_arg == 1)
            {
                element_type = SpellDescriptor.ACID;
                partsys_type = "sp-Resist Elements-acid";
            }
            else if (spell_arg == 2)
            {
                element_type = SpellDescriptor.COLD;
                partsys_type = "sp-Resist Elements-cold";
            }
            else if (spell_arg == 3)
            {
                element_type = SpellDescriptor.ELECTRICITY;
                partsys_type = "sp-Resist Elements-water";
            }
            else if (spell_arg == 4)
            {
                element_type = SpellDescriptor.FIRE;
                partsys_type = "sp-Resist Elements-fire";
            }
            else if (spell_arg == 5)
            {
                element_type = SpellDescriptor.SONIC;
                partsys_type = "sp-Resist Elements-sonic";
            }
            else
            {
                Logger.Error("Resist Elements called with invalid spell arg: {0}", spell_arg);
                return;
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

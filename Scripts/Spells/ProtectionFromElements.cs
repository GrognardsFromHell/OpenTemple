
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
    [SpellScript(369)]
    public class ProtectionFromElements : BaseSpellScript
    {
        // Added Belsornig casting - SA

        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Protection From Elements OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-abjuration-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Protection From Elements OnSpellEffect");
            var spell_arg = spell.GetMenuArg(RadialMenuParam.MinSetting);
            spell.duration = 100 * spell.casterLevel;
            var npc = spell.caster; // added so NPC's can pre-buff
            if (npc.type != ObjectType.pc && npc.GetLeader() == null && !GameSystems.Combat.IsCombatActive())
            {
                spell.duration = 2000 * spell.casterLevel;
            }

            // Solves Radial menu problem for Wands/NPCs
            spell_arg = spell.GetMenuArg(RadialMenuParam.MinSetting);
            if (spell_arg != 1 && spell_arg != 2 && spell_arg != 3 && spell_arg != 4 && spell_arg != 5)
            {
                spell_arg = RandomRange(1, 5);
            }

            if (npc.GetNameId() == 8047) // Alrrem
            {
                spell_arg = 4;
            }

            if (npc.GetNameId() == 8035) // added so NPC's can cast
            {
                spell_arg = 3;
            }

            if (npc.GetNameId() == 8092) // added so NPC's can cast
            {
                spell_arg = 5;
            }

            if (npc.GetNameId() == 8063) // added so NPC's can cast
            {
                spell.casterLevel = 10;
                spell.duration = 20000;
                spell_arg = 4;
            }

            if (npc.GetNameId() == 8091)
            {
                // Belsornig casting
                spell.casterLevel = 8;
                spell_arg = 4;
                spell.duration = 16000;
            }

            if (spell.Targets[0].Object.GetNameId() == 14195 || spell.Targets[0].Object.GetNameId() == 14224) // Oohlgrist or Aern; they already has a magic ring against fire
            {
                spell_arg = 2;
            }

            if (spell_arg == 1)
            {
                var element_type = SpellDescriptor.ACID;
                var partsys_type = "sp-Protection From Elements-acid";
            }
            else if (spell_arg == 2)
            {
                var element_type = SpellDescriptor.COLD;
                var partsys_type = "sp-Protection From Elements-cold";
            }
            else if (spell_arg == 3)
            {
                var element_type = SpellDescriptor.ELECTRICITY;
                var partsys_type = "sp-Protection From Elements-electricity";
            }
            else if (spell_arg == 4)
            {
                var element_type = SpellDescriptor.FIRE;
                var partsys_type = "sp-Protection From Elements-fire";
            }
            else if (spell_arg == 5)
            {
                var element_type = SpellDescriptor.SONIC;
                var partsys_type = "sp-Protection From Elements-sonic";
            }

            var target_item = spell.Targets[0];
            target_item.Object.AddCondition("sp-Protection From Elements", spell.spellId, element_type, spell.duration);
            target_item.ParticleSystem = AttachParticles(partsys_type, target_item.Object);
        }
        // spell.spell_end( spell.id )

        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Protection From Elements OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Protection From Elements OnEndSpellCast");
        }

    }
}

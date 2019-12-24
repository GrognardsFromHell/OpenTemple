
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
    [SpellScript(105)]
    public class DelayedBlastFireball : BaseSpellScript
    {
        // How this works:
        // A variable is set in moduels\ToEE\delayed_blast_fireball.txt, that determines how many rounds the fireball is delayed.
        // If it's 0, the spell works just like ordinary Fireball, except the damage is limited to 20d6 rather than 10d6.
        // If it's greater than 0:
        // The spell's duration is set to the delay.
        // A "spell object" is created. (proto 6400; actually an armor object)
        // This spell object is assigned a randomized ID, and borks the dc ???????
        // When the number of rounds equal to the delay passes, the spell "ends".
        // This triggers the OnEndSpellCast() script.
        // The script looks for the spell object in the spell caster's vicinity.
        // NB: This means that changing maps or walking far away will bork the spell.
        // If the spell object is found:
        // Target everything in its vicinity using a 360 degree targeting cone, with a reach of 20.
        // Asplode!
        // Possibly enlarge spell, maximize spell etc won't work?
        // Now given the Water Temple pool treatment

        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Delayed Blast Fireball OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-evocation-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Delayed Blast Fireball OnSpellEffect");
            AttachParticles("sp-Fireball-conjure", spell.caster);
            var remove_list = new List<GameObjectBody>();
            var dam = Dice.D6;
            dam = dam.WithCount(Math.Min(20, spell.casterLevel)); // edited by Allyx
            if (Co8Settings.ElementalSpellsAtElementalNodes)
            {
                if (SelectedPartyLeader.GetMap() == 5083) // Fire node - fire spells do x2 damage
                {
                    dam = dam.WithCount(dam.Count * 2);
                }
                else if (SelectedPartyLeader.GetMap() == 5084) // Water node - fire spells do 1/2 damage
                {
                    dam = dam.WithCount(dam.Count / 2);
                }

            }

            var (xx, yy) = spell.aoeCenter.location;
            if (SelectedPartyLeader.GetMap() == 5067 && (xx >= 521 && xx <= 555) && (yy >= 560 && yy <= 610))
            {
                // Water Temple Pool Enchantment prevents fire spells from working inside the chamber, according to the module -SA
                var tro = GameSystems.MapObject.CreateObject(14070, spell.aoeCenter);
                SpawnParticles("swirled gas", spell.aoeCenter);
                tro.FloatMesFileLine("mes/skill_ui.mes", 2000, TextFloaterColor.Red);
                tro.Destroy();
                Sound(7581, 1);
                Sound(7581, 1);
                foreach (var target_item in spell.Targets)
                {
                    remove_list.Add(target_item.Object);
                }

                spell.RemoveTargets(remove_list);
                spell.EndSpell();
                return;
            }

            SpawnParticles("sp-Fireball-Hit", spell.aoeCenter);
            var soundfizzle = 0;
            foreach (var target_item in spell.Targets)
            {
                (xx, yy) = target_item.Object.GetLocation();
                if (target_item.Object.GetMap() == 5067 && (xx >= 521 && xx <= 555) && (yy >= 560 && yy <= 610))
                {
                    // Water Temple Pool Enchantment prevents fire spells from working inside the chamber, according to the module -SA
                    target_item.Object.FloatMesFileLine("mes/skill_ui.mes", 2000, TextFloaterColor.Red);
                    AttachParticles("swirled gas", target_item.Object);
                    soundfizzle = 1;
                    remove_list.Add(target_item.Object);
                    continue;

                }

                if (target_item.Object.ReflexSaveAndDamage(spell.caster, spell.dc, D20SavingThrowReduction.Half, D20SavingThrowFlag.NONE, dam, DamageType.Fire, D20AttackPower.UNSPECIFIED, D20ActionType.CAST_SPELL, spell.spellId))
                {
                    // saving throw successful
                    target_item.Object.FloatMesFileLine("mes/spell.mes", 30001);
                }
                else
                {
                    // saving throw unsuccessful
                    target_item.Object.FloatMesFileLine("mes/spell.mes", 30002);
                }

                remove_list.Add(target_item.Object);
            }

            if (soundfizzle == 1)
            {
                Sound(7581, 1);
                Sound(7581, 1);
            }

            spell.RemoveTargets(remove_list);
            spell.EndSpell();

        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Delayed Blast Fireball OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Delayed Blast Fireball OnEndSpellCast");
        }

    }
}


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
    [SpellScript(72)]
    public class ConeOfCold : BaseSpellScript
    {
        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Cone of Cold OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-evocation-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Cone of Cold OnSpellEffect");
            var remove_list = new List<GameObjectBody>();
            var dam = Dice.D6;
            dam = dam.WithCount(Math.Min(15, spell.casterLevel));
            if (Co8Settings.ElementalSpellsAtElementalNodes)
            {
                if (SelectedPartyLeader.GetMap() == 5083) // Fire node - water spells do 1/2 damage
                {
                    dam = dam.WithCount(dam.Count / 2);
                }
                else if (SelectedPartyLeader.GetMap() == 5084) // Water node - water spells do X2 damage
                {
                    dam = dam.WithCount(dam.Count * 2);
                }

            }

            AttachParticles("sp-Cone of Cold", spell.caster);
            var npc = spell.caster;
            // Caster is NOT in game party
            if (npc.type != ObjectType.pc && npc.GetLeader() == null)
            {
                // range = 25 + 5 * int(spell.caster_level/2)
                var range = 60;
                var target_list = ObjList.ListCone(spell.caster, ObjectListFilter.OLC_CRITTERS, range, -30, 60);
                foreach (var obj in target_list)
                {
                    if (obj == spell.caster)
                    {
                        continue;
                    }
                    if (obj.ReflexSaveAndDamage(spell.caster, spell.dc, D20SavingThrowReduction.Half, D20SavingThrowFlag.NONE, dam, DamageType.Cold, D20AttackPower.UNSPECIFIED, D20ActionType.CAST_SPELL, spell.spellId))
                    {
                        // saving throw successful
                        obj.FloatMesFileLine("mes/spell.mes", 30001);
                    }
                    else
                    {
                        // saving throw unsuccessful
                        obj.FloatMesFileLine("mes/spell.mes", 30002);
                    }

                }

            }

            // Caster is in game party
            if (npc.type == ObjectType.pc || npc.GetLeader() != null)
            {
                // get all targets in a 25ft + 2ft/level cone (60')
                foreach (var target_item in spell.Targets)
                {
                    if (target_item.Object.ReflexSaveAndDamage(spell.caster, spell.dc, D20SavingThrowReduction.Half, D20SavingThrowFlag.NONE, dam, DamageType.Cold, D20AttackPower.UNSPECIFIED, D20ActionType.CAST_SPELL, spell.spellId))
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

            }

            spell.RemoveTargets(remove_list);
            spell.EndSpell();
        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Cone of Cold OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Cone of Cold OnEndSpellCast");
        }

    }
}

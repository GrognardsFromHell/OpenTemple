
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

namespace VanillaScripts.Spells
{
    [SpellScript(58)]
    public class ChillMetal : BaseSpellScript
    {

        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Chill Metal OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-transmutation-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Chill Metal OnSpellEffect");
            var remove_list = new List<GameObject>();

            spell.duration = 7;

            foreach (var target_item in spell.Targets)
            {
                var armor_obj = target_item.Object.ItemWornAt(EquipSlot.Armor);

                if ((armor_obj != null) && (armor_obj.GetMaterial() == Material.metal))
                {
                    var return_val = target_item.Object.AddCondition("sp-Chill Metal", spell.spellId, spell.duration, 0);

                    if (return_val)
                    {
                        target_item.ParticleSystem = AttachParticles("sp-Chill Metal", target_item.Object);

                    }

                }
                else
                {
                    target_item.Object.FloatMesFileLine("mes/spell.mes", 31010);
                    remove_list.Add(target_item.Object);
                }

            }

            spell.RemoveTargets(remove_list);
            spell.EndSpell();
        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Chill Metal OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Chill Metal OnEndSpellCast");
        }


    }
}

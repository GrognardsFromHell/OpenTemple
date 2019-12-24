
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
    [SpellScript(719)]
    public class RingOfAnimalSummoningDog : BaseSpellScript
    {
        // I now serve the Bag of Tricks

        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Ring of Animal Summoning (Dog) OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-conjuration-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Summon Natures Ally II OnSpellEffect");
            spell.duration = 100;
            var roll = RandomRange(1, 100);
            int monster_proto_id;
            if (roll < 15)
            {
                monster_proto_id = 14050;
            }
            else if (roll < 30)
            {
                monster_proto_id = 14051;
            }
            else if (roll < 45)
            {
                monster_proto_id = 14053;
            }
            else if (roll < 60)
            {
                monster_proto_id = 14052;
            }
            else if (roll < 75)
            {
                monster_proto_id = 14047;
            }
            else if (roll < 90)
            {
                monster_proto_id = 14090;
            }
            else
            {
                monster_proto_id = 14375;
            }

            // special effects
            SpawnParticles("sp-Summon Natures Ally II", spell.aoeCenter);
            spell.SummonMonsters(true, monster_proto_id);
            spell.EndSpell();
        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Ring of Animal Summoning (Dog) OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Ring of Animal Summoning (Dog) OnEndSpellCast");
        }

    }
}

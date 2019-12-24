
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
using OpenTemple.Core.Ui;
using System.Linq;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Utils;
using static OpenTemple.Core.Systems.Script.ScriptUtilities;

namespace VanillaScripts.Spells
{
    [SpellScript(719)]
    public class RingOfAnimalSummoningDog : BaseSpellScript
    {

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
            spell.duration = 10;

            var monster_proto_id = 14049;

            SpawnParticles("sp-Summon Natures Ally II", spell.aoeCenter);
            var monster_obj = GameSystems.MapObject.CreateObject(monster_proto_id, spell.aoeCenter);

            spell.caster.AddAIFollower(monster_obj);
            var caster_init_value = spell.caster.GetInitiative();

            monster_obj.AddToInitiative();
            monster_obj.SetInitiative(caster_init_value);
            UiSystems.Combat.Initiative.UpdateIfNeeded();
            monster_obj.AddCondition("sp-Summoned", spell.spellId, spell.duration, 0);
            spell.ClearTargets();
            spell.AddTarget(monster_obj);
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

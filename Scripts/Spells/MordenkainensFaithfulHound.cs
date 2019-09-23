
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
    [SpellScript(320)]
    public class MordenkainensFaithfulHound : BaseSpellScript
    {
        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Mordenkainen's Faithful Hound OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-conjuration-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Mordenkainen's Faithful Hound OnSpellEffect");
            spell.duration = 600 * spell.casterLevel;
            // get the proto_id for this monster (from radial menu)
            var monster_proto_id = 14278;
            // create monster
            // game.particles( 'sp-Mordenkainens Faithful Hound', spell.target_loc )
            spell.SummonMonsters(true, monster_proto_id);
            var monster_obj = Co8.GetCritterHandle(spell, 14278);
            var hit_points = 6 * spell.casterLevel;
            hit_points = 25 + hit_points;
            monster_obj.SetBaseStat(Stat.hp_max, hit_points);
            // add monster to follower list for spell_caster
            spell.caster.AddAIFollower(monster_obj);
            // add monster_obj to d20initiative, and set initiative to spell_caster's
            var caster_init_value = spell.caster.GetInitiative();
            monster_obj.AddToInitiative();
            monster_obj.SetInitiative(caster_init_value);
            UiSystems.Combat.Initiative.UpdateIfNeeded();
            // add monster to target list
            spell.Targets.Length = 1;
            spell.Targets[0].Object = monster_obj;
            // add condition
            spell.Targets[0].Object.AddCondition("sp-Mordenkainens Faithful Hound", spell.spellId, spell.duration, 0);
            spell.Targets[0].ParticleSystem = AttachParticles("sp-Mordenkainens Faithful Hound", spell.Targets[0].Object);
            spell.EndSpell();
        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            if (GameSystems.Combat.IsCombatActive())
            {
                var time = 6000 * spell.casterLevel;
                Co8.Timed_Destroy(spell.Targets[0].Object, time);
            }

            Logger.Info("Mordenkainen's Faithful Hound OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Mordenkainen's Faithful Hound OnEndSpellCast");
        }

    }
}

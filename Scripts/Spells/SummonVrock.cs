
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
    [SpellScript(729)]
    public class SummonVrock : BaseSpellScript
    {
        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Summon Vrock OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-conjuration-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Summon Vrock OnSpellEffect");
            spell.duration = 5;
            // get the proto_id for this monster
            var monster_proto_id = 14258;
            // create monster
            spell.SummonMonsters(true, monster_proto_id);
            // Gets handle on monster, and sets a flag so that it won't be mistaken for a new summoned monster
            var monster_obj = Co8.GetCritterHandle(spell, monster_proto_id);
            AttachParticles("Orb-Summon-Vrock", monster_obj);
            // add monster to follower list for spell_caster
            spell.caster.AddAIFollower(monster_obj);
            // add monster_obj to d20initiative, and set initiative to spell_caster's
            var caster_init_value = spell.caster.GetInitiative();
            monster_obj.AddToInitiative();
            monster_obj.SetInitiative(caster_init_value);
            UiSystems.Combat.Initiative.UpdateIfNeeded();
            // monster should disappear when duration is over, apply "TIMED_DISAPPEAR" condition
            monster_obj.AddCondition("sp-Summoned", spell.spellId, spell.duration, 0);
            // add monster to target list
            spell.Targets.Length = 1;
            spell.Targets[0].Object = monster_obj;
            spell.EndSpell();
        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Summon Vrock OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Summon Vrock OnEndSpellCast");
        }

    }
}

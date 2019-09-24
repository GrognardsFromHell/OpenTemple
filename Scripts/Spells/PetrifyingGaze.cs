
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
    [SpellScript(738)]
    public class PetrifyingGaze : BaseSpellScript
    {
        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Petrifying Gaze OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-enchantment-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Petrifying Gaze OnSpellEffect");
            spell.dc = 13;
            spell.duration = 100;
            spell.casterLevel = 10;
            var target = spell.Targets[0];
            var target_list = ObjList.ListCone(spell.caster, ObjectListFilter.OLC_CRITTERS, 60, -30, 60);
            // print >> efile, "spell range= ", range, "\n"
            // print >> efile, "target list: ", target_list, "\n"
            var candidates = new List<GameObjectBody>();

            foreach (var obj in target_list)
            {
                if (obj != spell.caster && !obj.IsFriendly(spell.caster))
                {
                    candidates.Add(obj);
                }
            }

            target.Object = GameSystems.Random.PickRandom(candidates);

            // print >> efile, "target.obj: ", target.obj, "\n"
            if (spell.caster.D20Query(D20DispatcherKey.QUE_Critter_Is_Blinded))
            {
                spell.caster.FloatMesFileLine("mes/spell.mes", 20019);
            }
            else if (target.Object.SavingThrow(spell.dc, SavingThrowType.Fortitude, D20SavingThrowFlag.NONE, spell.caster))
            {
                AttachParticles("sp-Shout", spell.caster);
                target.Object.FloatMesFileLine("mes/spell.mes", 30001);
                AttachParticles("Fizzle", target.Object);
                spell.RemoveTarget(target.Object);
            }
            else
            {
                AttachParticles("sp-Shout", spell.caster);
                target.Object.FloatMesFileLine("mes/spell.mes", 30002);
                // HTN - apply condition HALT (Petrifyed)
                target.Object.AddCondition("sp-Command", spell.spellId, spell.duration, 4);
                AttachParticles("sp-Bestow Curse", target.Object);
            }

            spell.EndSpell();
        }
        // efile.close()

        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Petrifying Gaze OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Petrifying Gaze OnEndSpellCast");
        }

    }
}

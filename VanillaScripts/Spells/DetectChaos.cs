
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
using SpicyTemple.Core.Ui;
using System.Linq;
using SpicyTemple.Core.Systems.Script.Extensions;
using SpicyTemple.Core.Utils;
using static SpicyTemple.Core.Systems.Script.ScriptUtilities;

namespace VanillaScripts.Spells
{
    [SpellScript(110)]
    public class DetectChaos : BaseSpellScript
    {

        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Detect Chaos OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-divination-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Detect Chaos OnSpellEffect");
            spell.duration = 100 * spell.casterLevel;

            var target = spell.Targets[0];

            target.Object.AddCondition("sp-Detect Chaos", spell.spellId, spell.duration, 0);
            target.ParticleSystem = AttachParticles("sp-Detect Alignment", target.Object);

        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Detect Chaos OnBeginRound");
            foreach (var obj in ObjList.ListCone(spell.caster, ObjectListFilter.OLC_CRITTERS, spell.spellRange, -45, 90))
            {
                if ((obj.GetAlignment().IsChaotic()))
                {
                    AttachParticles("sp-Detect Alignment Chaos", obj);
                }

            }

        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Detect Chaos OnEndSpellCast");
        }


    }
}

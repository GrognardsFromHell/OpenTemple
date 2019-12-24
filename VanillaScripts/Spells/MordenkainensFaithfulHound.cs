
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

            var monster_proto_id = 14278;

            var monster_obj = GameSystems.MapObject.CreateObject(monster_proto_id, spell.aoeCenter);

            spell.caster.AddAIFollower(monster_obj);
            var caster_init_value = spell.caster.GetInitiative();

            monster_obj.AddToInitiative();
            monster_obj.SetInitiative(caster_init_value);
            UiSystems.Combat.Initiative.UpdateIfNeeded();
            spell.ClearTargets();

            monster_obj.AddCondition("sp-Mordenkainens Faithful Hound", spell.spellId, spell.duration, 0);
            var particles = AttachParticles("sp-Mordenkainens Faithful Hound", monster_obj);
            spell.AddTarget(monster_obj, particles);

            spell.EndSpell();
        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Mordenkainen's Faithful Hound OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Mordenkainen's Faithful Hound OnEndSpellCast");
        }


    }
}

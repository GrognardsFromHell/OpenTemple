
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
    [SpellScript(742)]
    public class SummonGiantConstrictorSnake : BaseSpellScript
    {
        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Summon Giant Constictor Snake OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-conjuration-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Summon Giant Constictor Snake OnSpellEffect");
            spell.duration = 10;
            // get the proto_id for this monster
            var monster_proto_id = 14389;
            // create monster
            spell.SummonMonsters(true, monster_proto_id);
            // Gets handle on monster, and sets a flag so that it won't be mistaken for a new summoned monster
            var monster_obj = GetHandle(spell, monster_proto_id);
            set_ID(monster_obj, 1);
            AttachParticles("Orb-Summon-Air-Elemental", monster_obj);
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
            spell.ClearTargets();
            spell.AddTarget(monster_obj);
            spell.EndSpell();
        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Summon Giant Constictor Snake OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Summon Giant Constictor Snake OnEndSpellCast");
        }
        public static GameObjectBody GetHandle(SpellPacketBody spell, int proto_id)
        {
            // Returns a handle that can be used to manipulate the familiar creature object
            foreach (var npc in ObjList.ListVicinity(spell.aoeCenter.location, ObjectListFilter.OLC_CRITTERS))
            {
                if ((npc.GetNameId() == proto_id))
                {
                    if (get_ID(npc) == 0)
                    {
                        return npc;
                    }

                }

            }

            return null;
        }
        public static int get_ID(GameObjectBody obj)
        {
            // Returns embedded ID number
            return obj.GetInt(obj_f.secretdoor_dc);
        }
        public static int set_ID(GameObjectBody obj, int val)
        {
            // Embeds ID number into mobile object.  Returns ID number.
            obj.SetInt(obj_f.secretdoor_dc, val);
            return obj.GetInt(obj_f.secretdoor_dc);
        }
        public static void clear_ID(GameObjectBody obj)
        {
            // Clears embedded ID number from mobile object
            obj.SetInt(obj_f.secretdoor_dc, 0);
        }

    }
}

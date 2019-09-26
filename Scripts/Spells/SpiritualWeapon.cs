
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
    [SpellScript(457)]
    public class SpiritualWeapon : BaseSpellScript
    {
        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Spiritual Weapon OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-evocation-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Spiritual Weapon OnSpellEffect");
            spell.duration = 1 * spell.casterLevel;
            // get the caster's deity
            var deity = spell.caster.GetDeity();
            // find the deity's preferred weapon, default is dagger
            var weapon_proto = 4142;
            var weapon_portrait = 0;

            if ((deity == DeityId.BOCCOB))
            {
                // staff
                weapon_proto = 4152;
                weapon_portrait = 8060;

            }
            else if ((deity == DeityId.CORELLON_LARETHIAN))
            {
                // longsword
                weapon_proto = 4146;
                weapon_portrait = 8010;

            }
            else if ((deity == DeityId.EHLONNA))
            {
                // staff
                weapon_proto = 4152;
                weapon_portrait = 8060;

            }
            else if ((deity == DeityId.ERYTHNUL))
            {
                // morningstar
                weapon_proto = 4147;
                weapon_portrait = 8040;

            }
            else if ((deity == DeityId.FHARLANGHN))
            {
                // staff
                weapon_proto = 4152;
                weapon_portrait = 8060;

            }
            else if ((deity == DeityId.GARL_GLITTERGOLD))
            {
                // battleaxe blue
                weapon_proto = 4140;
                weapon_portrait = 7950;

            }
            else if ((deity == DeityId.GRUUMSH))
            {
                // spear
                weapon_proto = 4151;
                weapon_portrait = 8050;

            }
            else if ((deity == DeityId.HEIRONEOUS))
            {
                // longsword
                weapon_proto = 4146;
                weapon_portrait = 8010;

            }
            else if ((deity == DeityId.HEXTOR))
            {
                // morningstar
                weapon_proto = 4147;
                weapon_portrait = 8040;

            }
            else if ((deity == DeityId.KORD))
            {
                // greatsword
                weapon_proto = 4143;
                weapon_portrait = 7980;

            }
            else if ((deity == DeityId.MORADIN))
            {
                // warhammer
                weapon_proto = 4153;
                weapon_portrait = 8070;

            }
            else if ((deity == DeityId.NERULL))
            {
                // scythe
                weapon_proto = 4149;
                weapon_portrait = 8030;

            }
            else if ((deity == DeityId.OBAD_HAI))
            {
                // staff
                weapon_proto = 4152;
                weapon_portrait = 8060;

            }
            else if ((deity == DeityId.OLIDAMMARA))
            {
                // rapier
                weapon_proto = 4148;
                weapon_portrait = 8020;

            }
            else if ((deity == DeityId.PELOR))
            {
                // heavymace
                weapon_proto = 4144;
                weapon_portrait = 7970;

            }
            else if ((deity == DeityId.ST_CUTHBERT))
            {
                // heavymace
                weapon_proto = 4144;
                weapon_portrait = 7970;

            }
            else if ((deity == DeityId.VECNA))
            {
                // dagger
                weapon_proto = 4142;
                weapon_portrait = 7960;

            }
            else if ((deity == DeityId.WEE_JAS))
            {
                // dagger
                weapon_proto = 4142;
                weapon_portrait = 7960;

            }
            else if ((deity == DeityId.YONDALLA))
            {
                // shortsword
                weapon_proto = 4150;
                weapon_portrait = 8000;

            }
            else if ((deity == DeityId.OLD_FAITH))
            {
                // heavymace
                weapon_proto = 4144;
                weapon_portrait = 7970;

            }
            else if ((deity == DeityId.ZUGGTMOY))
            {
                // warhammer
                weapon_proto = 4153;
                weapon_portrait = 8070;

            }
            else if ((deity == DeityId.IUZ))
            {
                // greatsword
                weapon_proto = 4143;
                weapon_portrait = 7980;

            }
            else if ((deity == DeityId.LOLTH))
            {
                // dagger
                weapon_proto = 4142;
                weapon_portrait = 7960;

            }
            else if ((deity == DeityId.PROCAN))
            {
                // spear
                weapon_proto = 4151;
                weapon_portrait = 8050;

            }
            else if ((deity == DeityId.NOREBO))
            {
                // dagger
                weapon_proto = 4142;
                weapon_portrait = 7960;

            }
            else if ((deity == DeityId.PYREMIUS))
            {
                // longsword
                weapon_proto = 4146;
                weapon_portrait = 8010;

            }
            else if ((deity == DeityId.RALISHAZ))
            {
                // staff
                weapon_proto = 4152;
                weapon_portrait = 8060;

            }
            else
            {
                // staff
                weapon_proto = 4152;
                weapon_portrait = 8060;

                Logger.Info("SPIRITUAL WEAPON WARNING: deity={0} not found!", deity);
            }

            // figure out the proto_id from the deity
            // monster_proto_id = 14370
            var monster_proto_id = 14629;
            var npc = spell.caster;
            if (npc.GetNameId() == 8036 || npc.GetNameId() == 14425 || npc.GetNameId() == 14212 || npc.GetNameId() == 14211) // faction 7, 3,4,5,6 added -SA
            {
                monster_proto_id = 14621;
            }

            if (npc.GetNameId() == 14425 && npc.GetMap() == 5065) // faction 15
            {
                monster_proto_id = 14604;
            }

            // create monster
            var monster_obj = GameSystems.MapObject.CreateObject(monster_proto_id, spell.aoeCenter);
            monster_obj.SetInt(obj_f.critter_portrait, weapon_portrait);
            var hit_points = 6 * spell.casterLevel;
            hit_points = 25 + hit_points;
            monster_obj.SetBaseStat(Stat.hp_max, hit_points);
            // equip the tempman with the appropriate weapon
            var weapon_obj = GameSystems.MapObject.CreateObject(weapon_proto, monster_obj.GetLocation());
            monster_obj.GetItem(weapon_obj);
            monster_obj.WieldBestInAllSlots();
            Logger.Info("SPIRITUAL WEAPON: equipped obj=( {0} ) with weapon=( {1} )!", monster_obj, weapon_obj);
            // add monster to follower list for spell_caster
            spell.caster.AddAIFollower(monster_obj);
            Logger.Info("added as follower");
            // add monster_obj to d20initiative, and set initiative to spell_caster's
            var caster_init_value = spell.caster.GetInitiative();
            Logger.Info("got the caster's initiative");
            monster_obj.AddToInitiative();
            Logger.Info("added to initiative");
            var initt = -999;
            if (!((PartyLeader.GetPartyMembers()).Contains(spell.caster)))
            {
                var highest = -999;
                foreach (var dude in GameSystems.Party.PartyMembers)
                {
                    if (dude.GetInitiative() > highest && !Utilities.critter_is_unconscious(dude))
                    {
                        highest = dude.GetInitiative();
                    }

                    if (dude.GetInitiative() > initt && dude.GetInitiative() < caster_init_value && !Utilities.critter_is_unconscious(dude))
                    {
                        initt = Math.Max(dude.GetInitiative() - 1, 1);
                    }

                }

                if (initt == -999)
                {
                    initt = Math.Max(highest, 1);
                }

            }
            else
            {
                initt = caster_init_value;
            }

            monster_obj.SetInitiative(initt); // changed by S.A. - in case you have the same faction as the summoned weapon, it needs to see you fighting other members of its faction otherwise it won't act
                                              // monster_obj.set_initiative( caster_init_value ) # removed by S.A. - in case you have the same faction as the summoned weapon, it needs to see you fighting other members of its faction otherwise it won't act and lose a turn
            UiSystems.Combat.Initiative.UpdateIfNeeded();
            Logger.Info("update cmbat ui");
            // monster should disappear when duration is over, apply "TIMED_DISAPPEAR" condition
            monster_obj.AddCondition("sp-Summoned", spell.spellId, spell.duration, 0);
            monster_obj.AddCondition("sp-Spiritual Weapon", spell.spellId, spell.duration, weapon_proto);
            Logger.Info("condition have been added to Spiritual Weapon");
            // add monster to target list
            spell.ClearTargets();
            spell.AddTarget(monster_obj, AttachParticles("sp-spell resistance", monster_obj));
            Logger.Info("particles");
            spell.EndSpell();
            Logger.Info("spell ended, end of OnSpellEffect script");
        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Spiritual Weapon OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Spiritual Weapon OnEndSpellCast");
        }

    }
}

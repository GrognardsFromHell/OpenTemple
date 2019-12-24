
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

            var deity = spell.caster.GetDeity();

            var weapon_proto = 4142;

            var weapon_portrait = 0;


            if ((deity == DeityId.BOCCOB))
            {
                weapon_proto = 4152;

                weapon_portrait = 8060;


            }
            else if ((deity == DeityId.CORELLON_LARETHIAN))
            {
                weapon_proto = 4146;

                weapon_portrait = 8010;


            }
            else if ((deity == DeityId.EHLONNA))
            {
                weapon_proto = 4152;

                weapon_portrait = 8060;


            }
            else if ((deity == DeityId.ERYTHNUL))
            {
                weapon_proto = 4147;

                weapon_portrait = 8040;


            }
            else if ((deity == DeityId.FHARLANGHN))
            {
                weapon_proto = 4152;

                weapon_portrait = 8060;


            }
            else if ((deity == DeityId.GARL_GLITTERGOLD))
            {
                weapon_proto = 4140;

                weapon_portrait = 7950;


            }
            else if ((deity == DeityId.GRUUMSH))
            {
                weapon_proto = 4151;

                weapon_portrait = 8050;


            }
            else if ((deity == DeityId.HEIRONEOUS))
            {
                weapon_proto = 4146;

                weapon_portrait = 8010;


            }
            else if ((deity == DeityId.HEXTOR))
            {
                weapon_proto = 4147;

                weapon_portrait = 8040;


            }
            else if ((deity == DeityId.KORD))
            {
                weapon_proto = 4143;

                weapon_portrait = 7980;


            }
            else if ((deity == DeityId.MORADIN))
            {
                weapon_proto = 4153;

                weapon_portrait = 8070;


            }
            else if ((deity == DeityId.NERULL))
            {
                weapon_proto = 4149;

                weapon_portrait = 8030;


            }
            else if ((deity == DeityId.OBAD_HAI))
            {
                weapon_proto = 4152;

                weapon_portrait = 8060;


            }
            else if ((deity == DeityId.OLIDAMMARA))
            {
                weapon_proto = 4148;

                weapon_portrait = 8020;


            }
            else if ((deity == DeityId.PELOR))
            {
                weapon_proto = 4144;

                weapon_portrait = 7970;


            }
            else if ((deity == DeityId.ST_CUTHBERT))
            {
                weapon_proto = 4144;

                weapon_portrait = 7970;


            }
            else if ((deity == DeityId.VECNA))
            {
                weapon_proto = 4142;

                weapon_portrait = 7960;


            }
            else if ((deity == DeityId.WEE_JAS))
            {
                weapon_proto = 4142;

                weapon_portrait = 7960;


            }
            else if ((deity == DeityId.YONDALLA))
            {
                weapon_proto = 4150;

                weapon_portrait = 8000;


            }
            else if ((deity == DeityId.OLD_FAITH))
            {
                weapon_proto = 4144;

                weapon_portrait = 7970;


            }
            else if ((deity == DeityId.ZUGGTMOY))
            {
                weapon_proto = 4153;

                weapon_portrait = 8070;


            }
            else if ((deity == DeityId.IUZ))
            {
                weapon_proto = 4143;

                weapon_portrait = 7980;


            }
            else if ((deity == DeityId.LOLTH))
            {
                weapon_proto = 4142;

                weapon_portrait = 7960;


            }
            else if ((deity == DeityId.PROCAN))
            {
                weapon_proto = 4151;

                weapon_portrait = 8050;


            }
            else if ((deity == DeityId.NOREBO))
            {
                weapon_proto = 4142;

                weapon_portrait = 7960;


            }
            else if ((deity == DeityId.PYREMIUS))
            {
                weapon_proto = 4146;

                weapon_portrait = 8010;


            }
            else if ((deity == DeityId.RALISHAZ))
            {
                weapon_proto = 4152;

                weapon_portrait = 8060;


            }
            else
            {
                weapon_proto = 4152;

                weapon_portrait = 8060;


                Logger.Info("SPIRITUAL WEAPON WARNING: deity={0} not found!", deity);
            }

            var monster_proto_id = 14370;

            var monster_obj = GameSystems.MapObject.CreateObject(monster_proto_id, spell.aoeCenter);

            monster_obj.SetInt(obj_f.critter_portrait, weapon_portrait);
            var weapon_obj = GameSystems.MapObject.CreateObject(weapon_proto, monster_obj.GetLocation());

            monster_obj.GetItem(weapon_obj);
            monster_obj.WieldBestInAllSlots();
            Logger.Info("SPIRITUAL WEAPON: equipped obj=( {0} ) with weapon=( {1} )!", monster_obj, weapon_obj);
            spell.caster.AddAIFollower(monster_obj);
            var caster_init_value = spell.caster.GetInitiative();

            monster_obj.AddToInitiative();
            monster_obj.SetInitiative(caster_init_value);
            UiSystems.Combat.Initiative.UpdateIfNeeded();
            monster_obj.AddCondition("sp-Summoned", spell.spellId, spell.duration, 0);
            monster_obj.AddCondition("sp-Spiritual Weapon", spell.spellId, spell.duration, weapon_proto);

            spell.ClearTargets();
            spell.AddTarget(monster_obj, AttachParticles("sp-spell resistance", monster_obj));

            spell.EndSpell();
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

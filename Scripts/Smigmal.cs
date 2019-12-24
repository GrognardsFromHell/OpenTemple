
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

namespace Scripts
{
    [ObjectScript(141)]
    public class Smigmal : BaseObjectScript
    {
        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((!attachee.HasMet(triggerer)))
            {
                triggerer.BeginDialog(attachee, 1);
            }
            else if ((GetGlobalFlag(144)))
            {
                triggerer.BeginDialog(attachee, 90);
            }
            else
            {
                triggerer.BeginDialog(attachee, 80);
            }

            return SkipDefault;
        }
        public override bool OnFirstHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((GetGlobalFlag(372)))
            {
                attachee.SetObjectFlag(ObjectFlag.OFF);
            }

            return RunDefault;
        }
        public override bool OnDying(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (CombatStandardRoutines.should_modify_CR(attachee))
            {
                CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
            }

            SetGlobalFlag(338, true);
            return RunDefault;
        }
        public override bool OnStartCombat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var leader = SelectedPartyLeader;
            if ((!attachee.HasMet(leader)) && (!GetGlobalFlag(164))) // Smigmal attacks first time party visits
            {
                SetCounter(0, GetCounter(0) + 1);
                if (((GetCounter(0) >= 2) && (Utilities.group_percent_hp(leader) > 30) && (!GetGlobalFlag(852))))
                {
                    foreach (var pc in GameSystems.Party.PartyMembers)
                    {
                        if (pc.type == ObjectType.pc)
                        {
                            attachee.AIRemoveFromShitlist(pc);
                        }

                    }

                    SetGlobalFlag(852, true);
                    leader.BeginDialog(attachee, 1);
                    // game.new_sid = 0	## removed by Livonya
                    return SkipDefault;
                }

            }
            else if ((!attachee.HasMet(leader)) && (GetGlobalFlag(164))) // Smigmal attacks if Falrinth has been confronted and is away but Smigmal has not yet been met
            {
                SetCounter(0, GetCounter(0) + 1);
                if (((GetCounter(0) >= 1) && (Utilities.group_percent_hp(leader) > 30) && (!GetGlobalFlag(996))))
                {
                    foreach (var pc in GameSystems.Party.PartyMembers)
                    {
                        if (pc.type == ObjectType.pc)
                        {
                            attachee.AIRemoveFromShitlist(pc);
                        }

                    }

                    SetGlobalFlag(996, true);
                    leader.BeginDialog(attachee, 130);
                    // game.new_sid = 0	## removed by Livonya
                    return SkipDefault;
                }

            }
            else if ((attachee.HasMet(leader)) && (GetGlobalFlag(167))) // Smigmal attacks if she has been met, then Falrinth has been confronted and left, and now both are back
            {
                SetCounter(0, GetCounter(0) + 1);
                if (((GetCounter(0) >= 4) && (Utilities.group_percent_hp(leader) > 30) && (!GetGlobalFlag(996))))
                {
                    foreach (var pc in GameSystems.Party.PartyMembers)
                    {
                        if (pc.type == ObjectType.pc)
                        {
                            attachee.AIRemoveFromShitlist(pc);
                        }

                    }

                    SetGlobalFlag(996, true);
                    leader.BeginDialog(attachee, 120);
                    // game.new_sid = 0	## removed by Livonya
                    return SkipDefault;
                }

            }

            // THIS IS USED FOR BREAK FREE
            foreach (var obj in PartyLeader.GetPartyMembers())
            {
                if ((obj.DistanceTo(attachee) <= 3 && obj.GetStat(Stat.hp_current) >= -9))
                {
                    return RunDefault;
                }

            }

            while ((attachee.FindItemByName(8903) != null))
            {
                attachee.FindItemByName(8903).Destroy();
            }

            // if (attachee.d20_query(Q_Is_BreakFree_Possible)): # workaround no longer necessary!
            // create_item_in_inventory( 8903, attachee )
            // attachee.d20_send_signal(S_BreakFree)
            return RunDefault;
        }
        public override bool OnResurrect(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetGlobalFlag(338, false);
            return RunDefault;
        }
        public override bool OnHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((!GameSystems.Combat.IsCombatActive()))
            {
                if ((GetGlobalFlag(144)))
                {
                    foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                    {
                        if ((attachee.HasMet(obj)))
                        {
                            if ((Utilities.is_safe_to_talk(attachee, obj)))
                            {
                                obj.BeginDialog(attachee, 90);
                                DetachScript();
                            }

                        }

                    }

                }

            }

            return RunDefault;
        }
        public override bool OnWillKos(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((triggerer.type == ObjectType.pc))
            {
                if (((!attachee.HasMet(triggerer)) || (Utilities.group_percent_hp(triggerer) <= 30)))
                {
                    return RunDefault;
                }

            }

            return SkipDefault;
        }
        public static bool smigmal_escape(GameObjectBody attachee, GameObjectBody triggerer)
        {
            attachee.SetObjectFlag(ObjectFlag.OFF);
            StartTimer(7200000, () => smigmal_return(attachee));
            return RunDefault;
        }
        public static bool smigmal_return(GameObjectBody attachee)
        {
            attachee.ClearObjectFlag(ObjectFlag.OFF);
            SetGlobalFlag(144, true);
            return RunDefault;
        }
        public static bool smig_backup(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var assassin_1 = GameSystems.MapObject.CreateObject(14782, new locXY(623, 455));
            AttachParticles("sp-invisibility", assassin_1);
            Sound(4032, 1);
            assassin_1.TurnTowards(PartyLeader);
            var assassin_2 = GameSystems.MapObject.CreateObject(14783, new locXY(613, 463));
            AttachParticles("sp-invisibility", assassin_2);
            assassin_2.TurnTowards(PartyLeader);
            return RunDefault;
        }
        public static bool smigmal_well(GameObjectBody attachee, GameObjectBody pc)
        {
            var dice = Dice.Parse("1d10+1000");
            attachee.Heal(null, dice);
            attachee.HealSubdual(null, dice);
            return RunDefault;
        }
        public static bool smig_backup_2(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var assassin_1 = GameSystems.MapObject.CreateObject(14782, new locXY(621, 471));
            AttachParticles("sp-invisibility", assassin_1);
            Sound(4032, 1);
            assassin_1.TurnTowards(PartyLeader);
            var assassin_2 = GameSystems.MapObject.CreateObject(14783, new locXY(634, 472));
            AttachParticles("sp-invisibility", assassin_2);
            assassin_2.TurnTowards(PartyLeader);
            return RunDefault;
        }

    }
}


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
    [ObjectScript(149)]
    public class Thrommel : BaseObjectScript
    {
        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetLeader() != null))
            {
                triggerer.BeginDialog(attachee, 120); // thrommel in party
            }
            else if ((GetGlobalVar(907) == 32))
            {
                triggerer.BeginDialog(attachee, 230); // have attacked 3 or more farm animals with thrommel in party - probably impossible
            }
            else
            {
                triggerer.BeginDialog(attachee, 1); // none of the above
            }

            return SkipDefault;
        }
        public override bool OnFirstHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var itemF = attachee.FindItemByName(2201);
            if ((itemF != null))
            {
                itemF.SetItemFlag(ItemFlag.NO_TRANSFER);
            }

            return RunDefault;
        }
        public override bool OnDying(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (CombatStandardRoutines.should_modify_CR(attachee))
            {
                CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
            }

            SetGlobalFlag(150, true);
            if ((attachee.GetLeader() != null))
            {
                SetGlobalVar(29, GetGlobalVar(29) + 1);
            }

            return RunDefault;
        }
        public override bool OnEnterCombat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            return RunDefault;
        }
        public override bool OnResurrect(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetGlobalFlag(150, false);
            return RunDefault;
        }
        public override bool OnHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((!GameSystems.Combat.IsCombatActive()))
            {
                if ((GetGlobalVar(907) >= 3))
                {
                    if ((attachee != null))
                    {
                        var leader = attachee.GetLeader();
                        if ((leader != null))
                        {
                            leader.RemoveFollower(attachee);
                            attachee.FloatLine(22000, triggerer);
                        }

                    }

                }

            }

            return RunDefault;
        }
        public override bool OnJoin(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var itemD = attachee.FindItemByName(2201);
            if ((itemD != null))
            {
                itemD.SetItemFlag(ItemFlag.NO_TRANSFER);
            }

            return RunDefault;
        }
        public override bool OnNewMap(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (((attachee.GetMap() == 5062) || (attachee.GetMap() == 5111) || (attachee.GetMap() == 5112) || (attachee.GetMap() == 5001) || (attachee.GetMap() == 5121)))
            {
                // 5062 - Temple
                // 5111 - Temple Tower
                // 5112 - Ramshackle Farm (secret entrance to Temple Tower / level 3
                // 5001 - Hommlet
                // 5121 - Verbobonc
                var leader = attachee.GetLeader();
                if ((leader != null))
                {
                    leader.BeginDialog(attachee, 130);
                }

            }

            return RunDefault;
        }
        public override bool OnTrueSeeing(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.HasLineOfSight(triggerer)))
            {
                triggerer.BeginDialog(attachee, 1);
            }

            return RunDefault;
        }
        public override bool OnSpellCast(GameObjectBody attachee, GameObjectBody triggerer, SpellPacketBody spell)
        {
            if ((spell.spellEnum == WellKnownSpells.DispelMagic))
            {
                triggerer.BeginDialog(attachee, 1);
            }

            return RunDefault;
        }
        public static bool run_off(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetGlobalFlag(151, true);
            attachee.RunOff();
            return RunDefault;
        }
        public static bool check_follower_thrommel_comments(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var npc = Utilities.find_npc_near(attachee, 8014);
            if ((npc != null))
            {
                triggerer.BeginDialog(npc, 490);
            }
            else
            {
                npc = Utilities.find_npc_near(attachee, 8000);
                if ((npc != null))
                {
                    triggerer.BeginDialog(npc, 550);
                }
                else
                {
                    triggerer.BeginDialog(attachee, 10);
                }

            }

            return RunDefault;
        }
        public static bool schedule_reward(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetGlobalFlag(152, true);
            attachee.SetObjectFlag(ObjectFlag.OFF);
            StartTimer(1209600000, () => give_reward()); // 1209600000ms is 2 weeks
            ScriptDaemon.record_time_stamp("s_thrommel_reward");
            return RunDefault;
        }
        public static bool give_reward()
        {
            QueueRandomEncounter(3001);
            ScriptDaemon.set_f("s_thrommel_reward_scheduled");
            return RunDefault;
        }
        public static bool equip_transfer(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var itemA = attachee.FindItemByName(3014);
            if ((itemA != null))
            {
                itemA.ClearItemFlag(ItemFlag.NO_TRANSFER);
                attachee.TransferItemByNameTo(triggerer, 3014);
            }

            return RunDefault;
        }
        public static bool equip_transfer2(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (SelectedPartyLeader.GetPartyMembers().Any(o => o.HasItemByName(2201)))
            {
                Utilities.party_transfer_to(attachee, 2201);
            }

            return RunDefault;
        }

    }
}

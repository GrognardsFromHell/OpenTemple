
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

namespace Scripts
{
    [ObjectScript(67)]
    public class Turuko : BaseObjectScript
    {
        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetLeader() == null))
            {
                if ((GetGlobalFlag(44)))
                {
                    triggerer.BeginDialog(attachee, 200);
                }
                else
                {
                    triggerer.BeginDialog(attachee, 1);
                }

            }
            else if ((GetGlobalFlag(44)))
            {
                triggerer.BeginDialog(attachee, 400);
            }
            else
            {
                triggerer.BeginDialog(attachee, 300);
            }

            return SkipDefault;
        }
        public override bool OnFirstHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetLeader() == null))
            {
                if ((attachee.GetMap() == 5007 || attachee.GetMap() == 5008))
                {
                    if ((GetGlobalVar(501) == 4 || GetGlobalVar(501) == 5 || GetGlobalVar(501) == 6 || GetGlobalVar(510) == 2))
                    {
                        attachee.SetObjectFlag(ObjectFlag.OFF);
                    }
                    else
                    {
                        attachee.ClearObjectFlag(ObjectFlag.OFF);
                    }

                }

            }

            return RunDefault;
        }
        public override bool OnDying(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (CombatStandardRoutines.should_modify_CR(attachee))
            {
                CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
            }

            attachee.FloatLine(12014, triggerer);
            if ((attachee.GetLeader() != null && attachee.GetMap() != 5091 && attachee.GetMap() != 5002))
            {
                SetGlobalVar(29, GetGlobalVar(29) + 1);
            }

            SetGlobalFlag(45, true);
            return RunDefault;
        }
        public override bool OnResurrect(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetGlobalFlag(45, false);
            return RunDefault;
        }
        public override bool OnHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (GetGlobalFlag(45))
            {
                attachee.SetObjectFlag(ObjectFlag.OFF);
                return SkipDefault;
            }

            return RunDefault;
        }
        public override bool OnJoin(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((!GameSystems.Combat.IsCombatActive()))
            {
                var obj = Utilities.find_npc_near(attachee, 8005);
                if ((obj != null && !GetGlobalFlag(806)))
                {
                    triggerer.AddFollower(obj);
                }

                return RunDefault;
            }

        }
        public override bool OnDisband(GameObjectBody attachee, GameObjectBody triggerer)
        {
            foreach (var obj in triggerer.GetPartyMembers())
            {
                if ((obj.GetNameId() == 8005 && !GetGlobalFlag(806)))
                {
                    triggerer.RemoveFollower(obj);
                }

            }

            foreach (var pc in GameSystems.Party.PartyMembers)
            {
                attachee.AIRemoveFromShitlist(pc);
                attachee.SetReaction(pc, 50);
            }

            return RunDefault;
        }
        public override bool OnNewMap(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (((attachee.GetMap() == 5062) || (attachee.GetMap() == 5111) || (attachee.GetMap() == 5112) || (attachee.GetMap() == 5002) || (attachee.GetMap() == 5091)))
            {
                var leader = attachee.GetLeader();
                if ((leader != null))
                {
                    var percent = Utilities.group_kobort_percent_hp(attachee, leader);
                    if ((percent < 30))
                    {
                        if ((Utilities.obj_percent_hp(attachee) > 70))
                        {
                            leader.BeginDialog(attachee, 420);
                        }

                    }

                }

            }

            return RunDefault;
        }
        public static bool switch_to_zert(GameObjectBody attachee, GameObjectBody triggerer, int line)
        {
            var npc = Utilities.find_npc_near(attachee, 8010);
            if ((npc != null))
            {
                triggerer.BeginDialog(npc, line);
                npc.TurnTowards(triggerer);
            }

            return SkipDefault;
        }
        public static bool run_off(GameObjectBody attachee, GameObjectBody triggerer)
        {
            attachee.RunOff();
            var obj = Utilities.find_npc_near(attachee, 8005);
            if ((obj != null))
            {
                obj.RunOff();
            }

            return RunDefault;
        }
        public static bool equip_transfer(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var itemA = attachee.FindItemByName(4110);
            if ((itemA != null))
            {
                itemA.Destroy();
                Utilities.create_item_in_inventory(4110, triggerer);
            }

            var itemB = attachee.FindItemByName(4060);
            if ((itemB != null))
            {
                itemB.Destroy();
                Utilities.create_item_in_inventory(4060, triggerer);
            }

            var itemC = attachee.FindItemByName(4060);
            if ((itemC != null))
            {
                itemC.Destroy();
                Utilities.create_item_in_inventory(4060, triggerer);
            }

            var itemD = attachee.FindItemByName(6203);
            if ((itemD != null))
            {
                itemD.Destroy();
                Utilities.create_item_in_inventory(6203, triggerer);
            }

            var itemE = attachee.FindItemByName(6202);
            if ((itemE != null))
            {
                itemE.Destroy();
                Utilities.create_item_in_inventory(6202, triggerer);
            }

            var itemF = attachee.FindItemByName(6025);
            if ((itemF != null))
            {
                itemF.Destroy();
                Utilities.create_item_in_inventory(6025, triggerer);
            }

            Utilities.create_item_in_inventory(7001, attachee);
            Utilities.create_item_in_inventory(7001, attachee);
            Utilities.create_item_in_inventory(7001, attachee);
            return RunDefault;
        }
        public static bool talk_to_cleric(GameObjectBody attachee, GameObjectBody triggerer, int line)
        {
            var npc = Utilities.find_npc_near(attachee, 14717);
            if ((npc != null))
            {
                triggerer.BeginDialog(npc, line);
                npc.TurnTowards(attachee);
                attachee.TurnTowards(npc);
            }
            else
            {
                triggerer.BeginDialog(attachee, 460);
            }

            return SkipDefault;
        }

    }
}

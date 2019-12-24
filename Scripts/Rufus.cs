
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
    [ObjectScript(18)]
    public class Rufus : BaseObjectScript
    {
        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetLeader() != null))
            {
                triggerer.BeginDialog(attachee, 210); // rufus in party
            }
            else if ((GetGlobalVar(912) == 32 && attachee.GetMap() != 5016 && attachee.GetMap() != 5018))
            {
                triggerer.BeginDialog(attachee, 240); // have attacked 3 or more farm animals with rufus in party and not in castle main hall or upper hall
            }
            else if ((GetGlobalFlag(835) && !GetGlobalFlag(37) && GetGlobalFlag(842) && !GetGlobalFlag(839)))
            {
                triggerer.BeginDialog(attachee, 320); // handled tower fight diplomatically and lareth is alive and have heard about prisoner lareth and have not liberated lareth
            }
            else if ((PartyLeader.HasReputation(27)))
            {
                triggerer.BeginDialog(attachee, 11002); // have rabble-rouser reputation - rufus won't talk to you
            }
            else
            {
                triggerer.BeginDialog(attachee, 1); // none of the above
            }

            return SkipDefault;
        }
        public override bool OnFirstHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetLeader() == null))
            {
                if (((GetGlobalVar(501) >= 2 && GetQuestState(97) != QuestState.Completed && GetQuestState(96) != QuestState.Completed) || GetGlobalVar(510) == 2))
                {
                    attachee.SetObjectFlag(ObjectFlag.OFF);
                }
                else
                {
                    attachee.ClearObjectFlag(ObjectFlag.OFF);
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

            SetGlobalFlag(336, true);
            SetGlobalFlag(284, true);
            if ((attachee.GetLeader() == null && attachee.FindItemByName(5009) == null && GetGlobalFlag(850)))
            {
                SetGlobalFlag(850, false);
                Utilities.create_item_in_inventory(5009, attachee);
            }

            if ((!GetGlobalFlag(233)))
            {
                SetGlobalVar(23, GetGlobalVar(23) + 1);
                if ((GetGlobalVar(23) >= 2))
                {
                    PartyLeader.AddReputation(92);
                }

            }
            else
            {
                SetGlobalVar(29, GetGlobalVar(29) + 1);
            }

            return RunDefault;
        }
        public override bool OnEnterCombat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetLeader() == null && attachee.FindItemByName(5009) != null && !GetGlobalFlag(850)))
            {
                attachee.FindItemByName(5009).Destroy();
                SetGlobalFlag(850, true);
            }

            return RunDefault;
        }
        public override bool OnResurrect(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetGlobalFlag(336, false);
            SetGlobalFlag(284, false);
            return RunDefault;
        }
        public override bool OnHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((!GameSystems.Combat.IsCombatActive()))
            {
                if ((GetGlobalVar(912) >= 3))
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
            SetGlobalFlag(233, true);
            return RunDefault;
        }
        public override bool OnDisband(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetGlobalFlag(233, false);
            foreach (var pc in GameSystems.Party.PartyMembers)
            {
                attachee.AIRemoveFromShitlist(pc);
                attachee.SetReaction(pc, 50);
            }

            return RunDefault;
        }
        public override bool OnNewMap(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetStat(Stat.level_fighter) >= 8))
            {
                SelectedPartyLeader.BeginDialog(attachee, 230);
            }

            return SkipDefault;
        }

    }
}

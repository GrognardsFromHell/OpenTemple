
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
    [ObjectScript(356)]
    public class CorporalHolly : BaseObjectScript
    {
        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetLeader() == null))
            {
                if ((GetGlobalVar(914) == 32 && attachee.GetMap() != 5149))
                {
                    triggerer.BeginDialog(attachee, 340); // have killed 3 or more farm animals with holly in party and not in verbo watch post main floor
                }
                else if ((GetGlobalVar(976) == 2))
                {
                    attachee.TurnTowards(triggerer);
                    triggerer.BeginDialog(attachee, 1); // captain absalom offered to send holly with you to investigate drow and you accepted
                }
                else if ((Utilities.is_daytime()))
                {
                    triggerer.BeginDialog(attachee, 10); // generic daytime
                }
                else
                {
                    triggerer.BeginDialog(attachee, 20); // generic nighttime
                }

            }
            else
            {
                attachee.TurnTowards(triggerer);
                triggerer.BeginDialog(attachee, 170); // holly in party
            }

            return SkipDefault;
        }
        public override bool OnFirstHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (((attachee.GetMap() == 5149) && (GetGlobalVar(976) == 4)))
            {
                attachee.ClearObjectFlag(ObjectFlag.OFF);
                SetGlobalVar(976, 5);
            }

            return RunDefault;
        }
        public override bool OnDying(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (CombatStandardRoutines.should_modify_CR(attachee))
            {
                CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
            }

            SetGlobalFlag(962, true);
            return RunDefault;
        }
        public override bool OnEnterCombat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            return RunDefault;
        }
        public override bool OnResurrect(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetGlobalFlag(962, false);
            return RunDefault;
        }
        public override bool OnHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((PartyLeader.HasReputation(34) || PartyLeader.HasReputation(35) || PartyLeader.HasReputation(42) || PartyLeader.HasReputation(44) || PartyLeader.HasReputation(35) || PartyLeader.HasReputation(43) || PartyLeader.HasReputation(46) || (GetGlobalVar(993) == 5 && !GetGlobalFlag(870))))
            {
                if (((GetGlobalVar(969) == 0) && (!GetGlobalFlag(955))))
                {
                    if ((!GameSystems.Combat.IsCombatActive()))
                    {
                        foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                        {
                            if ((is_better_to_talk(attachee, obj)))
                            {
                                attachee.TurnTowards(obj);
                                obj.BeginDialog(attachee, 320);
                                SetGlobalVar(969, 1);
                            }

                        }

                    }

                }

            }
            else if ((!GameSystems.Combat.IsCombatActive()))
            {
                if ((GetGlobalVar(914) >= 3))
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
        public override bool OnWillKos(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetLeader() == null))
            {
                if ((PartyLeader.HasReputation(34)) || (PartyLeader.HasReputation(35)))
                {
                    return RunDefault;
                }
                else if ((!GetGlobalFlag(992)))
                {
                    return SkipDefault;
                }

            }

            return RunDefault;
        }
        public override bool OnJoin(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var cloak = attachee.FindItemByName(6269);
            cloak.SetItemFlag(ItemFlag.NO_TRANSFER);
            var armor = attachee.FindItemByName(6103);
            armor.SetItemFlag(ItemFlag.NO_TRANSFER);
            var boots = attachee.FindItemByName(6040);
            boots.SetItemFlag(ItemFlag.NO_TRANSFER);
            var gloves = attachee.FindItemByName(6041);
            gloves.SetItemFlag(ItemFlag.NO_TRANSFER);
            var helm = attachee.FindItemByName(6335);
            helm.SetItemFlag(ItemFlag.NO_TRANSFER);
            var shield = attachee.FindItemByName(6499);
            shield.SetItemFlag(ItemFlag.NO_TRANSFER);
            var sword = attachee.FindItemByName(4030);
            sword.SetItemFlag(ItemFlag.NO_TRANSFER);
            return RunDefault;
        }
        public override bool OnDisband(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetGlobalVar(976, 4);
            var cloak = attachee.FindItemByName(6269);
            cloak.ClearItemFlag(ItemFlag.NO_TRANSFER);
            var armor = attachee.FindItemByName(6103);
            armor.ClearItemFlag(ItemFlag.NO_TRANSFER);
            var boots = attachee.FindItemByName(6040);
            boots.ClearItemFlag(ItemFlag.NO_TRANSFER);
            var gloves = attachee.FindItemByName(6041);
            gloves.ClearItemFlag(ItemFlag.NO_TRANSFER);
            var helm = attachee.FindItemByName(6335);
            helm.ClearItemFlag(ItemFlag.NO_TRANSFER);
            var shield = attachee.FindItemByName(6499);
            shield.ClearItemFlag(ItemFlag.NO_TRANSFER);
            var sword = attachee.FindItemByName(4030);
            sword.ClearItemFlag(ItemFlag.NO_TRANSFER);
            return RunDefault;
        }
        public override bool OnNewMap(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetLeader() != null))
            {
                if ((attachee.GetMap() == 5121))
                {
                    if ((GetGlobalVar(999) >= 15))
                    {
                        attachee.TurnTowards(triggerer);
                        SelectedPartyLeader.BeginDialog(attachee, 30);
                    }
                    else if ((GetQuestState(66) == QuestState.Accepted) || (GetQuestState(67) == QuestState.Accepted) || (GetQuestState(77) == QuestState.Accepted))
                    {
                        attachee.TurnTowards(triggerer);
                        SelectedPartyLeader.BeginDialog(attachee, 200);
                    }

                }
                else if (((attachee.GetMap() == 5158) && (!GetGlobalFlag(959))))
                {
                    SetGlobalFlag(959, true);
                    SelectedPartyLeader.BeginDialog(attachee, 210);
                }
                else if ((attachee.GetMap() == 5007) || (attachee.GetMap() == 5060) || (attachee.GetMap() == 5151))
                {
                    if (((GetGlobalVar(968) == 1) && (!Utilities.is_daytime())))
                    {
                        SelectedPartyLeader.BeginDialog(attachee, 220);
                    }

                }
                else if ((attachee.GetMap() == 5008) || (attachee.GetMap() == 5061) || (attachee.GetMap() == 5152))
                {
                    if (((GetGlobalVar(968) == 9) && (!Utilities.is_daytime())))
                    {
                        attachee.TurnTowards(triggerer);
                        SelectedPartyLeader.BeginDialog(attachee, 260);
                    }

                }

            }

            return RunDefault;
        }
        public static bool run_off(GameObjectBody attachee, GameObjectBody triggerer)
        {
            attachee.SetObjectFlag(ObjectFlag.OFF);
            return RunDefault;
        }
        public static bool get_holly_drunk(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetGlobalVar(968, GetGlobalVar(968) + 1);
            var ale = triggerer.FindItemByProto(8004);
            ale.Destroy();
            return RunDefault;
        }
        public static bool is_better_to_talk(GameObjectBody speaker, GameObjectBody listener)
        {
            if ((speaker.HasLineOfSight(listener)))
            {
                if ((speaker.DistanceTo(listener) <= 35))
                {
                    return true;
                }

            }

            return false;
        }

    }
}

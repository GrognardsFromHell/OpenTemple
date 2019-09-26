
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
    [ObjectScript(386)]
    public class GnomeGuard : BaseObjectScript
    {
        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((Utilities.is_daytime()))
            {
                triggerer.BeginDialog(attachee, 10);
            }
            else
            {
                triggerer.BeginDialog(attachee, 20);
            }

            return SkipDefault;
        }
        public override bool OnFirstHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (((PartyLeader.HasReputation(35) || PartyLeader.HasReputation(42)) && (attachee.GetMap() == 5121))) // turns on Verbobonc Exterior backup patrol
            {
                attachee.ClearObjectFlag(ObjectFlag.OFF);
            }
            else if (((!PartyLeader.HasReputation(35)) && (attachee.GetMap() == 5121))) // turns off Verbobonc Exterior backup patrol
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

            foreach (var pc in GameSystems.Party.PartyMembers)
            {
                pc.AddCondition("fallen_paladin");
            }

            SetGlobalVar(334, GetGlobalVar(334) + 1);
            if ((GetGlobalVar(334) >= 2))
            {
                PartyLeader.AddReputation(35);
            }

            if ((GetQuestState(67) == QuestState.Accepted))
            {
                SetGlobalFlag(964, true);
            }

            StartTimer(60000, () => go_away(attachee));
            return RunDefault;
        }
        public override bool OnStartCombat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var leader = SelectedPartyLeader;
            if (((GetQuestState(67) == QuestState.Accepted) && (!GetGlobalFlag(963))))
            {
                SetCounter(0, GetCounter(0) + 1);
                if ((GetCounter(0) >= 2))
                {
                    foreach (var pc in GameSystems.Party.PartyMembers)
                    {
                        if (pc.type == ObjectType.pc)
                        {
                            attachee.AIRemoveFromShitlist(pc);
                        }

                    }

                    SetGlobalFlag(963, true);
                    leader.BeginDialog(attachee, 1);
                    return SkipDefault;
                }

            }

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
                                obj.BeginDialog(attachee, 30);
                                SetGlobalVar(969, 1);
                            }

                        }

                    }

                }

            }

            return RunDefault;
        }
        public override bool OnWillKos(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((PartyLeader.HasReputation(34)) || (PartyLeader.HasReputation(35)))
            {
                return RunDefault;
            }
            else if ((!GetGlobalFlag(992)) || (!GetGlobalFlag(975)))
            {
                return SkipDefault;
            }

            return RunDefault;
        }
        public static bool guard_backup(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var guard_1 = GameSystems.MapObject.CreateObject(14700, attachee.GetLocation().OffsetTiles(-4, 0));
            guard_1.TurnTowards(PartyLeader);
            var guard_2 = GameSystems.MapObject.CreateObject(14700, attachee.GetLocation().OffsetTiles(-4, 0));
            guard_2.TurnTowards(PartyLeader);
            var guard_3 = GameSystems.MapObject.CreateObject(14700, attachee.GetLocation().OffsetTiles(-4, 0));
            guard_3.TurnTowards(PartyLeader);
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
        public static int is_close(GameObjectBody speaker, GameObjectBody listener)
        {
            if ((speaker.HasLineOfSight(listener)))
            {
                if ((speaker.DistanceTo(listener) <= 15))
                {
                    return 1;
                }

            }

            return 0;
        }
        public static bool go_away(GameObjectBody attachee)
        {
            attachee.SetObjectFlag(ObjectFlag.OFF);
            return RunDefault;
        }

    }
}

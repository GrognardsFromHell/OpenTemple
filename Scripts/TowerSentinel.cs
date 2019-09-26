
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
    [ObjectScript(121)]
    public class TowerSentinel : BaseObjectScript
    {
        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((triggerer.GetPartyMembers().Any(o => o.HasFollowerByName(8002))))
            {
                triggerer.BeginDialog(attachee, 70);
            }
            else
            {
                triggerer.BeginDialog(attachee, 1);
            }

            return SkipDefault;
        }
        public override bool OnDying(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (CombatStandardRoutines.should_modify_CR(attachee))
            {
                CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
            }

            return RunDefault;
        }
        public override bool OnHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((!GameSystems.Combat.IsCombatActive()))
            {
                if ((!attachee.HasMet(PartyLeader)))
                {
                    if ((is_better_to_talk(attachee, PartyLeader)))
                    {
                        if ((!Utilities.critter_is_unconscious(PartyLeader)))
                        {
                            if ((PartyLeader.GetPartyMembers().Any(o => o.HasFollowerByName(8002))))
                            {
                                attachee.TurnTowards(PartyLeader);
                                PartyLeader.BeginDialog(attachee, 70);
                            }
                            else
                            {
                                attachee.TurnTowards(PartyLeader);
                                PartyLeader.BeginDialog(attachee, 1);
                            }

                        }

                    }
                    else
                    {
                        foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                        {
                            if ((Utilities.is_safe_to_talk(attachee, obj)))
                            {
                                if ((obj.GetPartyMembers().Any(o => o.HasFollowerByName(8002))))
                                {
                                    attachee.TurnTowards(obj);
                                    obj.BeginDialog(attachee, 70);
                                }
                                else
                                {
                                    attachee.TurnTowards(obj);
                                    obj.BeginDialog(attachee, 1);
                                }

                            }

                        }

                    }

                }

            }

            return RunDefault;
        }
        public static bool talk_lareth(GameObjectBody attachee, GameObjectBody triggerer, int line)
        {
            var npc = Utilities.find_npc_near(attachee, 8002);
            if ((npc != null))
            {
                triggerer.BeginDialog(npc, line);
                npc.TurnTowards(attachee);
                attachee.TurnTowards(npc);
            }

            return SkipDefault;
        }
        public static bool run_off(GameObjectBody attachee, GameObjectBody triggerer)
        {
            // loc = location_from_axis(427,406)
            // attachee.runoff(loc)
            attachee.RunOff();
            Timed_Destroy(attachee, 5000);
            return RunDefault;
        }
        public static bool is_better_to_talk(GameObjectBody speaker, GameObjectBody listener)
        {
            if ((speaker.HasLineOfSight(listener)))
            {
                if ((speaker.DistanceTo(listener) <= 20))
                {
                    return true;
                }

            }

            return false;
        }
        public static void call_leader(GameObjectBody npc, GameObjectBody pc)
        {
            var leader = PartyLeader;
            leader.Move(pc.GetLocation().OffsetTiles(-2, 0));
            leader.BeginDialog(npc, 1);
            return;
        }
        public static void call_leaderplease(GameObjectBody npc, GameObjectBody pc)
        {
            var leader = PartyLeader;
            leader.Move(pc.GetLocation().OffsetTiles(-2, 0));
            leader.BeginDialog(npc, 70);
            return;
        }

    }
}

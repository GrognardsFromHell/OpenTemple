
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
    [ObjectScript(76)]
    public class Lieutenant : BaseObjectScript
    {
        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var ggv400 = GetGlobalVar(400);
            if (attachee.GetLeader() != null)
            {
                return RunDefault;
            }
            else if (triggerer.GetPartyMembers().Any(o => o.HasFollowerByName(8002)))
            {
                attachee.FloatLine(16002, triggerer);
            }
            else if ((ggv400 & (1)) != 0)
            {
                attachee.FloatLine(15500, triggerer);
            }
            else if ((!attachee.HasMet(triggerer)))
            {
                triggerer.BeginDialog(attachee, 1);
            }
            else if ((GetGlobalFlag(48) && !GetGlobalFlag(49)))
            {
                triggerer.BeginDialog(attachee, 70);
            }
            else
            {
                triggerer.BeginDialog(attachee, 100);
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
            var ggv400 = GetGlobalVar(400);
            var ggv403 = GetGlobalVar(403);
            if ((!GameSystems.Combat.IsCombatActive() && (ggv403 & (2)) == 0) && (ggv400 & (0x20)) == 0)
            {
                if ((is_better_to_talk(attachee, PartyLeader)))
                {
                    if ((!Utilities.critter_is_unconscious(PartyLeader)))
                    {
                        if ((!attachee.HasMet(PartyLeader)))
                        {
                            if ((is_better_to_talk(attachee, PartyLeader)))
                            {
                                attachee.TurnTowards(PartyLeader);
                                PartyLeader.BeginDialog(attachee, 1);
                                DetachScript();
                            }

                        }

                    }

                }
                else
                {
                    foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                    {
                        if ((Utilities.is_safe_to_talk(attachee, obj)))
                        {
                            if ((!attachee.HasMet(obj)))
                            {
                                attachee.TurnTowards(obj);
                                obj.BeginDialog(attachee, 1);
                                DetachScript();
                            }

                        }

                    }

                }

            }

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

    }
}

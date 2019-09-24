
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
    [ObjectScript(74)]
    public class Lubash : BaseObjectScript
    {
        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((triggerer.GetPartyMembers().Any(o => o.HasEquippedByName(3005))))
            {
                if ((attachee.HasMet(triggerer)))
                {
                    triggerer.BeginDialog(attachee, 100);
                }
                else
                {
                    triggerer.BeginDialog(attachee, 1);
                }

            }
            else
            {
                attachee.Attack(triggerer);
            }

            return SkipDefault;
        }
        public override bool OnDying(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (CombatStandardRoutines.should_modify_CR(attachee))
            {
                CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
            }

            SetGlobalFlag(59, true);
            SetGlobalVar(756, GetGlobalVar(756) + 1);
            // Record time when you killed Lubash
            if (GetGlobalVar(406) == 0)
            {
                SetGlobalVar(406, CurrentTimeSeconds);
            }

            return RunDefault;
        }
        public override bool OnResurrect(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetGlobalFlag(59, false);
            return RunDefault;
        }
        public override bool OnWillKos(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((triggerer.GetPartyMembers().Any(o => o.HasEquippedByName(3005))))
            {
                return SkipDefault;
            }
            else
            {
                return RunDefault;
            }

        }
        public override bool OnHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var cloak = PartyLeader.GetPartyMembers().Any(o => o.HasEquippedByName(3005));
            if ((!GameSystems.Combat.IsCombatActive()))
            {
                if ((!attachee.HasMet(PartyLeader)))
                {
                    if ((is_better_to_talk(attachee, PartyLeader)))
                    {
                        if ((cloak && !Utilities.critter_is_unconscious(PartyLeader)))
                        {
                            attachee.TurnTowards(PartyLeader);
                            PartyLeader.BeginDialog(attachee, 1);
                            DetachScript();
                        }

                    }
                    else
                    {
                        foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                        {
                            if ((Utilities.is_safe_to_talk(attachee, obj)))
                            {
                                if (cloak)
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
            leader.Move(pc.GetLocation() - 2);
            leader.BeginDialog(npc, 1);
            return;
        }

    }
}

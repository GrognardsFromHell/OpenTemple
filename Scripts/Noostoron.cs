
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
    [ObjectScript(621)]
    public class Noostoron : BaseObjectScript
    {
        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            attachee.TurnTowards(triggerer);
            triggerer.TurnTowards(attachee);
            triggerer.BeginDialog(attachee, 1);
            return SkipDefault;
        }
        public override bool OnDying(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (CombatStandardRoutines.should_modify_CR(attachee))
            {
                CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
            }

            SetGlobalFlag(561, true);
            if ((GetGlobalFlag(560) && GetGlobalFlag(562)))
            {
                PartyLeader.AddReputation(62);
            }

            return RunDefault;
        }
        public override bool OnResurrect(GameObjectBody attachee, GameObjectBody triggerer)
        {
            return SkipDefault;
        }
        public override bool OnHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetLeader() == null))
            {
                if ((!GameSystems.Combat.IsCombatActive()))
                {
                    foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                    {
                        if ((is_30_and_under(attachee, obj)))
                        {
                            attachee.TurnTowards(obj);
                            obj.BeginDialog(attachee, 1);
                            DetachScript();
                        }

                    }

                }

            }

            return RunDefault;
        }
        public static bool is_30_and_under(GameObjectBody speaker, GameObjectBody listener)
        {
            if ((speaker.HasLineOfSight(listener)))
            {
                if ((speaker.DistanceTo(listener) <= 30))
                {
                    return true;
                }

            }

            return false;
        }
        public static bool increment_rep(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((PartyLeader.HasReputation(81)))
            {
                PartyLeader.AddReputation(82);
                PartyLeader.RemoveReputation(81);
            }
            else if ((PartyLeader.HasReputation(82)))
            {
                PartyLeader.AddReputation(83);
                PartyLeader.RemoveReputation(82);
            }
            else if ((PartyLeader.HasReputation(83)))
            {
                PartyLeader.AddReputation(84);
                PartyLeader.RemoveReputation(83);
            }
            else if ((PartyLeader.HasReputation(84)))
            {
                PartyLeader.AddReputation(85);
                PartyLeader.RemoveReputation(84);
            }
            else if ((PartyLeader.HasReputation(85)))
            {
                PartyLeader.AddReputation(86);
                PartyLeader.RemoveReputation(85);
            }
            else if ((PartyLeader.HasReputation(86)))
            {
                PartyLeader.AddReputation(87);
                PartyLeader.RemoveReputation(86);
            }
            else if ((PartyLeader.HasReputation(87)))
            {
                PartyLeader.AddReputation(88);
                PartyLeader.RemoveReputation(87);
            }
            else if ((PartyLeader.HasReputation(88)))
            {
                PartyLeader.AddReputation(89);
                PartyLeader.RemoveReputation(88);
            }
            else
            {
                PartyLeader.AddReputation(81);
            }

            return RunDefault;
        }

    }
}

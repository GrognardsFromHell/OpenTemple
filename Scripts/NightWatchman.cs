
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
    [ObjectScript(393)]
    public class NightWatchman : BaseObjectScript
    {
        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((!attachee.HasMet(triggerer)))
            {
                triggerer.BeginDialog(attachee, 1);
            }
            else if ((attachee.HasMet(triggerer) && GetGlobalVar(700) == 0))
            {
                triggerer.BeginDialog(attachee, 120);
            }
            else if ((GetGlobalVar(700) == 1))
            {
                triggerer.BeginDialog(attachee, 100);
            }
            else if ((GetGlobalVar(700) == 2))
            {
                triggerer.BeginDialog(attachee, 110);
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
                    if ((is_cool_to_talk(attachee, PartyLeader)))
                    {
                        attachee.TurnTowards(PartyLeader);
                        PartyLeader.BeginDialog(attachee, 1);
                    }

                }
                else if ((attachee.HasMet(PartyLeader) && GetGlobalVar(700) == 0))
                {
                    if ((is_cool_to_talk(attachee, PartyLeader)))
                    {
                        attachee.TurnTowards(PartyLeader);
                        PartyLeader.BeginDialog(attachee, 120);
                        DetachScript();
                    }

                }
                else if ((GetGlobalVar(700) == 1))
                {
                    if ((is_cool_to_talk(attachee, PartyLeader)))
                    {
                        attachee.TurnTowards(PartyLeader);
                        PartyLeader.BeginDialog(attachee, 100);
                    }

                }

            }

            return RunDefault;
        }
        public static bool is_cool_to_talk(GameObjectBody speaker, GameObjectBody listener)
        {
            if ((speaker.DistanceTo(listener) <= 20))
            {
                return true;
            }

            return false;
        }

    }
}

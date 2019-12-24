
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

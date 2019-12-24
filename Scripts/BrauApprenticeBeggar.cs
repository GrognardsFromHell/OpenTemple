
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
    [ObjectScript(180)]
    public class BrauApprenticeBeggar : BaseObjectScript
    {
        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((SelectedPartyLeader.HasReputation(32) || SelectedPartyLeader.HasReputation(30) || SelectedPartyLeader.HasReputation(29)))
            {
                attachee.FloatLine(11004, triggerer);
            }
            else
            {
                if ((attachee.GetLeader() != null))
                {
                    if ((GetGlobalFlag(207)))
                    {
                        triggerer.BeginDialog(attachee, 80);
                    }
                    else
                    {
                        triggerer.BeginDialog(attachee, 100);
                    }

                }
                else if ((GetGlobalFlag(207)))
                {
                    triggerer.BeginDialog(attachee, 50);
                }
                else if ((attachee.HasMet(triggerer)))
                {
                    triggerer.BeginDialog(attachee, 30);
                }
                else
                {
                    triggerer.BeginDialog(attachee, 1);
                }

            }

            return SkipDefault;
        }
        public override bool OnFirstHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((GetGlobalFlag(205)))
            {
                attachee.ClearObjectFlag(ObjectFlag.OFF);
            }

            return RunDefault;
        }
        public override bool OnDying(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (CombatStandardRoutines.should_modify_CR(attachee))
            {
                CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
            }

            if ((!GetGlobalFlag(240)))
            {
                SetGlobalVar(23, GetGlobalVar(23) + 1);
                if (GetGlobalVar(23) >= 2)
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
        public override bool OnHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetLeader() == null))
            {
                if ((GetGlobalVar(501) == 4 || GetGlobalVar(501) == 5 || GetGlobalVar(501) == 6 || GetGlobalVar(510) == 2))
                {
                    attachee.SetObjectFlag(ObjectFlag.OFF);
                }

            }

            return RunDefault;
        }
        public override bool OnJoin(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetGlobalFlag(240, true);
            return RunDefault;
        }
        public override bool OnDisband(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetGlobalFlag(240, false);
            foreach (var pc in GameSystems.Party.PartyMembers)
            {
                attachee.AIRemoveFromShitlist(pc);
                attachee.SetReaction(pc, 50);
            }

            return RunDefault;
        }
        public static bool get_drunk(GameObjectBody attachee, GameObjectBody triggerer)
        {
            attachee.SetObjectFlag(ObjectFlag.OFF);
            StartTimer(3600000, () => comeback_drunk(attachee));
            return RunDefault;
        }
        public static bool comeback_drunk(GameObjectBody attachee)
        {
            attachee.ClearObjectFlag(ObjectFlag.OFF);
            SetGlobalFlag(207, true);
            var time = (3600000 * GetGlobalVar(21));
            StartTimer(time, () => get_sober(attachee));
            return RunDefault;
        }
        public static bool get_sober(GameObjectBody attachee)
        {
            SetGlobalFlag(207, false);
            return RunDefault;
        }
        public static bool run_off(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var loc = new locXY(427, 406);
            attachee.RunOff(loc);
            return RunDefault;
        }

    }
}


using System;
using System.Collections.Generic;
using OpenTemple.Core.GameObjects;
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
    [ObjectScript(341)]
    public class Kenan : BaseObjectScript
    {
        public override bool OnDialog(GameObject attachee, GameObject triggerer)
        {
            return RunDefault;
        }
        public override bool OnFirstHeartbeat(GameObject attachee, GameObject triggerer)
        {
            if ((GetGlobalVar(993) == 2))
            {
                attachee.ClearObjectFlag(ObjectFlag.OFF);
            }
            else if ((GetGlobalVar(993) == 3))
            {
                attachee.SetObjectFlag(ObjectFlag.OFF);
            }

            return RunDefault;
        }
        public override bool OnDying(GameObject attachee, GameObject triggerer)
        {
            if (CombatStandardRoutines.should_modify_CR(attachee))
            {
                CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
            }

            SetGlobalFlag(948, true);
            if ((GetGlobalFlag(949) && GetGlobalFlag(950) && GetGlobalFlag(951) && GetGlobalFlag(952) && GetGlobalFlag(953) && GetGlobalFlag(954)))
            {
                PartyLeader.AddReputation(40);
            }

            return RunDefault;
        }
        public override bool OnResurrect(GameObject attachee, GameObject triggerer)
        {
            SetGlobalFlag(948, false);
            PartyLeader.RemoveReputation(40);
            return RunDefault;
        }
        public override bool OnHeartbeat(GameObject attachee, GameObject triggerer)
        {
            if ((!GameSystems.Combat.IsCombatActive()))
            {
                if ((GetGlobalVar(993) == 2))
                {
                    foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                    {
                        if ((is_better_to_talk(attachee, obj)))
                        {
                            attachee.TurnTowards(PartyLeader);
                            PartyLeader.BeginDialog(attachee, 1);
                            DetachScript();
                        }

                    }

                }

            }

            return RunDefault;
        }
        public static bool is_better_to_talk(GameObject speaker, GameObject listener)
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
        public static bool switch_to_tarah(GameObject attachee, GameObject triggerer, int line)
        {
            var npc = Utilities.find_npc_near(attachee, 8805);
            if ((npc != null))
            {
                triggerer.BeginDialog(npc, line);
            }

            return SkipDefault;
        }

    }
}

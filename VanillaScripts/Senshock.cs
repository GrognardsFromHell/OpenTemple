
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
using OpenTemple.Core.Ui;
using System.Linq;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Utils;
using static OpenTemple.Core.Systems.Script.ScriptUtilities;

namespace VanillaScripts
{
    [ObjectScript(142)]
    public class Senshock : BaseObjectScript
    {

        public override bool OnDialog(GameObject attachee, GameObject triggerer)
        {
            if ((Utilities.find_npc_near(attachee, 8032) != null))
            {
                return RunDefault;
            }

            if ((GetGlobalFlag(144)))
            {
                attachee.Attack(triggerer);
            }
            else if ((!attachee.HasMet(triggerer)))
            {
                triggerer.BeginDialog(attachee, 1);
            }
            else
            {
                triggerer.BeginDialog(attachee, 110);
            }

            return SkipDefault;
        }
        public override bool OnWillKos(GameObject attachee, GameObject triggerer)
        {
            if ((triggerer.type == ObjectType.pc))
            {
                if ((GetGlobalFlag(144)))
                {
                    return RunDefault;
                }

            }

            return SkipDefault;
        }
        public override bool OnHeartbeat(GameObject attachee, GameObject triggerer)
        {
            if ((!GameSystems.Combat.IsCombatActive()))
            {
                if ((!GetGlobalFlag(144)))
                {
                    if ((Utilities.find_npc_near(attachee, 8032) == null))
                    {
                        foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                        {
                            if ((!attachee.HasMet(obj)))
                            {
                                if ((Utilities.is_safe_to_talk(attachee, obj)))
                                {
                                    obj.BeginDialog(attachee, 1);
                                    DetachScript();

                                }

                            }

                        }

                    }

                }

            }

            return RunDefault;
        }
        public override bool OnDying(GameObject attachee, GameObject triggerer)
        {
            SetGlobalFlag(147, true);
            return RunDefault;
        }
        public override bool OnResurrect(GameObject attachee, GameObject triggerer)
        {
            SetGlobalFlag(147, false);
            return RunDefault;
        }
        public static bool senshock_kills_hedrack(GameObject attachee, GameObject triggerer)
        {
            StartTimer(7200000, () => senshock_check_kill(attachee));
            return RunDefault;
        }
        public static bool senshock_check_kill(GameObject attachee)
        {
            if ((!GetGlobalFlag(146)))
            {
                if ((!GetGlobalFlag(147)))
                {
                    attachee.SetObjectFlag(ObjectFlag.OFF);
                    SetGlobalFlag(146, true);
                }

            }

            return RunDefault;
        }


    }
}

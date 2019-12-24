
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
using OpenTemple.Core.Ui;
using System.Linq;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Utils;
using static OpenTemple.Core.Systems.Script.ScriptUtilities;

namespace VanillaScripts
{
    [ObjectScript(77)]
    public class Sergeant : BaseObjectScript
    {

        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((GetGlobalFlag(37) && (GetGlobalFlag(49) || !GetGlobalFlag(48))))
            {
                triggerer.BeginDialog(attachee, 40);
            }
            else if ((GetGlobalFlag(49)))
            {
                triggerer.BeginDialog(attachee, 60);
            }
            else if ((GetGlobalFlag(48)))
            {
                triggerer.BeginDialog(attachee, 50);
            }
            else
            {
                triggerer.BeginDialog(attachee, 1);
            }

            return SkipDefault;
        }
        public override bool OnHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((!GameSystems.Combat.IsCombatActive()) && (!GetGlobalFlag(363)) && (attachee.GetLeader() == null))
            {
                foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                {
                    if ((Utilities.is_safe_to_talk(attachee, obj)))
                    {
                        if ((!attachee.HasMet(obj)))
                        {
                            obj.BeginDialog(attachee, 1);
                        }
                        else if ((!GetGlobalFlag(49) && GetGlobalFlag(48) && GetGlobalFlag(62)))
                        {
                            obj.BeginDialog(attachee, 50);
                        }
                        else if ((GetGlobalFlag(49)))
                        {
                            obj.BeginDialog(attachee, 60);
                        }
                        else
                        {
                            obj.BeginDialog(attachee, 70);
                        }

                    }

                }

            }

            return RunDefault;
        }
        public static bool run_off(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var loc = new locXY(526, 569);

            attachee.RunOff(loc);
            return RunDefault;
        }
        public static bool move_pc(GameObjectBody attachee, GameObjectBody triggerer)
        {
            FadeAndTeleport(0, 0, 0, 5005, 537, 545);
            return RunDefault;
        }
        public static bool deliver_pc(GameObjectBody attachee, GameObjectBody triggerer)
        {
            triggerer.Move(new locXY(491, 541));
            return RunDefault;
        }


    }
}

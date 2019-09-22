
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
using SpicyTemple.Core.Ui;
using System.Linq;
using SpicyTemple.Core.Systems.Script.Extensions;
using SpicyTemple.Core.Utils;
using static SpicyTemple.Core.Systems.Script.ScriptUtilities;

namespace VanillaScripts
{
    [ObjectScript(146)]
    public class Kelleen : BaseObjectScript
    {

        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((triggerer.GetPartyMembers().Any(o => o.HasEquippedByName(3017))))
            {
                triggerer.BeginDialog(attachee, 160);
            }
            else if ((triggerer.GetPartyMembers().Any(o => o.HasFollowerByName(8023))))
            {
                triggerer.BeginDialog(attachee, 200);
            }
            else if ((attachee.HasMet(triggerer)))
            {
                triggerer.BeginDialog(attachee, 170);
            }
            else if ((triggerer.GetPartyMembers().Any(o => o.HasEquippedByName(3016))))
            {
                triggerer.BeginDialog(attachee, 1);
            }
            else
            {
                triggerer.BeginDialog(attachee, 100);
            }

            return SkipDefault;
        }
        public override bool OnHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((!GameSystems.Combat.IsCombatActive()))
            {
                foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                {
                    if ((Utilities.is_safe_to_talk(attachee, obj)))
                    {
                        if ((obj.GetPartyMembers().Any(o => o.HasEquippedByName(3017))))
                        {
                            obj.BeginDialog(attachee, 160);
                        }
                        else if ((obj.GetPartyMembers().Any(o => o.HasFollowerByName(8023))))
                        {
                            obj.BeginDialog(attachee, 200);
                        }
                        else if ((attachee.HasMet(obj)))
                        {
                            obj.BeginDialog(attachee, 170);
                        }
                        else if ((obj.GetPartyMembers().Any(o => o.HasEquippedByName(3016))))
                        {
                            obj.BeginDialog(attachee, 1);
                        }
                        else
                        {
                            obj.BeginDialog(attachee, 100);
                        }

                        DetachScript();

                    }

                }

            }

            return RunDefault;
        }


    }
}

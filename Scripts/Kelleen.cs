
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

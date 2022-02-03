
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
    [ObjectScript(125)]
    public class Belsornig : BaseObjectScript
    {

        public override bool OnDialog(GameObject attachee, GameObject triggerer)
        {
            if ((GetGlobalFlag(132)))
            {
                attachee.Attack(triggerer);
            }
            else if ((!attachee.HasMet(triggerer)))
            {
                triggerer.BeginDialog(attachee, 1);
            }
            else
            {
                triggerer.BeginDialog(attachee, 370);
            }

            return SkipDefault;
        }
        public override bool OnDying(GameObject attachee, GameObject triggerer)
        {
            SetGlobalFlag(105, true);
            return RunDefault;
        }
        public override bool OnResurrect(GameObject attachee, GameObject triggerer)
        {
            SetGlobalFlag(105, false);
            return RunDefault;
        }
        public override bool OnEnterCombat(GameObject attachee, GameObject triggerer)
        {
            SetGlobalFlag(345, false);
            foreach (var statue in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_SCENERY))
            {
                if ((statue.GetNameId() == 1618))
                {
                    var loc = statue.GetLocation();

                    var rot = statue.Rotation;

                    statue.Destroy();
                    var juggernaut = GameSystems.MapObject.CreateObject(14244, loc);

                    juggernaut.Rotation = rot;

                    AttachParticles("ef-MinoCloud", juggernaut);
                    return RunDefault;
                }

            }

            return RunDefault;
        }
        public override bool OnWillKos(GameObject attachee, GameObject triggerer)
        {
            if ((!GetGlobalFlag(132)))
            {
                return SkipDefault;
            }
            else
            {
                return RunDefault;
            }

        }
        public override bool OnHeartbeat(GameObject attachee, GameObject triggerer)
        {
            if ((!GameSystems.Combat.IsCombatActive()))
            {
                foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                {
                    if ((Utilities.is_safe_to_talk(attachee, obj)))
                    {
                        if ((!attachee.HasMet(obj)))
                        {
                            obj.BeginDialog(attachee, 1);
                            DetachScript();

                        }

                    }

                }

            }

            return RunDefault;
        }


    }
}

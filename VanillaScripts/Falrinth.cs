
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
    [ObjectScript(155)]
    public class Falrinth : BaseObjectScript
    {

        public override bool OnDialog(GameObject attachee, GameObject triggerer)
        {
            if ((!attachee.HasMet(triggerer)))
            {
                triggerer.BeginDialog(attachee, 1);
            }
            else if ((GetGlobalFlag(164)))
            {
                triggerer.BeginDialog(attachee, 130);
            }

            return SkipDefault;
        }
        public override bool OnStartCombat(GameObject attachee, GameObject triggerer)
        {
            if ((GetGlobalFlag(164)))
            {
                DetachScript();

            }
            else if ((Utilities.obj_percent_hp(attachee) < 50))
            {
                foreach (var pc in GameSystems.Party.PartyMembers)
                {
                    if (pc.type == ObjectType.pc)
                    {
                        attachee.AIRemoveFromShitlist(pc);
                    }

                }

                falrinth_escape(attachee, triggerer);
                DetachScript();

            }

            return RunDefault;
        }
        public override bool OnDying(GameObject attachee, GameObject triggerer)
        {
            SetGlobalFlag(335, true);
            return RunDefault;
        }
        public override bool OnWillKos(GameObject attachee, GameObject triggerer)
        {
            if ((GetGlobalFlag(164)))
            {
                return SkipDefault;
            }

            return RunDefault;
        }
        public override bool OnHeartbeat(GameObject attachee, GameObject triggerer)
        {
            if ((!GameSystems.Combat.IsCombatActive()))
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

            return RunDefault;
        }
        public static bool falrinth_escape(GameObject attachee, GameObject triggerer)
        {
            SetGlobalFlag(164, true);
            StartTimer(43200000, () => falrinth_return(attachee));
            AttachParticles("sp-Dimension Door", attachee);
            attachee.SetObjectFlag(ObjectFlag.OFF);
            return RunDefault;
        }
        public static bool falrinth_return(GameObject attachee)
        {
            SetGlobalFlag(164, false);
            attachee.ClearObjectFlag(ObjectFlag.OFF);
            return RunDefault;
        }


    }
}

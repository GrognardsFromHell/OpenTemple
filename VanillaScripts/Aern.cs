
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
    [ObjectScript(145)]
    public class Aern : BaseObjectScript
    {

        public override bool OnDialog(GameObject attachee, GameObject triggerer)
        {
            if ((triggerer.GetPartyMembers().Any(o => o.HasEquippedByName(3016))))
            {
                triggerer.BeginDialog(attachee, 160);
            }
            else if ((attachee.HasMet(triggerer)))
            {
                triggerer.BeginDialog(attachee, 170);
            }
            else if (((triggerer.GetPartyMembers().Any(o => o.HasEquippedByName(3015))) || (triggerer.GetPartyMembers().Any(o => o.HasEquippedByName(3017)))))
            {
                triggerer.BeginDialog(attachee, 1);
            }
            else
            {
                triggerer.BeginDialog(attachee, 100);
            }

            return SkipDefault;
        }
        public override bool OnHeartbeat(GameObject attachee, GameObject triggerer)
        {
            if ((!GameSystems.Combat.IsCombatActive()))
            {
                foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                {
                    if ((Utilities.is_safe_to_talk(attachee, obj)))
                    {
                        if ((obj.GetPartyMembers().Any(o => o.HasEquippedByName(3016))))
                        {
                            obj.BeginDialog(attachee, 160);
                        }
                        else if ((attachee.HasMet(obj)))
                        {
                            obj.BeginDialog(attachee, 170);
                        }
                        else if (((obj.GetPartyMembers().Any(o => o.HasEquippedByName(3015))) || (obj.GetPartyMembers().Any(o => o.HasEquippedByName(3017)))))
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
        public static bool TalkOohlgrist(GameObject attachee, GameObject triggerer, int line)
        {
            var npc = Utilities.find_npc_near(attachee, 8023);

            if ((npc != null))
            {
                triggerer.BeginDialog(npc, line);
                npc.TurnTowards(attachee);
                attachee.TurnTowards(npc);
            }
            else
            {
                triggerer.BeginDialog(attachee, 190);
            }

            return SkipDefault;
        }


    }
}

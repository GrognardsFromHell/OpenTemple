
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
    [ObjectScript(145)]
    public class Aern : BaseObjectScript
    {

        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
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
        public override bool OnHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
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
        public static bool TalkOohlgrist(GameObjectBody attachee, GameObjectBody triggerer, int line)
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


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
    [ObjectScript(147)]
    public class Barkinar : BaseObjectScript
    {

        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((GetGlobalFlag(144)))
            {
                attachee.Attack(triggerer);
            }
            else if ((!attachee.HasMet(triggerer)))
            {
                if ((triggerer.GetPartyMembers().Any(o => o.HasEquippedByName(3010)) || triggerer.GetPartyMembers().Any(o => o.HasEquippedByName(3016)) || triggerer.GetPartyMembers().Any(o => o.HasEquippedByName(3017)) || triggerer.GetPartyMembers().Any(o => o.HasEquippedByName(3020))))
                {
                    triggerer.BeginDialog(attachee, 130);
                }
                else
                {
                    triggerer.BeginDialog(attachee, 1);
                }

            }
            else if ((GetGlobalFlag(163)))
            {
                triggerer.BeginDialog(attachee, 220);
            }
            else if ((GetGlobalFlag(157)))
            {
                if (((GetGlobalFlag(146)) && (GetGlobalFlag(147)) && (!GetGlobalFlag(153)) && (!GetGlobalFlag(156))))
                {
                    triggerer.BeginDialog(attachee, 140);
                }
                else
                {
                    triggerer.BeginDialog(attachee, 150);
                }

            }
            else if ((GetGlobalFlag(162)))
            {
                triggerer.BeginDialog(attachee, 160);
            }
            else if ((GetGlobalFlag(158)))
            {
                triggerer.BeginDialog(attachee, 190);
            }
            else if ((triggerer.GetPartyMembers().Any(o => o.HasEquippedByName(3010)) || triggerer.GetPartyMembers().Any(o => o.HasEquippedByName(3016)) || triggerer.GetPartyMembers().Any(o => o.HasEquippedByName(3017)) || triggerer.GetPartyMembers().Any(o => o.HasEquippedByName(3020))))
            {
                triggerer.BeginDialog(attachee, 130);
            }
            else
            {
                triggerer.BeginDialog(attachee, 210);
            }

            return SkipDefault;
        }
        public override bool OnWillKos(GameObjectBody attachee, GameObjectBody triggerer)
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
        public override bool OnHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((!GameSystems.Combat.IsCombatActive()))
            {
                if ((!GetGlobalFlag(144)))
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

            return RunDefault;
        }
        public static bool banter(GameObjectBody attachee, GameObjectBody triggerer, int line)
        {
            var npc = Utilities.find_npc_near(attachee, 8036);

            if ((npc != null))
            {
                triggerer.BeginDialog(npc, line);
                npc.TurnTowards(attachee);
                attachee.TurnTowards(npc);
            }
            else
            {
                triggerer.BeginDialog(attachee, 70);
            }

            return SkipDefault;
        }
        public static bool banter2(GameObjectBody attachee, GameObjectBody triggerer, int line)
        {
            var npc = Utilities.find_npc_near(attachee, 8036);

            if ((npc != null))
            {
                triggerer.BeginDialog(npc, line);
                npc.TurnTowards(attachee);
                attachee.TurnTowards(npc);
            }
            else
            {
                triggerer.BeginDialog(attachee, 80);
            }

            return SkipDefault;
        }


    }
}

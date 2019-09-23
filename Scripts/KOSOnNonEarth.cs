
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
using SpicyTemple.Core.Systems.ObjScript;
using SpicyTemple.Core.Ui;
using System.Linq;
using SpicyTemple.Core.Systems.Script.Extensions;
using SpicyTemple.Core.Utils;
using static SpicyTemple.Core.Systems.Script.ScriptUtilities;

namespace Scripts
{
    [ObjectScript(207)]
    public class KOSOnNonEarth : BaseObjectScript
    {
        public override bool OnFirstHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((new[] { 14337, 14338 }).Contains(attachee.GetNameId()))
            {
                if (attachee.GetScriptId(ObjScriptEvent.StartCombat) == 0)
                {
                    attachee.SetScriptId(ObjScriptEvent.StartCombat, 2);
                }

            }

            remove_dagger(attachee);
            DetachScript();
            return RunDefault;
        }
        public override bool OnWillKos(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((ScriptDaemon.get_v(454) & 1) != 0 && GetGlobalFlag(104))
            {
                // Earth Temple was on alert, and you killed Romag; don't expect to be safe in the Earth Temple anymore!
                return RunDefault;
            }

            if ((GetGlobalFlag(347)))
            {
                // KOS override
                return SkipDefault;
            }

            var saw_ally_robe = 0;
            var saw_greater_robe = 0;
            var saw_enemy_robe = 0;
            foreach (var obj in triggerer.GetPartyMembers())
            {
                var robe = obj.ItemWornAt(EquipSlot.Robes);
                if ((robe != null))
                {
                    if ((robe.GetNameId() == 3010))
                    {
                        // Earth robe
                        saw_ally_robe = 1;
                    }
                    else if ((robe.GetNameId() == 3021))
                    {
                        saw_greater_robe = 1;
                        break;

                    }
                    else if (((robe.GetNameId() == 3020) || (robe.GetNameId() == 3016) || (robe.GetNameId() == 3017)))
                    {
                        saw_enemy_robe = 1;
                    }

                }

            }

            if ((saw_greater_robe))
            {
                return SkipDefault;
            }
            else if (((saw_ally_robe) && (!saw_enemy_robe)))
            {
                return SkipDefault;
            }
            else
            {
                return RunDefault;
            }

        }
        public static void remove_dagger(GameObjectBody npc)
        {
            var dagger = npc.FindItemByProto(4060);
            while (dagger != null)
            {
                dagger.Destroy();
                dagger = npc.FindItemByProto(4060);
            }

            return;
        }

    }
}


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
    [ObjectScript(107)]
    public class Preston : BaseObjectScript
    {
        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((!attachee.HasMet(triggerer)))
            {
                triggerer.BeginDialog(attachee, 1);
            }
            else if ((GetGlobalFlag(93)))
            {
                triggerer.BeginDialog(attachee, 180);
            }
            else
            {
                triggerer.BeginDialog(attachee, 240);
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
            if ((!GameSystems.Combat.IsCombatActive() && PartyLeader.HasReputation(23) && GetGlobalFlag(94) && GetGlobalVar(706) == 0))
            {
                StartTimer(172800000, () => set_heads_up_var(attachee, triggerer)); // 2 days
                SetGlobalVar(706, 1);
            }
            else if ((!GameSystems.Combat.IsCombatActive() && PartyLeader.HasReputation(23) && GetGlobalFlag(94) && GetGlobalVar(706) == 2))
            {
                foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                {
                    if ((Utilities.is_safe_to_talk(attachee, obj)))
                    {
                        obj.TurnTowards(attachee);
                        attachee.TurnTowards(obj);
                        obj.BeginDialog(attachee, 400);
                        SetGlobalVar(706, 3);
                    }

                }

            }

            return RunDefault;
        }
        public static bool buttin(GameObjectBody attachee, GameObjectBody triggerer, int line)
        {
            var npc = Utilities.find_npc_near(attachee, 8020);
            if ((npc != null))
            {
                triggerer.BeginDialog(npc, line);
                npc.TurnTowards(attachee);
                attachee.TurnTowards(npc);
            }
            else
            {
                triggerer.BeginDialog(attachee, 220);
            }

            return SkipDefault;
        }
        public static bool set_heads_up_var(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetGlobalVar(706, 2);
            return RunDefault;
        }

    }
}

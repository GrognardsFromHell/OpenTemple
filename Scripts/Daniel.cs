
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
    [ObjectScript(349)]
    public class Daniel : BaseObjectScript
    {
        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((GetGlobalVar(993) == 5))
            {
                triggerer.BeginDialog(attachee, 20);
            }
            else
            {
                return RunDefault;
            }

            return SkipDefault;
        }
        public override bool OnFirstHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((GetGlobalVar(993) == 2))
            {
                attachee.ClearObjectFlag(ObjectFlag.OFF);
            }
            else if ((GetGlobalVar(993) == 3 || GetGlobalFlag(947)))
            {
                attachee.SetObjectFlag(ObjectFlag.OFF);
            }

            return RunDefault;
        }
        public override bool OnDying(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (CombatStandardRoutines.should_modify_CR(attachee))
            {
                CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
            }

            SetGlobalVar(995, GetGlobalVar(995) + 1);
            return RunDefault;
        }
        public override bool OnStartCombat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (((GetGlobalFlag(948)) && (GetGlobalFlag(949)) && (GetGlobalFlag(950)) && (GetGlobalFlag(951)) && (GetGlobalFlag(952)) && (GetGlobalFlag(953)) && (GetGlobalFlag(954))))
            {
                foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                {
                    Co8.StopCombat(attachee, 0);
                    obj.BeginDialog(attachee, 20);
                    return RunDefault;
                }

            }
            else
            {
                return SkipDefault;
            }

            return RunDefault;
        }
        public override bool OnHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((GetGlobalVar(993) == 5))
            {
                if ((!GameSystems.Combat.IsCombatActive()))
                {
                    foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                    {
                        if ((is_better_to_talk(attachee, obj)))
                        {
                            attachee.TurnTowards(obj);
                            obj.BeginDialog(attachee, 20);
                            DetachScript();
                        }

                    }

                }

            }

            return RunDefault;
        }
        public static bool switch_to_tarah(GameObjectBody attachee, GameObjectBody triggerer, int line)
        {
            var npc = Utilities.find_npc_near(attachee, 8805);
            var daniel = Utilities.find_npc_near(attachee, 8720);
            if ((npc != null))
            {
                triggerer.BeginDialog(npc, line);
                npc.TurnTowards(daniel);
                daniel.TurnTowards(npc);
            }

            return SkipDefault;
        }
        public static bool is_better_to_talk(GameObjectBody speaker, GameObjectBody listener)
        {
            if ((speaker.DistanceTo(listener) <= 40))
            {
                return true;
            }

            return false;
        }

    }
}

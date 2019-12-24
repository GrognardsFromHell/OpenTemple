
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
    [ObjectScript(598)]
    public class OrcWitch : BaseObjectScript
    {
        public override bool OnDying(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (CombatStandardRoutines.should_modify_CR(attachee))
            {
                CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
            }

            return RunDefault;
        }
        public override bool OnStartCombat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var webbed = Livonya.break_free(attachee, 3);
            if ((GetGlobalVar(785) == 1))
            {
                attachee.SetInt(obj_f.critter_strategy, 516);
            }
            else if ((GetGlobalVar(785) == 2))
            {
                attachee.SetInt(obj_f.critter_strategy, 517);
            }
            else if ((GetGlobalVar(785) == 3))
            {
                attachee.SetInt(obj_f.critter_strategy, 518);
            }
            else if ((GetGlobalVar(785) == 4))
            {
                attachee.SetInt(obj_f.critter_strategy, 519);
            }
            else if ((GetGlobalVar(785) == 5))
            {
                attachee.SetInt(obj_f.critter_strategy, 520);
            }
            else if ((GetGlobalVar(785) == 6))
            {
                attachee.SetInt(obj_f.critter_strategy, 521);
            }
            else if ((GetGlobalVar(785) == 7))
            {
                attachee.SetInt(obj_f.critter_strategy, 522);
            }
            else if ((GetGlobalVar(785) == 8))
            {
                attachee.SetInt(obj_f.critter_strategy, 523);
            }
            else if ((GetGlobalVar(785) == 9))
            {
                attachee.SetInt(obj_f.critter_strategy, 524);
            }
            else if ((GetGlobalVar(785) == 10))
            {
                attachee.SetInt(obj_f.critter_strategy, 525);
            }
            else if ((GetGlobalVar(785) == 11))
            {
                attachee.SetInt(obj_f.critter_strategy, 526);
            }
            else if ((GetGlobalVar(785) == 12))
            {
                attachee.SetInt(obj_f.critter_strategy, 527);
            }
            else if ((GetGlobalVar(785) == 13))
            {
                attachee.SetInt(obj_f.critter_strategy, 528);
            }
            else if ((GetGlobalVar(785) == 14))
            {
                attachee.SetInt(obj_f.critter_strategy, 529);
            }
            else if ((GetGlobalVar(785) == 15))
            {
                attachee.SetInt(obj_f.critter_strategy, 530);
            }
            else if ((GetGlobalVar(785) == 16))
            {
                attachee.SetInt(obj_f.critter_strategy, 531);
            }
            else if ((GetGlobalVar(785) >= 17))
            {
                attachee.SetInt(obj_f.critter_strategy, 532);
            }

            return RunDefault;
        }
        public override bool OnEndCombat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((GetGlobalVar(785) == 0))
            {
                SetGlobalVar(785, 1);
            }
            else if ((GetGlobalVar(785) == 1))
            {
                SetGlobalVar(785, 2);
            }
            else if ((GetGlobalVar(785) == 2))
            {
                SetGlobalVar(785, 3);
            }
            else if ((GetGlobalVar(785) == 3))
            {
                SetGlobalVar(785, 4);
            }
            else if ((GetGlobalVar(785) == 4))
            {
                SetGlobalVar(785, 5);
            }
            else if ((GetGlobalVar(785) == 5))
            {
                SetGlobalVar(785, 6);
            }
            else if ((GetGlobalVar(785) == 6))
            {
                SetGlobalVar(785, 7);
            }
            else if ((GetGlobalVar(785) == 7))
            {
                SetGlobalVar(785, 8);
            }
            else if ((GetGlobalVar(785) == 8))
            {
                SetGlobalVar(785, 9);
            }
            else if ((GetGlobalVar(785) == 9))
            {
                SetGlobalVar(785, 10);
            }
            else if ((GetGlobalVar(785) == 10))
            {
                SetGlobalVar(785, 11);
            }
            else if ((GetGlobalVar(785) == 11))
            {
                SetGlobalVar(785, 12);
            }
            else if ((GetGlobalVar(785) == 12))
            {
                SetGlobalVar(785, 13);
            }
            else if ((GetGlobalVar(785) == 13))
            {
                SetGlobalVar(785, 14);
            }
            else if ((GetGlobalVar(785) == 14))
            {
                SetGlobalVar(785, 15);
            }
            else if ((GetGlobalVar(785) == 15))
            {
                SetGlobalVar(785, 16);
            }
            else if ((GetGlobalVar(785) == 16))
            {
                SetGlobalVar(785, 17);
            }

            return RunDefault;
        }
        public override bool OnHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((GetGlobalFlag(571)))
            {
                attachee.Unconceal();
                StartTimer(4000, () => cast_buff(attachee, triggerer));
                DetachScript();
            }

            return RunDefault;
        }
        public static void cast_buff(GameObjectBody attachee, GameObjectBody triggerer)
        {
            attachee.CastSpell(WellKnownSpells.MirrorImage, attachee);
            return;
        }

    }
}

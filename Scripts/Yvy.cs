
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
    [ObjectScript(281)]
    public class Yvy : BaseObjectScript
    {
        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((SelectedPartyLeader.HasReputation(32) || SelectedPartyLeader.HasReputation(30) || SelectedPartyLeader.HasReputation(29)))
            {
                attachee.FloatLine(11004, triggerer);
            }
            else
            {
                triggerer.BeginDialog(attachee, 1);
            }

            attachee.TurnTowards(triggerer);
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
        public override bool OnInsertItem(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var done = attachee.GetInt(obj_f.weapon_pad_i_1);
            if (triggerer.HasFeat(FeatId.FAR_SHOT))
            {
                if (done == 1)
                {
                    return RunDefault;
                }
                else
                {
                    var curr = attachee.GetInt(obj_f.weapon_range);
                    curr = curr * 2;
                    attachee.SetInt(obj_f.weapon_range, curr);
                    attachee.SetInt(obj_f.weapon_pad_i_1, 1);
                    Sound(3013, 1);
                }

            }
            else
            {
                if (done == 1)
                {
                    var curr = attachee.GetInt(obj_f.weapon_range);
                    curr = curr * 0.5f;
                    attachee.SetInt(obj_f.weapon_range, curr);
                    attachee.SetInt(obj_f.weapon_pad_i_1, 0);
                    Sound(3013, 1);
                }

            }

            return RunDefault;
        }

    }
}

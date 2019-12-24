
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
                    curr = (int) (curr * 0.5f);
                    attachee.SetInt(obj_f.weapon_range, curr);
                    attachee.SetInt(obj_f.weapon_pad_i_1, 0);
                    Sound(3013, 1);
                }

            }

            return RunDefault;
        }

    }
}

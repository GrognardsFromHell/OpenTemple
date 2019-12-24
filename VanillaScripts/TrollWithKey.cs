
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
using OpenTemple.Core.Ui;
using System.Linq;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Utils;
using static OpenTemple.Core.Systems.Script.ScriptUtilities;

namespace VanillaScripts
{
    [ObjectScript(211)]
    public class TrollWithKey : BaseObjectScript
    {

        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((triggerer.HasEquippedByName(3005)))
            {
                triggerer.BeginDialog(attachee, 1);
                return SkipDefault;
            }
            else if ((triggerer.HasEquippedByName(3021)))
            {
                triggerer.BeginDialog(attachee, 1);
                return SkipDefault;
            }
            else if ((triggerer.HasEquippedByName(3020)))
            {
                triggerer.BeginDialog(attachee, 40);
                return SkipDefault;
            }

            triggerer.BeginDialog(attachee, 30);
            return SkipDefault;
        }
        public override bool OnHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((!GameSystems.Combat.IsCombatActive()))
            {
                GameObjectBody first_pc_seen = null;

                foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                {
                    if ((Utilities.is_safe_to_talk(attachee, obj)))
                    {
                        if ((first_pc_seen == null))
                        {
                            first_pc_seen = obj;

                        }

                        if ((obj.HasEquippedByName(3005)))
                        {
                            obj.BeginDialog(attachee, 1);
                            DetachScript();

                            return RunDefault;
                        }
                        else if ((obj.HasEquippedByName(3021)))
                        {
                            obj.BeginDialog(attachee, 1);
                            DetachScript();

                            return RunDefault;
                        }
                        else if ((obj.HasEquippedByName(3020)))
                        {
                            obj.BeginDialog(attachee, 40);
                            DetachScript();

                            return RunDefault;
                        }

                    }

                }

                if ((first_pc_seen != null))
                {
                    first_pc_seen.BeginDialog(attachee, 30);
                    DetachScript();

                }

            }

            return RunDefault;
        }


    }
}


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
    [ObjectScript(163)]
    public class Bassanio : BaseObjectScript
    {

        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (((triggerer.GetPartyMembers().Any(o => o.HasEquippedByName(3017))) || (GetQuestState(52) == QuestState.Mentioned)))
            {
                if ((!attachee.HasMet(triggerer)))
                {
                    triggerer.BeginDialog(attachee, 30);
                }
                else
                {
                    triggerer.BeginDialog(attachee, 50);
                }

            }
            else
            {
                triggerer.BeginDialog(attachee, 1);
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
                        if (((obj.GetPartyMembers().Any(o => o.HasEquippedByName(3017))) || (GetQuestState(52) == QuestState.Mentioned)))
                        {
                            if ((!attachee.HasMet(obj)))
                            {
                                obj.BeginDialog(attachee, 30);
                            }
                            else
                            {
                                obj.BeginDialog(attachee, 50);
                            }

                        }
                        else
                        {
                            obj.BeginDialog(attachee, 1);
                        }

                        DetachScript();

                    }

                }

            }

            return RunDefault;
        }
        public override bool OnDying(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetGlobalFlag(139, true);
            return RunDefault;
        }
        public override bool OnResurrect(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetGlobalFlag(139, false);
            return RunDefault;
        }


    }
}

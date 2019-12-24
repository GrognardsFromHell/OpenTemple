
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
    [ObjectScript(91)]
    public class Elmo : BaseObjectScript
    {

        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.HasMet(triggerer)))
            {
                if ((attachee.GetLeader() == null))
                {
                    triggerer.BeginDialog(attachee, 90);
                }

                triggerer.BeginDialog(attachee, 200);
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
                        if ((!attachee.HasMet(obj)))
                        {
                            obj.BeginDialog(attachee, 1);
                            DetachScript();

                        }

                    }

                }

            }

            return RunDefault;
        }
        public override bool OnDying(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetLeader() == null))
            {
                SetGlobalVar(23, GetGlobalVar(23) + 1);
                if ((GetGlobalVar(23) >= 2))
                {
                    PartyLeader.AddReputation(1);
                }

            }
            else
            {
                SetGlobalVar(29, GetGlobalVar(29) + 1);
            }

            return RunDefault;
        }
        public static bool make_otis_talk(GameObjectBody attachee, GameObjectBody triggerer, int line)
        {
            var npc = Utilities.find_npc_near(attachee, 8014);

            if ((npc != null))
            {
                triggerer.BeginDialog(npc, line);
                npc.TurnTowards(attachee);
                attachee.TurnTowards(npc);
            }
            else
            {
                triggerer.BeginDialog(attachee, 320);
            }

            return SkipDefault;
        }
        public static bool switch_to_thrommel(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var npc = Utilities.find_npc_near(attachee, 8031);

            if ((npc != null))
            {
                triggerer.BeginDialog(npc, 40);
                npc.TurnTowards(attachee);
                attachee.TurnTowards(npc);
            }
            else
            {
                triggerer.BeginDialog(attachee, 570);
            }

            return SkipDefault;
        }
        public static bool elmo_joins_first_time(GameObjectBody attachee, GameObjectBody triggerer)
        {
            triggerer.AdjustMoney(-20000);
            Utilities.create_item_in_inventory(6049, attachee);
            Utilities.create_item_in_inventory(6051, attachee);
            Utilities.create_item_in_inventory(4098, attachee);
            attachee.WieldBestInAllSlots();
            return SkipDefault;
        }


    }
}

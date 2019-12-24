
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
    [ObjectScript(57)]
    public class Bing : BaseObjectScript
    {
        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((GetQuestState(11) != QuestState.Botched && !GetGlobalFlag(978) && !GetGlobalFlag(34)))
            {
                triggerer.BeginDialog(attachee, 1);
            }
            else
            {
                triggerer.BeginDialog(attachee, 30);
            }

            return SkipDefault;
        }
        public override bool OnFirstHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((GetGlobalVar(501) == 4 || GetGlobalVar(501) == 5 || GetGlobalVar(501) == 6 || GetGlobalVar(510) == 2))
            {
                attachee.SetObjectFlag(ObjectFlag.OFF);
            }
            else if (((GetGlobalFlag(978)) && (attachee.GetMap() == 5026))) // turns on substitute bing
            {
                attachee.ClearObjectFlag(ObjectFlag.OFF);
                if ((!GetGlobalFlag(906)))
                {
                    StartTimer(604800000, () => respawn_bing(attachee)); // 604800000ms is 1 week
                    SetGlobalFlag(906, true);
                }

            }

            return RunDefault;
        }
        public override bool OnHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((GetGlobalVar(501) == 4 || GetGlobalVar(501) == 5 || GetGlobalVar(501) == 6 || GetGlobalVar(510) == 2))
            {
                attachee.SetObjectFlag(ObjectFlag.OFF);
            }
            else if ((GetGlobalFlag(978)))
            {
                attachee.SetObjectFlag(ObjectFlag.OFF);
            }
            else
            {
                attachee.ClearObjectFlag(ObjectFlag.OFF);
            }

            return RunDefault;
        }
        public override bool OnSpellCast(GameObjectBody attachee, GameObjectBody triggerer, SpellPacketBody spell)
        {
            if ((spell.spellEnum == WellKnownSpells.Heal))
            {
                SetGlobalFlag(34, true);
                triggerer.BeginDialog(attachee, 60);
            }

            return RunDefault;
        }
        public static void respawn_bing(GameObjectBody attachee)
        {
            var box = Utilities.find_container_near(attachee, 1004);
            InventoryRespawn.RespawnInventory(box);
            StartTimer(604800000, () => respawn_bing(attachee)); // 604800000ms is 1 week
            return;
        }

    }
}

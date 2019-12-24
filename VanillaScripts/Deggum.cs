
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
    [ObjectScript(148)]
    public class Deggum : BaseObjectScript
    {

        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((GetGlobalFlag(144)))
            {
                attachee.Attack(triggerer);
            }
            else if ((!attachee.HasMet(triggerer)))
            {
                triggerer.BeginDialog(attachee, 1);
            }
            else if ((GetGlobalFlag(165)))
            {
                triggerer.BeginDialog(attachee, 150);
            }
            else
            {
                triggerer.BeginDialog(attachee, 260);
            }

            return SkipDefault;
        }
        public override bool OnWillKos(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((triggerer.type == ObjectType.pc))
            {
                if ((GetGlobalFlag(144)))
                {
                    return RunDefault;
                }

            }

            return SkipDefault;
        }
        public static bool banter(GameObjectBody attachee, GameObjectBody triggerer, int line)
        {
            var npc = Utilities.find_npc_near(attachee, 8035);

            if ((npc != null))
            {
                triggerer.BeginDialog(npc, line);
                npc.TurnTowards(attachee);
                attachee.TurnTowards(npc);
            }
            else
            {
                triggerer.BeginDialog(attachee, 90);
            }

            return SkipDefault;
        }
        public static bool banter2(GameObjectBody attachee, GameObjectBody triggerer, int line)
        {
            var npc = Utilities.find_npc_near(attachee, 8035);

            if ((npc != null))
            {
                triggerer.BeginDialog(npc, line);
                npc.TurnTowards(attachee);
                attachee.TurnTowards(npc);
            }
            else
            {
                triggerer.BeginDialog(attachee, 80);
            }

            return SkipDefault;
        }


    }
}


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
    [ObjectScript(620)]
    public class GenericCitizen : BaseObjectScript
    {
        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            attachee.TurnTowards(triggerer);
            attachee.FloatLine(1000, triggerer);
            return SkipDefault;
        }
        public override bool OnEnterCombat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var name_exceptions = new[] { 14012, 14023, 14044, 20001, 14321 };
            if (!((name_exceptions).Contains(attachee.GetNameId())))
            {
                foreach (var pc in GameSystems.Party.PartyMembers)
                {
                    attachee.AIRemoveFromShitlist(pc);
                    attachee.SetReaction(pc, 50);
                }

            }

            return RunDefault;
        }
        public override bool OnExitCombat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            attachee.ClearObjectFlag(ObjectFlag.OFF);
            return RunDefault;
        }
        public override bool OnStartCombat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            attachee.SetObjectFlag(ObjectFlag.OFF);
            AttachParticles("mon-Chicken-white-hit", attachee);
            attachee.FloatMesFileLine("mes/float.mes", 5);
            return SkipDefault;
        }
        public override bool OnWillKos(GameObjectBody attachee, GameObjectBody triggerer)
        {
            return SkipDefault;
        }

    }
}

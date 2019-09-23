
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
    [ObjectScript(362)]
    public class VerboboncMage : BaseObjectScript
    {
        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            return RunDefault;
        }
        public override bool OnFirstHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetMap() == 5093 && GetGlobalVar(960) == 3))
            {
                attachee.ClearObjectFlag(ObjectFlag.OFF);
                attachee.CastSpell(WellKnownSpells.Shield, attachee);
            }

            return RunDefault;
        }
        public override bool OnDying(GameObjectBody attachee, GameObjectBody triggerer)
        {
            foreach (var pc in GameSystems.Party.PartyMembers)
            {
                pc.AddCondition("fallen_paladin");
            }

            if ((attachee.GetMap() == 5093))
            {
                ditch_rings(attachee, triggerer);
            }

            return RunDefault;
        }
        public override bool OnStartCombat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((GetGlobalVar(704) == 8))
            {
                foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                {
                    Co8.StopCombat(attachee, 0);
                    var wilfrick = Utilities.find_npc_near(attachee, 8703);
                    attachee.TurnTowards(wilfrick);
                    obj.BeginDialog(attachee, 1);
                    SetGlobalVar(704, 9);
                    return RunDefault;
                }

            }

            return RunDefault;
        }
        public static bool ass_out(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var wilfrick = Utilities.find_npc_near(attachee, 8703);
            AttachParticles("sp-Teleport", attachee);
            AttachParticles("sp-Teleport", wilfrick);
            Sound(4035, 1);
            attachee.SetObjectFlag(ObjectFlag.OFF);
            wilfrick.SetObjectFlag(ObjectFlag.OFF);
            PartyLeader.AddReputation(42);
            resume_fighting(attachee, triggerer);
            return RunDefault;
        }
        public static void resume_fighting(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var samson = Utilities.find_npc_near(attachee, 8724);
            var goliath = Utilities.find_npc_near(attachee, 8725);
            var bathsheba = Utilities.find_npc_near(attachee, 8726);
            var mage = Utilities.find_npc_near(attachee, 14658);
            var priest = Utilities.find_npc_near(attachee, 14471);
            var guard1 = Utilities.find_npc_near(attachee, 14716);
            var guard2 = Utilities.find_npc_near(attachee, 14719);
            samson.Attack(triggerer);
            goliath.Attack(triggerer);
            bathsheba.Attack(triggerer);
            mage.Attack(triggerer);
            priest.Attack(triggerer);
            guard1.Attack(triggerer);
            guard2.Attack(triggerer);
            return;
        }

    }
}

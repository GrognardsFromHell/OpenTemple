
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
    [ObjectScript(392)]
    public class HextorClericWelkwood : BaseObjectScript
    {
        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            triggerer.BeginDialog(attachee, 1);
            return SkipDefault;
        }
        public override bool OnFirstHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetMap() == 5093 && GetGlobalVar(960) == 3))
            {
                attachee.ClearObjectFlag(ObjectFlag.OFF);
                StartTimer(3000, () => go_boom(attachee, triggerer));
            }

            return RunDefault;
        }
        public override bool OnHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((!GameSystems.Combat.IsCombatActive()))
            {
                if ((attachee.GetMap() == 5093))
                {
                    if ((is_cool_to_talk(attachee, PartyLeader)))
                    {
                        if ((GetGlobalVar(960) == 3))
                        {
                            PartyLeader.TurnTowards(attachee);
                            PartyLeader.BeginDialog(attachee, 1);
                            SetGlobalVar(960, 4);
                        }

                    }

                }

            }

            return RunDefault;
        }
        public static bool is_cool_to_talk(GameObjectBody speaker, GameObjectBody listener)
        {
            if ((speaker.DistanceTo(listener) <= 20))
            {
                return true;
            }

            return false;
        }
        public static bool run_off_gang(GameObjectBody attachee, GameObjectBody triggerer)
        {
            Sound(4031, 1);
            AttachParticles("sp-Invisibility", attachee);
            attachee.SetObjectFlag(ObjectFlag.OFF);
            var sb = Utilities.find_npc_near(attachee, 14720);
            AttachParticles("sp-Invisibility", sb);
            sb.SetObjectFlag(ObjectFlag.OFF);
            sb = Utilities.find_npc_near(attachee, 14720);
            AttachParticles("sp-Invisibility", sb);
            sb.SetObjectFlag(ObjectFlag.OFF);
            sb = Utilities.find_npc_near(attachee, 14720);
            AttachParticles("sp-Invisibility", sb);
            sb.SetObjectFlag(ObjectFlag.OFF);
            sb = Utilities.find_npc_near(attachee, 14720);
            AttachParticles("sp-Invisibility", sb);
            sb.SetObjectFlag(ObjectFlag.OFF);
            sb = Utilities.find_npc_near(attachee, 14720);
            AttachParticles("sp-Invisibility", sb);
            sb.SetObjectFlag(ObjectFlag.OFF);
            sb = Utilities.find_npc_near(attachee, 14720);
            AttachParticles("sp-Invisibility", sb);
            sb.SetObjectFlag(ObjectFlag.OFF);
            sb = Utilities.find_npc_near(attachee, 14720);
            AttachParticles("sp-Invisibility", sb);
            sb.SetObjectFlag(ObjectFlag.OFF);
            return RunDefault;
        }
        public static bool go_boom(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SpawnParticles("sp-Fireball-Hit", new locXY(484, 468));
            SpawnParticles("ef-fire-lazy", new locXY(484, 468));
            SpawnParticles("ef-Embers Small", new locXY(484, 468));
            SpawnParticles("sp-Fireball-Hit", new locXY(468, 452));
            SpawnParticles("ef-fire-lazy", new locXY(468, 452));
            SpawnParticles("ef-Embers Small", new locXY(468, 452));
            SpawnParticles("ef-fire-lazy", new locXY(468, 485));
            SpawnParticles("ef-Embers Small", new locXY(468, 485));
            SpawnParticles("ef-fire-lazy", new locXY(468, 464));
            SpawnParticles("ef-Embers Small", new locXY(468, 464));
            Sound(4111, 1);
            return RunDefault;
        }

    }
}

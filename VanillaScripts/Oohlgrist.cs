
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
    [ObjectScript(126)]
    public class Oohlgrist : BaseObjectScript
    {

        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((!attachee.HasMet(triggerer)))
            {
                triggerer.BeginDialog(attachee, 1);
            }
            else if ((attachee.GetLeader() != null))
            {
                triggerer.BeginDialog(attachee, 420);
            }
            else if ((GetGlobalFlag(121) || GetGlobalFlag(122) || GetGlobalFlag(123)))
            {
                triggerer.BeginDialog(attachee, 150);
            }
            else
            {
                triggerer.BeginDialog(attachee, 160);
            }

            return SkipDefault;
        }
        public override bool OnDying(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetGlobalFlag(110, true);
            if ((attachee.GetLeader() != null))
            {
                SetGlobalVar(29, GetGlobalVar(29) + 1);
            }

            return RunDefault;
        }
        public override bool OnResurrect(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetGlobalFlag(110, false);
            return RunDefault;
        }
        public override bool OnStartCombat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (((Utilities.obj_percent_hp(attachee) < 50) && (!GetGlobalFlag(350))))
            {
                GameObjectBody found_pc = null;

                foreach (var pc in GameSystems.Party.PartyMembers)
                {
                    if (pc.type == ObjectType.pc)
                    {
                        found_pc = pc;

                        attachee.AIRemoveFromShitlist(pc);
                    }

                }

                if (found_pc != null)
                {
                    SetGlobalFlag(349, true);
                    found_pc.BeginDialog(attachee, 70);
                    return SkipDefault;
                }

            }

            return RunDefault;
        }
        public override bool OnHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((!GameSystems.Combat.IsCombatActive()))
            {
                var leader = attachee.GetLeader();

                if ((leader != null))
                {
                    if ((Utilities.obj_percent_hp(attachee) > 70))
                    {
                        if ((Utilities.group_percent_hp(leader) < 30))
                        {
                            attachee.FloatLine(460, leader);
                            attachee.Attack(leader);
                        }

                    }

                }

            }

            return RunDefault;
        }
        public override bool OnJoin(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetGlobalFlag(112, true);
            return RunDefault;
        }
        public static bool TalkAern(GameObjectBody attachee, GameObjectBody triggerer, int line)
        {
            var npc = Utilities.find_npc_near(attachee, 8033);

            if ((npc != null))
            {
                triggerer.BeginDialog(npc, line);
                npc.TurnTowards(attachee);
                attachee.TurnTowards(npc);
            }
            else
            {
                triggerer.BeginDialog(attachee, 480);
            }

            return SkipDefault;
        }


    }
}


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
    [ObjectScript(167)]
    public class Feldrin : BaseObjectScript
    {

        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            return SkipDefault;
        }
        public override bool OnStartCombat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((Utilities.obj_percent_hp(attachee) < 50))
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
                    foreach (var npc in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_NPC))
                    {
                        foreach (var pc in GameSystems.Party.PartyMembers)
                        {
                            npc.AIRemoveFromShitlist(pc);
                            npc.SetReaction(pc, 50);
                        }

                    }

                    found_pc.BeginDialog(attachee, 1);
                    DetachScript();

                    return SkipDefault;
                }

            }

            return RunDefault;
        }
        public override bool OnDying(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetGlobalFlag(177, true);
            return RunDefault;
        }
        public override bool OnResurrect(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetGlobalFlag(177, false);
            return RunDefault;
        }
        public override bool OnHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((GetGlobalFlag(176)))
            {
                foreach (var pc in GameSystems.Party.PartyMembers)
                {
                    attachee.AIRemoveFromShitlist(pc);
                }

                var location = new locXY(560, 437);

                attachee.RunOff(location);
            }

            return RunDefault;
        }
        public static bool run_off(GameObjectBody attachee, GameObjectBody triggerer)
        {
            attachee.RunOff();
            if ((!GetGlobalFlag(176)))
            {
                StartTimer(28800000, () => kill_brunk(attachee));
                SetGlobalFlag(176, true);
            }

            return RunDefault;
        }
        public static bool kill_brunk(GameObjectBody attachee)
        {
            SetGlobalFlag(174, true);
            return RunDefault;
        }


    }
}

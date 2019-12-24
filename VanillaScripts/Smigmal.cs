
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
    [ObjectScript(141)]
    public class Smigmal : BaseObjectScript
    {

        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((!attachee.HasMet(triggerer)))
            {
                triggerer.BeginDialog(attachee, 1);
            }
            else if ((GetGlobalFlag(144)))
            {
                triggerer.BeginDialog(attachee, 90);
            }
            else
            {
                triggerer.BeginDialog(attachee, 80);
            }

            return SkipDefault;
        }
        public override bool OnStartCombat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var leader = SelectedPartyLeader;

            if ((!attachee.HasMet(leader)))
            {
                SetCounter(0, GetCounter(0) + 1);
                if (((GetCounter(0) >= 4) && (Utilities.group_percent_hp(leader) > 30)))
                {
                    foreach (var pc in GameSystems.Party.PartyMembers)
                    {
                        if (pc.type == ObjectType.pc)
                        {
                            attachee.AIRemoveFromShitlist(pc);
                        }

                    }

                    leader.BeginDialog(attachee, 1);
                    DetachScript();

                    return SkipDefault;
                }

            }

            return RunDefault;
        }
        public override bool OnDying(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetGlobalFlag(338, true);
            return RunDefault;
        }
        public override bool OnWillKos(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((triggerer.type == ObjectType.pc))
            {
                if (((!attachee.HasMet(triggerer)) || (Utilities.group_percent_hp(triggerer) <= 30)))
                {
                    return RunDefault;
                }

            }

            return SkipDefault;
        }
        public override bool OnHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((!GameSystems.Combat.IsCombatActive()))
            {
                if ((GetGlobalFlag(144)))
                {
                    foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                    {
                        if ((attachee.HasMet(obj)))
                        {
                            if ((Utilities.is_safe_to_talk(attachee, obj)))
                            {
                                obj.BeginDialog(attachee, 90);
                                DetachScript();

                            }

                        }

                    }

                }

            }

            return RunDefault;
        }
        public static bool smigmal_escape(GameObjectBody attachee, GameObjectBody triggerer)
        {
            attachee.RunOff();
            StartTimer(7200000, () => smigmal_return(attachee));
            return RunDefault;
        }
        public static bool smigmal_return(GameObjectBody attachee)
        {
            attachee.ClearObjectFlag(ObjectFlag.OFF);
            SetGlobalFlag(144, true);
            return RunDefault;
        }


    }
}

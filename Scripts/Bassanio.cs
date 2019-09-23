
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
    [ObjectScript(163)]
    public class Bassanio : BaseObjectScript
    {
        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            attachee.TurnTowards(triggerer);
            if (((triggerer.GetPartyMembers().Any(o => o.HasEquippedByName(3017))) || (GetQuestState(52) >= QuestState.Mentioned)))
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
        public override bool OnFirstHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetLeader() == null && !GameSystems.Combat.IsCombatActive()))
            {
                SetGlobalVar(718, 0);
            }

            return RunDefault;
        }
        public override bool OnDying(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (CombatStandardRoutines.should_modify_CR(attachee))
            {
                CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
            }

            SetGlobalFlag(139, true);
            return RunDefault;
        }
        public override bool OnResurrect(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetGlobalFlag(139, false);
            return RunDefault;
        }
        public override bool OnHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((!GameSystems.Combat.IsCombatActive() && GetGlobalFlag(874)))
            {
                foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                {
                    obj.BeginDialog(attachee, 60);
                }

            }

            if ((attachee.GetMap() == 5067 && !GetGlobalFlag(873)))
            {
                var goaway = Utilities.find_container_near(attachee, 1027);
                var rot = goaway.Rotation;
                var loc = goaway.GetLocation();
                goaway.Destroy();
                // chest = game.obj_create( 1056, loc)
                // chest = game.obj_create( 1056, location_from_axis (434L, 585L))
                // chest = game.obj_create( 1056, location_from_axis (427L, 590L))
                // chest = game.obj_create( 1056, location_from_axis (436L, 586L))
                // chest = game.obj_create( 1056, location_from_axis (436L, 590L))
                var chest = GameSystems.MapObject.CreateObject(1056, new locXY(436, 586));
                chest.Rotation = rot;
                Utilities.create_item_in_inventory(8020, chest);
                Utilities.create_item_in_inventory(8020, chest);
                Utilities.create_item_in_inventory(6101, chest);
                Utilities.create_item_in_inventory(4136, chest);
                SetGlobalFlag(873, true);
            }

            if ((attachee.GetLeader() == null && GetGlobalVar(718) == 0 && !GameSystems.Combat.IsCombatActive()))
            {
                attachee.CastSpell(WellKnownSpells.ProtectionFromElements, attachee);
                attachee.PendingSpellsToMemorized();
            }

            SetGlobalVar(718, GetGlobalVar(718) + 1);
            if ((!GameSystems.Combat.IsCombatActive() && !GetGlobalFlag(853)))
            {
                foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                {
                    if ((Utilities.is_safe_to_talk(attachee, obj)))
                    {
                        if (((obj.GetPartyMembers().Any(o => o.HasEquippedByName(3017))) || (GetQuestState(52) >= QuestState.Mentioned)))
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

                        SetGlobalFlag(853, true);
                    }

                }

            }

            // game.new_sid = 0		#	removed by Livonya
            return RunDefault;
        }

    }
}

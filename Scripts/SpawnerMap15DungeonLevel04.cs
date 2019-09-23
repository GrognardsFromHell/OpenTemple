
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
    [ObjectScript(802)]
    public class SpawnerMap15DungeonLevel04 : BaseObjectScript
    {
        public override bool OnFirstHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetMap() == 5085 && PartyLeader.HasReputation(23) && GetGlobalFlag(94) && (!GetGlobalFlag(815) || !GetGlobalFlag(814))))
            {
                attachee.SetObjectFlag(ObjectFlag.OFF);
                var party_level = Utilities.group_average_level(SelectedPartyLeader);
                SetGlobalVar(759, GetGlobalVar(759) + 1);
                if (((GetGlobalVar(759) <= 2 && RandomRange(1, 4) <= 3) || party_level <= 5))
                {
                    return RunDefault;
                }
                else
                {
                    SetGlobalFlag(851, true);
                    if ((!GetGlobalFlag(836)))
                    {
                        var assassin = GameSystems.MapObject.CreateObject(14303, new locXY(468, 496)); // location 1
                        assassin.TurnTowards(triggerer);
                        assassin.WieldBestInAllSlots();
                        assassin.SetConcealed(13);
                    }

                    if ((GetGlobalFlag(836)))
                    {
                        var assassin = GameSystems.MapObject.CreateObject(14613, new locXY(477, 487)); // location 2
                        assassin.Rotation = 2.5f;
                        assassin.WieldBestInAllSlots();
                    }

                    if ((!GetGlobalFlag(815) && !GetGlobalFlag(814)))
                    {
                        var rannos = GameSystems.MapObject.CreateObject(14611, new locXY(475, 486)); // location 7
                        var gremag = GameSystems.MapObject.CreateObject(14612, new locXY(479, 485)); // location 6
                        rannos.Rotation = 2.5f;
                        gremag.Rotation = 2.5f;
                        rannos.WieldBestInAllSlots();
                        gremag.WieldBestInAllSlots();
                    }

                    if ((GetGlobalFlag(815) && !GetGlobalFlag(814)))
                    {
                        var rannos = GameSystems.MapObject.CreateObject(14611, new locXY(475, 486)); // location 7
                        var hired = GameSystems.MapObject.CreateObject(14613, new locXY(479, 485)); // location 6
                        rannos.Rotation = 2.5f;
                        rannos.WieldBestInAllSlots();
                        hired.Rotation = 2.5f;
                        hired.WieldBestInAllSlots();
                    }

                    if ((!GetGlobalFlag(815) && GetGlobalFlag(814)))
                    {
                        var gremag = GameSystems.MapObject.CreateObject(14612, new locXY(475, 486)); // location 7
                        var hired = GameSystems.MapObject.CreateObject(14613, new locXY(479, 485)); // location 6
                        gremag.Rotation = 2.5f;
                        gremag.WieldBestInAllSlots();
                        hired.Rotation = 2.5f;
                        hired.WieldBestInAllSlots();
                    }

                    var thug = GameSystems.MapObject.CreateObject(14606, new locXY(477, 488)); // location 2
                    thug.Rotation = 2.5f;
                    thug.WieldBestInAllSlots();
                    var barb = GameSystems.MapObject.CreateObject(14608, new locXY(474, 489)); // location 5
                    barb.Rotation = 2.5f;
                    barb.WieldBestInAllSlots();
                    var rr = RandomRange(1, 2);
                    if ((rr == 1))
                    {
                        var cleric = GameSystems.MapObject.CreateObject(14609, new locXY(470, 484)); // location 4
                        var mage = GameSystems.MapObject.CreateObject(14607, new locXY(478, 481)); // location 3
                        cleric.Rotation = 2.5f;
                        mage.Rotation = 2.5f;
                    }

                    if ((rr == 2))
                    {
                        var cleric = GameSystems.MapObject.CreateObject(14609, new locXY(478, 481)); // location 3
                        var mage = GameSystems.MapObject.CreateObject(14607, new locXY(470, 484)); // location 4
                        cleric.Rotation = 2.5f;
                        mage.Rotation = 2.5f;
                    }

                    mage.item_wield_best_all/*Unknown*/();
                    cleric.item_wield_best_all/*Unknown*/();
                    if ((!GetGlobalFlag(815) && !GetGlobalFlag(814)))
                    {
                        var leader = PartyLeader;
                        leader.BeginDialog(rannos, 2000);
                        return SkipDefault;
                    }

                    if ((GetGlobalFlag(815) && !GetGlobalFlag(814)))
                    {
                        var leader = PartyLeader;
                        leader.BeginDialog(rannos, 2100);
                        return SkipDefault;
                    }

                    if ((!GetGlobalFlag(815) && GetGlobalFlag(814)))
                    {
                        var leader = PartyLeader;
                        leader.BeginDialog(gremag, 2100);
                        return SkipDefault;
                    }

                }

            }

            return RunDefault;
        }
        public override bool OnEnterCombat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            attachee.RemoveFromInitiative();
            attachee.SetObjectFlag(ObjectFlag.OFF);
            return SkipDefault;
        }
        public override bool OnStartCombat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            attachee.RemoveFromInitiative();
            attachee.SetObjectFlag(ObjectFlag.OFF);
            return SkipDefault;
        }
        public override bool OnHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetMap() == 5078))
            {
                foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                {
                    if ((obj.DistanceTo(attachee) <= 15))
                    {
                        obj.BeginDialog(attachee, 2100);
                    }

                }

                return SkipDefault;
            }

            if ((attachee.GetMap() == 5015 && !GetGlobalFlag(37) && GetGlobalFlag(835)))
            {
                attachee.Destroy();
                var npc = GameSystems.MapObject.CreateObject(14371, new locXY(477, 488));
                var npc2 = GameSystems.MapObject.CreateObject(14371, new locXY(481, 475));
                var npc3 = GameSystems.MapObject.CreateObject(14371, new locXY(486, 479));
                var npc4 = GameSystems.MapObject.CreateObject(14371, new locXY(482, 486));
            }

            if ((attachee.GetMap() == 5014 && !GetGlobalFlag(37) && GetGlobalFlag(835)))
            {
                attachee.Destroy();
                // chains = game.obj_create( 2135, location_from_axis (491L, 484L))
                // chains.rotation = 2.5
                // chains = game.obj_create( 2135, location_from_axis (491L, 484L))
                // chains.rotation = 4.5
                // chains = game.obj_create( 2135, location_from_axis (491L, 484L))
                // chains.rotation = 3.5
                var npc = GameSystems.MapObject.CreateObject(14614, new locXY(490, 483));
                // npc2 = game.obj_create( 14607, location_from_axis (490L, 483L))
                // loc = npc.location
                // npc.stat_level_get(stat_subdual_damage)
                // npc.damage(OBJ_HANDLE_NULL,D20DT_SUBDUAL,dice_new("7d1"),D20DAP_NORMAL)
                npc.Damage(null, DamageType.Bludgeoning, Dice.Parse("52d1"));
                SetGlobalVar(758, 0);
            }

            // game.particles( "sp-Hold Person", npc )
            // game.particles( "sp-Bestow Curse", npc )
            // npc2.cast_spell(spell_hold_person, npc)
            // npc.condition_add_with_args( 'sp-Tashas Hideous Laughter', OBJ_HANDLE_NULL, 50000, 0 )
            // npc.condition_add_with_args("Prone",0,5000)
            return RunDefault;
        }

    }
}

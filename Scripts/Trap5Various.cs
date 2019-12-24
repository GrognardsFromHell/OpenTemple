
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
    [ObjectScript(32015)]
    public class Trap5Various : BaseObjectScript
    {
        public override bool OnHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((GetGlobalVar(509) == 0))
            {
                GameSystems.MapObject.CreateObject(1054, new locXY(559, 438));
                SetGlobalVar(509, GetGlobalVar(500) + 1);
            }

            return RunDefault;
        }
        public override bool OnTrap(TrapSprungEvent trap, GameObjectBody triggerer)
        {
            // numP = 210 / (game.party_npc_size() + game.party_pc_size())
            // for obj in game.obj_list_vicinity( triggerer.location, OLC_CRITTERS ):
            // obj.stat_base_set(stat_experience, (obj.stat_level_get(stat_experience) - numP))
            if ((triggerer.GetMap() == 5094))
            {
                // monsterA = game.obj_create( 14424, location_from_axis (487L, 505L) )	## test
                // monsterA = game.obj_create( 14600, location_from_axis (487L, 505L) )	## test
                SetGlobalVar(708, 0);
                var monsterA = GameSystems.MapObject.CreateObject(14601, new locXY(487, 505)); // test
                                                                                               // create_item_in_inventory ( 6072, monsterA )
                Utilities.create_item_in_inventory(4068, monsterA);
                // create_item_in_inventory ( 4004, monsterA )
                monsterA.SetConcealed(true);
                var monsterC = GameSystems.MapObject.CreateObject(14602, new locXY(487, 506));
                Utilities.create_item_in_inventory(4194, monsterC);
                monsterC.SetConcealed(true);
                var monsterD = GameSystems.MapObject.CreateObject(14602, new locXY(487, 504));
                Utilities.create_item_in_inventory(4194, monsterD);
                monsterD.SetConcealed(true);
                var monsterE = GameSystems.MapObject.CreateObject(14602, new locXY(488, 505));
                Utilities.create_item_in_inventory(4194, monsterE);
                monsterE.SetConcealed(true);
                // monsterA = game.obj_create( 14081, location_from_axis (487L, 505L) )	## test
                // monsterA.turn_towards(triggerer) 							## test
                // monsterA.attack(triggerer) 								## test
                // monsterA.concealed_set(1) 								## test
                // monsterB = game.obj_create( 14128, location_from_axis (487L, 499L) )
                // monsterB = game.obj_create( 14425, location_from_axis (487L, 499L) )  	## test
                // monsterB.turn_towards(triggerer)
                // monsterB.attack(triggerer)
                // monsterB.concealed_set(1)
                monsterC = GameSystems.MapObject.CreateObject(14107, new locXY(482, 496));
                monsterC.TurnTowards(triggerer);
                monsterC.Attack(triggerer);
                monsterC.SetConcealed(true);
                monsterD = GameSystems.MapObject.CreateObject(14083, new locXY(478, 501));
                monsterD.TurnTowards(triggerer);
                monsterD.Attack(triggerer);
                monsterD.SetConcealed(true);
                monsterE = GameSystems.MapObject.CreateObject(14107, new locXY(481, 506));
                monsterE.TurnTowards(triggerer);
                monsterE.Attack(triggerer);
                monsterE.SetConcealed(true);
            }

            DetachScript();
            return SkipDefault;
        }

    }
}

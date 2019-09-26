
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

    public class SampleTimedEventScript
    {
        //Sample script demonstrating calls to timedEventAdd
        //Tset(time) sets into motion the spawning of Furnok.
        //(time) is the time delay, in hours from the time the script 
        //is run until Furnok spawns.  Jade Empress is spawned and is 
        //passed as an arguement, along with the party leader, to show 
        //that the method preserves PyObjHandles across saves and loads. 
        //The stopFlags that will keep the spawn from firing are 58 (Furnok dead) 
        //and 51 (caught Furnok cheating).  Note that flag 51 will only 
        //stop spawn from firing if it is not set, i.e. game.global_flags[51] == 0.
        public static void Tset(FIXME time)
        {
            var Jade = GameSystems.MapObject.CreateObject(14455, PartyLeader.GetLocation() + 5);
            timedEventAdd(spawn, (PartyLeader, Jade), time, new[] { 58, (51, 0) });
        }
        public static void spawn(GameObjectBody attachee, GameObjectBody triggerer)
        {
            AttachParticles("Orb-Summon-Fire-Elemental", triggerer);
            AttachParticles("Orb-Summon-Air-Elemental", attachee);
            GameSystems.MapObject.CreateObject(14025, triggerer.GetLocation().OffsetTiles(-5, 0));
        }
        //nullSet(time) is used to demonstrate how the timedEventAdd 
        //can be called with no arguements for nullspawn passed and no 
        //stopFlags.  The only restriction is that the number and type 
        //of the arguements passed must match the number and type of the 
        //arguements required by the function passed.
        public static void nullSet(FIXME time)
        {
            timedEventAdd(nullSpawn, (), time);
        }
        public static void nullSpawn()
        {
            AttachParticles("Orb-Summon-Air-Elemental", PartyLeader);
            GameSystems.MapObject.CreateObject(14025, PartyLeader.GetLocation().OffsetTiles(-5, 0));
        }

    }
}

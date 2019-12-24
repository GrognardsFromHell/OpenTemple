
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
    [ObjectScript(294)]
    public class Spawner : BaseObjectScript
    {
        // Script contains:
        // -Hommlet: DH spawns, Council spawns (maps 5001 - Hommlet exterior, 5048 - Town Hall, 5049 - Stonemason)
        // -Moathouse: Moathouse respawns
        // -delayed DH spawn in Town Hall until after council meeting

        public override bool OnFirstHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (((GetGlobalVar(972) == 2) && (attachee.GetMap() == 5004)))
            {
                Moathouse_Respawn(attachee, triggerer);
                return RunDefault;
            }

            if (((attachee.GetMap() == 5001 || attachee.GetMap() == 5048 || attachee.GetMap() == 5049) && ScriptDaemon.get_v(435) != 0))
            {
                Council_Script(attachee, triggerer);
            }

            return RunDefault;
        }
        public override bool OnHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var c_time = Council.council_time();
            if ((attachee.GetMap() == 5048 && ScriptDaemon.get_v(435) != 0 && c_time == 1))
            {
                // Townhall
                if (ScriptDaemon.get_v(435) == 1)
                {
                    ScriptDaemon.set_v(435, 3);
                    Council_Script(attachee, triggerer);
                }

            }

            if ((attachee.GetMap() == 5048))
            {
                // Townhall
                if ((ScriptDaemon.get_v(436) == 3 && ScriptDaemon.get_v(435) < 4))
                {
                    // you've found the trap, initiate dialogue with rufus to GTFO
                    foreach (var npc in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_NPC))
                    {
                        if ((npc.GetNameId() == 8071))
                        {
                            SelectedPartyLeader.BeginDialog(npc, 2000);
                        }

                    }

                }

            }

            return RunDefault;
        }
        public static void Moathouse_Respawn(GameObjectBody attachee, GameObjectBody triggerer)
        {
            GameSystems.MapObject.CreateObject(2050, new locXY(487, 480));
            GameSystems.MapObject.CreateObject(2051, new locXY(512, 478));
            var statue = GameSystems.MapObject.CreateObject(2048, new locXY(494, 484)); // statue
            statue.Rotation = 5;
            attachee.Destroy();
            return;
        }
        public static void Council_Script(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetMap() == 5048 && !GetGlobalFlag(432)))
            {
                if ((ScriptDaemon.get_v(435) == 3))
                {
                    // full council assembly spawn
                    var burne = GameSystems.MapObject.CreateObject(14004, new locXY(477, 470));
                    burne.Move(new locXY(477, 470), 0f, 15f);
                    burne.Rotation = 2.3f;
                    burne.SetInt(obj_f.critter_description_unknown, 20000);
                    ScriptDaemon.destroy_weapons(burne, 4058, 0, 0);
                    var rufus = GameSystems.MapObject.CreateObject(14006, new locXY(474, 472));
                    rufus.Move(new locXY(474, 472), 15f, -10f);
                    rufus.Rotation = 2.5f;
                    rufus.SetInt(obj_f.critter_description_unknown, 8071);
                    var jaroo = GameSystems.MapObject.CreateObject(14005, new locXY(474, 476));
                    jaroo.Rotation = 5.5f;
                    ScriptDaemon.destroy_weapons(jaroo, 4047, 4111, 0);
                    jaroo.SetInt(obj_f.critter_description_unknown, 20001);
                    var renton = GameSystems.MapObject.CreateObject(14012, new locXY(477, 475));
                    renton.Move(new locXY(477, 475), 0f, -8f);
                    renton.Rotation = 5.4f;
                    var terjon = GameSystems.MapObject.CreateObject(14007, new locXY(480, 474));
                    terjon.Move(new locXY(480, 474), -25f, 0f);
                    terjon.Rotation = 5.8f;
                    ScriptDaemon.destroy_weapons(terjon, 4124, 6054, 0);
                    var badger1 = GameSystems.MapObject.CreateObject(14371, new locXY(482, 474));
                    badger1.Move(new locXY(482, 474), 0f, 0f);
                    badger1.Rotation = 2.1f;
                    var nevets = GameSystems.MapObject.CreateObject(14102, new locXY(475, 477));
                    nevets.Move(new locXY(475, 477), -8f, 0f);
                    nevets.Rotation = 5.2f;
                    var miller = GameSystems.MapObject.CreateObject(14031, new locXY(477, 477));
                    miller.Move(new locXY(477, 477), 3f, 0f);
                    miller.Rotation = 5.3f;
                    var gundi = GameSystems.MapObject.CreateObject(14016, new locXY(479, 477));
                    gundi.Move(new locXY(479, 477), 0f, 0f);
                    gundi.Rotation = 5.8f;
                    SetGlobalFlag(432, true);
                    SelectedPartyLeader.BeginDialog(burne, 7000);
                }
                else if ((ScriptDaemon.get_v(435) == 2))
                {
                    // only badgers spawn, if you suspected R&G
                    var badger1 = GameSystems.MapObject.CreateObject(14371, new locXY(476, 477));
                    badger1.Rotation = 2.5f;
                    var badger2 = GameSystems.MapObject.CreateObject(14371, new locXY(479, 477));
                    badger2.Rotation = 2.6f;
                    var badger3 = GameSystems.MapObject.CreateObject(14371, new locXY(474, 476));
                    badger3.Rotation = 2.5f;
                    SetGlobalFlag(432, true);
                }

            }
            else if ((attachee.GetMap() == 5048 && ScriptDaemon.get_v(435) >= 4))
            {
                // this should delete everyone after it's all over
                foreach (var npc in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_NPC))
                {
                    if ((to_be_deleted(npc) == 1 && npc.GetLeader() == null))
                    {
                        npc.Destroy();
                    }

                }

            }

            return;
        }
        public static int to_be_deleted(GameObjectBody npc)
        {
            // 8008 - Gundigoot
            // 8054 - Burne
            // 14031 - Miller
            // 14102 - Nevets
            // 14371 - badger
            // 20001 - Jaroo
            // 20003 - Terjon
            // 20007 - Renton
            if ((npc.GetNameId() == 8008 || npc.GetNameId() == 8048 || npc.GetNameId() == 8049 || npc.GetNameId() == 8054 || npc.GetNameId() == 8071 || npc.GetNameId() == 14031 || npc.GetNameId() == 14102 || npc.GetNameId() == 14806 || npc.GetNameId() == 14371 || npc.GetNameId() == 20001 || npc.GetNameId() == 20003 || npc.GetNameId() == 20007))
            {
                return 1;
            }

            return 0;
        }
        public static void destroy_weapons(GameObjectBody npc, int item1, int item2, int item3)
        {
            if ((item1 != 0))
            {
                var moshe = npc.FindItemByName(item1);
                if ((moshe != null))
                {
                    moshe.Destroy();
                }

            }

            if ((item2 != 0))
            {
                var moshe = npc.FindItemByName(item2);
                if ((moshe != null))
                {
                    moshe.Destroy();
                }

            }

            if ((item3 != 0))
            {
                var moshe = npc.FindItemByName(item3);
                if ((moshe != null))
                {
                    moshe.Destroy();
                }

            }

            return;
        }

    }
}

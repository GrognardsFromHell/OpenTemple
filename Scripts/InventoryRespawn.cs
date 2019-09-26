
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
using SpicyTemple.Core.IO;
using SpicyTemple.Core.Logging;
using SpicyTemple.Core.Systems.Script.Extensions;
using SpicyTemple.Core.TigSubsystems;
using SpicyTemple.Core.Utils;
using static SpicyTemple.Core.Systems.Script.ScriptUtilities;

namespace Scripts
{

    public static class InventoryRespawn
    {

        private static readonly ILogger Logger = new ConsoleLogger();

        // Define dictionaries of ordinary gems and jewelry in the game.
        // Format is  key : [value in gp, [list of proto numbers]]

        private static readonly Dictionary<int, ValueTuple<int, int[]>> gem_table =
            new Dictionary<int, ValueTuple<int, int[]>>
            {
                {1, (10, new[] {12042, 12044})},
                {2, (50, new[] {12041, 12042})},
                {3, (100, new[] {12035, 12040})},
                {4, (500, new[] {12034, 12039})},
                {5, (1000, new[] {12010, 12038})},
                {6, (5000, new[] {12036, 12037})},
            };

        private static readonly Dictionary<int, ValueTuple<int, int[]>> jewelry_table =
            new Dictionary<int, ValueTuple<int, int[]>>
            {
                {1, (50, new[] {6180, 6190})},
                {2, (100, new[] {6181, 6185})},
                {3, (200, new[] {6157})},
                {4, (250, new[] {6182, 6194})},
                {5, (500, new[] {6186, 6191})},
                {6, (750, new[] {6183, 6193})},
                {7, (1000, new[] {6184, 6192})},
                {8, (2500, new[] {6187, 6197})},
                {9, (5000, new[] {6188, 6195})},
                {10, (7500, new[] {6189, 6196})},
            };

        public static void RespawnInventory(GameObjectBody attachee, int num = 0)
        {
            // Removes all attachee's inventory, and respawns it friom the InvenSource.mes line number specified by 'num'.
            // If num is not given in the function call, the function will attempt to use the default InvenSource.mes line number for the attachee, if one is defined.
            // If no InvenSource.mes line number is defined, the function will terminate.
            // Example call 1:  RespawnInventory(attachee, 1) will create Burne's inventory(per line number 1 in InvenSource.mes) in attachee's inventory.
            // Example call 2:  RespawnInventory(attachee) will attempt to create the attachee's pre-defined inventory (per InvenSource.mes).
            // If the attachee has no Inventory Source defined, the function will terminate.
            if (num == 0)
            {
                if (attachee.type == ObjectType.container)
                {
                    num = attachee.GetInt(obj_f.container_inventory_source);
                }
                else if (attachee.type == ObjectType.npc)
                {
                    num = attachee.GetInt(obj_f.critter_inventory_source);
                }
                else
                {
                    Logger.Info("{0}is not a valid type", attachee);
                    return;
                }

            }

            if (num == 0)
            {
                Logger.Info("{0}has no inventory source defined", attachee);
                Logger.Info("Please specify an inventory to respawn");
                return;
            }

            ClearInv(attachee);
            CreateInv(attachee, num);
            return;
        }
        public static void ClearInv(GameObjectBody attachee)
        {
            // Removes all inventory from attachee.
            for (var num = 4000; num < 13000; num++)
            {
                var item = attachee.FindItemByProto(num);
                while ((item != null))
                {
                    item.Destroy();
                    item = attachee.FindItemByProto(num);
                }

            }

            return;
        }
        public static void CreateInv(GameObjectBody attachee, int num)
        {
            // Creates inventory from the structured list created by GetInv from the InvenSource.mes line number 'num'.
            var inv = GetInv(num);
            for (var i = 0; i < inv.Count; i++)
            {
                if (!(typeof(inv[i][0]) is str))
                {
                    if (typeof(inv[i][1]) is int)
                    {
                        if (inv[i][0] <= 100)
                        {
                            var chance = inv[i][0];
                            if (chance >= RandomRange(1, 100))
                            {
                                Utilities.create_item_in_inventory(inv[i][1], attachee);
                            }

                        }
                        else
                        {
                            var money = Utilities.create_item_in_inventory(inv[i][0], attachee);
                            money.SetInt(obj_f.money_quantity, inv[i][1]);
                        }

                    }
                    else
                    {
                        if (inv[i][0] == 100)
                        {
                            var n = RandomRange(0, inv[i][1].Count - 1);
                            Utilities.create_item_in_inventory(inv[i][1][n], attachee);
                        }
                        else if (inv[i][0] >= 7000 && inv[i][0] <= 7003)
                        {
                            var money = Utilities.create_item_in_inventory(inv[i][0], attachee);
                            money.SetInt(obj_f.money_quantity, RandomRange(inv[i][1][0], inv[i][1][1]));
                        }

                    }

                }
                else
                {
                    var gjlist = CalcGJ(inv[i][0], inv[i][1]);
                    if (gjlist != new List<GameObjectBody>())
                    {
                        for (var k = 0; k < gjlist.Count; k++)
                        {
                            Utilities.create_item_in_inventory(gjlist[k], attachee);
                        }

                    }

                }

            }

            return;
        }
        public static FIXME GetInv(int num, string filename = "rules/InvenSource.mes")
        {
            // Reads InvenSource.mes, finds the line numbered 'num', and creates a structured list of the entries in that line.
            var InvDict = Tig.FS.ReadMesFile(filename); // readMes is in Co8.py
            var InvLine = InvDict[num].Split(":")[1].Trim();
            var n = InvLine.IndexOf("_num");
            if (n != -1)
            {
                n = n + 7;
                InvLine = InvLine[n..];
            }

            var parts = InvLine.Split(' ');
            foreach (var part in parts)
                if (!part.Contains("("))
                {
                    var subParts = part.Split(",");
                    for (var j = 0; j < subParts.Length; j++)
                    {
                        if (subParts[j] == "copper")
                        {
                            subParts[j] = 7000;
                        }
                        else if (subParts[j] == "silver")
                        {
                            subParts[j] = 7001;
                        }
                        else if (subParts[j] == "gold")
                        {
                            subParts[j] = 7002;
                        }
                        else if (subParts[j] == "platinum")
                        {
                            subParts[j] = 7003;
                        }
                        else if (typeof(subParts[j]) is str && subParts[j].IndexOf("-") != -1)
                        {
                            subParts[j] = subParts[j].Split("-");
                            for (var k = 0; k < subParts[j].Count; k++)
                            {
                                subParts[j][k] = ConvertToInt(subParts[j][k]);
                            }

                        }

                        if (typeof(subParts[j]) is str)
                        {
                            subParts[j] = ConvertToInt(part[j]);
                        }

                    }

                }
                else
                {
                    var temp1 = part;
                    temp1 = temp1.ToString();
                    temp1 = temp1[1..-1];
                    temp1 = temp1.Split(",");
                    for (var j = 0; j < temp1.Count; j++)
                    {
                        temp1[j] = ConvertToInt(temp1[j]);
                    }

                    part = new[] { 100, temp1 };
                }

            }

            return inv;
        }
        public static int ConvertToInt(FIXME string)
        {
            if (typeof(string) is str)
            {
                try
                {
                    string = (int)(string);
                }
                catch (Exception e)
                {
                    if (!(string == "gems" || string == "jewelry"))
                    {
                        Logger.Info("WARNING: NON-INTEGER FOUND");
                        Logger.Info("Non-integer found is{0}", string);
                    }

                }

            }
            else
            {
                Logger.Info("WARNING:  NON-STRING FOUND");
                Logger.Info("Non-string found is{0}", string);
            }

            return string;
        }
        public static List<GameObjectBody> CalcGJ(FIXME string, FIXME value)
        {
            var gjlist = new List<GameObjectBody>();
            if (string == "gems")
            {
                var table = gem_table;
            }
            else if (string == "jewelry")
            {
                var table = jewelry_table;
            }
            else
            {
                return gjlist;
            }

            if (!(typeof(value) is int))
            {
                value = ConvertToInt(value);
                if (!(typeof(value) is int))
                {
                    return gjlist;
                }

            }

            var n = table.Count;
            while (value >= table[1][0])
            {
                if (table[n][0] <= value)
                {
                    gjlist.Add(table[n][1][RandomRange(0, table[n][1].Count - 1)]);
                    value = value - table[n][0];
                }
                else
                {
                    n = n - 1;
                }

            }

            return gjlist;
        }

    }
}

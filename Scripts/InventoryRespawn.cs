
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

    public class InventoryRespawn
    {
        // Define dictionaries of ordinary gems and jewelry in the game.
        // Format is  key : [value in gp, [list of proto numbers]]

        private static readonly Dictionary<int, FIXME> gem_table = new Dictionary<int, FIXME> {
{1,new []{10, new []{12042, 12044}}},
{2,new []{50, new []{12041, 12042}}},
{3,new []{100, new []{12035, 12040}}},
{4,new []{500, new []{12034, 12039}}},
{5,new []{1000, new []{12010, 12038}}},
{6,new []{5000, new []{12036, 12037}}},
}
        ;
        private static readonly Dictionary<int, FIXME> jewelry_table = new Dictionary<int, FIXME> {
{1,new []{50, new []{6180, 6190}}},
{2,new []{100, new []{6181, 6185}}},
{3,new []{200, new []{6157}}},
{4,new []{250, new []{6182, 6194}}},
{5,new []{500, new []{6186, 6191}}},
{6,new []{750, new []{6183, 6193}}},
{7,new []{1000, new []{6184, 6192}}},
{8,new []{2500, new []{6187, 6197}}},
{9,new []{5000, new []{6188, 6195}}},
{10,new []{7500, new []{6189, 6196}}},
}
        ;
        public static void RespawnInventory(GameObjectBody attachee, FIXME num = 0)
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
        public static void CreateInv(GameObjectBody attachee, FIXME num)
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
        public static FIXME GetInv(FIXME num, FIXME filename = data\rules\InvenSource.mes)
        {
            // Reads InvenSource.mes, finds the line numbered 'num', and creates a structured list of the entries in that line.
            var InvDict = Co8.readMes(filename); // readMes is in Co8.py
            var InvLine = InvDict[num][0];
            InvLine = InvLine.split/*Unknown*/(":");
            InvLine.remove/*Unknown*/(InvLine[0]);
            InvLine[0] = InvLine[0].strip/*Unknown*/();
            var n = InvLine[0].find/*Unknown*/("_num");
            if (n != -1)
            {
                n = n + 7;
                InvLine[0] = InvLine[0][n..];
            }

            var inv = InvLine[0];
            inv = inv.split/*Unknown*/(" ");
            for (var i = 0; i < inv.Count; i++)
            {
                if (inv[i].find/*Unknown*/("(") == -1)
                {
                    inv[i] = inv[i].split/*Unknown*/(",");
                    for (var j = 0; j < inv[i].Count; j++)
                    {
                        if (inv[i][j] == "copper")
                        {
                            inv[i][j] = 7000;
                        }
                        else if (inv[i][j] == "silver")
                        {
                            inv[i][j] = 7001;
                        }
                        else if (inv[i][j] == "gold")
                        {
                            inv[i][j] = 7002;
                        }
                        else if (inv[i][j] == "platinum")
                        {
                            inv[i][j] = 7003;
                        }
                        else if (typeof(inv[i][j]) is str && inv[i][j].find/*Unknown*/("-") != -1)
                        {
                            inv[i][j] = inv[i][j].split/*Unknown*/("-");
                            for (var k = 0; k < inv[i][j].Count; k++)
                            {
                                inv[i][j][k] = ConvertToInt(inv[i][j][k]);
                            }

                        }

                        if (typeof(inv[i][j]) is str)
                        {
                            inv[i][j] = ConvertToInt(inv[i][j]);
                        }

                    }

                }
                else
                {
                    var temp1 = inv[i];
                    temp1 = temp1.ToString();
                    temp1 = temp1[1..-1];
                    temp1 = temp1.split/*Unknown*/(",");
                    for (var n = 0; n < temp1.Count; n++)
                    {
                        temp1[n] = ConvertToInt(temp1[n]);
                    }

                    var temp2 = new[] { 100, temp1 };
                    inv[i] = temp2;
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

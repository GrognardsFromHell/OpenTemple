
using System;
using System.Collections.Generic;
using OpenTemple.Core.GameObjects;
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

namespace Scripts;

[ObjectScript(416)]
public class StandardEquipmentChest : BaseObjectScript
{
    public override bool OnDialog(GameObject attachee, GameObject triggerer)
    {
        if ((!ScriptDaemon.npc_get(attachee, 1)))
        {
            triggerer.BeginDialog(attachee, 1);
        }
        else if ((ScriptDaemon.npc_get(attachee, 1)))
        {
            triggerer.BeginDialog(attachee, 100);
        }

        return SkipDefault;
    }
    public override bool OnStartCombat(GameObject attachee, GameObject triggerer)
    {
        var leader = PartyLeader;
        Co8.StopCombat(attachee, 0);
        leader.BeginDialog(attachee, 4000);
        return RunDefault;
    }
    public static void give_default_starting_equipment(int x = 0)
    {
        foreach (var pc in GameSystems.Party.PartyMembers)
        {
            if (pc.GetStat(Stat.level_barbarian) > 0)
            {
                foreach (var aaa in new[] { 4074, 6059, 6011, 6216, 8014 })
                {
                    Utilities.create_item_in_inventory(aaa, pc);
                }

            }
            else if (pc.GetStat(Stat.level_bard) > 0)
            {
                foreach (var aaa in new[] { 4009, 6147, 6011, 4096, 5005, 5005, 6012, 6238, 12564, 8014 })
                {
                    Utilities.create_item_in_inventory(aaa, pc);
                }

            }
            else if (pc.GetStat(Stat.level_druid) > 0)
            {
                foreach (var aaa in new[] { 6216, 6217, 4116, 4115, 5007, 5007, 8014 })
                {
                    Utilities.create_item_in_inventory(aaa, pc);
                }

            }
            else if (pc.GetStat(Stat.level_cleric) > 0 || pc.DivineSpellLevelCanCast() > 0)
            {
                foreach (var aaa in new[] { 6013, 6011, 6012, 6059, 4071, 8014 })
                {
                    Utilities.create_item_in_inventory(aaa, pc);
                }

            }
            else if (pc.GetStat(Stat.level_fighter) > 0)
            {
                foreach (var aaa in new[] { 6013, 6010, 6011, 6012, 6059, 4062, 8014 })
                {
                    Utilities.create_item_in_inventory(aaa, pc);
                }

            }
            else if (pc.GetStat(Stat.level_monk) > 0)
            {
                if ((new[] { RaceId.svirfneblin, RaceId.tallfellow }).Contains(pc.GetRace()))
                {
                    foreach (var aaa in new[] { 6205, 6202, 4060, 8014 })
                    {
                        // for aaa in [6205 ,6202 ,4060 ,8014]: # dagger (4060) instead of quarterstaff
                        Utilities.create_item_in_inventory(aaa, pc);
                    }

                }
                else
                {
                    foreach (var aaa in new[] { 6205, 6202, 4110, 8014 })
                    {
                        Utilities.create_item_in_inventory(aaa, pc);
                    }

                }

            }
            else if (pc.GetStat(Stat.level_paladin) > 0)
            {
                foreach (var aaa in new[] { 6013, 6012, 6011, 6032, 6059, 4036, 6124, 8014 })
                {
                    Utilities.create_item_in_inventory(aaa, pc);
                }

            }
            else if (pc.GetStat(Stat.level_ranger) > 0)
            {
                foreach (var aaa in new[] { 6013, 6012, 6011, 6059, 4049, 4201, 5004, 5004, 8014, 6269 })
                {
                    Utilities.create_item_in_inventory(aaa, pc);
                }

            }
            else if (pc.GetStat(Stat.level_rogue) > 0)
            {
                foreach (var aaa in new[] { 6042, 6045, 6046, 4049, 4060, 6233, 8014, 4096, 5005, 5005, 8014, 12012 })
                {
                    Utilities.create_item_in_inventory(aaa, pc);
                }

            }
            else if (pc.GetStat(Stat.level_sorcerer) > 0)
            {
                if ((new[] { RaceId.svirfneblin, RaceId.tallfellow }).Contains(pc.GetRace()))
                {
                    foreach (var aaa in new[] { 6211, 6045, 6046, 6124, 4060, 4115, 5007, 5007, 8014 })
                    {
                        // for aaa in [6211 ,6045 ,6046 ,6124 ,4060 ,4115 ,5007 ,5007 ,8014]: # dagger (4060) instead of spear
                        Utilities.create_item_in_inventory(aaa, pc);
                    }

                }
                else
                {
                    foreach (var aaa in new[] { 6211, 6045, 6046, 6124, 4117, 4115, 5007, 5007, 8014 })
                    {
                        Utilities.create_item_in_inventory(aaa, pc);
                    }

                }

            }
            else if (pc.GetStat(Stat.level_warmage) > 0)
            {
                if ((new[] { RaceId.svirfneblin, RaceId.tallfellow }).Contains(pc.GetRace()))
                {
                    foreach (var aaa in new[] { 6013, 6045, 6046, 6059, 4071, 4115, 5007, 5007, 8014 })
                    {
                        // for aaa in [6013 ,6045 ,6046 ,6059, 4071 , 4115 ,5007 ,5007, 8014]:  # mace (4071) instead of spear
                        Utilities.create_item_in_inventory(aaa, pc);
                    }

                }
                else
                {
                    foreach (var aaa in new[] { 6013, 6045, 6046, 6059, 4117, 4115, 5007, 5007, 8014 })
                    {
                        Utilities.create_item_in_inventory(aaa, pc);
                    }

                }

            }
            else if (pc.GetStat(Stat.level_beguilers) > 0)
            {
                foreach (var aaa in new[] { 6042, 6045, 6046, 4049, 4060, 6233, 8014, 4096, 5005, 5005, 8014, 12012 })
                {
                    Utilities.create_item_in_inventory(aaa, pc);
                }

            }
            else if (pc.GetStat(Stat.level_wizard) > 0 || pc.ArcaneSpellLevelCanCast() > 0)
            {
                if ((new[] { RaceId.svirfneblin, RaceId.tallfellow }).Contains(pc.GetRace()))
                {
                    foreach (var aaa in new[] { 4060, 4096, 5005, 5005, 6081, 6143, 6038, 6011, 8014 })
                    {
                        Utilities.create_item_in_inventory(aaa, pc);
                    }

                }
                else
                {
                    foreach (var aaa in new[] { 4110, 4096, 5005, 5005, 6081, 6143, 6038, 6011, 8014 })
                    {
                        Utilities.create_item_in_inventory(aaa, pc);
                    }

                }

            }
            else if (pc.GetStat(Stat.level_scout) > 0)
            {
                foreach (var aaa in new[] { 6013, 6012, 6011, 4049, 4201, 5004, 5004, 8014, 6269, 12012 })
                {
                    Utilities.create_item_in_inventory(aaa, pc);
                }

            }
            else
            {
                // else: # default to rogue outfit
                foreach (var aaa in new[] { 6042, 6045, 6046, 4049, 4060, 6233, 8014, 4096, 5005, 5005, 8014, 12012 })
                {
                    Utilities.create_item_in_inventory(aaa, pc);
                }

            }

        }

        return;
    }
    public static void defalt_equipment_autoequip()
    {
        foreach (var pc in GameSystems.Party.PartyMembers)
        {
            pc.WieldBestInAllSlots();
        }

    }

}
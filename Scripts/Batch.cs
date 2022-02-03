
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
using OpenTemple.Core.Logging;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Utils;
using static OpenTemple.Core.Systems.Script.ScriptUtilities;

namespace Scripts;

public class Batch
{

    private static readonly ILogger Logger = LoggingSystem.CreateLogger();

    // CB - sets entire groups experience points to xp

    public static int partyxpset(int xp)
    {
        // def partyxpset( xp ):  # CB - sets entire groups experience points to xp
        var pc = SelectedPartyLeader;
        foreach (var obj in pc.GetPartyMembers())
        {
            var curxp = obj.GetStat(Stat.experience);
            var newxp = curxp + xp;
            obj.SetBaseStat(Stat.experience, newxp);
        }

        return 1;
    }
    // CB - sets entire groups xp to minimum necessary for level imputted

    public static int partylevelset(int level)
    {
        // def partylevelset(level):  # CB - sets entire groups xp to minimum necessary for level imputted
        var pc = SelectedPartyLeader;
        foreach (var obj in pc.GetPartyMembers())
        {
            var newxp = (level * (500 * (level - 1)));
            obj.SetBaseStat(Stat.experience, newxp);
        }

        return 1;
    }
    // CB - sets ability to score for entire group

    public static int partyabset(int ab, int score)
    {
        // def partyabset (ab, score):  # CB - sets ability to score for entire group
        Stat abstat;
        switch (ab)
        {
            case 1:
                abstat = Stat.strength;
                break;
            case 2:
                abstat = Stat.dexterity;
                break;
            case 3:
                abstat = Stat.constitution;
                break;
            case 4:
                abstat = Stat.intelligence;
                break;
            case 5:
                abstat = Stat.wisdom;
                break;
            case 6:
                abstat = Stat.charisma;
                break;
            default:
                return 0;
        }

        if (((score > 0) && (score < 41)))
        {
            var pc = SelectedPartyLeader;
            foreach (var obj in pc.GetPartyMembers())
            {
                obj.SetBaseStat(abstat, score);
            }

        }
        else
        {
            return 0;
        }

        return 1;
    }
    // CB - sets all ability scores of specified pc to score

    public static int massabset(int num, int score)
    {
        // def massabset(num, score):  # CB - sets all ability scores of specified pc to score
        num = num - 1;
        var pc = GameSystems.Party.GetPartyGroupMemberN(num);
        if ((pc != null))
        {
            if (((score > 0) && (score < 41)))
            {
                pc.SetBaseStat(Stat.strength, score);
                pc.SetBaseStat(Stat.dexterity, score);
                pc.SetBaseStat(Stat.constitution, score);
                pc.SetBaseStat(Stat.intelligence, score);
                pc.SetBaseStat(Stat.wisdom, score);
                pc.SetBaseStat(Stat.charisma, score);
            }
            else
            {
                return 0;
            }

        }
        else
        {
            return 0;
        }

        return 1;
    }
    // CB - sets max hp of entire party to specified value

    public static int partyhpset(int hp)
    {
        // def partyhpset(hp):  # CB - sets max hp of entire party to specified value
        var pc = SelectedPartyLeader;
        foreach (var obj in pc.GetPartyMembers())
        {
            obj.SetBaseStat(Stat.hp_max, hp);
        }

        return 1;
    }
    public static bool setbonus(int x, string type, int bonus_one, int bonus_two)
    {
        var bonus = GameSystems.Party.GetPartyGroupMemberN(x).AddCondition(type, bonus_one, bonus_two);
        return bonus;
    }
    public static void follower(int proto_num)
    {
        var npc = GameSystems.MapObject.CreateObject(proto_num, SelectedPartyLeader.GetLocation());
        if (!(SelectedPartyLeader.HasMaxFollowers()))
        {
            SelectedPartyLeader.AddFollower(npc);
        }
        else
        {
            SelectedPartyLeader.AddAIFollower(npc);
        }

        // add familiar_obj to d20initiative, and set initiative to spell_caster's
        var caster_init_value = SelectedPartyLeader.GetInitiative();
        npc.AddToInitiative();
        npc.SetInitiative(caster_init_value);
        UiSystems.Combat.Initiative.UpdateIfNeeded();
    }

    public static void NighInvulnerable()
    {
        massabset(0, 24);
        var necklace = GameSystems.MapObject.CreateObject(6239, PartyLeader.GetLocation());
        var sword1 = GameSystems.MapObject.CreateObject(4599, PartyLeader.GetLocation());
        var sword2 = GameSystems.MapObject.CreateObject(4599, PartyLeader.GetLocation());
        var helm = GameSystems.MapObject.CreateObject(6036, PartyLeader.GetLocation());
        necklace.AddConditionToItem("Ring of freedom of movement", 0, 0);
        necklace.AddConditionToItem("Amulet of Mighty Fists", 5, 5);
        necklace.AddConditionToItem("Weapon Enhancement Bonus", 5, 0);
        // necklace.item_condition_add_with_args( 'Weapon Holy', 0, 0 )
        // necklace.item_condition_add_with_args( 'Weapon Lawful', 0, 0 )
        necklace.AddConditionToItem("Weapon Silver", 0, 0);
        // necklace.condition_add_with_args( 'Thieves Tools Masterwork', 0, 0)
        // sword.item_condition_add_with_args( 'Weapon Lawful', 0, 0 )
        // sword.item_condition_add_with_args( 'Weapon Silver', 0, 0 )
        PartyLeader.GetItem(necklace);
        PartyLeader.GetItem(sword1);
        PartyLeader.GetItem(sword2);
        PartyLeader.GetItem(helm);
        PartyLeader.AddCondition("Monster Regeneration 5", 0, 0);
        PartyLeader.AddCondition("Monster Subdual Immunity", 0, 0);
        PartyLeader.AddCondition("Monster Energy Immunity", "Fire", 0);
        PartyLeader.AddCondition("Monster Energy Immunity", "Cold", 0);
        PartyLeader.AddCondition("Monster Energy Immunity", "Electricity", 0);
        PartyLeader.AddCondition("Monster Energy Immunity", "Acid", 0);
        PartyLeader.AddCondition("Monster Confusion Immunity", 0, 0);
        PartyLeader.AddCondition("Monster Stable", 0, 0);
        PartyLeader.AddCondition("Monster Untripable", 0, 0);
        PartyLeader.AddCondition("Monster Plant", 0, 0);
        PartyLeader.AddCondition("Monster Poison Immunity", 0, 0);
        PartyLeader.AddCondition("Saving Throw Resistance Bonus", 0, 10);
        PartyLeader.AddCondition("Saving Throw Resistance Bonus", 1, 10);
        PartyLeader.AddCondition("Saving Throw Resistance Bonus", 2, 10);
        PartyLeader.AddCondition("Weapon Holy", 0, 0);
        PartyLeader.AddCondition("Weapon Lawful", 0, 0);
        PartyLeader.AddCondition("Weapon Silver", 0, 0);
        PartyLeader.SetInt(obj_f.speed_run, 2);
        PartyLeader.AdjustMoney(+90000000);
        AttachParticles("Orb-Summon-Air-Elemental", PartyLeader);
        return;
    }
    public static int alldie()
    {
        foreach (var obj in ObjList.ListVicinity(PartyLeader.GetLocation(), ObjectListFilter.OLC_CRITTERS))
        {
            if (!(GameSystems.Party.PartyMembers).Contains(obj) && obj.GetNameId() != 14455)
            {
                obj.KillWithDeathEffect();
            }

        }

        // damage_dice = dice_new( '104d20' )
        // obj.damage( OBJ_HANDLE_NULL, 0, damage_dice )
        return 1;
    }
    public static void speedup(int ec = 1, int file_override = -1)
    {
        if (file_override == -1)
        {
            throw new NotSupportedException("Reading speedup from INI is not supported");
        }
        else if (file_override != -1)
        {
            ec = file_override;
        }

        if (ec == 0)
        {
            // Vanilla - 106535216
            foreach (var pc in PartyLeader.GetPartyMembers())
            {
                if (pc.GetInt(obj_f.speed_run) != 1)
                {
                    pc.SetInt(obj_f.speed_run, 1);
                    if (GetGlobalFlag(403))
                    {
                        AttachParticles("Orb-Summon-Air-Elemental", pc);
                    }

                }

            }

        }

        if (ec == 1)
        {
            Logger.Info("Setting speed to x2");
            // The old value - slightly faster than vanilla (which was 106535216)
            foreach (var pc in PartyLeader.GetPartyMembers())
            {
                if (pc.GetInt(obj_f.speed_run) != 2)
                {
                    pc.SetInt(obj_f.speed_run, 2);
                    if (GetGlobalFlag(403))
                    {
                        AttachParticles("Orb-Summon-Air-Elemental", pc);
                    }

                }

            }

        }

        if (ec == 2)
        {
            foreach (var pc in PartyLeader.GetPartyMembers())
            {
                if (pc.GetInt(obj_f.speed_run) != 3)
                {
                    pc.SetInt(obj_f.speed_run, 3);
                    if (GetGlobalFlag(403))
                    {
                        AttachParticles("Orb-Summon-Air-Elemental", pc);
                    }

                }

            }

        }

        if (ec == 3)
        {
            foreach (var pc in PartyLeader.GetPartyMembers())
            {
                if (pc.GetInt(obj_f.speed_run) != 4)
                {
                    pc.SetInt(obj_f.speed_run, 4);
                    if (GetGlobalFlag(403))
                    {
                        AttachParticles("Orb-Summon-Air-Elemental", pc);
                    }

                }

            }

        }

        if (ec == 4)
        {
            foreach (var pc in PartyLeader.GetPartyMembers())
            {
                if (pc.GetInt(obj_f.speed_run) != 5)
                {
                    pc.SetInt(obj_f.speed_run, 5);
                    if (GetGlobalFlag(403))
                    {
                        AttachParticles("Orb-Summon-Air-Elemental", pc);
                    }

                }

            }

        }

        if (ec == 5)
        {
            // Blazingly fast
            foreach (var pc in PartyLeader.GetPartyMembers())
            {
                if (pc.GetInt(obj_f.speed_run) != 6)
                {
                    pc.SetInt(obj_f.speed_run, 6);
                    if (GetGlobalFlag(403))
                    {
                        AttachParticles("Orb-Summon-Air-Elemental", pc);
                    }

                }

            }

        }

        if (ec == 6)
        {
            // Can you say speeding bullet?
            foreach (var pc in PartyLeader.GetPartyMembers())
            {
                if (pc.GetInt(obj_f.speed_run) != 10)
                {
                    pc.SetInt(obj_f.speed_run, 10);
                    if (GetGlobalFlag(403))
                    {
                        AttachParticles("Orb-Summon-Air-Elemental", pc);
                    }

                }

            }

        }

        return;
    }

}
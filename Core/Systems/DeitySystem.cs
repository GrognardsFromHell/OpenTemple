using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.IO;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Systems.D20.Classes;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.TigSubsystems;

namespace OpenTemple.Core.Systems;

public class DeitySystem : IGameSystem
{
    private readonly Dictionary<DeityId, string> _deityNames;

    private readonly Dictionary<DeityId, string> _deityPraise;

    [TempleDllLocation(0x1004a760)]
    public DeitySystem()
    {
        var deityMes = Tig.FS.ReadMesFile("mes/deity.mes");

        _deityNames = new Dictionary<DeityId, string>(DeityIds.Deities.Length);
        _deityPraise = new Dictionary<DeityId, string>(DeityIds.Deities.Length);
        foreach (var deityId in DeityIds.Deities)
        {
            _deityNames[deityId] = deityMes[(int) deityId];
            _deityPraise[deityId] = deityMes[3000 + (int) deityId];
        }

        Stub.TODO(); // Missing condition naming
    }

    public IEnumerable<DeityId> PlayerSelectableDeities => Deities
        .Where(deity => deity.IsSelectable)
        .Select(deity => deity.Id);

    public void Dispose()
    {
    }

    [TempleDllLocation(0x1004a820)]
    public static bool GetDeityFromEnglishName(string name, out DeityId deityId)
    {
        foreach (var deity in Deities)
        {
            if (deity.EnglishName.Equals(name, StringComparison.InvariantCultureIgnoreCase))
            {
                deityId = deity.Id;
                return true;
            }
        }

        deityId = default;
        return false;
    }

    [TempleDllLocation(0x1004c0d0)]
    public static bool IsDomainSkill(GameObject obj, SkillId skillId)
    {
        var domain1 = (DomainId) obj.GetStat(Stat.domain_1);
        if (IsDomainSkill(domain1, skillId))
        {
            return true;
        }

        var domain2 = (DomainId) obj.GetStat(Stat.domain_2);
        return IsDomainSkill(domain2, skillId);
    }

    [TempleDllLocation(0x1004aba0)]
    private static bool IsDomainSkill(DomainId domain, SkillId skillId)
    {
        switch (domain)
        {
            case DomainId.Trickery:
                return skillId == SkillId.bluff || skillId == SkillId.disguise || skillId == SkillId.hide;
            case DomainId.Knowledge:
                return skillId == SkillId.knowledge_all || skillId == SkillId.knowledge_arcana ||
                       skillId == SkillId.knowledge_religion || skillId == SkillId.knowledge_nature;
            case DomainId.Travel:
                return skillId == SkillId.wilderness_lore;
            case DomainId.Animal:
            case DomainId.Plant:
                return skillId == SkillId.knowledge_nature;
            default:
                return false;
        }
    }

    [TempleDllLocation(0x1004a890)]
    public string GetName(DeityId deity)
    {
        return _deityNames[deity];
    }

    [TempleDllLocation(0x102FDA68)]
    public string GetHelpTopic(DeityId deity)
    {
        return DeitiesById[deity].HelpTopic ?? "TAG_RELIGION";
    }

    public string GetPrayerHeardMessage(DeityId deity)
    {
        var lineId = 910 + (int) deity;
        return GameSystems.Skill.GetSkillUiMessage(lineId);
    }

    [TempleDllLocation(0x1004ab00)]
    public void DeityPraiseFloatMessage(GameObject obj)
    {
        var deity = (DeityId) obj.GetStat(Stat.deity);
        if (_deityPraise.TryGetValue(deity, out var line))
        {
            var color = TextFloaterColor.Red;
            if (obj.IsPC())
            {
                color = TextFloaterColor.White;
            }
            else if (obj.IsNPC())
            {
                var leader = GameSystems.Critter.GetLeaderRecursive(obj);
                if (GameSystems.Party.IsInParty(leader))
                {
                    color = TextFloaterColor.Yellow;
                }
            }

            GameSystems.TextFloater.FloatLine(obj, TextFloaterCategory.Generic, color, line);
        }
    }

    // only good deities and St. Cuthbert (any suggestions?)
    private static readonly ISet<DeityId> GoodDeities = new HashSet<DeityId>
    {
        DeityId.CORELLON_LARETHIAN,
        DeityId.EHLONNA,
        DeityId.GARL_GLITTERGOLD,
        DeityId.HEIRONEOUS,
        DeityId.KORD,
        DeityId.MORADIN,
        DeityId.PELOR,
        DeityId.ST_CUTHBERT,
        DeityId.YONDALLA
    };

    public bool AllowsAtonement(DeityId casterDeity)
    {
        return GoodDeities.Contains(casterDeity);
    }

    [TempleDllLocation(0x1004a940)]
    public bool CanSelect(GameObject playerObj, DeityId deityId)
    {
        var deity = DeitiesById[deityId];
        if (!deity.IsSelectable)
        {
            return false;
        }

        var race = playerObj.GetRace();
        if (D20RaceSystem.UseBaseRaceForDeity(race))
        {
            race = D20RaceSystem.GetBaseRace(race);
        }

        var alignment = playerObj.GetAlignment();
        var classEnum = (Stat) playerObj.GetInt32(obj_f.critter_level_idx, 0);
        var deityClass = D20ClassSystem.GetDeityClass(classEnum);

        var isCleric = deityClass == Stat.level_cleric;
        if (isCleric)
        {
            if (deityId == DeityId.NONE)
            {
                // Clerics MUST have a deity.
                return false;
            }
        }

        var hasRace = deity.FavoredRaces.Contains(race);
        if (hasRace && !isCleric) // can't have Clerics casting spells of opposed alignments
            return true;

        // doesn't have race automatic, so check the supported calsses
        var hasClass = deity.FavoredClasses.Count == 0 || deity.FavoredClasses.Contains(deityClass);

        if (!hasClass && !isCleric)
        {
            return false;
        }

        // special casing - probably buggy but that's how it was in the original
        if (deity.FavoredRaces.Count > 0)
        {
            var isException = false;
            if (deityId == DeityId.CORELLON_LARETHIAN)
            {
                // Corellon Larethian for Bards
                isException = deityClass == Stat.level_bard;
            }
            else if (deityId == DeityId.EHLONNA)
            {
                // Ehlonna
                isException = deityClass == Stat.level_cleric;
            }

            if (!isException && !hasRace)
                return false;
        }

        // check alignment
        if (deityId == DeityId.ST_CUTHBERT) // St. Cuthbert special case
            return (alignment == Alignment.LAWFUL_GOOD || alignment == Alignment.LAWFUL);

        var deityAlignment = deity.Alignment;
        if (deityAlignment == Alignment.NEUTRAL && !isCleric)
            return true;

        if (alignment == deityAlignment)
        {
            return true;
        }

        return GameSystems.Stat.AlignmentsUnopposed(alignment, deityAlignment, true);
    }

    private static readonly DeitySpec[] Deities =
    {
        new DeitySpec(DeityId.NONE)
        {
            Alignment = Alignment.TRUE_NEUTRAL,
            EnglishName = "No Deity",
            IsSelectable = true,
            FavoredWeapon = WeaponType.bastard_sword // easter egg :)
        },
        new DeitySpec(DeityId.BOCCOB)
        {
            Alignment = Alignment.TRUE_NEUTRAL,
            EnglishName = "Boccob",
            HelpTopic = "TAG_BOCCOB",
            Domains = {DomainId.Knowledge, DomainId.Magic, DomainId.Trickery},
            FavoredClasses = {Stat.level_sorcerer, Stat.level_wizard},
            IsSelectable = true,
            FavoredWeapon = WeaponType.quarterstaff
        },
        new DeitySpec(DeityId.CORELLON_LARETHIAN)
        {
            Alignment = Alignment.CHAOTIC_GOOD,
            EnglishName = "Corellon Larethian",
            HelpTopic = "TAG_CORELLON",
            Domains = {DomainId.Chaos, DomainId.Good, DomainId.Protection, DomainId.War},
            FavoredRaces = {RaceId.elf, RaceId.half_elf},
            IsSelectable = true,
            FavoredWeapon = WeaponType.longsword
        },
        new DeitySpec(DeityId.EHLONNA)
        {
            Alignment = Alignment.NEUTRAL_GOOD,
            EnglishName = "Ehlonna",
            HelpTopic = "TAG_EHLONNA",
            Domains = {DomainId.Animal, DomainId.Good, DomainId.Plant, DomainId.Sun},
            FavoredClasses = {Stat.level_druid, Stat.level_ranger},
            FavoredRaces = {RaceId.elf, RaceId.gnome, RaceId.half_elf, RaceId.halfling},
            IsSelectable = true,
            FavoredWeapon = WeaponType.longbow
        },
        new DeitySpec(DeityId.ERYTHNUL)
        {
            Alignment = Alignment.CHAOTIC_EVIL,
            EnglishName = "Erythnul",
            HelpTopic = "TAG_ERYTHNUL",
            Domains = {DomainId.Chaos, DomainId.Evil, DomainId.Trickery, DomainId.War},
            FavoredClasses = {Stat.level_barbarian, Stat.level_bard, Stat.level_fighter, Stat.level_rogue},
            IsSelectable = true,
            FavoredWeapon = WeaponType.morningstar
        },
        new DeitySpec(DeityId.FHARLANGHN)
        {
            Alignment = Alignment.TRUE_NEUTRAL,
            EnglishName = "Fharlanghn",
            HelpTopic = "TAG_FHARLANGHN",
            Domains = {DomainId.Luck, DomainId.Protection, DomainId.Travel},
            FavoredClasses =
            {
                Stat.level_barbarian, Stat.level_bard, Stat.level_druid, Stat.level_fighter, Stat.level_monk,
                Stat.level_ranger, Stat.level_rogue, Stat.level_sorcerer, Stat.level_wizard
            },
            IsSelectable = true,
            FavoredWeapon = WeaponType.quarterstaff
        },
        new DeitySpec(DeityId.GARL_GLITTERGOLD)
        {
            Alignment = Alignment.NEUTRAL_GOOD,
            EnglishName = "Garl Glittergold",
            HelpTopic = "TAG_GARL",
            Domains = {DomainId.Good, DomainId.Protection, DomainId.Trickery},
            FavoredRaces = {RaceId.gnome},
            IsSelectable = true,
            FavoredWeapon = WeaponType.battleaxe
        },
        new DeitySpec(DeityId.GRUUMSH)
        {
            Alignment = Alignment.CHAOTIC_EVIL,
            EnglishName = "Gruumsh",
            HelpTopic = "TAG_GRUUMSH",
            Domains = {DomainId.Chaos, DomainId.Evil, DomainId.Strength, DomainId.War},
            FavoredRaces = {RaceId.half_orc},
            IsSelectable = true,
            FavoredWeapon = WeaponType.longspear
        },
        new DeitySpec(DeityId.HEIRONEOUS)
        {
            Alignment = Alignment.LAWFUL_GOOD,
            EnglishName = "Heironeous",
            HelpTopic = "TAG_HEIRONEOUS",
            Domains = {DomainId.Good, DomainId.Law, DomainId.War},
            FavoredClasses = {Stat.level_paladin, Stat.level_fighter, Stat.level_monk},
            IsSelectable = true,
            FavoredWeapon = WeaponType.longsword
        },
        new DeitySpec(DeityId.HEXTOR)
        {
            Alignment = Alignment.LAWFUL_EVIL,
            EnglishName = "Hextor",
            HelpTopic = "TAG_HEXTOR",
            Domains = {DomainId.Destruction, DomainId.Evil, DomainId.Law, DomainId.War},
            FavoredClasses = {Stat.level_fighter, Stat.level_monk},
            IsSelectable = true,
            FavoredWeapon = WeaponType.heavy_flail
        },
        new DeitySpec(DeityId.KORD)
        {
            Alignment = Alignment.CHAOTIC_GOOD,
            EnglishName = "Kord",
            HelpTopic = "TAG_KORD",
            Domains = {DomainId.Chaos, DomainId.Good, DomainId.Luck, DomainId.Strength},
            FavoredClasses = {Stat.level_barbarian, Stat.level_fighter, Stat.level_rogue},
            IsSelectable = true,
            FavoredWeapon = WeaponType.greatsword
        },
        new DeitySpec(DeityId.MORADIN)
        {
            Alignment = Alignment.LAWFUL_GOOD,
            EnglishName = "Moradin",
            HelpTopic = "TAG_MORADIN",
            Domains = {DomainId.Earth, DomainId.Good, DomainId.Law, DomainId.Protection},
            FavoredRaces = {RaceId.dwarf},
            IsSelectable = true,
            FavoredWeapon = WeaponType.warhammer
        },
        new DeitySpec(DeityId.NERULL)
        {
            Alignment = Alignment.NEUTRAL_EVIL,
            EnglishName = "Nerull",
            HelpTopic = "TAG_NERULL",
            Domains = {DomainId.Death, DomainId.Evil, DomainId.Trickery},
            FavoredClasses = {Stat.level_rogue, Stat.level_sorcerer, Stat.level_wizard},
            IsSelectable = true,
            FavoredWeapon = WeaponType.scythe
        },
        new DeitySpec(DeityId.OBAD_HAI)
        {
            Alignment = Alignment.TRUE_NEUTRAL,
            EnglishName = "Obad-Hai",
            HelpTopic = "TAG_OBAD_HAI",
            Domains =
                {DomainId.Air, DomainId.Animal, DomainId.Earth, DomainId.Fire, DomainId.Plant, DomainId.Water},
            FavoredClasses = {Stat.level_barbarian, Stat.level_druid, Stat.level_ranger},
            IsSelectable = true,
            FavoredWeapon = WeaponType.quarterstaff
        },
        new DeitySpec(DeityId.OLIDAMMARA)
        {
            Alignment = Alignment.CHAOTIC_NEUTRAL,
            EnglishName = "Olidammara",
            HelpTopic = "TAG_OLIDAMMARA",
            Domains = {DomainId.Chaos, DomainId.Luck, DomainId.Trickery},
            FavoredClasses = {Stat.level_bard, Stat.level_rogue},
            IsSelectable = true,
            FavoredWeapon = WeaponType.rapier
        },
        new DeitySpec(DeityId.PELOR)
        {
            Alignment = Alignment.NEUTRAL_GOOD,
            EnglishName = "Pelor",
            HelpTopic = "TAG_PELOR",
            Domains = {DomainId.Good, DomainId.Healing, DomainId.Strength, DomainId.Sun},
            FavoredClasses = {Stat.level_bard, Stat.level_paladin, Stat.level_ranger},
            IsSelectable = true,
            FavoredWeapon = WeaponType.heavy_mace
        },
        new DeitySpec(DeityId.ST_CUTHBERT)
        {
            Alignment = Alignment.LAWFUL_NEUTRAL,
            EnglishName = "St. Cuthbert",
            HelpTopic = "TAG_CUTHBERT",
            Domains = {DomainId.Destruction, DomainId.Law, DomainId.Protection, DomainId.Strength},
            FavoredClasses = {Stat.level_fighter, Stat.level_monk},
            IsSelectable = true,
            FavoredWeapon = WeaponType.heavy_mace
        },
        new DeitySpec(DeityId.VECNA)
        {
            Alignment = Alignment.NEUTRAL_EVIL,
            EnglishName = "Vecna",
            HelpTopic = "TAG_VECNA",
            Domains = {DomainId.Evil, DomainId.Knowledge, DomainId.Magic},
            FavoredClasses = {Stat.level_rogue, Stat.level_sorcerer, Stat.level_wizard},
            IsSelectable = true,
            FavoredWeapon = WeaponType.dagger
        },
        new DeitySpec(DeityId.WEE_JAS)
        {
            Alignment = Alignment.LAWFUL_NEUTRAL,
            EnglishName = "Wee Jas",
            HelpTopic = "TAG_WEE_JAS",
            Domains = {DomainId.Death, DomainId.Law, DomainId.Magic},
            FavoredClasses = {Stat.level_sorcerer, Stat.level_wizard},
            IsSelectable = true,
            FavoredWeapon = WeaponType.dagger
        },
        new DeitySpec(DeityId.YONDALLA)
        {
            Alignment = Alignment.LAWFUL_GOOD,
            EnglishName = "Yondalla",
            HelpTopic = "TAG_YONDALLA",
            Domains = {DomainId.Earth, DomainId.Good, DomainId.Law, DomainId.Protection},
            FavoredRaces = {RaceId.halfling},
            IsSelectable = true,
            FavoredWeapon = WeaponType.short_sword
        },
        new DeitySpec(DeityId.OLD_FAITH)
        {
            Alignment = Alignment.TRUE_NEUTRAL,
            EnglishName = "Old Faith",
            Domains = {DomainId.Earth, DomainId.Animal, DomainId.Plant, DomainId.Sun},
            IsSelectable = false,
            // taken to match Shillelagh, also seems appropriate
            FavoredWeapon = WeaponType.quarterstaff
        },
        new DeitySpec(DeityId.ZUGGTMOY)
        {
            Alignment = Alignment.CHAOTIC_EVIL,
            EnglishName = "Zuggtmoy",
            Domains = {DomainId.Plant, DomainId.Fire, DomainId.Earth, DomainId.Air, DomainId.Water},
            IsSelectable = false,
            FavoredWeapon = WeaponType.scythe
        },
        new DeitySpec(DeityId.IUZ)
        {
            Alignment = Alignment.CHAOTIC_EVIL,
            EnglishName = "Iuz",
            Domains = {DomainId.Chaos, DomainId.Evil, DomainId.Trickery},
            IsSelectable = false,
            FavoredWeapon = WeaponType.greatsword
        },
        new DeitySpec(DeityId.LOLTH)
        {
            Alignment = Alignment.CHAOTIC_EVIL,
            EnglishName = "Lolth",
            Domains = {DomainId.Chaos, DomainId.Destruction, DomainId.Evil, DomainId.Trickery},
            FavoredRaces = {RaceId.goblin},
            IsSelectable = false,
            FavoredWeapon = WeaponType.whip
        },
        new DeitySpec(DeityId.PROCAN)
        {
            Alignment = Alignment.TRUE_NEUTRAL,
            EnglishName = "Procan",
            Domains = {DomainId.Water, DomainId.Trickery, DomainId.Air},
            IsSelectable = false,
            FavoredWeapon = WeaponType.trident
        },
        new DeitySpec(DeityId.NOREBO)
        {
            Alignment = Alignment.CHAOTIC_NEUTRAL,
            EnglishName = "Norebo",
            Domains = {DomainId.Luck, DomainId.Trickery, DomainId.Chaos},
            IsSelectable = false,
            // https://en.wikipedia.org/wiki/Norebo
            FavoredWeapon = WeaponType.dagger
        },
        new DeitySpec(DeityId.PYREMIUS)
        {
            Alignment = Alignment.NEUTRAL_EVIL,
            EnglishName = "Pyremius",
            Domains = {DomainId.Fire, DomainId.Evil, DomainId.Trickery},
            IsSelectable = false,
            FavoredWeapon = WeaponType.longsword
        },
        new DeitySpec(DeityId.RALISHAZ)
        {
            Alignment = Alignment.CHAOTIC_NEUTRAL,
            EnglishName = "Ralishaz",
            Domains = {DomainId.Luck, DomainId.Trickery, DomainId.Chaos},
            IsSelectable = false,
            FavoredWeapon = WeaponType.heavy_mace
        },
    };

    private static readonly ImmutableDictionary<DeityId, DeitySpec> DeitiesById = Deities.ToImmutableDictionary(
        deity => deity.Id,
        deity => deity
    );

    [TempleDllLocation(0x1004bed0)]
    public AlignmentChoice GetAlignmentChoice(DeityId deityId, Alignment critterAlignment)
    {
        if (deityId == DeityId.WEE_JAS)
        {
            if (critterAlignment == Alignment.LAWFUL)
            {
                return AlignmentChoice.Negative;
            }
        }
        else if (deityId == DeityId.ST_CUTHBERT || deityId == DeityId.OBAD_HAI)
        {
            return AlignmentChoice.Positive;
        }

        if (critterAlignment.IsGood())
        {
            return AlignmentChoice.Positive;
        }

        if (critterAlignment.IsEvil())
        {
            return AlignmentChoice.Negative;
        }

        var deityAlign = DeitiesById[deityId].Alignment;
        if (deityAlign.IsGood())
        {
            return AlignmentChoice.Positive;
        }

        if (deityAlign.IsEvil())
        {
            return AlignmentChoice.Negative;
        }

        return AlignmentChoice.Undecided;
    }

    public WeaponType GetFavoredWeapon(DeityId deityId)
    {
        return DeitiesById[deityId].FavoredWeapon;
    }

    public IReadOnlyList<DomainId> GetDomains(DeityId deityId)
    {
        return DeitiesById[deityId].Domains;
    }

}

public class DeitySpec
{
    public DeityId Id { get; }

    public DeitySpec(DeityId id)
    {
        Id = id;
    }

    public Alignment Alignment { get; set; }

    /// <summary>
    /// These Deity Names are only used for parsing protos.tab.
    /// </summary>
    [TempleDllLocation(0x102B0868)]
    public string EnglishName { get; set; }

    public string HelpTopic { get; set; }

    public List<DomainId> Domains { get; } = new List<DomainId>();

    public List<Stat> FavoredClasses { get; } = new List<Stat>();

    public List<RaceId> FavoredRaces { get; } = new List<RaceId>();

    public bool IsSelectable { get; set; }

    public WeaponType FavoredWeapon { get; set; }

}
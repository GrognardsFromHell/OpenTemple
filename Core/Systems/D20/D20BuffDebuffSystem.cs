using System;
using System.Collections.Generic;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.GFX;
using OpenTemple.Core.IO;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Utils;

namespace OpenTemple.Core.Systems.D20;

public class D20BuffDebuffSystem : IDisposable
{
    public readonly Dictionary<int, string> HelpTopics;

    public readonly Dictionary<int, string> TexturePaths;

    // Used for pre-loading indicator textures
    private readonly List<ResourceRef<ITexture>> Textures;

    public readonly Dictionary<int, string> _names;

    [TempleDllLocation(0x100f4220)]
    public D20BuffDebuffSystem()
    {
        HelpTopics = new Dictionary<int, string>();
        Textures = new List<ResourceRef<ITexture>>();
        TexturePaths = new Dictionary<int, string>();

        var translations = Tig.FS.ReadMesFile("mes/indicator.mes");

        _names = new Dictionary<int, string>(translations.Count);
        foreach (var buffDebuffType in D20BuffDebuffTypes.Types)
        {
            TexturePaths[buffDebuffType.Id] = buffDebuffType.IconPath;
            Textures.Add(Tig.Textures.Resolve(buffDebuffType.IconPath, false));
            if (buffDebuffType.HelpTag != null)
            {
                HelpTopics[buffDebuffType.Id] = buffDebuffType.HelpTag;
            }

            if (translations.TryGetValue(buffDebuffType.Id, out var text))
            {
                _names[buffDebuffType.Id] = text;
            }
        }
    }

    [TempleDllLocation(0x100f4300)]
    [TempleDllLocation(0x1004f720)]
    public BuffDebuffPacket GetBuffDebuff(GameObject obj)
    {
        var result = new BuffDebuffPacket();

        var dispatcher = obj.GetDispatcher();
        if (dispatcher == null)
        {
            return result;
        }

        var dispIo = new DispIoEffectTooltip();
        dispIo.bdb = result;
        dispatcher.Process(DispatcherType.EffectTooltip, D20DispatcherKey.NONE, dispIo);

        return dispIo.bdb;
    }

    [TempleDllLocation(0x100f45a0)]
    private string GetName(BuffDebuffEntry entry)
    {
        if (entry.SpellEnum != -1)
        {
            return GameSystems.Spell.GetSpellName(entry.SpellEnum);
        }

        return _names.GetValueOrDefault(entry.EffectType, null);
    }

    [TempleDllLocation(0x100f46f0)]
    public string GetTooltip(BuffDebuffEntry entry)
    {
        var name = GetName(entry);
        if (entry.ExtraText != null)
        {
            return name + " " + entry.ExtraText;
        }

        return name;
    }

    [TempleDllLocation(0x100f4500)]
    public string GetHelpTopic(BuffDebuffEntry entry)
    {
        if (entry.SpellEnum != -1)
        {
            return GameSystems.Spell.GetSpellHelpTopic(entry.SpellEnum);
        }

        if (HelpTopics.TryGetValue(entry.EffectType, out var topicTag))
        {
            return topicTag;
        }

        return null;
    }

    [TempleDllLocation(0x100f44a0)]
    public string GetIconPath(BuffDebuffEntry entry)
    {
        if (TexturePaths.TryGetValue(entry.EffectType, out var texturePath))
        {
            return texturePath;
        }

        return null;
    }

    [TempleDllLocation(0x100f42f0)]
    public void Dispose()
    {
        Textures.DisposeAndClear();
    }
}

public enum BuffDebuffType
{
    Buff,
    Debuff,
    Condition
}

public class BuffDebuffPacket
{
    private readonly List<BuffDebuffEntry> _buffs = new();
    private readonly List<BuffDebuffEntry> _debuffs = new();
    private readonly List<BuffDebuffEntry> _conditions = new();

    public void AddEntry(BuffDebuffType type, int effectType, string extraText = null, int spellEnum = -1)
    {
        var entries = GetEntries(type);
        entries.Add(new BuffDebuffEntry(effectType, extraText, spellEnum));
    }

    private static BuffDebuffType GuessTypeFromId(int effectType)
    {
        if (effectType >= 168)
        {
            return BuffDebuffType.Condition;
        }
        else if (effectType >= 91)
        {
            return BuffDebuffType.Debuff;
        }
        else
        {
            return BuffDebuffType.Buff;
        }
    }

    public void AddEntry(int effectType, string extraText = null, int spellEnum = -1)
    {
        AddEntry(GuessTypeFromId(effectType), effectType, extraText, spellEnum);
    }

    [TempleDllLocation(0x100f4420)]
    public int GetCountByType(BuffDebuffType type)
    {
        return GetEntries(type).Count;
    }

    public bool TryGetEntry(BuffDebuffType type, int index, out BuffDebuffEntry entry)
    {
        var entries = GetEntries(type);
        if (index < 0 || index >= entries.Count)
        {
            entry = null;
            return false;
        }

        entry = entries[index];
        return true;
    }

    private List<BuffDebuffEntry> GetEntries(BuffDebuffType type)
    {
        switch (type)
        {
            case BuffDebuffType.Buff:
                return _buffs;
            case BuffDebuffType.Debuff:
                return _debuffs;
            case BuffDebuffType.Condition:
                return _conditions;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}

public class BuffDebuffEntry
{
    public int EffectType { get; }
    public string ExtraText { get; }
    public int SpellEnum { get; }

    public BuffDebuffEntry(int effectType, string extraText, int spellEnum)
    {
        EffectType = effectType;
        ExtraText = extraText;
        SpellEnum = spellEnum;
    }
}

internal static class D20BuffDebuffTypes
{
    internal static readonly Info[] Types =
    {
        new(
            0,
            0,
            "art/interface/player_conditions/buffs/Barbarian Rage.tga",
            "TAG_CLASS_FEATURES_BARBARIAN_RAGE"
        ),
        new(
            1,
            1,
            "art/interface/player_conditions/buffs/Couraged Aura.tga",
            "TAG_CLASS_FEATURES_PALADIN_AURA_OF_COURAGE"
        ),
        new(
            2,
            2,
            "art/interface/player_conditions/buffs/Holding Charge.tga",
            null
        ),
        new(
            3,
            3,
            "art/interface/player_conditions/buffs/Inspire Competence.tga",
            "TAG_CLASS_FEATURES_BARD_INSPIRE_COMPETENCE"
        ),
        new(
            4,
            4,
            "art/interface/player_conditions/buffs/Inspire Courage.tga",
            "TAG_CLASS_FEATURES_BARD_INSPIRE_COURAGE"
        ),
        new(
            5,
            5,
            "art/interface/player_conditions/buffs/Inspire Greatness.tga",
            "TAG_CLASS_FEATURES_BARD_INSPIRE_GREATNESS"
        ),
        new(
            6,
            6,
            "art/interface/player_conditions/buffs/Invisible.tga",
            "TAG_INVISIBLE"
        ),
        new(
            7,
            7,
            "art/interface/player_conditions/buffs/Smiting Evil.tga",
            "TAG_CLASS_FEATURES_PALADIN_SMITE_EVIL"
        ),
        new(
            8,
            8,
            "art/interface/player_conditions/buffs/Spell Aid.tga",
            "TAG_SPELLS_AID"
        ),
        new(
            9,
            9,
            "art/interface/player_conditions/buffs/Spell Barkskin.tga",
            "TAG_SPELLS_BARKSKIN"
        ),
        new(
            10,
            10,
            "art/interface/player_conditions/buffs/Spell Bless.tga",
            "TAG_SPELLS_BLESS"
        ),
        new(
            11,
            11,
            "art/interface/player_conditions/buffs/Spell Blink.tga",
            "TAG_SPELLS_BLINK"
        ),
        new(
            12,
            12,
            "art/interface/player_conditions/buffs/Spell Blur.tga",
            "TAG_SPELLS_BLUR"
        ),
        new(
            13,
            13,
            "art/interface/player_conditions/buffs/Spell Bull Strength.tga",
            "TAG_SPELLS_BULL'S_STRENGTH"
        ),
        new(
            14,
            14,
            "art/interface/player_conditions/buffs/Spell Cats Grace.tga",
            "TAG_SPELLS_CATS_GRACE"
        ),
        new(
            15,
            15,
            "art/interface/player_conditions/buffs/Spell Consecrate.tga",
            null
        ),
        new(
            16,
            16,
            "art/interface/player_conditions/buffs/Spell Darkvision.tga",
            null
        ),
        new(
            17,
            17,
            "art/interface/player_conditions/buffs/Spell Death Knell.tga",
            "TAG_SPELLS_DEATH_KNELL"
        ),
        new(
            18,
            18,
            "art/interface/player_conditions/buffs/Spell Death Ward.tga",
            "TAG_SPELLS_DEATH_WARD"
        ),
        new(
            19,
            19,
            "art/interface/player_conditions/buffs/Spell Delay Poison.tga",
            "TAG_SPELLS_DELAY_POISON"
        ),
        new(
            20,
            20,
            "art/interface/player_conditions/buffs/Spell Detect Chaos.tga",
            "TAG_SPELLS_DETECT_CHAOS"
        ),
        new(
            21,
            21,
            "art/interface/player_conditions/buffs/Spell Detect Evil.tga",
            "TAG_SPELLS_DETECT_EVIL"
        ),
        new(
            22,
            22,
            "art/interface/player_conditions/buffs/Spell Detect Law.tga",
            "TAG_SPELLS_DETECT_LAW"
        ),
        new(
            23,
            23,
            "art/interface/player_conditions/buffs/Spell Detect Magic.tga",
            "TAG_SPELLS_DETECT_MAGIC"
        ),
        new(
            24,
            24,
            "art/interface/player_conditions/buffs/Spell Detect Secret Doors.tga",
            "TAG_SPELLS_DETECT_SECRET_DOORS"
        ),
        new(
            25,
            25,
            "art/interface/player_conditions/buffs/Spell Detect Undead.tga",
            "TAG_SPELLS_DETECT_UNDEAD"
        ),
        new(
            26,
            26,
            "art/interface/player_conditions/buffs/Spell Discern Lies.tga",
            "TAG_SPELLS_DISCERN_LIES"
        ),
        new(
            27,
            27,
            "art/interface/player_conditions/buffs/Spell Displacement.tga",
            "TAG_SPELLS_DISPLACEMENT"
        ),
        new(
            28,
            28,
            "art/interface/player_conditions/buffs/Spell Divine Favor.tga",
            "TAG_SPELLS_DIVINE_FAVOR"
        ),
        new(
            29,
            29,
            "art/interface/player_conditions/buffs/Spell Divine Power.tga",
            "TAG_SPELLS_DIVINE_POWER"
        ),
        new(
            30,
            30,
            "art/interface/player_conditions/buffs/Spell Emotion.tga",
            "TAG_SPELLS_GOOD_HOPE"
        ),
        new(
            31,
            31,
            "art/interface/player_conditions/buffs/Spell Endurance.tga",
            "TAG_SPELLS_BEAR'S_ENDURANCE"
        ),
        new(
            32,
            32,
            "art/interface/player_conditions/buffs/Spell Endure Elements.tga",
            "TAG_SPELLS_ENDURE_ELEMENTS"
        ),
        new(
            33,
            33,
            "art/interface/player_conditions/buffs/Spell Enlarge.tga",
            "TAG_SPELLS_ENLARGE_PERSON"
        ),
        new(
            34,
            34,
            "art/interface/player_conditions/buffs/Spell Entropic Shield.tga",
            "TAG_SPELLS_ENTROPIC_SHIELD"
        ),
        new(
            35,
            35,
            "art/interface/player_conditions/buffs/Spell Expeditious Retreat.tga",
            "TAG_SPELLS_EXPEDITIOUS_RETREAT"
        ),
        new(
            36,
            36,
            "art/interface/player_conditions/buffs/Spell Find Traps.tga",
            "TAG_SPELLS_FIND_TRAPS"
        ),
        new(
            37,
            37,
            "art/interface/player_conditions/buffs/Spell Fire Shield.tga",
            "TAG_SPELLS_FIRE_SHIELD"
        ),
        new(
            38,
            38,
            "art/interface/player_conditions/buffs/Spell Freedom of Movement.tga",
            "TAG_SPELLS_FREEDOM_OF_MOVEMENT"
        ),
        new(
            39,
            39,
            "art/interface/player_conditions/buffs/Spell Gaseous Form.tga",
            "TAG_SPELLS_GASEOUS_FORM"
        ),
        new(
            40,
            40,
            "art/interface/player_conditions/buffs/Spell Greater Magic Fang.tga",
            "TAG_SPELLS_GREATER_MAGIC_FANG"
        ),
        new(
            41,
            41,
            "art/interface/player_conditions/buffs/Spell Greater Magic Weapon.tga",
            "TAG_SPELLS_GREATER_MAGIC_WEAPON"
        ),
        new(
            42,
            42,
            "art/interface/player_conditions/buffs/Spell Guidance.tga",
            "TAG_SPELLS_GUIDANCE"
        ),
        new(
            43,
            43,
            "art/interface/player_conditions/buffs/Spell Haste.tga",
            "TAG_SPELLS_HASTE"
        ),
        new(
            44,
            44,
            "art/interface/player_conditions/buffs/Spell Improved Invisibility.tga",
            "TAG_SPELLS_INVISIBILITY_GREATER"
        ),
        new(
            45,
            45,
            "art/interface/player_conditions/buffs/Spell Invisibility Sphere.tga",
            "TAG_SPELLS_INVISIBILITY_SPHERE"
        ),
        new(
            46,
            46,
            "art/interface/player_conditions/buffs/Spell Invisibility to Animals.tga",
            "TAG_SPELLS_HIDE_FROM_ANIMALS"
        ),
        new(
            47,
            47,
            "art/interface/player_conditions/buffs/Spell Invisibility to Undead.tga",
            "TAG_SPELLS_HIDE_FROM_UNDEAD"
        ),
        new(
            48,
            48,
            "art/interface/player_conditions/buffs/Spell Invisibility.tga",
            "TAG_SPELLS_INVISIBILITY"
        ),
        new(
            49,
            49,
            "art/interface/player_conditions/buffs/Spell Mage Armor.tga",
            "TAG_SPELLS_MAGE_ARMOR"
        ),
        new(
            50,
            50,
            "art/interface/player_conditions/buffs/Spell Magic Fang.tga",
            "TAG_SPELLS_MAGIC_FANG"
        ),
        new(
            51,
            51,
            "art/interface/player_conditions/buffs/Spell Magic Vestment.tga",
            "TAG_SPELLS_MAGIC_VESTMENT"
        ),
        new(
            52,
            52,
            "art/interface/player_conditions/buffs/Spell Magic Weapon.tga",
            "TAG_SPELLS_MAGIC_WEAPON"
        ),
        new(
            53,
            53,
            "art/interface/player_conditions/buffs/Spell Mirror Image.tga",
            "TAG_SPELLS_MIRROR_IMAGE"
        ),
        new(
            54,
            54,
            "art/interface/player_conditions/buffs/Spell Negative Energy Protection.tga",
            "TAG_SPELLS_DEATH_WARD"
        ),
        new(
            55,
            55,
            "art/interface/player_conditions/buffs/Spell Prayer.tga",
            "TAG_SPELLS_PRAYER"
        ),
        new(
            56,
            56,
            "art/interface/player_conditions/buffs/Spell Protection from Alignment.tga",
            null
        ),
        new(
            57,
            57,
            "art/interface/player_conditions/buffs/Spell Protection from Arrows.tga",
            "TAG_SPELLS_PROTECTION_FROM_ARROWS"
        ),
        new(
            58,
            58,
            "art/interface/player_conditions/buffs/Spell Protection From Elements.tga",
            "TAG_SPELLS_PROTECTION_FROM_ENERGY"
        ),
        new(
            59,
            59,
            "art/interface/player_conditions/buffs/Spell Resist Elements.tga",
            "TAG_SPELLS_RESIST_ENERGY"
        ),
        new(
            60,
            60,
            "art/interface/player_conditions/buffs/Spell Resistance.tga",
            "TAG_SPELLS_RESISTANCE"
        ),
        new(
            61,
            61,
            "art/interface/player_conditions/buffs/Spell Righteous Might.tga",
            "TAG_SPELLS_RIGHTEOUS_MIGHT"
        ),
        new(
            62,
            62,
            "art/interface/player_conditions/buffs/Spell Sanctuary.tga",
            "TAG_SPELLS_SANCTUARY"
        ),
        new(
            63,
            63,
            "art/interface/player_conditions/buffs/Spell See Invisibility.tga",
            "TAG_SPELLS_SEE_INVISIBILITY"
        ),
        new(
            64,
            64,
            "art/interface/player_conditions/buffs/Spell Shield of Faith.tga",
            "TAG_SPELLS_SHIELD_OF_FAITH"
        ),
        new(
            65,
            65,
            "art/interface/player_conditions/buffs/Spell Shield.tga",
            "TAG_SPELLS_SHIELD"
        ),
        new(
            66,
            66,
            "art/interface/player_conditions/buffs/Spell Shillelagh.tga",
            "TAG_SPELLS_SHILLELAGH"
        ),
        new(
            67,
            67,
            "art/interface/player_conditions/buffs/Spell Stoneskin.tga",
            "TAG_SPELLS_STONESKIN"
        ),
        new(
            68,
            68,
            "art/interface/player_conditions/buffs/Spell Tree Shape.tga",
            "TAG_SPELLS_TREE_SHAPE"
        ),
        new(
            69,
            69,
            "art/interface/player_conditions/buffs/Spell True Seeing.tga",
            "TAG_SPELLS_TRUE_SEEING"
        ),
        new(
            70,
            70,
            "art/interface/player_conditions/buffs/Spell True Strike.tga",
            "TAG_SPELLS_TRUE_STRIKE"
        ),
        new(
            71,
            71,
            "art/interface/player_conditions/buffs/Spell Vampiric Touch.tga",
            "TAG_SPELLS_VAMPIRIC_TOUCH"
        ),
        new(
            72,
            72,
            "art/interface/player_conditions/buffs/Spell Virtue.tga",
            "TAG_SPELLS_VIRTUE"
        ),
        new(
            73,
            73,
            "art/interface/player_conditions/buffs/Temporary Hit Points.tga",
            null
        ),
        new(
            74,
            74,
            "art/interface/player_conditions/buffs/Elixir of Hiding.tga",
            null
        ),
        new(
            75,
            75,
            "art/interface/player_conditions/buffs/Elixir of Sneaking.tga",
            null
        ),
        new(
            76,
            76,
            "art/interface/player_conditions/buffs/Elixir of Vision.tga",
            null
        ),
        new(
            77,
            77,
            "art/interface/player_conditions/buffs/Elixir of Vision.tga",
            "TAG_RADIAL_MENU_TOTAL_DEFENSE"
        ),
        new(
            78,
            78,
            "art/interface/player_conditions/buffs/Countersong.tga",
            null
        ),
        new(
            79,
            79,
            "art/interface/player_conditions/buffs/spell protective ward.tga",
            null
        ),
        new(
            80,
            80,
            "art/interface/player_conditions/buffs/feat of strength.tga",
            null
        ),
        new(
            81,
            81,
            "art/interface/player_conditions/buffs/spell otilukes resilient sphere.tga",
            null
        ),
        new(
            82,
            82,
            "art/interface/player_conditions/buffs/spell lesser globe of invulnerability.tga",
            null
        ),
        new(
            83,
            83,
            "art/interface/player_conditions/buffs/spell meld into stone.tga",
            null
        ),
        new(
            84,
            84,
            "art/interface/player_conditions/buffs/spell magic circle.tga",
            null
        ),
        new(
            85,
            85,
            "art/interface/player_conditions/buffs/spell detect good.tga",
            null
        ),
        new(
            86,
            86,
            "art/interface/player_conditions/buffs/spell clairvoyance.tga",
            null
        ),
        new(
            87,
            87,
            "art/interface/player_conditions/buffs/spell call lightning.tga",
            null
        ),
        new(
            88,
            88,
            "art/interface/player_conditions/buffs/Spell obscuring mist.tga",
            null
        ),
        new(
            89,
            89,
            "art/interface/player_conditions/buffs/Spell wind wall.tga",
            null
        ),
        new(
            90,
            90,
            "art/interface/player_conditions/buffs/Spell call lightning.tga",
            null
        ),
        new(
            91,
            1000,
            "art/interface/player_conditions/ailments/Disease Incubating.tga",
            null
        ),
        new(
            92,
            1001,
            "art/interface/player_conditions/ailments/Spell Bane.tga",
            "TAG_SPELLS_BANE"
        ),
        new(
            93,
            1002,
            "art/interface/player_conditions/ailments/Spell Blindness.tga",
            "TAG_SPELLS_BLINDNESS_DEAFNESS"
        ),
        new(
            94,
            1003,
            "art/interface/player_conditions/ailments/Spell Calm Emotions.tga",
            "TAG_SPELLS_CALM_EMOTIONS"
        ),
        new(
            95,
            1004,
            "art/interface/player_conditions/ailments/Spell Charm Monster.tga",
            "TAG_CHARMED"
        ),
        new(
            96,
            1005,
            "art/interface/player_conditions/ailments/Spell Charm Person or Animal.tga",
            "TAG_CHARMED"
        ),
        new(
            97,
            1006,
            "art/interface/player_conditions/ailments/Spell Charm Person.tga",
            "TAG_CHARMED"
        ),
        new(
            98,
            1007,
            "art/interface/player_conditions/ailments/Spell Charmed.tga",
            "TAG_CHARMED"
        ),
        new(
            99,
            1008,
            "art/interface/player_conditions/ailments/Spell Cloudkill.tga",
            "TAG_SPELLS_CLOUDKILL"
        ),
        new(
            100,
            1009,
            "art/interface/player_conditions/ailments/Spell Color Spray Blind.tga",
            "TAG_BLINDED"
        ),
        new(
            101,
            1010,
            "art/interface/player_conditions/ailments/Spell Color Spray Stun.tga",
            "TAG_STUNNED"
        ),
        new(
            102,
            1011,
            "art/interface/player_conditions/ailments/Spell Color Spray Unconscious.tga",
            "TAG_UNCONSCIOUS"
        ),
        new(
            103,
            1012,
            "art/interface/player_conditions/ailments/Spell Color Spray.tga",
            "TAG_SPELLS_COLOR_SPRAY"
        ),
        new(
            104,
            1013,
            "art/interface/player_conditions/ailments/Spell Confusion.tga",
            "TAG_SPELLS_CONFUSION"
        ),
        new(
            105,
            1014,
            "art/interface/player_conditions/ailments/Spell Control Plants Entangle.tga",
            "TAG_SPELLS_CONTROL_PLANTS"
        ),
        new(
            106,
            1015,
            "art/interface/player_conditions/ailments/Spell Cursed.tga",
            "TAG_SPELLS_BESTOW_CURSE"
        ),
        new(
            107,
            1016,
            "art/interface/player_conditions/ailments/Spell Daze.tga",
            "TAG_SPELLS_DAZE"
        ),
        new(
            108,
            1017,
            "art/interface/player_conditions/ailments/Spell Desecrate Undead.tga",
            "TAG_SPELLS_DESECRATE"
        ),
        new(
            109,
            1018,
            "art/interface/player_conditions/ailments/Spell Desecrate.tga",
            "TAG_SPELLS_DESECRATE"
        ),
        new(
            110,
            1019,
            "art/interface/player_conditions/ailments/Spell Disease.tga",
            "TAG_DISEASED"
        ),
        new(
            111,
            1020,
            "art/interface/player_conditions/ailments/Spell Doom.tga",
            "TAG_SPELLS_DOOM"
        ),
        new(
            112,
            1021,
            "art/interface/player_conditions/ailments/Spell Emotion Despair.tga",
            "TAG_SPELLS_CRUSHING_DESPAIR"
        ),
        new(
            113,
            1022,
            "art/interface/player_conditions/ailments/Spell Emotion Friendship.tga",
            null
        ),
        new(
            114,
            1023,
            "art/interface/player_conditions/ailments/Spell Emotion Hate.tga",
            null
        ),
        new(
            115,
            1024,
            "art/interface/player_conditions/ailments/Spell Emotion Rage.tga",
            null
        ),
        new(
            116,
            1025,
            "art/interface/player_conditions/ailments/Spell Fear-Cause Fear.tga",
            "TAG_SPELLS_CAUSE_FEAR"
        ),
        new(
            117,
            1026,
            "art/interface/player_conditions/ailments/Spell Fear.tga",
            "TAG_SPELLS_FEAR"
        ),
        new(
            118,
            1027,
            "art/interface/player_conditions/ailments/Spell Feeblemind.tga",
            "TAG_SPELLS_FEEBLEMIND"
        ),
        new(
            119,
            1028,
            "art/interface/player_conditions/ailments/Spell Fog Cloud.tga",
            "TAG_SPELLS_FOG_CLOUD"
        ),
        new(
            120,
            1029,
            "art/interface/player_conditions/ailments/Spell Ghoul Touch Paralyzed.tga",
            "TAG_PARALYZED"
        ),
        new(
            121,
            1030,
            "art/interface/player_conditions/ailments/Spell Ghoul Touch Stench.tga",
            "TAG_SICKENED"
        ),
        new(
            122,
            1031,
            "art/interface/player_conditions/ailments/Spell Glitterdust Blind.tga",
            "TAG_BLINDED"
        ),
        new(
            123,
            1032,
            "art/interface/player_conditions/ailments/Spell Halt Undead.tga",
            "TAG_SPELLS_HALT_UNDEAD"
        ),
        new(
            124,
            1033,
            "art/interface/player_conditions/ailments/Spell Held.tga",
            "TAG_HELD"
        ),
        new(
            125,
            1034,
            "art/interface/player_conditions/ailments/Spell Hold Monster.tga",
            "TAG_HELD"
        ),
        new(
            126,
            1035,
            "art/interface/player_conditions/ailments/Spell Hold Person.tga",
            "TAG_HELD"
        ),
        new(
            127,
            1036,
            "art/interface/player_conditions/ailments/Spell Holy Smite Blinded.tga",
            "TAG_BLINDED"
        ),
        new(
            128,
            1037,
            "art/interface/player_conditions/ailments/Spell Melfs Acid Arrow.tga",
            "TAG_SPELLS_MELF'S_ACID_ARROW"
        ),
        new(
            129,
            1038,
            "art/interface/player_conditions/ailments/Spell Poisoned.tga",
            "TAG_SPELLS_OBSCURING_MIST"
        ),
        new(
            130,
            1039,
            "art/interface/player_conditions/ailments/Spell Poisoned.tga",
            "TAG_POISON"
        ),
        new(
            131,
            1040,
            "art/interface/player_conditions/ailments/Spell Ray of Enfeeblement.tga",
            "TAG_SPELLS_RAY_OF_ENFEEBLEMENT"
        ),
        new(
            132,
            1041,
            "art/interface/player_conditions/ailments/Spell Reduce.tga",
            "TAG_SPELLS_REDUCE_PERSON"
        ),
        new(
            133,
            1042,
            "art/interface/player_conditions/ailments/Spell Silence.tga",
            "TAG_SPELLS_SILENCE"
        ),
        new(
            134,
            1043,
            "art/interface/player_conditions/ailments/Spell Sleep.tga",
            "TAG_SPELLS_SLEEP"
        ),
        new(
            135,
            1044,
            "art/interface/player_conditions/ailments/Spell Sleeping.tga",
            null
        ),
        new(
            136,
            1045,
            "art/interface/player_conditions/ailments/Spell Sleet Storm.tga",
            "TAG_SPELLS_SLEET_STORM"
        ),
        new(
            137,
            1046,
            "art/interface/player_conditions/ailments/Spell Slow.tga",
            "TAG_SPELLS_SLOW"
        ),
        new(
            138,
            1047,
            "art/interface/player_conditions/ailments/Spell Solid Fog.tga",
            "TAG_SPELLS_SOLID_FOG"
        ),
        new(
            139,
            1048,
            "art/interface/player_conditions/ailments/Spell Spike Growth.tga",
            "TAG_SPELLS_SPIKE_GROWTH"
        ),
        new(
            140,
            1049,
            "art/interface/player_conditions/ailments/Spell Spike Stones.tga",
            "TAG_SPELLS_SPIKE_STONES"
        ),
        new(
            141,
            1050,
            "art/interface/player_conditions/ailments/Spell Stinking Cloud.tga",
            "TAG_SPELLS_STINKING_CLOUD"
        ),
        new(
            142,
            1051,
            "art/interface/player_conditions/ailments/Spell Tashas Hideous.tga",
            "TAG_SPELLS_TASHA'S_HIDEOUS_LAUGHTER"
        ),
        new(
            143,
            1052,
            "art/interface/player_conditions/ailments/Spell Temp Ability Score Loss.tga",
            "TAG_ABILITY_DRAINED"
        ),
        new(
            144,
            1053,
            "art/interface/player_conditions/ailments/Spell Temp Damage Ability Score Loss.tga",
            "TAG_ABILITY_DAMAGED"
        ),
        new(
            145,
            1054,
            "art/interface/player_conditions/ailments/Spell Web.tga",
            "TAG_SPELLS_WEB"
        ),
        new(
            146,
            1055,
            "art/interface/player_conditions/ailments/Barbarian Fatigue.tga",
            "TAG_FATIGUED"
        ),
        new(
            147,
            1056,
            "art/interface/player_conditions/ailments/Level Negative Temp.tga",
            "TAG_SPECIAL_ABILITIES_ENERGY_DRAIN_AND_NEGATIVE_LEVELS"
        ),
        new(
            148,
            1057,
            "art/interface/player_conditions/ailments/Level Negative Perm.tga",
            "TAG_LEVEL_LOSS"
        ),
        new(
            149,
            1058,
            "art/interface/player_conditions/ailments/Suggestion.tga",
            null
        ),
        new(
            150,
            1059,
            "art/interface/player_conditions/ailments/Fascinated.tga",
            null
        ),
        new(
            151,
            1060,
            "art/interface/player_conditions/ailments/Spell Unholy Blight.tga",
            null
        ),
        new(
            152,
            1061,
            "art/interface/player_conditions/ailments/Spell Unholy Blight.tga",
            null
        ),
        new(
            153,
            1062,
            "art/interface/player_conditions/ailments/Spell Suggestion.tga",
            null
        ),
        new(
            154,
            1063,
            "art/interface/player_conditions/ailments/Spell Sound Burst.tga",
            null
        ),
        new(
            155,
            1064,
            "art/interface/player_conditions/ailments/Spell Shout.tga",
            null
        ),
        new(
            156,
            1065,
            "art/interface/player_conditions/ailments/Spell Mind Fog.tga",
            null
        ),
        new(
            157,
            1066,
            "art/interface/player_conditions/ailments/Spell Ice Storm.tga",
            null
        ),
        new(
            158,
            1067,
            "art/interface/player_conditions/ailments/Spell Grease.tga",
            null
        ),
        new(
            159,
            1068,
            "art/interface/player_conditions/ailments/Spell Flare.tga",
            null
        ),
        new(
            160,
            1069,
            "art/interface/player_conditions/ailments/Spell Dominate.tga",
            null
        ),
        new(
            161,
            1070,
            "art/interface/player_conditions/ailments/Spell Dimensional Anchor.tga",
            null
        ),
        new(
            162,
            1071,
            "art/interface/player_conditions/ailments/Deafness.tga",
            null
        ),
        new(
            163,
            1072,
            "art/interface/player_conditions/ailments/Spell Command.tga",
            null
        ),
        new(
            164,
            1073,
            "art/interface/player_conditions/ailments/Spell Heat Metal.tga",
            null
        ),
        new(
            165,
            1074,
            "art/interface/player_conditions/ailments/Spell Chill Metal.tga",
            null
        ),
        new(
            166,
            1075,
            "art/interface/player_conditions/ailments/Spell Chaos Hammer.tga",
            null
        ),
        new(
            167,
            1076,
            "art/interface/player_conditions/ailments/Spell Ghoul Touch Paralyzed.tga",
            "TAG_ABILITY_DRAINED"
        ),
        new(
            168,
            2000,
            "art/interface/player_conditions/conditions/unconscious.tga",
            "TAG_UNCONSCIOUS"
        ),
        new(
            169,
            2001,
            "art/interface/player_conditions/conditions/dying.tga",
            "TAG_DYING"
        ),
        new(
            170,
            2002,
            "art/interface/player_conditions/conditions/disabled.tga",
            "TAG_DISABLED"
        ),
        new(
            171,
            2003,
            "art/interface/player_conditions/conditions/suprised.tga",
            "TAG_SURPRISE"
        ),
        new(
            172,
            2004,
            "art/interface/player_conditions/conditions/stunned.tga",
            "TAG_STUNNED"
        ),
        new(
            173,
            2005,
            "art/interface/player_conditions/conditions/flat-footed.tga",
            "TAG_FLAT_FOOTED"
        ),
        new(
            174,
            2006,
            "art/interface/player_conditions/conditions/grappled.tga",
            "TAG_GRAPPLING"
        ),
        new(
            175,
            2007,
            "art/interface/player_conditions/conditions/Fallen-Paladin.tga",
            "TAG_EX_PALADIN"
        ),
        new(
            176,
            2008,
            "art/interface/player_conditions/conditions/prone.tga",
            "TAG_PRONE"
        ),
        new(
            177,
            2009,
            "art/interface/player_conditions/conditions/encumbered-medium.tga",
            "TAG_ADVENTURING_ENCUMBRANCE"
        ),
        new(
            178,
            2010,
            "art/interface/player_conditions/conditions/encumbered-heavy.tga",
            "TAG_ADVENTURING_ENCUMBRANCE"
        ),
        new(
            179,
            2011,
            "art/interface/player_conditions/conditions/encumbered-over.tga",
            "TAG_ADVENTURING_ENCUMBRANCE"
        ),
    };

    internal class Info
    {
        public int Id { get; }
        public int TranslationKey { get; }
        public string IconPath { get; }
        public string HelpTag { get; }

        public Info(int id, int translationKey, string iconPath, string helpTag)
        {
            Id = id;
            TranslationKey = translationKey;
            IconPath = iconPath;
            HelpTag = helpTag;
        }
    }
}
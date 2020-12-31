using System.IO;
using OpenTemple.Core.GameObject;
using OpenTemple.Core.IO;
using OpenTemple.Core.Systems.D20;

namespace OpenTemple.Core.Ui.PartyPool
{
    public class PartyPoolPlayer
    {
        public byte[] data;
        public ObjectId objId;
        public string name;
        public bool premade; // Was flag 8
        public string path;
        public int portraitId;
        public Gender gender;
        public Stat primaryClass;
        public RaceId race;
        public Alignment alignment;
        public int hpMax;

        public static PartyPoolPlayer Read(BinaryReader reader)
        {
            var result = new PartyPoolPlayer();
            var flags = reader.ReadInt32();
            result.premade = (flags & 8) == 0;
            var dataSize = reader.ReadInt32();
            result.objId = reader.ReadObjectId();
            result.name = reader.ReadPrefixedString();
            // 6 32-bit integers follow
            result.portraitId = reader.ReadInt32();
            result.gender = reader.ReadInt32() switch
            {
                0 => Gender.Female,
                1 => Gender.Male,
                _ => Gender.Male
            };
            result.primaryClass = reader.ReadInt32() switch
            {
                7 => Stat.level_barbarian,
                8 => Stat.level_bard,
                9 => Stat.level_cleric,
                10 => Stat.level_druid,
                11 => Stat.level_fighter,
                12 => Stat.level_monk,
                13 => Stat.level_paladin,
                14 => Stat.level_ranger,
                15 => Stat.level_rogue,
                16 => Stat.level_sorcerer,
                17 => Stat.level_wizard,
                _ => Stat.level_fighter
            };
            result.race = reader.ReadInt32() switch
            {
                0 => RaceId.human,
                1 => RaceId.dwarf,
                2 => RaceId.elf,
                3 => RaceId.gnome,
                4 => RaceId.halfelf,
                5 => RaceId.half_orc,
                6 => RaceId.halfling,
                _ => RaceId.human
            };
            result.alignment = reader.ReadInt32() switch
            {
                0 => Alignment.TRUE_NEUTRAL,
                1 => Alignment.LAWFUL_NEUTRAL,
                2 => Alignment.CHAOTIC_NEUTRAL,
                4 => Alignment.NEUTRAL_GOOD,
                5 => Alignment.LAWFUL_GOOD,
                6 => Alignment.CHAOTIC_GOOD,
                8 => Alignment.NEUTRAL_EVIL,
                9 => Alignment.LAWFUL_EVIL,
                10 => Alignment.CHAOTIC_EVIL,
                _ => Alignment.TRUE_NEUTRAL
            };
            result.hpMax = reader.ReadInt32();
            result.data = reader.ReadBytes(dataSize);
            return result;
        }
    }
}

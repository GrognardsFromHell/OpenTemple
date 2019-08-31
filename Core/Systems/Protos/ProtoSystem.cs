using System;
using System.Collections.Generic;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.IO;
using SpicyTemple.Core.IO.TabFiles;
using SpicyTemple.Core.Logging;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.TigSubsystems;
using SpicyTemple.Core.Utils;

namespace SpicyTemple.Core.Systems.Protos
{
    public class ProtoSystem : IGameSystem
    {
        private static readonly ILogger Logger = new ConsoleLogger();

        private const string UserProtoDir = "rules/protos/";

        private const string TemplePlusProtoFile = "rules/protos_override.tab";

        private const string VanillaProtoFile = "rules/protos.tab";

        private readonly Dictionary<int, GameObjectBody> _prototypes = new Dictionary<int, GameObjectBody>();

        private readonly struct ProtoIdRange
        {
            public readonly int Start;
            public readonly int End;

            public ProtoIdRange(int start, int end)
            {
                Start = start;
                End = end;
            }
        }

        private static readonly ProtoIdRange[] ProtoIdRanges =
        {
            new ProtoIdRange(0, 999), // portal
            new ProtoIdRange(1000, 1999), // container
            new ProtoIdRange(2000, 2999), // scenery
            new ProtoIdRange(3000, 3999), // projectile
            new ProtoIdRange(4000, 4999), // weapon
            new ProtoIdRange(5000, 5999), // ammo
            new ProtoIdRange(6000, 6999), // armor
            new ProtoIdRange(7000, 7999), // money
            new ProtoIdRange(8000, 8999), // food
            new ProtoIdRange(9000, 9999), // scroll
            new ProtoIdRange(10000, 10999), // key
            new ProtoIdRange(11000, 11999), // written
            new ProtoIdRange(12000, 12999), // generic
            new ProtoIdRange(13000, 13999), // pc
            new ProtoIdRange(14000, 14999), // npc
            new ProtoIdRange(15000, 15999), // trap
            new ProtoIdRange(16000, 16999), // bag
        };

        public ProtoSystem()
        {
            ParsePrototypesFile(VanillaProtoFile);

            if (Tig.FS.FileExists(TemplePlusProtoFile))
            {
                ParsePrototypesFile(TemplePlusProtoFile);
            }

            foreach (var protoFilename in Tig.FS.Search(UserProtoDir + "*.tab"))
            {
                ParsePrototypesFile(UserProtoDir + protoFilename);
            }
        }

        public void Dispose()
        {
            // Remove the prototype objects for each type
            foreach (var obj in _prototypes.Values)
            {
                GameSystems.Object.Remove(obj);
            }

            _prototypes.Clear();
        }

        [TempleDllLocation(0x10039220)]
        private static bool GetObjectTypeFromProtoId(int protoId, out ObjectType type)
        {
            for (var i = 0; i < ProtoIdRanges.Length; i++)
            {
                if (protoId >= ProtoIdRanges[i].Start && protoId <= ProtoIdRanges[i].End)
                {
                    type = (ObjectType) i;
                    return true;
                }
            }

            type = default;
            return false;
        }

        [TempleDllLocation(0x10039120)]
        public static int GetOeNameIdForType(ObjectType type)
        {
            switch (type)
            {
                case ObjectType.portal:
                    return 800;
                case ObjectType.container:
                    return 1200;
                case ObjectType.scenery:
                    return 1600;
                case ObjectType.projectile:
                    return 1980;
                case ObjectType.weapon:
                    return 2000;
                case ObjectType.ammo:
                    return 2400;
                case ObjectType.armor:
                    return 2800;
                case ObjectType.money:
                    return 3200;
                case ObjectType.food:
                    return 3600;
                case ObjectType.scroll:
                    return 4000;
                case ObjectType.key:
                    return 4400;
                case ObjectType.written:
                    return 5200;
                case ObjectType.generic:
                    return 5600;
                case ObjectType.pc:
                    return 6000;
                case ObjectType.npc:
                    return 6400;
                case ObjectType.trap:
                    return 10001;
                case ObjectType.bag:
                    return 10401;
                default:
                    return 0;
            }
        }

        [TempleDllLocation(0x1003b640)]
        private void ParsePrototypesFile(string path)
        {
            void ProcessProtoRecord(TabFileRecord record)
            {
                var protoId = record[0].GetInt();

                if (!GetObjectTypeFromProtoId(protoId, out var type))
                {
                    Logger.Error("Failed to determine object type for proto id {0}", protoId);
                    return;
                }

                if (_prototypes.ContainsKey(protoId))
                {
                    Logger.Debug("{0} overrides prototype {1}", path, protoId);
                    GameSystems.Object.Remove(_prototypes[protoId]);
                    _prototypes.Remove(protoId);
                }

                var obj = GameSystems.Object.CreateProto(type, ObjectId.CreatePrototype((ushort) protoId));
                _prototypes[protoId] = obj;

                obj.SetInt32(obj_f.name, GetOeNameIdForType(type));

                ProtoDefaultValues.SetDefaultValues(protoId, obj);

                ProtoColumns.ParseColumns(protoId, record, obj);

                if (obj.IsNPC())
                {
                    var race = GameSystems.Stat.ObjStatBaseGet(obj, Stat.race);
                    var gender = GameSystems.Stat.ObjStatBaseGet(obj, Stat.gender);
                    obj.SetInt32(obj_f.sound_effect, 10 * (gender + 2 * race + 1));
                }

                GameSystems.Level.NpcAddKnownSpells(obj);
                SetCritterAttacks(obj);
                SetCritterXp(obj);
            }

            TabFile.ParseFile(path, ProcessProtoRecord);
        }

        [TempleDllLocation(0x1003aac0)]
        [TemplePlusLocation("protos.cpp:68")]
        private void SetCritterAttacks(GameObjectBody proto)
        {
            if (proto.IsCritter())
            {
                int strMod = GameSystems.Stat.ObjStatBaseGet(proto, Stat.str_mod);
                int sizeMod = GameSystems.Critter.GetBonusFromSizeCategory((SizeCategory) proto.GetInt32(obj_f.size));
                for (int attackIndex = 0; attackIndex < 3; attackIndex++)
                {
                    if (proto.GetInt32(obj_f.critter_attacks_idx, attackIndex) > 0)
                    {
                        int attackBonusOld = proto.GetInt32(obj_f.attack_bonus_idx, attackIndex);
                        var attackBonusNew = attackBonusOld - (strMod + sizeMod);
                        proto.SetInt32(obj_f.attack_bonus_idx, attackIndex, attackBonusNew);

                        // Decrement dice damage modifier to remove STR bonus, as it will be added later on
                        Dice diceDamage = Dice.Unpack(proto.GetUInt32(obj_f.critter_damage_idx, attackIndex));
                        int newDiceMod = diceDamage.Modifier - strMod / 2;
                        if (attackIndex <= 0 || strMod <= 0)
                            newDiceMod = diceDamage.Modifier - strMod;

                        Dice diceDamageNew = diceDamage.WithModifier(newDiceMod);
                        proto.SetInt32(obj_f.critter_damage_idx, attackIndex, diceDamageNew.ToPacked());
                    }
                }

                // Last Natural Attack (3) is dex based, therefore dex and size is removed from attack bonus
                if (proto.GetInt32(obj_f.critter_attacks_idx, 3) > 0)
                {
                    var dexMod = GameSystems.Stat.ObjStatBaseGet(proto, Stat.dex_mod);
                    var attackBonusOld = proto.GetInt32(obj_f.attack_bonus_idx, 3);

                    proto.SetInt32(obj_f.attack_bonus_idx, 3, attackBonusOld - dexMod - sizeMod);
                }
            }
        }

        /// <summary>
        /// Sets the critter experience to what would be required to reach their current level, in case it is lower.
        /// </summary>
        [TempleDllLocation(0x1003ac50)]
        private void SetCritterXp(GameObjectBody obj)
        {
            if (obj.IsCritter())
            {
                var currentXp = obj.GetInt32(obj_f.critter_experience);
                var level = obj.GetStat(Stat.level);
                var actuallyNeededXp = GameSystems.Level.GetExperienceForLevel(level);
                if (currentXp < actuallyNeededXp)
                {
                    obj.SetInt32(obj_f.critter_experience, actuallyNeededXp);
                }
            }
        }

        public IEnumerable<GameObjectBody> EnumerateProtos()
        {
            return _prototypes.Values;
        }

        public IEnumerable<GameObjectBody> EnumerateProtos(ObjectType type)
        {
            foreach (var proto in _prototypes.Values)
            {
                if (proto.type == type)
                {
                    yield return proto;
                }
            }
        }

        [TempleDllLocation(0x1003ad70)]
        public GameObjectBody GetProtoById(ushort protoId)
        {
            var id = ObjectId.CreatePrototype(protoId);
            return GameSystems.Object.GetObject(id);
        }
    }
}
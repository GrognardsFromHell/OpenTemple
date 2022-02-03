using System.Collections.Generic;
using System.Diagnostics;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.Logging;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Systems.GameObjects;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Utils;

namespace OpenTemple.Core.Systems.Protos
{
    public class ProtoSystem : IGameSystem
    {
        private static readonly ILogger Logger = LoggingSystem.CreateLogger();

        private readonly ObjectSystem _objectSystem;

        private readonly Dictionary<int, GameObject> _prototypes = new Dictionary<int, GameObject>();

        public ProtoSystem(ObjectSystem objectSystem)
        {
            _objectSystem = objectSystem;

            foreach (var protoFilename in ProtoFileParser.EnumerateProtoFiles(Tig.FS))
            {
                ParsePrototypesFile(protoFilename);
            }
        }

        public void Dispose()
        {
            // Remove the prototype objects for each type
            foreach (var obj in _prototypes.Values)
            {
                _objectSystem.Remove(obj);
            }

            _prototypes.Clear();
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
            var protos = ProtoFileParser.Parse(path, (obj, protoId) =>
            {
                ObjectDefaultProperties.SetDefaultProperties(obj);
                obj.SetInt32(obj_f.name, GetOeNameIdForType(obj.type));
                ProtoDefaultValues.SetDefaultValues(protoId, obj);
            });

            foreach (var obj in protos)
            {
                var protoId = obj.id.protoId;

                if (obj.IsNPC())
                {
                    var race = GameSystems.Stat.ObjStatBaseGet(obj, Stat.race);
                    var gender = GameSystems.Stat.ObjStatBaseGet(obj, Stat.gender);
                    obj.SetInt32(obj_f.sound_effect, 10 * (gender + 2 * race + 1));
                }

                GameSystems.Level.NpcAddKnownSpells(obj);
                SetCritterAttacks(obj);
                SetCritterXp(obj);

                if (_prototypes.ContainsKey(protoId))
                {
                    Logger.Debug("{0} overrides prototype {1}", path, protoId);
                    _objectSystem.Remove(_prototypes[protoId]);
                    _prototypes.Remove(protoId);
                }

                _objectSystem.Add(obj);
                _prototypes[protoId] = obj;
            }
        }

        [TempleDllLocation(0x1003aac0)]
        [TemplePlusLocation("protos.cpp:68")]
        private void SetCritterAttacks(GameObject proto)
        {
            if (proto.IsCritter())
            {
                var strMod = GameSystems.Stat.ObjStatBaseGet(proto, Stat.str_mod);
                var sizeMod = GameSystems.Critter.GetBonusFromSizeCategory((SizeCategory) proto.GetInt32(obj_f.size));
                for (var attackIndex = 0; attackIndex < 3; attackIndex++)
                {
                    if (proto.GetInt32(obj_f.critter_attacks_idx, attackIndex) > 0)
                    {
                        var attackBonusOld = proto.GetInt32(obj_f.attack_bonus_idx, attackIndex);
                        var attackBonusNew = attackBonusOld - (strMod + sizeMod);
                        proto.SetInt32(obj_f.attack_bonus_idx, attackIndex, attackBonusNew);

                        // Decrement dice damage modifier to remove STR bonus, as it will be added later on
                        var diceDamage = Dice.Unpack(proto.GetUInt32(obj_f.critter_damage_idx, attackIndex));
                        var newDiceMod = diceDamage.Modifier - strMod / 2;
                        if (attackIndex <= 0 || strMod <= 0)
                        {
                            newDiceMod = diceDamage.Modifier - strMod;
                        }

                        var diceDamageNew = diceDamage.WithModifier(newDiceMod);
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
        ///     Sets the critter experience to what would be required to reach their current level, in case it is lower.
        /// </summary>
        [TempleDllLocation(0x1003ac50)]
        private void SetCritterXp(GameObject obj)
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

        public IEnumerable<GameObject> EnumerateProtos()
        {
            return _prototypes.Values;
        }

        public IEnumerable<GameObject> EnumerateProtos(ObjectType type)
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
        public GameObject GetProtoById(int protoId)
        {
            Trace.Assert(protoId <= ushort.MaxValue);
            var id = ObjectId.CreatePrototype((ushort) protoId);
            return _objectSystem.GetObject(id);
        }
    }
}
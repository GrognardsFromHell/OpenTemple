using System.Collections.Generic;
using System.Collections.Immutable;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.Logging;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Systems.MapSector;
using OpenTemple.Core.TigSubsystems;

namespace OpenTemple.Core.Systems;

public class SoundMapSystem : IGameSystem
{
    private static readonly ILogger Logger = LoggingSystem.CreateLogger();

    public void Dispose()
    {
    }

    [TempleDllLocation(0x1006df90)]
    public int GetPortalSoundEffect(GameObject portal, PortalSoundEffect type)
    {
        if (portal == null || portal.type != ObjectType.portal)
        {
            return -1;
        }

        return portal.GetInt32(obj_f.sound_effect) + (int) type;
    }

    private const int ItemGetBase = 5950;
    private const int ItemDropBase = 5960;

    private const int OffsetItemUse = 0;
    private const int OffsetItemHit1 = 1;
    private const int OffsetItemHit2 = 2;
    private const int OffsetItemHit3 = 3;
    private const int OffsetItemHitCritical = 4;
    private const int OffsetItemAttacking = 5;
    private const int OffsetItemMiss = 6;
    private const int OffsetItemOutOfAmmo = 7;

    // see snd_item.mes
    private const int ItemMatFlesh = 0;
    private const int ItemMatMetalLight = 1;
    private const int ItemMatMetal = 2;
    private const int ItemMatStone = 3;
    private const int ItemMatWood = 4;
    private const int ItemMatGlass = 5;
    private const int ItemMatCloth = 6;
    private const int ItemMatPaper = 7;
    private const int ItemMatCoin = 8;

    // Offsets for ITEM_GET_BASE or ITEM_DROP_BASE based on material
    [TempleDllLocation(0x102bea18)] private static readonly ImmutableDictionary<Material, int> MaterialSoundOffsets =
        new Dictionary<Material, int>
            {
                {Material.stone, ItemMatStone},
                {Material.brick, ItemMatStone},
                {Material.wood, ItemMatWood},
                {Material.plant, ItemMatWood},
                {Material.flesh, ItemMatFlesh},
                {Material.metal, ItemMatMetal},
                {Material.glass, ItemMatGlass},
                {Material.cloth, ItemMatCloth},
                {Material.liquid, ItemMatFlesh},
                {Material.paper, ItemMatPaper},
                {Material.gas, ItemMatFlesh},
                {Material.force, ItemMatMetalLight},
                {Material.fire, ItemMatFlesh},
                {Material.powder, ItemMatFlesh},
            }
            .ToImmutableDictionary();

    [TempleDllLocation(0x1006e0b0)]
    public int GetSoundIdForItemEvent(GameObject item, GameObject wielder, GameObject target, ItemSoundEffect eventType)
    {
        var itemSoundId = item?.GetInt32(obj_f.sound_effect) ?? 0;

        switch (eventType)
        {
            case ItemSoundEffect.PickUp:
                return GetItemMaterialDependentSound(item, ItemGetBase);
            case ItemSoundEffect.Drop:
                return GetItemMaterialDependentSound(item, ItemDropBase);
            case ItemSoundEffect.Use:
                if (itemSoundId == 0)
                {
                    return -1;
                }

                return itemSoundId + OffsetItemUse;
            case ItemSoundEffect.OutOfAmmo:
                if (itemSoundId == 0)
                {
                    return -1;
                }

                return itemSoundId + OffsetItemOutOfAmmo;
            case ItemSoundEffect.Attacking:
                if (itemSoundId == 0)
                {
                    return -1;
                }

                return itemSoundId + OffsetItemAttacking;
            case ItemSoundEffect.Hit:
            case ItemSoundEffect.CriticalHit:
            {
                if (target == null)
                {
                    return -1;
                }

                if (itemSoundId != 0)
                {
                    int soundId;
                    if (eventType == ItemSoundEffect.CriticalHit)
                    {
                        soundId = itemSoundId + OffsetItemHitCritical;
                    }
                    else
                    {
                        soundId = itemSoundId + (GameSystems.Random.GetBool() ? OffsetItemHit1 : OffsetItemHit2);
                    }

                    if (GameSystems.SoundGame.IsValidSoundId(soundId))
                    {
                        return soundId;
                    }
                }

                if (target.IsCritter())
                {
                    var armor = GameSystems.Item.ItemWornAt(target, EquipSlot.Armor);
                    if (armor != null)
                    {
                        target = armor;
                    }
                }

                var material = target.GetMaterial();
                var weaponType = item != null ? item.GetWeaponType() : wielder.GetUnarmedStrikeWeaponType();
                var hitSound = eventType == ItemSoundEffect.CriticalHit ? 0 : GameSystems.Random.GetInt(1, 3);
                return EncodeWeaponSound(weaponType, material, hitSound);
            }
            case ItemSoundEffect.Miss:
            {
                if (itemSoundId != 0 && GameSystems.SoundGame.IsValidSoundId(itemSoundId + OffsetItemMiss))
                {
                    return itemSoundId + OffsetItemMiss;
                }

                var weaponType = item != null ? item.GetWeaponType() : wielder.GetUnarmedStrikeWeaponType();
                return EncodeWeaponSound(weaponType, default, 4);
            }
            default:
                return -1;
        }
    }

    private static int EncodeWeaponSound(WeaponType weaponType, Material materialHit, int soundType)
    {
        var result = unchecked((int) 0xC0000000);
        result |= ((int) materialHit & 0x1F) << 10;
        result |= ((int) weaponType & 0x7F) << 3;
        result |= soundType & 0x7;
        return result;
    }

    private static void DecodeWeaponSound(int soundId, out WeaponType weaponType, out Material materialHit,
        out int soundType)
    {
        materialHit = (Material) ((soundId >> 10) & 0x1F);
        weaponType = (WeaponType) ((soundId >> 3) & 0x7F);
        soundType = soundId & 7;
    }

    public bool IsEncodedWeaponSound(int soundId)
    {
        return (soundId & 0xC0000000) == 0xC0000000;
    }

    private static int GetItemMaterialDependentSound(GameObject item, int baseId)
    {
        if (item == null)
        {
            return -1;
        }

        if (item.type == ObjectType.money)
        {
            return baseId + ItemMatCoin;
        }

        var materialOffset = MaterialSoundOffsets[item.GetMaterial()];
        if (materialOffset == ItemMatMetal && GameSystems.Item.GetItemWeight(item) <= 2000)
        {
            materialOffset = ItemMatMetalLight;
        }

        return baseId + materialOffset;
    }

    [TempleDllLocation(0x1006dfd0)]
    public int GetAnimateForeverSoundEffect(GameObject obj, int subtype)
    {
        if ((obj.type.IsCritter() || obj.type == ObjectType.container || obj.type == ObjectType.portal ||
             obj.type.IsEquipment()) && obj.type != ObjectType.weapon)
        {
            return -1;
        }

        var soundId = obj.GetInt32(obj_f.sound_effect);
        if (soundId == 0)
        {
            return -1;
        }

        switch (subtype)
        {
            case 0 when obj.type != ObjectType.weapon:
                return GameSystems.SoundGame.IsValidSoundId(soundId) ? soundId : -1;
            case 1 when obj.type != ObjectType.weapon:
                soundId++;
                return GameSystems.SoundGame.IsValidSoundId(soundId) ? soundId : -1;
            case 2:
                return soundId;
            default:
                return -1;
        }
    }

    private static readonly Dictionary<TileMaterial, int> FootstepBaseSound = new()
    {
        {TileMaterial.Dirt, 2904},
        {TileMaterial.Grass, 2912},
        {TileMaterial.Water, 2928},
        {TileMaterial.Ice, 2916},
        {TileMaterial.Wood, 2932},
        {TileMaterial.Stone, 2920},
        {TileMaterial.Metal, 2920},
    };

    [TempleDllLocation(0x1006def0)]
    public int GetCritterSoundEffect(GameObject obj, CritterSoundEffect type)
    {
        if (!obj.IsCritter())
        {
            return -1;
        }

        var partyLeader = GameSystems.Party.GetLeader();
        if (partyLeader == null || partyLeader.HasFlag(ObjectFlag.OFF))
        {
            return -1;
        }

        if (type == CritterSoundEffect.Footsteps)
        {
            var critterPos = obj.GetLocation();
            var groundMaterial = GameSystems.Tile.GetMaterial(critterPos);
            if (FootstepBaseSound.TryGetValue(groundMaterial, out var baseId))
            {
                return baseId + GameSystems.Random.GetInt(0, 3);
            }

            return -1;
        }
        else
        {
            var soundEffect = obj.GetInt32(obj_f.sound_effect);
            return soundEffect + (int) type;
        }
    }

    [TempleDllLocation(0x1006e440)]
    public string GetWeaponHitSoundPath(int soundId)
    {
        DecodeWeaponSound(soundId, out var weaponType, out var materialHit, out var soundType);

        var weaponTypeName = WeaponTypes.TypeToId[weaponType];
        var materialHitName = Materials.MaterialToId[materialHit];
        while (true)
        {
            var filename = soundType switch
            {
                0 => $"sound/dynamic/hitsounds/WT_{weaponTypeName}-{materialHitName}-critical.wav",
                4 => $"sound/dynamic/hitsounds/WT_{weaponTypeName}-miss.wav",
                _ => $"sound/dynamic/hitsounds/WT_{weaponTypeName}-{materialHitName}-{soundType}.wav"
            };

            if (Tig.FS.FileExists(filename))
            {
                return filename;
            }

            // Attempt other hit types before falling back to other weapon types
            if (soundType == 2 || soundType == 3)
            {
                --soundType;
            }
            // Fall back to another weapon type
            else if (!WeaponTypes.FallbackSound.TryGetValue(weaponType, out weaponType))
            {
                Logger.Info("Missing sound (no fallback weapon type): {0}", filename);
                return "sound/soundmissing.wav";
            }
        }
    }
}

public enum ItemSoundEffect
{
    PickUp = 0,
    Drop,
    Use,
    OutOfAmmo,
    Attacking,
    Hit,
    Miss,
    CriticalHit
}

public enum PortalSoundEffect
{
    Open = 0,
    Close,
    Locked = 2
}

public enum CritterSoundEffect
{
    Attack = 0,
    Death = 1,
    Fidget1 = 2,
    Fidget2 = 2,
    /// <summary>
    /// None of the critters seems to have this flag.
    /// </summary>
    Alerted = 5,
    Footsteps = 7
}
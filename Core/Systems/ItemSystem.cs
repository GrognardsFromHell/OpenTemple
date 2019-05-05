using System;
using System.Collections.Immutable;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.GFX;
using SpicyTemple.Core.Logging;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.Systems.Feats;
using SpicyTemple.Core.Systems.GameObjects;

namespace SpicyTemple.Core.Systems
{
    public class ItemSystem : IGameSystem, IBufferResettingSystem
    {
        private static readonly ILogger Logger = new ConsoleLogger();

        private const bool IsEditor = false;

        public void Dispose()
        {
        }

        public ObjHndl GetParent(ObjHndl handle)
        {
            throw new NotImplementedException();
        }

        public GameObjectBody GetParent(GameObjectBody handle)
        {
            throw new NotImplementedException();
        }

        public const int CRITTER_INVENTORY_SLOT_COUNT = 24; // amount of inventory slots visible
        public const int INVENTORY_WORN_IDX_START = 200; // the first inventory index for worn items
        public const int INVENTORY_WORN_IDX_END = 216; // the last index for worn items (non-inclusive, actually)
        public const int INVENTORY_WORN_IDX_COUNT = 16;
        public const int INVENTORY_BAG_IDX_START = 217;
        public const int INVENTORY_BAG_IDX_END = 221;
        public const int INVENTORY_IDX_UNDEFINED = -1;

        public const int
            INVENTORY_IDX_HOTBAR_START =
                2000; // seems to be Arcanum leftover (appears in some IF conditions but associated callbacks are stubs)

        public const int INVENTORY_IDX_HOTBAR_END = 2009; // last index for hotbar items

        private const string sBoneLeftForearm = "Bip01 L Forearm";

        private static readonly string[] sBoneNames =
        {
            "HEAD_REF", // helm
            "CHEST_REF", // necklace
            null, // gloves
            "HANDR_REF", //main hand
            "HANDL_REF", // offhand
            "CHEST_REF", // armor
            "HANDR_REF", // left ring
            "HANDL_REF", // right ring
            null, // arrows
            null, // cloak
            null, // shield
            null, // robe
            null, // bracers
            null, // bardic instrument
            null // misc (thieves' tools, belt etc)
        };

        public int GetInventoryLocationForSlot(EquipSlot slot)
        {
            switch (slot)
            {
                case EquipSlot.Helmet:
                    return 200;
                case EquipSlot.Necklace:
                    return 201;
                case EquipSlot.Gloves:
                    return 202;
                case EquipSlot.WeaponPrimary:
                    return 203;
                case EquipSlot.WeaponSecondary:
                    return 204;
                case EquipSlot.Armor:
                    return 205;
                case EquipSlot.RingPrimary:
                    return 206;
                case EquipSlot.RingSecondary:
                    return 207;
                case EquipSlot.Boots:
                    return 208;
                case EquipSlot.Ammo:
                    return 209;
                case EquipSlot.Cloak:
                    return 210;
                case EquipSlot.Shield:
                    return 211;
                case EquipSlot.Robes:
                    return 212;
                case EquipSlot.Bracers:
                    return 213;
                case EquipSlot.BardicItem:
                    return 214;
                case EquipSlot.Lockpicks:
                    return 215;
                default:
                    throw new ArgumentOutOfRangeException(nameof(slot), slot, null);
            }
        }

        public string GetAttachBone(GameObjectBody obj)
        {
            var wearFlags = obj.GetItemWearFlags();
            var slot = ObjectHandles.GetItemInventoryLocation(obj);

            if (slot < INVENTORY_WORN_IDX_START)
            {
                return null; // Apparently not equipped
            }

            var type = obj.type;
            if (wearFlags.HasFlag(ItemWearFlag.TWOHANDED_REQUIRED) && type == ObjectType.weapon)
            {
                var weaponType = obj.GetWeaponType();
                if (weaponType == WeaponType.shortbow || weaponType == WeaponType.longbow ||
                    weaponType == WeaponType.spike_chain)
                    return sBoneNames[4]; // HANDL_REF
            }
            else if (type == ObjectType.armor)
            {
                return sBoneLeftForearm;
            }

            return sBoneNames[slot - INVENTORY_WORN_IDX_START];
        }

        [TempleDllLocation(0x10065010)]
        public GameObjectBody ItemWornAt(GameObjectBody wearer, EquipSlot slot)
        {
            if (wearer == null)
            {
                return null;
            }

            if (GameSystems.D20.D20Query(wearer, D20DispatcherKey.QUE_Polymorphed) != 0)
            {
                return null;
            }

            var expectedLocation = GetInventoryLocationForSlot(slot);

            foreach (var wornItem in wearer.EnumerateChildren())
            {
                var location = wornItem.GetItemInventoryLocation();
                if (location == expectedLocation)
                {
                    return wornItem;
                }
            }

            return null;
        }

        [TempleDllLocation(0x100651B0)]
        public GameObjectBody GetItemAtInvIdx(GameObjectBody container, int index)
        {
            if (container.IsCritter() && GameSystems.D20.D20Query(container, D20DispatcherKey.QUE_Polymorphed) != 0)
            {
                return null;
            }

            foreach (var childItem in container.EnumerateChildren())
            {
                if (childItem.type.IsEquipment() && childItem.GetItemInventoryLocation() == index)
                {
                    return childItem;
                }
            }

            return null;
        }

        [TempleDllLocation(0x10066730)]
        public WeaponAnimType GetWeaponAnimType(GameObjectBody weapon, GameObjectBody wielder)
        {
            var wieldType = GetWieldType(wielder, weapon);
            IImmutableDictionary<WeaponType, WeaponAnimType> weaponAnimTypes;
            if (wieldType == 2)
            {
                weaponAnimTypes = WeaponAnimMapping.TwoHanded;
            }
            else
            {
                if (wieldType != 1 && wieldType != 0)
                    return WeaponAnimType.Unarmed; // Wield type 3 most likely
                weaponAnimTypes = WeaponAnimMapping.SingleHanded;
            }

            var weaponType = weapon.GetWeaponType();
            return weaponAnimTypes[weaponType];
        }

        /// <summary>
        /// 0 if it's light weapon (smaller than your size)
        /// 1 if it's your size cat (can single wield)
        /// 2 if requires double wield
        /// 3 for special stuff?
        /// </summary>
        [TempleDllLocation(0x10066580)]
        public int GetWieldType(GameObjectBody wearer, GameObjectBody item)
        {
            var wieldType = 3;
            if (!item.id)
                return 4;

            var wearFlags = item.GetItemWearFlags();

            if (item.type == ObjectType.armor)
            {
                var v4 = item.GetArmorFlags();
                if (!v4.HasFlag(ArmorFlag.TYPE_NONE) && (v4 & ArmorFlag.TYPE_SHIELD) == ArmorFlag.TYPE_SHIELD)
                {
                    return wearFlags.HasFlag(ItemWearFlag.BUCKLER) ? 0 : 1;
                }
            }

            var wearerSizeCategory = GameSystems.Stat.DispatchGetSizeCategory(wearer);
            var itemSizeCategory = (SizeCategory) item.GetInt32(obj_f.size);
            if (itemSizeCategory < wearerSizeCategory)
            {
                wieldType = 0;
            }
            else if (itemSizeCategory == wearerSizeCategory)
            {
                wieldType = 1;
            }
            else if (itemSizeCategory == wearerSizeCategory + 1)
            {
                wieldType = 2;
            }

            if (wearFlags.HasFlag(ItemWearFlag.TWOHANDED_REQUIRED))
            {
                wieldType = 2;
            }

            if (item.type != ObjectType.weapon)
            {
                return wieldType;
            }

            var weaponType = item.GetWeaponType();
            if (weaponType == WeaponType.bastard_sword)
            {
                if (!GameSystems.Feat.HasFeat(wearer, FeatId.EXOTIC_WEAPON_PROFICIENCY_BASTARD_SWORD))
                {
                    wieldType = (wearerSizeCategory <= SizeCategory.Small ? 3 : 2);
                }

                return wieldType;
            }

            if (weaponType == WeaponType.dwarven_waraxe)
            {
                if (GameSystems.Feat.HasFeat(wearer, FeatId.EXOTIC_WEAPON_PROFICIENCY_DWARVEN_WARAXE)
                    || GameSystems.Stat.StatLevelGet(wearer, Stat.race) == 1)
                {
                    return wieldType;
                }

                if (wearerSizeCategory < SizeCategory.Medium)
                {
                    return 3;
                }
                else if (wearerSizeCategory == SizeCategory.Medium)
                {
                    return 2;
                }
                else
                {
                    return 1;
                }
            }

            return wieldType;
        }

        public bool IsStackable(GameObjectBody item)
        {
            switch (item.type)
            {
                case ObjectType.weapon:
                    return item.GetWeaponType() == WeaponType.shuriken;
                case ObjectType.food:
                    var flags = item.GetItemFlags();
                    return flags.HasFlag(ItemFlag.IS_MAGICAL) && flags.HasFlag(ItemFlag.EXPIRES_AFTER_USE);

                case ObjectType.generic:
                    return item.GetInt32(obj_f.category) == 5; // jewelry / gems
                case ObjectType.armor:
                    return item.GetInt32(obj_f.category) == 17; // necklaces
                case ObjectType.ammo:
                case ObjectType.money:
                case ObjectType.scroll:
                    return true;
                default:
                    return false;
            }
        }

        [TempleDllLocation(0x100641B0)]
        public bool GetQuantityField(GameObjectBody item, out obj_f field)
        {
            switch (item.type)
            {
                case ObjectType.money:
                    field = obj_f.money_quantity;
                    return true;
                case ObjectType.ammo:
                    field = obj_f.ammo_quantity;
                    return true;
                case ObjectType.food:
                    var itemFlags = item.GetItemFlags();
                    if (itemFlags.HasFlag(ItemFlag.IS_MAGICAL) && itemFlags.HasFlag(ItemFlag.EXPIRES_AFTER_USE))
                    {
                        field = obj_f.item_quantity;
                        return true;
                    }

                    field = default;
                    return false;
                case ObjectType.scroll:
                    field = obj_f.item_quantity;
                    return true;
                case ObjectType.weapon:
                    if (item.GetWeaponType() == WeaponType.shuriken)
                    {
                        field = obj_f.item_quantity;
                        return true;
                    }

                    field = default;
                    return false;
                case ObjectType.generic:
                    if (item.GetInt32(obj_f.category) == 5) // jewelry / gems
                    {
                        field = obj_f.item_quantity;
                        return true;
                    }

                    field = default;
                    return false;
                case ObjectType.armor:
                    if (item.GetInt32(obj_f.category) == 17) // necklaces
                    {
                        field = obj_f.item_quantity;
                        return true;
                    }

                    field = default;
                    return false;
                default:
                    field = default;
                    return false;
            }
        }

        [TempleDllLocation(0x10066C60)]
        public int GetItemWeight(GameObjectBody item)
        {
            if (item.type == ObjectType.money)
            {
                return 0;
            }
            else
            {
                var weight = item.GetInt32(obj_f.item_weight);
                if (GetQuantityField(item, out var field))
                {
                    weight = weight * item.GetInt32(field) / 4;
                }

                return weight;
            }
        }

        [TempleDllLocation(0x10067B80)]
        public int GetTotalCarriedWeight(GameObjectBody obj)
        {
            if (obj.type == ObjectType.container)
            {
                var itemCount = obj.GetInt32(obj_f.container_inventory_num);
                var result = 0;
                for (int i = 0; i < itemCount; i++)
                {
                    var item = obj.GetObject(obj_f.container_inventory_list_idx, i);
                    result += GetItemWeight(item);
                }

                return result;
            }
            else if (obj.IsCritter())
            {
                // TODO: Clean this up, this is iterating over the children n * n times.
                var result = 0;
                var slotCount = GetBagSlotCount(1);
                for (int i = 0; i < slotCount; i++)
                {
                    var item = GetItemAtInvIdx(obj, i);
                    if (item != null)
                    {
                        result += GetItemWeight(item);
                    }
                }

                for (int i = 200; i < 216; i++)
                {
                    var item = GetItemAtInvIdx(obj, i);
                    if (item != null)
                    {
                        result += GetItemWeight(item);
                    }
                }

                return result;
            }
            else
            {
                return 0;
            }
        }

        [TempleDllLocation(0x10066AC0)]
        public int GetBagSlotCount(int bagNumber)
        {
            int rows;
            if (bagNumber == 1)
            {
                rows = 4;
            }
            else if (bagNumber == 2)
            {
                rows = 2;
            }
            else
            {
                rows = 0;
            }

            if (bagNumber == 1)
            {
                return 6 * rows;
            }
            else if (bagNumber == 2)
            {
                return 4 * rows;
            }
            else
            {
                return 0;
            }
        }

        public void ResetBuffers()
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x1006d890)]
        public void PoopInventory(GameObjectBody obj, bool unsetNoTransfer, bool onlyInvulnerable = false)
        {
            // Note: Also replaces 0x1006db80 by introducing "onlyInvulnerable" param and moving
            // check for proto 1000 to callers

            var type = obj.type;
            if (type == ObjectType.container)
            {
                if (!IsEditor)
                {
                    if (obj.GetContainerFlags().HasFlag(ContainerFlag.INVEN_SPAWN_ONCE))
                    {
                        SpawnInventorySource(obj);
                    }
                }
            }

            if (!GameSystems.Object.GetInventoryFields(obj.type, out var indexField, out var numField))
            {
                return;
            }

            var itemCount = obj.GetInt32(numField);
            var moveTo = obj.GetLocation();
            for (var i = itemCount - 1; i >= 0; i--)
            {
                var item = obj.GetObject(indexField, i);

                // Skip items that are not invulnerable - if requested. Applies if the items are being
                // pooped because the parent obj is being destroyed
                if (onlyInvulnerable && !item.GetFlags().HasFlag(ObjectFlag.INVULNERABLE))
                {
                    continue;
                }

                ForceRemove(item, obj);
                if (unsetNoTransfer)
                {
                    item.SetItemFlag(ItemFlag.NO_TRANSFER, false);
                }

                GameSystems.Object.MoveItem(item, moveTo);

                if (item.GetItemFlags().HasFlag(ItemFlag.NO_LOOT))
                {
                    GameSystems.Object.Destroy(item);
                }
            }
        }

        [TempleDllLocation(0x10069ae0)]
        public void ForceRemove(GameObjectBody item, GameObjectBody container)
        {
        }

        [TempleDllLocation(0x1006d3d0)]
        public void SpawnInventorySource(GameObjectBody obj)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10069f60)]
        public void Remove(GameObjectBody item)
        {
            var parent = GetParent(item);
            if (parent == null)
            {
                Logger.Warn("Warning: item_remove called on item that doesn't think it has a parent_obj.");
                return;
            }

            ForceRemove(item, parent);

            if (parent.IsCritter())
            {
                GameSystems.D20.D20SendSignal(parent, D20DispatcherKey.SIG_Inventory_Update, item);
            }
        }

        [TempleDllLocation(0x10063e20)]
        public static void SetWeaponSlotDefaults(GameObjectBody obj)
        {
            if (obj.IsPC())
            {
                obj.SetInt32(obj_f.pc_weaponslots_idx, 0, 0);
                for (int i = 1; i <= 20; i++)
                {
                    obj.SetInt32(obj_f.pc_weaponslots_idx, i, -1);
                }
            }
        }
    }
}
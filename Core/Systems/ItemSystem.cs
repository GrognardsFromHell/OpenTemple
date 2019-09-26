using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Xml.Schema;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.GFX;
using SpicyTemple.Core.IO;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Logging;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.Systems.Feats;
using SpicyTemple.Core.Systems.ObjScript;
using SpicyTemple.Core.Systems.Script.Extensions;
using SpicyTemple.Core.Systems.TimeEvents;
using SpicyTemple.Core.TigSubsystems;
using SpicyTemple.Core.Utils;

namespace SpicyTemple.Core.Systems
{
    public class ItemSystem : IGameSystem, IBufferResettingSystem
    {
        private static readonly ILogger Logger = new ConsoleLogger();

        private const bool IsEditor = false;

        private readonly Dictionary<int, int[]> _startEquipment;

        private readonly Dictionary<ItemErrorCode, string> _itemErrorCodeText;

        [TempleDllLocation(0x10AA847C)]
        private bool _junkpileActive;

        [TempleDllLocation(0x10063c70)]
        public ItemSystem()
        {
            _startEquipment = Tig.FS.ReadMesFile("rules/start_equipment.mes")
                .ToDictionary(
                    kp => kp.Key,
                    kp => kp.Value.Split(new[] {' ', '\t', '\n'}, StringSplitOptions.RemoveEmptyEntries)
                        .Select(int.Parse)
                        .ToArray()
                );

            _itemErrorCodeText = new Dictionary<ItemErrorCode, string>(ItemErrorCodes.Codes.Count);
            var itemMes = Tig.FS.ReadMesFile("mes/item.mes");
            foreach (var code in ItemErrorCodes.Codes)
            {
                _itemErrorCodeText[code] = itemMes[100 + (int) code];
            }

            _junkpileActive = true;

            Stub.TODO();
        }

        public void Dispose()
        {
        }

        [TempleDllLocation(0x10063e80)]
        public GameObjectBody GetParent(GameObjectBody item)
        {
            if (item == null || !item.IsItem())
            {
                Logger.Warn("GetParent called on non-item: {0}", item);
                return null;
            }

            if (!item.HasFlag(ObjectFlag.INVENTORY))
            {
                return null;
            }

            return item.GetObject(obj_f.item_parent);
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
                case EquipSlot.Bag1:
                    return 218;
                case EquipSlot.Bag2:
                    return 219;
                case EquipSlot.Bag3:
                    return 220;
                case EquipSlot.Bag4:
                    return 221;
                default:
                    throw new ArgumentOutOfRangeException(nameof(slot), slot, null);
            }
        }

        public string GetAttachBone(GameObjectBody obj)
        {
            var wearFlags = obj.GetItemWearFlags();
            var slot = obj.GetItemInventoryLocation();

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

            if (GameSystems.D20.D20Query(wearer, D20DispatcherKey.QUE_Polymorphed))
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
            if (container.IsCritter() && GameSystems.D20.D20Query(container, D20DispatcherKey.QUE_Polymorphed))
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
        /// 3 for special stuff? (item too large)
        /// </summary>
        [TempleDllLocation(0x10066580)]
        public int GetWieldType(GameObjectBody wearer, GameObjectBody item, bool regardEnlargement = false)
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
            var wielderSizeBase = regardEnlargement ? (SizeCategory) wearer.GetInt32(obj_f.size) : wearerSizeCategory;
            var itemSizeCategory = (SizeCategory) item.GetInt32(obj_f.size);
            if (itemSizeCategory < wielderSizeBase)
            {
                wieldType = 0;
            }
            else if (itemSizeCategory == wielderSizeBase)
            {
                wieldType = 1;
            }
            else if (itemSizeCategory == wielderSizeBase + 1)
            {
                wieldType = 2;
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
                    wieldType = (wielderSizeBase <= SizeCategory.Small ? 3 : 2);
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

                GameSystems.MapObject.MoveItem(item, moveTo);

                if (item.GetItemFlags().HasFlag(ItemFlag.NO_LOOT))
                {
                    GameSystems.Object.Destroy(item);
                }
            }
        }

        [TempleDllLocation(0x10069ae0)]
        public void ForceRemove(GameObjectBody item, GameObjectBody parent)
        {
            if (parent == null)
            {
                Logger.Warn("ForceRemove called on null parent!");
                return;
            }

            var realParent = GetParent(item);
            if (realParent == null)
            {
                Logger.Warn("ForceRemove called on item that doesn't think it has a parent.");
                // return;
            }
            else
            {
                if (parent != realParent)
                {
                    Logger.Warn("ForceRemove called on item with different parent");
                }
            }

            if (!GameSystems.Object.GetInventoryFields(parent.type, out var listField, out var numfield))
            {
                throw new ArgumentException("ForceRemove called on something that is not a container: "
                                            + parent);
            }

            var itemCount = parent.GetInt32(numfield);
            var idx = -1;
            for (var i = 0; i < itemCount; i++)
            {
                if (item == parent.GetObject(listField, i))
                {
                    idx = i;
                    break;
                }
            }

            if (idx < 0)
            {
                Logger.Error("ForceRemove: Couldn't match object in parent!");
                return;
            }

            if (parent.IsCritter() && !parent.HasFlag(ObjectFlag.DESTROYED))
            {
                var dispatcher = parent.GetDispatcher();
                dispatcher?.Process(DispatcherType.ItemForceRemove, D20DispatcherKey.NONE, null);
            }

            // Delete item from inventory list
            var invIdxOrg = item.GetItemInventoryLocation();

            // Move everything foward one spot, then reduce the count
            while (idx < itemCount - 1)
            {
                var tmp = parent.GetObject(listField, idx + 1);
                parent.SetObject(listField, idx, tmp);
                idx++;
            }

            parent.RemoveObject(listField, itemCount - 1);
            parent.SetInt32(numfield, itemCount - 1);
            item.SetInt32(obj_f.item_inv_location, -1);

            if (IsInvIdxWorn(invIdxOrg))
            {
                if (IsNormalCrossbow(item))
                {
                    // unset OWF_WEAPON_LOADED
                    item.WeaponFlags &= ~WeaponFlag.WEAPON_LOADED;
                }

                UpdateCritterLightFlags(item, parent);
                GameSystems.Script.ExecuteObjectScript(parent, item, ObjScriptEvent.WieldOff);
                GameSystems.Critter.UpdateModelEquipment(parent);
            }

            if (parent.IsContainer())
            {
                if (!IsEditor && _junkpileActive)
                {
                    JunkpileOnRemoveItem(parent);
                }
            }
            else if (parent.IsNPC())
            {
                parent.AiFlags |= AiFlag.CheckWield;
            }

            GameSystems.Anim.NotifySpeedRecalc(parent);
            GameSystems.Script.ExecuteObjectScript(parent, item, ObjScriptEvent.RemoveItem);
            if (parent.IsCritter() && !parent.HasFlag(ObjectFlag.DESTROYED))
            {
                GameSystems.D20.Status.initItemConditions(parent);
                GameSystems.Critter.BuildRadialMenu(parent);
            }
        }

        public bool IsNormalCrossbow(GameObjectBody obj)
        {
            if (obj.type == ObjectType.weapon)
            {
                var weapType = obj.GetWeaponType();
                if (weapType == WeaponType.heavy_crossbow || weapType == WeaponType.light_crossbow)
                    return
                        true; // TODO: should this include repeating crossbow? I think the context is reloading action in some cases
                // || weapType == wt_hand_crossbow
            }

            return false;
        }

        public void AddItemToContainer(GameObjectBody container, int protoId, int quantity = 1)
        {
            if (quantity <= 0)
            {
                return;
            }

            var item = GameSystems.MapObject.CreateObject(protoId, container.GetLocationFull());
            if (quantity > 1)
            {
                item.SetQuantity(quantity);
            }

            if (!SetItemParent(item, container, ItemInsertFlag.Use_Max_Idx_200))
            {
                GameSystems.MapObject.RemoveMapObj(item);
                return;
            }

            SpawnAmmoForWeapon(container, item);
        }

        [TempleDllLocation(0x1006d3d0)]
        public void SpawnInventorySource(GameObjectBody obj)
        {
            var spawnOnce = false;
            int invSrcId;
            if (obj.IsContainer())
            {
                if ((obj.GetContainerFlags() & ContainerFlag.INVEN_SPAWN_ONCE) != 0)
                {
                    if (IsEditor)
                    {
                        return;
                    }

                    spawnOnce = true;
                }

                invSrcId = obj.GetInt32(obj_f.container_inventory_source);
            }
            else if (obj.IsCritter())
            {
                invSrcId = obj.GetInt32(obj_f.critter_inventory_source);
            }
            else
            {
                return;
            }

            GameSystems.Item.ClearInventory(obj, true);
            if (invSrcId == 0)
            {
                return;
            }

            if (!GameSystems.InvenSource.TryGetById(invSrcId, out var invenSource))
            {
                Logger.Error("Invalid inventory source {0} attached to {1}", invSrcId, obj);
                return;
            }

            if (spawnOnce)
            {
                int randomSeed = obj.GetLocation().locx;
                foreach (var partyMember in GameSystems.Party.PartyMembers)
                {
                    var name = GameSystems.MapObject.GetDisplayName(partyMember);
                    for (var i = 0; i + 1 < name.Length; i += 2)
                    {
                        randomSeed += name[i] * name[i + 1];
                    }
                }

                GameSystems.Random.SetSeed(randomSeed);
            }

            var copperCoins = GameSystems.Random.GetInt(invenSource.CopperMin, invenSource.CopperMax);
            if (copperCoins > 0)
            {
                AddItemToContainer(obj, WellKnownProtos.CopperCoin, copperCoins);
            }

            var silverCoins = GameSystems.Random.GetInt(invenSource.SilverMin, invenSource.SilverMax);
            if (silverCoins > 0)
            {
                AddItemToContainer(obj, WellKnownProtos.SilverCoin, silverCoins);
            }

            var goldCoins = GameSystems.Random.GetInt(invenSource.GoldMin, invenSource.GoldMax);
            if (goldCoins > 0)
            {
                AddItemToContainer(obj, WellKnownProtos.GoldCoin, goldCoins);
            }

            var platinumCoins = GameSystems.Random.GetInt(invenSource.PlatinumMin, invenSource.PlatinumMax);
            if (platinumCoins > 0)
            {
                AddItemToContainer(obj, WellKnownProtos.PlatinumCoin, platinumCoins);
            }

            AddGemsToContainer(invenSource.GemsMin, invenSource.GemsMax, obj);
            AddJewelryToContainer(invenSource.JewelryMax, invenSource.JewelryMax, obj);

            foreach (var itemSpec in invenSource.Items)
            {
                if (GameSystems.Random.GetInt(1, 100) <= itemSpec.PercentChance)
                {
                    AddItemToContainer(obj, itemSpec.ProtoId);
                }
            }

            foreach (var oneOfList in invenSource.OneOfLists)
            {
                var itemSpec = GameSystems.Random.PickRandom(oneOfList);
                AddItemToContainer(obj, itemSpec.ProtoId);
            }

            if (obj.type == ObjectType.npc)
            {
                if ((obj.GetNPCFlags() & NpcFlag.KOS) != 0)
                {
                    WieldBestAll(obj);
                }
                else
                {
                    WieldBestAllExceptWeapons(obj);
                }
            }

            if (obj.IsContainer() && spawnOnce)
            {
                obj.SetInt32(obj_f.container_inventory_source, 0);
                var flags = obj.GetContainerFlags();
                obj.SetContainerFlags(flags & ~ContainerFlag.INVEN_SPAWN_ONCE);
            }
        }

        [TempleDllLocation(0x1006bd30)]
        private void SpawnAmmoForWeapon(GameObjectBody container, GameObjectBody item)
        {
            if (item.type == ObjectType.weapon)
            {
                var ammoType = item.GetWeaponAmmoType();
                if (ammoType != WeaponAmmoType.no_ammo)
                {
                    SpawnAmmoForWeapon(container, ammoType, 20);
                }
            }
        }

        [TempleDllLocation(0x1006bba0)]
        [TempleDllLocation(0x10065400)]
        public void SpawnAmmoForWeapon(GameObjectBody container, WeaponAmmoType ammoType, int quantity)
        {
            if (quantity <= 0)
            {
                return;
            }

            int ammoProtoId;
            switch (ammoType)
            {
                case WeaponAmmoType.arrow:
                    ammoProtoId = 5004;
                    break;
                case WeaponAmmoType.bolt:
                    ammoProtoId = 5005;
                    break;
                case WeaponAmmoType.bullet:
                    ammoProtoId = 5007;
                    break;
                default:
                    return;
            }

            var existingItem = GameSystems.Item.FindAmmoItem(container, ammoType);
            if (existingItem != null)
            {
                existingItem.IncreaseQuantity(quantity);
            }
            else
            {
                AddItemToContainer(container, ammoProtoId, quantity);
            }

            GameSystems.Anim.NotifySpeedRecalc(container);
        }

        [TempleDllLocation(0x1006b780)]
        private void AddGemsToContainer(int minAmount, int maxAmount, GameObjectBody container)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x102be7a4)]
        private int dword_102BE7A4 = 10;

        private class GemTypeSpec
        {

            public int ValuePerGem { get; }

            public IList<int> ProtoIds { get; }

            public GemTypeSpec(int valuePerGem, params int[] protoIds)
            {
                ValuePerGem = valuePerGem;
                ProtoIds = protoIds.ToImmutableList();
            }
        }

        // TODO: This list is likely fucked and shifted by one
        [TempleDllLocation(0x102be7a8)]
        private static readonly GemTypeSpec[] GemTypes = {
            new GemTypeSpec(50, 12041, 12042),
            new GemTypeSpec(100, 12035, 12040),
            new GemTypeSpec(500, 12034, 12039),
            new GemTypeSpec(1000, 12010, 12038),
            new GemTypeSpec(5000, 12036, 12037)
        };

        [TempleDllLocation(0x1006b780)]
        public void  SpawnGems(int minValue, int maxValue, GameObjectBody container)
        {
            if (minValue <= 0 && maxValue <= 0)
            {
                return;
            }

            var v3 = (GameSystems.Random.GetInt(minValue, maxValue) + dword_102BE7A4 / 2) / dword_102BE7A4;
            var remainingValue = dword_102BE7A4 * v3;
            if ( remainingValue < dword_102BE7A4 )
            {
                remainingValue = dword_102BE7A4;
            }
            var v5 = 10;

            for (var i = GemTypes.Length - 1; i >= 0; i--)
            {
                var gemType = GemTypes[i];
                var v7 = remainingValue / gemType.ValuePerGem;
                if (v7 >= GameSystems.Random.GetInt(1, 4))
                {
                    var protoId = GameSystems.Random.PickRandom(gemType.ProtoIds);

                    sub_1006AAB0/*0x1006aab0*/(container, protoId, v7, -1);
                    remainingValue -= v7 * gemType.ValuePerGem;
                }

            }

            int* v6 = GemTypes;
            while ( remainingValue > 0 )
            {
                var v7 = remainingValue / *v6;
                if ( v7 >= GameSystems.Random.GetInt(1, 4) )
                {
                    var v8 = GameSystems.Random.GetInt(1, 2);
                    sub_1006AAB0/*0x1006aab0*/(container, dword_102BE7BC/*0x102be7bc*/[v5 + v8 - 1], v7, -1);
                    remainingValue -= v7 * *v6;
                }
                --v6;
                v5 -= 2;
                if ( (int)v6 <= (int)&unk_102BE7A8/*0x102be7a8*/ )
                {
                    if ( remainingValue > 0 )
                    {
                        var v9 = remainingValue / dword_102BE7A4;
                        var v10 = GameSystems.Random.GetInt(1, 2);
                        sub_1006AAB0/*0x1006aab0*/(container, dword_102BE7B8/*0x102be7b8*/[v10], v9, -1);
                    }
                    return;
                }
            }
        }


        [TempleDllLocation(0x1006b860)]
        private void AddJewelryToContainer(int minValue, int maxValue, GameObjectBody container)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x1006dcf0)]
        public void PossiblySpawnInvenSource(GameObjectBody obj)
        {
            var inventorySource = GetInventorySource(obj);
            if (inventorySource == 0)
            {
                return;
            }

            Stub.TODO();
        }

        [TempleDllLocation(0x10064be0)]
        public int GetInventorySource(GameObjectBody obj)
        {
            if (obj.IsContainer())
            {
                return obj.GetInt32(obj_f.container_inventory_source);
            }
            else if (obj.IsCritter())
            {
                return obj.GetInt32(obj_f.critter_inventory_source);
            }

            return 0;
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

        [TempleDllLocation(0x10069e00)]
        public void ClearInventory(GameObjectBody obj, bool keepPersistent)
        {
            // Objects are moved here temporarily to be deleted
            var targetLocation = obj.GetLocation();

            var children = new List<GameObjectBody>(obj.EnumerateChildren());

            foreach (var child in children)
            {
                if (keepPersistent && child.GetItemFlags().HasFlag(ItemFlag.PERSISTENT))
                {
                    continue;
                }

                ForceRemove(child, obj);
                GameSystems.MapObject.MoveItem(child, targetLocation);
                GameSystems.Object.Destroy(child);
            }

            if (obj.IsCritter())
            {
                GameSystems.Critter.ClearMoney(obj);
            }
        }

        [TempleDllLocation(0x1006d300)]
        public void SpawnTutorialEquipment(GameObjectBody obj)
        {
            ClearInventory(obj, false);

            if (_startEquipment.TryGetValue(11, out var equipment))
            {
                foreach (var protoId in equipment)
                {
                    GiveItemByProto(obj, (ushort) protoId);
                }
            }

            WieldBestAll(obj, null);
        }

        [TempleDllLocation(0x1006d100)]
        public void WieldBestAll(GameObjectBody critter, GameObjectBody target = null)
        {
            for (var invIdx = INVENTORY_WORN_IDX_START; invIdx < INVENTORY_WORN_IDX_END; invIdx++)
            {
                WieldBest(critter, invIdx, target);
            }
        }

        [TempleDllLocation(0x1006d140)]
        private void WieldBestAllExceptWeapons(GameObjectBody critter)
        {
            for (var invIdx = INVENTORY_WORN_IDX_START; invIdx < INVENTORY_WORN_IDX_END; invIdx++)
            {
                if (invIdx == InvIdxWeaponPrimary || invIdx == InvIdxWeaponSecondary || invIdx == InvIdxShield)
                {
                    continue;
                }

                WieldBest(critter, invIdx, null);
            }
        }

        [TempleDllLocation(0x1006aa80)]
        public bool UnequipItemInSlot(GameObjectBody critter, EquipSlot slot)
        {
            var item = ItemWornAt(critter, slot);
            if (item != null)
            {
                return UnequipItem(item);
            }
            else
            {
                return true;
            }
        }

        [TempleDllLocation(0x1006a640)]
        public bool UnequipItem(GameObjectBody item)
        {
            var invIdx = item.GetItemInventoryLocation();
            if (IsInvIdxWorn(invIdx))
            {
                var receiver = GetParent(item);
                var itemInsertLocation = INVENTORY_IDX_UNDEFINED;
                if (IsItemNonTransferrable(item, receiver) != ItemErrorCode.OK)
                {
                    return false;
                }

                if (ItemInsertGetLocation(item, receiver, ref itemInsertLocation, null, default) != ItemErrorCode.OK)
                {
                    return false;
                }

                Remove(item);
                InsertAtLocation(item, receiver, itemInsertLocation);
            }

            return true;
        }

        [TempleDllLocation(0x10064c40)]
        public bool IsMagical(GameObjectBody item) => item.GetItemFlags().HasFlag(ItemFlag.IS_MAGICAL);

        /// <summary>
        /// Identifies all items in the given container / critter.
        /// </summary>
        [TempleDllLocation(0x10064c70)]
        public void IdentifyAll(GameObjectBody container)
        {
            foreach (var item in container.EnumerateChildren())
            {
                item.SetItemFlag(ItemFlag.IDENTIFIED, true);
            }
        }

        /// <summary>
        /// The sum of copper coins to add to an items worth for considering it the best item to wear.
        /// </summary>
        private const int MagicalItemWorthBonus = 1000000;

        [TempleDllLocation(0x1006CCC0)]
        public void WieldBest(GameObjectBody critter, int invIdx, GameObjectBody target)
        {
            if (invIdx == InvIdxWeaponSecondary)
            {
                return;
            }

            var bestItem = GetItemAtInvIdx(critter, invIdx);
            var bestItemWorth = (float) int.MinValue;
            if (bestItem != null)
            {
                if (CheckTransferToWieldSlot(bestItem, invIdx, critter) != ItemErrorCode.OK)
                {
                    UnequipItemInSlot(critter, SlotByInvIdx(invIdx));
                    bestItem = null;
                }
                else if (invIdx != InvIdxWeaponPrimary)
                {
                    bestItemWorth = GetItemWorthRegardIdentified(bestItem);
                    if (IsMagical(bestItem))
                    {
                        bestItemWorth += MagicalItemWorthBonus;
                    }
                }
            }

            GameObjectBody bestSecondaryWeapon = null;

            var inventory = critter.EnumerateChildren().ToArray();

            for (var i = 0; i < inventory.Length; i++)
            {
                var itemCandidate = inventory[i];
                if (CheckTransferToWieldSlot(itemCandidate, invIdx, critter) != ItemErrorCode.OK)
                {
                    // Skip anything that cannot be worn in the desired slot
                    continue;
                }

                // Anything but the primary weapon is chosen only based on it's monetary worth,
                // preferring anything that is magical
                if (invIdx != InvIdxWeaponPrimary)
                {
                    if (!IsInvIdxWorn(itemCandidate.GetItemInventoryLocation()))
                    {
                        var candidateWorth = GetItemWorthRegardIdentified(itemCandidate);
                        if (IsMagical(itemCandidate))
                        {
                            candidateWorth += MagicalItemWorthBonus;
                        }

                        if (candidateWorth > bestItemWorth)
                        {
                            bestItemWorth = candidateWorth;
                            bestItem = itemCandidate;
                        }
                    }

                    continue;
                }

                var weaponAmmoType = itemCandidate.GetWeaponAmmoType();
                if (weaponAmmoType == WeaponAmmoType.no_ammo)
                {
                    if (GameSystems.Feat.HasFeat(critter, FeatId.TWO_WEAPON_FIGHTING))
                    {
                        for (int j = 0; j < inventory.Length; j++)
                        {
                            if (j != i)
                            {
                                var offHandWeapon = inventory[j];
                                if (CheckTransferToWieldSlot(offHandWeapon, InvIdxWeaponSecondary, critter) ==
                                    ItemErrorCode.OK &&
                                    offHandWeapon.GetWeaponAmmoType() == WeaponAmmoType.no_ammo)
                                {
                                    var v17 = GetExpectedWeaponDamage(itemCandidate, offHandWeapon, critter, target);
                                    if (v17 > bestItemWorth)
                                    {
                                        bestItemWorth = v17;
                                        bestItem = itemCandidate;
                                        bestSecondaryWeapon = offHandWeapon;
                                    }
                                }
                            }
                        }
                    }

                    var expectedDamage = GetExpectedWeaponDamage(itemCandidate, null, critter, target);
                    if (expectedDamage > bestItemWorth)
                    {
                        bestItemWorth = expectedDamage;
                        bestItem = itemCandidate;
                        bestSecondaryWeapon = null;
                    }
                }
                else
                {
                    // Check that the considered weapon has enough ammo to be used
                    var availableAmmo = GetAmmoQuantity(critter, weaponAmmoType);
                    var requiredAmmo = itemCandidate.GetInt32(obj_f.weapon_ammo_consumption);
                    if (availableAmmo >= requiredAmmo)
                    {
                        var expectedDamage = GetExpectedWeaponDamage(itemCandidate, null,
                            critter, target);

                        // Ammo types with a (potential) infinite supply seem to be preferred
                        if (weaponAmmoType.TryGetInfiniteSupplyProtoId(out _))
                        {
                            expectedDamage *= 3.0f;
                        }

                        if (expectedDamage > bestItemWorth)
                        {
                            bestItemWorth = expectedDamage;
                            bestItem = itemCandidate;
                            bestSecondaryWeapon = null;
                        }

                        continue;
                    }
                }
            }

            if (invIdx != InvIdxWeaponPrimary)
            {
                if (bestItem != null)
                {
                    ItemPlaceInIndex(bestItem, invIdx);
                }

                return;
            }

            if (bestItem != null)
            {
                ItemPlaceInIndex(bestItem, invIdx);
            }
            else
            {
                UnequipItemInSlot(critter, EquipSlot.WeaponPrimary);
            }

            if (bestSecondaryWeapon != null)
            {
                ItemPlaceInIndex(bestSecondaryWeapon, InvIdxWeaponSecondary);
            }
            else
            {
                UnequipItemInSlot(critter, EquipSlot.WeaponSecondary);
            }
        }

        [TempleDllLocation(0x10067310)]
        private int GetAmmoQuantity(GameObjectBody critter, WeaponAmmoType ammoType)
        {
            var ammoItem = FindAmmoItem(critter, ammoType);

            if (ammoItem != null)
            {
                return ammoItem.GetInt32(obj_f.ammo_quantity);
            }

            return 0;
        }

        [TempleDllLocation(0x10065320)]
        private GameObjectBody FindAmmoItem(GameObjectBody container, WeaponAmmoType ammoType)
        {
            foreach (var item in container.EnumerateChildren())
            {
                if (item.type == ObjectType.ammo && item.GetInt32(obj_f.ammo_type) == (int) ammoType)
                {
                    return item;
                }
            }

            return null;
        }

        [TempleDllLocation(0x1006bd90)]
        private float GetExpectedWeaponDamage(GameObjectBody weapon,
            GameObjectBody offhandWeapon,
            GameObjectBody attacker,
            GameObjectBody target)
        {
            var primaryWeaponExpectedDmg = 0.0f;
            var secondaryWeapExpectedDmg = 0.0f;
            var tgtAc = 15;

            // Unequip current main and offhand weapons
            var currentMainHand = ItemWornAt(attacker, EquipSlot.WeaponPrimary);
            var currentOffHand = ItemWornAt(attacker, EquipSlot.WeaponSecondary);
            if (currentMainHand != null)
            {
                UnequipItem(currentMainHand);
            }

            if (currentOffHand != null)
            {
                UnequipItem(currentOffHand);
            }

            if (weapon != null)
            {
                ItemPlaceInIndex(weapon, 203);
            }

            if (offhandWeapon != null)
            {
                ItemPlaceInIndex(offhandWeapon, 204);
            }

            if (target != null)
            {
                var acDispIo = DispIoAttackBonus.Default;
                acDispIo.attackPacket.attacker = target;
                acDispIo.attackPacket.victim = attacker;
                acDispIo.attackPacket.weaponUsed = weapon;
                acDispIo.attackPacket.ammoItem = CheckRangedWeaponAmmo(attacker);
                tgtAc = GameSystems.Stat.GetAC(target, acDispIo);
            }

            var primaryToHitDispIo = DispIoAttackBonus.Default;
            primaryToHitDispIo.attackPacket.victim = target;
            primaryToHitDispIo.attackPacket.d20ActnType = D20ActionType.FULL_ATTACK;
            primaryToHitDispIo.attackPacket.attacker = attacker;
            primaryToHitDispIo.attackPacket.weaponUsed = weapon;
            primaryToHitDispIo.attackPacket.ammoItem = CheckRangedWeaponAmmo(attacker);
            primaryToHitDispIo.attackPacket.dispKey = 1;
            if (offhandWeapon != null)
            {
                primaryToHitDispIo.attackPacket.dispKey = 5;
            }

            var toHitBonPrimaryAtk = GameSystems.Stat.Dispatch16GetToHitBonus(attacker, primaryToHitDispIo);

            var toHitBonSecondaryAtk = 0;
            if (offhandWeapon != null && offhandWeapon.type == ObjectType.weapon)
            {
                var secondaryToHitDispIo = DispIoAttackBonus.Default;
                secondaryToHitDispIo.attackPacket.victim = target;
                secondaryToHitDispIo.attackPacket.d20ActnType = D20ActionType.FULL_ATTACK;
                secondaryToHitDispIo.attackPacket.attacker = attacker;
                secondaryToHitDispIo.attackPacket.dispKey = 6;
                secondaryToHitDispIo.attackPacket.weaponUsed = offhandWeapon;
                secondaryToHitDispIo.attackPacket.ammoItem = null;
                secondaryToHitDispIo.attackPacket.flags = D20CAF.SECONDARY_WEAPON;
                toHitBonSecondaryAtk = GameSystems.Stat.Dispatch16GetToHitBonus(attacker, secondaryToHitDispIo);
            }

            var hitChancePrimary = 1.0f - (tgtAc - toHitBonPrimaryAtk) / 20.0f;
            var hitChanceSecondary = 1.0f - (tgtAc - toHitBonSecondaryAtk) / 20.0f;
            if (weapon != null && weapon.type == ObjectType.weapon)
            {
                var damDice = Dice.Unpack(weapon.GetUInt32(obj_f.weapon_damage_dice));
                primaryWeaponExpectedDmg = damDice.ExpectedValue;
            }

            if (offhandWeapon != null && offhandWeapon.type == ObjectType.weapon)
            {
                var damDice = Dice.Unpack(offhandWeapon.GetUInt32(obj_f.weapon_damage_dice));
                secondaryWeapExpectedDmg = damDice.ExpectedValue;
            }

            // Re-Equip the previous weapons
            if (currentMainHand != null)
            {
                ItemPlaceInIndex(currentMainHand, 203);
            }
            else
            {
                var currentItem = ItemWornAt(attacker, EquipSlot.WeaponPrimary);
                if (currentItem != null)
                {
                    UnequipItem(currentItem);
                }
            }

            if (currentOffHand != null)
            {
                ItemPlaceInIndex(currentOffHand, 204);
            }
            else
            {
                var currentItem = ItemWornAt(attacker, EquipSlot.WeaponSecondary);
                if (currentItem != null)
                {
                    UnequipItem(currentItem);
                }
            }

            return secondaryWeapExpectedDmg * hitChanceSecondary
                   + primaryWeaponExpectedDmg * hitChancePrimary;
        }

        [TempleDllLocation(0x1006bb50)]
        public void ItemPlaceInIndex(GameObjectBody item, int invIdx)
        {
            var parent = GetParent(item);
            ItemTransferWithFlags(item, parent, invIdx, ItemInsertFlag.Unk4, null);
        }

        [TempleDllLocation(0x1006AA60)]
        public bool ItemDrop(GameObjectBody item)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x1006b040)]
        public ItemErrorCode ItemTransferWithFlags(GameObjectBody item, GameObjectBody receiver, int invIdx,
            ItemInsertFlag flags, GameObjectBody bag)
        {
            var parent = GetParent(item);
            if (parent == null || receiver == null || item == null)
            {
                return ItemErrorCode.Cannot_Transfer;
            }

            ItemErrorCode ReportInsertFailed(ItemErrorCode code)
            {
                Logger.Warn("TransferWithFlags: item '{0}' cannot be inserted into inventory of '{1}', reason={2}",
                    item, parent, code.ToString());
                return code;
            }

            var itemInvLocation = item.GetInt32(obj_f.item_inv_location);
            var itemInsertLocation = INVENTORY_IDX_UNDEFINED;
            if (invIdx != INVENTORY_IDX_UNDEFINED)
                itemInsertLocation = invIdx;

            if (!receiver.IsCritter() && invIdx != INVENTORY_IDX_UNDEFINED && IsInvIdxWorn(invIdx))
            {
                return ItemErrorCode.No_Room_For_Item;
            }

            var isNonTransferable = IsItemNonTransferrable(item, receiver);
            if (isNonTransferable != ItemErrorCode.OK)
            {
                if (isNonTransferable > 0)
                {
                    Logger.Warn("TransferWithFlags: item {0} cannot be removed from {1}'s inventory, reason = {2}.",
                        item, parent, isNonTransferable);
                }

                return isNonTransferable;
            }

            var result = ItemInsertGetLocation(item, receiver, ref itemInsertLocation, bag, flags);
            if (result == ItemErrorCode.Wrong_Type_For_Slot)
            {
                /* Handle Wrong Type For Slot error
                 try to transfer to matching equip slot
                */

                if (invIdx == INVENTORY_IDX_UNDEFINED || !(flags.HasFlag(ItemInsertFlag.Use_Wield_Slots)))
                {
                    return ReportInsertFailed(ItemErrorCode.Wrong_Type_For_Slot);
                }

                result = ItemTransferToSuitableEquipSlot(parent, receiver, item, itemInsertLocation,
                    flags);
                TransferPcInvLocation(item, itemInvLocation);
                return result;
            }

            if (result != ItemErrorCode.OK)
            {
                // Handle Wield Slot Occupied error
                // try to swap with existing item.
                if (result != ItemErrorCode.Wield_Slot_Occupied || !(flags.HasFlag(ItemInsertFlag.Allow_Swap)))
                {
                    return ReportInsertFailed(result);
                }

                // try to swap Secondary Weapon with Shield slot (this looks a bit weird but eh)
                if (itemInsertLocation == InvIdxWeaponSecondary)
                {
                    var shield = ItemWornAt(parent, EquipSlot.Shield);
                    if (shield != null)
                    {
                        return ReportInsertFailed(ItemErrorCode.Wield_Slot_Occupied);
                    }
                }

                var swapWithItem = GetItemAtInvIdx(receiver, itemInsertLocation);
                if (swapWithItem == null)
                {
                    return ReportInsertFailed(ItemErrorCode.Wield_Slot_Occupied);
                }

                result = PerformSwap(parent, receiver, item, swapWithItem, itemInsertLocation, itemInsertLocation,
                    flags);
                TransferPcInvLocation(item, itemInvLocation);
                return result;
            }


            // ItemErrorCode.OK
            if (itemInsertLocation == invIdx || itemInsertLocation != INVENTORY_IDX_UNDEFINED)
            {
                if ((flags.HasFlag(ItemInsertFlag.Allow_Swap)) && GetItemAtInvIdx(parent, invIdx) != null)
                {
                    itemInsertLocation = invIdx;
                }

                result = PerformTransfer(parent, receiver, item, itemInsertLocation, flags);
                TransferPcInvLocation(item, itemInvLocation);
                return result;
            }

            // Unspecified Inventory Slot
            if (invIdx == INVENTORY_IDX_UNDEFINED)
            {
                if (flags.HasFlag(ItemInsertFlag.Use_Wield_Slots))
                {
                    result = ItemTransferToSuitableEquipSlot(parent, receiver, item,
                        INVENTORY_IDX_UNDEFINED, flags);
                    TransferPcInvLocation(item, itemInvLocation);
                    return result;
                }

                if (flags.HasFlag(ItemInsertFlag.Unk4))
                {
                    result = PerformTransfer(parent, receiver, item, INVENTORY_IDX_UNDEFINED, flags);
                    TransferPcInvLocation(item, itemInvLocation);
                    return result;
                }

                return ReportInsertFailed(ItemErrorCode.Cannot_Transfer);
            }

            // Specified Slot
            // Check if slot is clear
            var existingItem = GetItemAtInvIdx(receiver, invIdx);
            if (existingItem == null)
            {
                result = PerformTransfer(parent, receiver, item, invIdx, flags);
                TransferPcInvLocation(item, itemInvLocation);
                return result;
            }

            // If not, check swappage
            if (flags.HasFlag(ItemInsertFlag.Allow_Swap))
            {
                result = PerformSwap(parent, receiver, item, existingItem, invIdx, INVENTORY_IDX_UNDEFINED,
                    flags);
                TransferPcInvLocation(item, itemInvLocation);
                return result;
            }

            // If not, try unspecified slot
            if (!(flags.HasFlag(ItemInsertFlag.Use_Wield_Slots)))
            {
                if (!(flags.HasFlag(ItemInsertFlag.Unk4)))
                {
                    return ItemErrorCode.No_Room_For_Item;
                }

                result = PerformTransfer(parent, receiver, item, INVENTORY_IDX_UNDEFINED, flags);
                TransferPcInvLocation(item, itemInvLocation);
                return result;
            }

            // Go over wield slots
            foreach (var equipSlot in EquipSlots.Slots)
            {
                if (ItemWornAt(receiver, equipSlot) != null)
                {
                    continue;
                }

                int slotInvIdx = GetInventoryLocationForSlot(equipSlot);
                if (CheckTransferToWieldSlot(item, slotInvIdx, receiver) != ItemErrorCode.OK)
                {
                    result = PerformTransfer(parent, receiver, item, slotInvIdx, flags);
                    TransferPcInvLocation(item, itemInvLocation);
                    return result;
                }
            }

            return ItemErrorCode.Cannot_Transfer;
        }

        [TempleDllLocation(0x10067b10)]
        private void TransferPcInvLocation(GameObjectBody item, int itemInvLocation)
        {
            var parent = GetParent(item);
            if (parent == null)
            {
                return;
            }

            if (parent.IsPC())
            {
                var oldInvLocation = item.GetItemInventoryLocation();
                PcInvLocationSet(parent, itemInvLocation, oldInvLocation);
            }
        }

        [TempleDllLocation(0x10067970)]
        private void PcInvLocationSet(GameObjectBody parent, int itemInvLocation, int itemInvLocationNew)
        {
            for (var i = 1; i < 21; ++i)
            {
                if (parent.GetInt32(obj_f.pc_weaponslots_idx, i) == itemInvLocation)
                {
                    parent.SetInt32(obj_f.pc_weaponslots_idx, i, itemInvLocationNew);
                }
            }
        }

        [TempleDllLocation(0x1006A6E0)]
        private ItemErrorCode ItemTransferToSuitableEquipSlot(GameObjectBody owner, GameObjectBody receiver,
            GameObjectBody item, int a6, ItemInsertFlag flags)
        {
            if (!receiver.IsCritter())
            {
                return ItemErrorCode.Cannot_Transfer;
            }

            foreach (var equipSlot in EquipSlots.Slots)
            {
                var invIdx = GetInventoryLocationForSlot(equipSlot);

                // TODO: Shouldn't this use CanCurrentlyEquipItem??
                if (ItemCheckSlotAndWieldFlags(item, receiver, invIdx) != ItemErrorCode.OK)
                {
                    continue;
                }

                var itemAlreadyWorn = ItemWornAt(receiver, equipSlot);
                if (itemAlreadyWorn == null)
                {
                    return PerformTransfer(owner, receiver, item, invIdx, flags);
                }

                var v10 = itemAlreadyWorn.GetItemInventoryLocation();
                if (flags.HasFlag(ItemInsertFlag.Allow_Swap))
                {
                    return PerformSwap(owner, receiver, item, itemAlreadyWorn, v10, a6, flags);
                }
            }

            return ItemErrorCode.Cannot_Transfer;
        }

        [TempleDllLocation(0x1006AB50)]
        private ItemErrorCode PerformSwap(GameObjectBody owner, GameObjectBody receiver, GameObjectBody item,
            GameObjectBody itemPrevious, int equippedItemSlot, int a7, ItemInsertFlag flags)
        {
            if (IsItemNonTransferrable(itemPrevious, receiver) != ItemErrorCode.OK)
            {
                if (flags.HasFlag(ItemInsertFlag.Use_Wield_Slots))
                {
                    return ItemTransferToSuitableEquipSlot(owner, receiver, item, a7, flags);
                }
                else if (flags.HasFlag(ItemInsertFlag.Unk4))
                {
                    return PerformTransfer(owner, receiver, item, a7, flags);
                }
                else
                {
                    return ItemErrorCode.Cannot_Transfer;
                }
            }

            var v10 = item;
            var itemInsertLocation = item.GetItemInventoryLocation();
            if (IsInvIdxWorn(equippedItemSlot))
            {
                if (equippedItemSlot == InvIdxArmor)
                {
                    if (IsIncompatibleWithDruid(item, receiver))
                    {
                        return ItemErrorCode.ItemCannotBeUsed;
                    }
                }
                else if (equippedItemSlot == InvIdxShield)
                {
                    var primaryWeapon = ItemWornAt(receiver, EquipSlot.WeaponPrimary);
                    if (primaryWeapon != null)
                    {
                        if (GetWieldType(receiver, primaryWeapon) >= 2
                            || primaryWeapon.GetItemWearFlags().HasFlag(ItemWearFlag.TWOHANDED_REQUIRED))
                        {
                            return ItemErrorCode.No_Free_Hand;
                        }
                    }
                }
                else if (equippedItemSlot == InvIdxWeaponPrimary)
                {
                    if (GetWieldType(receiver, item) == 2 ||
                        item.GetItemWearFlags().HasFlag(ItemWearFlag.TWOHANDED_REQUIRED))
                    {
                        // Check for a free hand if trying to equip a two-handed weapon
                        var secondaryWeapon = ItemWornAt(receiver, EquipSlot.WeaponSecondary);
                        if (secondaryWeapon != null)
                        {
                            return ItemErrorCode.No_Free_Hand;
                        }

                        var shield = ItemWornAt(receiver, EquipSlot.Shield);
                        if (shield != null)
                        {
                            if (!shield.GetItemWearFlags().HasFlag(ItemWearFlag.BUCKLER))
                            {
                                return ItemErrorCode.No_Free_Hand;
                            }
                        }
                    }
                    else if (GetWieldType(receiver, item) == 3)
                    {
                        return ItemErrorCode.Item_Too_Large;
                    }
                }

                var result = ItemCheckSlotAndWieldFlags(v10, receiver, equippedItemSlot);
                if (result != ItemErrorCode.OK)
                {
                    return result;
                }
            }

            if (IsInvIdxWorn(itemInsertLocation) &&
                ItemCheckSlotAndWieldFlags(itemPrevious, owner, itemInsertLocation) != ItemErrorCode.OK)
            {
                return PerformTransfer(owner, receiver, v10, a7, flags);
            }

            if (owner != receiver)
            {
                v10.SetItemFlag(ItemFlag.NO_TRANSFER, false);
            }

            Remove(itemPrevious);
            if (v10 != itemPrevious)
            {
                Remove(v10);
            }

            InsertAtLocation(v10, receiver, equippedItemSlot);
            InsertAtLocation(itemPrevious, owner, itemInsertLocation);
            return ItemErrorCode.OK;
        }

        [TempleDllLocation(0x1006a000)]
        private ItemErrorCode PerformTransfer(GameObjectBody owner, GameObjectBody receiver, GameObjectBody item,
            int invSlot, ItemInsertFlag flags)
        {
            if (owner != null && owner.IsCritter() && IsInvIdxWorn(invSlot))
            {
                var result = CheckTransferToWieldSlot(item, invSlot, receiver);
                if (result != ItemErrorCode.OK)
                {
                    if (!(flags.HasFlag(ItemInsertFlag.Allow_Swap)))
                    {
                        return result;
                    }

                    var existingItem = GetItemAtInvIdx(owner, invSlot);
                    if (existingItem == null)
                    {
                        return result;
                    }

                    result = PerformSwap(owner, receiver, item, existingItem, invSlot, invSlot, flags);
                    return result;
                }
            }

            if (owner == receiver)
            {
                Remove(item);
                InsertAtLocation(item, receiver, invSlot);
                return ItemErrorCode.OK;
            }

            // owner != receiver
            Remove(item);
            item.SetItemFlag(ItemFlag.NO_TRANSFER, false);
            InsertAtLocation(item, receiver, invSlot);
            return ItemErrorCode.OK;
        }

        [TempleDllLocation(0x100654e0)]
        public GameObjectBody CheckRangedWeaponAmmo(GameObjectBody critter)
        {
            if (GameSystems.D20.D20Query(critter, D20DispatcherKey.QUE_Polymorphed))
            {
                return null;
            }

            var ammoItem = ItemWornAt(critter, EquipSlot.Ammo);
            if (ammoItem == null)
            {
                return null;
            }

            var primaryWeapon = ItemWornAt(critter, EquipSlot.WeaponPrimary);
            if (primaryWeapon != null)
            {
                // TODO: This is weird. There is no check whether the ammo matches the primary weapon in Vanilla.
                return primaryWeapon;
            }

            var secondaryWeapon = ItemWornAt(critter, EquipSlot.WeaponSecondary);
            if (secondaryWeapon != null && AmmoMatchesWeapon(secondaryWeapon, ammoItem))
            {
                return ammoItem;
            }

            return null;
        }

        [TempleDllLocation(0x10065470)]
        public bool AmmoMatchesWeapon(GameObjectBody weapon, GameObjectBody ammoItem)
        {
            if (weapon.type != ObjectType.weapon)
            {
                return false;
            }

            var ammoType = weapon.GetWeaponAmmoType();
            if (ammoType.IsThrown())
            {
                // Thrown weapons don't need an ammo item
                return true;
            }

            if (ammoItem == null)
            {
                return false;
            }

            return ammoItem.GetInt32(obj_f.ammo_type) == (int) ammoType;
        }

        [TempleDllLocation(0x10065990)]
        public bool AmmoMatchesItemAtSlot(GameObjectBody critter, EquipSlot slot)
        {
            var ammo = CheckRangedWeaponAmmo(critter);
            var weapon = ItemWornAt(critter, slot);
            return weapon != null && AmmoMatchesWeapon(weapon, ammo);
        }

        /// <summary>
        /// Returned value is in copper coins.
        /// </summary>
        [TempleDllLocation(0x10067c90)]
        private int GetItemWorthRegardIdentified(GameObjectBody item)
        {
            if (item.type == ObjectType.money)
            {
                return GetTotalCurrencyAmount(item);
            }
            else if (IsIdentified(item))
            {
                var result = item.GetInt32(obj_f.item_worth);
                if (result < 2)
                    result = 2;
                return result;
            }
            else
            {
                if (item.type == ObjectType.food)
                {
                    return 1500;
                }
                else if (item.type == ObjectType.scroll)
                {
                    return 1000;
                }
                else
                {
                    return 10000;
                }
            }
        }

        [TempleDllLocation(0x10064d20)]
        public int GetTotalCurrencyAmount(GameObjectBody obj)
        {
            switch (obj.type)
            {
                case ObjectType.container:
                    // Just sum up the container's content
                    var sum = 0;
                    foreach (var item in obj.EnumerateChildren())
                    {
                        sum += GetTotalCurrencyAmount(item);
                    }

                    return sum;
                case ObjectType.money:
                    var amount = obj.GetInt32(obj_f.money_quantity);
                    switch ((MoneyType) obj.GetInt32(obj_f.money_type))
                    {
                        case MoneyType.Copper:
                            return GameSystems.Party.GetCoinWorth(copperCoins: amount);
                        case MoneyType.Silver:
                            return GameSystems.Party.GetCoinWorth(silverCoins: amount);
                        case MoneyType.Gold:
                            return GameSystems.Party.GetCoinWorth(goldCoins: amount);
                        case MoneyType.Platinum:
                            return GameSystems.Party.GetCoinWorth(platinCoins: amount);
                        default:
                            return 0;
                    }

                case ObjectType.pc:
                case ObjectType.npc:
                    return GameSystems.Critter.GetMoney(obj);
                default:
                    return 0;
            }
        }

        [TempleDllLocation(0x10067100)]
        public bool IsIdentified(GameObjectBody item)
        {
            if (!item.IsItem())
            {
                return false;
            }

            var itemFlags = item.GetItemFlags();

            return !itemFlags.HasFlag(ItemFlag.IS_MAGICAL) || itemFlags.HasFlag(ItemFlag.IDENTIFIED);
        }

        [TempleDllLocation(0x1006cc30)]
        public GameObjectBody GiveItemByProto(GameObjectBody obj, ushort protoId)
        {
            var proto = GameSystems.Proto.GetProtoById(protoId);
            var item = GameSystems.MapObject.CreateObject(proto, obj.GetLocation());

            item.SetItemFlags(item.GetItemFlags() | ItemFlag.IDENTIFIED);
            SetItemParent(item, obj, ItemInsertFlag.Use_Max_Idx_200);

            return item;
        }

        [TempleDllLocation(0x1006b6c0)]
        public bool SetItemParent(GameObjectBody item, GameObjectBody receiver, ItemInsertFlag flags = default)
        {
            if (receiver.IsContainer())
            {
                return SetParentAdvanced(item, receiver, INVENTORY_IDX_UNDEFINED, ItemInsertFlag.Use_Max_Idx_200);
            }

            if (!receiver.IsCritter())
            {
                return false;
            }

            // Darley's Necklace special casing
            if (item.ProtoId == 6239)
            {
                return SetParentAdvanced(item, receiver, InvIdxNecklace, flags);
            }

            return SetParentAdvanced(item, receiver, INVENTORY_IDX_UNDEFINED, flags);
        }

        [TempleDllLocation(0x1006a810)]
        public bool SetParentAdvanced(GameObjectBody item, GameObjectBody parent, int invIdx, ItemInsertFlag flags)
        {
            var itemInsertLocation = INVENTORY_IDX_UNDEFINED;

            if (!item.IsItem())
            {
                Logger.Error("SetParentAdvanced call on non-item!");
                return false;
            }

            // if is already inventory item
            if (item.HasFlag(ObjectFlag.INVENTORY))
            {
                return false;
            }

            // run the object's san_get script
            if (GameSystems.Script.ExecuteObjectScript(parent, item, ObjScriptEvent.Get) == 0)
            {
                return false;
            }

            GameObjectBody itemAtSlot = null;
            var invIdxIsWornSlot = false;
            if (IsInvIdxWorn(invIdx))
            {
                invIdxIsWornSlot = true;
                itemAtSlot = GetItemAtInvIdx(parent, invIdx);
            }

            var insertionErrorCode = ItemInsertGetLocation(item, parent, ref itemInsertLocation, null, flags);

            // handle insertion error
            if (insertionErrorCode != ItemErrorCode.OK
                && (insertionErrorCode != ItemErrorCode.No_Room_For_Item || itemAtSlot != null || !invIdxIsWornSlot))
            {
                if (parent.type == ObjectType.npc)
                {
                    var errorText = GetItemErrorString(insertionErrorCode);
                    GameSystems.TextFloater.FloatLine(parent, TextFloaterCategory.Generic,
                        TextFloaterColor.White, errorText);
                }

                return false;
            }

            // on success
            GameSystems.MapObject.MakeItemParented(item, parent);
            if (invIdx == INVENTORY_IDX_UNDEFINED)
            {
                invIdx = itemInsertLocation;
            }
            else if (invIdxIsWornSlot)
            {
                if (itemAtSlot != null)
                {
                    invIdx = itemInsertLocation;
                }
            }
            else
            {
                if (!CheckInvIdxOrStackable(item, parent, invIdx))
                {
                    invIdx = itemInsertLocation;
                }
            }

            InsertAtLocation(item, parent, invIdx);

            RemoveDecayTimer(item);
            return true;
        }

        [TempleDllLocation(0x10066120)]
        public string GetItemErrorString(ItemErrorCode code)
        {
            if (code == ItemErrorCode.OK)
            {
                return null;
            }

            return _itemErrorCodeText[code];
        }

        [TempleDllLocation(0x1006A3A0)]
        private bool TransferTo(GameObjectBody item, GameObjectBody parent, int itemInsertLocation)
        {
            var currentParent = GetParent(item);
            if (currentParent == null)
                return SetParentAdvanced(item, parent, itemInsertLocation, 0);
            if (GameSystems.Script.ExecuteObjectScript(currentParent, parent, item, ObjScriptEvent.Transfer,
                    0) == 0)
            {
                return false;
            }

            var errorCode = IsItemNonTransferrable(item, parent);
            if (errorCode != ItemErrorCode.OK)
            {
                Logger.Debug($"Transfer of {item} from {currentParent} to {parent} failed: {errorCode}");
                return false;
            }

            GameObjectBody currentItemAtIdx = null;
            var isEquipmentSlot = false;
            if (itemInsertLocation != -1 & IsInvIdxWorn(itemInsertLocation))
            {
                isEquipmentSlot = true;
                currentItemAtIdx = GetItemAtInvIdx(parent, itemInsertLocation);
            }

            var v14 = INVENTORY_IDX_UNDEFINED;
            var v13 = ItemInsertGetLocation(item, parent, ref v14, null, default);
            if (v13 != ItemErrorCode.OK && (!(v13 == ItemErrorCode.No_Room_For_Item & currentItemAtIdx == null) ||
                                            !isEquipmentSlot))
            {
                Logger.Debug($"Transfer of {item} from {currentParent} to {parent} failed: {v13}");
                return false;
            }

            if (parent != currentParent)
            {
                item.SetItemFlag(ItemFlag.NO_TRANSFER, false);
            }

            Remove(item);

            if (itemInsertLocation != INVENTORY_IDX_UNDEFINED)
            {
                if (!isEquipmentSlot)
                {
                    if (CheckInvIdxOrStackable(item, parent, itemInsertLocation))
                    {
                        InsertAtLocation(item, parent, itemInsertLocation);
                    }
                    else
                    {
                        InsertAtLocation(item, parent, v14);
                    }
                }
                else
                {
                    if (currentItemAtIdx != null)
                    {
                        InsertAtLocation(item, parent, v14);
                    }
                    else
                    {
                        InsertAtLocation(item, parent, itemInsertLocation);
                    }
                }
            }
            else
            {
                InsertAtLocation(item, parent, v14);
            }

            RemoveDecayTimer(item);
            return true;
        }

        private const int ContainerRows = 96;
        private const int CritterRows = 12;
        private const int Columns = 10;

        [TempleDllLocation(0x10068e80)]
        private bool CheckInvIdxOrStackable(GameObjectBody item, GameObjectBody parent, int invIdx)
        {
            if (IsInvIdxWorn(invIdx))
            {
                return false;
            }

            if (!GameSystems.Object.GetInventoryFields(parent.type, out var invField, out var invCountField))
            {
                return false;
            }

            if (IsStackable(item))
            {
                var stackableItem = FindMatchingStackableItem(parent, item);
                if (stackableItem != null && stackableItem != item)
                {
                    return true;
                }
            }

            int rows = parent.IsContainer() ? ContainerRows : CritterRows;

            // TODO: How can the first condition ever be true???
            if (invIdx % Columns >= Columns || invIdx / Columns >= rows)
            {
                return false;
            }

            return GetItemAtInvIdx(parent, invIdx) == null;
        }

        [TempleDllLocation(0x10067640)]
        private void RemoveDecayTimer(GameObjectBody item)
        {
            GameSystems.TimeEvent.Remove(TimeEventType.ItemDecay, evt => evt.arg1.handle == item);
        }

        [TempleDllLocation(0x100694b0)]
        public void InsertAtLocation(GameObjectBody item, GameObjectBody receiver, int itemInsertLocation)
        {
            if (itemInsertLocation == -1)
            {
                Logger.Error("InsertAtLocation: Attempt to insert an item at location -1!");
            }

            var itemType = item.type;
            var isQuantity = GetQuantityField(item, out var qtyField);

            var mergeStackables = false;

            if (itemType == ObjectType.money)
            {
                if (receiver.IsPC())
                {
                    GameSystems.Party.GiveMoneyFromItem(item);
                    var loc = receiver.GetLocation();
                    GameSystems.MapObject.MoveItem(item, loc);
                    GameSystems.Script.ExecuteObjectScript(receiver, item, ObjScriptEvent.InsertItem);
                    GameSystems.Object.Destroy(item);
                    return;
                }

                if (receiver.IsNPC())
                {
                    var stackable = FindMatchingStackableItem(receiver, item);
                    if (stackable != null)
                    {
                        var itemQty = item.GetInt32(qtyField);
                        var stackQty = stackable.GetInt32(qtyField);
                        stackable.SetInt32(qtyField, itemQty + stackQty);
                        var loc = receiver.GetLocation();
                        GameSystems.MapObject.MoveItem(item, loc);
                        GameSystems.Script.ExecuteObjectScript(receiver, item, ObjScriptEvent.InsertItem);
                        GameSystems.Object.Destroy(item);
                        return;
                    }
                }
            }
            else if (itemType == ObjectType.key)
            {
                if (receiver.IsPC())
                {
                    var keyId = item.GetInt32(obj_f.key_key_id);
                    GameUiBridge.OnKeyReceived(keyId, GameSystems.TimeEvent.GameTime);
                    var loc = receiver.GetLocation();
                    GameSystems.MapObject.MoveItem(item, loc);
                    GameSystems.Script.ExecuteObjectScript(receiver, item, ObjScriptEvent.InsertItem);
                    GameSystems.Object.Destroy(item);
                    return;
                }
            }
            else if (itemType == ObjectType.ammo)
            {
                mergeStackables = true;
            }
            else if (!IsInvIdxWorn(itemInsertLocation))
            {
                mergeStackables = true;
            }

            if (mergeStackables && isQuantity)
            {
                var stackable = FindMatchingStackableItem(receiver, item);
                if (stackable != null)
                {
                    var itemQty = item.GetInt32(qtyField);
                    var stackQty = stackable.GetInt32(qtyField);
                    stackable.SetInt32(qtyField, itemQty + stackQty);
                    var loc = receiver.GetLocation();
                    GameSystems.MapObject.MoveItem(item, loc);
                    GameSystems.Anim.NotifySpeedRecalc(receiver);
                    GameSystems.Script.ExecuteObjectScript(receiver, item, ObjScriptEvent.InsertItem);
                    GameSystems.Object.Destroy(item);
                    return;
                }
            }

            // Set internal fields
            if (!GameSystems.Object.GetInventoryFields(receiver.type, out var invenField, out var invenNumField))
            {
                throw new ArgumentException("Trying to insert an item into a non-container!");
            }

            var invenCount = receiver.GetInt32(invenNumField);
            receiver.SetInt32(invenNumField, invenCount + 1);
            receiver.SetObject(invenField, invenCount, item);
            item.SetInt32(obj_f.item_inv_location, itemInsertLocation);
            item.SetObject(obj_f.item_parent, receiver);

            // Do updates and notifications
            if (IsInvIdxWorn(itemInsertLocation))
            {
                UpdateCritterLightFlags(item, receiver);
                GameSystems.Script.ExecuteObjectScript(receiver, item, ObjScriptEvent.WieldOn);
                GameSystems.Critter.UpdateModelEquipment(receiver);
            }

            if (receiver.IsNPC())
            {
                receiver.AiFlags |= AiFlag.CheckWield;
            }

            if (receiver.IsCritter())
            {
                GameSystems.D20.Status.initItemConditions(receiver);
                GameSystems.D20.D20SendSignal(receiver, D20DispatcherKey.SIG_Inventory_Update, item);
            }

            GameSystems.Anim.NotifySpeedRecalc(receiver);
            GameSystems.Script.ExecuteObjectScript(receiver, item, ObjScriptEvent.InsertItem);
        }

        /// <summary>
        /// Updates the critter's light flags based on the equipped item's light flags.
        /// </summary>
        [TempleDllLocation(0x10066260)]
        private void UpdateCritterLightFlags(GameObjectBody item, GameObjectBody receiver)
        {
            const ItemFlag lightMask = ItemFlag.LIGHT_XLARGE
                                       | ItemFlag.LIGHT_LARGE
                                       | ItemFlag.LIGHT_MEDIUM
                                       | ItemFlag.LIGHT_SMALL;

            if (item == null || (item.GetItemFlags() & lightMask) != 0)
            {
                ItemFlag activeLight = default;

                foreach (var slot in EquipSlots.Slots)
                {
                    var equipped = ItemWornAt(receiver, slot);
                    if (equipped != null)
                    {
                        activeLight |= equipped.GetItemFlags() & lightMask;
                    }
                }

                const CritterFlag critterLightMask = CritterFlag.LIGHT_XLARGE
                                                     | CritterFlag.LIGHT_LARGE
                                                     | CritterFlag.LIGHT_MEDIUM
                                                     | CritterFlag.LIGHT_SMALL;

                var critterFlags = receiver.GetCritterFlags() & ~critterLightMask;
                if (activeLight.HasFlag(ItemFlag.LIGHT_XLARGE))
                {
                    critterFlags |= CritterFlag.LIGHT_XLARGE;
                }
                else if (activeLight.HasFlag(ItemFlag.LIGHT_LARGE))
                {
                    critterFlags |= CritterFlag.LIGHT_LARGE;
                }
                else if (activeLight.HasFlag(ItemFlag.LIGHT_MEDIUM))
                {
                    critterFlags |= CritterFlag.LIGHT_MEDIUM;
                }
                else if (activeLight.HasFlag(ItemFlag.LIGHT_SMALL))
                {
                    critterFlags |= CritterFlag.LIGHT_SMALL;
                }

                receiver.SetCritterFlags(critterFlags);
            }
        }

        public static bool IsInvIdxWorn(int invIdx)
        {
            return invIdx >= INVENTORY_WORN_IDX_START && invIdx <= INVENTORY_WORN_IDX_END;
        }

        public int InvIdxForSlot(EquipSlot slot)
        {
            return (int) slot + INVENTORY_WORN_IDX_START;
        }

        [TempleDllLocation(0x10069000)]
        public ItemErrorCode ItemInsertGetLocation(GameObjectBody item, GameObjectBody receiver,
            ref int itemInsertLocation, GameObjectBody bag, ItemInsertFlag flags)
        {
            var hasLocationOutput = true;
            var invIdx = INVENTORY_IDX_UNDEFINED;

            if (GameSystems.D20.D20Query(receiver, D20DispatcherKey.QUE_Polymorphed))
            {
                return ItemErrorCode.Cannot_Use_While_Polymorphed;
            }

            var isUseWieldSlots = (flags & ItemInsertFlag.Use_Wield_Slots) != 0;

            if (receiver.IsCritter() && hasLocationOutput)
            {
                if (flags.HasFlag(ItemInsertFlag.Allow_Swap) || (!IsInvIdxWorn(itemInsertLocation) && !isUseWieldSlots))
                {
                    if (isUseWieldSlots)
                    {
                        foreach (var equipSlot in EquipSlots.Slots)
                        {
                            var itemWornAtSlot = ItemWornAt(receiver, equipSlot);
                            var itemFlagCheck = ItemCheckSlotAndWieldFlags(item, receiver, InvIdxForSlot(equipSlot)) ==
                                                ItemErrorCode.OK;
                            var slotIsOccupied =
                                itemInsertLocation == INVENTORY_IDX_UNDEFINED
                                && isUseWieldSlots
                                && itemWornAtSlot != null
                                && itemFlagCheck;

                            if (slotIsOccupied)
                            {
                                if (ItemWornAt(receiver, equipSlot) != item)
                                {
                                    itemInsertLocation = InvIdxForSlot(equipSlot);
                                    return ItemErrorCode.Wield_Slot_Occupied;
                                }
                            }
                            else if (itemWornAtSlot == null
                                     && itemFlagCheck
                                     && itemWornAtSlot != item)
                            {
                                itemInsertLocation = InvIdxForSlot(equipSlot);
                                return ItemErrorCode.OK;
                            }
                        }
                    }
                }
                else
                {
                    var shouldTrySlots = true;
                    var result = ItemErrorCode.Wrong_Type_For_Slot;
                    if (itemInsertLocation != INVENTORY_IDX_UNDEFINED)
                    {
                        result = CheckTransferToWieldSlot(item, itemInsertLocation, receiver);
                        if (result == ItemErrorCode.OK)
                        {
                            shouldTrySlots = false;
                            invIdx = itemInsertLocation;
                        }
                        else
                        {
                            if (result != ItemErrorCode.Wrong_Type_For_Slot &&
                                result != ItemErrorCode.Wield_Slot_Occupied)
                                return result;
                        }
                    }

                    if (shouldTrySlots)
                    {
                        var found = false;
                        for (var i = 0; i < INVENTORY_WORN_IDX_COUNT; i++)
                        {
                            var equipSlot = (EquipSlot) i;
                            if (ItemWornAt(receiver, equipSlot) == null)
                            {
                                result = CheckTransferToWieldSlot(item, i + INVENTORY_WORN_IDX_START, receiver);
                                if (result == ItemErrorCode.OK)
                                {
                                    found = true;
                                    invIdx = i + INVENTORY_WORN_IDX_START;
                                    break;
                                }
                            }
                        }

                        if (!found)
                            return result;
                    }
                }
            }

            // handling for stackable items, and money for PCs
            // this is vanilla; I suppose it doesn't matter, since it'll stack it anyway in the calling
            // function (but I wonder if it isn't better to return the stackable item's index?)
            if (IsStackable(item) && FindMatchingStackableItem(receiver, item) != null
                || item.type == ObjectType.money && receiver.IsPC())
            {
                if (hasLocationOutput)
                    itemInsertLocation = 0;
                return ItemErrorCode.OK;
            }

            // if already found it in the above section
            if (invIdx != INVENTORY_IDX_UNDEFINED)
            {
                if (hasLocationOutput)
                    itemInsertLocation = invIdx;
                return ItemErrorCode.OK;
            }

            var maxSlot = 120;

            // already provided with designated location
            if (itemInsertLocation != INVENTORY_IDX_UNDEFINED && IsInvIdxWorn(itemInsertLocation))
            {
                invIdx = itemInsertLocation;
            }
            // Containers
            else if (!receiver.IsCritter())
            {
                if (!receiver.IsContainer())
                    return ItemErrorCode.No_Room_For_Item;

                if (itemInsertLocation != INVENTORY_IDX_UNDEFINED &&
                    GetItemAtInvIdx(receiver, itemInsertLocation) == null)
                {
                    invIdx = itemInsertLocation;
                    if (hasLocationOutput)
                    {
                        itemInsertLocation = invIdx;
                    }

                    return ItemErrorCode.OK;
                }

                maxSlot = 120;
                if (flags.HasFlag(ItemInsertFlag.Use_Max_Idx_200)) // fix - vanilla lacked this line
                    maxSlot = INVENTORY_WORN_IDX_START;
                invIdx = FindEmptyInvIdx(item, receiver, 0, maxSlot);
            }
            // Critters
            else if (itemInsertLocation != INVENTORY_IDX_UNDEFINED &&
                     GetItemAtInvIdx(receiver, itemInsertLocation) == null)
            {
                invIdx = itemInsertLocation;
            }
            else
            {
                if (flags.HasFlag(ItemInsertFlag.Use_Max_Idx_200))
                {
                    maxSlot = INVENTORY_WORN_IDX_START;
                    invIdx = FindEmptyInvIdx(item, receiver, 0, maxSlot);
                }
                else if ((flags.HasFlag(ItemInsertFlag.Use_Bags)) && BagFindLast(receiver) != null)
                {
                    var receiverBag = BagFindLast(receiver);
                    if (receiverBag != null)
                    {
                        var bagMaxIdx = BagGetContentMaxIdx(receiver, receiverBag);
                        invIdx = FindEmptyInvIdx(item, receiver, 0, bagMaxIdx); // todo BUG!
                    }
                }
                else if (flags.HasFlag(ItemInsertFlag.Allow_Swap))
                {
                    // bug?
                    if (itemInsertLocation != INVENTORY_IDX_UNDEFINED)
                        return ItemErrorCode.Wield_Slot_Occupied;
                    return ItemErrorCode.No_Room_For_Item;
                }
                else if (bag != null)
                {
                    var bagMaxIdx = BagGetContentMaxIdx(receiver, bag);
                    var bagBaseIdx = BagGetContentStartIdx(receiver, bag);
                    invIdx = FindEmptyInvIdx(item, receiver, bagBaseIdx, bagMaxIdx);
                }
                else
                {
                    maxSlot = CRITTER_INVENTORY_SLOT_COUNT;
                    invIdx = FindEmptyInvIdx(item, receiver, 0, maxSlot);
                }
            }

            if (invIdx != INVENTORY_IDX_UNDEFINED)
            {
                if (hasLocationOutput)
                {
                    itemInsertLocation = invIdx;
                }

                return ItemErrorCode.OK;
            }

            return ItemErrorCode.No_Room_For_Item;
        }

        private const int InvIdxHelmet = 200;
        private const int InvIdxNecklace = 201;
        private const int InvIdxGloves = 202;
        private const int InvIdxWeaponPrimary = 203;
        private const int InvIdxWeaponSecondary = 204;
        private const int InvIdxArmor = 205;
        private const int InvIdxRingPrimary = 206;
        private const int InvIdxRingSecondary = 207;
        private const int InvIdxBoots = 208;
        private const int InvIdxAmmo = 209;
        private const int InvIdxCloak = 210;
        private const int InvIdxShield = 211;
        private const int InvIdxRobes = 212;
        private const int InvIdxBracers = 213;
        private const int InvIdxBardicItem = 214;
        private const int InvIdxLockpicks = 215;

        [TempleDllLocation(0x10067F90)]
        private ItemErrorCode CheckTransferToWieldSlot(GameObjectBody item, int itemInsertLocation,
            GameObjectBody receiver)
        {
            // These might be old Arkanum polymorph spell flags not used in ToEE
            var spellFlags = receiver.GetSpellFlags();
            if ((spellFlags & (SpellFlag.POLYMORPHED | SpellFlag.BODY_OF_WATER | SpellFlag.BODY_OF_FIRE |
                               SpellFlag.BODY_OF_EARTH | SpellFlag.BODY_OF_AIR)) != 0)
            {
                return ItemErrorCode.Has_No_Art;
            }

            if (GameSystems.Critter.IsAnimal(receiver)
                || receiver.IsNPC() && receiver.GetNPCFlags().HasFlag(NpcFlag.NO_EQUIP))
            {
                return ItemErrorCode.ItemCannotBeUsed;
            }

            var wieldFlagResult = ItemCheckSlotAndWieldFlags(item, receiver, itemInsertLocation);
            if (wieldFlagResult != ItemErrorCode.OK)
            {
                return wieldFlagResult;
            }

            if (IsInvIdxWorn(itemInsertLocation))
            {
                var wornItem = ItemWornAt(receiver, SlotByInvIdx(itemInsertLocation));
                if (wornItem != null)
                {
                    return ItemErrorCode.Wield_Slot_Occupied;
                }
            }

            switch (itemInsertLocation)
            {
                case InvIdxWeaponPrimary:
                    return CheckHandOccupancy(item, receiver);

                case InvIdxWeaponSecondary:
                    return CheckHandOccupancy2(item, receiver);

                case InvIdxArmor:
                    if (IsIncompatibleWithDruid(item, receiver))
                    {
                        return ItemErrorCode.ItemCannotBeUsed;
                    }

                    return ItemErrorCode.OK;

                case InvIdxShield:
                    if (item.GetItemWearFlags().HasFlag(ItemWearFlag.BUCKLER))
                    {
                        return CheckHandOccupancyForBuckler(item, receiver);
                    }
                    else
                    {
                        return CheckHandOccupancyForShield(item, receiver);
                    }

                default:
                    return ItemErrorCode.OK;
            }
        }

        private ItemErrorCode CheckHandOccupancyForBuckler(GameObjectBody item, GameObjectBody receiver)
        {
            var offHand = ItemWornAt(receiver, EquipSlot.WeaponSecondary);
            var shield = ItemWornAt(receiver, EquipSlot.Shield);
            if (shield != null)
            {
                return ItemErrorCode.Wield_Slot_Occupied;
            }

            if (offHand != item && offHand != null && offHand.type == ObjectType.armor)
            {
                return ItemErrorCode.No_Free_Hand;
            }

            return ItemErrorCode.OK;
        }

        private ItemErrorCode CheckHandOccupancyForShield(GameObjectBody item, GameObjectBody receiver)
        {
            var secondarya = ItemWornAt(receiver, EquipSlot.WeaponPrimary);

            if (secondarya != null && secondarya != item)
            {
                if (secondarya.type == ObjectType.armor)
                {
                    if (secondarya.GetArmorFlags().IsShield())
                        return ItemErrorCode.No_Free_Hand;
                }

                if (GetWieldType(receiver, secondarya) >= 2
                    || secondarya.GetItemWearFlags().HasFlag(ItemWearFlag.TWOHANDED_REQUIRED))
                {
                    return ItemErrorCode.No_Free_Hand;
                }
            }

            var offHand = ItemWornAt(receiver, EquipSlot.WeaponSecondary);
            if (offHand != null)
            {
                if (offHand == item)
                {
                    return ItemErrorCode.OK;
                }

                if (secondarya != null)
                {
                    if (offHand.type == ObjectType.armor)
                    {
                        return ItemErrorCode.No_Free_Hand;
                    }

                    return ItemErrorCode.No_Free_Hand;
                }
            }

            if (offHand != item && offHand != null && offHand.type == ObjectType.armor &&
                item.type == ObjectType.armor)
            {
                return ItemErrorCode.No_Free_Hand;
            }

            return ItemErrorCode.OK;
        }

        private ItemErrorCode CheckHandOccupancy2(GameObjectBody item, GameObjectBody receiver)
        {
            GameObjectBody mainHand = null;
            GameObjectBody offHand = null;
            GameObjectBody shield = null;
            if (!GameSystems.D20.D20Query(receiver, D20DispatcherKey.QUE_Polymorphed))
            {
                mainHand = ItemWornAt(receiver, EquipSlot.WeaponPrimary);
                offHand = ItemWornAt(receiver, EquipSlot.WeaponSecondary);
                shield = ItemWornAt(receiver, EquipSlot.Shield);

                if (mainHand != null && mainHand != item)
                {
                    if (GetWieldType(receiver, item) >= 2
                        || GetWieldType(receiver, mainHand) >= 2
                        || item.GetItemWearFlags().HasFlag(ItemWearFlag.TWOHANDED_REQUIRED))
                    {
                        return ItemErrorCode.No_Free_Hand;
                    }

                    if (shield != null && shield != item)
                    {
                        if (!shield.GetItemWearFlags().HasFlag(ItemWearFlag.BUCKLER))
                            return ItemErrorCode.No_Free_Hand;
                    }

                    if (mainHand.type == ObjectType.armor)
                    {
                        if (!mainHand.GetArmorFlags().IsShield())
                            return ItemErrorCode.Wrong_Type_For_Slot;
                    }
                    else if (mainHand.type == ObjectType.weapon
                             && mainHand.GetItemWearFlags().HasFlag(ItemWearFlag.TWOHANDED_REQUIRED))
                    {
                        return ItemErrorCode.No_Free_Hand;
                    }
                }
            }

            if (GetWieldType(receiver, item) == 3)
                return ItemErrorCode.Item_Too_Large;

            LABEL_97:
            if (shield != null)
            {
                if (!shield.GetItemWearFlags().HasFlag(ItemWearFlag.BUCKLER))
                {
                    if (shield != item)
                        return ItemErrorCode.No_Free_Hand;
                }

                if (item.type == ObjectType.armor && shield != item)
                    return ItemErrorCode.No_Free_Hand;
            }

            var animId = GameSystems.Critter.GetWeaponAnim(receiver, item, offHand, WeaponAnim.Idle);
            if (GameSystems.MapObject.HasAnim(receiver, animId))
                return ItemErrorCode.OK;
            return ItemErrorCode.Has_No_Art;
        }

        private ItemErrorCode CheckHandOccupancy(GameObjectBody item, GameObjectBody receiver)
        {
            GameObjectBody offHand = null;
            GameObjectBody shield = null;
            if (!GameSystems.D20.D20Query(receiver, D20DispatcherKey.QUE_Polymorphed))
            {
                offHand = ItemWornAt(receiver, EquipSlot.WeaponSecondary);
                shield = ItemWornAt(receiver, EquipSlot.Shield);
            }

            if (offHand == null
                || offHand == item
                || offHand.GetItemWearFlags().HasFlag(ItemWearFlag.BUCKLER))
            {
                if (GetWieldType(receiver, item) >= 2
                    && offHand != null
                    && offHand.type == ObjectType.armor)
                {
                    return ItemErrorCode.Wield_Slot_Occupied;
                }

                if (GetWieldType(receiver, item) == 3)
                    return ItemErrorCode.Item_Too_Large;
            }
            else
            {
                // The other hand is occupied

                if (GetWieldType(receiver, item) >= 2
                    || item.GetItemWearFlags().HasFlag(ItemWearFlag.TWOHANDED_REQUIRED))
                {
                    // The off-hand is occupied but wielding the item requires two hands
                    return ItemErrorCode.No_Free_Hand;
                }

                if (offHand.type == ObjectType.armor)
                {
                    if (!offHand.GetArmorFlags().IsShield())
                    {
                        // There is a non-shield (what??) in the off-hand
                        return ItemErrorCode.Wrong_Type_For_Slot;
                    }
                }
                else if (offHand.type == ObjectType.weapon
                         && offHand.GetItemWearFlags().HasFlag(ItemWearFlag.TWOHANDED_REQUIRED))
                {
                    // There is a two-handed weapon in the off-hand
                    return ItemErrorCode.No_Free_Hand;
                }
            }

            if (shield != null)
            {
                if (!shield.GetItemWearFlags().HasFlag(ItemWearFlag.BUCKLER)
                    && (GetWieldType(receiver, item) >= 2
                        || item.GetItemWearFlags().HasFlag(ItemWearFlag.TWOHANDED_REQUIRED)
                        || offHand != item && offHand != null))
                {
                    return ItemErrorCode.No_Free_Hand;
                }
            }

            var animId = GameSystems.Critter.GetWeaponAnim(receiver, item, offHand, WeaponAnim.Idle);
            if (GameSystems.MapObject.HasAnim(receiver, animId))
                return ItemErrorCode.OK;
            return ItemErrorCode.Has_No_Art;
        }

        [TempleDllLocation(0x10067680)]
        private ItemErrorCode ItemCheckSlotAndWieldFlags(GameObjectBody item, GameObjectBody receiver, in int invIdx)
        {
            var wearFlags = item.GetItemWearFlags();
            switch (invIdx)
            {
                case InvIdxHelmet:
                    if (!wearFlags.HasFlag(ItemWearFlag.HELMET))
                        return ItemErrorCode.Wrong_Type_For_Slot;
                    return ItemErrorCode.OK;
                case InvIdxNecklace:
                    if (!wearFlags.HasFlag(ItemWearFlag.NECKLACE))
                        return ItemErrorCode.Wrong_Type_For_Slot;
                    return ItemErrorCode.OK;
                case InvIdxGloves:
                    if (!wearFlags.HasFlag(ItemWearFlag.GLOVES))
                        return ItemErrorCode.Wrong_Type_For_Slot;
                    return ItemErrorCode.OK;
                case InvIdxWeaponSecondary:
                    if (wearFlags.HasFlag(ItemWearFlag.TWOHANDED_REQUIRED) ||
                        (item.type == ObjectType.weapon && GetWieldType(receiver, item) >= 2))
                    {
                        return ItemErrorCode.No_Free_Hand;
                    }

                    if (item.type != ObjectType.weapon)
                        return ItemErrorCode.Wrong_Type_For_Slot;
                    return ItemErrorCode.OK;
                case InvIdxWeaponPrimary:
                    if (item.type != ObjectType.weapon)
                        return ItemErrorCode.Wrong_Type_For_Slot;
                    return ItemErrorCode.OK;
                case InvIdxArmor:
                    if (!wearFlags.HasFlag(ItemWearFlag.ARMOR))
                        return ItemErrorCode.Wrong_Type_For_Slot;
                    if (item.type != ObjectType.armor)
                        return ItemErrorCode.Wrong_Type_For_Slot;
                    if (item.GetArmorFlags().IsShield())
                        return ItemErrorCode.Wrong_Type_For_Slot;
                    return ItemErrorCode.OK;
                case InvIdxRingPrimary:
                case InvIdxRingSecondary:
                    if (!wearFlags.HasFlag(ItemWearFlag.RING))
                        return ItemErrorCode.Wrong_Type_For_Slot;
                    return ItemErrorCode.OK;
                case InvIdxBoots:
                    if (!wearFlags.HasFlag(ItemWearFlag.BOOTS))
                        return ItemErrorCode.Wrong_Type_For_Slot;
                    return ItemErrorCode.OK;
                case InvIdxAmmo:
                    if (!wearFlags.HasFlag(ItemWearFlag.AMMO))
                        return ItemErrorCode.Wrong_Type_For_Slot;
                    return ItemErrorCode.OK;
                case InvIdxCloak:
                    if (!wearFlags.HasFlag(ItemWearFlag.CLOAK))
                        return ItemErrorCode.Wrong_Type_For_Slot;
                    return ItemErrorCode.OK;
                case InvIdxShield:
                    if (item.type != ObjectType.armor)
                        return ItemErrorCode.Wrong_Type_For_Slot;
                    if (!item.GetArmorFlags().IsShield())
                        return ItemErrorCode.Wrong_Type_For_Slot;
                    return ItemErrorCode.OK;
                case InvIdxRobes:
                    if (!wearFlags.HasFlag(ItemWearFlag.ROBES))
                        return ItemErrorCode.Wrong_Type_For_Slot;
                    return ItemErrorCode.OK;
                case InvIdxBracers:
                    if (!wearFlags.HasFlag(ItemWearFlag.BRACERS))
                        return ItemErrorCode.Wrong_Type_For_Slot;
                    return ItemErrorCode.OK;
                case InvIdxBardicItem:
                    if (!wearFlags.HasFlag(ItemWearFlag.BARDIC_ITEM))
                        return ItemErrorCode.Wrong_Type_For_Slot;
                    return ItemErrorCode.OK;
                case InvIdxLockpicks:
                    if (!wearFlags.HasFlag(ItemWearFlag.LOCKPICKS))
                        return ItemErrorCode.Wrong_Type_For_Slot;
                    return ItemErrorCode.OK;
                default:
                    return ItemErrorCode.Wrong_Type_For_Slot;
            }
        }

        public EquipSlot SlotByInvIdx(int invIdx)
        {
            if (IsInvIdxWorn(invIdx))
            {
                return (EquipSlot) (invIdx - 200);
            }

            throw new ArgumentOutOfRangeException();
        }

        public bool TryGetSlotByInvIdx(int invIdx, out EquipSlot slot)
        {
            if (IsInvIdxWorn(invIdx))
            {
                slot = (EquipSlot) (invIdx - INVENTORY_WORN_IDX_START);
                return true;
            }

            slot = default;
            return false;
        }

        [TempleDllLocation(0x10065fa0)]
        private int FindEmptyInvIdx(GameObjectBody item, GameObjectBody parent, int idxMin, int idxMax)
        {
            for (var i = idxMin; i < idxMax; i++)
            {
                if (GetItemAtInvIdx(parent, i) == null)
                {
                    return i;
                }
            }

            return INVENTORY_IDX_UNDEFINED;
        }

        [TempleDllLocation(0x10067DF0)]
        public GameObjectBody FindMatchingStackableItem(GameObjectBody receiver, GameObjectBody item)
        {
            if (!IsIdentified(item))
            {
                // if not identified - does not stack
                return null;
            }

            var itemProto = item.ProtoId;
            if (itemProto == 12000)
            {
                // generic item proto
                return null;
            }

            // cycle thru inventory
            foreach (var invenItem in receiver.EnumerateChildren())
            {
                // ensure not same item handle
                if (item == invenItem)
                {
                    continue;
                }

                // ensure same proto ID
                if (invenItem.ProtoId != itemProto)
                {
                    continue;
                }

                // ensure is identified
                if (!IsIdentified(invenItem))
                {
                    continue;
                }

                // if item worn - ensure is ammo
                var invenItemLoc = invenItem.GetInt32(obj_f.item_inv_location);
                if (IsInvIdxWorn(invenItemLoc))
                {
                    if (invenItem.type != ObjectType.ammo)
                    {
                        continue;
                    }
                }

                if (item.type == ObjectType.scroll || item.type == ObjectType.food)
                {
                    // ensure potions/scrolls of different levels / schools do not stack
                    var itemSpell = item.GetSpell(obj_f.item_spell_idx, 0);
                    var invenItemSpell = invenItem.GetSpell(obj_f.item_spell_idx, 0);
                    if (itemSpell.spellLevel != invenItemSpell.spellLevel
                        || (item.type == ObjectType.scroll
                            && GameSystems.Spell.IsArcaneSpellClass(itemSpell.classCode)
                            != GameSystems.Spell.IsArcaneSpellClass(invenItemSpell.classCode)))
                        continue;
                }

                return invenItem;
            }

            return null;
        }

        [TempleDllLocation(0x100679c0)]
        private GameObjectBody BagFindLast(GameObjectBody parent)
        {
            for (var i = INVENTORY_BAG_IDX_END; i >= INVENTORY_BAG_IDX_START; i--)
            {
                var res = GetItemAtInvIdx(parent, i);
                if (res != null)
                {
                    return res;
                }
            }

            return null;
        }

        [TempleDllLocation(0x10066a50)]
        private int BagFindInvenIdx(GameObjectBody parent, GameObjectBody receiverBag)
        {
            for (var i = INVENTORY_BAG_IDX_START; i <= INVENTORY_BAG_IDX_END; i++)
            {
                if (GetItemAtInvIdx(parent, i) == receiverBag)
                    return i;
            }

            return INVENTORY_IDX_UNDEFINED;
        }

        [TempleDllLocation(0x100698b0)]
        public bool BagIsEmpty(GameObjectBody critter, GameObjectBody bag)
        {
            return GetBagItemsCount(critter, bag) == 0;
        }

        [TempleDllLocation(0x10067aa0)]
        public int GetBagItemsCount(GameObjectBody critter, GameObjectBody bag)
        {
            var count = 0;
            var startIdx = BagGetContentStartIdx(critter, bag);
            var maxIdx = BagGetContentMaxIdx(critter, bag);
            for (; startIdx < maxIdx; ++startIdx)
            {
                if (GetItemAtInvIdx(critter, startIdx) != null)
                {
                    ++count;
                }
            }

            return count;
        }

        [TempleDllLocation(0x100679f0)]
        private int BagGetContentStartIdx(GameObjectBody parent, GameObjectBody receiverBag)
        {
            var bagIdx = BagFindInvenIdx(parent, receiverBag);
            return 24 * (bagIdx - INVENTORY_BAG_IDX_START);
        }

        [TempleDllLocation(0x10067a20)]
        private int BagGetContentMaxIdx(GameObjectBody parent, GameObjectBody receiverBag)
        {
            var bagContentStartIdx = BagGetContentStartIdx(parent, receiverBag);

            var bagSize = receiverBag.GetInt32(obj_f.bag_size);

            var bagRows = 0;
            switch (bagSize)
            {
                case 1:
                    bagRows = 4;
                    break;
                case 2:
                    bagRows = 2;
                    break;
                default:
                    bagRows = 0;
                    break;
            }

            if (bagSize == 1)
            {
                return bagContentStartIdx + 6 * bagRows;
            }
            else if (bagSize == 2)
            {
                return bagContentStartIdx + 4 * bagRows;
            }

            return bagContentStartIdx;
        }

        [TempleDllLocation(0x10066010)]
        public ItemErrorCode IsItemNonTransferrable(GameObjectBody item, GameObjectBody receiver)
        {
            var parent = GetParent(item);
            var itemFlags = item.GetItemFlags();
            if (itemFlags.HasFlag(ItemFlag.NO_DROP)
                && (receiver == null || parent == null || item.ProtoId == 6239 || receiver != parent))
            {
                return ItemErrorCode.Item_Cannot_Be_Dropped;
            }

            if (itemFlags.HasFlag(ItemFlag.NO_TRANSFER)
                && receiver != parent && parent != null && !GameSystems.Critter.IsDeadNullDestroyed(parent))
            {
                return ItemErrorCode.NPC_Will_Not_Drop;
            }

            if (itemFlags.HasFlag(ItemFlag.NO_TRANSFER_SPECIAL) && receiver != parent)
            {
                return ItemErrorCode.Item_Cannot_Be_Dropped;
            }

            return ItemErrorCode.OK;
        }

        [TempleDllLocation(0x10066430)]
        public bool IsIncompatibleWithDruid(GameObjectBody item, GameObjectBody wearer)
        {
            if (GameSystems.Stat.StatLevelGet(wearer, Stat.level_druid) >= 1 && item.GetMaterial() == Material.metal)
            {
                var wearFlags = item.GetItemWearFlags();
                if (wearFlags.HasFlag(ItemWearFlag.ARMOR)
                    || wearFlags.HasFlag(ItemWearFlag.BUCKLER)
                    || item.GetArmorFlags().IsShield())
                {
                    return true;
                }
            }

            return false;
        }

        [TempleDllLocation(0x10AA84BC)]
        private bool _workingOnJunkpile;

        [TempleDllLocation(0x10069A20)]
        private bool JunkpileOnRemoveItem(GameObjectBody parent)
        {
            var result = false;
            if (_workingOnJunkpile)
            {
                return false;
            }

            _workingOnJunkpile = true;
            if (parent.ProtoId == 1000)
            {
                var itemCount = parent.GetInt32(obj_f.container_inventory_num);
                if (itemCount == 0)
                {
                    GameSystems.Object.Destroy(parent);
                    result = true;
                }
                else if (itemCount == 1)
                {
                    var lastItem = parent.GetObject(obj_f.container_inventory_list_idx, 0);
                    Remove(lastItem);
                    var worldLocation = parent.GetLocation();
                    GameSystems.Object.Destroy(parent);
                    MoveItemClearNoTransfer(lastItem, worldLocation);
                    result = true;
                }
            }

            _workingOnJunkpile = false;
            return result;
        }

        [TempleDllLocation(0x10069870)]
        public void MoveItemClearNoTransfer(GameObjectBody item, locXY location)
        {
            item.SetItemFlag(ItemFlag.NO_TRANSFER, false);
            GameSystems.MapObject.MoveItem(item, location);
        }

        [TempleDllLocation(0x100669a0)]
        public int GetWeaponSlotsIndex(GameObjectBody pc)
        {
            return pc.GetInt32(obj_f.pc_weaponslots_idx, 0);
        }

        public bool IsSlotPartOfWeaponSet(EquipSlot slot)
        {
            return slot == EquipSlot.WeaponPrimary
                   || slot == EquipSlot.WeaponSecondary
                   || slot == EquipSlot.Ammo
                   || slot == EquipSlot.Shield;
        }

        [TempleDllLocation(0x10069970)]
        public int GetAppraisedWorth(GameObjectBody item, GameObjectBody appraise, GameObjectBody seller, int a4 = 0)
        {
            // TODO: a4 was always zero
            Stub.TODO();
            return 0;
        }

        [TempleDllLocation(0x10064e20)]
        public void SplitMoney(int money, bool usePlat, out int plat,
            bool useGold, out int gold,
            bool useSilver, out int silver,
            out int copper)
        {
            if (usePlat)
            {
                plat = money / 1000;
                money = money % 1000;
            }
            else
            {
                plat = 0;
            }

            if (useGold)
            {
                gold = money / 100;
                money %= 100;
            }
            else
            {
                gold = 0;
            }

            if (useSilver)
            {
                silver = money / 10;
                money %= 10;
            }
            else
            {
                silver = 0;
            }

            copper = money;
        }

        public bool IsEquipped(GameObjectBody item)
        {
            var parent = GameSystems.Item.GetParent(item);
            if (parent == null)
            {
                return false;
            }

            var invLoc = item.GetInt32(obj_f.item_inv_location);
            return (invLoc >= INVENTORY_WORN_IDX_START && invLoc <= INVENTORY_BAG_IDX_END);
        }

        [TempleDllLocation(0x1007d190)]
        public bool ItemCanBePickpocketed(GameObjectBody item)
        {
            if (IsEquipped(item) || GetItemWeight(item) > 1)
            {
                return false;
            }

            return !item.GetItemFlags().HasFlag(ItemFlag.NO_PICKPOCKET);
        }

        [TempleDllLocation(0x10064a50)]
        public bool HasKey(GameObjectBody critter, int keyId)
        {
            if (GameUiBridge.IsKeyAcquired(keyId))
            {
                return true;
            }

            var leader = GameSystems.Critter.GetLeaderRecursive(critter);
            if (leader == null)
            {
                leader = GameSystems.Critter.GetLeader(critter) ?? critter;
            }

            if (HasKeyInInventory(leader, keyId))
            {
                return true;
            }

            foreach (var follower in leader.EnumerateFollowers(true))
            {
                if (HasKeyInInventory(follower, keyId))
                {
                    return true;
                }
            }

            return false;
        }

        [TempleDllLocation(0x100649b0)]
        private bool HasKeyInInventory(GameObjectBody obj, int keyId)
        {
            if (keyId == 0)
            {
                return false;
            }

            foreach (var child in obj.EnumerateChildren())
            {
                if (child.type == ObjectType.key)
                {
                    var itemKeyId = child.GetInt32(obj_f.key_key_id);
                    if (keyId == 1 || itemKeyId == 1 || itemKeyId == keyId)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        [TempleDllLocation(0x10066e90)]
        public void UseOnObject(GameObjectBody user, GameObjectBody item, GameObjectBody target)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10066fe0)]
        public void UseOnLocation(GameObjectBody critter, GameObjectBody item, LocAndOffsets location)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10065c30)]
        public int GetReachWithWeapon(GameObjectBody weapon, GameObjectBody critter)
        {
            if (weapon != null && weapon.type == ObjectType.weapon)
            {
                var v2 = weapon.WeaponFlags;
                if (v2.HasFlag(WeaponFlag.RANGED_WEAPON))
                {
                    return weapon.GetInt32(obj_f.weapon_range);
                }
                else
                {
                    var critterReach = critter.GetInt32(obj_f.critter_reach);
                    return critterReach + weapon.GetInt32(obj_f.weapon_range);
                }
            }
            else
            {
                return critter.GetInt32(obj_f.critter_reach);
            }
        }

        [TempleDllLocation(0x100643a0)]
        public bool IsThrownWeaponProjectile(GameObjectBody item)
        {
            switch (item.ProtoId)
            {
                case 3005:
                case 3006:
                case 3011:
                case 3012:
                case 3014:
                case 3015:
                case 3016:
                case 3018:
                    return true;
                default:
                    return false;
            }
        }

        [TempleDllLocation(0x100648b0)]
        public void ItemSpellChargeConsume(GameObjectBody item, int chargesUsedUp = 1)
        {
            var spellCharges = item.GetInt32(obj_f.item_spell_charges_idx);
            if (spellCharges == -1)
            {
                return;
            }

            // stacked items (scrolls, potions)
            var itemQty = item.GetQuantity();
            if (itemQty > 1)
            {
                item.SetQuantity(itemQty - 1);
                return;
            }
            else if (itemQty == 1)
            {
                GameSystems.Object.Destroy(item);
                return;
            }

            // items with charges
            item.SetInt32(obj_f.item_spell_charges_idx, spellCharges - chargesUsedUp);

            var itemFlags = item.GetItemFlags();
            if (!itemFlags.HasFlag(ItemFlag.EXPIRES_AFTER_USE))
            {
                return;
            }

            if (spellCharges - chargesUsedUp <= 0)
            {
                GameSystems.Object.Destroy(item);
            }
        }

        [TempleDllLocation(0x10064b40)]
        public bool UsesWandAnim(GameObjectBody item)
        {
            return item != null
                   && item.type.IsEquipment()
                   && item.GetItemFlags().HasFlag(ItemFlag.USES_WAND_ANIM);
        }

        [TempleDllLocation(0x100659e0)]
        public bool IsWieldingUnloadedCrossbow(GameObjectBody critter)
        {
            if (GameSystems.D20.D20Query(critter, D20DispatcherKey.QUE_Polymorphed))
            {
                return false;
            }

            var weapon = ItemWornAt(critter, EquipSlot.WeaponPrimary);

            if (weapon == null)
            {
                return false;
            }

            if (!IsCrossbow(weapon))
            {
                return false;
            }

            return !weapon.WeaponFlags.HasFlag(WeaponFlag.WEAPON_LOADED);
        }

        [TempleDllLocation(0x10065780)]
        public bool IsCrossbow(GameObjectBody weapon)
        {
            return weapon.type == ObjectType.weapon && GameSystems.Weapon.IsCrossbow(weapon.GetWeaponType());
        }

        [TempleDllLocation(0x1008f330)]
        public bool IsThrowingWeapon(GameObjectBody weapon)
        {
            return weapon.type == ObjectType.weapon && GameSystems.Weapon.IsThrowingWeapon(weapon.GetWeaponType());
        }

        public bool IsRangedWeapon(GameObjectBody weapon)
        {
            return weapon.type == ObjectType.weapon && weapon.WeaponFlags.HasFlag(WeaponFlag.RANGED_WEAPON);
        }

        public bool IsTripWeapon(GameObjectBody weapon)
        {
            return weapon.type == ObjectType.weapon && GameSystems.Weapon.IsTripWeapon(weapon.GetWeaponType());
        }

        [TempleDllLocation(0x10065760)]
        public int GetWeaponProjectileProto(GameObjectBody weapon)
        {
            return weapon.GetWeaponAmmoType().GetProjectileProtoId();
        }

        [TempleDllLocation(0x10066b00)]
        public GameObjectBody SplitObjectFromStack(GameObjectBody itemStack, LocAndOffsets placeAt)
        {
            var canSplit = IsStackable(itemStack);

            if (!canSplit || itemStack.GetQuantity() <= 1)
            {
                return itemStack;
            }

            var handleOut = GameSystems.MapObject.CreateObject(itemStack.GetProtoObj(), placeAt);
            handleOut.SetQuantity(1); // Seems unnecessary since it's not cloned
            itemStack.ReduceQuantity(1);
            return handleOut;
        }

        /// <summary>
        /// TODO: Weird function. It gives the item to the receiver and finds a matching stack.
        /// Why not merge it directly?
        /// </summary>
        [TempleDllLocation(0x1006cb10)]
        public GameObjectBody GiveItemAndFindMatchingStack(GameObjectBody item, GameObjectBody receiver)
        {
            if (!GameSystems.Item.SetItemParent(item, receiver))
            {
                return item;
            }

            if (!IsStackable(item))
            {
                return item;
            }

            return GameSystems.Item.FindMatchingStackableItem(receiver, item) ?? item;
        }

        [TempleDllLocation(0x10067360)]
        public void RangedWeaponDeductAmmo(GameObjectBody attacker)
        {
            if (GameSystems.D20.D20Query(attacker, D20DispatcherKey.QUE_Polymorphed))
            {
                return;
            }

            var weapon = ItemWornAt(attacker, EquipSlot.WeaponPrimary);
            if (weapon == null)
            {
                return;
            }

            var ammoType = weapon.GetWeaponAmmoType();

            if (!ammoType.IsThrown())
            {
                var ammoItem = GameSystems.Item.CheckRangedWeaponAmmo(attacker);
                if (ammoItem != null)
                {
                    var ammoQuantity = ammoItem.GetInt32(obj_f.ammo_quantity) - 1;
                    ammoItem.SetInt32(obj_f.ammo_quantity, ammoQuantity);
                    if (ammoQuantity == 0)
                    {
                        GameSystems.Object.Destroy(ammoItem);
                    }

                    if (IsCrossbow(weapon))
                    {
                        weapon.WeaponFlags &= ~WeaponFlag.WEAPON_LOADED;
                    }
                }
            }
        }

        [TempleDllLocation(0x10065890)]
        public bool MainWeaponUsesAmmo(GameObjectBody critter)
        {
            if (GameSystems.D20.D20Query(critter, D20DispatcherKey.QUE_Polymorphed))
            {
                return false;
            }

            var weapon = ItemWornAt(critter, EquipSlot.WeaponPrimary);
            if (weapon == null || weapon.type != ObjectType.weapon)
            {
                return false;
            }

            return weapon.GetWeaponAmmoType() != WeaponAmmoType.no_ammo;
        }

        [TempleDllLocation(0x10065ad0)]
        public bool ReloadEquippedWeapon(GameObjectBody critter)
        {
            if (GameSystems.D20.D20Query(critter, D20DispatcherKey.QUE_Polymorphed))
            {
                return false;
            }

            var weapon = ItemWornAt(critter, EquipSlot.WeaponPrimary);
            if (weapon == null || !IsCrossbow(weapon))
            {
                return false;
            }

            weapon.WeaponFlags |= WeaponFlag.WEAPON_LOADED;
            return true;
        }

        [TempleDllLocation(0x10064870)]
        public int GetItemSpellCharges(GameObjectBody item)
        {
            return item.GetInt32(obj_f.item_spell_charges_idx);
        }

        [TempleDllLocation(0x11eb6453)]
        public bool IsProtoWornAt(GameObjectBody wearer, EquipSlot slot, int protoId)
        {
            var item = ItemWornAt(wearer, slot);
            return item != null && item.ProtoId == protoId;
        }

        public bool IsBuckler(GameObjectBody shield)
        {
            if (!shield.type.IsEquipment())
            {
                return false;
            }

            return (shield.GetItemWearFlags() & ItemWearFlag.BUCKLER) != 0;
        }

        [TempleDllLocation(0x10067080)]
        public void ScheduleContainerRestock(GameObjectBody container)
        {
            // We cannot respawn the content of container that do not have an inventory source specified
            if (GameSystems.Item.GetInventorySource(container) == 0)
            {
                return;
            }

            // TODO: I think it's fishy that it resets the respawn time if you talk to a vendor...
            GameSystems.TimeEvent.Remove(TimeEventType.NPCRespawn, evt => evt.arg1.handle == container);

            var newEvt = new TimeEvent(TimeEventType.NPCRespawn);
            newEvt.arg1.handle = container;
            // Between 12 and 24 hours
            var delay = GameSystems.Random.GetInt(43200000, 86400000);
            Logger.Info("Scheduling NPC inventory restock for {0} in {1}ms", container, delay);
            GameSystems.TimeEvent.Schedule(newEvt, delay, out _);
        }
    }

    public enum ItemErrorCode
    {
        OK = 0,
        Cannot_Transfer = 1,
        Item_Too_Heavy = 2,
        No_Room_For_Item = 3,
        Cannot_Use_While_Polymorphed = 4,
        Cannot_Pickup_Magical_Items = 5,
        Cannot_Pickup_Techno_Items = 6,
        Item_Cannot_Be_Dropped = 7,
        NPC_Will_Not_Drop = 8,
        Wrong_Type_For_Slot = 9,
        No_Free_Hand = 10,
        Crippled_Arm_Prevents_Wielding = 11,
        Item_Too_Large = 12,
        Has_No_Art = 13,
        Opposite_Gender = 14,
        Cannot_Wield_Magical = 15,
        Cannot_Wield_Techno = 16,
        Wield_Slot_Occupied = 17,
        Prohibited_Due_To_Class = 18,

        ItemCannotBeUsed = 40,
        ItemIsBroken = 41,
        ScrollRequires5Int = 42,
        CannotUseMagicalItems = 43,
        CannotUseTechItems = 44,
        FailedToUseItem = 45,

        CannotBuyItem = 80,
        CannotSellItem = 81
    }

    public static class ItemErrorCodes
    {
        public static readonly IImmutableList<ItemErrorCode> Codes = ImmutableList.Create(
            (ItemErrorCode[]) Enum.GetValues(typeof(ItemErrorCode))
        );
    }

    [Flags]
    public enum ItemInsertFlag : byte
    {
        None = 0,
        Allow_Swap = 0x1,

        Use_Wield_Slots =
            0x2, // will let the item transfer try to insert in the wielded item slots (note: will not replace if there is already an item equipped!)
        Unk4 = 0x4, // I think this allows to fall back to unspecified slots
        Use_Max_Idx_200 = 0x8, // will use up to inventory index 200 (invisible slots)
        Unk10 = 0x10,
        Use_Bags = 0x20, // use inventory indices of bags (not really supported in ToEE)
        Unk40 = 0x40,
        Unk80 = 0x80
    }

    public static class ItemExtensions
    {
        public static int GetItemInventoryLocation(GameObjectBody obj) =>
            obj.GetInt32(obj_f.item_inv_location);

        public static bool TryGetQuantity(this GameObjectBody obj, out int quantity)
        {
            if (GameSystems.Item.GetQuantityField(obj, out var quantityField))
            {
                quantity = obj.GetInt32(quantityField);
                return true;
            }

            quantity = 0;
            return false;
        }

        [TempleDllLocation(0x100642b0)]
        public static int GetQuantity(this GameObjectBody obj)
        {
            if (!TryGetQuantity(obj, out var quantity))
            {
                return 1;
            }

            return quantity;
        }

        [TempleDllLocation(0x100642f0)]
        public static bool SetQuantity(this GameObjectBody obj, int quantity)
        {
            if (GameSystems.Item.GetQuantityField(obj, out var quantityField))
            {
                obj.SetInt32(quantityField, quantity);
                return true;
            }

            return false;
        }

        [TempleDllLocation(0x10064330)]
        public static bool ReduceQuantity(this GameObjectBody obj, int byAmount)
        {
            if (GameSystems.Item.GetQuantityField(obj, out var quantityField))
            {
                var quantity = obj.GetInt32(quantityField) - byAmount;

                if (quantity < 0)
                {
                    quantity = 0;
                }

                obj.SetInt32(quantityField, quantity);
                return true;
            }

            return false;
        }

        public static bool IncreaseQuantity(this GameObjectBody obj, int byAmount)
        {
            if (GameSystems.Item.GetQuantityField(obj, out var quantityField))
            {
                var quantity = obj.GetInt32(quantityField) + byAmount;
                obj.SetInt32(quantityField, quantity);
                return true;
            }

            return false;
        }

        public static string GetInventoryIconPath(this GameObjectBody item)
        {
            var artId = item.GetInt32(obj_f.item_inv_aid);
            return GameSystems.UiArtManager.GetInventoryIconPath(artId);
        }

        [TempleDllLocation(0x100644b0)]
        public static GameObjectBody FindItemByProto(this GameObjectBody container, int protoId,
            bool skipEquipment = false)
        {
            foreach (var item in container.EnumerateChildren())
            {
                if (skipEquipment && ItemSystem.IsInvIdxWorn(item.GetItemInventoryLocation()))
                {
                    continue;
                }

                if (item.ProtoId == protoId)
                {
                    return item;
                }
            }

            return null;
        }

        [TempleDllLocation(0x100643f0)]
        public static GameObjectBody FindItemByName(this GameObjectBody container, int nameId,
            bool skipEquipment = false)
        {
            foreach (var item in container.EnumerateChildren())
            {
                if (skipEquipment && ItemSystem.IsInvIdxWorn(item.GetItemInventoryLocation()))
                {
                    continue;
                }

                if (item.GetInt32(obj_f.name) == nameId)
                {
                    return item;
                }
            }

            return null;
        }
    }
}
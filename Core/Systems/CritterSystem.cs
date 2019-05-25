using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using SpicyTemple.Core.Config;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.GFX;
using SpicyTemple.Core.IO;
using SpicyTemple.Core.Logging;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.Systems.Feats;
using SpicyTemple.Core.Systems.GameObjects;
using SpicyTemple.Core.Systems.TimeEvents;
using SpicyTemple.Core.TigSubsystems;
using SpicyTemple.Core.Time;
using SpicyTemple.Core.Utils;

namespace SpicyTemple.Core.Systems
{
    public class CritterSystem : IGameSystem
    {
        private static readonly ILogger Logger = new ConsoleLogger();

        private const bool IsEditor = false;

        public void Dispose()
        {
        }

        public void SetStandPoint(in GameObjectBody obj, StandPointType type, StandPoint standpoint)
        {
            throw new NotImplementedException();
        }

        public void GenerateHp(ObjHndl objHandle) => GenerateHp(GameSystems.Object.GetObject(objHandle));

        [TempleDllLocation(0x1007F720)]
        public void GenerateHp(GameObjectBody obj)
        {
            var hpPts = 0;
            var critterLvlIdx = 0;

            var conMod = 0;
            if (GameSystems.D20.D20Query(obj, D20DispatcherKey.QUE_Critter_Has_No_Con_Score) == 0)
            {
                int conScore;

                var dispatcher = obj.GetDispatcher();
                if (dispatcher != null)
                {
                    conScore = GameSystems.Stat.DispatchForCritter(obj, null, DispatcherType.StatBaseGet,
                        D20DispatcherKey.STAT_CONSTITUTION);
                }
                else
                {
                    conScore = GameSystems.Stat.ObjStatBaseGet(obj, Stat.constitution);
                }

                conMod = D20StatSystem.GetModifierForAbilityScore(conScore);
            }

            var numLvls = obj.GetInt32Array(obj_f.critter_level_idx).Count;
            for (var i = 0; i < numLvls; i++)
            {
                var classType = (Stat) obj.GetInt32(obj_f.critter_level_idx, i);
                var classHd = D20ClassSystem.GetClassHitDice(classType);
                if (i == 0)
                {
                    hpPts = classHd; // first class level gets full HP
                }
                else
                {
                    int hdRoll;
                    if (Globals.Config.HpOnLevelUpMode == HpOnLevelUpMode.Max)
                    {
                        hdRoll = classHd;
                    }
                    else if (Globals.Config.HpOnLevelUpMode == HpOnLevelUpMode.Average)
                    {
                        // hit die are always even numbered so randomize the roundoff
                        hdRoll = classHd / 2 + RandomSystem.GetInt(0, 1);
                    }
                    else
                    {
                        hdRoll = Dice.Roll(1, classHd);
                    }

                    if (hdRoll + conMod < 1)
                    {
                        // note: the con mod is applied separately! This just makes sure it doesn't dip to negatives
                        hdRoll = 1 - conMod;
                    }

                    hpPts += hdRoll;
                }
            }

            var racialHd = D20RaceSystem.GetHitDice(GameSystems.Critter.GetRace(obj, false));
            if (racialHd.IsValid)
            {
                hpPts += racialHd.Roll();
            }

            if (obj.IsNPC())
            {
                var numDice = obj.GetInt32(obj_f.npc_hitdice_idx, 0);
                var sides = obj.GetInt32(obj_f.npc_hitdice_idx, 1);
                var modifier = obj.GetInt32(obj_f.npc_hitdice_idx, 2);
                var npcHd = new Dice(numDice, sides, modifier);
                var npcHdVal = npcHd.Roll();
                if (Globals.Config.MaxHpForNpcHitdice)
                {
                    npcHdVal = numDice * npcHd.Sides + npcHd.Modifier;
                }

                if (npcHdVal + conMod * numDice < 1)
                    npcHdVal = numDice * (1 - conMod);
                hpPts += npcHdVal;
            }

            if (hpPts < 1)
            {
                hpPts = 1;
            }

            obj.SetInt32(obj_f.hp_pts, hpPts);
        }

        [TempleDllLocation(0x1007e650)]
        public bool IsDeadNullDestroyed(GameObjectBody critter)
        {
            if (critter == null)
            {
                return true;
            }

            var flags = critter.GetFlags();
            if (flags.HasFlag(ObjectFlag.DESTROYED))
            {
                return true;
            }

            return GameSystems.Stat.StatLevelGet(critter, Stat.hp_current) <= -10;
        }

        public bool IsDeadOrUnconscious(ObjHndl handle) => IsDeadOrUnconscious(GameSystems.Object.GetObject(handle));

        public bool IsDeadOrUnconscious(GameObjectBody critter)
        {
            if (IsDeadNullDestroyed(critter))
            {
                return true;
            }

            return GameSystems.D20.D20Query(critter, D20DispatcherKey.QUE_Unconscious) != 0;
        }

        public bool IsProne(ObjHndl handle) => IsProne(GameSystems.Object.GetObject(handle));

        public bool IsProne(GameObjectBody critter)
        {
            return GameSystems.D20.D20Query(critter, D20DispatcherKey.QUE_Prone) != 0;
        }

        public bool IsMovingSilently(ObjHndl handle) => IsMovingSilently(GameSystems.Object.GetObject(handle));

        public bool IsMovingSilently(GameObjectBody critter)
        {
            var flags = critter.GetCritterFlags();
            return flags.HasFlag(CritterFlag.MOVING_SILENTLY);
        }

        public bool IsCombatModeActive(ObjHndl handle) => IsCombatModeActive(GameSystems.Object.GetObject(handle));

        public bool IsCombatModeActive(GameObjectBody critter)
        {
            var flags = critter.GetCritterFlags();
            return flags.HasFlag(CritterFlag.COMBAT_MODE_ACTIVE);
        }

        public bool IsConcealed(ObjHndl handle) => IsConcealed(GameSystems.Object.GetObject(handle));

        public bool IsConcealed(GameObjectBody critter)
        {
            var flags = critter.GetCritterFlags();
            return flags.HasFlag(CritterFlag.IS_CONCEALED);
        }

        public EncodedAnimId GetAnimId(ObjHndl handle, WeaponAnim weaponAnim) =>
            GetAnimId(GameSystems.Object.GetObject(handle), weaponAnim);

        public EncodedAnimId GetAnimId(GameObjectBody critter, WeaponAnim animType)
        {
            var weaponPrim = GetWornItem(critter, EquipSlot.WeaponPrimary);
            var weaponSec = GetWornItem(critter, EquipSlot.WeaponSecondary);
            if (weaponSec == null)
            {
                weaponSec = GetWornItem(critter, EquipSlot.Shield);
            }

            return GetWeaponAnim(critter, weaponPrim, weaponSec, animType);
        }

        [TempleDllLocation(0x10020B60)]
        public EncodedAnimId GetWeaponAnim(GameObjectBody wielder, GameObjectBody primaryWeapon,
            GameObjectBody secondaryWeapon, WeaponAnim animType)
        {
            var mainHandAnim = WeaponAnimType.Unarmed;
            var offHandAnim = WeaponAnimType.Unarmed;
            var ignoreOffHand = false;

            if (primaryWeapon != null)
            {
                if (primaryWeapon.type == ObjectType.weapon)
                {
                    mainHandAnim = GameSystems.Item.GetWeaponAnimType(primaryWeapon, wielder);
                }
                else if (primaryWeapon.type == ObjectType.armor)
                {
                    mainHandAnim = WeaponAnimType.Shield;
                }

                if (GameSystems.Item.GetWieldType(wielder, primaryWeapon) == 2)
                {
                    offHandAnim = mainHandAnim;
                    ignoreOffHand = true;
                }
            }

            if (!ignoreOffHand && secondaryWeapon != null)
            {
                if (secondaryWeapon.type == ObjectType.weapon)
                {
                    offHandAnim = GameSystems.Item.GetWeaponAnimType(secondaryWeapon, wielder);
                }
                else if (secondaryWeapon.type == ObjectType.armor)
                {
                    offHandAnim = WeaponAnimType.Shield;
                }
            }

            // If the user is fully unarmed and has unarmed strike, we'll show the monk stance
            if (mainHandAnim == WeaponAnimType.Unarmed
                && offHandAnim == WeaponAnimType.Unarmed
                && GameSystems.Feat.HasFeat(wielder, FeatId.IMPROVED_UNARMED_STRIKE))
            {
                offHandAnim = WeaponAnimType.Monk;
                mainHandAnim = WeaponAnimType.Monk;
            }

            return new EncodedAnimId(animType, mainHandAnim, offHandAnim);
        }

        public GameObjectBody GetWornItem(GameObjectBody critter, EquipSlot slot)
        {
            return GameSystems.Item.ItemWornAt(critter, slot);
        }

        public int GetCasterLevel(GameObjectBody obj)
        {
            int result = 0;
            foreach (var classEnum in D20ClassSystem.AllClasses)
            {
                if (D20ClassSystem.IsCastingClass(classEnum))
                {
                    var cl = GetCasterLevelForClass(obj, classEnum);
                    if (cl > result)
                        result = cl;
                }
            }

            return result;
        }

        public int GetCasterLevelForClass(GameObjectBody obj, Stat classCode)
        {
            return DispatchGetBaseCasterLevel(obj, classCode);
        }

        public int DispatchGetBaseCasterLevel(GameObjectBody obj, Stat casterClass)
        {
            var dispatcher = obj.GetDispatcher();
            if (dispatcher == null)
            {
                return 0;
            }

            var evtObj = EvtObjSpellCaster.Default;
            evtObj.handle = obj;
            evtObj.arg0 = casterClass;
            dispatcher.Process(DispatcherType.GetBaseCasterLevel, D20DispatcherKey.NONE, evtObj);
            return evtObj.bonlist.OverallBonus;
        }

        public int GetSpellListLevelExtension(GameObjectBody handle, Stat classCode)
        {
            return DispatchSpellListLevelExtension(handle, classCode);
        }

        private int DispatchSpellListLevelExtension(GameObjectBody handle, Stat casterClass)
        {
            var dispatcher = handle.GetDispatcher();
            if (dispatcher == null)
            {
                return 0;
            }

            var evtObj = EvtObjSpellCaster.Default;
            evtObj.handle = handle;
            evtObj.arg0 = casterClass;
            dispatcher.Process(DispatcherType.SpellListExtension, D20DispatcherKey.NONE, evtObj); // TODO REF OUT

            return evtObj.bonlist.OverallBonus;
        }

        public int GetBaseAttackBonus(GameObjectBody obj, Stat classBeingLeveled = default)
        {
            var bab = 0;
            foreach (var it in D20ClassSystem.AllClasses)
            {
                var classLvl = GameSystems.Stat.StatLevelGet(obj, it);
                if (classBeingLeveled == it)
                    classLvl++;
                bab += D20ClassSystem.GetBaseAttackBonus(it, classLvl);
            }

            // get BAB from NPC HD
            if (obj.type == ObjectType.npc)
            {
                var npcHd = obj.GetInt32(obj_f.npc_hitdice_idx, 0);
                var moncat = GameSystems.Critter.GetCategory(obj);
                switch (moncat)
                {
                    case MonsterCategory.aberration:
                    case MonsterCategory.animal:
                    case MonsterCategory.beast:
                    case MonsterCategory.construct:
                    case MonsterCategory.elemental:
                    case MonsterCategory.giant:
                    case MonsterCategory.humanoid:
                    case MonsterCategory.ooze:
                    case MonsterCategory.plant:
                    case MonsterCategory.shapechanger:
                    case MonsterCategory.vermin:
                        return bab + (3 * npcHd / 4);


                    case MonsterCategory.dragon:
                    case MonsterCategory.magical_beast:
                    case MonsterCategory.monstrous_humanoid:
                    case MonsterCategory.outsider:
                        return bab + npcHd;

                    case MonsterCategory.fey:
                    case MonsterCategory.undead:
                        return bab + npcHd / 2;

                    default: break;
                }
            }

            return bab;
        }

        public MonsterCategory GetCategory(GameObjectBody obj)
        {
            if (obj.IsCritter())
            {
                var monCat = obj.GetInt64(obj_f.critter_monster_category);
                return (MonsterCategory) (monCat & 0xFFFFFFFF);
            }

            return MonsterCategory.monstrous_humanoid; // default - so they have at least a weapons proficiency
        }

        public bool IsCategoryType(GameObjectBody obj, MonsterCategory category)
        {
            if (obj != null && obj.IsCritter())
            {
                var monsterCategory = GetCategory(obj);
                return monsterCategory == category;
            }

            return false;
        }

        public RaceId GetRace(GameObjectBody obj, bool baseRace)
        {
            var race = GameSystems.Stat.StatLevelGet(obj, Stat.race);
            if (!baseRace)
            {
                race += GameSystems.Stat.StatLevelGet(obj, Stat.subrace) << 5;
            }

            return (RaceId) race;
        }

        [TempleDllLocation(0x1001f3b0)]
        public IEnumerable<GameObjectBody> GetFollowers(GameObjectBody obj)
        {
            var objArray = obj.GetObjectIdArray(obj_f.critter_follower_idx);

            var result = new List<GameObjectBody>(objArray.Count);
            for (var i = 0; i < objArray.Count; i++)
            {
                var follower = GameSystems.Object.GetObject(objArray[i]);
                if (follower != null)
                {
                    result.Add(follower);
                }
            }

            return result;
        }

        [TempleDllLocation(0x10080c20)]
        public void RemoveFollowerFromLeaderCritterFollowers(GameObjectBody obj)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10080430)]
        public GameObjectBody GetLeaderRecursive(GameObjectBody obj)
        {
            if (!obj.IsNPC())
            {
                return null;
            }

            var result = obj;
            while (result != null)
            {
                result = GetLeader(result);
                if (result != null && result.IsPC())
                {
                    return result;
                }
            }

            return null;
        }

        [TempleDllLocation(0x1007ea70)]
        public GameObjectBody GetLeader(GameObjectBody obj)
        {
            if (obj.IsNPC())
            {
                return obj.GetObject(obj_f.npc_leader);
            }
            else if (obj.IsPC())
            {
                if (GameSystems.D20.D20Query(obj, D20DispatcherKey.QUE_Critter_Is_Charmed) == 0)
                {
                    return null;
                }

                return GameSystems.D20.D20QueryReturnObject(obj, D20DispatcherKey.QUE_Critter_Is_Charmed);
            }
            else
            {
                return null;
            }
        }

        private Dictionary<int, ImmutableList<string>> _addMeshes;

        /// <summary>
        /// This is called initially when the model is loaded for an object and adds NPC specific add meshes.
        /// </summary>
        public void AddNpcAddMeshes(GameObjectBody obj)
        {
            var id = obj.GetInt32(obj_f.npc_add_mesh);
            var model = obj.GetOrCreateAnimHandle();

            foreach (var addMesh in GetAddMeshes(id, 0))
            {
                model.AddAddMesh(addMesh);
            }
        }

        private IImmutableList<string> GetAddMeshes(int matIdx, int raceOffset)
        {
            if (_addMeshes == null)
            {
                var mapping = Tig.FS.ReadMesFile("rules/addmesh.mes");
                _addMeshes = new Dictionary<int, ImmutableList<string>>(mapping.Count);
                foreach (var (key, line) in mapping)
                {
                    _addMeshes[key] = line.Split(";", StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => s.Trim())
                        .ToImmutableList();
                }
            }

            if (_addMeshes.TryGetValue(matIdx + raceOffset, out var materials))
            {
                return materials;
            }

            return ImmutableList<string>.Empty;
        }

        [TempleDllLocation(0x101391c0)]
        public bool IsLootableCorpse(GameObjectBody obj)
        {
            if (!obj.IsCritter())
            {
                return false;
            }

            if (!GameSystems.Critter.IsDeadNullDestroyed(obj))
            {
                return false; // It's still alive
            }

            // Find any item in the critters inventory that would be considered lootable
            foreach (var item in obj.EnumerateChildren())
            {
                if (item.GetItemFlags().HasFlag(ItemFlag.NO_LOOT))
                {
                    continue; // Flagged as unlootable
                }

                // ToEE previously excluded worn items here, but we'll consider all items

                return true; // Found an item that is lootable
            }

            return false;
        }

        [TempleDllLocation(0x10080490)]
        public void UpdateNpcHealingTimers()
        {
            foreach (var obj in GameSystems.Object.EnumerateNonProtos())
            {
                if (obj.IsNPC())
                {
                    UpdateSubdualHealingTimer(obj, true);
                    UpdateNormalHealingTimer(obj, true);
                }
            }
        }

        private bool _isRemovingSubdualHealingTimers;

        [TempleDllLocation(0x1007edc0)]
        private void UpdateSubdualHealingTimer(GameObjectBody obj, bool applyQueuedHealing)
        {
            if (_isRemovingSubdualHealingTimers)
            {
                return; // Could lead to infinite recursion
            }

            GameSystems.TimeEvent.Remove(TimeEventType.SubdualHealing, evt =>
            {
                var timerObj = evt.arg1.handle;
                if (timerObj == obj)
                {
                    if (applyQueuedHealing)
                    {
                        _isRemovingSubdualHealingTimers = true;
                        CritterHealSubdualDamageOverTime(timerObj, evt.arg2.timePoint);
                        _isRemovingSubdualHealingTimers = false;
                    }

                    return true;
                }

                return false;
            });

            var newEvt = new TimeEvent(TimeEventType.SubdualHealing);
            newEvt.arg1.handle = obj;
            newEvt.arg2.timePoint = GameSystems.TimeEvent.GameTime;
            GameSystems.TimeEvent.Schedule(newEvt, TimeSpan.FromHours(1), out _);
        }

        [TempleDllLocation(0x1007EBD0)]
        private void CritterHealSubdualDamageOverTime(GameObjectBody obj, TimePoint lastHealing)
        {
            var flags = obj.GetFlags();

            if (IsDeadNullDestroyed(obj) || flags.HasFlag(ObjectFlag.DONTDRAW) || flags.HasFlag(ObjectFlag.OFF))
            {
                return;
            }

            if (!GameSystems.Party.IsInParty(obj) && obj.GetInt32(obj_f.critter_subdual_damage) > 0)
            {
                // Heal one hit point of subdual damage per level and hour elapsed
                var hoursElapsed = (int) (GameSystems.TimeEvent.GameTime - lastHealing).TotalHours;
                if (hoursElapsed < 1 && !_isRemovingSubdualHealingTimers)
                {
                    hoursElapsed = 1;
                }

                var levels = GameSystems.Stat.StatLevelGet(obj, Stat.level);
                if (levels < 1)
                {
                    levels = 1;
                }

                HealSubdualSub_100B9030(obj, hoursElapsed * levels);
            }
        }

        [TempleDllLocation(0x100B9030)]
        private void HealSubdualSub_100B9030(GameObjectBody obj, int amount)
        {
            throw new NotImplementedException();
        }

        private bool _isRemovingHealingTimers;

        [TempleDllLocation(0x1007f140)]
        private void UpdateNormalHealingTimer(GameObjectBody obj, bool applyQueuedHealing)
        {
            if (_isRemovingHealingTimers)
            {
                return; // Could lead to infinite recursion
            }

            GameSystems.TimeEvent.Remove(TimeEventType.NormalHealing, evt =>
            {
                var timerObj = evt.arg1.handle;
                if (timerObj == obj)
                {
                    if (applyQueuedHealing)
                    {
                        _isRemovingHealingTimers = true;
                        // TODO CritterHealNormalDamageOverTime(timerObj, evt.arg2.timePoint);
                        _isRemovingHealingTimers = false;
                    }

                    return true;
                }

                return false;
            });

            var newEvt = new TimeEvent(TimeEventType.NormalHealing);
            newEvt.arg1.handle = obj;
            newEvt.arg2.timePoint = GameSystems.TimeEvent.GameTime;
            GameSystems.TimeEvent.Schedule(newEvt, TimeSpan.FromHours(8), out _);
        }

        [TempleDllLocation(0x1007e480)]
        public void AddFaction(GameObjectBody obj, int factionId)
        {
            if (!obj.IsNPC())
            {
                return;
            }

            var factionCount = 0;
            while (factionCount < 50 && obj.GetInt32(obj_f.npc_faction, factionCount) != 0)
            {
                factionCount++;
            }

            obj.SetInt32(obj_f.npc_faction, factionCount, factionId);
            obj.SetInt32(obj_f.npc_faction, factionCount + 1, 0);

            if (factionCount == 50)
            {
                Logger.Warn("Critter {0} has too many factions, cannot add more.", obj);
            }
        }

        [TempleDllLocation(0x1007f630)]
        public void SaveTeleportMap(GameObjectBody obj)
        {
            var teleportMap = obj.GetInt32(obj_f.critter_teleport_map);
            if (teleportMap == 0)
            {
                teleportMap = GameSystems.Map.GetCurrentMapId();
                obj.SetInt32(obj_f.critter_teleport_map, teleportMap);
            }
        }

        [TempleDllLocation(0x10080790)]
        public int GetTeleportMap(GameObjectBody obj)
        {
            if (GameSystems.Teleport.HasDayNightTransfer(obj))
            {
                return GameSystems.Teleport.GetCurrentDayNightTransferMap(obj);
            }
            else
            {
                var mapId = obj.GetInt32(obj_f.critter_teleport_map);

                if (mapId == 0)
                {
                    mapId = GameSystems.Map.GetCurrentMapId();
                    obj.SetInt32(obj_f.critter_teleport_map, mapId);
                }

                return mapId;
            }
        }

        [TempleDllLocation(0x1007f670)]
        public int GetDescriptionId(GameObjectBody critter, GameObjectBody observer)
        {
            if (IsEditor || critter == observer || observer != null && GameSystems.Reaction.HasMet(critter, observer))
            {
                return critter.GetInt32(obj_f.description);
            }
            else
            {
                return critter.GetInt32(obj_f.critter_description_unknown);
            }
        }

        [TempleDllLocation(0x1007E9D0)]
        public void UpdateModelEquipment(GameObjectBody obj)
        {
            UpdateAddMeshes(obj);
            var raceOffset = GetModelRaceOffset(obj, false);
            if (raceOffset == -1)
            {
                return;
            }

            var model = obj.GetOrCreateAnimHandle();
            if (model == null)
            {
                return; // No animation really present
            }

            // This is a bit shit but since AAS will just splice the
            // add meshes into the list of model parts,
            // we have to reset the render buffers
            model.RenderState?.Dispose();
            model.RenderState = null;

            // Apply the naked replacement materials for
            // equipment slots that support them
            GameSystems.MapObject.ApplyReplacementMaterial(model, 0 + raceOffset); // Helmet
            GameSystems.MapObject.ApplyReplacementMaterial(model, 100 + raceOffset); // Cloak
            GameSystems.MapObject.ApplyReplacementMaterial(model, 200 + raceOffset); // Gloves
            GameSystems.MapObject.ApplyReplacementMaterial(model, 500 + raceOffset); // Armor
            GameSystems.MapObject.ApplyReplacementMaterial(model, 800 + raceOffset); // Boots

            // Items can also apply replacement materials. I.e. Worn boots can replace the feet texture.
            // Use base race for equipment because holy crap ToEE has an entry for each race!!!
            var baseRaceOffset = GetModelRaceOffset(obj, true);
            foreach (var slot in EquipSlots.Slots)
            {
                var item = GetWornItem(obj, slot);
                if (item != null)
                {
                    var materialSlot = item.GetInt32(obj_f.item_material_slot);
                    if (materialSlot != -1)
                    {
                        GameSystems.MapObject.ApplyReplacementMaterial(model, materialSlot + raceOffset,
                            materialSlot + baseRaceOffset);
                    }
                }
            }
        }

        [TempleDllLocation(0x1007e760)]
        private void UpdateAddMeshes(GameObjectBody obj)
        {
            var raceOffset = GetModelRaceOffset(obj);

            // For monsters, normal addmeshes are not processed
            if (raceOffset == -1)
            {
                return;
            }

            var model = obj.GetOrCreateAnimHandle();

            if (model == null)
            {
                return;
            }

            // Reset all existing add meshes
            model.ClearAddMeshes();

            // Do not process add meshes if the user is polymorphed
            if (GameSystems.D20.D20Query(obj, D20DispatcherKey.QUE_Polymorphed) != 0)
            {
                return;
            }

            // Adjust the hair style size based on the worn helmet
            var helmet = GetWornItem(obj, EquipSlot.Helmet);

            var helmetType = ArmorFlag.HELM_TYPE_SMALL;
            if (helmet != null)
            {
                var wearFlags = helmet.GetItemWearFlags();
                if (wearFlags.HasFlag(ItemWearFlag.HELMET))
                {
                    helmetType = helmet.GetArmorFlags() & ArmorFlag.HELM_BITMASK;
                }
            }

            var robes = GetWornItem(obj, EquipSlot.Robes);

            foreach (var slot in EquipSlots.Slots)
            {
                var item = GetWornItem(obj, slot);
                if (item == null)
                {
                    continue;
                }

                // Armor is hidden by a robe
                if (robes != null && slot == EquipSlot.Armor)
                {
                    continue;
                }

                // Addmesh / Material index of the item
                var matIdx = item.GetInt32(obj_f.item_material_slot);
                if (matIdx == -1)
                {
                    continue; // No assigned addmesh or material replacement
                }

                var addMeshes = GetAddMeshes(matIdx, raceOffset);

                if (addMeshes.Count > 0)
                {
                    model.AddAddMesh(addMeshes[0]);
                }

                // Only add the helmet part if there's no real helmet
                if (helmetType == ArmorFlag.HELM_TYPE_SMALL && addMeshes.Count >= 2)
                {
                    model.AddAddMesh(addMeshes[1]);
                    helmetType = ArmorFlag.HELM_TYPE_LARGE; // The addmesh counts as a large helmet
                }
            }

            // Add the hair model
            var packedHairStyle = obj.GetInt32(obj_f.critter_hair_style);
            var hairSettings = HairSettings.Unpack(packedHairStyle);

            // Modify the hair style size based on the used helmet,
            // since it might cover it
            if (helmetType.HasFlag(ArmorFlag.HELM_TYPE_LARGE))
            {
                // Large helmets remove most of the hair
                hairSettings.Size = HairStyleSize.None;
            }
            else if (helmetType.HasFlag(ArmorFlag.HELM_TYPE_MEDIUM))
            {
                // Smaller helmets only part of it
                hairSettings.Size = HairStyleSize.Small;
            }
            else
            {
                hairSettings.Size = HairStyleSize.Big;
            }

            var hairModel = hairSettings.ModelPath;
            if (hairModel != null)
            {
                model.AddAddMesh(hairModel);
            }
        }

        [TempleDllLocation(0x1007e690)]
        private int GetModelRaceOffset(GameObjectBody obj, bool useBaseRace = true)
        {
            // Meshes above 1000 are monsters, they dont get a creature type
            var meshId = obj.GetInt32(obj_f.base_mesh);
            if (meshId >= 1000)
            {
                return -1;
            }

            var race = GetRace(obj, useBaseRace);
            var gender = GetGender(obj);
            var isMale = (gender == Gender.Male);

            // The following table comes from materials.mes (or materials_ext.mes), where
            // the offsets into the materials and addmesh table are listed.
            return D20RaceSystem.GetRaceMaterialOffset(race) + (isMale ? 0 : 1);
        }

        private Gender GetGender(GameObjectBody critter)
        {
            return (Gender) GameSystems.Stat.StatLevelGet(critter, Stat.gender);
        }

        [TempleDllLocation(0x1007FCC0)]
        public void ClearMoney(GameObjectBody obj)
        {
            if (obj.IsPC())
            {
                GameSystems.Party.ClearPartyMoney();
            }
            else
            {
                GameSystems.Stat.SetBasicStat(obj, Stat.money_cp, 0);
                GameSystems.Stat.SetBasicStat(obj, Stat.money_sp, 0);
                GameSystems.Stat.SetBasicStat(obj, Stat.money_gp, 0);
                GameSystems.Stat.SetBasicStat(obj, Stat.money_pp, 0);
            }
        }

        [TempleDllLocation(0x1007f880)]
        public int GetMoney(GameObjectBody obj)
        {
            if (obj.IsPC())
            {
                return GameSystems.Party.GetPartyMoney();
            }
            else
            {
                var copper = GameSystems.Stat.StatLevelGet(obj, Stat.money_cp);
                var silver = GameSystems.Stat.StatLevelGet(obj, Stat.money_sp);
                var gold = GameSystems.Stat.StatLevelGet(obj, Stat.money_gp);
                var platinum = GameSystems.Stat.StatLevelGet(obj, Stat.money_pp);
                return GameSystems.Party.GetCoinWorth(platinum, gold, silver, copper);
            }
        }

        [TempleDllLocation(0x10074710)]
        public bool IsCategory(GameObjectBody obj, MonsterCategory category)
        {
            return obj.IsCritter() && GetCategory(obj) == category;
        }

        public bool IsCategorySubtype(GameObjectBody critter, MonsterSubtype subtype)
        {
            if (critter.IsCritter())
            {
                var monCat = critter.GetInt64(obj_f.critter_monster_category);
                var moncatSubtype = (MonsterSubtype) (monCat >> 32);
                return moncatSubtype.HasFlag(subtype);
            }

            return false;
        }

        [TempleDllLocation(0x1007fef0)]
        public bool IsAnimal(GameObjectBody critter)
        {
            return GetCategory(critter) == MonsterCategory.animal;
        }

        [TempleDllLocation(0x1004d1f0)]
        public void BuildRadialMenu(GameObjectBody critter)
        {
            var dispatcher = critter.GetDispatcher();

            if (dispatcher != null)
            {
                if (GameSystems.Party.IsPlayerControlled(critter))
                {
                    dispatcher.Process(DispatcherType.RadialMenuEntry, 0, null);
                    GameSystems.D20.RadialMenu.SetActive(critter);
                }
            }
        }

        // TemplePlus enhancement
        public int GetEffectiveLevel(GameObjectBody obj)
        {
            var currentLevel = GameSystems.Stat.StatLevelGet(obj, Stat.level);
            var race = GetRace(obj, false);
            var lvlAdj = D20RaceSystem.GetLevelAdjustment(race);
            int racialHdCount;
            if (obj.IsPC())
            {
                racialHdCount = D20RaceSystem.GetHitDice(race).Count;
            }
            else
            {
                racialHdCount = obj.GetInt32(obj_f.npc_hitdice_idx, 0);
            }

            currentLevel += lvlAdj + racialHdCount;
            return currentLevel;
        }

        [TempleDllLocation(0x10080220)]
        public bool CanLevelUp(GameObjectBody obj)
        {
            // Effective character level
            var ecl = GetEffectiveLevel(obj);

            if (ecl < 0 || ecl >= Globals.Config.MaxLevel)
            {
                return false;
            }

            if (GameSystems.D20.D20Query(obj, D20DispatcherKey.QUE_ExperienceExempt) != 0)
            {
                return false;
            }

            var experience = obj.GetInt32(obj_f.critter_experience);
            return experience >= GameSystems.Level.GetExperienceForLevel(ecl + 1);
        }
    }
}
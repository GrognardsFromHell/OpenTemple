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
using SpicyTemple.Core.Systems.ObjScript;
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

        private Dictionary<int, string> _skillUi;

        public CritterSystem()
        {
            _skillUi = Tig.FS.ReadMesFile("mes/skill_ui.mes");

            Stub.TODO();
        }

        public void Dispose()
        {
        }

        public void SetStandPoint(in GameObjectBody obj, StandPointType type, StandPoint standpoint)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x1007F720)]
        public void GenerateHp(GameObjectBody obj)
        {
            var hpPts = 0;
            var critterLvlIdx = 0;

            var conMod = 0;
            if (GameSystems.D20.D20Query(obj, D20DispatcherKey.QUE_Critter_Has_No_Con_Score) == 0)
            {
                var conScore = obj.GetBaseStat(Stat.constitution);
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
                        hdRoll = classHd / 2 + GameSystems.Random.GetInt(0, 1);
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

        [TempleDllLocation(0x100803e0)]
        public bool IsDeadOrUnconscious(GameObjectBody critter)
        {
            if (IsDeadNullDestroyed(critter))
            {
                return true;
            }

            return GameSystems.D20.D20Query(critter, D20DispatcherKey.QUE_Unconscious) != 0;
        }

        [TempleDllLocation(0x1007e590)]
        public bool IsProne(GameObjectBody critter)
        {
            return GameSystems.D20.D20Query(critter, D20DispatcherKey.QUE_Prone) != 0;
        }

        [TempleDllLocation(0x1007f3d0)]
        public bool IsMovingSilently(GameObjectBody critter)
        {
            var flags = critter.GetCritterFlags();
            return flags.HasFlag(CritterFlag.MOVING_SILENTLY);
        }

        public bool IsCombatModeActive(GameObjectBody critter)
        {
            var flags = critter.GetCritterFlags();
            return flags.HasFlag(CritterFlag.COMBAT_MODE_ACTIVE);
        }

        [TempleDllLocation(0x1007f420)]
        public bool IsConcealed(GameObjectBody critter)
        {
            var flags = critter.GetCritterFlags();
            return flags.HasFlag(CritterFlag.IS_CONCEALED);
        }

        [TempleDllLocation(0x10020c60)]
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

        [TempleDllLocation(0x100746d0)]
        public MonsterCategory GetCategory(GameObjectBody obj)
        {
            if (obj.IsCritter())
            {
                var monCat = obj.GetInt64(obj_f.critter_monster_category);
                return (MonsterCategory) (monCat & 0xFFFFFFFF);
            }

            return MonsterCategory.monstrous_humanoid; // default - so they have at least a weapons proficiency
        }

        [TempleDllLocation(0x10074710)]
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
        public IEnumerable<GameObjectBody> EnumerateDirectFollowers(GameObjectBody obj)
        {
            var followers = obj.GetObjectArray(obj_f.critter_follower_idx);

            var result = new List<GameObjectBody>(followers.Count);
            foreach (var follower in followers)
            {
                if (follower != null)
                {
                    result.Add(follower);
                }
            }

            return result;
        }

        [TempleDllLocation(0x1001f450)]
        public IEnumerable<GameObjectBody> EnumerateAllFollowers(GameObjectBody critter)
        {
            foreach (var follower in EnumerateDirectFollowers(critter))
            {
                yield return follower;

                foreach (var recursiveFollower in EnumerateAllFollowers(follower))
                {
                    yield return recursiveFollower;
                }
            }
        }

        [TempleDllLocation(0x10080c20)]
        public void RemoveFollowerFromLeaderCritterFollowers(GameObjectBody obj)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10080fd0)]
        public bool RemoveFollower(GameObjectBody follower, bool force)
        {
            if ((follower.HasFlag(ObjectFlag.DESTROYED) || follower.GetStat(Stat.hp_current) <= -10)
                && GameSystems.Party.IsAiFollower(follower) || follower.IsPC() && GameSystems.Party.IsInParty(follower))
            {
                return false;
            }
            else
            {
                var leader = GetLeader(follower);
                if (leader != null)
                {
                    if (GameSystems.Party.IsInParty(follower))
                    {
                        GameSystems.Party.RemoveFromAllGroups(follower);
                    }

                    GameSystems.AI.FollowerAddWithTimeEvent(follower, force);
                    RemoveFollowerFromLeaderCritterFollowers(follower);
                    GameSystems.Script.ExecuteObjectScript(leader, follower, 0, 0, ObjScriptEvent.Disband, 0);
                    return true;
                }
                else
                {
                    return false;
                }
            }
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
        public void UpdateNormalHealingTimer(GameObjectBody obj, bool applyQueuedHealing)
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

        [TempleDllLocation(0x1007f8f0)]
        private void NormalizeMoney(ref int platinum, ref int gold, ref int silver, ref int copper)
        {
            silver += copper / 10;
            copper %= 10;

            gold += silver / 10;
            silver %= 10;

            platinum += gold / 10;
            gold %= 10;
        }

        [TempleDllLocation(0x1007FA40)]
        public void TakeMoney(GameObjectBody critter, int platinum, int gold, int silver, int copper)
        {
            if (critter.IsPC())
            {
                GameSystems.Party.RemovePartyMoney(platinum, gold, silver, copper);
            }
            else
            {
                Stub.TODO();

                // TODO: This seems borked. shouldn't it remove the total worth and not just the actual copper val?
                GameSystems.Party.GetCoinWorth(platinum, gold, silver, copper);

                // Deduct the copper amount and then start filling up the missing coins with coins from the
                // higher tiers
                GameSystems.Stat.SetBasicStat(critter, Stat.money_cp, critter.GetStat(Stat.money_cp) - copper);
                var remainingCp = critter.GetStat(Stat.money_cp);
                if (remainingCp < 0)
                {
                    var coins = (-remainingCp + 9) / 10;
                    GameSystems.Stat.SetBasicStat(critter, Stat.money_sp, critter.GetStat(Stat.money_sp) - coins);
                    GameSystems.Stat.SetBasicStat(critter, Stat.money_cp, critter.GetStat(Stat.money_cp) + coins * 10);
                }

                var remainingSp = critter.GetStat(Stat.money_sp);
                if (remainingSp < 0)
                {
                    var coins = (-remainingSp + 9) / 10;
                    GameSystems.Stat.SetBasicStat(critter, Stat.money_gp, critter.GetStat(Stat.money_gp) - coins);
                    GameSystems.Stat.SetBasicStat(critter, Stat.money_sp, critter.GetStat(Stat.money_sp) + coins * 10);
                }

                var remainingGp = critter.GetStat(Stat.money_gp);
                if (remainingGp < 0)
                {
                    var coins = (-remainingSp + 9) / 10;
                    GameSystems.Stat.SetBasicStat(critter, Stat.money_pp, critter.GetStat(Stat.money_pp) - coins);
                    GameSystems.Stat.SetBasicStat(critter, Stat.money_gp, critter.GetStat(Stat.money_gp) + coins * 10);
                }
            }
        }

        [TempleDllLocation(0x1007F960)]
        public void GiveMoney(GameObjectBody critter, int platinum, int gold, int silver, int copper)
        {
            NormalizeMoney(ref platinum, ref gold, ref silver, ref copper);
            if (critter.IsPC())
            {
                GameSystems.Party.AddPartyMoney(platinum, gold, silver, copper);
            }
            else
            {
                copper += critter.GetStat(Stat.money_cp);
                GameSystems.Stat.SetBasicStat(critter, Stat.money_cp, copper);

                silver += critter.GetStat(Stat.money_sp);
                GameSystems.Stat.SetBasicStat(critter, Stat.money_sp, silver);

                gold += critter.GetStat(Stat.money_gp);
                GameSystems.Stat.SetBasicStat(critter, Stat.money_gp, gold);

                platinum += critter.GetStat(Stat.money_pp);
                GameSystems.Stat.SetBasicStat(critter, Stat.money_pp, platinum);
            }
        }

        [TempleDllLocation(0x10074710)]
        public bool IsCategory(GameObjectBody obj, MonsterCategory category)
        {
            return obj.IsCritter() && GetCategory(obj) == category;
        }

        [TempleDllLocation(0x10074780)]
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

        [TempleDllLocation(0x1007ff10)]
        public bool IsUndead(GameObjectBody critter)
        {
            return GetCategory(critter) == MonsterCategory.undead;
        }

        [TempleDllLocation(0x1007ff30)]
        public bool IsOoze(GameObjectBody critter)
        {
            return GetCategory(critter) == MonsterCategory.ooze;
        }

        [TempleDllLocation(0x1007ff50)]
        public bool IsWaterSubtype(GameObjectBody critter)
        {
            return IsCategorySubtype(critter, MonsterSubtype.water);
        }

        [TempleDllLocation(0x1007ff70)]
        public bool IsFireSubtype(GameObjectBody critter)
        {
            return IsCategorySubtype(critter, MonsterSubtype.fire);
        }

        [TempleDllLocation(0x1007ff90)]
        public bool IsAirSubtype(GameObjectBody critter)
        {
            return IsCategorySubtype(critter, MonsterSubtype.air);
        }

        [TempleDllLocation(0x1007ffb0)]
        public bool IsEarthSubtype(GameObjectBody critter)
        {
            return IsCategorySubtype(critter, MonsterSubtype.earth);
        }

        [TempleDllLocation(0x1007ffd0)]
        public bool IsPlant(GameObjectBody critter)
        {
            return GetCategory(critter) == MonsterCategory.plant;
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

        [TempleDllLocation(0x100107E0)]
        public void Pickpocket(GameObjectBody obj, GameObjectBody tgtObj, out bool gotCaught)
        {
            gotCaught = false;

            if (obj.IsOffOrDestroyed || obj.IsDeadOrUnconscious())
            {
                return;
            }

            var pickpocketFailed = true;

            var tgtMoney = GetMoney(tgtObj) / 100;
            var isStealingMoney = Dice.Roll(1, 2) == 1;
            var dc = 20;

            if (isStealingMoney)
            {
                if (tgtMoney < 1)
                {
                    // less than 1 GP
                    isStealingMoney = false;
                }
            }

            if (GameSystems.Skill.SkillRoll(obj, SkillId.pick_pocket, dc, out var deltaFromDc, 1))
            {
                if (isStealingMoney)
                {
                    var moneyAmt = Dice.Roll(1, 1900, 99) / 100;
                    if (moneyAmt > tgtMoney)
                        moneyAmt = tgtMoney;
                    GameSystems.Critter.TakeMoney(tgtObj, 0, moneyAmt, 0, 0);
                    GameSystems.Critter.GiveMoney(obj, 0, moneyAmt, 0, 0);
                    GameSystems.RollHistory.CreateFromFreeText($"Stole {moneyAmt} GP.\n\n");
                    pickpocketFailed = false;
                }
                else
                {
                    var stealableItems = new List<GameObjectBody>();
                    foreach (var item in tgtObj.EnumerateChildren())
                    {
                        if (GameSystems.Item.ItemCanBePickpocketed(item))
                        {
                            stealableItems.Add(item);
                        }
                    }

                    if (stealableItems.Count > 0)
                    {
                        var itemStolen = GameSystems.Random.PickRandom(stealableItems);
                        GameSystems.RollHistory.CreateFromFreeText(
                            $"Stole {GameSystems.MapObject.GetDisplayName(itemStolen, obj)}.\n\n");
                        GameSystems.Item.SetItemParent(itemStolen, obj, 0);
                        pickpocketFailed = false;
                    }
                    else if (tgtMoney > 0) // steal coins instead
                    {
                        var moneyAmt = Dice.Roll(1, 1900, 99) / 100;
                        if (moneyAmt > tgtMoney)
                            moneyAmt = tgtMoney;
                        GameSystems.Critter.TakeMoney(tgtObj, 0, moneyAmt, 0, 0);
                        GameSystems.Critter.GiveMoney(obj, 0, moneyAmt, 0, 0);
                        GameSystems.RollHistory.CreateFromFreeText($"Stole {moneyAmt} GP.\n\n");
                        pickpocketFailed = false;
                    }
                    else
                    {
                        GameSystems.RollHistory.CreateFromFreeText("Nothing to steal...\n\n");
                    }
                }
            }


            if (GameSystems.Skill.SkillRoll(tgtObj, SkillId.spot, 20 + deltaFromDc, out _, 1))
            {
                GameSystems.Script.ExecuteObjectScript(tgtObj, obj,
                    ObjScriptEvent.CaughtThief); // e.g. when Dala is stealing from you
                gotCaught = true;
                GameSystems.AI.ProvokeHostility(obj, tgtObj, 1, 2);
            }

            if (GameSystems.Party.IsPlayerControlled(obj) || gotCaught)
            {
                var line = 1100;
                if (pickpocketFailed)
                {
                    line = 1101;
                }

                var lineText = _skillUi[line];
                GameSystems.TextFloater.FloatLine(obj, TextFloaterCategory.Generic, TextFloaterColor.Blue, lineText);

                if (gotCaught)
                {
                    lineText = _skillUi[1102];
                    GameSystems.TextFloater.FloatLine(obj, TextFloaterCategory.Generic, TextFloaterColor.Red, lineText);
                }
            }
        }

        [TempleDllLocation(0x1007f590)]
        [TempleDllLocation(0x10059270)]
        public bool CanOpenPortals(GameObjectBody critter)
        {
            if (critter.IsPC())
            {
                return true;
            }
            else if (critter.IsNPC())
            {
                throw new NotImplementedException();
            }
            else
            {
                return false;
            }
        }

        [TempleDllLocation(0x1007FE90)]
        private static bool IsNotCharmedPartyMember(GameObjectBody obj)
        {
            if (!GameSystems.Party.IsInParty(obj))
            {
                return true;
            }

            if (GameSystems.D20.D20Query(obj, D20DispatcherKey.QUE_Critter_Is_Charmed) != 0)
            {
                return true;
            }

            var leader = GameSystems.D20.D20QueryReturnObject(obj, D20DispatcherKey.QUE_Critter_Is_Charmed);
            if (leader == null || !GameSystems.Party.IsInParty(leader))
            {
                return true;
            }

            return false;
        }

        private static bool IsCharmedBy(GameObjectBody critter, GameObjectBody byCritter)
        {
            if (GameSystems.D20.D20Query(critter, D20DispatcherKey.QUE_Critter_Is_Charmed) != 0)
            {
                var charmedBy = GameSystems.D20.D20QueryReturnObject(critter, D20DispatcherKey.QUE_Critter_Is_Charmed);
                return charmedBy == byCritter;
            }

            return false;
        }

        [TempleDllLocation(0x10080e00)]
        public bool IsFriendly(GameObjectBody critter1, GameObjectBody critter2)
        {
            if (critter1 == critter2)
            {
                return true;
            }

            // added to account for both being AI controlled (assumed friendly - TODO overhaul in the future!)
            if (GameSystems.D20.D20Query(critter1, D20DispatcherKey.QUE_Critter_Is_AIControlled) != 0
                && GameSystems.D20.D20Query(critter2, D20DispatcherKey.QUE_Critter_Is_AIControlled) != 0)
            {
                return true;
            }

            var critter1InParty = GameSystems.Party.IsInParty(critter1);
            var critter2InParty = GameSystems.Party.IsInParty(critter2);
            var critter1Leader = GetLeader(critter1);
            var critter2Leader = GetLeader(critter2);

            // if both are in party, or critter2's leader is in party
            if ((critter1InParty && critter2InParty)
                || (GameSystems.Party.IsInParty(critter2Leader) && critter1InParty)
                || (GameSystems.Party.IsInParty(critter1Leader) && critter2InParty))
            {
                // added the flip condition too () - was missing in vanilla, looked like a bug

                if (IsNotCharmedPartyMember(critter2) && IsNotCharmedPartyMember(critter1))
                    return true;
            }
            //else{
            //	// bug? was in vanilla code...
            //	if (critter1_in_party && GameSystems.Party.IsInParty(critter2_leader)){
            //		if (checkNotCharmedPartyMember(critter2) && checkNotCharmedPartyMember(critter1))
            //			return TRUE;
            //	}
            //}

            // if both are NPCs:
            if (critter1.IsNPC() && critter2.IsNPC())
            {
                if (GameSystems.D20.D20Query(critter1, D20DispatcherKey.QUE_Critter_Is_Charmed) == 0)
                {
                    if (critter1Leader == critter2)
                    {
                        return true;
                    }

                    if (critter2Leader == critter1
                        || NpcAllegianceShared(critter1, critter2)
                        || HasNoAllegiance(critter1) && HasNoAllegiance(critter2))
                    {
                        return true;
                    }
                }

                return false;
            }

            // in this section, at least one of the critters is a PC
            var pc = critter1;
            var npc = critter2;

            if (!pc.IsPC())
            {
                pc = critter2;
                npc = critter1;
                // they can't be both NPCs at this point - if they were, it'd have returned in the previous section.
            }

            if (!pc.IsPC())
            {
                return false; // just in case something that's not even a critter somehow got here
            }

            return IsCharmedBy(pc, npc);
        }

        private const int FACTION_ARRAY_MAX = 50;

        [TempleDllLocation(0x10080a70)]
        public bool NpcAllegianceShared(GameObjectBody critter1, GameObjectBody critter2)
        {
            GameObjectBody pc, npc;
            if (critter1.IsPC())
            {
                if (critter2.IsPC())
                {
                    return false;
                }

                pc = critter1;
                npc = critter2;
            }
            // handle1 is NPC
            else if (critter2.IsNPC())
            {
                // handle2 is also NPC
                var leader1 = GetLeader(critter1);
                var leader2 = GetLeader(critter2);

                // check leaders:
                // if one is the leader of the other, or their leaders are identical (and not null) - TRUE
                if (leader1 != null && leader1 == leader2
                    || leader1 == critter2
                    || leader2 == critter1)
                {
                    return true;
                }

                // check joint factions
                for (var factionIdx = 0; factionIdx < FACTION_ARRAY_MAX; factionIdx++)
                {
                    var objFaction = critter1.GetInt32(obj_f.npc_faction, factionIdx);
                    if (objFaction == 0)
                        return false;
                    if (HasFaction(critter2, objFaction))
                        return true;
                }

                // If no joint factions - return FALSE
                return false;
            }
            else
            {
                // handle2 is PC
                pc = critter2;
                npc = critter1;
            }

            var leader = GetLeader(npc);
            if (pc == leader)
                return true;

            for (var factionIdx = 0; factionIdx < FACTION_ARRAY_MAX; factionIdx++)
            {
                var objFaction = npc.GetInt32(obj_f.npc_faction, factionIdx);
                if (objFaction == 0)
                {
                    return false;
                }

                if (GameSystems.Reputation.HasFactionFromReputation(pc, objFaction))
                {
                    return true;
                }
            }

            return false;
        }

        [TempleDllLocation(0x1007E430)]
        public bool HasFaction(GameObjectBody critter, int faction)
        {
            for (int i = 0; i < FACTION_ARRAY_MAX; i++)
            {
                var npcFaction = critter.GetInt32(obj_f.npc_faction, i);
                if (npcFaction == 0)
                {
                    return false; // Array terminator
                }

                if (npcFaction == faction)
                {
                    return true;
                }
            }

            return false;
        }

        [TempleDllLocation(0x1007e480)]
        public void AddFaction(GameObjectBody critter, int factionId)
        {
            if (!critter.IsNPC())
            {
                return;
            }

            var factionCount = 0;
            while (factionCount < FACTION_ARRAY_MAX && critter.GetInt32(obj_f.npc_faction, factionCount) != 0)
            {
                factionCount++;
            }

            critter.SetInt32(obj_f.npc_faction, factionCount, factionId);
            critter.SetInt32(obj_f.npc_faction, factionCount + 1, 0);

            if (factionCount == FACTION_ARRAY_MAX)
            {
                Logger.Warn("Critter {0} has too many factions, cannot add more.", critter);
            }
        }

        [TempleDllLocation(0x1007e510)]
        public bool HasNoAllegiance(GameObjectBody critter)
        {
            if (critter.IsNPC())
            {
                if (GameSystems.Party.IsInParty(critter))
                {
                    return false;
                }
                else
                {
                    return critter.GetInt32(obj_f.npc_faction, 0) == 0;
                }
            }

            return true;
        }

        [TempleDllLocation(0x10080670)]
        public void SetConcealed(GameObjectBody obj, bool concealed)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x100805c0)]
        public void SetMovingSilently(GameObjectBody critter, bool enable)
        {
            void SetMovingSilentlyFlag(GameObjectBody obj)
            {
                var critterFlags = obj.GetCritterFlags();
                if (enable)
                {
                    critterFlags |= CritterFlag.MOVING_SILENTLY;
                }
                else
                {
                    critterFlags &= ~CritterFlag.MOVING_SILENTLY;
                }

                obj.SetCritterFlags(critterFlags);
            }

            SetMovingSilentlyFlag(critter);

            // Apply the sneak state to all followers
            foreach (var follower in EnumerateAllFollowers(critter))
            {
                SetMovingSilentlyFlag(follower);
            }
        }

        [TempleDllLocation(0x10AB73E0)]
        private GameObjectBody _critterCurrentlyDying;

        [TempleDllLocation(0x100810a0)]
        public void HandleDeath(GameObjectBody critter, GameObjectBody killer, EncodedAnimId deathAnim)
        {
            GameSystems.TextFloater.CritterDied(critter);
            if (_critterCurrentlyDying != critter)
            {
                var previousCritterDying = _critterCurrentlyDying;
                _critterCurrentlyDying.id = critter.id;
                if (GameSystems.Script.ExecuteObjectScript(killer, critter, 0, 0, ObjScriptEvent.Dying, 0) != 0)
                {
                    _critterCurrentlyDying = previousCritterDying;
                    if (!critter.HasFlag(ObjectFlag.DESTROYED))
                    {
                        GameSystems.D20.ObjectRegistry.SendSignalAll(D20DispatcherKey.SIG_Critter_Killed, critter);
                        GameSystems.Combat.CritterLeaveCombat(critter);

                        // TODO This seems HIGHLY suspect!
                        var secondsInMinute = GameSystems.TimeEvent.GameTime.Seconds % 60;
                        critter.SetInt32(obj_f.critter_death_time, (int) secondsInMinute);

                        if (critter.IsNPC())
                        {
                            GameSystems.Teleport.RemoveDayNightTransfer(critter);
                            QueueWipeCombatFocus(critter);
                            critter.SetObject(obj_f.npc_combat_focus, killer);
                            GameSystems.AI.CritterKilled(critter, killer);
                            if (!killer.IsPC())
                            {
                                killer = GetLeaderRecursive(killer);
                            }

                            if (killer != null)
                            {
                                foreach (var follower in EnumerateDirectFollowers(killer))
                                {
                                    GameSystems.Script.ExecuteObjectScript(critter,
                                        follower, killer, ObjScriptEvent.LeaderKilling, 0);
                                }
                            }

                            var leader = GetLeader(critter);
                            if (leader != null)
                            {
                                RemoveFollower(critter, true);
                            }

                            SetNpcLeader(critter, leader);
                            GameSystems.MonsterGen.CritterKilled(critter);
                            if (!critter.GetCritterFlags2().HasFlag(CritterFlag2.NO_DECAY))
                            {
                                AddDecayBodyTimer(critter);
                            }
                        }

                        if (!IsProne(critter) &&
                            GameSystems.D20.D20Query(critter, D20DispatcherKey.QUE_Unconscious) == 0)
                        {
                            GameSystems.Anim.PushDying(critter, deathAnim);
                        }

                        GameUiBridge.UpdatePartyUi();
                    }
                }
                else
                {
                    _critterCurrentlyDying = previousCritterDying;
                }
            }
        }

        [TempleDllLocation(0x1007f2c0)]
        private void AddDecayBodyTimer(GameObjectBody obj)
        {
            var evt = new TimeEvent(TimeEventType.DecayDeadBodies);
            evt.arg1.handle = obj;
            evt.arg2.int32 = (int) GameSystems.TimeEvent.GameTime.Seconds;
            int delay;
            if (!obj.IsCritter())
            {
                delay = 172800000; // 48 hours
            }
            else
            {
                delay = 86400000; // 24 hours
            }

            GameSystems.TimeEvent.Schedule(evt, delay, out _);
        }

        [TempleDllLocation(0x1007eaf0)]
        public void SetNpcLeader(GameObjectBody npc, GameObjectBody leader)
        {
            if (npc.IsNPC())
            {
                npc.SetObject(obj_f.npc_leader, leader);
            }
        }

        [TempleDllLocation(0x1007f360)]
        private void QueueWipeCombatFocus(GameObjectBody critter)
        {
            var evt = new TimeEvent(TimeEventType.CombatFocusWipe);
            evt.arg1.handle = critter;
            evt.arg2.int32 = (int) GameSystems.TimeEvent.GameTime.Seconds;
            GameSystems.TimeEvent.Schedule(evt, 600000, out _);
        }

        public bool HasDomain(GameObjectBody caster, DomainId domain)
        {
            return caster.GetStat(Stat.domain_1) == (int) domain || caster.GetStat(Stat.domain_2) == (int) domain;
        }

        [TempleDllLocation(0x100801d0)]
        public int GetHitDiceNum(GameObjectBody critter)
        {
            var hitDice = 0;
            if (critter.IsNPC())
            {
                hitDice = critter.GetInt32(obj_f.npc_hitdice_idx, 0);
            }

            return hitDice + critter.GetInt32Array(obj_f.critter_level_idx).Count;
        }

        public int NumOffhandExtraAttacks(GameObjectBody critter)
        {
            if (GameSystems.Feat.HasFeat(critter, FeatId.GREATER_TWO_WEAPON_FIGHTING)
                || GameSystems.Feat.HasFeat(critter, FeatId.GREATER_TWO_WEAPON_FIGHTING_RANGER))
            {
                return 3;
            }

            if (GameSystems.Feat.HasFeat(critter, FeatId.IMPROVED_TWO_WEAPON_FIGHTING)
                || GameSystems.Feat.HasFeat(critter, FeatId.IMPROVED_TWO_WEAPON_FIGHTING_RANGER))
                return 2;

            return 1;
        }
    }

    public static class CritterExtensions
    {
        public static bool IsDeadOrUnconscious(this GameObjectBody critter) =>
            GameSystems.Critter.IsDeadOrUnconscious(critter);

        [TempleDllLocation(0x1001f3b0)]
        public static IEnumerable<GameObjectBody> EnumerateFollowers(this GameObjectBody critter,
            bool recursive = false)
        {
            var followers = critter.GetObjectArray(obj_f.critter_follower_idx);
            foreach (var follower in followers)
            {
                if (follower != null)
                {
                    yield return follower;
                }
            }

            if (recursive)
            {
                foreach (var follower in EnumerateFollowers(critter, false))
                {
                    foreach (var transitiveFollower in follower.EnumerateFollowers(true))
                    {
                        yield return transitiveFollower;
                    }
                }
            }
        }

        public static bool HasRangedWeaponEquipped(this GameObjectBody obj)
        {
            var weapon = GameSystems.Item.ItemWornAt(obj, EquipSlot.WeaponPrimary);
            if (weapon == null)
            {
                weapon = GameSystems.Item.ItemWornAt(obj, EquipSlot.WeaponSecondary);
            }

            if (weapon == null)
            {
                return false;
            }

            return weapon.WeaponFlags.HasFlag(WeaponFlag.RANGED_WEAPON);
        }

        /// <summary>
        /// Returns the critter's reach in feet.
        /// </summary>
        [TempleDllLocation(0x100b52d0)]
        public static float GetReach(this GameObjectBody obj,
            D20ActionType actionType = D20ActionType.UNSPECIFIED_ATTACK)
        {
            float naturalReach = obj.GetInt32(obj_f.critter_reach);

            var protoId = GameSystems.D20.D20Query(obj, D20DispatcherKey.QUE_Polymorphed);
            if (protoId != 0)
            {
                var protoHandle = GameSystems.Proto.GetProtoById((ushort) protoId);
                if (protoHandle != null)
                {
                    naturalReach = protoHandle.GetInt32(obj_f.critter_reach);
                }
            }

            if (naturalReach < 0.01f)
            {
                naturalReach = 5.0f;
            }

            if (actionType != D20ActionType.TOUCH_ATTACK)
            {
                var weapon = GameSystems.Item.ItemWornAt(obj, EquipSlot.WeaponPrimary);
                // todo: handle cases where enlarged creatures dual wield polearms ><
                if (weapon != null)
                {
                    var weapType = weapon.GetWeaponType();
                    if (GameSystems.Weapon.IsReachWeaponType(weapType))
                    {
                        return naturalReach + 3.0f; // +5.0 - 2.0
                    }

                    return naturalReach - 2.0f;
                }
            }

            return naturalReach - 2.0f;
        }

        public static bool HasNaturalAttacks(this GameObjectBody critter)
        {
            if (critter.GetInt32(obj_f.critter_attacks_idx, 0) != 0)
            {
                return true;
            }

            // check polymorphed
            var protoId = GameSystems.D20.D20Query(critter, D20DispatcherKey.QUE_Polymorphed);
            if (protoId == 0)
            {
                return false;
            }

            var protoObj = GameSystems.Proto.GetProtoById((ushort) protoId);
            if (protoObj == null)
            {
                return false;
            }

            return protoObj.GetInt32(obj_f.critter_attacks_idx, 0) != 0;
        }


        public static bool IsWearingLightArmorOrLess(this GameObjectBody critter)
        {
            var armor = GameSystems.Item.ItemWornAt(critter, EquipSlot.Armor);
            if (armor == null || armor.type != ObjectType.armor)
            {
                return true; // Wearing no armor at all
            }

            var armorFlags = armor.GetArmorFlags();
            if (armorFlags.HasFlag(ArmorFlag.TYPE_NONE))
            {
                return true; // Marked as not being armor
            }

            return armorFlags.GetArmorType() == ArmorFlag.TYPE_LIGHT;
        }

        public static bool IsWearingMediumArmorOrLess(this GameObjectBody critter)
        {
            var armor = GameSystems.Item.ItemWornAt(critter, EquipSlot.Armor);
            if (armor == null || armor.type != ObjectType.armor)
            {
                return true; // Wearing no armor at all
            }

            var armorFlags = armor.GetArmorFlags();
            if (armorFlags.HasFlag(ArmorFlag.TYPE_NONE))
            {
                return true; // Marked as not being armor
            }

            return armorFlags.GetArmorType() == ArmorFlag.TYPE_LIGHT;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using OpenTemple.Core.GameObject;
using OpenTemple.Core.GFX;
using OpenTemple.Core.IO;
using OpenTemple.Core.Location;
using OpenTemple.Core.Logging;
using OpenTemple.Core.Systems.Anim;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Systems.D20.Actions;
using OpenTemple.Core.Systems.D20.Classes;
using OpenTemple.Core.Systems.D20.Conditions;
using OpenTemple.Core.Systems.Dialog;
using OpenTemple.Core.Systems.Feats;
using OpenTemple.Core.Systems.GameObjects;
using OpenTemple.Core.Systems.ObjScript;
using OpenTemple.Core.Systems.Spells;
using OpenTemple.Core.Systems.TimeEvents;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Ui;
using OpenTemple.Core.Ui.InGameSelect;
using OpenTemple.Core.Utils;

namespace OpenTemple.Core.Systems.Script.Extensions
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class PythonNameAttribute : Attribute
    {
        public string Name { get; }

        public PythonNameAttribute(string name)
        {
            Name = name;
        }
    }

    public static class ScriptObjectExtensions
    {
        private static readonly ILogger Logger = LoggingSystem.CreateLogger();

        [TempleDllLocation(0x100af4f0)]
        [PythonName("begin_dialog")]
        public static void BeginDialog(this GameObjectBody pc, GameObjectBody npc, int line)
        {
            var evt = new TimeEvent(TimeEventType.PythonDialog);
            evt.arg1.handle = pc;
            evt.arg2.handle = npc;
            evt.arg3.int32 = line;
            GameSystems.TimeEvent.ScheduleNow(evt);
            UiSystems.InGameSelect.CancelPicker();
        }

        [TempleDllLocation(0x100af5a0)]
        [PythonName("reaction_get")]
        public static int GetReaction(this GameObjectBody critter, GameObjectBody towards)
        {
            return GameSystems.Reaction.GetReaction(critter, towards);
        }

        [TempleDllLocation(0x100af5e0)]
        [PythonName("reaction_set")]
        public static void SetReaction(this GameObjectBody critter, GameObjectBody towards, int newReaction)
        {
            var current = GameSystems.Reaction.GetReaction(critter, towards);
            GameSystems.Reaction.AdjustReaction(critter, towards, newReaction - current);
        }

        [TempleDllLocation(0x100af670)]
        [PythonName("reaction_adj")]
        public static void AdjustReaction(this GameObjectBody critter, GameObjectBody towards, int adjustment)
        {
            GameSystems.Reaction.AdjustReaction(critter, towards, adjustment);
        }

        [TempleDllLocation(0x100af6e0)]
        [PythonName("item_find")]
        public static GameObjectBody FindItemByName(this GameObjectBody container, int nameId)
        {
            return ItemExtensions.FindItemByName(container, nameId);
        }

        [TempleDllLocation(0x100af750)]
        [PythonName("item_transfer_to")]
        public static bool TransferItemByNameTo(this GameObjectBody container, GameObjectBody transferTo, int nameId)
        {
            var item = container.FindItemByName(nameId);
            if (item != null)
            {
                return GameSystems.Item.SetItemParent(item, transferTo);
            }

            return false;
        }

        [TempleDllLocation(0x100af7d0)]
        [PythonName("item_find_by_proto")]
        public static GameObjectBody FindItemByProto(this GameObjectBody container, int protoId)
        {
            return ItemExtensions.FindItemByProto(container, protoId);
        }

        [TempleDllLocation(0x100af850)]
        [PythonName("item_transfer_to_by_proto")]
        public static bool TransferItemByProtoTo(this GameObjectBody container, GameObjectBody transferTo, int protoId)
        {
            var item = container.FindItemByName(protoId);
            if (item != null)
            {
                return GameSystems.Item.SetItemParent(item, transferTo);
            }

            return false;
        }

        [TempleDllLocation(0x100af8d0)]
        [PythonName("money_get")]
        public static int GetMoney(this GameObjectBody obj)
        {
            return obj.GetStat(Stat.money);
        }

        [TempleDllLocation(0x100af910)]
        [PythonName("money_adj")]
        public static void AdjustMoney(this GameObjectBody obj, int copperCoins)
        {
            if (copperCoins <= 0)
            {
                GameSystems.Critter.TakeMoney(obj, 0, 0, 0, -copperCoins);
            }
            else
            {
                GameSystems.Critter.GiveMoney(obj, 0, 0, 0, copperCoins);
            }
        }

        [TempleDllLocation(0x100af9a0)]
        [PythonName("cast_spell")]
        public static void CastSpell(this GameObjectBody caster, int spellEnum, GameObjectBody targetObj = null)
        {
            var spellClasses = new List<int>();
            var spellLevels = new List<int>();

            if (!GameSystems.Spell.TryGetSpellEntry(spellEnum, out var spellEntry))
            {
                Logger.Warn("Trying to cast unknown spell: {0}", spellEnum);
                return;
            }

            SpellPacketBody spellPktBody = new SpellPacketBody();
            spellPktBody.spellEnum = spellEnum;
            spellPktBody.spellEnumOriginal = spellEnum;
            spellPktBody.caster = caster;
            if (!GameSystems.Spell.SpellKnownQueryGetData(caster, spellEnum, spellClasses, spellLevels))
            {
                return;
            }

            for (var i = 0; i < spellClasses.Count; i = ++i)
            {
                if (!GameSystems.Spell.spellCanCast(caster, spellEnum, spellClasses[i], spellLevels[i]))
                {
                    continue;
                }

                spellPktBody.spellKnownSlotLevel = spellLevels[i];
                spellPktBody.spellClass = spellClasses[i];
                GameSystems.Spell.SpellPacketSetCasterLevel(spellPktBody);

                var pickArgs = new PickerArgs();
                GameSystems.Spell.PickerArgsFromSpellEntry(spellEntry, pickArgs, caster, spellPktBody.casterLevel);
                pickArgs.flagsTarget &= ~UiPickerFlagsTarget.Range;
                pickArgs.flagsTarget |= UiPickerFlagsTarget.LosNotRequired;

                LocAndOffsets? loc = null;
                if (pickArgs.modeTarget.IsBaseMode(UiPickerType.Single)
                    || pickArgs.modeTarget.IsBaseMode(UiPickerType.Multi))
                {
                    loc = targetObj?.GetLocationFull();
                    pickArgs.SetSingleTgt(targetObj);
                }
                else if (pickArgs.modeTarget.IsBaseMode(UiPickerType.Cone))
                {
                    loc = targetObj?.GetLocationFull();
                    if (loc.HasValue)
                    {
                        pickArgs.SetConeTargets(loc.Value);
                    }
                }
                else if (pickArgs.modeTarget.IsBaseMode(UiPickerType.Area))
                {
                    if (spellEntry.spellRangeType == SpellRangeType.SRT_Personal)
                    {
                        loc = caster.GetLocationFull();
                    }
                    else
                    {
                        loc = targetObj?.GetLocationFull();
                    }

                    if (loc.HasValue)
                    {
                        pickArgs.SetAreaTargets(loc.Value);
                    }
                }
                else if (pickArgs.modeTarget.IsBaseMode(UiPickerType.Personal))
                {
                    loc = caster.GetLocationFull();
                    pickArgs.SetSingleTgt(caster);
                }

                GameSystems.Spell.ConfigSpellTargetting(pickArgs, spellPktBody);
                if (spellPktBody.Targets.Length > 0 && GameSystems.D20.Actions.TurnBasedStatusInit(caster))
                {
                    GameSystems.D20.Actions.GlobD20ActnInit();
                    GameSystems.D20.Actions.GlobD20ActnSetTypeAndData1(D20ActionType.CAST_SPELL, 0);
                    GameSystems.D20.Actions.ActSeqCurSetSpellPacket(spellPktBody, true);
                    var spellData = new D20SpellData(spellEnum, spellClasses[i], spellLevels[i]);
                    GameSystems.D20.Actions.GlobD20ActnSetSpellData(spellData);
                    GameSystems.D20.Actions.GlobD20ActnSetTarget(targetObj, loc);
                    GameSystems.D20.Actions.ActionAddToSeq();
                    GameSystems.D20.Actions.sequencePerform();
                    break;
                }
            }
        }

        [PythonName("skill_level_get")]
        public static int GetSkillLevel(this GameObjectBody obj, SkillId skill)
        {
            return GetSkillLevel(obj, null, skill);
        }

        [TempleDllLocation(0x100afd30)]
        [PythonName("skill_level_get")]
        public static int GetSkillLevel(this GameObjectBody obj, GameObjectBody against, SkillId skill)
        {
            return obj.dispatch1ESkillLevel(skill, against, SkillCheckFlags.UnderDuress);
        }

        [TempleDllLocation(0x100afe00)]
        [PythonName("has_met")]
        public static bool HasMet(this GameObjectBody npc, GameObjectBody pc)
        {
            return GameSystems.Reaction.HasMet(npc, pc);
        }

        public static int GetNameId(this GameObjectBody critter) => critter.GetInt32(obj_f.name);

        [TempleDllLocation(0x100afeb0)]
        [PythonName("has_follower")]
        public static bool HasFollowerByName(this GameObjectBody critter, int nameId)
        {
            foreach (var follower in GameSystems.Critter.EnumerateAllFollowers(critter))
            {
                if (follower.GetInt32(obj_f.name) == nameId)
                {
                    return true;
                }
            }

            return false;
        }

        [TempleDllLocation(0x100aff50)]
        [PythonName("group_list")]
        public static IEnumerable<GameObjectBody> GetPartyMembers(this GameObjectBody obj)
        {
            return GameSystems.Party.PartyMembers;
        }

        [TempleDllLocation(0x100affd0)]
        [PythonName("stat_level_get")]
        private static int GetStat(this GameObjectBody obj, Stat stat)
        {
            return D20StatExtensions.GetStat(obj, stat);
        }

        [TempleDllLocation(0x100b0020)]
        [PythonName("stat_base_get")]
        private static int GetBaseStat(this GameObjectBody obj, Stat stat)
        {
            return D20StatExtensions.GetBaseStat(obj, stat);
        }

        [TempleDllLocation(0x100b0070)]
        [PythonName("stat_base_set")]
        public static int SetBaseStat(this GameObjectBody obj, Stat stat, int value)
        {
            return GameSystems.Stat.SetBasicStat(obj, stat, value);
        }

        [TempleDllLocation(0x100b00d0)]
        [PythonName("follower_add")]
        public static bool AddFollower(this GameObjectBody leader, GameObjectBody follower)
        {
            var result = GameSystems.Critter.AddFollower(follower, leader, true, false);
            GameUiBridge.UpdatePartyUi();
            return result;
        }

        [TempleDllLocation(0x100b0130)]
        [PythonName("follower_remove")]
        public static bool RemoveFollower(this GameObjectBody obj, GameObjectBody follower)
        {
            var result = GameSystems.Critter.RemoveFollower(follower, true);
            GameUiBridge.UpdatePartyUi();
            return result;
        }

        [TempleDllLocation(0x100b0180)]
        [PythonName("follower_atmax")]
        public static bool HasMaxFollowers(this GameObjectBody obj)
        {
            return GameSystems.Party.HasMaxNPCFollowers;
        }

        [TempleDllLocation(0x100b01a0)]
        [PythonName("ai_follower_add")]
        public static bool AddAIFollower(this GameObjectBody obj, GameObjectBody follower)
        {
            var result = GameSystems.Critter.AddFollower(follower, obj, true, true);
            GameUiBridge.UpdatePartyUi();
            return result;
        }

        [TempleDllLocation(0x100b0200)]
        [PythonName("ai_follower_remove")]
        public static bool RemoveAIFollower(this GameObjectBody obj, GameObjectBody follower)
        {
            throw new NotImplementedException(); // lol, okay! original returned 0!
        }

        [TempleDllLocation(0x100b0200)]
        [PythonName("ai_follower_atmax")]
        public static bool HasMaxAIFollowers(this GameObjectBody obj)
        {
            return false; // TODO Participation medal for ToEE
        }

        [PythonName("obj_remove_from_all_groups")]
        public static void RemoveFromAllGroups(this GameObjectBody critter)
        {
            GameSystems.Party.RemoveFromAllGroups(critter);
            UiSystems.Party.Update();
        }

        [TempleDllLocation(0x100b0210)]
        [PythonName("leader_get")]
        private static GameObjectBody GetLeader(this GameObjectBody obj)
        {
            return GameSystems.Critter.GetLeader(obj);
        }

        [TempleDllLocation(0x100b3a30)]
        [PythonName("can_see")]
        [PythonName("has_los")]
        public static bool HasLineOfSight(this GameObjectBody obj, GameObjectBody target)
        {
            return GameSystems.AI.HasLineOfSight(obj, target) == 0;
        }

        [TempleDllLocation(0x100b0260)]
        [PythonName("has_wielded")]
        public static bool HasEquippedByName(this GameObjectBody obj, int nameId)
        {
            foreach (var item in obj.EnumerateEquipment())
            {
                if (item.Value.GetInt32(obj_f.name) == nameId)
                {
                    return true;
                }
            }

            return false;
        }

        [TempleDllLocation(0x100b02e0)]
        [PythonName("has_item")]
        public static bool HasItemByName(this GameObjectBody container, int nameId)
        {
            return container.FindItemByName(nameId) != null;
        }

        [PythonName("inventory_item")]
        public static GameObjectBody GetInventoryItem(this GameObjectBody obj, int slot)
        {
            return GameSystems.Item.GetItemAtInvIdx(obj, slot);
        }

        [TempleDllLocation(0x100b0340)]
        [PythonName("item_worn_at")]
        public static GameObjectBody ItemWornAt(this GameObjectBody obj, EquipSlot slot)
        {
            return GameSystems.Item.ItemWornAt(obj, slot);
        }

        [PythonName("divine_spell_level_can_cast")]
        public static int DivineSpellLevelCanCast(this GameObjectBody obj)
        {
            var spellLvlMax = 0;

            foreach (var classEnum in D20ClassSystem.Classes.Keys) {
                if (D20ClassSystem.IsDivineCastingClass(classEnum)) {
                    spellLvlMax = Math.Max(spellLvlMax, GameSystems.Spell.GetMaxSpellLevel(obj, classEnum, 0));
                }
            }

            return spellLvlMax;
        }

        [PythonName("arcane_spell_level_can_cast")]
        public static int ArcaneSpellLevelCanCast(this GameObjectBody obj)
        {
            var spellLvlMax = 0;

            foreach (var classEnum in D20ClassSystem.Classes.Keys) {
                if (D20ClassSystem.IsArcaneCastingClass(classEnum)) {
                    spellLvlMax = Math.Max(spellLvlMax, GameSystems.Spell.GetMaxSpellLevel(obj, classEnum, 0));
                }
            }

            return spellLvlMax;
        }

        [TempleDllLocation(0x100b03c0)]
        [PythonName("attack")]
        public static void Attack(this GameObjectBody obj, GameObjectBody target)
        {
            if (!GameSystems.Party.IsInParty(target))
            {
                GameSystems.AI.ProvokeHostility(obj, target, 1, 2);
            }
            else
            {
                foreach (var partyMember in GameSystems.Party.PartyMembers)
                {
                    if (GameSystems.Party.IsPlayerControlled(partyMember))
                    {
                        GameSystems.AI.ProvokeHostility(target, obj, 1, 2);
                    }
                }
            }
        }

        [TempleDllLocation(0x100b04a0)]
        [PythonName("turn_towards")]
        public static void TurnTowards(this GameObjectBody obj, GameObjectBody target)
        {
            var rotation = obj.RotationTo(target);
            GameSystems.Anim.PushRotate(obj, rotation);
        }

        [TempleDllLocation(0x100b0510)]
        [PythonName("float_line")]
        public static void FloatLine(this GameObjectBody obj, int line, GameObjectBody listener)
        {
            var script = obj.GetScript(obj_f.scripts_idx, (int) ObjScriptEvent.Dialog);
            if (!GameSystems.ScriptName.TryGetDialogScriptPath(script.scriptId, out var dialogScriptPath))
            {
                throw new ArgumentException($"Invalid dialog script {script.scriptId} attached to NPC.");
            }

            throw new NotImplementedException();
        }

        public static void FloatLine(this GameObjectBody obj, string text, TextFloaterColor color = TextFloaterColor.White)
        {
            GameSystems.TextFloater.FloatLine(obj, TextFloaterCategory.Generic, color, text);
        }

        [TempleDllLocation(0x100b0690)]
        [PythonName("damage")]
        public static void Damage(this GameObjectBody victim, GameObjectBody attacker, DamageType damageType, Dice dice,
            D20AttackPower attackPower = default, D20ActionType actionType = D20ActionType.NONE)
        {
            GameSystems.D20.Combat.DoUnclassifiedDamage(victim, attacker, dice, damageType, attackPower, actionType);
        }

        [TempleDllLocation(0x100b0740)]
        [PythonName("damage_with_reduction")]
        public static void DamageWithReduction(this GameObjectBody victim, GameObjectBody attacker,
            DamageType damageType,
            Dice dice, D20AttackPower attackPower, int reductionPercent, D20ActionType actionType = D20ActionType.NONE,
            int spellId = -1)
        {
            //  NOTE: reductionPercent is in the 0-100 range
            GameSystems.D20.Combat.DoDamage(victim, attacker, dice, damageType, attackPower, reductionPercent, 105,
                actionType);
        }

        [TempleDllLocation(0x100b09b0)]
        [PythonName("heal")]
        public static void Heal(this GameObjectBody obj, GameObjectBody healer, Dice dice,
            D20ActionType actionType = D20ActionType.NONE, int spellId = 0)
        {
            GameSystems.Combat.Heal(obj, healer, dice, actionType);
        }

        [TempleDllLocation(0x100b0b00)]
        [PythonName("healsubdual")]
        public static void HealSubdual(this GameObjectBody obj, GameObjectBody healer, Dice dice,
            D20ActionType actionType = D20ActionType.NONE, int spellId = 0)
        {
            var amount = dice.Roll();
            GameSystems.Critter.HealSubdualSub_100B9030(obj, amount);
        }

        [TempleDllLocation(0x100b1390)]
        [PythonName("steal_from")]
        public static void StealFrom(this GameObjectBody obj, GameObjectBody target)
        {
            GameSystems.Anim.PushUseSkillOn(obj, target, SkillId.pick_pocket);
        }

        [TempleDllLocation(0x100b1400)]
        [PythonName("reputation_has")]
        public static bool HasReputation(this GameObjectBody obj, int reputation)
        {
            return GameSystems.Reputation.HasReputation(obj, reputation);
        }

        [TempleDllLocation(0x100b1460)]
        [PythonName("reputation_add")]
        public static void AddReputation(this GameObjectBody obj, int reputation)
        {
            // TODO: I find this kinda shoddy
            var leader = GameSystems.Party.GetLeader();
            GameSystems.Reputation.AddReputation(leader, reputation);
        }

        [TempleDllLocation(0x100b14c0)]
        [PythonName("reputation_remove")]
        public static void RemoveReputation(this GameObjectBody obj, int reputation)
        {
            // TODO: I find this kinda shoddy
            var leader = GameSystems.Party.GetLeader();
            GameSystems.Reputation.RemoveReputation(leader, reputation);
        }

// condition_add_with_args has been deprecated, use condition_add and add_with_args instead
// TODO: Still being used a ton in scripts though...
        [TempleDllLocation(0x100b1520)]
        [PythonName("item_condition_add_with_args")]
        public static void AddConditionToItem(this GameObjectBody item, string conditionName, params int[] args)
        {
            var condition = GameSystems.D20.Conditions[conditionName];
            if (condition == null)
            {
                throw new ArgumentException($"Unknown condition: '{conditionName}'");
            }

            item.AddConditionToItem(condition, args);
        }

        // condition_add_with_args has been deprecated, use condition_add and add_with_args instead
        [TempleDllLocation(0x100b1620)]
        [TempleDllLocation(0x100b17b0)]
        [PythonName("condition_add")]
        [PythonName("condition_add_with_args")]
        private static bool AddCondition(this GameObjectBody obj, string conditionName, params object[] args)
        {
            return ConditionExtensions.AddCondition(obj, conditionName, args);
        }

        [TempleDllLocation(0x100b1810)]
        [PythonName("is_friendly")]
        public static bool IsFriendly(this GameObjectBody obj, GameObjectBody source)
        {
            return GameSystems.Critter.IsFriendly(source, obj);
        }

        [TempleDllLocation(0x100b1880)]
        [PythonName("fade_to")]
        public static void FadeTo(this GameObjectBody obj, int goalOpacity, int timeTickMs, int fadeQuantum,
            FadeOutResult callback = FadeOutResult.None)
        {
            GameSystems.ObjFade.FadeTo(obj, goalOpacity, timeTickMs, fadeQuantum, callback);
        }

        [TempleDllLocation(0x100b18f0)]
        [PythonName("move")]
        public static void Move(this GameObjectBody obj, locXY location, float offsetX = 0, float offsetY = 0)
        {
            GameSystems.MapObject.Move(obj, new LocAndOffsets(location, offsetX, offsetY));
        }

        [TempleDllLocation(0x100b18f0)]
        [PythonName("move")]
        public static void Move(this GameObjectBody obj, LocAndOffsets location)
        {
            GameSystems.MapObject.Move(obj, location);
        }

        [TempleDllLocation(0x100b1970)]
        [PythonName("float_mesfile_line")]
        public static void FloatMesFileLine(this GameObjectBody obj, string path, int line,
            TextFloaterColor color = TextFloaterColor.White)
        {
            // TODO: This should be changed to something more sensible and cached
            var mesLines = Tig.FS.ReadMesFile(path);
            var lineText = mesLines[line];
            GameSystems.TextFloater.FloatLine(obj, TextFloaterCategory.Generic, color, lineText);
        }

        [TempleDllLocation(0x100b1f30)]
        [PythonName("object_flags_get")]
        public static ObjectFlag GetObjectFlags(this GameObjectBody obj)
        {
            return obj.GetFlags();
        }

        [TempleDllLocation(0x100b1f50)]
        [PythonName("object_flag_set")]
        public static void SetObjectFlag(this GameObjectBody obj, ObjectFlag flag)
        {
            GameSystems.MapObject.SetFlags(obj, flag);
        }

        [TempleDllLocation(0x100b1fd0)]
        [PythonName("object_flag_unset")]
        public static void ClearObjectFlag(this GameObjectBody obj, ObjectFlag flag)
        {
            GameSystems.MapObject.ClearFlags(obj, flag);
        }

        [TempleDllLocation(0x100b20b0)]
        [PythonName("portal_flags_get")]
        public static PortalFlag GetPortalFlags(this GameObjectBody obj)
        {
            return obj.GetPortalFlags();
        }

        [TempleDllLocation(0x100b20d0)]
        [PythonName("portal_flag_set")]
        public static void SetPortalFlag(this GameObjectBody obj, PortalFlag flag)
        {
            obj.SetPortalFlags(obj.GetPortalFlags() | flag);
        }

        [TempleDllLocation(0x100b2170)]
        [PythonName("portal_flag_unset")]
        public static void ClearPortalFlag(this GameObjectBody obj, PortalFlag flag)
        {
            obj.SetPortalFlags(obj.GetPortalFlags() & ~flag);
        }

        [TempleDllLocation(0x100b2210)]
        [PythonName("container_flags_get")]
        public static ContainerFlag GetContainerFlags(this GameObjectBody obj)
        {
            return obj.GetContainerFlags();
        }

        [TempleDllLocation(0x100b2230)]
        [PythonName("container_flag_set")]
        public static void SetContainerFlag(this GameObjectBody obj, ContainerFlag flag)
        {
            obj.SetContainerFlags(obj.GetContainerFlags() | flag);
        }

        [TempleDllLocation(0x100b22d0)]
        [PythonName("container_flag_unset")]
        public static void ClearContainerFlag(this GameObjectBody obj, ContainerFlag flag)
        {
            obj.SetContainerFlags(obj.GetContainerFlags() & ~flag);
        }

        [TempleDllLocation(0x100b2050)]
        [PythonName("portal_toggle_open")]
        private static void TogglePortalOpen(this GameObjectBody obj)
        {
            PortalExtensions.TogglePortalOpen(obj);
        }

        [TempleDllLocation(0x100b2080)]
        [TempleDllLocation(0x1010ea00)]
        [PythonName("container_toggle_open")]
        public static void ToggleContainerOpen(this GameObjectBody obj)
        {
            var flags = obj.GetContainerFlags() ^ ContainerFlag.OPEN;
            obj.SetContainerFlags(flags);

            if ((flags & ContainerFlag.OPEN) != 0)
            {
                GameSystems.Anim.PushAnimate(obj, NormalAnimType.Open);
            }
            else
            {
                GameSystems.Anim.PushAnimate(obj, NormalAnimType.Close);
            }
        }


        [TempleDllLocation(0x100b1dc0)]
        [PythonName("item_flags_get")]
        public static ItemFlag GetItemFlags(this GameObjectBody obj)
        {
            return obj.GetItemFlags();
        }

        [TempleDllLocation(0x100b1df0)]
        [PythonName("item_flag_set")]
        public static void SetItemFlag(this GameObjectBody obj, ItemFlag flag)
        {
            obj.SetItemFlag(flag, true);
        }

        [TempleDllLocation(0x100b1e90)]
        [PythonName("item_flag_unset")]
        public static void ClearItemFlag(this GameObjectBody obj, ItemFlag flag)
        {
            obj.SetItemFlag(flag, false);
        }

        [TempleDllLocation(0x100b1ae0)]
        [PythonName("critter_flags_get")]
        public static CritterFlag GetCritterFlags(this GameObjectBody obj)
        {
            return obj.GetCritterFlags();
        }

        [TempleDllLocation(0x100b1b10)]
        [PythonName("critter_flag_set")]
        public static void SetCritterFlag(this GameObjectBody obj, CritterFlag flag)
        {
            obj.SetCritterFlags(obj.GetCritterFlags() | flag);
        }

        [TempleDllLocation(0x100b1bb0)]
        [PythonName("critter_flag_unset")]
        public static void ClearCritterFlag(this GameObjectBody obj, CritterFlag flag)
        {
            obj.SetCritterFlags(obj.GetCritterFlags() & ~flag);
        }

        [TempleDllLocation(0x100b1c50)]
        [PythonName("npc_flags_get")]
        public static NpcFlag GetNpcFlags(this GameObjectBody obj)
        {
            return obj.GetNPCFlags();
        }

        [TempleDllLocation(0x100b1c80)]
        [PythonName("npc_flag_set")]
        public static void SetNpcFlag(this GameObjectBody obj, NpcFlag flag)
        {
            obj.SetNPCFlags(obj.GetNPCFlags() | flag);
        }

        [TempleDllLocation(0x100b1d20)]
        [PythonName("npc_flag_unset")]
        public static void ClearNpcFlag(this GameObjectBody obj, NpcFlag flag)
        {
            obj.SetNPCFlags(obj.GetNPCFlags() & ~flag);
        }

        [TempleDllLocation(0x100b2370)]
        [PythonName("saving_throw")]
        public static bool SavingThrow(this GameObjectBody obj, int dc, SavingThrowType saveType,
            D20SavingThrowFlag flags,
            GameObjectBody opponent = null)
        {
            return GameSystems.D20.Combat.SavingThrow(obj, opponent, dc, saveType, flags);
        }

        [TempleDllLocation(0x100b25d0)]
        [PythonName("saving_throw_spell")]
        public static bool SavingThrowSpell(this GameObjectBody obj, int dc, SavingThrowType saveType,
            D20SavingThrowFlag flags,
            GameObjectBody caster, int spellId)
        {
            return GameSystems.D20.Combat.SavingThrowSpell(obj, caster, dc, saveType, flags, spellId);
        }

        [TempleDllLocation(0x100b2730)]
        [PythonName("reflex_save_and_damage")]
        public static bool ReflexSaveAndDamage(this GameObjectBody obj,
            GameObjectBody attacker, int dc, D20SavingThrowReduction reduction, D20SavingThrowFlag flags,
            Dice damageDice,
            DamageType damageType, D20AttackPower attackPower,
            D20ActionType actionType = D20ActionType.NONE, int spellId = 0)
        {
            return GameSystems.D20.Combat.ReflexSaveAndDamage(obj, attacker, dc, reduction, flags, damageDice,
                damageType, attackPower, actionType, spellId);
        }

        [TempleDllLocation(0x100b2a20)]
        [PythonName("soundmap_critter")]
        public static int GetCritterSoundEffect(this GameObjectBody obj, CritterSoundEffect soundEffect)
        {
            return GameSystems.SoundMap.GetCritterSoundEffect(obj, soundEffect);
        }

        [TempleDllLocation(0x100b2a60)]
        [PythonName("footstep")]
        public static void Footstep(this GameObjectBody obj)
        {
            GameSystems.Critter.MakeFootstepSound(obj);
        }

        [TempleDllLocation(0x100b2a90)]
        [PythonName("secretdoor_detect")]
        public static void DetectSecretDoor(this GameObjectBody door, GameObjectBody viewer)
        {
            GameSystems.Secretdoor.SecretDoorSpotted(door, viewer);
        }

        [TempleDllLocation(0x100b2b00)]
        [PythonName("has_spell_effects")]
        public static bool IsAffectedBySpell(this GameObjectBody obj)
        {
            return GameSystems.Spell.IsAffectedBySpell(obj);
        }

        [TempleDllLocation(0x100b2b50)]
        [PythonName("critter_kill")]
        public static void Kill(this GameObjectBody obj)
        {
            GameSystems.D20.Combat.Kill(obj, null);
        }

        [TempleDllLocation(0x100b2b20)]
        [PythonName("critter_kill_by_effect")]
        public static void KillWithDeathEffect(this GameObjectBody obj, GameObjectBody killer = null)
        {
            GameSystems.D20.Combat.KillWithDeathEffect(obj, killer);
        }

        [TempleDllLocation(0x100b2b80)]
        [PythonName("destroy")]
        public static void Destroy(this GameObjectBody obj)
        {
            GameSystems.Object.Destroy(obj);
        }

        [TempleDllLocation(0x100b2bb0)]
        [PythonName("item_get")]
        public static bool GetItem(this GameObjectBody obj, GameObjectBody item)
        {
            return GameSystems.Item.SetItemParent(item, obj);
        }

        [TempleDllLocation(0x100b2c20)]
        [PythonName("perform_touch_attack")]
        public static D20CAF PerformTouchAttack(this GameObjectBody obj, GameObjectBody target)
        {
            var action = new D20Action(D20ActionType.TOUCH_ATTACK, obj);
            action.d20ATarget = target;
            action.d20Caf |= D20CAF.TOUCH_ATTACK | D20CAF.RANGED;
            action.data1 = 1;
            GameSystems.D20.Combat.ToHitProcessing(action);
            GameSystems.RollHistory.CreateRollHistoryString(action.rollHistId1);
            GameSystems.RollHistory.CreateRollHistoryString(action.rollHistId2);
            GameSystems.RollHistory.CreateRollHistoryString(action.rollHistId0);
            if ((action.d20Caf & D20CAF.CRITICAL) != 0)
            {
                return D20CAF.CRITICAL;
            }
            else if ((action.d20Caf & D20CAF.HIT) != 0)
            {
                return D20CAF.HIT;
            }
            else
            {
                return 0;
            }
        }

        [TempleDllLocation(0x100b2d50)]
        [PythonName("add_to_initiative")]
        public static void AddToInitiative(this GameObjectBody obj)
        {
            GameSystems.D20.Initiative.AddToInitiative(obj);
        }

        [TempleDllLocation(0x100b2d80)]
        [PythonName("remove_from_initiative")]
        public static void RemoveFromInitiative(this GameObjectBody obj)
        {
            GameSystems.D20.Initiative.RemoveFromInitiative(obj);
        }

        [TempleDllLocation(0x100b2db0)]
        [PythonName("get_initiative")]
        public static int GetInitiative(this GameObjectBody obj)
        {
            return GameSystems.D20.Initiative.GetInitiative(obj);
        }

        [TempleDllLocation(0x100b2dd0)]
        [PythonName("set_initiative")]
        public static void SetInitiative(this GameObjectBody obj, int initiative)
        {
            GameSystems.D20.Initiative.SetInitiative(obj, initiative);
        }

        [TempleDllLocation(0x100b2fb0)]
        [PythonName("d20_query")]
        public static bool D20Query(this GameObjectBody obj, D20DispatcherKey key, int data1 = 0, int data2 = 0)
        {
            return GameSystems.D20.D20Query(obj, key, data1, data2);
        }

        public static bool D20Query(this GameObjectBody obj, string key, int data1 = 0, int data2 = 0)
        {
            return GameSystems.D20.D20QueryPython(obj, key, data1, data2) != 0;
        }

        [TempleDllLocation(0x100b2ff0)]
        [PythonName("d20_query_has_spell_condition")]
        private static bool HasCondition(this GameObjectBody obj, ConditionSpec condition)
        {
            return ConditionExtensions.HasCondition(obj, condition);
        }

        [TempleDllLocation(0x100b31b0)]
        [PythonName("critter_get_alignment")]
        private static Alignment GetAlignment(this GameObjectBody obj)
        {
            return AlignmentExtensions.GetAlignment(obj);
        }

        [TempleDllLocation(0x100b31e0)]
        [PythonName("distance_to")]
        public static float DistanceTo(this GameObjectBody obj, GameObjectBody target)
        {
            return LocationExtensions.DistanceToObjInFeet(obj, target);
        }

        [TempleDllLocation(0x100b31e0)]
        [PythonName("distance_to")]
        public static float DistanceTo(this GameObjectBody obj, locXY targetTile, float offX = 0, float offY = 0)
        {
            LocAndOffsets targetLoc;
            targetLoc.location = targetTile;
            targetLoc.off_x = offX;
            targetLoc.off_y = offY;
            return obj.DistanceToLocInFeet(targetLoc);
        }

        [TempleDllLocation(0x100b3320)]
        [PythonName("anim_callback")]
        public static void StartFrogGrapplePhase(this GameObjectBody giantFrog, FrogGrapplePhase phase)
        {
            switch (phase)
            {
                case FrogGrapplePhase.FailedLatch:
                    FrogGrappleController.PlayFailedLatch(giantFrog);
                    break;
                case FrogGrapplePhase.Latch:
                    FrogGrappleController.PlayLatch(giantFrog);
                    break;
                case FrogGrapplePhase.Pull:
                    FrogGrappleController.PlayPull(giantFrog);
                    break;
                case FrogGrapplePhase.Swallow:
                    FrogGrappleController.PlaySwallow(giantFrog);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(phase), phase, "Invalid frog grapple phase");
            }
        }

        [TempleDllLocation(0x100b32f0)]
        [PythonName("anim_goal_interrupt")]
        public static void InterruptAnimations(this GameObjectBody obj)
        {
            GameSystems.Anim.Interrupt(obj, AnimGoalPriority.AGP_HIGHEST);
        }

        [TempleDllLocation(0x100b33f0)]
        [PythonName("d20_status_init")]
        public static void InitD20Status(this GameObjectBody obj)
        {
            GameSystems.D20.Status.D20StatusInit(obj);
        }

        [TempleDllLocation(0x100b3420)]
        [PythonName("object_script_execute")]
        public static int ExecuteObjectScript(this GameObjectBody attachee, GameObjectBody triggerer,
            ObjScriptEvent scriptEvent)
        {
            return GameSystems.Script.ExecuteObjectScript(triggerer, attachee, scriptEvent);
        }

        [TempleDllLocation(0x100b34c0)]
        [PythonName("standpoint_set")]
        public static void SetStandpoint(this GameObjectBody obj, StandPointType type, int jumpPointId)
        {
            var standpoint = new StandPoint();
            standpoint.jumpPointId = jumpPointId;
            if (!GameSystems.JumpPoint.TryGet(jumpPointId, out _, out standpoint.mapId,
                out standpoint.location.location))
            {
                Logger.Warn("Used invalid jump point id {0} when setting standpoint for {1}", jumpPointId, obj);
            }

            GameSystems.AI.SetStandPoint(obj, type, standpoint);
        }

        public static LocAndOffsets RunOff(this GameObjectBody obj, locXY location) =>
            RunOff(obj, new LocAndOffsets(location));

        [TempleDllLocation(0x100b3540)]
        [PythonName("runoff")]
        public static LocAndOffsets RunOff(this GameObjectBody obj, LocAndOffsets location)
        {
            GameSystems.MapObject.SetFlags(obj, ObjectFlag.CLICK_THROUGH);
            obj.AiFlags |= AiFlag.RunningOff;
            GameSystems.ObjFade.FadeTo(obj, 0, 25, 5, FadeOutResult.RunOff);

            GameSystems.Anim.PushRunNearTile(obj, location, 5);
            return location;
        }

        public static LocAndOffsets RunOff(this GameObjectBody obj)
        {
            var location = obj.GetLocationFull();
            location.location.locx -= 3;

            return obj.RunOff(location);
        }

        [TempleDllLocation(0x100b3630)]
        [PythonName("get_category_type")]
        public static MonsterCategory GetMonsterCategory(this GameObjectBody obj)
        {
            return GameSystems.Critter.GetCategory(obj);
        }

        [TempleDllLocation(0x100b3650)]
        [PythonName("is_category_type")]
        public static bool IsMonsterCategory(this GameObjectBody obj, MonsterCategory category)
        {
            return GameSystems.Critter.IsCategory(obj, category);
        }

        [TempleDllLocation(0x100b36a0)]
        [PythonName("is_category_subtype")]
        public static bool IsMonsterSubtype(this GameObjectBody obj, MonsterSubtype subtype)
        {
            return GameSystems.Critter.IsCategorySubtype(obj, subtype);
        }

        [TempleDllLocation(0x100b2e70)]
        [PythonName("rumor_log_add")]
        public static void AddRumor(this GameObjectBody obj, int rumorId)
        {
            GameSystems.Rumor.Add(obj, rumorId);
        }

        [TempleDllLocation(0x100b2ec0)]
        [PythonName("obj_set_int")]
        public static void SetInt(this GameObjectBody obj, obj_f field, int value)
        {
            if (field == obj_f.critter_subdual_damage)
            {
                GameSystems.MapObject.ChangeSubdualDamage(obj, value);
            }
            else
            {
                obj.SetInt32(field, value);
            }
        }

        [TempleDllLocation(0x100b2f60)]
        [PythonName("obj_get_int")]
        public static int GetInt(this GameObjectBody obj, obj_f field)
        {
            return obj.GetInt32(field);
        }

        [TempleDllLocation(0x100b36f0)]
        [PythonName("has_feat")]
        private static bool HasFeat(this GameObjectBody obj, FeatId feat)
        {
            return GameSystems.Feat.HasFeat(obj, feat);
        }

        [TempleDllLocation(0x100b0c30)]
        [PythonName("spell_damage")]
        public static void DealSpellDamage(this GameObjectBody victim, GameObjectBody attacker, DamageType damageType,
            Dice dice,
            D20AttackPower attackPower = D20AttackPower.NORMAL,
            D20ActionType actionType = D20ActionType.NONE,
            int spellId = 0)
        {
            GameSystems.D20.Combat.SpellDamageFull(victim, attacker, dice, damageType, attackPower, actionType, spellId,
                0);
        }

        [PythonName("spell_damage_weaponlike")]
        public static void DealSpellDamage(this GameObjectBody victim, GameObjectBody attacker, DamageType damageType,
            Dice dice,
            D20AttackPower attackPower = D20AttackPower.NORMAL,
            int reduction = 100,
            D20ActionType actionType = D20ActionType.NONE,
            int spellId = 0,
            D20CAF flags = 0,
            int projectileIdx = 0)
        {
            // Line 105: Saving Throw
            GameSystems.D20.Combat.DealWeaponlikeSpellDamage(victim, attacker, dice, damageType, attackPower, reduction, 105,
                actionType, spellId, flags, projectileIdx);
        }

        [TempleDllLocation(0x100b0ec0)]
        [PythonName("spell_damage_with_reduction")]
        public static void DealReducedSpellDamage(this GameObjectBody victim, GameObjectBody attacker,
            DamageType damageType, Dice dice,
            D20AttackPower attackPower = D20AttackPower.NORMAL,
            int reduction = 100,
            D20ActionType actionType = D20ActionType.NONE,
            int spellId = 0)
        {
            GameSystems.D20.Combat.DealSpellDamage(victim, attacker, dice, damageType, attackPower, reduction, 105,
                actionType, spellId, 0);
        }

        [TempleDllLocation(0x100b11b0)]
        [PythonName("spell_heal")]
        public static void HealFromSpell(this GameObjectBody obj, GameObjectBody healer, Dice dice,
            D20ActionType actionType = D20ActionType.NONE, int spellId = 0)
        {
            GameSystems.Combat.SpellHeal(obj, healer, dice, actionType, spellId);
        }

        [TempleDllLocation(0x100b3860)]
        [PythonName("identify_all")]
        public static void IdentifyAll(this GameObjectBody obj)
        {
            GameSystems.Item.IdentifyAll(obj);
        }

        [TempleDllLocation(0x100b3890)]
        [PythonName("ai_flee_add")]
        public static void AIAddFleeFrom(this GameObjectBody obj, GameObjectBody fleeFrom)
        {
            GameSystems.AI.FleeFrom(obj, fleeFrom);
        }

        [TempleDllLocation(0x100b38f0)]
        [PythonName("get_deity")]
        public static DeityId GetDeity(this GameObjectBody obj)
        {
            return (DeityId) obj.GetStat(Stat.deity);
        }

        [TempleDllLocation(0x100b3920)]
        [PythonName("item_wield_best_all")]
        public static void WieldBestInAllSlots(this GameObjectBody obj, GameObjectBody target = null)
        {
            GameSystems.Item.WieldBestAll(obj, target);
        }

        [TempleDllLocation(0x100b39a0)]
        [PythonName("award_experience")]
        public static void AwardExperience(this GameObjectBody obj, int xpAwarded)
        {
            GameSystems.Critter.AwardXp(obj, xpAwarded);
            GameUiBridge.UpdatePartyUi(); // Potentially shows the level up button
        }

        [TempleDllLocation(0x100b3a90)]
        [PythonName("has_atoned")]
        public static void AtoneFallenPaladin(this GameObjectBody obj)
        {
            GameSystems.D20.D20SendSignal(obj, D20DispatcherKey.SIG_Atone_Fallen_Paladin);
        }

        [TempleDllLocation(0x100b3ac0)]
        [PythonName("d20_send_signal")]
        public static void D20SendSignal(this GameObjectBody obj, D20DispatcherKey signal)
        {
            GameSystems.D20.D20SendSignal(obj, signal);
        }

        public static void D20SendSignal(this GameObjectBody obj, D20DispatcherKey signal, GameObjectBody data)
        {
            GameSystems.D20.D20SendSignal(obj, signal, data);
        }

        public static void D20SendSignal(this GameObjectBody obj, D20DispatcherKey signal, int data)
        {
            GameSystems.D20.D20SendSignal(obj, signal, data);
        }

        public static void D20SendSignal(this GameObjectBody obj, string signal, int data = 0)
        {
            GameSystems.D20.D20SendSignal(obj, (D20DispatcherKey) ElfHash.Hash(signal), data);
        }

// TODO: This is ONLY used for produce flame ?!?!
        [TempleDllLocation(0x100b3b80)]
        [PythonName("d20_send_signal_ex")]
        public static void D20SendSignalEx(this GameObjectBody obj, D20DispatcherKey signal, GameObjectBody target)
        {
            D20Action d20a = new D20Action(D20ActionType.CAST_SPELL, obj);
            d20a.d20ATarget = target;
            d20a.d20Caf = D20CAF.HIT;

            if (signal == D20DispatcherKey.SIG_TouchAttack)
            {
                d20a.d20Caf = obj.PerformTouchAttack(target);
            }

            GameSystems.D20.D20SendSignal(obj, signal, d20a);
        }

        [TempleDllLocation(0x100b3c70)]
        [PythonName("balor_death")]
        public static void BalorDeath(this GameObjectBody obj)
        {
            GameSystems.Critter.BalorDeath(obj);
        }

        [TempleDllLocation(0x100b3ca0)]
        [PythonName("concealed_set")]
        public static void SetConcealed(this GameObjectBody obj, bool concealed)
        {
            GameSystems.Critter.SetConcealedWithFollowers(obj, concealed);
        }

        [TempleDllLocation(0x100b3cf0)]
        [PythonName("ai_shitlist_add")]
        public static void AIAddToShitlist(this GameObjectBody obj, GameObjectBody target)
        {
            GameSystems.AI.AddToShitlist(obj, target);
        }

        [TempleDllLocation(0x100b3d50)]
        [PythonName("ai_shitlist_remove")]
        public static void AIRemoveFromShitlist(this GameObjectBody obj, GameObjectBody target)
        {
            GameSystems.AI.RemoveFromShitlist(obj, target);

            var combatFocus = obj.GetObject(obj_f.npc_combat_focus);
            if (combatFocus == target)
            {
                obj.SetObject(obj_f.npc_combat_focus, null);
            }

            var lastHitBy = obj.GetObject(obj_f.npc_who_hit_me_last);
            if (lastHitBy == target)
            {
                obj.SetObject(obj_f.npc_who_hit_me_last, null);
            }

            GameSystems.AI.ResetFightStatus(obj);
        }

        [TempleDllLocation(0x100b3e50)]
        [PythonName("unconceal")]
        public static bool Unconceal(this GameObjectBody obj)
        {
            return GameSystems.Anim.PushUnconceal(obj);
        }

        [TempleDllLocation(0x100b3e70)]
        [PythonName("spells_pending_to_memorized")]
        public static void PendingSpellsToMemorized(this GameObjectBody obj)
        {
            GameSystems.Spell.PendingSpellsToMemorized(obj);
        }

        [TempleDllLocation(0x100b3ea0)]
        [PythonName("spells_memorized_forget")]
        public static void ForgetMemorizedSpells(this GameObjectBody obj)
        {
            obj.ClearArray(obj_f.critter_spells_memorized_idx);
        }

        [PythonName("spells_cast_reset")]
        public static void ResetCastSpells(this GameObjectBody caster, Stat? forClass = null)
        {
            GameSystems.Spell.SpellsCastReset(caster, forClass);
        }

        [TempleDllLocation(0x100b3ed0)]
        [PythonName("ai_stop_attacking")]
        public static void AIStopAttacking(this GameObjectBody obj)
        {
            GameSystems.AI.StopAttacking(obj);
        }

        [TempleDllLocation(0x100b3f00)]
        [PythonName("resurrect")]
        public static bool Resurrect(this GameObjectBody obj, ResurrectionType type, int casterLevel = 0)
        {
            return Resurrection.Resurrect(obj, type);
        }

        [TempleDllLocation(0x100b3f60)]
        [PythonName("dominate")]
        public static void Dominate(this GameObjectBody critter, GameObjectBody caster)
        {
            GameSystems.Spell.FloatSpellLine(critter, 20018,
                TextFloaterColor.Red); // Float a "charmed!" line above the critter
            var partSys = GameSystems.ParticleSys.CreateAtObj("sp-Dominate Person", critter);
            critter.AddCondition(StatusEffects.Dominate, partSys, caster);
        }

        [TempleDllLocation(0x100b4080)]
        [PythonName("is_unconscious")]
        public static bool IsUnconscious(this GameObjectBody obj)
        {
            return GameSystems.Critter.IsDeadOrUnconscious(obj);
        }

        public static int GetMap(this GameObjectBody obj) => GameSystems.Map.GetCurrentMapId();


        public static int GetArea(this GameObjectBody obj) => GameSystems.Area.GetCurrentArea();

        public static Gender GetGender(this GameObjectBody obj) => (Gender) obj.GetStat(Stat.gender);

        public static RaceId GetRace(this GameObjectBody obj) => (RaceId) obj.GetStat(Stat.race);

        public static LootSharingType GetLootSharingType(this GameObjectBody obj)
        {
            var type = obj.GetInt32(obj_f.npc_pad_i_3) & 0xF;
            return (LootSharingType) type;
        }

        public static void SetLootSharingType(this GameObjectBody obj, LootSharingType type)
        {
            obj.SetInt32(obj_f.npc_pad_i_3, ((int) type) & 0xF);
        }

        public static int GetScriptId(this GameObjectBody obj, ObjScriptEvent evt)
        {
            // See current python obj.scripts
            return obj.GetScript(obj_f.scripts_idx, (int) evt).scriptId;
        }

        public static void SetScriptId(this GameObjectBody obj, ObjScriptEvent evt, int scriptId)
        {
            // See current python obj.scripts
            var script = obj.GetScript(obj_f.scripts_idx, (int) evt);
            script.scriptId = scriptId;
            obj.SetScript(obj_f.scripts_idx, (int) evt, script);
        }

        public static void RemoveScript(this GameObjectBody obj, ObjScriptEvent evt)
        {
            obj.SetScriptId(evt, 0);
        }

    }

    public enum FrogGrapplePhase
    {
        FailedLatch = 0,
        Latch = 1,
        Pull = 2,
        Swallow = 3
    }
}
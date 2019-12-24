using System;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Logging;
using SpicyTemple.Core.Systems.GameObjects;

namespace SpicyTemple.Core.Systems.Anim
{
    public class AnimSlotGoalStackEntry
    {
        private static readonly ILogger Logger = LoggingSystem.CreateLogger();

        public AnimGoalType goalType;
        public int unk1;
        public AnimParam self; // 0x8
        public AnimParam target; // 0x18
        public AnimParam block; // 0x28
        public AnimParam scratch; //0x38
        public AnimParam parent; // 0x48
        public AnimParam targetTile; //0x58
        public AnimParam range; //0x68
        public AnimParam animId; //0x78
        public AnimParam animIdPrevious; // 0x88
        public AnimParam animData; // 0x98
        public AnimParam spellData; // 0xA8
        public AnimParam skillData; // 0xB8
        public AnimParam flagsData; // 0xC8
        public AnimParam scratchVal1; // 0xD8
        /// <summary>
        /// Used by <see cref="AnimGoalType.rotate"/> to store the target rotation in radians.
        /// </summary>
        public AnimParam scratchVal2; // 0xE8
        public AnimParam scratchVal3; // 0xF8
        public AnimParam scratchVal4; // 0x108
        public AnimParam scratchVal5; // 0x118
        public AnimParam scratchVal6; // 0x128
        public AnimParam soundHandle; // 0x138
        public int soundStreamId; // 0x148
        public int soundStreamId2; // Probably padding
        public FrozenObjRef selfTracking;
        public FrozenObjRef targetTracking;
        public FrozenObjRef blockTracking;
        public FrozenObjRef scratchTracking;
        public FrozenObjRef parentTracking;

        [TempleDllLocation(0x100556C0)]
        public bool InitWithInterrupt(GameObjectBody obj, AnimGoalType goalType)
        {
            return Init(obj, goalType, true);
        }

        public bool Push(out AnimSlotId idNew)
        {
            return GameSystems.Anim.PushGoal(this, out idNew);
        }

        [TempleDllLocation(0x10055570)]
        public bool Init(GameObjectBody handle, AnimGoalType type, bool withInterrupt = false)
        {
            if (((uint) goalType & 0x80000000) != 0 || goalType >= AnimGoalType.count)
            {
                Logger.Error("Illegal goalType");
                GameSystems.Anim.Debug();
            }

            animId.number = -1;
            animIdPrevious.number = -1;
            animData.number = -1;
            spellData.number = -1;
            flagsData.number = -1;
            soundStreamId = -1;
            goalType = type;
            self.obj = handle;
            target.obj = null;
            block.obj = null;
            scratch.obj = null;
            parent.obj = null;
            targetTile.location = LocAndOffsets.Zero;
            range.location = LocAndOffsets.Zero;
            skillData.number = 0;
            scratchVal1.number = 0;
            scratchVal2.number = 0;
            scratchVal3.number = 0;
            scratchVal4.number = 0;
            scratchVal5.number = 0;
            scratchVal6.number = 0;
            soundHandle.number = 0;
            if (withInterrupt)
            {
                var ag = GameSystems.Anim.Goals.GetByType(goalType);
                if (ag == null)
                {
                    Logger.Error("pGoalNode != NULL assertion failed");
                    GameSystems.Anim.Debug();
                }

                return GameSystems.Anim.Interrupt(handle, ag.priority, ag.interruptAll);
            }

            return true;
        }

        public void FreezeObjectRefs()
        {
            selfTracking = GameSystems.MapObject.CreateFrozenRef(self.obj);
            targetTracking = GameSystems.MapObject.CreateFrozenRef(target.obj);
            blockTracking = GameSystems.MapObject.CreateFrozenRef(block.obj);
            scratchTracking = GameSystems.MapObject.CreateFrozenRef(scratch.obj);
            parentTracking = GameSystems.MapObject.CreateFrozenRef(parent.obj);
        }

        private static bool ValidateObjectRef(ref AnimParam animParam, FrozenObjRef frozenRef)
        {
            if (animParam.obj != null)
            {
                // NOTE: Vanilla also checked that the handle was valid, but we no longer do this
                // since handles never are "really" invalid...
                // This check however could be replaced with a check whether the object still belongs to the
                // currently loaded map or not.
                return true;
            }

            if (!GameSystems.MapObject.Unfreeze(frozenRef, out var obj))
            {
                Logger.Error("Failed to recover object reference in anim goal");
                animParam.obj = null;
                return false;
            }

            animParam.obj = obj;
            return true;
        }

        public bool ValidateObjectRefs()
        {
            if (GameSystems.Map.IsClearingMap())
            {
                self.obj = null;
                target.obj = null;
                block.obj = null;
                scratch.obj = null;
                parent.obj = null;
                return false;
            }

            if (!ValidateObjectRef(ref self, selfTracking))
            {
                return false;
            }

            if (!ValidateObjectRef(ref target, targetTracking))
            {
                return false;
            }

            if (!ValidateObjectRef(ref block, blockTracking))
            {
                return false;
            }

            if (!ValidateObjectRef(ref scratch, scratchTracking))
            {
                return false;
            }

            if (!ValidateObjectRef(ref parent, parentTracking))
            {
                return false;
            }

            return true;
        }

        public AnimSlotGoalStackEntry(GameObjectBody handle, AnimGoalType type, bool withInterrupt = false)
        {
            Init(handle, type, withInterrupt);
        }

        internal AnimSlotGoalStackEntry()
        {
        }

        public AnimParam GetAnimParam(AnimGoalProperty property)
        {
            var result = new AnimParam();

            switch (property)
            {
                case AnimGoalProperty.SELF_OBJ_PRECISE_LOC:
                    result.location = self.obj.GetLocationFull();
                    break;
                case AnimGoalProperty.TARGET_OBJ_PRECISE_LOC:
                    if (target.obj.type.IsEquipment())
                    {
                        var parentObj = GameSystems.Item.GetParent(target.obj);
                        if (parentObj != null)
                        {
                            result.location = parentObj.GetLocationFull();
                            break;
                        }
                    }

                    result.location = target.obj.GetLocationFull();
                    break;
                case AnimGoalProperty.NULL_HANDLE:
                    result.obj = null;
                    break;
                case AnimGoalProperty.TARGET_LOC_PRECISE:
                    if (targetTile.location.location.locx != 0 || targetTile.location.location.locy != 0)
                    {
                        result.location = targetTile.location;
                    }
                    else
                    {
                        if (target.obj != null)
                        {
                            targetTile.location = target.obj.GetLocationFull();
                        }

                        result.location = targetTile.location;
                    }

                    break;
                case AnimGoalProperty.SELF_OBJ:
                    result.obj = self.obj;
                    break;
                case AnimGoalProperty.TARGET_OBJ:
                    result.obj = target.obj;
                    break;
                case AnimGoalProperty.BLOCK_OBJ:
                    result.obj = block.obj;
                    break;
                case AnimGoalProperty.SCRATCH_OBJ:
                    result.obj = scratch.obj;
                    break;
                case AnimGoalProperty.PARENT_OBJ:
                    result.obj = parent.obj;
                    break;
                case AnimGoalProperty.TARGET_TILE:
                    result.location = targetTile.location;
                    break;
                case AnimGoalProperty.RANGE_DATA:
                    result.location = range.location;
                    break;
                case AnimGoalProperty.ANIM_ID:
                    result.number = animId.number;
                    break;
                case AnimGoalProperty.ANIM_ID_PREV:
                    result.number = animIdPrevious.number;
                    break;
                case AnimGoalProperty.ANIM_DATA:
                    result.number = animData.number;
                    break;
                case AnimGoalProperty.SPELL_DATA:
                    result.number = spellData.number;
                    break;
                case AnimGoalProperty.SKILL_DATA:
                    result.number = skillData.number;
                    break;
                case AnimGoalProperty.FLAGS_DATA:
                    result.number = flagsData.number;
                    break;
                case AnimGoalProperty.SCRATCH_VAL1:
                    result.number = scratchVal1.number;
                    break;
                case AnimGoalProperty.SCRATCH_VAL2:
                    result = scratchVal2;
                    break;
                case AnimGoalProperty.SCRATCH_VAL3:
                    result.number = scratchVal3.number;
                    break;
                case AnimGoalProperty.SCRATCH_VAL4:
                    result.number = scratchVal4.number;
                    break;
                case AnimGoalProperty.SCRATCH_VAL5:
                    result.number = scratchVal5.number;
                    break;
                case AnimGoalProperty.SCRATCH_VAL6:
                    result.number = scratchVal6.number;
                    break;
                case AnimGoalProperty.SOUND_HANDLE:
                    result.number = soundHandle.number;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("Unknown data type: " + property);
            }

            return result;
        }

        public void CopyTo(AnimSlotGoalStackEntry otherGoal)
        {
            otherGoal.goalType = goalType;
            otherGoal.unk1 = unk1;
            otherGoal.self = self;
            otherGoal.target = target;
            otherGoal.block = block;
            otherGoal.scratch = scratch;
            otherGoal.parent = parent;
            otherGoal.targetTile = targetTile;
            otherGoal.range = range;
            otherGoal.animId = animId;
            otherGoal.animIdPrevious = animIdPrevious;
            otherGoal.animData = animData;
            otherGoal.spellData = spellData;
            otherGoal.skillData = skillData;
            otherGoal.flagsData = flagsData;
            otherGoal.scratchVal1 = scratchVal1;
            otherGoal.scratchVal2 = scratchVal2;
            otherGoal.scratchVal3 = scratchVal3;
            otherGoal.scratchVal4 = scratchVal4;
            otherGoal.scratchVal5 = scratchVal5;
            otherGoal.scratchVal6 = scratchVal6;
            otherGoal.soundHandle = soundHandle;
            otherGoal.soundStreamId = soundStreamId;
            otherGoal.soundStreamId2 = soundStreamId2;
            otherGoal.selfTracking = selfTracking;
            otherGoal.targetTracking = targetTracking;
            otherGoal.blockTracking = blockTracking;
            otherGoal.scratchTracking = scratchTracking;
            otherGoal.parentTracking = parentTracking;
        }

        public void CopyParamsTo(AnimSlotGoalStackEntry otherGoal)
        {
            otherGoal.target = target;
            otherGoal.block = block;
            otherGoal.scratch = scratch;
            otherGoal.parent = parent;
            otherGoal.targetTile = targetTile;
            otherGoal.range = range;
            otherGoal.animId = animId;
            otherGoal.animIdPrevious = animIdPrevious;
            otherGoal.animData = animData;
            otherGoal.spellData = spellData;
        }

        public override string ToString()
        {
            return $"StackEntry{goalType}";
        }
    }
}
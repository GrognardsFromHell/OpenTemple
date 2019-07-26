using System;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Location;

namespace SpicyTemple.Core.Systems.D20.Actions
{
    public static class D20ActionVanillaCallbacks
    {


        [TempleDllLocation(0x1008c220)]
        public static ActionErrorCode DoubleMoveActionCost(D20Action action, TurnBasedStatus tbStatus,
            ActionCostPacket actionCost)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x1008c2e0)]
        public static ActionErrorCode ActionCostRun(D20Action action, TurnBasedStatus tbStatus,
            ActionCostPacket actionCost)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x1008c4c0)]
        public static ActionErrorCode TargetCheckActionValid(D20Action action, TurnBasedStatus tbStatus)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x1008c7c0)]
        public static ActionErrorCode _Check_StandardAttack(D20Action action, TurnBasedStatus tbStatus,
            LocAndOffsets location)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x1008c910)]
        public static ActionErrorCode StandardAttackLocationCheck(D20Action action, TurnBasedStatus tbStatus,
            LocAndOffsets location)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x1008cdf0)]
        public static ActionErrorCode LocationCheckWithinReach(D20Action action, TurnBasedStatus tbStatus,
            LocAndOffsets location)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x1008cf60)]
        public static ActionErrorCode MoveTBStatusCheck(D20Action action, TurnBasedStatus tbStatus)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x1008cfc0)]
        public static ActionErrorCode MoveActionCheck(D20Action action, TurnBasedStatus tbStatus)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x1008d010)]
        public static ActionErrorCode MovePerform(D20Action action)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x1008d090)]
        public static int MovementSequenceRender(D20Action action, int flags)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x1008d0f0)]
        public static ActionErrorCode RunPerform(D20Action action)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x1008d1a0)]
        public static ActionErrorCode FivefootstepActionCost(D20Action action, TurnBasedStatus tbStatus,
            ActionCostPacket actionCost)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x1008d220)]
        public static ActionErrorCode FivefootstepTBStatusCheck(D20Action action, TurnBasedStatus tbStatus)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x1008d240)]
        public static ActionErrorCode DoubleMoveTBStatusCheck(D20Action action, TurnBasedStatus tbStatus)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x1008d2a0)]
        public static ActionErrorCode DoubleMoveActionCheck(D20Action action, TurnBasedStatus tbStatus)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x1008d2b0)]
        public static ActionErrorCode DoubleMovePerform(D20Action action)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x1008d330)]
        public static ActionErrorCode RunTBStatusCheck(D20Action action, TurnBasedStatus tbStatus)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x1008d390)]
        public static ActionErrorCode RunActionCheck(D20Action action, TurnBasedStatus tbStatus)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x1008d410)]
        public static ActionErrorCode Dispatch36D20ActnCheck_(D20Action action, TurnBasedStatus tbStatus)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x1008d420)]
        public static ActionErrorCode CastSpellTargetCheck(D20Action action, TurnBasedStatus tbStatus)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x1008dea0)]
        public static bool CastSpellActionFrame(D20Action action)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x1008e660)]
        public static ActionErrorCode SpellCallLightningPerform(D20Action action)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x1008eb90)]
        public static bool RangedAttackActionFrame(D20Action action)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x1008edf0)]
        public static int PickerFuncTooltipToHitChance(D20Action action, int flags)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x1008ee60)]
        public static ActionErrorCode RangedAttackActionCheck(D20Action action, TurnBasedStatus tbStatus)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x1008f380)]
        public static ActionErrorCode RangedAttackPerform(D20Action action)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x1008f460)]
        public static int ThrowSequenceRender(D20Action action, int flags)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x1008f690)]
        public static ActionErrorCode ThrowGrenadeLocationCheck(D20Action action, TurnBasedStatus tbStatus,
            LocAndOffsets location)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x1008fb50)]
        public static ActionErrorCode ThrowGrenadePerform(D20Action action)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x1008fbe0)]
        public static bool ThrowGrenadeProjectileHit(D20Action action, GameObjectBody projectile,
            GameObjectBody ammoItem)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10090000)]
        public static bool ThrowGrenadeActionFrame(D20Action action)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x100902f0)]
        public static ActionErrorCode HealPerform(D20Action action)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10090340)]
        public static bool HealActionFrame(D20Action action)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x100903b0)]
        public static ActionErrorCode ReloadActionCost(D20Action action, TurnBasedStatus tbStatus,
            ActionCostPacket actionCost)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10090610)]
        public static ActionErrorCode TouchAttackPerform(D20Action action)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x100907a0)]
        public static ActionErrorCode TotalDefensePerform(D20Action action)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10090900)]
        public static ActionErrorCode FallToProneActionCheck(D20Action action, TurnBasedStatus tbStatus)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10090930)]
        public static ActionErrorCode FallToPronePerform(D20Action action)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10090980)]
        public static ActionErrorCode StandUpActionCheck(D20Action action, TurnBasedStatus tbStatus)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x100909b0)]
        public static ActionErrorCode StandUpPerform(D20Action action)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10090a10)]
        public static ActionErrorCode PerformActionClericPaladin(D20Action action)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10090ab0)]
        public static ActionErrorCode PickupObjectTargetCheck(D20Action action, TurnBasedStatus tbStatus)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10090b50)]
        public static ActionErrorCode Dispatch37OnActionPerformSimple(D20Action action)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10090b60)]
        public static bool Dispatch38OnActionFrame(D20Action action)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10090b70)]
        public static ActionErrorCode CoupDeGraceTargetCheck(D20Action action, TurnBasedStatus tbStatus)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10090bd0)]
        public static ActionErrorCode CoupDeGracePerform(D20Action action)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10090c80)]
        public static bool CoupDeGraceActionFrame(D20Action action)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10090d60)]
        public static ActionErrorCode BreakFreePerform(D20Action action)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10090db0)]
        public static ActionErrorCode FleeCombatActionCheck(D20Action action, TurnBasedStatus tbStatus)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10090e90)]
        public static ActionErrorCode ItemCreationLocationCheck(D20Action action, TurnBasedStatus tbStatus,
            LocAndOffsets location)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10090ec0)]
        public static ActionErrorCode ItemCreationPerform(D20Action action)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10090ee0)]
        public static ActionErrorCode UseMagicDeviceDecipherWrittenSpellPerform(D20Action action)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10090f00)]
        public static ActionErrorCode OpenInventoryPerform(D20Action action)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10090f20)]
        public static ActionErrorCode DisableDevicePerform(D20Action action)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10090f50)]
        public static ActionErrorCode SearchPerform(D20Action action)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10091090)]
        public static ActionErrorCode OpenLockPerform(D20Action action)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x100910c0)]
        public static ActionErrorCode SleightOfHandPerform(D20Action action)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10091280)]
        public static ActionErrorCode OpenContainerLocationCheck(D20Action action, TurnBasedStatus tbStatus,
            LocAndOffsets location)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x100912e0)]
        public static ActionErrorCode OpenContainerPerform(D20Action action)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x100913d0)]
        public static bool OpenContainerActionFrame(D20Action action)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10091a40)]
        public static ActionErrorCode ReadySpellPerform(D20Action action)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10091a90)]
        public static ActionErrorCode ReadyCounterspellPerform(D20Action action)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10091ae0)]
        public static ActionErrorCode ReadyEnterPerform(D20Action action)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10091b30)]
        public static ActionErrorCode ReadyExitPerform(D20Action action)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10091b80)]
        public static ActionErrorCode CopyScrollActionCheck(D20Action action, TurnBasedStatus tbStatus)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10091d60)]
        public static int AooMovementSequenceRender(D20Action action, int flags)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10091de0)]
        public static ActionErrorCode TalkActionCheck(D20Action action, TurnBasedStatus tbStatus)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10091df0)]
        public static ActionErrorCode TalkPerform(D20Action action)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10091e10)]
        public static int TalkSequenceRender(D20Action action, int flags)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10091e20)]
        public static ActionErrorCode FeintPerform(D20Action action)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10091e70)]
        public static ActionErrorCode FeintActionCost(D20Action action, TurnBasedStatus tbStatus,
            ActionCostPacket actionCost)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10091ed0)]
        public static bool FeintActionFrame(D20Action action)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10091fa0)]
        public static int HealSequenceRender(D20Action action, int flags)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10091fb0)]
        public static int UseItemSequenceRender(D20Action action, int flags)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10092020)]
        public static int CastSpellSequenceRender(D20Action action, int flags)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10092040)]
        public static int OpenContainerSequenceRender(D20Action action, int flags)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x100920b0)]
        public static ActionErrorCode ReadiedInterruptPerform(D20Action action)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10094770)]
        public static ActionErrorCode SpellCallLightningLocationCheck(D20Action action, TurnBasedStatus tbStatus,
            LocAndOffsets location)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x100947c0)]
        public static ActionErrorCode ThrowAddToSeq(D20Action action, ActionSequence sequence,
            TurnBasedStatus tbStatus)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10094840)]
        public static ActionErrorCode ThrowGrenadeAddToSeq(D20Action action, ActionSequence sequence,
            TurnBasedStatus tbStatus)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10094860)]
        public static int AttackSequenceRender(D20Action action, int flags)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x100948b0)]
        public static ActionErrorCode ReloadPerform(D20Action action)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10095860)]
        public static ActionErrorCode UnspecifiedMoveAddToSeq(D20Action action, ActionSequence sequence,
            TurnBasedStatus tbStatus)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10095880)]
        public static ActionErrorCode ActionSequencesAddMovement(D20Action action, ActionSequence sequence,
            TurnBasedStatus tbStatus)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10096760)]
        public static ActionErrorCode TouchAttackAddToSeq(D20Action action, ActionSequence sequence,
            TurnBasedStatus tbStatus)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10097ba0)]
        public static ActionErrorCode ActionSequencesAddSmite(D20Action action, ActionSequence sequence,
            TurnBasedStatus tbStatus)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x100982e0)]
        public static bool RangedAttackProjectileHit(D20Action action, GameObjectBody projectile,
            GameObjectBody ammoItem)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10098810)]
        public static ActionErrorCode FleeCombatPerform(D20Action action)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x100999b0)]
        public static ActionErrorCode PickupObjectPerform(D20Action action)
        {
            throw new NotImplementedException();
        }
    }
}
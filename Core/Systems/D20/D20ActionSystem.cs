using System;
using System.Collections.Generic;
using System.Diagnostics;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.GFX;
using SpicyTemple.Core.TigSubsystems;
using SpicyTemple.Core.Utils;

namespace SpicyTemple.Core.Systems.D20
{
    public enum CursorType
    {
        Sword = 1,
        Arrow = 2,
        FeetGreen = 3,
        FeetYellow = 4,
        SlidePortraits = 5,
        Locked = 6,
        HaveKey = 7,
        UseSkill = 8,
        UsePotion = 9,
        UseSpell = 10,
        UseTeleportIcon = 11,
        HotKeySelection = 12,
        Talk = 13,
        IdentifyCursor = 14,
        IdentifyCursor2 = 15,
        ArrowInvalid = 16,
        SwordInvalid = 17,
        ArrowInvalid2 = 18,
        FeetRed = 19,
        FeetRed2 = 20,
        InvalidSelection = 21,
        Locked2 = 22,
        HaveKey2 = 23,
        UseSkillInvalid = 24,
        UsePotionInvalid = 25,
        UseSpellInvalid = 26,
        PlaceFlag = 27,
        HotKeySelectionInvalid = 28,
        InvalidSelection2 = 29,
        InvalidSelection3 = 30,
        InvalidSelection4 = 31,
        AttackOfOpportunity = 32,
        AttackOfOpportunityGrey = 33,
    }

    public class D20ActionSystem : IDisposable
    {
        private static readonly Dictionary<CursorType, string> CursorPaths = new Dictionary<CursorType, string>
        {
            {CursorType.AttackOfOpportunity, "art/interface/combat_ui/attack-of-opportunity.tga"},
            {CursorType.AttackOfOpportunityGrey, "art/interface/combat_ui/attack-of-opportunity-grey.tga"},
            {CursorType.Sword, "art/interface/cursors/sword.tga"},
            {CursorType.Arrow, "art/interface/cursors/arrow.tga"},
            {CursorType.FeetGreen, "art/interface/cursors/feet_green.tga"},
            {CursorType.FeetYellow, "art/interface/cursors/feet_yellow.tga"},
            {CursorType.SlidePortraits, "art/interface/cursors/SlidePortraits.tga"},
            {CursorType.Locked, "art/interface/cursors/Locked.tga"},
            {CursorType.HaveKey, "art/interface/cursors/havekey.tga"},
            {CursorType.UseSkill, "art/interface/cursors/useskill.tga"},
            {CursorType.UsePotion, "art/interface/cursors/usepotion.tga"},
            {CursorType.UseSpell, "art/interface/cursors/usespell.tga"},
            {CursorType.UseTeleportIcon, "art/interface/cursors/useteleporticon.tga"},
            {CursorType.HotKeySelection, "art/interface/cursors/hotkeyselection.tga"},
            {CursorType.Talk, "art/interface/cursors/talk.tga"},
            {CursorType.IdentifyCursor, "art/interface/cursors/IdentifyCursor.tga"},
            {CursorType.IdentifyCursor2, "art/interface/cursors/IdentifyCursor.tga"},
            {CursorType.ArrowInvalid, "art/interface/cursors/arrow_invalid.tga"},
            {CursorType.SwordInvalid, "art/interface/cursors/sword_invalid.tga"},
            {CursorType.ArrowInvalid2, "art/interface/cursors/arrow_invalid.tga"},
            {CursorType.FeetRed, "art/interface/cursors/feet_red.tga"},
            {CursorType.FeetRed2, "art/interface/cursors/feet_red.tga"},
            {CursorType.InvalidSelection, "art/interface/cursors/InvalidSelection.tga"},
            {CursorType.Locked2, "art/interface/cursors/locked.tga"},
            {CursorType.HaveKey2, "art/interface/cursors/havekey.tga"},
            {CursorType.UseSkillInvalid, "art/interface/cursors/useskill_invalid.tga"},
            {CursorType.UsePotionInvalid, "art/interface/cursors/usepotion_invalid.tga"},
            {CursorType.UseSpellInvalid, "art/interface/cursors/usespell_invalid.tga"},
            {CursorType.PlaceFlag, "art/interface/cursors/placeflagcursor.tga"},
            {CursorType.HotKeySelectionInvalid, "art/interface/cursors/hotkeyselection_invalid.tga"},
            {CursorType.InvalidSelection2, "art/interface/cursors/invalidSelection.tga"},
            {CursorType.InvalidSelection3, "art/interface/cursors/invalidSelection.tga"},
            {CursorType.InvalidSelection4, "art/interface/cursors/invalidSelection.tga"},
        };

        [TempleDllLocation(0x10092800)]
        public D20ActionSystem()
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x10089ef0)]
        public void Dispose()
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x10097be0)]
        public void ActionSequencesResetOnCombatEnd()
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x10095fd0)]
        public bool TurnBasedStatusInit(GameObjectBody obj)
        {
            // NOTE: TEMPLEPLUS REPLACED
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10089f70)]
        public TurnBasedStatus curSeqGetTurnBasedStatus()
        {
            throw new NotImplementedException();
            return null;
        }


        // describes the new hourglass state when current state is i after doing an action that costs j
        [TempleDllLocation(0x102CC538)]
        private static readonly int[,] TurnBasedStatusTransitionMatrix = new int[7, 5]
        {
            {0, -1, -1, -1, -1},
            {1, 0, -1, -1, -1},
            {2, 0, 0, -1, -1},
            {3, 0, 0, 0, -1},
            {4, 2, 1, -1, 0},
            {5, 2, 2, -1, 0},
            {6, 5, 2, -1, 3}
        };

        [TempleDllLocation(0x1008b020)]
        public int GetHourglassTransition(int hourglassCurrent, int hourglassCost)
        {
            if (hourglassCurrent == -1)
            {
                return hourglassCurrent;
            }

            return TurnBasedStatusTransitionMatrix[hourglassCurrent, hourglassCost];
        }

        [TempleDllLocation(0x10094A00)]
        public void CurSeqReset(GameObjectBody obj)
        {
            // NOTE: TEMPLEPLUS REPLACED
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x100949e0)]
        public void GlobD20ActnInit()
        {
            // NOTE: TEMPLEPLUS REPLACED
            // D20ActnInit(globD20Action->d20APerformer, globD20Action);
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10097C20)]
        public void ActionAddToSeq()
        {
            // NOTE: TEMPLEPLUS REPLACED
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x100961C0)]
        public void sequencePerform()
        {
            // NOTE: TEMPLEPLUS REPLACED
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10098c90)]
        public bool DoUseItemAction(GameObjectBody holder, GameObjectBody aiObj, GameObjectBody item)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10099cf0)]
        public void PerformOnAnimComplete(GameObjectBody obj, uint uniqueId)
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x100933F0)]
        public void ActionFrameProcess(GameObjectBody obj)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10089f00)]
        public bool WasInterrupted(GameObjectBody obj)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x1004e790)]
        public void dispatchTurnBasedStatusInit(GameObjectBody obj, DispIOTurnBasedStatus dispIo)
        {
            var dispatcher = obj.GetDispatcher();
            if (dispatcher != null)
            {
                dispatcher.Process(DispatcherType.TurnBasedStatusInit, D20DispatcherKey.NONE, dispIo);
                if (dispIo.tbStatus != null)
                {
                    if (dispIo.tbStatus.hourglassState > 0)
                    {
                        GameSystems.D20.D20SendSignal(obj, D20DispatcherKey.SIG_BeginTurn);
                    }
                }
            }
        }

        [TempleDllLocation(0x10099b10)]
        public void ProjectileHit(GameObjectBody projectile, GameObjectBody critter)
        {
            throw new NotImplementedException();
        }

    }
}
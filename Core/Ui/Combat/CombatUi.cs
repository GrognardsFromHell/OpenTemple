using SpicyTemple.Core.Systems.D20.Actions;

namespace SpicyTemple.Core.Ui.Combat
{
    public class CombatUi
    {
        private CombatDebugOutput DebugOutput { get; }

        [TempleDllLocation(0x10BE700C)]
        private int dword_10BE700C;

        [TempleDllLocation(0x10BE7010)]
        private int dword_10BE7010;

        [TempleDllLocation(0x10173690)]
        public CombatUi()
        {
            Stub.TODO();

            DebugOutput = new CombatDebugOutput();
        }

        [TempleDllLocation(0x10172E70)]
        public void Reset()
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x10142740)]
        public void Update()
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x10141760)]
        public CursorType? GetCursor()
        {
            if (dword_10BE700C != 0)
            {
                return (dword_10BE7010 != 0) ? CursorType.SlidePortraits : CursorType.InvalidSelection;
            }
            else
            {
                return null;
            }
        }

        [TempleDllLocation(0x10172e80)]
        public void SthCallback()
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x10173440)]
        public void UiActionBarGetValuesFromMovement()
        {
//            TurnBasedStatus *v0;
//            float startDist;
//            float endDist;
//            TurnBasedStatus tbStat;
//
//            v0 = GameSystems.D20.Actions.curSeqGetTurnBasedStatus();
//            startDist = UiCombatActionBarGetRemainingMoveDistance/*0x10172fb0*/(v0);
//            GameSystems.D20.Actions.seqCheckFuncs(&tbStat);
//            endDist = UiCombatActionBarGetRemainingMoveDistance/*0x10172fb0*/(&tbStat);
//            GameSystems.Vagrant.ActionBarSetMovementValues(uiCombatActionBar/*0x10c040b8*/, startDist, endDist, uiCombatDepletionSpeed/*0x10c0407c*/);
//            actionBarActor/*0x10c040c0*/ = GameSystems.D20.Initiative.get_CurrentActor();
//            actionBarEndingMoveDist/*0x10c040c8*/ = LODWORD(endDist);
        }
    }
}
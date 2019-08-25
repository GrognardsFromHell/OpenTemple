using SpicyTemple.Core.Systems.D20.Actions;

namespace SpicyTemple.Core.Ui.Combat
{
    public class CombatUi
    {
        public ActionBarUi ActionBar { get; }

        public InitiativeUi Initiative { get; }

        [TempleDllLocation(0x10BE700C)]
        private int dword_10BE700C;

        [TempleDllLocation(0x10BE7010)]
        private int dword_10BE7010;

        [TempleDllLocation(0x10173690)]
        public CombatUi()
        {
            Stub.TODO();

            ActionBar = new ActionBarUi();
            Initiative = new InitiativeUi();
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

    }
}
using SpicyTemple.Core.Systems.D20.Actions;

namespace SpicyTemple.Core.Ui.Combat
{
    public class CombatUi
    {
        public ActionBarUi ActionBar { get; }

        public InitiativeUi Initiative { get; }

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

        [TempleDllLocation(0x10172e80)]
        public void SthCallback()
        {
            Stub.TODO();
            Initiative.Update();
        }

    }
}
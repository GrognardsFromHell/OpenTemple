using SpicyTemple.Core.GameObject;

namespace SpicyTemple.Core.Systems.D20
{
    public class RadialMenuSystem
    {

        [TempleDllLocation(0x10BD0234)]
        [TempleDllLocation(0x100f0100)]
        public bool ShiftPressed { get; set; }

        [TempleDllLocation(0x100F0A70)]
        public void SetActive(GameObjectBody obj)
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x100f0050)]
        public int GetCurrentNode()
        {
            // TODO
            return -1;
        }

        [TempleDllLocation(0x100eff60)]
        public void ClearActiveRadialMenu()
        {
            throw new System.NotImplementedException();
        }
    }
}
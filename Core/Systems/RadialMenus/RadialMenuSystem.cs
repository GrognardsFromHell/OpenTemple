using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Systems.RadialMenus;
using System;

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

        [TempleDllLocation(0x100f12b0)]
        public int GetStandardNode(RadialMenuStandardNode standardNode)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x100f0670)]
        public int AddChildNode(GameObjectBody handle, ref RadialMenuEntry entry, int parentIdx)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x100f0d10)]
        public int AddParentChildNode(GameObjectBody handle, ref RadialMenuEntry entry, int parentIdx)
        {
            throw new NotImplementedException();
        }

        public int AddToStandardNode(GameObjectBody handle, ref RadialMenuEntry entry, RadialMenuStandardNode standardNode)
        {
            var node = GetStandardNode(RadialMenuStandardNode.Class);
            return AddParentChildNode(handle, ref entry, node);
        }

        [TempleDllLocation(0x100eff60)]
        public void ClearActiveRadialMenu()
        {
            throw new System.NotImplementedException();
        }

        [TempleDllLocation(0x100f0200)]
        public int RadialMenuCheckboxOrSliderCallback(GameObjectBody obj, ref RadialMenuEntry radEntry)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x100f2650)]
        [TemplePlusLocation("radialmenu.cpp:110")]
        public void BuildStandardRadialMenu(GameObjectBody critter)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x100f12c0)]
        public string GetAbilityReducedName(int statIdx)
        {
            throw new NotImplementedException();
        }

    }

}
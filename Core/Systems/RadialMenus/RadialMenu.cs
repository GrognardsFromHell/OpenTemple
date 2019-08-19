using System;
using System.Collections.Generic;
using SpicyTemple.Core.GameObject;

namespace SpicyTemple.Core.Systems.RadialMenus
{
    public class RadialMenu
    {
        public GameObjectBody obj { get; } // For which object is this the radial menu?
        public List<RadialMenuNode> nodes = new List<RadialMenuNode>();
        private readonly IComparer<int> _radialMenuNodeComparer;

        public RadialMenu(GameObjectBody obj)
        {
            this.obj = obj;
            _radialMenuNodeComparer = Comparer<int>.Create(CompareRadialNodes);
        }

        private int CompareRadialNodes(int x, int y)
        {
            var nodeX = nodes[x];
            var nodeY = nodes[y];
            return string.Compare(nodeX.entry.text, nodeY.entry.text, StringComparison.CurrentCulture);
        }

        public void Sort()
        {
            SortNode(nodes[0]);
        }

        [TempleDllLocation(0x100f0450)]
        private void SortNode(RadialMenuNode node)
        {
            node.children.Sort(_radialMenuNodeComparer);
            foreach (var childNodeIdx in node.children)
            {
                var childNode = nodes[childNodeIdx];
                if (childNode.entry.type == RadialMenuEntryType.Parent)
                {
                    SortNode(childNode);
                }
            }
        }
    }

    public class RadialMenuNode
    {
        public RadialMenuEntry entry;

        // Indices of children in the radial menu
        public List<int> children = new List<int>();

        // Index of parent node or -1
        public int parent;

        // is set for spontaneous casting (shift held down); see function inside AddSpell
        public int morphsTo;
    }
}
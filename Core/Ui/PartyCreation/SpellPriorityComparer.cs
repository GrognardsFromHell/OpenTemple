using System.Collections.Generic;
using System.Linq;
using SpicyTemple.Core.GameObject;

namespace SpicyTemple.Core.Ui.PartyCreation
{
    public class SpellPriorityComparer : IComparer<SpellStoreData>
    {
        // Key is the spell enum, value is the priority (lower value is higher priority)
        private readonly Dictionary<int, int> _priorities;

        private readonly int _defaultPriority;

        public SpellPriorityComparer(Dictionary<int, int> priorities)
        {
            _priorities = priorities;
            _defaultPriority = _priorities.Max(kvp => kvp.Key) + 1;
        }

        public int Compare(SpellStoreData x, SpellStoreData y)
        {
            var xPrio = _priorities.GetValueOrDefault(x.spellEnum, _defaultPriority);
            var yPrio = _priorities.GetValueOrDefault(y.spellEnum, _defaultPriority);
            return xPrio.CompareTo(yPrio);
        }
    }
}
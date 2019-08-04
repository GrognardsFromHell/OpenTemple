using System.Collections.Generic;
using SpicyTemple.Core.Systems.D20.Actions;

namespace SpicyTemple.Core.Systems.AI
{
    internal struct AiSpellList
    {
        public List<int> spellEnums;
        public List<D20SpellData> spellData;
    }
}
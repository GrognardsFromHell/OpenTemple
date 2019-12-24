using System.Collections.Generic;
using OpenTemple.Core.Systems.D20.Actions;

namespace OpenTemple.Core.Systems.AI
{
    internal struct AiSpellList
    {
        public List<int> spellEnums;
        public List<D20SpellData> spellData;
    }
}
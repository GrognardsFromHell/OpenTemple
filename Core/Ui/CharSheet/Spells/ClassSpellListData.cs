using System.Collections.Generic;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Systems.Spells;

namespace SpicyTemple.Core.Ui.CharSheet.Spells
{
    public class ClassSpellListData
    {
        public GameObjectBody Caster { get; }

        public bool Active { get; set; }

        public SpellsPerDay SpellsPerDay { get; set; }

        public ClassTabButton Button { get; set; }

        public ClassSpellListData(GameObjectBody caster)
        {
            Caster = caster;
        }
    }
}
using System.Collections.Generic;
using OpenTemple.Core.GameObject;
using OpenTemple.Core.Systems.Spells;

namespace OpenTemple.Core.Ui.CharSheet.Spells
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
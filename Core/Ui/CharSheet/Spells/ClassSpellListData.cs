using OpenTemple.Core.GameObjects;
using OpenTemple.Core.Systems.Spells;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui.CharSheet.Spells
{
    public class ClassSpellListData
    {
        public GameObject Caster { get; }

        public bool Active { get; set; }

        public SpellsPerDay SpellsPerDay { get; set; }

        public WidgetTabButton Button { get; set; }

        public ClassSpellListData(GameObject caster)
        {
            Caster = caster;
        }
    }
}

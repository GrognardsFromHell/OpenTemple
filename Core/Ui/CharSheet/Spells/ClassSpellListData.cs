using OpenTemple.Core.GameObject;
using OpenTemple.Core.Systems.Spells;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui.CharSheet.Spells
{
    public class ClassSpellListData
    {
        public GameObjectBody Caster { get; }

        public bool Active { get; set; }

        public SpellsPerDay SpellsPerDay { get; set; }

        public WidgetTabButton Button { get; set; }

        public ClassSpellListData(GameObjectBody caster)
        {
            Caster = caster;
        }
    }
}

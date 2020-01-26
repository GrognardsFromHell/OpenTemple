using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui.PartyCreation.Systems
{
    public class GenderSystem : IChargenSystem
    {
        public string HelpTopic { get; }
        public WidgetContainer Container { get; }
        public ChargenStages Stage { get; }

        public GenderSystem()
        {
            Container = new WidgetContainer(0, 0, 0, 0);
        }

        public bool CheckComplete() => false;

    }
}
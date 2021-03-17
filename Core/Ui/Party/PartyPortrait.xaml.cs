using Avalonia;
using Avalonia.Markup.Xaml;
using OpenTemple.Core.GameObject;
using Canvas = OpenTemple.Widgets.Canvas;

namespace OpenTemple.Core.Ui.Party
{
    public class PartyPortrait : Canvas
    {
        public static readonly AvaloniaProperty<GameObjectBody> CritterProperty = AvaloniaProperty.RegisterDirect<PartyPortrait, GameObjectBody>(nameof(Critter), x => x.Critter, (x, v) => x.Critter = v);

        private GameObjectBody _critter;

        public GameObjectBody Critter
        {
            get => _critter;
            set => SetAndRaise(CritterProperty, ref _critter, value);
        }

        public PartyPortrait()
        {
            AvaloniaXamlLoader.Load(this);
        }

        protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == CritterProperty)
            {
                UpdateCritter();
            }
        }

        private void UpdateCritter()
        {
        }
    }
}

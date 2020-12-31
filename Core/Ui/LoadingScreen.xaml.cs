using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ReactiveUI;

namespace OpenTemple.Core.Ui
{
    public class LoadingScreenViewModel : ReactiveObject
    {
        private double value;

        public double Value
        {
            get => value;
            set => this.RaiseAndSetIfChanged(ref this.value, value);
        }
    }

    public class LoadingScreen : UserControl
    {
        public LoadingScreen()
        {
            DataContext = new LoadingScreenViewModel();
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ReactiveUI;

namespace OpenTemple.Core.Ui.Options
{
    public class OptionsDialog : UserControl
    {
        public OptionsDialog()
        {
            AvaloniaXamlLoader.Load(this);
        }

        protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == IsVisibleProperty)
            {
                if (IsVisible)
                {
                    // Reset back to the first tab
                    this.Find<TabControl>("TabControl").SelectedIndex = 0;
                }
            }
        }
    }

    public class OptionsDialogViewModel : ReactiveObject
    {
        private List<OptionsPage> _pages;

        public event Action OnAccept;

        public event Action OnCancel;

        public List<OptionsPage> Pages
        {
            get => _pages;
            set => this.RaiseAndSetIfChanged(ref _pages, value);
        }

        public ICommand Accept { get; }

        public ICommand Cancel { get; }

        public OptionsDialogViewModel()
        {
            Accept = new ActionCommand(() => OnAccept?.Invoke());
            Cancel = new ActionCommand(() => OnCancel?.Invoke());
        }
    }
}
using System;
using System.Collections.Generic;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ReactiveUI;

namespace OpenTemple.Core.Ui.MainMenu
{
    public class MainMenu : UserControl
    {
        public MainMenu()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }

    /// <summary>
    /// Represents what is currently on-screen in the main menu.
    /// </summary>
    public class MainMenuViewModel : ReactiveObject
    {
        private List<MainMenuEntry> _entries = new();

        public List<MainMenuEntry> Entries
        {
            get => _entries;
            set => this.RaiseAndSetIfChanged(ref _entries, value);
        }
    }

    /// <summary>
    /// Represents a single entry in the main menu
    /// </summary>
    public class MainMenuEntry
    {
        public string TranslationKey { get; }

        public ICommand Activate { get; }

        public MainMenuEntry(string translationKey, Action onClick)
        {
            TranslationKey = translationKey;
            Activate = new ActionCommand(onClick);
        }
    }
}
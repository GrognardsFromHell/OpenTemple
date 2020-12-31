using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Markup.Xaml;
using ReactiveUI;

namespace OpenTemple.Core.Ui.PartyPool
{
    /// <summary>
    /// Widget that displays an available player character in the pool.
    /// </summary>
    [PseudoClasses(":inparty", ":incompatible")]
    internal class PartyPoolRow : UserControl
    {
        private IDisposable _subscription;

        private PartyPoolRowModel ViewModel => DataContext as PartyPoolRowModel;

        // Original render function was @ 0x101652e0
        public PartyPoolRow()
        {
            AvaloniaXamlLoader.Load(this);
        }

        protected override void OnDataContextChanged(EventArgs e)
        {
            _subscription?.Dispose();

            base.OnDataContextChanged(e);

            _subscription = ViewModel
                ?.WhenAnyValue(m => m.InParty, m => m.IsOpposedAlignment, m => m.PaladinOpposedAlignment)
                .Subscribe(args =>
                {
                    var (inParty, opposedAlignment, paladinOpposedAlignment) = args;
                    PseudoClasses.Toggle(":inparty", inParty);
                    PseudoClasses.Toggle(":incompatible", opposedAlignment || paladinOpposedAlignment);
                });
        }
    }
}

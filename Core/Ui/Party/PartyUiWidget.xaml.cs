using System.Collections.Generic;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using OpenTemple.Core.GameObject;
using OpenTemple.Core.Systems;
using ReactiveUI;

namespace OpenTemple.Core.Ui.Party
{
    public class PartyUiModel : ReactiveObject
    {
        private readonly ObservableAsPropertyHelper<IReadOnlyList<GameObjectBody>> _partyMembers;

        public PartyUiModel()
        {
            _partyMembers = GameSystems.Party.PartyMembersChanged.ToProperty(this, nameof(PartyMembers),
                initialValue: GameSystems.Party.PartyMembers);
        }

        public IReadOnlyList<GameObjectBody> PartyMembers => _partyMembers.Value;

    }

    public class PartyUiWidget : Canvas
    {
        public static readonly AvaloniaProperty PortraitsProperty = AvaloniaProperty.Register<PartyUiWidget, AvaloniaList<PartyPortrait>>(nameof(Portraits));

        public AvaloniaList<PartyPortrait> Portraits
        {
            get => (AvaloniaList<PartyPortrait>) GetValue(PortraitsProperty);
            set => SetValue(PortraitsProperty, value);
        }

        public PartyUiWidget()
        {
            DataContext = new PartyUiModel();
            AvaloniaXamlLoader.Load(this);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Linq;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using OpenTemple.Core.GameObject;
using OpenTemple.Core.Systems;

namespace OpenTemple.Core.Ui.Party
{
    public class PartyUiWidget : Canvas
    {
        public IObservable<IReadOnlyList<GameObjectBody>> PartyMembers => GameSystems.Party.PartyMembersChanged
            // Something in the binding chain breaks if we keep emitting the same object
            .Select(partyMembers =>
                ImmutableList.CreateRange(
                    // Don't show portraits for AI followers
                    partyMembers.Where(partyMember => !GameSystems.Party.IsAiFollower(partyMember))
                )
            );

        public PartyUiWidget()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}

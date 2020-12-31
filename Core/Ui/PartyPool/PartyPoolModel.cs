using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using DynamicData;
using ReactiveUI;

namespace OpenTemple.Core.Ui.PartyPool
{
    public class PartyPoolModel : ReactiveObject
    {
        /// <summary>
        /// Compare party pool rows by the player names (ignoring case).
        /// </summary>
        private static readonly IComparer<PartyPoolRowModel> NameComparator =
            Comparer<PartyPoolRowModel>.Create((x, y) =>
                string.Compare(x.PlayerName, y.PlayerName, StringComparison.CurrentCultureIgnoreCase));

        private readonly ObservableAsPropertyHelper<IReadOnlyCollection<PartyPoolRowModel>> _filteredPlayers;

        public SourceList<PartyPoolRowModel> Players { get; } = new();

        public IReadOnlyCollection<PartyPoolRowModel> FilteredPlayers => _filteredPlayers.Value;

        private PartyPoolRowModel _selectedPlayer;

        public PartyPoolRowModel SelectedPlayer
        {
            get => _selectedPlayer;
            set => this.RaiseAndSetIfChanged(ref _selectedPlayer, value);
        }

        private List<PartyMemberSlotModel> _slots = new();

        public List<PartyMemberSlotModel> Slots
        {
            get => _slots;
            set => this.RaiseAndSetIfChanged(ref _slots, value);
        }

        private string _partyAlignmentText;

        public string PartyAlignmentText
        {
            get => _partyAlignmentText;
            set => this.RaiseAndSetIfChanged(ref _partyAlignmentText, value);
        }

        private bool _hidePremadePlayers;

        public bool HidePremadePlayers
        {
            get => _hidePremadePlayers;
            set => this.RaiseAndSetIfChanged(ref _hidePremadePlayers, value);
        }

        private bool _hideIncompatiblePlayers;

        public bool HideIncompatiblePlayers
        {
            get => _hideIncompatiblePlayers;
            set => this.RaiseAndSetIfChanged(ref _hideIncompatiblePlayers, value);
        }

        public PartyPoolModel()
        {
            var filterObservable = this.WhenAnyValue(
                p => p.HideIncompatiblePlayers,
                p => p.HidePremadePlayers).Select((filter) =>
            {
                var (hideIncompatible, hidePremade) = filter;
                return (Func<PartyPoolRowModel, bool>) (model =>
                {
                    if (hideIncompatible && !model.CanJoin)
                    {
                        return false;
                    }

                    if (hidePremade && model.IsPremade)
                    {
                        return false;
                    }

                    return true;
                });
            });

            _filteredPlayers = Players.Connect()
                .Filter(filterObservable)
                .Sort(NameComparator)
                .ToCollection()
                .ToProperty(this, p => p.FilteredPlayers);
        }
    }
}

using System;
using System.Reactive.Linq;
using System.Windows.Input;
using ReactiveUI;

namespace OpenTemple.Core.Ui.PartyPool
{
    public class PartyMemberSlotModel : ReactiveObject
    {
        private readonly PartyPoolModel _poolModel;

        private readonly ObservableAsPropertyHelper<bool> _selected;

        public PartyMemberSlotModel(PartyPoolModel poolModel)
        {
            _poolModel = poolModel;
            Select = new ActionCommand(SelectThis);
            _selected = poolModel.WhenAnyValue(pm => pm.SelectedPlayer)
                .Select(x => Player != null && x == Player)
                .ToProperty(this, x => x.IsSelected);
        }

        private void SelectThis()
        {
            _poolModel.SelectedPlayer = Player;
        }

        public bool IsSelected => _selected.Value;

        private PartyPoolRowModel _player;

        public PartyPoolRowModel Player
        {
            get => _player;
            set => this.RaiseAndSetIfChanged(ref _player, value);
        }

        public ICommand Select { get; }
    }
}

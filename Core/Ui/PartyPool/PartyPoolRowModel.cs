using System;
using System.Reactive.Linq;
using Avalonia.Media.Imaging;
using OpenTemple.Core.GameObject;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.D20;
using ReactiveUI;

namespace OpenTemple.Core.Ui.PartyPool
{
    public class PartyPoolRowModel : ReactiveObject
    {
        private readonly ObservableAsPropertyHelper<bool> _canJoin;

        private readonly ObservableAsPropertyHelper<string> _statusText;

        public PartyPoolPlayer Player { get; }

        public string PlayerName { get; init; }

        public GameObjectBody GameObject { get; set; }

        public string GenderText { get; init; }

        public string RaceText { get; init; }

        public string ClassText { get; init; }

        public string AlignmentText { get; init; }

        public bool IsPremade { get; }

        public IBitmap Portrait { get; init; }

        public string StatusText => _statusText.Value;

        private bool _isOpposedAlignment;

        public bool IsOpposedAlignment
        {
            get => _isOpposedAlignment;
            set => this.RaiseAndSetIfChanged(ref _isOpposedAlignment, value);
        }

        private bool _paladinOpposedAlignment;

        public bool PaladinOpposedAlignment
        {
            get => _paladinOpposedAlignment;
            set => this.RaiseAndSetIfChanged(ref _paladinOpposedAlignment, value);
        }

        private bool _wasInParty;

        /// <summary>
        /// Was previously in the party, which means somewhere in the game world, the PC exists.
        /// Only applies if an existing in-game party is being edited.
        /// </summary>
        public bool WasInParty
        {
            get => _wasInParty;
            set => this.RaiseAndSetIfChanged(ref _wasInParty, value);
        }

        private bool _inParty;

        public bool InParty
        {
            get => _inParty;
            set => this.RaiseAndSetIfChanged(ref _inParty, value);
        }

        public bool CanJoin => _canJoin.Value;

        public static readonly PartyPoolRowModel DesignInstance = new(new PartyPoolPlayer
        {
            alignment = Alignment.EVIL
        });

        public PartyPoolRowModel(PartyPoolPlayer player)
        {
            var portrait =
                new Bitmap(GameSystems.UiArtManager.GetPortraitPath(player.portraitId, PortraitVariant.Small));

            Player = player;
            PlayerName = player.name;
            Portrait = portrait;
            GenderText = GameSystems.Stat.GetGenderName(player.gender);
            RaceText = GameSystems.Stat.GetRaceName(player.race);
            ClassText = GameSystems.Stat.GetStatName(player.primaryClass);
            AlignmentText = GameSystems.Stat.GetAlignmentName(player.alignment);
            IsPremade = player.premade;

            _canJoin = this.WhenAnyValue(p => p.IsOpposedAlignment,
                    p => p.PaladinOpposedAlignment, p => p.WasInParty)
                .Select(args =>
                {
                    var (opposedAlignment, paladinOpposedAlignment, wasInParty) = args;
                    return !opposedAlignment && !paladinOpposedAlignment && !wasInParty;
                }).ToProperty(this, p => p.CanJoin);

            _statusText = this.WhenAnyValue(x => x.WasInParty, x => x.CanJoin, x => x.InParty)
                .Select(tuple =>
                {
                    var (wasInParty, canJoin, inParty) = tuple;
                    return UpdateStatusText(wasInParty, canJoin, inParty);
                })
                .ToProperty(this, x => x.StatusText);
        }

        private static string UpdateStatusText(bool wasInParty, bool canJoin, bool inParty)
        {
            if (wasInParty)
            {
                return "#{party_pool:31}"; // Has joined party
            }
            else if (!canJoin)
            {
                return "#{party_pool:32}"; // Not compatible
            }
            else if (inParty)
            {
                // Currently in party
                return "#{party_pool:31}"; // Has joined party
            }
            else
            {
                return "#{party_pool:30}"; // Not in party
            }
        }
    }
}

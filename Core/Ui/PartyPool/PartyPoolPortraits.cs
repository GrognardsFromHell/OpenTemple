using System;
using System.Drawing;
using System.Linq;
using OpenTemple.Core.GameObject;
using OpenTemple.Core.GFX;
using OpenTemple.Core.Startup;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui.PartyPool
{
    /// <summary>
    /// This shows a row of portraits for the current players in the party.
    /// </summary>
    internal class PartyPoolPortraits
    {
        public WidgetContainer Container { get; }

        private readonly PartyPoolPortrait[] _portraits;

        private GameObjectBody _selected;

        public GameObjectBody Selected
        {
            get => _selected;
            set
            {
                _selected = value;
                Update();
            }
        }

        public event Action OnSelectedChanged;

        public PartyPoolPortraits()
        {
            Container = new WidgetContainer(0, 0, 0, 0);

            // Create as many portraits as the maximum number of players in the party
            _portraits = new PartyPoolPortrait[Globals.Config.MaxPCs];
            for (var i = 0; i < Globals.Config.MaxPCs; i++)
            {
                var portrait = new PartyPoolPortrait();
                if (i > 0)
                {
                    portrait.X = _portraits[i - 1].X + _portraits[i - 1].Width;
                }

                portrait.SetClickHandler(() => Select(portrait.Player));
                _portraits[i] = portrait;
                Container.Add(portrait);
            }

            var lastPortrait = _portraits[^1];
            Container.Height = lastPortrait.Height;
            Container.Width = lastPortrait.X + lastPortrait.Width;
        }

        private void Select(GameObjectBody player)
        {
            Selected = player;
            OnSelectedChanged?.Invoke();
        }

        public void Update()
        {
            var players = GameSystems.Party.PlayerCharacters.ToArray();

            for (var i = 0; i < _portraits.Length; i++)
            {
                if (i < players.Length)
                {
                    _portraits[i].Player = players[i];
                    _portraits[i].SetActive(players[i] == _selected);
                }
                else
                {
                    _portraits[i].Player = null;
                    _portraits[i].SetActive(false);
                }
            }
        }
    }

    internal class PartyPoolPortrait : WidgetButton
    {
        private WidgetImage _portrait;

        private GameObjectBody _player;

        public GameObjectBody Player
        {
            get => _player;
            set
            {
                _player = value;
                Update();
            }
        }

        public PartyPoolPortrait()
        {
            SetStyle("partyPoolPortrait");
        }

        public override void Render()
        {
            base.Render();

            if (_portrait != null)
            {
                _portrait.Color = IsActive() ? new PackedLinearColorA(0xFF1AC3FF) : PackedLinearColorA.White;

                var contentArea = GetContentArea();
                contentArea.Offset(4, 4);
                contentArea.Size = new Size(53, 47);
                _portrait.SetContentArea(contentArea);
                _portrait.Render();
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                _portrait.Dispose();
            }
        }

        private void Update()
        {
            SetDisabled(_player == null);
            if (_player != null)
            {
                var portraitId = _player.GetInt32(obj_f.critter_portrait);
                var portrait = GameSystems.UiArtManager.GetPortraitPath(portraitId, PortraitVariant.Small);
                if (_portrait == null)
                {
                    _portrait = new WidgetImage(portrait);
                }
                else
                {
                    _portrait.SetTexture(portrait);
                }
            }
            else
            {
                _portrait?.Dispose();
                _portrait = null;
            }
        }
    }
}
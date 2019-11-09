using System.Drawing;
using SpicyTemple.Core.GFX;
using SpicyTemple.Core.GFX.TextRendering;
using SpicyTemple.Core.Systems;
using SpicyTemple.Core.TigSubsystems;
using SpicyTemple.Core.Ui.WidgetDocs;

namespace SpicyTemple.Core.Ui.PartyPool
{
    /// <summary>
    /// Widget that displays an available player character in the pool.
    /// </summary>
    internal class PartyPoolSlot : WidgetButtonBase
    {
        private PartyPoolPlayer _player;

        private readonly WidgetRectangle _border;

        private readonly WidgetImage _portrait;

        private readonly WidgetText _text;

        public PartyPoolPlayer Player
        {
            get => _player;
            set
            {
                _player = value;
                UpdateWidgets();
            }
        }

        public PartyPoolSlot() : base(new Rectangle(0, 0, 452, 51))
        {
            // Original render function was @ 0x101652e0
            _border = new WidgetRectangle();
            AddContent(_border);

            _portrait = new WidgetImage();
            _portrait.FixedSize = new Size(51, 45);
            _portrait.SetX(4);
            _portrait.SetY(3);
            AddContent(_portrait);

            _text = new WidgetText();
            _text.SetX(57);
            _text.SetStyleId("partyPoolSlot");
            _text.LegacyAdditionalTextColors = new[]
            {
                new ColorRect(new PackedLinearColorA(255, 0, 0, 255)),
            };
            AddContent(_text);
        }

        private void UpdateWidgets()
        {
            if (_player == null)
            {
                SetVisible(false);
                return;
            }

            // Update the portrait
            PackedLinearColorA? backgroundColor = null;
            var borderColor = new PackedLinearColorA(0xFF5d5d5d);

            SetVisible(true);
            var statusText = "#{party_pool:30}"; // Not in party
            if (_player.state != SlotState.CanJoin && _player.Selected)
            {
                borderColor = new PackedLinearColorA(0xFFA90000);
                backgroundColor = new PackedLinearColorA(0xFF960000);
                statusText = "#{party_pool:32}"; // Not compatible
            }
            else if (_player.state == SlotState.WasInParty)
            {
                // Was previously in party, which means somewhere in the game world, the PC exists
                borderColor = new PackedLinearColorA(0xFFA90000);
                backgroundColor = new PackedLinearColorA(0xFF520000);
                statusText = "#{party_pool:31}"; // Has joined party
            }
            else if (_player.state != SlotState.CanJoin)
            {
                borderColor = new PackedLinearColorA(0xFFA90000);
                backgroundColor = new PackedLinearColorA(0xFF520000);
                statusText = "#{party_pool:32}"; // Not compatible
            }
            else if (_player.flag4)
            {
                // Currently in party
                backgroundColor = new PackedLinearColorA(0xFF205120);
                borderColor = new PackedLinearColorA(0xFF08C403);
                statusText = "#{party_pool:31}"; // Has joined party
            }
            else if (_player.Selected)
            {
                backgroundColor = new PackedLinearColorA(0xFF133159);
                borderColor = new PackedLinearColorA(0xFF1AC3FF);
            }

            _border.Pen = borderColor;
            if (backgroundColor.HasValue)
            {
                _border.Brush = new Brush(backgroundColor.Value);
            }
            else
            {
                _border.Brush = null;
            }
            _portrait.SetTexture(GameSystems.UiArtManager.GetPortraitPath(_player.portraitId, PortraitVariant.Small));

            var classColorMarker = "";
            var classSuffix = "";
            var alignmentColorMarker = "";
            var alignmentSuffix = "";
            if (_player.state == SlotState.OpposedAlignment)
            {
                alignmentSuffix = "#{party_pool:40}";
                alignmentColorMarker = "@1";
            }
            else if (_player.state == SlotState.PaladinOpoposedAlignment)
            {
                classSuffix = "#{party_pool:41}";
                classColorMarker = "@1";
            }

            var genderText = GameSystems.Stat.GetGenderName(_player.gender);
            var raceText = GameSystems.Stat.GetRaceName(_player.race);
            var classText = GameSystems.Stat.GetStatName(_player.primaryClass);
            var alignmentText = GameSystems.Stat.GetAlignmentName(_player.alignment);

            var text = $"{_player.name} - {statusText}\n"
                       + $"{genderText} {raceText}\n"
                       + $"{classColorMarker}{classText}{classSuffix}\n"
                       + $"{alignmentColorMarker}{alignmentText}{alignmentSuffix}\n";
            _text.SetText(text);
        }
    }

    public enum SlotState
    {
        CanJoin = 0,
        OpposedAlignment = 2,
        PaladinOpoposedAlignment = 3,
        WasInParty = 4
    }
}
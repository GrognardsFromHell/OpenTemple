using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;
using SpicyTemple.Core.GFX;
using SpicyTemple.Core.IO.Images;
using SpicyTemple.Core.IO.SaveGames;
using SpicyTemple.Core.Systems;
using SpicyTemple.Core.TigSubsystems;
using SpicyTemple.Core.Ui.WidgetDocs;

namespace SpicyTemple.Core.Ui.SaveGame
{
    public class SaveGameSlotButton : WidgetButtonBase
    {
        private const string BorderImagePath = "art/interface/loadgame/box.img";

        private const string SelectedBorderImagePath = "art/interface/loadgame/box_selected.img";

        private bool _selected;

        private readonly WidgetImage _borderImage;

        private readonly WidgetImage _screenshot;

        private readonly WidgetText _label;

        public SaveGameInfo SaveGame { get; private set; }

        public SaveGameSlotButton(Rectangle rect) : base(rect)
        {
            _borderImage = new WidgetImage(BorderImagePath);
            AddContent(_borderImage);

            _screenshot = new WidgetImage();
            _screenshot.SetX(2);
            _screenshot.SetY(2);
            _screenshot.FixedSize = new Size(64, 48);
            AddContent(_screenshot);

            _label = new WidgetText("", "loadGameSlot");
            _label.SetX(67);
            _label.SetY(2);
            _label.FixedSize = new Size(268, 48);
            AddContent(_label);
        }

        public bool Selected
        {
            get => _selected;
            set
            {
                _selected = value;
                if (value)
                {
                    _borderImage.SetTexture(SelectedBorderImagePath);
                }
                else
                {
                    _borderImage.SetTexture(BorderImagePath);
                }
            }
        }

        public void SetSaveInfo(SaveGameInfo info)
        {
            SetVisible(true);
            SaveGame = info;

            var areaId = GameSystems.Area.GetAreaFromMap(info.MapId);
            var areaDescription = GameSystems.Area.GetAreaDescription(areaId);

            var displayText = new StringBuilder();
            displayText.Append(info.Name).Append('\n');
            displayText.Append(areaDescription).Append('\n');

            displayText.Append(info.LastModified.ToString("d", CultureInfo.CurrentCulture));
            displayText.Append(' ');
            displayText.Append(info.LastModified.ToString("t", CultureInfo.CurrentCulture));
            displayText.Append(" - Day ");
            displayText.Append(info.GameTime.timeInDays + 1);
            displayText.Append('\n');

            _label.SetText(displayText.ToString());

            if (info.SmallScreenshotPath != null)
            {
                // We're doing this crap because the textures can't load data from an OS filesystem path
                var data = File.ReadAllBytes(info.SmallScreenshotPath);
                var decodedImage = ImageIO.DecodeImage(data);
                using var texture = Tig.RenderingDevice.CreateDynamicTexture(BufferFormat.X8R8G8B8,
                    decodedImage.info.width,
                    decodedImage.info.height);
                texture.Resource.UpdateRaw(decodedImage.data, decodedImage.info.width * 4);

                _screenshot.SetTexture(texture.Resource);
                _screenshot.Visible = true;
            }
            else
            {
                _screenshot.Visible = false;
            }
        }

        public void ClearSaveInfo()
        {
            SetVisible(false);
            SaveGame = null;
        }
    }
}
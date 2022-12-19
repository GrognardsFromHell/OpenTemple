using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;
using OpenTemple.Core.GFX;
using OpenTemple.Core.IO.Images;
using OpenTemple.Core.IO.SaveGames;
using OpenTemple.Core.Platform;
using OpenTemple.Core.Systems;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Ui.Widgets;
using static SDL2.SDL;

namespace OpenTemple.Core.Ui.SaveGame;

public class SaveGameSlotButton : WidgetButtonBase
{
    private const string BorderImagePath = "art/interface/loadgame/box.img";

    private const string SelectedBorderImagePath = "art/interface/loadgame/box_selected.img";

    private bool _selected;

    private readonly WidgetImage _borderImage;

    private readonly WidgetImage _screenshot;

    private readonly WidgetText _label;

    public int CaretPosition { get; set; }

    public SaveGameInfo SaveGame { get; private set; }

    public bool IsOverwritingSave { get; private set; }

    /// <summary>
    /// When the player is entering a new name for a save game this contains what
    /// is currently being entered.
    /// </summary>
    public string NewName { get; private set; }

    public SaveGameSlotButton(Rectangle rect) : base(rect)
    {
        _borderImage = new WidgetImage(BorderImagePath);
        AddContent(_borderImage);

        _screenshot = new WidgetImage();
        _screenshot.X = 2;
        _screenshot.Y = 2;
        _screenshot.FixedSize = new Size(64, 48);
        AddContent(_screenshot);

        _label = new WidgetText("", "loadGameSlot");
        _label.X = 67;
        _label.Y = 2;
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

            if (IsOverwritingSave)
            {
                CaretPosition = NewName.Length;
                if (SaveGame != null)
                {
                    UpdateLabel();
                }
            }
        }
    }

    public void SetSaveInfo(SaveGameInfo info, bool overwriteSave = false)
    {
        Visible = true;
        SaveGame = info;
        IsOverwritingSave = overwriteSave;
        NewName = info.Name ?? "";

        UpdateLabel();

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

    private void UpdateLabel()
    {
        var info = SaveGame;

        if (IsOverwritingSave && (info.Type == SaveGameType.NewSave || _selected))
        {
            if (_selected)
            {
                _label.Text = NewName + "|";
            }
            else
            {
                _label.Text = "#{savegame:3}";
            }

            return;
        }

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

        _label.Text = displayText.ToString();
    }

    private void UpdateNewSaveName()
    {
        if (!Selected)
        {
            _label.Text = "#{savegame:3}";
            return;
        }

        CaretPosition = Math.Clamp(CaretPosition, 0, NewName.Length);

        // Insert the caret
        var displayedText = NewName.Insert(CaretPosition, "|");
        _label.Text = displayedText;

        // This is _incredibly_ bad, but it's what vanilla ToEE did :-(
        while (_label.GetPreferredSize().Width >= _label.FixedWidth)
        {
            displayedText = displayedText.Substring(1);
            _label.Text = displayedText;
        }
    }

    public void ClearSaveInfo()
    {
        Visible = false;
        SaveGame = null;
    }

    public void AppendNewName(string text)
    {
        if (IsOverwritingSave && _selected)
        {
            NewName = NewName.Insert(CaretPosition, text);
            CaretPosition += text.Length;
            UpdateNewSaveName();
        }
    }
}
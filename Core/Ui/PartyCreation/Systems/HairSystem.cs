using System;
using System.Collections.Generic;
using System.Text.Json;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.GFX;
using OpenTemple.Core.IO;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Systems.GameObjects;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui.PartyCreation.Systems;

[TempleDllLocation(0x102f79e8)]
internal class HairSystem : IChargenSystem
{
    public string HelpTopic => "TAG_CHARGEN_HAIR";

    public ChargenStages Stage => ChargenStages.CG_Stage_Hair;

    public WidgetContainer Container { get; }

    private CharEditorSelectionPacket _pkt;

    // Previously read from rules/pc_creation_hair.mes, but since the color
    // is baked into the textures this seems kinda pointless
    private static readonly PackedLinearColorA[] HairColors =
    {
        // 0-7 Hair Colors (RGBA)
        new(24, 22, 30, 255), //Black
        new(255, 224, 146, 255), //Blonde
        new(68, 146, 192, 255), //Blue
        new(115, 75, 67, 255), //Brown
        new(207, 144, 102, 255), //LtBrown
        new(223, 158, 205, 255), //Pink
        new(217, 131, 75, 255), //Red
        new(251, 251, 251, 255), //White
    };

    private static readonly HairStyle[] HairStyles =
    {
        HairStyle.Longhair,
        HairStyle.Ponytail,
        HairStyle.Shorthair,
        HairStyle.Topknot,
        HairStyle.Mullet,
        HairStyle.Bald,
        HairStyle.Mohawk,
        HairStyle.Medium
    };

    private readonly HairColorButton[] _colorButtons = new HairColorButton[8];
    private readonly HairColorButton[] _styleButtons = new HairColorButton[HairStyles.Length];

    [TempleDllLocation(0x10189240)]
    public HairSystem()
    {
        var doc = WidgetDoc.Load("ui/pc_creation/hair_ui.json", CustomElementFactory);
        Container = doc.GetRootContainer();
        Container.Visible = false;

        for (var i = 0; i < HairColors.Length; i++)
        {
            var button = (HairColorButton) doc.GetWidget("color" + i);
            _colorButtons[i] = button;
            button.HairColor = HairColors[i];
            var color = (HairColor) i;
            button.AddClickListener(() =>
            {
                _pkt.hairColor = color;
                UpdateButtons();
                UpdateModelHair(_pkt, UiSystems.PCCreation.EditedChar);
            });
        }

        for (var i = 0; i < HairStyles.Length; i++)
        {
            var style = HairStyles[i];
            var button = (HairColorButton) doc.GetWidget("style" + i);
            _styleButtons[i] = button;
            button.AddClickListener(() =>
            {
                _pkt.hairStyle = style;
                UpdateButtons();
                UpdateModelHair(_pkt, UiSystems.PCCreation.EditedChar);
            });
        }
    }

    private static WidgetBase CustomElementFactory(string type, JsonElement definition)
    {
        if (type == "colorButton" || type == "styleButton")
        {
            return new HairColorButton();
        }

        throw new ArgumentException();
    }

    [TempleDllLocation(0x10188a30)]
    public void Reset(CharEditorSelectionPacket selPkt)
    {
        _pkt = selPkt;
        selPkt.hairStyle = null;
        selPkt.hairColor = null;
    }

    [TempleDllLocation(0x10188ef0)]
    public void Dispose()
    {
    }

    [TempleDllLocation(0x10188ee0)]
    public void Activate()
    {
        UpdateButtons();
    }

    private void UpdateButtons()
    {
        if (!_pkt.raceId.HasValue || !_pkt.genderId.HasValue)
        {
            return;
        }

        // Update the images of the style buttons to show the correct style AND color
        for (var i = 0; i < _styleButtons.Length; i++)
        {
            var hairSettings = CreateHairSettings(_pkt);
            hairSettings.Style = HairStyles[i];
            _styleButtons[i].HairStyleTexture = hairSettings.PreviewTexturePath;
            _styleButtons[i].Selected = _pkt.hairStyle == HairStyles[i];
        }

        for (var i = 0; i < _colorButtons.Length; i++)
        {
            var color = (HairColor) i;
            _colorButtons[i].Selected = _pkt.hairColor == color;
        }
    }

    [TempleDllLocation(0x10188a90)]
    public bool CheckComplete()
    {
        return _pkt.hairStyle.HasValue && _pkt.hairColor.HasValue;
    }

    [TempleDllLocation(0x10188ab0)]
    public void Finalize(CharEditorSelectionPacket charSpec, ref GameObject playerObj)
    {
        UpdateModelHair(charSpec, playerObj);
    }

    private static HairSettings CreateHairSettings(CharEditorSelectionPacket charSpec)
    {
        return new HairSettings
        {
            Size = HairStyleSize.Small,
            Race = D20RaceSystem.GetHairStyle(charSpec.raceId.GetValueOrDefault()),
            Gender = charSpec.genderId.GetValueOrDefault(),
            Color = charSpec.hairColor.GetValueOrDefault(),
            Style = charSpec.hairStyle.GetValueOrDefault()
        };
    }

    private static void UpdateModelHair(CharEditorSelectionPacket charSpec, GameObject playerObj)
    {
        var hairSettings = CreateHairSettings(charSpec);
        playerObj.SetInt32(obj_f.critter_hair_style, hairSettings.Pack());
        GameSystems.Critter.UpdateModelEquipment(playerObj);
    }

    public bool CompleteForTesting(Dictionary<string, object> props)
    {
        _pkt.hairStyle = GameSystems.Random.PickRandom(HairStyles);
        _pkt.hairColor = (HairColor) GameSystems.Random.GetInt(0, 7);
        UpdateModelHair(_pkt, UiSystems.PCCreation.EditedChar);
        return true;
    }
}
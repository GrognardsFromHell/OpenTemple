using System;
using System.Collections.Generic;
using System.Drawing;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.GFX;
using SpicyTemple.Core.IO;
using SpicyTemple.Core.Systems;
using SpicyTemple.Core.TigSubsystems;
using SpicyTemple.Core.Ui.WidgetDocs;

namespace SpicyTemple.Core.Ui.Party
{
    public class PartyUi : AbstractUi, IDisposable
    {
        private Dictionary<int, string> _translations;

        private Dictionary<int, string> _uiParams;

        private int party_ui_cell_spacing;
        private Rectangle party_ui_main_window;
        private Rectangle party_ui_portrait_button;
        private Rectangle party_ui_fx_button;
        private Rectangle party_ui_fx_button_all;
        private Rectangle party_ui_hot_key_button;
        private Rectangle party_ui_hp_meter;
        private Rectangle party_ui_subdual_damage_meter;
        private Rectangle party_ui_remove_icon;
        private Rectangle party_ui_level_icon;
        private PackedLinearColorA outer_border_color;
        private PackedLinearColorA inner_border_color;
        private string font_normal_name;
        private int font_normal_size;
        private string font_bold_name;
        private int font_bold_size;
        private string font_big_name;
        private int font_big_size;
        private PackedLinearColorA font_normal_color;
        private PackedLinearColorA font_dark_color;
        private int tooltip_style;
        private Rectangle buff_icons;
        private int spacing;
        private int ailment_y;
        private Rectangle condition;
        private int condition_spacing;
        private Rectangle party_ui_frame;

        private ResourceRef<ITexture> dismissBtnNormal;
        private ResourceRef<ITexture> dismissBtnHover;
        private ResourceRef<ITexture> dismissBtnPressed;
        private ResourceRef<ITexture> dismissBtnDisabled;
        private ResourceRef<ITexture> levelUpBtnNormal;
        private ResourceRef<ITexture> levelUpBtnHovered;
        private ResourceRef<ITexture> levelUpBtnPressed;
        private ResourceRef<ITexture> levelUpBtnDisabled;
        private ResourceRef<ITexture> healthBar;
        private ResourceRef<ITexture> subdualMeterOff;
        private ResourceRef<ITexture> highlight;
        private ResourceRef<ITexture> highlightPressed;
        private ResourceRef<ITexture> highlightHover;
        private ResourceRef<ITexture> portraitGotHit;
        private ResourceRef<ITexture> healthBarEmpty;
        private ResourceRef<ITexture> healthBarHurt;
        private ResourceRef<ITexture> portraitFrame;

        [TempleDllLocation(0x10BE33F8)]
        public GameObjectBody ForceHovered { get; set; }

        [TempleDllLocation(0x10BE3400)]
        public GameObjectBody ForcePressed { get; set; }

        [TempleDllLocation(0x10BE2E98)]
        private List<PartyUiPortrait> _portraits = new List<PartyUiPortrait>();

        [TempleDllLocation(0x10BE33E0)]
        private bool ui_party_widgets_need_refresh;

        private WidgetContainer _container;

        [TempleDllLocation(0x10134bf0)]
        public PartyUi()
        {
            _translations = Tig.FS.ReadMesFile("mes/party_ui.mes");

            _uiParams = Tig.FS.ReadMesFile("art/interface/party_ui/1_party_ui.mes");

            LoadUiParameters(Tig.RenderingDevice.GetCamera().ScreenSize);
            LoadTextures();

            CreateWidgets();
            ui_party_widgets_need_refresh = true;

            UiSystems.InGameSelect.LoadSelectionShaders();
        }

        [TempleDllLocation(0x10133800)]
        private void CreateWidgets()
        {
            var doc = WidgetDoc.Load("ui/party_ui.json");

            _container = doc.TakeRootContainer();

            var screenSize = Tig.RenderingDevice.GetCamera().ScreenSize;

            _container.SetWidth(screenSize.Width - 2 * party_ui_main_window.X);
            _container.SetY(party_ui_main_window.Y);
        }

        private WidgetContainer CreateWidget(GameObjectBody partyMember)
        {
            var container = new WidgetContainer(party_ui_main_window.Width, party_ui_main_window.Height);

            var image = new WidgetImage("art/interface/party_ui/Character Portrait Frame.tga");
            image.SetX(party_ui_frame.X);
            image.SetY(party_ui_frame.Y);
            image.SetFixedWidth(party_ui_frame.Width);
            image.SetFixedHeight(party_ui_frame.Height);
            container.AddContent(image);

            var portraitButton = new PortraitButton(partyMember);
            portraitButton.SetSize(party_ui_portrait_button.Size);
            portraitButton.SetPos(party_ui_portrait_button.X, party_ui_portrait_button.Y);
            container.Add(portraitButton);

            _container.Add(container);

            return container;
        }

        [TempleDllLocation(0x101342a0)]
        private void LoadUiParameters(Size viewportSize)
        {
            party_ui_cell_spacing = int.Parse(_uiParams[10]);

            Rectangle LoadRectangleParam(int baseId) =>
                new Rectangle(
                    int.Parse(_uiParams[baseId]),
                    int.Parse(_uiParams[baseId + 1]),
                    int.Parse(_uiParams[baseId + 2]),
                    int.Parse(_uiParams[baseId + 3])
                );

            PackedLinearColorA LoadColorParam(int baseId) =>
                new PackedLinearColorA(
                    byte.Parse(_uiParams[baseId]),
                    byte.Parse(_uiParams[baseId + 1]),
                    byte.Parse(_uiParams[baseId + 2]),
                    byte.Parse(_uiParams[baseId + 3])
                );

            party_ui_main_window = LoadRectangleParam(20);
            if (party_ui_main_window.Y < 0)
            {
                party_ui_main_window.Y = viewportSize.Height + party_ui_main_window.Y;
            }

            party_ui_portrait_button = LoadRectangleParam(40);
            party_ui_fx_button = LoadRectangleParam(60); // TODO: UNUSED?
            party_ui_fx_button_all = LoadRectangleParam(80); // TODO: UNUSED?
            party_ui_hot_key_button = LoadRectangleParam(100); // TODO: UNUSED?
            party_ui_hp_meter = LoadRectangleParam(120);
            party_ui_subdual_damage_meter = LoadRectangleParam(140);
            party_ui_remove_icon = LoadRectangleParam(160);
            party_ui_level_icon = LoadRectangleParam(170);
            outer_border_color = LoadColorParam(700);
            inner_border_color = LoadColorParam(720);

            font_normal_name = _uiParams[900];
            font_normal_size = int.Parse(_uiParams[901]);
            font_bold_name = _uiParams[902];
            font_bold_size = int.Parse(_uiParams[903]);
            font_big_name = _uiParams[904];
            font_big_size = int.Parse(_uiParams[905]);
            font_normal_color = LoadColorParam(920);
            font_dark_color = LoadColorParam(940);

            tooltip_style = int.Parse(_uiParams[2000]);

            buff_icons = LoadRectangleParam(2100);

            spacing = int.Parse(_uiParams[2110]);

            ailment_y = int.Parse(_uiParams[2200]);

            condition = LoadRectangleParam(2250);
            condition_spacing = int.Parse(_uiParams[2254]);

            party_ui_frame = LoadRectangleParam(2300);
        }

        [TempleDllLocation(0x101325e0)]
        private void LoadTextures()
        {
            var textures = Tig.FS.ReadMesFile("art/interface/party_ui/1_party_ui_textures.mes");

            ResourceRef<ITexture> LoadTexture(int id) =>
                Tig.RenderingDevice.GetTextures().Resolve("art/interface/party_ui/" + textures[id], false);

            dismissBtnNormal = LoadTexture(150);
            dismissBtnHover = LoadTexture(151);
            dismissBtnPressed = LoadTexture(152);
            dismissBtnDisabled = LoadTexture(153);
            levelUpBtnNormal = LoadTexture(160);
            levelUpBtnHovered = LoadTexture(161);
            levelUpBtnPressed = LoadTexture(162);
            levelUpBtnDisabled = LoadTexture(163);
            healthBar = LoadTexture(301);
            subdualMeterOff = LoadTexture(601);
            highlight = LoadTexture(900);
            highlightPressed = LoadTexture(901);
            highlightHover = LoadTexture(902);
            portraitGotHit = LoadTexture(903);
            healthBarEmpty = LoadTexture(910);
            healthBarHurt = LoadTexture(911);
            portraitFrame = LoadTexture(920);
        }

        [TempleDllLocation(0x10134cb0)]
        public void Update()
        {
            var availablePortraits = _portraits;
            _portraits = new List<PartyUiPortrait>(GameSystems.Party.PartySize);

            foreach (var partyMember in GameSystems.Party.PartyMembers)
            {
                if (GameSystems.Party.IsAiFollower(partyMember))
                {
                    continue;
                }

                var existingIdx = availablePortraits.FindIndex(p => p.PartyMember == partyMember);
                if (existingIdx != -1)
                {
                    _portraits.Add(availablePortraits[existingIdx]);
                    availablePortraits.RemoveAt(existingIdx);
                }
                else
                {
                    var widget = CreateWidget(partyMember);
                    _portraits.Add(new PartyUiPortrait(partyMember, widget));
                }
            }

            // Free any remaining unused portraits
            foreach (var portrait in availablePortraits)
            {
                portrait.Dispose();
            }

            Stub.TODO();
        }

        [TempleDllLocation(0x10135000)]
        public void UpdateAndShowMaybe()
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x101350b0)]
        public override void ResizeViewport(Size size)
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x10132760)]
        public void Dispose()
        {
            Stub.TODO();
        }
    }
}
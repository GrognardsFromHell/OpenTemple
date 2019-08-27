using System;
using System.Collections.Generic;
using System.Drawing;
using SpicyTemple.Core.GFX;
using SpicyTemple.Core.IO;
using SpicyTemple.Core.TigSubsystems;

namespace SpicyTemple.Core.Ui.Party
{
    public class PartyUiParams : IDisposable
    {
        public int party_ui_cell_spacing { get; private set; }
        public Rectangle party_ui_main_window { get; private set; }
        public Rectangle party_ui_portrait_button { get; private set; }
        public Rectangle party_ui_fx_button { get; private set; }
        public Rectangle party_ui_fx_button_all { get; private set; }
        public Rectangle party_ui_hot_key_button { get; private set; }
        public Rectangle party_ui_hp_meter { get; private set; }
        public Rectangle party_ui_subdual_damage_meter { get; private set; }
        public Rectangle party_ui_remove_icon { get; private set; }
        public Rectangle party_ui_level_icon { get; private set; }
        public PackedLinearColorA outer_border_color { get; private set; }
        public PackedLinearColorA inner_border_color { get; private set; }
        public string font_normal_name { get; private set; }
        public int font_normal_size { get; private set; }
        public string font_bold_name { get; private set; }
        public int font_bold_size { get; private set; }
        public string font_big_name { get; private set; }
        public int font_big_size { get; private set; }
        public PackedLinearColorA font_normal_color { get; private set; }
        public PackedLinearColorA font_dark_color { get; private set; }
        public int tooltip_style { get; private set; }
        public Rectangle buff_icons { get; private set; }
        public int buff_spacing { get; private set; }
        public int ailment_y { get; private set; }
        public Rectangle condition { get; private set; }
        public int condition_spacing { get; private set; }
        public Rectangle party_ui_frame { get; private set; }

        public ResourceRef<ITexture> dismissBtnNormal { get; private set; }
        public ResourceRef<ITexture> dismissBtnHover { get; private set; }
        public ResourceRef<ITexture> dismissBtnPressed { get; private set; }
        public ResourceRef<ITexture> dismissBtnDisabled { get; private set; }
        public ResourceRef<ITexture> levelUpBtnNormal { get; private set; }
        public ResourceRef<ITexture> levelUpBtnHovered { get; private set; }
        public ResourceRef<ITexture> levelUpBtnPressed { get; private set; }
        public ResourceRef<ITexture> levelUpBtnDisabled { get; private set; }
        public ResourceRef<ITexture> healthBar { get; private set; }
        public ResourceRef<ITexture> subdualMeterOff { get; private set; }
        public ResourceRef<ITexture> highlight { get; private set; }
        public ResourceRef<ITexture> highlightPressed { get; private set; }
        public ResourceRef<ITexture> highlightHover { get; private set; }
        public ResourceRef<ITexture> portraitGotHit { get; private set; }
        public ResourceRef<ITexture> healthBarEmpty { get; private set; }
        public ResourceRef<ITexture> healthBarHurt { get; private set; }
        public ResourceRef<ITexture> portraitFrame { get; private set; }
        public Dictionary<int, string> _translations { get; private set; }

        public Dictionary<PartyUiTexture, string> Textures { get; private set; }

        public PartyUiParams()
        {
            _translations = Tig.FS.ReadMesFile("mes/party_ui.mes");

            var uiParams = Tig.FS.ReadMesFile("art/interface/party_ui/1_party_ui.mes");

            LoadUiParameters(uiParams, Tig.RenderingDevice.GetCamera().ScreenSize);
            LoadTextures();
        }

        [TempleDllLocation(0x101342a0)]
        private void LoadUiParameters(Dictionary<int, string> uiParams, Size viewportSize)
        {
            party_ui_cell_spacing = int.Parse(uiParams[10]);

            var partyUiMainWindow = uiParams.GetRectangleParam(20);
            if (partyUiMainWindow.Y < 0)
            {
                partyUiMainWindow.Y += viewportSize.Height;
            }

            this.party_ui_main_window = partyUiMainWindow;

            party_ui_portrait_button = uiParams.GetRectangleParam(40);
            party_ui_fx_button = uiParams.GetRectangleParam(60); // TODO: UNUSED?
            party_ui_fx_button_all = uiParams.GetRectangleParam(80); // TODO: UNUSED?
            party_ui_hot_key_button = uiParams.GetRectangleParam(100); // TODO: UNUSED?
            party_ui_hp_meter = uiParams.GetRectangleParam(120);
            party_ui_subdual_damage_meter = uiParams.GetRectangleParam(140);
            var partyUiRemoveIcon = uiParams.GetRectangleParam(160);
            // ToEE rendered a src-rect with x=1,y=1 and the original width and we render the entire image instead
            partyUiRemoveIcon.X--;
            partyUiRemoveIcon.Y--;
            partyUiRemoveIcon.Width += 2;
            partyUiRemoveIcon.Height += 2;

            this.party_ui_remove_icon = partyUiRemoveIcon;

            var partyUiLevelIcon = uiParams.GetRectangleParam(170);
            partyUiLevelIcon.X--;
            partyUiLevelIcon.Y--;
            partyUiLevelIcon.Width += 2;
            partyUiLevelIcon.Height += 2;
            this.party_ui_level_icon = partyUiLevelIcon;

            outer_border_color = uiParams.GetColorParam(700);
            inner_border_color = uiParams.GetColorParam(720);

            font_normal_name = uiParams[900];
            font_normal_size = int.Parse(uiParams[901]);
            font_bold_name = uiParams[902];
            font_bold_size = int.Parse(uiParams[903]);
            font_big_name = uiParams[904];
            font_big_size = int.Parse(uiParams[905]);
            font_normal_color = uiParams.GetColorParam(920);
            font_dark_color = uiParams.GetColorParam(940);

            tooltip_style = int.Parse(uiParams[2000]);

            buff_icons = uiParams.GetRectangleParam(2100);

            buff_spacing = int.Parse(uiParams[2110]);

            ailment_y = int.Parse(uiParams[2200]);

            condition = uiParams.GetRectangleParam(2250);
            condition_spacing = int.Parse(uiParams[2254]);

            party_ui_frame = uiParams.GetRectangleParam(2300);
        }

        [TempleDllLocation(0x101325e0)]
        private void LoadTextures()
        {
            var textures = Tig.FS.ReadMesFile("art/interface/party_ui/1_party_ui_textures.mes");

            ResourceRef<ITexture> LoadTexture(int id) =>
                Tig.RenderingDevice.GetTextures().Resolve("art/interface/party_ui/" + textures[id], false);

            Textures = new Dictionary<PartyUiTexture, string>();
            foreach (var textureId in (PartyUiTexture[]) Enum.GetValues(typeof(PartyUiTexture)))
            {
                Textures[textureId] = "art/interface/party_ui/" + textures[(int) textureId];
            }

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


        public void Dispose()
        {
            dismissBtnNormal.Dispose();
            dismissBtnHover.Dispose();
            dismissBtnPressed.Dispose();
            dismissBtnDisabled.Dispose();
            levelUpBtnNormal.Dispose();
            levelUpBtnHovered.Dispose();
            levelUpBtnPressed.Dispose();
            levelUpBtnDisabled.Dispose();
            healthBar.Dispose();
            subdualMeterOff.Dispose();
            highlight.Dispose();
            highlightPressed.Dispose();
            highlightHover.Dispose();
            portraitGotHit.Dispose();
            healthBarEmpty.Dispose();
            healthBarHurt.Dispose();
            portraitFrame.Dispose();
        }
    }
}
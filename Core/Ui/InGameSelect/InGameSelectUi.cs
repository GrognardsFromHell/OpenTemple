using System;
using SpicyTemple.Core.GFX;
using SpicyTemple.Core.GFX.RenderMaterials;
using SpicyTemple.Core.IO;
using SpicyTemple.Core.TigSubsystems;
using SpicyTemple.Core.Ui.WidgetDocs;

namespace SpicyTemple.Core.Ui.InGameSelect
{
    public class InGameSelectUi : AbstractUi, IDisposable
    {
        private ResourceRef<IMdfRenderMaterial> quarterArcShaderId;
        private ResourceRef<IMdfRenderMaterial> invalidSelectionShaderId;

        private PackedLinearColorA validConeOutlineRGBA;
        private PackedLinearColorA validConeInsideRGBA;
        private PackedLinearColorA invalidArcOutlineRGBA;
        private PackedLinearColorA invalidArcInsideRGBA;
        private PackedLinearColorA validAreaOutlineRGBA;
        private PackedLinearColorA validAreaInsideRGBA;
        private PackedLinearColorA invalidAreaOutlineRGBA;
        private PackedLinearColorA invalidAreaInsideRGBA;

        [TempleDllLocation(0x10138a40)]
        public InGameSelectUi()
        {
            LoadShaders();

            quarterArcShaderId = Tig.MdfFactory.LoadMaterial("art/interface/intgame_select/quarter-arc.mdf");
            invalidSelectionShaderId = Tig.MdfFactory.LoadMaterial("art/interface/cursors/InvalidSelection.mdf");

            var intgameRules = Tig.FS.ReadMesFile("rules/intgame_select.mes");

            PackedLinearColorA IntgameGetRGBA(int index)
            {
                if (intgameRules.TryGetValue(index, out var line))
                {
                    var tokenizer = new Tokenizer(line);

                    if (tokenizer.NextToken() && tokenizer.IsNumber)
                    {
                        var red = tokenizer.TokenInt;
                        if (tokenizer.NextToken() && tokenizer.IsNumber)
                        {
                            var green = tokenizer.TokenInt;
                            if (tokenizer.NextToken() && tokenizer.IsNumber)
                            {
                                var blue = tokenizer.TokenInt;
                                if (tokenizer.NextToken() && tokenizer.IsNumber)
                                {
                                    var alpha = tokenizer.TokenInt;
                                    return new PackedLinearColorA(red, green, blue, alpha);
                                }
                            }
                        }
                    }
                }

                return PackedLinearColorA.White;
            }

            validConeOutlineRGBA = IntgameGetRGBA(0);
            validConeInsideRGBA = IntgameGetRGBA(1);
            invalidArcOutlineRGBA = IntgameGetRGBA(2);
            invalidArcInsideRGBA = IntgameGetRGBA(3);
            validAreaOutlineRGBA = IntgameGetRGBA(10);
            validAreaInsideRGBA = IntgameGetRGBA(11);
            invalidAreaOutlineRGBA = IntgameGetRGBA(12);
            invalidAreaInsideRGBA = IntgameGetRGBA(13);

            _window = new WidgetContainer(Tig.RenderingDevice.GetCamera().ScreenSize.Width, 20);

            InitCastSpellButton();
        }

        [TempleDllLocation(0x101387c0)]
        private void InitCastSpellButton()
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x10137560)]
        public void Dispose()
        {
        }

        [TempleDllLocation(0x10137640)]
        public override void Reset()
        {
            base.Reset();
        }

        [TempleDllLocation(0x115B1E94)]
        private ResourceRef<IMdfRenderMaterial> _redLineMaterial;

        [TempleDllLocation(0x115B1E98)]
        private ResourceRef<IMdfRenderMaterial> _redLineOccludedMaterial;

        [TempleDllLocation(0x115B1E8C)]
        private ResourceRef<IMdfRenderMaterial> _greenLineMaterial;

        [TempleDllLocation(0x115B1E9C)]
        private ResourceRef<IMdfRenderMaterial> _greenLineOccludedMaterial;

        [TempleDllLocation(0x115B1E88)]
        private ResourceRef<IMdfRenderMaterial> _blueLineMaterial;

        [TempleDllLocation(0x115B1E90)]
        private ResourceRef<IMdfRenderMaterial> _blueLineOccludedMaterial;

        [TempleDllLocation(0x115B1E80)]
        private ResourceRef<IMdfRenderMaterial> _yellowLineMaterial;

        [TempleDllLocation(0x115B1E84)]
        private ResourceRef<IMdfRenderMaterial> _yellowLineOccludedMaterial;

        [TempleDllLocation(0x10BD2C50)]
        private ResourceRef<IMdfRenderMaterial> _spellPointerMaterial;

        [TempleDllLocation(0x10BD2C00)]
        private TigTextStyle _textStyle;

        [TempleDllLocation(0x10BD2CD8)]
        private int intgameselTexts;

        private WidgetContainer _window;

        [TempleDllLocation(0x101066d0)]
        private void LoadShaders()
        {
            _textStyle = new TigTextStyle();
            _textStyle.flags = 0;
            _textStyle.textColor = new ColorRect(PackedLinearColorA.White);
            _textStyle.shadowColor = new ColorRect(PackedLinearColorA.White);
            _textStyle.kerning = 2;
            _textStyle.leading = 0;

            _redLineMaterial = Tig.MdfFactory.LoadMaterial("art/interface/intgame_select/red-line.mdf");
            _redLineOccludedMaterial = Tig.MdfFactory.LoadMaterial("art/interface/intgame_select/red-line_oc.mdf");
            _greenLineMaterial = Tig.MdfFactory.LoadMaterial("art/interface/intgame_select/green-line.mdf");
            _greenLineOccludedMaterial = Tig.MdfFactory.LoadMaterial("art/interface/intgame_select/green-line_oc.mdf");
            _blueLineMaterial = Tig.MdfFactory.LoadMaterial("art/interface/intgame_select/blue-line.mdf");
            _blueLineOccludedMaterial = Tig.MdfFactory.LoadMaterial("art/interface/intgame_select/blue-line_oc.mdf");
            _yellowLineMaterial = Tig.MdfFactory.LoadMaterial("art/interface/intgame_select/yellow-line.mdf");
            _yellowLineOccludedMaterial =
                Tig.MdfFactory.LoadMaterial("art/interface/intgame_select/yellow-line_oc.mdf");
            _spellPointerMaterial =
                Tig.MdfFactory.LoadMaterial("art/interface/intgame_select/spell_player-pointer.mdf");

            intgameselTexts = 0;
        }
    }
}
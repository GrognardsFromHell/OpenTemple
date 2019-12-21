using System;
using System.Drawing;
using SpicyTemple.Core.GFX;
using SpicyTemple.Core.TigSubsystems;
using SpicyTemple.Core.Ui.Widgets;

namespace SpicyTemple.Core.Ui.CharSheet
{
    /**
     * Button for switching between skills, spells and feats.
     */
    public class CharUiTopButton : WidgetButton
    {
        // 0 is the default inventory
        private readonly int _sheetPage;
        private readonly ResourceRef<ITexture> _arcTopTexture;
        private readonly ResourceRef<ITexture> _normalTexture;
        private readonly ResourceRef<ITexture> _hoverTexture;
        private readonly ResourceRef<ITexture> _pressedTexture;
        private readonly ResourceRef<ITexture> _selectedTexture;

        public CharUiTopButton(CharUiParams uiParams, int sheetPage)
        {
            _sheetPage = sheetPage;

            ResourceRef<ITexture> ResolveTexture(CharUiTexture textureId)
            {
                return Tig.Textures.Resolve(uiParams.TexturePaths[textureId], false);
            }

            Rectangle rectangle;
            switch (sheetPage)
            {
                case 5:
                    rectangle = uiParams.CharUiSelectSkillsButton;
                    _normalTexture = ResolveTexture(CharUiTexture.ButtonSkillsUnselected);
                    _hoverTexture = ResolveTexture(CharUiTexture.ButtonSkillsHover);
                    _selectedTexture = ResolveTexture(CharUiTexture.ButtonSkillsSelected);
                    _pressedTexture = ResolveTexture(CharUiTexture.ButtonSkillsClick);
                    break;
                case 6:
                    rectangle = uiParams.CharUiSelectFeatsButton;
                    _normalTexture = ResolveTexture(CharUiTexture.ButtonFeatsUnselected);
                    _hoverTexture = ResolveTexture(CharUiTexture.ButtonFeatsHover);
                    _selectedTexture = ResolveTexture(CharUiTexture.ButtonFeatsSelected);
                    _pressedTexture = ResolveTexture(CharUiTexture.ButtonFeatsClick);
                    break;
                case 7:
                    rectangle = uiParams.CharUiSelectSpellsButton;
                    _normalTexture = ResolveTexture(CharUiTexture.ButtonSpellsUnselected);
                    _hoverTexture = ResolveTexture(CharUiTexture.ButtonSpellsHover);
                    _selectedTexture = ResolveTexture(CharUiTexture.ButtonSpellsSelected);
                    _pressedTexture = ResolveTexture(CharUiTexture.ButtonSpellsClick);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            SetPos(rectangle.Location);
            SetSize(rectangle.Size);

            _arcTopTexture = ResolveTexture(CharUiTexture.ButtonArcTopSelected);
        }

        [TempleDllLocation(0x10145560)]
        public override void Render()
        {
            var contentArea = GetContentArea();

            var destRect = GetContentArea();
            var srcRect = new Rectangle(0, 0, destRect.Width, destRect.Height);
            destRect.Y -= 2;

            ResourceRef<ITexture> buttonTexture = default;
            try
            {
                switch (ButtonState)
                {
                    case LgcyButtonState.Normal:
                        buttonTexture = _normalTexture;
                        break;
                    case LgcyButtonState.Disabled:
                        buttonTexture = _selectedTexture;
                        break;
                    case LgcyButtonState.Down:
                        destRect.Width += 3;
                        destRect.Height += 3;
                        srcRect.Width += 3;
                        srcRect.Height += 3;
                        buttonTexture = _pressedTexture;
                        break;
                    case LgcyButtonState.Hovered:
                        buttonTexture = _hoverTexture;
                        break;
                    default:
                        buttonTexture = _normalTexture;
                        break;
                }

                if (UiSystems.CharSheet.CurrentPage == _sheetPage)
                {
                    Render2dArgs args = default;
                    args.customTexture = _arcTopTexture.Resource;
                    args.flags = Render2dFlag.BUFFERTEXTURE;
                    args.srcRect = new Rectangle(0, 0, 21, 21);
                    args.destRect = new Rectangle(contentArea.X - 18, contentArea.Y + 5, 21, 21);
                    Tig.ShapeRenderer2d.DrawRectangle(ref args);

                    args.destRect.X = contentArea.X + contentArea.Width;
                    args.flags |= Render2dFlag.FLIPH;
                    Tig.ShapeRenderer2d.DrawRectangle(ref args);

                    if (ButtonState != LgcyButtonState.Down)
                    {
                        srcRect.Width += 3;
                        srcRect.Height += 3;
                        destRect.Width += 3;
                        destRect.Height += 3;
                        buttonTexture = _selectedTexture;
                    }
                }

                var buttonArgs = new Render2dArgs();
                buttonArgs.flags = Render2dFlag.BUFFERTEXTURE;
                buttonArgs.srcRect = srcRect;
                buttonArgs.destRect = destRect;
                buttonArgs.customTexture = buttonTexture.Resource;
                Tig.ShapeRenderer2d.DrawRectangle(ref buttonArgs);
            }
            finally
            {
                buttonTexture.Dispose();
            }
        }
    }
}
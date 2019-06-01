using System;
using System.Drawing;

namespace SpicyTemple.Core.Ui.WidgetDocs
{
    public class WidgetButton : WidgetButtonBase
    {
        private readonly WidgetText mLabel = new WidgetText();

        private WidgetImage mActivatedImage;
        private WidgetImage mDisabledImage;
        private WidgetImage mFrameImage;
        private WidgetImage mHoverImage;

        private WidgetImage mNormalImage;
        private WidgetImage mPressedImage;

        private WidgetButtonStyle mStyle;

        private readonly WidgetTooltipRenderer _tooltipRenderer = new WidgetTooltipRenderer();

        public WidgetButton()
        {
        }

        public WidgetButton(Rectangle rect) : base(rect)
        {
        }

        public TooltipStyle TooltipStyle
        {
            get => _tooltipRenderer.TooltipStyle;
            set => _tooltipRenderer.TooltipStyle = value;
        }

        public string TooltipText
        {
            get => _tooltipRenderer.TooltipText;
            set => _tooltipRenderer.TooltipText = value;
        }

        /*
         central style definitions:
         templeplus/button_styles.json
         */
        public void SetStyle(WidgetButtonStyle style)
        {
            mStyle = style;
            mButton.sndHoverOn = style.soundEnter;
            mButton.sndHoverOff = style.soundLeave;
            mButton.sndDown = style.soundDown;
            mButton.sndClick = style.soundClick;
            UpdateContent();
        }

        /*
         directly fetch style from Globals.WidgetButtonStyles
         */
        public void SetStyle(string styleName)
        {
            SetStyle(Globals.WidgetButtonStyles.GetStyle(styleName));
        }

        public WidgetButtonStyle GetStyle()
        {
            return mStyle;
        }

        public override void Render()
        {
            InvokeOnBeforeRender();

            var contentArea = GetContentArea();

            // Always fall back to the default
            var image = mNormalImage;

            if (mDisabled)
            {
                if (mDisabledImage != null)
                {
                    image = mDisabledImage;
                }
                else
                {
                    image = mNormalImage;
                }

                if (mStyle.disabledTextStyleId != null)
                {
                    mLabel.SetStyleId(mStyle.disabledTextStyleId);
                }
                else
                {
                    mLabel.SetStyleId(mStyle.textStyleId);
                }
            }
            else
            {
                if (mButton.buttonState == LgcyButtonState.Down)
                {
                    if (mPressedImage != null)
                    {
                        image = mPressedImage;
                    }
                    else if (mHoverImage != null)
                    {
                        image = mHoverImage;
                    }
                    else
                    {
                        image = mNormalImage;
                    }

                    if (mStyle.pressedTextStyleId != null)
                    {
                        mLabel.SetStyleId(mStyle.pressedTextStyleId);
                    }
                    else if (mStyle.hoverTextStyleId != null)
                    {
                        mLabel.SetStyleId(mStyle.hoverTextStyleId);
                    }
                    else
                    {
                        mLabel.SetStyleId(mStyle.textStyleId);
                    }
                }
                else if (IsActive())
                {
                    // Activated, else Pressed, else Hovered, (else Normal)
                    if (mActivatedImage != null)
                    {
                        image = mActivatedImage;
                    }
                    else if (mPressedImage != null)
                    {
                        image = mPressedImage;
                    }
                    else if (mHoverImage != null)
                    {
                        image = mHoverImage;
                    }


                    if (mButton.buttonState == LgcyButtonState.Hovered
                        || mButton.buttonState == LgcyButtonState.Released)
                    {
                        if (mStyle.hoverTextStyleId != null)
                        {
                            mLabel.SetStyleId(mStyle.hoverTextStyleId);
                        }
                        else
                        {
                            mLabel.SetStyleId(mStyle.textStyleId);
                        }
                    }
                    else
                    {
                        mLabel.SetStyleId(mStyle.textStyleId);
                    }
                }
                else if (mButton.buttonState == LgcyButtonState.Hovered
                         || mButton.buttonState == LgcyButtonState.Released)
                {
                    if (mHoverImage != null)
                    {
                        image = mHoverImage;
                    }
                    else
                    {
                        image = mNormalImage;
                    }

                    if (mStyle.hoverTextStyleId != null)
                    {
                        mLabel.SetStyleId(mStyle.hoverTextStyleId);
                    }
                    else
                    {
                        mLabel.SetStyleId(mStyle.textStyleId);
                    }
                }
                else
                {
                    image = mNormalImage;
                    mLabel.SetStyleId(mStyle.textStyleId);
                }
            }

            var fr = mFrameImage;
            if (fr != null)
            {
                var contentAreaWithMargins = GetContentArea(true);
                fr.SetContentArea(contentAreaWithMargins);
                fr.Render();
            }

            if (image != null)
            {
                image.SetContentArea(contentArea);
                image.Render();
            }

            mLabel.SetContentArea(contentArea);
            mLabel.Render();
        }

        public void SetText(string text)
        {
            mLabel.SetText(text);
            UpdateAutoSize();
        }

        /*
          1. updates the WidgetImage pointers below, using WidgetButtonStyle file paths
          2. Updates mLabel
         */
        private void UpdateContent()
        {
            if (mStyle.normalImagePath != null)
            {
                mNormalImage = new WidgetImage(mStyle.normalImagePath);
            }
            else
            {
                mNormalImage?.Dispose();
                mNormalImage = null;
            }

            if (mStyle.activatedImagePath != null)
            {
                mActivatedImage = new WidgetImage(mStyle.activatedImagePath);
            }
            else
            {
                mActivatedImage?.Dispose();
                mActivatedImage = null;
            }

            if (mStyle.hoverImagePath != null)
            {
                mHoverImage = new WidgetImage(mStyle.hoverImagePath);
            }
            else
            {
                mHoverImage?.Dispose();
                mHoverImage = null;
            }

            if (mStyle.pressedImagePath != null)
            {
                mPressedImage = new WidgetImage(mStyle.pressedImagePath);
            }
            else
            {
                mPressedImage?.Dispose();
                mPressedImage = null;
            }

            if (mStyle.disabledImagePath != null)
            {
                mDisabledImage = new WidgetImage(mStyle.disabledImagePath);
            }
            else
            {
                mDisabledImage?.Dispose();
                mDisabledImage = null;
            }

            if (mStyle.frameImagePath != null)
            {
                mFrameImage = new WidgetImage(mStyle.frameImagePath);
            }
            else
            {
                mFrameImage?.Dispose();
                mFrameImage = null;
            }

            mLabel.SetStyleId(mStyle.textStyleId);
            mLabel.SetCenterVertically(true);
            UpdateAutoSize();
        }

        private void UpdateAutoSize()
        {
            // Try to var-size
            if (mAutoSizeWidth || mAutoSizeHeight)
            {
                Size prefSize;
                if (mNormalImage != null)
                {
                    prefSize = mNormalImage.GetPreferredSize();
                }
                else
                {
                    prefSize = mLabel.GetPreferredSize();
                }

                if (mFrameImage != null)
                {
                    // update margins from frame size
                    var framePrefSize = mFrameImage.GetPreferredSize();
                    var marginW = framePrefSize.Width - prefSize.Width;
                    var marginH = framePrefSize.Height - prefSize.Height;
                    if (marginW > 0)
                    {
                        mMargins.Right = marginW / 2;
                        mMargins.Left = marginW - mMargins.Right;
                    }

                    if (marginH > 0)
                    {
                        mMargins.Bottom = marginH / 2;
                        mMargins.Top = marginH - mMargins.Bottom;
                    }
                }

                prefSize.Height += mMargins.Bottom + mMargins.Top;
                prefSize.Width += mMargins.Left + mMargins.Right;

                if (mAutoSizeWidth && mAutoSizeHeight)
                {
                    SetSize(prefSize);
                }
                else if (mAutoSizeWidth)
                {
                    SetWidth(prefSize.Width);
                }
                else if (mAutoSizeHeight)
                {
                    SetHeight(prefSize.Height);
                }
            }
        }

        public override void RenderTooltip(int x, int y)
        {
            _tooltipRenderer.Render(x, y);
        }
    }
}
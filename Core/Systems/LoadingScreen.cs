using System;
using System.Collections.Generic;
using System.Drawing;
using OpenTemple.Core.GFX;
using OpenTemple.Core.IO;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Systems
{
    public class LoadingScreen : IDisposable, ILoadingProgress
    {
        private const int BarBorderSize = 1;
        private const int BarHeight = 18;
        private const int BarWidth = 512;
        private readonly UiRectangle _barBorder;
        private readonly UiRectangle _barFilled;
        private readonly UiRectangle _barUnfilled;

        private readonly RenderingDevice _device;
        private WidgetImage _imageFile;
        public string Message { get; set; }
        private Dictionary<int, string> _messages;

        private bool _messagesLoaded;
        private float _progress;
        private int _resizeListener;

        public float Progress
        {
            get => _progress;
            set
            {
                _progress = value;
                UpdateFilledBarWidth();
            }
        }

        public LoadingScreen(RenderingDevice device, ShapeRenderer2d shapeRenderer2d)
        {
            _device = device;
            _barBorder = new UiRectangle(shapeRenderer2d);
            _barUnfilled = new UiRectangle(shapeRenderer2d);
            _barFilled = new UiRectangle(shapeRenderer2d);

            SetImage("art/splash/legal0322.img");

            _barBorder.SetColor(0xFF808080);
            _barUnfilled.SetColor(0xFF1C324E);
            _barFilled.SetColor(0xFF1AC3FF);

            _resizeListener = device.AddResizeListener((x, y) => Layout());
        }

        public void Dispose()
        {
            _imageFile?.Dispose();
            _imageFile = null;
            _device.RemoveResizeListener(_resizeListener);
        }

        public void SetMessageId(int messageId)
        {
            if (!_messagesLoaded)
            {
                _messages = Tig.FS.ReadMesFile("mes/loadscreen.mes");
                _messagesLoaded = true;
            }

            if (_messages.TryGetValue(messageId, out var message))
            {
                Message = message;
            }
            else
            {
                Message = $"Unknown Message ID: {messageId}";
            }
        }

        public void SetImage(string imagePath)
        {
            _imageFile?.Dispose();
            _imageFile = new WidgetImage(imagePath);
            Layout();
        }

        public void Update()
        {
            if (_imageFile == null)
            {
                return;
            }

            _device.BeginFrame();
            _imageFile.Render();
            _barBorder.Render();
            _barUnfilled.Render();
            _barFilled.Render();

            if (Message.Length > 0)
            {
                var style = new TigTextStyle(new ColorRect(PackedLinearColorA.White));
                style.kerning = 1;
                style.tracking = 3;

                var extents = new RectangleF();
                extents.X = _barBorder.X;
                extents.Y = _barBorder.Y + BarHeight + 5;

                Tig.Fonts.PushFont(PredefinedFont.ARIAL_10);

                Tig.Fonts.RenderText(Message, extents, style);

                Tig.Fonts.PopFont();
            }

            _device.Present();
        }

        private void Layout()
        {
            var screenSize = Tig.RenderingDevice.GetCamera().ScreenSize;
            var centerX = screenSize.Width / 2.0f;
            var centerY = screenSize.Height / 2.0f;

            var imageSize = _imageFile.GetPreferredSize();
            var imgX = (int) (centerX - imageSize.Width / 2.0f);
            var imgY = (int) (centerY - imageSize.Height / 2.0f);
            _imageFile.ContentArea = new RectangleF(new Point(imgX, imgY), imageSize);

            var barY = imgY + 20 + imageSize.Height;
            var barX = (int) (centerX - BarWidth / 2.0f);

            // Set up the border
            _barBorder.X = barX;
            _barBorder.Y = barY;
            _barBorder.Width = BarWidth;
            _barBorder.Height = BarHeight;

            // Set up the background
            _barUnfilled.X = barX + BarBorderSize;
            _barUnfilled.Y = barY + BarBorderSize;
            _barUnfilled.Width = BarWidth - BarBorderSize * 2;
            _barUnfilled.Height = BarHeight - BarBorderSize * 2;

            // Set up the filling (width remains unset)
            _barFilled.X = barX + BarBorderSize;
            _barFilled.Y = barY + BarBorderSize;
            _barFilled.Height = BarHeight - BarBorderSize * 2;
            UpdateFilledBarWidth();
        }

        private void UpdateFilledBarWidth()
        {
            var fullWidth = BarWidth - BarBorderSize * 2;
            _barFilled.Width = (int) (fullWidth * Math.Min(1.0f, _progress));
        }
    }


/*
	Draws a rectangle on screen.
*/
    internal class UiRectangle
    {
        private readonly PackedLinearColorA[] _colors = new PackedLinearColorA[4];
        private readonly ShapeRenderer2d _shapeRenderer2d;

        private Render2dArgs _args;

        public UiRectangle(ShapeRenderer2d shapeRenderer2d)
        {
            _shapeRenderer2d = shapeRenderer2d;
            _args.vertexColors = _colors;
            _args.flags = Render2dFlag.VERTEXCOLORS;
        }

        public float X
        {
            set
            {
                _args.srcRect.X = value;
                _args.destRect.X = value;
            }
            get => _args.srcRect.X;
        }

        public float Y
        {
            set
            {
                _args.srcRect.Y = value;
                _args.destRect.Y = value;
            }
            get => _args.srcRect.Y;
        }

        public float Width
        {
            set
            {
                _args.srcRect.Width = value;
                _args.destRect.Width = value;
            }
        }

        public float Height
        {
            set
            {
                _args.srcRect.Height = value;
                _args.destRect.Height = value;
            }
        }

        public void SetColor(uint color)
        {
            var packedColor = new PackedLinearColorA(color);
            Array.Fill(_colors, packedColor);

            if (packedColor.A != 255)
            {
                _args.flags |= Render2dFlag.VERTEXALPHA;
            }
            else
            {
                _args.flags &= ~Render2dFlag.VERTEXALPHA;
            }
        }

        public void Render()
        {
            _shapeRenderer2d.DrawRectangle(ref _args);
        }
    }
}
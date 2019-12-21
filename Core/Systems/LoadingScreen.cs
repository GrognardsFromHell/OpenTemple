using System;
using System.Collections.Generic;
using System.Drawing;
using SpicyTemple.Core.GFX;
using SpicyTemple.Core.IO;
using SpicyTemple.Core.TigSubsystems;
using SpicyTemple.Core.Ui.Widgets;

namespace SpicyTemple.Core.Systems
{
    public class LoadingScreen : IDisposable
    {
        private const int BarBorderSize = 1;
        private const int BarHeight = 18;
        private const int BarWidth = 512;
        private readonly UiRectangle _barBorder;
        private readonly UiRectangle _barFilled;
        private readonly UiRectangle _barUnfilled;

        private readonly RenderingDevice _device;
        private WidgetImage _imageFile;
        private string _message;
        private Dictionary<int, string> _messages;

        private bool _messagesLoaded;
        private float _progress;

        public LoadingScreen(RenderingDevice device, ShapeRenderer2d shapeRenderer2d)
        {
            _device = device;
            _barBorder = new UiRectangle(shapeRenderer2d);
            _barUnfilled = new UiRectangle(shapeRenderer2d);
            _barFilled = new UiRectangle(shapeRenderer2d);

            SetImage("art\\splash\\legal0322.img");

            _barBorder.SetColor(0xFF808080);
            _barUnfilled.SetColor(0xFF1C324E);
            _barFilled.SetColor(0xFF1AC3FF);
        }

        public void Dispose()
        {
            _imageFile?.Dispose();
            _imageFile = null;
        }

        public void SetProgress(float progress)
        {
            _progress = progress;

            UpdateFilledBarWidth();
        }

        public float GetProgress()
        {
            return _progress;
        }

        public void SetMessageId(int messageId)
        {
            if (!_messagesLoaded)
            {
                _messages = Tig.FS.ReadMesFile("mes/loadscreen.mes");
                _messagesLoaded = true;
            }

            if (!_messages.TryGetValue(messageId, out _message))
            {
                _message = $"Unknown Message ID: {messageId}";
            }
        }

        public void SetMessage(string message)
        {
            _message = message;
        }

        public void SetImage(string imagePath)
        {
            _imageFile?.Dispose();
            _imageFile = new WidgetImage(imagePath);
            Layout();
        }

        public void Render()
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

            if (_message.Length > 0)
            {
                var style = new TigTextStyle(new ColorRect(PackedLinearColorA.White));
                style.kerning = 1;
                style.tracking = 3;

                var extents = new Rectangle();
                extents.X = _barBorder.GetX();
                extents.Y = _barBorder.GetY() + BarHeight + 5;

                Tig.Fonts.PushFont(PredefinedFont.ARIAL_10);

                Tig.Fonts.RenderText(_message, extents, style);

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
            _imageFile.SetContentArea(new Rectangle(new Point(imgX, imgY), imageSize));

            var barY = imgY + 20 + imageSize.Height;
            var barX = (int) (centerX - BarWidth / 2.0f);

            // Set up the border
            _barBorder.SetX(barX);
            _barBorder.SetY(barY);
            _barBorder.SetWidth(BarWidth);
            _barBorder.SetHeight(BarHeight);

            // Set up the background
            _barUnfilled.SetX(barX + BarBorderSize);
            _barUnfilled.SetY(barY + BarBorderSize);
            _barUnfilled.SetWidth(BarWidth - BarBorderSize * 2);
            _barUnfilled.SetHeight(BarHeight - BarBorderSize * 2);

            // Set up the filling (width remains unset)
            _barFilled.SetX(barX + BarBorderSize);
            _barFilled.SetY(barY + BarBorderSize);
            _barFilled.SetHeight(BarHeight - BarBorderSize * 2);
            UpdateFilledBarWidth();
        }

        private void UpdateFilledBarWidth()
        {
            var fullWidth = BarWidth - BarBorderSize * 2;
            _barFilled.SetWidth((int) (fullWidth * Math.Min(1.0f, _progress)));
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

        public void SetX(int x)
        {
            _args.srcRect.X = x;
            _args.destRect.X = x;
        }

        public void SetY(int y)
        {
            _args.srcRect.Y = y;
            _args.destRect.Y = y;
        }

        public int GetX()
        {
            return _args.srcRect.X;
        }

        public int GetY()
        {
            return _args.srcRect.Y;
        }

        public void SetWidth(int width)
        {
            _args.srcRect.Width = width;
            _args.destRect.Width = width;
        }

        public void SetHeight(int height)
        {
            _args.srcRect.Height = height;
            _args.destRect.Height = height;
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
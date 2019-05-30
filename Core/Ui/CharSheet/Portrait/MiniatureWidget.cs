using System;
using System.Drawing;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Platform;
using SpicyTemple.Core.Systems;
using SpicyTemple.Core.Ui.WidgetDocs;
using SpicyTemple.Core.Utils;

namespace SpicyTemple.Core.Ui.CharSheet.Portrait
{
    public class MiniatureWidget : WidgetButtonBase
    {
        public GameObjectBody Object { get; set; }

        private int _rotationPivot;

        private int _rotationMode = 0;

        private float _rotation;

        public MiniatureWidget()
        {
            SetMouseMsgHandler(OnMouseMove);
        }

        // TODO: This should be injected in some other way
        private MapObjectRenderer Renderer => Globals.GameLoop.GameRenderer.GetMapObjectRenderer();

        public override void Render()
        {
            base.Render();

            if (Object == null)
            {
                return;
            }

            var contentArea = GetContentArea();
            var centerX = contentArea.X + contentArea.Width / 2;
            var centerY = contentArea.Y + contentArea.Height / 2;

            Renderer.RenderObjectInUi(Object, centerX, centerY + 35, _rotation, 1.5f);
        }

        public override bool HandleMessage(Message msg)
        {
            if (msg.type == MessageType.WIDGET)
            {
                switch (msg.WidgetArgs.widgetEventType)
                {
                    case TigMsgWidgetEvent.Clicked:
                        _rotationMode = 1;
                        break;
                    case TigMsgWidgetEvent.MouseReleased:
                    case TigMsgWidgetEvent.MouseReleasedAtDifferentButton:
                        _rotationMode = 0;
                        break;
                }
            }

            return base.HandleMessage(msg);
        }

        private bool OnMouseMove(MessageMouseArgs arg)
        {
            if (_rotationMode == 1)
            {
                _rotationMode = 2;
                _rotationPivot = arg.X;
                return true;
            }
            else if (_rotationMode == 2)
            {
                int deltaX = arg.X - _rotationPivot;
                _rotation -= deltaX * Angles.ToRadians(1.8f);
                _rotationPivot = arg.X;
                return true;
            }

            return false;
        }
    }
}
using SpicyTemple.Core.Platform;
using SpicyTemple.Core.Systems;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.TigSubsystems;

namespace SpicyTemple.Core.Ui.RadialMenu
{
    public class RadialMenuUi
    {
        [TempleDllLocation(0x10BE6D9C)]
        private int dword_10BE6D9C;

        [TempleDllLocation(0x10BE6D70)]
        public bool dword_10BE6D70;

        [TempleDllLocation(0x1013dc90)]
        public bool HandleMessage(Message message)
        {
            var shiftPressed = Tig.Keyboard.IsKeyPressed(VirtualKey.VK_LSHIFT)
                               || Tig.Keyboard.IsKeyPressed(VirtualKey.VK_RSHIFT);
            GameSystems.D20.RadialMenu.ShiftPressed = shiftPressed;

            if (GameSystems.D20.RadialMenu.GetCurrentNode() == -1)
            {
                return false;
            }


            if (message.type == MessageType.KEYSTATECHANGE)
            {
                return HandleKeyMessage(message.KeyStateChangeArgs);
            }
            else if (message.type == MessageType.MOUSE)
            {
                var args = message.MouseArgs;

                if (!args.flags.HasFlag(MouseEventFlag.LeftReleased))
                {
                    if (args.flags.HasFlag(MouseEventFlag.RightReleased))
                    {
                        //return RmbReleasedHandler(msg);
                        Stub.TODO();
                    }
                }

                Stub.TODO();
                return false;
            }
            else
            {
                return false;
            }
        }

        [TempleDllLocation(0x10139e50)]
        public bool IsOpen
        {
            get
            {
                Stub.TODO();
                return false;
            }
        }

        [TempleDllLocation(0x1013c9c0)]
        public bool HandleKeyMessage(MessageKeyStateChangeArgs args)
        {
            Stub.TODO();
            return false;
        }

        [TempleDllLocation(0x1013b250)]
        public void Spawn(int screenX, int screenY)
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x10139e60)]
        public CursorType? GetCursor()
        {
            if (dword_10BE6D9C != 0)
            {
                return CursorType.HotKeySelection;
            }
            else
            {
                return null;
            }
        }

        [TempleDllLocation(0x1013dd10)]
        public void HandleRightMouseClick(int x, int y)
        {
            throw new System.NotImplementedException();
        }
    }
}
using SpicyTemple.Core.Platform;
using SpicyTemple.Core.Systems;
using SpicyTemple.Core.TigSubsystems;

namespace SpicyTemple.Core.Ui.RadialMenu
{
    public class RadialMenuUi
    {
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
    }
}
using OpenTemple.Core.GFX;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.Script.Extensions;

namespace OpenTemple.Core.GameObjects
{
    public static class PortalExtensions
    {
        [TempleDllLocation(0x100b46b0)]
        public static bool IsPortalOpen(this GameObject obj)
        {
            var sdFlags = obj.GetSecretDoorFlags();
            if (sdFlags.HasFlag(SecretDoorFlag.SECRET_DOOR) && !sdFlags.HasFlag(SecretDoorFlag.SECRET_DOOR_FOUND))
            {
                return false;
            }

            var portalFlags = obj.GetPortalFlags();
            return portalFlags.HasFlag(PortalFlag.OPEN);
        }


        [TempleDllLocation(0x100b4700)]
        public static void TogglePortalOpen(this GameObject obj)
        {
            var sdFlags = obj.GetSecretDoorFlags();
            if (sdFlags.HasFlag(SecretDoorFlag.SECRET_DOOR) && !sdFlags.HasFlag(SecretDoorFlag.SECRET_DOOR_FOUND))
            {
                return;
            }

            var portalFlags = obj.GetPortalFlags() ^ PortalFlag.OPEN;
            obj.SetPortalFlags(portalFlags);
            if ((portalFlags & PortalFlag.OPEN) != 0)
            {
                GameSystems.Anim.PushAnimate(obj, NormalAnimType.Open);
            }
            else
            {
                GameSystems.Anim.PushAnimate(obj, NormalAnimType.Close);
            }
        }

        [TempleDllLocation(0x1001fd70)]
        public static bool NeedsToBeUnlocked(this GameObject obj)
        {
            if (obj.ProtoId == 1000)
            {
                return false;
            }

            if (obj.type == ObjectType.container)
            {

                var containerFlags = obj.GetContainerFlags();
                if (containerFlags.HasFlag(ContainerFlag.BUSTED))
                {
                    return false;
                }

                if (containerFlags.HasFlag(ContainerFlag.NEVER_LOCKED))
                {
                    return false;
                }

                if (containerFlags.HasFlag(ContainerFlag.LOCKED_DAY) && GameSystems.TimeEvent.IsDaytime)
                {
                    return false;
                }

                if (containerFlags.HasFlag(ContainerFlag.LOCKED_NIGHT) && GameSystems.TimeEvent.IsDaytime)
                {
                    return false;
                }

                return containerFlags.HasFlag(ContainerFlag.LOCKED);

            }
            else if (obj.type == ObjectType.portal)
            {
                var secretDoorFlags = obj.GetSecretDoorFlags();
                if (secretDoorFlags.HasFlag(SecretDoorFlag.SECRET_DOOR)
                    && !secretDoorFlags.HasFlag(SecretDoorFlag.SECRET_DOOR_FOUND))
                {
                    return true;
                }

                var portalFlags = obj.GetPortalFlags();
                if (portalFlags.HasFlag(PortalFlag.BUSTED))
                {
                    return false;
                }

                if (portalFlags.HasFlag(PortalFlag.NEVER_LOCKED))
                {
                    return false;
                }

                if (portalFlags.HasFlag(PortalFlag.LOCKED_DAY) && !GameSystems.TimeEvent.IsDaytime)
                {
                    return false;
                }

                if (portalFlags.HasFlag(PortalFlag.LOCKED_NIGHT) && GameSystems.TimeEvent.IsDaytime)
                {
                    return false;
                }

                return portalFlags.HasFlag(PortalFlag.LOCKED);
            }

            return false;

        }
    }
}
namespace SpicyTemple.Core.GameObject
{
    public static class PortalExtensions
    {
        [TempleDllLocation(0x100b46b0)]
        public static bool IsPortalOpen(this GameObjectBody obj)
        {
            var sdFlags = obj.GetSecretDoorFlags();
            if (sdFlags.HasFlag(SecretDoorFlag.SECRET_DOOR) && !sdFlags.HasFlag(SecretDoorFlag.SECRET_DOOR_FOUND))
            {
                return false;
            }

            var portalFlags = obj.GetPortalFlags();
            return portalFlags.HasFlag(PortalFlag.OPEN);
        }
    }
}
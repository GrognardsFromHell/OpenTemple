using SpicyTemple.Core.AAS;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.GFX;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Systems.D20;

namespace SpicyTemple.Core.Systems.GameObjects
{
    public static class ObjectHandles
    {
        private static GameObjectBody GetObject(ObjHndl handle) => GameSystems.Object.GetObject(handle);

        public static ObjectType GetType(ObjHndl handle) => GetObject(handle).type;

        public static ItemFlag GetItemFlags(ObjHndl handle) => GetObject(handle).GetItemFlags();

        public static SecretDoorFlag GetSecretDoorFlags(ObjHndl handle) => GetObject(handle).GetSecretDoorFlags();

        private static bool IsCritter(ObjHndl handle) => GetType(handle).IsCritter();

        private static bool IsEquipment(ObjHndl handle) => GetType(handle).IsEquipment();

        public static bool IsDoorOpen(GameObjectBody obj)
        {
            // Undetected secret doors are never open
            var secretDoorFlags = obj.GetSecretDoorFlags();
            if (secretDoorFlags.HasFlag(SecretDoorFlag.SECRET_DOOR)
                && !secretDoorFlags.HasFlag(SecretDoorFlag.SECRET_DOOR_FOUND)) {
                return false;
            }

            return obj.GetPortalFlags().HasFlag(PortalFlag.OPEN);
        }

        public static int GetItemInventoryLocation(GameObjectBody obj) =>
            obj.GetInt32(obj_f.item_inv_location);

    }
}
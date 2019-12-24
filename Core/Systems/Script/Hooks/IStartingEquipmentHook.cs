using OpenTemple.Core.GameObject;

namespace OpenTemple.Core.Systems.Script.Hooks
{
    [HookInterface]
    public interface IStartingEquipmentHook
    {
        void GiveStartingEquipment(GameObjectBody pc);
    }
}
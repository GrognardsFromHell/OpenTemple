using SpicyTemple.Core.GameObject;

namespace SpicyTemple.Core.Systems.Script.Hooks
{
    [HookInterface]
    public interface IStartingEquipmentHook
    {
        void GiveStartingEquipment(GameObjectBody pc);
    }
}
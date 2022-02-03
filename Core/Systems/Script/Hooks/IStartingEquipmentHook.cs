using OpenTemple.Core.GameObjects;

namespace OpenTemple.Core.Systems.Script.Hooks
{
    [HookInterface]
    public interface IStartingEquipmentHook
    {
        void GiveStartingEquipment(GameObject pc);
    }
}
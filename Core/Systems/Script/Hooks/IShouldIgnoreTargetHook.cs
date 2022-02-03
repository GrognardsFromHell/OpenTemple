using OpenTemple.Core.GameObjects;

namespace OpenTemple.Core.Systems.Script.Hooks
{
    [HookInterface]
    public interface IShouldIgnoreTargetHook
    {
        bool ShouldIgnoreTarget(GameObject npc, GameObject target);
    }

    public static class ShouldIgnoreTargetExtension
    {

        public static bool ShouldIgnoreTarget(this GameObject npc, GameObject critter)
        {
            var hook = GameSystems.Script.GetHook<IShouldIgnoreTargetHook>();
            return hook != null && hook.ShouldIgnoreTarget(npc, critter);
        }

    }

}
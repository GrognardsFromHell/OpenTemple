using OpenTemple.Core.GameObject;

namespace OpenTemple.Core.Systems.Script.Hooks
{
    [HookInterface]
    public interface IShouldIgnoreTargetHook
    {
        bool ShouldIgnoreTarget(GameObjectBody npc, GameObjectBody target);
    }

    public static class ShouldIgnoreTargetExtension
    {

        public static bool ShouldIgnoreTarget(this GameObjectBody npc, GameObjectBody critter)
        {
            var hook = GameSystems.Script.GetHook<IShouldIgnoreTargetHook>();
            return hook != null && hook.ShouldIgnoreTarget(npc, critter);
        }

    }

}
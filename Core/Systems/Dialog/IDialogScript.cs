using SpicyTemple.Core.GameObject;

namespace SpicyTemple.Core.Systems.Dialog
{
    public interface IDialogScript
    {
        bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript);

        void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript);

        bool TryGetSkillChecks(int lineNumber, out DialogSkillChecks skillChecks);
    }
}
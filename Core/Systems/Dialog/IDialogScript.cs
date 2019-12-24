using OpenTemple.Core.GameObject;

namespace OpenTemple.Core.Systems.Dialog
{
    public interface IDialogScript
    {
        bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript);

        void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript);

        bool TryGetSkillChecks(int lineNumber, out DialogSkillChecks skillChecks);
    }
}
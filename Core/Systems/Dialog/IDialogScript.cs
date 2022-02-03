using OpenTemple.Core.GameObjects;

namespace OpenTemple.Core.Systems.Dialog
{
    public interface IDialogScript
    {
        bool CheckPrecondition(GameObject npc, GameObject pc, int lineNumber, out string originalScript);

        void ApplySideEffect(GameObject npc, GameObject pc, int lineNumber, out string originalScript);

        bool TryGetSkillChecks(int lineNumber, out DialogSkillChecks skillChecks);
    }
}
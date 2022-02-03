using OpenTemple.Core.GameObjects;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Systems.D20.Actions;
using OpenTemple.Core.Ui.InGameSelect;

namespace OpenTemple.Core.Systems.Script
{
    public class ActionScriptSystem
    {
        public bool PyProjectileHit(D20DispatcherKey actionData1, D20Action action, GameObject projectile, GameObject obj2)
        {
            throw new System.NotImplementedException();
        }

        public ActionErrorCode PyAddToSeq(D20DispatcherKey actionData1, D20Action action, ActionSequence actSeq, TurnBasedStatus tbStatus)
        {
            throw new System.NotImplementedException();
        }

        public ActionErrorCode GetPyActionCost(D20Action action, TurnBasedStatus tbStatus, ActionCostPacket acp)
        {
            throw new System.NotImplementedException();
        }

        public void ModifyPicker(int data1, PickerArgs pickerArgs)
        {
            throw new System.NotImplementedException();
        }
    }
}
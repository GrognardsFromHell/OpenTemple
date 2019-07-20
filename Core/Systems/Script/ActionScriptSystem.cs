using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.Systems.D20.Actions;

namespace SpicyTemple.Core.Systems.Script
{
    public class ActionScriptSystem
    {
        public bool PyProjectileHit(D20DispatcherKey actionData1, D20Action action, GameObjectBody projectile, GameObjectBody obj2)
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
    }
}
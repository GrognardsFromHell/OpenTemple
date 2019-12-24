namespace OpenTemple.Core.Systems.D20.Actions
{
    public enum ActionCostType
    {
        Null = 0,
        Move = 1,
        Standard = 2,
        PartialCharge = 3,
        FullRound = 4
    };

    public class ActionCostPacket
    {
        public ActionCostType hourglassCost;
        public int chargeAfterPicker; // flag I think; is only set at stuff that requires using the picker it seems
        public float moveDistCost;

        public ActionCostPacket Copy()
        {
            return (ActionCostPacket) MemberwiseClone();
        }
    }
}
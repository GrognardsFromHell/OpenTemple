namespace SpicyTemple.Core.Systems.D20.Actions
{
    public enum ActionCostType
    {
        Null = 0,
        Move,
        Standard,
        PartialCharge,
        FullRound
    };

    public class ActionCostPacket
    {
        public int hourglassCost;
        public int chargeAfterPicker; // flag I think; is only set at stuff that requires using the picker it seems
        public float moveDistCost;

        public ActionCostPacket Copy()
        {
            return (ActionCostPacket) MemberwiseClone();
        }
    }
}
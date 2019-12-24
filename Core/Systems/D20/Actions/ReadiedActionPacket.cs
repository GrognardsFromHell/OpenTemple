using OpenTemple.Core.GameObject;

namespace OpenTemple.Core.Systems.D20.Actions
{
    public class ReadiedActionPacket
    {
        public int flags;
        public GameObjectBody interrupter;
        public ReadyVsTypeEnum readyType;
    }

    public enum ReadyVsTypeEnum
    {
        RV_Spell = 0,
        RV_Counterspell = 1,
        RV_Approach = 2,
        RV_Withdrawal = 3
    }
}
namespace OpenTemple.Core.Systems.D20.Actions
{
    public class PythonActionSpec {
        public D20ADF flags;
        public D20TargetClassification tgtClass;
        public string name;
        public ActionCostType costType;
        public AddToSeqCallback OnAddToSequence;
    }
}
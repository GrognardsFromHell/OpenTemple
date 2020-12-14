namespace OpenTemple.Core.Ui.DOM
{
    public class AbstractRange
    {
        protected BoundaryPoint _start;
        protected BoundaryPoint _end;

        public AbstractRange(BoundaryPoint start, BoundaryPoint end)
        {
            _start = start;
            _end = end;
        }

        public BoundaryPoint Start => _start;

        public BoundaryPoint End => _end;

        // https://dom.spec.whatwg.org/#dom-range-startcontainer
        public Node StartContainer => _start.Node;

        // https://dom.spec.whatwg.org/#dom-range-startoffset
        public int StartOffset => _start.Offset;

        // https://dom.spec.whatwg.org/#dom-range-endcontainer
        public Node EndContainer => _end.Node;

        // https://dom.spec.whatwg.org/#dom-range-endoffset
        public int EndOffset => _end.Offset;

        // https://dom.spec.whatwg.org/#dom-range-collapsed
        public bool IsCollapsed => _start.Node == _end.Node && _start.Offset == _end.Offset;

        public void Deconstruct(out BoundaryPoint start, out BoundaryPoint end)
        {
            start = _start;
            end = _end;
        }
    };
}
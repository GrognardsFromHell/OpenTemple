using System;
using System.Collections.Generic;

namespace OpenTemple.Core.Ui.DOM
{
    public class HTMLCollection
    {
        private readonly ParentNode _element;

        private readonly Func<List<Node>> _supplier;

        public HTMLCollection(ParentNode element, Func<List<Node>> supplier)
        {
            this._element = element;
            this._supplier = supplier;
        }

        public int Length { get; } // TODO

        internal void _update()
        {
            // TODO
        }
    }
}
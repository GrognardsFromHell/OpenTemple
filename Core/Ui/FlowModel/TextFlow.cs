using System.Collections.Generic;

namespace OpenTemple.Core.Ui.FlowModel
{
    public class TextFlow
    {
        public string Text { get; }
        public IReadOnlyList<Element> Elements { get; }

        public TextFlow(string text, IReadOnlyList<Element> elements)
        {
            Text = text;
            Elements = elements;
        }

        public readonly struct Element
        {
            public readonly int Start;
            public readonly int Length;
            public readonly SimpleInlineElement Source;

            public Element(int start, int length, SimpleInlineElement source)
            {
                Start = start;
                Length = length;
                Source = source;
            }
        }
    }
}
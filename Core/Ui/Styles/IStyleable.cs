
#nullable enable
namespace OpenTemple.Core.Ui.Styles
{
    public interface IStyleable
    {
        /// <summary>
        /// Gets the computed styles of this styleable element.
        /// </summary>
        public ComputedStyles ComputedStyles { get; }

        /// <summary>
        /// Indicates whether this styleable has local style rules. If not, this can be used to
        /// optimize child element's style computations by reusing this element's computed styles
        /// (since they only contain inheritable or default properties).
        /// </summary>
        public bool HasLocalStyles { get; }
    }
}
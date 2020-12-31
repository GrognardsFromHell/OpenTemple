using Avalonia.Controls;
using Vortice.Direct3D11;

namespace OpenTemple.Core.Ui
{
    public static class PseudoClassesExtensions
    {
        public static void Toggle(this IPseudoClasses pseudoClasses, string className, bool enable)
        {
            if (enable)
            {
                pseudoClasses.Add(className);
            }
            else
            {
                pseudoClasses.Remove(className);
            }
        }
    }
}

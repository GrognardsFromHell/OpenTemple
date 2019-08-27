using System.Collections.Generic;
using System.Drawing;
using SpicyTemple.Core.GFX;

namespace SpicyTemple.Core.Ui
{
    /// <summary>
    /// Helpers for loading UI-related parameters from MES files.
    /// </summary>
    public static class UiParamExtensions
    {
        public static Rectangle GetRectangleParam(this Dictionary<int, string> uiParams, int baseId) =>
            new Rectangle(
                int.Parse(uiParams[baseId]),
                int.Parse(uiParams[baseId + 1]),
                int.Parse(uiParams[baseId + 2]),
                int.Parse(uiParams[baseId + 3])
            );

        public static PackedLinearColorA GetColorParam(this Dictionary<int, string> uiParams, int baseId) =>
            new PackedLinearColorA(
                byte.Parse(uiParams[baseId]),
                byte.Parse(uiParams[baseId + 1]),
                byte.Parse(uiParams[baseId + 2]),
                byte.Parse(uiParams[baseId + 3])
            );
    }
}
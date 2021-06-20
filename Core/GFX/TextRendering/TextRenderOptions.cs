using System;
using System.Diagnostics.Contracts;
using System.Numerics;

namespace OpenTemple.Core.GFX.TextRendering
{
    /// <summary>
    /// Additional rendering options for rendering text that does not affect layout.
    /// </summary>
    public readonly struct TextRenderOptions
    {
        public readonly TextRenderFlags Flags;
        public readonly float Opacity;

        /// <summary>
        /// The rotation angle in radians.
        /// </summary>
        public readonly float RotationAngle;

        public readonly Vector2 RotationCenter;

        public static readonly TextRenderOptions Default = default;

        [Pure]
        public bool HasRotation => (Flags & TextRenderFlags.ApplyRotation) != 0;

        [Pure]
        public bool HasOpacity => (Flags & TextRenderFlags.ApplyOpacity) != 0;

        public TextRenderOptions(TextRenderFlags flags, float opacity, float rotationAngle, Vector2 rotationCenter)
        {
            Flags = flags;
            Opacity = opacity;
            RotationAngle = rotationAngle;
            RotationCenter = rotationCenter;
        }

        [Pure]
        public TextRenderOptions WithRotation(float angle, Vector2 center = default)
        {
            return new(
                Flags | TextRenderFlags.ApplyRotation,
                Opacity,
                angle,
                center
            );
        }

        [Pure]
        public TextRenderOptions WithOpacity(float opacity)
        {
            return new(
                Flags | TextRenderFlags.ApplyOpacity,
                Math.Clamp(opacity, 0f, 1f),
                RotationAngle,
                RotationCenter
            );
        }
    }

    [Flags]
    public enum TextRenderFlags
    {
        ApplyOpacity = 1,
        ApplyRotation = 2
    }
}
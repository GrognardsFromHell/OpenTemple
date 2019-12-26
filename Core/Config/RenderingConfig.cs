using System.Drawing;

namespace OpenTemple.Core.Config
{
    public class RenderingConfig
    {
        /// <summary>
        /// Enables Multisample Anti-Aliasing.
        /// </summary>
        public bool IsAntiAliasing { get; set; }

        /// <summary>
        /// The number of MSAA samples.
        /// </summary>
        public int MSAASamples { get; set; } = 4;

        /// <summary>
        /// The MSAA quality setting, which is only used by
        /// vendor specific MSAA extensions.
        /// </summary>
        public int MSAAQuality { get; set; } = 0;

        public bool IsUpscaleLinearFiltering { get; set; } = true;

        /// <summary>
        /// Enables the Direct3D 11.1 debug layer.
        /// </summary>
        public bool DebugDevice { get; set; }

        /// <summary>
        /// Index of the DXGI adapter to use for rendering.
        /// Defaults to 0, which usually is the primary adapter.
        /// </summary>
        public int AdapterIndex { get; set; }

        public RenderingConfig Copy() => (RenderingConfig) MemberwiseClone();

    }

}
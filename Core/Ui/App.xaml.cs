using Avalonia;
using Avalonia.Markup.Xaml;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Angle;
using Avalonia.OpenGL.Egl;
using Avalonia.Platform;
using OpenTemple.Core.TigSubsystems;
using SharpDX;
using SharpDX.Direct3D11;

namespace OpenTemple.Core.Ui
{
    public class App : Application
    {
        public Device Direct3D11Device { get; private set; }

#if CUSTOM_ANGLE
        class CustomEglInterface : EglInterface
        {
            [DllImport("libGLESv2.dll", CharSet = CharSet.Ansi)]
            static extern IntPtr EGL_GetProcAddress(string proc);

            public CustomEglInterface() : base(LoadAngle())
            {
            }

            static Func<string, IntPtr> LoadAngle()
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    var disp = EGL_GetProcAddress("eglGetPlatformDisplayEXT");

                    if (disp == IntPtr.Zero)
                    {
                        throw new OpenGlException("libegl.dll doesn't have eglGetPlatformDisplayEXT entry point");
                    }

                    return EGL_GetProcAddress;
                }

                throw new PlatformNotSupportedException();
            }
        }
#endif

        public override void Initialize()
        {
            AvaloniaLocator.CurrentMutable.Bind<IPlatformRenderInterface>().ToConstant(
                new DelegatingRenderPlatform(AvaloniaLocator.Current.GetService<IPlatformRenderInterface>())
            );

            VfsAssetLoader.Install();

            var display = new AngleWin32EglDisplay();
            var egl = new EglPlatformOpenGlInterface(display);
            AvaloniaLocator.CurrentMutable.Bind<IPlatformOpenGlInterface>().ToConstant(egl);

            var devicePtr = display.GetDirect3DDevice();
            Direct3D11Device = new Device(devicePtr);
            ((IUnknown) Direct3D11Device).AddReference(); // ANGLE returns a pointer without AddRef()'ing it

            AvaloniaXamlLoader.Load(this);
        }
    }
}
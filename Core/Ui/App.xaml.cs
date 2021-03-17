using System;
using System.Reactive.Subjects;
using Avalonia;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Widgets;

namespace OpenTemple.Core.Ui
{
    public class App : Application
    {
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
            AvaloniaLocator.CurrentMutable.Bind<IConfigService>().ToConstant(
                new ConfigServiceAdapter()
            );

            AvaloniaLocator.CurrentMutable.Bind<IPlatformRenderInterface>().ToConstant(
                new DelegatingRenderPlatform(AvaloniaLocator.Current.GetService<IPlatformRenderInterface>())
            );

            VfsAssetLoader.Install();

            AvaloniaXamlLoader.Load(this);
        }
    }

    public class ConfigServiceAdapter : IConfigService
    {
        public IObservable<object> ObserveConfigProperty(string name)
        {
            var prop = Globals.Config.GetType().GetProperty(name);
            if (prop == null)
            {
                throw new ArgumentException("Config property not found: " + name);
            }

            var subj = new BehaviorSubject<object>(prop.GetValue(Globals.Config));
            Globals.ConfigManager.OnConfigChanged += () => subj.OnNext(prop.GetValue(Globals.Config));
            return subj;
        }
    }
}

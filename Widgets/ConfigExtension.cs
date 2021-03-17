using System;
using Avalonia;
using Avalonia.Markup.Xaml;
using JetBrains.Annotations;

namespace OpenTemple.Widgets
{
    public interface IConfigService
    {
        IObservable<object> ObserveConfigProperty(string name);
    }

    [UsedImplicitly]
    public class ConfigBinding : MarkupExtension
    {
        private readonly string _configPropName;

        public ConfigBinding(string configPropName)
        {
            _configPropName = configPropName;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var configService = AvaloniaLocator.Current.GetService<IConfigService>();
            if (configService == null)
            {
                return null;
            }

            return configService.ObserveConfigProperty(_configPropName).ToBinding();
        }
    }
}

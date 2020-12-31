using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;
using JetBrains.Annotations;

namespace OpenTemple.Widgets
{
    public static class TranslationService
    {
        public static Func<string, string> Translator { get; set; } = str => str;
    }

    [UsedImplicitly]
    public class T : MarkupExtension
    {
        private readonly Binding _textBinding;

        private readonly string _text;

        public T(Binding textBinding)
        {
            _textBinding = textBinding;
        }

        public T(string text)
        {
            _text = text;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (_textBinding != null)
            {
                _textBinding.Converter = TranslationProxy.Instance;
                return _textBinding;
            }
            else
            {
                return TranslationService.Translator(_text);
            }
        }
    }

    public class TranslationProxy : IValueConverter
    {
        public static readonly TranslationProxy Instance = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }
            return TranslationService.Translator(value.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

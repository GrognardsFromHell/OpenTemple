using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using OpenTemple.Core.GFX;
using OpenTemple.Core.TigSubsystems;

namespace OpenTemple.Core.Ui.MainMenu
{
    public class MainMenuButton : Avalonia.Controls.Button
    {
        public static TigTextStyle TextStyle { get; }

        public static TigTextStyle HoverTextStyle { get; }

        public static TigTextStyle PressedTextStyle { get; }

        static MainMenuButton()
        {
            TextStyle = new()
            {
                textColor = new ColorRect()
                {
                    topLeft = new PackedLinearColorA(0xFF0064a4),
                    topRight = new PackedLinearColorA(0xFF0064a4),
                    bottomLeft = new PackedLinearColorA(0xFF01415d),
                    bottomRight = new PackedLinearColorA(0xFF01415d),
                },
                shadowColor = new ColorRect(PackedLinearColorA.Black),
                flags = TigTextStyleFlag.TTSF_DROP_SHADOW,
                leading = 1,
                tracking = 10
            };

            HoverTextStyle = TextStyle.Copy();
            HoverTextStyle.textColor = new ColorRect()
            {
                topLeft = new PackedLinearColorA(0xFF01ffff),
                topRight = new PackedLinearColorA(0xFF01ffff),
                bottomLeft = new PackedLinearColorA(0xFF01d0ff),
                bottomRight = new PackedLinearColorA(0xFF01d0ff),
            };

            PressedTextStyle = TextStyle.Copy();
            PressedTextStyle.textColor = new ColorRect()
            {
                topLeft = new PackedLinearColorA(0xFFeb1510),
                topRight = new PackedLinearColorA(0xFFeb1510),
                bottomLeft = new PackedLinearColorA(0xFFda5b61),
                bottomRight = new PackedLinearColorA(0xFFda5b61),
            };
        }

        public static readonly StyledProperty<string> TextProperty =
            AvaloniaProperty.Register<MainMenuButton, string>(nameof(Text));

        public string Text
        {
            get => GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public MainMenuButton()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
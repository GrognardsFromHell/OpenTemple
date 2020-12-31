using Avalonia;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;

namespace OpenTemple.Widgets
{
    public class Button : Avalonia.Controls.Button
    {
        public static readonly AvaloniaProperty CurrentImageProperty = AvaloniaProperty.Register<Button, IBitmap>(
            nameof(CurrentImage));

        public IBitmap CurrentImage
        {
            get => (IBitmap) GetValue(CurrentImageProperty);
            set => SetValue(CurrentImageProperty, value);
        }

        public static readonly AvaloniaProperty DisabledImageProperty =
            AvaloniaProperty.Register<Button, IBitmap>(nameof(DisabledImage));

        public IBitmap DisabledImage
        {
            get => (IBitmap) GetValue(DisabledImageProperty);
            set => SetValue(DisabledImageProperty, value);
        }

        public static readonly AvaloniaProperty NormalImageProperty =
            AvaloniaProperty.Register<Button, IBitmap>(nameof(NormalImage));

        public IBitmap NormalImage
        {
            get => (IBitmap) GetValue(NormalImageProperty);
            set => SetValue(NormalImageProperty, value);
        }

        public static readonly AvaloniaProperty PressedImageProperty =
            AvaloniaProperty.Register<Button, IBitmap>(nameof(PressedImage));

        public IBitmap PressedImage
        {
            get => (IBitmap) GetValue(PressedImageProperty);
            set => SetValue(PressedImageProperty, value);
        }

        public static readonly AvaloniaProperty HoverImageProperty =
            AvaloniaProperty.Register<Button, IBitmap>(nameof(HoverImage));

        public IBitmap HoverImage
        {
            get => (IBitmap) GetValue(HoverImageProperty);
            set => SetValue(HoverImageProperty, value);
        }

        public static readonly AvaloniaProperty ActiveImageProperty = AvaloniaProperty.Register<Button, IBitmap>(nameof(ActiveImage));

        public IBitmap ActiveImage
        {
            get => (IBitmap) GetValue(ActiveImageProperty);
            set => SetValue(ActiveImageProperty, value);
        }

        public static readonly AvaloniaProperty IsActiveProperty = AvaloniaProperty.Register<Button, bool>(nameof(IsActive));

        public bool IsActive
        {
            get => (bool) GetValue(IsActiveProperty);
            set => SetValue(IsActiveProperty, value);
        }

        public static readonly AvaloniaProperty SoundEnterProperty = AvaloniaProperty.Register<Button, int>(nameof(SoundEnter));

        public int SoundEnter
        {
            get => (int) GetValue(SoundEnterProperty);
            set => SetValue(SoundEnterProperty, value);
        }

        public static readonly AvaloniaProperty SoundLeaveProperty = AvaloniaProperty.Register<Button, int>(nameof(SoundLeave));

        public int SoundLeave
        {
            get => (int) GetValue(SoundLeaveProperty);
            set => SetValue(SoundLeaveProperty, value);
        }

        public static readonly AvaloniaProperty SoundClickProperty = AvaloniaProperty.Register<Button, int>(nameof(SoundClick));

        public int SoundClick
        {
            get => (int) GetValue(SoundClickProperty);
            set => SetValue(SoundClickProperty, value);
        }

        public static readonly AvaloniaProperty SoundDownProperty = AvaloniaProperty.Register<Button, int>(nameof(SoundDown));

        public int SoundDown
        {
            get => (int) GetValue(SoundDownProperty);
            set => SetValue(SoundDownProperty, value);
        }

        public Button()
        {
            AvaloniaXamlLoader.Load(this);
        }

        protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
        {
            base.OnPropertyChanged(change);

            var property = change.Property;
            if (property == IsPointerOverProperty || property == IsPressedProperty
                                                  || property == IsEnabledProperty
                                                  || property == NormalImageProperty
                                                  || property == HoverImageProperty
                                                  || property == PressedImageProperty
                                                  || property == DisabledImageProperty
                                                  || property == ActiveImageProperty
                                                  || property == IsActiveProperty)
            {
                UpdateImage();
            }
        }

        private void UpdateImage()
        {
            if (!IsEnabled)
            {
                if (DisabledImage != null)
                {
                    CurrentImage = DisabledImage;
                }
                else
                {
                    CurrentImage = NormalImage;
                }
            }
            else
            {
                if (IsPressed && IsPointerOver)
                {
                    if (PressedImage != null)
                    {
                        CurrentImage = PressedImage;
                    }
                    else if (HoverImage != null)
                    {
                        CurrentImage = HoverImage;
                    }
                    else
                    {
                        CurrentImage = NormalImage;
                    }
                }
                else if (IsActive)
                {
                    // Activated, else Pressed, else Hovered, (else Normal)
                    if (ActiveImage != null)
                    {
                        CurrentImage = ActiveImage;
                    }
                    else if (PressedImage != null)
                    {
                        CurrentImage = PressedImage;
                    }
                    else if (HoverImage != null)
                    {
                        CurrentImage = HoverImage;
                    }
                    else
                    {
                        CurrentImage = NormalImage;
                    }
                }
                else if (IsPressed || IsPointerOver)
                {
                    if (HoverImage != null)
                    {
                        CurrentImage = HoverImage;
                    }
                    else
                    {
                        CurrentImage = NormalImage;
                    }
                }
                else
                {
                    CurrentImage = NormalImage;
                }
            }
        }
    }
}

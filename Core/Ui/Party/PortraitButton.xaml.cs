using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls.Metadata;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using OpenTemple.Core.GameObject;
using OpenTemple.Core.GFX;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.TigSubsystems;
using ReactiveUI;
using Canvas = OpenTemple.Widgets.Canvas;

namespace OpenTemple.Core.Ui.Party
{
    [PseudoClasses(":greyPortrait", ":selected", ":hover", ":pressed")]
    internal class PortraitButton : Canvas
    {
        public static readonly AvaloniaProperty<GameObjectBody> CritterProperty = AvaloniaProperty.RegisterDirect<PortraitButton, GameObjectBody>(nameof(Critter), x => x.Critter, (x, v) => x.Critter = v);

        private GameObjectBody _critter;

        public GameObjectBody Critter
        {
            get => _critter;
            set => SetAndRaise(CritterProperty, ref _critter, value);
        }

        public static readonly AvaloniaProperty<IBitmap> NormalPortraitProperty = AvaloniaProperty.RegisterDirect<PortraitButton, IBitmap>(nameof(NormalPortrait), x => x._normalPortrait, (x, v) => x._normalPortrait = v);

        private IBitmap _normalPortrait;

        public IBitmap NormalPortrait
        {
            get => _normalPortrait;
            private set => SetAndRaise(NormalPortraitProperty, ref _normalPortrait, value);
        }

        public static readonly AvaloniaProperty<IBitmap> GreyPortraitProperty = AvaloniaProperty.RegisterDirect<PortraitButton, IBitmap>(nameof(GreyPortrait), x => x._greyPortrait, (x, v) => x._greyPortrait = v);

        private IBitmap _greyPortrait;

        public IBitmap GreyPortrait
        {
            get => _greyPortrait;
            private set => SetAndRaise(GreyPortraitProperty, ref _greyPortrait, value);
        }

        public static readonly AvaloniaProperty<string> HpTextProperty = AvaloniaProperty.RegisterDirect<PortraitButton, string>(nameof(HpText), x => x._hpText, (x, v) => x._hpText = v);

        private string _hpText;

        public string HpText
        {
            get => _hpText;
            private set => SetAndRaise(HpTextProperty, ref _hpText, value);
        }

        public static readonly AvaloniaProperty<string> SubdualHpTextProperty = AvaloniaProperty.RegisterDirect<PortraitButton, string>(nameof(SubdualHpText), x => x.SubdualHpText, (x, v) => x.SubdualHpText = v);

        private string _subdualHpText;

        public string SubdualHpText
        {
            get => _subdualHpText;
            set => SetAndRaise(SubdualHpTextProperty, ref _subdualHpText, value);
        }

        public static readonly AvaloniaProperty<bool> IsPointerDownProperty = AvaloniaProperty.RegisterDirect<PortraitButton, bool>(nameof(IsPointerDown), x => x.IsPointerDown, (x, v) => x.IsPointerDown = v);

        private bool _isPointerDown;

        public bool IsPointerDown
        {
            get => _isPointerDown;
            set => SetAndRaise(IsPointerDownProperty, ref _isPointerDown, value);
        }

        private CompositeDisposable _disposable;

        public PortraitButton()
        {
            AvaloniaXamlLoader.Load(this);
        }
        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            _disposable = new CompositeDisposable();

            this.WhenAnyValue(x => x.IsPointerDown, x => x.IsPointerOver, x => x.Critter)
                .CombineLatest(UiSystems.Party.WhenAnyValue(x => x.ForceHovered))
                .CombineLatest(UiSystems.Party.WhenAnyValue(x => x.ForcePressed))
                .Subscribe(args =>
                {
                    var (((pointerDown, pointerOver, critter), forcedHover), forcedPress) = args;
                    var showAsHover = pointerOver || forcedHover == critter;
                    var showAsPressed = pointerDown || forcedPress == critter;
                    PseudoClasses.Toggle(":hover", showAsHover);
                    PseudoClasses.Toggle(":pressed", showAsPressed);
                    if (critter == null)
                    {
                        return;
                    }

                    if (showAsHover)
                    {
                        if (!UiSystems.CharSheet.HasCurrentCritter && !UiSystems.Logbook.IsVisible)
                        {
                            UiSystems.InGameSelect.Focus = Critter;
                        }
                    }
                    if (showAsPressed)
                    {
                         if (!UiSystems.CharSheet.HasCurrentCritter)
                         {
                             UiSystems.InGameSelect.AddToFocusGroup(Critter);
                         }
                    }
                })
                .DisposeWith(_disposable);

            this.WhenAnyValue(x => x.Critter).Select(c => c?.GetInt32(obj_f.critter_portrait))
                .DistinctUntilChanged()
                .Subscribe(UpdatePortrait)
                .DisposeWith(_disposable);

            this.WhenAnyValue(x => x.Critter)
                .Select(c => c.NullSafeChanges())
                .Switch()
                .Subscribe(UpdateHp)
                .DisposeWith(_disposable);
        }

        /// <inheritdoc/>
        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            base.OnPointerPressed(e);

            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                IsPointerDown = true;
                e.Handled = true;
            }
        }

        /// <inheritdoc/>
        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            base.OnPointerReleased(e);

            if (IsPointerDown && e.InitialPressMouseButton == MouseButton.Left)
            {
                IsPointerDown = false;
                e.Handled = true;
            }
        }

        protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
        {
            IsPointerDown = false;
        }

        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            _disposable?.Dispose();
        }

        [TempleDllLocation(0x10132850)]
        private void UpdateHp(GameObjectBody critter)
        {
            if (critter == null)
            {
                return;
            }

            var hpMax = GameSystems.Stat.StatLevelGet(critter, Stat.hp_max);
            var hpCurrent = GameSystems.Stat.StatLevelGet(critter, Stat.hp_current);
            if (hpCurrent < 1)
            {
                // TODO: This should probably use a D20 dispatch to see if
                // TODO: the character has the disabled or dying status
                PseudoClasses.Add(":greyPortrait");
            }
            else
            {
                PseudoClasses.Toggle(":greyPortrait", !IsEnabled);
            }

            if (Globals.Config.ShowPartyHitPoints)
            {
                HpText = $"{hpCurrent}/{hpMax}";
                var subdualDamage = Critter.GetInt32(obj_f.critter_subdual_damage);
                SubdualHpText = (subdualDamage > 0) ? $"({subdualDamage})" : "";
            }

            // TODO: Render flashing get-hit indicator
            PseudoClasses.Toggle(":selected", GameSystems.Party.IsSelected(critter));
        }

        [TempleDllLocation(0x10132850)]
        private void UpdatePortrait(int? portraitId)
        {
            NormalPortrait?.Dispose();
            GreyPortrait?.Dispose();

            if (portraitId.HasValue)
            {
                var normalPath = GameSystems.UiArtManager.GetPortraitPath(portraitId.Value, PortraitVariant.Small);
                NormalPortrait = new Bitmap(normalPath);

                var greyPath = GameSystems.UiArtManager.GetPortraitPath(portraitId.Value, PortraitVariant.SmallGrey);
                if (greyPath != null)
                {
                    GreyPortrait = new Bitmap(greyPath);
                }
            }
        }
    }
}

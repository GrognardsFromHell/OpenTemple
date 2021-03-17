using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls.Metadata;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using OpenTemple.Core.GameObject;
using OpenTemple.Core.GFX;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Ui.Widgets;
using ReactiveUI;
using Canvas = OpenTemple.Widgets.Canvas;

namespace OpenTemple.Core.Ui.Party
{
    [PseudoClasses(":greyPortrait", ":selected", ":pointerdown")]
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

        private static readonly TigTextStyle HpTextStyle = new TigTextStyle(new ColorRect(PackedLinearColorA.White))
        {
            flags = TigTextStyleFlag.TTSF_CENTER | TigTextStyleFlag.TTSF_DROP_SHADOW,
            shadowColor = new ColorRect(PackedLinearColorA.Black),
            bgColor = new ColorRect(new PackedLinearColorA(17, 17, 17, 153)),
            kerning = 2,
            tracking = 5,
            additionalTextColors = new[]
            {
                // Used for subdual damage
                new ColorRect(new PackedLinearColorA(0xFF6666FF)),
            }
        };

        private WidgetLegacyText _hpLabel;

        private CompositeDisposable _disposable;

        public PortraitButton()
        {
            AvaloniaXamlLoader.Load(this);
        }
        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            _disposable = new CompositeDisposable();

            this.WhenAnyValue(x => x.IsPointerOver, x => x.Critter)
                .CombineLatest(UiSystems.Party.WhenAnyValue(x => x.ForceHovered))
                .CombineLatest(UiSystems.Party.WhenAnyValue(x => x.ForcePressed))
                .Subscribe(args =>
                {
                    var (((pointerOver, critter), forcedHover), forcedPress) = args;
                    var showAsHover = pointerOver || forcedHover == critter;
                    PseudoClasses.Toggle(":hover", showAsHover);

                    if (showAsHover)
                    {
                        if (!UiSystems.CharSheet.HasCurrentCritter && !UiSystems.Logbook.IsVisible)
                        {
                            UiSystems.InGameSelect.Focus = Critter;
                        }
                    }
                    if (Classes.Contains(":pressed") || forcedPress == critter)
                    {
                    //     AddContent(_highlightPressed);
                    //
                    //     if (!UiSystems.CharSheet.HasCurrentCritter)
                    //     {
                    //         UiSystems.InGameSelect.AddToFocusGroup(Critter);
                    //     }
                    }
                })
                .DisposeWith(_disposable);

            this.WhenAnyValue(x => x.Critter).Select(c => c?.GetInt32(obj_f.critter_portrait))
                .DistinctUntilChanged()
                .Subscribe(UpdatePortrait)
                .DisposeWith(_disposable);

            this.WhenAnyValue(x => x.Critter)
                .Select(c => c.Changes)
                .Switch()
                .Subscribe(UpdateHp)
                .DisposeWith(_disposable);
        }

        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            _disposable?.Dispose();
        }

        [TempleDllLocation(0x10132850)]
        public void UpdateHp(GameObjectBody critter)
        {
            var hpMax = GameSystems.Stat.StatLevelGet(Critter, Stat.hp_max);
            var hpCurrent = GameSystems.Stat.StatLevelGet(Critter, Stat.hp_current);
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
                if (_hpLabel == null)
                {
                    _hpLabel = new WidgetLegacyText("HP", PredefinedFont.ARIAL_10, HpTextStyle);
                }

                HpText = $"@0{hpCurrent}/{hpMax}";
                var subdualDamage = Critter.GetInt32(obj_f.critter_subdual_damage);
                if (subdualDamage > 0)
                {
                    HpText += $"@1({subdualDamage})";
                }
            }

            // TODO: Render flashing get-hit indicator
            PseudoClasses.Toggle(":selected", GameSystems.Party.IsSelected(Critter));

            // if (ButtonState == LgcyButtonState.Hovered || UiSystems.Party.ForceHovered == Critter)
            // {
            //     AddContent(_highlightHover);
            // }
            // else
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

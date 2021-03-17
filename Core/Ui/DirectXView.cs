using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Angle;
using Avalonia.OpenGL.Egl;
using Avalonia.Platform;
using Avalonia.Platform.Interop;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using OpenTemple.Core.GFX;
using OpenTemple.Core.TigSubsystems;
using SharpDX;
using SharpDX.Direct3D11;
using SkiaSharp;
using Vortice.DXGI;
using Point = Avalonia.Point;
using Resource = SharpDX.DXGI.Resource;

namespace OpenTemple.Core.Ui
{
    public abstract unsafe class DirectXView : Control
    {
        private const double BlurRadiusTolerance = 0.1;

        private ResourceRef<RenderTargetTexture> _backBuffer;
        private RenderTargetView _renderView;
        private ResourceRef<RenderTargetDepthStencil> _depthBuffer;
        private DepthStencilView _depthView;

        private readonly EglPlatformOpenGlInterface _egl;
        private readonly AngleWin32EglDisplay _display;
        private readonly RenderingDevice _renderingDevice = Tig.RenderingDevice;

        // An EGL surface that is backed by our backbuffer
        private EglSurface _surface;
        private SKImage _image;
        private SKPaint _paint;

        private PixelSize _currentPixelSize;

        private readonly delegate*<IntPtr, IntPtr, int, bool> _bindTexImage;

        public static readonly AvaloniaProperty RenderScaleProperty = AvaloniaProperty.Register<DirectXView, float>(
            nameof(RenderScale), 1.0f);

        public float RenderScale
        {
            get => (float) GetValue(RenderScaleProperty);
            set => SetValue(RenderScaleProperty, value);
        }

        public static readonly AvaloniaProperty MultiSamplingProperty = AvaloniaProperty.Register<DirectXView, MultiSampleSettings>(
            nameof(MultiSampling));

        public MultiSampleSettings MultiSampling
        {
            get => (MultiSampleSettings) GetValue(MultiSamplingProperty);
            set => SetValue(MultiSamplingProperty, value);
        }

        public static readonly AvaloniaProperty BlurRadiusProperty = AvaloniaProperty.Register<DirectXView, float>(nameof(BlurRadius));

        public float BlurRadius
        {
            get => (float) GetValue(BlurRadiusProperty);
            set => SetValue(BlurRadiusProperty, value);
        }

        static DirectXView()
        {
            AffectsRender<DirectXView>(BlurRadiusProperty);
        }

        public DirectXView()
        {
            _egl = (EglPlatformOpenGlInterface) AvaloniaLocator.Current.GetService<IPlatformOpenGlInterface>();
            _display = (AngleWin32EglDisplay) _egl.Display;

            using var procName = new Utf8Buffer("eglBindTexImage");
            _bindTexImage = (delegate*<IntPtr, IntPtr, int, bool>) _display.EglInterface.GetProcAddress(procName);
        }

        protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == MultiSamplingProperty || change.Property == RenderScaleProperty)
            {
                FreeDeviceResources();
            }
            else if (change.Property == BlurRadiusProperty)
            {
                _paint?.Dispose();
                _paint = null;
            }
        }

        protected override void OnMeasureInvalidated()
        {
            base.OnMeasureInvalidated();
            FreeDeviceResources();
        }

        public sealed override void Render(DrawingContext context)
        {
            base.Render(context);

            var pixelSize = RenderPixelSize;
            if (pixelSize != _currentPixelSize || !_backBuffer.IsValid || !_depthBuffer.IsValid)
            {
                Resize(pixelSize);
                _currentPixelSize = pixelSize;
            }

            _renderingDevice.PushRenderTarget(_backBuffer, _depthBuffer);
            _renderingDevice.ClearCurrentColorTarget(LinearColorA.Black);
            _renderingDevice.ClearCurrentDepthTarget();
            try
            {
                OnRender(_renderingDevice, _currentPixelSize);
            }
            finally
            {
                _renderingDevice.PopRenderTarget();
            }
            _renderingDevice.Device.ImmediateContext.Flush();

            context.Custom(new DrawOp(this, BlurRadius));
        }

        private class DrawOp : ICustomDrawOperation
        {
            private readonly DirectXView _view;

            private readonly float _blurRadius;

            public DrawOp(DirectXView view, float blurRadius)
            {
                _view = view;
                _blurRadius = blurRadius;
            }

            public bool HitTest(Point p) => true;

            public void Render(IDrawingContextImpl context)
            {
                var skiaContext = (ISkiaDrawingContextImpl) context;
                _view.RenderOnRenderThread(skiaContext, _blurRadius);
            }

            public Rect Bounds => _view.Bounds;

            public bool Equals(ICustomDrawOperation? other)
            {
                return other is DrawOp drawOp
                       && ReferenceEquals(drawOp._view, _view)
                       && Math.Abs(drawOp._blurRadius - _blurRadius) < BlurRadiusTolerance;
            }

            public void Dispose()
            {
            }
        }

        private void RenderOnRenderThread(ISkiaDrawingContextImpl skiaContext, float blurRadius)
        {
            // Re-Create the Skia wrappers as needed
            if (_surface == null || _image == null)
            {
                var texture = _backBuffer.Resource.ResolvedTexture ?? _backBuffer.Resource.Texture;
                using var dxgiResource = texture.QueryInterface<Resource>();
                var shareHandle = dxgiResource.SharedHandle;
                if (shareHandle == IntPtr.Zero)
                {
                    throw new ArgumentException("There's no share handle defined for the backbuffer");
                }

                _surface = _egl.CreatePBufferFromClientBuffer(EglConsts.EGL_D3D_TEXTURE_2D_SHARE_HANDLE_ANGLE,
                    shareHandle, new[]
                    {
                        EglConsts.EGL_WIDTH, _backBuffer.Resource.GetSize().Width,
                        EglConsts.EGL_HEIGHT, _backBuffer.Resource.GetSize().Height,
                        EglConsts.EGL_TEXTURE_TARGET, EglConsts.EGL_TEXTURE_2D,
                        EglConsts.EGL_TEXTURE_FORMAT, EglConsts.EGL_TEXTURE_RGBA,
                        EglConsts.EGL_NONE, EglConsts.EGL_NONE
                    });

                // Create a OpenGL texture, and bind the eGL surface to it, which effectively makes the
                // OpenGL texture 1:1 with our back buffer.
                var gl = _egl.PrimaryContext.GlInterface;
                var textureIds = new int[1];
                gl.GenTextures(1, textureIds);
                gl.ActiveTexture(0);
                gl.BindTexture(GlConsts.GL_TEXTURE_2D, textureIds[0]);
                gl.TexParameteri(GlConsts.GL_TEXTURE_2D, GlConsts.GL_TEXTURE_MAG_FILTER, GlConsts.GL_NEAREST);
                gl.TexParameteri(GlConsts.GL_TEXTURE_2D, GlConsts.GL_TEXTURE_MIN_FILTER, GlConsts.GL_NEAREST);
                gl.TexParameteri(GlConsts.GL_TEXTURE_2D, GlConsts.GL_TEXTURE_WRAP_R, GlConsts.GL_CLAMP);
                gl.TexParameteri(GlConsts.GL_TEXTURE_2D, GlConsts.GL_TEXTURE_WRAP_S, GlConsts.GL_CLAMP);
                _bindTexImage(_display.Handle, _surface.DangerousGetHandle(), EglConsts.EGL_BACK_BUFFER);

                // Skia docs say "Note - Skia will delete or recycle the texture when the image is released."
                var size = _backBuffer.Resource.GetSize();
                using var backendTexture = new GRBackendTexture(size.Width, size.Height, false,
                    new GRGlTextureInfo(GlConsts.GL_TEXTURE_2D, (uint) textureIds[0], GlConsts.GL_RGBA8));
                _image = SKImage.FromAdoptedTexture(skiaContext.GrContext, backendTexture, GRSurfaceOrigin.TopLeft,
                    SKColorType.Rgba8888, SKAlphaType.Opaque);
            }

            if (blurRadius > BlurRadiusTolerance)
            {
                if (_paint == null)
                {
                    _paint = new SKPaint();
                    using var filter = SKImageFilter.CreateBlur(blurRadius, blurRadius, SKShaderTileMode.Clamp);
                    _paint.ImageFilter = filter;
                }

                skiaContext.SkCanvas.DrawImage(_image, Bounds.ToSKRect(), _paint);
            }
            else
            {
                skiaContext.SkCanvas.DrawImage(_image, Bounds.ToSKRect());
            }
        }

        protected virtual void OnRender(RenderingDevice device, PixelSize pixelSize)
        {
        }

        protected virtual void OnResize(PixelSize pixelSize)
        {
        }

        private void Resize(PixelSize size)
        {
            FreeDeviceResources();

            // Create the buffers for the scaled game view
            _backBuffer = _renderingDevice.CreateRenderTargetTexture(
                BufferFormat.A8R8G8B8, size.Width, size.Height, MultiSampling
            );
            _depthBuffer = _renderingDevice.CreateRenderTargetDepthStencil(
                size.Width, size.Height, MultiSampling
            );

            OnResize(size);
        }

        /// <summary>
        /// Size in pixels of the render target that is used for rendering the 3D content.
        /// </summary>
        protected PixelSize RenderPixelSize
        {
            get
            {
                var outputPixelSize = OutputPixelSize;
                return new PixelSize(
                    Math.Max(1, (int)(RenderScale * outputPixelSize.Width)),
                    Math.Max(1, (int)(RenderScale * outputPixelSize.Height))
                );
            }
        }

        /// <summary>
        /// Size in pixels of the output drawing surface.
        /// </summary>
        protected PixelSize OutputPixelSize
        {
            get
            {
                var scaling = VisualRoot.RenderScaling;
                return new PixelSize(Math.Max(1, (int) (Bounds.Width * scaling)),
                    Math.Max(1, (int) (Bounds.Height * scaling)));
            }
        }

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);

            Tig.MainWindow.BeforeRenderContent += InvalidateVisual;
        }

        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnDetachedFromVisualTree(e);

            Tig.MainWindow.BeforeRenderContent -= InvalidateVisual;
            FreeDeviceResources();
        }

        private void FreeDeviceResources()
        {
            Utilities.Dispose(ref _renderView);
            Utilities.Dispose(ref _depthView);
            Utilities.Dispose(ref _surface);
            Utilities.Dispose(ref _image);
            _backBuffer.Dispose();
            _depthBuffer.Dispose();
        }
    }
}

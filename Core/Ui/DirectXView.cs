using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Angle;
using Avalonia.OpenGL.Egl;
using Avalonia.Platform.Interop;
using Avalonia.Skia;
using OpenTemple.Core.GFX;
using OpenTemple.Core.TigSubsystems;
using SharpDX;
using SharpDX.Direct3D11;
using SkiaSharp;

namespace OpenTemple.Core.Ui
{
    public abstract unsafe class DirectXView : Control
    {
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
        }

        public sealed override void Render(DrawingContext context)
        {
            base.Render(context);

            var skiaContext = (ISkiaDrawingContextImpl) context.PlatformImpl;

            var pixelSize = RenderPixelSize;
            if (pixelSize != _currentPixelSize || !_backBuffer.IsValid || !_depthBuffer.IsValid)
            {
                Resize(skiaContext, pixelSize);
                _currentPixelSize = pixelSize;
            }

            _renderingDevice.SaveState();
            _renderingDevice.PushRenderTarget(_backBuffer, _depthBuffer);
            try
            {
                OnRender(_renderingDevice, pixelSize);
            }
            finally
            {
                _renderingDevice.PopRenderTarget();
                _renderingDevice.RestoreState();
            }

            using var paint = new SKPaint();
            using var filter = SKImageFilter.CreateBlur(8, 8);
            paint.ImageFilter = filter;

            skiaContext.SkCanvas.DrawImage(_image, Bounds.ToSKRect(), paint);
        }

        protected virtual void OnRender(RenderingDevice device, PixelSize pixelSize)
        {
        }

        protected virtual void OnResize(PixelSize pixelSize)
        {
        }

        private void Resize(ISkiaDrawingContextImpl skiaContext, PixelSize size)
        {
            FreeDeviceResources();

            // Create the buffers for the scaled game view
            _backBuffer = _renderingDevice.CreateRenderTargetTexture(
                BufferFormat.A8R8G8B8, size.Width, size.Height, MultiSampling
            );
            _depthBuffer = _renderingDevice.CreateRenderTargetDepthStencil(
                size.Width, size.Height, MultiSampling
            );

            using (var unused = _egl.PrimaryContext.MakeCurrent())
            {
                _surface = _egl.CreatePBufferFromClientBuffer(EglConsts.EGL_D3D_TEXTURE_ANGLE,
                    _backBuffer.Resource.Texture.NativePointer, new[]
                    {
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
                using var backendTexture = new GRBackendTexture(size.Width, size.Height, false,
                    new GRGlTextureInfo(GlConsts.GL_TEXTURE_2D, (uint) textureIds[0], GlConsts.GL_RGBA8));
                _image = SKImage.FromAdoptedTexture(skiaContext.GrContext, backendTexture, GRSurfaceOrigin.TopLeft,
                    SKColorType.Rgba8888, SKAlphaType.Opaque);
            }

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

        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnDetachedFromVisualTree(e);

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

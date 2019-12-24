using System;
using System.Diagnostics;
using System.Numerics;
using OpenTemple.Core.AAS;
using OpenTemple.Core.GFX;
using OpenTemple.Core.Particles.Instances;
using OpenTemple.Core.Particles.Render;
using OpenTemple.Core.TigSubsystems;

namespace OpenTemple.Core.Systems
{
    public class ParticleSystemsRenderer : IDisposable
    {
        private readonly ParticleSysSystem _particleSysSystem;
        private int _renderedLastFrame;
        private readonly ParticleRendererManager _rendererManager;
        private readonly RenderingDevice _renderingDevice;
        private readonly int[] _renderTimes = new int[100];
        private int _renderTimesPos;
        private readonly ShapeRenderer2d _shapeRenderer2d;
        private int _totalLastFrame;

        public ParticleSystemsRenderer(
            RenderingDevice renderingDevice,
            ShapeRenderer2d shapeRenderer2d,
            IAnimatedModelFactory modelFactory,
            IAnimatedModelRenderer modelRenderer,
            ParticleSysSystem particleSysSystem
        )
        {
            _renderingDevice = renderingDevice;
            _shapeRenderer2d = shapeRenderer2d;
            _particleSysSystem = particleSysSystem;

            _rendererManager = new ParticleRendererManager(
                renderingDevice,
                modelFactory,
                modelRenderer
            );
        }

        public void Dispose()
        {
            _rendererManager.Dispose();
        }

        public void Render()
        {
            using var perfGroup = _renderingDevice.CreatePerfGroup("Particles");

            var camera = _renderingDevice.GetCamera();

            _totalLastFrame = 0;
            _renderedLastFrame = 0;

            var sw = Stopwatch.StartNew();

            foreach (var partSys in _particleSysSystem.ActiveSystems)
            {
                _totalLastFrame++;

                var screenBounds = partSys.GetScreenBounds();

                if (!camera.IsBoxOnScreen(partSys.GetScreenPosAbs(),
                    screenBounds.left, screenBounds.top,
                    screenBounds.right, screenBounds.bottom))
                {
                    continue;
                }

                _renderedLastFrame++;

                using var sysPerfGroup = _renderingDevice.CreatePerfGroup("PartSys '{0}'", partSys.GetSpec().GetName());

                // each emitter is rendered individually
                foreach (var emitter in partSys.GetEmitters())
                {
                    if (emitter.GetActiveCount() == 0)
                    {
                        continue; // Skip emitters with no particles
                    }

                    using var emitterPerfGroup =
                        _renderingDevice.CreatePerfGroup("Emitter '{0}'", emitter.GetSpec().GetName());

                    var type = emitter.GetSpec().GetParticleType();
                    var renderer = _rendererManager.GetRenderer(type);
                    renderer.Render(emitter);
                }

                if (Globals.Config.DebugPartSys)
                {
                    RenderDebugInfo(partSys);
                }
            }

            _renderTimes[_renderTimesPos++] = (int) sw.ElapsedMilliseconds;
            if (_renderTimesPos >= _renderTimes.Length)
            {
                _renderTimesPos = 0;
            }
        }

        private int GetRenderedLastFrame()
        {
            return _renderedLastFrame;
        }

        private int GetTotalLastFrame()
        {
            return _totalLastFrame;
        }

        private int GetRenderTimeAvg()
        {
            var sum = 0;
            foreach (var renderTime in _renderTimes)
            {
                sum += renderTime;
            }

            return sum / _renderTimes.Length;
        }

        private void RenderDebugInfo(PartSys sys)
        {
            var camera = _renderingDevice.GetCamera();

            var dx = camera.Get2dTranslation().X;
            var dy = camera.Get2dTranslation().Y;

            var screenX = dx - sys.GetScreenPosAbs().X;
            var screenY = dy - sys.GetScreenPosAbs().Y;
            var screenBounds = sys.GetScreenBounds();

            screenX *= camera.GetScale();
            screenY *= camera.GetScale();

            screenX += camera.GetScreenWidth() * 0.5f;
            screenY += camera.GetScreenHeight() * 0.5f;

            var left = screenX + screenBounds.left;
            var top = screenY + screenBounds.top;
            var right = screenX + screenBounds.right;
            var bottom = screenY + screenBounds.bottom;

            var color = new PackedLinearColorA(0, 0, 255, 255);

            Span<Line2d> lines = stackalloc Line2d[4]
            {
                new Line2d(new Vector2(left, top), new Vector2(right, top), color),
                new Line2d(new Vector2(right, top), new Vector2(right, bottom), color),
                new Line2d(new Vector2(right, bottom), new Vector2(left, bottom), color),
                new Line2d(new Vector2(left, bottom), new Vector2(left, top), color)
            };
            _shapeRenderer2d.DrawLines(lines);

            Tig.Fonts.PushFont(PredefinedFont.ARIAL_10);

            var style = new TigTextStyle
            {
                field10 = 25,
                textColor = new ColorRect(PackedLinearColorA.White)
            };

            var text = $"{sys.GetSpec().GetName()}";
            var rect = Tig.Fonts.MeasureTextSize(text, style);

            rect.X = (int) left;
            rect.Y = (int) top;
            Tig.Fonts.RenderText(text, rect, style);

            Tig.Fonts.PopFont();
        }
    }
}
using System;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using OpenTemple.Core.AAS;
using OpenTemple.Core.GFX;
using OpenTemple.Core.Particles.Instances;
using OpenTemple.Core.Particles.Render;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Ui;
using OpenTemple.Core.Ui.FlowModel;

namespace OpenTemple.Core.Systems;

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

    public void Render(IGameViewport viewport)
    {
        using var perfGroup = _renderingDevice.CreatePerfGroup("Particles");

        _totalLastFrame = 0;
        _renderedLastFrame = 0;

        var sw = Stopwatch.StartNew();

        foreach (var partSys in _particleSysSystem.ActiveSystems)
        {
            _totalLastFrame++;

            if (!partSys.IsOnScreen(viewport))
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
                renderer.Render(viewport, emitter);
            }

            if (Globals.Config.Debug.ParticleSystems)
            {
                RenderDebugInfo(viewport, partSys);
            }
        }

        _renderTimes[_renderTimesPos++] = (int)sw.ElapsedMilliseconds;
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

    private void RenderDebugInfo(IGameViewport gameViewport, PartSys sys)
    {
        var screenPos = gameViewport.WorldToScreen(sys.WorldPos);
        var screenX = screenPos.X;
        var screenY = screenPos.Y;
        var screenBounds = sys.GetScreenBounds();

        var left = screenX + screenBounds.left * gameViewport.Zoom;
        var top = screenY + screenBounds.top * gameViewport.Zoom;
        var right = screenX + screenBounds.right * gameViewport.Zoom;
        var bottom = screenY + screenBounds.bottom * gameViewport.Zoom;

        var color = new PackedLinearColorA(0, 0, 255, 255);

        Span<Line2d> lines = stackalloc Line2d[4]
        {
            new Line2d(new Vector2(left, top), new Vector2(right, top), color),
            new Line2d(new Vector2(right, top), new Vector2(right, bottom), color),
            new Line2d(new Vector2(right, bottom), new Vector2(left, bottom), color),
            new Line2d(new Vector2(left, bottom), new Vector2(left, top), color)
        };
        _shapeRenderer2d.DrawLines(lines);

        var textEngine = Tig.RenderingDevice.TextEngine;

        var text = $"{sys.GetSpec().GetName()}";

        var style = Globals.UiStyles.GetComputed("default");
        var metrics = textEngine.MeasureText(style, text, (int)(right - left), (int)(bottom - top));

        textEngine.RenderText(
            new RectangleF(
                (left + right - metrics.width) / 2f,
                (top + bottom - metrics.height) / 2f,
                (int)(right - left),
                (int)(bottom - top)
            ),
            style,
            text
        );
    }
}
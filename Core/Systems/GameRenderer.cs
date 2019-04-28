using System.Numerics;
using SpicyTemple.Core.AAS;
using SpicyTemple.Core.GFX;
using SpicyTemple.Core.GFX.RenderMaterials;
using SpicyTemple.Core.Time;

namespace SpicyTemple.Core.Systems
{
    public class GameRenderer
    {
        private readonly AasRenderer _renderer;

        private readonly IAnimatedModel _model;
        private TimePoint _lastUpdate = TimePoint.Now;

        public GameRenderer()
        {
            _renderer = GameSystems.AAS.Renderer;

            var animParams = AnimatedModelParams.Default;
            _model = GameSystems.AAS.ModelFactory.FromIds(
                1000,
                1000,
                new EncodedAnimId(WeaponAnim.Idle),
                animParams
            );
            _model.SetAnimId(new EncodedAnimId(WeaponAnim.Idle));
        }

        public void Render()
        {
            var animParams = AnimatedModelParams.Default;
            animParams.rotation = 0.8f;

            var elapsed = TimePoint.Now - _lastUpdate;
            if (elapsed.Milliseconds > 10)
            {
                _lastUpdate = TimePoint.Now;
                _model.Advance((float) elapsed.Seconds, 0, 0, animParams);
            }

            var lights = new Light3d[1];
            lights[0] = new Light3d()
            {
                ambient = Vector4.One,
                dir = new Vector4(0, -1, 0, 0),
                color = Vector4.One,
                type = Light3dType.Directional
            };
            _renderer.Render(_model, animParams, lights);
        }
    }
}
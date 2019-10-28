using System;
using System.Text;
using System.Text.RegularExpressions;
using SpicyTemple.Core.Systems;

namespace SpicyTemple.Core.AAS
{
    /// <summary>
    /// Converts legacy python scripts into the newer more declarative events, and uses a static lookup
    /// table of all known script snippets that do not fit into the more easily parsable patterns.
    /// </summary>
    public class LegacyScriptConverter
    {
        private const string StringArg = @"'([^']*)'";
        private const string IntArg = @"(\d+)";

        private static readonly (Regex, Func<Match, AasEvent>)[] ParticlesPattern =
        {
            (CreateMethodRegex("game.particles", StringArg, "anim_obj"), ConvertParticles),
            (CreateMethodRegex("game.shake", IntArg, IntArg), ConvertScreenShake),
            (CreateMethodRegex("game.sound_local_obj", IntArg, "anim_obj"), ConvertSound),
            (CreateMethodRegex("anim_obj.footstep"), ConvertFootstep),
            (CreateMethodRegex("anim_obj.fade_to", IntArg, IntArg, IntArg, "OBJFADE_C_POOP_OFF"), ConvertFade),
            (CreateMethodRegex("anim_obj.balor_death"), ConvertBalorDeath),
        };

        private static Regex CreateMethodRegex(string method, params string[] argPatterns)
        {
            var result = new StringBuilder();
            result.Append(@"^\s*");
            result.Append(method);
            result.Append(@"\s*\(");
            for (var i = 0; i < argPatterns.Length; i++)
            {
                if (i > 0)
                {
                    result.Append(@",\s*");
                }

                result.Append(@"\s*");
                result.Append(argPatterns[i]);
                result.Append(@"\s*");
            }

            result.Append(@"\s*\)\s*$");

            return new Regex(result.ToString());
        }

        public bool TryConvert(string script, out AasEvent evt)
        {
            foreach (var (pattern, handler) in ParticlesPattern)
            {
                var m = pattern.Match(script);
                if (m.Success)
                {
                    evt = handler(m);
                    if (evt != null)
                    {
                        return true;
                    }
                }
            }

            evt = null;
            return false;
        }

        private static AasEvent ConvertParticles(Match m)
        {
            var particlesId = m.Groups[1].Value;
            return new AasParticlesEvent(particlesId);
        }

        private static AasEvent ConvertScreenShake(Match m)
        {
            var amount = int.Parse(m.Groups[1].Value);
            var durationMs = int.Parse(m.Groups[2].Value);
            return new AasShakeScreenEvent(amount, TimeSpan.FromMilliseconds(durationMs));
        }

        private static AasEvent ConvertSound(Match m)
        {
            var soundId = int.Parse(m.Groups[1].Value);
            return new AasSoundEvent(soundId);
        }

        private static AasEvent ConvertFootstep(Match m)
        {
            return new AasFootstepEvent();
        }

        private static AasEvent ConvertFade(Match m)
        {
            var targetOpacity = int.Parse(m.Groups[1].Value);
            var msPerTick = int.Parse(m.Groups[2].Value);
            var changePerTick = int.Parse(m.Groups[3].Value);

            return new AasFadeEvent(targetOpacity, msPerTick, changePerTick, FadeOutResult.DropItemsAndDestroy);
        }

        private static AasEvent ConvertBalorDeath(Match m)
        {
            return new AasCustomEvent("balor_death");
        }
    }

    public abstract class AasEvent
    {
    }

    public class AasParticlesEvent : AasEvent
    {
        public string ParticlesId { get; }

        public AasParticlesEvent(string particlesId)
        {
            ParticlesId = particlesId;
        }
    }

    public class AasShakeScreenEvent : AasEvent
    {
        public TimeSpan Duration { get; }

        public int PeakAmplitude { get; }

        public AasShakeScreenEvent(int peakAmplitude, TimeSpan duration)
        {
            PeakAmplitude = peakAmplitude;
            Duration = duration;
        }
    }

    public class AasSoundEvent : AasEvent
    {
        public int SoundId { get; }

        public AasSoundEvent(int soundId)
        {
            SoundId = soundId;
        }
    }

    public class AasFootstepEvent : AasEvent
    {
    }

    public class AasCustomEvent : AasEvent
    {
        public string Name { get; }

        public AasCustomEvent(string name)
        {
            Name = name;
        }
    }

    public class AasFadeEvent : AasEvent
    {
        // 255= Fully visible, 0=Invisible
        public int TargetOpacity { get; }

        // How many milliseconds per tick
        public int TickTimeMs { get; }

        // How much opacity to change per tick
        public int ChangePerTick { get; }

        // What to do once the transition is done
        public FadeOutResult Action { get; }

        public AasFadeEvent(int targetOpacity, int tickTimeMs, int changePerTick, FadeOutResult action)
        {
            TargetOpacity = targetOpacity;
            TickTimeMs = tickTimeMs;
            ChangePerTick = changePerTick;
            Action = action;
        }
    }
}
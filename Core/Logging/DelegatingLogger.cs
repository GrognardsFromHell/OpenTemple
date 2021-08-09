using System;

namespace OpenTemple.Core.Logging
{
    internal sealed class DelegatingLogger : LoggerBase
    {
        internal LoggerBase Delegate { get; set; }

        public DelegatingLogger(LoggerBase @delegate)
        {
            Delegate = @delegate;
        }

        public override void Error(ReadOnlySpan<char> message)
        {
            Delegate.Error(message);
        }

        public override void Warn(ReadOnlySpan<char> message)
        {
            Delegate.Warn(message);
        }

        public override void Info(ReadOnlySpan<char> message)
        {
            Delegate.Info(message);
        }

        public override void Debug(ReadOnlySpan<char> message)
        {
            Delegate.Debug(message);
        }
    }
}
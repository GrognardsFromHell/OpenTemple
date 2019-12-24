namespace SpicyTemple.Core.Logging
{
    internal sealed class DelegatingLogger : LoggerBase
    {
        internal LoggerBase Delegate { get; set; }

        public DelegatingLogger(LoggerBase @delegate)
        {
            Delegate = @delegate;
        }

        public override void Error(string message)
        {
            Delegate.Error(message);
        }

        public override void Warn(string message)
        {
            Delegate.Warn(message);
        }

        public override void Info(string message)
        {
            Delegate.Info(message);
        }

        public override void Debug(string message)
        {
            Delegate.Debug(message);
        }
    }
}
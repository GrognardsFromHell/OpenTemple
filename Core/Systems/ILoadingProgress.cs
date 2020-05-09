using System;

namespace OpenTemple.Core.Systems
{
    public interface ILoadingProgress : IDisposable
    {
        public string Message { set; }

        public float Progress { set; }
    }

    public class DummyLoadingProgress : ILoadingProgress
    {
        public string Message { get; set; }
        public float Progress { get; set; }

        public void Dispose()
        {
        }
    }
}
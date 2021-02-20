namespace OpenTemple.Core.Systems
{
    public interface ILoadingProgress
    {
        public string Message { set; get; }

        public double Progress { get; set; }

        public void Update();
    }

    public class DummyLoadingProgress : ILoadingProgress
    {
        public string Message { get; set; }
        public double Progress { get; set; }

        public void Update()
        {
        }
    }
}

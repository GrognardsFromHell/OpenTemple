namespace OpenTemple.Core.DebugUI
{
    public interface IDebugUI
    {
        void NewFrame();
        void Render();
        void PushSmallFont();
    }
}
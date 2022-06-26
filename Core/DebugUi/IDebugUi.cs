namespace OpenTemple.Core.DebugUi;

public interface IDebugUi
{
    void NewFrame();
    void Render();
    void PushSmallFont();
    void PushBoldFont();
}
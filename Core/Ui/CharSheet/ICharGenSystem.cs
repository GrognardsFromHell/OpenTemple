using System;

namespace OpenTemple.Core.Ui.CharSheet;

public interface ICharGenSystem : IDisposable
{
    string Name { get; }

    void ResetSystem();

    void Resize();

    void Show();

    void Hide();

    void CheckComplete();

    void Complete();

    void Reset();

    void Activate();
}
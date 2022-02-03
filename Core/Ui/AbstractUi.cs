using System;
using System.Drawing;

namespace OpenTemple.Core.Ui;

public abstract class AbstractUi
{
    public virtual void LoadModule()
    {
    }

    public virtual void UnloadModule()
    {
    }

    public virtual void ResizeViewport(Size size)
    {
    }
}
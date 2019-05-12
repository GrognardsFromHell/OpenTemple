using System;
using System.Drawing;

namespace SpicyTemple.Core.Ui
{
    public abstract class AbstractUi
    {
        public virtual void Reset()
        {
        }

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
}
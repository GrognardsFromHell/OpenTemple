using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using OpenTemple.Core.GFX;
using OpenTemple.Core.Platform;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Time;

namespace OpenTemple.Core.TigSubsystems;

public delegate void CursorDrawCallback(int x, int y, object userArg);

public class TigMouse
{
}
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;

namespace Core.WidgetEventGenerator;

public class RootModel
{
    public string Tool { get; }
    public string Version { get; }

    public List<EventDefinition> Events = new()
    {
        new EventDefinition("MouseDown", "OpenTemple.Core.Ui.Events.MouseEvent", true, true, true),
        new EventDefinition("MouseUp", "OpenTemple.Core.Ui.Events.MouseEvent", true, true, true),
        new EventDefinition("MouseEnter", "OpenTemple.Core.Ui.Events.MouseEvent", false, false, false),
        new EventDefinition("MouseLeave", "OpenTemple.Core.Ui.Events.MouseEvent", false, false, false),
        new EventDefinition("MouseMove", "OpenTemple.Core.Ui.Events.MouseEvent", true, true, true),
        new EventDefinition("MouseWheel", "OpenTemple.Core.Ui.Events.WheelEvent", true, true, true),
        new EventDefinition("Click", "OpenTemple.Core.Ui.Events.MouseEvent", true, true, true),
        new EventDefinition("TextInput", "OpenTemple.Core.Ui.Events.TextInputEvent", true, true, true),
        new EventDefinition("KeyDown", "OpenTemple.Core.Ui.Events.KeyboardEvent", true, true, true),
        new EventDefinition("KeyUp", "OpenTemple.Core.Ui.Events.KeyboardEvent", true, true, true),
    };

    public ImmutableSortedSet<string> AdditionalNamespaces;

    public RootModel()
    {
        // Used for the GeneratedCode attribute
        var assemblyName = typeof(Program).Assembly.GetName();
        Debug.Assert(assemblyName.Name != null, "assemblyName.Name != null");
        Tool = assemblyName.Name;
        Version = DateTime.UtcNow.ToString("O");

        AdditionalNamespaces = Events.Select(e => e.EventClassNamespace)
            .Where(n => n != null)
            .ToImmutableSortedSet()!;
    }
}
using System.Collections.Generic;

namespace OpenTemple.Core.GFX;

// An output of a display device (think: monitor)
public class DisplayDeviceOutput {
    // Technical id for use in a configuration or log file
    public string id;
    // Name to display to the end user
    public string name;
};

// A display device that can be used to render the game (think: GPU)
public class DisplayDevice {
    // Technical id for use in a configuration or log file
    public int id;
    // Name to display to the end user
    public string name;
    // Outputs associated with this display device
    public List<DisplayDeviceOutput> outputs = new List<DisplayDeviceOutput>();
};
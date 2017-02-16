
using System;

namespace Mapgenix.GSuite.Android
{
    /// <summary>
    /// Enumeration for the mode to process other overlays
    /// when doing interactive actions with InterativeOverlay.
    /// </summary>
    [Serializable]
    public enum ProcessOtherOverlaysMode
    {
        Default = 2,
        ProcessOtherOverlays = 0,
        DoNotProcessOtherOverlays = 1
    }
}

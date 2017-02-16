namespace Mapgenix.GSuite.Android

{
    /// <summary>
    /// Enumeration for the type of extent change
    /// when doing interactive actions with InteractiveOverlay.
    /// </summary>
    public enum ExtentChangedType
    {
        None = 0,
        Pan = 1,
        TrackZoomIn = 2,
        TrackZoomOut = 3,
        DoubleClickZoomIn = 4,
        DoubleClickZoomOut = 5,
        MouseWheelZoomIn = 6,
        MouseWheelZoomOut = 7,
    }
}

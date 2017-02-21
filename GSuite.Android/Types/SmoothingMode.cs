namespace Mapgenix.GSuite.Android.Types
{
    //
    // Resumen:
    //     Specifies whether smoothing (antialiasing) is applied to lines and curves and
    //     the edges of filled areas.
    public enum SmoothingMode
    {
        //
        // Resumen:
        //     Specifies an invalid mode.
        Invalid = -1,
        //
        // Resumen:
        //     Specifies no antialiasing.
        Default = 0,
        //
        // Resumen:
        //     Specifies no antialiasing.
        HighSpeed = 1,
        //
        // Resumen:
        //     Specifies antialiased rendering.
        HighQuality = 2,
        //
        // Resumen:
        //     Specifies no antialiasing.
        None = 3,
        //
        // Resumen:
        //     Specifies antialiased rendering.
        AntiAlias = 4
    }
}
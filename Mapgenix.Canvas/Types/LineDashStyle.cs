namespace Mapgenix.Canvas
{
    /// <summary>Styles of dashed lines.</summary>
    public enum LineDashStyle
    {
        /// <summary>Solid line.</summary>
        Solid = 0,
        /// <summary>User-defined custom dash style.</summary>
        Custom = 1,
        /// <summary>Line with repeating pattern of dash-dot.</summary>
        DashDot = 2,
        /// <summary>Line with repeating pattern of dash-dot-dot.</summary>
        DashDotDot = 3,
        /// <summary>Line consisting of dots.</summary>
        Dot = 4,
        /// <summary>Line consisting of dashes.</summary>
        Dash = 5
    }
}
namespace Mapgenix.Shapes
{
    /// <summary>Valid types of geometries.</summary>
    public enum WellKnownType
    {
        /// <summary>Invalid shape type.</summary>
        Invalid = 0,

        /// <summary>Geometry representing a single location in coordinate space.</summary>
        Point = 1,

        /// <summary>Curve with linear interpolation between points.</summary>
        Line = 2,

        /// <summary>Planar surface representing a polygon with a single inner ring and zero to multiple inner rings (holes).</summary>
        Polygon = 3,

        /// <summary>Geometry collection composed of Point elements.</summary>
        Multipoint = 4,

        /// <summary>Geometry collection composed of Line elements.</summary>
        Multiline = 5,

        /// <summary>Geometry composed of one or more Polygon elements.</summary>
        Multipolygon = 6,

        /// <summary>Collection composed of one or more Shape elements.</summary>
        GeometryCollection = 7
    }
}

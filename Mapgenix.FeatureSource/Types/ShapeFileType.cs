namespace Mapgenix.FeatureSource
{
    /// <summary>Types of shapefile.</summary>
    public enum ShapeFileType
    {
        Null = 0,
        Point = 1,
        Polyline = 3,
        Polygon = 5,
        Multipoint = 8,
        PointZ = 11,
        PolylineZ = 13,
        PolygonZ = 15,
        MultipointZ = 18,
        PointM = 21,
        PolylineM = 23,
        PolygonM = 25,
        MultipointM = 28,
        Multipatch = 31
    }
}

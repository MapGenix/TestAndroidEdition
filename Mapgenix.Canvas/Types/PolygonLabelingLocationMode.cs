namespace Mapgenix.Canvas
{
    /// <summary>Labeling Location Modes for labeling of polygon.</summary>
    public enum PolygonLabelingLocationMode
    {
        /// <summary>Centroid of polygon.</summary>
        Default = 0,
        /// <summary>Centroid of polygon.</summary>
        Centroid = 1,
        /// <summary>Boundingbox center of polygon.</summary>
        BoundingBoxCenter = 2
    }
}
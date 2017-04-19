namespace Mapgenix.FeatureSource 
{
    /// <summary>Types of spatial query is executed against a FeatureSource.</summary>
    public enum QueryType
    {
        /// <summary>The current shape and the targetShape have no points in common.</summary>
        Disjoint,

        /// <summary>The current shape and the targetShape have at least one point in common.</summary>
        Intersects,

        /// <summary>
        /// The current shape and the targetShape have at least one boundary point in common,
        /// but no interior points.
        /// </summary>
        Touches,

        /// <summary>
        /// The current shape and the targetShape share some but not all interior
        /// points.
        /// </summary>
        Crosses,

        /// <summary>The current shape lies within the interior of the targetShape.</summary>
        Within,

        /// <summary>The targetShape lies within the interior of the current shape.</summary>
        Contains,

        /// <summary>
        /// The current shape and the targetShape share some but not all points in
        /// common.
        /// </summary>
        Overlaps,

        /// <summary>The current shape and the target Shape are topologically equal.</summary>
        TopologicalEqual,
    }
}

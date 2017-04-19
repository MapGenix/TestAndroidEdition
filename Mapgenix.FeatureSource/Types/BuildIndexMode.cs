namespace Mapgenix.FeatureSource
{
    /// <summary>Modes whether an index should be rebuilt or not.</summary>
    /// <remarks>None</remarks>
    public enum BuildIndexMode
    {
        /// <summary>Do not rebuild the index if it exists.</summary>
        DoNotRebuild = 0,

        /// <summary>Rebuild the index if it exists.</summary>
        Rebuild = 1
    }
}

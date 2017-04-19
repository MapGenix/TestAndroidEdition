namespace Mapgenix.Canvas
{
    /// <summary>Spline types of labeling line based features.</summary>
    public enum SplineType
    {
        /// <summary>Default SplineType in LabelStyle.</summary>
        Default = 0,

        /// <summary>No spline. Better performance.</summary>
        None = 1,

        /// <summary>Standard splining. Recommended for latin based languages.</summary>
        StandardSplining = 2,

        /// <summary>Force splining. Recommned for character based languages such as Chinese or Japonese. </summary>
        ForceSplining = 3,
    }
}
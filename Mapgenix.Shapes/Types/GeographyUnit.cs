namespace Mapgenix.Shapes
{
    /// <summary>Units of the map</summary>
    /// <remarks>The geography unit represents how the map data is stored. It means that, for example, data stored in a geographic unit of meter, an increase of 100
    /// on the x-axis means moving 100 meters in the X axis. 
    /// If the data is projected it is in either meters or feet. If it is unprojected it is in decimal degrees.</remarks>
    public enum GeographyUnit
    {
        /// <summary>Unknown</summary>
        Unknown = 0,
        /// <summary>Decimal Degrees</summary>
        DecimalDegree = 1,
        /// <summary>Feet</summary>
        Feet = 2,
        /// <summary>Meters </summary>
        Meter = 3
    }
}

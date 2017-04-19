using System;

namespace Mapgenix.Layers
{
    /// <summary>Different types of Bing Maps</summary>
    [Flags]
    public enum BingMapsMapType : int
    {
        Aerial = 1,
        AerialWithLabels = 2,
        Road = 4,
        Traffic = 8 
    }
}
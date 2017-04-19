namespace Mapgenix.Canvas
{
    /// <summary>Drawing levels used in GeoCanvas.</summary>
    /// <remarks>Used to control the drawing level. 
    /// Items drawn on the first level are drawn below the items drawn on the second level etc. Label drawing is the very last level to draw.</remarks>
    public enum DrawingLevel
    {
        /// <summary>First drawing level.</summary>
        LevelOne = 0,
        /// <summary>Second drawing level.</summary>
        LevelTwo = 1,
        /// <summary>Third drawing level.</summary>
        LevelThree = 2,
        /// <summary>Fourth drawing level.</summary>
        LevelFour = 3,
        /// <summary>Label drawing level</summary>
        LabelLevel = 4,
    }
}
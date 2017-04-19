namespace Mapgenix.Canvas
{
    /// <summary>Cap styles for the end of lines.</summary>
    public enum DrawingLineCap
    {
        /// <summary>Round line cap.</summary>
        Round = 5,
        /// <summary>Mask used to check whether a line cap is an anchor cap.</summary>
        AnchorMask = 1,
        /// <summary>Arrow-shaped anchor cap.</summary>
        ArrowAnchor = 2,
        /// <summary>Custom line cap.</summary>
        Custom = 3,
        /// <summary>Diamond anchor cap.</summary>
        DiamondAnchor = 4,
        /// <summary>Flat line cap.</summary>
        Flat = 0,
        /// <summary>No anchor.</summary>
        NoAnchor = 6,
        /// <summary>Round anchor cap.</summary>
        RoundAnchor = 7,
        /// <summary>Square line cap.</summary>
        Square = 8,
        /// <summary>Square anchor line cap.</summary>
        SquareAnchor = 9,
        /// <summary>Triangular line cap.</summary>
        Triangle = 10
    }
}
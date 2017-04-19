namespace Mapgenix.Canvas
{
    /// <summary>Enumeration for the different labeling duplicate rules.</summary>
    /// <remarks>Duplication happens where labels with the same text are repeated on the map.
    /// A common case is for roads when each segment between two intersction is labeled being redundant.</remarks>
    public enum LabelDuplicateRule
    {
        /// <summary>Only one label with same text by quadrant.</summary>
        OneDuplicateLabelPerQuadrant = 0,
        /// <summary>Only one label with same text in the entire map.</summary>
        NoDuplicateLabels = 1,
        /// <summary>Duplicated labels with same text allowed.</summary>
        UnlimitedDuplicateLabels = 2
    }
}
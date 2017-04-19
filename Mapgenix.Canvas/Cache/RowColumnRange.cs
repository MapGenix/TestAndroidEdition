namespace Mapgenix.Canvas
{
    /// <summary>
    /// Describes a range of tiles for a given extent.
    /// </summary>
    public struct RowColumnRange
    {
        private long _minRowIndex;
        private long _maxRowIndex;
        private long _minColumnIndex;
        private long _maxColumnIndex;

        /// <summary>
        /// Contructor.
        /// </summary>
        /// <param name="minRowIndex">Min Row Index for the upper boundary.</param>
        /// <param name="maxRowIndex">Max Row Index for the lower boundary.</param>
        /// <param name="minColumnIndex">Min Column Index for the left boundary.</param>
        /// <param name="maxColumnIndex">Max Column Index for the right boundary.</param>
        public RowColumnRange(long minRowIndex, long maxRowIndex, long minColumnIndex, long maxColumnIndex)
        {
            _minRowIndex = minRowIndex;
            _maxRowIndex = maxRowIndex;
            _minColumnIndex = minColumnIndex;
            _maxColumnIndex = maxColumnIndex;
        }

        /// <summary>
        /// Gets or sets the Min Row Index for the upper boundary.
        /// </summary>
        public long MinRowIndex
        {
            get { return _minRowIndex; }
            set { _minRowIndex = value; }
        }

        /// <summary>
        /// Gets or sets the Max Row Index for the lower boundary.
        /// </summary>
        public long MaxRowIndex
        {
            get { return _maxRowIndex; }
            set { _maxRowIndex = value; }
        }

        /// <summary>
        /// Gets or sets the Min Column Index for the left boundary.
        /// </summary>
        public long MinColumnIndex
        {
            get { return _minColumnIndex; }
            set { _minColumnIndex = value; }
        }

        /// <summary>
        /// Gets or sets the Max Column Index for the right boundary.
        /// </summary>
        public long MaxColumnIndex
        {
            get { return _maxColumnIndex; }
            set { _maxColumnIndex = value; }
        }

        /// <summary>Overrides the GetHashCode functionality.</summary>
        /// <remarks>None</remarks>
        public override int GetHashCode()
        {
            return _minRowIndex.GetHashCode() ^ _maxRowIndex.GetHashCode() ^ _minColumnIndex.GetHashCode() ^
                   _maxColumnIndex.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (obj is RowColumnRange)
            {
                return Equals((RowColumnRange) obj);
            }
            return false;
        }

        private bool Equals(RowColumnRange compareObj)
        {
            var range = compareObj;
            return (_minRowIndex == range._minRowIndex) && (_maxRowIndex == range._maxRowIndex) &&
                   (_minColumnIndex == range._minColumnIndex) && (_maxColumnIndex == range._maxColumnIndex);
        }

        /// <summary>Overrides of the == functionality.</summary>
        /// <remarks>None</remarks>
        /// <param name="rowColumnRange1">First rowColumnRange to compare.</param>
        /// <param name="rowColumnRange2">Second rowColumnRange to compare.</param>
        public static bool operator ==(RowColumnRange rowColumnRange1, RowColumnRange rowColumnRange2)
        {
            return rowColumnRange1.Equals(rowColumnRange2);
        }

        /// <summary>Overrides of the != functionality.</summary>
        /// <remarks>None</remarks>
        /// <param name="rowColumnRange1">First rowColumnRange to compare.</param>
        /// <param name="rowColumnRange2">Second rowColumnRange to compare.</param>
        public static bool operator !=(RowColumnRange rowColumnRange1, RowColumnRange rowColumnRange2)
        {
            return !(rowColumnRange1 == rowColumnRange2);
        }
    }
}
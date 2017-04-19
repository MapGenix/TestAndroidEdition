using System;
using System.Globalization;
using System.Text;
using Mapgenix.Shapes;

namespace Mapgenix.Canvas
{
    /// <summary>
    ///Represents the TileMatrixCell used in the tile matrix system.
    /// </summary>
    [Serializable]
    public struct TileMatrixCell
    {
        private long _row;
        private long _column;
        private RectangleShape _boundingBox;

        /// <summary>Constructor to create a TileMatrixCell.</summary>
        /// <remarks>None</remarks>
        /// <param name="row">Row number of the TileMatrixCell.</param>
        /// <param name="column">Column number of the TileMatrixCell.</param>
        /// <param name="boundingBox">Bounding box of the TileMatrixCell.</param>
        public TileMatrixCell(long row, long column, RectangleShape boundingBox)
        {
            _row = row;
            _column = column;
            _boundingBox = boundingBox;
        }

        /// <summary>
        /// Gets or sets the row number.
        /// </summary>
        public long Row
        {
            get { return _row; }
            set { _row = value; }
        }

        /// <summary>
        /// Gets or sets the column number.
        /// </summary>
        public long Column
        {
            get { return _column; }
            set { _column = value; }
        }

        /// <summary>
        /// Gets or sets the bounding box.
        /// </summary>
        public RectangleShape BoundingBox
        {
            get { return _boundingBox; }
            set { _boundingBox = value; }
        }

        /// <summary>Override the ToString method.</summary>
        /// <remarks>None</remarks>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendFormat(CultureInfo.InvariantCulture, "Row:{0},", _row);
            sb.AppendFormat(CultureInfo.InvariantCulture, "Column:{0},", _column);
            sb.AppendFormat(CultureInfo.InvariantCulture, "BoundingBox: {0} {1} {2} {3}", _boundingBox.UpperLeftPoint.X,
                _boundingBox.UpperLeftPoint.Y, _boundingBox.LowerRightPoint.X, _boundingBox.LowerRightPoint.Y);

            return sb.ToString();
        }

        /// <summary>Overrides the GetHashCode method.</summary>
        /// <remarks>None</remarks>
        public override int GetHashCode()
        {
            return
                (_row + _column + _boundingBox.UpperLeftPoint.X + _boundingBox.UpperLeftPoint.Y +
                 _boundingBox.LowerRightPoint.X + _boundingBox.LowerRightPoint.Y).GetHashCode();
        }

        /// <summary>Overrides the Equals method.</summary>
        /// <remarks>None</remarks>
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (obj is TileMatrixCell)
            {
                return this == (TileMatrixCell) obj;
            }
            return false;
        }

        /// <summary>Overrides the == operator.</summary>
        /// <remarks>None</remarks>
        /// <param name="cell1">First TileMatrixCell to compare.</param>
        /// <param name="cell2">Second TileMatrixCell to compare.</param>
        public static bool operator ==(TileMatrixCell cell1, TileMatrixCell cell2)
        {
            return (cell1._row == cell2._row &&
                    cell1._column == cell2._column &&
                    cell1._boundingBox.UpperLeftPoint.X == cell2._boundingBox.UpperLeftPoint.X &&
                    cell1._boundingBox.UpperLeftPoint.Y == cell2._boundingBox.UpperLeftPoint.Y &&
                    cell1._boundingBox.LowerRightPoint.X == cell2._boundingBox.LowerRightPoint.X &&
                    cell1._boundingBox.LowerRightPoint.Y == cell2._boundingBox.LowerRightPoint.Y);
        }

        /// <summary>Overrides the != operator.</summary>
        /// <remarks>None</remarks>
        /// <param name="cell1">First TileMatrixCell to compare.</param>
        /// <param name="cell2">Second TileMatrixCell to compare.</param>
        public static bool operator !=(TileMatrixCell cell1, TileMatrixCell cell2)
        {
            return !(cell1 == cell2);
        }
    }
}
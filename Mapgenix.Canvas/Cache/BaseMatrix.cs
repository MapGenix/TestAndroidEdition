using System;
using System.Collections.ObjectModel;
using Mapgenix.Shapes;

namespace Mapgenix.Canvas
{
    /// <summary>
    /// Base class for the Matrix encapsulating the tile caculation logics.
    /// </summary>
    [Serializable]
    public abstract class BaseMatrix
    {
        private const int MaximumCellCount = 1000000;
        private const double Epsilon = 1e-6;
        private RectangleShape _boundingBox;

        private string _id;
        private MatrixReferenceCorner _referenceCorner;

        private PointShape _referencePoint;

        /// <summary>Default protected constructor.</summary>
        /// <returns>None</returns>
        /// <remarks>Matrix based on unprojected coordinate system.</remarks>
        protected BaseMatrix()
            : this(string.Empty, 360, 180, new RectangleShape(-180, 90, 180, -90), MatrixReferenceCorner.UpperLeft)
        {
        }

        /// <summary>Protected constructor.</summary>
        /// <param name="boundingBox">The bounding box of the Matrix.</param>
        /// <param name="cellHeight">The cell height(in world coordinates) of the Matrix.</param>
        /// <param name="cellWidth">The cell width(in world coordinates) of the Matrix.</param>
        /// <param name="id">The id of the Matrix.</param>
        /// <param name="referenceCorner">The reference corner of the Matrix.</param>
        /// <returns>None</returns>
        protected BaseMatrix(string id, double cellWidth, double cellHeight, RectangleShape boundingBox,
            MatrixReferenceCorner referenceCorner)
        {
            SetMatrix(id, cellWidth, cellHeight, boundingBox, referenceCorner);
        }

        /// <summary>Protected constructor.</summary>
        /// <param name="cellHeight">The cell height(in world coordinates) of the Matrix.</param>
        /// <param name="cellWidth">The cell width(in world coordinates) of the Matrix.</param>
        /// <param name="id">The id of the Matrix.</param>
        /// <param name="referencePoint">The reference point of the Matrix.</param>
        /// <param name="referenceCorner">The reference corner of the Matrix.</param>
        /// <param name="columnCount">The column count of the Matrix.</param>
        /// <param name="rowCount">The row count of the Matrix.</param>
        protected BaseMatrix(string id, double cellWidth, double cellHeight, PointShape referencePoint,
            MatrixReferenceCorner referenceCorner, long rowCount, long columnCount)
        {
            SetMatrix(id, cellWidth, cellHeight, referencePoint, referenceCorner, rowCount, columnCount);
        }

        /// <summary>
        /// The cell width of the Matrix.
        /// </summary>
       public double CellWidth { get; private set; }

       /// <summary>
       /// The cell height of the Matrix.
       /// </summary>
       public double CellHeight { get; private set; }

       /// <summary>
       /// Gets or sets the bounding box of the Matrix.
       /// </summary>
       /// <remarks>The bounding box of the Matrix is based on the 
       /// referencePoint, cellWidth, cellHeight and the rowCount and columnCount.</remarks>
        public RectangleShape BoundingBox
        {
            get
            {
                double minX, minY, maxX, maxY;
                switch (_referenceCorner)
                {
                    case MatrixReferenceCorner.UpperLeft:
                        minX = _referencePoint.X;
                        maxY = _referencePoint.Y;
                        maxX = minX + CellWidth*ColumnCount;
                        minY = maxY - CellHeight*RowCount;
                        break;
                    case MatrixReferenceCorner.UpperRight:
                        maxX = _referencePoint.X;
                        maxY = _referencePoint.Y;
                        minX = maxX - CellWidth*ColumnCount;
                        minY = maxY - CellHeight*RowCount;
                        break;
                    case MatrixReferenceCorner.LowerLeft:
                        minX = _referencePoint.X;
                        minY = _referencePoint.Y;
                        maxX = minX + CellWidth*ColumnCount;
                        maxY = minY + CellHeight*RowCount;
                        break;
                    case MatrixReferenceCorner.LowerRight:
                        maxX = _referencePoint.X;
                        minY = _referencePoint.Y;
                        minX = maxX - CellWidth*ColumnCount;
                        maxY = minY + CellHeight*RowCount;
                        break;
                    default:
                        minX = _referencePoint.X;
                        maxY = _referencePoint.Y;
                        maxX = minX + CellWidth*ColumnCount;
                        minY = maxY - CellHeight*RowCount;
                        break;
                }
                return new RectangleShape(minX, maxY, maxX, minY);
            }
            set
            {
                _boundingBox = value;
                SetMatrix(_id, CellWidth, CellHeight, _boundingBox, _referenceCorner);
            }
        }

        /// <summary>
        /// Gets the row count of the Matrix.
        /// </summary>
        public long RowCount { get; private set; }

        /// <summary>
        /// Gets the column count of the Matrix.
        /// </summary>
        public long ColumnCount { get; private set; }

        /// <summary>
        /// Gets or sets the id of the Matrix.
        /// </summary>
        public string Id
        {
            get { return _id; }
            set { _id = value; }
        }

        /// <summary>Returns all the cells of the tile matrix.</summary>
        /// <returns>Returns a collection of TileMatrixCell.</returns>
         public Collection<TileMatrixCell> GetAllCells()
        {
            if (RowCount*ColumnCount > MaximumCellCount)
            {
                throw new InvalidOperationException("Use GetCell");
            }

            var returnCells = new Collection<TileMatrixCell>();

            for (long i = 0; i < RowCount; i++)
            {
                for (long j = 0; j < ColumnCount; j++)
                {
                    returnCells.Add(GetCell(i, j));
                }
            }

            return returnCells;
        }

         /// <summary>
         /// Gets a cell based on a row and a column.
         /// </summary>
         /// <param name="row">The row based on 1.</param>
         /// <param name="column">The row based on 1.</param>
         /// <returns>A cell based on a row and a column.</returns>
        public TileMatrixCell GetCell(long row, long column)
        {
            var minX = BoundingBox.UpperLeftPoint.X + CellWidth*column;
            var maxY = BoundingBox.UpperLeftPoint.Y - CellHeight*row;
            var maxX = minX + CellWidth;
            var minY = maxY - CellHeight;

            var cellExtent = new RectangleShape(minX, maxY, maxX, minY);
            var returnCell = new TileMatrixCell(row, column, cellExtent);
            return returnCell;
        }

        /// <summary>
        /// Gets a cell based on a point location.
        /// </summary>
        /// <param name="intersectingPoint">The point location.</param>
        /// <returns>A cell based on a point location.</returns>
        public TileMatrixCell GetCell(PointShape intersectingPoint)
        {
            return GetCell(GetRowIndex(intersectingPoint), GetColumnIndex(intersectingPoint));
        }

        /// <summary>
        /// Gets a row index based on a point location.
        /// </summary>
        /// <param name="intersectingPoint">The point location.</param>
        /// <returns>A row index based on a point location.</returns>
        public long GetRowIndex(PointShape intersectingPoint)
        {
            Validators.CheckParameterIsNotNull(intersectingPoint, "intersectingPoint");

            var row =
                Convert.ToInt64(Math.Floor((BoundingBox.UpperLeftPoint.Y - intersectingPoint.Y)/CellHeight + Epsilon));
            return row;
        }

        /// <summary>
        /// Gets a column index based on a point location.
        /// </summary>
        /// <param name="intersectingPoint">The point location.</param>
        /// <returns>A column index based on a point location.</returns>
        public long GetColumnIndex(PointShape intersectingPoint)
        {
            Validators.CheckParameterIsNotNull(intersectingPoint, "intersectingPoint");

            var column =
                Convert.ToInt64(Math.Floor((intersectingPoint.X - BoundingBox.UpperLeftPoint.X)/CellWidth + Epsilon));
            return column;
        }

        /// <summary>Returns the intersecting cells of a TileMatrix.</summary>
        /// <param name="worldExtent">The extent to get the tiles from. </param>
        /// <returns>A collection of TileMatrixCell intersecting the extent passed in.</returns>
        public Collection<TileMatrixCell> GetIntersectingCells(RectangleShape worldExtent)
        {
            Validators.CheckParameterIsNotNull(worldExtent, "worldExtent");

            var minColumnIndex =
                Convert.ToInt64(
                    Math.Floor((worldExtent.UpperLeftPoint.X - BoundingBox.UpperLeftPoint.X)/CellWidth + Epsilon));
            var maxColumnIndex =
                Convert.ToInt64(
                    Math.Floor((worldExtent.LowerRightPoint.X - BoundingBox.UpperLeftPoint.X)/CellWidth - Epsilon));
            var minRowIndex =
                Convert.ToInt64(
                    Math.Floor((BoundingBox.UpperLeftPoint.Y - worldExtent.UpperLeftPoint.Y)/CellHeight + Epsilon));
            var maxRowIndex =
                Convert.ToInt64(
                    Math.Floor((BoundingBox.UpperLeftPoint.Y - worldExtent.LowerRightPoint.Y)/CellHeight - Epsilon));

            if (minColumnIndex < 0) minColumnIndex = 0;
            if (maxColumnIndex >= ColumnCount) maxColumnIndex = ColumnCount - 1;
            if (minRowIndex < 0) minRowIndex = 0;
            if (maxRowIndex >= RowCount) maxRowIndex = RowCount - 1;

            var gridRowCount = maxRowIndex - minRowIndex + 1;
            var gridColumnCount = maxColumnIndex - minColumnIndex + 1;

            if (gridRowCount*gridColumnCount > MaximumCellCount)
            {
                throw new InvalidOperationException("Use GetIntersectingRowColumnRange");
            }

            var returnCells = new Collection<TileMatrixCell>();
            for (var row = minRowIndex; row <= maxRowIndex; row++)
            {
                for (var column = minColumnIndex; column <= maxColumnIndex; column++)
                {
                    returnCells.Add(GetCell(row, column));
                }
            }

            return returnCells;
        }

        /// <summary>Returns the contained cells by a TileMatrix.</summary>
        /// <param name="worldExtent">The extent to get the tiles from. </param>
        /// <returns>A collection of TileMatrixCell intersecting the extent passed in.</returns>
        public Collection<TileMatrixCell> GetContainedCells(RectangleShape worldExtent)
        {
            Validators.CheckParameterIsNotNull(worldExtent, "worldExtent");

            var minColumnIndex =
                Convert.ToInt64(
                    Math.Ceiling((worldExtent.UpperLeftPoint.X - BoundingBox.UpperLeftPoint.X)/CellWidth - Epsilon));
            var maxColumnIndex =
                Convert.ToInt64(
                    Math.Floor((worldExtent.LowerRightPoint.X - BoundingBox.UpperLeftPoint.X)/CellWidth + Epsilon)) - 1;
            var minRowIndex =
                Convert.ToInt64(
                    Math.Ceiling((BoundingBox.UpperLeftPoint.Y - worldExtent.UpperLeftPoint.Y)/CellHeight - Epsilon));
            var maxRowIndex =
                Convert.ToInt64(
                    Math.Floor((BoundingBox.UpperLeftPoint.Y - worldExtent.LowerRightPoint.Y)/CellHeight + Epsilon)) - 1;

            if (minColumnIndex < 0) minColumnIndex = 0;
            if (maxColumnIndex >= ColumnCount) maxColumnIndex = ColumnCount - 1;
            if (minRowIndex < 0) minRowIndex = 0;
            if (maxRowIndex >= RowCount) maxRowIndex = RowCount - 1;

            var gridRowCount = maxRowIndex - minRowIndex + 1;
            var gridColumnCount = maxColumnIndex - minColumnIndex + 1;

            if (gridRowCount*gridColumnCount > MaximumCellCount)
            {
                throw new InvalidOperationException("Use GetContainedRowColumnRange");
            }

            var returnCells = new Collection<TileMatrixCell>();
            for (var row = minRowIndex; row <= maxRowIndex; row++)
            {
                for (var column = minColumnIndex; column <= maxColumnIndex; column++)
                {
                    returnCells.Add(GetCell(row, column));
                }
            }

            return returnCells;
        }

        /// <summary>Returns a RowColumnRange of a TileMatrix based on an extent</summary>
        /// <param name="worldExtent">The extent to get tiles from. </param>
        /// <returns>A RowColumnRange of a TileMatrix based on an extent.</returns>
        public RowColumnRange GetIntersectingRowColumnRange(RectangleShape worldExtent)
        {
            Validators.CheckParameterIsNotNull(worldExtent, "worldExtent");

            var minColumnIndex =
                Convert.ToInt64(
                    Math.Floor((worldExtent.UpperLeftPoint.X - BoundingBox.UpperLeftPoint.X)/CellWidth + Epsilon));
            var maxColumnIndex =
                Convert.ToInt64(
                    Math.Floor((worldExtent.LowerRightPoint.X - BoundingBox.UpperLeftPoint.X)/CellWidth - Epsilon));
            var minRowIndex =
                Convert.ToInt64(
                    Math.Floor((BoundingBox.UpperLeftPoint.Y - worldExtent.UpperLeftPoint.Y)/CellHeight + Epsilon));
            var maxRowIndex =
                Convert.ToInt64(
                    Math.Floor((BoundingBox.UpperLeftPoint.Y - worldExtent.LowerRightPoint.Y)/CellHeight - Epsilon));

            if (minColumnIndex < 0) minColumnIndex = 0;
            if (maxColumnIndex >= ColumnCount) maxColumnIndex = ColumnCount - 1;
            if (minRowIndex < 0) minRowIndex = 0;
            if (maxRowIndex >= RowCount) maxRowIndex = RowCount - 1;

            return new RowColumnRange(minRowIndex, maxRowIndex, minColumnIndex, maxColumnIndex);
        }

        /// <summary>Returns a RowColumnRange of a TileMatrix contained by an extent.</summary>
        /// <param name="worldExtent">The extent to get the tiles from. </param>
        /// <returns>A RowColumnRange of a TileMatrix contained by an extent.</returns>
        public RowColumnRange GetContainedRowColumnRange(RectangleShape worldExtent)
        {
            Validators.CheckParameterIsNotNull(worldExtent, "worldExtent");

            var minColumnIndex =
                Convert.ToInt64(
                    Math.Ceiling((worldExtent.UpperLeftPoint.X - BoundingBox.UpperLeftPoint.X)/CellWidth - Epsilon));
            var maxColumnIndex =
                Convert.ToInt64(
                    Math.Floor((worldExtent.LowerRightPoint.X - BoundingBox.UpperLeftPoint.X)/CellWidth + Epsilon)) - 1;
            var minRowIndex =
                Convert.ToInt64(
                    Math.Ceiling((BoundingBox.UpperLeftPoint.Y - worldExtent.UpperLeftPoint.Y)/CellHeight - Epsilon));
            var maxRowIndex =
                Convert.ToInt64(
                    Math.Floor((BoundingBox.UpperLeftPoint.Y - worldExtent.LowerRightPoint.Y)/CellHeight + Epsilon)) - 1;

            if (minColumnIndex < 0) minColumnIndex = 0;
            if (maxColumnIndex >= ColumnCount) maxColumnIndex = ColumnCount - 1;
            if (minRowIndex < 0) minRowIndex = 0;
            if (maxRowIndex >= RowCount) maxRowIndex = RowCount - 1;

            return new RowColumnRange(minRowIndex, maxRowIndex, minColumnIndex, maxColumnIndex);
        }

        /// <summary>
        /// Sets the Matrix system.
        /// </summary>
        /// <param name="id">The id of the Matrix.</param>
        /// <param name="cellWidth">The cell width(in world coordinates) to set the Matrix.</param>
        /// <param name="cellHeight">The cell height(in world coordinates) to set the Matrix.</param>
        /// <param name="boundingBox">The boundingBox to set the Matrix.</param>
        /// <param name="referenceCorner">The reference corner to set the Matrix.</param>
        /// <remarks>Based on those parameters, the reference Point, rowCount, columnCount can be caculated.</remarks>
        protected void SetMatrix(string id, double cellWidth, double cellHeight, RectangleShape boundingBox,
            MatrixReferenceCorner referenceCorner)
        {
            Validators.CheckParameterIsNotNull(boundingBox, "boundingBox");

            _id = id;
            CellWidth = cellWidth;
            CellHeight = cellHeight;
            _boundingBox = boundingBox;
            _referenceCorner = referenceCorner;

            RowCount = Convert.ToInt64(Math.Ceiling(boundingBox.Height/cellHeight - Epsilon));
            ColumnCount = Convert.ToInt64(Math.Ceiling(boundingBox.Width/cellWidth - Epsilon));
            if (RowCount <= 0) RowCount = 1;
            if (ColumnCount <= 0) ColumnCount = 1;

            switch (referenceCorner)
            {
                case MatrixReferenceCorner.UpperLeft:
                    _referencePoint = boundingBox.UpperLeftPoint;
                    break;
                case MatrixReferenceCorner.UpperRight:
                    _referencePoint = boundingBox.UpperRightPoint;
                    break;
                case MatrixReferenceCorner.LowerLeft:
                    _referencePoint = boundingBox.LowerLeftPoint;
                    break;
                case MatrixReferenceCorner.LowerRight:
                    _referencePoint = boundingBox.LowerRightPoint;
                    break;
                default:
                    _referencePoint = boundingBox.UpperLeftPoint;
                    break;
            }
        }

        /// <summary>
        /// Sets the Matrix system.
        /// </summary>
        /// <param name="id">The id of the Matrix.</param>
        /// <param name="cellWidth">The cell width(in world coordinates) to set the Matrix.</param>
        /// <param name="cellHeight">The cell height(in world coordinates) to set the Matrix.</param>
        /// <param name="referencePoint">The reference point to set the Matrix.</param>
        /// <param name="referenceCorner">The reference corner to set the Matrix.</param>
        /// <param name="rowCount">The row count to set the Matrix.</param>
        /// <param name="columnCount">The column count to set the Matrix.</param>
        protected void SetMatrix(string id, double cellWidth, double cellHeight, PointShape referencePoint,
            MatrixReferenceCorner referenceCorner, long rowCount, long columnCount)
        {
            _id = id;
            CellWidth = cellWidth;
            CellHeight = cellHeight;
            _referencePoint = referencePoint;
            _referenceCorner = referenceCorner;
            RowCount = rowCount;
            ColumnCount = columnCount;
        }

        /// <summary>
        /// Sets the Matrix system.
        /// </summary>
        /// <param name="cellWidth">The cell width(in world coordinates) to set the Matrix.</param>
        /// <param name="cellHeight">The cell height(in world coordinates) to set the Matrix.</param>
        protected void SetMatrix(double cellWidth, double cellHeight)
        {
            CellWidth = cellWidth;
            CellHeight = cellHeight;

            RowCount = Convert.ToInt64(Math.Ceiling(_boundingBox.Height/cellHeight - Epsilon));
            ColumnCount = Convert.ToInt64(Math.Ceiling(_boundingBox.Width/cellWidth - Epsilon));
            if (RowCount <= 0) RowCount = 1;
            if (ColumnCount <= 0) ColumnCount = 1;
        }
    }
}
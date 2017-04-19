using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Threading;
using Mapgenix.FeatureSource.Properties;
using Mapgenix.Shapes;
using Mapgenix.Utils;

namespace Mapgenix.FeatureSource
{
    /// <summary>Source for Grid file.</summary>
    /// <remarks>
    ///     The <strong>GridFeatureSource</strong> is raster GIS file format. 
    ///     The grid defines geographic space as an array of equally sized square grid points arranged in rows and columns. 
    ///     Each grid point stores a numeric value that represents a geographic attribute (such as elevation or surface slope) for that unit of space. 
    ///     Each grid cell is referenced by its x,y coordinate location.
    /// </remarks>
    [Serializable]
    public class GridFeatureSource : BaseFeatureSource
    {
        private const string CellValueName = "CellValue";

        private double _cellSize;
        private Collection<FeatureSourceColumn> _featureSourceColumns;
        private RectangleShape _gridExtent;
        private string _gridPathFilename;
        private double[,] _gridValues;
        private PointShape _lowerLeftPoint;
        private double _noDataValue;
        private int _numberOfColumns;
        private int _numberOfRows;
		public bool InitializedWithGridCellMatrix { get; set; }
        public GridCell[,] GridCellMatrix {get; set; }

        public event EventHandler<StreamLoadingEventArgs> StreamLoading;

        /// <summary>Gets and sets the path and file of the grid file to use.</summary>
        /// <returns>Path and file of the grid file to use.</returns>
        public string PathFilename
        {
            get { return _gridPathFilename; }
            set { _gridPathFilename = value; }
        }

        /// <summary>
        /// Gets the cell size of the grid.
        /// </summary>
        public double CellSize
        {
            get { return _cellSize; }
        }

        /// <summary> Gets the number of column in the grid.</summary>
        public int NumberOfColumns
        {
            get { return _numberOfColumns; }
        }

        /// <summary>Gets the number of row in the grid.</summary>
        public int NumberOfRows
        {
            get { return _numberOfRows; }
        }

        /// <summary>Gets LowerLeft PointShape of the grid.</summary>
        public PointShape LowerLeftPoint
        {
            get { return _lowerLeftPoint; }
        }

        /// <summary>Gets the NoDataValue in the grid.</summary>
        public double NoDataValue
        {
            get { return _noDataValue; }
        }

        /// <summary>Returns true if the FeatureSource allows edits or false if is read only.</summary>
        public override bool IsEditable
        {
            get { return false; }
        }


        public string DataValueColumnName
        {
            get { return CellValueName; }
        }

        /// <summary>Opens the GridFeatureSource to have it ready to use.</summary>
        protected override void OpenCore()
        {
            if (InitializedWithGridCellMatrix)
            {
                OpenWithGridCellMatrix();
            }
            else
            {
                OpenWithGridFile();
            }
        }

        protected virtual void OnStreamLoading(StreamLoadingEventArgs e)
        {
            EventHandler<StreamLoadingEventArgs> handler = StreamLoading;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>Closes the FeatureSource and releases any resources it was using.</summary>
        protected override void CloseCore()
        {
            _gridValues = null;
        }

        /// <summary>Returns the columns available for the GridFeatureSource.</summary>
        protected override Collection<FeatureSourceColumn> GetColumnsCore()
        {
            Validators.CheckFeatureSourceIsOpen(IsOpen);

            if (_featureSourceColumns == null)
            {
                _featureSourceColumns = new Collection<FeatureSourceColumn>();
                string name = CellValueName;
                int length = 50;
                string typeName = "String";

                FeatureSourceColumn tmp = new FeatureSourceColumn(name, typeName, length);
                _featureSourceColumns.Add(tmp);
            }

            return _featureSourceColumns;
        }

        protected override TransactionResult CommitTransactionCore(TransactionBuffer transactions)
        {
            throw new NotSupportedException(ExceptionDescription.NotSupported);
        }

        /// <summary>Returns a collection of Features in the GridFeatureSource.</summary>
        /// <returns>Collection of Features in the GridFeatureSource.</returns>
        protected override Collection<Feature> GetAllFeaturesCore(IEnumerable<string> returningColumnNames)
        {
            Validators.CheckFeatureSourceIsOpen(IsOpen);

            return GetFeaturesInsideBoundingBox(new RectangleShape(double.MinValue, double.MaxValue, double.MaxValue, double.MinValue), returningColumnNames);
        }

        /// <summary>Returns the bounding box encompassing all of the features in the GridFeatureSource.</summary>
        /// <returns>Bounding box encompassing all of the features in the GridFeatureSource.</returns>
        protected override RectangleShape GetBoundingBoxCore()
        {
            Validators.CheckFeatureSourceIsOpen(IsOpen);

            return _gridExtent;
        }

        /// <summary>Returns the number of records in the GridFeatureSource.</summary>
        /// <returns>The number of records in the GridFeatureSource.</returns>
        protected override int GetCountCore()
        {
            Validators.CheckFeatureSourceIsOpen(IsOpen);

            return _numberOfColumns * _numberOfRows;
        }

        /// <summary>Returns a collection of features of the GridFeatureSource inside of a bounding box.</summary>
        /// <returns>Collection of features of the GridFeatureSource inside of a bounding box.</returns>
        protected override Collection<Feature> GetFeaturesInsideBoundingBoxCore(RectangleShape boundingBox, IEnumerable<string> returningColumnNames)
        {
            Validators.CheckParameterIsNotNull(boundingBox, "boundingBox");

            Collection<Feature> featuresInsideBoundingBox = new Collection<Feature>();

            if (boundingBox.Intersects(_gridExtent))
            {
                double upperLeftX = Math.Max(_gridExtent.UpperLeftPoint.X, boundingBox.UpperLeftPoint.X);
                double upperLeftY = Math.Min(_gridExtent.UpperLeftPoint.Y, boundingBox.UpperLeftPoint.Y);
                double lowerRightX = Math.Min(_gridExtent.LowerRightPoint.X, boundingBox.LowerRightPoint.X);
                double lowerRightY = Math.Max(_gridExtent.LowerRightPoint.Y, boundingBox.LowerRightPoint.Y);

                int startColumnIndex = Convert.ToInt32(Math.Floor(Math.Round((upperLeftX - _gridExtent.UpperLeftPoint.X) / _cellSize, 8)));
                int endColumnIndex = Convert.ToInt32(Math.Ceiling(Math.Round((lowerRightX - _gridExtent.UpperLeftPoint.X) / _cellSize, 8)));
                int startRowIndex = Convert.ToInt32(Math.Floor(Math.Round((_gridExtent.UpperLeftPoint.Y - upperLeftY) / _cellSize, 8)));
                int endRowIndex = Convert.ToInt32(Math.Ceiling(Math.Round((_gridExtent.UpperLeftPoint.Y - lowerRightY) / _cellSize, 8)));

                Dictionary<string, string> dictionary = null;
                for (int i = startRowIndex; i < endRowIndex; i++)
                {
                    for (int j = startColumnIndex; j < endColumnIndex; j++)
                    {
                        dictionary = new Dictionary<string, string>();
                        double cellValue = _gridValues[i, j];
                        dictionary.Add(CellValueName, cellValue.ToString(CultureInfo.InvariantCulture));
                        RectangleShape gridRectangle = new RectangleShape(_lowerLeftPoint.X + j * _cellSize,
                                                                          _lowerLeftPoint.Y + _numberOfRows * _cellSize - i * _cellSize,
                                                                          _lowerLeftPoint.X + (j + 1) * _cellSize,
                                                                          _lowerLeftPoint.Y + _numberOfRows * _cellSize - (i + 1) * _cellSize);
                        gridRectangle.Id = string.Format(CultureInfo.InvariantCulture, "{0} : {1}", i, j);
                        Feature feature = new Feature(gridRectangle, dictionary);
                        featuresInsideBoundingBox.Add(feature);
                    }
                }
            }

            return featuresInsideBoundingBox;
        }

        private static string[] SplitStringBySpace(string stringToSplit)
        {
            char[] charSpace = new char[1] { ' ' };
            return stringToSplit.Split(charSpace, StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>Calculates all the cells in the grid and returns the result in the form of a GridCell matrix.</summary>
        /// <param name="gridDefinition">Grid definition.</param>
        /// <param name="gridInterpolationModel">Interpolation model for calculating cell value of the grid</param>
        /// <returns>GridCell matrix which is calculated based on the input parameters and the interpolation model.</returns>
        public static GridCell[,] GenerateGrid(GridDefinition gridDefinition, BaseGridInterpolationModel gridInterpolationModel)
        {
            Validators.CheckParameterIsNotNull(gridDefinition, "gridDefinition");
            Validators.CheckParameterIsNotNull(gridInterpolationModel, "gridInterpolationModel");

            int columnNumber = Convert.ToInt32(Math.Ceiling(Math.Round((gridDefinition.GridExtent.LowerRightPoint.X - gridDefinition.GridExtent.UpperLeftPoint.X) / gridDefinition.CellSize, 8)));
            int rowNumber = Convert.ToInt32(Math.Ceiling(Math.Round((gridDefinition.GridExtent.UpperLeftPoint.Y - gridDefinition.GridExtent.LowerRightPoint.Y) / gridDefinition.CellSize, 8)));
            GridCell[,] points = new GridCell[rowNumber, columnNumber];
            ManualResetEvent finished = new ManualResetEvent(false);
         
            CellValueCalculator cellValueCalculator = new CellValueCalculator();
            cellValueCalculator.CellsToCalculate = rowNumber * columnNumber;

            for (int y = 0; y < rowNumber; y++)
            {
                for (int x = 0; x < columnNumber; x++)
                {
                    double cellULx = gridDefinition.GridExtent.UpperLeftPoint.X + x * gridDefinition.CellSize;
                    double cellULy = gridDefinition.GridExtent.UpperLeftPoint.Y - y * gridDefinition.CellSize;
                    double cellLRx = cellULx + gridDefinition.CellSize;
                    double cellLRy = cellULy - gridDefinition.CellSize;
                    CalculateGridCellStateInfo state = new CalculateGridCellStateInfo(x, y, new RectangleShape(cellULx, cellULy, cellLRx, cellLRy), points, gridInterpolationModel, gridDefinition, finished);
                    ThreadPool.QueueUserWorkItem(cellValueCalculator.CalculatGridCell, state);
                }
            }
            finished.WaitOne();
            return points;
        }

        /// <summary>Generates the cell matrix based on number of column and number of row of current grid.</summary>
        /// <returns>Cell matrix of current grid.</returns>
        public GridCell[,] GenerateGrid()
        {
            int collumnNumber = _numberOfColumns;
            int rowNumber = _numberOfRows;

            GridCell[,] points = new GridCell[rowNumber, collumnNumber];

            for (int y = 0; y <= rowNumber - 1; y++)
            {
                for (int x = 0; x <= collumnNumber - 1; x++)
                {
                    double centerX = _gridExtent.LowerLeftPoint.X + x * _cellSize + _cellSize / 2;
                    double centerY = _gridExtent.LowerLeftPoint.Y + (rowNumber - y) * _cellSize - _cellSize / 2;
                    points[y, x] = new GridCell(centerX, centerY, _gridValues[y, x]);
                }
            }

            return points;
        }

        /// <summary>Calculates all the cells in the grid and returns the result in the form of a GridCell matrix.</summary>
        /// <param name="gridDefinition">Grid definition.</param>
        /// <param name="gridInterpolationModel">Interpolation model for calculating cell value of the grid</param>
        /// <param name="outputStream">Stream associated with the output file.</param>
        /// <returns>GridCell matrix which is calculated based on the input parameters and the interpolation model.</returns>
        public static void GenerateGrid(GridDefinition gridDefinition, BaseGridInterpolationModel gridInterpolationModel, Stream outputStream)
        {
            Validators.CheckParameterIsNotNull(gridDefinition, "gridDefinition");
            Validators.CheckParameterIsNotNull(gridInterpolationModel, "gridInterpolationModel");
            Validators.CheckParameterIsNotNull(outputStream, "outputStream");
            Validators.CheckStreamWritable(outputStream, "outputStream");

            GridCell[,] cells = GenerateGrid(gridDefinition, gridInterpolationModel);

            int columnNumber = Convert.ToInt32(Math.Ceiling(Math.Round((gridDefinition.GridExtent.LowerRightPoint.X - gridDefinition.GridExtent.UpperLeftPoint.X) / gridDefinition.CellSize, 8)));
            int rowNumber = Convert.ToInt32(Math.Ceiling(Math.Round((gridDefinition.GridExtent.UpperLeftPoint.Y - gridDefinition.GridExtent.LowerRightPoint.Y) / gridDefinition.CellSize, 8)));

            using (StreamWriter streamWriter = new StreamWriter(outputStream))
            {
                streamWriter.WriteLine("ncols {0}", columnNumber);

                streamWriter.WriteLine("nrows {0}", rowNumber);

                streamWriter.WriteLine("xllcorner {0}", gridDefinition.GridExtent.UpperLeftPoint.X);

                streamWriter.WriteLine("yllcorner {0}", gridDefinition.GridExtent.LowerRightPoint.Y);

                streamWriter.WriteLine("cellsize {0}", gridDefinition.CellSize);

                streamWriter.WriteLine("NODATA_Value {0}", gridDefinition.NoDataValue);

                for (int row = 0; row < rowNumber; row++)
                {
                    for (int col = 0; col < columnNumber; col++)
                    {
                        if (col != columnNumber - 1)
                        {
                            streamWriter.Write("{0} ", cells[row, col].Value);
                        }
                        else
                            streamWriter.Write(cells[row, col].Value);
                    }
                    streamWriter.WriteLine();
                }
                streamWriter.Flush();
            }
        }

        private void OpenWithGridCellMatrix()
        {
            Validators.CheckParameterIsNotNull(GridCellMatrix, "gridCellMatrix");

            _numberOfColumns = GridCellMatrix.GetLength(1);
            _numberOfRows = GridCellMatrix.GetLength(0);

            _gridValues = new double[GridCellMatrix.GetLength(0), GridCellMatrix.GetLength(1)];

            for (int i = 0; i < GridCellMatrix.GetLength(0); i++)
            {
                for (int j = 0; j < GridCellMatrix.GetLength(1); j++)
                {
                    _gridValues[i, j] = GridCellMatrix[i, j].Value;
                }
            }

            _cellSize = GridCellMatrix[0, 0].CenterY - GridCellMatrix[1, 0].CenterY;

            _noDataValue = -9999;

            double minX = GridCellMatrix[0, 0].CenterX - _cellSize / 2;
            double maxY = GridCellMatrix[0, 0].CenterY + _cellSize / 2;
            double maxX = GridCellMatrix[0, 0].CenterX - _cellSize / 2 + _cellSize * NumberOfColumns;
            double minY = GridCellMatrix[0, 0].CenterY + _cellSize / 2 - _cellSize * _numberOfRows;
            _gridExtent = new RectangleShape(minX, maxY, maxX, minY);

            _lowerLeftPoint = new PointShape(minX, minY);
        }

        private void OpenWithGridFile()
        {
            Validators.CheckParameterIsNotNullOrEmpty(_gridPathFilename, "PathFilename");

            Stream stream = null;
            if (File.Exists(PathFilename))
            {
                stream = new FileStream(PathFilename, FileMode.Open, FileAccess.Read);
            }
            else if (StreamLoading != null)
            {
                StreamLoadingEventArgs args = new StreamLoadingEventArgs(PathFilename, "Grid File");
                OnStreamLoading(args);
                stream = args.Stream;
            }

            StreamReader streamReader = null;

            try
            {
                streamReader = new StreamReader(stream);

                string[] lineString = SplitStringBySpace(streamReader.ReadLine());
                _numberOfColumns = Convert.ToInt32(lineString[1], CultureInfo.InvariantCulture);

                lineString = SplitStringBySpace(streamReader.ReadLine());
                _numberOfRows = Convert.ToInt32(lineString[1], CultureInfo.InvariantCulture);

                _lowerLeftPoint = new PointShape();
                lineString = SplitStringBySpace(streamReader.ReadLine());
                _lowerLeftPoint.X = Convert.ToDouble(lineString[1], CultureInfo.InvariantCulture);
                lineString = SplitStringBySpace(streamReader.ReadLine());
                _lowerLeftPoint.Y = Convert.ToDouble(lineString[1], CultureInfo.InvariantCulture);

                lineString = SplitStringBySpace(streamReader.ReadLine());
                _cellSize = Convert.ToDouble(lineString[1], CultureInfo.InvariantCulture);

                lineString = SplitStringBySpace(streamReader.ReadLine());
                _noDataValue = Convert.ToDouble(lineString[1], CultureInfo.InvariantCulture);

                _gridExtent = new RectangleShape(_lowerLeftPoint.X, _lowerLeftPoint.Y + _numberOfRows * _cellSize, _lowerLeftPoint.X + _numberOfColumns * _cellSize, _lowerLeftPoint.Y);
                _gridValues = new double[_numberOfRows, _numberOfColumns];
                for (int i = 0; i < _numberOfRows; i++)
                {
                    string line = streamReader.ReadLine();
                    lineString = SplitStringBySpace(line);
                    for (int j = 0; j < _numberOfColumns; j++)
                    {
                        _gridValues[i, j] = Convert.ToDouble(lineString[j], CultureInfo.InvariantCulture);
                    }
                }
            }
            finally
            {
                streamReader.Close();
            }
        }
    }
}

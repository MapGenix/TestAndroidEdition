using System;
using System.IO;
using Mapgenix.FeatureSource;
using Mapgenix.Shapes;
using Mapgenix.Utils;

namespace Mapgenix.Layers
{
    /// <summary>Layer for Grid file.</summary>
    [Serializable]
    public class GridFeatureLayer : BaseFeatureLayer
    {
        /// <summary>Gets and sets the path and filename of the grid file.</summary>
        /// <returns>Path and filename of the grid file.</returns>
        /// <remarks>Needs to be the complete path and filename of the grid file.</remarks>
        public string PathFilename
        {
            get { return ((GridFeatureSource)FeatureSource).PathFilename; }
            set { ((GridFeatureSource)FeatureSource).PathFilename = value; }
        }

        /// <summary>Gets the cell size of the grid in world coordinates.</summary>
        public double CellSize
        {
            get { return ((GridFeatureSource)FeatureSource).CellSize; }
        }

        /// <summary>Gets the column number of the grid.</summary>
        public int NumberOfColumns
        {
            get { return ((GridFeatureSource)FeatureSource).NumberOfColumns; }
        }

        /// <summary>Gets the row number of the grid.</summary>
        public int NumberOfRows
        {
            get { return ((GridFeatureSource)FeatureSource).NumberOfRows; }
        }

        /// <summary>Gets the NoDataValue from the grid.</summary>
        public double NoDataValue
        {
            get { return ((GridFeatureSource)FeatureSource).NoDataValue; }
        }

  
        public string DataValueColumnName
        {
            get { return ((GridFeatureSource)FeatureSource).DataValueColumnName; }
        }

        /// <summary>Returns the bounding box of the grid.</summary>
        /// <returns>Bounding box of the grid.</returns>
        /// <remarks>Called from concrete public method GetBoundingBox.</remarks>
        protected override RectangleShape GetBoundingBoxCore()
        {
            Validators.CheckFeatureSourceIsOpen(IsOpen);

            return ((GridFeatureSource)FeatureSource).GetBoundingBox();
        }

        /// <summary>Generates a grid based on grid definition and interpolation model.</summary>
        public static void GenerateGrid(GridDefinition gridDefinition, BaseGridInterpolationModel gridInterpolationModel, Stream outputStream)
        {
            Validators.CheckParameterIsNotNull(gridDefinition, "gridDefinition");
            Validators.CheckParameterIsNotNull(gridInterpolationModel, "gridInterpolationModel");
            Validators.CheckParameterIsNotNull(outputStream, "outputStream");
            Validators.CheckStreamWritable(outputStream, "outputStream");

            GridFeatureSource.GenerateGrid(gridDefinition, gridInterpolationModel, outputStream);
        }

        /// <summary>Generates a grid based on grid definition and interpolation model.</summary>
        public static GridCell[,] GenerateGrid(GridDefinition gridDefinition, BaseGridInterpolationModel gridInterpolationModel)
        {
            Validators.CheckParameterIsNotNull(gridDefinition, "gridDefinition");
            Validators.CheckParameterIsNotNull(gridInterpolationModel, "gridInterpolationModel");

            return GridFeatureSource.GenerateGrid(gridDefinition, gridInterpolationModel);
        }

        /// <summary>Generates a grid.</summary>
        public GridCell[,] GenerateGrid()
        {
            return ((GridFeatureSource)FeatureSource).GenerateGrid();
        }

        public event EventHandler<StreamLoadingEventArgs> StreamLoading
        {
            add
            {
                ((GridFeatureSource)FeatureSource).StreamLoading += value;
            }
            remove
            {
                ((GridFeatureSource)FeatureSource).StreamLoading -= value;
            }
        }
    }
}
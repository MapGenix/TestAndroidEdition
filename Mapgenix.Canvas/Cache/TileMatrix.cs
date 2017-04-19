using System;
using System.Globalization;
using Mapgenix.Shapes;


namespace Mapgenix.Canvas
{
    /// <summary>
    /// Base class inheriting from BaseMatrix. describes the matrix system used for Tiling.
    /// </summary>
    [Serializable]
    public class TileMatrix : BaseMatrix
    {
        private const int DotsPerInch = 96;
        private GeographyUnit _boundingBoxUnit;
        private double _scale;
        private int _tileHeight;
        private int _tileWidth;

        /// <summary>Overloaded constructor.</summary>
        /// <returns>None</returns>
        /// <param name="scale">Target scale to set for the Matrix.</param>
        public TileMatrix(double scale)
            : this(
                scale.ToString(CultureInfo.InvariantCulture), scale, 256, 256, GeographyUnit.DecimalDegree,
                new RectangleShape(-180, 90, 180, -90))
        {
        }

       /// <summary>Overloaded constructor.</summary>
       /// <returns>None</returns>
       /// <param name="scale">Target scale to set for the Matrix.</param>
       /// <param name="tileHeight">Tile height(in world coordinates) to set for the Matrix.</param>
       /// <param name="tileWidth">Tile width(in world coordinates) to set for the Matrix.</param>
       /// <param name="boundingBox">Bounding box to set for the Matrix.</param>
       public TileMatrix(double scale, int tileWidth, int tileHeight, GeographyUnit boundingBoxUnit)
            : this(
                scale.ToString(CultureInfo.InvariantCulture), scale, tileWidth, tileHeight, boundingBoxUnit,
                new RectangleShape(-180, 90, 180, -90))
        {
            if (boundingBoxUnit != GeographyUnit.DecimalDegree)
            {
                BoundingBox = new RectangleShape(-1000000000, 1000000000, 1000000000, -1000000000);
            }
        }

        /// <summary>Overloaded constructor.</summary>
        /// <returns>None</returns>
        /// <param name="id">Id to set the Matrix.</param>
        /// <param name="boundingBoxUnit">Bounding box Unit to set for the Matrix.</param>
        /// <param name="scale">Target scale to set for the Matrix.</param>
        /// <param name="tileHeight">Tile height(in world coordinates) to set for the Matrix.</param>
        /// <param name="tileWidth">Tile width(in world coordinates) to set for the Matrix.</param>
        /// <param name="boundingBox">Bounding box to set for the Matrix.</param>
        public TileMatrix(string id, double scale, int tileWidth, int tileHeight, GeographyUnit boundingBoxUnit,
            RectangleShape boundingBox)
            : this(id, scale, tileWidth, tileHeight, boundingBoxUnit, boundingBox, MatrixReferenceCorner.LowerLeft)
        {
        }

       
        /// <summary>Overloaded constructor.</summary>
        /// <returns>None</returns>
        /// <param name="id">Id to set the Matrix.</param>
        /// <param name="boundingBoxUnit">Bounding box Unit to set for the Matrix.</param>
        /// <param name="scale">Target scale to set for the Matrix.</param>
        /// <param name="tileHeight">Tile height(in world coordinates) to set for the Matrix.</param>
        /// <param name="tileWidth">Tile width(in world coordinates) to set for the Matrix.</param>
        /// <param name="boundingBox">Bounding box to set for the Matrix.</param>
        /// <param name="referenceCorner">Reference corner to set for the Matrix.</param>
        protected TileMatrix(string id, double scale, int tileWidth, int tileHeight, GeographyUnit boundingBoxUnit,
            RectangleShape boundingBox, MatrixReferenceCorner referenceCorner)
        {
            _scale = scale;
            _tileWidth = tileWidth;
            _tileHeight = tileHeight;
            _boundingBoxUnit = boundingBoxUnit;

            var resolution = GetResolutionFromScale(scale);
            var cellWidth = tileWidth*resolution;
            var cellHeight = tileHeight*resolution;
            SetMatrix(id, cellWidth, cellHeight, boundingBox, referenceCorner);
        }

        /// <summary>
        /// Gets or sets the scale for the TileMatrix.
        /// </summary>
        /// <remarks> When setting a different scale, the parameters in TileMatrix are recalculated.</remarks>
        public double Scale
        {
            get { return _scale; }
            set
            {
                _scale = value;
                CalculateMatrix();
            }
        }

        /// <summary>
        /// Gets or sets the tile width for the TileMatrix.
        /// </summary>
        /// <remarks> When setting a different tile width, the parameters in TileMatrix are recalculated.</remarks>
        public int TileWidth
        {
            get { return _tileWidth; }
            set
            {
                _tileWidth = value;

                CalculateMatrix();
            }
        }

        /// <summary>
        /// Gets or sets the tile height for the TileMatrix.
        /// </summary>
        /// <remarks> When setting a different tile height, the parameters in TileMatrix are recalculated.</remarks>
        public int TileHeight
        {
            get { return _tileHeight; }
            set
            {
                _tileHeight = value;
                CalculateMatrix();
            }
        }

        /// <summary>
        /// Gets or sets the BoundingBoxUnit for the TileMatrix.
        /// </summary>
        /// <remarks> When setting a different BoundingBoxUnit, the parameters in TileMatrix are recalculated.
        /// The default boundingBox value depends on the BoundingBoxUnit.</remarks>
        public GeographyUnit BoundingBoxUnit
        {
            get { return _boundingBoxUnit; }
            set
            {
                _boundingBoxUnit = value;

                if (_boundingBoxUnit == GeographyUnit.DecimalDegree)
                {
                    BoundingBox = new RectangleShape(-180, 90, 180, -90);
                }
                else
                {
                    BoundingBox = new RectangleShape(-1000000000, 1000000000, 1000000000, -1000000000);
                }

                CalculateMatrix();
            }
        }

        /// <summary>
        /// Gets resolution based on a scale.
        /// </summary>
        /// <param name="scale">Target scale to get resolution from.</param>
        /// <returns>A resolution based on the scale passed.</returns>
        protected double GetResolutionFromScale(double scale)
        {
            return scale/(GetInchesPerUnit(BoundingBoxUnit)*DotsPerInch);
        }

        private static double GetInchesPerUnit(GeographyUnit mapUnit)
        {
            double inchesPerUnit = 0;

            switch (mapUnit)
            {
                case GeographyUnit.DecimalDegree:
                    inchesPerUnit = 4374754;
                    break;
                case GeographyUnit.Feet:
                    inchesPerUnit = 12;
                    break;
                case GeographyUnit.Meter:
                    inchesPerUnit = 39.3701;
                    break;
            }

            return inchesPerUnit;
        }

        private void CalculateMatrix()
        {
            var resolution = GetResolutionFromScale(_scale);
            var cellWidth = _tileWidth*resolution;
            var cellHeight = _tileHeight*resolution;
            SetMatrix(cellWidth, cellHeight);
        }
    }
}
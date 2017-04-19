using System;
using Mapgenix.Utils;

namespace Mapgenix.Canvas
{
    /// <summary>Represents the drawing of a rectangle in screen coordinates.</summary>
    [Serializable]
    public struct DrawingRectangleF
    {
        private readonly float _centerX;
        private readonly float _centerY;
        private readonly float _width;
        private readonly float _height;

        /// <summary>Constructor</summary>
        /// <param name="centerX">Center horizontal value of the rectangle.</param>
        /// <param name="centerY">Center vertical value of the rectangle.</param>
        /// <param name="width">Width of the rectangle.</param>
        /// <param name="height">Height of the rectangle.</param>
        public DrawingRectangleF(float centerX, float centerY, float width, float height)
        {
            Validators.CheckIfInputValueIsBiggerThan(width, "width", 0, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckIfInputValueIsBiggerThan(height, "height", 0, RangeCheckingInclusion.ExcludeValue);
            _centerX = centerX;
            _centerY = centerY;
            _width = width;
            _height = height;
        }

        /// <summary>Center in X value (horizontal) of the rectangle.</summary>
        /// <value>Center in X value (horizontal)of the rectangle.</value>
        /// <remarks>None</remarks>
        public float CenterX
        {
            get { return _centerX; }
        }

        /// <summary>Center in Y value (vertical) of the rectangle.</summary>
        /// <value>Center in Y value (vertical)of the rectangle.</value>
        public float CenterY
        {
            get { return _centerY; }
        }

        /// <summary>Width of the rectangle.</summary>
        /// <value>Width of the rectangle.</value>
        /// <remarks>None</remarks>
        public float Width
        {
            get { return _width; }
        }

        /// <summary>Height of the rectangle.</summary>
        /// <value>Height of the rectangle.</value>
        /// <remarks>None</remarks>
        public float Height
        {
            get { return _height; }
        }

        /// <summary>Overrides of the == operator.</summary>
        /// <remarks>None</remarks>
        /// <param name="rectangleF1">First DrawingRectangleF to compare.</param>
        /// <param name="rectangleF2">Second DrawingRectangleF to compare.</param>
        public static bool operator ==(DrawingRectangleF rectangleF1, DrawingRectangleF rectangleF2)
        {
            return (rectangleF1.Equals(rectangleF2));
        }

        /// <summary>Overrides of the != operator.</summary>
        /// <remarks>None</remarks>
        /// <param name="rectangleF1">First DrawingRectangleF to compare.</param>
        /// <param name="rectangleF2">Second DrawingRectangleF to compare.</param>
        public static bool operator !=(DrawingRectangleF rectangleF1, DrawingRectangleF rectangleF2)
        {
            return !(rectangleF1.Equals(rectangleF2));
        }

        /// <summary>Overrides of the Equals method.</summary>
        /// <remarks>None</remarks>
        public override bool Equals(object obj)
        {
            if (!(obj is DrawingRectangleF)) return false;

            return Equals((DrawingRectangleF) obj);
        }

        private bool Equals(DrawingRectangleF obj)
        {
            if (_centerX != obj._centerX)
            {
                return false;
            }
            if (_centerY != obj._centerY)
            {
                return false;
            }
            if (_width != obj._width)
            {
                return false;
            }
            if (_height != obj._height)
            {
                return false;
            }

            return true;
        }

        /// <summary>Overrides of GetHashCode method.</summary>
        /// <remarks>None</remarks>
        public override int GetHashCode()
        {
            return _centerX.GetHashCode() ^ _centerY.GetHashCode() ^ _width.GetHashCode() ^ _height.GetHashCode();
        }
    }
}
using System;

namespace Mapgenix.Canvas
{
    /// <summary>A <strong>BaseGeoBrush</strong> with a linear gradient.</summary>
    /// <remarks>
    /// 	<para>Encapsulates both two-color gradients and custom multicolor gradients.</para>
    /// 	<para></para>
    /// 	<para>All linear gradients are defined along a line specified either by the width of a rectangle or by two points.</para>
    /// 	<para></para>
    /// 	<para>By default, a two-color linear gradient is an even horizontal linear blend from the starting color to the ending color along the specified line.</para>
    /// </remarks>
    [Serializable]
    public class GeoLinearGradientBrush : BaseGeoBrush
    {
        private float _angle;
        private GeoColor _endColor;
        private GeoColor _startColor;
        private GeoWrapMode _wrapMode;

        /// <summary>Creates an instance of GeoLinearGradientBrush with default settings.</summary>
        protected GeoLinearGradientBrush()
            : this(GeoColor.StandardColors.Black, GeoColor.StandardColors.White, (float) 0)
        {
        }

        /// <summary> Creates an instance of GeoLinearGradientBrush with a gradient start color and 
        /// gradient end color followed by the gradient direction enum.</summary>
        /// <param name="startColor">Starting <strong>GeoColor</strong> of the <strong>GeoLinearGradientBrush</strong>.</param>
        /// <param name="endColor">Ending <strong>GeoColor</strong> of the<strong>GeoLinearGradientBrush</strong>.</param>
        /// <param name="direction"><strong>GeoLinearGradientDirection</strong> enumeration of the <strong>GeoLinearGradientBrush</strong>.</param>
        public GeoLinearGradientBrush(GeoColor startColor, GeoColor endColor, GeoLinearGradientDirection direction)
            : this(startColor, endColor, (float) 0)
        {
            _angle = ConvertDirectionToAngle(direction);
        }

        /// <summary>Creates an instance of GeoLinearGradientBrush with a gradient start color and 
        /// gradient end color followed by the gradient direction angle.</summary>
        /// <param name="startColor">Starting <strong>GeoColor</strong> of the <strong>GeoLinearGradientBrush</strong>.</param>
        /// <param name="endColor">Ending <strong>GeoColor</strong> of the <strong>GeoLinearGradientBrush</strong>.</param>
        /// <param name="directionAngle">Direction angle value of the <strong>GeoLinearGradientBrush</strong>.</param>
        public GeoLinearGradientBrush(GeoColor startColor, GeoColor endColor, float directionAngle)
        {
            _startColor = startColor;
            _endColor = endColor;
            _angle = directionAngle;
        }

        /// <summary>Gets or sets the starting <strong>GeoColor</strong> of the gradient.</summary>
        public GeoColor StartColor
        {
            get { return _startColor; }
            set { _startColor = value; }
        }

        /// <summary>Gets or sets the ending <strong>GeoColor</strong> of the gradient.</summary>
        public GeoColor EndColor
        {
            get { return _endColor; }
            set { _endColor = value; }
        }

        /// <summary>Gets or sets the direction angle of the <strong>GeoLinearGradientBrush</strong>.</summary>
        public float DirectionAngle
        {
            get { return _angle; }
            set { _angle = value; }
        }

        /// <summary>Gets or sets a <strong>GeoWrapMode</strong> that indicates the wrap
        /// mode for the <strong>GeoLinearGradientBrush</strong>.</summary>
        public GeoWrapMode WrapMode
        {
            get { return _wrapMode; }
            set { _wrapMode = value; }
        }

        private static float ConvertDirectionToAngle(GeoLinearGradientDirection direction)
        {
            switch (direction)
            {
                case GeoLinearGradientDirection.LeftToRight:
                    return 0;
                case GeoLinearGradientDirection.RightToLeft:
                    return 180;
                case GeoLinearGradientDirection.TopToBottom:
                    return 270;
                case GeoLinearGradientDirection.BottomToTop:
                    return 90;
                case GeoLinearGradientDirection.UpperLeftToLowerRight:
                    return 135;
                case GeoLinearGradientDirection.LowerRightToUpperLeft:
                    return 315;
                case GeoLinearGradientDirection.LowerLeftToUpperRight:
                    return 225;
                case GeoLinearGradientDirection.UpperRightToLowerLeft:
                    return 45;
                default:
                    return 0;
            }
        }
    }
}
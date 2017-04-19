using System;
using System.Collections.ObjectModel;
using Mapgenix.Canvas;

namespace Mapgenix.Styles
{
    /// <summary>A single class break used in ClassBreakStyle</summary>
    /// <remarks>To display data depending on ranges of data in a column.</remarks>
    [Serializable]
    public class ClassBreak
    {
        private double _breakValue;
        private PointStyle _defaultPointStyle;
        private LineStyle _defaultLineStyle;
        private AreaStyle _defaultAreaStyle;
        private LabelStyle _defaultTextStyle;
        private Collection<BaseStyle> _customStyles;

        /// <summary>Class constructor.</summary>
        /// <overloads>Default constructor</overloads>
        /// <remarks>You need to set the properties manually.</remarks>
        public ClassBreak()
            : this(double.MinValue, new AreaStyle(), new PointStyle(), new LineStyle(), new LabelStyle(), new Collection<BaseStyle>())
        {
        }

        /// <summary>Class constructor.</summary>
        /// <overloads>To pass in a break value and an AreaStyle.</overloads>
        /// <returns>None</returns>
        /// <remarks>To pass in a break value and an AreaStyle.</remarks>
        /// <param name="value">Break value.</param>
        /// <param name="areaStyle">AreaStyle to represent the break.</param>
        public ClassBreak(double value, AreaStyle areaStyle)
            : this(value, areaStyle, new PointStyle(), new LineStyle(), new LabelStyle(), new Collection<BaseStyle>())
        {
        }

        /// <summary>Class constructor.</summary>
        /// <overloads>To pass in a break value and a Point </overloads>
        /// <returns>None</returns>
        /// <remarks>To pass in a break value and a Point </remarks>
        /// <param name="value">Break value.</param>
        /// <param name="pointStyle">PointStyle to represent the break.</param>
        public ClassBreak(double value, PointStyle pointStyle)
            : this(value, new AreaStyle(), pointStyle, new LineStyle(), new LabelStyle(), new Collection<BaseStyle>())
        {
        }

        /// <summary>Class constructor.</summary>
        /// <overloads>To pass in a break value and a LineStyle </overloads>
        /// <returns>None</returns>
        /// <remarks>To pass in a break value and a LineStyle </remarks>
        /// <param name="value">Break value.</param>
        /// <param name="lineStyle">LineStyle to represent the break.</param>
        public ClassBreak(double value, LineStyle lineStyle)
            : this(value, new AreaStyle(), new PointStyle(), lineStyle, new LabelStyle(), new Collection<BaseStyle>())
        {
        }

        /// <summary>Class constructor.</summary>
        /// <overloads>To pass in a break value and a TextStyle </overloads>
        /// <returns>None</returns>
        /// <remarks>To pass in a break value and a TextStyle </remarks>
        /// <param name="value">Break value.</param>
        /// <param name="textStyle">TextStyle to represent the break.</param>
        public ClassBreak(double value, LabelStyle textStyle)
            : this(value, new AreaStyle(), new PointStyle(), new LineStyle(), textStyle, new Collection<BaseStyle>())
        {
        }

        /// <summary>Class constructor.</summary>
        /// <overloads>To pass in a break value and a collection of TextStyles </overloads>
        /// <returns>None</returns>
        /// <remarks>To pass in a break value and a collection of TextStyles </remarks>
        /// <param name="value">Break value.</param>
        /// <param name="customStyles">TextStyles to represent the break.</param>
        public ClassBreak(double value, Collection<BaseStyle> customStyles)
            : this(value, new AreaStyle(), new PointStyle(), new LineStyle(), new LabelStyle(), customStyles)
        {
        }

        private ClassBreak(double value, AreaStyle areaStyle, PointStyle pointStyle, LineStyle lineStyle, LabelStyle textStyle, Collection<BaseStyle> styles)
        {
            _defaultAreaStyle = areaStyle;
            _defaultPointStyle = pointStyle;
            _defaultLineStyle = lineStyle;
            _defaultTextStyle = textStyle;
            _customStyles = styles;
            _breakValue = value;
        }

        /// <summary>Gets and sets the break value.</summary>
        /// <value>Break value.</value>
        /// <remarks>To determine where the break is in the ClassBreakStyle.</remarks>
        public double Value
        {
            get { return _breakValue; }
            set { _breakValue = value; }
        }

        /// <summary>Gets and sets the default AreaStyle to draw the class break.</summary>
        /// <value>Default AreaStyle to draw the class break.</value>
        public AreaStyle DefaultAreaStyle
        {
            get { return _defaultAreaStyle; }
            set { _defaultAreaStyle = value; }
        }

        /// <summary>Gets and sets the default LineStyle to draw the class break.</summary>
        /// <value>Default LineStyle to draw the class break.</value>
        public LineStyle DefaultLineStyle
        {
            get { return _defaultLineStyle; }
            set { _defaultLineStyle = value; }
        }

        /// <summary>Gets and sets the default PointStyle to draw the class break.</summary>
        /// <value>Default PointStyle to draw the class break.</value>
        public PointStyle DefaultPointStyle
        {
            get { return _defaultPointStyle; }
            set { _defaultPointStyle = value; }
        }

        /// <summary>Gets and sets the default TextStyle to draw the class break.</summary>
        /// <value>Default TextStyle to draw the class break.</value>
        public LabelStyle DefaultTextStyle
        {
            get { return _defaultTextStyle; }
            set { _defaultTextStyle = value; }
        }

        /// <summary>Gets a collection of custom styles to draw the class break.</summary>
        /// <value>Collection of custom styles to draw the class break.</value>
        public Collection<BaseStyle> CustomStyles
        {
            get { return _customStyles; }
        }

    }
}

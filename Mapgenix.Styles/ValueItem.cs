using System;
using System.Collections.ObjectModel;
using Mapgenix.Canvas;

namespace Mapgenix.Styles 
{
    /// <summary>A single value with Style for ValueStyle.</summary>
    [Serializable]
    public class ValueItem
    {
        private string _itemValue;
        private PointStyle _defaultPointStyle;
        private LineStyle _defaultLineStyle;
        private AreaStyle _defaultAreaStyle;
        private LabelStyle _defaultTextStyle;
        private Collection<BaseStyle> _customStyles;

       
        public ValueItem()
            : this(string.Empty, new AreaStyle(), new LineStyle(), new PointStyle(), new LabelStyle(), new Collection<BaseStyle>())
        {
        }

        public ValueItem(string value, AreaStyle areaStyle)
            : this(value, areaStyle, new LineStyle(), new PointStyle(), new LabelStyle(), new Collection<BaseStyle>())
        {
        }

        public ValueItem(string value, LineStyle lineStyle)
            : this(value, new AreaStyle(), lineStyle, new PointStyle(), new LabelStyle(), new Collection<BaseStyle>())
        {
        }
        
        public ValueItem(string value, PointStyle pointStyle)
            : this(value, new AreaStyle(), new LineStyle(), pointStyle, new LabelStyle(), new Collection<BaseStyle>())
        {
        }

        public ValueItem(string value, LabelStyle textStyle)
            : this(value, new AreaStyle(), new LineStyle(), new PointStyle(), textStyle, new Collection<BaseStyle>())
        {
        }

        public ValueItem(string value, Collection<BaseStyle> customStyles)
            : this(value, new AreaStyle(), new LineStyle(), new PointStyle(), new LabelStyle(), customStyles)
        {
        }

        private ValueItem(string value, AreaStyle areaStyle, LineStyle lineStyle, PointStyle pointStyle, LabelStyle textStyle, Collection<BaseStyle> styles)
        {
            _itemValue = value;
            _defaultAreaStyle = areaStyle;
            _defaultLineStyle = lineStyle;
            _defaultPointStyle = pointStyle;
            _defaultTextStyle = textStyle;
            _customStyles = styles;
        }

       
        public string Value
        {
            get { return _itemValue; }
            set { _itemValue = value; }
        }

        public AreaStyle DefaultAreaStyle
        {
            get { return _defaultAreaStyle; }
            set { _defaultAreaStyle = value; }
        }

       
        public LineStyle DefaultLineStyle
        {
            get { return _defaultLineStyle; }
            set { _defaultLineStyle = value; }
        }

        public PointStyle DefaultPointStyle
        {
            get { return _defaultPointStyle; }
            set { _defaultPointStyle = value; }
        }

       
        public LabelStyle DefaultTextStyle
        {
            get { return _defaultTextStyle; }
            set { _defaultTextStyle = value; }
        }

        public Collection<BaseStyle> CustomStyles
        {
            get { return _customStyles; }
        }
    }
}

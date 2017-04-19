using System;
using System.Collections.ObjectModel;
using Mapgenix.Canvas;

namespace Mapgenix.Styles 
{
    ///<summary>Style for a single Regex Match statement and styles to draw.</summary>
    [Serializable]
    public class RegexItem
    {
        private string _regularExpression;
        private PointStyle _defaultPointStyle;
        private LineStyle _defaultLineStyle;
        private AreaStyle _defaultAreaStyle;
        private LabelStyle _defaultTextStyle;
        private Collection<BaseStyle> _customStyles;

       
        public RegexItem()
            : this(string.Empty, new AreaStyle(), new LineStyle(), new PointStyle(), new LabelStyle(), new Collection<BaseStyle>())
        {
        }

       
        public RegexItem(string regularExpression, AreaStyle areaStyle)
            : this(regularExpression, areaStyle, new LineStyle(), new PointStyle(), new LabelStyle(), new Collection<BaseStyle>())
        {
        }

       
        public RegexItem(string regularExpression, LineStyle lineStyle)
            : this(regularExpression, new AreaStyle(), lineStyle, new PointStyle(), new LabelStyle(), new Collection<BaseStyle>())
        {
        }

        
        public RegexItem(string regularExpression, PointStyle pointStyle)
            : this(regularExpression, new AreaStyle(), new LineStyle(), pointStyle, new LabelStyle(), new Collection<BaseStyle>())
        {
        }

       
        public RegexItem(string regularExpression, LabelStyle textStyle)
            : this(regularExpression, new AreaStyle(), new LineStyle(), new PointStyle(), textStyle, new Collection<BaseStyle>())
        {
        }

        
        public RegexItem(string regularExpression, Collection<BaseStyle> styles)
            : this(regularExpression, new AreaStyle(), new LineStyle(), new PointStyle(), new LabelStyle(), styles)
        {
        }

        private RegexItem(string regularExpression, AreaStyle areaStyle, LineStyle lineStyle, PointStyle pointStyle, LabelStyle textStyle, Collection<BaseStyle> styles)
        {
            _regularExpression = regularExpression;
            _defaultAreaStyle = areaStyle;
            _defaultLineStyle = lineStyle;
            _defaultPointStyle = pointStyle;
            _defaultTextStyle = textStyle;
            _customStyles = styles;
        }

        public string RegularExpression
        {
            get { return _regularExpression; }
            set { _regularExpression = value; }
        }

        public Collection<BaseStyle> CustomStyles
        {
            get { return _customStyles; }
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
    }
}

using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Mapgenix.Utils;

namespace Mapgenix.Canvas
{
    /// <summary>GeoPen for drawing lines on a canvas.</summary>
    [Serializable]
    public class GeoPen
    {
        private static long _geoPenIdCounter;

        private readonly long _id;

        private Collection<float> _dashPattern;
        private DrawingLineCap _drawingEndCap;
        private DrawingLineJoin _drawingLineJoin;

        private DrawingLineCap _drawingStartCap;

        private BaseGeoBrush _geoBrush;
        private GeoColor _geoColor;
        private GeoDashCap _geoDashCap;
        private LineDashStyle _lineDashStyle;
        private float _miterLimit = 10.0f;
        private float _width;

        /// <summary>Creates a GeoPen.</summary>
        /// <overloads>Creates a GeoPen with default values.</overloads>
        public GeoPen()
            : this(new GeoSolidBrush(GeoColor.StandardColors.Transparent), 1)
        {
        }

        /// <summary>Creates a GeoPen.</summary>
        /// <overloads>Creates a GeoPen with a GeoBrush.</overloads>
        /// <remarks>None</remarks>
        /// <param name="brush">Brush to draw lines.</param>
        public GeoPen(BaseGeoBrush brush)
            : this(brush, 1)
        {
        }

        /// <summary>Creates a GeoPen.</summary>
        /// <overloads>Creates a GeoPen with a GeoColor.</overloads>
        /// <remarks>None</remarks>
        /// <param name="color">Color to draw lines.</param>
        public GeoPen(GeoColor color)
            : this(new GeoSolidBrush(color), 1)
        {
        }

        /// <summary>Creates a GeoPen.</summary>
        /// <overloads>Creates a GeoPen with a GeoColor and a line width.</overloads>
        /// <remarks>None</remarks>
        /// <param name="color">Color to draw lines.</param>
        /// <param name="width">Width of the lines to draw.</param>
        public GeoPen(GeoColor color, float width)
            : this(new GeoSolidBrush(color), width)
        {
        }

        /// <summary>Creates a GeoPen.</summary>
        /// <overloads>Creates a GeoPen with a GeoBrush and a line width.</overloads>
        /// <remarks>None</remarks>
        /// <param name="brush">Brush to draw the lines.</param>
        /// <param name="width">Width of the line to draw.</param>
        public GeoPen(BaseGeoBrush brush, float width)
        {
            Validators.CheckParameterIsNotNull(brush, "bursh");
            Validators.CheckIfInputValueIsBiggerThan(width, "width", 0, RangeCheckingInclusion.IncludeValue);

            _geoBrush = brush;
            var solidBrush = _geoBrush as GeoSolidBrush;
            if (solidBrush != null)
            {
                _geoColor = solidBrush.Color;
            }
            _width = width;
            LineJoin = DrawingLineJoin.Round;

            _geoPenIdCounter += 1;
            _id = _geoPenIdCounter;
        }

        /// <summary>Gets and sets the brush of the GeoPen.</summary>
        public BaseGeoBrush Brush
        {
            get { return _geoBrush; }
            set
            {
                Validators.CheckParameterIsNotNull(value, "value");
                _geoBrush = value;
                var solidBrush = _geoBrush as GeoSolidBrush;
                if (solidBrush != null)
                {
                    _geoColor = solidBrush.Color;
                }
            }
        }

        /// <summary>Gets and sets the GeoColor of the GeoPen.</summary>
        public GeoColor Color
        {
            get { return _geoColor; }
            set
            {
                _geoColor = value;
                _geoBrush = new GeoSolidBrush(_geoColor);
            }
        }

        /// <summary>Gets and sets the dash cap of the GeoPen.</summary>
        public GeoDashCap DashCap
        {
            get { return _geoDashCap; }
            set
            {
                Validators.CheckGeoDashCapIsValid(value, "value");
                _geoDashCap = value;
            }
        }

        /// <summary>Gets the dash pattern of the GeoPen.</summary>
        /// <remarks>Assigning a value other than null sets the DashStyle property to Custom.
        /// The elements in the dashArray array set the length of each dash and space in the dash pattern. 
        /// The first element sets the length of a dash, the second element sets the length of a space, the third element sets the length of a dash, and so on. 
        /// The length of each dash and space in the dash pattern is the product of the element value in the array and the width of the GeoPen.</remarks>
        public Collection<float> DashPattern
        {
            get
            {
                if (_dashPattern == null)
                {
                    _dashPattern = new Collection<float>();
                }
                return _dashPattern;
            }
        }

        /// <summary>Gets and sets the dash style of the GeoPen.</summary>
        public LineDashStyle DashStyle
        {
            get
            {
                if (IsDashStyleCustom())
                {
                    _lineDashStyle = LineDashStyle.Custom;
                }

                return _lineDashStyle;
            }
            set
            {
                Validators.ChecklineDashStyleIsValid(value, "value");
                _lineDashStyle = value;

                SynchronizeDashStyleAndDashPattern();
            }
        }

        /// <summary>Gets and sets the end cap of the GeoPen.</summary>
        public DrawingLineCap EndCap
        {
            get { return _drawingEndCap; }
            set
            {
                Validators.CheckDrawingLineCapIsValid(value, "value");
                _drawingEndCap = value;
            }
        }

        /// <summary>Gets and sets the line join of the GeoPen.</summary>
        public DrawingLineJoin LineJoin
        {
            get { return _drawingLineJoin; }
            set
            {
                Validators.CheckDrawingLineJoinIsValid(value, "value");
                _drawingLineJoin = value;
            }
        }

        /// <summary>Gets and sets the miter limit of the GeoPen.</summary>
        public float MiterLimit
        {
            get { return _miterLimit; }
            set { _miterLimit = value; }
        }

        /// <summary>Gets and sets the start cap of the GeoPen.</summary>
        public DrawingLineCap StartCap
        {
            get { return _drawingStartCap; }
            set
            {
                Validators.CheckDrawingLineCapIsValid(value, "value");
                _drawingStartCap = value;
            }
        }

        /// <summary>Gets and sets the width of the GeoPen.</summary>
        public float Width
        {
            get { return _width; }
            set
            {
                Validators.CheckIfInputValueIsBiggerThan(value, "value", 0, RangeCheckingInclusion.IncludeValue);
                _width = value;
            }
        }

        /// <summary>Gets the Id of the GeoPen.</summary>
        public long Id
        {
            get { return _id; }
        }

        /// <summary>Sets the start, end and dash caps at one time.</summary>
        /// <remarks>None</remarks>
        /// <param name="startCap">Start cap to be used.</param>
        /// <param name="endCap">End cap to be used.</param>
        /// <param name="dashCap">Dash cap to be used.</param>
        public void SetLineCap(DrawingLineCap startCap, DrawingLineCap endCap, GeoDashCap dashCap)
        {
            Validators.CheckDrawingLineCapIsValid(startCap, "startCap");
            Validators.CheckDrawingLineCapIsValid(endCap, "endCap");
            Validators.CheckGeoDashCapIsValid(dashCap, "dashCap");

            _drawingStartCap = startCap;
            _drawingEndCap = endCap;
            _geoDashCap = dashCap;
        }

        /// <summary>Creates a copy of GeoPen using the deep clone technique.</summary>
        /// <returns>A cloned GeoPen.</returns>
        /// <remarks>Deep cloning copies the cloned object as well as all the objects within it.</remarks>
        public GeoPen CloneDeep()
        {
            return CloneDeepCore();
        }

        /// <summary>Creates a copy of GeoPen using the deep clone technique.</summary>
        /// <returns>A cloned GeoPen.</returns>
        /// <remarks>Deep cloning copies the cloned object as well as all the objects within it.</remarks>
        protected virtual GeoPen CloneDeepCore()
        {
            return (GeoPen) SerializeCloneDeep(this);
        }

        private void SynchronizeDashStyleAndDashPattern()
        {
            if (_dashPattern == null)
            {
                _dashPattern = new Collection<float>();
            }

            switch (_lineDashStyle)
            {
                case LineDashStyle.Solid:
                    _dashPattern.Clear();
                    break;
                case LineDashStyle.Custom:
                    break;
                case LineDashStyle.DashDot:
                    _dashPattern.Clear();
                    _dashPattern.Add(3f);
                    _dashPattern.Add(1f);
                    _dashPattern.Add(1f);
                    _dashPattern.Add(1f);
                    break;
                case LineDashStyle.DashDotDot:
                    _dashPattern.Clear();
                    _dashPattern.Add(3f);
                    _dashPattern.Add(1f);
                    _dashPattern.Add(1f);
                    _dashPattern.Add(1f);
                    _dashPattern.Add(1f);
                    _dashPattern.Add(1f);
                    break;
                case LineDashStyle.Dot:
                    _dashPattern.Clear();
                    _dashPattern.Add(1f);
                    _dashPattern.Add(1f);
                    break;
                case LineDashStyle.Dash:
                    _dashPattern.Clear();
                    _dashPattern.Add(3f);
                    _dashPattern.Add(1f);
                    break;
                default:
                    break;
            }
        }

        private bool IsDashStyleCustom()
        {
            if (_dashPattern == null)
            {
                return false;
            }

            switch (_lineDashStyle)
            {
                case LineDashStyle.Solid:
                    if (_dashPattern.Count == 0)
                    {
                        return false;
                    }
                    break;

                case LineDashStyle.DashDot:
                    if (_dashPattern.Count == 4 &&
                        _dashPattern[0] == 3f &&
                        _dashPattern[1] == 1f &&
                        _dashPattern[2] == 1f &&
                        _dashPattern[3] == 1f)
                    {
                        return false;
                    }

                    break;
                case LineDashStyle.DashDotDot:
                    if (_dashPattern.Count == 6 &&
                        _dashPattern[0] == 3f &&
                        _dashPattern[1] == 1f &&
                        _dashPattern[2] == 1f &&
                        _dashPattern[3] == 1f &&
                        _dashPattern[4] == 1f &&
                        _dashPattern[5] == 1f)
                    {
                        return false;
                    }

                    break;
                case LineDashStyle.Dot:
                    if (_dashPattern.Count == 2 &&
                        _dashPattern[0] == 1f &&
                        _dashPattern[1] == 1f)
                    {
                        return false;
                    }

                    break;
                case LineDashStyle.Dash:
                    if (_dashPattern.Count == 2 &&
                        _dashPattern[0] == 3f &&
                        _dashPattern[1] == 1f)
                    {
                        return false;
                    }

                    break;
                default:
                    break;
            }

            return true;
        }

        private static object SerializeCloneDeep(object instance)
        {
            var stream = new MemoryStream();
            var formatter = new BinaryFormatter();
            formatter.Serialize(stream, instance);
            stream.Seek(0, SeekOrigin.Begin);
            return formatter.Deserialize(stream);
        }
    }
}
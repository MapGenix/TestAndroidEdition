using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Mapgenix.Canvas.Properties;
using Mapgenix.Shapes;
using Mapgenix.Utils;

namespace Mapgenix.Canvas
{
    /// <summary>Abstract class for canvas. Inherited classes are used for different drawing systems, such as GDI+ or WPF.</summary>
    /// <remarks>
    /// 	<para>For implementing different drawing systems, such as GDI+,
    ///     GDI, or WPF. It can also be used for PDF, Printing systems etc.
    ///     The first call is always BeginDrawing by passing
    ///     in an image along with its world extent.</para>
    /// </remarks>
    [Serializable]
    public abstract class BaseGeoCanvas
    {
        private const int PositionByteOrder = 0;
        private const int PositionShapeType = 1;
        private const int PositionCount = 5;
        private const int PositionData = 9;
        private const int PositionPointData = 5;
        private const int SizeOfPoint = 16;
        private const int SizeOfInt = 4;
        private const int SizeOfDouble = 8;
        private readonly Collection<GeoColor> _keyColors;

        private readonly int _progressDrawingRaisingFrenquency;
        private RectangleShape _clipingArea;
        protected RectangleShape _currentExtent;
        protected double _currentScale;
        protected float _dpi = 96.0f;
        private DrawingQuality _drawingQuality;
        private bool _enableCliping;
        protected float _geoCanvasHeight;
        protected float _geoCanvasWidth;
        protected bool _hasDrawn;

        private GeoColor _keyColor;
        private int _progressDrawingRaisedCount;
        protected double _worldToScreenFactorX;
        protected double _worldToScreenFactorY;

        /// <summary>Default constructor.</summary>
        /// <remarks>None</remarks>
        protected BaseGeoCanvas()
        {
            _progressDrawingRaisingFrenquency = 200;
            _progressDrawingRaisedCount = 0;

            MapUnit = GeographyUnit.Unknown;
            _keyColors = new Collection<GeoColor>();
        }

        public RectangleShape ClipingArea
        {
            get { return _clipingArea; }
            set { _clipingArea = value; }
        }

        public bool EnableCliping
        {
            get { return _enableCliping; }
            set { _enableCliping = value; }
        }

        /// <summary>
        /// Gets the current scale in the canvas.
        /// </summary>
        /// <exception cref="System.InvalidOperationException" caption="InvalidOperationException">If IsDrawing mode is set to false, it throws an InvalidOperationException.</exception>
        
        public double CurrentScale
        {
            get
            {
                Validators.CheckGeoCanvasIsInDrawing(IsDrawing);

                return _currentScale;
            }
        }

        /// <summary>
        /// Gets the drawing quality.
        /// </summary>
        /// <remarks>
        /// The DrawingQuality determines if anti-aliasing methods or other techniques are used when drawing shapes.
        /// </remarks>
        public DrawingQuality DrawingQuality
        {
            get { return _drawingQuality; }
            set { _drawingQuality = value; }
        }

        /// <summary>Gets the MapUnit passed in on the BeginDrawingAPI in the GeoCanvas.</summary>
        public GeographyUnit MapUnit { get; protected set; }

        /// <summary>Gets the width of the canvas.</summary>
        /// <remarks>
        /// Gets the width of the canvas image that was passed in on BeginDrawing method.
        /// </remarks>
        /// <exception cref="System.InvalidOperationException" caption="InvalidOperationException">If IsDrawing mode is false, it throws an InvalidOperationException.</exception>
        public float Width
        {
            get
            {
                Validators.CheckGeoCanvasIsInDrawing(IsDrawing);
                return _geoCanvasWidth;
            }
            protected set
            {
                Validators.CheckGeoCanvasIsInDrawing(IsDrawing);
                _geoCanvasWidth = value;
            }
        }

        /// <summary>Gets the height of the canvas.</summary>
        /// <remarks>
        /// Gets the height of the canvas image that was passed in on BeginDrawing method.
        /// </remarks>
        /// <exception cref="System.InvalidOperationException" caption="InvalidOperationException">If IsDrawing mode is false, it throws an InvalidOperationException.</exception>
        public float Height
        {
            get
            {
                Validators.CheckGeoCanvasIsInDrawing(IsDrawing);
                return _geoCanvasHeight;
            }
            protected set
            {
                Validators.CheckGeoCanvasIsInDrawing(IsDrawing);
                _geoCanvasHeight = value;
            }
        }

        /// <summary>
        /// Gets the adjusted current extent based on how BeginDrawing method was called.
        /// </summary>
        /// <remarks>
        /// The extent that gets passed in on BeginDrawing is adjusted for the width and
        /// height of the physical media being drawn on. For example if the current extent is wider
        /// than taller than the bitmap being drawn on, the current extent needs to be
        /// adjusted to ensure the entire original extent is represented. 
        /// </remarks>
        /// <returns>
        /// Gets the adjusted current extent based on how BeginDrawing method was called.
        /// </returns>
        /// <exception cref="System.InvalidOperationException" caption="InvalidOperationException">If IsDrawing mode is false, it throws an InvalidOperationException.</exception>
        public RectangleShape CurrentWorldExtent
        {
            get
            {
                Validators.CheckGeoCanvasIsInDrawing(IsDrawing);
                return _currentExtent;
            }
            protected set
            {
                Validators.CheckGeoCanvasIsInDrawing(IsDrawing);
                _currentExtent = value;
            }
        }

        /// <summary>
        /// Indicates if GeoCanvas has KeyColor or not. 
        /// </summary>
        /// <remarks>The default value is false.</remarks>
        public virtual bool HasKeyColor
        {
            get { return false; }
        }

        /// <summary>
        /// Gets or sets a value representing a key color. If HasKeyColor property is false, it throws exception when using KeyColors.
        /// </summary>
        /// <remarks>Makes this color transparent when drawing image.</remarks>
        [Obsolete("This property is obsolete, please use property: KeyColors", false)]
        public GeoColor KeyColor
        {
            get
            {
                if (HasKeyColor)
                {
                    return _keyColor;
                }
                throw new Exception(ExceptionDescription.KeyColorIsNotSupported);
            }
            set
            {
                if (HasKeyColor)
                {
                    _keyColor = value;
                }
                else
                {
                    throw new Exception(ExceptionDescription.KeyColorIsNotSupported);
                }
            }
        }

        /// <summary>
        /// Gets a collection of key colors. If HasKeyColor property is false, it throws exception when you use KeyColors.
        /// </summary>
        /// <remarks>Makes these colors transparent when drawing image.</remarks>
        public Collection<GeoColor> KeyColors
        {
            get
            {
                if (HasKeyColor)
                {
                    return _keyColors;
                }
                throw new Exception(ExceptionDescription.KeyColorIsNotSupported);
            }
        }

        /// <summary>
        /// Gets the native image from the BeginDrawing function.
        /// </summary>
        public object NativeImage { get; protected set; }

        /// <summary>Gets the drawing status of the GeoCanvas.</summary>
        /// <remarks>
        /// This property is set to true when BeginDrawing method is called.
        /// It is set to false after EndDrawing method is called.
        /// </remarks>
        public bool IsDrawing { get; protected set; }

        /// <summary>
        /// The DPI value. Is only valid when HasDpi is set to true.
        /// </summary>
        public virtual float Dpi
        {
            get
            {
                Validators.CheckGeoCanvasIsInDrawing(IsDrawing);

                return _dpi;
            }

            set { _dpi = value; }
        }

        internal bool HasDrawn
        {
            get { return _hasDrawn; }
            set { _hasDrawn = value; }
        }

        protected bool IsCancelled
        {
            get
            {
                var args = new ProgressEventArgs();
                OnDrawingProgressChanged(args);
                return args.Cancel;
            }
        }

        public event EventHandler<ProgressEventArgs> DrawingProgressChanged;

        /// <summary>Clears the current canvas using the color specified.</summary>
        public void Clear(BaseGeoBrush fillBrush)
        {
            Validators.CheckGeoCanvasIsInDrawing(IsDrawing);
            Validators.CheckParameterIsNotNull(fillBrush, "fillBrush");

            ClearCore(fillBrush);

            _hasDrawn = false;
        }

        /// <summary>Clears the current canvas using the color specified.</summary>
        /// <remarks>
        /// 	<para>Use this method to clear the GeoCanvas.</para>
        /// 	<para>This method is designed to be overridden by the sub class.</para>
        /// </remarks>
        /// <param name="fillBrush">Brush used to clear the canvas.</param>
        protected virtual void ClearCore(BaseGeoBrush fillBrush)
        {
            Validators.CheckGeoCanvasIsInDrawing(IsDrawing);
            Validators.CheckParameterIsNotNull(fillBrush, "fillBrush");
            var rectangle = (RectangleShape) (_currentExtent.CloneDeep());
            rectangle.ScaleUp(100);
            DrawArea(new Feature(rectangle), fillBrush, DrawingLevel.LevelOne);
        }

        /// <summary>Draws an area (polygons, circles, ellipses etc) on the canvas.</summary>
        /// <overloads>
        /// GeoPen and the DrawingLevel to draw on.
        /// </overloads>
        /// <remarks>
        /// 	<para>Gives flexibility on how to draw area based shapes with a GeoBrush to
        ///     fill in an area, a GeoPen for the outline using.</para>
        /// 	<para>DrawingLevel allows to specify the level to draw on when drawing multiple areas for drop shadow effect for example.</para>
        /// </remarks>
        /// <exception cref="System.InvalidOperationException" caption="InvalidOperationException">If IsDrawing mode is false, throws an InvalidOperationException.</exception>
        /// <exception cref="System.ArgumentException" caption="ArgumentException">if well-known binary is invalid in the areaShapeWkb parameter, throws an ArgumentException.</exception>
        /// <exception cref="System.ArgumentNullException" caption="ArgumentNullException">If areaShapeWkb is null, throws an ArgumentNullException.</exception>
        /// <exception cref="System.ArgumentNullException" caption="ArgumentNullException">If outlinePen is null, throws an ArgumentNullException.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException" caption="ArgumentOutOfRangeException">If drawingLevel is not defined in the enumeration, throws a ArgumentOutOfRangeException.</exception>
        /// <param name="feature">Area based feature.</param>
        /// <param name="outlinePen">
        /// Outline GeoPen used to draw the area.
        /// </param>
        /// <param name="drawingLevel">DrawingLevel the GeoPen draws on.</param>
        public void DrawArea(Feature feature, GeoPen outlinePen, DrawingLevel drawingLevel)
        {
            var areaShapeWkb = feature.GetWellKnownBinary();

            Validators.CheckParameterIsNotNull(areaShapeWkb, "areaShapeWkb");
            Validators.CheckParameterIsNotNull(outlinePen, "outlinePen");
            Validators.CheckGeoCanvasIsInDrawing(IsDrawing);
            Validators.CheckWkbIsValid(areaShapeWkb, "areaShapeWkb");
            Validators.CheckDrawingLevelIsValid(drawingLevel, "drawingLevel");

            _hasDrawn = true;

            DrawArea(feature, outlinePen, null, drawingLevel, 0, 0, PenBrushDrawingOrder.BrushFirst);
        }

        /// <summary>Draws an area (polygons, circles, ellipses etc) on the canvas.</summary>
        /// <overloads>
        /// GeoPen and the DrawingLevel to draw on.
        /// </overloads>
        /// <remarks>
        /// 	<para>Gives flexibility on how to draw area based shapes with a GeoBrush to
        ///     fill in an area, a GeoPen for the outline using.</para>
        /// 	<para>DrawingLevel allows to specify the level to draw on when drawing multiple areas for drop shadow effect for example.</para>
        /// </remarks>
        /// <exception cref="System.InvalidOperationException" caption="InvalidOperationException">If IsDrawing mode is false, throws an InvalidOperationException.</exception>
        /// <exception cref="System.ArgumentException" caption="ArgumentException">if well-known binary is invalid in the areaShapeWkb parameter, throws an ArgumentException.</exception>
        /// <exception cref="System.ArgumentNullException" caption="ArgumentNullException">If areaShapeWkb is null, throws an ArgumentNullException.</exception>
        /// <exception cref="System.ArgumentNullException" caption="ArgumentNullException">If outlinePen is null, throws an ArgumentNullException.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException" caption="ArgumentOutOfRangeException">If drawingLevel is not defined in the enumeration, throws a ArgumentOutOfRangeException.</exception>
        /// <param name="feature">Area based shape.</param>
        /// <param name="outlinePen">
        /// Outline GeoPen used to draw the area.
        /// </param>
        /// <param name="drawingLevel">DrawingLevel the GeoPen draws on.</param>
        public void DrawArea(BaseAreaShape shape, GeoPen outlinePen, DrawingLevel drawingLevel)
        {
            Validators.CheckParameterIsNotNull(outlinePen, "outlinePen");
            Validators.CheckGeoCanvasIsInDrawing(IsDrawing);
            Validators.CheckDrawingLevelIsValid(drawingLevel, "drawingLevel");
            Validators.CheckParameterIsNotNull(shape, "shape");

            var feature = new Feature(shape);
            DrawArea(feature, outlinePen, drawingLevel);
        }

        /// <summary>Draws an area (polygons, circles, ellipses etc) on the canvas.</summary>
        /// <overloads>
        /// GeoBrush and the DrawingLevel to draw on.
        /// </overloads>
        /// <remarks>
        /// 	<para>Gives flexibility on how to draw area based shapes with a GeoBrush to
        ///     fill in an area, a GeoPen for the outline using.</para>
        /// 	<para>DrawingLevel allows to specify the level to draw on when drawing multiple areas for drop shadow effect for example.</para>
        /// </remarks>
       /// <param name="feature">Area based feature.</param>
        /// <param name="fillBrush">
        /// Fill GeoBrush used to draw the area.
        /// </param>
        /// <param name="drawingLevel">DrawingLevel the GeoBrush draws on.</param>
        public void DrawArea(Feature feature, BaseGeoBrush fillBrush, DrawingLevel drawingLevel)
        {
            var areaShapeWkb = feature.GetWellKnownBinary();

            Validators.CheckParameterIsNotNull(areaShapeWkb, "areaShapeWkb");
            Validators.CheckParameterIsNotNull(fillBrush, "fillBrush");
            Validators.CheckWkbIsValid(areaShapeWkb, "areaShapeWkb");
            Validators.CheckDrawingLevelIsValid(drawingLevel, "drawingLevel");
            Validators.CheckGeoCanvasIsInDrawing(IsDrawing);

            _hasDrawn = true;

            DrawArea(feature, null, fillBrush, drawingLevel, 0, 0, PenBrushDrawingOrder.BrushFirst);
        }

        /// <summary>Draws an area (polygons, circles, ellipses etc) on the canvas.</summary>
        /// <overloads>
        /// GeoBrush and the DrawingLevel to draw on.
        /// </overloads>
        /// <remarks>
        /// 	<para>Gives flexibility on how to draw area based shapes with a GeoBrush to
        ///     fill in an area, a GeoPen for the outline using.</para>
        /// 	<para>DrawingLevel allows to specify the level to draw on when drawing multiple areas for drop shadow effect for example.</para>
        /// </remarks>
        /// <param name="shape">Area based shape.</param>
        /// <param name="fillBrush">
        /// Fill GeoBrush used to draw the area.
        /// </param>
        /// <param name="drawingLevel">DrawingLevel the GeoBrush draws on.</param>
        public void DrawArea(BaseAreaShape shape, BaseGeoBrush fillBrush, DrawingLevel drawingLevel)
        {
            Validators.CheckParameterIsNotNull(fillBrush, "fillBrush");
            Validators.CheckDrawingLevelIsValid(drawingLevel, "drawingLevel");
            Validators.CheckGeoCanvasIsInDrawing(IsDrawing);
            Validators.CheckParameterIsNotNull(shape, "shape");

            var feature = new Feature(shape);
            DrawArea(feature, fillBrush, drawingLevel);
        }

        /// <summary>Draws an area (polygons, circles, ellipses etc) on the canvas.</summary>
        /// <overloads>
        /// GeoPen, GeoBrush and the DrawingLevel to draw on.
        /// </overloads>
        /// <remarks>
        /// 	<para>Gives flexibility on how to draw area based shapes with a GeoBrush to
        ///     fill in an area, a GeoPen for the outline using.</para>
        /// 	<para>DrawingLevel allows to specify the level to draw on when drawing multiple areas for drop shadow effect for example.</para>
        /// </remarks>
        /// <param name="feature">Area based feature.</param>
        /// <param name="outlinePen"> Outline GeoPen used to draw the area. </param>
        /// <param name="fillBrush"> Fill GeoBrush used to draw the area. </param>
        /// <param name="drawingLevel">DrawingLevel the GeoPen draws on.</param>
        public void DrawArea(Feature feature, GeoPen outlinePen, BaseGeoBrush fillBrush, DrawingLevel drawingLevel)
        {
            var areaShapeWkb = feature.GetWellKnownBinary();

            Validators.CheckGeoCanvasIsInDrawing(IsDrawing);
            Validators.CheckParameterIsNotNull(areaShapeWkb, "areaShapeWkb");
            Validators.CheckParameterIsNotBothNull(outlinePen, fillBrush, "outlinePen", "fillBrush");
            Validators.CheckDrawingLevelIsValid(drawingLevel, "drawingLevel");
            Validators.CheckWkbIsValid(areaShapeWkb, "areaShapeWkb");

            _hasDrawn = true;

            DrawArea(feature, outlinePen, fillBrush, drawingLevel, 0, 0, PenBrushDrawingOrder.BrushFirst);
        }

        /// <summary>Draws an area (polygons, circles, ellipses etc) on the canvas.</summary>
        /// <overloads>
        /// GeoPen, GeoBrush and the DrawingLevel to draw on.
        /// </overloads>
        /// <remarks>
        /// 	<para>Gives flexibility on how to draw area based shapes with a GeoBrush to
        ///     fill in an area, a GeoPen for the outline using.</para>
        /// 	<para>DrawingLevel allows to specify the level to draw on when drawing multiple areas for drop shadow effect for example.</para>
        /// </remarks>
        /// <param name="shape">Area shape to be drawn.</param>
        /// <param name="outlinePen">Outline pen used to draw the AreaShape.</param>
        /// <param name="fillBrush"> Fill Brush that will be used to draw the AreaShape. </param>
        /// <param name="drawingLevel">DrawingLevel the GeoPen draws on.</param>
        public void DrawArea(BaseAreaShape shape, GeoPen outlinePen, BaseGeoBrush fillBrush, DrawingLevel drawingLevel)
        {
            Validators.CheckParameterIsNotNull(shape, "shape");
            Validators.CheckGeoCanvasIsInDrawing(IsDrawing);
            Validators.CheckParameterIsNotBothNull(outlinePen, fillBrush, "outlinePen", "fillBrush");
            Validators.CheckDrawingLevelIsValid(drawingLevel, "drawingLevel");

            var feature = new Feature(shape);
            DrawArea(feature, outlinePen, fillBrush, drawingLevel);
        }

        /// <summary>Draws an area (polygons, circles, ellipses etc) on the canvas.</summary>
        /// <overloads>
        /// GeoPen, GeoBrush and the DrawingLevel to draw on.
        /// </overloads>
        /// <remarks>
        /// 	<para>Gives flexibility on how to draw area based shapes with a GeoBrush to
        ///     fill in an area, a GeoPen for the outline using.</para>
        /// 	<para>DrawingLevel allows to specify the level to draw on when drawing multiple areas for drop shadow effect for example.</para>
        /// </remarks>
        /// <param name="drawingLevel">DrawingLevel the GeoPen draws on.</param>
        /// <param name="xOffset"> X offset in screen coordinates for the feature that draws.</param>
        /// <param name="yOffset"> Y offset in screen coordinates for the feature that draws.</param>
        /// <param name="penBrushDrawingOrder">PenBrushingDrawingOrder used when drawing the area based feature.</param>
        public void DrawArea(Feature feature, GeoPen outlinePen, BaseGeoBrush fillBrush, DrawingLevel drawingLevel,
            float xOffset, float yOffset, PenBrushDrawingOrder penBrushDrawingOrder)
        {
            var areaShapeWkb = feature.GetWellKnownBinary();

            Validators.CheckParameterIsNotBothNull(outlinePen, fillBrush, "outlinePen", "fillBrush");
            Validators.CheckParameterIsNotNull(areaShapeWkb, "areaShapeWkb");
            Validators.CheckDrawingLevelIsValid(drawingLevel, "drawingLevel");
            Validators.CheckGeoCanvasIsInDrawing(IsDrawing);
            Validators.CheckWkbIsValid(areaShapeWkb, "areaShapeWkb");
            if (_enableCliping)
            {
                var areaShape = BaseShape.CreateShapeFromWellKnownData(areaShapeWkb) as BaseAreaShape;
                areaShape = areaShape.GetIntersection(_clipingArea);
                
                if (areaShape == null) 
                {
                    return;
                }
                areaShapeWkb = areaShape.GetWellKnownBinary();
            }

            var shapeType = ArrayHelper.GetShapeTypeFromByteArray(areaShapeWkb, PositionShapeType);

            if (shapeType != WkbShapeType.Polygon && shapeType != WkbShapeType.Multipolygon &&
                shapeType != WkbShapeType.GeometryCollection)
            {
                throw new ArgumentException(ExceptionDescription.WkbIsNotValidForDrawing, "feature");
            }

            var byteOrder = areaShapeWkb[PositionByteOrder];

            List<ScreenPointF[]> rings;
            if (shapeType == WkbShapeType.Polygon)
            {
                rings = GetScreenPointsFromPolygon(areaShapeWkb);

                if (!IsCancelled)
                {
                    DrawAreaCore(rings, outlinePen, fillBrush, drawingLevel, xOffset, yOffset, penBrushDrawingOrder);
                }
            }
            else if (shapeType == WkbShapeType.Multipolygon)
            {
                rings = GetScreenPointsFromMultiPolygon(areaShapeWkb, byteOrder);
                if (!IsCancelled)
                {
                    DrawAreaCore(rings, outlinePen, fillBrush, drawingLevel, xOffset, yOffset, penBrushDrawingOrder);
                }
            }
            else if (shapeType == WkbShapeType.GeometryCollection)
            {
                var geometryCollectionShape = (GeometryCollectionShape) feature.GetShape();

                foreach (var baseShape in geometryCollectionShape.Shapes)
                {
                    var subShapeWellKnownType = baseShape.GetWellKnownType();
                    if (subShapeWellKnownType == WellKnownType.Polygon ||
                        subShapeWellKnownType == WellKnownType.Multipolygon)
                    {
                        baseShape.Id = feature.Id;
                        var subShapeFeature = new Feature(baseShape, feature.ColumnValues);
                        DrawArea(subShapeFeature, outlinePen, fillBrush, drawingLevel, xOffset, yOffset,
                            penBrushDrawingOrder);
                    }
                }
            }

            _hasDrawn = true;
        }

        /// <summary>Draws an area (polygons, circles, ellipses etc) on the canvas.</summary>
        /// <overloads>
        /// GeoPen, GeoBrush and the DrawingLevel to draw on.
        /// </overloads>
        /// <remarks>
        /// 	<para>Gives flexibility on how to draw area based shapes with a GeoBrush to
        ///     fill in an area, a GeoPen for the outline using.</para>
        /// 	<para>DrawingLevel allows to specify the level to draw on when drawing multiple areas for drop shadow effect for example.</para>
        /// </remarks>
        /// <param name="feature">Area based feature to be drawn.</param>
        /// <param name="outlinePen"> Outline GeoPen used to draw the AreaShape. </param>
        /// <param name="fillBrush">Fill GeoBrush used to draw the AreaShape. </param>
        /// <param name="drawingLevel">DrawingLevel that the GeoPen will draw on.</param>
        /// <param name="xOffset"> X offset in screeen coordinates for the feature that will be drawn.</param>
        /// <param name="yOffset"> Y offset in screen coordinates for the feature that will be drawn.</param>
        /// <param name="penBrushDrawingOrder">PenBrushingDrawingOrder used when drawing area based feature.</param>
        public void DrawArea(BaseAreaShape shape, GeoPen outlinePen, BaseGeoBrush fillBrush, DrawingLevel drawingLevel,
            float xOffset, float yOffset, PenBrushDrawingOrder penBrushDrawingOrder)
        {
            Validators.CheckParameterIsNotBothNull(outlinePen, fillBrush, "outlinePen", "fillBrush");
            Validators.CheckDrawingLevelIsValid(drawingLevel, "drawingLevel");
            Validators.CheckGeoCanvasIsInDrawing(IsDrawing);
            Validators.CheckParameterIsNotNull(shape, "shape");
            Validators.CheckShapeIsAreaBaseShape(shape);

            var feature = new Feature(shape);
            DrawArea(feature, outlinePen, fillBrush, drawingLevel, xOffset, yOffset, penBrushDrawingOrder);
        }

        private List<ScreenPointF[]> GetScreenPointsFromMultiPolygon(byte[] areaShapeWkb, byte byteOrder)
        {
            var numberOfPolygon = ArrayHelper.GetIntFromByteArray(areaShapeWkb, PositionCount, byteOrder);
            var dataIndex = PositionData;

            var rings = new List<ScreenPointF[]>();
            for (var i = 0; i < numberOfPolygon; i++)
            {
                var byteOrder1 = areaShapeWkb[dataIndex + PositionByteOrder];
                var shapeType1 = ArrayHelper.GetShapeTypeFromByteArray(areaShapeWkb, dataIndex + PositionShapeType);

                if (shapeType1 != WkbShapeType.Polygon)
                {
                    throw new ArgumentException(ExceptionDescription.WkbIsNotValidForDrawing, "areaShapeWkb");
                }

                var numberOfRings = ArrayHelper.GetIntFromByteArray(areaShapeWkb, dataIndex + PositionCount, byteOrder1);
                dataIndex = dataIndex + PositionData;

                for (var j = 0; j < numberOfRings; j++)
                {
                    ScreenPointF[] points = null;
                    dataIndex = AddRingForPolygon(ref points, areaShapeWkb, dataIndex, byteOrder1);
                    rings.Add(points);
                }
            }
            return rings;
        }

        /// <summary>Draws an area (polygons, circles, ellipses etc) on the canvas.</summary>
        /// <overloads>
        /// GeoPen, GeoBrush and the DrawingLevel to draw on.
        /// </overloads>
        /// <remarks>
        /// 	<para>Gives flexibility on how to draw area based shapes with a GeoBrush to
        ///     fill in an area, a GeoPen for the outline using.</para>
        /// 	<para>DrawingLevel allows to specify the level to draw on when drawing multiple areas for drop shadow effect for example.</para>
        /// </remarks>
        /// <param name="screenPoints">AreaShape in well-known binary format.</param>
        /// <param name="outlinePen">Outline GeoPen used to draw the AreaShape.</param>
        /// <param name="fillBrush"> Fill GeoBrush used to draw the AreaShape. </param>
        /// <param name="drawingLevel">DrawingLevel that the GeoPe draws on.</param>        
        /// <param name="xOffset"> X offset in screen coordinates for the feature to be drawn.</param>
        /// <param name="yOffset"> Y offset in screen coordinates for the feature to be drawn.</param>
        /// <param name="penBrushDrawingOrder">PenBrushDrawingOrder used when drawing the area based feature.</param>
        public void DrawArea(IEnumerable<ScreenPointF[]> screenPoints, GeoPen outlinePen, BaseGeoBrush fillBrush,
            DrawingLevel drawingLevel, float xOffset, float yOffset, PenBrushDrawingOrder penBrushDrawingOrder)
        {
            Validators.CheckParameterIsNotNull(screenPoints, "screenPoints");
            Validators.CheckParameterIsNotBothNull(outlinePen, fillBrush, "outlinePen", "fillBrush");
            Validators.CheckDrawingLevelIsValid(drawingLevel, "drawingLevel");
            Validators.CheckGeoCanvasIsInDrawing(IsDrawing);

            _hasDrawn = true;
            if (!IsCancelled)
            {
                DrawAreaCore(screenPoints, outlinePen, fillBrush, drawingLevel, xOffset, yOffset, penBrushDrawingOrder);
            }
        }

        /// <summary>Draws an area (polygons, circles, ellipses etc) on the canvas.</summary>
        /// <overloads>
        /// GeoPen, GeoBrush and the DrawingLevel to draw on.
        /// </overloads>
        /// <remarks>
        /// 	<para>Gives flexibility on how to draw area based shapes with a GeoBrush to
        ///     fill in an area, a GeoPen for the outline using.</para>
        /// 	<para>DrawingLevel allows to specify the level to draw on when drawing multiple areas for drop shadow effect for example.</para>
        /// </remarks>
        /// <param name="screenPoints">AreaShape in well-known binary format.</param>
        /// <param name="outlinePen"> Outline GeoPen used to draw the AreaShape.</param>
        /// <param name="fillBrush"> Fill GeoBrush used to draw the AreaShape. </param>
        /// <param name="drawingLevel">DrawingLevel the GeoPen draws on.</param>        
        /// <param name="xOffset"> X offset in screen coordinates for the feature to be drawn.</param>
        /// <param name="yOffset"> Y offset in screen coordinates for the feature to be drawn.</param>
        /// <param name="penBrushDrawingOrder">PenBrushDrawingOrder used when drawing the area based feature.</param>
        protected abstract void DrawAreaCore(IEnumerable<ScreenPointF[]> screenPoints, GeoPen outlinePen,
            BaseGeoBrush fillBrush, DrawingLevel drawingLevel, float xOffset, float yOffset,
            PenBrushDrawingOrder penBrushDrawingOrder);

        /// <summary>Draws a line based feature on the canvas.</summary>
        /// <remarks>
        /// 	<para>Used to draw a line on the GeoCanvas using a specified GeoPen.</para>
        /// 	<para>The DrawingLevel allows to specify the level to draw you will draw on when
        ///     drawing multiple lines. For example for drawing a road.</para>
        /// </remarks>
        /// <param name="feature">Line based feature.</param>
        /// <param name="linePen">GeoPen used to draw the line based feature.</param>
        /// <param name="drawingLevel">DrawingLeve the GeoPen draws on.</param>
        public void DrawLine(Feature feature, GeoPen linePen, DrawingLevel drawingLevel)
        {
            var lineShapeWkb = feature.GetWellKnownBinary();

            Validators.CheckParameterIsNotNull(lineShapeWkb, "lineShapeWkb");
            Validators.CheckParameterIsNotNull(linePen, "linePen");
            Validators.CheckGeoCanvasIsInDrawing(IsDrawing);
            Validators.CheckDrawingLevelIsValid(drawingLevel, "drawingLevel");
            Validators.CheckWkbIsValid(lineShapeWkb, "lineShapeWkb");

            _hasDrawn = true;

            DrawLine(feature, linePen, drawingLevel, 0, 0);
        }
        /// <summary>Draws a line based shape on the canvas.</summary>
        /// <remarks>
        /// 	<para>Used to draw a line on the GeoCanvas using a specified GeoPen.</para>
        /// 	<para>The DrawingLevel allows to specify the level to draw you will draw on when
        ///     drawing multiple lines. For example for drawing a road.</para>
        /// </remarks>
        /// <param name="shape">Line shape to be drawn by GeoCanvas.</param>
        /// <param name="linePen">GeoPen used to draw the line.</param>
        /// <param name="drawingLevel">DrawingLevel the GeoPen draws on.</param>
        public void DrawLine(BaseLineShape shape, GeoPen linePen, DrawingLevel drawingLevel)
        {
            Validators.CheckParameterIsNotNull(linePen, "linePen");
            Validators.CheckGeoCanvasIsInDrawing(IsDrawing);
            Validators.CheckDrawingLevelIsValid(drawingLevel, "drawingLevel");
            Validators.CheckParameterIsNotNull(shape, "shape");

            DrawLine(new Feature(shape), linePen, drawingLevel);
        }

        /// <summary>Draws a line based feature on the canvas.</summary>
        /// <remarks>
        /// 	<para>Used to draw a line on the GeoCanvas using a specified GeoPen.</para>
        /// 	<para>The DrawingLevel allows to specify the level to draw you will draw on when
        ///     drawing multiple lines. For example for drawing a road.</para>
        /// </remarks>
        /// <param name="feature">Line feature to be drawn by GeoCanvas.</param>
        /// <param name="linePen">GeoPen used to draw the line.</param>
        /// <param name="drawingLevel">DrawingLevel the GeoPen draws on.</param>
        /// <param name="xOffset">X offset in screen coordinates for the feature to be drawn.</param>
        /// <param name="yOffset">Y offset in screen coordinates for the feature to be drawn.</param>
        public void DrawLine(Feature feature, GeoPen linePen, DrawingLevel drawingLevel, float xOffset, float yOffset)
        {
            var lineShapeWkb = feature.GetWellKnownBinary();

            Validators.CheckParameterIsNotNull(linePen, "outlinePen");
            Validators.CheckParameterIsNotNull(lineShapeWkb, "lineShapeWkb");
            Validators.CheckDrawingLevelIsValid(drawingLevel, "drawingLevel");
            Validators.CheckGeoCanvasIsInDrawing(IsDrawing);
            Validators.CheckWkbIsValid(lineShapeWkb, "lineShapeWkb");

            var shapeType = ArrayHelper.GetShapeTypeFromByteArray(lineShapeWkb, PositionShapeType);

            if (shapeType != WkbShapeType.LineString && shapeType != WkbShapeType.Multiline &&
                shapeType != WkbShapeType.GeometryCollection)
            {
                throw new ArgumentException(ExceptionDescription.WkbIsNotValidForDrawing, "feature");
            }

            if (_enableCliping)
            {
                var lineShape = BaseShape.CreateShapeFromWellKnownData(lineShapeWkb) as BaseLineShape;
                lineShape = lineShape.GetIntersection(_clipingArea);
                if (lineShape != null
                    && lineShape.Validate(ShapeValidationMode.Simple).IsValid)
                {
                    lineShapeWkb = lineShape.GetWellKnownBinary();
                    shapeType = ArrayHelper.GetShapeTypeFromByteArray(lineShapeWkb, PositionShapeType);
                }
                else
                {
                    return;
                }
            }

            var byteOrder = lineShapeWkb[PositionByteOrder];

            var startIndex = 0;

            if (shapeType == WkbShapeType.LineString)
            {
                DrawLine(lineShapeWkb, startIndex, xOffset, yOffset, drawingLevel, linePen);
            }
            else if (shapeType == WkbShapeType.Multiline)
            {
                var numberOfLines = ArrayHelper.GetIntFromByteArray(lineShapeWkb, startIndex + PositionCount, byteOrder);
                var dataIndex = startIndex + PositionData;

                for (var i = 0; i < numberOfLines; i++)
                {
                    dataIndex = DrawLine(lineShapeWkb, dataIndex, xOffset, yOffset, drawingLevel, linePen);
                }
            }
            else if (shapeType == WkbShapeType.GeometryCollection)
            {
                var geometryCollectionShape = (GeometryCollectionShape) feature.GetShape();

                foreach (var baseShape in geometryCollectionShape.Shapes)
                {
                    var subShapeWellKnownType = baseShape.GetWellKnownType();
                    if (subShapeWellKnownType == WellKnownType.Line || subShapeWellKnownType == WellKnownType.Multiline)
                    {
                        baseShape.Id = feature.Id;
                        var subShapeFeature = new Feature(baseShape, feature.ColumnValues);
                        DrawLine(subShapeFeature, linePen, drawingLevel, xOffset, yOffset);
                    }
                }
            }

            _hasDrawn = true;
        }

        /// <summary>Draws a line based shape on the canvas.</summary>
        /// <remarks>
        /// 	<para>Used to draw a line on the GeoCanvas using a specified GeoPen.</para>
        /// 	<para>The DrawingLevel allows to specify the level to draw you will draw on when
        ///     drawing multiple lines. For example for drawing a road.</para>
        /// </remarks>
        /// <param name="shape">Line shape to be drawn by GeoCanvas.</param>
        /// <param name="linePen">GeoPen used to draw the line.</param>
        /// <param name="drawingLevel">DrawingLevel the GeoPen draws on.</param>
        /// <param name="xOffset">X offset in screen coordinates for the feature to be drawn.</param>
        /// <param name="yOffset">Y offset in screen coordinates for the feature to be drawn.</param>
        public void DrawLine(BaseLineShape shape, GeoPen linePen, DrawingLevel drawingLevel, float xOffset,
            float yOffset)
        {
            Validators.CheckParameterIsNotNull(linePen, "linePen");
            Validators.CheckGeoCanvasIsInDrawing(IsDrawing);
            Validators.CheckDrawingLevelIsValid(drawingLevel, "drawingLevel");
            Validators.CheckParameterIsNotNull(shape, "shape");

            DrawLine(new Feature(shape), linePen, drawingLevel, xOffset, yOffset);
        }

        /// <summary>Draws a line based shape on the canvas.</summary>
        /// <remarks>
        /// 	<para>Used to draw a line on the GeoCanvas using a specified GeoPen.</para>
        /// 	<para>The DrawingLevel allows to specify the level to draw you will draw on when
        ///     drawing multiple lines. For example for drawing a road.</para>
        /// </remarks>
        /// <param name="screenPoints">LineShape in well-known binary format.</param>
        /// <param name="linePen"> GeoPen used to draw the LineShape.</param>
        /// <param name="drawingLevel">DrawingLevel the GeoPen draws on.</param>
        /// <param name="xOffset">X offset in screen coordinates for the feature to be drawn.</param>
        /// <param name="yOffset">Y offset in screen coordinates for the feature to be drawn.</param>
        public void DrawLine(IEnumerable<ScreenPointF> screenPoints, GeoPen linePen, DrawingLevel drawingLevel,
            float xOffset, float yOffset)
        {
            Validators.CheckParameterIsNotNull(screenPoints, "screenPoints");
            Validators.CheckParameterIsNotNull(linePen, "outlinePen");
            Validators.CheckDrawingLevelIsValid(drawingLevel, "drawingLevel");
            Validators.CheckGeoCanvasIsInDrawing(IsDrawing);
            if (!IsCancelled)
            {
                DrawLineCore(screenPoints, linePen, drawingLevel, xOffset, yOffset);
            }
        }

        /// <summary>Draws a line based shape on the canvas.</summary>
        /// <remarks>
        /// 	<para>Used to draw a line on the GeoCanvas using a specified GeoPen.</para>
        /// 	<para>The DrawingLevel allows to specify the level to draw you will draw on when
        ///     drawing multiple lines. For example for drawing a road.</para>
        /// </remarks>
        /// <param name="screenPoints">LineShape in well-known binary format.</param>
        /// <param name="linePen">GeoPen that will be used to draw the LineShape.</param>
        /// <param name="drawingLevel">DrawingLevel the GeoPen draws on.</param>
        /// <param name="xOffset">X offset in screen coordinate for the feature to be drawn.</param>
        /// <param name="yOffset">Y offset in screen coordinate for the feature to be drawn.</param>
        protected abstract void DrawLineCore(IEnumerable<ScreenPointF> screenPoints, GeoPen linePen,
            DrawingLevel drawingLevel, float xOffset, float yOffset);



        /// <summary>Draws an ellipse on the canvas.</summary>
        /// <overloads>
        /// Allows you to pass in a GeoPen and the DrawingLevel.
        /// </overloads>
        /// <remarks>
        /// 	<para>Provides various overloads for control on the drawing of the ellipse. GeoBrush to fill in the
        ///     area of the ellipse. GeoPen for the outline of the ellipse.
        ///     You can also call a overload that will allow you to specify
        ///     both a GeoPen and a GeoBrush.</para>
        /// 	<para>The DrawingLevel allows to specify the drawing level.</para>
        /// </remarks>
        /// <param name="feature">Center of the feature for the center of the ellipse.</param>
        /// <param name="width">Width of the ellipse to draw.</param>
        /// <param name="height">Height of the ellipse to draw.</param>
        /// <param name="outlinePen">Outline GeoPen used to draw the ellipse. </param>
        /// <param name="drawingLevel">DrawingLevel the GeoPen will draw on.</param>
        public void DrawEllipse(Feature feature, float width, float height, GeoPen outlinePen, DrawingLevel drawingLevel)
        {
            var centerPointWkb = feature.GetWellKnownBinary();

            Validators.CheckParameterIsNotNull(centerPointWkb, "pointShapeWkb");
            Validators.CheckParameterIsNotNull(outlinePen, "outlinePen");
            Validators.CheckGeoCanvasIsInDrawing(IsDrawing);
            Validators.CheckDrawingLevelIsValid(drawingLevel, "drawingLevel");
            Validators.CheckWkbIsValid(centerPointWkb, "pointShapeWkb");

            _hasDrawn = true;

            DrawEllipse(feature, width, height, outlinePen, null, drawingLevel, 0, 0, PenBrushDrawingOrder.BrushFirst);
        }

        /// <summary>Draws an ellipse on the canvas.</summary>
        /// <overloads>
        /// Allows you to pass in a GeoPen and the DrawingLevel.
        /// </overloads>
        /// <remarks>
        /// 	<para>Provides various overloads for control on the drawing of the ellipse. GeoBrush to fill in the
        ///     area of the ellipse. GeoPen for the outline of the ellipse.
        ///     You can also call a overload that will allow you to specify
        ///     both a GeoPen and a GeoBrush.</para>
        /// 	<para>The DrawingLevel allows to specify the drawing level.</para>
        /// </remarks>
        /// <param name="shape">Center of the shape for the center of the ellipse..</param>
        /// <param name="width">Width of the ellipse to draw.</param>
        /// <param name="height">Height of the ellipse to draw.</param>
        /// <param name="outlinePen">Outline GeoPen used to draw the point.</param>
        /// <param name="drawingLevel">DrawingLevel the GeoPen draws on.</param>
        public void DrawEllipse(BasePoint shape, float width, float height, GeoPen outlinePen,
            DrawingLevel drawingLevel)
        {
            Validators.CheckParameterIsNotNull(outlinePen, "outlinePen");
            Validators.CheckGeoCanvasIsInDrawing(IsDrawing);
            Validators.CheckDrawingLevelIsValid(drawingLevel, "drawingLevel");
            Validators.CheckParameterIsNotNull(shape, "shape");

            DrawEllipse(new Feature(shape), width, height, outlinePen, drawingLevel);
        }

        /// <summary>Draws an ellipse on the canvas.</summary>
        /// <overloads>
        /// Allows you to pass in a GeoPen and the DrawingLevel.
        /// </overloads>
        /// <remarks>
        /// 	<para>Provides various overloads for control on the drawing of the ellipse. GeoBrush to fill in the
        ///     area of the ellipse. GeoPen for the outline of the ellipse.
        ///     You can also call a overload that will allow you to specify
        ///     both a GeoPen and a GeoBrush.</para>
        /// 	<para>The DrawingLevel allows to specify the drawing level.</para>
        /// </remarks>
        /// <param name="centerPointFeature">Center point feature.</param>
        /// <param name="width">Width of the ellipse to be drawn.</param>
        /// <param name="height">Height of the ellipse to be drawn.</param>
        /// <param name="fillBrush">Fill GeoBrush used to draw the point.</param>
        /// <param name="drawingLevel">DrawingLevel the GeoBrush draws on.</param>
        public void DrawEllipse(Feature centerPointFeature, float width, float height, BaseGeoBrush fillBrush,
            DrawingLevel drawingLevel)
        {
            var centerPointWkb = centerPointFeature.GetWellKnownBinary();

            Validators.CheckParameterIsNotNull(centerPointWkb, "pointShapeWkb");
            Validators.CheckParameterIsNotNull(fillBrush, "fillBrush");
            Validators.CheckGeoCanvasIsInDrawing(IsDrawing);
            Validators.CheckDrawingLevelIsValid(drawingLevel, "drawingLevel");
            Validators.CheckWkbIsValid(centerPointWkb, "pointShapeWkb");

            _hasDrawn = true;

            DrawEllipse(centerPointFeature, width, height, null, fillBrush, drawingLevel, 0, 0,
                PenBrushDrawingOrder.BrushFirst);
        }

        /// <summary>Draws an ellipse on the canvas.</summary>
        /// <overloads>
        /// Allows you to pass in a GeoPen and the DrawingLevel.
        /// </overloads>
        /// <remarks>
        /// 	<para>Provides various overloads for control on the drawing of the ellipse. GeoBrush to fill in the
        ///     area of the ellipse. GeoPen for the outline of the ellipse.
        ///     You can also call a overload that will allow you to specify
        ///     both a GeoPen and a GeoBrush.</para>
        /// 	<para>The DrawingLevel allows to specify the drawing level.</para>
        /// </remarks>
        /// <param name="shape">Center point shape.</param>
        /// <param name="width">Width of the ellipse to draw.</param>
        /// <param name="height">Height of the ellipse to draw.</param>
        /// <param name="fillBrush">Fill GeoBrush used to draw the point.</param>
        /// <param name="drawingLevel">DrawingLevel the GeoBrush draws on.</param>
        public void DrawEllipse(BasePoint shape, float width, float height, BaseGeoBrush fillBrush,
            DrawingLevel drawingLevel)
        {
            Validators.CheckParameterIsNotNull(fillBrush, "fillBrush");
            Validators.CheckGeoCanvasIsInDrawing(IsDrawing);
            Validators.CheckDrawingLevelIsValid(drawingLevel, "drawingLevel");
            Validators.CheckParameterIsNotNull(shape, "shape");

            DrawEllipse(new Feature(shape), width, height, fillBrush, drawingLevel);
        }

        /// <summary>Draws an ellipse on the canvas.</summary>
        /// <overloads>
        /// Allows you to pass in a GeoPen and the DrawingLevel.
        /// </overloads>
        /// <remarks>
        /// 	<para>Provides various overloads for control on the drawing of the ellipse. GeoBrush to fill in the
        ///     area of the ellipse. GeoPen for the outline of the ellipse.
        ///     You can also call a overload that will allow you to specify
        ///     both a GeoPen and a GeoBrush.</para>
        /// 	<para>The DrawingLevel allows to specify the drawing level.</para>
        /// </remarks>
        /// <param name="feature">Center point feature.</param>
        /// <param name="width">Width of the ellipse to be drawn.</param>
        /// <param name="height">Height of the ellipse to be drawn.</param>
        /// <param name="outlinePen">Outline GeoPen used to draw the point.</param>
        /// <param name="fillBrush">Fill GeoBrush used to draw the point.</param>
        /// <param name="drawingLevel">DrawingLevel the GeoPen or GeoBrush draws on.</param>
        public void DrawEllipse(Feature feature, float width, float height, GeoPen outlinePen, BaseGeoBrush fillBrush,
            DrawingLevel drawingLevel)
        {
            var centerPointWkb = feature.GetWellKnownBinary();

            Validators.CheckParameterIsNotBothNull(outlinePen, fillBrush, "outlinePen", "fillBrush");
            Validators.CheckParameterIsNotNull(centerPointWkb, "pointShapeWkb");
            Validators.CheckGeoCanvasIsInDrawing(IsDrawing);
            Validators.CheckDrawingLevelIsValid(drawingLevel, "drawingLevel");
            Validators.CheckWkbIsValid(centerPointWkb, "pointShapeWkb");

            _hasDrawn = true;

            DrawEllipse(feature, width, height, outlinePen, fillBrush, drawingLevel, 0, 0,
                PenBrushDrawingOrder.BrushFirst);
        }

        /// <summary>Draws an ellipse on the canvas.</summary>
        /// <overloads>
        /// Allows you to pass in a GeoPen and the DrawingLevel.
        /// </overloads>
        /// <remarks>
        /// 	<para>Provides various overloads for control on the drawing of the ellipse. GeoBrush to fill in the
        ///     area of the ellipse. GeoPen for the outline of the ellipse.
        ///     You can also call a overload that will allow you to specify
        ///     both a GeoPen and a GeoBrush.</para>
        /// 	<para>The DrawingLevel allows to specify the drawing level.</para>
        /// </remarks>
        /// <param name="shape">Center point shape.</param>
        /// <param name="width">Width of the ellipse to be drawn.</param>
        /// <param name="height">Height of the ellipse to be drawn.</param>
        /// <param name="outlinePen">Outline GeoPen used to draw the point.</param>
        /// <param name="fillBrush">Fill GeoBrush used to draw the point.</param>
        /// <param name="drawingLevel">DrawingLevel the GeoPen or GeoBrush draws on. </param>
        public void DrawEllipse(BasePoint shape, float width, float height, GeoPen outlinePen, BaseGeoBrush fillBrush,
            DrawingLevel drawingLevel)
        {
            Validators.CheckParameterIsNotBothNull(outlinePen, fillBrush, "outlinePen", "fillBrush");
            Validators.CheckGeoCanvasIsInDrawing(IsDrawing);
            Validators.CheckDrawingLevelIsValid(drawingLevel, "drawingLevel");
            Validators.CheckParameterIsNotNull(shape, "shape");

            DrawEllipse(new Feature(shape), width, height, outlinePen, fillBrush, drawingLevel);
        }

        /// <summary>Draws an ellipse on the canvas.</summary>
        /// <overloads>
        /// Allows you to pass in a GeoPen and the DrawingLevel.
        /// </overloads>
        /// <remarks>
        /// 	<para>Provides various overloads for control on the drawing of the ellipse. GeoBrush to fill in the
        ///     area of the ellipse. GeoPen for the outline of the ellipse.
        ///     You can also call a overload that will allow you to specify
        ///     both a GeoPen and a GeoBrush.</para>
        /// 	<para>The DrawingLevel allows to specify the drawing level.</para>
        /// </remarks>
        /// <param name="feature">Center point feature.</param>
        /// <param name="width">Width of the ellipse to be drawn.</param>
        /// <param name="height">Height of the ellipse to be drawn.</param>
        /// <param name="outlinePen">Outline GeoPen used to draw the point.</param>
        /// <param name="fillBrush">Fill GeoBrush used to draw the point.</param>
        /// <param name="drawingLevel">DrawingLevel the GeoPen or GeoBrush will draw on.</param>
        /// <param name="xOffset">X offset in world coordinates for the ellipse to be drawn.</param>
        /// <param name="yOffset">Y offset in world coordinates for the ellipse to be drawn.</param>
        /// <param name="penBrushDrawingOrder">PenBrushDrawingOrder for drawing the ellipse.</param>
        public void DrawEllipse(Feature feature, float width, float height, GeoPen outlinePen, BaseGeoBrush fillBrush,
            DrawingLevel drawingLevel, float xOffset, float yOffset, PenBrushDrawingOrder penBrushDrawingOrder)
        {
            var centerPointWkb = feature.GetWellKnownBinary();

            Validators.CheckParameterIsNotBothNull(outlinePen, fillBrush, "outlinePen", "fillBrush");
            Validators.CheckParameterIsNotNull(centerPointWkb, "pointShapeWkb");
            Validators.CheckDrawingLevelIsValid(drawingLevel, "drawingLevel");
            Validators.CheckWkbIsValid(centerPointWkb, "pointShapeWkb");
            Validators.CheckGeoCanvasIsInDrawing(IsDrawing);

            var shapeType = ArrayHelper.GetShapeTypeFromByteArray(centerPointWkb, PositionShapeType);

            if (shapeType != WkbShapeType.Point && shapeType != WkbShapeType.Multipoint &&
                shapeType != WkbShapeType.GeometryCollection)
            {
                throw new ArgumentException(ExceptionDescription.WkbIsNotValidForDrawing, "feature");
            }

            var byteOrder = centerPointWkb[PositionByteOrder];

            var startIndex = 0;

            if (shapeType == WkbShapeType.Point)
            {
                DrawCircle(centerPointWkb, startIndex, width, height, outlinePen, fillBrush, drawingLevel, xOffset,
                    yOffset, penBrushDrawingOrder);
            }
            else if (shapeType == WkbShapeType.Multipoint)
            {
                var numberOfPoints = ArrayHelper.GetIntFromByteArray(centerPointWkb, startIndex + PositionCount, byteOrder);
                var dataIndex = startIndex + PositionData;

                for (var i = 0; i < numberOfPoints; i++)
                {
                    dataIndex = DrawCircle(centerPointWkb, dataIndex, width, height, outlinePen, fillBrush, drawingLevel,
                        xOffset, yOffset, penBrushDrawingOrder);
                }
            }
            else if (shapeType == WkbShapeType.GeometryCollection)
            {
                var geometryCollectionShape = (GeometryCollectionShape) feature.GetShape();

                foreach (var baseShape in geometryCollectionShape.Shapes)
                {
                    var subShapeWellKnownType = baseShape.GetWellKnownType();
                    if (subShapeWellKnownType == WellKnownType.Point ||
                        subShapeWellKnownType == WellKnownType.Multipoint)
                    {
                        baseShape.Id = feature.Id;
                        var subShapeFeature = new Feature(baseShape, feature.ColumnValues);
                        DrawEllipse(subShapeFeature, width, height, outlinePen, fillBrush, drawingLevel, xOffset,
                            yOffset, penBrushDrawingOrder);
                    }
                }
            }

            _hasDrawn = true;
        }

        /// <summary>Draws an ellipse on the canvas.</summary>
        /// <overloads>
        /// Allows you to pass in a GeoPen and the DrawingLevel.
        /// </overloads>
        /// <remarks>
        /// 	<para>Provides various overloads for control on the drawing of the ellipse. GeoBrush to fill in the
        ///     area of the ellipse. GeoPen for the outline of the ellipse.
        ///     You can also call a overload that will allow you to specify
        ///     both a GeoPen and a GeoBrush.</para>
        /// 	<para>The DrawingLevel allows to specify the drawing level.</para>
        /// </remarks>
        /// <param name="shape">Center point shape.</param>
        /// <param name="width">Width of the ellipse to be drawn.</param>
        /// <param name="height">Height of the ellipse to be drawn.</param>
        /// <param name="outlinePen">Outline GeoPen used to draw the point.</param>
        /// <param name="fillBrush">Fill GeoBrush used to draw the point.</param>
        /// <param name="drawingLevel">DrawingLevel the GeoPen or GeoBrush draws on.</param>
        /// <param name="xOffset">X offset in world coordinates for the ellipse to be drawn.</param>
        /// <param name="yOffset">Y offset in world coordinates for the ellipse to be drawn.</param>
        /// <param name="penBrushDrawingOrder">PenBrushDrawingOrder for drawing the ellipse.</param>
        public void DrawEllipse(BasePoint shape, float width, float height, GeoPen outlinePen, BaseGeoBrush fillBrush,
            DrawingLevel drawingLevel, float xOffset, float yOffset, PenBrushDrawingOrder penBrushDrawingOrder)
        {
            Validators.CheckParameterIsNotBothNull(outlinePen, fillBrush, "outlinePen", "fillBrush");
            Validators.CheckDrawingLevelIsValid(drawingLevel, "drawingLevel");
            Validators.CheckGeoCanvasIsInDrawing(IsDrawing);
            Validators.CheckParameterIsNotNull(shape, "shape");

            DrawEllipse(new Feature(shape), width, height, outlinePen, fillBrush, drawingLevel, xOffset, yOffset,
                penBrushDrawingOrder);
        }

        /// <summary>Draws an ellipse on the canvas.</summary>
        /// <overloads>
        /// Allows you to pass in a GeoPen and the DrawingLevel.
        /// </overloads>
        /// <remarks>
        /// 	<para>Provides various overloads for control on the drawing of the ellipse. GeoBrush to fill in the
        ///     area of the ellipse. GeoPen for the outline of the ellipse.
        ///     You can also call a overload that will allow you to specify
        ///     both a GeoPen and a GeoBrush.</para>
        /// 	<para>The DrawingLevel allows to specify the drawing level.</para>
        /// </remarks>
        /// <param name="screenPoint">Center point in screen coordinates.</param>
        /// <param name="width">Width of the ellipse to be drawn.</param>
        /// <param name="height">Height of the ellipse to be drawn.</param>
        /// <param name="outlinePen">Outline GeoPen used to draw the point.</param>
        /// <param name="fillBrush">Fill GeoBrush used to draw the point.</param>
        /// <param name="drawingLevel">DrawingLevel the GeoPen or GeoBrush draws on.</param>
        /// <param name="xOffset">X offset in screen coordinates for the ellipse to draw.</param>
        /// <param name="yOffset">Y offset in screen coordinates for the ellipse to draw.</param>
        /// <param name="penBrushDrawingOrder"> </param>
        public void DrawEllipse(ScreenPointF screenPoint, float width, float height, GeoPen outlinePen,
            BaseGeoBrush fillBrush, DrawingLevel drawingLevel, float xOffset, float yOffset,
            PenBrushDrawingOrder penBrushDrawingOrder)
        {
            Validators.CheckParameterIsNotBothNull(outlinePen, fillBrush, "outlinePen", "fillBrush");
            Validators.CheckDrawingLevelIsValid(drawingLevel, "drawingLevel");
            Validators.CheckGeoCanvasIsInDrawing(IsDrawing);
            if (!IsCancelled)
            {
                DrawEllipseCore(screenPoint, width, height, outlinePen, fillBrush, drawingLevel, xOffset, yOffset,
                    penBrushDrawingOrder);
            }
        }

        /// <summary>Draws an ellipse on the canvas.</summary>
        /// <overloads>
        /// Allows you to pass in a GeoPen and the DrawingLevel.
        /// </overloads>
        /// <remarks>
        /// 	<para>Provides various overloads for control on the drawing of the ellipse. GeoBrush to fill in the
        ///     area of the ellipse. GeoPen for the outline of the ellipse.
        ///     You can also call a overload that will allow you to specify
        ///     both a GeoPen and a GeoBrush.</para>
        /// 	<para>The DrawingLevel allows to specify the drawing level.</para>
        /// </remarks>
        /// <param name="screenPoint">Center point in well-known binary format.</param>
        /// <param name="outlinePen">Outline GeoPen used to draw the point.</param>
        /// <param name="width">Width of the ellipse to be drawn.</param>
        /// <param name="height">Height of the ellipse to be drawn.</param>
        /// <param name="fillBrush">Fill GeoBrush used to draw the point.</param>
        /// <param name="drawingLevel">DrawingLevel the GeoPen or GeoBrush will draw on.</param>
        /// <param name="xOffset">X offset in screen coordinates for the ellipse to be drawn.</param>
        /// <param name="yOffset">Y offset in screen coodinates for the ellipse to be drawn.</param>
        /// <param name="penBrushDrawingOrder">PenBrushDrawingOrder for drawing the ellipse.</param>
        protected abstract void DrawEllipseCore(ScreenPointF screenPoint, float width, float height, GeoPen outlinePen,
            BaseGeoBrush fillBrush, DrawingLevel drawingLevel, float xOffset, float yOffset,
            PenBrushDrawingOrder penBrushDrawingOrder);

        /// <summary>Draws an unscaled image on the canvas.</summary>
        /// <remarks>
        /// 	<para>Drawing an image unscaled is faster than scaling it.</para>
        /// 	<para>The X &amp; Y in world coordinates is where the center of the image draws.</para>
        /// </remarks>
        /// <param name="image">Image to draw unscaled.</param>
        /// <param name="centerXInWorld">X coordinate in world coordinates of the center point of the image to draw.</param>
        /// <param name="centerYInWorld">Y coordinate in world coordinates of the center point of the image to draw.</param>
        /// <param name="drawingLevel">DrawingLevel the image draws on.</param>
        public void DrawWorldImageWithoutScaling(GeoImage image, double centerXInWorld, double centerYInWorld,
            DrawingLevel drawingLevel)
        {
            Validators.CheckParameterIsNotNull(image, "image");
            Validators.CheckGeoCanvasIsInDrawing(IsDrawing);
            Validators.CheckDrawingLevelIsValid(drawingLevel, "drawingLevel");


            _hasDrawn = true;

            DrawWorldImageWithoutScaling(image, centerXInWorld, centerYInWorld, drawingLevel, 0, 0, 0);
        }

        /// <summary>Draws an unscaled image on the canvas.</summary>
        /// <remarks>
        /// 	<para>Drawing an image unscaled is faster than scaling it.</para>
        /// 	<para>The X &amp; Y in world coordinates is where the center of the image draws.</para>
        /// </remarks>
        /// <param name="image">Image to draw unscaled.</param>
        /// <param name="centerXInWorld">X coordinate of the center point (in world coordinates) of the image to draw.</param>
        /// <param name="centerYInWorld">Y coordinate of the center point (in world coordinates) of the image to draw.</param>
        /// <param name="drawingLevel">DrawingLevel the image draws on.</param>
        /// <param name="xOffset">X offset in world coordinates for the image to be drawn.</param>
        /// <param name="yOffset">Y offset in world coordinates for the image to be drawn.</param>
        /// <param name="rotateAngle">Rotation angle of the image to be drawn.</param>
        public void DrawWorldImageWithoutScaling(GeoImage image, double centerXInWorld, double centerYInWorld,
            DrawingLevel drawingLevel, float xOffset, float yOffset, float rotateAngle)
        {
            Validators.CheckParameterIsNotNull(image, "image");
            Validators.CheckGeoCanvasIsInDrawing(IsDrawing);
            Validators.CheckDrawingLevelIsValid(drawingLevel, "drawingLevel");


            var upperLeftX = CurrentWorldExtent.UpperLeftPoint.X;
            var upperLeftY = CurrentWorldExtent.UpperLeftPoint.Y;

            var screenX = (float) ((centerXInWorld - upperLeftX)*_geoCanvasWidth/CurrentWorldExtent.Width);
            var screenY = (float) ((upperLeftY - centerYInWorld)*_geoCanvasHeight/CurrentWorldExtent.Height);

            if (!IsCancelled)
            {
                DrawScreenImageWithoutScalingCore(image, screenX, screenY, drawingLevel, xOffset, yOffset, rotateAngle);
            }

            _hasDrawn = true;
        }

        /// <summary>Draws an unscaled image on the canvas.</summary>
        /// <remarks>
        /// 	<para>Drawing an image unscaled is faster than scaling it.</para>
        /// 	<para>The X &amp; Y in screen coordinates is where the center of the image will be
        ///     drawn.</para>
        /// </remarks>
        /// <param name="image">Image to draw unscaled.</param>
        /// <param name="centerXInScreen">X coordinate of the center point (in screen coordinates) of the image to draw.</param>
        /// <param name="centerYInScreen">Y coordinate of the center point (in screen coordinates) of the image to draw.</param>
        /// <param name="drawingLevel">DrawingLevel the image draws on.</param>
        /// <param name="xOffset">X offset in screen coordinates for the image to be drawn.</param>
        /// <param name="yOffset">Y offset in screen coordinates for the image to be drawn.</param>
        /// <param name="rotateAngle">Rotation angle of the image to draw.</param>
        public void DrawScreenImageWithoutScaling(GeoImage image, float centerXInScreen, float centerYInScreen,
            DrawingLevel drawingLevel, float xOffset, float yOffset, float rotateAngle)
        {
            if (!IsCancelled)
            {
                DrawScreenImageWithoutScalingCore(image, centerXInScreen, centerYInScreen, drawingLevel, xOffset,
                    yOffset, rotateAngle);
            }
        }

        /// <summary>Draws an unscaled image on the canvas.</summary>
        /// <remarks>
        /// 	<para>Drawing an image unscaled is faster than scaling it.</para>
        /// 	<para>The X &amp; Y in screen coordinates is where the center of the image will be
        ///     drawn.</para>
        /// </remarks>
        /// <param name="image">Image to draw unscaled.</param>
        /// <param name="centerXInScreen">X coordinate of the center point (in screen coordinates) of the image to draw.</param>
        /// <param name="centerYInScreen">Y coordinate of the center point (in screen coordinates) of the image to draw.</param>
        /// <param name="drawingLevel">DrawingLevel the image draws on.</param>
        /// <param name="xOffset">X offset in screen coordinates for the image to be drawn.</param>
        /// <param name="yOffset">Y offset in screen coordinates for the image to be drawn.</param>
        /// <param name="rotateAngle">Rotation angle of the image to draw.</param>
        protected abstract void DrawScreenImageWithoutScalingCore(GeoImage image, float centerXInScreen,
            float centerYInScreen, DrawingLevel drawingLevel, float xOffset, float yOffset, float rotateAngle);

        /// <summary>Draws a scaled image on the canvas.</summary>
        /// <remarks>Drawing an image scaled is slower than drawing it unscaled.</remarks>
        /// <param name="image">Image to draw.</param>
        /// <param name="centerXInWorld">X coordinate of the center point of the image to draw.</param>
        /// <param name="centerYInWorld">Y coordinate of the center point of the image to draw.</param>
        /// <param name="widthInScreen">Width of the image to draw.</param>
        /// <param name="heightInScreen">Height of the image to draw.</param>
        /// <param name="drawingLevel">DrawingLevel the image draws on.</param>
        public void DrawWorldImage(GeoImage image, double centerXInWorld, double centerYInWorld, float widthInScreen,
            float heightInScreen, DrawingLevel drawingLevel)
        {
            Validators.CheckParameterIsNotNull(image, "image");
            Validators.CheckGeoCanvasIsInDrawing(IsDrawing);
            Validators.CheckDrawingLevelIsValid(drawingLevel, "drawingLevel");
            Validators.CheckIfInputValueIsBiggerThan(widthInScreen, "widthInScreen", 0,
                RangeCheckingInclusion.IncludeValue);
            Validators.CheckIfInputValueIsBiggerThan(heightInScreen, "heightInScreen", 0,
                RangeCheckingInclusion.IncludeValue);


            _hasDrawn = true;

            DrawWorldImage(image, centerXInWorld, centerYInWorld, widthInScreen, heightInScreen, drawingLevel, 0, 0, 0);
        }


        /// <summary>Draws a scaled image on the canvas.</summary>
        /// <remarks>Drawing an image scaled is slower than drawing it unscaled.</remarks>
        /// <param name="image">Image to draw.</param>
        /// <param name="centerXInWorld">X coordinate of the center point of the image to draw.</param>
        /// <param name="centerYInWorld">Y coordinate of the center point of the image to draw.</param>
        /// <param name="imageScale">Scale of the image from original size. Width and height are readjusted.</param>
        /// <param name="drawingLevel">DrawingLevel the image draws on.</param>
        public void DrawWorldImage(GeoImage image, double centerXInWorld, double centerYInWorld, double imageScale,
            DrawingLevel drawingLevel, float xOffset, float yOffset, float rotateAngle)
        {
            Validators.CheckParameterIsNotNull(image, "image");
            Validators.CheckGeoCanvasIsInDrawing(IsDrawing);
            Validators.CheckDrawingLevelIsValid(drawingLevel, "drawingLevel");
            Validators.CheckIfInputValueIsBiggerThan(imageScale, "imageScale", 0, RangeCheckingInclusion.ExcludeValue);

            var newWidth = (float) (image.GetWidth()*imageScale);
            var newHeight = (float) (image.GetHeight()*imageScale);

            DrawWorldImage(image, centerXInWorld, centerYInWorld, newWidth, newHeight, DrawingLevel.LevelOne, xOffset,
                yOffset, rotateAngle);

            _hasDrawn = true;
        }

        /// <summary>Draws a scaled image on the canvas.</summary>
        /// <remarks>Drawing an image scaled is slower than drawing it unscaled.</remarks>
        /// <param name="image">The image you want to draw.</param>
        /// <param name="centerXInWorld">X coordinate of the center point of the image.</param>
        /// <param name="centerYInWorld">Y coordinate of the center point of the image.</param>
        /// <param name="widthInScreen">Width of the image to draw.</param>
        /// <param name="heightInScreen">Height of the image to draw.</param>
        /// <param name="drawingLevel">DrawingLevel the image draws on.</param>
        /// <param name="xOffset">X offset in screen coordinates of the image to be drawn.</param>
        /// <param name="yOffset">Y offset in screen coordinates of the image to be drawn.</param>
        /// <param name="rotateAngle">Rotation angle of the image to be drawn.</param>
        public void DrawWorldImage(GeoImage image, double centerXInWorld, double centerYInWorld, float widthInScreen,
            float heightInScreen, DrawingLevel drawingLevel, float xOffset, float yOffset, float rotateAngle)
        {
            Validators.CheckParameterIsNotNull(image, "image");
            Validators.CheckGeoCanvasIsInDrawing(IsDrawing);
            Validators.CheckDrawingLevelIsValid(drawingLevel, "drawingLevel");
            Validators.CheckIfInputValueIsBiggerThan(widthInScreen, "widthInScreen", 0,
                RangeCheckingInclusion.IncludeValue);
            Validators.CheckIfInputValueIsBiggerThan(heightInScreen, "heightInScreen", 0,
                RangeCheckingInclusion.IncludeValue);


            _hasDrawn = true;

            var upperLeftX = CurrentWorldExtent.UpperLeftPoint.X;
            var upperLeftY = CurrentWorldExtent.UpperLeftPoint.Y;

            var screenX = (float) ((centerXInWorld - upperLeftX)*_geoCanvasWidth/CurrentWorldExtent.Width);
            var screenY = (float) ((upperLeftY - centerYInWorld)*_geoCanvasHeight/CurrentWorldExtent.Height);

            if (!IsCancelled)
            {
                DrawScreenImageCore(image, screenX, screenY, widthInScreen, heightInScreen, drawingLevel, xOffset,
                    yOffset, rotateAngle);
            }
        }

        /// <summary>Draws an image based on screen coordinates on the canvas.</summary>
        /// <remarks>Drawing a scaled image is slower than at original size.</remarks>
        /// <param name="image">Image to draw.</param>
        /// <param name="centerXInScreen">X coordinate of the center point (in screen coordinates) of the image.</param>
        /// <param name="centerYInScreen">Y coordinate of the center point (in screen coordinates) of the image.</param>
        /// <param name="widthInScreen">Width of the image.</param>
        /// <param name="heightInScreen">Height of the image.</param>
        /// <param name="drawingLevel">DrawingLevel the image draws on.</param>
        /// <param name="xOffset">X offset in screen coordinates of the image to be drawn.</param>
        /// <param name="yOffset">Y offset in screen coordinates of the image to be drawn.</param>
        /// <param name="rotateAngle">Rotation angle of the image to be drawn.</param>
        public void DrawScreenImage(GeoImage image, float centerXInScreen, float centerYInScreen, float widthInScreen,
            float heightInScreen, DrawingLevel drawingLevel, float xOffset, float yOffset, float rotateAngle)
        {
            if (!IsCancelled)
            {
                DrawScreenImageCore(image, centerXInScreen, centerYInScreen, widthInScreen, heightInScreen, drawingLevel,
                    xOffset, yOffset, rotateAngle);
            }
        }

        /// <summary>Draws an image based on screen coordinates on the canvas.</summary>
        /// <remarks>Drawing a scaled image is slower than at original size.</remarks>
        /// <param name="image">Image to draw.</param>
        /// <param name="centerXInScreen">X coordinate of the center point (in screen coordinates) of the image.</param>
        /// <param name="centerYInScreen">Y coordinate of the center point (in screen coordinates) of the image.</param>
        /// <param name="widthInScreen">Width of the image.</param>
        /// <param name="heightInScreen">Height of the image.</param>
        /// <param name="drawingLevel">DrawingLevel the image draws on.</param>
        /// <param name="xOffset">X offset in screen coordinates of the image to be drawn.</param>
        /// <param name="yOffset">Y offset in screen coordinates of the image to be drawn.</param>
        /// <param name="rotateAngle">Rotation angle of the image to be drawn.</param>
        protected abstract void DrawScreenImageCore(GeoImage image, float centerXInScreen, float centerYInScreen,
            float widthInScreen, float heightInScreen, DrawingLevel drawingLevel, float xOffset, float yOffset,
            float rotateAngle);

        /// <summary>
        /// 	<para>Draws text based on brush and font.</para>
        /// </summary>
        /// <overloads>Specifies location of text in screen coordinates.</overloads>
        /// <remarks>
        /// 	<para>Draws text on the GeoCanvas at specific screen coordinates. Usefull for legends, titles, etc.</para>
        /// 	<para>DrawingLevel allows to specify the drawing level with multiple text items. For example, for drop shadow using offsets.</para>
        /// </remarks>
        /// <param name="text">This parameter specifies the text you wish to draw.</param>
        /// <param name="font">This parameter represents the font you wish to use to draw the text.</param>
        /// <param name="fillBrush"> Color fill of the text.</param>
        /// <param name="upperLeftXInScreen">Upper left horizontal point in screen coordinates from where to start drawing the text. </param>
        /// <param name="upperLeftYInScreen">Upper left vertical point in screen coordinates from where to start drawing the text.</param>
        /// <param name="drawingLevel">Drawing level for the text.</param>
        public void DrawTextWithScreenCoordinate(string text, GeoFont font, BaseGeoBrush fillBrush, float upperLeftXInScreen,
            float upperLeftYInScreen, DrawingLevel drawingLevel)
        {
            DrawTextWithScreenCoordinate(text, font, fillBrush, null, upperLeftXInScreen, upperLeftYInScreen,
                drawingLevel);
        }

        /// <summary>
        /// 	<para>Draws text based on brush and font.</para>
        /// </summary>
        /// <overloads>Specifies location of text in screen coordinates.</overloads>
        /// <remarks>
        /// 	<para>Draws text on the GeoCanvas at specific screen coordinates. Usefull for legends, titles, etc.</para>
        /// 	<para>DrawingLevel allows to specify the drawing level with multiple text items. For example, for drop shadow using offsets.</para>
        /// </remarks>
        /// <param name="text">This parameter specifies the text you wish to draw.</param>
        /// <param name="font">This parameter represents the font you wish to use to draw the text.</param>
        /// <param name="fillBrush"> Color fill of the text.</param>
        /// <param name="haloPen"> Halo of the text.</param>
        /// <param name="upperLeftXInScreen">Upper left horizontal point in screen coordinates from where to start drawing the text. </param>
        /// <param name="upperLeftYInScreen">Upper left vertical point in screen coordinates from where to start drawing the text.</param>
        /// <param name="drawingLevel">Drawing level for the text.</param>
        public void DrawTextWithScreenCoordinate(string text, GeoFont font, BaseGeoBrush fillBrush, GeoPen haloPen,
            float upperLeftXInScreen, float upperLeftYInScreen, DrawingLevel drawingLevel)
        {
            Validators.CheckParameterIsNotNull(fillBrush, "fillBrush");
            Validators.CheckParameterIsNotNull(text, "text");
            Validators.CheckParameterIsNotNull(font, "font");
            Validators.CheckGeoCanvasIsInDrawing(IsDrawing);
            Validators.CheckDrawingLevelIsValid(drawingLevel, "drawingLevel");
            Validators.CheckParameterIsNotNullOrEmpty(text, "text");

            _hasDrawn = true;

            ScreenPointF[] screenPoints = {new ScreenPointF(upperLeftXInScreen, upperLeftYInScreen)};
            if (!IsCancelled)
            {
                DrawText(text, font, fillBrush, haloPen, screenPoints, drawingLevel, 0, 0, 0);
            }
        }

        /// <summary>
        /// 	<para>Draws text with brush and font using world coordinates.</para>
        /// </summary>
        /// <overloads>Specifies the location in world coordinates.</overloads>
        /// <remarks>
        /// 	<para>Draws text on the GeoCanvas at specific screen coordinates.</para>
        /// 	<para>The DrawingLevel allows to specify the drawing level with multiple text items. For example for drop shadow using offsets.</para>
        /// </remarks>
        /// <param name="text">Text to write on the map.</param>
        /// <param name="font">Font of the text.</param>
        /// <param name="fillBrush">Color fill of the text.</param>
        /// <param name="upperLeftXInWorld">Upper left horizontal point in world coordinates from where to start drawing the text. </param>
        /// <param name="upperLeftYInWorld">Upper left horizontal point in world coordinates from where to start drawing the text.</param>
        /// <param name="drawingLevel">Drawing level of the text. Higher.</param>
        public void DrawTextWithWorldCoordinate(string text, GeoFont font, BaseGeoBrush fillBrush, double upperLeftXInWorld,
            double upperLeftYInWorld, DrawingLevel drawingLevel)
        {
            DrawTextWithWorldCoordinate(text, font, fillBrush, null, upperLeftXInWorld, upperLeftYInWorld, drawingLevel);
        }

        /// <summary>
        /// 	<para>Draws text with brush and font using world coordinates.</para>
        /// </summary>
        /// <overloads>Specifies the location in world coordinates.</overloads>
        /// <remarks>
        /// 	<para>Draws text on the GeoCanvas at specific screen coordinates.</para>
        /// 	<para>The DrawingLevel allows to specify the drawing level with multiple text items. For example for drop shadow using offsets.</para>
        /// </remarks>
        /// <param name="text">Text to write on the map.</param>
        /// <param name="font">Font of the text.</param>
        /// <param name="fillBrush">Color fill of the text.</param>
        /// <param name="haloPen">Halo of the text.</param>
        /// <param name="upperLeftXInWorld">Upper left horizontal point in world coordinates from where to start drawing the text. </param>
        /// <param name="upperLeftYInWorld">Upper left horizontal point in world coordinates from where to start drawing the text.</param>
        /// <param name="drawingLevel">Drawing level of the text. Higher.</param>
        public void DrawTextWithWorldCoordinate(string text, GeoFont font, BaseGeoBrush fillBrush, GeoPen haloPen,
            double upperLeftXInWorld, double upperLeftYInWorld, DrawingLevel drawingLevel)
        {
            Validators.CheckGeoCanvasIsInDrawing(IsDrawing);
            Validators.CheckDrawingLevelIsValid(drawingLevel, "drawingLevel");
            Validators.CheckParameterIsNotNull(fillBrush, "fillBrush");
            Validators.CheckParameterIsNotNull(text, "text");
            Validators.CheckParameterIsNotNull(font, "font");
            Validators.CheckParameterIsNotNullOrEmpty(text, "text");

            var upperLeftX = _currentExtent.UpperLeftPoint.X;
            var upperLeftY = _currentExtent.UpperLeftPoint.Y;
            var currentExtentWidth = _currentExtent.Width;
            var currentExtentHeight = _currentExtent.Height;

            var screenX = (upperLeftXInWorld - upperLeftX)*_geoCanvasWidth/currentExtentWidth;
            var screenY = (upperLeftY - upperLeftYInWorld)*_geoCanvasHeight/currentExtentHeight;

            ScreenPointF[] screenPoints = {new ScreenPointF((float) screenX, (float) screenY)};
            if (!IsCancelled)
            {
                DrawText(text, font, fillBrush, haloPen, screenPoints, drawingLevel, 0, 0, 0);
            }

            _hasDrawn = true;
        }

        /// <summary>
        /// 	<para>Draws text with brush and font.</para>
        /// </summary>
        /// <param name="text">Text to write on the map.</param>
        /// <param name="font">Font of the text.</param>
        /// <param name="fillBrush">Color fill of the text.</param>
        /// <param name="textPathInScreen">Path of the text to write on the map.</param>
        /// <param name="drawingLevel">Drawing level of the text.</param>
        public void DrawText(string text, GeoFont font, BaseGeoBrush fillBrush, IEnumerable<ScreenPointF> textPathInScreen,
            DrawingLevel drawingLevel)
        {
            Validators.CheckParameterIsNotNull(fillBrush, "fillBrush");
            Validators.CheckParameterIsNotNull(text, "text");
            Validators.CheckParameterIsNotNull(font, "font");
            Validators.CheckGeoCanvasIsInDrawing(IsDrawing);
            Validators.CheckDrawingLevelIsValid(drawingLevel, "drawingLevel");
            Validators.CheckParameterIsNotNullOrEmpty(text, "text");

            _hasDrawn = true;
            if (!IsCancelled)
            {
                DrawText(text, font, fillBrush, null, textPathInScreen, drawingLevel, 0, 0, 0);
            }
        }

        /// <summary>
        /// 	<para>Draws text with brush and font.</para>
        /// </summary>
        /// <param name="text">Text to write on the map.</param>
        /// <param name="font">Font of the text.</param>
        /// <param name="fillBrush">Color fill of the text.</param>
        /// <param name="haloPen">Pen for halo effect on the text.</param>
        /// <param name="textPathInScreen">Path of the text to write on the map.</param>
        /// <param name="drawingLevel">Drawing level of the text</param>
        /// <param name="xOffset">X offset in world coordinates of the text.</param>
        /// <param name="yOffset">Y offset in world coordinates of the text.</param>
        /// <param name="rotateAngle">Rotation angle of the text.</param>
        public void DrawText(string text, GeoFont font, BaseGeoBrush fillBrush, GeoPen haloPen,
            IEnumerable<ScreenPointF> textPathInScreen, DrawingLevel drawingLevel, float xOffset, float yOffset,
            float rotateAngle)
        {
            Validators.CheckParameterIsNotNull(fillBrush, "fillBrush");
            Validators.CheckParameterIsNotNull(text, "text");
            Validators.CheckParameterIsNotNull(font, "font");
            Validators.CheckGeoCanvasIsInDrawing(IsDrawing);
            Validators.CheckDrawingLevelIsValid(drawingLevel, "drawingLevel");
            Validators.CheckParameterIsNotNullOrEmpty(text, "text");

            _hasDrawn = true;
            if (!IsCancelled)
            {
                DrawTextCore(text, font, fillBrush, haloPen, textPathInScreen, drawingLevel, xOffset, yOffset,
                    rotateAngle);
            }
        }

        /// <summary>
        /// 	<para>Draws text with brush and font.</para>
        /// </summary>
        /// <param name="text">Text to write on the map.</param>
        /// <param name="font">Font of the text.</param>
        /// <param name="fillBrush">Color fill of the text.</param>
        /// <param name="haloPen">Pen for halo effect on the text.</param>
        /// <param name="textPathInScreen">Path of the text to write on the map.</param>
        /// <param name="drawingLevel">Drawing level of the text</param>
        /// <param name="xOffset">X offset in world coordinates of the text.</param>
        /// <param name="yOffset">Y offset in world coordinates of the text.</param>
        /// <param name="rotateAngle">Rotation angle of the text.</param>
        protected abstract void DrawTextCore(string text, GeoFont font, BaseGeoBrush fillBrush, GeoPen haloPen,
            IEnumerable<ScreenPointF> textPathInScreen, DrawingLevel drawingLevel, float xOffset, float yOffset,
            float rotateAngle);

        /// <summary>Returns the rectangle containing a text.</summary>
        /// <returns>A rectangle containing a text taking into account the font.</returns>
        /// <remarks>Typically used for labeling logic to determine the overlapping of labels.</remarks>
        /// <param name="text">Text to measure.</param>
        /// <param name="font">Font of the text to measure.</param>
        public DrawingRectangleF MeasureText(string text, GeoFont font)
        {
            Validators.CheckParameterIsNotNull(text, "text");
            Validators.CheckParameterIsNotNull(font, "font");
            Validators.CheckParameterIsNotNullOrEmpty(text, "text");

            return MeasureTextCore(text, font);
        }

        /// <summary>Returns the rectangle containing a text.</summary>
        /// <returns>A rectangle containing a text taking into account the font.</returns>
        /// <remarks>Typically used for labeling logic to determine the overlapping of labels.</remarks>
        /// <param name="text">Text to measure.</param>
        /// <param name="font">Font of the text to measure.</param>
        protected abstract DrawingRectangleF MeasureTextCore(string text, GeoFont font);

        /// <summary>Converts a GeoImage to a commonly-used object. For example, in GdiPlus the object is a Bitmap.</summary>
        /// <remarks>BaseClass method to be implemented in the sub-concrete classes.</remarks>
        /// <param name="image">Target geoImage to convert.</param>
        /// <returns>Object</returns>
        public object ToNativeImage(GeoImage image)
        {
            Validators.CheckParameterIsNotNull(image, "image");

            return ToNativeImageCore(image);
        }

        /// <summary>Converts a GeoImage to a commonly-used object. For example, in GdiPlus the object is a Bitmap.</summary>
        /// <remarks>BaseClass method to be implemented in sub-concrete classes.</remarks>
        /// <param name="image">Target geoImage to convert.</param>
        /// <returns>Object</returns>
        protected abstract object ToNativeImageCore(GeoImage image);

        /// <summary>Converts an object to a GeoImage. For example, in GdiPlus  object is a Bitmap.</summary>
        /// <remarks>BaseClass method to be implemented in sub-concrete classes.
        /// </remarks>
        /// <param name="nativeImage">Target object to convert.</param>
        /// <returns>GeoImage containing.</returns>
        public GeoImage ToGeoImage(object nativeImage)
        {
            Validators.CheckParameterIsNotNull(nativeImage, "nativeImage");

            return ToGeoImageCore(nativeImage);
        }

        /// <summary>Converts an object to a GeoImage. For example, in GdiPlus  object is a Bitmap.</summary>
        /// <remarks>BaseClass method to be implemented in the sub-concrete classes.
        /// </remarks>
        /// <param name="nativeImage">Target object to convert.</param>
        /// <returns>GeoImage containing.</returns>
        protected abstract GeoImage ToGeoImageCore(object nativeImage);

        /// <summary>Gets canvas width of the native image object.</summary>
        /// <remarks>BaseClass method to be implemented in sub-concrete classes.</remarks>
        /// <param name="nativeImage">Native image to get the width.</param>
        /// <returns>Canvas width.</returns>
        protected float GetCanvasWidth(object nativeImage)
        {
            return GetCanvasWidthCore(nativeImage);
        }

        /// <summary>Gets the canvas width of the passed-in native image object.</summary>
        /// <remarks>BaseClass method to be implemented in sub-concrete classes.</remarks>
        /// <param name="nativeImage">Native image used to get the width.</param>
        /// <returns>Canvas width.</returns>
        protected abstract float GetCanvasWidthCore(object nativeImage);

        /// <summary>Gets the canvas height of native image object.</summary>
        /// <remarks>BaseClass method to be implemented in sub-concrete classes.</remarks>
        /// <param name="nativeImage">Native image used to get the height.</param>
        /// <returns>Canvas height.</returns>
        protected float GetCanvasHeight(object nativeImage)
        {
            return GetCanvasHeightCore(nativeImage);
        }

        /// <summary>Gets the canvas height of native image object.</summary>
        /// <remarks>BaseClass method to be implemented in sub-concrete classes.</remarks>
        /// <param name="nativeImage">Native image used to get the height.</param>
        /// <returns>Canvas height.</returns>
        protected abstract float GetCanvasHeightCore(object nativeImage);


        private List<ScreenPointF[]> GetScreenPointsFromPolygon(byte[] areaShapeWkb)
        {
            var rings = new List<ScreenPointF[]>();

            var byteOrder = areaShapeWkb[PositionByteOrder];

            var shapeType = ArrayHelper.GetShapeTypeFromByteArray(areaShapeWkb, PositionShapeType);

            if (shapeType != WkbShapeType.Polygon)
            {
                throw new ArgumentException(ExceptionDescription.WkbIsNotValidForDrawing, "areaShapeWkb");
            }

            var numberOfRings = ArrayHelper.GetIntFromByteArray(areaShapeWkb, PositionCount, byteOrder);
            var dataIndex = PositionData;

            for (var i = 0; i < numberOfRings; i++)
            {
                ScreenPointF[] points = null;
                dataIndex = AddRingForPolygon(ref points, areaShapeWkb, dataIndex, byteOrder);
                rings.Add(points);
            }

            return rings;
        }

        private int AddRingForPolygon(ref ScreenPointF[] areaPoints, byte[] wkb, int dataIndex, byte byteOrder)
        {
            var upperLeftX = CurrentWorldExtent.UpperLeftPoint.X;
            var upperLeftY = CurrentWorldExtent.UpperLeftPoint.Y;

            var numberOfPoints = ArrayHelper.GetIntFromByteArray(wkb, dataIndex, byteOrder);
            dataIndex += SizeOfInt;

            var points = new ScreenPointF[numberOfPoints];

            if (byteOrder == 1)
            {
                for (var i = 0; i < points.Length; i++)
                {
                    var worldX = BitConverter.ToDouble(wkb, dataIndex);
                    var worldY = BitConverter.ToDouble(wkb, dataIndex + SizeOfDouble);
                    var screenX = (float) ((worldX - upperLeftX)*_worldToScreenFactorX);
                    var screenY = (float) ((upperLeftY - worldY)*_worldToScreenFactorY);
                    points[i] = new ScreenPointF(screenX, screenY);
                    dataIndex += SizeOfPoint;
                }
            }
            else
            {
                for (var i = 0; i < points.Length; i++)
                {
                    var worldX = ArrayHelper.GetDoubleFromByteArray(wkb, dataIndex, byteOrder);
                    var worldY = ArrayHelper.GetDoubleFromByteArray(wkb, dataIndex + SizeOfDouble, byteOrder);
                    var screenX = (float) ((worldX - upperLeftX)*_worldToScreenFactorX);
                    var screenY = (float) ((upperLeftY - worldY)*_worldToScreenFactorY);
                    points[i] = new ScreenPointF(screenX, screenY);
                    dataIndex += SizeOfPoint;
                }
            }

            areaPoints = points;

            return dataIndex;
        }

        private int DrawLine(byte[] lineShapeWkb, int startIndex, float xOffset, float yOffset,
            DrawingLevel drawingLevel, GeoPen linePen)
        {
            var byteOrder = lineShapeWkb[startIndex + PositionByteOrder];

            var shapeType = ArrayHelper.GetShapeTypeFromByteArray(lineShapeWkb, startIndex + PositionShapeType);

            if (shapeType != WkbShapeType.LineString)
            {
                throw new ArgumentException(ExceptionDescription.WkbIsNotValidForDrawing, "lineShapeWkb");
            }

            var numberOfPoints = ArrayHelper.GetIntFromByteArray(lineShapeWkb, startIndex + PositionCount, byteOrder);
            var dataIndex = startIndex + PositionData;

            var upperLeftX = CurrentWorldExtent.UpperLeftPoint.X;
            var upperLeftY = CurrentWorldExtent.UpperLeftPoint.Y;

            var worldX = ArrayHelper.GetDoubleFromByteArray(lineShapeWkb, dataIndex, byteOrder);
            var worldY = ArrayHelper.GetDoubleFromByteArray(lineShapeWkb, dataIndex + SizeOfDouble, byteOrder);

            dataIndex += SizeOfPoint;

            var screenX = (worldX - upperLeftX)*_worldToScreenFactorX;
            var screenY = (upperLeftY - worldY)*_worldToScreenFactorY;

            var points = new List<ScreenPointF>();
            points.Add(new ScreenPointF((float) screenX, (float) screenY));

            for (var i = 1; i < numberOfPoints; i++)
            {
                worldX = ArrayHelper.GetDoubleFromByteArray(lineShapeWkb, dataIndex, byteOrder);
                worldY = ArrayHelper.GetDoubleFromByteArray(lineShapeWkb, dataIndex + SizeOfDouble, byteOrder);
                screenX = (worldX - upperLeftX)*_worldToScreenFactorX;
                screenY = (upperLeftY - worldY)*_worldToScreenFactorY;
                points.Add(new ScreenPointF((float) screenX, (float) screenY));
                dataIndex += SizeOfPoint;
            }
            if (!IsCancelled)
            {
                DrawLineCore(points, linePen, drawingLevel, xOffset, yOffset);
            }

            return dataIndex;
        }

        private int DrawCircle(byte[] centerPointWkb, int startIndex, float width, float height, GeoPen outlinePen,
            BaseGeoBrush fillBrush, DrawingLevel drawingLevel, float xOffset, float yOffset,
            PenBrushDrawingOrder penBrushDrawingOrder)
        {
            var byteOrder = centerPointWkb[startIndex + PositionByteOrder];

            var shapeType = ArrayHelper.GetShapeTypeFromByteArray(centerPointWkb, startIndex + PositionShapeType);

            if (shapeType != WkbShapeType.Point)
            {
                throw new ArgumentException(ExceptionDescription.WkbIsNotValidForDrawing, "centerPointWkb");
            }

            var upperLeftX = CurrentWorldExtent.UpperLeftPoint.X;
            var upperLeftY = CurrentWorldExtent.UpperLeftPoint.Y;

            startIndex += PositionPointData;

            var worldX = ArrayHelper.GetDoubleFromByteArray(centerPointWkb, startIndex, byteOrder);
            var worldY = ArrayHelper.GetDoubleFromByteArray(centerPointWkb, startIndex + SizeOfDouble, byteOrder);
            startIndex += SizeOfPoint;

            var screenX = (float) ((worldX - upperLeftX)*_worldToScreenFactorX);
            var screenY = (float) ((upperLeftY - worldY)*_worldToScreenFactorY);

            if (!IsCancelled)
            {
                if (_enableCliping)
                {
                    var ellips = new EllipseShape(new PointShape(centerPointWkb), width, height);
                    var drawingShape = ellips.ToPolygon();

                    DrawArea(drawingShape, outlinePen, fillBrush, drawingLevel, xOffset, yOffset, penBrushDrawingOrder);
                }
                else
                {
                    DrawEllipseCore(new ScreenPointF(screenX, screenY), width, height, outlinePen, fillBrush,
                        drawingLevel, xOffset, yOffset, penBrushDrawingOrder);
                }
            }

            return startIndex;
        }

        /// <summary>Returns stream representing the GeoImage in TIFF format.</summary>
        /// <returns>Stream that represents the GeoImage in TIFF format.</returns>
        /// <remarks>Allows to get the stream out of a GeoImage. The stream is a
        /// memory stream and the bytes are in TIFF format. Can be saved in the desired format.</remarks>
        /// <param name="image">GeoImage to convert to a stream.</param>
        public abstract Stream GetStreamFromGeoImage(GeoImage image);

        /// <summary>Flushes drawing and commits the drawing on the canvas.</summary>
        /// <remarks>Method called when drawing finished. It commits the
        /// image changes to the image passed in on BeginDrawing. Also it sets IsDrawing to false. Finally it sets GeoCanvas to invalid state, 
        /// not allowing further drawing.</remarks>
        public void Flush()
        {
            FlushCore();
        }

        /// <summary>Flushes drawing and commits the drawing on the canvas.</summary>
        /// <remarks>Method to call when drawing finished. It commits the
        /// image changes to the image passed in on BeginDrawing. Also it sets IsDrawing to false. Finally it sets GeoCanvas to invalid state, 
        /// not allowing further drawing.</remarks>
        protected abstract void FlushCore();

        /// <summary>Raised when drawing progress is changed.</summary>
        protected virtual void OnDrawingProgressChanged(ProgressEventArgs e)
        {
            _progressDrawingRaisedCount++;
            if (_progressDrawingRaisedCount == _progressDrawingRaisingFrenquency)
            {
                _progressDrawingRaisedCount = 0;
                var handler = DrawingProgressChanged;
                if (handler != null)
                {
                    handler(this, e);
                }
            }
        }
        
       
        internal RectangleShape ToWorldCoordinate(DrawingRectangleF drawingRectangle)
        {
            var upperLeftPoint = ExtentHelper.ToWorldCoordinate(CurrentWorldExtent,
                drawingRectangle.CenterX - drawingRectangle.Width/2,
                drawingRectangle.CenterY - drawingRectangle.Height/2, Width, Height);
            var lowerRightPoint = ExtentHelper.ToWorldCoordinate(CurrentWorldExtent,
                drawingRectangle.CenterX + drawingRectangle.Width/2,
                drawingRectangle.CenterY + drawingRectangle.Height/2, Width, Height);

            return new RectangleShape(upperLeftPoint, lowerRightPoint);
        }
    }
}
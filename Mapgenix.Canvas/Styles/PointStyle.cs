using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Mapgenix.Shapes;
using Mapgenix.Utils;

namespace Mapgenix.Canvas
{
    /// <summary>Style for drawing point based features</summary>
    /// <remarks>There are 3 main types: bitmap, character, and symbol. 
    /// The bitmap type represents point with a bitmap. The character type chooses a font and an
    /// index in the font to represent a point. The symbol type has a collection of simple geometric symbols such as triangles, circles, squares 
    /// to represent points.</remarks>
    [Serializable]
    public class PointStyle : BaseStyle
    {
        private const int Diamond2CharacterIndex = 117;
        private const int DiamondCharacterIndex = 116;
        private const int FirstPointPosition = 13;
        private const int PositionByteOrder = 0;
        private const int PositionCount = 5;
        private const int PositionData = 9;
        private const int PositionPointData = 5;
        private const int PositionShapeType = 1;
        private const double RadientPenDegree = Math.PI/180;
        private const int SizeOfDouble = 8;
        private const int SizeOfPoint = 16;
        private const int Star2CharacterIndex = 181;
        private const int StarCharacterIndex = 171;
        private readonly PointStyleCustom _pointStyleCustom;
        private readonly Collection<PointStyle> _customPointStyles;
        private readonly Dictionary<float, GeoFont> _wingdingsFontCache = new Dictionary<float, GeoFont>();
        private GeoFont _characterFont;
        private int _characterIndex;
        private GeoSolidBrush _characterSolidBrush;
        private DrawingLevel _drawingLevel;
        private GeoImage _image;
        private double _imageScale;

        private PointSymbolType _pointSymbolType;
        private PointType _pointType;
        private float _rotateAngle;
        private GeoPen _symbolPen;
        private float _symbolSize;
        private GeoSolidBrush _symbolSolidBrush;

        private float _xOffsetInPixel;
        private float _yOffsetInPixel;

        /// <summary>Constructor of the class.</summary>
        /// <overloads>Default constructor. Set the properties manually.</overloads>
        /// <returns>None</returns>
        public PointStyle()
            : this(
                null, new GeoFont(), 0, new GeoSolidBrush(), PointType.Symbol, PointSymbolType.Circle,
                new GeoSolidBrush(), new GeoPen(), 3)
        {
        }

        /// <summary>Constructor of the class.</summary>
        /// <overloads>For bitmap based points.</overloads>
        /// <returns>None</returns>
        /// <remarks>PointStyle gets set to Bitmap.</remarks>
        /// <param name="image">GeoImage of the image to display the point.</param>
        public PointStyle(GeoImage image)
            : this(
                image, new GeoFont(), 0, new GeoSolidBrush(), PointType.Bitmap, PointSymbolType.Circle,
                new GeoSolidBrush(), new GeoPen(), 3)
        {
        }

        /// <summary>Constructor of the class.</summary>
        /// <overloads>For character based points.</overloads>
        /// <returns>None</returns>
        /// <remarks>PointStyle gets set to Character.</remarks>
        /// <param name="characterFont">Font from which the character will be selected.</param>
        /// <param name="characterIndex">Index of the character in the font.</param>
        /// <param name="characterSolidBrush">SolidBrush to draw the character.</param>
        public PointStyle(GeoFont characterFont, int characterIndex, GeoSolidBrush characterSolidBrush)
             : this(
                null, characterFont, characterIndex, characterSolidBrush, PointType.Character, PointSymbolType.Circle,
                new GeoSolidBrush(), new GeoPen(), 3)
        {
        }

        /// <summary>Constructor of the class.</summary>
        /// <overloads>For symbol-based points (squares, circles, triangles etc) with a fill.</overloads>
        /// <returns>None</returns>
        /// <remarks>PointStyle gets set to Symbol.</remarks>
        /// <param name="symbolType">Type of symbol (square, circle, triangle etc)</param>
        /// <param name="symbolSolidBrush">SolidBrush to fill the inside of the symbol.</param>
        /// <param name="symbolSize">Size of the symbol.</param>
        public PointStyle(PointSymbolType symbolType, GeoSolidBrush symbolSolidBrush, int symbolSize)
            : this(
                null, new GeoFont(), 0, new GeoSolidBrush(), PointType.Symbol, symbolType, symbolSolidBrush,
                new GeoPen(), symbolSize)
        {
        }

        /// <summary>Constructor of the class.</summary>
        /// <overloads>For symbol-based points (square, circle, triangle etc) with  fill and outline.</overloads>
        /// <returns>None</returns>
        /// <remarks>PointStyle gets set to Symbol.</remarks>
        /// <param name="symbolType">Type of symbol.</param>
        /// <param name="symbolSolidBrush">SolidBrush to fill the inside of the symbol.</param>
        /// <param name="symbolPen">Pen to draw the outline of the symbol.</param>
        /// <param name="symbolSize">Size of the symbol.</param>
        public PointStyle(PointSymbolType symbolType, GeoSolidBrush symbolSolidBrush, GeoPen symbolPen, int symbolSize)
            : this(
                null, new GeoFont(), 0, new GeoSolidBrush(), PointType.Symbol, symbolType, symbolSolidBrush, symbolPen,
                symbolSize)
        {
        }

        private PointStyle(GeoImage image, GeoFont characterFont, int characterIndex, GeoSolidBrush characterSolidBrush,
            PointType pointType, PointSymbolType pointSymbolType, GeoSolidBrush symbolSolidBrush, GeoPen symbolPen,
            int symbolSize)
        {
            _image = image;
            _characterFont = characterFont;
            _characterIndex = characterIndex;
            _characterSolidBrush = characterSolidBrush;
            _pointType = pointType;
            _pointSymbolType = pointSymbolType;
            _symbolSolidBrush = symbolSolidBrush;
            _symbolPen = symbolPen;
            _symbolSize = symbolSize;
            _imageScale = 1;
            _pointStyleCustom = new PointStyleCustom();
            _customPointStyles = new Collection<PointStyle>();
            _drawingLevel = DrawingLevel.LevelOne;
        }

        /// <summary>Gets and sets the X offset in pixel unit for the features to draw.</summary>
        /// <value>X offset in pixel unit for the features to draw.</value>
        /// <remarks>Combining with a Y offset, it allows effects such as drop shadows.</remarks>
        public float XOffsetInPixel
        {
            get { return _xOffsetInPixel; }
            set { _xOffsetInPixel = value; }
        }

        /// <summary>Gets and sets the Y offset in pixel unit for the features to draw.</summary>
        /// <value>Y offset in pixel unit for the features to draw.</value>
        /// <remarks>Combining with a X offset, it allows effects such as drop shadows.</remarks>
        public float YOffsetInPixel
        {
            get { return _yOffsetInPixel; }
            set { _yOffsetInPixel = value; }
        }

        /// <summary>Returns a collection of point styles to stack multiple point styles on top of each other.</summary>
        /// <value>Collection of point styles.</value>
        /// <remarks>Use these stacks to create drop shadow effects, multiple colored outlines, etc.</remarks>
        public Collection<PointStyle> CustomPointStyles
        {
            get { return _customPointStyles; }
        }

        /// <summary>Gets and sets the type of point to draw.</summary>
        /// <value>Type of point to draw.</value>
        /// <remarks>There are 3 main types: bitmap, character, and symbol. 
        /// The bitmap type represents point with a bitmap. The character type chooses a font and an
        /// index in the font to represent a point. The symbol type has a collection of simple geometric symbols such as triangles, circles, squares 
        /// to represent points.</remarks>
        public PointType PointType
        {
            get { return _pointType; }
            set { _pointType = value; }
        }

        /// <summary>Gets and sets the rotation angle of the point style.</summary>
        /// <value>Rotation angle of the point style.</value>
        /// <remarks>None</remarks>
        public float RotationAngle
        {
            get { return _rotateAngle; }
            set { _rotateAngle = value; }
        }

        /// <summary>Gets and sets the image when PointType property is Bitmap.</summary>
        /// <value>Image when PointType property is Bitmap.</value>
        /// <remarks>Uses a GeoImage either referencing a file or supplying a stream.</remarks>
        public GeoImage Image
        {
            get { return _image; }
            set { _image = value; }
        }

        /// <summary>Gets and sets the scale of the image to draw.</summary>
        /// <value>Gets the scale of the image to draw.</value>
        /// <remarks>Allows to scale the image up and down.</remarks>
        public double ImageScale
        {
            get { return _imageScale; }
            set { _imageScale = value; }
        }

        /// <summary>Gets and sets the font for the character if the PointType is Character.</summary>
        /// <value>Font for the character if the PointType is Character.</value>
        /// <remarks>Allows to set the font from which to select a character index.</remarks>
        public GeoFont CharacterFont
        {
            get { return _characterFont; }
            set { _characterFont = value; }
        }

        /// <summary>Gets and sets the index position of the character to use from the selected font in CharacterFont property.</summary>
        /// <value>Index position of the character to use from the selected font in CharacterFont property.</value>
        public int CharacterIndex
        {
            get { return _characterIndex; }
            set { _characterIndex = value; }
        }

        /// <summary>Gets and sets the SolidBrush to color the font character for the point.</summary>
        /// <value>SolidBrush used to color the font character for the point.</value>
        /// <remarks>For a brush other than the SolidBrush, look in custom property of the class.</remarks>
        public GeoSolidBrush CharacterSolidBrush
        {
            get { return _characterSolidBrush; }
            set { _characterSolidBrush = value; }
        }

        /// <summary>Gets and sets the type of symbol to use if the PointType is Symbol.</summary>
        /// <value>Type of symbol to use if the PointType is Symbol.</value>
        /// <remarks>The symbols are simple geometric objects used for abstract representations on a map.</remarks>
        public PointSymbolType SymbolType
        {
            get { return _pointSymbolType; }
            set { _pointSymbolType = value; }
        }

        /// <summary>Gets and sets the SolidBrush used to color the interior of the Symbol.</summary>
        /// <remarks>To draw the interior of the symbol. It is only used if the
        /// PointType is Symbol. For brush other than the SolidBrush, look in
        /// the custom property of the class.</remarks>
        /// <value>SolidBrush used to color the interior of the Symbol.</value>
        public GeoSolidBrush SymbolSolidBrush
        {
            get { return _symbolSolidBrush; }
            set { _symbolSolidBrush = value; }
        }

        /// <summary>Gets and sets the SolidBrush to draw the outline of the Symbol.</summary>
        /// <value>SolidBrush to draw the outline of the Symbol.</value>
        /// <remarks>Used to draw the outline of the Symbol. By default the pen is transparent.</remarks>
        public GeoPen SymbolPen
        {
            get { return _symbolPen; }
            set { _symbolPen = value; }
        }

        /// <summary>Gets and sets the size of the symbol if the PointType is Symbol.</summary>
        /// <value>Size of the symbol if the PointType is Symbol.</value>
        public float SymbolSize
        {
            get { return _symbolSize; }
            set { _symbolSize = value; }
        }

        /// <summary>Gets the custom properties of the PointStyle.</summary>
        /// <value>Custom properties of the PointStyle.</value>
        public PointStyleCustom PointStyleCustom
        {
            get { return _pointStyleCustom; }
        }

        /// <summary>Gets and sets the drawing level of the style.</summary>
        public DrawingLevel DrawingLevel
        {
            get { return _drawingLevel; }
            set { _drawingLevel = value; }
        }

        public bool IsDefault
        {
            get
            {
                if (!IsActive)
                {
                    return true;
                }

                switch (_pointType)
                {
                    case PointType.Symbol:
                        var isDefault = false;
                        if (_symbolPen.Color.IsTransparent && _symbolSolidBrush.Color.IsTransparent &&
                            _symbolPen.Brush is GeoSolidBrush)
                        {
                            isDefault = true;
                        }

                        if (isDefault && _pointStyleCustom.CustomBrush != null)
                        {
                            isDefault = false;
                        }

                        return isDefault;

                    case PointType.Bitmap:
                        return _image == null;

                    case PointType.Character:
                        return _characterSolidBrush.Color.IsTransparent;

                    default:
                        return true;
                }
            }
        }

        /// <summary>Draws the features on the canvas passed in.</summary>
        /// <remarks>Called from the concrete public method Draw. 
        /// When overriding this method, consider each feature and its column data values.If column data for a
        /// feature is needed, be sure to override GetRequiredColumnNamesCore and add the columns to the collection.</remarks>
        /// <returns>None</returns>
        /// <param name="features">Features to draw on the canvas.</param>
        /// <param name="canvas">Canvas to draw the features on.</param>
        /// <param name="labelsInThisLayer">Labels to draw in the current layer only.</param>
        /// <param name="labelsInAllLayers">Labels to drawn in all layers.</param>
        protected override void DrawCore(IEnumerable<Feature> features, BaseGeoCanvas canvas,
            Collection<SimpleCandidate> labelsInThisLayer, Collection<SimpleCandidate> labelsInAllLayers)
        {
            Validators.CheckParameterIsNotNull(features, "features");
            Validators.CheckParameterIsNotNull(canvas, "canvas");
            Validators.CheckParameterIsNotNull(_symbolPen, "symbolPen");
            Validators.CheckParameterIsNotNull(_symbolSolidBrush, "symbolSolidBrush");
            Validators.CheckParameterIsNotNull(_characterSolidBrush, "characterSolidBrush");
            Validators.CheckParameterIsNotNull(_characterFont, "characterFont");
            Validators.CheckParameterIsNotNull(labelsInThisLayer, "labelsInThisLayer");
            Validators.CheckParameterIsNotNull(labelsInAllLayers, "labelsInAllLayers");
            Validators.CheckGeoCanvasIsInDrawing(canvas.IsDrawing);
            Validators.CheckPointSymbolTypeIsValid(_pointSymbolType, "pointSymbolType");
            Validators.CheckPointTypeIsValid(_pointType, "pointType");
            Validators.CheckScaleIsBiggerThanZero(_imageScale, "imageScale");

            if (!IsDefault)
            {
                var tmpGeoPen = _symbolPen;
                if (tmpGeoPen.Color.IsTransparent && tmpGeoPen.Brush is GeoSolidBrush)
                {
                    tmpGeoPen = null;
                }

                var tmpGeoBrush = _pointStyleCustom.CustomBrush;
                if (tmpGeoBrush == null && !_symbolSolidBrush.Color.IsTransparent)
                {
                    tmpGeoBrush = _symbolSolidBrush;
                }

                foreach (var feature in features)
                {
                    var shapeWellKnownType = feature.GetWellKnownType();
                    if (shapeWellKnownType != WellKnownType.Point && shapeWellKnownType != WellKnownType.Multipoint &&
                        shapeWellKnownType != WellKnownType.GeometryCollection)
                    {
                        continue;
                    }

                    switch (_pointType)
                    {
                        case PointType.Symbol:
                            DrawSymbol(feature, canvas, tmpGeoPen, tmpGeoBrush);
                            break;

                        case PointType.Bitmap:
                            DrawBitmap(feature, canvas);
                            break;

                        case PointType.Character:
                            tmpGeoBrush = _pointStyleCustom.CustomBrush;
                            if (tmpGeoBrush == null && !_characterSolidBrush.Color.IsTransparent)
                            {
                                tmpGeoBrush = _characterSolidBrush;
                            }

                            DrawCharacter(_characterIndex, feature, canvas, tmpGeoBrush, tmpGeoPen);
                            break;

                        default:
                            break;
                    }
                    foreach (var pointStyle in _customPointStyles)
                    {
                        var tmpFeatures = new Feature[1] {feature};
                        pointStyle.Draw(tmpFeatures, canvas, labelsInThisLayer, labelsInAllLayers);
                    }
                }
            }
        }

        /// <summary>Draws a sample feature on the canvas passed in.</summary>
        /// <remarks>Virtual method called from the concrete public method Draw. Can be used to
        /// display a legend or other sample area.</remarks>
        /// <returns>None</returns>
        /// <param name="canvas">Canvas to draw the features on.</param>
        protected override void DrawSampleCore(BaseGeoCanvas canvas, DrawingRectangleF drawingExtent)
        {
            Validators.CheckParameterIsNotNull(canvas, "canvas");

            var rectangle = canvas.ToWorldCoordinate(drawingExtent);

            var centerPoint = rectangle.GetCenterPoint();
            var features = new Feature[1] {new Feature(centerPoint.X, centerPoint.Y)};
            Draw(features, canvas, new Collection<SimpleCandidate>(), new Collection<SimpleCandidate>());
        }

        
        private void DrawSymbol(Feature feature, BaseGeoCanvas canvas, GeoPen geoPen, BaseGeoBrush geoBrush)
        {
            _characterFont = GetWingdingFont();

            switch (_pointSymbolType)
            {
                case PointSymbolType.Circle:
                    DrawCircleSymbol(feature, canvas, geoPen, geoBrush);
                    break;

                case PointSymbolType.Square:
                    DrawSquareSymbol(feature, canvas, geoPen, geoBrush);
                    break;

                case PointSymbolType.Triangle:
                    DrawTriangleSymbol(feature, canvas, geoPen, geoBrush);
                    break;

                case PointSymbolType.Cross:
                    if (geoPen == null && _symbolSolidBrush.Color != GeoColor.StandardColors.Transparent)
                    {
                        geoPen = new GeoPen(_symbolSolidBrush.Color);
                    }
                    DrawCrossSymbol(feature, canvas, geoPen);
                    break;

                case PointSymbolType.Diamond:
                    DrawCharacter(DiamondCharacterIndex, feature, canvas, geoBrush, geoPen);
                    break;

                case PointSymbolType.Diamond2:
                    DrawCharacter(Diamond2CharacterIndex, feature, canvas, geoBrush, geoPen);
                    break;

                case PointSymbolType.Star:
                    DrawCharacter(StarCharacterIndex, feature, canvas, geoBrush, geoPen);
                    break;

                case PointSymbolType.Star2:
                    DrawCharacter(Star2CharacterIndex, feature, canvas, geoBrush, geoPen);
                    break;

                default:
                    break;
            }
        }

        private void DrawBitmap(Feature feature, BaseGeoCanvas canvas)
        {
            // It can not be checked in DrawCore function, so move the check in here.
            Validators.CheckImageInPointStyleIsNotNull(_image);
            var shapeType = feature.GetWellKnownType();

            if (shapeType == WellKnownType.Point)
            {
                var point = new PointShape(feature.GetWellKnownBinary());
                DrawOneBitmap(point, canvas);
            }
            else if (shapeType == WellKnownType.Multipoint)
            {
                var multipoint = new MultipointShape(feature.GetWellKnownBinary());
                foreach (var point in multipoint.Points)
                {
                    DrawOneBitmap(point, canvas);
                }
            }
        }

        private void DrawOneBitmap(PointShape point, BaseGeoCanvas canvas)
        {
            if (_imageScale == 1)
            {
                canvas.DrawWorldImageWithoutScaling(_image, point.X, point.Y, _drawingLevel, _xOffsetInPixel,
                    _yOffsetInPixel, _rotateAngle);
            }
            else
            {
                canvas.DrawWorldImage(_image, point.X, point.Y, _imageScale, _drawingLevel, _xOffsetInPixel,
                    _yOffsetInPixel, _rotateAngle);
            }
        }

        private void DrawCharacter(int indexOfCharacter, Feature feature, BaseGeoCanvas canvas, BaseGeoBrush geoBrush,
            GeoPen haloPen)
        {
            if (geoBrush == null)
            {
                return;
            }
            var shapeType = feature.GetWellKnownType();
            if (shapeType == WellKnownType.Point)
            {
                var point = new PointShape(feature.GetWellKnownBinary());
                DrawOneStar(indexOfCharacter, point, canvas, geoBrush, haloPen);
            }
            else if (shapeType == WellKnownType.Multipoint)
            {
                var multipoint = new MultipointShape(feature.GetWellKnownBinary());
                foreach (var point in multipoint.Points)
                {
                    DrawOneStar(indexOfCharacter, point, canvas, geoBrush, haloPen);
                }
            }
        }

        private void DrawOneStar(int indexOfCharacter, PointShape point, BaseGeoCanvas canvas, BaseGeoBrush geoBrush,
            GeoPen haloPen)
        {
            var currentExtent = canvas.CurrentWorldExtent;
            var canvasWidth = canvas.Width;
            var canvasHeight = canvas.Height;
            var extentWidth = currentExtent.Width;
            var extentHeight = currentExtent.Height;
            var upperLeftX = currentExtent.UpperLeftPoint.X;
            var upperLeftY = currentExtent.UpperLeftPoint.Y;

            var screenX = (float) ((point.X - upperLeftX)*canvasWidth/extentWidth);
            var screenY = (float) ((upperLeftY - point.Y)*canvasHeight/extentHeight);

            var text = string.Empty + Convert.ToChar(indexOfCharacter);

            screenX += _xOffsetInPixel;
            screenY += _yOffsetInPixel;

            ScreenPointF[] textpathInScreen = {new ScreenPointF(screenX, screenY)};

            canvas.DrawText(text, _characterFont, geoBrush, haloPen, textpathInScreen, _drawingLevel, _xOffsetInPixel,
                _yOffsetInPixel, _rotateAngle);
        }

        private void DrawCircleSymbol(Feature feature, BaseGeoCanvas canvas, GeoPen geoPen, BaseGeoBrush geoBrush)
        {
            canvas.DrawEllipse(feature, _symbolSize, _symbolSize, geoPen, geoBrush, _drawingLevel, _xOffsetInPixel,
                _yOffsetInPixel, PenBrushDrawingOrder.BrushFirst);
        }

        private void DrawSquareSymbol(Feature feature, BaseGeoCanvas canvas, GeoPen geoPen, BaseGeoBrush geoBrush)
        {
            var centerPointWkb = feature.GetWellKnownBinary();

            var shapeType = ArrayHelper.GetShapeTypeFromByteArray(centerPointWkb, PositionShapeType);

            var byteOrder = centerPointWkb[PositionByteOrder];

            var startIndex = 0;

            if (shapeType == WkbShapeType.Point)
            {
                DrawOneSquare(centerPointWkb, startIndex, canvas, geoPen, geoBrush);
            }
            else
            {
                var numberOfPoints = ArrayHelper.GetIntFromByteArray(centerPointWkb, startIndex + PositionCount, byteOrder);
                var dataIndex = startIndex + PositionData;

                for (var i = 0; i < numberOfPoints; i++)
                {
                    dataIndex = DrawOneSquare(centerPointWkb, dataIndex, canvas, geoPen, geoBrush);
                }
            }
        }

        private void DrawTriangleSymbol(Feature feature, BaseGeoCanvas canvas, GeoPen geoPen, BaseGeoBrush geoBrush)
        {
            var centerPointWkb = feature.GetWellKnownBinary();

            var shapeType = ArrayHelper.GetShapeTypeFromByteArray(centerPointWkb, PositionShapeType);

            var byteOrder = centerPointWkb[PositionByteOrder];

            var startIndex = 0;

            if (shapeType == WkbShapeType.Point)
            {
                DrawOneTriangle(centerPointWkb, startIndex, canvas, geoPen, geoBrush);
            }
            else
            {
                var numberOfPoints = ArrayHelper.GetIntFromByteArray(centerPointWkb, startIndex + PositionCount, byteOrder);
                var dataIndex = startIndex + PositionData;

                for (var i = 0; i < numberOfPoints; i++)
                {
                    dataIndex = DrawOneTriangle(centerPointWkb, dataIndex, canvas, geoPen, geoBrush);
                }
            }
        }

        private void DrawCrossSymbol(Feature feature, BaseGeoCanvas canvas, GeoPen geoPen)
        {
            if (geoPen == null)
            {
                return;
            }

            var centerPointWkb = feature.GetWellKnownBinary();

            var shapeType = ArrayHelper.GetShapeTypeFromByteArray(centerPointWkb, PositionShapeType);

            var byteOrder = centerPointWkb[PositionByteOrder];

            var startIndex = 0;

            if (shapeType == WkbShapeType.Point)
            {
                DrawOneCross(centerPointWkb, startIndex, canvas, geoPen);
            }
            else
            {
                var numberOfPoints = ArrayHelper.GetIntFromByteArray(centerPointWkb, startIndex + PositionCount, byteOrder);
                var dataIndex = startIndex + PositionData;

                for (var i = 0; i < numberOfPoints; i++)
                {
                    dataIndex = DrawOneCross(centerPointWkb, dataIndex, canvas, geoPen);
                }
            }
        }

        private int DrawOneSquare(byte[] centerPointWkb, int startIndex, BaseGeoCanvas canvas, GeoPen geoPen,
            BaseGeoBrush geoBrush)
        {
            var currentExtent = canvas.CurrentWorldExtent;
            var canvasWidth = canvas.Width;
            var canvasHeight = canvas.Height;
            var extentWidth = currentExtent.Width;
            var extentHeight = currentExtent.Height;
            var upperLeftX = currentExtent.UpperLeftPoint.X;
            var upperLeftY = currentExtent.UpperLeftPoint.Y;

            var byteOrder = centerPointWkb[startIndex + PositionByteOrder];

            startIndex += PositionPointData;

            var worldX = ArrayHelper.GetDoubleFromByteArray(centerPointWkb, startIndex, byteOrder);
            var worldY = ArrayHelper.GetDoubleFromByteArray(centerPointWkb, startIndex + SizeOfDouble, byteOrder);
            startIndex += SizeOfPoint;

            var screenX = (worldX - upperLeftX)*canvasWidth/extentWidth;
            var screenY = (upperLeftY - worldY)*canvasHeight/extentHeight;

            if (CheckSymbolPointIsInCanvas(screenX, screenY, canvasWidth, canvasHeight))
            {
                return startIndex;
            }

            var radient = (45 + _rotateAngle)*RadientPenDegree;
            var radius = _symbolSize*0.707106781185;
            var sin = Math.Sin(radient)*radius;
            var cos = Math.Cos(radient)*radius;

            var polygonWkb = new byte[93];
            byte[] polygonHeader = {1, 3, 0, 0, 0, 1, 0, 0, 0, 5, 0, 0, 0};
            long index = 0;

            Array.Copy(polygonHeader, 0, polygonWkb, 0, polygonHeader.Length);
           
            index += polygonHeader.Length;

            worldX = (screenX + cos)*extentWidth/canvasWidth + upperLeftX;
            worldY = upperLeftY - (screenY - sin)*extentHeight/canvasHeight;
            ArrayHelper.CopyToArray(BitConverter.GetBytes(worldX), polygonWkb, index);
            index += SizeOfDouble;
            ArrayHelper.CopyToArray(BitConverter.GetBytes(worldY), polygonWkb, index);
            index += SizeOfDouble;

            worldX = (screenX - sin)*extentWidth/canvasWidth + upperLeftX;
            worldY = upperLeftY - (screenY - cos)*extentHeight/canvasHeight;
            ArrayHelper.CopyToArray(BitConverter.GetBytes(worldX), polygonWkb, index);
            index += SizeOfDouble;
            ArrayHelper.CopyToArray(BitConverter.GetBytes(worldY), polygonWkb, index);
            index += SizeOfDouble;

            worldX = (screenX - cos)*extentWidth/canvasWidth + upperLeftX;
            worldY = upperLeftY - (screenY + sin)*extentHeight/canvasHeight;
            ArrayHelper.CopyToArray(BitConverter.GetBytes(worldX), polygonWkb, index);
            index += SizeOfDouble;
            ArrayHelper.CopyToArray(BitConverter.GetBytes(worldY), polygonWkb, index);
            index += SizeOfDouble;

            worldX = (screenX + sin)*extentWidth/canvasWidth + upperLeftX;
            worldY = upperLeftY - (screenY + cos)*extentHeight/canvasHeight;
            ArrayHelper.CopyToArray(BitConverter.GetBytes(worldX), polygonWkb, index);
            index += SizeOfDouble;
            ArrayHelper.CopyToArray(BitConverter.GetBytes(worldY), polygonWkb, index);
            index += SizeOfDouble;

            Array.Copy(polygonWkb, FirstPointPosition, polygonWkb, index, SizeOfPoint);

            canvas.DrawArea(new Feature(polygonWkb, string.Empty), geoPen, geoBrush, _drawingLevel, _xOffsetInPixel,
                _yOffsetInPixel, PenBrushDrawingOrder.BrushFirst);

            return startIndex;
        }

        private int DrawOneTriangle(byte[] centerPointWkb, int startIndex, BaseGeoCanvas canvas, GeoPen geoPen,
            BaseGeoBrush geoBrush)
        {
            var currentExtent = canvas.CurrentWorldExtent;
            var canvasWidth = canvas.Width;
            var canvasHeight = canvas.Height;
            var extentWidth = currentExtent.Width;
            var extentHeight = currentExtent.Height;
            var upperLeftX = currentExtent.UpperLeftPoint.X;
            var upperLeftY = currentExtent.UpperLeftPoint.Y;

            var byteOrder = centerPointWkb[startIndex + PositionByteOrder];

            startIndex += PositionPointData;

            var worldX = ArrayHelper.GetDoubleFromByteArray(centerPointWkb, startIndex, byteOrder);
            var worldY = ArrayHelper.GetDoubleFromByteArray(centerPointWkb, startIndex + SizeOfDouble, byteOrder);
            startIndex += SizeOfPoint;

            var screenX = (worldX - upperLeftX)*canvasWidth/extentWidth;
            var screenY = (upperLeftY - worldY)*canvasHeight/extentHeight;

            if (CheckSymbolPointIsInCanvas(screenX, screenY, canvasWidth, canvasHeight))
            {
                return startIndex;
            }

            var radius = 0.5773502691*_symbolSize;
            var radient = _rotateAngle*RadientPenDegree;
            var cos = Math.Cos(radient)*radius;
            var sin = Math.Sin(radient)*radius;

            
            var polygonWkb = new byte[77];
            byte[] polygonHeader = {1, 3, 0, 0, 0, 1, 0, 0, 0, 4, 0, 0, 0};
            long index = 0;

            Array.Copy(polygonHeader, 0, polygonWkb, 0, polygonHeader.Length);
           
            index += polygonHeader.Length;

            worldX = (screenX - sin)*extentWidth/canvasWidth + upperLeftX;
            worldY = upperLeftY - (screenY - cos)*extentHeight/canvasHeight;
            ArrayHelper.CopyToArray(BitConverter.GetBytes(worldX), polygonWkb, index);
            index += SizeOfDouble;
            ArrayHelper.CopyToArray(BitConverter.GetBytes(worldY), polygonWkb, index);
            index += SizeOfDouble;

            worldX = (screenX - 0.866025403*cos + 0.5*sin)*extentWidth/canvasWidth + upperLeftX;
            worldY = upperLeftY - (screenY + 0.5*cos + 0.866025403*sin)*extentHeight/canvasHeight;
            ArrayHelper.CopyToArray(BitConverter.GetBytes(worldX), polygonWkb, index);
            index += SizeOfDouble;
            ArrayHelper.CopyToArray(BitConverter.GetBytes(worldY), polygonWkb, index);
            index += SizeOfDouble;

            worldX = (screenX + 0.866025403*cos + 0.5*sin)*extentWidth/canvasWidth + upperLeftX;
            worldY = upperLeftY - (screenY + 0.5*cos - 0.866025403*sin)*extentHeight/canvasHeight;
            ArrayHelper.CopyToArray(BitConverter.GetBytes(worldX), polygonWkb, index);
            index += SizeOfDouble;
            ArrayHelper.CopyToArray(BitConverter.GetBytes(worldY), polygonWkb, index);
            index += SizeOfDouble;

            Array.Copy(polygonWkb, FirstPointPosition, polygonWkb, index, SizeOfPoint);

            canvas.DrawArea(new Feature(polygonWkb, string.Empty), geoPen, geoBrush, _drawingLevel, _xOffsetInPixel,
                _yOffsetInPixel, PenBrushDrawingOrder.BrushFirst);

            return startIndex;
        }

        private int DrawOneCross(byte[] centerPointWkb, int startIndex, BaseGeoCanvas canvas, GeoPen geoPen)
        {
            var currentExtent = canvas.CurrentWorldExtent;
            var canvasWidth = canvas.Width;
            var canvasHeight = canvas.Height;
            var extentWidth = currentExtent.Width;
            var extentHeight = currentExtent.Height;
            var upperLeftX = currentExtent.UpperLeftPoint.X;
            var upperLeftY = currentExtent.UpperLeftPoint.Y;

            var byteOrder = centerPointWkb[startIndex + PositionByteOrder];

            startIndex += PositionPointData;

            var worldX = ArrayHelper.GetDoubleFromByteArray(centerPointWkb, startIndex, byteOrder);
            var worldY = ArrayHelper.GetDoubleFromByteArray(centerPointWkb, startIndex + SizeOfDouble, byteOrder);
            startIndex += SizeOfPoint;

            var screenX = (worldX - upperLeftX)*canvasWidth/extentWidth;
            var screenY = (upperLeftY - worldY)*canvasHeight/extentHeight;

            if (CheckSymbolPointIsInCanvas(screenX, screenY, canvasWidth, canvasHeight))
            {
                return startIndex;
            }

            var radius = _symbolSize*0.5;
            var radient = _rotateAngle*RadientPenDegree;
            var sin = Math.Sin(radient)*radius;
            var cos = Math.Cos(radient)*radius;

            var multiLineWkb = new byte[91];
           
            byte[] multiLineHeader = {1, 5, 0, 0, 0, 2, 0, 0, 0};
          
            byte[] lineStringHeader = {1, 2, 0, 0, 0, 2, 0, 0, 0};
            long index = 0;

            ArrayHelper.CopyToArray(multiLineHeader, multiLineWkb, index);
            index += multiLineHeader.Length;
            ArrayHelper.CopyToArray(lineStringHeader, multiLineWkb, index);
            index += lineStringHeader.Length;

            worldX = (screenX - sin)*extentWidth/canvasWidth + upperLeftX;
            worldY = upperLeftY - (screenY - cos)*extentHeight/canvasHeight;
            ArrayHelper.CopyToArray(BitConverter.GetBytes(worldX), multiLineWkb, index);
            index += SizeOfDouble;
            ArrayHelper.CopyToArray(BitConverter.GetBytes(worldY), multiLineWkb, index);
            index += SizeOfDouble;

            worldX = (screenX + sin)*extentWidth/canvasWidth + upperLeftX;
            worldY = upperLeftY - (screenY + cos)*extentHeight/canvasHeight;
            ArrayHelper.CopyToArray(BitConverter.GetBytes(worldX), multiLineWkb, index);
            index += SizeOfDouble;
            ArrayHelper.CopyToArray(BitConverter.GetBytes(worldY), multiLineWkb, index);
            index += SizeOfDouble;

            ArrayHelper.CopyToArray(lineStringHeader, multiLineWkb, index);
            index += lineStringHeader.Length;

            worldX = (screenX + cos)*extentWidth/canvasWidth + upperLeftX;
            worldY = upperLeftY - (screenY - sin)*extentHeight/canvasHeight;
            ArrayHelper.CopyToArray(BitConverter.GetBytes(worldX), multiLineWkb, index);
            index += SizeOfDouble;
            ArrayHelper.CopyToArray(BitConverter.GetBytes(worldY), multiLineWkb, index);
            index += SizeOfDouble;

            worldX = (screenX - cos)*extentWidth/canvasWidth + upperLeftX;
            worldY = upperLeftY - (screenY + sin)*extentHeight/canvasHeight;
            ArrayHelper.CopyToArray(BitConverter.GetBytes(worldX), multiLineWkb, index);
            index += SizeOfDouble;
            ArrayHelper.CopyToArray(BitConverter.GetBytes(worldY), multiLineWkb, index);
            index += SizeOfDouble;

            canvas.DrawLine(new Feature(multiLineWkb, string.Empty), geoPen, _drawingLevel, _xOffsetInPixel,
                _yOffsetInPixel);

            return startIndex;
        }

        private bool CheckSymbolPointIsInCanvas(double screenX, double screenY, float canvasWidth, float canvasHeight)
        {
            return screenX < -_symbolSize || screenX > canvasWidth + _symbolSize || screenY < -_symbolSize ||
                   screenY > canvasHeight + _symbolSize;
        }

        private GeoFont GetWingdingFont()
        {
            GeoFont wingdingsFont;

            if (_wingdingsFontCache.ContainsKey(_symbolSize))
            {
                wingdingsFont = _wingdingsFontCache[_symbolSize];
            }
            else
            {
                wingdingsFont = new GeoFont("wingdings", _symbolSize);
                _wingdingsFontCache.Add(_symbolSize, wingdingsFont);
            }

            return wingdingsFont;
        }


    }
}
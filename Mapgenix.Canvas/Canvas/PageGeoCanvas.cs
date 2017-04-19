using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using Mapgenix.Shapes;
using Mapgenix.Utils;

namespace Mapgenix.Canvas
{
    /// <summary>Canvas for the drawing of the layers to go from page coordinate system to screen coordinate system.
    ///Used by PrintLayer in DrawCore. </summary>
    /// <remarks>
    /// 	<para>The first call is always BeginDrawing by passing in an image along with its world extent.</para>
    /// </remarks>
   public class PageGeoCanvas : BaseGeoCanvas
    {
        private const double PageBaseUnitToPageUnitRatio = 12*96;
        private readonly RectangleShape _pageBoundingBox;
        private readonly RectangleShape _printBoundingBox;

        private BaseGeoCanvas _canvas;
        private double _x1, _x2, _y1, _y2, _xp1, _xp2, _yp1, _yp2;

        public PageGeoCanvas(RectangleShape pageBoundingBox, RectangleShape printBoundingBox)
        {
            _pageBoundingBox = pageBoundingBox;
            _printBoundingBox = printBoundingBox;
        }

        /// <summary>Begins the drawing on the canvas.</summary>
        /// <remarks>
        /// First method to call before doing any drawing on the canvas.
        /// Sets IsDrawing property to true. After finishing drawing,
        /// call EndDrawing to commit the changes to the image.
        /// </remarks>
        /// <param name="nativeImage">Image the canvas draws on.</param>
        /// <param name="worldExtent">World extent of the canvas.</param>
        /// <param name="drawingMapUnit">Map unit of the canvas.</param>
        public void BeginDrawing(BaseGeoCanvas nativeImage, RectangleShape worldExtent, GeographyUnit drawingMapUnit)
        {
            if (!IsCancelled)
            {
                IsDrawing = true;
                MapUnit = drawingMapUnit;
                NativeImage = nativeImage;

                _hasDrawn = false;
                _currentExtent = worldExtent;

                BeginDrawingCore(nativeImage, worldExtent);

                if (_geoCanvasWidth == 0 && _geoCanvasHeight == 0)
                {
                    _geoCanvasWidth = GetCanvasWidth(nativeImage);
                    _geoCanvasHeight = GetCanvasHeight(nativeImage);
                    
                }

                _currentScale = ExtentHelper.GetScale(worldExtent, _geoCanvasWidth, drawingMapUnit, _dpi);

                _worldToScreenFactorX = _geoCanvasWidth / _currentExtent.Width;
                _worldToScreenFactorY = _geoCanvasHeight / _currentExtent.Height;
            }
        }


        private void BeginDrawingCore(BaseGeoCanvas nativeImage, RectangleShape worldExtent)
        {
            _canvas = nativeImage;
            CurrentWorldExtent = worldExtent;

            Width = (float) ((_pageBoundingBox.Width));
            Height = (float) ((_pageBoundingBox.Height));

            _x1 = _pageBoundingBox.LowerLeftPoint.X;
            _x2 = _pageBoundingBox.UpperRightPoint.X;
            _y1 = _pageBoundingBox.UpperRightPoint.Y;
            _y2 = _pageBoundingBox.LowerLeftPoint.Y;

            _xp1 =
                ExtentHelper.ToScreenCoordinate(_canvas.CurrentWorldExtent, _printBoundingBox.UpperLeftPoint,
                    _canvas.Width, _canvas.Height).X;
            _xp2 =
                ExtentHelper.ToScreenCoordinate(_canvas.CurrentWorldExtent, _printBoundingBox.LowerRightPoint,
                    _canvas.Width, _canvas.Height).X;
            _yp1 =
                ExtentHelper.ToScreenCoordinate(_canvas.CurrentWorldExtent, _printBoundingBox.LowerRightPoint,
                    _canvas.Width, _canvas.Height).Y;
            _yp2 =
                ExtentHelper.ToScreenCoordinate(_canvas.CurrentWorldExtent, _printBoundingBox.UpperLeftPoint,
                    _canvas.Width, _canvas.Height).Y;
        }

        /// <summary>Ends drawing and commits the drawing on canvas.</summary>
        /// <remarks>Last method to call after finished drawing. Commits the
        /// changes to the image passed in BeginDrawing. Sets IsDrawing to false. 
        /// Finally puts the canvas into invalid state.</remarks>
        public void EndDrawing()
        {
            _hasDrawn = false;
            NativeImage = null;

            IsDrawing = false;
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
        protected override void DrawAreaCore(IEnumerable<ScreenPointF[]> screenPoints, GeoPen outlinePen,
            BaseGeoBrush fillBrush, DrawingLevel drawingLevel, float xOffset, float yOffset,
            PenBrushDrawingOrder penBrushDrawingOrder)
        {
            foreach (var screenPointFs in screenPoints)
            {
                for (var i = 0; i < screenPointFs.Length; i++)
                {
                    screenPointFs[i] = GetScreenPoint(screenPointFs[i]);
                }
            }

            var scaledPen = GetScaledPen(outlinePen);

            _canvas.DrawArea(screenPoints, scaledPen, fillBrush, drawingLevel, xOffset, yOffset, penBrushDrawingOrder);
        }

        /// <summary>Draws an ellipse on the canvas.</summary>
        /// <overloads>
        /// Allows you to pass in a GeoPen and the DrawingLevel.
        /// </overloads>
        /// <remarks>
        /// 	<para>Provides various overloads for control on the drawing of the ellipse. GeoBrush to fill in the
        ///     area of the ellipse. GeoPen for the outline of the ellipse. You can also call a overload that will allow you to specify
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
        protected override void DrawEllipseCore(ScreenPointF screenPoint, float width, float height, GeoPen outlinePen,
            BaseGeoBrush fillBrush, DrawingLevel drawingLevel, float xOffset, float yOffset,
            PenBrushDrawingOrder penBrushDrawingOrder)
        {
            Validators.CheckParameterIsNotBothNull(outlinePen, fillBrush, "outlinePen", "fillBrush");
            Validators.CheckDrawingLevelIsValid(drawingLevel, "drawingLevel");
            Validators.CheckGeoCanvasIsInDrawing(IsDrawing);

            screenPoint = GetScreenPoint(screenPoint);

            var scaledPen = GetScaledPen(outlinePen);

            _canvas.DrawEllipse(new ScreenPointF(screenPoint.X, screenPoint.Y), width, height, scaledPen, fillBrush,
                drawingLevel, xOffset, yOffset, penBrushDrawingOrder);
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
        protected override void DrawLineCore(IEnumerable<ScreenPointF> screenPoints, GeoPen linePen,
            DrawingLevel drawingLevel, float xOffset, float yOffset)
        {
            Validators.CheckParameterIsNotNull(screenPoints, "screenPoints");
            Validators.CheckParameterIsNotNull(linePen, "outlinePen");
            Validators.CheckDrawingLevelIsValid(drawingLevel, "drawingLevel");
            Validators.CheckGeoCanvasIsInDrawing(IsDrawing);

            var convertedPoints = new Collection<ScreenPointF>();
            foreach (var item in screenPoints)
            {
                convertedPoints.Add(GetScreenPoint(item));
            }

            var scaledPen = GetScaledPen(linePen);

            _canvas.DrawLine(convertedPoints, scaledPen, drawingLevel, xOffset, yOffset);
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
        protected override void DrawScreenImageCore(GeoImage image, float centerXInScreen, float centerYInScreen,
            float widthInScreen, float heightInScreen, DrawingLevel drawingLevel, float xOffset, float yOffset,
            float rotateAngle)
        {
            Validators.CheckParameterIsNotNull(image, "image");
            Validators.CheckDrawingLevelIsValid(drawingLevel, "drawingLevel");
            Validators.CheckIfInputValueIsBiggerThan(widthInScreen, "widthInScreen", 0,
                RangeCheckingInclusion.ExcludeValue);
            Validators.CheckIfInputValueIsBiggerThan(heightInScreen, "heightInScreen", 0,
                RangeCheckingInclusion.ExcludeValue);

            var newCenterPoint = GetScreenPoint(new ScreenPointF(centerXInScreen, centerYInScreen));

            var newWidth = (float) (GetScaledLength(widthInScreen));
            var newHeight = (float) (GetScaledLength(heightInScreen));

            _canvas.DrawScreenImage(image, newCenterPoint.X, newCenterPoint.Y, newWidth, newHeight, drawingLevel,
                xOffset, yOffset, rotateAngle);
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
        protected override void DrawScreenImageWithoutScalingCore(GeoImage image, float centerXInScreen,
            float centerYInScreen, DrawingLevel drawingLevel, float xOffset, float yOffset, float rotateAngle)
        {
            DrawScreenImageCore(image, centerXInScreen, centerYInScreen, image.GetWidth(), image.GetHeight(),
                drawingLevel, xOffset, yOffset, rotateAngle);
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
        protected override void DrawTextCore(string text, GeoFont font, BaseGeoBrush fillBrush, GeoPen haloPen,
            IEnumerable<ScreenPointF> textPathInScreen, DrawingLevel drawingLevel, float xOffset, float yOffset,
            float rotateAngle)
        {
            var screenPoints = new List<ScreenPointF>();

            foreach (var screenPointF in textPathInScreen)
            {
                screenPoints.Add(GetScreenPoint(screenPointF));
            }

            var scaledFont = GetScaledFont(font);

            _canvas.DrawText(text, scaledFont, fillBrush, haloPen, screenPoints, drawingLevel, xOffset, yOffset,
                rotateAngle);
        }

        /// <summary>Returns the rectangle containing a text.</summary>
        /// <returns>A rectangle containing a text taking into account the font.</returns>
        /// <remarks>Typically used for labeling logic to determine the overlapping of labels.</remarks>
        /// <param name="text">Text to measure.</param>
        /// <param name="font">Font of the text to measure.</param>
        protected override DrawingRectangleF MeasureTextCore(string text, GeoFont font)
        {
            Bitmap bitmap = null;
            Graphics graphics = null;
            SizeF size;

            try
            {
                bitmap = new Bitmap(1, 1);
                bitmap.SetResolution(Dpi, Dpi);
                graphics = Graphics.FromImage(bitmap);

                size = graphics.MeasureString(text, GetGdiPlusFontFromGeoFont(font), new PointF(),
                    StringFormat.GenericTypographic);
                if (size.Width == 0 && size.Height != 0 && text.Length != 0)
                {
                    size.Width = 1;
                }
            }
            finally
            {
                if (graphics != null)
                {
                    graphics.Dispose();
                }
                if (bitmap != null)
                {
                    bitmap.Dispose();
                }
            }

            var drawingRectangleF = new DrawingRectangleF(size.Width/2, size.Height/2, size.Width, size.Height);

            return drawingRectangleF;
        }

        /// <summary>Converts an object to a GeoImage. For example, in GdiPlus  object is a Bitmap.</summary>
        /// <param name="nativeImage">Target object to convert.</param>
        /// <returns>GeoImage containing.</returns>
        protected override GeoImage ToGeoImageCore(object nativeImage)
        {
            return _canvas.ToGeoImage(nativeImage);
        }

        /// <summary>Converts an object to a GeoImage. For example, in GdiPlus  object is a Bitmap.</summary>
        /// <param name="nativeImage">Target object to convert.</param>
        /// <returns>GeoImage containing.</returns>
        protected override object ToNativeImageCore(GeoImage image)
        {
            return _canvas.ToNativeImage(image);
        }

        /// <summary>Flushes drawing and commits the drawing on the canvas.</summary>
        /// <remarks>Method to call when drawing finished. It commits the
        /// image changes to the image passed in on BeginDrawing. Also it sets IsDrawing to false. Finally it sets GeoCanvas to invalid state, 
        /// not allowing further drawing.</remarks>
        protected override void FlushCore()
        {
            _canvas.Flush();
        }

        /// <summary>Gets the canvas height of a native image object.</summary>
        /// <param name="nativeImage">Native image will be used to get the canvas height.</param>
        /// <returns>Canvas height.</returns>
        protected override float GetCanvasHeightCore(object nativeImage)
        {
            return (float) _pageBoundingBox.Height;
        }

        /// <summary>Gets the canvas width of a native image object.</summary>
        /// <param name="nativeImage">Native image used to get the canvas width.</param>
        /// <returns>Canvas width.</returns>
        protected override float GetCanvasWidthCore(object nativeImage)
        {
            return (float) _pageBoundingBox.Width;
        }

        /// <summary>Returns stream representing the GeoImage in TIFF format.</summary>
        /// <returns>Stream that represents the GeoImage in TIFF format.</returns>
        /// <remarks>Allows to get the stream out of a GeoImage. The stream is a
        /// memory stream and the bytes are in TIFF format. Can be saved in the desired format.</remarks>
        /// <param name="image">GeoImage to convert to a stream.</param>
        public override Stream GetStreamFromGeoImage(GeoImage image)
        {
            return _canvas.GetStreamFromGeoImage(image);
        }

        private Font GetGdiPlusFontFromGeoFont(GeoFont font)
        {
            if (font == null)
            {
                return null;
            }

            var gdiplusFontStyle = GetFontStyleFromDrawingFontStyle(font.Style);

            var resultFont = new Font(font.FontName, font.Size, gdiplusFontStyle);

            return resultFont;
        }

        private static FontStyle GetFontStyleFromDrawingFontStyle(DrawingFontStyles style)
        {
            var returnFontStyle = FontStyle.Regular;

            var value = (int) style;


            if ((style & DrawingFontStyles.Bold) != 0)
            {
                returnFontStyle = returnFontStyle | FontStyle.Bold;
            }
            if ((style & DrawingFontStyles.Italic) != 0)
            {
                returnFontStyle = returnFontStyle | FontStyle.Italic;
            }
            if ((style & DrawingFontStyles.Underline) != 0)
            {
                returnFontStyle = returnFontStyle | FontStyle.Underline;
            }
            if ((style & DrawingFontStyles.Strikeout) != 0)
            {
                returnFontStyle = returnFontStyle | FontStyle.Strikeout;
            }

            return returnFontStyle;
        }

        private GeoPen GetScaledPen(GeoPen geoPen)
        {
            var rtn = geoPen;
            if (rtn != null)
            {
                rtn = geoPen.CloneDeep();
                rtn.Width = (float) (rtn.Width/_canvas.CurrentScale*PageBaseUnitToPageUnitRatio);
            }
            return rtn;
        }

        private GeoFont GetScaledFont(GeoFont geoFont)
        {
            var rtn = geoFont;

            if (rtn != null)
            {
                var newSize = (float) (rtn.Size/(_canvas.CurrentScale)*PageBaseUnitToPageUnitRatio);
                rtn = new GeoFont(rtn.FontName, newSize, rtn.Style);
            }

            return rtn;
        }

        private double GetScaledLength(double length)
        {
            return length/(_canvas.CurrentScale)*PageBaseUnitToPageUnitRatio;
        }

        private ScreenPointF GetScreenPoint(ScreenPointF pointF)
        {
            var x = (float) ((((pointF.X - _x1)*(_xp2 - _xp1))/(_x2 - _x1)) + _xp1);
            var y = (float) ((((pointF.Y - _y1)*(_yp2 - _yp1))/(_y2 - _y1)) + _yp1);

            return new ScreenPointF(x, y);
        }
    }
}
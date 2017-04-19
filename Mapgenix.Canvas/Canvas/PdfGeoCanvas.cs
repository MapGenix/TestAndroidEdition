using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Mapgenix.Shapes;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace Mapgenix.Canvas
{
    /// <summary>Canvas for the drawing of layers on PDF. </summary>
    /// <remarks>
    /// 	<para>The first call is always BeginDrawing by passing in an image along with its world extent.</para>
    /// </remarks>
    [Serializable]
    public class PdfGeoCanvas : BaseGeoCanvas
    {
        private readonly Dictionary<DrawingLevel, Collection<Dictionary<string, object>>> _featuresByLevel =
            new Dictionary<DrawingLevel, Collection<Dictionary<string, object>>>();

        private Dictionary<long, XBrush> _brushCache;
        private Rectangle _drawingArea;
        private Dictionary<long, XFont> _fontCache;
        private Dictionary<long, XPen> _penCache;

        [NonSerialized] private XGraphics _xGraphics;

        public Rectangle DrawingArea
        {
            get { return _drawingArea; }
            set { _drawingArea = value; }
        }

        private float ToPoint(float pixel)
        {
            return pixel*.75f;
        }

        private float ToPixel(float point)
        {
            return point*1.33333333f;
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
            if (!_featuresByLevel.ContainsKey(drawingLevel))
            {
                _featuresByLevel.Add(drawingLevel, new Collection<Dictionary<string, object>>());
            }

            var parameters = new Dictionary<string, object>();
            parameters.Add("screenPoints", screenPoints);
            parameters.Add("outlinePen", outlinePen);
            parameters.Add("fillBrush", fillBrush);
            parameters.Add("xOffset", xOffset);
            parameters.Add("yOffset", yOffset);
            parameters.Add("penBrushDrawingOrder", penBrushDrawingOrder);
            parameters.Add("method", "DrawAreaCache");

            _featuresByLevel[drawingLevel].Add(parameters);
        }

        private void DrawAreaCache(IEnumerable<ScreenPointF[]> screenPoints, GeoPen outlinePen, BaseGeoBrush fillBrush,
            float xOffset, float yOffset, PenBrushDrawingOrder penBrushDrawingOrder)
        {
            var ringsCollection = new List<PointF[]>();
            ScreenPointF[] cachePoints = null;

            foreach (var rings in screenPoints)
            {
                if (cachePoints == null)
                {
                    cachePoints = rings;
                }
                var points = new PointF[rings.Length];
                var previousX = float.MaxValue;
                var previousY = float.MaxValue;
                var tempCount = 0;
                for (var i = 0; i < rings.Length; i++)
                {
                    var point = rings[i];
                    var screenX = ToPoint(point.X + xOffset) + _drawingArea.Location.X;
                    var screenY = ToPoint(point.Y + yOffset) + _drawingArea.Location.Y;

                    if (previousX != screenX || previousY != screenY)
                    {
                        previousX = screenX;
                        previousY = screenY;
                        points[tempCount] = new PointF(screenX, screenY);
                        tempCount++;
                    }
                }
                if (tempCount <= 2)
                {
                    continue;
                }
                if (tempCount != rings.Length)
                {
                    Array.Resize(ref points, tempCount);
                }
                ringsCollection.Add(points);
            }

            var count = ringsCollection.Count;

            if (count == 1)
            {
                if (penBrushDrawingOrder == PenBrushDrawingOrder.BrushFirst)
                {
                    if (fillBrush != null)
                    {
                        _xGraphics.DrawPolygon(GetXBrushFromGeoBrush(fillBrush, cachePoints), ringsCollection[0],
                            XFillMode.Winding);
                    }
                    if (outlinePen != null)
                    {
                        _xGraphics.DrawPolygon(GetXPenFromGeoPen(outlinePen), ringsCollection[0]);
                    }
                }
                else
                {
                    if (outlinePen != null)
                    {
                        _xGraphics.DrawPolygon(GetXPenFromGeoPen(outlinePen), ringsCollection[0]);
                    }
                    if (fillBrush != null)
                    {
                        _xGraphics.DrawPolygon(GetXBrushFromGeoBrush(fillBrush, cachePoints), ringsCollection[0],
                            XFillMode.Winding);
                    }
                }
            }
            else if (count != 0)
            {
                var graphicsPath = new XGraphicsPath();

                foreach (var points in ringsCollection)
                {
                    graphicsPath.AddPolygon(points);
                }

                if (penBrushDrawingOrder == PenBrushDrawingOrder.BrushFirst)
                {
                    if (fillBrush != null)
                    {
                        _xGraphics.DrawPath(GetXBrushFromGeoBrush(fillBrush, cachePoints), graphicsPath);
                    }
                    if (outlinePen != null)
                    {
                        _xGraphics.DrawPath(GetXPenFromGeoPen(outlinePen), graphicsPath);
                    }
                }
                else
                {
                    if (outlinePen != null)
                    {
                        _xGraphics.DrawPath(GetXPenFromGeoPen(outlinePen), graphicsPath);
                    }
                    if (fillBrush != null)
                    {
                        _xGraphics.DrawPath(GetXBrushFromGeoBrush(fillBrush, cachePoints), graphicsPath);
                    }
                }
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
        protected override void DrawLineCore(IEnumerable<ScreenPointF> screenPoints, GeoPen linePen,
            DrawingLevel drawingLevel, float xOffset, float yOffset)
        {
            if (!_featuresByLevel.ContainsKey(drawingLevel))
            {
                _featuresByLevel.Add(drawingLevel, new Collection<Dictionary<string, object>>());
            }

            var parameters = new Dictionary<string, object>();
            parameters.Add("screenPoints", screenPoints);
            parameters.Add("linePen", linePen);
            parameters.Add("xOffset", xOffset);
            parameters.Add("yOffset", yOffset);
            parameters.Add("method", "DrawLineCache");

            _featuresByLevel[drawingLevel].Add(parameters);
        }

        private void DrawLineCache(IEnumerable<ScreenPointF> screenPoints, GeoPen linePen, float xOffset, float yOffset)
        {
            var points = new List<PointF>();

            var previousX = float.MaxValue;
            var previousY = float.MaxValue;

            foreach (var point in screenPoints)
            {
                var screenX = ToPoint(point.X + xOffset) + _drawingArea.Location.X;
                var screenY = ToPoint(point.Y + yOffset) + _drawingArea.Location.Y;
                if (previousX != screenX || previousY != screenY)
                {
                    previousX = screenX;
                    previousY = screenY;
                    points.Add(new PointF(previousX, previousY));
                }
            }

            if (points.Count > 1)
            {
                _xGraphics.DrawLines(GetXPenFromGeoPen(linePen), points.ToArray());
            }
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
            if (!_featuresByLevel.ContainsKey(drawingLevel))
            {
                _featuresByLevel.Add(drawingLevel, new Collection<Dictionary<string, object>>());
            }

            var parameters = new Dictionary<string, object>();
            parameters.Add("screenPoint", screenPoint);
            parameters.Add("width", width);
            parameters.Add("height", height);
            parameters.Add("outlinePen", outlinePen);
            parameters.Add("fillBrush", fillBrush);
            parameters.Add("xOffset", xOffset);
            parameters.Add("yOffset", yOffset);
            parameters.Add("penBrushDrawingOrder", penBrushDrawingOrder);
            parameters.Add("method", "DrawEllipseCache");

            _featuresByLevel[drawingLevel].Add(parameters);
        }

        private void DrawEllipseCache(ScreenPointF screenPoint, float width, float height, GeoPen outlinePen,
            BaseGeoBrush fillBrush, float xOffset, float yOffset, PenBrushDrawingOrder penBrushDrawingOrder)
        {
            var screenX = ToPoint(screenPoint.X + xOffset) + _drawingArea.Location.X;
            var screenY = ToPoint(screenPoint.Y + yOffset) + _drawingArea.Location.Y;

            if (penBrushDrawingOrder == PenBrushDrawingOrder.BrushFirst)
            {
                if (fillBrush != null)
                {
                    _xGraphics.DrawEllipse(GetXBrushFromGeoBrush(fillBrush, new[] {screenPoint}), screenX, screenY,
                        width, height);
                }
                if (outlinePen != null)
                {
                    _xGraphics.DrawEllipse(GetXPenFromGeoPen(outlinePen), screenX, screenY, width, height);
                }
            }
            else
            {
                if (outlinePen != null)
                {
                    _xGraphics.DrawEllipse(GetXPenFromGeoPen(outlinePen), screenX, screenY, width, height);
                }
                if (fillBrush != null)
                {
                    _xGraphics.DrawEllipse(GetXBrushFromGeoBrush(fillBrush, new[] {screenPoint}), screenX, screenY,
                        width, height);
                }
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
        protected override void DrawScreenImageWithoutScalingCore(GeoImage image, float centerXInScreen,
            float centerYInScreen, DrawingLevel drawingLevel, float xOffset, float yOffset, float rotateAngle)
        {
            if (!_featuresByLevel.ContainsKey(drawingLevel))
            {
                _featuresByLevel.Add(drawingLevel, new Collection<Dictionary<string, object>>());
            }

            var parameters = new Dictionary<string, object>();

            var buffer = new byte[image.GetImageStream(this).Length];
            image.GetImageStream(this).Read(buffer, 0, buffer.Length);
            var imageCopy = new GeoImage(new MemoryStream(buffer));

            parameters.Add("image", imageCopy);
            parameters.Add("centerXInScreen", centerXInScreen);
            parameters.Add("centerYInScreen", centerYInScreen);
            parameters.Add("xOffset", xOffset);
            parameters.Add("yOffset", yOffset);
            parameters.Add("rotateAngle", rotateAngle);
            parameters.Add("method", "DrawScreenImageWithoutScalingCache");

            _featuresByLevel[drawingLevel].Add(parameters);
        }

        private void DrawScreenImageWithoutScalingCache(GeoImage image, float centerXInScreen, float centerYInScreen,
            float xOffset, float yOffset, float rotateAngle)
        {
            var bitmap = new Bitmap(image.GetImageStream(this));
            var imageWidth = ToPoint(bitmap.Width);
            var imageHeight = ToPoint(bitmap.Height);

            var screenX = ToPoint(centerXInScreen) + _drawingArea.Location.X;
            var screenY = ToPoint(centerYInScreen) + _drawingArea.Location.Y;

            screenX += -imageWidth/2 + xOffset;
            screenY += -imageHeight/2 + yOffset;

            if (rotateAngle == 0)
            {
                try
                {
                    _xGraphics.DrawImage(bitmap, screenX, screenY);
                }
                finally
                {
                    bitmap.Dispose();
                }
            }
            else
            {
                var upperLeftPointX = -imageWidth*0.5f;
                var upperLeftPointY = imageHeight*0.5f;
                var rotateRadient = rotateAngle*Math.PI/180;
                var baseRadient = Math.PI - Math.Atan(imageHeight/imageWidth);
                var radius = Math.Sqrt(imageWidth*imageWidth + imageHeight*imageHeight)*0.5;
                var newRadient = baseRadient + rotateRadient;
                var newPointX = radius*Math.Cos(newRadient);
                var newPointY = radius*Math.Sin(newRadient);
                var xOffsetInScreen = newPointX - upperLeftPointX;
                var yOffsetInScreen = -(newPointY - upperLeftPointY);
                screenX += (float) xOffsetInScreen;
                screenY += (float) yOffsetInScreen;

                try
                {
                    bitmap = new Bitmap(image.GetImageStream(this));

                    _xGraphics.TranslateTransform(screenX, screenY);
                    _xGraphics.RotateTransform(-rotateAngle);

                    _xGraphics.DrawImage(bitmap, 0, 0);

                    _xGraphics.RotateTransform(rotateAngle);
                    _xGraphics.TranslateTransform(-screenX, -screenY);
                }
                finally
                {
                    bitmap.Dispose();
                }
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
        protected override void DrawScreenImageCore(GeoImage image, float centerXInScreen, float centerYInScreen,
            float widthInScreen, float heightInScreen, DrawingLevel drawingLevel, float xOffset, float yOffset,
            float rotateAngle)
        {
            var screenX = centerXInScreen;
            var screenY = centerYInScreen;

            screenX += -widthInScreen/2 + xOffset;
            screenY += -heightInScreen/2 + yOffset;

            if (rotateAngle == 0)
            {
                var destinateRectangle = new RectangleF(screenX, screenY, widthInScreen, heightInScreen);
                Bitmap bitmap = null;

                try
                {
                    bitmap = new Bitmap(image.GetImageStream(this));
                    var sourceRectangle = new RectangleF(0, 0, bitmap.Width, bitmap.Height);
                  
                    _xGraphics.DrawImage(bitmap, destinateRectangle, sourceRectangle, XGraphicsUnit.Point);
                }
                finally
                {
                    if (bitmap != null)
                    {
                        bitmap.Dispose();
                    }
                }
            }
            else
            {
                var upperLeftPointX = -widthInScreen*0.5;
                var upperLeftPointY = heightInScreen*0.5;
                var rotateRadient = rotateAngle*Math.PI/180;
                var baseRadient = Math.PI - Math.Atan(heightInScreen/widthInScreen);
                var radius = Math.Sqrt(widthInScreen*widthInScreen + heightInScreen*heightInScreen)*0.5;
                var newRadient = baseRadient + rotateRadient;
                var newPointX = radius*Math.Cos(newRadient);
                var newPointY = radius*Math.Sin(newRadient);
                var xOffsetInScreen = newPointX - upperLeftPointX;
                var yOffsetInScreen = -(newPointY - upperLeftPointY);
                screenX += (float) xOffsetInScreen;
                screenY += (float) yOffsetInScreen;

                var destinateRectangle = new RectangleF(0, 0, widthInScreen, heightInScreen);

                Bitmap bitmap = null;
                try
                {
                    bitmap = new Bitmap(image.GetImageStream(this));

                    var sourceRectangle = new RectangleF(0, 0, bitmap.Width, bitmap.Height);

                    _xGraphics.TranslateTransform(screenX, screenY);
                    _xGraphics.RotateTransform(-rotateAngle);
                    _xGraphics.DrawImage(bitmap, destinateRectangle, sourceRectangle, XGraphicsUnit.Point);

                    _xGraphics.RotateTransform(rotateAngle);
                    _xGraphics.TranslateTransform(-screenX, -screenY);
                }
                finally
                {
                    if (bitmap != null)
                    {
                        bitmap.Dispose();
                    }
                }
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
        protected override void DrawTextCore(string text, GeoFont font, BaseGeoBrush fillBrush, GeoPen haloPen,
            IEnumerable<ScreenPointF> textPathInScreen, DrawingLevel drawingLevel, float xOffset, float yOffset,
            float rotateAngle)
        {
            if (!_featuresByLevel.ContainsKey(drawingLevel))
            {
                _featuresByLevel.Add(drawingLevel, new Collection<Dictionary<string, object>>());
            }

            var parameters = new Dictionary<string, object>();
            parameters.Add("text", text);
            parameters.Add("font", font);
            parameters.Add("fillBrush", fillBrush);
            parameters.Add("haloPen", haloPen);
            parameters.Add("textPathInScreen", textPathInScreen);
            parameters.Add("rotateAngle", rotateAngle);
            parameters.Add("xOffset", xOffset);
            parameters.Add("yOffset", yOffset);
            parameters.Add("method", "DrawTextCache");

            _featuresByLevel[drawingLevel].Add(parameters);
        }

        private void DrawTextCache(string text, GeoFont font, BaseGeoBrush fillBrush, GeoPen haloPen,
            IEnumerable<ScreenPointF> textPathInScreen, float xOffset, float yOffset, float rotateAngle)
        {
            var gdiFont = GetXFontFromGeoFont(font);
            var rectangle = MeasureText(text, font);

            double width = rectangle.Width;
            double height = rectangle.Height;

            double rotateRadient;
            double xOffsetRotated = 0;
            double yOffsetRotated = 0;

            if (rotateAngle != 0)
            {
                var upperLeftX = -width*0.5;
                var upperLeftY = height*0.5;

                var radius = Math.Sqrt(width*width + height*height)*0.5;

                rotateRadient = rotateAngle*Math.PI/180;

                var newRadient = rotateRadient - Math.Atan(height/width) + Math.PI;

                xOffsetRotated = radius*Math.Cos(newRadient) - upperLeftX;
                yOffsetRotated = -(radius*Math.Sin(newRadient) - upperLeftY);
            }

            foreach (var point in textPathInScreen)
            {
                var pointForLabel = new ScreenPointF((float) (point.X - width*0.5), (float) (point.Y - height*0.5));

                if (rotateAngle == 0)
                {
                    DrawString(text, gdiFont, fillBrush, haloPen,
                        new PointF(ToPoint(pointForLabel.X + xOffset) + _drawingArea.Location.X,
                            ToPoint(pointForLabel.Y + yOffset) + _drawingArea.Location.Y), width, height);
                }
                else
                {
                    var newPointX = ToPoint((float) (pointForLabel.X + xOffset + xOffsetRotated)) +
                                    _drawingArea.Location.X;
                    var newPointY = ToPoint((float) (pointForLabel.Y + yOffset + yOffsetRotated)) +
                                    _drawingArea.Location.Y;

                    _xGraphics.TranslateTransform(newPointX, newPointY);
                    _xGraphics.RotateTransform(-rotateAngle);

                    DrawString(text, gdiFont, fillBrush, haloPen, new PointF(0, 0), width, height);

                    _xGraphics.RotateTransform(rotateAngle);
                    _xGraphics.TranslateTransform(-newPointX, -newPointY);
                }
            }
        }

        /// <summary>Returns the rectangle containing a text.</summary>
        /// <returns>A rectangle containing a text taking into account the font.</returns>
        /// <remarks>Typically used for labeling logic to determine the overlapping of labels.</remarks>
        /// <param name="text">Text to measure.</param>
        /// <param name="font">Font of the text to measure.</param>
        protected override DrawingRectangleF MeasureTextCore(string text, GeoFont font)
        {
            var graphics = _xGraphics;
            XSize size;

            size = graphics.MeasureString(text, GetXFontFromGeoFont(font), XStringFormats.TopLeft);

            return new DrawingRectangleF((float) size.Width/2, (float) size.Height/2, (float) size.Width,
                (float) size.Height);
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
        public void BeginDrawing(object nativeImage, RectangleShape worldExtent, GeographyUnit drawingMapUnit)
        {
            if (!IsCancelled)
            {
                IsDrawing = true;
                MapUnit = drawingMapUnit;
                NativeImage = nativeImage;

                _hasDrawn = false;
                _currentExtent = worldExtent;

                BeginDrawingCore(nativeImage, worldExtent, drawingMapUnit);

                if (_geoCanvasWidth == 0 && _geoCanvasHeight == 0)
                {
                    var image = nativeImage as GeoImage;

                    if (image != null)
                    {
                        _geoCanvasWidth = image.GetWidth();
                        _geoCanvasHeight = image.GetHeight();
                    }
                    else
                    {
                        _geoCanvasWidth = GetCanvasWidth(nativeImage);
                        _geoCanvasHeight = GetCanvasHeight(nativeImage);
                    }
                }

                _currentScale = ExtentHelper.GetScale(worldExtent, _geoCanvasWidth, drawingMapUnit, _dpi);

                _worldToScreenFactorX = _geoCanvasWidth / _currentExtent.Width;
                _worldToScreenFactorY = _geoCanvasHeight / _currentExtent.Height;
            }
        }


        private void BeginDrawingCore(object nativeImage, RectangleShape worldExtent,
            GeographyUnit drawingMapUnit)
        {
            var page = (PdfPage) nativeImage;
            _xGraphics = XGraphics.FromPdfPage(page);
            if (_drawingArea.Width == 0)
            {
                Width = (float) page.Width.Presentation;
                Height = (float) page.Height.Presentation;
            }
            else
            {
                _xGraphics.IntersectClip(_drawingArea);
                Width = ToPixel(_drawingArea.Width);
                Height = ToPixel(_drawingArea.Height);
            }

            CurrentWorldExtent = ExtentHelper.GetDrawingExtent(worldExtent, Width, Height);

            SetGraphicsMode();
        }

        /// <summary>Ends drawing and commits the drawing on canvas.</summary>
        /// <remarks>Last method to call after finished drawing. Commits the
        /// changes to the image passed in BeginDrawing. Sets IsDrawing to false. 
        /// Finally puts the canvas into invalid state.</remarks>
        public void EndDrawing()
        {
            _hasDrawn = false;
            NativeImage = null;
            EndDrawingCore();

            IsDrawing = false;
        }

        private void EndDrawingCore()
        {
            if (_featuresByLevel.ContainsKey(DrawingLevel.LevelOne))
            {
                DrawLevel(_featuresByLevel[DrawingLevel.LevelOne]);
            }
            if (_featuresByLevel.ContainsKey(DrawingLevel.LevelTwo))
            {
                DrawLevel(_featuresByLevel[DrawingLevel.LevelTwo]);
            }
            if (_featuresByLevel.ContainsKey(DrawingLevel.LevelThree))
            {
                DrawLevel(_featuresByLevel[DrawingLevel.LevelThree]);
            }
            if (_featuresByLevel.ContainsKey(DrawingLevel.LevelFour))
            {
                DrawLevel(_featuresByLevel[DrawingLevel.LevelFour]);
            }
            if (_featuresByLevel.ContainsKey(DrawingLevel.LabelLevel))
            {
                DrawLevel(_featuresByLevel[DrawingLevel.LabelLevel]);
            }

            _featuresByLevel.Clear();
            ClearCache();
            _xGraphics.Dispose();
        }

        private void DrawLevel(Collection<Dictionary<string, object>> drawingItems)
        {
            foreach (var item in drawingItems)
            {
                switch (item["method"].ToString())
                {
                    case "DrawAreaCache":
                        DrawAreaCache((IEnumerable<ScreenPointF[]>) item["screenPoints"], (GeoPen) item["outlinePen"],
                            (BaseGeoBrush) item["fillBrush"], (float) item["xOffset"], (float) item["yOffset"],
                            (PenBrushDrawingOrder) item["penBrushDrawingOrder"]);
                        break;
                    case "DrawLineCache":
                        DrawLineCache((IEnumerable<ScreenPointF>) item["screenPoints"], (GeoPen) item["linePen"],
                            (float) item["xOffset"], (float) item["yOffset"]);
                        break;
                    case "DrawTextCache":
                        DrawTextCache((string) item["text"], (GeoFont) item["font"], (BaseGeoBrush) item["fillBrush"],
                            (GeoPen) item["haloPen"], (IEnumerable<ScreenPointF>) item["textPathInScreen"],
                            (float) item["xOffset"], (float) item["yOffset"], (float) item["rotateAngle"]);
                        break;
                    case "DrawEllipseCache":
                        DrawEllipseCache((ScreenPointF) item["screenPoint"], (float) item["width"],
                            (float) item["height"], (GeoPen) item["outlinePen"], (BaseGeoBrush) item["fillBrush"],
                            (float) item["xOffset"], (float) item["yOffset"],
                            (PenBrushDrawingOrder) item["penBrushDrawingOrder"]);
                        break;
                    case "DrawScreenImageWithoutScalingCache":
                        DrawScreenImageWithoutScalingCache((GeoImage) item["image"], (float) item["centerXInScreen"],
                            (float) item["centerYInScreen"], (float) item["xOffset"], (float) item["yOffset"],
                            (float) item["rotateAngle"]);
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>Converts an object to a GeoImage.</summary>
        /// <param name="nativeImage">Target object to convert.</param>
        /// <returns>GeoImage containing.</returns>
        protected override object ToNativeImageCore(GeoImage image)
        {
            throw new NotSupportedException();
        }

        /// <summary>Converts an object to a GeoImage.</summary>
        /// <param name="nativeImage">Target object to convert.</param>
        /// <returns>GeoImage containing.</returns>
        protected override GeoImage ToGeoImageCore(object nativeImage)
        {
            throw new NotSupportedException();
        }

        /// <summary>Returns stream representing the GeoImage in TIFF format.</summary>
        /// <returns>Stream that represents the GeoImage in TIFF format.</returns>
        /// <remarks>Allows to get the stream out of a GeoImage. The stream is a
        /// memory stream and the bytes are in TIFF format. Can be saved in the desired format.</remarks>
        /// <param name="image">GeoImage to convert to a stream.</param>
        public override Stream GetStreamFromGeoImage(GeoImage image)
        {
            if (string.IsNullOrEmpty(image.PathFilename))
            {
                return null;
            }

            MemoryStream imageStream;
            Bitmap bitmap = null;

            try
            {
                imageStream = new MemoryStream();
                bitmap = new Bitmap(image.PathFilename);

                image.CanvasImageFormat = bitmap.RawFormat.Guid;
                bitmap.Save(imageStream, ImageFormat.Png);
                imageStream.Seek(0, SeekOrigin.Begin);
            }
            finally
            {
                if (bitmap != null)
                {
                    bitmap.Dispose();
                }
            }

            return imageStream;
        }

        /// <summary>Converts GeoImage to memory stream.</summary>
        /// <returns>Memory stream with the bytes formatted.</returns>
        /// <param name="image">GeoImage to convert to a memory stream.</param>
        public static MemoryStream ConvertGeoImageToMemoryStream(GeoImage image)
        {
            var canvas = new GdiPlusGeoCanvas();
            var memoryStream = new MemoryStream();
            var stream = image.GetImageStream(canvas);
            for (var i = 0; i < stream.Length; i++)
            {
                memoryStream.WriteByte((byte) (stream.ReadByte()));
            }
            memoryStream.Seek(0, SeekOrigin.Begin);
            return memoryStream;
        }

        /// <summary>Converts GeoImage to memory stream.</summary>
        /// <overloads>Saves the bytes to a memory stream in the format specified.</overloads>
        /// <returns>Memory stream with the bytes formatted according to the image format specified.</returns>
        /// <param name="image">GeoImage to convert to a memory stream.</param>
        /// <param name="imageFormat">Image format the stream to be in.</param>
        public static MemoryStream ConvertGeoImageToMemoryStream(GeoImage image, ImageFormat imageFormat)
        {
            var canvas = new GdiPlusGeoCanvas();
            var memoryStream = new MemoryStream();
            var stream = image.GetImageStream(canvas);
            Bitmap bitmap = null;

            try
            {
                bitmap = new Bitmap(stream);
                bitmap.Save(memoryStream, imageFormat);
                memoryStream.Seek(0, SeekOrigin.Begin);
            }
            finally
            {
                if (bitmap != null)
                {
                    bitmap.Dispose();
                }
            }

            return memoryStream;
        }

        /// <summary>Returns a GeoImage based height and width.</summary>
        /// <returns>Returns a GeoImage based on height and width.</returns>
        /// <remarks>Use this static member to create a GeoCanvas.</remarks>
        /// <param name="width">Width of the GeoImage.</param>
        /// <param name="height">Height of the GeoImage.</param>
        public static GeoImage CreateGeoImage(int width, int height)
        {
            Bitmap bitmap = null;
            GeoImage geoImage;
            try
            {
                bitmap = new Bitmap(width, height);
                var imageStream = new MemoryStream();
                bitmap.Save(imageStream, ImageFormat.Png);
                imageStream.Seek(0, SeekOrigin.Begin);
                geoImage = new GeoImage(imageStream);
            }
            finally
            {
                if (bitmap != null)
                {
                    bitmap.Dispose();
                }
            }

            return geoImage;
        }

        private void DrawString(string text, XFont font, BaseGeoBrush fillBrush, GeoPen haloPen, PointF position,
            double width, double height)
        {
            var brush = GetXBrushFromGeoBrush(fillBrush,
                new[]
                {
                    new ScreenPointF(position.X, position.Y),
                    new ScreenPointF(position.X + (float) width, position.Y + (float) height)
                });
            var pen = GetXPenFromGeoPen(haloPen);

            if (haloPen == null || haloPen.Brush == null || haloPen.Color.AlphaComponent == 0)
            {
                _xGraphics.DrawString(text, font, brush, position.X, position.Y, XStringFormats.TopLeft);
            }
            else
            {
                var tempSmoothingMode = _xGraphics.SmoothingMode;
                XGraphicsPath path;
                float xOffset = 0;
                float yOffset = 0;

                try
                {
                    path = new XGraphicsPath();

                    path.AddString(text, font.FontFamily, font.Style, font.Size, position, XStringFormats.TopLeft);

                    var bound = path.Internals.GdiPath.GetBounds();
                    var boundWidth = bound.Width;
                    var boundHeight = bound.Height;

                    xOffset = (float) ((width - boundWidth)*0.5);
                    yOffset = (float) ((height - boundHeight)*0.3);

                    _xGraphics.SmoothingMode = XSmoothingMode.AntiAlias;

                    _xGraphics.TranslateTransform(xOffset, yOffset);

                    _xGraphics.DrawPath(pen, path);
                    _xGraphics.DrawPath(brush, path);
                }
                finally
                {
                    _xGraphics.SmoothingMode = tempSmoothingMode;
                    _xGraphics.TranslateTransform(-xOffset, -yOffset);
                }
            }
        }

        private void ClearCache()
        {
            if (_brushCache != null)
            {
                _brushCache.Clear();
            }
            if (_penCache != null)
            {
                _penCache.Clear();
            }
            if (_fontCache != null)
            {
                _fontCache.Clear();
            }
        }

        private XFont GetXFontFromGeoFont(GeoFont font)
        {
            if (font == null)
            {
                return null;
            }
            if (_fontCache == null)
            {
                _fontCache = new Dictionary<long, XFont>();
            }

            if (_fontCache.ContainsKey(font.GetHashCode()))
            {
                return _fontCache[font.GetHashCode()];
            }

            var gdiplusFontStyle = GetXFontStyleFromDrawingFontStyle(font.Style);
            var resultFont = new XFont(font.FontName, ToPoint(font.Size), gdiplusFontStyle);
            _fontCache.Add(font.GetHashCode(), resultFont);

            return resultFont;
        }

        private XBrush GetXBrushFromGeoBrush(BaseGeoBrush brush, ScreenPointF[] areaPointsCache)
        {
            if (brush == null)
            {
                return null;
            }

            XBrush resultBrush = null;


            if (_brushCache == null)
            {
                _brushCache = new Dictionary<long, XBrush>();
            }

            if (_brushCache.ContainsKey(brush.Id))
            {
                return _brushCache[brush.Id];
            }
            var typeName = brush.GetType().Name;

            switch (typeName)
            {
                case "GeoSolidBrush":
                    var geoSolidBrush = brush as GeoSolidBrush;
                    if (geoSolidBrush != null)
                    {
                        resultBrush = GetXBrushFromGeoSolidBrush(geoSolidBrush);
                    }
                    break;
                case "GeoLinearGradientBrush":
                    throw new NotSupportedException();
                default:
                    throw new NotSupportedException();
            }
            _brushCache.Add(brush.Id, resultBrush);

            return resultBrush;
        }

        private XPen GetXPenFromGeoPen(GeoPen pen)
        {
            if (pen == null)
            {
                return null;
            }

            if (_penCache == null)
            {
                _penCache = new Dictionary<long, XPen>();
            }

            if (_penCache.ContainsKey(pen.Id))
            {
                return _penCache[pen.Id];
            }

            var resultPen = new XPen(GetXColorFromGeoColor(pen.Color));
            resultPen.Width = ToPoint(pen.Width);

            if (pen.Brush != null)
            {
                if (pen.Brush is GeoSolidBrush)
                {
                    resultPen.Color = GetXColorFromGeoColor(((GeoSolidBrush) pen.Brush).Color);
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
            else
            {
                resultPen.Color = GetXColorFromGeoColor(pen.Color);
            }

            if (pen.DashCap != GeoDashCap.Flat)
            {
                throw new NotSupportedException();
            }

            resultPen.LineJoin = GetXLineJoinFromDrawingLingJoin(pen.LineJoin);
            resultPen.MiterLimit = pen.MiterLimit;
            resultPen.LineCap = GetXLineCapFromDrawingLineCap(pen.StartCap);

            if (pen.DashPattern != null && pen.DashPattern.Count > 0)
            {
                var dashPattern = new double[pen.DashPattern.Count];
                for (var i = 0; i < dashPattern.Length; i++)
                {
                    dashPattern[i] = pen.DashPattern[i];
                }
                resultPen.DashPattern = dashPattern;
            }

            _penCache.Add(pen.Id, resultPen);

            return resultPen;
        }

        private static XColor GetXColorFromGeoColor(GeoColor color)
        {
            return Color.FromArgb(color.AlphaComponent, color.RedComponent, color.GreenComponent, color.BlueComponent);
        }

        private static XBrush GetXBrushFromGeoSolidBrush(GeoSolidBrush brush)
        {
            var color = GetXColorFromGeoColor(brush.Color);
            return new XSolidBrush(color);
        }


        private static XFontStyle GetXFontStyleFromDrawingFontStyle(DrawingFontStyles style)
        {
            var returnFontStyle = XFontStyle.Regular;
            var value = (int) style;
            if (value < 1 ||
                value >
                (int)
                    (DrawingFontStyles.Regular | DrawingFontStyles.Bold | DrawingFontStyles.Italic |
                     DrawingFontStyles.Underline | DrawingFontStyles.Strikeout))
            {
                throw new ArgumentOutOfRangeException("style", "");
            }

            if ((style & DrawingFontStyles.Bold) != 0)
            {
                returnFontStyle = returnFontStyle | XFontStyle.Bold;
            }
            if ((style & DrawingFontStyles.Italic) != 0)
            {
                returnFontStyle = returnFontStyle | XFontStyle.Italic;
            }
            if ((style & DrawingFontStyles.Underline) != 0)
            {
                returnFontStyle = returnFontStyle | XFontStyle.Underline;
            }
            if ((style & DrawingFontStyles.Strikeout) != 0)
            {
                returnFontStyle = returnFontStyle | XFontStyle.Strikeout;
            }

            return returnFontStyle;
        }

        private static XLineCap GetXLineCapFromDrawingLineCap(DrawingLineCap lineCap)
        {
            XLineCap returnLineCap;

            switch (lineCap)
            {
                case DrawingLineCap.Round:
                    returnLineCap = XLineCap.Round;
                    break;
                case DrawingLineCap.Square:
                    returnLineCap = XLineCap.Square;
                    break;
                case DrawingLineCap.Flat:
                    returnLineCap = XLineCap.Flat;
                    break;
                default:
                    throw new NotSupportedException("The type of: " + lineCap +
                                                    " is not supported now! Round, Square, Flat, this three types are supported now");
            }

            return returnLineCap;
        }

        private static XLineJoin GetXLineJoinFromDrawingLingJoin(DrawingLineJoin lineJoin)
        {
            XLineJoin returnLineJoin;

            switch (lineJoin)
            {
                case DrawingLineJoin.Bevel:
                    returnLineJoin = XLineJoin.Bevel;
                    break;
                case DrawingLineJoin.Miter:
                    returnLineJoin = XLineJoin.Miter;
                    break;
                case DrawingLineJoin.Round:
                    returnLineJoin = XLineJoin.Round;
                    break;
                default:
                    throw new NotSupportedException();
            }

            return returnLineJoin;
        }

        private void SetGraphicsMode()
        {
            switch (DrawingQuality)
            {
                case DrawingQuality.Default:
                    _xGraphics.SmoothingMode = XSmoothingMode.HighQuality;
                    break;
                case DrawingQuality.HighQuality:
                    _xGraphics.SmoothingMode = XSmoothingMode.AntiAlias;
                    break;
                case DrawingQuality.HighSpeed:
                    _xGraphics.SmoothingMode = XSmoothingMode.HighSpeed;
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        /// <summary>Gets the canvas width of a native image object.</summary>
        /// <param name="nativeImage">Native image used to get the canvas width.</param>
        /// <returns>Canvas width.</returns>
        protected override float GetCanvasWidthCore(object nativeImage)
        {
            var page = (PdfPage) nativeImage;
            return (float) page.Width.Presentation;
        }

        /// <summary>Gets the canvas height of a native image object.</summary>
        /// <param name="nativeImage">Native image will be used to get the canvas height.</param>
        /// <returns>Canvas height.</returns>
        protected override float GetCanvasHeightCore(object nativeImage)
        {
            var page = (PdfPage) nativeImage;
            return (float) page.Height.Presentation;
        }

        /// <summary>Flushes drawing and commits the drawing on the canvas.</summary>
        /// <remarks>Method to call when drawing finished. It commits the
        /// image changes to the image passed in on BeginDrawing. Also it sets IsDrawing to false. Finally it sets GeoCanvas to invalid state, 
        /// not allowing further drawing.</remarks>
        protected override void FlushCore()
        {
        }
    }
}
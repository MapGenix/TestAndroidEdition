using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.IO;
using Mapgenix.Shapes;


namespace Mapgenix.Canvas
{
    /// <summary>Canvas for the drawing of the layers on printer page.</summary>
    /// <remarks>
    /// 	<para>The first call is always BeginDrawing by passing in an image along with its world extent.</para>
    /// </remarks>
    [Serializable]
    public class PrinterGeoCanvas : BaseGeoCanvas
    {
        private const int PrinterDpi = 96;

        private readonly Dictionary<DrawingLevel, Collection<Dictionary<string, object>>> _featuresByLevel;
        private Dictionary<long, Brush> _brushCache;
        private Rectangle _drawingArea;
        private Dictionary<long, Font> _fontCache;
        private bool _isMessure;

        private GeoFont _messureFont;
        private Dictionary<long, Pen> _penCache;

        [NonSerialized] private PrintDocument _printDocument;

        [NonSerialized] private PrintPageEventArgs _printPageEventArgs;

        private bool _stopPrint;

        public PrinterGeoCanvas()
        {
            _featuresByLevel = new Dictionary<DrawingLevel, Collection<Dictionary<string, object>>>();
            _stopPrint = true;
        }

        public Rectangle DrawingArea
        {
            get { return _drawingArea; }
            set { _drawingArea = value; }
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
            _printDocument = (PrintDocument) nativeImage;

            _printDocument.PrintPage += printDocument_PrintPage;

            Width = _drawingArea.Width;
            Height = _drawingArea.Height;

            CurrentWorldExtent = ExtentHelper.GetDrawingExtent(worldExtent, Width, Height);
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
            _stopPrint = false;

            _printDocument.Print();

            ClearCache();
        }

        private void printDocument_PrintPage(object sender, PrintPageEventArgs e)
        {
            _printPageEventArgs = new PrintPageEventArgs(e.Graphics, e.MarginBounds, e.PageBounds, e.PageSettings);


            if (_featuresByLevel.ContainsKey(DrawingLevel.LevelOne))
            {
                DrawLevel(_featuresByLevel[DrawingLevel.LevelOne], e);
            }
            if (_featuresByLevel.ContainsKey(DrawingLevel.LevelTwo))
            {
                DrawLevel(_featuresByLevel[DrawingLevel.LevelTwo], e);
            }
            if (_featuresByLevel.ContainsKey(DrawingLevel.LevelThree))
            {
                DrawLevel(_featuresByLevel[DrawingLevel.LevelThree], e);
            }
            if (_featuresByLevel.ContainsKey(DrawingLevel.LevelFour))
            {
                DrawLevel(_featuresByLevel[DrawingLevel.LevelFour], e);
            }
            if (_isMessure)
            {
                if (_featuresByLevel.ContainsKey(DrawingLevel.LabelLevel))
                {
                    DrawLevel(_featuresByLevel[DrawingLevel.LabelLevel], e);
                }
            }

            _featuresByLevel.Clear();
            _printDocument.Dispose();

            if (_stopPrint)
            {
                _printPageEventArgs.Cancel = true;
                e.Cancel = true;
            }
        }

        private DrawingRectangleF MeasureTextApi(Graphics g, string text, GeoFont font)
        {
            var gdiPlusFont = GetGdiPlusFontFromGeoFont(font);

            var size = g.MeasureString(text, gdiPlusFont);

            return new DrawingRectangleF(size.Width/2, size.Height/2, size.Width, size.Height);
        }

        private void DrawLevel(Collection<Dictionary<string, object>> drawingItems, PrintPageEventArgs e)
        {
            var clipWidth = ToPrinterDpiPixel(_drawingArea.Width);
            var clipHeight = ToPrinterDpiPixel(_drawingArea.Height);
            e.Graphics.SetClip(new RectangleF(_drawingArea.Left, _drawingArea.Top, clipWidth, clipHeight));
            foreach (var item in drawingItems)
            {
                switch (item["method"].ToString())
                {
                    case "DrawAreaCache":
                        DrawAreaCache((IEnumerable<ScreenPointF[]>) item["screenPoints"], (GeoPen) item["outlinePen"],
                            (BaseGeoBrush) item["fillBrush"], (float) item["xOffset"], (float) item["yOffset"],
                            (PenBrushDrawingOrder) item["penBrushDrawingOrder"], e);
                        break;
                    case "DrawLineCache":
                        DrawLineCache((IEnumerable<ScreenPointF>) item["screenPoints"], (GeoPen) item["linePen"],
                            (float) item["xOffset"], (float) item["yOffset"], e);
                        break;
                    case "DrawTextCache":
                        DrawTextCache((string) item["text"], (GeoFont) item["font"], (BaseGeoBrush) item["fillBrush"],
                            (GeoPen) item["haloPen"], (IEnumerable<ScreenPointF>) item["textPathInScreen"],
                            (float) item["xOffset"], (float) item["yOffset"], (float) item["rotateAngle"], e);
                        break;
                    case "DrawEllipseCache":
                        DrawEllipseCache((ScreenPointF) item["screenPoint"], (float) item["width"],
                            (float) item["height"], (GeoPen) item["outlinePen"], (BaseGeoBrush) item["fillBrush"],
                            (float) item["xOffset"], (float) item["yOffset"],
                            (PenBrushDrawingOrder) item["penBrushDrawingOrder"], e);
                        break;
                    case "DrawScreenImageWithoutScalingCache":
                        DrawScreenImageWithoutScalingCache((GeoImage) item["image"], (float) item["centerXInScreen"],
                            (float) item["centerYInScreen"], (float) item["xOffset"], (float) item["yOffset"],
                            (float) item["rotateAngle"], e);
                        break;
                    case "DrawScreenImageCache":
                        DrawScreenImageCache((GeoImage) item["image"], (float) item["centerXInScreen"],
                            (float) item["centerYInScreen"], (float) item["widthInScreen"],
                            (float) item["heightInScreen"], (float) item["xOffset"], (float) item["yOffset"],
                            (float) item["rotateAngle"], e);
                        break;
                    default:
                        break;
                }
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
            float xOffset, float yOffset, PenBrushDrawingOrder penBrushDrawingOrder, PrintPageEventArgs e)
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
                    var screenX = ToPrinterDpiPixel(point.X + xOffset) + _drawingArea.Location.X;
                    var screenY = ToPrinterDpiPixel(point.Y + yOffset) + _drawingArea.Location.Y;

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
                        e.Graphics.FillPolygon(GetGdiPlusBrushFromGeoBrush(fillBrush), ringsCollection[0],
                            FillMode.Winding);
                    }
                    if (outlinePen != null)
                    {
                        e.Graphics.DrawPolygon(new Pen(GetGdiPlusColorFromGeoColor(outlinePen.Color)),
                            ringsCollection[0]);
                    }
                }
                else
                {
                    if (outlinePen != null)
                    {
                        e.Graphics.DrawPolygon(new Pen(GetGdiPlusColorFromGeoColor(outlinePen.Color)),
                            ringsCollection[0]);
                    }
                    if (fillBrush != null)
                    {
                        e.Graphics.FillPolygon(GetGdiPlusBrushFromGeoBrush(fillBrush), ringsCollection[0],
                            FillMode.Winding);
                    }
                }
            }
            else if (count != 0)
            {
                var graphicsPath = new GraphicsPath();

                foreach (var points in ringsCollection)
                {
                    graphicsPath.AddPolygon(points);
                }

                if (penBrushDrawingOrder == PenBrushDrawingOrder.BrushFirst)
                {
                    if (fillBrush != null)
                    {
                        e.Graphics.FillPath(GetGdiPlusBrushFromGeoBrush(fillBrush), graphicsPath);
                    }
                    if (outlinePen != null)
                    {
                        e.Graphics.DrawPath(GetGdiPlusPenFromGeoPen(outlinePen), graphicsPath);
                    }
                }
                else
                {
                    if (outlinePen != null)
                    {
                        e.Graphics.DrawPath(GetGdiPlusPenFromGeoPen(outlinePen), graphicsPath);
                    }
                    if (fillBrush != null)
                    {
                        e.Graphics.FillPath(GetGdiPlusBrushFromGeoBrush(fillBrush), graphicsPath);
                    }
                }
            }
        }

        private void DrawLineCache(IEnumerable<ScreenPointF> screenPoints, GeoPen linePen, float xOffset, float yOffset,
            PrintPageEventArgs e)
        {
            var points = new List<PointF>();

            var previousX = float.MaxValue;
            var previousY = float.MaxValue;

            foreach (var point in screenPoints)
            {
                var screenX = ToPrinterDpiPixel(point.X + xOffset) + _drawingArea.Location.X;
                var screenY = ToPrinterDpiPixel(point.Y + yOffset) + _drawingArea.Location.Y;
                if (previousX != screenX || previousY != screenY)
                {
                    previousX = screenX;
                    previousY = screenY;
                    points.Add(new PointF(previousX, previousY));
                }
            }

            if (points.Count > 1)
            {
                var gdiPlusPen = GetGdiPlusPenFromGeoPen(linePen);
                e.Graphics.DrawLines(gdiPlusPen, points.ToArray());
            }
        }

        private void DrawTextCache(string text, GeoFont font, BaseGeoBrush fillBrush, GeoPen haloPen,
            IEnumerable<ScreenPointF> textPathInScreen, float xOffset, float yOffset, float rotateAngle,
            PrintPageEventArgs e)
        {
            var gdiFont = GetGdiPlusFontFromGeoFont(font);
            var rectangle = new DrawingRectangleF();
            if (_messureFont != null)
            {
                rectangle = MeasureTextApi(e.Graphics, text, font);
            }

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
                        new PointF(ToPrinterDpiPixel(pointForLabel.X + xOffset) + _drawingArea.Location.X,
                            ToPrinterDpiPixel(pointForLabel.Y + yOffset) + _drawingArea.Location.Y), width, height, e);
                }
                else
                {
                    var newPointX = ToPrinterDpiPixel((float) (pointForLabel.X + xOffset + xOffsetRotated)) +
                                    _drawingArea.Location.X;
                    var newPointY = ToPrinterDpiPixel((float) (pointForLabel.Y + yOffset + yOffsetRotated)) +
                                    _drawingArea.Location.Y;

                    e.Graphics.TranslateTransform(newPointX, newPointY);
                    e.Graphics.RotateTransform(-rotateAngle);

                    DrawString(text, gdiFont, fillBrush, haloPen, new PointF(0, 0), width, height, e);

                    e.Graphics.RotateTransform(rotateAngle);
                    e.Graphics.TranslateTransform(-newPointX, -newPointY);
                }
            }
        }

        private void DrawString(string text, Font font, BaseGeoBrush fillBrush, GeoPen haloPen, PointF position,
            double width, double height, PrintPageEventArgs e)
        {
            var brush = GetGdiPlusBrushFromGeoBrush(fillBrush);
            var pen = GetGdiPlusPenFromGeoPen(haloPen);

            if (haloPen == null || haloPen.Brush == null || haloPen.Color.AlphaComponent == 0)
            {
                e.Graphics.DrawString(text, font, brush, position.X, position.Y, StringFormat.GenericDefault);
            }
            else
            {
                var tempSmoothingMode = e.Graphics.SmoothingMode;
                GraphicsPath path;
                float xOffset = 0;
                float yOffset = 0;

                try
                {
                    path = new GraphicsPath();

                    path.AddString(text, font.FontFamily, (int) font.Style, font.Size, position,
                        StringFormat.GenericDefault);

                    var bound = path.GetBounds();
                    var boundWidth = bound.Width;
                    var boundHeight = bound.Height;

                    xOffset = (float) ((width - boundWidth)*0.5);
                    yOffset = (float) ((height - boundHeight)*0.3);

                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

                    e.Graphics.TranslateTransform(xOffset, yOffset);

                    e.Graphics.DrawPath(pen, path);
                    e.Graphics.FillPath(brush, path);
                }
                finally
                {
                    e.Graphics.SmoothingMode = tempSmoothingMode;
                    e.Graphics.TranslateTransform(-xOffset, -yOffset);
                }
            }
        }

        private Brush GetGdiPlusBrushFromGeoBrush(BaseGeoBrush brush) 
        {
            if (brush == null)
            {
                return null;
            }

            Brush resultBrush = null;

            if (_brushCache == null)
            {
                _brushCache = new Dictionary<long, Brush>();
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
                        resultBrush = GetGdiPlusBrushFromGeoSolidBrush(geoSolidBrush);
                    }
                    break;
                case "GeoHatchBrush":
                    var geoHatchBrush = brush as GeoHatchBrush;
                    if (geoHatchBrush != null)
                    {
                        resultBrush = GetGdiPlusBrushFromGeoHatchBrush(geoHatchBrush);
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

        private Pen GetGdiPlusPenFromGeoPen(GeoPen pen)
        {
            if (pen == null)
            {
                return null;
            }

            if (_penCache == null)
            {
                _penCache = new Dictionary<long, Pen>();
            }

            if (_penCache.ContainsKey(pen.Id))
            {
                return _penCache[pen.Id];
            }

            var resultPen = new Pen(GetGdiPlusColorFromGeoColor(pen.Color));
            resultPen.Width = ToPrinterDpiPixel(pen.Width);

            if (pen.Brush != null)
            {
                if (pen.Brush is GeoSolidBrush)
                {
                    resultPen.Color = GetGdiPlusColorFromGeoColor(((GeoSolidBrush) pen.Brush).Color);
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
            else
            {
                resultPen.Color = GetGdiPlusColorFromGeoColor(pen.Color);
            }

            if (pen.DashCap != GeoDashCap.Flat)
            {
                throw new NotSupportedException();
            }

            resultPen.LineJoin = GetGdiPlusLineJoinFromDrawingLingJoin(pen.LineJoin);
            resultPen.MiterLimit = pen.MiterLimit;
            resultPen.StartCap = GetGdiPlusLineCapFromDrawingLineCap(pen.StartCap);
            resultPen.EndCap = GetGdiPlusLineCapFromDrawingLineCap(pen.EndCap);

            if (pen.DashPattern != null && pen.DashPattern.Count > 0)
            {
                var dashPattern = new float[pen.DashPattern.Count];
                for (var i = 0; i < dashPattern.Length; i++)
                {
                    dashPattern[i] = pen.DashPattern[i];
                }
                resultPen.DashPattern = dashPattern;
            }

            _penCache.Add(pen.Id, resultPen);

            return resultPen;
        }

        private static LineJoin GetGdiPlusLineJoinFromDrawingLingJoin(DrawingLineJoin lineJoin)
        {
            LineJoin returnLineJoin;

            switch (lineJoin)
            {
                case DrawingLineJoin.Bevel:
                    returnLineJoin = LineJoin.Bevel;
                    break;
                case DrawingLineJoin.Miter:
                    returnLineJoin = LineJoin.Miter;
                    break;
                case DrawingLineJoin.Round:
                    returnLineJoin = LineJoin.Round;
                    break;
                default:
                    throw new NotSupportedException();
            }

            return returnLineJoin;
        }

        private static LineCap GetGdiPlusLineCapFromDrawingLineCap(DrawingLineCap lineCap)
        {
            LineCap returnLineCap;

            switch (lineCap)
            {
                case DrawingLineCap.Round:
                    returnLineCap = LineCap.Round;
                    break;
                case DrawingLineCap.Square:
                    returnLineCap = LineCap.Square;
                    break;
                case DrawingLineCap.Flat:
                    returnLineCap = LineCap.Flat;
                    break;
                default:
                    throw new NotSupportedException();
            }

            return returnLineCap;
        }

        private static Brush GetGdiPlusBrushFromGeoSolidBrush(GeoSolidBrush brush)
        {
            var color = GetGdiPlusColorFromGeoColor(brush.Color);
            return new SolidBrush(color);
        }

        private static Brush GetGdiPlusBrushFromGeoHatchBrush(GeoHatchBrush brush)
        {
            var foregroundColor = GetGdiPlusColorFromGeoColor(brush.ForegroundColor);
            var backgroundColor = GetGdiPlusColorFromGeoColor(brush.BackgroundColor);
            var hatchStyle = GetGdiPlusHatchStyleFromGeoHatchStyle(brush.HatchStyle);

            return new HatchBrush(hatchStyle, foregroundColor, backgroundColor);
        }

        private void DrawEllipseCache(ScreenPointF screenPoint, float width, float height, GeoPen outlinePen,
            BaseGeoBrush fillBrush, float xOffset, float yOffset, PenBrushDrawingOrder penBrushDrawingOrder,
            PrintPageEventArgs e)
        {
            var screenX = ToPrinterDpiPixel(screenPoint.X + xOffset) + _drawingArea.Location.X;
            var screenY = ToPrinterDpiPixel(screenPoint.Y + yOffset) + _drawingArea.Location.Y;

            if (penBrushDrawingOrder == PenBrushDrawingOrder.BrushFirst)
            {
                if (fillBrush != null)
                {
                    e.Graphics.FillEllipse(GetGdiPlusBrushFromGeoBrush(fillBrush), screenX, screenY, width, height);
                }
                if (outlinePen != null)
                {
                    e.Graphics.DrawEllipse(new Pen(GetGdiPlusColorFromGeoColor(outlinePen.Color)), screenX, screenY,
                        width, height);
                }
            }
            else
            {
                if (outlinePen != null)
                {
                    e.Graphics.DrawEllipse(new Pen(GetGdiPlusColorFromGeoColor(outlinePen.Color)), screenX, screenY,
                        width, height);
                }
                if (fillBrush != null)
                {
                    e.Graphics.FillEllipse(GetGdiPlusBrushFromGeoBrush(fillBrush), screenX, screenY, width, height);
                }
            }
        }

        private void DrawScreenImageWithoutScalingCache(GeoImage image, float centerXInScreen, float centerYInScreen,
            float xOffset, float yOffset, float rotateAngle, PrintPageEventArgs e)
        {
            var bitmap = new Bitmap(image.GetImageStream(this));
            var imageWidth = ToPrinterDpiPixel(bitmap.Width);
            var imageHeight = ToPrinterDpiPixel(bitmap.Height);

            var screenX = ToPrinterDpiPixel(centerXInScreen) + _drawingArea.Location.X;
            var screenY = ToPrinterDpiPixel(centerYInScreen) + _drawingArea.Location.Y;

            screenX += -imageWidth/2 + xOffset;
            screenY += -imageHeight/2 + yOffset;

            if (rotateAngle == 0)
            {
                try
                {
                    bitmap.SetResolution(PrinterDpi, PrinterDpi);
                    e.Graphics.DrawImage(bitmap, screenX, screenY);
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

                    e.Graphics.TranslateTransform(screenX, screenY);
                    e.Graphics.RotateTransform(-rotateAngle);

                    bitmap.SetResolution(PrinterDpi, PrinterDpi);
                    e.Graphics.DrawImage(bitmap, 0, 0);

                    e.Graphics.RotateTransform(rotateAngle);
                    e.Graphics.TranslateTransform(-screenX, -screenY);
                }
                finally
                {
                    bitmap.Dispose();
                }
            }
        }

        private void DrawScreenImageCache(GeoImage image, float centerXInScreen, float centerYInScreen,
            float widthInScreen, float heightInScreen, float xOffset, float yOffset, float rotateAngle,
            PrintPageEventArgs e)
        {
            var bitmap = new Bitmap(image.GetImageStream(this));
            var imageWidth = widthInScreen;
            var imageHeight = heightInScreen;

            var screenX = ToPrinterDpiPixel(centerXInScreen) + _drawingArea.Location.X;
            var screenY = ToPrinterDpiPixel(centerYInScreen) + _drawingArea.Location.Y;

            screenX += -imageWidth/2 + xOffset;
            screenY += -imageHeight/2 + yOffset;

            if (rotateAngle == 0)
            {
                try
                {
                    bitmap.SetResolution(PrinterDpi, PrinterDpi);
                    e.Graphics.DrawImage(bitmap, screenX, screenY, imageWidth, imageHeight);
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

                    e.Graphics.TranslateTransform(screenX, screenY);
                    e.Graphics.RotateTransform(-rotateAngle);

                    bitmap.SetResolution(PrinterDpi, PrinterDpi);
                    e.Graphics.DrawImage(bitmap, 0, 0);

                    e.Graphics.RotateTransform(rotateAngle);
                    e.Graphics.TranslateTransform(-screenX, -screenY);
                }
                finally
                {
                    bitmap.Dispose();
                }
            }
        }

        private static HatchStyle GetGdiPlusHatchStyleFromGeoHatchStyle(GeoHatchStyle geoHatchStyle)
        {
            var gdiPlusHatchStyle = HatchStyle.BackwardDiagonal;

            switch (geoHatchStyle)
            {
                case GeoHatchStyle.Horizontal:
                    gdiPlusHatchStyle = HatchStyle.Horizontal;
                    break;
                case GeoHatchStyle.Vertical:
                    gdiPlusHatchStyle = HatchStyle.Vertical;
                    break;
                case GeoHatchStyle.ForwardDiagonal:
                    gdiPlusHatchStyle = HatchStyle.ForwardDiagonal;
                    break;
                case GeoHatchStyle.BackwardDiagonal:
                    gdiPlusHatchStyle = HatchStyle.BackwardDiagonal;
                    break;
                case GeoHatchStyle.LargeGrid:
                    gdiPlusHatchStyle = HatchStyle.LargeGrid;
                    break;
                case GeoHatchStyle.DiagonalCross:
                    gdiPlusHatchStyle = HatchStyle.DiagonalCross;
                    break;
                case GeoHatchStyle.Percent05:
                    gdiPlusHatchStyle = HatchStyle.Percent05;
                    break;
                case GeoHatchStyle.Percent10:
                    gdiPlusHatchStyle = HatchStyle.Percent10;
                    break;
                case GeoHatchStyle.Percent20:
                    gdiPlusHatchStyle = HatchStyle.Percent20;
                    break;
                case GeoHatchStyle.Percent25:
                    gdiPlusHatchStyle = HatchStyle.Percent25;
                    break;
                case GeoHatchStyle.Percent30:
                    gdiPlusHatchStyle = HatchStyle.Percent30;
                    break;
                case GeoHatchStyle.Percent40:
                    gdiPlusHatchStyle = HatchStyle.Percent40;
                    break;
                case GeoHatchStyle.Percent50:
                    gdiPlusHatchStyle = HatchStyle.Percent50;
                    break;
                case GeoHatchStyle.Percent60:
                    gdiPlusHatchStyle = HatchStyle.Percent60;
                    break;
                case GeoHatchStyle.Percent70:
                    gdiPlusHatchStyle = HatchStyle.Percent70;
                    break;
                case GeoHatchStyle.Percent75:
                    gdiPlusHatchStyle = HatchStyle.Percent75;
                    break;
                case GeoHatchStyle.Percent80:
                    gdiPlusHatchStyle = HatchStyle.Percent80;
                    break;
                case GeoHatchStyle.Percent90:
                    gdiPlusHatchStyle = HatchStyle.Percent90;
                    break;
                case GeoHatchStyle.LightDownwardDiagonal:
                    gdiPlusHatchStyle = HatchStyle.LightDownwardDiagonal;
                    break;
                case GeoHatchStyle.LightUpwardDiagonal:
                    gdiPlusHatchStyle = HatchStyle.LightUpwardDiagonal;
                    break;
                case GeoHatchStyle.DarkDownwardDiagonal:
                    gdiPlusHatchStyle = HatchStyle.DarkDownwardDiagonal;
                    break;
                case GeoHatchStyle.DarkUpwardDiagonal:
                    gdiPlusHatchStyle = HatchStyle.DarkUpwardDiagonal;
                    break;
                case GeoHatchStyle.WideDownwardDiagonal:
                    gdiPlusHatchStyle = HatchStyle.WideDownwardDiagonal;
                    break;
                case GeoHatchStyle.WideUpwardDiagonal:
                    gdiPlusHatchStyle = HatchStyle.WideUpwardDiagonal;
                    break;
                case GeoHatchStyle.LightVertical:
                    gdiPlusHatchStyle = HatchStyle.LightVertical;
                    break;
                case GeoHatchStyle.LightHorizontal:
                    gdiPlusHatchStyle = HatchStyle.LightHorizontal;
                    break;
                case GeoHatchStyle.NarrowVertical:
                    gdiPlusHatchStyle = HatchStyle.NarrowVertical;
                    break;
                case GeoHatchStyle.NarrowHorizontal:
                    gdiPlusHatchStyle = HatchStyle.NarrowHorizontal;
                    break;
                case GeoHatchStyle.DarkVertical:
                    gdiPlusHatchStyle = HatchStyle.DarkVertical;
                    break;
                case GeoHatchStyle.DarkHorizontal:
                    gdiPlusHatchStyle = HatchStyle.DarkHorizontal;
                    break;
                case GeoHatchStyle.DashedDownwardDiagonal:
                    gdiPlusHatchStyle = HatchStyle.DashedDownwardDiagonal;
                    break;
                case GeoHatchStyle.DashedUpwardDiagonal:
                    gdiPlusHatchStyle = HatchStyle.DashedUpwardDiagonal;
                    break;
                case GeoHatchStyle.DashedHorizontal:
                    gdiPlusHatchStyle = HatchStyle.DashedHorizontal;
                    break;
                case GeoHatchStyle.DashedVertical:
                    gdiPlusHatchStyle = HatchStyle.DashedVertical;
                    break;
                case GeoHatchStyle.SmallConfetti:
                    gdiPlusHatchStyle = HatchStyle.SmallConfetti;
                    break;
                case GeoHatchStyle.LargeConfetti:
                    gdiPlusHatchStyle = HatchStyle.LargeConfetti;
                    break;
                case GeoHatchStyle.ZigZag:
                    gdiPlusHatchStyle = HatchStyle.ZigZag;
                    break;
                case GeoHatchStyle.Wave:
                    gdiPlusHatchStyle = HatchStyle.Wave;
                    break;
                case GeoHatchStyle.DiagonalBrick:
                    gdiPlusHatchStyle = HatchStyle.DiagonalBrick;
                    break;
                case GeoHatchStyle.HorizontalBrick:
                    gdiPlusHatchStyle = HatchStyle.HorizontalBrick;
                    break;
                case GeoHatchStyle.Weave:
                    gdiPlusHatchStyle = HatchStyle.Weave;
                    break;
                case GeoHatchStyle.Plaid:
                    gdiPlusHatchStyle = HatchStyle.Plaid;
                    break;
                case GeoHatchStyle.Divot:
                    gdiPlusHatchStyle = HatchStyle.Divot;
                    break;
                case GeoHatchStyle.DottedGrid:
                    gdiPlusHatchStyle = HatchStyle.DottedGrid;
                    break;
                case GeoHatchStyle.DottedDiamond:
                    gdiPlusHatchStyle = HatchStyle.DottedDiamond;
                    break;
                case GeoHatchStyle.Shingle:
                    gdiPlusHatchStyle = HatchStyle.Shingle;
                    break;
                case GeoHatchStyle.Trellis:
                    gdiPlusHatchStyle = HatchStyle.Trellis;
                    break;
                case GeoHatchStyle.Sphere:
                    gdiPlusHatchStyle = HatchStyle.Sphere;
                    break;
                case GeoHatchStyle.SmallGrid:
                    gdiPlusHatchStyle = HatchStyle.SmallGrid;
                    break;
                case GeoHatchStyle.SmallCheckerBoard:
                    gdiPlusHatchStyle = HatchStyle.SmallCheckerBoard;
                    break;
                case GeoHatchStyle.LargeCheckerBoard:
                    gdiPlusHatchStyle = HatchStyle.LargeCheckerBoard;
                    break;
                case GeoHatchStyle.OutlinedDiamond:
                    gdiPlusHatchStyle = HatchStyle.OutlinedDiamond;
                    break;
                case GeoHatchStyle.SolidDiamond:
                    gdiPlusHatchStyle = HatchStyle.SolidDiamond;
                    break;
                case GeoHatchStyle.Min:
                    gdiPlusHatchStyle = HatchStyle.Min;
                    break;
                case GeoHatchStyle.Max:
                    gdiPlusHatchStyle = HatchStyle.Max;
                    break;
                case GeoHatchStyle.Cross:
                    gdiPlusHatchStyle = HatchStyle.Cross;
                    break;
                default:
                    break;
            }

            return gdiPlusHatchStyle;
        }

        private static Color GetGdiPlusColorFromGeoColor(GeoColor color)
        {
            return Color.FromArgb(color.AlphaComponent, color.RedComponent, color.GreenComponent, color.BlueComponent);
        }

        private Font GetGdiPlusFontFromGeoFont(GeoFont font)
        {
            if (font == null)
            {
                return null;
            }
            if (_fontCache == null)
            {
                _fontCache = new Dictionary<long, Font>();
            }

            if (_fontCache.ContainsKey(font.GetHashCode()))
            {
                return _fontCache[font.GetHashCode()];
            }

            var gdiPlusFontStyle = GetGdiPlusFontStyleFromDrawingFontStyle(font.Style);
            var resultFont = new Font(font.FontName, font.Size, gdiPlusFontStyle, GraphicsUnit.Point);
            _fontCache.Add(font.GetHashCode(), resultFont);

            return resultFont;
        }

        private static FontStyle GetGdiPlusFontStyleFromDrawingFontStyle(DrawingFontStyles style)
        {
            var returnFontStyle = FontStyle.Regular;
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
            Validators.CheckParameterIsNotNull(image, "image");

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
            parameters.Add("widthInScreen", widthInScreen);
            parameters.Add("heightInScreen", heightInScreen);
            parameters.Add("xOffset", xOffset);
            parameters.Add("yOffset", yOffset);
            parameters.Add("rotateAngle", rotateAngle);
            parameters.Add("method", "DrawScreenImageCache");

            _featuresByLevel[drawingLevel].Add(parameters);
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

            _isMessure = true;
        }

        /// <summary>Returns the rectangle containing a text.</summary>
        /// <returns>A rectangle containing a text taking into account the font.</returns>
        /// <remarks>Typically used for labeling logic to determine the overlapping of labels.</remarks>
        /// <param name="text">Text to measure.</param>
        /// <param name="font">Font of the text to measure.</param>
        protected override DrawingRectangleF MeasureTextCore(string text, GeoFont font)
        {
            _messureFont = font;

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

            return new DrawingRectangleF(size.Width/2, size.Height/2, size.Width, size.Height);
        }

        /// <summary>Converts an object to a GeoImage.</summary>
        /// <param name="nativeImage">Target object to convert.</param>
        /// <returns>GeoImage containing.</returns>
        protected override object ToNativeImageCore(GeoImage image)
        {
            throw new NotImplementedException();
        }

        /// <summary>Converts an object to a GeoImage.</summary>
        /// <param name="nativeImage">Target object to convert.</param>
        /// <returns>GeoImage containing.</returns>
        protected override GeoImage ToGeoImageCore(object nativeImage)
        {
            throw new NotImplementedException();
        }

        /// <summary>Gets the canvas width of a native image object.</summary>
        /// <param name="nativeImage">Native image used to get the canvas width.</param>
        /// <returns>Canvas width.</returns>
        protected override float GetCanvasWidthCore(object nativeImage)
        {
            _printDocument = (PrintDocument) nativeImage;
            _stopPrint = true;
            _printDocument.Print();

            float width = _printPageEventArgs.MarginBounds.Width;

            return width;
        }

        /// <summary>Gets the canvas height of a native image object.</summary>
        /// <param name="nativeImage">Native image will be used to get the canvas height.</param>
        /// <returns>Canvas height.</returns>
        protected override float GetCanvasHeightCore(object nativeImage)
        {
            _printDocument = (PrintDocument) nativeImage;
            _stopPrint = true;
            _printDocument.Print();
            float width = _printPageEventArgs.MarginBounds.Height;

            return width;
        }

        /// <summary>Returns stream representing the GeoImage in TIFF format.</summary>
        /// <returns>Stream that represents the GeoImage in TIFF format.</returns>
        /// <remarks>Allows to get the stream out of a GeoImage. The stream is a
        /// memory stream and the bytes are in TIFF format. Can be saved in the desired format.</remarks>
        /// <param name="image">GeoImage to convert to a stream.</param>
        public override Stream GetStreamFromGeoImage(GeoImage image)
        {
            Validators.CheckParameterIsNotNull(image, "image");

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

        /// <summary>Flushes drawing and commits the drawing on the canvas.</summary>
        protected override void FlushCore()
        {
        }

        private float ToPrinterDpiPixel(float pixel)
        {
            return pixel*Dpi/PrinterDpi;
        }
    }
}
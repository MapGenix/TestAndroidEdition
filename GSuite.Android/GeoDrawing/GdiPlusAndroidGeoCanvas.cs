using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Mapgenix.Canvas;
/*using NativeDrawing = System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;*/
using System.Collections.ObjectModel;
using Android.Graphics;
using NativeAndroid = Android;
using Mapgenix.GSuite.Android.Properties;
using Mapgenix.Shapes;
using System.IO;

namespace Mapgenix.GSuite.Android
{
    public class GdiPlusAndroidGeoCanvas : BaseGeoCanvas
    {
        private const float StandardDpi = 96f;

        private Dictionary<long, Color> _brushCache;
        private Bitmap _bufferImageForLabel;
        private Bitmap _bufferImageForLevel01;
        private Bitmap _bufferImageForLevel02;
        private Bitmap _bufferImageForLevel03;
        private Bitmap _bufferImageForLevel04;

        //private CompositingQuality _compositiongQuality = CompositingQuality.Default;
        private object _drawingImage;
        //private Dictionary<long, NativeDrawing.Font> _fontCache;
        private Guid _formatOfCanvasImage;
        private object _graphicsForLabel;

        private object _graphicsForLevel01;
        private object _graphicsForLevel02;
        private object _graphicsForLevel03;
        private object _graphicsForLevel04;

        private Bitmap _haloPenBitmap;

        //private InterpolationMode _interpolationMode;

        private bool _isBitmap;
        private int _localCanvasHeight;

        private int _localCanvasWidth;
        private Dictionary<long, Paint> _penCache;
        //private SmoothingMode _smoothingMode = SmoothingMode.HighQuality;
        //private TextRenderingHint _textRenderingHint = TextRenderingHint.AntiAlias;

        /// <summary>Gets or sets the quality level of composing.</summary>
        /// <remarks>
        /// 	<para>Used for GDI+ drawing.<br/>
        ///     <br/>
        ///     Compositing is done during rendering when the source pixels are
        ///     combined with the destination pixels to produce the resultant pixels. The quality
        ///     of compositing directly relates to the visual quality of the output and is
        ///     inversely proportional to the render time. The higher the quality, the slower the
        ///     render time. This is because the higher the quality level, the more surrounding
        ///     pixels need to be taken into account during the composite. The linear quality
        ///     setting (AssumeLinear) compromises by providing better quality than the default
        ///     quality at a slightly lower speed.</para>
        /// </remarks>

        /*public CompositingQuality CompositingQuality
        {
            get { return _compositiongQuality; }
            set { _compositiongQuality = value; }
        }*/

       /// <summary>Gets or sets the rendering quality.</summary>
       /// <remarks>
       /// 	<para>Used for GDI+ drawing.<br/>
       /// 		<br/>
       /// 		<br/>
       ///     The smoothing mode specifies whether lines, curves, and the edges of filled areas
       ///     use smoothing (also called antialiasing). One exception is that path gradient
       ///     brushes do not obey the smoothing mode. Areas filled using a PathGradientBrush are
       ///     rendered the same way (aliased) regardless of the SmoothingMode property.</para>
       /// </remarks>
       /*public SmoothingMode SmoothingMode
        {
            get { return _smoothingMode; }
            set { _smoothingMode = value; }
        }*/

        /// <summary>Indicates if GdiPlusGeoCanvas has the KeyColor or not. </summary>
        /// <remarks>The default value is true.</remarks>
        public override bool HasKeyColor
        {
            get { return true; }
        }

       
        /// <summary>Gets or sets the rendering mode of the text in this canvas.</summary>
        /// <remarks>
        /// 	<para>Used for GDI+ drawing.<br/>
        /// 		<br/>
        ///     The text rendering hint specifies whether text renders with antialiasing.</para>
        /// </remarks>
        /// <value>Rendering mode of the text in this canvas.</value>
        /*public TextRenderingHint TextRenderingHint
        {
            get { return _textRenderingHint; }
            set { _textRenderingHint = value; }
        }*/

        /// <summary>Gets or sets the interpolation mode.</summary>
        /// <value>System.Drawing.Drawing2D.InterpolationMode.</value>
        /*public InterpolationMode InterpolationMode
        {
            get { return _interpolationMode; }
            set { _interpolationMode = value; }
        }*/

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
            Validators.CheckParameterIsNotNull(screenPoints, "screenPoints");
            //Validators.CheckParameterIsNotBothNull(outlinePen, fillBrush, "outlinePen", "fillBrush");
            //Validators.CheckDrawingLevelIsValid(drawingLevel, "drawingLevel");
            Validators.CheckGeoCanvasIsInDrawing(IsDrawing);

            foreach (var screenPointsArray in screenPoints)
            {
                for (var i = 0; i < screenPointsArray.Length; i++)
                {
                    var screenPointX = screenPointsArray[i].X;
                    var screenPointY = screenPointsArray[i].Y;

                    if (screenPointX < -1E+8f)
                    {
                        screenPointX = -1E+8f;
                    }

                    if (screenPointX > 1E+8f)
                    {
                        screenPointX = 1E+8f;
                    }

                    if (screenPointY < -1E+8f)
                    {
                        screenPointY = -1E+8f;
                    }

                    if (screenPointY > 1E+8f)
                    {
                        screenPointY = 1E+8f;
                    }

                    screenPointsArray[i] = new ScreenPointF(screenPointX, screenPointY);
                }
            }

            /*if (DrawingQuality == DrawingQuality.HighSpeed ||
                (DrawingQuality == DrawingQuality.CanvasSettings && SmoothingMode == SmoothingMode.HighSpeed))*/
            if (DrawingQuality == DrawingQuality.HighSpeed ||
                (DrawingQuality == DrawingQuality.CanvasSettings))
            {
                DrawAreaForHighSpeed(screenPoints, outlinePen, fillBrush, drawingLevel, xOffset, yOffset,
                    penBrushDrawingOrder);
            }
            else
            {
                DrawAreaForNotHighSpeed(screenPoints, outlinePen, fillBrush, drawingLevel, xOffset, yOffset,
                    penBrushDrawingOrder);
            }
        }

        private void DrawAreaForHighSpeed(IEnumerable<ScreenPointF[]> screenPoints, GeoPen outlinePen,
            BaseGeoBrush fillBrush, DrawingLevel drawingLevel, float xOffset, float yOffset,
            PenBrushDrawingOrder penBrushDrawingOrder)
        {
            var ringsCollection = new List<Point[]>();
            var cachePoints = new Collection<ScreenPointF>();

            foreach (var rings in screenPoints)
            {
                foreach (var pointF in rings)
                {
                    cachePoints.Add(pointF);
                }

                var points = new Point[rings.Length];
                var previousX = int.MaxValue;
                var previousY = int.MaxValue;
                var tempCount = 0;
                foreach (var point in rings)
                {
                    var screenX = (int) (point.X + xOffset);
                    var screenY = (int) (point.Y + yOffset);
                    if (previousX != screenX || previousY != screenY)
                    {
                        previousX = screenX;
                        previousY = screenY;
                        points[tempCount] = new Point(screenX, screenY);
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
                var graphics = SelectImageGraphicsByDrawingLevel(drawingLevel);

                if (penBrushDrawingOrder == PenBrushDrawingOrder.BrushFirst)
                {
                    if (fillBrush != null)
                    {
                        graphics.FillPolygon(GetGdiPlusBrushFromGeoBrush(fillBrush, cachePoints), ringsCollection[0]);
                    }
                    if (outlinePen != null)
                    {
                        graphics.DrawPolygon(GetGdiPlusPenFromGeoPen(outlinePen), ringsCollection[0]);
                    }
                }
                else
                {
                    if (outlinePen != null)
                    {
                        graphics.DrawPolygon(GetGdiPlusPenFromGeoPen(outlinePen), ringsCollection[0]);
                    }
                    if (fillBrush != null)
                    {
                        graphics.FillPolygon(GetGdiPlusBrushFromGeoBrush(fillBrush, cachePoints), ringsCollection[0]);
                    }
                }
            }
            else if (count != 0)
            {
                GraphicsPath graphicsPath = null;

                try
                {
                    graphicsPath = new GraphicsPath();
                    graphicsPath.FillMode = NativeAndroid.Graphics.Path.FillType.Winding;
                    foreach (var points in ringsCollection)
                    {
                        graphicsPath.AddPolygon(points);
                    }

                    var graphics = SelectImageGraphicsByDrawingLevel(drawingLevel);

                    if (penBrushDrawingOrder == PenBrushDrawingOrder.BrushFirst)
                    {
                        if (fillBrush != null)
                        {
                            graphics.FillPath(GetGdiPlusBrushFromGeoBrush(fillBrush, cachePoints), graphicsPath);
                        }
                        if (outlinePen != null)
                        {
                            graphics.DrawPath(GetGdiPlusPenFromGeoPen(outlinePen), graphicsPath);
                        }
                    }
                    else
                    {
                        if (outlinePen != null)
                        {
                            graphics.DrawPath(GetGdiPlusPenFromGeoPen(outlinePen), graphicsPath);
                        }
                        if (fillBrush != null)
                        {
                            graphics.FillPath(GetGdiPlusBrushFromGeoBrush(fillBrush, cachePoints), graphicsPath);
                        }
                    }
                }
                finally
                {
                    if (graphicsPath != null)
                    {
                        graphicsPath.Dispose();
                    }
                }
            }
        }

        private void DrawAreaForNotHighSpeed(IEnumerable<ScreenPointF[]> screenPoints, GeoPen outlinePen,
            BaseGeoBrush fillBrush, DrawingLevel drawingLevel, float xOffset, float yOffset,
            PenBrushDrawingOrder penBrushDrawingOrder)
        {
            var ringsCollection = new List<PointF[]>();
            var cachePoints = new Collection<ScreenPointF>();

            foreach (var rings in screenPoints)
            {
                foreach (var pointF in rings)
                {
                    cachePoints.Add(pointF);
                }

                var points = new PointF[rings.Length];
                var previousX = int.MaxValue;
                var previousY = int.MaxValue;
                var tempCount = 0;
                for (var i = 0; i < rings.Length; i++)
                {
                    var point = rings[i];
                    var screenX = point.X + xOffset;
                    var screenY = point.Y + yOffset;
                    if (previousX != (int) screenX || previousY != (int) screenY)
                    {
                        previousX = (int) screenX;
                        previousY = (int) screenY;
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
                var graphics = SelectImageGraphicsByDrawingLevel(drawingLevel);

                if (penBrushDrawingOrder == PenBrushDrawingOrder.BrushFirst)
                {
                    if (fillBrush != null)
                    {
                        graphics.FillPolygon(GetGdiPlusBrushFromGeoBrush(fillBrush, cachePoints), ringsCollection[0]);
                    }
                    if (outlinePen != null)
                    {
                        graphics.DrawPolygon(GetGdiPlusPenFromGeoPen(outlinePen), ringsCollection[0]);
                    }
                }
                else
                {
                    if (outlinePen != null)
                    {
                        graphics.DrawPolygon(GetGdiPlusPenFromGeoPen(outlinePen), ringsCollection[0]);
                    }
                    if (fillBrush != null)
                    {
                        graphics.FillPolygon(GetGdiPlusBrushFromGeoBrush(fillBrush, cachePoints), ringsCollection[0]);
                    }
                }
            }
            else if (count != 0)
            {
                GraphicsPath graphicsPath = null;

                try
                {
                    graphicsPath = new GraphicsPath();
                    //graphicsPath.FillMode = FillMode.Winding;
                    foreach (var points in ringsCollection)
                    {
                        graphicsPath.AddPolygon(points);
                    }

                    var graphics = SelectImageGraphicsByDrawingLevel(drawingLevel);

                    if (penBrushDrawingOrder == PenBrushDrawingOrder.BrushFirst)
                    {
                        if (fillBrush != null)
                        {
                            graphics.FillPath(GetGdiPlusBrushFromGeoBrush(fillBrush, cachePoints), graphicsPath);
                        }
                        if (outlinePen != null)
                        {
                            graphics.DrawPath(GetGdiPlusPenFromGeoPen(outlinePen), graphicsPath);
                        }
                    }
                    else
                    {
                        if (outlinePen != null)
                        {
                            graphics.DrawPath(GetGdiPlusPenFromGeoPen(outlinePen), graphicsPath);
                        }
                        if (fillBrush != null)
                        {
                            graphics.FillPath(GetGdiPlusBrushFromGeoBrush(fillBrush, cachePoints), graphicsPath);
                        }
                    }
                }
                finally
                {
                    if (graphicsPath != null)
                    {
                        graphicsPath.Dispose();
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
            Validators.CheckParameterIsNotNull(screenPoints, "screenPoints");
            Validators.CheckParameterIsNotNull(linePen, "outlinePen");
            //Validators.CheckDrawingLevelIsValid(drawingLevel, "drawingLevel");
            Validators.CheckGeoCanvasIsInDrawing(IsDrawing);

            var points = new List<PointF>();

            var previousX = int.MaxValue;
            var previousY = int.MaxValue;

            foreach (var point in screenPoints)
            {
                var x = (point.X + xOffset);
                var y = (point.Y + yOffset);
                var screenX = (int) x;
                var screenY = (int) y;

                if (screenX < -1E+8)
                {
                    screenX = (int) -1E+8;
                }

                if (screenX > 1E+8)
                {
                    screenX = (int) 1E+8;
                }

                if (screenY < -1E+8f)
                {
                    screenY = (int) -1E+8;
                }

                if (screenY > 1E+8f)
                {
                    screenY = (int) 1E+8f;
                }

                if (previousX != screenX || previousY != screenY)
                {
                    previousX = screenX;
                    previousY = screenY;
                    points.Add(new PointF(x, y));
                }
            }

            if (points.Count == 1)
            {
                points.Add(new PointF(points[0].X + 0.1f, points[0].Y));
            }

            if (points.Count > 1)
            {
                var graphics = SelectImageGraphicsByDrawingLevel(drawingLevel);
                graphics.DrawLines(GetGdiPlusPenFromGeoPen(linePen), points.ToArray());
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
        protected override void DrawEllipseCore(ScreenPointF screenPoint, float width, float height, GeoPen outlinePen,
            BaseGeoBrush fillBrush, DrawingLevel drawingLevel, float xOffset, float yOffset,
            PenBrushDrawingOrder penBrushDrawingOrder)
        {
            //Validators.CheckParameterIsNotBothNull(outlinePen, fillBrush, "outlinePen", "fillBrush");
            //Validators.CheckDrawingLevelIsValid(drawingLevel, "drawingLevel");
            Validators.CheckGeoCanvasIsInDrawing(IsDrawing);

            var screenX = screenPoint.X - width*0.5f + xOffset;
            var screenY = screenPoint.Y - height*0.5f + yOffset;

            var g = SelectImageGraphicsByDrawingLevel(drawingLevel);

            if (penBrushDrawingOrder == PenBrushDrawingOrder.BrushFirst)
            {
                if (fillBrush != null)
                {
                    g.FillEllipse(GetGdiPlusBrushFromGeoBrush(fillBrush, new[] {screenPoint}), screenX, screenY, width,
                        height);
                }
                if (outlinePen != null)
                {
                    g.DrawEllipse(GetGdiPlusPenFromGeoPen(outlinePen), screenX, screenY, width, height);
                }
            }
            else
            {
                if (outlinePen != null)
                {
                    g.DrawEllipse(GetGdiPlusPenFromGeoPen(outlinePen), screenX, screenY, width, height);
                }
                if (fillBrush != null)
                {
                    g.FillEllipse(GetGdiPlusBrushFromGeoBrush(fillBrush, new[] {screenPoint}), screenX, screenY, width,
                        height);
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
            Validators.CheckGeoCanvasIsInDrawing(IsDrawing);
            Validators.CheckParameterIsNotNull(image, "image");
            //Validators.CheckDrawingLevelIsValid(drawingLevel, "drawingLevel");


            Bitmap bitmap;
            lock (image)
            {
                //bitmap = new System.Drawing.Bitmap(image.GetImageStream(this));
                //Bitmap.CreateBitmap()
                bitmap = BitmapFactory.DecodeStream(image.GetImageStream(this));
            }

            //bitmap.SetResolution(Dpi, Dpi);
            UseKeyColor(bitmap);
            var imageWidth = bitmap.Width;
            var imageHeight = bitmap.Height;

            var screenX = centerXInScreen;
            var screenY = centerYInScreen;

            screenX += -imageWidth/2.0f + xOffset;
            screenY += -imageHeight/2.0f + yOffset;

            if (rotateAngle == 0)
            {
                try
                {
                    var graphics = SelectImageGraphicsByDrawingLevel(drawingLevel);
                    graphics.DrawImageUnscaled(bitmap, (int) Math.Round(screenX), (int) Math.Round(screenY));
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
                var baseRadient = Math.PI - Math.Atan(imageHeight/(double) imageWidth);
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
                    var graphics = SelectImageGraphicsByDrawingLevel(drawingLevel);

                    graphics.TranslateTransform(screenX, screenY);
                    graphics.RotateTransform(-rotateAngle);

                    graphics.DrawImageUnscaled(bitmap, 0, 0);

                    graphics.RotateTransform(rotateAngle);
                    graphics.TranslateTransform(-screenX, -screenY);
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
            Validators.CheckParameterIsNotNull(image, "image");
            //Validators.CheckDrawingLevelIsValid(drawingLevel, "drawingLevel");
            //Validators.CheckIfInputValueIsBiggerThan(widthInScreen, "widthInScreen", 0,RangeCheckingInclusion.ExcludeValue);
            //Validators.CheckIfInputValueIsBiggerThan(heightInScreen, "heightInScreen", 0, RangeCheckingInclusion.ExcludeValue);


            var screenX = centerXInScreen;
            var screenY = centerYInScreen;

            screenX += -widthInScreen/2 + xOffset;
            screenY += -heightInScreen/2 + yOffset;

            if (rotateAngle == 0)
            {
                var destinateRectangle = new RectF(screenX, screenY, widthInScreen, heightInScreen);
                Bitmap bitmap = null;

                try
                {
                    lock (image)
                    {
                        //bitmap = new Bitmap(image.GetImageStream(this));
                        bitmap = BitmapFactory.DecodeStream(image.GetImageStream(this));
                    }
                    //bitmap.SetResolution(Dpi, Dpi);
                    UseKeyColor(bitmap);
                    var sourceRectangle = new RectF(0, 0, bitmap.Width, bitmap.Height);
                    var graphics = SelectImageGraphicsByDrawingLevel(drawingLevel);
                    //graphics.DrawImage(bitmap, destinateRectangle, sourceRectangle, GraphicsUnit.Pixel);
                    graphics.DrawImage(bitmap, destinateRectangle, sourceRectangle);
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

                var destinateRectangle = new RectF(0, 0, widthInScreen, heightInScreen);

                Bitmap bitmap = null;
                try
                {
                    lock (image)
                    {
                        //bitmap = new Bitmap(image.GetImageStream(this));
                        bitmap = BitmapFactory.DecodeStream(image.GetImageStream(this));
                    }
                    //bitmap.SetResolution(Dpi, Dpi);
                    UseKeyColor(bitmap);
                    var sourceRectangle = new RectF(0, 0, bitmap.Width, bitmap.Height);

                    var graphics = SelectImageGraphicsByDrawingLevel(drawingLevel);

                    graphics.TranslateTransform(screenX, screenY);
                    graphics.RotateTransform(-rotateAngle);

                    //graphics.DrawImage(bitmap, destinateRectangle, sourceRectangle, GraphicsUnit.Pixel);
                    graphics.DrawImage(bitmap, destinateRectangle, sourceRectangle);

                    graphics.RotateTransform(rotateAngle);
                    graphics.TranslateTransform(-screenX, -screenY);
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
            /*Validators.CheckParameterIsNotNull(fillBrush, "fillBrush");
            Validators.CheckParameterIsNotNull(font, "font");
            Validators.CheckParameterIsNotNull(text, "text");
            Validators.CheckDrawingLevelIsValid(drawingLevel, "drawingLevel");
            Validators.CheckParameterIsNotNullOrEmpty(text, "text");
            Validators.CheckGeoCanvasIsInDrawing(IsDrawing);
            Validators.CheckParameterIsNotNull(textPathInScreen, "textPathInScreen");

            var gdiFont = GetGdiPlusFontFromGeoFont(font);

            var g = SelectImageGraphicsByDrawingLevel(drawingLevel);
            var mode = g.CompositingMode;
            g.CompositingMode = CompositingMode.SourceOver;

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
                    var dpixOffset = xOffset*(Dpi/StandardDpi);
                    var dpiyOffset = yOffset*(Dpi/StandardDpi);

                    DrawString(g, text, gdiFont, fillBrush, haloPen,
                        new PointF(pointForLabel.X + dpixOffset, pointForLabel.Y + dpiyOffset), width, height);
                }
                else
                {
                    var newPointX = (float) (pointForLabel.X + (xOffset*(Dpi/StandardDpi)) + xOffsetRotated);
                    var newPointY = (float) (pointForLabel.Y + (yOffset*(Dpi/StandardDpi)) + yOffsetRotated);

                    g.TranslateTransform(newPointX, newPointY);
                    g.RotateTransform(-rotateAngle);

                    DrawString(g, text, gdiFont, fillBrush, haloPen, new PointF(0, 0), width, height);

                    g.RotateTransform(rotateAngle);
                    g.TranslateTransform(-newPointX, -newPointY);
                }
            }

            g.CompositingMode = mode;*/
        }

        /// <summary>Returns the rectangle containing a text.</summary>
        /// <returns>A rectangle containing a text taking into account the font.</returns>
        /// <remarks>Typically used for labeling logic to determine the overlapping of labels.</remarks>
        /// <param name="text">Text to measure.</param>
        /// <param name="font">Font of the text to measure.</param>
        protected override DrawingRectangleF MeasureTextCore(string text, GeoFont font)
        {
            /*Validators.CheckParameterIsNotNull(text, "text");

            Bitmap bitmap = null;
            Graphics graphics = null;

            SizeF size;

            try
            {
                bitmap = new Bitmap(1, 1);
                if (_bufferImageForLevel01 != null)
                {
                    bitmap.SetResolution(Dpi, Dpi);
                }
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

            return new DrawingRectangleF(size.Width/2, size.Height/2, size.Width, size.Height);*/
            return new DrawingRectangleF();
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

        /// <summary>Begins the drawing on the canvas.</summary>
        /// <remarks>
        /// First method to call before doing any drawing on the canvas.
        /// Sets IsDrawing property to true. After finishing drawing,
        /// call EndDrawing to commit the changes to the image.
        /// </remarks>
        /// <param name="nativeImage">Image the canvas draws on.</param>
        /// <param name="worldExtent">World extent of the canvas.</param>
        /// <param name="drawingMapUnit">Map unit of the canvas.</param>
        private void BeginDrawingCore(object nativeImage, RectangleShape worldExtent,
            GeographyUnit drawingMapUnit)
        {
            Validators.CheckParameterIsNotNull(nativeImage, "nativeImage");
            Validators.CheckParameterIsNotNull(worldExtent, "worldExtent");
            Validators.CheckMapEngineExtentIsValid(worldExtent, "worldExtent");

            _drawingImage = nativeImage;
            CurrentWorldExtent = worldExtent;

            var tempBitmap = nativeImage as Bitmap;

            if (tempBitmap != null)
            {
                _isBitmap = true;
                _bufferImageForLevel01 = tempBitmap;
                _graphicsForLevel01 = Graphics.FromImage(_bufferImageForLevel01);
                Dpi = _bufferImageForLevel01.Width;
                SetGraphicsMode((Graphics) _graphicsForLevel01);
            }
            else
            {
                _isBitmap = false;
                var tempGeoImage = (GeoImage) nativeImage;

                lock (tempGeoImage)
                {
                    tempBitmap = BitmapFactory.DecodeStream(tempGeoImage.GetImageStream(this));
                }
                _bufferImageForLevel01 = tempBitmap;
                _formatOfCanvasImage = tempGeoImage.CanvasImageFormat;
            }

            _localCanvasWidth = tempBitmap.Width;
            _localCanvasHeight = tempBitmap.Height;
            Width = _localCanvasWidth;
            Height = _localCanvasHeight;
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
            if (_graphicsForLevel01 == null)
            {
                return;
            }

            Bitmap bitmap;
            Graphics graphics;

            try
            {
                bitmap = _bufferImageForLevel01;
                graphics = (Graphics) _graphicsForLevel01;

                if (_bufferImageForLevel02 != null)
                {
                    graphics.DrawImage(_bufferImageForLevel02, 0, 0);
                }
                if (_bufferImageForLevel03 != null)
                {
                    graphics.DrawImage(_bufferImageForLevel03, 0, 0);
                }
                if (_bufferImageForLevel04 != null)
                {
                    graphics.DrawImage(_bufferImageForLevel04, 0, 0);
                }
                if (_bufferImageForLabel != null)
                {
                    graphics.DrawImage(_bufferImageForLabel, 0, 0);
                }

                if (!_isBitmap)
                {
                    bitmap.Compress(Bitmap.CompressFormat.Png, 0,((GeoImage) _drawingImage).GetImageStream(this));
                    if (!string.IsNullOrEmpty(((GeoImage) _drawingImage).PathFilename))
                    {
                        //bitmap.Compress(Bitmap.CompressFormat.Png, 0,((GeoImage) _drawingImage).PathFilename, new ImageFormat(_formatOfCanvasImage));
                    }
                }
            }
            finally
            {
                if (_graphicsForLevel01 != null)
                {
                    var g1 = (Graphics) _graphicsForLevel01;
                    g1.Dispose();
                    _graphicsForLevel01 = null;
                }
                if (_graphicsForLevel02 != null)
                {
                    var g2 = (Graphics) _graphicsForLevel02;
                    g2.Dispose();
                    _graphicsForLevel02 = null;
                }
                if (_graphicsForLevel03 != null)
                {
                    var g3 = (Graphics) _graphicsForLevel03;
                    g3.Dispose();
                    _graphicsForLevel03 = null;
                }
                if (_graphicsForLevel04 != null)
                {
                    var g4 = (Graphics) _graphicsForLevel04;
                    g4.Dispose();
                    _graphicsForLevel04 = null;
                }
                if (_graphicsForLabel != null)
                {
                    var g5 = (Graphics) _graphicsForLabel;
                    g5.Dispose();
                    _graphicsForLabel = null;
                }

                if (!_isBitmap && _bufferImageForLevel01 != null)
                {
                    _bufferImageForLevel01.Dispose();
                    _bufferImageForLevel01 = null;
                }
                if (_bufferImageForLevel02 != null)
                {
                    _bufferImageForLevel02.Dispose();
                    _bufferImageForLevel02 = null;
                }
                if (_bufferImageForLevel03 != null)
                {
                    _bufferImageForLevel03.Dispose();
                    _bufferImageForLevel03 = null;
                }
                if (_bufferImageForLevel04 != null)
                {
                    _bufferImageForLevel04.Dispose();
                    _bufferImageForLevel04 = null;
                }
                if (_bufferImageForLabel != null)
                {
                    _bufferImageForLabel.Dispose();
                    _bufferImageForLabel = null;
                }

                if (_haloPenBitmap != null)
                {
                    _haloPenBitmap.Dispose();
                    _haloPenBitmap = null;
                }
                ClearCache();
            }

            if (_bufferImageForLevel01 != null && KeyColor.AlphaComponent != 0)
            {
                //_bufferImageForLevel01.MakeTransparent(Color.Transparent);
            }
        }

        /// <summary>Flushes drawing and commits the drawing on the canvas.</summary>
        /// <remarks>Method to call when drawing finished. It commits the
        /// image changes to the image passed in on BeginDrawing. Also it sets IsDrawing to false. Finally it sets GeoCanvas to invalid state, 
        /// not allowing further drawing.</remarks>
        protected override void FlushCore()
        {
            if (_graphicsForLevel01 == null)
            {
                return;
            }

            Bitmap bitmap;
            Graphics graphics;

            try
            {
                bitmap = _bufferImageForLevel01;
                graphics = (Graphics) _graphicsForLevel01;

                if (_bufferImageForLevel02 != null)
                {
                    graphics.DrawImageUnscaled(_bufferImageForLevel02, 0, 0);
                }
                if (_bufferImageForLevel03 != null)
                {
                    graphics.DrawImageUnscaled(_bufferImageForLevel03, 0, 0);
                }
                if (_bufferImageForLevel04 != null)
                {
                    graphics.DrawImageUnscaled(_bufferImageForLevel04, 0, 0);
                }

                if (!_isBitmap)
                {
                    //bitmap.Save(((GeoImage) _drawingImage).GetImageStream(this), ImageFormat.Png);
                    bitmap.Compress(Bitmap.CompressFormat.Png, 0, ((GeoImage)_drawingImage).GetImageStream(this));
                    if (!string.IsNullOrEmpty(((GeoImage) _drawingImage).PathFilename))
                    {
                        //bitmap.Save(((GeoImage) _drawingImage).PathFilename, new ImageFormat(_formatOfCanvasImage));
                    }
                }
            }
            finally
            {
                if (_graphicsForLevel02 != null)
                {
                    var g2 = (Graphics) _graphicsForLevel02;
                    g2.Dispose();
                    _graphicsForLevel02 = null;
                }
                if (_graphicsForLevel03 != null)
                {
                    var g3 = (Graphics) _graphicsForLevel03;
                    g3.Dispose();
                    _graphicsForLevel03 = null;
                }
                if (_graphicsForLevel04 != null)
                {
                    var g4 = (Graphics) _graphicsForLevel04;
                    g4.Dispose();
                    _graphicsForLevel04 = null;
                }

                if (_bufferImageForLevel02 != null)
                {
                    _bufferImageForLevel02.Dispose();
                    _bufferImageForLevel02 = null;
                }
                if (_bufferImageForLevel03 != null)
                {
                    _bufferImageForLevel03.Dispose();
                    _bufferImageForLevel03 = null;
                }
                if (_bufferImageForLevel04 != null)
                {
                    _bufferImageForLevel04.Dispose();
                    _bufferImageForLevel04 = null;
                }
                if (_haloPenBitmap != null)
                {
                    _haloPenBitmap.Dispose();
                    _haloPenBitmap = null;
                }
                ClearCache();
            }
        }

        /// <summary>Converts a GeoImage to a commonly-used object. For example, in GdiPlus the object is a Bitmap.</summary>
        /// <param name="image">Target geoImage to convert.</param>
        /// <returns>Object</returns>
        protected override object ToNativeImageCore(GeoImage image)
        {
            Validators.CheckParameterIsNotNull(image, "image");

            Bitmap bitmap;
            lock (image)
            {
                var imageStream = image.GetImageStream(this);
                bitmap = BitmapFactory.DecodeStream(imageStream);
            }
            return bitmap;
        }

        /// <summary>Converts an object to a GeoImage. For example, in GdiPlus  object is a Bitmap.</summary>
        /// <param name="nativeImage">Target object to convert.</param>
        /// <returns>GeoImage containing.</returns>
        protected override GeoImage ToGeoImageCore(object nativeImage)
        {
            /*var image = nativeImage as Image;
            if (image != null)
            {
                var stream = new MemoryStream();
                image.Save(stream, ImageFormat.Png);
                stream.Seek(0, SeekOrigin.Begin);
                return new GeoImage(stream);
            }

            throw new ArgumentException(ExceptionDescription.ParameterIsNull, "nativeImage");*/
            return new GeoImage();
        }

        /// <summary>Returns stream representing the GeoImage in TIFF format.</summary>
        /// <returns>Stream that represents the GeoImage in TIFF format.</returns>
        /// <remarks>Allows to get the stream out of a GeoImage. The stream is a
        /// memory stream and the bytes are in TIFF format. Can be saved in the desired format.</remarks>
        /// <param name="image">GeoImage to convert to a stream.</param>
        public override Stream GetStreamFromGeoImage(GeoImage image)
        {
            Validators.CheckParameterIsNotNull(image, "image");

            if (string.IsNullOrEmpty(image.PathFilename) || !File.Exists(image.PathFilename))
            {
                return null;
            }

            MemoryStream imageStream = new MemoryStream();
            Bitmap bitmap = null;

            try
            {
                /*imageStream = new MemoryStream();
                bitmap = new Bitmap(image.PathFilename);

                image.CanvasImageFormat = bitmap.RawFormat.Guid;
                bitmap.Save(imageStream, ImageFormat.Png);
                imageStream.Seek(0, SeekOrigin.Begin);*/
            }
            finally
            {
                /*if (bitmap != null)
                {
                    bitmap.Dispose();
                }*/
            }

            return imageStream;
        }

        /// <summary>Fills the GeoImage with GeoBrush.</summary>
        /// <returns>None</returns>
        /// <remarks>Allows to fill a GeoImage with a GeoBrush. Useful for setting backgrounds.</remarks>
        /// <param name="image">GeoImage to set the background on.</param>
        /// <param name="brush">GeoBrush to fill the background with.</param>
        public static void FillBackground(GeoImage image, BaseGeoBrush brush)
        {
            Validators.CheckParameterIsNotNull(image, "image");

            var canvas = new GdiPlusAndroidGeoCanvas();
            Stream imageStream;
            Bitmap bitmap = null;
            Graphics g = null;
            Color gdiBrush = Color.Transparent;
            try
            {
                lock (image)
                {
                    imageStream = image.GetImageStream(canvas);
                    bitmap = BitmapFactory.DecodeStream(imageStream);
                }

                g = Graphics.FromImage(bitmap);
                gdiBrush = canvas.GetGdiPlusBrushFromGeoBrush(brush, null);
                g.FillRectangle(gdiBrush, 0, 0, bitmap.Width, bitmap.Height);
                imageStream.Seek(0, SeekOrigin.Begin);
                //bitmap.Save(imageStream, bitmap.RawFormat);
                bitmap.Compress(Bitmap.CompressFormat.Png, 0, imageStream);
            }
            finally
            {
                if (g != null)
                {
                    g.Dispose();
                }
                if (bitmap != null)
                {
                    bitmap.Dispose();
                }
                if (gdiBrush != null)
                {
                   // gdiBrush.Dispose();
                }
            }
        }

        /// <summary>Converts GeoImage to memory stream.</summary>
        /// <returns>Memory stream with the bytes formatted.</returns>
        /// <param name="image">GeoImage to convert to a memory stream.</param>
        public static MemoryStream ConvertGeoImageToMemoryStream(GeoImage image)
        {
            Validators.CheckParameterIsNotNull(image, "image");

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
            Validators.CheckParameterIsNotNull(image, "image");

            var canvas = new GdiPlusGeoCanvas();
            var memoryStream = new MemoryStream();
            Stream stream;
            Bitmap bitmap = null;

            try
            {
                lock (image)
                {
                    stream = image.GetImageStream(canvas);
                    bitmap = BitmapFactory.DecodeStream(stream);
                }

                bitmap.Compress(Bitmap.CompressFormat.Png, 0, memoryStream);      //Save(memoryStream, imageFormat);
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
                bitmap = Bitmap.CreateBitmap(width, height, Bitmap.Config.Argb8888);
                var imageStream = new MemoryStream();
                //bitmap.Save(imageStream, ImageFormat.Png);
                bitmap.Compress(Bitmap.CompressFormat.Png, 0, imageStream);
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

        /*private void DrawString(Graphics graphics, string text, Font font, BaseGeoBrush fillBrush, GeoPen haloPen,
            PointF position, double width, double height)
        {
            var brush = GetGdiPlusBrushFromGeoBrush(fillBrush,
                new[]
                {
                    new ScreenPointF(position.X, position.Y),
                    new ScreenPointF(position.X + (float) width, position.Y + (float) height)
                });
            var pen = GetGdiPlusPenFromGeoPen(haloPen);

            if (haloPen == null || haloPen.Brush == null || haloPen.Color.AlphaComponent == 0)
            {
                graphics.DrawString(text, font, brush, position, StringFormat.GenericTypographic);
            }
            else
            {
                var tempSmoothingMode = graphics.SmoothingMode;
                GraphicsPath path = null;
                float xOffset = 0;
                float yOffset = 0;

                try
                {
                    path = new GraphicsPath();
                    var fontSize = font.Size*Dpi/StandardDpi;
                    path.AddString(text, font.FontFamily, (int) font.Style, fontSize*1.3f, position,
                        StringFormat.GenericTypographic);

                    var bound = path.GetBounds();
                    var boundWidth = bound.Width;
                    var boundHeight = bound.Height;

                    xOffset = (float) ((width - boundWidth)*0.5);
                    yOffset = (float) ((height - boundHeight)*0.3);

                    graphics.SmoothingMode = SmoothingMode.AntiAlias;

                    graphics.TranslateTransform(xOffset, yOffset);

                    graphics.DrawPath(pen, path);
                    graphics.FillPath(brush, path);
                }
                finally
                {
                    if (path != null)
                    {
                        path.Dispose();
                    }

                    graphics.SmoothingMode = tempSmoothingMode;
                    graphics.TranslateTransform(-xOffset, -yOffset);
                }

            }
        }*/

        private void ClearCache()
        {
            if (_brushCache != null)
            {
                foreach (var brush in _brushCache.Values)
                {
                    if (brush != null)
                    {
                        //brush.Dispose();
                    }
                }
                _brushCache.Clear();
            }

            if (_penCache != null)
            {
                foreach (var pen in _penCache.Values)
                {
                    if (pen != null)
                    {
                        /*if (pen.Brush != null)
                        {
                            pen.Brush.Dispose();
                        }*/
                        pen.Dispose();
                    }
                }
                _penCache.Clear();
            }

            /*if (_fontCache != null)
            {
                foreach (var font in _fontCache.Values)
                {
                    if (font != null)
                    {
                        if (font.FontFamily != null)
                        {
                            font.FontFamily.Dispose();
                        }
                        font.Dispose();
                    }
                }
                _fontCache.Clear();
            }*/
        }

        private Graphics SelectImageGraphicsByDrawingLevel(DrawingLevel drawingLevel)
        {
            //Validators.CheckDrawingLevelIsValid(drawingLevel, "drawingLevel");

            switch (drawingLevel)
            {
                case DrawingLevel.LevelOne:
                    if (_graphicsForLevel01 == null)
                    {
                        if (!_isBitmap)
                        {
                            var tempBitmap = _bufferImageForLevel01;
                            _bufferImageForLevel01 = Bitmap.CreateBitmap(_localCanvasWidth, _localCanvasHeight, Bitmap.Config.Argb8888);
                            _graphicsForLevel01 = Graphics.FromImage(_bufferImageForLevel01);
                            var g = (Graphics) _graphicsForLevel01;
                            g.DrawImage(tempBitmap, 0, 0);
                            tempBitmap.Dispose();
                        }
                        else
                        {
                            _graphicsForLevel01 = Graphics.FromImage(_bufferImageForLevel01);
                        }
                    }
                    SetGraphicsMode((Graphics) _graphicsForLevel01);
                    return (Graphics) _graphicsForLevel01;

                case DrawingLevel.LevelTwo:
                    if (_graphicsForLevel02 == null)
                    {
                        if (_bufferImageForLevel02 == null)
                        {
                            _bufferImageForLevel02 = Bitmap.CreateBitmap(_localCanvasWidth, _localCanvasHeight, Bitmap.Config.Argb8888);
                            //_bufferImageForLevel02.SetResolution(Dpi, Dpi);
                        }
                        _graphicsForLevel02 = Graphics.FromImage(_bufferImageForLevel02);
                    }
                    SetGraphicsMode((Graphics)_graphicsForLevel02);
                    return (Graphics)_graphicsForLevel02;

                case DrawingLevel.LevelThree:
                    if (_graphicsForLevel03 == null)
                    {
                        if (_bufferImageForLevel03 == null)
                        {
                            _bufferImageForLevel03 = Bitmap.CreateBitmap(_localCanvasWidth, _localCanvasHeight, Bitmap.Config.Argb8888);
                            //_bufferImageForLevel03.SetResolution(Dpi, Dpi);
                        }
                        _graphicsForLevel03 = Graphics.FromImage(_bufferImageForLevel03);
                    }
                    SetGraphicsMode((Graphics)_graphicsForLevel03);
                    return (Graphics)_graphicsForLevel03;

                case DrawingLevel.LevelFour:
                    if (_graphicsForLevel04 == null)
                    {
                        if (_bufferImageForLevel04 == null)
                        {
                            _bufferImageForLevel04 = Bitmap.CreateBitmap(_localCanvasWidth, _localCanvasHeight, Bitmap.Config.Argb8888);
                            //new Bitmap(_localCanvasWidth, _localCanvasHeight);
                            //_bufferImageForLevel04.SetResolution(Dpi, Dpi);
                        }
                        _graphicsForLevel04 = Graphics.FromImage(_bufferImageForLevel04);
                    }
                    SetGraphicsMode((Graphics)_graphicsForLevel04);
                    return (Graphics)_graphicsForLevel04;

                case DrawingLevel.LabelLevel:
                    if (_graphicsForLabel == null)
                    {
                        if (_bufferImageForLabel == null)
                        {
                            _bufferImageForLabel = Bitmap.CreateBitmap(_localCanvasWidth, _localCanvasHeight, Bitmap.Config.Argb8888);//new Bitmap(_localCanvasWidth, _localCanvasHeight);
                            //_bufferImageForLabel.SetResolution(Dpi, Dpi);
                        }
                        _graphicsForLabel = Graphics.FromImage(_bufferImageForLabel);
                    }
                    SetGraphicsMode((Graphics)_graphicsForLabel);
                    return (Graphics)_graphicsForLabel;
            }

            return null;
        }

        /*private Font GetGdiPlusFontFromGeoFont(GeoFont font)
        {
            if (font == null)
            {
                return null;
            }

            if (_fontCache == null)
            {
                _fontCache = new Dictionary<long, Font>();
            }

            if (_fontCache.ContainsKey(font.Id))
            {
                return _fontCache[font.Id];
            }

            var gdiplusFontStyle = GetFontStyleFromDrawingFontStyle(font.Style);

            var resultFont = new Font(font.FontName, font.Size, gdiplusFontStyle);

            _fontCache.Add(font.Id, resultFont);

            return resultFont;
        }*/

        private Color GetGdiPlusBrushFromGeoBrush(BaseGeoBrush brush, IEnumerable<ScreenPointF> areaPointsCache)
        {
            if (brush == null)
            {
                return Color.Transparent;
            }

            Color resultBrush = Color.Transparent;
            var geoLinearGradientBrush = brush as GeoLinearGradientBrush;

            if (geoLinearGradientBrush != null)
            {
                if (areaPointsCache != null)
                {
                    var minX = float.MaxValue;
                    var maxX = float.MinValue;
                    var minY = float.MaxValue;
                    var maxY = float.MinValue;

                    foreach (var pointF in areaPointsCache)
                    {
                        if (minX > pointF.X)
                        {
                            minX = pointF.X;
                        }
                        if (maxX < pointF.X)
                        {
                            maxX = pointF.X;
                        }
                        if (minY > pointF.Y)
                        {
                            minY = pointF.Y;
                        }
                        if (maxY < pointF.Y)
                        {
                            maxY = pointF.Y;
                        }
                    }
                    var width = maxX - minX;
                    var height = maxY - minY;
                    if (width == 0)
                    {
                        width = 1;
                    }
                    if (height == 0)
                    {
                        height = 1;
                    }

                    /*resultBrush = GetGdiBrushFromGeoLinearGradientBrush(geoLinearGradientBrush,
                        new RectangleF(minX, minY, width, height));*/
                }
                else
                {
                    /*resultBrush = GetGdiBrushFromGeoLinearGradientBrush(geoLinearGradientBrush,
                        new RectangleF(0, 0, _localCanvasWidth, _localCanvasHeight));*/
                }
            }
            else
            {
                if (_brushCache == null)
                {
                    _brushCache = new Dictionary<long, Color>();
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
                            resultBrush = GetGdiBrushFromGeoSolidBrush(geoSolidBrush);
                        }
                        break;

                    case "GeoLinearGradientBrush":

                        break;

                    case "GeoHatchBrush":
                        var geoHatchBrush = brush as GeoHatchBrush;
                        if (geoHatchBrush != null)
                        {
                            //resultBrush = GetGdiBrushFromGeoHatchBrush(geoHatchBrush);
                        }
                        break;

                    case "GeoTextureBrush":
                        var geoTextureBrush = brush as GeoTextureBrush;
                        if (geoTextureBrush != null)
                        {
                            //resultBrush = GetGdiBrushFromGeoTextureBrush(geoTextureBrush);
                        }
                        break;

                    default:
                        break;
                }

                _brushCache.Add(brush.Id, resultBrush);
            }

            return resultBrush;
        }

        private Paint GetGdiPlusPenFromGeoPen(GeoPen pen)
        {
            if (pen == null)
            {
                return null;
            }

            if (_penCache == null)
            {
                _penCache = new Dictionary<long, Paint>();
            }

            if (_penCache.ContainsKey(pen.Id))
            {
                return _penCache[pen.Id];
            }

            var resultPen = new Paint();//new Pen(GetGdiPlusColorFromGeoColor(pen.Color));
            resultPen.SetStyle(Paint.Style.Stroke);
            //resultPen.Width = pen.Width*(Dpi/StandardDpi);
            resultPen.StrokeWidth = pen.Width * (Dpi / StandardDpi);
            //resultPen.Brush = GetGdiPlusBrushFromGeoBrush(pen.Brush, null);
            resultPen.Color = GetGdiPlusColorFromGeoColor(pen.Color);
            //resultPen.DashCap = GetDashCapFromGeoDashCap(pen.DashCap);
            resultPen.StrokeCap = GetDashCapFromGeoDashCap(pen.DashCap);
            //resultPen.LineJoin = GetLineJoinFromDrawingLingJoin(pen.LineJoin);
            resultPen.StrokeJoin = GetLineJoinFromDrawingLingJoin(pen.LineJoin);
            //resultPen.MiterLimit = pen.MiterLimit;
            resultPen.StrokeMiter = pen.MiterLimit;
            //resultPen.StartCap = GetLineCapFromDrawingLineCap(pen.StartCap);
            //resultPen.EndCap = GetLineCapFromDrawingLineCap(pen.EndCap);

            if (pen.DashPattern != null && pen.DashPattern.Count > 0)
            {
                var dashPattern = new float[pen.DashPattern.Count];
                for (var i = 0; i < dashPattern.Length; i++)
                {
                    dashPattern[i] = pen.DashPattern[i];
                }

                //resultPen.DashPattern = dashPattern;
            }

            _penCache.Add(pen.Id, resultPen);

            return resultPen;
        }

        private static Color GetGdiPlusColorFromGeoColor(GeoColor color)
        {
            return new Color(color.RedComponent, color.GreenComponent, color.BlueComponent, color.AlphaComponent);
        }

        private static Color GetGdiBrushFromGeoSolidBrush(GeoSolidBrush brush)
        {
            return GetGdiPlusColorFromGeoColor(brush.Color);
            //return new SolidBrush(color);
        }

        private static Color GetGdiBrushFromGeoLinearGradientBrush(GeoLinearGradientBrush brush, RectF rectangle)
        {
            if (brush == null)
            {
                return Color.Transparent;
            }

            /*LinearGradientBrush resultBrush;

            var color1 = GetGdiPlusColorFromGeoColor(brush.StartColor);
            var color2 = GetGdiPlusColorFromGeoColor(brush.EndColor);

            resultBrush = new LinearGradientBrush(rectangle, color1, color2, brush.DirectionAngle);
            resultBrush.WrapMode = GetWrapModeFromGeoWrapMode(brush.WrapMode);*/

            return Color.Transparent;
        }

        private void UseKeyColor(Bitmap image)
        {
            if (KeyColor.AlphaComponent != 0)
            {
                /*image.MakeTransparent(Color.FromArgb(KeyColor.AlphaComponent, KeyColor.RedComponent,
                    KeyColor.GreenComponent, KeyColor.BlueComponent));

                var canvas = new NativeAndroid.Graphics.Canvas(image);
                canvas.DrawColor(Color.Transparent);*/
            }

            foreach (var oneKeyColor in KeyColors)
            {
                /*if (oneKeyColor.AlphaComponent != 0)
                {
                    image.MakeTransparent(Color.FromArgb(oneKeyColor.AlphaComponent, oneKeyColor.RedComponent,
                        oneKeyColor.GreenComponent, oneKeyColor.BlueComponent));
                }*/
            }
        }

        /*private static Brush GetGdiBrushFromGeoHatchBrush(GeoHatchBrush brush)
        {
            if (brush == null)
            {
                return null;
            }

            var foregroundColor = GetGdiPlusColorFromGeoColor(brush.ForegroundColor);
            var backgroundColor = GetGdiPlusColorFromGeoColor(brush.BackgroundColor);
            var hatchStyle = GetHatchStyleFromGeoHatchStyle(brush.HatchStyle);

            return new HatchBrush(hatchStyle, foregroundColor, backgroundColor);
        }*/

        /*private static Brush GetGdiBrushFromGeoTextureBrush(GeoTextureBrush brush)
        {
            if (brush == null)
            {
                return null;
            }

            var drawingRectangleF = brush.DrawingRectangleF;
            var upperLeftX = drawingRectangleF.CenterX - drawingRectangleF.Width/2;
            var upperLeftY = drawingRectangleF.CenterY - drawingRectangleF.Width/2;

            var image = Image.FromStream(brush.GeoImage.GetImageStream(new GdiPlusGeoCanvas()));
            var rectangleF = new RectangleF(upperLeftX, upperLeftY, drawingRectangleF.Width, drawingRectangleF.Height);

            TextureBrush resultBrush;
            if (rectangleF.Width == 0)
            {
                resultBrush = new TextureBrush(image);
            }
            else
            {
                resultBrush = new TextureBrush(image, GetWrapModeFromGeoWrapMode(brush.GeoWrapMode), rectangleF);
            }
            return resultBrush;
        }*/

        /*private static WrapMode GetWrapModeFromGeoWrapMode(GeoWrapMode geoWrapMode)
        {
            WrapMode wrapMode;

            switch (geoWrapMode)
            {
                case GeoWrapMode.Clamp:
                    wrapMode = WrapMode.Clamp;
                    break;
                case GeoWrapMode.Tile:
                    wrapMode = WrapMode.Tile;
                    break;
                case GeoWrapMode.TileFlipX:
                    wrapMode = WrapMode.TileFlipX;
                    break;
                case GeoWrapMode.TileFlipXY:
                    wrapMode = WrapMode.TileFlipXY;
                    break;
                case GeoWrapMode.TileFlipY:
                    wrapMode = WrapMode.TileFlipY;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("geoWrapMode");
            }

            return wrapMode;
        }*/

        /*private static FontStyle GetFontStyleFromDrawingFontStyle(DrawingFontStyles style)
        {
            var returnFontStyle = FontStyle.Regular;

            var value = (int) style;

            if (value < 1 || value > (int) (DrawingFontStyles.Regular |
                                            DrawingFontStyles.Bold |
                                            DrawingFontStyles.Italic |
                                            DrawingFontStyles.Underline |
                                            DrawingFontStyles.Strikeout))
            {
                throw new ArgumentOutOfRangeException("style", ExceptionDescription.EnumerationOutOfRange);
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
        }*/

        private static Paint.Cap GetDashCapFromGeoDashCap(GeoDashCap dashCap)
        {
            Paint.Cap returnDashCap;

            switch (dashCap)
            {
                case GeoDashCap.Flat:
                    returnDashCap = Paint.Cap.Square;
                    break;
                case GeoDashCap.Round:
                    returnDashCap = Paint.Cap.Round;
                    break;
                case GeoDashCap.Triangle:
                    //returnDashCap = Paint.Cap.;
                    throw new ArgumentOutOfRangeException("dashCap", ExceptionMessage.EnumerationOutOfRange);
                default:
                    throw new ArgumentOutOfRangeException("dashCap", ExceptionMessage.EnumerationOutOfRange);
            }

            return returnDashCap;
        }

        /*private static LineCap GetLineCapFromDrawingLineCap(DrawingLineCap lineCap)
        {
            LineCap returnLineCap;

            switch (lineCap)
            {
                case DrawingLineCap.Round:
                    returnLineCap = LineCap.Round;
                    break;
                case DrawingLineCap.AnchorMask:
                    returnLineCap = LineCap.AnchorMask;
                    break;
                case DrawingLineCap.ArrowAnchor:
                    returnLineCap = LineCap.ArrowAnchor;
                    break;
                case DrawingLineCap.Custom:
                    returnLineCap = LineCap.Custom;
                    break;
                case DrawingLineCap.DiamondAnchor:
                    returnLineCap = LineCap.DiamondAnchor;
                    break;
                case DrawingLineCap.Flat:
                    returnLineCap = LineCap.Flat;
                    break;
                case DrawingLineCap.NoAnchor:
                    returnLineCap = LineCap.NoAnchor;
                    break;
                case DrawingLineCap.RoundAnchor:
                    returnLineCap = LineCap.RoundAnchor;
                    break;
                case DrawingLineCap.Square:
                    returnLineCap = LineCap.Square;
                    break;
                case DrawingLineCap.SquareAnchor:
                    returnLineCap = LineCap.SquareAnchor;
                    break;
                case DrawingLineCap.Triangle:
                    returnLineCap = LineCap.Triangle;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("lineCap", ExceptionDescription.EnumerationOutOfRange);
            }

            return returnLineCap;
        }*/

        private static Paint.Join GetLineJoinFromDrawingLingJoin(DrawingLineJoin lineJoin)
        {
            Paint.Join returnLineJoin;

            switch (lineJoin)
            {
                case DrawingLineJoin.Bevel:
                    returnLineJoin = Paint.Join.Bevel;
                    break;
                case DrawingLineJoin.Miter:
                    returnLineJoin = Paint.Join.Miter;
                    break;
                case DrawingLineJoin.MiterClipped:
                    returnLineJoin = Paint.Join.Miter;
                    break;
                case DrawingLineJoin.Round:
                    returnLineJoin = Paint.Join.Round;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("lineJoin", ExceptionMessage.EnumerationOutOfRange);
            }

            return returnLineJoin;
        }

        /*private static HatchStyle GetHatchStyleFromGeoHatchStyle(GeoHatchStyle hatchStyle)
        {
            switch (hatchStyle)
            {
                case GeoHatchStyle.Horizontal:
                    return HatchStyle.Horizontal;
                case GeoHatchStyle.Vertical:
                    return HatchStyle.Vertical;
                case GeoHatchStyle.ForwardDiagonal:
                    return HatchStyle.ForwardDiagonal;
                case GeoHatchStyle.BackwardDiagonal:
                    return HatchStyle.BackwardDiagonal;
                case GeoHatchStyle.LargeGrid:
                    return HatchStyle.LargeGrid;
                case GeoHatchStyle.DiagonalCross:
                    return HatchStyle.DiagonalCross;
                case GeoHatchStyle.Percent05:
                    return HatchStyle.Percent05;
                case GeoHatchStyle.Percent10:
                    return HatchStyle.Percent10;
                case GeoHatchStyle.Percent20:
                    return HatchStyle.Percent20;
                case GeoHatchStyle.Percent25:
                    return HatchStyle.Percent25;
                case GeoHatchStyle.Percent30:
                    return HatchStyle.Percent30;
                case GeoHatchStyle.Percent40:
                    return HatchStyle.Percent40;
                case GeoHatchStyle.Percent50:
                    return HatchStyle.Percent50;
                case GeoHatchStyle.Percent60:
                    return HatchStyle.Percent60;
                case GeoHatchStyle.Percent70:
                    return HatchStyle.Percent70;
                case GeoHatchStyle.Percent75:
                    return HatchStyle.Percent75;
                case GeoHatchStyle.Percent80:
                    return HatchStyle.Percent80;
                case GeoHatchStyle.Percent90:
                    return HatchStyle.Percent90;
                case GeoHatchStyle.LightDownwardDiagonal:
                    return HatchStyle.LightDownwardDiagonal;
                case GeoHatchStyle.LightUpwardDiagonal:
                    return HatchStyle.LightUpwardDiagonal;
                case GeoHatchStyle.DarkDownwardDiagonal:
                    return HatchStyle.DarkDownwardDiagonal;
                case GeoHatchStyle.DarkUpwardDiagonal:
                    return HatchStyle.DarkUpwardDiagonal;
                case GeoHatchStyle.WideDownwardDiagonal:
                    return HatchStyle.WideDownwardDiagonal;
                case GeoHatchStyle.WideUpwardDiagonal:
                    return HatchStyle.WideUpwardDiagonal;
                case GeoHatchStyle.LightVertical:
                    return HatchStyle.LightVertical;
                case GeoHatchStyle.LightHorizontal:
                    return HatchStyle.LightHorizontal;
                case GeoHatchStyle.NarrowVertical:
                    return HatchStyle.NarrowVertical;
                case GeoHatchStyle.NarrowHorizontal:
                    return HatchStyle.NarrowHorizontal;
                case GeoHatchStyle.DarkVertical:
                    return HatchStyle.DarkVertical;
                case GeoHatchStyle.DarkHorizontal:
                    return HatchStyle.DarkHorizontal;
                case GeoHatchStyle.DashedDownwardDiagonal:
                    return HatchStyle.DashedDownwardDiagonal;
                case GeoHatchStyle.DashedUpwardDiagonal:
                    return HatchStyle.DashedUpwardDiagonal;
                case GeoHatchStyle.DashedHorizontal:
                    return HatchStyle.DashedHorizontal;
                case GeoHatchStyle.DashedVertical:
                    return HatchStyle.DashedVertical;
                case GeoHatchStyle.SmallConfetti:
                    return HatchStyle.SmallConfetti;
                case GeoHatchStyle.LargeConfetti:
                    return HatchStyle.LargeConfetti;
                case GeoHatchStyle.ZigZag:
                    return HatchStyle.ZigZag;
                case GeoHatchStyle.Wave:
                    return HatchStyle.Wave;
                case GeoHatchStyle.DiagonalBrick:
                    return HatchStyle.DiagonalBrick;
                case GeoHatchStyle.HorizontalBrick:
                    return HatchStyle.HorizontalBrick;
                case GeoHatchStyle.Weave:
                    return HatchStyle.Weave;
                case GeoHatchStyle.Plaid:
                    return HatchStyle.Plaid;
                case GeoHatchStyle.Divot:
                    return HatchStyle.Divot;
                case GeoHatchStyle.DottedGrid:
                    return HatchStyle.DottedGrid;
                case GeoHatchStyle.DottedDiamond:
                    return HatchStyle.DottedDiamond;
                case GeoHatchStyle.Shingle:
                    return HatchStyle.Shingle;
                case GeoHatchStyle.Trellis:
                    return HatchStyle.Trellis;
                case GeoHatchStyle.Sphere:
                    return HatchStyle.Sphere;
                case GeoHatchStyle.SmallGrid:
                    return HatchStyle.SmallGrid;
                case GeoHatchStyle.SmallCheckerBoard:
                    return HatchStyle.SmallCheckerBoard;
                case GeoHatchStyle.LargeCheckerBoard:
                    return HatchStyle.LargeCheckerBoard;
                case GeoHatchStyle.OutlinedDiamond:
                    return HatchStyle.OutlinedDiamond;
                case GeoHatchStyle.SolidDiamond:
                    return HatchStyle.SolidDiamond;
                case GeoHatchStyle.Min:
                    return HatchStyle.Min;
                case GeoHatchStyle.Max:
                    return HatchStyle.Max;
                case GeoHatchStyle.Cross:
                    return HatchStyle.Cross;
                default:
                    throw new ArgumentOutOfRangeException("hatchStyle", ExceptionDescription.EnumerationOutOfRange);
            }
        }*/

        private void SetGraphicsMode(Graphics graphics)
        {
            if (graphics == null)
            {
                return;
            }

            switch (DrawingQuality)
            {
                case DrawingQuality.Default:
                    //graphics.SmoothingMode = SmoothingMode.HighQuality;
                    //graphics.CompositingQuality = CompositingQuality.HighSpeed;
                    //graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
                    graphics.DrawConfig.AntiAlias = true;
                    graphics.DrawConfig.Hinting = PaintHinting.On;
                    break;

                case DrawingQuality.HighQuality:
                    //graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    //graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
                    //graphics.CompositingQuality = CompositingQuality.Default;
                    graphics.DrawConfig.AntiAlias = true;
                    graphics.DrawConfig.Hinting = PaintHinting.On;
                    break;

                case DrawingQuality.HighSpeed:
                    //graphics.SmoothingMode = SmoothingMode.HighSpeed;
                    //graphics.CompositingQuality = CompositingQuality.HighSpeed;
                    //graphics.TextRenderingHint = TextRenderingHint.SystemDefault;
                    graphics.DrawConfig.AntiAlias = false;
                    graphics.DrawConfig.Hinting = PaintHinting.Off;
                    break;

                case DrawingQuality.CanvasSettings:
                    //graphics.SmoothingMode = _smoothingMode;
                    //graphics.TextRenderingHint = _textRenderingHint;
                    //graphics.CompositingQuality = _compositiongQuality;
                    graphics.DrawConfig.AntiAlias = true;
                    graphics.DrawConfig.Hinting = PaintHinting.On;
                    break;
                default:
                    break;
            }
            //graphics.InterpolationMode = InterpolationMode;
        }

        /// <summary>Gets the canvas width of a native image object.</summary>
        /// <param name="nativeImage">Native image used to get the canvas width.</param>
        /// <returns>Canvas width.</returns>
        protected override float GetCanvasWidthCore(object nativeImage)
        {
            var image = nativeImage as GeoImage;

            if (image != null)
            {
                return image.GetWidth();
            }
            return ((NativeAndroid.Graphics.Canvas) nativeImage).Width;
        }

        /// <summary>Gets the canvas height of a native image object.</summary>
        /// <param name="nativeImage">Native image will be used to get the canvas height.</param>
        /// <returns>Canvas height.</returns>
        protected override float GetCanvasHeightCore(object nativeImage)
        {
            var image = nativeImage as GeoImage;

            if (image != null)
            {
                return image.GetHeight();
            }
            return ((NativeAndroid.Graphics.Canvas) nativeImage).Height;
        }

        /// <summary>Draws an unscaled image on the canvas.</summary>
        /// <remarks>
        /// 	<para>Drawing an image unscaled is faster than scaling it.</para>
        /// 	<para>The X &amp; Y in world coordinates is where the center of the image draws.</para>
        /// </remarks>
        /// <param name="image">Image to draw unscaled.</param>
        /// <param name="centerXInWorld">X coordinate in world coordinates of the center point of the image to draw.</param>
        /// <param name="centerYInWorld">Y coordinate in world coordinates of the center point of the image to draw.</param>
        /// <param name="drawingLevel">DrawingLevel the image draws on.</param>
        public void DrawWorldImageWithoutScaling(Bitmap image, double centerXInWorld, double centerYInWorld,
            DrawingLevel drawingLevel)
        {
            Validators.CheckParameterIsNotNull(image, "image");
            Validators.CheckGeoCanvasIsInDrawing(IsDrawing);
            //Validators.CheckDrawingLevelIsValid(drawingLevel, "drawingLevel");
   
            //HasDrawn = true;

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
        public void DrawWorldImageWithoutScaling(Bitmap image, double centerXInWorld, double centerYInWorld,
            DrawingLevel drawingLevel, float xOffset, float yOffset, float rotateAngle)
        {
            Validators.CheckParameterIsNotNull(image, "image");
            Validators.CheckGeoCanvasIsInDrawing(IsDrawing);
            //Validators.CheckDrawingLevelIsValid(drawingLevel, "drawingLevel");
   
            var upperLeftX = CurrentWorldExtent.UpperLeftPoint.X;
            var upperLeftY = CurrentWorldExtent.UpperLeftPoint.Y;

            var screenX = (float) ((centerXInWorld - upperLeftX)*Width/CurrentWorldExtent.Width);
            var screenY = (float) ((upperLeftY - centerYInWorld)*Height/CurrentWorldExtent.Height);

            DrawScreenImageWithoutScalingCore(image, screenX, screenY, drawingLevel, xOffset, yOffset, rotateAngle);

            //HasDrawn = true;
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
        public void DrawScreenImageWithoutScaling(Bitmap image, float centerXInScreen, float centerYInScreen,
            DrawingLevel drawingLevel, float xOffset, float yOffset, float rotateAngle)
        {
            DrawScreenImageWithoutScalingCore(image, centerXInScreen, centerYInScreen, drawingLevel, xOffset, yOffset,
                rotateAngle);
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
        private void DrawScreenImageWithoutScalingCore(Bitmap image, float centerXInScreen, float centerYInScreen,
            DrawingLevel drawingLevel, float xOffset, float yOffset, float rotateAngle)
        {
            var imageWidth = image.Width;
            var imageHeight = image.Height;

            var screenX = centerXInScreen;
            var screenY = centerYInScreen;

            screenX += -imageWidth/2.0f + xOffset;
            screenY += -imageHeight/2.0f + yOffset;        

            /*var tempVDpi = image.HorizontalResoluton;
            var tempHDpi = image.VerticalResolution;
            if (image.HorizontalResolution != Dpi)
            {
                image.SetResolution(Dpi, Dpi);
            }*/
            UseKeyColor(image);

            if (rotateAngle == 0)
            {
                var graphics = SelectImageGraphicsByDrawingLevel(drawingLevel);
                graphics.DrawImageUnscaled(image, (int) Math.Round(screenX), (int) Math.Round(screenY));
            }
            else
            {
                var upperLeftPointX = -imageWidth*0.5f;
                var upperLeftPointY = imageHeight*0.5f;
                var rotateRadient = rotateAngle*Math.PI/180;
                var baseRadient = Math.PI - Math.Atan(imageHeight/(double) imageWidth);
                var radius = Math.Sqrt(imageWidth*imageWidth + imageHeight*imageHeight)*0.5;
                var newRadient = baseRadient + rotateRadient;
                var newPointX = radius*Math.Cos(newRadient);
                var newPointY = radius*Math.Sin(newRadient);
                var xOffsetInScreen = newPointX - upperLeftPointX;
                var yOffsetInScreen = -(newPointY - upperLeftPointY);
                screenX += (float) xOffsetInScreen;
                screenY += (float) yOffsetInScreen;

                var graphics = SelectImageGraphicsByDrawingLevel(drawingLevel);

                graphics.TranslateTransform(screenX, screenY);
                graphics.RotateTransform(-rotateAngle);

                graphics.DrawImageUnscaled(image, 0, 0);

                graphics.RotateTransform(rotateAngle);
                graphics.TranslateTransform(-screenX, -screenY);
            }

            //image.SetResolution(tempHDpi, tempVDpi);
        }
    }
}
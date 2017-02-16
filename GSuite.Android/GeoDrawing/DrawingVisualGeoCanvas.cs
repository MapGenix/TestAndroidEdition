using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Windows;
using Mapgenix.Canvas;
using Mapgenix.Shapes;
using Android.Graphics;

namespace Mapgenix.GSuite.Android
{
    /// <summary>
    /// This class represents a canvas that is used to draw shapes in world coordinates. It is
    /// intended to be inherited for the implementation of the drawing.
    /// </summary>
    [Serializable]
    public class DrawingVisualGeoCanvas : BaseGeoCanvas
    {
        private const float StandardDpi = 96f;
        private float _dpi;

        [NonSerialized]
        private Bitmap _imageSource;
        //private Dictionary<DrawingLevel, KeyValuePair<DrawingVisual, DrawingContext>> _drawingVisuals;

        public DrawingVisualGeoCanvas()
        {
            _dpi = StandardDpi;
        }

        public override float Dpi
        {
            get
            {
                return _dpi;
            }
            set
            {
                _dpi = value;
            }
        }

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


        private void BeginDrawingCore(object nativeImage, RectangleShape worldExtent, GeographyUnit drawingMapUnit)
        {
            _imageSource = (Bitmap)nativeImage;
            CurrentWorldExtent = worldExtent;
            Width = (float)_imageSource.Width;
            Height = (float)_imageSource.Height;

            //_drawingVisuals = new Dictionary<DrawingLevel, KeyValuePair<DrawingVisual, DrawingContext>>();
            AddDrawingVisual(DrawingLevel.LevelOne);
            AddDrawingVisual(DrawingLevel.LevelTwo);
            AddDrawingVisual(DrawingLevel.LevelThree);
            AddDrawingVisual(DrawingLevel.LevelFour);
            AddDrawingVisual(DrawingLevel.LabelLevel);
        }

        private void AddDrawingVisual(DrawingLevel drawingLevel)
        { 
            //DrawingVisual bufferedVisual = new DrawingVisual();
            //DrawingContext bufferedContext = bufferedVisual.RenderOpen();
            //KeyValuePair<DrawingVisual, DrawingContext> visualContext = new KeyValuePair<DrawingVisual, DrawingContext>(bufferedVisual, bufferedContext);
            //_drawingVisuals.Add(drawingLevel, visualContext);
        }

        public void EndDrawing()
        {
            _hasDrawn = false;
            NativeImage = null;
            EndDrawingCore();

            IsDrawing = false;
        }


        private void EndDrawingCore()
        {
            /*foreach (KeyValuePair<DrawingLevel, KeyValuePair<DrawingVisual, DrawingContext>> bufferedVisualContext in _drawingVisuals)
            {
                bufferedVisualContext.Value.Value.Close();
                _imageSource.Render(bufferedVisualContext.Value.Key);
            }*/
        }

        protected override void DrawAreaCore(IEnumerable<ScreenPointF[]> screenPoints, GeoPen outlinePen, BaseGeoBrush fillBrush, DrawingLevel drawingLevel, float xOffset, float yOffset, PenBrushDrawingOrder penBrushDrawingOrder)
        {
            /*Validators.CheckParameterIsNotNull(screenPoints, "screenPoints");
            Validators.CheckParametersAreNotBothNull(outlinePen, fillBrush, "outlinePen", "fillBrush");
            Validators.CheckGeoCanvasIsInDrawing(IsDrawing);

            Collection<ScreenPointF[]> ringsCollection = new Collection<ScreenPointF[]>();
            foreach (ScreenPointF[] ring in screenPoints)
            {
                ScreenPointF[] points = new ScreenPointF[ring.Length];
                double previousX = double.MaxValue;
                double previousY = double.MaxValue;
                int tempCount = 0;
                for (int i = 0; i < ring.Length; i++)
                {
                    ScreenPointF point = ring[i];
                    double screenX = point.X + xOffset;
                    double screenY = point.Y + yOffset;
                    if (previousX != screenX || previousY != screenY)
                    {
                        previousX = screenX;
                        previousY = screenY;
                        points[tempCount] = new ScreenPointF((float)screenX, (float)screenY);
                        tempCount++;
                    }
                }
                if (tempCount <= 2) { continue; }
                if (tempCount != ring.Length)
                {
                    Array.Resize<ScreenPointF>(ref points, tempCount);
                }
                ringsCollection.Add(points);
            }

            PathGeometry pathGeometry = new PathGeometry();
            foreach (ScreenPointF[] points in ringsCollection)
            {
                PathFigure figure = new PathFigure();
                if (points.Length >= 1)
                {
                    figure.StartPoint = new System.Windows.Point(points[0].X, points[0].Y);
                }

                int pointIndex = 0;
                ScreenPointF tempPoint = points[0];
                foreach (ScreenPointF point in points)
                {
                    if (pointIndex == 0)
                    {
                        pointIndex++;
                        continue;
                    }
                    else if (pointIndex != points.Length - 1)
                    {
                        if (Math.Abs(point.X - tempPoint.X) <= 1 && Math.Abs(point.Y - tempPoint.Y) <= 1)
                        {
                            pointIndex++;
                            continue;
                        }
                        else
                        {
                            LineSegment lineSegment = new LineSegment();
                            lineSegment.Point = new Point(point.X, point.Y);
                            figure.Segments.Add(lineSegment);
                            tempPoint = point;
                            pointIndex++;
                        }
                    }
                    else
                    {
                        LineSegment lineSegment = new LineSegment();
                        lineSegment.Point = new Point(point.X, point.Y);
                        figure.Segments.Add(lineSegment);
                        tempPoint = point;
                        pointIndex++;
                    }
                }
                pathGeometry.Figures.Add(figure);
            }

            Pen pen = StyleHelper.GetWpfPenFromGeoPen(outlinePen);
            Brush brush = StyleHelper.GetWpfBrushFromGeoBrush(fillBrush);
            _drawingVisuals[drawingLevel].Value.DrawGeometry(brush, pen, pathGeometry);*/
        }

        protected override void DrawLineCore(IEnumerable<ScreenPointF> screenPoints, GeoPen linePen, DrawingLevel drawingLevel, float xOffset, float yOffset)
        {
            /*Validators.CheckParameterIsNotNull(screenPoints, "screenPoints");
            Validators.CheckParameterIsNotNull(linePen, "outlinePen");
            Validators.CheckGeoCanvasIsInDrawing(IsDrawing);

            PointCollection points = new PointCollection();
            double previousX = double.MaxValue;
            double previousY = double.MaxValue;

            int pointsToRemove = 0;
            foreach (ScreenPointF point in screenPoints)
            {
                double screenX = point.X + xOffset;
                double screenY = point.Y + yOffset;
                if (previousX != screenX || previousY != screenY)
                {
                    if (Math.Abs(screenX - previousX) > 1 || Math.Abs(screenY - previousY) > 1)
                    {
                        previousX = screenX;
                        previousY = screenY;
                        points.Add(new Point(screenX, screenY));
                    }
                    else
                    {
                        pointsToRemove++;
                    }
                }
            }

            if (points.Count > 1)
            {
                PathGeometry path = new PathGeometry();
                PathFigure figure = new PathFigure();
                int pointIndex = 0;
                foreach (Point point in points)
                {
                    if (pointIndex++ == 0)
                    {
                        figure.StartPoint = new Point(point.X, point.Y);
                    }
                    else
                    {
                        LineSegment lineSegment = new LineSegment();
                        lineSegment.Point = new Point(point.X, point.Y);
                        figure.Segments.Add(lineSegment);
                    }
                }

                path.Figures.Add(figure);
                Pen pen = StyleHelper.GetWpfPenFromGeoPen(linePen);
                _drawingVisuals[drawingLevel].Value.DrawGeometry(null, pen, path);
            }*/
        }

        protected override void DrawEllipseCore(ScreenPointF screenPoint, float width, float height, GeoPen outlinePen, BaseGeoBrush fillBrush, DrawingLevel drawingLevel, float xOffset, float yOffset, PenBrushDrawingOrder penBrushDrawingOrder)
        {
            /*Validators.CheckParametersAreNotBothNull(outlinePen, fillBrush, "outlinePen", "fillBrush");
            Validators.CheckGeoCanvasIsInDrawing(IsDrawing);

            double screenX = screenPoint.X + xOffset;
            double screenY = screenPoint.Y + yOffset;

            Point center = new Point(screenX, screenY);
            EllipseGeometry ellipse = new EllipseGeometry(center, width * .5, height * .5);

            Pen pen = StyleHelper.GetWpfPenFromGeoPen(outlinePen);
            Brush brush = StyleHelper.GetWpfBrushFromGeoBrush(fillBrush);
            _drawingVisuals[drawingLevel].Value.DrawGeometry(brush, pen, ellipse);*/
        }

        protected override void DrawScreenImageWithoutScalingCore(GeoImage image, float centerXInScreen, float centerYInScreen, DrawingLevel drawingLevel, float xOffset, float yOffset, float rotateAngle)
        {
            /*float drawingWidth = float.NaN;
            float drawingHeight = float.NaN;

            WpfGeoImage wpfGeoImage = image as WpfGeoImage;
            if (wpfGeoImage != null)
            {
                drawingWidth = (float)wpfGeoImage.WidthInPixel;
                drawingHeight = (float)wpfGeoImage.HeightInPixel;
            }
            else
            {
                drawingWidth = image.GetWidth();
                drawingHeight = image.GetHeight();
            }

            DrawScreenImageCore(image, centerXInScreen, centerYInScreen, drawingWidth, drawingHeight, drawingLevel, xOffset, yOffset, rotateAngle);*/
        }

        protected override void DrawScreenImageCore(GeoImage image, float centerXInScreen, float centerYInScreen, float widthInScreen, float heightInScreen, DrawingLevel drawingLevel, float xOffset, float yOffset, float rotateAngle)
        {
            /*double drawingWidth = (double)widthInScreen;
            double drawingHeight = (double)heightInScreen;

            double screenX = centerXInScreen;
            double screenY = centerYInScreen;

            BitmapSource tagetImageSource = GetImageSource(image);

            if (double.IsNaN(drawingWidth) || double.IsNaN(drawingHeight))
            {
                drawingWidth = tagetImageSource.Width;
                drawingHeight = tagetImageSource.Height;
            }

            screenX += -drawingWidth * .5f;
            screenY += -drawingHeight * .5f;

            Rect drawingRect = new Rect(new Point(screenX, screenY), new Size(drawingWidth, drawingHeight));
            if (!float.IsNaN(rotateAngle) && rotateAngle != 0f)
            {
                RotateTransform transform = new RotateTransform(rotateAngle);
                TransformedBitmap transformedImageSource = new TransformedBitmap(tagetImageSource, transform);
                _drawingVisuals[drawingLevel].Value.DrawImage(transformedImageSource, drawingRect);
            }
            else
            {
                _drawingVisuals[drawingLevel].Value.DrawImage(tagetImageSource, drawingRect);
            }*/
        }

        protected override void DrawTextCore(string text, GeoFont font, BaseGeoBrush fillBrush, GeoPen haloPen, IEnumerable<ScreenPointF> textPathInScreen, DrawingLevel drawingLevel, float xOffset, float yOffset, float rotateAngle)
        {
           /* DrawingRectangleF rectangle = MeasureText(text, font);

            double centerX = rectangle.Width * .5;
            double centerY = rectangle.Height * .5;
            foreach (ScreenPointF point in textPathInScreen)
            {
                double drawingX = point.X + xOffset - centerX;
                double drawingY = point.Y + yOffset - centerY;

                if (rotateAngle != 0)
                {
                    RotateTransform rotateTrans = new RotateTransform();
                    rotateTrans.CenterX = point.X;
                    rotateTrans.CenterY = point.Y;
                    rotateTrans.Angle = -rotateAngle;
                    _drawingVisuals[drawingLevel].Value.PushTransform(rotateTrans);
                }

                Brush foreground = StyleHelper.GetWpfBrushFromGeoBrush(fillBrush);
                FormattedText formattedText = new FormattedText(text, CultureInfo.InvariantCulture, FlowDirection.LeftToRight, StyleHelper.GetTypeface(font), font.Size, foreground);
                _drawingVisuals[drawingLevel].Value.DrawText(formattedText, new Point(drawingX, drawingY));

                if (rotateAngle != 0)
                {
                    _drawingVisuals[drawingLevel].Value.Pop();
                }
            }*/
        }

        protected override DrawingRectangleF MeasureTextCore(string text, GeoFont font)
        {
            /*Validators.CheckParameterIsNotNull(font, "font");
            Validators.CheckParameterIsNotNull(text, "text");
            Validators.CheckParameterIsNotNullOrEmpty(text, "text");
            Validators.CheckGeoCanvasIsInDrawing(IsDrawing);

            TextBlock label = new TextBlock();
            label.Text = text;
            label.FontSize = font.Size;
            if (font.IsItalic)
            {
                label.FontStyle = FontStyles.Italic;
            }
            else
            {
                label.FontStyle = FontStyles.Normal;
            }
            label.FontFamily = new FontFamily(font.FontName);
            label.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            float width = (float)label.DesiredSize.Width;
            float height = (float)label.DesiredSize.Height;

            return new DrawingRectangleF(width / 2, height / 2, width, height);*/

            return new DrawingRectangleF();
        }

        protected override object ToNativeImageCore(GeoImage image)
        {
            throw new NotImplementedException();
        }

        protected override GeoImage ToGeoImageCore(object nativeImage)
        {
            throw new NotImplementedException();
        }

        protected override float GetCanvasWidthCore(object nativeImage)
        {
            return (float)_imageSource.Width;
        }
        
        protected override float GetCanvasHeightCore(object nativeImage)
        {
            return (float)_imageSource.Height;
        }

        public override Stream GetStreamFromGeoImage(GeoImage image)
        {
            /*WpfGeoImage wpfGeoImage = image as WpfGeoImage;
            if (wpfGeoImage != null && wpfGeoImage.SourceStream != null)
            {
                return wpfGeoImage.SourceStream;
            }
            else if (wpfGeoImage != null && wpfGeoImage.ImageUri != null)
            {
               
                return null;
            }

            if (String.IsNullOrEmpty(image.PathFilename) || !File.Exists(image.PathFilename))
            {
                return null;
            }

            MemoryStream imageStream = null;
            System.Drawing.Bitmap bitmap = null;

            try
            {
                imageStream = new MemoryStream();
                bitmap = new System.Drawing.Bitmap(image.PathFilename);

                image.CanvasImageFormat = bitmap.RawFormat.Guid;
                bitmap.Save(imageStream, System.Drawing.Imaging.ImageFormat.Png);
                imageStream.Seek(0, SeekOrigin.Begin);
            }
            finally
            {
                if (bitmap != null) { bitmap.Dispose(); }
            }

            return imageStream;*/
            return null;
        }


        protected override void FlushCore() { }

        /*private BitmapImage GetImageSource(GeoImage geoImage)
        {
            BitmapImage newImageSource = new BitmapImage();
            WpfGeoImage wpfGeoImage = geoImage as WpfGeoImage;
            if (wpfGeoImage != null)
            {
                if (wpfGeoImage.SourceStream != null)
                {
                    newImageSource.BeginInit();
                    newImageSource.StreamSource = wpfGeoImage.SourceStream;
                    newImageSource.EndInit();
                }
                else if (wpfGeoImage.ImageUri != null)
                {
                    newImageSource.BeginInit();
                    newImageSource.UriSource = wpfGeoImage.ImageUri;
                    newImageSource.EndInit();
                    return newImageSource;
                }
            }

            Stream imageStream = geoImage.GetImageStream(this);
            if (imageStream != null)
            {
                newImageSource.BeginInit();
                newImageSource.StreamSource = imageStream;
                newImageSource.EndInit();
                return newImageSource;
            }
            else if (!String.IsNullOrEmpty(geoImage.PathFilename) && File.Exists(geoImage.PathFilename))
            {
                using (MemoryStream ms = new MemoryStream(File.ReadAllBytes(geoImage.PathFilename)))
                {
                    newImageSource.BeginInit();
                    newImageSource.StreamSource = ms;
                    newImageSource.EndInit();
                    return newImageSource;
                }
            }

            return newImageSource;
        }*/
    }
}

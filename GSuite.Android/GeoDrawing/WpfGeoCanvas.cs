using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Mapgenix.Canvas;
using Mapgenix.Shapes;

namespace Mapgenix.GSuite.Android
{
    /// <summary>
    /// This class represents a GeoCanvas to draw on Wpf shapes.
    /// </summary>
    [Serializable]
    public class WpfGeoCanvas : BaseGeoCanvas
    {
        [NonSerialized]
        private System.Windows.Controls.Canvas _drawingCanvas;

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
            this._drawingCanvas = (System.Windows.Controls.Canvas)nativeImage;
            this.Width = (float)this._drawingCanvas.Width;
            this.Height = (float)this._drawingCanvas.Height;

            this.CurrentWorldExtent = worldExtent;
        }

       
        protected override void DrawAreaCore(IEnumerable<ScreenPointF[]> screenPoints, GeoPen outlinePen, BaseGeoBrush fillBrush, DrawingLevel drawingLevel, float xOffset, float yOffset, PenBrushDrawingOrder penBrushDrawingOrder)
        {
            Validators.CheckParameterIsNotNull(screenPoints, "screenPoints");
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

            int count = ringsCollection.Count;
            if (count == 1)
            {
                foreach (ScreenPointF[] points in ringsCollection)
                {
                    PointCollection polygonPoints = new PointCollection();
                    foreach (ScreenPointF point in points)
                    {
                        polygonPoints.Add(new System.Windows.Point(point.X, point.Y));
                    }
                    Polygon polygon = new Polygon();
                    polygon.Points = polygonPoints;
                    if (outlinePen != null)
                    {
                        ApplyGeoPenToShape(outlinePen, polygon);
                    }
                    if (fillBrush != null)
                    {
                        Brush brush = StyleHelper.GetWpfBrushFromGeoBrush(fillBrush);
                        polygon.Fill = brush;
                    }
                    _drawingCanvas.Children.Add(polygon);
                }
            }
            else if (count != 0)
            {
                PathGeometry pathGeometry = new PathGeometry();
                int pointsToRemove = 0;
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
                            pointsToRemove++;
                            continue;
                        }
                        else if (pointIndex != points.Length - 1)
                        {
                            if (Math.Abs(point.X - tempPoint.X) <= 1 && Math.Abs(point.Y - tempPoint.Y) <= 1)
                            {
                                pointIndex++;
                                pointsToRemove++;
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

                System.Windows.Shapes.Path path = new System.Windows.Shapes.Path();
                path.Data = pathGeometry;

                if (outlinePen != null)
                {
                    ApplyGeoPenToShape(outlinePen, path);
                }
                if (fillBrush != null)
                {
                    Brush brush = StyleHelper.GetWpfBrushFromGeoBrush(fillBrush);
                    path.Fill = brush;
                }
                _drawingCanvas.Children.Add(path);
            }
        }

      
        protected override void DrawLineCore(IEnumerable<ScreenPointF> screenPoints, GeoPen linePen, DrawingLevel drawingLevel, float xOffset, float yOffset)
        {
            Validators.CheckParameterIsNotNull(screenPoints, "screenPoints");
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
                Polyline line = new Polyline();
                line.Points = points;
                if (linePen != null)
                {
                    ApplyGeoPenToShape(linePen, line);
                }
                _drawingCanvas.Children.Add(line);
            }
        }

       
        protected override void DrawEllipseCore(ScreenPointF screenPoint, float width, float height, GeoPen outlinePen, BaseGeoBrush fillBrush, DrawingLevel drawingLevel, float xOffset, float yOffset, PenBrushDrawingOrder penBrushDrawingOrder)
        {
            Validators.CheckParametersAreNotBothNull(outlinePen, fillBrush, "outlinePen", "fillBrush");
            Validators.CheckGeoCanvasIsInDrawing(IsDrawing);

            double screenX = screenPoint.X + xOffset - width * .5;
            double screenY = screenPoint.Y + yOffset - height * .5;
            Ellipse ellipse = new Ellipse();
            ellipse.SetValue(System.Windows.Controls.Canvas.LeftProperty, screenX);
            ellipse.SetValue(System.Windows.Controls.Canvas.TopProperty, screenY);
            ellipse.Width = width;
            ellipse.Height = height;

            if (outlinePen != null)
            {
                ApplyGeoPenToShape(outlinePen, ellipse);
            }
            if (fillBrush != null)
            {
                Brush brush = StyleHelper.GetWpfBrushFromGeoBrush(fillBrush);
                ellipse.Fill = brush;
            }
            _drawingCanvas.Children.Add(ellipse);
        }

       
        protected override void DrawScreenImageWithoutScalingCore(GeoImage image, float centerXInScreen, float centerYInScreen, DrawingLevel drawingLevel, float xOffset, float yOffset, float rotateAngle)
        {
            Image wpfImage = GetImage(image);
            double widthInScreen = wpfImage.ActualWidth;
            double heightInScreen = wpfImage.ActualHeight;

            double screenX = centerXInScreen;
            double screenY = centerYInScreen;
            screenX += -widthInScreen / 2.0f + xOffset;
            screenY += -heightInScreen / 2.0f + yOffset;

            wpfImage.SetValue(System.Windows.Controls.Canvas.LeftProperty, screenX);
            wpfImage.SetValue(System.Windows.Controls.Canvas.TopProperty, screenY);
            if (widthInScreen != .0d)
            {
                wpfImage.Width = widthInScreen;
            }
            if (heightInScreen != .0d)
            {
                wpfImage.Height = heightInScreen;
            }
            if (rotateAngle != 0)
            {
                TransformGroup transformGroup = new TransformGroup();
                RotateTransform rotateTransform = new RotateTransform();
                rotateTransform.Angle = rotateAngle;
                rotateTransform.CenterX = widthInScreen / 2;
                rotateTransform.CenterY = heightInScreen / 2;
                transformGroup.Children.Add(rotateTransform);
                wpfImage.RenderTransform = transformGroup;
            }

            _drawingCanvas.Children.Add(wpfImage);
        }

       
        protected override void DrawScreenImageCore(GeoImage image, float centerXInScreen, float centerYInScreen, float widthInScreen, float heightInScreen, DrawingLevel drawingLevel, float xOffset, float yOffset, float rotateAngle)
        {
            Image wpfImage = GetImage(image);
            wpfImage.Width = widthInScreen;
            wpfImage.Height = heightInScreen;
            wpfImage.Stretch = Stretch.Fill;

            double screenX = centerXInScreen;
            double screenY = centerYInScreen;
            screenX += -widthInScreen / 2.0f + xOffset;
            screenY += -heightInScreen / 2.0f + yOffset;

            wpfImage.SetValue(System.Windows.Controls.Canvas.LeftProperty, screenX);
            wpfImage.SetValue(System.Windows.Controls.Canvas.TopProperty, screenY);
            if (widthInScreen != .0d)
            {
                wpfImage.Width = widthInScreen;
            }
            if (heightInScreen != .0d)
            {
                wpfImage.Height = heightInScreen;
            }
            if (rotateAngle != 0)
            {
                TransformGroup transformGroup = new TransformGroup();
                RotateTransform rotateTransform = new RotateTransform();
                rotateTransform.Angle = rotateAngle;
                rotateTransform.CenterX = widthInScreen / 2;
                rotateTransform.CenterY = heightInScreen / 2;
                transformGroup.Children.Add(rotateTransform);
                wpfImage.RenderTransform = transformGroup;
            }

            _drawingCanvas.Children.Add(wpfImage);
        }

       
        protected override void DrawTextCore(string text, GeoFont font, BaseGeoBrush fillBrush, GeoPen haloPen, IEnumerable<ScreenPointF> textPathInScreen, DrawingLevel drawingLevel, float xOffset, float yOffset, float rotateAngle)
        {
            DrawingRectangleF rectangle = MeasureText(text, font);

            double width = rectangle.Width;
            double height = rectangle.Height;

            double rotateRadient = 0;
            double xOffsetRotated = 0;
            double yOffsetRotated = 0;

            if (rotateAngle != 0)
            {
                double upperLeftX = -width * 0.5;
                double upperLeftY = height * 0.5;

                double radius = Math.Sqrt(width * width + height * height) * 0.5;

                rotateRadient = rotateAngle * Math.PI / 180;

                double newRadient = rotateRadient - Math.Atan(height / width) + Math.PI;

                xOffsetRotated = radius * Math.Cos(newRadient) - upperLeftX;
                yOffsetRotated = -(radius * Math.Sin(newRadient) - upperLeftY);
            }

            foreach (ScreenPointF point in textPathInScreen)
            {
                ScreenPointF pointForLabel = new ScreenPointF((float)(point.X - width * .5), (float)(point.Y - height * .5));
                TextBlock textBlock = GetDrawString(text, font, fillBrush, new Point(pointForLabel.X + xOffset + xOffsetRotated, pointForLabel.Y + yOffset + yOffsetRotated));

                TransformGroup transGroup = new TransformGroup();
                if (rotateAngle != 0)
                {
                    double centerX = xOffset;
                    double centerY = yOffset;

                    RotateTransform rotateTrans = new RotateTransform();
                    rotateTrans.CenterX = centerX; 
                    rotateTrans.CenterY = centerY; 
                    rotateTrans.Angle = -rotateAngle;
                    transGroup.Children.Add(rotateTrans);
                }
                _drawingCanvas.Children.Add(textBlock);
                textBlock.RenderTransform = transGroup;
            }
        }

      
        protected override DrawingRectangleF MeasureTextCore(string text, GeoFont font)
        {
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

            return new DrawingRectangleF(width / 2, height / 2, width, height);
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
            return (float)_drawingCanvas.ActualWidth;
        }

      
        protected override float GetCanvasHeightCore(object nativeImage)
        {
            return (float)_drawingCanvas.ActualHeight;
        }

      
        public override Stream GetStreamFromGeoImage(GeoImage image)
        {
            WpfGeoImage wpfGeoImage = image as WpfGeoImage;
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

            return imageStream;
        }

     
        protected override void FlushCore() { }

        private static void ApplyGeoPenToShape(GeoPen geoPen, Shape shape)
        {
            if (geoPen.Brush != null)
            {
                shape.Stroke = StyleHelper.GetWpfBrushFromGeoBrush(geoPen.Brush);
            }
            GeoColor color = geoPen.Color;
            shape.Stroke = new SolidColorBrush(Color.FromArgb(color.AlphaComponent, color.RedComponent, color.GreenComponent, color.BlueComponent));
            shape.StrokeDashCap = StyleHelper.GetPenLineCapFromGeoDashCap(geoPen.DashCap);
            shape.StrokeLineJoin = StyleHelper.GetLineJoinFromGeoLineJoin(geoPen.LineJoin);
            shape.StrokeDashArray = ConvertToDoubleCollection(geoPen.DashPattern);
            shape.StrokeMiterLimit = geoPen.MiterLimit;
            shape.StrokeThickness = geoPen.Width;
        }

        private static DoubleCollection ConvertToDoubleCollection(Collection<float> collection)
        {
            DoubleCollection doubleCollection = new DoubleCollection();
            foreach (float item in collection)
            {
                doubleCollection.Add((double)item);
            }
            return doubleCollection;
        }

        private static TextBlock GetDrawString(string text, GeoFont font, BaseGeoBrush fillBrush, Point position)
        {
            TextBlock textBlock = new TextBlock();
            textBlock.Text = text;
            textBlock.FontFamily = new FontFamily(font.FontName);
            textBlock.Foreground = StyleHelper.GetWpfBrushFromGeoBrush(fillBrush);
            textBlock.SetValue(System.Windows.Controls.Canvas.LeftProperty, position.X);
            textBlock.SetValue(System.Windows.Controls.Canvas.TopProperty, position.Y);
            return textBlock;
        }

        private static Image GetImage(GeoImage geoImage)
        {
            BitmapImage imageSource = new BitmapImage();
            Image image = new Image();
            image.Stretch = Stretch.Fill;

            WpfGeoImage wpfGeoImage = geoImage as WpfGeoImage;
            if (wpfGeoImage != null)
            {
                if (wpfGeoImage.WidthInPixel != .0)
                {
                    image.Width = wpfGeoImage.WidthInPixel;
                }

                if (wpfGeoImage.HeightInPixel != .0)
                {
                    image.Height = wpfGeoImage.HeightInPixel;
                }

                if (wpfGeoImage.SourceStream != null)
                {
                    imageSource.BeginInit();
                    imageSource.StreamSource = wpfGeoImage.SourceStream;
                    imageSource.EndInit();
                    image.Source = imageSource;
                    return image;
                }
                else if (wpfGeoImage.ImageUri != null)
                {
                    imageSource.BeginInit();
                    imageSource.UriSource = wpfGeoImage.ImageUri;
                    imageSource.EndInit();
                    return image;
                }
            }

            if (!String.IsNullOrEmpty(geoImage.PathFilename) && File.Exists(geoImage.PathFilename))
            {
                using (MemoryStream ms = new MemoryStream(File.ReadAllBytes(geoImage.PathFilename)))
                {
                    imageSource.BeginInit();
                    imageSource.StreamSource = ms;
                    imageSource.EndInit();
                    image.Source = imageSource;
                    return image;
                }
            }

            return image;
        }
    }
}

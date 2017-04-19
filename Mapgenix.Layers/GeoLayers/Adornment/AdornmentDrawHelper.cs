using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Mapgenix.Canvas;
using Mapgenix.Layers.Properties;
using Mapgenix.Shapes;
using Mapgenix.Utils;
using Mapgenix.Styles;

namespace Mapgenix.Layers
{

    public static class AdornmentDrawHelper
    {
        public static void DrawAdornmentCore(BaseAdornmentLayer itSelf, BaseGeoCanvas canvas)
        {
            if (!itSelf.BackgroundMask.OutlinePen.Color.IsTransparent ||
                !itSelf.BackgroundMask.FillSolidBrush.Color.IsTransparent)
            {
                Validators.CheckParameterIsNotNull(canvas, "canvas");
                Validators.CheckGeoCanvasIsInDrawing(canvas.IsDrawing);

                ScreenPointF drawingLocation = itSelf.GetDrawingLocation(canvas, itSelf.Width, itSelf.Height);
                ScreenPointF[] screenPoints = new ScreenPointF[5];
                screenPoints[0] = drawingLocation;
                screenPoints[1] = new ScreenPointF(drawingLocation.X + itSelf.Width, drawingLocation.Y);
                screenPoints[2] = new ScreenPointF(drawingLocation.X + itSelf.Width, drawingLocation.Y + itSelf.Height);
                screenPoints[3] = new ScreenPointF(drawingLocation.X, drawingLocation.Y + itSelf.Height);
                screenPoints[4] = new ScreenPointF(drawingLocation.X, drawingLocation.Y);

                Collection<ScreenPointF[]> screenPointsF = new Collection<ScreenPointF[]>();
                screenPointsF.Add(screenPoints);

                canvas.DrawArea(screenPointsF, itSelf.BackgroundMask.OutlinePen, itSelf.BackgroundMask.FillSolidBrush,
                    DrawingLevel.LevelOne, 0, 0, PenBrushDrawingOrder.BrushFirst);
            }

        }

        public static void DrawLegendAdornmentLayer(LegendAdornmentLayer itSelf, BaseGeoCanvas canvas,
            Collection<SimpleCandidate> labelsInAllLayers)
        {
            GeoImage geoImage = new GeoImage((int) itSelf.Width, (int) itSelf.Height);
            GdiPlusGeoCanvas adornmentGeoCanvas = new GdiPlusGeoCanvas();

            adornmentGeoCanvas.BeginDrawing(geoImage, canvas.CurrentWorldExtent, canvas.MapUnit);
            DrawingRectangleF backgroudExtent = new DrawingRectangleF(adornmentGeoCanvas.Width/2,
                adornmentGeoCanvas.Height/2, adornmentGeoCanvas.Width, adornmentGeoCanvas.Height);
            itSelf.BackgroundMask.DrawSample(adornmentGeoCanvas, backgroudExtent);

            itSelf.CountTheOffsetOfEachLegendItem();

            if (itSelf.Title != null)
            {
                itSelf.DrawTitle(labelsInAllLayers, adornmentGeoCanvas);
            }

            int countPostion = 0;
            foreach (LegendItem legendItem in itSelf.LegendItems)
            {
                itSelf.DrawEachLegendItem(labelsInAllLayers, adornmentGeoCanvas, countPostion, legendItem);
                countPostion++;
            }

            if (itSelf.Footer != null)
            {
                itSelf.DrawFooter(labelsInAllLayers, adornmentGeoCanvas);
            }

            adornmentGeoCanvas.EndDrawing();

            ScreenPointF drawingLocation = itSelf.GetDrawingLocation(canvas, (int) itSelf.Width, (int) itSelf.Height);
            canvas.DrawScreenImageWithoutScaling(geoImage, (drawingLocation.X + itSelf.Width/2),
                (drawingLocation.Y + itSelf.Height/2), DrawingLevel.LabelLevel, 0, 0, 0);
        }

        public static void DrawLogoAdornmentLayer(LogoAdornmentLayer itSelf, BaseGeoCanvas canvas,
            Collection<SimpleCandidate> labelsInAllLayers)
        {
            Validators.CheckParameterIsNotNull(canvas, "canvas");
            Validators.CheckGeoCanvasIsInDrawing(canvas.IsDrawing);
            Validators.CheckParameterIsNotNull(labelsInAllLayers, "labeledInLayers");

            if (itSelf.IsVisible)
            {
                if (itSelf.Image != null)
                {
                    int imageWidth = itSelf.Image.GetWidth();
                    int imageHeight = itSelf.Image.GetHeight();
                    ScreenPointF screenPotinF = itSelf.GetDrawingLocation(canvas, imageWidth, imageHeight);
                    canvas.DrawScreenImageWithoutScaling(itSelf.Image, screenPotinF.X + imageWidth*0.5f,
                        screenPotinF.Y + imageHeight*0.5f, DrawingLevel.LevelOne, 0, 0, 0);
                }
                else
                {
                    Bitmap logoBitmap = Resources.MapgenixLogo;
                    itSelf.Location = AdornmentLocation.LowerRight;
                    ScreenPointF screenPointF = itSelf.GetDrawingLocation(canvas, logoBitmap.Width, logoBitmap.Height);
                    GdiPlusGeoCanvas gdiPlusGeoCanvas = canvas as GdiPlusGeoCanvas;
                    if (gdiPlusGeoCanvas != null)
                    {
                        gdiPlusGeoCanvas.DrawScreenImageWithoutScaling(logoBitmap,
                            screenPointF.X + logoBitmap.Width*0.5f, screenPointF.Y + logoBitmap.Height*0.5f,
                            DrawingLevel.LevelOne, 0, 0, 0);
                    }
                    else
                    {
                        Stream stream = new MemoryStream();
                        logoBitmap.Save(stream, ImageFormat.Tiff);
                        GeoImage geoImage = new GeoImage(stream);

                        canvas.DrawScreenImageWithoutScaling(geoImage, screenPointF.X + logoBitmap.Width*0.5f,
                            screenPointF.Y + logoBitmap.Height*0.5f, DrawingLevel.LevelOne, 0, 0, 0);
                    }
                }
            }
        }

        public static void DrawRestrictionAdornmentLayer(RestrictionAdornmentLayer itSelf, BaseGeoCanvas canvas,
            Collection<SimpleCandidate> labelsInAllLayers)
        {
            ScreenPointF screenPosition = itSelf.GetDrawingLocation(canvas, itSelf.Width, itSelf.Height);
            PointShape upperLeftPoint = ExtentHelper.ToWorldCoordinate(canvas.CurrentWorldExtent,
                new ScreenPointF(screenPosition.X, screenPosition.Y), canvas.Width, canvas.Height);
            PointShape lowerRightPoint = ExtentHelper.ToWorldCoordinate(canvas.CurrentWorldExtent,
                new ScreenPointF(screenPosition.X + (itSelf.Width), screenPosition.Y + (itSelf.Height)), canvas.Width,
                canvas.Height);

            RectangleShape mapZone = new RectangleShape(upperLeftPoint, lowerRightPoint);
            BaseAreaShape maskZone = canvas.CurrentWorldExtent.GetDifference(mapZone);

            if (maskZone != null)
            {
                Feature feature = new Feature(maskZone);
                DrawingQuality tempDrawingQuality = canvas.DrawingQuality;
                canvas.DrawingQuality = DrawingQuality.HighSpeed;

                if (itSelf.RestrictionAreaStyle != null)
                {
                    itSelf.RestrictionAreaStyle.Draw(new[] {feature}, canvas, new SafeCollection<SimpleCandidate>(),
                        new SafeCollection<SimpleCandidate>());
                }

                canvas.DrawingQuality = tempDrawingQuality;
            }

        }

        public static void DrawScaleLineAdornmentLayer(ScaleLineAdornmentLayer itSelf, BaseGeoCanvas canvas, 
            Collection<SimpleCandidate> labelsInAllLayers)
        {
            Validators.CheckParameterIsNotNull(canvas, "canvas");
            Validators.CheckGeoCanvasIsInDrawing(canvas.IsDrawing);
            Validators.CheckParameterIsNotNull(labelsInAllLayers, "labeledInLayers");

            if (itSelf.IsVisible)
            {
                bool scaleLineValid = itSelf.SetBarItems(canvas, canvas.MapUnit);

                if (scaleLineValid)
                {
                    itSelf.DrawScaleLineImage(canvas);
                }
            }
        }

        public static void DrawGraticuleWithoutProjection(GraticuleAdornmentLayer itSelf, BaseGeoCanvas canvas)
        {
            RectangleShape currentExtent = canvas.CurrentWorldExtent;
            double currentMinX = currentExtent.UpperLeftPoint.X;
            double currentMaxX = currentExtent.UpperRightPoint.X;
            double currentMaxY = currentExtent.UpperLeftPoint.Y;
            double currentMinY = currentExtent.LowerLeftPoint.Y;

            double increment = itSelf.GetIncrement(currentExtent.Width, itSelf.GraticuleDensity);

            Collection<GraticuleLabel> meridianGraticuleLabels = new Collection<GraticuleLabel>();
            Collection<GraticuleLabel> parallelGraticuleLabels = new Collection<GraticuleLabel>();

            Collection<BaseShape> lineShapeMeridians = new Collection<BaseShape>();
            for (double x = itSelf.CeilingNumber(currentExtent.UpperLeftPoint.X, increment); x <= currentExtent.UpperRightPoint.X; x += increment)
            {
                LineShape lineShapeMeridian = new LineShape();
                lineShapeMeridian.Vertices.Add(new Vertex(x, currentMaxY));
                lineShapeMeridian.Vertices.Add(new Vertex(x, currentMinY));

                lineShapeMeridians.Add(lineShapeMeridian);
                double valueForWrapping = x;
               
                if (x > 180)
                        valueForWrapping = Math.IEEERemainder(x, 360);
                if (x < -180)
                        valueForWrapping = Math.IEEERemainder(x, -360);
                
                ScreenPointF meridianLabelPosition = ExtentHelper.ToScreenCoordinate(canvas.CurrentWorldExtent, x, currentMaxY, canvas.Width, canvas.Height);
                meridianGraticuleLabels.Add(new GraticuleLabel(itSelf.FormatLatLong(valueForWrapping, LineType.Meridian, increment), meridianLabelPosition));
            }
            itSelf.GraticuleLineStyle.Draw(lineShapeMeridians, canvas, new Collection<SimpleCandidate>(), new Collection<SimpleCandidate>());

            Collection<BaseShape> lineShapeParallels = new Collection<BaseShape>();

            for (double y = itSelf.CeilingNumber(currentExtent.LowerLeftPoint.Y, increment); y <= currentExtent.UpperRightPoint.Y; y += increment)
            {
                LineShape lineShapeParallel = new LineShape();
                lineShapeParallel.Vertices.Add(new Vertex(currentMaxX, y));
                lineShapeParallel.Vertices.Add(new Vertex(currentMinX, y));
                lineShapeParallels.Add(lineShapeParallel);

                ScreenPointF parallelLabelPosition = ExtentHelper.ToScreenCoordinate(canvas.CurrentWorldExtent, currentMinX, y, canvas.Width, canvas.Height);

                double valueForWrapping = y;
                if (y > 180)
                    valueForWrapping = Math.IEEERemainder(y, 180);
                if (y < -180)
                    valueForWrapping = Math.IEEERemainder(y, -180);


                parallelGraticuleLabels.Add(new GraticuleLabel(itSelf.FormatLatLong(valueForWrapping, LineType.Parallel, increment), parallelLabelPosition));
            }
            itSelf.GraticuleLineStyle.Draw(lineShapeParallels, canvas, new Collection<SimpleCandidate>(), new Collection<SimpleCandidate>());

            foreach (GraticuleLabel meridianGraticuleLabel in meridianGraticuleLabels)
            {
                Collection<ScreenPointF> locations = new Collection<ScreenPointF>
                {
                    new ScreenPointF(meridianGraticuleLabel.Location.X, meridianGraticuleLabel.Location.Y + 6)
                };

                canvas.DrawText(meridianGraticuleLabel.Label, itSelf.GraticuleTextFont, itSelf.GraticuleTextBrush,
                                new GeoPen(GeoColor.StandardColors.White, 2), locations, DrawingLevel.LevelFour, 8, 0, 0);
            }

            foreach (GraticuleLabel parallelGraticuleLabel in parallelGraticuleLabels)
            {
                Collection<ScreenPointF> locations = new Collection<ScreenPointF>
                {
                    new ScreenPointF(parallelGraticuleLabel.Location.X, parallelGraticuleLabel.Location.Y)
                };

                canvas.DrawText(parallelGraticuleLabel.Label, itSelf.GraticuleTextFont, itSelf.GraticuleTextBrush,
                                new GeoPen(GeoColor.StandardColors.White, 2), locations, DrawingLevel.LevelFour, 8, 0, 90);
            }
        
        }

        public static void DrawGraticuleWithProjection(GraticuleAdornmentLayer itSelf, BaseGeoCanvas canvas)
        {
            double currentMinX;
            double currentMaxX;
            double currentMaxY;
            double currentMinY;
            if (itSelf.WrappingMode == WrappingMode.WrapDateline)
            {
                Debug.WriteLine("Canvas extent : " + canvas.CurrentWorldExtent);
                RectangleShape extentAfterWrapping = itSelf.GetDrawingExtentForWrapping(canvas);
                Debug.WriteLine("extentAfterWrapping extent : " + extentAfterWrapping);
                currentMaxX = extentAfterWrapping.LowerRightPoint.X;
                currentMaxY = extentAfterWrapping.UpperRightPoint.Y;
                currentMinX = extentAfterWrapping.LowerLeftPoint.X;
                currentMinY = extentAfterWrapping.LowerLeftPoint.Y;
            }
            else
            {
                ExtremesLatLong extremesLatLong = itSelf.GetExtremesLatLong(canvas);

                currentMinX = extremesLatLong.MinLong;
                currentMaxX = extremesLatLong.MaxLong;
                currentMaxY = extremesLatLong.MaxLat;
                currentMinY = extremesLatLong.MinLat;
            }

            double increment = itSelf.GetIncrement((currentMaxX - currentMinX), itSelf.GraticuleDensity);
            Collection<GraticuleLabel> meridianGraticuleLabels = new Collection<GraticuleLabel>();
            Collection<GraticuleLabel> parallelGraticuleLabels = new Collection<GraticuleLabel>();

            double dividor = 100;
            Collection<BaseShape> lineShapeMeridians = new Collection<BaseShape>();
            double xRight = itSelf.CeilingNumber(currentMaxX, increment);
            for (double x = itSelf.FloorNumber(currentMinX, increment); x <= xRight; x += increment)
            {
                LineShape lineShapeMeridian = new LineShape();
                double yInt;
                double iIncr = (itSelf.CeilingNumber(currentMaxY, increment) - itSelf.FloorNumber(currentMinY, increment)) / dividor;
                for (yInt = itSelf.FloorNumber(currentMinY, increment); yInt <= itSelf.CeilingNumber(currentMaxY, increment); yInt += iIncr)
                {
                    if (x <= -180 || x >= 180)
                    {
                        lineShapeMeridian.Vertices.Add(itSelf.CalculateOriginalVertexWithProjection(x, yInt, x < -180));
                    }
                    else
                    {
                        Vertex projVertex = itSelf.Projection.ConvertToExternalProjection(x, yInt);
                        lineShapeMeridian.Vertices.Add(projVertex);
                    }
                }

                if (x <= -180 || x >= 180)
                {
                    lineShapeMeridian.Vertices.Add(itSelf.CalculateOriginalVertexWithProjection(currentMaxX, itSelf.CeilingNumber(currentMaxY, increment), x < -180));
                }
                else
                {
                    Vertex projVertex2 = itSelf.Projection.ConvertToExternalProjection(x, itSelf.CeilingNumber(currentMaxY, increment));
                    lineShapeMeridian.Vertices.Add(projVertex2);
                }

                lineShapeMeridians.Add(lineShapeMeridian);

                LineShape topLineShape = new LineShape();
                topLineShape.Vertices.Add(new Vertex(canvas.CurrentWorldExtent.UpperLeftPoint.X, canvas.CurrentWorldExtent.UpperLeftPoint.Y));
                topLineShape.Vertices.Add(new Vertex(canvas.CurrentWorldExtent.UpperRightPoint.X, canvas.CurrentWorldExtent.UpperRightPoint.Y));

                MultipointShape multiPointShape = lineShapeMeridian.GetCrossing(topLineShape);
                if ((multiPointShape.Points.Count > 0))
                {
                    double xAfterWrapping;
                    if ((x >= -180) && (x <= 180))
                    {
                        xAfterWrapping = x;
                    }
                    else
                    {
                        xAfterWrapping = x > 0 ? Math.IEEERemainder(x, 360) : Math.IEEERemainder(x, -360);
                    }
                    ScreenPointF meridianLabelPosition = ExtentHelper.ToScreenCoordinate(canvas.CurrentWorldExtent, multiPointShape.Points[0].X, multiPointShape.Points[0].Y, canvas.Width, canvas.Height);
                    meridianGraticuleLabels.Add(new GraticuleLabel(itSelf.FormatLatLong(xAfterWrapping, LineType.Meridian, increment), meridianLabelPosition));
                }
            }
            itSelf.GraticuleLineStyle.Draw(lineShapeMeridians, canvas, new Collection<SimpleCandidate>(), new Collection<SimpleCandidate>());

            Collection<BaseShape> lineShapeParallels = new Collection<BaseShape>();
            for (double y = itSelf.FloorNumber(currentMinY, increment); y <= itSelf.CeilingNumber(currentMaxY, increment); y += increment)
            {
                LineShape lineShapeParallel = new LineShape();
                double xInt;
                double iIncr = (itSelf.CeilingNumber(currentMaxX, increment) - itSelf.FloorNumber(currentMinX, increment)) / dividor;
                for (xInt = itSelf.FloorNumber(currentMinX, increment); xInt <= itSelf.CeilingNumber(currentMaxX, increment); xInt += iIncr)
                {
                    if (xInt < -180 || xInt > 180)
                    {
                        lineShapeParallel.Vertices.Add(itSelf.CalculateOriginalVertexWithProjection(xInt, y, xInt < -180));
                    }
                    else
                    {
                        Vertex projVertex = itSelf.Projection.ConvertToExternalProjection(xInt, y);
                        lineShapeParallel.Vertices.Add(projVertex);
                    }
                }
                lineShapeParallels.Add(lineShapeParallel);

                LineShape leftLineShape = new LineShape();
                leftLineShape.Vertices.Add(new Vertex(canvas.CurrentWorldExtent.UpperLeftPoint.X, canvas.CurrentWorldExtent.UpperLeftPoint.Y));
                leftLineShape.Vertices.Add(new Vertex(canvas.CurrentWorldExtent.LowerLeftPoint.X, canvas.CurrentWorldExtent.LowerLeftPoint.Y));

                MultipointShape multiPointShape = lineShapeParallel.GetCrossing(leftLineShape);

                if ((multiPointShape.Points.Count > 0))
                {
                    ScreenPointF parallelLabelPosition = ExtentHelper.ToScreenCoordinate(canvas.CurrentWorldExtent, multiPointShape.Points[0].X, multiPointShape.Points[0].Y, canvas.Width, canvas.Height);
                    parallelGraticuleLabels.Add(new GraticuleLabel(itSelf.FormatLatLong(y, LineType.Parallel, increment), parallelLabelPosition));
                }
            }
            itSelf.GraticuleLineStyle.Draw(lineShapeParallels, canvas, new Collection<SimpleCandidate>(), new Collection<SimpleCandidate>());

            foreach (GraticuleLabel meridianGraticuleLabel in meridianGraticuleLabels)
            {
                Collection<ScreenPointF> locations = new Collection<ScreenPointF>
                {
                    new ScreenPointF(meridianGraticuleLabel.Location.X, meridianGraticuleLabel.Location.Y + 6)
                };

                canvas.DrawText(meridianGraticuleLabel.Label, itSelf.GraticuleTextFont, itSelf.GraticuleTextBrush,
                                new GeoPen(GeoColor.StandardColors.White, 2), locations, DrawingLevel.LevelFour, 8, 0, 0);
            }

            foreach (GraticuleLabel parallelGraticuleLabel in parallelGraticuleLabels)
            {
                Collection<ScreenPointF> locations = new Collection<ScreenPointF>
                {
                    new ScreenPointF(parallelGraticuleLabel.Location.X, parallelGraticuleLabel.Location.Y)
                };

                canvas.DrawText(parallelGraticuleLabel.Label, itSelf.GraticuleTextFont, itSelf.GraticuleTextBrush,
                                new GeoPen(GeoColor.StandardColors.White, 2), locations, DrawingLevel.LevelFour, 8, 0, 90);
            }
        }
    }
}



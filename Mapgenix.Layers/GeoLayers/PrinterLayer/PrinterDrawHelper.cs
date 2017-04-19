using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using Mapgenix.Canvas;
using Mapgenix.Shapes;


namespace Mapgenix.Layers
{
    public static class PrinterDrawHelper
    {
        public static void DrawPrinterCore(BasePrinterLayer itSelf, BaseGeoCanvas canvas, Collection<SimpleCandidate> labelsInAllLayers)
        {
            itSelf.BackgroundMask.Draw(new BaseShape[] { itSelf.GetBoundBoxCore() }, canvas, new Collection<SimpleCandidate>(), labelsInAllLayers);
        }

        public static void DrawCompassPrintersCore(CompassPrinterLayer itSelf, BaseGeoCanvas canvas)
        {
            RectangleShape currentBoundingBox = GetBoundingBox(itSelf);

            double oneToOneScale = PrinterHelper.GetGeographyUnitToPointRatio(canvas.MapUnit);
            float widthInScreen = (float)(currentBoundingBox.Width * oneToOneScale / canvas.CurrentScale);
            float heightInScreen = (float)(currentBoundingBox.Height * oneToOneScale / canvas.CurrentScale);

            canvas.DrawWorldImage(itSelf.NeedleImage, currentBoundingBox.GetCenterPoint().X, currentBoundingBox.GetCenterPoint().Y, widthInScreen, heightInScreen, DrawingLevel.LevelOne, 0, 0, itSelf.RotateAngle);

            if (itSelf.FrameImage != null)
            {
                RectangleShape box = GetBoundingBox(itSelf);
                canvas.DrawWorldImage(itSelf.FrameImage, box.GetCenterPoint().X, box.GetCenterPoint().Y, widthInScreen, heightInScreen, DrawingLevel.LevelOne, 0, 0, itSelf.RotateAngle);
            }
        }

        public static void DrawGridPrinterCore(DataGridPrinterLayer itSelf, BaseGeoCanvas canvas)
        {

            DataTable drawingData = itSelf.GetDataWithTitle();

            RectangleShape currentBoundingBox = GetBoundingBox(itSelf);
            double tableWidth = currentBoundingBox.Width;
            double tableHeight = currentBoundingBox.Height;

            double cellWidth = tableWidth / drawingData.Columns.Count;
            double cellHeight = tableHeight / drawingData.Rows.Count;

            double oneToOneScale = PrinterHelper.GetGeographyUnitToPointRatio(canvas.MapUnit);

            MultilineShape lines = itSelf.GetCellBorder(drawingData, cellWidth, cellHeight);
            float penWidth = (float)(itSelf.CellBorderPen.Width * oneToOneScale / canvas.CurrentScale);
            GeoPen drawingBorderPen = new GeoPen(itSelf.CellBorderPen.Brush, penWidth);
            canvas.DrawLine(lines, drawingBorderPen, DrawingLevel.LevelOne);

            Dictionary<RectangleShape, string> cells = itSelf.GetCells(drawingData, cellWidth, cellHeight);
            float drawingSize = (float)(itSelf.TextFont.Size * oneToOneScale / canvas.CurrentScale);
            GeoFont drawingFont = new GeoFont(itSelf.TextFont.FontName, drawingSize, itSelf.TextFont.Style);
            foreach (KeyValuePair<RectangleShape, string> cell in cells)
            {
                string drawingCellText = cell.Value;
                if (drawingCellText.Trim() != "")
                {
                    DrawingRectangleF drawingTextRectInScreen = canvas.MeasureText(drawingCellText, drawingFont);
                    float cellWidthInScreen = ExtentHelper.GetScreenDistanceBetweenTwoWorldPoints(canvas.CurrentWorldExtent, cell.Key.UpperLeftPoint, cell.Key.UpperRightPoint, canvas.Width, canvas.Height);
                    if (drawingTextRectInScreen.Width > cellWidthInScreen)
                    {
                        double scaledStringLength = cellWidthInScreen / drawingTextRectInScreen.Width * cell.Value.Length;
                        drawingCellText = itSelf.TruncateString(cell.Value, scaledStringLength);
                    }
                    if (drawingCellText != "")
                    {
                        canvas.DrawTextWithWorldCoordinate(drawingCellText, drawingFont, itSelf.FontBrush, cell.Key.GetCenterPoint().X, cell.Key.GetCenterPoint().Y, DrawingLevel.LevelOne);
                    }
                }
            }
        }

        public static void DrawImagePrinterCore(ImagePrinterLayer itSelf, BaseGeoCanvas canvas)
        {
            double oneToOneScale = PrinterHelper.GetGeographyUnitToPointRatio(canvas.MapUnit);
            RectangleShape currentBoundingBox = GetBoundingBox(itSelf);

            float widthInScreen = (float)(currentBoundingBox.Width * oneToOneScale / canvas.CurrentScale);
            float heightInScreen = (float)(currentBoundingBox.Height * oneToOneScale / canvas.CurrentScale);

            canvas.DrawWorldImage(itSelf.Image, currentBoundingBox.GetCenterPoint().X, currentBoundingBox.GetCenterPoint().Y, widthInScreen, heightInScreen, DrawingLevel.LevelOne, 0, 0, 0);
 
        }

        public static void DrawLabelPrinterCore(LabelPrinterLayer itSelf, BaseGeoCanvas canvas)
        {
            double oneToOneScale = PrinterHelper.GetGeographyUnitToPointRatio(canvas.MapUnit);
            RectangleShape currentBoundingBox = GetBoundingBox(itSelf);

            string drawingText = itSelf.Text;
            if (itSelf.PrinterWrapMode == PrinterWrapMode.WrapText)
            {
                float drawingSize = (float)(itSelf.Font.Size * oneToOneScale / canvas.CurrentScale);
                itSelf.DrawingFont = new GeoFont(itSelf.Font.FontName, drawingSize, itSelf.Font.Style);

                drawingText = itSelf.WrapText(canvas, currentBoundingBox, itSelf.DrawingFont, itSelf.Text);
            }
            else
            {
                if (itSelf.LastBoundingBox == null)
                {
                    float drawingSize = itSelf.Font.Size;
                    if (currentBoundingBox.GetWellKnownText() != new RectangleShape().GetWellKnownText())
                    {
                        drawingSize = itSelf.GetFontSizeByBoundingBox(canvas, itSelf.Font, drawingText, currentBoundingBox);
                    }
                    itSelf.DrawingFont = new GeoFont(itSelf.Font.FontName, drawingSize, itSelf.Font.Style);

                    drawingSize = (float)(drawingSize / oneToOneScale * canvas.CurrentScale);
                    itSelf.Font = new GeoFont(itSelf.Font.FontName, drawingSize, itSelf.Font.Style);
                }
                else
                {
                    if ((Math.Round(itSelf.LastBoundingBox.Width, 8) != Math.Round(currentBoundingBox.Width, 8))
                        || (Math.Round(itSelf.LastBoundingBox.Height, 8) != Math.Round(currentBoundingBox.Height, 8)))  
                    {
                        float drawingSize = itSelf.GetFontSizeByBoundingBox(canvas, itSelf.DrawingFont, drawingText, currentBoundingBox);
                        itSelf.DrawingFont = new GeoFont(itSelf.Font.FontName, drawingSize, itSelf.Font.Style);

                        drawingSize = (float)(drawingSize / oneToOneScale * canvas.CurrentScale);
                        itSelf.Font = new GeoFont(itSelf.Font.FontName, drawingSize, itSelf.Font.Style);
                    }
                    else
                    {
                        float drawingSize = (float)(itSelf.Font.Size * oneToOneScale / canvas.CurrentScale);
                        itSelf.DrawingFont = new GeoFont(itSelf.Font.FontName, drawingSize, itSelf.Font.Style);
                    }
                }

                itSelf.LastBoundingBox = currentBoundingBox;
            }

            canvas.DrawTextWithWorldCoordinate(drawingText, itSelf.DrawingFont, itSelf.FontBrush, currentBoundingBox.GetCenterPoint().X, currentBoundingBox.GetCenterPoint().Y, DrawingLevel.LevelOne);

        }

        public static void DrawMapPrinterCore(MapPrinterLayer itSelf, BaseGeoCanvas canvas, Collection<SimpleCandidate> labelsInAllLayers)
        {
            RectangleShape currentBoundingBox = GetBoundingBox(itSelf);

            if (itSelf.LastMapExtent != null)
            {
                if (itSelf.LastMapExtent.UpperLeftPoint.X != itSelf.LastMapExtent.UpperLeftPoint.X
                || itSelf.LastMapExtent.UpperLeftPoint.Y != itSelf.LastMapExtent.UpperLeftPoint.Y
                || itSelf.LastMapExtent.LowerRightPoint.X != itSelf.LastMapExtent.LowerRightPoint.X
                || itSelf.LastMapExtent.LowerRightPoint.Y != itSelf.LastMapExtent.LowerRightPoint.Y)
                {
                    itSelf.MapImageCache = null;
                }
            }

            RectangleShape adjustedWorldExtent = ExtentHelper.GetDrawingExtent(itSelf.MapExtent, (float)currentBoundingBox.Width, (float)currentBoundingBox.Height);
            PageGeoCanvas pageGeoCanvas = new PageGeoCanvas(new RectangleShape(0, GetBoundingBox(itSelf).Height, GetBoundingBox(itSelf).Width, 0), currentBoundingBox);

            if (itSelf.IsDrawing)
            {
                pageGeoCanvas.BeginDrawing(canvas, adjustedWorldExtent, GeographyUnit.Feet);
                pageGeoCanvas.EnableCliping = true;
                pageGeoCanvas.ClipingArea = adjustedWorldExtent;
                foreach (BaseLayer layer in itSelf.Layers)
                {
                    layer.Open();
                    layer.Draw(pageGeoCanvas, labelsInAllLayers);
                    layer.Close();
                }
                pageGeoCanvas.EnableCliping = false;;
            }
            else
            {
                GeoImage image = itSelf.GetCacheImage(pageGeoCanvas, canvas.MapUnit, adjustedWorldExtent, labelsInAllLayers);

                pageGeoCanvas.BeginDrawing(canvas, adjustedWorldExtent, GeographyUnit.Feet);
                pageGeoCanvas.DrawWorldImage(image, adjustedWorldExtent.GetCenterPoint().X, adjustedWorldExtent.GetCenterPoint().Y, (float)GetBoundingBox(itSelf).Width, (float)GetBoundingBox(itSelf).Height, DrawingLevel.LevelOne);
                pageGeoCanvas.EndDrawing();
            }

            itSelf.LastMapExtent = new RectangleShape(itSelf.MapExtent.UpperLeftPoint.X, itSelf.MapExtent.UpperLeftPoint.Y, itSelf.MapExtent.LowerRightPoint.X, itSelf.MapExtent.LowerRightPoint.Y);
        }

        public static void DrawScaleBarPrinterLayer(ScaleBarPrinterLayer itSelf, BaseGeoCanvas canvas)
        {
           
            itSelf.MaxWidth = (int)GetBoundingBox(itSelf).Width;
       

            double oneToOneScale = PrinterHelper.GetGeographyUnitToPointRatio(canvas.MapUnit);
            itSelf.ZoomRadio = oneToOneScale / canvas.CurrentScale;
            itSelf.ScreenLocation = ExtentHelper.ToScreenCoordinate(canvas.CurrentWorldExtent, GetBoundingBox(itSelf).UpperLeftPoint, canvas.Width, canvas.Height);

            RectangleShape mapExtentInWorld = itSelf.MapPrinterLayer.MapExtent;
            RectangleShape mapExtentInScreen = itSelf.MapPrinterLayer.GetBoundingBox();

            double mapExtentInWorldCenterY = (mapExtentInWorld.UpperLeftPoint.Y + mapExtentInWorld.LowerLeftPoint.Y) / 2;

            ScreenPointF barReferenceLeftPointInScreen = ExtentHelper.ToScreenCoordinate(mapExtentInWorld, new PointShape(mapExtentInWorld.LowerLeftPoint.X, mapExtentInWorldCenterY), (float)mapExtentInScreen.Width, (float)mapExtentInScreen.Height);
            ScreenPointF barReferenceRightPointInScreen = new ScreenPointF(barReferenceLeftPointInScreen.X + itSelf.Width, barReferenceLeftPointInScreen.Y);

            double barLengthInUnit = 0;
            DistanceUnit distanceUnit = DistanceUnit.Kilometer;
            PointShape barReferenceLeftPointInWorld = ExtentHelper.ToWorldCoordinate(mapExtentInWorld, barReferenceLeftPointInScreen, (float)mapExtentInScreen.Width, (float)mapExtentInScreen.Height);
            PointShape barReferenceRightointInWorld = ExtentHelper.ToWorldCoordinate(mapExtentInWorld, barReferenceRightPointInScreen, (float)mapExtentInScreen.Width, (float)mapExtentInScreen.Height);
            if (itSelf.MapUnit == GeographyUnit.DecimalDegree)
            {
                barLengthInUnit = DecimalDegreesHelper.GetDistanceFromDecimalDegrees(barReferenceLeftPointInWorld, barReferenceRightointInWorld, distanceUnit);
            }
            else
            {
                distanceUnit = itSelf.ConvertGeographyUnitToDistanceUnit(itSelf.MapUnit);
                barLengthInUnit = barReferenceLeftPointInWorld.X - barReferenceLeftPointInWorld.X;
            }

            FeetValues feetValues = new FeetValues();
            MilesValues milesValues = new MilesValues();
            MetersValues metersValues = new MetersValues();
            KiloMetersValues kiloMetersValues = new KiloMetersValues();

            switch (itSelf.UnitFamily)
            {
                case UnitSystem.Imperial:
                    feetValues = new FeetValues(itSelf.Width, barLengthInUnit, distanceUnit);
                    milesValues = new MilesValues(itSelf.Width, barLengthInUnit, distanceUnit);
                    break;

                case UnitSystem.Metric:
                    metersValues = new MetersValues(itSelf.Width, barLengthInUnit, distanceUnit);
                    kiloMetersValues = new KiloMetersValues(itSelf.Width, barLengthInUnit, distanceUnit);
                    break;
            }

            bool refresh = true;
            if (feetValues.V10 > 350 | metersValues.V10 > 350)
            {
                refresh = false;
            }

            if (refresh)
            {
                if (string.IsNullOrEmpty(itSelf.NumberFormat)) { itSelf.NumberFormat = "###,###"; }
                Dictionary<double, double> relevantDict = null;
                string unitName = "";

         
                int margin = 0;
                switch (itSelf.UnitFamily)
                {
                    case UnitSystem.Imperial:
                        Dictionary<double, double> xFeetHashTable = itSelf.GetFeetPoints(itSelf.LowerX, itSelf.MaxWidth, feetValues);
                        double feetMax = itSelf.GetMaxValue(xFeetHashTable);

                        Dictionary<double, double> xMilesHashTable = itSelf.GetMilesPoints(itSelf.LowerX, itSelf.MaxWidth, milesValues);
                        double milesMax = itSelf.GetMaxValue(xMilesHashTable);

                        if (milesMax > feetMax) 
                        {
                            relevantDict = xMilesHashTable;
                            unitName = itSelf.DisplayUnitString["Mile"];
                        }
                        else
                        {
                            relevantDict = xFeetHashTable;
                            unitName = itSelf.DisplayUnitString["Feet"];
                        }
                        margin = 29;
                        break;

                    case UnitSystem.Metric:
                        Dictionary<double, double> xMetersHashTable = itSelf.GetMetersPoints(itSelf.LowerX, itSelf.MaxWidth, metersValues);
                        double metersMax = itSelf.GetMaxValue(xMetersHashTable);

                        Dictionary<double, double> xKiloMetersHashTable = itSelf.GetKiloMetersPoints(itSelf.LowerX, itSelf.MaxWidth, kiloMetersValues);
                        double kiloMetersMax = itSelf.GetMaxValue(xKiloMetersHashTable);

                        if (kiloMetersMax > metersMax)
                        {
                            relevantDict = xKiloMetersHashTable;
                            unitName = itSelf.DisplayUnitString["Kilometer"];
                            margin = 22;
                        }
                        else
                        {
                            relevantDict = xMetersHashTable;
                            unitName = itSelf.DisplayUnitString["Meter"];
                            margin = 37;
                        }
                        break;
                }

                float fontRatio = itSelf.GetScaledNumber(itSelf.Font.Size);


                Collection<RectangleShape> rectCollection = itSelf.GetRectangleShapeCollection(relevantDict, fontRatio);

                GeoFont drawingFont = itSelf.GetScaledFont(itSelf.Font);
                int drawingFontMargin = itSelf.GetScaledNumber(itSelf.FontMargin);
                int drawingYDent = itSelf.GetScaledNumber(itSelf.FontMargin);
                int drawingThickness = itSelf.GetScaledNumber(itSelf.Thickness);
                margin = itSelf.GetScaledNumber(margin);


                int yPosition = 25;
                ScreenPointF ptStart = new ScreenPointF(itSelf.GetScaledNumber(itSelf.LeftStart) + itSelf.ScreenLocation.X, itSelf.GetScaledNumber(yPosition) + itSelf.ScreenLocation.Y);

                if (itSelf.HasMask && rectCollection.Count > 1)
                {
                    DrawingRectangleF textDrawingRectangleF = canvas.MeasureText(unitName, drawingFont);
                    itSelf.DrawMask(canvas, textDrawingRectangleF.Width, margin, (int)fontRatio, rectCollection);
                }

                GeoPen pen = new GeoPen(GeoColor.StandardColors.Black, 1F);
                pen.Width = itSelf.GetScaledNumber(pen.Width);
                if (rectCollection.Count > 0)
                {
                    for (int i = 0; i < rectCollection.Count; i++)
                    {
                        RectangleShape rect = rectCollection[i];
                        ScreenPointF[] points = {
                                                        new ScreenPointF((float)rect.UpperLeftPoint.X, (float)rect.UpperLeftPoint.Y)
                                                        , new ScreenPointF((float)rect.UpperRightPoint.X, (float)rect.UpperRightPoint.Y)
                                                        , new ScreenPointF((float)rect.LowerRightPoint.X, (float)rect.LowerRightPoint.Y)
                                                        , new ScreenPointF((float)rect.LowerLeftPoint.X, (float)rect.LowerLeftPoint.Y)
                                                    };

                        canvas.DrawArea(new[] { points }, pen, itSelf.BarBrush, DrawingLevel.LevelOne, 0, 0, PenBrushDrawingOrder.BrushFirst);
                        if (itSelf.AlternateBarBrush != null)
                        {
                            float rectWidth = (float)(rect.UpperRightPoint.X - rect.UpperLeftPoint.X);
                            if (i == 0) continue;
                            if (i == 1)
                            {
                                points = new[]
                                {
                                    new ScreenPointF((float)rect.UpperRightPoint.X,(float)rect.UpperRightPoint.Y),
                                    new ScreenPointF((float)(rect.UpperRightPoint.X + 2 * rectWidth),(float)rect.UpperRightPoint.Y),
                                    new ScreenPointF((float)(rect.UpperRightPoint.X + 2 * rectWidth),(float)rect.LowerRightPoint.Y),
                                    new ScreenPointF((float)rect.LowerRightPoint.X,(float)rect.LowerRightPoint.Y)
                                };
                                canvas.DrawArea(new[] { points }, pen, itSelf.AlternateBarBrush, DrawingLevel.LevelOne, 0, 0, PenBrushDrawingOrder.BrushFirst);
                            }
                            else
                            {
                                points = new[]
                                {
                                    new ScreenPointF((float)rect.UpperRightPoint.X,(float)rect.UpperRightPoint.Y),
                                    new ScreenPointF((float)(rect.UpperRightPoint.X + rectWidth),(float)rect.UpperRightPoint.Y),
                                    new ScreenPointF((float)(rect.UpperRightPoint.X + rectWidth),(float)rect.LowerRightPoint.Y),
                                    new ScreenPointF((float)rect.LowerRightPoint.X,(float)rect.LowerRightPoint.Y)
                                };
                                canvas.DrawArea(new[] { points }, pen, itSelf.AlternateBarBrush, DrawingLevel.LevelOne, 0, 0, PenBrushDrawingOrder.BrushFirst);
                            }
                        }
                    }
                }

                canvas.DrawLine(new[] { 
                        new ScreenPointF(ptStart.X, itSelf.ScreenLocation.Y + drawingFont.Size + drawingFontMargin + drawingYDent + fontRatio),
                        new ScreenPointF(ptStart.X, itSelf.ScreenLocation.Y + drawingFont.Size + drawingFontMargin + fontRatio) }
                        , pen, DrawingLevel.LevelOne, 0, 0);
                canvas.DrawText("0", drawingFont, new GeoSolidBrush(itSelf.TextColor),
                    new[] { new ScreenPointF(ptStart.X, itSelf.ScreenLocation.Y + drawingFont.Size) }, DrawingLevel.LevelOne);

                ScreenPointF[] ptBar = new ScreenPointF[relevantDict.Count];
                ScreenPointF[] ptBar2 = new ScreenPointF[relevantDict.Count];
                float[] xValues = new float[relevantDict.Count];
                int j = 0;
                foreach (KeyValuePair<double, double> item in relevantDict)
                {
                    ptBar[j] = new ScreenPointF(
                        (float)(itSelf.GetScaledNumber(item.Key) + ptStart.X)
                        , itSelf.ScreenLocation.Y + drawingFont.Size + drawingFontMargin + drawingYDent + fontRatio + drawingThickness);
                    ptBar2[j] = new ScreenPointF(
                        (float)(itSelf.GetScaledNumber(item.Key) + ptStart.X)
                        , itSelf.ScreenLocation.Y + drawingFont.Size + drawingFontMargin + fontRatio);

                    xValues[j] = ptBar[j].X;

                    canvas.DrawLine(new[] { new ScreenPointF(ptBar[j].X, ptBar[j].Y), new ScreenPointF(ptBar2[j].X, ptBar2[j].Y) },
                        pen, DrawingLevel.LevelOne, 0, 0);
                    canvas.DrawText(item.Value.ToString(), drawingFont, new GeoSolidBrush(itSelf.TextColor),
                        new[] { new ScreenPointF(ptBar2[j].X, itSelf.ScreenLocation.Y + drawingFont.Size) }, DrawingLevel.LevelOne);

                    j++;
                }
                Array.Sort(xValues);
                if (j > 0)
                {
                    canvas.DrawLine(
                        new[] { new ScreenPointF(ptStart.X, itSelf.ScreenLocation.Y + drawingFont.Size + drawingFontMargin + drawingYDent + fontRatio), 
                                new ScreenPointF(xValues[relevantDict.Count - 1], itSelf.ScreenLocation.Y + drawingFont.Size + drawingFontMargin + drawingYDent + fontRatio) },
                            pen, DrawingLevel.LevelOne, 0, 0);
                    canvas.DrawLine(
                        new[] { new ScreenPointF(ptStart.X, itSelf.ScreenLocation.Y + drawingFont.Size + drawingFontMargin + drawingYDent + fontRatio + drawingThickness), 
                                new ScreenPointF(xValues[relevantDict.Count - 1], itSelf.ScreenLocation.Y + drawingFont.Size + drawingFontMargin + drawingYDent + fontRatio + drawingThickness) },
                            pen, DrawingLevel.LevelOne, 0, 0);
                    canvas.DrawText(unitName, drawingFont, new GeoSolidBrush(itSelf.TextColor),
                        new[] { new ScreenPointF(xValues[relevantDict.Count - 1] + itSelf.GetScaledNumber(18), itSelf.ScreenLocation.Y + drawingFont.Size + drawingFontMargin + drawingYDent + fontRatio + drawingThickness / 2) },
                        DrawingLevel.LevelOne);
                }
            }
        }

        public static void DrawScaleLinePrinterLayer(ScaleLinePrinterLayer itSelf, BaseGeoCanvas canvas)
        {
            if (GetBoundingBox(itSelf).Width > itSelf.ScaleLineWidthPixel)
            {
                itSelf.ScaleLineWidthPixel = (int)(GetBoundingBox(itSelf).Width * 2.5);
            }

            double oneToOneScale = PrinterHelper.GetGeographyUnitToPointRatio(canvas.MapUnit);

            bool scaleLineValid = itSelf.SetBarItems(itSelf.MapUnit);

            if (scaleLineValid)
            {
                double zoomRatio = oneToOneScale / canvas.CurrentScale;

                itSelf.DrawScaleLineImage(canvas, GetBoundingBox(itSelf), zoomRatio);
            }
        }

        static RectangleShape GetBoundingBox(BaseLayer self)
        {
            Validators.CheckLayerIsOpened(self.IsOpen);
            Validators.CheckLayerHasBoundingBox(self.HasBoundingBox);

            return new RectangleShape(-180, 90, 180, -90);
        }
    }
}

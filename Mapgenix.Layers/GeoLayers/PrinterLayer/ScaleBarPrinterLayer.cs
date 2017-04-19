using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using Mapgenix.Canvas;
using Mapgenix.Shapes;

namespace Mapgenix.Layers
{
    public class ScaleBarPrinterLayer : BasePrinterLayer
    {
        public int FontMargin = 5;
        public int LeftStart = 10;
        public int LowerX = 30;
        public int YDent = 4;

        Dictionary<string, string> _displayUnitString = new Dictionary<string,string>();

        public ScreenPointF ScreenLocation { get; set; }

        public double ZoomRadio { get; set; }

        public int Width { get; set; }

        public MapPrinterLayer MapPrinterLayer { get; set; }

        public GeographyUnit MapUnit { get; set; }

      
        public Dictionary<string, string> DisplayUnitString
        {
            get { return _displayUnitString; }
			set { _displayUnitString = value; }

        }

        public UnitSystem UnitFamily { get; set; }

        public string NumberFormat { get; set; }

        public GeoFont Font { get; set; }


        public GeoColor TextColor { get; set; }

        public int Thickness { get; set; }

        public int MaxWidth { get; set; }

        public BaseGeoBrush BarBrush { get; set; }

        public BaseGeoBrush AlternateBarBrush { get; set; }

        public BaseGeoBrush MaskBrush { get; set; }

        public bool HasMask { get; set; }

        public GeoPen MaskContour { get; set; }

        protected override void DrawCore(BaseGeoCanvas canvas, Collection<SimpleCandidate> labelsInAllLayers)
        {
            Validators.CheckParameterIsNotNull(canvas, "canvas");
            PrinterDrawHelper.DrawPrinterCore(this,canvas,labelsInAllLayers);
            PrinterDrawHelper.DrawScaleBarPrinterLayer(this,canvas);

        }

        public void DrawMask(BaseGeoCanvas canvas, float textWidth, int margin, int fontRatio, Collection<RectangleShape> rectCollection)
        {
            int index = 0;
            int tempWidth = 0;
            int x = 0;

            GeoFont drawingFont = GetScaledFont(Font);

            if (rectCollection.Count >= 2)
            {
                index = rectCollection.Count - 1;

                x = Convert.ToInt32(rectCollection[index].UpperLeftPoint.X, CultureInfo.InvariantCulture);
                if (rectCollection.Count == 2)
                {
                    tempWidth = Convert.ToInt32(rectCollection[index].Width * 3, CultureInfo.InvariantCulture);
                }
                else
                {
                    tempWidth = Convert.ToInt32(rectCollection[index].Width * 2, CultureInfo.InvariantCulture);
                }
            }

            int realWidth = Convert.ToInt32((x + tempWidth + margin + textWidth / 2) - ScreenLocation.X);

            ScreenPointF[] screenPoints = {
                                              new ScreenPointF(ScreenLocation.X, ScreenLocation.Y),
                                              new ScreenPointF(ScreenLocation.X + realWidth, ScreenLocation.Y),
                                              new ScreenPointF(ScreenLocation.X + realWidth,
                                                  ScreenLocation.Y + drawingFont.Size + GetScaledNumber ( FontMargin )+ GetScaledNumber (YDent )+ fontRatio + GetScaledNumber (Thickness) + 5),
                                              new ScreenPointF(ScreenLocation.X,
                                                  ScreenLocation.Y + drawingFont.Size +GetScaledNumber (FontMargin )+ GetScaledNumber (YDent )+ fontRatio + GetScaledNumber (Thickness )+ 5)
                                          };
            canvas.DrawArea(new Collection<ScreenPointF[]> { screenPoints }, MaskContour, MaskBrush, DrawingLevel.LevelOne, 0, 0,
                PenBrushDrawingOrder.BrushFirst);
        }

        public double GetMaxValue(Dictionary<double, double> xUnitHashTable)
        {
            int j = 0;
            double maxValue = 0;
            foreach (double key in xUnitHashTable.Keys)
            {
                if (j == 0 || maxValue < key)
                {
                    maxValue = key;
                }

                j++;
            }
            return maxValue;
        }

        public Dictionary<double, double> GetFeetPoints(int drawingLowerX, int drawingMaxWidth, FeetValues feetValues)
        {
            int i = 0;
            Dictionary<double, double> xHashTable = new Dictionary<double, double>();
            double[] xValues = new double[12];
            double[] numberValues = new double[12];
            numberValues[0] = 10;
            numberValues[1] = 20;
            numberValues[2] = 30;
            numberValues[3] = 40;
            numberValues[4] = 50;
            numberValues[5] = 100;
            numberValues[6] = 200;
            numberValues[7] = 300;
            numberValues[8] = 400;
            numberValues[9] = 500;
            numberValues[10] = 1000;
            numberValues[11] = 2000;

            if (feetValues.V10 > drawingLowerX & feetValues.V10 < drawingMaxWidth)
            {
                xValues[i] = feetValues.V10;
                xHashTable.Add(xValues[i], numberValues[0]);
                i++;
            }
            if (feetValues.V20 > drawingLowerX & feetValues.V20 < drawingMaxWidth)
            {
                xValues[i] = feetValues.V20;
                xHashTable.Add(xValues[i], numberValues[1]);
                i++;
            }

            if (feetValues.V30 > drawingLowerX & feetValues.V30 < drawingMaxWidth)
            {
                xValues[i] = feetValues.V30;
                xHashTable.Add(xValues[i], numberValues[2]);
                i++;
            }
            if (feetValues.V40 > drawingLowerX & feetValues.V40 < drawingMaxWidth)
            {
                xValues[i] = feetValues.V40;
                xHashTable.Add(xValues[i], numberValues[3]);
                i++;
            }
            if (feetValues.V50 > drawingLowerX & feetValues.V50 < drawingMaxWidth)
            {
                xValues[i] = feetValues.V50;
                xHashTable.Add(xValues[i], numberValues[4]);
                i++;
            }

            if (feetValues.V100 > drawingLowerX & feetValues.V100 < drawingMaxWidth)
            {
                xValues[i] = feetValues.V100;
                xHashTable.Add(xValues[i], numberValues[5]);
                i++;
            }
            if (feetValues.V200 > drawingLowerX & feetValues.V200 < drawingMaxWidth)
            {
                xValues[i] = feetValues.V200;
                xHashTable.Add(xValues[i], numberValues[6]);
                i++;
            }
            if (feetValues.V300 > drawingLowerX & feetValues.V300 < drawingMaxWidth)
            {
                xValues[i] = feetValues.V300;
                xHashTable.Add(xValues[i], numberValues[7]);
                i++;
            }
            if (feetValues.V400 > drawingLowerX & feetValues.V400 < drawingMaxWidth)
            {
                xValues[i] = feetValues.V400;
                xHashTable.Add(xValues[i], numberValues[8]);
                i++;
            }
            if (feetValues.V500 > drawingLowerX & feetValues.V500 < drawingMaxWidth)
            {
                xValues[i] = feetValues.V500;
                xHashTable.Add(xValues[i], numberValues[9]);
                i++;
            }
            if (feetValues.V1000 > drawingLowerX & feetValues.V1000 < drawingMaxWidth)
            {
                xValues[i] = feetValues.V1000;
                xHashTable.Add(xValues[i], numberValues[10]);
                i++;
            }
            if (feetValues.V2000 > drawingLowerX & feetValues.V2000 < drawingMaxWidth)
            {
                xValues[i] = feetValues.V2000;
                xHashTable.Add(xValues[i], numberValues[11]);
                i++;
            }

            RemoveGap(xHashTable, xValues);

            return xHashTable;
        }

        public Dictionary<double, double> GetMilesPoints(int drawingLowerX, int drawingMaxWidth, MilesValues milesValues)
        {
            int i = 0;
            Dictionary<double, double> xHashTable = new Dictionary<double, double>();
            double[] xValues = new double[21];
            double[] numberValues = new double[21];
            numberValues[0] = 0.1;
            numberValues[1] = 0.2;
            numberValues[2] = 0.3;
            numberValues[3] = 0.4;
            numberValues[4] = 0.5;
            numberValues[5] = 1;
            numberValues[6] = 2;
            numberValues[7] = 3;
            numberValues[8] = 4;
            numberValues[9] = 5;
            numberValues[10] = 10;
            numberValues[11] = 20;
            numberValues[12] = 30;
            numberValues[13] = 40;
            numberValues[14] = 50;
            numberValues[15] = 100;
            numberValues[16] = 200;
            numberValues[17] = 300;
            numberValues[18] = 400;
            numberValues[19] = 500;
            numberValues[20] = 1000;

            if (milesValues.V01 > drawingLowerX & milesValues.V01 < drawingMaxWidth)
            {
                xValues[i] = milesValues.V01;
                xHashTable.Add(xValues[i], numberValues[0]);
                i++;
            }
            if (milesValues.V02 > drawingLowerX & milesValues.V02 < drawingMaxWidth)
            {
                xValues[i] = milesValues.V02;
                xHashTable.Add(xValues[i], numberValues[1]);
                i++;
            }

            if (milesValues.V03 > drawingLowerX & milesValues.V03 < drawingMaxWidth)
            {
                xValues[i] = milesValues.V03;
                xHashTable.Add(xValues[i], numberValues[2]);
                i++;
            }
            if (milesValues.V04 > drawingLowerX & milesValues.V04 < drawingMaxWidth)
            {
                xValues[i] = milesValues.V04;
                xHashTable.Add(xValues[i], numberValues[3]);
                i++;
            }
            if (milesValues.V05 > drawingLowerX & milesValues.V05 < drawingMaxWidth)
            {
                xValues[i] = milesValues.V05;
                xHashTable.Add(xValues[i], numberValues[4]);
                i++;
            }

            if (milesValues.V1 > drawingLowerX & milesValues.V1 < drawingMaxWidth)
            {
                xValues[i] = milesValues.V1;
                xHashTable.Add(xValues[i], numberValues[5]);
                i++;
            }
            if (milesValues.V2 > drawingLowerX & milesValues.V2 < drawingMaxWidth)
            {
                xValues[i] = milesValues.V2;
                xHashTable.Add(xValues[i], numberValues[6]);
                i++;
            }

            if (milesValues.V3 > drawingLowerX & milesValues.V3 < drawingMaxWidth)
            {
                xValues[i] = milesValues.V3;
                xHashTable.Add(xValues[i], numberValues[7]);
                i++;
            }
            if (milesValues.V4 > drawingLowerX & milesValues.V4 < drawingMaxWidth)
            {
                xValues[i] = milesValues.V4;
                xHashTable.Add(xValues[i], numberValues[8]);
                i++;
            }
            if (milesValues.V5 > drawingLowerX & milesValues.V5 < drawingMaxWidth)
            {
                xValues[i] = milesValues.V5;
                xHashTable.Add(xValues[i], numberValues[9]);
                i++;
            }

            if (milesValues.V10 > drawingLowerX & milesValues.V10 < drawingMaxWidth)
            {
                xValues[i] = milesValues.V10;
                xHashTable.Add(xValues[i], numberValues[10]);
                i++;
            }
            if (milesValues.V20 > drawingLowerX & milesValues.V20 < drawingMaxWidth)
            {
                xValues[i] = milesValues.V20;
                xHashTable.Add(xValues[i], numberValues[11]);
                i++;
            }

            if (milesValues.V30 > drawingLowerX & milesValues.V30 < drawingMaxWidth)
            {
                xValues[i] = milesValues.V30;
                xHashTable.Add(xValues[i], numberValues[12]);
                i++;
            }
            if (milesValues.V40 > drawingLowerX & milesValues.V40 < drawingMaxWidth)
            {
                xValues[i] = milesValues.V40;
                xHashTable.Add(xValues[i], numberValues[13]);
                i++;
            }
            if (milesValues.V50 > drawingLowerX & milesValues.V50 < drawingMaxWidth)
            {
                xValues[i] = milesValues.V50;
                xHashTable.Add(xValues[i], numberValues[14]);
                i++;
            }
            if (milesValues.V100 > drawingLowerX & milesValues.V100 < drawingMaxWidth)
            {
                xValues[i] = milesValues.V100;
                xHashTable.Add(xValues[i], numberValues[15]);
                i++;
            }
            if (milesValues.V200 > drawingLowerX & milesValues.V200 < drawingMaxWidth)
            {
                xValues[i] = milesValues.V200;
                xHashTable.Add(xValues[i], numberValues[16]);
                i++;
            }
            if (milesValues.V300 > drawingLowerX & milesValues.V300 < drawingMaxWidth)
            {
                xValues[i] = milesValues.V300;
                xHashTable.Add(xValues[i], numberValues[17]);
                i++;
            }
            if (milesValues.V400 > drawingLowerX & milesValues.V400 < drawingMaxWidth)
            {
                xValues[i] = milesValues.V400;
                xHashTable.Add(xValues[i], numberValues[18]);
                i++;
            }
            if (milesValues.V500 > drawingLowerX & milesValues.V500 < drawingMaxWidth)
            {
                xValues[i] = milesValues.V500;
                xHashTable.Add(xValues[i], numberValues[19]);
                i++;
            }
            if (milesValues.V1000 > drawingLowerX & milesValues.V1000 < drawingMaxWidth)
            {
                xValues[i] = milesValues.V1000;
                xHashTable.Add(xValues[i], numberValues[20]);
                i++;
            }

            RemoveGap(xHashTable, xValues);

            return xHashTable;
        }

        public Dictionary<double, double> GetMetersPoints(int drawingLowerX, int drawingMaxWidth, MetersValues metersValues)
        {
            int i = 0;
            Dictionary<double, double> xHashTable = new Dictionary<double, double>();
            double[] xValues = new double[11];
            double[] numberValues = new double[11];
            numberValues[0] = 10;
            numberValues[1] = 20;
            numberValues[2] = 30;
            numberValues[3] = 40;
            numberValues[4] = 50;
            numberValues[5] = 100;
            numberValues[6] = 200;
            numberValues[7] = 300;
            numberValues[8] = 400;
            numberValues[9] = 500;
            numberValues[10] = 1000;

            if (metersValues.V10 > drawingLowerX & metersValues.V10 < drawingMaxWidth)
            {
                xValues[i] = metersValues.V10;
                xHashTable.Add(xValues[i], numberValues[0]);
                i++;
            }
            if (metersValues.V20 > drawingLowerX & metersValues.V20 < drawingMaxWidth)
            {
                xValues[i] = metersValues.V20;
                xHashTable.Add(xValues[i], numberValues[1]);
                i++;
            }

            if (metersValues.V30 > drawingLowerX & metersValues.V30 < drawingMaxWidth)
            {
                xValues[i] = metersValues.V30;
                xHashTable.Add(xValues[i], numberValues[2]);
                i++;
            }
            if (metersValues.V40 > drawingLowerX & metersValues.V40 < drawingMaxWidth)
            {
                xValues[i] = metersValues.V40;
                xHashTable.Add(xValues[i], numberValues[3]);
                i++;
            }
            if (metersValues.V50 > drawingLowerX & metersValues.V50 < drawingMaxWidth)
            {
                xValues[i] = metersValues.V50;
                xHashTable.Add(xValues[i], numberValues[4]);
                i++;
            }

            if (metersValues.V100 > drawingLowerX & metersValues.V100 < drawingMaxWidth)
            {
                xValues[i] = metersValues.V100;
                xHashTable.Add(xValues[i], numberValues[5]);
                i++;
            }
            if (metersValues.V200 > drawingLowerX & metersValues.V200 < drawingMaxWidth)
            {
                xValues[i] = metersValues.V200;
                xHashTable.Add(xValues[i], numberValues[6]);
                i++;
            }
            if (metersValues.V300 > drawingLowerX & metersValues.V300 < drawingMaxWidth)
            {
                xValues[i] = metersValues.V300;
                xHashTable.Add(xValues[i], numberValues[7]);
                i++;
            }
            if (metersValues.V400 > drawingLowerX & metersValues.V400 < drawingMaxWidth)
            {
                xValues[i] = metersValues.V400;
                xHashTable.Add(xValues[i], numberValues[8]);
                i++;
            }
            if (metersValues.V500 > drawingLowerX & metersValues.V500 < drawingMaxWidth)
            {
                xValues[i] = metersValues.V500;
                xHashTable.Add(xValues[i], numberValues[9]);
                i++;
            }
            if (metersValues.V1000 > drawingLowerX & metersValues.V1000 < drawingMaxWidth)
            {
                xValues[i] = metersValues.V1000;
                xHashTable.Add(xValues[i], numberValues[10]);
                i++;
            }

            RemoveGap(xHashTable, xValues);

            return xHashTable;
        }

        public Dictionary<double, double> GetKiloMetersPoints(int drawingLowerX, int drawingMaxWidth, KiloMetersValues kiloMetersValues)
        {
            int i = 0;
            Dictionary<double, double> xHashTable = new Dictionary<double, double>();
            double[] xValues = new double[18];
            double[] numberValues = new double[18];
            numberValues[0] = 1;
            numberValues[1] = 2;
            numberValues[2] = 3;
            numberValues[3] = 4;
            numberValues[4] = 5;
            numberValues[5] = 10;
            numberValues[6] = 20;
            numberValues[7] = 30;
            numberValues[8] = 40;
            numberValues[9] = 50;
            numberValues[10] = 100;
            numberValues[11] = 200;
            numberValues[12] = 300;
            numberValues[13] = 400;
            numberValues[14] = 500;
            numberValues[15] = 1000;
            numberValues[16] = 2000;
            numberValues[17] = 4000;

            if (kiloMetersValues.V1 > drawingLowerX & kiloMetersValues.V1 < drawingMaxWidth)
            {
                xValues[i] = kiloMetersValues.V1;
                xHashTable.Add(xValues[i], numberValues[0]);
                i++;
            }
            if (kiloMetersValues.V2 > drawingLowerX & kiloMetersValues.V2 < drawingMaxWidth)
            {
                xValues[i] = kiloMetersValues.V2;
                xHashTable.Add(xValues[i], numberValues[1]);
                i++;
            }
            if (kiloMetersValues.V3 > drawingLowerX & kiloMetersValues.V3 < drawingMaxWidth)
            {
                xValues[i] = kiloMetersValues.V3;
                xHashTable.Add(xValues[i], numberValues[2]);
                i++;
            }
            if (kiloMetersValues.V4 > drawingLowerX & kiloMetersValues.V4 < drawingMaxWidth)
            {
                xValues[i] = kiloMetersValues.V4;
                xHashTable.Add(xValues[i], numberValues[3]);
                i++;
            }
            if (kiloMetersValues.V5 > drawingLowerX & kiloMetersValues.V5 < drawingMaxWidth)
            {
                xValues[i] = kiloMetersValues.V5;
                xHashTable.Add(xValues[i], numberValues[4]);
                i++;
            }
            if (kiloMetersValues.V10 > drawingLowerX & kiloMetersValues.V10 < drawingMaxWidth)
            {
                xValues[i] = kiloMetersValues.V10;
                xHashTable.Add(xValues[i], numberValues[5]);
                i++;
            }
            if (kiloMetersValues.V20 > drawingLowerX & kiloMetersValues.V20 < drawingMaxWidth)
            {
                xValues[i] = kiloMetersValues.V20;
                xHashTable.Add(xValues[i], numberValues[6]);
                i++;
            }

            if (kiloMetersValues.V30 > drawingLowerX & kiloMetersValues.V30 < drawingMaxWidth)
            {
                xValues[i] = kiloMetersValues.V30;
                xHashTable.Add(xValues[i], numberValues[7]);
                i++;
            }
            if (kiloMetersValues.V40 > drawingLowerX & kiloMetersValues.V40 < drawingMaxWidth)
            {
                xValues[i] = kiloMetersValues.V40;
                xHashTable.Add(xValues[i], numberValues[8]);
                i++;
            }
            if (kiloMetersValues.V50 > drawingLowerX & kiloMetersValues.V50 < drawingMaxWidth)
            {
                xValues[i] = kiloMetersValues.V50;
                xHashTable.Add(xValues[i], numberValues[9]);
                i++;
            }

            if (kiloMetersValues.V100 > drawingLowerX & kiloMetersValues.V100 < drawingMaxWidth)
            {
                xValues[i] = kiloMetersValues.V100;
                xHashTable.Add(xValues[i], numberValues[10]);
                i++;
            }
            if (kiloMetersValues.V200 > drawingLowerX & kiloMetersValues.V200 < drawingMaxWidth)
            {
                xValues[i] = kiloMetersValues.V200;
                xHashTable.Add(xValues[i], numberValues[11]);
                i++;
            }
            if (kiloMetersValues.V300 > drawingLowerX & kiloMetersValues.V300 < drawingMaxWidth)
            {
                xValues[i] = kiloMetersValues.V300;
                xHashTable.Add(xValues[i], numberValues[12]);
                i++;
            }
            if (kiloMetersValues.V400 > drawingLowerX & kiloMetersValues.V400 < drawingMaxWidth)
            {
                xValues[i] = kiloMetersValues.V400;
                xHashTable.Add(xValues[i], numberValues[13]);
                i++;
            }
            if (kiloMetersValues.V500 > drawingLowerX & kiloMetersValues.V500 < drawingMaxWidth)
            {
                xValues[i] = kiloMetersValues.V500;
                xHashTable.Add(xValues[i], numberValues[14]);
                i++;
            }
            if (kiloMetersValues.V1000 > drawingLowerX & kiloMetersValues.V1000 < drawingMaxWidth)
            {
                xValues[i] = kiloMetersValues.V1000;
                xHashTable.Add(xValues[i], numberValues[15]);
                i++;
            }
            if (kiloMetersValues.V2000 > drawingLowerX & kiloMetersValues.V2000 < drawingMaxWidth)
            {
                xValues[i] = kiloMetersValues.V2000;
                xHashTable.Add(xValues[i], numberValues[16]);
                i++;
            }
            if (kiloMetersValues.V4000 > drawingLowerX & kiloMetersValues.V4000 < drawingMaxWidth)
            {
                xValues[i] = kiloMetersValues.V4000;
                xHashTable.Add(xValues[i], numberValues[17]);
                i++;
            }

            RemoveGap(xHashTable, xValues);

            return xHashTable;
        }

         void RemoveGap(Dictionary<double, double> xHashTable, double[] xValues)
        {
            int incrementGap = 35;
            int count = xHashTable.Count;
            for (int i = 0; i < count - 1; i++)
            {
                double gap = xValues[i + 1] - xValues[i];
                gap = GetScaledNumber(gap);
                int drawingIncrementGap = GetScaledNumber(incrementGap);
                if (gap < drawingIncrementGap)
                {
                    xHashTable.Remove(xValues[i]);
                }
            }
        }

        public Collection<RectangleShape> GetRectangleShapeCollection(Dictionary<double, double> hashTable, float fontRatio)
        {
            Collection<RectangleShape> rectCollection = new Collection<RectangleShape>();
            if (hashTable.Count == 0)
            {
                return rectCollection;
            }

            int drawingLeftStart = GetScaledNumber(LeftStart);
            GeoFont drawingFont = GetScaledFont(Font);
            int drawingFontMargin = GetScaledNumber(FontMargin);
            int drawingYDent = GetScaledNumber(YDent);
            int drawingThickness = GetScaledNumber(Thickness);

            int[] value = new int[hashTable.Count];
            int i = 0;
            foreach (double key in hashTable.Keys)
            {
                value[i] = (int)(GetScaledNumber(key) + drawingLeftStart + ScreenLocation.X);
                i++;
            }
            Array.Sort(value);

            int tempWidth = (int)((value[0] - drawingLeftStart - ScreenLocation.X) / 2);
            RectangleShape rectangle = new RectangleShape(
               (int)(drawingLeftStart + ScreenLocation.X)
                , (int)(ScreenLocation.Y + drawingFont.Size + drawingFontMargin + drawingYDent + fontRatio + drawingThickness)
                , (int)(drawingLeftStart + ScreenLocation.X + tempWidth)
                , (int)(ScreenLocation.Y + drawingFont.Size + drawingFontMargin + drawingYDent + fontRatio));
            rectCollection.Add(rectangle);

            RectangleShape rectangle2 = (RectangleShape)rectangle.CloneDeep();
            rectangle2.UpperLeftPoint.X = drawingLeftStart + tempWidth / 2 + ScreenLocation.X;
            rectCollection.Add(rectangle2);

            for (int j = 0; j < hashTable.Count - 1; j++)
            {
                RectangleShape aRectangle = (RectangleShape)rectangle.CloneDeep();
                aRectangle.UpperLeftPoint.X = value[j];
                aRectangle.LowerRightPoint.X = value[j] + (value[j + 1] - value[j]) / 2;
                rectCollection.Add(aRectangle);
            }
            return rectCollection;
        }


        public DistanceUnit ConvertGeographyUnitToDistanceUnit(GeographyUnit unit)
        {
            Validators.CheckGeographyUnitIsValid(unit, "unit");

            switch (unit)
            {
                case GeographyUnit.Feet:
                    return DistanceUnit.Feet;
                case GeographyUnit.Meter:
                    return DistanceUnit.Meter;
                default:

                    throw new ArgumentException("InputGeometryTypeIsNotValid", "unit");
            }
        }

        public GeoFont GetScaledFont(GeoFont font)
        {
            return new GeoFont(font.FontName, (float)(font.Size * ZoomRadio));
        }

        public double GetScaledNumber(double number)
        {
            return number * ZoomRadio;
        }

       public float GetScaledNumber(float number)
        {
            return (float)GetScaledNumber((double)number);
        }

        public int GetScaledNumber(int number)
        {
            return (int)GetScaledNumber((double)number);
        }

    }
}

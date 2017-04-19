using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using Mapgenix.Canvas;
using Mapgenix.Shapes;

namespace Mapgenix.Styles
{
    /// <summary>
    /// Style for visualizing dense point data to easily identify clusters of high concentration of an activity. 
    /// </summary>
    [Serializable]
    public class HeatStyle : BaseStyle
    {
        private int _alpha;
        private Collection<GeoColor> _colorPalette;
        private string _intensityColumnName;
        private double _intensityRangeEnd;
        private double _intensityRangeStart;
        private int _pointIntensity;
        private double _pointRadius;
        private DistanceUnit _pointRadiusUnit;

        public HeatStyle()
            : this(10, 255, string.Empty, 0, 0, 50, DistanceUnit.Kilometer)
        {
        }

        public HeatStyle(int pointIntensity)
            : this(pointIntensity, 255, string.Empty, 0, 0, 50, DistanceUnit.Kilometer)
        {
        }

        public HeatStyle(int pointIntensity, int alpha)
            : this(pointIntensity, alpha, string.Empty, 0, 0, 50, DistanceUnit.Kilometer)
        {
        }

        public HeatStyle(int pointIntensity, double pointRadius, DistanceUnit pointRadiusUnit)
            : this(pointIntensity, 255, string.Empty, 0, 0, pointRadius, pointRadiusUnit)
        {
        }

        public HeatStyle(int pointIntensity, int alpha, double pointRadius, DistanceUnit pointRadiusUnit)
            : this(pointIntensity, alpha, string.Empty, 0, 0, pointRadius, pointRadiusUnit)
        {
        }

        public HeatStyle(string intensityColumnName, double intensityRangeStart, int intensityRangeEnd)
            : this(10, 255, intensityColumnName, intensityRangeStart, intensityRangeEnd, 50, DistanceUnit.Kilometer)
        {
        }

        public HeatStyle(int alpha, string intensityColumnName, double intensityRangeStart, int intensityRangeEnd)
            : this(10, alpha, intensityColumnName, intensityRangeStart, intensityRangeEnd, 50, DistanceUnit.Kilometer)
        {
        }

        public HeatStyle(string intensityColumnName, double intensityRangeStart, int intensityRangeEnd, double pointRadius, DistanceUnit pointRadiusUnit)
            : this(10, 255, intensityColumnName, intensityRangeStart, intensityRangeEnd, pointRadius, pointRadiusUnit)
        {
        }

        public HeatStyle(int alpha, string intensityColumnName, double intensityRangeStart, int intensityRangeEnd, double pointRadius, DistanceUnit pointRadiusUnit)
            : this(10, alpha, intensityColumnName, intensityRangeStart, intensityRangeEnd, pointRadius, pointRadiusUnit)
        {
        }

       
        public HeatStyle(int pointIntensity, int alpha, string intensityColumnName, double intensityRangeStart, int intensityRangeEnd, double pointRadius, DistanceUnit pointRadiusUnit)
        {
            _pointIntensity = pointIntensity;
            _alpha = alpha;
            _intensityColumnName = intensityColumnName;
            _intensityRangeStart = intensityRangeStart;
            _intensityRangeEnd = intensityRangeEnd;
            _pointRadius = pointRadius;
            _pointRadiusUnit = pointRadiusUnit;
            _colorPalette = GetDefaultColorPalette();
        }

        public int PointIntensity
        {
            get { return _pointIntensity; }
            set { _pointIntensity = value; }
        }

        public int Alpha
        {
            get { return _alpha; }
            set { _alpha = value; }
        }

       
        public double IntensityRangeStart
        {
            get { return _intensityRangeStart; }
            set { _intensityRangeStart = value; }
        }

        public double IntensityRangeEnd
        {
            get { return _intensityRangeEnd; }
            set { _intensityRangeEnd = value; }
        }

        public string IntensityColumnName
        {
            get { return _intensityColumnName; }
            set { _intensityColumnName = value; }
        }

        public double PointRadius
        {
            get { return _pointRadius; }
            set { _pointRadius = value; }
        }

        public DistanceUnit PointRadiusUnit
        {
            get { return _pointRadiusUnit; }
            set { _pointRadiusUnit = value; }
        }

        public Collection<GeoColor> ColorPalette
        {
            get { return _colorPalette; }
        }

        protected override void DrawCore(IEnumerable<Feature> features, BaseGeoCanvas canvas, Collection<SimpleCandidate> labelsInThisLayer, Collection<SimpleCandidate> labelsInAllLayers)
        {
            Validators.CheckParameterIsNotNull(features, "features");
            Validators.CheckParameterIsNotNull(canvas, "canvas");

            Bitmap intensityBitmap = null;
            Bitmap colorBitmap = null;
            GeoImage geoImage = null;
            MemoryStream pngStream = null;

            try
            {
                intensityBitmap = new Bitmap(Convert.ToInt32(canvas.Width), Convert.ToInt32(canvas.Height));
                Graphics surface = Graphics.FromImage(intensityBitmap);
                surface.Clear(Color.Transparent);
                surface.Dispose();

                List<HeatPoint> heatPoints = new List<HeatPoint>();

                foreach (Feature feature in features)
                {
                    if (feature.GetWellKnownType() == WellKnownType.Point)
                    {
                        PointShape pointShape = (PointShape)feature.GetShape();
                        ScreenPointF screenPoint = ExtentHelper.ToScreenCoordinate(canvas.CurrentWorldExtent, pointShape, canvas.Width, canvas.Height);

                        double realValue;
                        if (_intensityRangeStart != 0 && _intensityRangeEnd != 0 && _intensityRangeStart != _intensityRangeEnd && !string.IsNullOrEmpty(_intensityColumnName))// != string.Empty)
                        {
                            if (_intensityRangeStart < _intensityRangeEnd)
                            {
                                realValue = (255 / (_intensityRangeEnd - _intensityRangeStart)) * (GetDoubleValue(feature.ColumnValues[_intensityColumnName], _intensityRangeStart, _intensityRangeEnd) - _intensityRangeStart);
                            }
                            else
                            {
                                realValue = (255 / (_intensityRangeEnd - _intensityRangeStart)) * (_intensityRangeEnd - GetDoubleValue(feature.ColumnValues[_intensityColumnName], _intensityRangeStart, _intensityRangeEnd));
                            }
                        }
                        else
                        {
                            realValue = _pointIntensity;
                        }

                        HeatPoint heatPoint = new HeatPoint(Convert.ToInt32(screenPoint.X), Convert.ToInt32(screenPoint.Y), Convert.ToByte(realValue));
                        heatPoints.Add(heatPoint);
                    }
                }

                float size = GetPointSize(canvas);

                intensityBitmap = CreateIntensityMask(intensityBitmap, heatPoints, Convert.ToInt32(size));

                colorBitmap = Colorize(intensityBitmap, (byte)_alpha, _colorPalette);

                pngStream = new MemoryStream();
                colorBitmap.Save(pngStream, ImageFormat.Png);
                geoImage = new GeoImage(pngStream);

                canvas.DrawWorldImageWithoutScaling(geoImage, canvas.CurrentWorldExtent.GetCenterPoint().X, canvas.CurrentWorldExtent.GetCenterPoint().Y, DrawingLevel.LevelOne);
            }
            finally
            {
                if (intensityBitmap != null)
                {
                    intensityBitmap.Dispose();
                }
                if (colorBitmap != null)
                {
                    colorBitmap.Dispose();
                }
                if (geoImage != null)
                {
                    geoImage.Dispose();
                }
                if (pngStream != null)
                {
                    pngStream.Dispose();
                }
            }
        }

        protected override Collection<string> GetRequiredColumnNamesCore()
        {
            Collection<string> requiredColumnNames = new Collection<string>();
            if (_intensityColumnName.Length > 0)
            {
                requiredColumnNames.Add(_intensityColumnName);
            }

            return requiredColumnNames;
        }

        private float GetPointSize(BaseGeoCanvas canvas)
        {
            // Calculate the size of the points in pixel based on the world point size and the current extent.
            //Gets the proper point size according to the current extent. (meter is used as base unit)
            double canvasSizeMeter;
            double pointSizeMeter = Conversion.ConvertMeasureUnits(_pointRadius, _pointRadiusUnit, DistanceUnit.Meter);
            
            if (canvas.MapUnit == GeographyUnit.DecimalDegree)
            {
                if (canvas.CurrentWorldExtent.UpperLeftPoint.X < -180 || canvas.CurrentWorldExtent.UpperLeftPoint.X > 180
                    || canvas.CurrentWorldExtent.LowerLeftPoint.X < -180 || canvas.CurrentWorldExtent.LowerLeftPoint.X > 180
                    || canvas.CurrentWorldExtent.UpperLeftPoint.Y < -90 || canvas.CurrentWorldExtent.UpperLeftPoint.Y > 90
                    || canvas.CurrentWorldExtent.LowerLeftPoint.Y < -90 || canvas.CurrentWorldExtent.LowerLeftPoint.Y > 90)
                {
                    canvasSizeMeter = DecimalDegreesHelper.GetDistanceFromDecimalDegrees(180, 90, 180, -90, DistanceUnit.Meter);
                }
                else
                {
                    canvasSizeMeter = DecimalDegreesHelper.GetDistanceFromDecimalDegrees(canvas.CurrentWorldExtent.UpperLeftPoint.X,
                                                                                         canvas.CurrentWorldExtent.UpperLeftPoint.Y, canvas.CurrentWorldExtent.LowerLeftPoint.X, canvas.CurrentWorldExtent.LowerLeftPoint.Y, DistanceUnit.Meter);
                }
            }
            else
            {
                DistanceUnit fromUnit = DistanceUnit.Meter;
                if (canvas.MapUnit == GeographyUnit.Feet)
                {
                    fromUnit = DistanceUnit.Feet;
                }
                canvasSizeMeter = Conversion.ConvertMeasureUnits(canvas.CurrentWorldExtent.Height, fromUnit, DistanceUnit.Meter);
            }
            float size = (float)((pointSizeMeter * canvas.Height) / canvasSizeMeter);

            return size;
        }

      
        private static double GetDoubleValue(string value, double lowValue, double highValue)
        {
            double returnValue;

            if (string.IsNullOrEmpty(value))
            {
                returnValue = lowValue;
            }
            else
            {
                double doubleValue;
                if (double.TryParse(value, out doubleValue))
                {
                    returnValue = doubleValue;
                }
                else
                {
                    returnValue = lowValue;
                }
            }

            if (returnValue < lowValue)
            {
                returnValue = lowValue;
            }

            if (returnValue > highValue)
            {
                returnValue = highValue;
            }

            return returnValue;
        }

        private static Bitmap CreateIntensityMask(Bitmap bSurface, IEnumerable<HeatPoint> aHeatPoints, int radius)
        {
            Graphics drawSurface = Graphics.FromImage(bSurface);

            drawSurface.Clear(Color.White);

            foreach (HeatPoint dataPoint in aHeatPoints)
            {
                DrawHeatPoint(drawSurface, dataPoint, radius);
            }

            return bSurface;
        }

        private static void DrawHeatPoint(Graphics canvas, HeatPoint heatPoint, int radius)
        {
            List<Point> circumferencePointsList = new List<Point>();

            Point circumferencePoint;

            float fRatio = 1F / Byte.MaxValue;
            byte bHalf = Byte.MaxValue / 2;
            int iIntensity = (byte)(heatPoint.Intensity - ((heatPoint.Intensity - bHalf) * 2));
            float fIntensity = iIntensity * fRatio;
            for (double i = 0; i <= 360; i += 20)
            {
                circumferencePoint = new Point();

                circumferencePoint.X = Convert.ToInt32(heatPoint.X + radius * Math.Cos(ConvertDegreesToRadians(i)));
                circumferencePoint.Y = Convert.ToInt32(heatPoint.Y + radius * Math.Sin(ConvertDegreesToRadians(i)));

                circumferencePointsList.Add(circumferencePoint);
            }

            Point[] circumferencePointsArray;

            circumferencePointsArray = circumferencePointsList.ToArray();

            PathGradientBrush gradientShaper = new PathGradientBrush(circumferencePointsArray);

            ColorBlend cradientSpecifications = new ColorBlend(3);

            cradientSpecifications.Positions = new float[3] { 0, fIntensity, 1 };
            cradientSpecifications.Colors = new Color[3] { Color.FromArgb(0, Color.White), Color.FromArgb(heatPoint.Intensity, Color.Black), Color.FromArgb(heatPoint.Intensity, Color.Black) };

            gradientShaper.InterpolationColors = cradientSpecifications;

            canvas.FillPolygon(gradientShaper, circumferencePointsArray);
        }

        private static double ConvertDegreesToRadians(double degrees)
        {
            double radians = (Math.PI / 180) * degrees;
            return (radians);
        }

        private static Bitmap Colorize(Bitmap mask, byte alpha, Collection<GeoColor> colorPalette)
        {
            Bitmap output = new Bitmap(mask.Width, mask.Height, PixelFormat.Format32bppArgb);

            Graphics surface = Graphics.FromImage(output);
            surface.Clear(Color.Transparent);

            ColorMap[] colors = CreatePaletteIndex(alpha, colorPalette);

            ImageAttributes remapper = new ImageAttributes();
            remapper.SetRemapTable(colors);

            surface.DrawImage(mask, new Rectangle(0, 0, mask.Width, mask.Height), 0, 0, mask.Width, mask.Height, GraphicsUnit.Pixel, remapper);

            return output;
        }

        private static ColorMap[] CreatePaletteIndex(byte alpha, Collection<GeoColor> colorPalette)
        {
            ColorMap[] outputMap = new ColorMap[256];

            for (int x = 0; x <= 255; x++)
            {
                outputMap[x] = new ColorMap();
                outputMap[x].OldColor = Color.FromArgb(x, x, x);

                if (colorPalette[x].AlphaComponent < alpha)
                {
                    outputMap[x].NewColor = Color.FromArgb(colorPalette[x].AlphaComponent, colorPalette[x].RedComponent, colorPalette[x].GreenComponent, colorPalette[x].BlueComponent);
                }
                else
                {
                    outputMap[x].NewColor = Color.FromArgb(alpha, colorPalette[x].RedComponent, colorPalette[x].GreenComponent, colorPalette[x].BlueComponent);
                }
            }

            return outputMap;
        }

        private static Collection<GeoColor> GetDefaultColorPalette()
        {
            Collection<GeoColor> colorPalette = new Collection<GeoColor>();

            colorPalette.Add(GeoColor.FromArgb(255, 254, 248, 248));
            colorPalette.Add(GeoColor.FromArgb(255, 255, 249, 249));
            colorPalette.Add(GeoColor.FromArgb(255, 255, 251, 250));
            colorPalette.Add(GeoColor.FromArgb(255, 255, 252, 251));
            colorPalette.Add(GeoColor.FromArgb(255, 255, 255, 253));
            colorPalette.Add(GeoColor.FromArgb(255, 255, 255, 253));
            colorPalette.Add(GeoColor.FromArgb(255, 255, 255, 255));
            colorPalette.Add(GeoColor.FromArgb(255, 255, 255, 255));
            colorPalette.Add(GeoColor.FromArgb(255, 255, 255, 255));
            colorPalette.Add(GeoColor.FromArgb(255, 255, 255, 255));
            colorPalette.Add(GeoColor.FromArgb(255, 255, 255, 255));
            colorPalette.Add(GeoColor.FromArgb(255, 255, 255, 255));
            colorPalette.Add(GeoColor.FromArgb(255, 255, 255, 255));
            colorPalette.Add(GeoColor.FromArgb(255, 255, 255, 255));
            colorPalette.Add(GeoColor.FromArgb(255, 255, 254, 255));
            colorPalette.Add(GeoColor.FromArgb(255, 255, 253, 253));
            colorPalette.Add(GeoColor.FromArgb(255, 255, 245, 245));
            colorPalette.Add(GeoColor.FromArgb(255, 255, 236, 235));
            colorPalette.Add(GeoColor.FromArgb(255, 254, 223, 221));
            colorPalette.Add(GeoColor.FromArgb(255, 255, 207, 205));
            colorPalette.Add(GeoColor.FromArgb(255, 253, 188, 186));
            colorPalette.Add(GeoColor.FromArgb(255, 255, 170, 167));
            colorPalette.Add(GeoColor.FromArgb(255, 255, 149, 149));
            colorPalette.Add(GeoColor.FromArgb(255, 255, 130, 128));
            colorPalette.Add(GeoColor.FromArgb(255, 255, 109, 109));
            colorPalette.Add(GeoColor.FromArgb(255, 255, 91, 89));
            colorPalette.Add(GeoColor.FromArgb(255, 255, 71, 71));
            colorPalette.Add(GeoColor.FromArgb(255, 255, 53, 53));
            colorPalette.Add(GeoColor.FromArgb(255, 255, 35, 35));
            colorPalette.Add(GeoColor.FromArgb(255, 254, 22, 22));
            colorPalette.Add(GeoColor.FromArgb(255, 254, 10, 10));
            colorPalette.Add(GeoColor.FromArgb(255, 250, 2, 2));
            colorPalette.Add(GeoColor.FromArgb(255, 253, 1, 0));
            colorPalette.Add(GeoColor.FromArgb(255, 253, 1, 0));
            colorPalette.Add(GeoColor.FromArgb(255, 254, 0, 0));
            colorPalette.Add(GeoColor.FromArgb(255, 254, 0, 0));
            colorPalette.Add(GeoColor.FromArgb(255, 253, 1, 0));
            colorPalette.Add(GeoColor.FromArgb(255, 255, 3, 0));
            colorPalette.Add(GeoColor.FromArgb(255, 255, 6, 0));
            colorPalette.Add(GeoColor.FromArgb(255, 255, 9, 0));
            colorPalette.Add(GeoColor.FromArgb(255, 255, 11, 0));
            colorPalette.Add(GeoColor.FromArgb(255, 255, 16, 0));
            colorPalette.Add(GeoColor.FromArgb(255, 255, 20, 0));
            colorPalette.Add(GeoColor.FromArgb(255, 255, 25, 0));
            colorPalette.Add(GeoColor.FromArgb(255, 253, 30, 0));
            colorPalette.Add(GeoColor.FromArgb(255, 253, 35, 0));
            colorPalette.Add(GeoColor.FromArgb(255, 255, 39, 3));
            colorPalette.Add(GeoColor.FromArgb(255, 255, 45, 2));
            colorPalette.Add(GeoColor.FromArgb(255, 254, 48, 0));
            colorPalette.Add(GeoColor.FromArgb(255, 255, 58, 1));
            colorPalette.Add(GeoColor.FromArgb(255, 254, 60, 0));
            colorPalette.Add(GeoColor.FromArgb(255, 254, 67, 0));
            colorPalette.Add(GeoColor.FromArgb(255, 255, 74, 0));
            colorPalette.Add(GeoColor.FromArgb(255, 253, 78, 0));
            colorPalette.Add(GeoColor.FromArgb(255, 255, 87, 0));
            colorPalette.Add(GeoColor.FromArgb(255, 255, 92, 0));
            colorPalette.Add(GeoColor.FromArgb(255, 255, 99, 0));
            colorPalette.Add(GeoColor.FromArgb(255, 253, 105, 0));
            colorPalette.Add(GeoColor.FromArgb(255, 255, 112, 0));
            colorPalette.Add(GeoColor.FromArgb(255, 253, 118, 0));
            colorPalette.Add(GeoColor.FromArgb(255, 253, 124, 0));
            colorPalette.Add(GeoColor.FromArgb(255, 253, 131, 0));
            colorPalette.Add(GeoColor.FromArgb(255, 255, 137, 1));
            colorPalette.Add(GeoColor.FromArgb(255, 255, 142, 1));
            colorPalette.Add(GeoColor.FromArgb(255, 255, 150, 0));
            colorPalette.Add(GeoColor.FromArgb(255, 255, 156, 0));
            colorPalette.Add(GeoColor.FromArgb(255, 255, 161, 1));
            colorPalette.Add(GeoColor.FromArgb(255, 254, 167, 0));
            colorPalette.Add(GeoColor.FromArgb(255, 255, 174, 2));
            colorPalette.Add(GeoColor.FromArgb(255, 255, 180, 1));
            colorPalette.Add(GeoColor.FromArgb(255, 254, 186, 1));
            colorPalette.Add(GeoColor.FromArgb(255, 255, 192, 1));
            colorPalette.Add(GeoColor.FromArgb(255, 255, 197, 2));
            colorPalette.Add(GeoColor.FromArgb(255, 255, 203, 3));
            colorPalette.Add(GeoColor.FromArgb(255, 254, 208, 1));
            colorPalette.Add(GeoColor.FromArgb(255, 253, 212, 0));
            colorPalette.Add(GeoColor.FromArgb(255, 254, 218, 0));
            colorPalette.Add(GeoColor.FromArgb(255, 254, 222, 0));
            colorPalette.Add(GeoColor.FromArgb(255, 254, 225, 0));
            colorPalette.Add(GeoColor.FromArgb(255, 255, 231, 0));
            colorPalette.Add(GeoColor.FromArgb(255, 255, 235, 0));
            colorPalette.Add(GeoColor.FromArgb(255, 254, 239, 0));
            colorPalette.Add(GeoColor.FromArgb(255, 255, 241, 0));
            colorPalette.Add(GeoColor.FromArgb(255, 255, 245, 0));
            colorPalette.Add(GeoColor.FromArgb(255, 255, 246, 0));
            colorPalette.Add(GeoColor.FromArgb(255, 255, 250, 1));
            colorPalette.Add(GeoColor.FromArgb(255, 255, 251, 1));
            colorPalette.Add(GeoColor.FromArgb(255, 253, 252, 1));
            colorPalette.Add(GeoColor.FromArgb(255, 251, 252, 1));
            colorPalette.Add(GeoColor.FromArgb(255, 247, 252, 0));
            colorPalette.Add(GeoColor.FromArgb(255, 245, 251, 3));
            colorPalette.Add(GeoColor.FromArgb(255, 241, 252, 2));
            colorPalette.Add(GeoColor.FromArgb(255, 237, 251, 5));
            colorPalette.Add(GeoColor.FromArgb(255, 233, 252, 4));
            colorPalette.Add(GeoColor.FromArgb(255, 231, 251, 6));
            colorPalette.Add(GeoColor.FromArgb(255, 226, 252, 5));
            colorPalette.Add(GeoColor.FromArgb(255, 221, 251, 5));
            colorPalette.Add(GeoColor.FromArgb(255, 216, 252, 4));
            colorPalette.Add(GeoColor.FromArgb(255, 212, 252, 6));
            colorPalette.Add(GeoColor.FromArgb(255, 207, 251, 6));
            colorPalette.Add(GeoColor.FromArgb(255, 202, 252, 7));
            colorPalette.Add(GeoColor.FromArgb(255, 198, 252, 10));
            colorPalette.Add(GeoColor.FromArgb(255, 191, 250, 8));
            colorPalette.Add(GeoColor.FromArgb(255, 186, 249, 10));
            colorPalette.Add(GeoColor.FromArgb(255, 180, 247, 9));
            colorPalette.Add(GeoColor.FromArgb(255, 176, 246, 10));
            colorPalette.Add(GeoColor.FromArgb(255, 168, 244, 11));
            colorPalette.Add(GeoColor.FromArgb(255, 163, 242, 11));
            colorPalette.Add(GeoColor.FromArgb(255, 158, 241, 13));
            colorPalette.Add(GeoColor.FromArgb(255, 152, 239, 13));
            colorPalette.Add(GeoColor.FromArgb(255, 147, 235, 13));
            colorPalette.Add(GeoColor.FromArgb(255, 140, 231, 12));
            colorPalette.Add(GeoColor.FromArgb(255, 135, 232, 15));
            colorPalette.Add(GeoColor.FromArgb(255, 129, 230, 16));
            colorPalette.Add(GeoColor.FromArgb(255, 122, 226, 15));
            colorPalette.Add(GeoColor.FromArgb(255, 116, 224, 17));
            colorPalette.Add(GeoColor.FromArgb(255, 110, 221, 20));
            colorPalette.Add(GeoColor.FromArgb(255, 104, 217, 21));
            colorPalette.Add(GeoColor.FromArgb(255, 96, 216, 22));
            colorPalette.Add(GeoColor.FromArgb(255, 92, 215, 23));
            colorPalette.Add(GeoColor.FromArgb(255, 87, 212, 22));
            colorPalette.Add(GeoColor.FromArgb(255, 82, 211, 23));
            colorPalette.Add(GeoColor.FromArgb(255, 75, 207, 25));
            colorPalette.Add(GeoColor.FromArgb(255, 70, 205, 25));
            colorPalette.Add(GeoColor.FromArgb(255, 66, 202, 30));
            colorPalette.Add(GeoColor.FromArgb(255, 60, 200, 31));
            colorPalette.Add(GeoColor.FromArgb(255, 56, 197, 33));
            colorPalette.Add(GeoColor.FromArgb(255, 50, 194, 34));
            colorPalette.Add(GeoColor.FromArgb(255, 43, 193, 34));
            colorPalette.Add(GeoColor.FromArgb(255, 38, 192, 36));
            colorPalette.Add(GeoColor.FromArgb(255, 36, 189, 36));
            colorPalette.Add(GeoColor.FromArgb(255, 31, 188, 37));
            colorPalette.Add(GeoColor.FromArgb(255, 29, 187, 41));
            colorPalette.Add(GeoColor.FromArgb(255, 24, 185, 43));
            colorPalette.Add(GeoColor.FromArgb(255, 19, 184, 45));
            colorPalette.Add(GeoColor.FromArgb(255, 15, 183, 46));
            colorPalette.Add(GeoColor.FromArgb(255, 12, 182, 49));
            colorPalette.Add(GeoColor.FromArgb(255, 8, 182, 51));
            colorPalette.Add(GeoColor.FromArgb(255, 6, 181, 52));
            colorPalette.Add(GeoColor.FromArgb(255, 4, 180, 55));
            colorPalette.Add(GeoColor.FromArgb(255, 1, 179, 57));
            colorPalette.Add(GeoColor.FromArgb(255, 1, 178, 61));
            colorPalette.Add(GeoColor.FromArgb(255, 0, 179, 62));
            colorPalette.Add(GeoColor.FromArgb(255, 0, 180, 65));
            colorPalette.Add(GeoColor.FromArgb(255, 0, 181, 68));
            colorPalette.Add(GeoColor.FromArgb(255, 0, 181, 70));
            colorPalette.Add(GeoColor.FromArgb(255, 0, 182, 72));
            colorPalette.Add(GeoColor.FromArgb(255, 1, 182, 77));
            colorPalette.Add(GeoColor.FromArgb(255, 1, 183, 82));
            colorPalette.Add(GeoColor.FromArgb(255, 0, 184, 86));
            colorPalette.Add(GeoColor.FromArgb(255, 1, 185, 89));
            colorPalette.Add(GeoColor.FromArgb(255, 0, 186, 91));
            colorPalette.Add(GeoColor.FromArgb(255, 1, 188, 97));
            colorPalette.Add(GeoColor.FromArgb(255, 0, 190, 100));
            colorPalette.Add(GeoColor.FromArgb(255, 0, 192, 103));
            colorPalette.Add(GeoColor.FromArgb(255, 0, 194, 108));
            colorPalette.Add(GeoColor.FromArgb(255, 0, 195, 113));
            colorPalette.Add(GeoColor.FromArgb(255, 0, 197, 118));
            colorPalette.Add(GeoColor.FromArgb(255, 0, 198, 121));
            colorPalette.Add(GeoColor.FromArgb(255, 0, 199, 126));
            colorPalette.Add(GeoColor.FromArgb(255, 0, 201, 131));
            colorPalette.Add(GeoColor.FromArgb(255, 0, 204, 135));
            colorPalette.Add(GeoColor.FromArgb(255, 0, 205, 140));
            colorPalette.Add(GeoColor.FromArgb(255, 0, 208, 143));
            colorPalette.Add(GeoColor.FromArgb(255, 0, 210, 149));
            colorPalette.Add(GeoColor.FromArgb(255, 0, 212, 154));
            colorPalette.Add(GeoColor.FromArgb(255, 0, 213, 157));
            colorPalette.Add(GeoColor.FromArgb(255, 0, 214, 162));
            colorPalette.Add(GeoColor.FromArgb(255, 0, 216, 167));
            colorPalette.Add(GeoColor.FromArgb(255, 2, 219, 174));
            colorPalette.Add(GeoColor.FromArgb(255, 1, 222, 181));
            colorPalette.Add(GeoColor.FromArgb(255, 0, 224, 187));
            colorPalette.Add(GeoColor.FromArgb(255, 0, 228, 195));
            colorPalette.Add(GeoColor.FromArgb(255, 0, 230, 201));
            colorPalette.Add(GeoColor.FromArgb(255, 0, 232, 206));
            colorPalette.Add(GeoColor.FromArgb(255, 0, 233, 211));
            colorPalette.Add(GeoColor.FromArgb(255, 0, 234, 219));
            colorPalette.Add(GeoColor.FromArgb(255, 0, 234, 223));
            colorPalette.Add(GeoColor.FromArgb(255, 0, 234, 225));
            colorPalette.Add(GeoColor.FromArgb(255, 1, 234, 230));
            colorPalette.Add(GeoColor.FromArgb(255, 0, 234, 235));
            colorPalette.Add(GeoColor.FromArgb(255, 1, 234, 239));
            colorPalette.Add(GeoColor.FromArgb(255, 0, 234, 243));
            colorPalette.Add(GeoColor.FromArgb(255, 0, 233, 247));
            colorPalette.Add(GeoColor.FromArgb(255, 0, 235, 249));
            colorPalette.Add(GeoColor.FromArgb(255, 0, 234, 251));
            colorPalette.Add(GeoColor.FromArgb(255, 1, 233, 255));
            colorPalette.Add(GeoColor.FromArgb(255, 1, 233, 255));
            colorPalette.Add(GeoColor.FromArgb(255, 1, 229, 255));
            colorPalette.Add(GeoColor.FromArgb(255, 2, 227, 255));
            colorPalette.Add(GeoColor.FromArgb(255, 0, 222, 255));
            colorPalette.Add(GeoColor.FromArgb(255, 0, 217, 252));
            colorPalette.Add(GeoColor.FromArgb(255, 1, 213, 253));
            colorPalette.Add(GeoColor.FromArgb(255, 0, 206, 248));
            colorPalette.Add(GeoColor.FromArgb(255, 0, 200, 249));
            colorPalette.Add(GeoColor.FromArgb(255, 0, 194, 247));
            colorPalette.Add(GeoColor.FromArgb(255, 0, 185, 242));
            colorPalette.Add(GeoColor.FromArgb(255, 0, 177, 239));
            colorPalette.Add(GeoColor.FromArgb(255, 0, 169, 236));
            colorPalette.Add(GeoColor.FromArgb(255, 0, 160, 230));
            colorPalette.Add(GeoColor.FromArgb(255, 1, 153, 226));
            colorPalette.Add(GeoColor.FromArgb(255, 1, 144, 220));
            colorPalette.Add(GeoColor.FromArgb(255, 0, 136, 214));
            colorPalette.Add(GeoColor.FromArgb(255, 0, 127, 208));
            colorPalette.Add(GeoColor.FromArgb(255, 0, 120, 205));
            colorPalette.Add(GeoColor.FromArgb(255, 0, 116, 203));
            colorPalette.Add(GeoColor.FromArgb(255, 2, 111, 202));
            colorPalette.Add(GeoColor.FromArgb(255, 0, 105, 197));
            colorPalette.Add(GeoColor.FromArgb(255, 1, 97, 197));
            colorPalette.Add(GeoColor.FromArgb(255, 0, 91, 192));
            colorPalette.Add(GeoColor.FromArgb(255, 0, 82, 186));
            colorPalette.Add(GeoColor.FromArgb(255, 0, 76, 182));
            colorPalette.Add(GeoColor.FromArgb(255, 1, 69, 180));
            colorPalette.Add(GeoColor.FromArgb(255, 0, 62, 175));
            colorPalette.Add(GeoColor.FromArgb(255, 1, 54, 168));
            colorPalette.Add(GeoColor.FromArgb(255, 0, 50, 165));
            colorPalette.Add(GeoColor.FromArgb(255, 0, 43, 158));
            colorPalette.Add(GeoColor.FromArgb(255, 0, 39, 153));
            colorPalette.Add(GeoColor.FromArgb(255, 0, 33, 149));
            colorPalette.Add(GeoColor.FromArgb(255, 0, 30, 143));
            colorPalette.Add(GeoColor.FromArgb(255, 1, 24, 140));
            colorPalette.Add(GeoColor.FromArgb(255, 0, 20, 135));
            colorPalette.Add(GeoColor.FromArgb(255, 0, 15, 130));
            colorPalette.Add(GeoColor.FromArgb(255, 0, 13, 127));
            colorPalette.Add(GeoColor.FromArgb(255, 0, 11, 119));
            colorPalette.Add(GeoColor.FromArgb(255, 1, 10, 115));
            colorPalette.Add(GeoColor.FromArgb(255, 0, 7, 111));
            colorPalette.Add(GeoColor.FromArgb(255, 1, 5, 104));
            colorPalette.Add(GeoColor.FromArgb(255, 2, 5, 100));
            colorPalette.Add(GeoColor.FromArgb(255, 3, 4, 94));
            colorPalette.Add(GeoColor.FromArgb(255, 4, 4, 90));
            colorPalette.Add(GeoColor.FromArgb(255, 4, 5, 87));
            colorPalette.Add(GeoColor.FromArgb(255, 6, 5, 85));
            colorPalette.Add(GeoColor.FromArgb(255, 7, 6, 82));
            colorPalette.Add(GeoColor.FromArgb(255, 6, 9, 80));
            colorPalette.Add(GeoColor.FromArgb(255, 7, 10, 77));
            colorPalette.Add(GeoColor.FromArgb(255, 11, 12, 77));
            colorPalette.Add(GeoColor.FromArgb(255, 13, 15, 76));
            colorPalette.Add(GeoColor.FromArgb(255, 16, 16, 76));
            colorPalette.Add(GeoColor.FromArgb(255, 16, 17, 73));
            colorPalette.Add(GeoColor.FromArgb(255, 20, 20, 74));
            colorPalette.Add(GeoColor.FromArgb(255, 22, 22, 72));
            colorPalette.Add(GeoColor.FromArgb(255, 27, 25, 74));
            colorPalette.Add(GeoColor.FromArgb(255, 30, 29, 73));
            colorPalette.Add(GeoColor.FromArgb(255, 31, 31, 69));
            colorPalette.Add(GeoColor.FromArgb(255, 33, 33, 67));
            colorPalette.Add(GeoColor.FromArgb(255, 37, 36, 67));
            colorPalette.Add(GeoColor.FromArgb(255, 39, 39, 65));
            colorPalette.Add(GeoColor.FromArgb(255, 41, 41, 67));
            colorPalette.Add(GeoColor.FromArgb(255, 44, 45, 66));
            colorPalette.Add(GeoColor.FromArgb(255, 46, 44, 65));
            colorPalette.Add(GeoColor.FromArgb(255, 48, 47, 63));
            colorPalette.Add(GeoColor.FromArgb(255, 49, 48, 62));
            colorPalette.Add(GeoColor.FromArgb(255, 50, 50, 60));
            colorPalette.Add(GeoColor.FromArgb(255, 54, 52, 63));
            colorPalette.Add(GeoColor.FromArgb(0, 53, 52, 60));

            return colorPalette;
        }
    }
}
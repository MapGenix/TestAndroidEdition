using System;
using System.Collections.ObjectModel;
using System.Linq;
using Mapgenix.Canvas;
using Mapgenix.Shapes;

namespace Mapgenix.Layers
{
    /// <summary>
    /// Adornment layer for graticule (meridians and parallels)
    /// </summary>
    [Serializable]
    public class GraticuleAdornmentLayer : BaseAdornmentLayer
    {
       
        public int GraticuleDensity { get; set; }

        public Collection<double> Intervals { get; set; }

        public LineStyle GraticuleLineStyle{get; set; }

        public GeoFont GraticuleTextFont { get; set; }

        public BaseGeoBrush GraticuleTextBrush { get; set; }

        public BaseProjection Projection { get; set; }

        public WrappingMode WrappingMode { get; set; }

        public RectangleShape WrappingExtent{ get; set; }

       
        protected override void DrawCore(BaseGeoCanvas canvas, Collection<SimpleCandidate> labelsInAllLayers)
        {
            if (Projection != null)
            {
                AdornmentDrawHelper.DrawGraticuleWithProjection(this,canvas);
            }
            else
            {
                AdornmentDrawHelper.DrawGraticuleWithoutProjection(this,canvas);
            }
        }

        public RectangleShape GetDrawingExtentForWrapping(BaseGeoCanvas canvas)
        {
            PointShape lowerLeftPoint = Projection.ConvertToInternalProjection(canvas.CurrentWorldExtent.LowerLeftPoint) as PointShape;
            PointShape upperRightPoint = Projection.ConvertToInternalProjection(canvas.CurrentWorldExtent.UpperRightPoint) as PointShape;

            PointShape lowerLeftPointInWrappingExtent = Projection.ConvertToExternalProjection(lowerLeftPoint) as PointShape;
            PointShape upperRightPointInWrappingExtent = Projection.ConvertToExternalProjection(upperRightPoint) as PointShape;
            int wrappingLeftWorldCount;
            int wrappingRightWorldCount;

            if (canvas.CurrentWorldExtent.LowerLeftPoint.X > lowerLeftPointInWrappingExtent.X)
                wrappingLeftWorldCount = (int)Math.Floor((canvas.CurrentWorldExtent.LowerLeftPoint.X - lowerLeftPointInWrappingExtent.X) / WrappingExtent.Width);
            else
                wrappingLeftWorldCount = (int)Math.Ceiling((canvas.CurrentWorldExtent.LowerLeftPoint.X - lowerLeftPointInWrappingExtent.X) / WrappingExtent.Width);
            if (canvas.CurrentWorldExtent.LowerRightPoint.X > upperRightPointInWrappingExtent.X)
                wrappingRightWorldCount = (int)Math.Floor((canvas.CurrentWorldExtent.LowerRightPoint.X - upperRightPointInWrappingExtent.X) / WrappingExtent.Width);
            else
                wrappingRightWorldCount = (int)Math.Ceiling((canvas.CurrentWorldExtent.LowerRightPoint.X - upperRightPointInWrappingExtent.X) / WrappingExtent.Width);
            RectangleShape extentAfeterWrapping = new RectangleShape(new PointShape(lowerLeftPoint.X + 360 * wrappingLeftWorldCount, upperRightPoint.Y),
                new PointShape(upperRightPoint.X + wrappingRightWorldCount * 360, lowerLeftPoint.Y));
            return extentAfeterWrapping;
        }

        public Vertex CalculateOriginalVertexWithProjection(double x, double y, bool isWrappingLeft)
        {
            int wrappedWorldCount;
            double originalX = double.MinValue;
            if (isWrappingLeft)
            {
                wrappedWorldCount = Math.Abs(x % 360) > 180 ? (int)Math.Floor(x / 360) : (int)Math.Ceiling(x / 360);
            }
            else
            {
                wrappedWorldCount = Math.Abs(x % 360) > 180 ? (int)Math.Ceiling(x / 360) : (int)Math.Floor(x / 360);
            }
            originalX = (x - wrappedWorldCount * 360);

            Vertex projVertex = Projection.ConvertToExternalProjection(originalX, y);

            return new Vertex(projVertex.X + wrappedWorldCount * WrappingExtent.Width, projVertex.Y);
        }

        public ExtremesLatLong GetExtremesLatLong(BaseGeoCanvas canvas)

        {
            LineShape lineShape = new LineShape();
            lineShape.Vertices.Add(new Vertex(canvas.CurrentWorldExtent.UpperLeftPoint.X, canvas.CurrentWorldExtent.UpperLeftPoint.Y));
            lineShape.Vertices.Add(new Vertex(canvas.CurrentWorldExtent.UpperRightPoint.X, canvas.CurrentWorldExtent.UpperRightPoint.Y));
            lineShape.Vertices.Add(new Vertex(canvas.CurrentWorldExtent.LowerRightPoint.X, canvas.CurrentWorldExtent.LowerRightPoint.Y));
            lineShape.Vertices.Add(new Vertex(canvas.CurrentWorldExtent.LowerLeftPoint.X, canvas.CurrentWorldExtent.LowerLeftPoint.Y));

            Collection<double> lonValues = new Collection<double>();
            Collection<double> latValues = new Collection<double>();

            Vertex geoUlVertex = Projection.ConvertToInternalProjection(canvas.CurrentWorldExtent.UpperLeftPoint.X,
                                                                             canvas.CurrentWorldExtent.UpperLeftPoint.Y);
            Vertex geoUrVertex = Projection.ConvertToInternalProjection(canvas.CurrentWorldExtent.UpperRightPoint.X,
                                                                             canvas.CurrentWorldExtent.UpperRightPoint.Y);
            Vertex geoLrVertex = Projection.ConvertToInternalProjection(canvas.CurrentWorldExtent.LowerRightPoint.X,
                                                                             canvas.CurrentWorldExtent.LowerRightPoint.Y);
            Vertex geoLlVertex = Projection.ConvertToInternalProjection(canvas.CurrentWorldExtent.LowerLeftPoint.X,
                                                                             canvas.CurrentWorldExtent.LowerLeftPoint.Y);

            lonValues.Add(geoUlVertex.X);
            lonValues.Add(geoUrVertex.X);
            lonValues.Add(geoLrVertex.X);
            lonValues.Add(geoLlVertex.X);
            latValues.Add(geoUlVertex.Y);
            latValues.Add(geoUrVertex.Y);
            latValues.Add(geoLrVertex.Y);
            latValues.Add(geoLlVertex.Y);

            for (int i = 0; i <= 100; i += 1)
            {
                PointShape pointShape = lineShape.GetPointOnALine(StartingPoint.FirstPoint, i);
                Vertex geoVertex = Projection.ConvertToInternalProjection(pointShape.X, pointShape.Y);
                lonValues.Add(geoVertex.X);
                latValues.Add(geoVertex.Y);
            }

            double maxLat = GetLargestNumber(latValues);
            double minLat = GetSmallestNumber(latValues);
            double maxLon = GetLargestNumber(lonValues);
            double minLon = GetSmallestNumber(lonValues);

            if (maxLat > 90)
            {
                maxLat = 90;
            }
            if (minLat < -90)
            {
                minLat = -90;
            }
            if (maxLon > 180)
            {
                maxLon = 180;
            }
            if (minLon < -180)
            {
                minLon = -180;
            }

            return new ExtremesLatLong(maxLat, minLat, maxLon, minLon);
        }

        static double GetLargestNumber(Collection<double> values)
        {
            double result = values[0];
            return values.Concat(new[] {result}).Max();
        }

        static double GetSmallestNumber(Collection<double> values)
        {
            double result = values[0];
            return values.Concat(new[] {result}).Min();
        }

        internal string FormatLatLong(double value, LineType lineType, double increment)
        {
            string result;
            try
            {
                if (increment >= 1)
                {
                    result = DecimalDegreesHelper.GetDegreesMinutesSecondsStringFromDecimalDegree(Math.Abs(value));
                    result = result.Substring(0, result.Length - 9);
                }
                else if (increment >= 0.1)
                {
                    result = DecimalDegreesHelper.GetDegreesMinutesSecondsStringFromDecimalDegree(Math.Abs(value));
                    result = result.Substring(0, result.Length - 5);
                }
                else if (increment >= 0.01)
                {
                    result = DecimalDegreesHelper.GetDegreesMinutesSecondsStringFromDecimalDegree(Math.Abs(value));
                }
                else
                {
                    result = DecimalDegreesHelper.GetDegreesMinutesSecondsStringFromDecimalDegree(Math.Abs(value), 2);
                }

                if (lineType == LineType.Meridian)
                {
                    if (value > 0)
                    {
                        result = result + " E";
                    }
                    else if (value < 0)
                    {
                        result = result + " W";
                    }
                }

                if (lineType == LineType.Parallel)
                {
                    if (value > 0)
                    {
                        result = result + " N";
                    }
                    else if (value < 0)
                    {
                        result = result + " S";
                    }
                }
            }
            catch
            {
                result = "N/A";
            }

            return result;
        }

        public double CeilingNumber(double number, double interval)
        {
            double result = number;
            double ieeeRemainder = Math.IEEERemainder(number, interval);
            if (ieeeRemainder > 0)
            {
                result = (number - ieeeRemainder) + interval;
            }
            else if (ieeeRemainder < 0)
            {
                result = number + Math.Abs(ieeeRemainder);
            }

            return result;
        }

        public double FloorNumber(double number, double interval)
        {
            double result = number;
            double ieeeRemainder = Math.IEEERemainder(number, interval);
            if (ieeeRemainder > 0)
            {
                result = number - ieeeRemainder;
            }
            else if (ieeeRemainder < 0)
            {
                result = number - (interval - Math.Abs(ieeeRemainder));
            }

            return result;
        }

        public double GetIncrement(double currentExtentWidth, double divisor)
        {
            double result = Intervals[Intervals.Count - 1];
            double rawInterval = currentExtentWidth / divisor;

            foreach (double interval in Intervals.Where(interval => rawInterval < interval))
            {
                result = interval;
                break;
            }

            return result;
        }
    }
}
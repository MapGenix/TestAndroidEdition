using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using Mapgenix.Canvas;
using Mapgenix.Shapes;
using Mapgenix.Styles.Properties;
using Mapgenix.Utils;

namespace Mapgenix.Styles
{
    /// <summary>Style using dot symbol to show the presence of a phenomena.
    /// </summary>
    [Serializable]
    public class DotDensityStyle : BaseStyle
    {
        private const int FirstPointPosition = 5;
        private const int SecondPointPosition = 13;

        private string _columnName;
        private PointStyle _customPointStyle;
        private PointStyle _defaultPointStyle;

        [NonSerialized]
        private Dictionary<string, Collection<Vertex>> _dotDensityCache;

        private double _pointToValueRatio;

        public DotDensityStyle()
            : this(string.Empty, 1, 1, GeoColor.StandardColors.Red, null)
        {
        }

        public DotDensityStyle(string columnName, double pointToValueRatio, int pointSize, GeoColor pointColor)
            : this(columnName, pointToValueRatio, pointSize, pointColor, null)
        {
        }

       
        public DotDensityStyle(string columnName, double pointToValueRatio, PointStyle customPointStyle)
            : this(columnName, pointToValueRatio, 1, GeoColor.StandardColors.Transparent, customPointStyle)
        {
        }

        private DotDensityStyle(string columnName, double pointToValueRatio, int pointSize, GeoColor pointColor, PointStyle customPointStyle)
        {
            _columnName = columnName;
            _pointToValueRatio = pointToValueRatio;
            _customPointStyle = customPointStyle;
            _defaultPointStyle = new PointStyle(PointSymbolType.Circle, new GeoSolidBrush(pointColor), pointSize);
            _dotDensityCache = new Dictionary<string, Collection<Vertex>>();
        }

        public PointStyle CustomPointStyle
        {
            get { return _customPointStyle; }
            set { _customPointStyle = value; }
        }

        public string ColumnName
        {
            get { return _columnName; }
            set { _columnName = value; }
        }

        public int PointSize
        {
            get { return (int)_defaultPointStyle.SymbolSize; }
            set { _defaultPointStyle.SymbolSize = value; }
        }

        public GeoColor PointColor
        {
            get { return _defaultPointStyle.SymbolSolidBrush.Color; }
            set { _defaultPointStyle.SymbolSolidBrush.Color = value; }
        }

        public double PointToValueRatio
        {
            get { return _pointToValueRatio; }
            set { _pointToValueRatio = value; }
        }

        public Dictionary<string, Collection<Vertex>> CachedPoints
        {
            get { return _dotDensityCache; }
        }

        protected override Collection<string> GetRequiredColumnNamesCore()
        {
            Validators.CheckParameterIsNotNullOrEmpty(_columnName, "columnName");

            Collection<string> requiredFieldNames = new Collection<string>();
            requiredFieldNames.Add(_columnName);

            return requiredFieldNames;
        }

        protected override void DrawCore(IEnumerable<Feature> features, BaseGeoCanvas canvas, Collection<SimpleCandidate> labelsInThisLayer, Collection<SimpleCandidate> labelsInAllLayers)
        {
            Validators.CheckParameterIsNotNull(features, "features");
            Validators.CheckParameterIsNotNull(canvas, "canvas");
            Validators.CheckParameterIsNotNull(labelsInThisLayer, "labelsInThisLayer");
            Validators.CheckParameterIsNotNull(labelsInAllLayers, "labelsInAllLayers");
            Validators.CheckGeoCanvasIsInDrawing(canvas.IsDrawing);

            Collection<Vertex> result = null;
            if (_dotDensityCache == null)
            {
                _dotDensityCache = new Dictionary<string, Collection<Vertex>>();
            }
            foreach (Feature feature in features)
            {
                if (_dotDensityCache.ContainsKey(feature.Id))
                {
                    result = _dotDensityCache[feature.Id];
                }
                else
                {
                    int count = 0;
                    double value = 0;

                    if (!Double.TryParse(feature.ColumnValues[_columnName], NumberStyles.Any, CultureInfo.InvariantCulture, out value))
                    {
                        throw new ArgumentException(ExceptionDescription.TheFieldTtypeShouldBeNumeric, _columnName);
                    }

                    count = Convert.ToInt32(value, CultureInfo.InvariantCulture);

                    if (count == 0)
                    {
                        continue;
                    }

                    count = (int)(count * _pointToValueRatio);

                    result = GetDots(feature, canvas, count);

                    _dotDensityCache.Add(feature.Id, result);
                }

                Dictionary<string, string> fieldValues = new Dictionary<string, string>();

                for (int i = 0; i < result.Count; i++)
                {
                    DrawPoint(result[i].X, result[i].Y, canvas, fieldValues, labelsInThisLayer, labelsInAllLayers);
                }
            }
        }

        private void DrawPoint(double worldX, double worldY, BaseGeoCanvas canvas, Dictionary<string, string> columnValues, Collection<SimpleCandidate> labelsInThisLayer, Collection<SimpleCandidate> labelsInAllLayers)
        {
            byte[] wkb = new byte[21];
            ArrayHelper.CopyToArray(new byte[] { 1, 1, 0, 0, 0 }, wkb, 0);
            ArrayHelper.CopyToArray(BitConverter.GetBytes(worldX), wkb, FirstPointPosition);
            ArrayHelper.CopyToArray(BitConverter.GetBytes(worldY), wkb, SecondPointPosition);

            Feature[] tmpFeatures = new Feature[1] { new Feature(wkb, string.Empty, columnValues) };
            if (_customPointStyle != null)
            {
                _customPointStyle.Draw(tmpFeatures, canvas, labelsInThisLayer, labelsInAllLayers);
            }
            else
            {
                _defaultPointStyle.Draw(tmpFeatures, canvas, labelsInThisLayer, labelsInAllLayers);
            }
        }

        private static Collection<Vertex> GetDots(Feature feature, BaseGeoCanvas canvas, int count)
        {
            Collection<Vertex> resultDots = new Collection<Vertex>();

            BaseShape shape = BaseShape.CreateShapeFromWellKnownData(feature.GetWellKnownBinary());

            WellKnownType type = feature.GetWellKnownType();

            if (type == WellKnownType.Polygon)
            {
                PolygonShape polygon = shape as PolygonShape;

                GetDotsFromOnePolygon(polygon, canvas, resultDots, count);
            }
            else if (type == WellKnownType.Multipolygon)
            {
                MultipolygonShape multiPolygon = shape as MultipolygonShape;
                Collection<PolygonShape> polygons = multiPolygon.Polygons;

                double[] areaList = new double[polygons.Count];

                for (int i = 0; i < polygons.Count; i++)
                {
                    areaList[i] = polygons[i].GetArea(GeographyUnit.Meter, AreaUnit.SquareMeters);
                }

                double sumArea = 0;

                for (int i = 0; i < areaList.Length; i++)
                {
                    sumArea += areaList[i];
                }

                for (int i = 0; i < polygons.Count; i++)
                {
                    GetDotsFromOnePolygon(polygons[i], canvas, resultDots, (int)Math.Round(count * areaList[i] / sumArea));
                }
            }

            return resultDots;
        }

        private static void GetDotsFromOnePolygon(PolygonShape polygon, BaseGeoCanvas canvas, Collection<Vertex> resultDots, int count)
        {
            PolygonShape screenPolygon = ConvertToScreenPolygon(polygon, canvas);
            RectangleShape extentInScreen = screenPolygon.GetBoundingBox();

            double upperLeftX = extentInScreen.UpperLeftPoint.X;
            double upperLeftY = extentInScreen.UpperLeftPoint.Y;
            double lowerRightX = extentInScreen.LowerRightPoint.X;
            double lowerRightY = extentInScreen.LowerRightPoint.Y;

            double worldUpperLeftX = canvas.CurrentWorldExtent.UpperLeftPoint.X;
            double worldUpperLeftY = canvas.CurrentWorldExtent.UpperLeftPoint.Y;
            double worldWidth = canvas.CurrentWorldExtent.Width;
            double worldHeight = canvas.CurrentWorldExtent.Height;

            double toWorldFactorX = worldWidth / canvas.Width;
            double toWorldFactorY = worldHeight / canvas.Height;

            Random random = new Random(BitConverter.ToInt32(Guid.NewGuid().ToByteArray(), 0));

            int i = 0;

            while (i < count)
            {
                double randomX = random.Next((int)upperLeftX, (int)lowerRightX) + random.NextDouble();
                double randomY = random.Next((int)lowerRightY, (int)upperLeftY) + random.NextDouble();

                if (IsPointInsidePolygon(screenPolygon, randomX, randomY, extentInScreen))
                {
                    i++;

                    double worldX = worldUpperLeftX + randomX * toWorldFactorX;
                    double worldY = worldUpperLeftY - randomY * toWorldFactorY;

                    resultDots.Add(new Vertex(worldX, worldY));
                }
            }
        }

        private static PolygonShape ConvertToScreenPolygon(PolygonShape polygon, BaseGeoCanvas canvas)
        {
            double extentWidth = canvas.CurrentWorldExtent.Width;
            double extentHeight = canvas.CurrentWorldExtent.Height;
            double upperLeftX = canvas.CurrentWorldExtent.UpperLeftPoint.X;
            double upperLeftY = canvas.CurrentWorldExtent.UpperLeftPoint.Y;

            double toScreenFactorX = canvas.Width / extentWidth;
            double toScreenFactorY = canvas.Height / extentHeight;

            PolygonShape screenPolygon = new PolygonShape();
            Collection<Vertex> newVertices = screenPolygon.OuterRing.Vertices;
            Collection<Vertex> outerVertices = polygon.OuterRing.Vertices;

            for (int i = 0; i < outerVertices.Count; i++)
            {
                double screenX = (outerVertices[i].X - upperLeftX) * toScreenFactorX;
                double screenY = (upperLeftY - outerVertices[i].Y) * toScreenFactorY;

                newVertices.Add(new Vertex(screenX, screenY));
            }

            foreach (RingShape ringShape in polygon.InnerRings)
            {
                Collection<Vertex> innerRing = new Collection<Vertex>();

                for (int i = 0; i < ringShape.Vertices.Count; i++)
                {
                    double screenX = (ringShape.Vertices[i].X - upperLeftX) * toScreenFactorX;
                    double screenY = (upperLeftY - ringShape.Vertices[i].Y) * toScreenFactorY;

                    innerRing.Add(new Vertex(screenX, screenY));
                }

                screenPolygon.InnerRings.Add(new RingShape(innerRing));
            }

            return screenPolygon;
        }

      

        private static bool IsPointInsidePolygon(PolygonShape polygon, double pointX, double pointY, RectangleShape boundingBox)
        {
            double upperLeftX = boundingBox.UpperLeftPoint.X;
            double upperLeftY = boundingBox.UpperLeftPoint.Y;
            double lowerRightX = boundingBox.LowerRightPoint.X;
            double lowerRightY = boundingBox.LowerRightPoint.Y;

            if (pointX < upperLeftX || pointX > lowerRightX || pointY > upperLeftY || pointY < lowerRightY)
            {
                return false;
            }

            Collection<Vertex> crossingPoints = new Collection<Vertex>();

            GetCrossingPoints(crossingPoints, polygon.OuterRing, pointX, pointY, pointX + boundingBox.Width);

            foreach (RingShape ringShape in polygon.InnerRings)
            {
                GetCrossingPoints(crossingPoints, ringShape, pointX, pointY, pointX + boundingBox.Width);
            }

            if (crossingPoints.Count != 0)
            {
                return (Math.IEEERemainder(crossingPoints.Count, 2) != 0);
            }
            return false;
        }

        private static void GetCrossingPoints(Collection<Vertex> crossingPoints, RingShape ringShape, double pointX, double pointY, double secondLineEndPointX)
        {
            Collection<Vertex> outerVertices = ringShape.Vertices;

            for (int i = 1; i < outerVertices.Count; i++)
            {
                Vertex startPoint = outerVertices[i - 1];
                Vertex endPoint = outerVertices[i];
                Vertex interPoint = GetPointFFromLineSegmentIntersection(startPoint, endPoint, pointX, pointY, secondLineEndPointX);

                if (IsPointValid(interPoint) && IsPointNotDuplicate(crossingPoints, interPoint))
                {
                    crossingPoints.Add(interPoint);
                }
            }
        }

        private static Vertex GetPointFFromLineSegmentIntersection(Vertex startPoint, Vertex endPoint, double secondLineStartX, double secondLineStartY, double secondLineEndPointX)
        {
            double x1 = startPoint.X;
            double x2 = endPoint.X;
            double y1 = startPoint.Y;
            double y2 = endPoint.Y;
            double xp1 = secondLineStartX;
            double xp2 = secondLineEndPointX;

            if (y1 != y2)
            {
                double a = (x2 - x1) / (y2 - y1);
                double b = x1 - (a * y1);
                double yi = secondLineStartY;
                double xi = (a * yi) + b;

                if (IsInRange(yi, y1, y2) && IsInRange(xi, x1, x2) && IsInRange(xi, xp1, xp2))
                {
                    return new Vertex(xi, yi);
                }
            }

            return new Vertex(double.MinValue, double.MinValue);
        }

        private static bool IsPointNotDuplicate(Collection<Vertex> crossingPoints, Vertex vertex)
        {
            for (int i = 0; i < crossingPoints.Count; i++)
            {
                if (Math.Round(crossingPoints[i].X, 12) == Math.Round(vertex.X, 12) && Math.Round(crossingPoints[i].Y, 12) == Math.Round(vertex.Y, 12))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsInRange(double value, double start, double end)
        {
            return (value >= start && value <= end) || (value <= start && value >= end);
        }

        private static bool IsPointValid(Vertex vertex)
        {
            return (vertex.X != double.MinValue || vertex.Y != double.MinValue);
        }
    }
}
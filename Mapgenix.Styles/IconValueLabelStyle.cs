using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Mapgenix.Canvas;
using Mapgenix.Shapes;

namespace Mapgenix.Styles
{

    [Serializable]
    public class IconValueLabelStyle : BaseLabelStyle
    {
        private const double DefaultFittingPolygonFactor = 1;
        private const int DefaultGridSize = 100;
        private const double DefaultTextLineSegmentRatio = 0.9;
        private string _columnName;
        private Dictionary<int, GeoImage> _geoImageCache;
        private Dictionary<int, int> _geoImageHeightCache;
        private Dictionary<int, int> _geoImageWidthCache;
        private Collection<IconValueItem> _iconValueItems;
        private int _previousValueItemId;

        public IconValueLabelStyle()
            : this(string.Empty, new IconValueItem[] { })
        {
        }

       
        public IconValueLabelStyle(string columnName)
            : this(columnName, new IconValueItem[] { })
        {
        }

        public IconValueLabelStyle(string columnName, IEnumerable<IconValueItem> iconValueItems)
        {
            GridSize = DefaultGridSize;
            TextLineSegmentRatio = DefaultTextLineSegmentRatio;
            FittingPolygonFactor = DefaultFittingPolygonFactor;
            _previousValueItemId = -1;
            _columnName = columnName;
            _iconValueItems = new Collection<IconValueItem>(new List<IconValueItem>(iconValueItems));
        }

        public new int GridSize
        {
            get { return base.GridSize; }
            set { base.GridSize = value; }
        }

        public string ColumnName
        {
            get { return _columnName; }
            set { _columnName = value; }
        }

       
        public new LabelDuplicateRule DuplicateRule
        {
            get { return base.DuplicateRule; }
            set { base.DuplicateRule = value; }
        }

        public new LabelOverlappingRule OverlappingRule
        {
            get { return base.OverlappingRule; }
            set { base.OverlappingRule = value; }
        }

        public new bool AllowLineCarriage
        {
            get { return base.AllowLineCarriage; }
            set { base.AllowLineCarriage = value; }
        }

        public new bool ForceLineCarriage
        {
            get { return base.ForceLineCarriage; }
            set { base.ForceLineCarriage = value; }
        }

        public new bool FittingPolygon
        {
            get { return base.FittingPolygon; }
            set { base.FittingPolygon = value; }
        }

        public new PolygonLabelingLocationMode PolygonLabelingLocationMode
        {
            get { return base.PolygonLabelingLocationMode; }
            set { base.PolygonLabelingLocationMode = value; }
        }

       
        public new bool LabelAllPolygonParts
        {
            get { return base.LabelAllPolygonParts; }
            set { base.LabelAllPolygonParts = value; }
        }

        public new bool LabelAllLineParts
        {
            get { return base.LabelAllLineParts; }
            set { base.LabelAllLineParts = value; }
        }

      
        public new double FittingPolygonFactor
        {
            get { return base.FittingPolygonFactor; }
            set { base.FittingPolygonFactor = value; }
        }

        public new double TextLineSegmentRatio
        {
            get { return base.TextLineSegmentRatio; }
            set { base.TextLineSegmentRatio = value; }
        }

      
        public new bool BestPlacement
        {
            get { return base.BestPlacement; }
            set { base.BestPlacement = value; }
        }

       
        public new PointPlacement PointPlacement
        {
            get { return base.PointPlacement; }
            set { base.PointPlacement = value; }
        }

        public Collection<IconValueItem> IconValueItems
        {
            get { return _iconValueItems; }
        }

      public new bool SuppressPartialLabels
        {
            get { return base.SuppressPartialLabels; }
            set { base.SuppressPartialLabels = value; }
        }

        private bool IsStyleDefault
        {
            get
            {
                if (!IsActive)
                {
                    return true;
                }

                bool isDefault = false;

                if (TextSolidBrush.Color.IsTransparent)
                {
                    isDefault = true;
                }

                if (isDefault && Advanced.TextCustomBrush != null)
                {
                    isDefault = false;
                }

                return isDefault;
            }
        }

     
        protected override void DrawCore(IEnumerable<Feature> features, BaseGeoCanvas canvas, Collection<SimpleCandidate> labelsInThisLayer, Collection<SimpleCandidate> labelsInAllLayers)
        {
            Validators.CheckParameterIsNotNull(features, "features");
            Validators.CheckParameterIsNotNull(canvas, "canvas");
            Validators.CheckParameterIsNotNull(labelsInThisLayer, "labelsInThisLayer");
            Validators.CheckParameterIsNotNull(labelsInAllLayers, "labelsInAllLayers");
            Validators.CheckGeoCanvasIsInDrawing(canvas.IsDrawing);

            if (IconValueItems.Count == 0)
            {
                return;
            }

            _previousValueItemId = -1;
            _geoImageCache = new Dictionary<int, GeoImage>();
            _geoImageWidthCache = new Dictionary<int, int>();
            _geoImageHeightCache = new Dictionary<int, int>();

            try
            {
                Collection<Feature> candidateFeatures = FilterFeatures(features, canvas);

                foreach (Feature feature in candidateFeatures)
                {
                    DrawOneFeature(feature, canvas, labelsInThisLayer, labelsInAllLayers);
                }
            }
            finally
            {
                if (_geoImageWidthCache != null)
                {
                    _geoImageWidthCache.Clear();
                    _geoImageWidthCache = null;
                }
                if (_geoImageHeightCache != null)
                {
                    _geoImageHeightCache.Clear();
                    _geoImageHeightCache = null;
                }
                if (_geoImageCache != null)
                {
                    foreach (GeoImage image in _geoImageCache.Values)
                    {
                        image.Dispose();
                    }
                    _geoImageCache.Clear();
                    _geoImageCache = null;
                }
            }
        }

           
        protected override Collection<string> GetRequiredColumnNamesCore()
        {
            Collection<string> fields = base.GetRequiredColumnNamesCore();
            if (!string.IsNullOrEmpty(_columnName))
            {
                fields.Add(_columnName);
            }
            foreach (IconValueItem item in _iconValueItems)
            {
                Collection<string> tempFields = item.TextStyle.GetRequiredColumnNames();
                foreach (string field in tempFields)
                {
                    if (!string.IsNullOrEmpty(field))
                    {
                        fields.Add(field);
                    }
                }
            }

            return fields;
        }

      
        protected override Collection<LabelingCandidate> GetLabelingCandidateCore(Feature feature, BaseGeoCanvas canvas)
        {
            bool tempForceHorizontalLabelForLine = ForceHorizontalLabelForLine;
            ForceHorizontalLabelForLine = true;
            Collection<LabelingCandidate> labelingCandidates = base.GetLabelingCandidateCore(feature, canvas);
            ForceHorizontalLabelForLine = tempForceHorizontalLabelForLine;

            return labelingCandidates;
        }

        private void DrawOneFeature(Feature feature, BaseGeoCanvas canvas, Collection<SimpleCandidate> labelsInThisLayer, Collection<SimpleCandidate> labelsInAllLayers)
        {
            int key = GetKeyInImageCacheByIconValueItems(feature, canvas);
            if (key == Int32.MaxValue)
            {
                return;
            }

            if (!IsStyleDefault)
            {
                BaseGeoBrush tmpGeoBrush = Advanced.TextCustomBrush;
                if (tmpGeoBrush == null && !TextSolidBrush.Color.IsTransparent)
                {
                    tmpGeoBrush = TextSolidBrush;
                }

                ScreenPointF[] textPathInScreen = new ScreenPointF[1];

                Collection<LabelingCandidate> labelingCandidates = GetLabelingCandidates(feature, canvas);

                RectangleShape canvasScreenExtent = null;
                if (SuppressPartialLabels)
                {
                    canvasScreenExtent = ConvertToScreenShape(new Feature(canvas.CurrentWorldExtent), canvas).GetBoundingBox();
                }
                foreach (LabelingCandidate labelingCandidate in labelingCandidates)
                {
                    GeoImage imageForIcon = _geoImageCache[key];
                    int imageWidth = _geoImageWidthCache[key];
                    int imageHeight = _geoImageHeightCache[key];

                    RectangleShape boundingBoxOfCandidate = labelingCandidate.ScreenArea.GetBoundingBox();
                    PointShape centerPoint = boundingBoxOfCandidate.GetCenterPoint();
                    boundingBoxOfCandidate.ExpandToInclude(GetImageBoundingBox(centerPoint, imageWidth, imageHeight));
                    labelingCandidate.ScreenArea = boundingBoxOfCandidate.ToPolygon();

                    if (CheckDuplicate(labelingCandidate, canvas, labelsInThisLayer, labelsInAllLayers))
                    {
                        continue;
                    }
                    if (CheckOverlapping(labelingCandidate, canvas, labelsInThisLayer, labelsInAllLayers))
                    {
                        continue;
                    }
                    if (SuppressPartialLabels)
                    {
                        if (!canvasScreenExtent.Contains(labelingCandidate.ScreenArea))
                        {
                            continue;
                        }
                    }

                    SimpleCandidate simpleCandidate = new SimpleCandidate(labelingCandidate.OriginalText, labelingCandidate.ScreenArea);
                    if (labelsInAllLayers != null)
                    {
                        labelsInAllLayers.Add(simpleCandidate);
                    }
                    if (labelsInThisLayer != null)
                    {
                        labelsInThisLayer.Add(simpleCandidate);
                    }
                    double worldX = canvas.CurrentWorldExtent.UpperLeftPoint.X + centerPoint.X * canvas.CurrentWorldExtent.Width / canvas.Width;
                    double worldY = canvas.CurrentWorldExtent.UpperLeftPoint.Y - centerPoint.Y * canvas.CurrentWorldExtent.Height / canvas.Height;

                    canvas.DrawWorldImageWithoutScaling(imageForIcon, worldX, worldY, DrawingLevel);

                    if (Mask != null)
                    {
                        Feature[] maskFeatures = { ConvertToWorldCoordinate(labelingCandidate.ScreenArea, canvas) };
                        Mask.Draw(maskFeatures, canvas, null, null);
                    }

                    foreach (LabelInformation labelInfo in labelingCandidate.LabelInformation)
                    {
                        textPathInScreen[0] = new ScreenPointF((float)labelInfo.PositionInScreenCoordinates.X, (float)labelInfo.PositionInScreenCoordinates.Y);
                        canvas.DrawText(labelInfo.Text, Font, tmpGeoBrush, HaloPen, textPathInScreen, DrawingLevel, 0, 0, 0);
                    }
                }
            }
        }

        private static RectangleShape GetImageBoundingBox(PointShape centerPoint, int imageWidth, int imageHeight)
        {
            PointShape upperLeftPoint = new PointShape(centerPoint.X - imageWidth * 0.5, centerPoint.Y + imageHeight * 0.5);
            PointShape lowerRightPoint = new PointShape(centerPoint.X + imageWidth * 0.5, centerPoint.Y - imageHeight * 0.5);

            return new RectangleShape(upperLeftPoint, lowerRightPoint);
        }

        private int GetKeyInImageCacheByIconValueItems(Feature feature, BaseGeoCanvas canvas)
        {
            string fieldValue = feature.ColumnValues[_columnName].Trim();

            for (int i = 0; i < _iconValueItems.Count; i++)
            {
                IconValueItem iconValueItem = _iconValueItems[i];

                if (iconValueItem.TextStyle.IsActive && fieldValue == iconValueItem.FieldValue)
                {
                    string text = feature.ColumnValues[iconValueItem.TextStyle.TextColumnName].Trim();
                    if (string.IsNullOrEmpty(text))
                    {
                        return Int32.MaxValue;
                    }

                    if (!_geoImageCache.ContainsKey(i))
                    {
                        GeoImage image = iconValueItem.GetIconImage();

                        if (image != null)
                        {
                            Stream tempStream = image.GetImageStream(canvas);
                            byte[] imageBytes = new byte[tempStream.Length];
                            tempStream.Read(imageBytes, 0, imageBytes.Length);
                            MemoryStream imageStream = new MemoryStream();
                            imageStream.Write(imageBytes, 0, imageBytes.Length);

                            image = new GeoImage(imageStream);
                        }
                        _geoImageCache.Add(i, image);

                        _geoImageWidthCache.Add(i, image.GetWidth());
                        _geoImageHeightCache.Add(i, image.GetHeight());
                    }


                    if (_previousValueItemId != i)
                    {
                        _previousValueItemId = i;
                        SetEachProperties(iconValueItem.TextStyle);
                    }

                    if (text.Length >= iconValueItem.TextValueLengthMin &&
                        text.Length <= iconValueItem.TextValueLengthMax)
                    {
                        return i;
                    }
                }
            }

            return Int32.MaxValue;
        }

        private static Feature ConvertToWorldCoordinate(PolygonShape simplyPolygon, BaseGeoCanvas canvas)
        {
            double upperLeftX = canvas.CurrentWorldExtent.UpperLeftPoint.X;
            double upperLeftY = canvas.CurrentWorldExtent.UpperLeftPoint.Y;
            double canvasWidth = canvas.Width;
            double canvasHeight = canvas.Height;
            double extentWidth = canvas.CurrentWorldExtent.Width;
            double extentHeight = canvas.CurrentWorldExtent.Height;

            double widthFactor = extentWidth / canvasWidth;
            double heightFactor = extentHeight / canvasHeight;

            int count = simplyPolygon.InnerRings.Count + 1;
            RingShape ringShape = null;

            int verticesCount = 0;

            for (int i = 0; i < count; i++)
            {
                ringShape = (i == 0) ? simplyPolygon.OuterRing : simplyPolygon.InnerRings[i - 1];

                verticesCount += ringShape.Vertices.Count;
            }

            byte[] wellKnownBinary = new byte[9 + count * 4 + verticesCount * 16];
            byte[] header = new byte[5] { 1, 3, 0, 0, 0 };
            CopyToArray(header, wellKnownBinary, 0);
            CopyToArray(BitConverter.GetBytes(count), wellKnownBinary, 5);
            int index = 9;

            for (int i = 0; i < count; i++)
            {
                ringShape = (i == 0) ? simplyPolygon.OuterRing : simplyPolygon.InnerRings[i - 1];

                CopyToArray(BitConverter.GetBytes(ringShape.Vertices.Count), wellKnownBinary, index);
                index += 4;

                for (int j = 0; j < ringShape.Vertices.Count; j++)
                {
                    double pointX = ringShape.Vertices[j].X;
                    double pointY = ringShape.Vertices[j].Y;

                    double worldPointX = pointX * widthFactor + upperLeftX;
                    double worldPointY = upperLeftY - pointY * heightFactor;

                    CopyToArray(BitConverter.GetBytes(worldPointX), wellKnownBinary, index);
                    index += 8;
                    CopyToArray(BitConverter.GetBytes(worldPointY), wellKnownBinary, index);
                    index += 8;
                }
            }

            return new Feature(wellKnownBinary, string.Empty, new Dictionary<string, string>());
        }

        private static void CopyToArray(byte[] sourceArray, byte[] destinateArray, long destinateIndex)
        {
            for (int i = 0; i < sourceArray.Length; i++)
            {
                destinateArray[destinateIndex + i] = sourceArray[i];
            }
        }

        private void SetEachProperties(LabelStyle textStyle)
        {
            Advanced.TextCustomBrush = textStyle.Advanced.TextCustomBrush;
            Font = textStyle.Font;
            TextFormat = textStyle.TextFormat;
            HaloPen = textStyle.HaloPen;
            Mask = textStyle.Mask;
            MaskMargin = textStyle.MaskMargin;
            TextSolidBrush = textStyle.TextSolidBrush;
            TextColumnName = textStyle.TextColumnName;
            XOffsetInPixel = textStyle.XOffsetInPixel;
            YOffsetInPixel = textStyle.YOffsetInPixel;
        }
    }
}
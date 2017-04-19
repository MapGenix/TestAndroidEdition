using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Mapgenix.Canvas;
using Mapgenix.Shapes;
using Mapgenix.Utils;

namespace Mapgenix.Styles
{
    /// <summary>Style for an icon with text from the data of the feature.</summary>
    [Serializable]
    public class IconStyle : LabelStyle
    {
        private GeoImage _iconImage;
        private double _iconImageScale;
        private string _iconPathFileName;

        public IconStyle()
        {
            _iconImageScale = 1;
        }

        public IconStyle(string iconPathFilename, string textColumnName, GeoFont textFont, GeoSolidBrush textSolidBrush)
            : base(textColumnName, textFont, textSolidBrush)
        {
            _iconPathFileName = iconPathFilename;
            _iconImageScale = 1;
        }

        
        public IconStyle(GeoImage iconImage, string textColumnName, GeoFont textFont, GeoSolidBrush textSolidBrush)
            : base(textColumnName, textFont, textSolidBrush)
        {
            _iconImage = iconImage;
            _iconImageScale = 1;
        }

        public string IconFilePathName
        {
            get { return _iconPathFileName; }
            set { _iconPathFileName = value; }
        }

        public GeoImage IconImage
        {
            get { return _iconImage; }
            set { _iconImage = value; }
        }

        public double IconImageScale
        {
            get { return _iconImageScale; }
            set { _iconImageScale = value; }
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

                bool isDefault = false || TextSolidBrush.Color.IsTransparent;

                if (isDefault && Advanced.TextCustomBrush != null)
                {
                    isDefault = false;
                }

                return isDefault;
            }
        }

       
        protected override void DrawCore(IEnumerable<Feature> features, BaseGeoCanvas canvas, Collection<SimpleCandidate> labelsInThisLayer, Collection<SimpleCandidate> labelsInAllLayers)
        {
            if (_iconImage == null && _iconPathFileName == null)
            {
                base.DrawCore(features, canvas, labelsInThisLayer, labelsInAllLayers);
                return;
            }

            Validators.CheckParameterIsNotNull(features, "feature");
            Validators.CheckParameterIsNotNull(canvas, "canvas");
            Validators.CheckParameterIsNotNull(labelsInThisLayer, "labelsInThisLayer");
            Validators.CheckParameterIsNotNull(labelsInAllLayers, "labelsInAllLayers");
            Validators.CheckGeoCanvasIsInDrawing(canvas.IsDrawing);
            Validators.CheckScaleIsBiggerThanZero(_iconImageScale, "iconImageScale");
        
            GeoImage imageForIcon = null;
            MemoryStream imageStream = new MemoryStream();

            try
            {
                if (_iconPathFileName != null)
                {
                    FileStream imageFile = null;
                    try
                    {
                        imageFile = File.OpenRead(_iconPathFileName);
                        byte[] imageBytes = new byte[imageFile.Length];
                        imageFile.Read(imageBytes, 0, imageBytes.Length);
                        imageStream.Write(imageBytes, 0, imageBytes.Length);
                    }
                    finally
                    {
                        if (imageFile != null)
                        {
                            imageFile.Close();
                            imageFile = null;
                        }
                    }
                }
                else if (_iconImage != null)
                {
                    Stream tempStream = _iconImage.GetImageStream(canvas);
                    byte[] imageBytes = new byte[tempStream.Length];
                    tempStream.Read(imageBytes, 0, imageBytes.Length);
                    imageStream.Write(imageBytes, 0, imageBytes.Length);
                }
                imageForIcon = new GeoImage(imageStream);

                var imageWidth = imageForIcon.GetWidth();
                var imageHeight = imageForIcon.GetHeight();

                Collection<Feature> candidateFeatures = FilterFeatures(features, canvas);

                if (!IsStyleDefault)
                {
                    foreach (Feature feature in candidateFeatures)
                    {
                        DrawOneFeature(imageForIcon, feature, canvas, imageWidth, imageHeight, labelsInThisLayer, labelsInAllLayers);
                    }
                }

                foreach (LabelStyle textStyle in CustomTextStyles)
                {
                    textStyle.Draw(features, canvas, labelsInThisLayer, labelsInAllLayers);
                }
            }
            finally
            {
                if (imageForIcon != null)
                {
                    imageForIcon.Dispose();
                }
            }
        }

       protected override Collection<LabelingCandidate> GetLabelingCandidateCore(Feature feature, BaseGeoCanvas canvas)
        {
            Validators.CheckParameterIsNotNull(canvas, "canvas");
            Validators.CheckGeoCanvasIsInDrawing(canvas.IsDrawing);

            bool tempForceHorizontalLabelForLine = ForceHorizontalLabelForLine;
            ForceHorizontalLabelForLine = true;
            Collection<LabelingCandidate> labelingCandidates = base.GetLabelingCandidateCore(feature, canvas);
            ForceHorizontalLabelForLine = tempForceHorizontalLabelForLine;

            return labelingCandidates;
        }

        private void DrawOneFeature(GeoImage imageForIcon, Feature feature, BaseGeoCanvas canvas, int imageWidth, int imageHeight, Collection<SimpleCandidate> labelsInThisLayer, Collection<SimpleCandidate> labelsInAllLayers)
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

                if (_iconImageScale == 1)
                {
                    canvas.DrawWorldImageWithoutScaling(imageForIcon, worldX, worldY, DrawingLevel);
                }
                else
                {
                    canvas.DrawWorldImage(imageForIcon, worldX, worldY, _iconImageScale, DrawingLevel, XOffsetInPixel, YOffsetInPixel, (float)RotationAngle);
                }

                if (Mask != null)
                {
                    Feature[] maskFeatures = { ConvertToWorldCoordinate(labelingCandidate.ScreenArea, canvas) };
                    Mask.Draw(maskFeatures, canvas, labelsInThisLayer, labelsInAllLayers);
                }

                foreach (LabelInformation labelInfo in labelingCandidate.LabelInformation)
                {
                    textPathInScreen[0] = new ScreenPointF((float)labelInfo.PositionInScreenCoordinates.X, (float)labelInfo.PositionInScreenCoordinates.Y);
                    canvas.DrawText(labelInfo.Text, Font, tmpGeoBrush, HaloPen, textPathInScreen, DrawingLevel, 0, 0, 0);
                }
            }
        }

        private static RectangleShape GetImageBoundingBox(PointShape centerPoint, int imageWidth, int imageHeight)
        {
            PointShape upperLeftPoint = new PointShape(centerPoint.X - imageWidth * 0.5, centerPoint.Y + imageHeight * 0.5);
            PointShape lowerRightPoint = new PointShape(centerPoint.X + imageWidth * 0.5, centerPoint.Y - imageHeight * 0.5);

            return new RectangleShape(upperLeftPoint, lowerRightPoint);
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
            ArrayHelper.CopyToArray(header, wellKnownBinary, 0);
            ArrayHelper.CopyToArray(BitConverter.GetBytes(count), wellKnownBinary, 5);
            int index = 9;

            for (int i = 0; i < count; i++)
            {
                ringShape = (i == 0) ? simplyPolygon.OuterRing : simplyPolygon.InnerRings[i - 1];

                ArrayHelper.CopyToArray(BitConverter.GetBytes(ringShape.Vertices.Count), wellKnownBinary, index);
                index += 4;

                for (int j = 0; j < ringShape.Vertices.Count; j++)
                {
                    double pointX = ringShape.Vertices[j].X;
                    double pointY = ringShape.Vertices[j].Y;

                    double worldPointX = pointX * widthFactor + upperLeftX;
                    double worldPointY = upperLeftY - pointY * heightFactor;

                    ArrayHelper.CopyToArray(BitConverter.GetBytes(worldPointX), wellKnownBinary, index);
                    index += 8;
                    ArrayHelper.CopyToArray(BitConverter.GetBytes(worldPointY), wellKnownBinary, index);
                    index += 8;
                }
            }

            return new Feature(wellKnownBinary, string.Empty, new Dictionary<string, string>());
        }

       
    }
}
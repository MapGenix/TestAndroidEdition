using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using Mapgenix.Canvas;
using Mapgenix.Shapes;

namespace Mapgenix.Styles
{
    /// <summary>A style based on clustering point data into a single point</summary>
    public class ClusterPointStyle : PointStyle
    {
        private int _cellSize = 100;
        private LabelStyle _textSytle = new LabelStyle();

        public ClusterPointStyle()
        { }

        public ClusterPointStyle(GeoImage image)
            : base(image)
        { }

        public ClusterPointStyle(GeoFont characterFont, int characterIndex, GeoSolidBrush characterSolidBrush)
            : base(characterFont, characterIndex, characterSolidBrush)
        { }

        public ClusterPointStyle(PointSymbolType symbolType, GeoSolidBrush symbolSolidBrush, int symbolSize)
            : base(symbolType, symbolSolidBrush, symbolSize)
        { }

        public ClusterPointStyle(PointSymbolType symbolType, GeoSolidBrush symbolSolidBrush, GeoPen symbolPen, int symbolSize)
            : base(symbolType, symbolSolidBrush, symbolPen, symbolSize)
        { }

        public LabelStyle TextStyle
        {
            get { return _textSytle; }
            set { _textSytle = value; }
        }

        public int CellSize
        {
            get { return _cellSize; }
            set { _cellSize = value; }
        }

        protected override void DrawCore(IEnumerable<Feature> features, BaseGeoCanvas canvas, Collection<SimpleCandidate> labelsInThisLayer, Collection<SimpleCandidate> labelsInAllLayers)
        {
            Validators.CheckParameterIsNotNull(features, "features");
            Validators.CheckParameterIsNotNull(canvas, "canvas");

            double scale = ExtentHelper.GetScale(canvas.CurrentWorldExtent, canvas.Width, canvas.MapUnit);

            TileMatrix gSuiteTileMatrix = new TileMatrix(scale, _cellSize, _cellSize, canvas.MapUnit);

            IEnumerable<TileMatrixCell> tileMatricCells = gSuiteTileMatrix.GetContainedCells(canvas.CurrentWorldExtent);

            Dictionary<string, string> unusedFeatures = new Dictionary<string, string>();
            foreach (Feature feature in features)
            {
                unusedFeatures.Add(feature.Id, feature.Id);
            }

            foreach (TileMatrixCell cell in tileMatricCells)
            {
                int featureCount = 0;
                MultipointShape multiPointShape = new MultipointShape();
                foreach (Feature feature in features)
                {             
                    if (unusedFeatures.ContainsKey(feature.Id))
                    {
                        if (cell.BoundingBox.Contains(feature.GetBoundingBox()))
                        {
                            featureCount++;
                            unusedFeatures.Remove(feature.Id);
                            multiPointShape.Points.Add(feature.GetBoundingBox().GetCenterPoint());
                        }
                    }
                }
                if (featureCount > 0)
                {
                    Dictionary<string, string> featureValues = new Dictionary<string, string>();
                    featureValues.Add("FeatureCount", featureCount.ToString(CultureInfo.InvariantCulture));

                    base.DrawCore(new[] { new Feature(multiPointShape.GetCenterPoint(), featureValues) }, canvas, labelsInThisLayer, labelsInAllLayers);

                    _textSytle.Draw(new[] { new Feature(multiPointShape.GetCenterPoint(), featureValues) }, canvas, labelsInThisLayer, labelsInAllLayers);
                }
            }
        }

    }
}

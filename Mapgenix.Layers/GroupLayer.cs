using System;
using System.Collections.ObjectModel;
using Mapgenix.Canvas.Canvas;
using Mapgenix.Canvas.Labels;
using Mapgenix.Collection;
using Mapgenix.Layers.GeoLayers;
using Mapgenix.Layers.GeoLayers.Features;
using Mapgenix.Shapes;

namespace Mapgenix.Layers
{
    
    [Serializable]
    public class GroupLayer : Layer
    {
        SafeCollection<Layer> _layers;

        public SafeCollection<Layer> Layers
        {
            get { return _layers; }
            set { _layers = value; }
        }

        protected override void OpenCore()
        {
            foreach (Layer layer in _layers)
            {
                layer.Open();
            }
        }

        protected override void CloseCore()
        {
            foreach (Layer layer in _layers)
            {
                layer.Close();
            }
        }

        protected override RectangleShape GetBoundingBoxCore()
        {
            Validators.CheckGroupLayerIsEmpty(_layers);

            RectangleShape boundingBox = null;
            foreach (Layer layer in _layers)
            {
                if (layer.HasBoundingBox)
                {
                    if (boundingBox == null)
                    {
                        boundingBox = layer.GetBoundingBox();
                    }
                    else
                    {
                        boundingBox.ExpandToInclude(layer.GetBoundingBox());
                    }
                }
            }

            if (boundingBox == null)
            {
                boundingBox = new RectangleShape(-180, 90, 180, -90);
            }

            return boundingBox;
        }

        protected override void DrawCore(GeoCanvas canvas, Collection<SimpleCandidate> labelsInAllLayers)
        {
            Validators.CheckParameterIsNotNull(canvas, "canvas");

            Collection<SimpleCandidate> labelingInAllLayers = new Collection<SimpleCandidate>();
            foreach (Layer layer in _layers)
            {
                lock (layer)
                {
                    if (!layer.IsOpen)
                    {
                        layer.Open();
                    }

                    VectorLayer featureLayer = layer as VectorLayer;
                    if (featureLayer != null)
                    {
                        featureLayer.DrawingQuality = canvas.DrawingQuality;
                    }
                    layer.Draw(canvas, labelingInAllLayers);
                }
                canvas.Flush();
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Mapgenix.Canvas;
using Mapgenix.Utils;
using Mapgenix.Layers;
using Android.Content;
using Android.Widget;

namespace Mapgenix.GSuite.Android
{
    /// <summary>
    /// Tile that formed from a collection of layers.
    /// </summary>
    [Serializable]
    [ToolboxItem(false)]
    [DesignTimeVisible(false)]
    public class LayerTile : Tile
    {
        private Collection<BaseLayer> _drawingLayers;

        public LayerTile(Context context)
            : this(context, new Collection<BaseLayer>())
        { }

       
        public LayerTile(Context context, IEnumerable<BaseLayer> layers)
            :base (context)
        {
            _drawingLayers = new Collection<BaseLayer>();
            foreach (BaseLayer layer in layers)
            {
                _drawingLayers.Add(layer);
            }
        }

       
        public Collection<BaseLayer> DrawingLayers
        {
            get { return _drawingLayers; }
        }

       
        protected override void DrawCore(BaseGeoCanvas geoCanvas)
        {
            Collection<BaseLayer> layers = new Collection<BaseLayer>();
            lock (_drawingLayers)
            {
                foreach (BaseLayer layer in _drawingLayers)
                {
                    layers.Add(layer);
                }
            }

            geoCanvas.Clear(new GeoSolidBrush(GeoColor.StandardColors.Transparent));
            Collection<SimpleCandidate> labelsInAllLayers = new Collection<SimpleCandidate>();
            foreach (BaseLayer layer in layers)
            {
                if (CancellationPending) { break; }
                lock (layer)
                {
                    try
                    {
                        layer.DrawingProgressChanged += layer_ProgressDrawing;
                        if (!layer.IsOpen)
                        {
                            layer.Open();
                        }
                        geoCanvas.Flush();
                        if (layer.IsVisible)
                        {
                            layer.Draw(geoCanvas, labelsInAllLayers);
                            layer.Close();

                        }
                    }
                    catch(Exception ex)
                    {
                        Toast.MakeText(Context, this.GetType().ToString() + " ERROR:" + ex.Message + " SOURCE: " + ex.Source, ToastLength.Short).Show();
                    }
                    finally
                    {
                        layer.DrawingProgressChanged -= layer_ProgressDrawing;
                    }
                }
            }
        }

        private void layer_ProgressDrawing(object sender, ProgressEventArgs e)
        {
            e.Cancel = CancellationPending;
        }
    }
}

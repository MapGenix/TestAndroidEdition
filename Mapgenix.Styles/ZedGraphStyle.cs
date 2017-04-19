using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Mapgenix.Canvas;
using Mapgenix.Shapes;

namespace Mapgenix.Styles
{
    ///<summary>Style for bar charts, line charts and pie charts</summary>
    public class ZedGraphStyle : BaseStyle
    {
        public event EventHandler<ZedGraphDrawingEventArgs> ZedGraphDrawing;

        public ZedGraphStyle()
        { }

        protected virtual void OnZedGraphDrawing(ZedGraphDrawingEventArgs e)
        {
            EventHandler<ZedGraphDrawingEventArgs> handler = ZedGraphDrawing;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected override void DrawCore(IEnumerable<Feature> features, BaseGeoCanvas canvas, Collection<SimpleCandidate> labeledInLayer, Collection<SimpleCandidate> labeledInLayers)
        {
            foreach (Feature feature in features)
            {
                ZedGraphDrawingEventArgs args = new ZedGraphDrawingEventArgs();
                args.Feature = feature;
                args.LabeledInLayer = labeledInLayer;
                args.LabeledInLayers = labeledInLayers;
                OnZedGraphDrawing(args);

                if (args.Bitmap != null)
                {
                    GeoImage geoImage = GetGeoImageFromImage(args.Bitmap);

                    PointShape centerPoint = feature.GetShape().GetCenterPoint();
                    canvas.DrawWorldImageWithoutScaling(geoImage, centerPoint.X, centerPoint.Y, DrawingLevel.LevelOne, 0f, 0f, 0f);

                    if (geoImage != null) { geoImage.Dispose(); }
                }
            }
        }

        private GeoImage GetGeoImageFromImage(Image bitmap)
        {
            Stream bitmapStream = new MemoryStream();
            bitmap.Save(bitmapStream, ImageFormat.Tiff);

            return new GeoImage(bitmapStream);
        }
    }
}

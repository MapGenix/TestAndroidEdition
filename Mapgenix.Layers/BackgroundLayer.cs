using System;
using System.Collections.ObjectModel;
using Mapgenix.Canvas;

namespace Mapgenix.Layers
{
    /// <summary>Background layer of the map.</summary>
    [Serializable]
    public class BackgroundLayer : BaseLayer
    {
        BaseGeoBrush _backgroundBrush;

		public BaseGeoBrush BackgroundBrush
        {
            get { return _backgroundBrush; }
            set
            {
                Validators.CheckParameterIsNotNull(value, "GeoBrush");
                _backgroundBrush = value;
            }
        }

        protected override void DrawCore(BaseGeoCanvas canvas, Collection<SimpleCandidate> labelsInAllLayers)
        {
            Validators.CheckParameterIsNotNull(canvas, "canvas");
           
            DrawingQuality drawingQuality = canvas.DrawingQuality;

            canvas.DrawingQuality = DrawingQuality.HighSpeed;
            canvas.DrawArea(canvas.CurrentWorldExtent, BackgroundBrush, DrawingLevel.LevelOne);
            canvas.DrawingQuality = drawingQuality;
        }
    }
}
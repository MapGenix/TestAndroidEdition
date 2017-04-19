using System;
using System.Collections.ObjectModel;
using Mapgenix.Canvas;

namespace Mapgenix.Layers
{
    /// <summary>
    /// Adorment layer for logo on the map.
    /// </summary>
    [Serializable]
    public class LogoAdornmentLayer : BaseAdornmentLayer
    {
        public GeoImage Image { get; set; }
        
        protected override void DrawCore(BaseGeoCanvas canvas, Collection<SimpleCandidate> labelsInAllLayers)
        {
            AdornmentDrawHelper.DrawLogoAdornmentLayer(this,canvas,labelsInAllLayers);
        }
    }
}
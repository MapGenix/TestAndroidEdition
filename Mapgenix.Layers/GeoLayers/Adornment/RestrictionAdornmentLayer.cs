using System;
using System.Collections.ObjectModel;
using Mapgenix.Canvas;

namespace Mapgenix.Layers
{
    [Serializable]
    public class RestrictionAdornmentLayer : BaseAdornmentLayer
    {
        

        public AreaStyle RestrictionAreaStyle { get; set; }
        
        protected override void DrawCore(BaseGeoCanvas canvas, Collection<SimpleCandidate> labelsInAllLayers)
        {            
            AdornmentDrawHelper.DrawRestrictionAdornmentLayer(this,canvas,labelsInAllLayers);
            AdornmentDrawHelper.DrawAdornmentCore(this,canvas);

            
        }
    }
}

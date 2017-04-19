using System;
using System.Collections.Generic;


namespace Mapgenix.Layers
{
   
    [Serializable]
    public class AdornmentLayersEventArgs : EventArgs
    {
        IEnumerable<BaseAdornmentLayer> _adornmentLayers;

       
        public AdornmentLayersEventArgs(IEnumerable<BaseAdornmentLayer> adornmentLayers)
        {
            _adornmentLayers = adornmentLayers;
        }

        public IEnumerable<BaseAdornmentLayer> AdornmentLayers
        {
            get { return _adornmentLayers; }
            set { _adornmentLayers = value; }
        }
    }
}

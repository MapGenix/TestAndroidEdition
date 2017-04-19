using System;

namespace Mapgenix.Layers
{
   
    [Serializable]
    public class AdornmentLayerEventArgs : EventArgs
    {
        BaseAdornmentLayer _adornmentLayer;

        public AdornmentLayerEventArgs()
            : this(null)
        { }

        public AdornmentLayerEventArgs(BaseAdornmentLayer adornmentLayer)
        {
            _adornmentLayer = adornmentLayer;
        }

        public BaseAdornmentLayer AdornmentLayer
        {
            get { return _adornmentLayer; }
            set { _adornmentLayer = value; }
        }
    }
}

using System;
using Mapgenix.Shapes;

namespace Mapgenix.Layers
{
    [Serializable]
    public class LayerEventArgs : EventArgs
    {
        BaseLayer _layer;
        RectangleShape _extent;
        Object _image;
        bool _cancel;

        public LayerEventArgs()
            : this(null,new RectangleShape(),null)
        { }

        public LayerEventArgs(BaseLayer currentLayer, RectangleShape worldExtent, object nativeImage)
        {
            _layer = currentLayer;
            _extent = worldExtent;
            _image = nativeImage;
        }

        public bool Cancel
        {
            get { return _cancel; }
            set { _cancel = value; }
        }

        public BaseLayer CurrentLayer
        {
            get { return _layer; }
            set { _layer = value; }
        }

        public RectangleShape WorldExtent
        {
            get { return _extent; }
            set { _extent = value; }
        }

        public Object NativeImage
        {
            get { return _image; }
            set { _image = value; }
        }
    }
}

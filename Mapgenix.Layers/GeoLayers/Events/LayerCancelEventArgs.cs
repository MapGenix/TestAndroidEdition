using System;
using Mapgenix.Shapes;

namespace Mapgenix.Layers.GeoLayers.Events
{
   
    [Serializable]
    public class LayerCancelEventArgs : EventArgs
    {
        Layer _layer;
        RectangleShape _extent;
        Object _image;
        bool _cancel;

        public LayerCancelEventArgs()
            : this(null,new RectangleShape(),null)
        { }

       
        public LayerCancelEventArgs(Layer currentLayer, RectangleShape worldExtent, object nativeImage)
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

        public Layer CurrentLayer
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

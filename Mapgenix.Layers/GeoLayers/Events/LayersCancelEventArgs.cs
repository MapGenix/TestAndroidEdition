using System;
using System.Collections.Generic;
using Mapgenix.Shapes;

namespace Mapgenix.Layers.GeoLayers.Events
{
   
    [Serializable]
    public class LayersCancelEventArgs : EventArgs
    {
        IEnumerable<Layer> _layers;
        RectangleShape _extent;
        Object _image;
        bool _cancel;

        public LayersCancelEventArgs()
            : this(new Layer[] { }, new RectangleShape(), null)
        { }

        public LayersCancelEventArgs(IEnumerable<Layer> layers, RectangleShape worldExtent, object nativeImage)
        {
            _layers = layers;
            _extent = worldExtent;
            _image = nativeImage;
        }

        
        public bool Cancel
        {
            get { return _cancel; }
            set { _cancel = value; }
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

        public IEnumerable<Layer> Layers
        {
            get { return _layers; }
            set { _layers = value; }
        }
    }
}

using System;
using System.Collections.Generic;
using Mapgenix.Shapes;

namespace Mapgenix.Layers
{
    [Serializable]
    public class LayersEventArgs : EventArgs
    {
        IEnumerable<BaseLayer> _layers;
        RectangleShape _extent;
        Object _image;
        bool _cancel;

       
        public LayersEventArgs()
            : this(new BaseLayer[] { }, new RectangleShape(), null)
        { }

        public LayersEventArgs(IEnumerable<BaseLayer> layers, RectangleShape worldExtent, object nativeImage)
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

        public IEnumerable<BaseLayer> Layers
        {
            get { return _layers; }
            set { _layers = value; }
        }
    }
}

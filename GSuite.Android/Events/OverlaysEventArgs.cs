using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Mapgenix.Shapes;

namespace Mapgenix.GSuite.Android
{
    [Serializable]
    public class OverlaysEventArgs : EventArgs
    {
        private IEnumerable<BaseOverlay> _overlays;
        private RectangleShape _worldExtent;
        private bool _cancel;

        public OverlaysEventArgs()
            : this(new Collection<BaseOverlay>(), new RectangleShape())
        { }

        public OverlaysEventArgs(IEnumerable<BaseOverlay> overlays, RectangleShape worldExtent)
        {
            this._overlays = overlays;
            this._worldExtent = worldExtent;
            this._cancel = false;
        }

        public bool Cancel
        {
            get { return _cancel; }
            set { _cancel = value; }
        }

        public RectangleShape WorldExtent
        {
            get { return _worldExtent; }
            set { _worldExtent = value; }
        }

        public IEnumerable<BaseOverlay> Overlays
        {
            get { return _overlays; }
        }
    }
}

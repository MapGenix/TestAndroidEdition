using System;
using Mapgenix.Shapes;

namespace Mapgenix.GSuite.Android
{
    [Serializable]
    public class OverlayEventArgs : EventArgs
    {
        private BaseOverlay _overlay;
        private bool _cancel;
        private RectangleShape _targetExtent;

        public OverlayEventArgs(BaseOverlay overlay, RectangleShape targetExtent)
        {
            this._cancel = false;
            this._overlay = overlay;
            this._targetExtent = targetExtent;
        }

        public BaseOverlay Overlay
        {
            get { return _overlay; }
            set { _overlay = value; }
        }

        public bool Cancel
        {
            get { return _cancel; }
            set { _cancel = value; }
        }

        public RectangleShape Extent
        {
            get { return _targetExtent; }
            set { _targetExtent = value; }
        }
    }
}

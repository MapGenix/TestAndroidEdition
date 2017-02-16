using System;
using Mapgenix.Shapes;

namespace Mapgenix.GSuite.Android
{

    [Serializable]
    public class PointShapeEventArgs : EventArgs
    {
        private PointShape _targetPointShape;
        private bool _cancel;

        public PointShapeEventArgs()
            : this(null)
        {
        }

        public PointShapeEventArgs(PointShape targetPointShape)
        {
            this._cancel = false;
            this._targetPointShape = targetPointShape;
        }

        public PointShape Target
        {
            get { return _targetPointShape; }
            set { _targetPointShape = value; }
        }

        public bool Cancel
        {
            get { return _cancel; }
            set { _cancel = value; }
        }
    }
}
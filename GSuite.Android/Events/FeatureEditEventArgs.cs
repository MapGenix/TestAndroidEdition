using System;
using Mapgenix.Shapes;

namespace Mapgenix.GSuite.Android
{
    [Serializable]
    public class FeatureEditEventArgs : EventArgs
    {
        private Feature _draggingFeature;
        private PointShape _sourceControlPoint;
        private PointShape _targetControlPoint;
        private bool _cancel;

        public FeatureEditEventArgs(Feature draggingFeature, PointShape sourceControlPoint, PointShape targetControlPoint)
        {
            this._draggingFeature = draggingFeature;
            this._cancel = false;
            this._sourceControlPoint = sourceControlPoint;
            this._targetControlPoint = targetControlPoint;
        }

        public Feature Feature
        {
            get { return _draggingFeature; }
            set { _draggingFeature = value; }
        }

        public PointShape Source
        {
            get { return _sourceControlPoint; }
            set { _sourceControlPoint = value; }
        }

        public PointShape Target
        {
            get { return _targetControlPoint; }
            set { _targetControlPoint = value; }
        }

        public bool Cancel
        {
            get { return _cancel; }
            set { _cancel = value; }
        }
    }
}
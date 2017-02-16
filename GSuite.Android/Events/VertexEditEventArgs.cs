using System;
using Mapgenix.Shapes;

namespace Mapgenix.GSuite.Android
{
    [Serializable]
    public class VertexEditEventArgs : EventArgs
    {
        private PointShape _targetVertex;
        private Feature _affectedVertexFeature;
        private Vertex _movingVertex;
        private bool _cancel;

        public VertexEditEventArgs()
            : this(new Feature(), new Vertex(), new PointShape())
        {
        }

        public VertexEditEventArgs(Feature affectedVertexFeature, Vertex movingVertex)
            : this(affectedVertexFeature, movingVertex, new PointShape())
        { }

        public VertexEditEventArgs(Feature affectedVertexFeature, Vertex movingVertex, PointShape targetVertex)
        {
            this._cancel = false;
            this._movingVertex = movingVertex;
            this._affectedVertexFeature = affectedVertexFeature;
            this._targetVertex = targetVertex;
        }

        public PointShape TargetVertex
        {
            get { return _targetVertex; }
            set { _targetVertex = value; }
        }

        public Feature Feature
        {
            get { return _affectedVertexFeature; }
            set { _affectedVertexFeature = value; }
        }

        public Vertex MovingVertex
        {
            get { return _movingVertex; }
            set { _movingVertex = value; }
        }

        public bool Cancel
        {
            get { return _cancel; }
            set { _cancel = value; }
        }
    }
}
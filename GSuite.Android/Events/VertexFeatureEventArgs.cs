using System;
using Mapgenix.Shapes;

namespace Mapgenix.GSuite.Android
{
    [Serializable]
    public class VertexFeatureEventArgs : EventArgs
    {
        private bool _cancel;
        private Vertex _addingVertex;
        private Feature _affectedFeature;

        public VertexFeatureEventArgs(Feature affectedFeature, Vertex addingVertex)
        {
            this._cancel = false;
            this._addingVertex = addingVertex;
            this._affectedFeature = affectedFeature;
        }

        public VertexFeatureEventArgs(Vertex addingVertex, Feature affectedFeature)
        {
            this._cancel = false;
            this._addingVertex = addingVertex;
            this._affectedFeature = affectedFeature;
        }

        public bool Cancel
        {
            get { return _cancel; }
            set { _cancel = value; }
        }

        public Vertex Vertex
        {
            get { return _addingVertex; }
            set { _addingVertex = value; }
        }

        public Feature Feature
        {
            get { return _affectedFeature; }
            set { _affectedFeature = value; }
        }
    }
}
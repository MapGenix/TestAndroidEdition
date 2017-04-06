using System;
using Mapgenix.Shapes;

namespace Mapgenix.GSuite.Android
{
    [Serializable]
    public class VertexEventArgs : EventArgs
    {
        private bool _cancel;
        private Vertex _startingVertex;

        public VertexEventArgs(Vertex startingVertex)
        {
            this._cancel = false;
            this._startingVertex = startingVertex;
        }

        public bool Cancel
        {
            get { return _cancel; }
            set { _cancel = value; }
        }

        public Vertex Vertex
        {
            get { return _startingVertex; }
            set { _startingVertex = value; }
        }
    }
}

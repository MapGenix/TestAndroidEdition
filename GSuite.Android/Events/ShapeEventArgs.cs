using System;
using Mapgenix.Shapes;

namespace Mapgenix.GSuite.Android
{
    [Serializable]
    public class ShapeEventArgs : EventArgs
    {
        private BaseShape _shape;
        private bool _cancel;

        public ShapeEventArgs(BaseShape shape)
        {
            this._shape = shape;
            this._cancel = false;
        }

        public BaseShape Shape
        {
            get { return _shape; }
            set { _shape = value; }
        }

        public bool Cancel
        {
            get { return _cancel; }
            set { _cancel = value; }
        }
    }
}

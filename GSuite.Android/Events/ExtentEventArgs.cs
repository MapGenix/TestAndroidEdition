using System;
using Mapgenix.Shapes;

namespace Mapgenix.GSuite.Android
{
    [Serializable]
    public class ExtentEventArgs : EventArgs
    {
        public ExtentEventArgs(RectangleShape worldExtent)
        {
            Extent = worldExtent;
            Cancel = false;
        }

        public RectangleShape Extent { get; set; }

        public bool Cancel { get; set; }
    }
}
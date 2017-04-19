using System;

namespace Mapgenix.Canvas
{
    [Serializable]
    public struct LabelStyleGridCell
    {
        private int _x;
        private int _y;

        public LabelStyleGridCell(int x, int y)
        {
            _x = x;
            _y = y;
        }
    }
}
using System;
using Mapgenix.Shapes;

namespace Mapgenix.Canvas
{
    [Serializable]
    internal class LineLabelAdjuster
    {
        private readonly double _angle;
        private readonly double _lineSegmentLength;
        private readonly PointShape _midLinePoint;

        public LineLabelAdjuster()
            : this(new PointShape(0, 0), 0, 0)
        {
        }

        public LineLabelAdjuster(PointShape midLinePoint, double lineSegmentLength, double angle)
        {
            _midLinePoint = midLinePoint;
            _lineSegmentLength = lineSegmentLength;
            _angle = angle;
        }

        public PointShape MidLinePoint
        {
            get { return _midLinePoint; }
        }

        public double LineSegmentLength
        {
            get { return _lineSegmentLength; }
        }

        public double Angle
        {
            get { return _angle; }
        }
    }
}
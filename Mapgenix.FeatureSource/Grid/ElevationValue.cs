namespace Mapgenix.FeatureSource
{
    internal class ElevationValue
    {
        public static double GetCenterRectValue(ValueRectangle rectangle)
        {
            return (rectangle.BottomRight + rectangle.LeftBottom + rectangle.RightTop + rectangle.TopLeft) * 0.25;
        }

        public static double GetCenterTopValue(ValueRectangle rectangle)
        {
            return (rectangle.TopLeft + rectangle.RightTop) * 0.5;
        }

        public static double GetCenterLeftValue(ValueRectangle rectangle)
        {
            return (rectangle.TopLeft + rectangle.LeftBottom) * 0.5;
        }

        public static double GetCenterBottomValue(ValueRectangle rectangle)
        {
            return (rectangle.BottomRight + rectangle.LeftBottom) * 0.5;
        }

        public static double GetCenterRightValue(ValueRectangle rectangle)
        {
            return (rectangle.RightTop + rectangle.BottomRight) * 0.5;
        }

        public static ValueRectangle GetTopLeftRectValue(ValueRectangle mainRectangle)
        {
            ValueRectangle ResRect = new ValueRectangle();
            ResRect.LeftBottom = GetCenterLeftValue(mainRectangle);
            ResRect.BottomRight = GetCenterRectValue(mainRectangle);
            ResRect.RightTop = GetCenterTopValue(mainRectangle);
            ResRect.TopLeft = mainRectangle.TopLeft;
            return ResRect;
        }

        public static ValueRectangle GetRightTopRectValue(ValueRectangle mainRectangle)
        {
            ValueRectangle ResRect = new ValueRectangle();
            ResRect.LeftBottom = GetCenterRectValue(mainRectangle);
            ResRect.BottomRight = GetCenterRightValue(mainRectangle);
            ResRect.RightTop = mainRectangle.RightTop;
            ResRect.TopLeft = GetCenterTopValue(mainRectangle);
            return ResRect;
        }

        public static ValueRectangle GetBottomRightRectValue(ValueRectangle mainRectangle)
        {
            ValueRectangle ResRect = new ValueRectangle();
            ResRect.LeftBottom = GetCenterBottomValue(mainRectangle);
            ResRect.BottomRight = mainRectangle.BottomRight;
            ResRect.RightTop = GetCenterRightValue(mainRectangle);
            ResRect.TopLeft = GetCenterRectValue(mainRectangle);
            return ResRect;
        }

        public static ValueRectangle GetLeftBottomRectValue(ValueRectangle mainRectangle)
        {
            ValueRectangle ResRect = new ValueRectangle();
            ResRect.LeftBottom = mainRectangle.LeftBottom;
            ResRect.BottomRight = GetCenterBottomValue(mainRectangle);
            ResRect.RightTop = GetCenterRectValue(mainRectangle);
            ResRect.TopLeft = GetCenterLeftValue(mainRectangle);
            return ResRect;
        }
    }
}

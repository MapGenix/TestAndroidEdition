using Mapgenix.Shapes;

namespace Mapgenix.GSuite.Android
{
    public abstract partial class BaseTileOverlay
    {
        private RectangleShape _previousMaxExtent;
        private const double EnlargeTimeToMaxExtent = 10000d;
        private readonly static RectangleShape MaxExtentForDecimalDegree = new RectangleShape(-180, 90, 180, -90);
        private readonly static RectangleShape MaxExtentForMeter = new RectangleShape(-20001365, 20001365, 20001365, -20001365);
        private readonly static RectangleShape MaxExtentForFeet = new RectangleShape(-65621310, 65621310, 65621310, -65621310);

      
        public WrapDatelineMode WrapDatelineMode { get; set; }

        private void SaveOrRestoreMaxExtent()
        {
            switch (WrapDatelineMode)
            {
                case WrapDatelineMode.None:
                    MaxExtent = _previousMaxExtent;
                    break;
                case WrapDatelineMode.WrapDateline:
                    _previousMaxExtent = MaxExtent;
                    MaxExtent = GetPositiveMaxExtent();
                    break;
            }
        }

        private RectangleShape AdjustExtentForWrapDateline(RectangleShape targetDrawingExtent)
        {
            double left = targetDrawingExtent.UpperLeftPoint.X;
            double right = targetDrawingExtent.UpperRightPoint.X;
            RectangleShape worldFullExtent = GetWorldFullExtent();

            if (left < worldFullExtent.UpperLeftPoint.X && right <= worldFullExtent.UpperLeftPoint.X)
            {
                while (left < worldFullExtent.UpperLeftPoint.X && right <= worldFullExtent.UpperLeftPoint.X)
                {
                    left += worldFullExtent.Width;
                    right += worldFullExtent.Width;
                }
            }
            else if (left >= worldFullExtent.UpperRightPoint.X && right > worldFullExtent.UpperRightPoint.X)
            {
                while (left >= worldFullExtent.UpperRightPoint.X && right > worldFullExtent.UpperRightPoint.X)
                {
                    left -= worldFullExtent.Width;
                    right -= worldFullExtent.Width;
                }
            }

            if (right > worldFullExtent.UpperRightPoint.X)
            {
                left -= worldFullExtent.Width;
                right -= worldFullExtent.Width;
            }

            return new RectangleShape(left, targetDrawingExtent.UpperLeftPoint.Y, right, targetDrawingExtent.LowerLeftPoint.Y);
        }

        private RectangleShape GetWorldFullExtent()
        {
            switch (MapArguments.MapUnit)
            {
                case GeographyUnit.Meter:
                    return MaxExtentForMeter;
                case GeographyUnit.Feet:
                    return MaxExtentForFeet;
                case GeographyUnit.DecimalDegree:
                default:
                    return MaxExtentForDecimalDegree;
            }
        }

        private RectangleShape GetPositiveMaxExtent()
        {
            RectangleShape worldFullExtent = GetWorldFullExtent();
            double enlargeWidth = worldFullExtent.Width * EnlargeTimeToMaxExtent;
            return new RectangleShape(worldFullExtent.UpperLeftPoint.X - enlargeWidth, worldFullExtent.UpperLeftPoint.Y, worldFullExtent.LowerRightPoint.X + enlargeWidth, worldFullExtent.LowerRightPoint.Y);
        }
    }
}

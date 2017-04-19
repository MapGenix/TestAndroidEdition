
namespace Mapgenix.FeatureSource
{
    public class IsoRectangle
    {
        private readonly GridCell pointTopLeft;
        private readonly GridCell pointRightTop;
        private readonly GridCell pointBottomRight;
        private readonly GridCell pointLeftBottom;

        public IsoRectangle(GridCell leftBottom, GridCell bottomRight, GridCell rightTop, GridCell topLeft)
        {
            pointTopLeft = topLeft;
            pointRightTop = rightTop;
            pointBottomRight = bottomRight;
            pointLeftBottom = leftBottom;
        }

        public GridCell TopLeft
        {
            get { return pointTopLeft; }
        }

        public GridCell RightTop
        {
            get { return pointRightTop; }
        }

        public GridCell BottomRight
        {
            get { return pointBottomRight; }
        }

        public GridCell LeftBottom
        {
            get { return pointLeftBottom; }
        }

    }
}

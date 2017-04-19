
namespace Mapgenix.FeatureSource
{
    public class IsoRectangleIndex : IsoRectangle
    {
        private readonly long indexX;
        private readonly long indexY;

        public IsoRectangleIndex(GridCell leftBottom, GridCell bottomRight, GridCell rightTop, GridCell topLeft, long leftBottomIndexX, long leftBottomIndexY)
            : base(leftBottom, bottomRight, rightTop, topLeft)
        {
            indexX = leftBottomIndexX;
            indexY = leftBottomIndexY;
        }

        public long IndexX
        {
            get { return indexX; }
        }

        public long IndexY
        {
            get { return indexY; }
        }
    }
}

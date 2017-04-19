
namespace Mapgenix.FeatureSource
{
    internal class RectangleOuter
    {
        private FunctionThrough OutFunction;
        private readonly ValueRectangle DvalueRectangle;

        public RectangleOuter()
        {
            OutFunction = new FunctionThrough();
            DvalueRectangle = new ValueRectangle();
        }

        public FunctionThrough EnterLeft(ValueRectangle desRectangle, double enterValue)
        {
            GetRectangleDvalue(desRectangle, enterValue);

            if (DvalueRectangle.LeftBottom > 0 && DvalueRectangle.BottomRight < 0 && DvalueRectangle.RightTop < 0 && DvalueRectangle.TopLeft < 0
               || DvalueRectangle.LeftBottom < 0 && DvalueRectangle.BottomRight > 0 && DvalueRectangle.RightTop > 0 && DvalueRectangle.TopLeft > 0)
            {
                OutFunction.SetBottom();
            }
            else if (DvalueRectangle.LeftBottom > 0 && DvalueRectangle.BottomRight > 0 && DvalueRectangle.RightTop > 0 && DvalueRectangle.TopLeft < 0
                || DvalueRectangle.LeftBottom < 0 && DvalueRectangle.BottomRight < 0 && DvalueRectangle.RightTop < 0 && DvalueRectangle.TopLeft > 0)
            {
                OutFunction.SetTop();
            }
            else if (DvalueRectangle.LeftBottom > 0 && DvalueRectangle.BottomRight > 0 && DvalueRectangle.RightTop < 0 && DvalueRectangle.TopLeft < 0
                || DvalueRectangle.LeftBottom < 0 && DvalueRectangle.BottomRight < 0 && DvalueRectangle.RightTop > 0 && DvalueRectangle.TopLeft > 0)
            {
                OutFunction.SetRight();
            }
            else if (DvalueRectangle.LeftBottom > 0 && DvalueRectangle.BottomRight < 0 && DvalueRectangle.RightTop > 0 && DvalueRectangle.TopLeft < 0
                || DvalueRectangle.LeftBottom < 0 && DvalueRectangle.BottomRight > 0 && DvalueRectangle.RightTop < 0 && DvalueRectangle.TopLeft > 0)
            {
                TrackDiagonal Diagonal = new TrackDiagonal(desRectangle, enterValue);
                OutFunction = Diagonal.EnterLeft();
            }
            else
            {
                throw new System.SystemException("Input data must be more detailized to build isolines.");
            }
            return OutFunction;
        }

        public FunctionThrough EnterTop(ValueRectangle desRectangle, double enterValue)
        {
            GetRectangleDvalue(desRectangle, enterValue);

            if (DvalueRectangle.LeftBottom > 0 && DvalueRectangle.BottomRight > 0 && DvalueRectangle.RightTop > 0 && DvalueRectangle.TopLeft < 0
                || DvalueRectangle.LeftBottom < 0 && DvalueRectangle.BottomRight < 0 && DvalueRectangle.RightTop < 0 && DvalueRectangle.TopLeft > 0)
            {
                OutFunction.SetLeft();
            }
            else if (DvalueRectangle.LeftBottom < 0 && DvalueRectangle.BottomRight < 0 && DvalueRectangle.RightTop > 0 && DvalueRectangle.TopLeft < 0
                || DvalueRectangle.LeftBottom > 0 && DvalueRectangle.BottomRight > 0 && DvalueRectangle.RightTop < 0 && DvalueRectangle.TopLeft > 0)
            {
                OutFunction.SetRight();
            }
            else if (DvalueRectangle.LeftBottom < 0 && DvalueRectangle.BottomRight > 0 && DvalueRectangle.RightTop > 0 && DvalueRectangle.TopLeft < 0
                || DvalueRectangle.LeftBottom > 0 && DvalueRectangle.BottomRight < 0 && DvalueRectangle.RightTop < 0 && DvalueRectangle.TopLeft > 0)
            {
                OutFunction.SetBottom();
            }
            else if (DvalueRectangle.LeftBottom > 0 && DvalueRectangle.BottomRight < 0 && DvalueRectangle.RightTop > 0 && DvalueRectangle.TopLeft < 0
                || DvalueRectangle.LeftBottom < 0 && DvalueRectangle.BottomRight > 0 && DvalueRectangle.RightTop < 0 && DvalueRectangle.TopLeft > 0)
            {
                TrackDiagonal Diagonal = new TrackDiagonal(desRectangle, enterValue);
                OutFunction = Diagonal.EnterTop();
            }
            else
            {
                throw new System.SystemException("Input data must be more detailized to build isolines.");
            }
            return OutFunction;
        }

        public FunctionThrough EnterRight(ValueRectangle desRectangle, double enterValue)
        {
            GetRectangleDvalue(desRectangle, enterValue);

            if (DvalueRectangle.LeftBottom > 0 && DvalueRectangle.BottomRight > 0 && DvalueRectangle.RightTop < 0 && DvalueRectangle.TopLeft > 0
                || DvalueRectangle.LeftBottom < 0 && DvalueRectangle.BottomRight < 0 && DvalueRectangle.RightTop > 0 && DvalueRectangle.TopLeft < 0)
            {
                OutFunction.SetTop();
            }
            else if (DvalueRectangle.LeftBottom < 0 && DvalueRectangle.BottomRight > 0 && DvalueRectangle.RightTop < 0 && DvalueRectangle.TopLeft < 0
                || DvalueRectangle.LeftBottom > 0 && DvalueRectangle.BottomRight < 0 && DvalueRectangle.RightTop > 0 && DvalueRectangle.TopLeft > 0)
            {
                OutFunction.SetBottom();
            }
            else if (DvalueRectangle.LeftBottom > 0 && DvalueRectangle.BottomRight > 0 && DvalueRectangle.RightTop < 0 && DvalueRectangle.TopLeft < 0
                || DvalueRectangle.LeftBottom < 0 && DvalueRectangle.BottomRight < 0 && DvalueRectangle.RightTop > 0 && DvalueRectangle.TopLeft > 0)
            {
                OutFunction.SetLeft();
            }
            else if (DvalueRectangle.LeftBottom > 0 && DvalueRectangle.BottomRight < 0 && DvalueRectangle.RightTop > 0 && DvalueRectangle.TopLeft < 0
                || DvalueRectangle.LeftBottom < 0 && DvalueRectangle.BottomRight > 0 && DvalueRectangle.RightTop < 0 && DvalueRectangle.TopLeft > 0)
            {
                TrackDiagonal Diagonal = new TrackDiagonal(desRectangle, enterValue);
                OutFunction = Diagonal.EnterRight();
            }
            else
            {
                throw new System.SystemException("Input data must be more detailized to build isolines.");
            }
            return OutFunction;
        }

        public FunctionThrough EnterBottom(ValueRectangle desRectangle, double enterValue)
        {
            GetRectangleDvalue(desRectangle, enterValue);

            if (DvalueRectangle.LeftBottom < 0 && DvalueRectangle.BottomRight > 0 && DvalueRectangle.RightTop < 0 && DvalueRectangle.TopLeft < 0
                || DvalueRectangle.LeftBottom > 0 && DvalueRectangle.BottomRight < 0 && DvalueRectangle.RightTop > 0 && DvalueRectangle.TopLeft > 0)
            {
                OutFunction.SetRight();
            }
            else if (DvalueRectangle.LeftBottom < 0 && DvalueRectangle.BottomRight > 0 && DvalueRectangle.RightTop > 0 && DvalueRectangle.TopLeft > 0
                || DvalueRectangle.LeftBottom > 0 && DvalueRectangle.BottomRight < 0 && DvalueRectangle.RightTop < 0 && DvalueRectangle.TopLeft < 0)
            {
                OutFunction.SetLeft();
            }
            else if (DvalueRectangle.LeftBottom < 0 && DvalueRectangle.BottomRight > 0 && DvalueRectangle.RightTop > 0 && DvalueRectangle.TopLeft < 0
                || DvalueRectangle.LeftBottom > 0 && DvalueRectangle.BottomRight < 0 && DvalueRectangle.RightTop < 0 && DvalueRectangle.TopLeft > 0)
            {
                OutFunction.SetTop();
            }
            else if (DvalueRectangle.LeftBottom > 0 && DvalueRectangle.BottomRight < 0 && DvalueRectangle.RightTop > 0 && DvalueRectangle.TopLeft < 0
                 || DvalueRectangle.LeftBottom < 0 && DvalueRectangle.BottomRight > 0 && DvalueRectangle.RightTop < 0 && DvalueRectangle.TopLeft > 0)
            {
               
                TrackDiagonal Diagonal = new TrackDiagonal(desRectangle, enterValue);
                OutFunction = Diagonal.EnterBottom();
            }
            else
            {
                throw new System.SystemException("Input data must be more detailized to build isolines.");
            }
            return OutFunction;
        }

        private void GetRectangleDvalue(ValueRectangle DesRectangle, double EnterValue)
        {
            DvalueRectangle.LeftBottom = DesRectangle.LeftBottom - EnterValue;
            DvalueRectangle.BottomRight = DesRectangle.BottomRight - EnterValue;
            DvalueRectangle.RightTop = DesRectangle.RightTop - EnterValue;
            DvalueRectangle.TopLeft = DesRectangle.TopLeft - EnterValue;
            if (DvalueRectangle.LeftBottom.Equals(0.0))
            {
                DvalueRectangle.LeftBottom = DvalueRectangle.LeftBottom - 0.000000000001;
            }
            if (DvalueRectangle.BottomRight.Equals(0.0))
            {
                DvalueRectangle.BottomRight = DvalueRectangle.BottomRight - 0.000000000001;
            }
            if (DvalueRectangle.RightTop.Equals(0.0))
            {
                DvalueRectangle.RightTop = DvalueRectangle.RightTop - 0.000000000001;
            }
            if (DvalueRectangle.TopLeft.Equals(0.0))
            {
                DvalueRectangle.TopLeft = DvalueRectangle.TopLeft - 0.000000000001;
            }
        }
    }
}

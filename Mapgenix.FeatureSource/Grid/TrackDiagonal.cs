using System;

namespace Mapgenix.FeatureSource
{
    internal class TrackDiagonal
    {
        private readonly ValueRectangle MainRectangle;
        private FunctionThrough outFunction;
        private double enterValue;
        private static int trackCount;
        private const int MaxTracker = 100;

        public TrackDiagonal(ValueRectangle desRectangle, double enterValue)
        {
            MainRectangle = desRectangle;
            this.enterValue = enterValue;
            outFunction = new FunctionThrough();
            trackCount += 1;
        }

        public static int TrackCount
        {
            set { trackCount = value; }
            get { return trackCount; }
        }

        public FunctionThrough EnterLeft()
        {
            if (trackCount >= MaxTracker)
            {
                throw new SystemException("Input data.");
            }
            FunctionThrough ChildEnter = new FunctionThrough();
            ChildEnter.SetLeft();
            if (enterValue > ElevationValue.GetCenterLeftValue(MainRectangle) && enterValue < MainRectangle.TopLeft
                || enterValue < ElevationValue.GetCenterLeftValue(MainRectangle) && enterValue > MainRectangle.TopLeft)
            {
                outFunction = EnterTopLeftRect(ChildEnter);
            }
            else
            {
                outFunction = EnterLeftBottomRect(ChildEnter);
            }
            return outFunction;
        }

        public FunctionThrough EnterTop()
        {
            if (trackCount >= MaxTracker)
            {
                throw new SystemException("Input data must be more detailized to build isolines.");
            }
            FunctionThrough ChildEnter = new FunctionThrough();
            ChildEnter.SetTop();
            if (enterValue > ElevationValue.GetCenterTopValue(MainRectangle) && enterValue < MainRectangle.TopLeft
                || enterValue < ElevationValue.GetCenterTopValue(MainRectangle) && enterValue > MainRectangle.TopLeft)
            {
                outFunction = EnterTopLeftRect(ChildEnter);
            }
            else
            {
                outFunction = EnterRightTopRect(ChildEnter);
            }
            return outFunction;
        }

        public FunctionThrough EnterRight()
        {
            if (trackCount >= MaxTracker)
            {
                throw new SystemException("Input data must be more detailized to build isolines.");
            }
            FunctionThrough ChildEnter = new FunctionThrough();
            ChildEnter.SetRight();
            if (enterValue > ElevationValue.GetCenterRightValue(MainRectangle) && enterValue < MainRectangle.BottomRight
                || enterValue < ElevationValue.GetCenterRightValue(MainRectangle) && enterValue > MainRectangle.BottomRight)
            {
                outFunction = EnterBottomRightRect(ChildEnter);
            }
            else
            {
                outFunction = EnterRightTopRect(ChildEnter);
            }
            return outFunction;
        }

        public FunctionThrough EnterBottom()
        {
            if (trackCount >= MaxTracker)
            {
                throw new SystemException("Input data must be more detailized to build isolines.");
            }
            FunctionThrough ChildEnter = new FunctionThrough();
            ChildEnter.SetBottom();
            if (enterValue > ElevationValue.GetCenterBottomValue(MainRectangle) && enterValue < MainRectangle.BottomRight
                || enterValue < ElevationValue.GetCenterBottomValue(MainRectangle) && enterValue > MainRectangle.BottomRight)
            {
                outFunction = EnterBottomRightRect(ChildEnter);
            }
            else
            {
                outFunction = EnterLeftBottomRect(ChildEnter);
            }
            return outFunction;
        }

        private FunctionThrough EnterTopLeftRect(FunctionThrough EnterFunction)
        {
            ValueRectangle ChildRect = ElevationValue.GetTopLeftRectValue(MainRectangle);
            FunctionThrough ChildOuter = ChildRectOuter(ChildRect, EnterFunction);
            RectangleEnter ValueRevise = new RectangleEnter(enterValue);
            if (ChildOuter.Left.Equals(true))
            {
                if (ValueRevise.JudgeLine(ChildRect.LeftBottom, ChildRect.TopLeft).Equals(true))
                {
                    enterValue = ValueRevise.PointValue;
                    ChildOuter.SetLeft();
                }
            }
            else if (ChildOuter.Bottom.Equals(true))
            {
                if (ValueRevise.JudgeLine(ChildRect.LeftBottom, ChildRect.BottomRight).Equals(true))
                {
                    enterValue = ValueRevise.PointValue;
                    EnterFunction.SetTop();
                    ChildOuter = EnterLeftBottomRect(EnterFunction);
                }
            }
            else if (ChildOuter.Right.Equals(true))
            {
                if (ValueRevise.JudgeLine(ChildRect.BottomRight, ChildRect.RightTop).Equals(true))
                {
                    enterValue = ValueRevise.PointValue;
                    EnterFunction.SetLeft();
                    ChildOuter = EnterRightTopRect(EnterFunction);
                }
            }
            else if (ChildOuter.Top.Equals(true))
            {
                if (ValueRevise.JudgeLine(ChildRect.TopLeft, ChildRect.RightTop).Equals(true))
                {
                    enterValue = ValueRevise.PointValue;
                    ChildOuter.SetTop();
                }
            }
            return ChildOuter;
        }

        private FunctionThrough EnterRightTopRect(FunctionThrough EnterFunction)
        {
            ValueRectangle ChildRect = ElevationValue.GetRightTopRectValue(MainRectangle);
            FunctionThrough OutChild = ChildRectOuter(ChildRect, EnterFunction);
            RectangleEnter OutPosition = new RectangleEnter(enterValue);
            if (OutChild.Left.Equals(true))
            {
                if (OutPosition.JudgeLine(ChildRect.LeftBottom, ChildRect.TopLeft).Equals(true))
                {
                    enterValue = OutPosition.PointValue;
                    EnterFunction.SetRight();
                    OutChild = EnterTopLeftRect(EnterFunction);
                }
            }
            else if (OutChild.Bottom.Equals(true))
            {
                if (OutPosition.JudgeLine(ChildRect.LeftBottom, ChildRect.BottomRight).Equals(true))
                {
                    enterValue = OutPosition.PointValue;
                    EnterFunction.SetTop();
                    OutChild = EnterBottomRightRect(EnterFunction);
                }
            }
            else if (OutChild.Right.Equals(true))
            {
                if (OutPosition.JudgeLine(ChildRect.BottomRight, ChildRect.RightTop).Equals(true))
                {
                    enterValue = OutPosition.PointValue;
                    OutChild.SetRight();
                }
            }
            else if (OutChild.Top.Equals(true))
            {
                if (OutPosition.JudgeLine(ChildRect.TopLeft, ChildRect.RightTop).Equals(true))
                {
                    enterValue = OutPosition.PointValue;
                    OutChild.SetTop();
                }
            }
            return OutChild;
        }

        private FunctionThrough EnterBottomRightRect(FunctionThrough EnterFunction)
        {
            ValueRectangle ChildRect = ElevationValue.GetBottomRightRectValue(MainRectangle);
            FunctionThrough OutChild = ChildRectOuter(ChildRect, EnterFunction);
            RectangleEnter OutPosition = new RectangleEnter(enterValue);
            if (OutChild.Left.Equals(true))
            {
                if (OutPosition.JudgeLine(ChildRect.LeftBottom, ChildRect.TopLeft).Equals(true))
                {
                    enterValue = OutPosition.PointValue;
                    EnterFunction.SetRight();
                    OutChild = EnterLeftBottomRect(EnterFunction);
                }
            }
            else if (OutChild.Bottom.Equals(true))
            {
                if (OutPosition.JudgeLine(ChildRect.LeftBottom, ChildRect.BottomRight).Equals(true))
                {
                    enterValue = OutPosition.PointValue;
                    OutChild.SetBottom();
                }
            }
            else if (OutChild.Right.Equals(true))
            {
                if (OutPosition.JudgeLine(ChildRect.BottomRight, ChildRect.RightTop).Equals(true))
                {
                    enterValue = OutPosition.PointValue;
                    OutChild.SetRight();
                }
            }
            else if (OutChild.Top.Equals(true))
            {
                if (OutPosition.JudgeLine(ChildRect.TopLeft, ChildRect.RightTop).Equals(true))
                {
                    enterValue = OutPosition.PointValue;
                    EnterFunction.SetBottom();
                    OutChild = EnterRightTopRect(EnterFunction);
                }
            }
            return OutChild;
        }

        private FunctionThrough EnterLeftBottomRect(FunctionThrough EnterFunction)
        {
            ValueRectangle ChildRect = ElevationValue.GetLeftBottomRectValue(MainRectangle);
            FunctionThrough OutChild = ChildRectOuter(ChildRect, EnterFunction);
            RectangleEnter OutPosition = new RectangleEnter(enterValue);
            if (OutChild.Left.Equals(true))
            {
                if (OutPosition.JudgeLine(ChildRect.LeftBottom, ChildRect.TopLeft).Equals(true))
                {
                    enterValue = OutPosition.PointValue;
                    OutChild.SetLeft();
                }
            }
            else if (OutChild.Bottom.Equals(true))
            {
                if (OutPosition.JudgeLine(ChildRect.LeftBottom, ChildRect.BottomRight).Equals(true))
                {
                    enterValue = OutPosition.PointValue;
                    OutChild.SetBottom();
                }
            }
            else if (OutChild.Right.Equals(true))
            {
                if (OutPosition.JudgeLine(ChildRect.BottomRight, ChildRect.RightTop).Equals(true))
                {
                    enterValue = OutPosition.PointValue;
                    EnterFunction.SetLeft();
                    OutChild = EnterBottomRightRect(EnterFunction);
                }
            }
            else if (OutChild.Top.Equals(true))
            {
                if (OutPosition.JudgeLine(ChildRect.TopLeft, ChildRect.RightTop).Equals(true))
                {
                    enterValue = OutPosition.PointValue;
                    EnterFunction.SetBottom();
                    OutChild = EnterTopLeftRect(EnterFunction);
                }
            }
            return OutChild;
        }

        private FunctionThrough ChildRectOuter(ValueRectangle EnterRect, FunctionThrough EnterFunction)
        {
            RectangleOuter JudgeOuter = new RectangleOuter();
            if (EnterFunction.Left.Equals(true))
            {
                return JudgeOuter.EnterLeft(EnterRect, enterValue);
            }
            if (EnterFunction.Top.Equals(true))
            {
                return JudgeOuter.EnterTop(EnterRect, enterValue);
            }
            if (EnterFunction.Right.Equals(true))
            {
                return JudgeOuter.EnterRight(EnterRect, enterValue);
            }
            if (EnterFunction.Bottom.Equals(true))
            {
                return JudgeOuter.EnterBottom(EnterRect, enterValue);
            }
            throw new SystemException("IsolinesUnsupportedCase");
        }
    }
}

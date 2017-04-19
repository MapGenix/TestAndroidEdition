using System;
using System.Collections.ObjectModel;

namespace Mapgenix.FeatureSource
{
    public class TrackLine
    {
        private RectangleEnter JudegEnter;
        private Collection<GridCell> pointList;
        private readonly RectangleOuter JudgeOuter;
        private GridCell RectangleEnterPoint;
        private GridCell StartPoint;
        private GridCell[,] grid;

        private FunctionThrough trackEnterFunction = new FunctionThrough();
        private IsoRectangleIndex indexRectangle;

        public bool[,] BottomMark;
        public bool[,] LeftMark;
        public long Width;
        public long Height;


        public TrackLine(GridCell[,] grid)
        {
            this.grid = grid;
            this.Width = grid.GetLength(0);
            this.Height = grid.GetLength(1);
            BottomMark = new bool[Width, Height];
            LeftMark = new bool[Width, Height];
            new EventArgs();
            JudgeOuter = new RectangleOuter();
        }

        public Collection<GridCell> PointList
        {
            get { return pointList; }
        }

        public GridCell[,] Grid
        {
            set { grid = value; }
            get { return grid; }
        }

        public void TrackEnter(IsoRectangleIndex enterRectangle, GridCell enterPoint, FunctionThrough enterFunction)
        {
            pointList = new Collection<GridCell>();
            JudegEnter = new RectangleEnter(enterPoint.Value);
            RectangleEnterPoint = enterPoint;
            StartPoint = enterPoint;
           
            MarkLine(enterRectangle, enterFunction);
            pointList.Add(enterPoint);
            
            Boolean trackEnd = true;
            indexRectangle = new IsoRectangleIndex(enterRectangle.LeftBottom, enterRectangle.BottomRight, enterRectangle.RightTop, enterRectangle.TopLeft, enterRectangle.IndexX, enterRectangle.IndexY);
            trackEnterFunction = enterFunction;
            while (trackEnd == true)
            {
                trackEnd = TrackEachRectangle();
            }
        }

        private Boolean TrackEachRectangle()
        {
            FunctionThrough OutFunction = new FunctionThrough();
            ValueRectangle valueRectangle = new ValueRectangle();
            valueRectangle.LeftBottom = indexRectangle.LeftBottom.Value;
            valueRectangle.BottomRight = indexRectangle.BottomRight.Value;
            valueRectangle.RightTop = indexRectangle.RightTop.Value;
            valueRectangle.TopLeft = indexRectangle.TopLeft.Value;
           
            TrackDiagonal.TrackCount = 0;
          
            if (trackEnterFunction.Left.Equals(true))
            {
                OutFunction = JudgeOuter.EnterLeft(valueRectangle, RectangleEnterPoint.Value);
            }
            if (trackEnterFunction.Top.Equals(true))
            {
                OutFunction = JudgeOuter.EnterTop(valueRectangle, RectangleEnterPoint.Value);
            }
            if (trackEnterFunction.Right.Equals(true))
            {
                OutFunction = JudgeOuter.EnterRight(valueRectangle, RectangleEnterPoint.Value);
            }
            if (trackEnterFunction.Bottom.Equals(true))
            {
                OutFunction = JudgeOuter.EnterBottom(valueRectangle, RectangleEnterPoint.Value);
            }
           
            if (OutFunction.Left.Equals(true))
            {
                if (OutLineLeft(OutFunction).Equals(true))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            if (OutFunction.Bottom.Equals(true))
            {
                if (OutLineBottom(OutFunction).Equals(true))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            if (OutFunction.Top.Equals(true))
            {
                if (OutLineTop(OutFunction).Equals(true))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            if (OutFunction.Right.Equals(true))
            {
                if (OutLineRight(OutFunction).Equals(true))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            throw new SystemException("IsolinesUnsupportedCase");
        }

        private bool OutLineLeft(FunctionThrough OutFunction)
        {
            if (JudegEnter.JudgeLine(indexRectangle.LeftBottom,
                indexRectangle.TopLeft).Equals(true))
            {
                OutFunction.SetLeft();
                if (Frontier(indexRectangle, OutFunction).Equals(true))
                {
                    pointList.Add(JudegEnter.PointResult);
                    MarkLine(indexRectangle, OutFunction);
                    return false;
                }
                else if (LeftMark[indexRectangle.IndexX, indexRectangle.IndexY].Equals(true))
                {
                    pointList.Add(StartPoint);
                    return false;
                }
                else
                {
                    pointList.Add(JudegEnter.PointResult);
                    RectangleEnterPoint = JudegEnter.PointResult;
                    MarkLine(indexRectangle, OutFunction);
                    OutFunction.SetRight();
                     indexRectangle=
                        new IsoRectangleIndex(
                            grid[indexRectangle.IndexX - 1, indexRectangle.IndexY],
                            grid[indexRectangle.IndexX, indexRectangle.IndexY],
                            grid[indexRectangle.IndexX, indexRectangle.IndexY + 1],
                            grid[indexRectangle.IndexX - 1, indexRectangle.IndexY + 1],
                            indexRectangle.IndexX - 1, indexRectangle.IndexY);
                     trackEnterFunction.SetRight();
                     return true;
                }
            }
            else
            {
                return false;
            }
        }

        private bool OutLineTop( FunctionThrough OutFunction)
        {
            if (JudegEnter.JudgeLine(indexRectangle.TopLeft,
                indexRectangle.RightTop).Equals(true))
            {
                OutFunction.SetTop();
                if (Frontier(indexRectangle, OutFunction).Equals(true))
                {
                    pointList.Add(JudegEnter.PointResult);
                    MarkLine(indexRectangle, OutFunction);
                    return false;
                }
                else if (BottomMark[indexRectangle.IndexX, indexRectangle.IndexY + 1].Equals(true))
                {
                    pointList.Add(StartPoint);
                    return false;
                }
                else
                {
                    pointList.Add(JudegEnter.PointResult);
                    RectangleEnterPoint = JudegEnter.PointResult;
                   
                     indexRectangle  = new IsoRectangleIndex(
                            grid[indexRectangle.IndexX, indexRectangle.IndexY + 1],
                            grid[indexRectangle.IndexX + 1, indexRectangle.IndexY + 1],
                            grid[indexRectangle.IndexX + 1, indexRectangle.IndexY + 2],
                            grid[indexRectangle.IndexX, indexRectangle.IndexY + 2],
                            indexRectangle.IndexX, indexRectangle.IndexY + 1);
                    OutFunction.SetBottom();
                    MarkLine(indexRectangle, OutFunction);
                    trackEnterFunction.SetBottom();
                    return true;
                }
            }
            else
            {
                return false;
            }
        }

        private bool OutLineRight( FunctionThrough OutFunction)
        {
            if (JudegEnter.JudgeLine(indexRectangle.BottomRight,
                indexRectangle.RightTop).Equals(true))
            {
                OutFunction.SetRight();
                if (Frontier(indexRectangle, OutFunction).Equals(true))
                {
                    pointList.Add(JudegEnter.PointResult);
                    MarkLine(indexRectangle, OutFunction);
                    return false;
                }
                else if (LeftMark[indexRectangle.IndexX + 1, indexRectangle.IndexY].Equals(true))
                {
                    pointList.Add(StartPoint);
                    return false;
                }
                else
                {
                    pointList.Add(JudegEnter.PointResult);
                    RectangleEnterPoint = JudegEnter.PointResult;
                   
                    indexRectangle = new IsoRectangleIndex(
                            grid[indexRectangle.IndexX + 1, indexRectangle.IndexY],
                            grid[indexRectangle.IndexX + 2, indexRectangle.IndexY],
                            grid[indexRectangle.IndexX + 2, indexRectangle.IndexY + 1],
                            grid[indexRectangle.IndexX + 1, indexRectangle.IndexY + 1],
                            indexRectangle.IndexX + 1, indexRectangle.IndexY);
                    OutFunction.SetLeft();
                    MarkLine(indexRectangle, OutFunction);
                    trackEnterFunction.SetLeft();
                    return true;
                }               
            }
            else
            {
                return false;
            }
        }

        private bool OutLineBottom( FunctionThrough OutFunction)
        {
            if (JudegEnter.JudgeLine(indexRectangle.LeftBottom,
                indexRectangle.BottomRight).Equals(true))
            {
                OutFunction.SetBottom();
                if (Frontier(indexRectangle, OutFunction).Equals(true))
                {
                    pointList.Add(JudegEnter.PointResult);
                    MarkLine(indexRectangle, OutFunction);
                    return false;
                }
                else if (BottomMark[indexRectangle.IndexX, indexRectangle.IndexY].Equals(true))
                {
                    pointList.Add(StartPoint);
                    return false;
                }
                else
                {
                    pointList.Add(JudegEnter.PointResult);
                    RectangleEnterPoint = JudegEnter.PointResult;
                    MarkLine(indexRectangle, OutFunction);
                    OutFunction.SetTop();
                     indexRectangle =
                        new IsoRectangleIndex(
                            grid[indexRectangle.IndexX, indexRectangle.IndexY - 1],
                            grid[indexRectangle.IndexX + 1, indexRectangle.IndexY - 1],
                            grid[indexRectangle.IndexX + 1, indexRectangle.IndexY],
                            grid[indexRectangle.IndexX, indexRectangle.IndexY],
                            indexRectangle.IndexX, indexRectangle.IndexY - 1);
                     trackEnterFunction.SetTop();
                     return true;
                }
            }
            else
            {
                return false;
            }
        }

        public void MarkLine(IsoRectangleIndex IndexRectangle, FunctionThrough throughFunction)
        {
            if (throughFunction.Left.Equals(true))
            {
                LeftMark[IndexRectangle.IndexX, IndexRectangle.IndexY] = true;
            }
            else if (throughFunction.Top.Equals(true))
            {
                BottomMark[IndexRectangle.IndexX, IndexRectangle.IndexY + 1] = true;
            }
            else if (throughFunction.Right.Equals(true))
            {
                LeftMark[IndexRectangle.IndexX + 1, IndexRectangle.IndexY] = true;
            }
            else if (throughFunction.Bottom.Equals(true))
            {
                BottomMark[IndexRectangle.IndexX, IndexRectangle.IndexY] = true;
            }
        }

        private bool Frontier(IsoRectangleIndex IndexRectangle, FunctionThrough OutFunction)
        {
            if (OutFunction.Left.Equals(true))
            {
                if (IndexRectangle.IndexX.Equals(0))
                {
                    return true;
                }
            }
            if (OutFunction.Right.Equals(true))
            {
                if (IndexRectangle.IndexX.Equals(Width - 2))
                {
                    return true;
                }
            }
            if (OutFunction.Top.Equals(true))
            {
                if (IndexRectangle.IndexY.Equals(Height - 2))
                {
                    return true;
                }
            }
            if (OutFunction.Bottom.Equals(true))
            {
                if (IndexRectangle.IndexY.Equals(0))
                {
                    return true;
                }
            }
            return false;
        }

       

        public void InitMark()
        {
            for (int IndexX = 0; IndexX < Width; IndexX++)
            {
                for (int IndexY = 0; IndexY < Height; IndexY++)
                {
                    BottomMark[IndexX, IndexY] = false;
                    LeftMark[IndexX, IndexY] = false;
                }
            }
        }
    }
}

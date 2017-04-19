using System;
using System.Collections.ObjectModel;
using Mapgenix.Canvas;
using Mapgenix.Shapes;

namespace Mapgenix.Layers
{
    public abstract class BasePrinterLayer : BaseLayer
    {
        RectangleShape _boundingBox = new RectangleShape();
        AreaStyle _backgroundMask;
        PrinterResizeMode _resizeMode;
        PrinterDragMode _dragMode;
        bool _isDrawing;
        RectangleShape _lastBoundingBox;

       

       public bool IsDrawing
        {
            get { return _isDrawing; }
            set { _isDrawing = value; }
        }

        public AreaStyle BackgroundMask
        {
            get { return _backgroundMask; }
            set { _backgroundMask = value; }
        }


        public PrinterResizeMode ResizeMode
        {
            get { return _resizeMode; }
            set { _resizeMode = value; }
        }

        public PrinterDragMode DragMode
        {
            get { return _dragMode; }
            set { _dragMode = value; }
        }

        protected override void DrawCore(BaseGeoCanvas canvas, Collection<SimpleCandidate> labelsInAllLayers)
        {
            BackgroundMask.Draw(new BaseShape[] { _boundingBox }, canvas, new Collection<SimpleCandidate>(), labelsInAllLayers);
        }

        protected override RectangleShape GetBoundingBoxCore()
        {
            return _boundingBox;
        }

        public RectangleShape GetBoundBoxCore()
        {
            return _boundingBox;
        }

        public RectangleShape GetPosition()
        {
            return GetPosition(PrintingUnit.Point);
        }

        public RectangleShape GetPosition(PrintingUnit unit)
        {
            return GetPositionCore(unit);
        }

        protected virtual RectangleShape GetPositionCore(PrintingUnit unit)
        {
            RectangleShape currentBoundingBox = GetBoundingBox();

            double width = PrinterHelper.ConvertLength(currentBoundingBox.Width, PrintingUnit.Point, unit);
            double height = PrinterHelper.ConvertLength(currentBoundingBox.Height, PrintingUnit.Point, unit);

            double centerPointX = PrinterHelper.ConvertLength(currentBoundingBox.GetCenterPoint().X, PrintingUnit.Point, unit);
            double centerPointY = PrinterHelper.ConvertLength(currentBoundingBox.GetCenterPoint().Y, PrintingUnit.Point, unit);

            return new RectangleShape(centerPointX - width / 2, centerPointY + height / 2, centerPointX + width / 2, centerPointY - height / 2);
        }

        public void SetPosition(RectangleShape boundingBox)
        {
            SetPosition(boundingBox.Width, boundingBox.Height, boundingBox.GetCenterPoint().X, boundingBox.GetCenterPoint().Y, PrintingUnit.Point);
        }

        public void SetPosition(RectangleShape boundingBox, PrintingUnit unit)
        {
            SetPosition(boundingBox.Width, boundingBox.Height, boundingBox.GetCenterPoint().X, boundingBox.GetCenterPoint().Y, unit);
        }

        public void SetPosition(double width, double height, PointShape centerPoint, PrintingUnit unit)
        {
            SetPosition(width, height, centerPoint.X, centerPoint.Y, unit);
        }

        public void SetPosition(double width, double height, double centerPointX, double centerPointY, PrintingUnit unit)
        {
            width = PrinterHelper.ConvertLength(width, unit, PrintingUnit.Point);
            height = PrinterHelper.ConvertLength(height, unit, PrintingUnit.Point);

            centerPointX = PrinterHelper.ConvertLength(centerPointX, unit, PrintingUnit.Point);
            centerPointY = PrinterHelper.ConvertLength(centerPointY, unit, PrintingUnit.Point);

            if (_lastBoundingBox == null)
            {
                _lastBoundingBox = new RectangleShape(centerPointX - width / 2, centerPointY + height / 2, centerPointX + width / 2, centerPointY - height / 2);
            }

            _boundingBox = SetPositionCore(width, height, centerPointX, centerPointY, PrintingUnit.Point);
            _lastBoundingBox = new RectangleShape(_boundingBox.UpperLeftPoint.X, _boundingBox.UpperLeftPoint.Y, _boundingBox.LowerRightPoint.X, _boundingBox.LowerRightPoint.Y);
        }

        protected virtual RectangleShape SetPositionCore(double width, double height, double centerPointX, double centerPointY, PrintingUnit unit)
        {
            RectangleShape rtn = Resize(new RectangleShape(centerPointX - width / 2, centerPointY + height / 2, centerPointX + width / 2, centerPointY - height / 2));

            return rtn;
        }

        RectangleShape Resize(RectangleShape newBoundingBox)
        {
            RectangleShape rtn = newBoundingBox;

            switch (_dragMode)
            {
                case PrinterDragMode.Fixed:
                    switch (_resizeMode)
                    {
                        case PrinterResizeMode.Fixed:
                            rtn = _lastBoundingBox;
                            break;
                        case PrinterResizeMode.MaintainAspectRatio:
                            rtn = GetBoundingBoxBasedOnRatio(newBoundingBox, true, false);
                            break;
                        case PrinterResizeMode.Resizable:
                            rtn = GetBoundingBoxBasedOnRatio(newBoundingBox, false, false);
                            break;
                        default:
                            break;
                    }
                    break;
                case PrinterDragMode.Dragable:
                    switch (_resizeMode)
                    {
                        case PrinterResizeMode.Fixed:
                            rtn = GetBoundingBoxBasedOnRatio(newBoundingBox, true, true);
                            break;
                        case PrinterResizeMode.MaintainAspectRatio:
                            rtn = GetBoundingBoxBasedOnRatio(newBoundingBox, true, true);
                            break;
                        case PrinterResizeMode.Resizable:
                            rtn = GetBoundingBoxBasedOnRatio(newBoundingBox, false, true);
                            break;
                        default:
                            break;
                    }
                    break;
            }

            return rtn;
        }

        RectangleShape GetBoundingBoxBasedOnRatio(RectangleShape newBoundingBox, bool maintainAspectRatio, bool dragable)
        {
            RectangleShape rtn = newBoundingBox;

            double lastRatio = _lastBoundingBox.Width / _lastBoundingBox.Height;
            double currentRatio = newBoundingBox.Width / newBoundingBox.Height;
            if (Math.Round(currentRatio, 6) != Math.Round(lastRatio, 6))
            {
                if (_resizeMode == PrinterResizeMode.Fixed)
                {
                    rtn = _lastBoundingBox;
                }
                else
                {
                    double newWidth = newBoundingBox.Width;
                    double newHeight = newBoundingBox.Height;

                    if (maintainAspectRatio)
                    {
                        newHeight = newBoundingBox.Width / lastRatio;
                    }

                    PointShape newUpperLeft = newBoundingBox.UpperLeftPoint;
                    PointShape newLowerRight = newBoundingBox.LowerRightPoint;
                    if (Equals(newBoundingBox.UpperLeftPoint, _lastBoundingBox.UpperLeftPoint)) 
                    {
                        newLowerRight = new PointShape(newBoundingBox.UpperLeftPoint.X + newWidth, newBoundingBox.UpperLeftPoint.Y - newHeight);
                    }
                    else if (Equals(newBoundingBox.LowerLeftPoint, _lastBoundingBox.LowerLeftPoint)) 
                    {
                        newUpperLeft = new PointShape(newBoundingBox.UpperLeftPoint.X, newBoundingBox.LowerLeftPoint.Y + newHeight);
                        newLowerRight = new PointShape(newBoundingBox.UpperLeftPoint.X + newWidth, newBoundingBox.LowerRightPoint.Y);
                    }
                    else if (Equals(newBoundingBox.LowerRightPoint, _lastBoundingBox.LowerRightPoint)) 
                    {
                        newUpperLeft = new PointShape(newBoundingBox.LowerRightPoint.X - newWidth, newBoundingBox.LowerRightPoint.Y + newHeight);
                    }
                    else if (Equals(newBoundingBox.UpperRightPoint, _lastBoundingBox.UpperRightPoint)) 
                    {
                        newUpperLeft = new PointShape(newBoundingBox.LowerRightPoint.X - newWidth, newBoundingBox.UpperLeftPoint.Y);
                        newLowerRight = new PointShape(newBoundingBox.LowerRightPoint.X, newBoundingBox.UpperLeftPoint.Y - newHeight);
                    }

                    rtn = new RectangleShape(newUpperLeft, newLowerRight);
                }
            }
            else if (dragable)
            {
                rtn = newBoundingBox;
            }
            else
            {
                rtn = _lastBoundingBox;
            }

            return rtn;
        }

        bool Equals(PointShape point1, PointShape point2)
        {
            bool rtn = false;

            if (Math.Round(point1.X, 8) == Math.Round(point2.X, 8)
                && Math.Round(point1.Y, 8) == Math.Round(point2.Y, 8))
            {
                rtn = true;
            }

            return rtn;
        }
    }
}

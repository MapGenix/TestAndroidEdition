using System;
using System.Collections.ObjectModel;
using Mapgenix.Canvas;
using Mapgenix.Shapes;

namespace Mapgenix.Layers
{
    public class ImagePrinterLayer : BasePrinterLayer
    {
        RectangleShape _lastBoundingBox;
        public GeoImage Image { get; set; }

        public ImagePrinterLayer(GeoImage image, double centerX, double centerY, PrintingUnit unit)
        {
            Image = image;
            ResizeMode = PrinterResizeMode.MaintainAspectRatio;

            double widthInUnit = PrinterHelper.ConvertLength(image.GetWidth(), PrintingUnit.Point, unit);
            double widthInHeidht = PrinterHelper.ConvertLength(image.GetHeight(), PrintingUnit.Point, unit);

            SetPosition(widthInUnit, widthInHeidht, centerX, centerY, unit);

			
			BackgroundMask = new AreaStyle();
			DragMode = PrinterDragMode.Dragable;
			HasBoundingBox = true;
        }

        protected override RectangleShape SetPositionCore(double width, double height, double centerPointX, double centerPointY, PrintingUnit unit)
        {
            if (_lastBoundingBox == null)
            {
                _lastBoundingBox = new RectangleShape(centerPointX - width / 2, centerPointY + height / 2, centerPointX + width / 2, centerPointY - height / 2);
            }

            RectangleShape rtn = Resize(new RectangleShape(centerPointX - width / 2, centerPointY + height / 2, centerPointX + width / 2, centerPointY - height / 2));

            _lastBoundingBox = new RectangleShape(rtn.UpperLeftPoint.X, rtn.UpperLeftPoint.Y, rtn.LowerRightPoint.X, rtn.LowerRightPoint.Y);

            return rtn;
        }

        protected override void DrawCore(BaseGeoCanvas canvas, Collection<SimpleCandidate> labelsInAllLayers)
        {
            PrinterDrawHelper.DrawPrinterCore(this, canvas, labelsInAllLayers);
            PrinterDrawHelper.DrawImagePrinterCore(this, canvas);
        }

        RectangleShape Resize(RectangleShape newBoundingBox)
        {
            RectangleShape rtn = newBoundingBox;

            switch (DragMode)
            {
                case PrinterDragMode.Fixed:
                    switch (ResizeMode)
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
                    switch (ResizeMode)
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
                default:
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
                if (ResizeMode == PrinterResizeMode.Fixed)
                {
                    rtn = _lastBoundingBox;
                }
                else
                {
                    double newWidth = newBoundingBox.Width;
                    double newHeight = newBoundingBox.Height;

                    if (maintainAspectRatio)
                    {
                        double imageWidth = PrinterHelper.ConvertLength(Image.GetWidth(), PrintingUnit.Point, PrintingUnit.Point);
                        double imageHeight = PrinterHelper.ConvertLength(Image.GetHeight(), PrintingUnit.Point, PrintingUnit.Point);

                        double imageRatio = imageWidth / imageHeight;
                        newHeight = newBoundingBox.Width / imageRatio;

                        PointShape center = newBoundingBox.GetCenterPoint();
                        newBoundingBox = new RectangleShape(center.X - newWidth / 2, center.Y + newHeight / 2, center.X + newWidth / 2, center.Y - newHeight / 2);

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
    }
}

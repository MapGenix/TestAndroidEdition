using System.Collections.ObjectModel;
using Mapgenix.Canvas;
using Mapgenix.Shapes;

namespace Mapgenix.Layers
{
    public class PagePrinterLayer : BasePrinterLayer
    {
        PrinterPageSize _pageSize;
        private PrinterOrientation _orientation;
        private float _customHeight;
        private float _customWidth;

        public PrinterPageSize PageSize
        {
            get { return _pageSize; }
            set { _pageSize = value; }
        }

        public PrinterOrientation Orientation
        {
            get { return _orientation; }
            set { _orientation = value; }
        }

        public float CustomWidth
        {
            get { return _customWidth; }
            set { _customWidth = value; }
        }

        public float CustomHeight
        {
            get { return _customHeight; }
            set { _customHeight = value; }
        }

        protected override void DrawCore(BaseGeoCanvas canvas, Collection<SimpleCandidate> labelsInAllLayers)
        {
            SetPosition(GetPageExtent());
            PrinterDrawHelper.DrawPrinterCore(this,canvas,labelsInAllLayers);
        }

        protected override RectangleShape GetBoundingBoxCore()
        {
            return GetPositionCore(PrintingUnit.Point);
        }

        protected override RectangleShape SetPositionCore(double width, double height, double centerPointX, double centerPointY, PrintingUnit unit)
        {
            return new RectangleShape(centerPointX - width / 2, centerPointY + height / 2, centerPointX + width / 2, centerPointY - height / 2);
        }

        protected override RectangleShape GetPositionCore(PrintingUnit unit)
        {
            RectangleShape rtn = GetPageExtent();  

            PointShape centerPoint = rtn.GetCenterPoint();
            double width = PrinterHelper.ConvertLength(rtn.Width, PrintingUnit.Point, unit);
            double height = PrinterHelper.ConvertLength(rtn.Height, PrintingUnit.Point, unit);

            rtn = new RectangleShape(centerPoint.X - width / 2, centerPoint.Y + height / 2, centerPoint.X + width / 2, centerPoint.Y - height / 2);

            return rtn;
        }

        RectangleShape GetPageExtent()
        {
            RectangleShape rtn = null;

            int inchToPointRatio = 96;
            switch (_pageSize)
            {
                case PrinterPageSize.AnsiA:
                    rtn = new RectangleShape(-4.25 * inchToPointRatio, 5.5 * inchToPointRatio, 4.25 * inchToPointRatio, -5.5 * inchToPointRatio);
                    break;
                case PrinterPageSize.AnsiB:
                    rtn = new RectangleShape(-5.5 * inchToPointRatio, 8.5 * inchToPointRatio, 5.5 * inchToPointRatio, -8.5 * inchToPointRatio);
                    break;
                case PrinterPageSize.AnsiC:
                    rtn = new RectangleShape(-8.5 * inchToPointRatio, 11 * inchToPointRatio, 8.5 * inchToPointRatio, -11 * inchToPointRatio);
                    break;
                case PrinterPageSize.AnsiD:
                    rtn = new RectangleShape(-11 * inchToPointRatio, 17 * inchToPointRatio, 11 * inchToPointRatio, -17 * inchToPointRatio);
                    break;
                case PrinterPageSize.AnsiE:
                    rtn = new RectangleShape(-18 * inchToPointRatio, 24 * inchToPointRatio, 18 * inchToPointRatio, -24 * inchToPointRatio);
                    break;
                case PrinterPageSize.Custom:
                    rtn = new RectangleShape(0 - _customWidth / 2, 0 + _customHeight / 2, 0 + _customWidth / 2, 0 - _customHeight / 2);
                    break;
                default:
                    break;
            }

            switch (_orientation)
            {
                case PrinterOrientation.Portrait:
                    break;
                case PrinterOrientation.Landscape:
                    PolygonShape polygon = new PolygonShape(rtn.GetWellKnownText());
                    polygon.Rotate(polygon.GetCenterPoint(), 90);
                    rtn = polygon.GetBoundingBox();
                    break;
                default:
                    break;
            }

            return rtn;
        }
    }
}

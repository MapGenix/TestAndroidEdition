using Mapgenix.Canvas;
using Mapgenix.Shapes;

namespace Mapgenix.Layers
{
    public class PrinterZoomLevelSet : ZoomLevelSet
    {
        GeographyUnit _pageBaseUnit;
        double _pageBaseUnitToPageUnitRatio;

        public PrinterZoomLevelSet()
            : this(GeographyUnit.Feet, 12 * 96)
        { }

        public PrinterZoomLevelSet(GeographyUnit pageBaseUnit, double pageBaseUnitToPageUnitRatio)
        {
            _pageBaseUnit = pageBaseUnit;
            _pageBaseUnitToPageUnitRatio = pageBaseUnitToPageUnitRatio;
            ZoomLevel01.Scale = pageBaseUnitToPageUnitRatio * GetScaleMultiplicator(5); 
            ZoomLevel02.Scale = pageBaseUnitToPageUnitRatio * GetScaleMultiplicator(10); 
            ZoomLevel03.Scale = pageBaseUnitToPageUnitRatio * GetScaleMultiplicator(20); 
            ZoomLevel04.Scale = pageBaseUnitToPageUnitRatio * GetScaleMultiplicator(30); 
            ZoomLevel05.Scale = pageBaseUnitToPageUnitRatio * GetScaleMultiplicator(40); 
            ZoomLevel06.Scale = pageBaseUnitToPageUnitRatio * GetScaleMultiplicator(50); 
            ZoomLevel07.Scale = pageBaseUnitToPageUnitRatio * GetScaleMultiplicator(60);
            ZoomLevel08.Scale = pageBaseUnitToPageUnitRatio * GetScaleMultiplicator(70); 
            ZoomLevel09.Scale = pageBaseUnitToPageUnitRatio * GetScaleMultiplicator(80);
            ZoomLevel10.Scale = pageBaseUnitToPageUnitRatio * GetScaleMultiplicator(90); 
            ZoomLevel11.Scale = pageBaseUnitToPageUnitRatio;                             
            ZoomLevel12.Scale = pageBaseUnitToPageUnitRatio * GetScaleMultiplicator(110); 
            ZoomLevel13.Scale = pageBaseUnitToPageUnitRatio * GetScaleMultiplicator(120); 
            ZoomLevel14.Scale = pageBaseUnitToPageUnitRatio * GetScaleMultiplicator(150); 
            ZoomLevel15.Scale = pageBaseUnitToPageUnitRatio * GetScaleMultiplicator(200); 
            ZoomLevel16.Scale = pageBaseUnitToPageUnitRatio * GetScaleMultiplicator(300); 
            ZoomLevel17.Scale = pageBaseUnitToPageUnitRatio * GetScaleMultiplicator(400); 
            ZoomLevel18.Scale = pageBaseUnitToPageUnitRatio * GetScaleMultiplicator(500); 
            ZoomLevel19.Scale = pageBaseUnitToPageUnitRatio * GetScaleMultiplicator(600); 
            ZoomLevel20.Scale = pageBaseUnitToPageUnitRatio * GetScaleMultiplicator(800); 
        }

        public GeographyUnit PageBaseUnit
        {
            get { return _pageBaseUnit; }
            set { _pageBaseUnit = value; }
        }

        public double PageBaseUnitToPageUnitRatio
        {
            get { return _pageBaseUnitToPageUnitRatio; }
            set { _pageBaseUnitToPageUnitRatio = value; }
        }

        public double GetZoomPercentage(ZoomLevel zoomLevel)
        {
            return (_pageBaseUnitToPageUnitRatio / zoomLevel.Scale) * 100;
        }

        double GetScaleMultiplicator(double percentage)
        {
            return ((1 / percentage) * 100);
        }
    }
}

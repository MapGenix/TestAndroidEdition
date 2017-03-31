namespace Mapgenix.GSuite.Android
{
    /// <summary>
    /// Shortcut to the different map tools as properties and acts as an entrance for
    /// setting these map controls.
    /// </summary>
    public class MapTools
    {
        private Map _currentMap;
        private LogoMapTool _logo;
        private MouseCoordinateMapTool _mouseCoordinate;
        private ZoomMapTool _panZoomBar;
        private ScaleLineMapTool _scaleLine;

      
        public MapTools()
            : this(null)
        { }

       
        public MapTools(Map wpfMap)
        {
            _currentMap = wpfMap;
            _logo = new LogoMapTool(wpfMap.Context);
            _mouseCoordinate = new MouseCoordinateMapTool(_currentMap.Context);
            _panZoomBar = new ZoomMapTool(_currentMap.Context);
            _scaleLine = new ScaleLineMapTool(_currentMap.Context);
        }

      
        public LogoMapTool Logo { get { return _logo; } }

      
        public MouseCoordinateMapTool MouseCoordinate { get { return _mouseCoordinate; } }

     
        public ZoomMapTool PanZoomBar { get { return _panZoomBar; } }

      
        public ScaleLineMapTool ScaleLine { get { return _scaleLine; } }

      
        public void Refresh()
        {
            Refresh(_logo);
            Refresh(_mouseCoordinate);
            Refresh(_panZoomBar);
            Refresh(_scaleLine);
        }

        private void Refresh(BaseMapTool mapTool)
        {
            if (mapTool != null)
            {
                mapTool.Initialize(_currentMap);
            }
        }
    }
}

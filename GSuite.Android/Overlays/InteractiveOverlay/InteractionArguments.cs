﻿using System;
using System.Globalization;
using System.Text;
using Mapgenix.Shapes;
using Mapgenix.Canvas;

namespace Mapgenix.GSuite.Android
{
    /// <summary>
    /// Class encapsulating the information and methods for interactive actions.
    /// </summary>
    [Serializable]
    public class InteractionArguments
    {
        private float _screenX;
        private float _screenY;
        private double _worldX;
        private double _worldY;
        private RectangleShape _currentExtent;
        private double _searchingTolerance;
        private int _mapWidth;
        private int _mapHeight;
        private GeographyUnit _mapUnit;
        private ZoomLevelSet _zoomLevelSet;
        private int _mouseWheelDelta;
        private double _scale;
        private MapMouseButton _mouseButton;

        public InteractionArguments()
        {
            this._mouseButton = MapMouseButton.None;
            this._currentExtent = new RectangleShape();
            this._mapUnit = GeographyUnit.Unknown;
            this._zoomLevelSet = new ZoomLevelSet();
        }

        
        public float ScreenX
        {
            get { return _screenX; }
            set { _screenX = value; }
        }

        public float ScreenY
        {
            get { return _screenY; }
            set { _screenY = value; }
        }

        public double WorldX
        {
            get { return _worldX; }
            set { _worldX = value; }
        }

        public double WorldY
        {
            get { return _worldY; }
            set { _worldY = value; }
        }

        public MapMouseButton MouseButton
        {
            get { return _mouseButton; }
            set { _mouseButton = value; }
        }

        public RectangleShape CurrentExtent
        {
            get { return _currentExtent; }
            set { _currentExtent = value; }
        }

        public double SearchingTolerance
        {
            get { return _searchingTolerance; }
            set { _searchingTolerance = value; }
        }

        public int MapWidth
        {
            get { return _mapWidth; }
            set { _mapWidth = value; }
        }

        public int MapHeight
        {
            get { return _mapHeight; }
            set { _mapHeight = value; }
        }

        public GeographyUnit MapUnit
        {
            get { return _mapUnit; }
            set { _mapUnit = value; }
        }

        public ZoomLevelSet ZoomLevelSet
        {
            get { return _zoomLevelSet; }
            set { _zoomLevelSet = value; }
        }

        public int MouseWheelDelta
        {
            get { return _mouseWheelDelta; }
            set { _mouseWheelDelta = value; }
        }

        public double Scale
        {
            get { return _scale; }
            set { _scale = value; }
        }

        public override string ToString()
        {
            StringBuilder returnValue = new StringBuilder();
            returnValue.AppendLine(string.Format(CultureInfo.InvariantCulture, "ScreenX : {0}, ScreenY : {1}", _screenX, _screenY));
            returnValue.AppendLine(string.Format(CultureInfo.InvariantCulture, "WolrdX : {0}, WorldY : {1}", _worldX, _worldY));
            returnValue.AppendLine(string.Format(CultureInfo.InvariantCulture, "MouseButton : {0}", _mouseButton));
            returnValue.AppendLine(string.Empty);

            return returnValue.ToString();
        }
    }
}


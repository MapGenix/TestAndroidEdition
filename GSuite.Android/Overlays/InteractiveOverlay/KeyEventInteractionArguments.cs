using System;
using Mapgenix.Shapes;

namespace Mapgenix.GSuite.Android
{
    /// <summary>
    /// Key event arguments when interacting with the Map Control.
    /// </summary>
    [Serializable]
    public class KeyEventInteractionArguments
    {
        private string _key;
        private bool _isCtrlKeyPressed;
        private bool _isShiftKeyPressed;
        private bool _isAltKeyPressed;
        private RectangleShape _currentExtent;
        private double _currentScale;

        public KeyEventInteractionArguments(string key, bool isCtrlKeyPressed, bool isShiftKeyPressed, bool isAltKeyPressed)
        {
            this._key = key;
            this._isCtrlKeyPressed = isCtrlKeyPressed;
            this._isShiftKeyPressed = isShiftKeyPressed;
            this._isAltKeyPressed = isAltKeyPressed;

            this._currentExtent = new RectangleShape();
        }

        public string Key
        {
            get { return _key; }
            set { _key = value; }
        }

        public bool IsCtrlKeyPressed
        {
            get { return _isCtrlKeyPressed; }
            set { _isCtrlKeyPressed = value; }
        }

        public bool IsShiftKeyPressed
        {
            get { return _isShiftKeyPressed; }
            set { _isShiftKeyPressed = value; }
        }

        public bool IsAltKeyPressed
        {
            get { return _isAltKeyPressed; }
            set { _isAltKeyPressed = value; }
        }

        public RectangleShape CurrentExtent
        {
            get { return _currentExtent; }
            set { _currentExtent = value; }
        }

        public double CurrentScale
        {
            get { return _currentScale; }
            set { _currentScale = value; }
        }
    }
}

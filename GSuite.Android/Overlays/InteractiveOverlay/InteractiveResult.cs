using System;
using Mapgenix.Shapes;
using Android.Graphics;

namespace Mapgenix.GSuite.Android
{
    /// <summary>
    /// Class specifying the InteractiveResult when dealing with the Interactive methods of an InteractiveOverlay.
    /// </summary>
    [Serializable]
    public class InteractiveResult
    {
        private DrawType _interactiveOverlayDrawType;
        private ProcessOtherOverlaysMode _processOtherOverlays;
        private RectangleShape _newCurrentExtent;
        private int _newZoomLevel;
        private PointF _screenCenterPoint;

        public InteractiveResult()
            : this(DrawType.DoNotDraw, ProcessOtherOverlaysMode.ProcessOtherOverlays)
        { }

        public InteractiveResult(DrawType interactiveOverlayDrawType, ProcessOtherOverlaysMode processOtherOverlays)
        {
            this._interactiveOverlayDrawType = interactiveOverlayDrawType;
            this._processOtherOverlays = processOtherOverlays;
        }

        public DrawType DrawThisOverlay
        {
            get { return _interactiveOverlayDrawType; }
            set { _interactiveOverlayDrawType = value; }
        }

        public ProcessOtherOverlaysMode ProcessOtherOverlaysMode
        {
            get { return _processOtherOverlays; }
            set { _processOtherOverlays = value; }
        }

        public RectangleShape NewCurrentExtent
        {
            get { return _newCurrentExtent; }
            set { _newCurrentExtent = value; }
        }

        public int NewZoomLevel
        {
            get { return _newZoomLevel; }
            set { _newZoomLevel = value; }
        }

        public PointF ScreenCenterPoint
        {
            get { return _screenCenterPoint; }
            set { _screenCenterPoint = value; }
        }
    }
}

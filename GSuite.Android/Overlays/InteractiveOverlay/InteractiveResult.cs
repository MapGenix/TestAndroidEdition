using System;
using Mapgenix.Shapes;

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
    }
}

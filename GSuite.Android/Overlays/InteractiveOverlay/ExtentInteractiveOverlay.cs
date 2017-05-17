using System;
using System.Windows;
using Mapgenix.Shapes;
using Mapgenix.Canvas;
using Android.Graphics;
using Android.Content;
using Android.Views;
using Android.Widget;

namespace Mapgenix.GSuite.Android
{
    /// <summary>
    /// Class inheriting from InteractiveOverlay abstract class. This overlay deals with the
    /// Extent Changing interactive process with the Map Control using Mouse or Keyboard.
    /// </summary>
    [Serializable]
    public class ExtentInteractiveOverlay : BaseInteractiveOverlay
    {
        private PointF _tapPosition = new PointF();
        private PointF _originPosition = new PointF(0, 0);

        private PointF _trackStartScreenPoint = new PointF();
        private PointF _trackEndScreenPoint = new PointF();

        private int _zoomPercentage;

        [NonSerialized]
        private RelativeLayout _trackShape;

        public ExtentInteractiveOverlay(Context context)
            : base(context)
        {

            OverlayCanvas.Elevation = ZIndexes.ExtentInteractiveOverlay;
            PanMode = MapPanMode.Default;
            MapDoubleTapMode = MapDoubleTapMode.Default;
            _zoomPercentage = 50;
        }

        public MapPanMode PanMode { get; set; }

        public MapDoubleTapMode MapDoubleTapMode { get; set; }

        //public MapDoubleLeftClickMode DoubleLeftClickMode { get; set; }

        //public MapDoubleRightClickMode DoubleRightClickMode { get; set; }

        //public MapLeftClickDragMode LeftClickDragMode { get; set; }

        //public Keys LeftClickDragKey { get; set; }

        //public MapRightClickDragMode RightClickDragMode { get; set; }

        //public Keys RightClickDragKey { get; set; }

        public ExtentChangedType ExtentChangedType { get; protected set; }

        public int ZoomPercentage
        {
            get { return _zoomPercentage; }
            set { _zoomPercentage = value; }
        }

        protected override InteractiveResult MotionDownCore(MapMotionEventArgs motionArgs)
        {
            InteractiveResult interactiveResult = base.MotionDownCore(motionArgs);
       
            if(motionArgs.MotionAction == MotionEventActions.Down)
            {
                ExtentChangedType = ExtentChangedType.Pan;
            }
            else return interactiveResult;

            _originPosition = new PointF(motionArgs.ScreenX, motionArgs.ScreenY);
            interactiveResult.ProcessOtherOverlaysMode = ProcessOtherOverlaysMode.DoNotProcessOtherOverlays;
            return interactiveResult;
        }

        protected override InteractiveResult MotionMoveCore(MapMotionEventArgs motionArgs)
        {
            InteractiveResult interactiveResult = base.MotionMoveCore(motionArgs);

            PointF currentPosition = new PointF(motionArgs.ScreenX, motionArgs.ScreenY);
            _tapPosition = currentPosition;

            if (ExtentChangedType == ExtentChangedType.Pan && motionArgs.ScreenPointers.Count == 1)
            {
                double currentResolution = MapArguments.CurrentResolution;
                double offsetScreenX = currentPosition.X - _originPosition.X;
                double offsetScreenY = currentPosition.Y - _originPosition.Y;
                double offsetWorldX = offsetScreenX * currentResolution;
                double offsetWorldY = offsetScreenY * currentResolution;

                double left = motionArgs.CurrentExtent.UpperLeftPoint.X - offsetWorldX;
                double top = motionArgs.CurrentExtent.UpperLeftPoint.Y + offsetWorldY;
                double right = motionArgs.CurrentExtent.LowerRightPoint.X - offsetWorldX;
                double bottom = motionArgs.CurrentExtent.LowerRightPoint.Y + offsetWorldY;

                interactiveResult.NewCurrentExtent = new RectangleShape(left, top, right, bottom);
                _originPosition = currentPosition;
            }

            return interactiveResult;
        }

        protected override InteractiveResult DoubleTapCore(MapMotionEventArgs interactionArguments)
        {
            InteractiveResult interactiveResult = base.DoubleTapCore(interactionArguments);
            if (MapDoubleTapMode == MapDoubleTapMode.Disabled)
            {
                return interactiveResult;
            }

            int level = MapArguments.GetSnappedZoomLevelIndex(interactionArguments.CurrentExtent);
            double targetScale = MapArguments.ZoomLevelScales[level];

            ExtentChangedType = ExtentChangedType.DoubleTap;
            targetScale *= ((100d - (double)ZoomPercentage) / 100d);

            _tapPosition = new PointF(interactionArguments.ScreenX, interactionArguments.ScreenY);
            targetScale = MapArguments.ZoomLevelScales[MapUtil.GetSnappedZoomLevelIndex(targetScale, MapArguments.ZoomLevelScales, MapArguments.MinimumScale, MapArguments.MaximumScale)];
            double deltaX = interactionArguments.MapWidth * .5 - _tapPosition.X;
            double deltaY = _tapPosition.Y - interactionArguments.MapHeight * .5;
            double newResolution = MapUtil.GetResolutionFromScale(targetScale, interactionArguments.MapUnit);
            PointShape newWorldCenter = MapArguments.ToWorldCoordinate(new PointShape(_tapPosition.X, _tapPosition.Y));
            newWorldCenter.X += deltaX * newResolution;
            newWorldCenter.Y += deltaY * newResolution;
            interactiveResult.NewCurrentExtent = MapUtil.CalculateExtent(new PointF((float)newWorldCenter.X, (float)newWorldCenter.Y), targetScale, interactionArguments.MapUnit, interactionArguments.MapWidth, interactionArguments.MapHeight);

            return interactiveResult;
        }

        protected override InteractiveResult PinchEndCore(MapMotionEventArgs motionArgs)
        {
            InteractiveResult interactiveResult = base.PinchEndCore(motionArgs);

            int level = MapArguments.GetSnappedZoomLevelIndex(motionArgs.CurrentExtent);
            double targetScale = MapArguments.ZoomLevelScales[level];

            targetScale /= motionArgs.PinchFactor;

            ExtentChangedType = ExtentChangedType.Pinch;
            PointShape currentCenter = new PointShape(motionArgs.WorldX, motionArgs.WorldY);
            interactiveResult.NewCurrentExtent = MapUtil.CalculateExtent(new PointF((float)currentCenter.X, (float)currentCenter.Y), targetScale, motionArgs.MapUnit, motionArgs.MapWidth, motionArgs.MapHeight, motionArgs.Dpi);
            interactiveResult.NewZoomLevel = MapUtil.GetSnappedZoomLevelIndex(MapUtil.GetScale(motionArgs.MapUnit, interactiveResult.NewCurrentExtent, motionArgs.MapWidth, motionArgs.MapHeight), MapArguments.ZoomLevelScales);
            interactiveResult.ScreenCenterPoint = new PointF(motionArgs.ScreenX, motionArgs.ScreenY);

            return interactiveResult;
        }
    }
}

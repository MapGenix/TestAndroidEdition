using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Mapgenix.Shapes;
using Android.Graphics;
using Android.Views;
using Android.Widget;

namespace Mapgenix.GSuite.Android
{
    public partial class Map
    {
        private const int ClickPointTolerance = 50;
        private GestureDetector _gestureDetector;
        private ScaleGestureDetector _scaleDetector;
        
        public event EventHandler<MapMotionEventArgs> MapTapDown;

        public event EventHandler<MapMotionEventArgs> MapTapUp;

        public event EventHandler<MapMotionEventArgs> MapDoubleTap;

        public event EventHandler<MapMotionEventArgs> MapMove;

        public event EventHandler<MapMotionEventArgs> MapPinchMove;

        public event EventHandler<MapMotionEventArgs> MapPinchEnd;

        public event EventHandler<ExtentChangingEventArgs> CurrentExtentChanging;

        public event EventHandler<ExtentChangedEventArgs> CurrentExtentChanged;

        public event EventHandler<ScaleChangingEventArgs> CurrentScaleChanging;

        public event EventHandler<ScaleChangedEventArgs> CurrentScaleChanged;

        public event EventHandler<OverlayEventArgs> OverlayDrawing;

        public event EventHandler<OverlayEventArgs> OverlayDrawn;

        public event EventHandler<OverlaysEventArgs> OverlaysDrawing;

        public event EventHandler<OverlaysEventArgs> OverlaysDrawn;

        private void InitMapGestures()
        {
            _gestureDetector = new GestureDetector(Context, new MapSimpleGestureManager());

            _gestureDetector.DoubleTap += (object sender, GestureDetector.DoubleTapEventArgs e) => {
                MapMotionEventArgs motionArgs = CollectMotionEventArguments(e.Event);
                EventManagerDoubleTap(motionArgs);
                OnMapDoubleTap(motionArgs);
            };

            _scaleDetector = new ScaleGestureDetector(Context, new MapPinchGestureManager()
            {
                Sender = this
            });
        }

        public override bool OnTouchEvent(MotionEvent e)
        {
            if (TrackOverlay.TrackMode != TrackMode.None)
            {
                return TrackOverlay.OnTouchEvent(e, this);
            }              
            _scaleDetector.OnTouchEvent(e);
            _gestureDetector.OnTouchEvent(e);

            MapMotionEventArgs motionArgs = CollectMotionEventArguments(e);

            if (_scaleDetector.IsInProgress)
            {
                return true;
            }

            _currentMousePosition = new PointF(e.GetX(), e.GetY());
            switch (e.Action)
            {
                case MotionEventActions.Down:
                    EventManagerMotionDownCore(motionArgs);
                    OnMapTapDown(motionArgs);
                    break;

                case MotionEventActions.Move:                    
                    EventManagerMotionMoveCore(motionArgs);
                    OnMapMove(motionArgs);
                    break;

                case MotionEventActions.Up:
                    EventManagerMotionUpCore(motionArgs);
                    OnMapTapUp(motionArgs);
                    break;
                case MotionEventActions.Cancel:
                    break;

                case MotionEventActions.PointerUp:
                    break;
            }
            return true;
        }

        protected virtual void OnMapTapDown(MapMotionEventArgs e)
        {
            EventHandler<MapMotionEventArgs> handler = MapTapDown;
            if(handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnMapTapUp(MapMotionEventArgs e)
        {
            EventHandler<MapMotionEventArgs> handler = MapTapUp;
            if(handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnMapDoubleTap(MapMotionEventArgs e)
        {
            EventHandler<MapMotionEventArgs> handler = MapDoubleTap;
            if(handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnMapPinchMove(MapMotionEventArgs e)
        {
            EventHandler<MapMotionEventArgs> handler = MapPinchMove;
            if(handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnMapPinchEnd(MapMotionEventArgs e)
        {
            EventHandler<MapMotionEventArgs> handler = MapPinchEnd;
            if(handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnCurrentExtentChanging(ExtentChangingEventArgs e)
        {
            EventHandler<ExtentChangingEventArgs> handler = CurrentExtentChanging;
            if (handler != null)
            {
                handler(this, e);
            }
        }
      
        protected virtual void OnCurrentExtentChanged(ExtentChangedEventArgs e)
        {
            EventHandler<ExtentChangedEventArgs> handler = CurrentExtentChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

       
        protected virtual void OnCurrentScaleChanging(ScaleChangingEventArgs e)
        {
            EventHandler<ScaleChangingEventArgs> handler = CurrentScaleChanging;
            if (handler != null)
            {
                handler(this, e);
            }
        }

       
        protected virtual void OnCurrentScaleChanged(ScaleChangedEventArgs e)
        {
            EventHandler<ScaleChangedEventArgs> handler = CurrentScaleChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

       
        protected virtual void OnOverlayDrawing(OverlayEventArgs e)
        {
            EventHandler<OverlayEventArgs> handler = OverlayDrawing;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        
        protected virtual void OnOverlayDrawn(OverlayEventArgs e)
        {
            EventHandler<OverlayEventArgs> handler = OverlayDrawn;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        
        protected virtual void OnOverlaysDrawing(OverlaysEventArgs e)
        {
            EventHandler<OverlaysEventArgs> handler = OverlaysDrawing;
            if (handler != null)
            {
                handler(this, e);
            }
        }

       
        protected virtual void OnOverlaysDrawn(OverlaysEventArgs e)
        {
            EventHandler<OverlaysEventArgs> handler = OverlaysDrawn;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnMapMove(MapMotionEventArgs e)
        {
            EventHandler<MapMotionEventArgs> handler = MapMove;
            if(handler != null)
            {
                handler(this, e);
            }
        }

        private MapMotionEventArgs CollectMotionEventArguments(MotionEvent e)
        {
            PointShape currentWorldPoint = ToWorldCoordinate(new PointF(e.GetX(), e.GetY()));
            MapMotionEventArgs arguments = new MapMotionEventArgs();
            arguments.CurrentExtent = CurrentExtent;
            arguments.MapHeight = (int)MapHeight;
            arguments.MapWidth = (int)MapWidth;
            arguments.MapUnit = MapUnit;
            arguments.Scale = CurrentScale;
            arguments.ScreenX = (float)e.GetX();
            arguments.ScreenY = (float)e.GetY();
            arguments.WorldX = currentWorldPoint.X;
            arguments.WorldY = currentWorldPoint.Y;
            arguments.MotionAction = e.Action;

            if(e.PointerCount > 0)
                for(int i = 0; i < e.PointerCount; i++)
                    arguments.ScreenPointers.Add(new PointF(e.GetX(i), e.GetY(i)));

            if (!Double.IsNaN(MapWidth) && MapWidth != 0 && !Double.IsNaN(MapHeight) && MapHeight != 0)
            {
                arguments.SearchingTolerance = ClickPointTolerance * Math.Max(CurrentExtent.Width / MapWidth, CurrentExtent.Height / MapHeight);
            }

            return arguments;
        }

        private MapMotionEventArgs CollectMotionEventArguments(PointF point)
        {
            PointShape currentWorldPoint = ToWorldCoordinate(point);
            MapMotionEventArgs arguments = new MapMotionEventArgs();
            arguments.CurrentExtent = CurrentExtent;
            arguments.MapHeight = (int)MapHeight;
            arguments.MapWidth = (int)MapWidth;
            arguments.MapUnit = MapUnit;
            arguments.Scale = CurrentScale;
            arguments.ScreenX = (float)point.X;
            arguments.ScreenY = (float)point.Y;
            arguments.WorldX = currentWorldPoint.X;
            arguments.WorldY = currentWorldPoint.Y;
            arguments.MotionAction = MotionEventActions.Mask;

            if (!Double.IsNaN(MapWidth) && MapWidth != 0 && !Double.IsNaN(MapHeight) && MapHeight != 0)
            {
                arguments.SearchingTolerance = ClickPointTolerance * Math.Max(CurrentExtent.Width / MapWidth, CurrentExtent.Height / MapHeight);
            }

            return arguments;
        }

        private bool ProcessWithInteractiveResult(InteractiveResult interactiveResult, BaseInteractiveOverlay interactiveOverlay)
        {
            bool isBreak = false;
            if (interactiveResult.NewCurrentExtent != null)
            {                
                if (IsInPanning())
                {
                    Draw(interactiveResult.NewCurrentExtent, OverlayRefreshType.Pan);
                }
                else
                {
                    Draw(interactiveResult.NewCurrentExtent, OverlayRefreshType.Redraw);
                }
            }

            if (interactiveResult.DrawThisOverlay == DrawType.Draw)
            {
                interactiveOverlay.Draw(CurrentExtent);
            }

            if (interactiveResult.ProcessOtherOverlaysMode == ProcessOtherOverlaysMode.DoNotProcessOtherOverlays)
            {
                isBreak = true;
            }

            return isBreak;
        }        

        private void EventManagerMotionMoveCore(MapMotionEventArgs interactionArguments)
        {
            Collection<BaseInteractiveOverlay> currentInteractiveOverlays = CollectCurrentInteractiveOverlays();
            foreach (BaseInteractiveOverlay overlay in currentInteractiveOverlays)
            {
                if (!overlay.IsVisible) continue;
                InteractiveResult interactiveResult = overlay.MotionMove(interactionArguments);
                if (ProcessWithInteractiveResult(interactiveResult, overlay)) { break; }
            }
        }

        private void EventManagerMotionDownCore(MapMotionEventArgs motionArgs)
        {
            Collection<BaseInteractiveOverlay> currentInteractiveOverlays = CollectCurrentInteractiveOverlays();
            foreach (BaseInteractiveOverlay overlay in currentInteractiveOverlays)
            {
                if (!overlay.IsVisible) continue;
                InteractiveResult interactiveResult = overlay.MotionDown(motionArgs);
                if (ProcessWithInteractiveResult(interactiveResult, overlay)) { break; };
            }
        }

        private void EventManagerMotionUpCore(MapMotionEventArgs motionArgs)
        {
            Collection<BaseInteractiveOverlay> currentInteractiveOverlays = CollectCurrentInteractiveOverlays();
            foreach (BaseInteractiveOverlay overlay in currentInteractiveOverlays)
            {
                if (!overlay.IsVisible) continue;
                InteractiveResult interactiveResult = overlay.MotionUp(motionArgs);
                if (ProcessWithInteractiveResult(interactiveResult, overlay)) { break; };
            }
        }

        private void EventManagerDoubleTap(MapMotionEventArgs motionArgs)
        {
            motionArgs.MotionAction = MotionEventActions.Down;
            motionArgs.CurrentExtent = _targetSnappedExtent == null ? CurrentExtent : _targetSnappedExtent;            
            EventManagerDoubleTapCore(motionArgs);           
        }

        private void EventManagerDoubleTapCore(MapMotionEventArgs interactionArguments)
        {
            Collection<BaseInteractiveOverlay> currentInteractiveOverlays = CollectCurrentInteractiveOverlays();
            foreach (BaseInteractiveOverlay overlay in currentInteractiveOverlays)
            {
                if (!overlay.IsVisible) continue;
                InteractiveResult interactiveResult = overlay.DoubleTap(interactionArguments);
                if (ProcessWithInteractiveResult(interactiveResult, overlay)) { break; }
            }
        }

        internal void EventManagerPinch(float scaleFactor, float startingFocusX, float startingFocusY)
        {
            PointF currentScreenPoint = new PointF(startingFocusX, startingFocusY);
            MapMotionEventArgs interactionArguments = CollectMotionEventArguments(currentScreenPoint);
            interactionArguments.PinchFactor = scaleFactor;

            interactionArguments.CurrentExtent = _targetSnappedExtent == null ? CurrentExtent : _targetSnappedExtent;
            EventManagerPinchCore(interactionArguments);
            OnMapPinchMove(interactionArguments);
        }

        private void EventManagerPinchCore(MapMotionEventArgs interactionArguments)
        {           
            OverlayCanvas.PostScale(interactionArguments.PinchFactor, interactionArguments.ScreenX, interactionArguments.ScreenY);
        }

        internal void EventManagerPinchEnd(float scaleFactor, float startingFocusX, float startingFocusY)
        {
            PointF currentScreenPoint = new PointF(startingFocusX, startingFocusY);
            MapMotionEventArgs interactionArguments = CollectMotionEventArguments(currentScreenPoint);
            interactionArguments.PinchFactor = scaleFactor;
            interactionArguments.CurrentExtent = _targetSnappedExtent == null ? CurrentExtent : _targetSnappedExtent;
            interactionArguments.Dpi = _dpi;
            PointShape point = ToScreenCoordinate(CurrentExtent.GetCenterPoint());

            _currentMousePosition = new PointF(startingFocusX, startingFocusY);
            OverlayCanvas.PostScale(1, startingFocusX, startingFocusY);
            EventManagerPinchEndCore(interactionArguments);
            OnMapPinchEnd(interactionArguments);
        }

        private void EventManagerPinchEndCore(MapMotionEventArgs interactionArguments)
        {
            Collection<BaseInteractiveOverlay> currentInteractiveOverlays = CollectCurrentInteractiveOverlays();
            foreach (BaseInteractiveOverlay overlay in currentInteractiveOverlays)
            {
                if (!overlay.IsVisible) continue;
                InteractiveResult interactiveResult = overlay.PinchEnd(interactionArguments);
                if (ProcessWithInteractiveResult(interactiveResult, overlay)) { break; }
            }
        }
    }
}

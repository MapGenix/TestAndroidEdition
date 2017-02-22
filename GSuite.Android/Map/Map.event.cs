using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Mapgenix.Shapes;
using Android.Graphics;
using Android.Views;

namespace Mapgenix.GSuite.Android
{
    public partial class Map
    {
        private const int ClickPointTolerance = 14;
        private PointF _tempMotionDownPosition = new PointF(0, 0);
        private int _activePointerId = -1;
        private float _lastTouchX;
        private float _lastTouchY;
        private float _posX;
        private float _posY;

        //public event EventHandler<MapClickEventArgs> MapClick;

        //public event EventHandler<MapClickEventArgs> MapDoubleClick;

        public event EventHandler<ExtentChangingEventArgs> CurrentExtentChanging;

        public event EventHandler<ExtentChangedEventArgs> CurrentExtentChanged;

        public event EventHandler<ScaleChangingEventArgs> CurrentScaleChanging;

        public event EventHandler<ScaleChangedEventArgs> CurrentScaleChanged;

        public event EventHandler<OverlayEventArgs> OverlayDrawing;

        public event EventHandler<OverlayEventArgs> OverlayDrawn;

        public event EventHandler<OverlaysEventArgs> OverlaysDrawing;

        public event EventHandler<OverlaysEventArgs> OverlaysDrawn;

        /*protected virtual void OnMapClick(MapClickEventArgs e)
        {
            EventHandler<MapClickEventArgs> handler = MapClick;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnMapDoubleClick(MapClickEventArgs e)
        {
            EventHandler<MapClickEventArgs> handler = MapDoubleClick;
            if (handler != null)
            {
                handler(this, e);
            }
        }*/

        public override bool OnTouchEvent(MotionEvent ev)
        {
            //_scaleDetector.OnTouchEvent(ev);

            MotionEventActions action = ev.Action & MotionEventActions.Mask;
            int pointerIndex;
            PointF currentScreenPoint = new PointF();
            MotionEventArgs motionArgs = new MotionEventArgs();
            switch (action)
            {
                case MotionEventActions.Down:
                    _tempMotionDownPosition = new PointF(ev.GetX(), ev.GetY());
                    _activePointerId = ev.GetPointerId(0);
                    _currentMousePosition = _tempMotionDownPosition;
                    //MotionEventArgs motionArgs = CollectMotionEventArguments(_tempMotionDownPosition);

                    

                    //MapMouseButton mouseButton = ConvertToMapMouseButton(e);
                    currentScreenPoint = _tempMotionDownPosition;
                    PointShape worldCoordinate = ToWorldCoordinate(currentScreenPoint);
                    //MapClickEventArgs clickArgs = new MapClickEventArgs((float)currentScreenPoint.X, (float)currentScreenPoint.Y, worldCoordinate.X, worldCoordinate.Y, mouseButton);

                    //OnMapClick(clickArgs);

                    motionArgs = CollectMotionEventArguments(_tempMotionDownPosition);
                    motionArgs.MotionAction = MotionEventActions.Move;
                    //interactionArguments.MouseButton = mouseButton;
                    //EventManagerMouseClickCore(interactionArguments);
                    EventManagerMotionDownCore(motionArgs);

                    break;

                case MotionEventActions.Move:

                    if (ev != null)
                    {
                        pointerIndex = ev.FindPointerIndex(_activePointerId);

                        float x = ev.GetX(pointerIndex);
                        float y = ev.GetY(pointerIndex);

                        float deltaX = x - _lastTouchX;
                        float deltaY = y - _lastTouchY;
                        _posX += deltaX;
                        _posY += deltaY;

                        currentScreenPoint = new PointF(x, y);
                        _currentMousePosition = currentScreenPoint;
                        motionArgs = CollectMotionEventArguments(currentScreenPoint);

                        motionArgs.MotionAction = MotionEventActions.Move;

                        /*MotionEventActions mapMouseButton = CollectMapMouseButton(e);
                        if (mapMouseButton != MapMouseButton.None)
                        {
                            interactionArguments.MouseButton = mapMouseButton;
                        }*/

                        _lastTouchX = x;
                        _lastTouchY = y;

                        EventManagerMotionMoveCore(motionArgs);
                    }

                    /*pointerIndex = ev.FindPointerIndex(_activePointerId);
                    float x = ev.GetX(pointerIndex);
                    float y = ev.GetY(pointerIndex);
                    if (!_scaleDetector.IsInProgress)
                    {
                        // Only move the ScaleGestureDetector isn't already processing a gesture.
                        float deltaX = x - _lastTouchX;
                        float deltaY = y - _lastTouchY;
                        _posX += deltaX;
                        _posY += deltaY;
                        Invalidate();
                    }

                    _lastTouchX = x;
                    _lastTouchY = y;*/
                    break;

                case MotionEventActions.Up:
                case MotionEventActions.Cancel:
                    // This events occur when something cancels the gesture (for example the
                    // activity going in the background) or when the pointer has been lifted up.
                    // We no longer need to keep track of the active pointer.
                    //_activePointerId = InvalidPointerId;
                    break;

                case MotionEventActions.PointerUp:
                    /*// We only want to update the last touch position if the the appropriate pointer
                    // has been lifted off the screen.
                    pointerIndex = (int)(ev.Action & MotionEventActions.PointerIndexMask) >> (int)MotionEventActions.PointerIndexShift;
                    int pointerId = ev.GetPointerId(pointerIndex);
                    if (pointerId == _activePointerId)
                    {
                        // This was our active pointer going up. Choose a new
                        // action pointer and adjust accordingly
                        int newPointerIndex = pointerIndex == 0 ? 1 : 0;
                        _lastTouchX = ev.GetX(newPointerIndex);
                        _lastTouchY = ev.GetY(newPointerIndex);
                        _activePointerId = ev.GetPointerId(newPointerIndex);
                    }*/
                    break;
            }
            return true;
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

        private void BindingEvent()
        {
            /*MouseEventManager eventManager = new MouseEventManager(_eventCanvas);

            eventManager.MouseLeave += EventManagerMouseLeave;
            eventManager.MouseButtonDown += EventManagerMouseButtonDown;
            eventManager.MouseButtonUp += EventManagerMouseButtonUp;
            eventManager.MouseClick += EventManagerMouseClick;
            eventManager.MouseDoubleClick += EventManagerMouseDoubleClick;
            eventManager.MouseMove += EventManagerMouseMove;
            eventManager.MouseWheel += EventManagerMouseWheel;
            eventManager.MouseEnter += EventManagerMouseEnter;
            eventManager.ActualMouseButtonDown += EventManagerActualMouseButtonDown;

            _eventCanvas.KeyDown += EventCanvasKeyDown;
            _eventCanvas.KeyUp += EventCanvasKeyUp;*/
        }

        private MotionEventArgs CollectMotionEventArguments(PointF currentScreenPoint)
        {
            PointShape currentWorldPoint = ToWorldCoordinate(currentScreenPoint);
            MotionEventArgs arguments = new MotionEventArgs();
            arguments.CurrentExtent = CurrentExtent;
            arguments.MapHeight = (int)LayoutParameters.Height;
            arguments.MapWidth = (int)LayoutParameters.Width;
            arguments.MapUnit = MapUnit;
            arguments.MouseWheelDelta = 0;
            arguments.Scale = CurrentScale;
            arguments.ScreenX = (float)currentScreenPoint.X;
            arguments.ScreenY = (float)currentScreenPoint.Y;
            arguments.WorldX = currentWorldPoint.X;
            arguments.WorldY = currentWorldPoint.Y;
            arguments.MotionAction = MotionEventActions.Mask;

            if (!Double.IsNaN(LayoutParameters.Width) && LayoutParameters.Width != 0 && !Double.IsNaN(LayoutParameters.Height) && LayoutParameters.Height != 0)
            {
                arguments.SearchingTolerance = ClickPointTolerance * Math.Max(CurrentExtent.Width / LayoutParameters.Height, CurrentExtent.Height / LayoutParameters.Height);
            }

            return arguments;
        }

        /*private KeyEventInteractionArguments CollectKeyEventInteractiveArguments(KeyEventArgs e)
        {
            string keyCode = e.Key.ToString();
            bool isAltPressed = (Keyboard.Modifiers == ModifierKeys.Alt);
            bool isCtlPressed = (Keyboard.Modifiers == ModifierKeys.Control);
            bool isShtPressed = (Keyboard.Modifiers == ModifierKeys.Shift);

            KeyEventInteractionArguments keyInteractiveArguments = new KeyEventInteractionArguments(keyCode, isCtlPressed, isShtPressed, isAltPressed);
            keyInteractiveArguments.CurrentExtent = CurrentExtent;
            keyInteractiveArguments.CurrentScale = CurrentScale;

            return keyInteractiveArguments;
        }*/

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

        /*private void EventManagerMouseWheel(object sender, MouseWheelEventArgs e)
        {
            Point currentScreenPoint = e.GetPosition((UIElement)sender);
            InteractionArguments interactionArguments = CollectMouseEventArguments(currentScreenPoint);
            interactionArguments.MouseButton = MapMouseButton.Middle;
            interactionArguments.MouseWheelDelta = e.Delta;
            interactionArguments.CurrentExtent = _targetSnappedExtent == null ? CurrentExtent : _targetSnappedExtent;

            EventManagerMouseWheelCore(interactionArguments);
        }

        private void EventManagerMouseWheelCore(InteractionArguments interactionArguments)
        {
            Collection<BaseInteractiveOverlay> currentInteractiveOverlays = CollectCurrentInteractiveOverlays();
            foreach (BaseInteractiveOverlay overlay in currentInteractiveOverlays)
            {
                if (!overlay.IsVisible) continue;
                InteractiveResult interactiveResult = overlay.MouseWheel(interactionArguments);
                if (ProcessWithInteractiveResult(interactiveResult, overlay)) { break; }
            }
        }

        private void EventManagerMouseMove(object sender, MouseEventArgs e)
        {
            Point currentScreenPoint = e.GetPosition((UIElement)sender);
            _currentMousePosition = currentScreenPoint;
            InteractionArguments interactionArguments = CollectMouseEventArguments(currentScreenPoint);

            MapMouseButton mapMouseButton = CollectMapMouseButton(e);
            if (mapMouseButton != MapMouseButton.None)
            {
                interactionArguments.MouseButton = mapMouseButton;
            }
            
            EventManagerMouseMoveCore(interactionArguments);
        }*/

        private void EventManagerMotionMoveCore(MotionEventArgs interactionArguments)
        {
            Collection<BaseInteractiveOverlay> currentInteractiveOverlays = CollectCurrentInteractiveOverlays();
            foreach (BaseInteractiveOverlay overlay in currentInteractiveOverlays)
            {
                if (!overlay.IsVisible) continue;
                InteractiveResult interactiveResult = overlay.MotionMove(interactionArguments);
                if (ProcessWithInteractiveResult(interactiveResult, overlay)) { break; }
            }
        }

        /*private void EventManagerMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            MapMouseButton mouseButton = ConvertToMapMouseButton(e);
            Point currentScreenPoint = _tempMouseDownPosition;
            PointShape worldCoordinate = ToWorldCoordinate(currentScreenPoint);
            MapClickEventArgs clickArgs = new MapClickEventArgs((float)currentScreenPoint.X, (float)currentScreenPoint.Y, worldCoordinate.X, worldCoordinate.Y, mouseButton);

            OnMapDoubleClick(clickArgs);

            InteractionArguments interactionArguments = CollectMouseEventArguments(currentScreenPoint);
            interactionArguments.MouseButton = mouseButton;
            EventManagerMouseDoubleClickCore(interactionArguments);
        }

        private void EventManagerMouseDoubleClickCore(InteractionArguments interactionArguments)
        {
            Collection<BaseInteractiveOverlay> currentInteractiveOverlays = CollectCurrentInteractiveOverlays();
            foreach (BaseInteractiveOverlay overlay in currentInteractiveOverlays)
            {
                if (!overlay.IsVisible) continue;
                InteractiveResult interactiveResult = overlay.MouseDoubleClick(interactionArguments);
                if (ProcessWithInteractiveResult(interactiveResult, overlay)) { break; };
            }
        }

        private void EventManagerMouseClick(object sender, MouseButtonEventArgs e)
        {
            MapMouseButton mouseButton = ConvertToMapMouseButton(e);
            Point currentScreenPoint = _tempMouseDownPosition; 
            PointShape worldCoordinate = ToWorldCoordinate(currentScreenPoint);
            MapClickEventArgs clickArgs = new MapClickEventArgs((float)currentScreenPoint.X, (float)currentScreenPoint.Y, worldCoordinate.X, worldCoordinate.Y, mouseButton);

            OnMapClick(clickArgs);

            InteractionArguments interactionArguments = CollectMouseEventArguments(currentScreenPoint);
            interactionArguments.MouseButton = mouseButton;
            EventManagerMouseClickCore(interactionArguments);
        }

        private void EventManagerMouseClickCore(InteractionArguments interactionArguments)
        {
            Collection<BaseInteractiveOverlay> currentInteractiveOverlays = CollectCurrentInteractiveOverlays();
            foreach (BaseInteractiveOverlay overlay in currentInteractiveOverlays)
            {
                if (!overlay.IsVisible) continue;
                InteractiveResult interactiveResult = overlay.MouseClick(interactionArguments);
                if (ProcessWithInteractiveResult(interactiveResult, overlay)) { break; };
            }
        }*/

        private void EventManagerMotionDownCore(MotionEventArgs motionArgs)
        {
            Collection<BaseInteractiveOverlay> currentInteractiveOverlays = CollectCurrentInteractiveOverlays();
            foreach (BaseInteractiveOverlay overlay in currentInteractiveOverlays)
            {
                if (!overlay.IsVisible) continue;
                InteractiveResult interactiveResult = overlay.MotionDown(motionArgs);
                if (ProcessWithInteractiveResult(interactiveResult, overlay)) { break; };
            }
        }

        /*private void EventManagerMouseButtonUp(object sender, MouseButtonEventArgs e)
        {
            Point currentScreenPoint = e.GetPosition((UIElement)sender);
            InteractionArguments interactionArguments = CollectMouseEventArguments(currentScreenPoint);
            interactionArguments.MouseButton = ConvertToMapMouseButton(e);
            EventManagerMouseButtonUpCore(interactionArguments);
          
        }

        private void EventManagerMouseButtonUpCore(InteractionArguments interactionArguments)
        {
            Collection<BaseInteractiveOverlay> currentInteractiveOverlays = CollectCurrentInteractiveOverlays();
            foreach (BaseInteractiveOverlay overlay in currentInteractiveOverlays)
            {
                if (!overlay.IsVisible) continue;
                InteractiveResult interactiveResult = overlay.MouseUp(interactionArguments);
                if (ProcessWithInteractiveResult(interactiveResult, overlay)) { break; };
            }
        }

        private void EventManagerMouseButtonDown(object sender, MouseButtonEventArgs e)
        {
            
            Point currentScreenPoint = _tempMouseDownPosition;
            InteractionArguments interactionArguments = CollectMouseEventArguments(currentScreenPoint);
            interactionArguments.MouseButton = ConvertToMapMouseButton(e);
            EventManagerMouseButtonDownCore(interactionArguments);
        }

        private void EventManagerMouseButtonDownCore(InteractionArguments interactionArguments)
        {
            Collection<BaseInteractiveOverlay> currentInteractiveOverlays = CollectCurrentInteractiveOverlays();
            foreach (BaseInteractiveOverlay overlay in currentInteractiveOverlays)
            {
                if (!overlay.IsVisible) continue;
                InteractiveResult interactiveResult = overlay.MouseDown(interactionArguments);
                if (ProcessWithInteractiveResult(interactiveResult, overlay)) { break; }
            }
        }

        private void EventManagerMouseLeave(object sender, MouseEventArgs e)
        {
            Point currentScreenPoint = e.GetPosition((UIElement)sender);
            InteractionArguments interactionArguments = CollectMouseEventArguments(currentScreenPoint);
            EventManagerMouseLeaveCore(interactionArguments);
        }

        private void EventManagerMouseLeaveCore(InteractionArguments interactionArguments)
        {
            Collection<BaseInteractiveOverlay> currentInteractiveOverlays = CollectCurrentInteractiveOverlays();
            foreach (BaseInteractiveOverlay overlay in currentInteractiveOverlays)
            {
                if (!overlay.IsVisible) continue;
                InteractiveResult interactiveResult = overlay.MouseLeave(interactionArguments);
                if (ProcessWithInteractiveResult(interactiveResult, overlay)) { break; }
            }
        }

        private void EventManagerMouseEnter(object sender, MouseEventArgs e)
        {
            Point currentScreenPoint = e.GetPosition((UIElement)sender);
            InteractionArguments interactionArguments = CollectMouseEventArguments(currentScreenPoint);

            MapMouseButton mapMouseButton = CollectMapMouseButton(e);
            if (mapMouseButton != MapMouseButton.None)
            {
                interactionArguments.MouseButton = mapMouseButton;
            }

            EventManagerMouseEnterCore(interactionArguments);
        }

        private void EventManagerMouseEnterCore(InteractionArguments interactionArguments)
        {
            Collection<BaseInteractiveOverlay> currentInteractiveOverlays = CollectCurrentInteractiveOverlays();
            foreach (BaseInteractiveOverlay overlay in currentInteractiveOverlays)
            {
                if (!overlay.IsVisible) continue;
                InteractiveResult interactiveResult = overlay.MouseEnter(interactionArguments);
                if (ProcessWithInteractiveResult(interactiveResult, overlay)) { break; }
            }
        }

        private void EventCanvasKeyUp(object sender, KeyEventArgs e)
        {
            KeyEventInteractionArguments interactiveArguments = CollectKeyEventInteractiveArguments(e);
            EventCanvasKeyUpCore(interactiveArguments);
        }

        private void EventManagerActualMouseButtonDown(object sender, MouseButtonEventArgs e)
        {
            _tempMouseDownPosition = e.GetPosition(_eventCanvas);
        }

        private void EventCanvasKeyUpCore(KeyEventInteractionArguments interactionArguments)
        {
            Collection<BaseInteractiveOverlay> currentInteractiveOverlays = CollectCurrentInteractiveOverlays();
            foreach (BaseInteractiveOverlay overlay in currentInteractiveOverlays)
            {
                if (!overlay.IsVisible) continue;
                InteractiveResult interactiveResult = overlay.KeyUp(interactionArguments);
                if (ProcessWithInteractiveResult(interactiveResult, overlay)) { break; }
            }
        }

        private void EventCanvasKeyDown(object sender, KeyEventArgs e)
        {
            KeyEventInteractionArguments interactiveArguments = CollectKeyEventInteractiveArguments(e);
            EventCanvasKeyDownCore(interactiveArguments);
        }

        private void EventCanvasKeyDownCore(KeyEventInteractionArguments interactiveArguments)
        {
            Collection<BaseInteractiveOverlay> currentInteractiveOverlays = CollectCurrentInteractiveOverlays();
            foreach (BaseInteractiveOverlay overlay in currentInteractiveOverlays)
            {
                if (!overlay.IsVisible) continue;
                InteractiveResult interactiveResult = overlay.KeyDown(interactiveArguments);
                if (ProcessWithInteractiveResult(interactiveResult, overlay)) { break; }
            }
        }

        private static MapMouseButton ConvertToMapMouseButton(MouseButtonEventArgs e)
        {
            string changedButton = e.ChangedButton.ToString();
            MapMouseButton mouseButton = (MapMouseButton)Enum.Parse(typeof(MapMouseButton), changedButton);
            return mouseButton;
        }

        private static MapMouseButton CollectMapMouseButton(MouseEventArgs e)
        {
            MapMouseButton mouseButton = MapMouseButton.None;
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                mouseButton = MapMouseButton.Left;
            }
            else if (e.RightButton == MouseButtonState.Pressed)
            {
                mouseButton = MapMouseButton.Right;
            }
            else if (e.MiddleButton == MouseButtonState.Pressed)
            {
                mouseButton = MapMouseButton.Middle;
            }

            return mouseButton;
        }*/
    }
}

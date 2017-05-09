﻿using System;
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
        private const int ClickPointTolerance = 50;
        /*private PointF _tempMotionDownPosition = new PointF(0, 0);
        private int _activePointerId = -1;
        private float _lastTouchX;
        private float _lastTouchY;
        private float _posX;
        private float _posY;*/
        private GestureDetector _gestureDetector;
        private ScaleGestureDetector _scaleDetector;
        //private int _fingerDown = 0;

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

        public event EventHandler<MapMotionEventArgs> MapMove;

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


        private void InitMapGestures()
        {
            _gestureDetector = new GestureDetector(Context, new MapSimpleGestureManager());

            _gestureDetector.DoubleTap += (object sender, GestureDetector.DoubleTapEventArgs e) => {
                EventManagerDoubleTap(sender, e.Event);
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

            _currentMousePosition = new PointF(e.GetX(), e.GetY());

            MapMotionEventArgs motionArgs = CollectMotionEventArguments(e);

            switch (e.Action)
            {
                case MotionEventActions.Down:
                    EventManagerMotionDownCore(motionArgs);
                    break;

                case MotionEventActions.Move:

                    if (!_scaleDetector.IsInProgress)
                    {
                        EventManagerMotionMoveCore(motionArgs);
                        OnMapMove(motionArgs);
                    }

                    break;

                case MotionEventActions.Up:
                    EventManagerMotionUpCore(motionArgs);
                    break;
                case MotionEventActions.Cancel:
                    break;

                case MotionEventActions.PointerUp:
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
                //if (TrackOverlay.TrackMode != TrackMode.None && !(overlay is TrackInteractiveOverlay)) continue;
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
                //if (TrackOverlay.TrackMode != TrackMode.None && !(overlay is TrackInteractiveOverlay)) continue;
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
                //if (TrackOverlay.TrackMode != TrackMode.None && !(overlay is TrackInteractiveOverlay)) continue;
                InteractiveResult interactiveResult = overlay.MotionUp(motionArgs);
                if (ProcessWithInteractiveResult(interactiveResult, overlay)) { break; };
            }
        }

        private void EventManagerDoubleTap(object sender, MotionEvent e)
        {
            MapMotionEventArgs interactionArguments = CollectMotionEventArguments(e);
            interactionArguments.MotionAction = MotionEventActions.Down;
            interactionArguments.CurrentExtent = _targetSnappedExtent == null ? CurrentExtent : _targetSnappedExtent;            
            EventManagerDoubleTapCore(interactionArguments);
        }

        private void EventManagerDoubleTapCore(MapMotionEventArgs interactionArguments)
        {
            Collection<BaseInteractiveOverlay> currentInteractiveOverlays = CollectCurrentInteractiveOverlays();
            foreach (BaseInteractiveOverlay overlay in currentInteractiveOverlays)
            {
                if (!overlay.IsVisible) continue;
                //if (TrackOverlay.TrackMode != TrackMode.None && !(overlay is TrackInteractiveOverlay)) continue;
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
            OverlayCanvas.PostScale(1, startingFocusX, startingFocusY);
            EventManagerPinchEndCore(interactionArguments);
        }

        private void EventManagerPinchEndCore(MapMotionEventArgs interactionArguments)
        {
            Collection<BaseInteractiveOverlay> currentInteractiveOverlays = CollectCurrentInteractiveOverlays();
            foreach (BaseInteractiveOverlay overlay in currentInteractiveOverlays)
            {
                if (!overlay.IsVisible) continue;
                //if (TrackOverlay.TrackMode != TrackMode.None && !(overlay is TrackInteractiveOverlay)) continue;
                InteractiveResult interactiveResult = overlay.PinchEnd(interactionArguments);
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
        }
        
        private void EventManagerMouseButtonUp(object sender, MouseButtonEventArgs e)
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

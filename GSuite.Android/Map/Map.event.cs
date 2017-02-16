using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Mapgenix.Shapes;
using Android.Graphics;

namespace Mapgenix.GSuite.Android
{
    public partial class Map
    {
        private const int ClickPointTolerance = 14;
        private Point _tempMouseDownPosition = new Point();

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

        /*private InteractionArguments CollectMouseEventArguments(Point currentScreenPoint)
        {
            PointShape currentWorldPoint = ToWorldCoordinate(currentScreenPoint);
            InteractionArguments arguments = new InteractionArguments();
            arguments.CurrentExtent = CurrentExtent;
            arguments.MapHeight = (int)ActualHeight;
            arguments.MapWidth = (int)ActualWidth;
            arguments.MapUnit = MapUnit;
            arguments.MouseWheelDelta = 0;
            arguments.Scale = CurrentScale;
            arguments.ScreenX = (float)currentScreenPoint.X;
            arguments.ScreenY = (float)currentScreenPoint.Y;
            arguments.WorldX = currentWorldPoint.X;
            arguments.WorldY = currentWorldPoint.Y;
            arguments.MouseButton = MapMouseButton.None;

            if (!Double.IsNaN(ActualWidth) && ActualWidth != 0 && !Double.IsNaN(ActualHeight) && ActualHeight != 0)
            {
                arguments.SearchingTolerance = ClickPointTolerance * Math.Max(CurrentExtent.Width / ActualWidth, CurrentExtent.Height / ActualHeight);
            }

            return arguments;
        }*/

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

        private void EventManagerMouseWheel(object sender, MouseWheelEventArgs e)
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
        }

        private void EventManagerMouseMoveCore(InteractionArguments interactionArguments)
        {
            Collection<BaseInteractiveOverlay> currentInteractiveOverlays = CollectCurrentInteractiveOverlays();
            foreach (BaseInteractiveOverlay overlay in currentInteractiveOverlays)
            {
                if (!overlay.IsVisible) continue;
                InteractiveResult interactiveResult = overlay.MouseMove(interactionArguments);
                if (ProcessWithInteractiveResult(interactiveResult, overlay)) { break; }
            }
        }

        private void EventManagerMouseDoubleClick(object sender, MouseButtonEventArgs e)
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

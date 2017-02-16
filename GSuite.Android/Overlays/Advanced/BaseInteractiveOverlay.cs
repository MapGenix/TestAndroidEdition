using System;
using Mapgenix.Shapes;
using Android.Content;

namespace Mapgenix.GSuite.Android
{
    /// <summary>
    /// Class inheriting from Overlay abstract class. This specified overlay describle the interative
    /// process with MapControl using Mouse or Keyboard.
    /// </summary>
    [Serializable]
    public abstract class BaseInteractiveOverlay : BaseOverlay
    {
        public event EventHandler<MapMouseEventArgs> MapMouseDown;

        public event EventHandler<MapMouseEventArgs> MapMouseMove;

        public event EventHandler<MapMouseEventArgs> MapMouseUp;

        public event EventHandler<MapMouseEventArgs> MapMouseClick;

        public event EventHandler<MapMouseEventArgs> MapMouseDoubleClick;

        public event EventHandler<MapMouseEventArgs> MapMouseWheel;

        public event EventHandler<MapMouseEventArgs> MapMouseLeave;

        public event EventHandler<MapMouseEventArgs> MapMouseEnter;

        public event EventHandler<MapKeyEventArgs> MapKeyDown;

        public event EventHandler<MapKeyEventArgs> MapKeyUp;

        protected BaseInteractiveOverlay(Context context)
            : base(context)
        { }

        /*public static int InteractiveClickInterval
        {
            get { return MouseEventManager.SystemClickInterval; }
            set
            {
                Validators.CheckValueIsGreaterThanZero(value, "InteractiveClickInterval");
                MouseEventManager.SystemClickInterval = value;
            }
        }

        public InteractiveResult MouseDown(InteractionArguments interactionArguments)
        {
            Validators.CheckParameterIsNotNull(interactionArguments, "interactionArguments");
            if (!IsOverlayInitialized) return new InteractiveResult();

            OnMapMouseDown(new MapMouseEventArgs(interactionArguments));
            return MouseDownCore(interactionArguments);
        }

        protected virtual InteractiveResult MouseDownCore(InteractionArguments interactionArguments)
        {
     
            return new InteractiveResult();
        }

        public InteractiveResult MouseMove(InteractionArguments interactionArguments)
        {
            Validators.CheckParameterIsNotNull(interactionArguments, "interactionArguments");
            if (!IsOverlayInitialized) return new InteractiveResult();

            OnMapMouseMove(new MapMouseEventArgs(interactionArguments));
            return MouseMoveCore(interactionArguments);
        }

        protected virtual InteractiveResult MouseMoveCore(InteractionArguments interactionArguments)
        {
     
            return new InteractiveResult();
        }

        public InteractiveResult MouseUp(InteractionArguments interactionArguments)
        {
            Validators.CheckParameterIsNotNull(interactionArguments, "interactionArguments");
            if (!IsOverlayInitialized) return new InteractiveResult();

            OnMapMouseUp(new MapMouseEventArgs(interactionArguments));
            return MouseUpCore(interactionArguments);
        }

        protected virtual InteractiveResult MouseUpCore(InteractionArguments interactionArguments)
        {
     
            return new InteractiveResult();
        }

        public InteractiveResult MouseClick(InteractionArguments interactionArguments)
        {
            Validators.CheckParameterIsNotNull(interactionArguments, "interactionArguments");
            if (!IsOverlayInitialized) return new InteractiveResult();

            OnMapMouseClick(new MapMouseEventArgs(interactionArguments));
            return MouseClickCore(interactionArguments);
        }

        protected virtual InteractiveResult MouseClickCore(InteractionArguments interactionArguments)
        {
     
            return new InteractiveResult();
        }

        public InteractiveResult MouseDoubleClick(InteractionArguments interactionArguments)
        {
            Validators.CheckParameterIsNotNull(interactionArguments, "interactionArguments");
            if (!IsOverlayInitialized) return new InteractiveResult();

            OnMapMouseDoubleClick(new MapMouseEventArgs(interactionArguments));
            return MouseDoubleClickCore(interactionArguments);
        }

        protected virtual InteractiveResult MouseDoubleClickCore(InteractionArguments interactionArguments)
        {
     
            return new InteractiveResult();
        }

        public InteractiveResult MouseWheel(InteractionArguments interactionArguments)
        {
            Validators.CheckParameterIsNotNull(interactionArguments, "interactionArguments");
            if (!IsOverlayInitialized) return new InteractiveResult();

            OnMapMouseWheel(new MapMouseEventArgs(interactionArguments));
            return MouseWheelCore(interactionArguments);
        }

        protected virtual InteractiveResult MouseWheelCore(InteractionArguments interactionArguments)
        {
            return new InteractiveResult();
        }

        public InteractiveResult MouseLeave(InteractionArguments interactionArguments)
        {
            Validators.CheckParameterIsNotNull(interactionArguments, "interactionArguments");
            if (!IsOverlayInitialized) return new InteractiveResult();

            OnMapMouseLeave(new MapMouseEventArgs(interactionArguments));
            return MouseLeaveCore(interactionArguments);
        }

        protected virtual InteractiveResult MouseLeaveCore(InteractionArguments interactionArguments)
        {
            return new InteractiveResult();
        }

        public InteractiveResult MouseEnter(InteractionArguments interactionArguments)
        {
            Validators.CheckParameterIsNotNull(interactionArguments, "interactionArguments");
            if (!IsOverlayInitialized) return new InteractiveResult();

            OnMapMouseEnter(new MapMouseEventArgs(interactionArguments));
            return MouseEnterCore(interactionArguments);
        }

        protected virtual InteractiveResult MouseEnterCore(InteractionArguments interactionArguments)
        {
            return new InteractiveResult();
        }


        public InteractiveResult KeyUp(KeyEventInteractionArguments interactionArguments)
        {
            Validators.CheckParameterIsNotNull(interactionArguments, "interactionArguments");
            if (!IsOverlayInitialized) return new InteractiveResult();

            OnMapKeyUp(new MapKeyEventArgs(interactionArguments));
            return new InteractiveResult();
        }
        
        public InteractiveResult KeyDown(KeyEventInteractionArguments interactionArguments)
        {
            Validators.CheckParameterIsNotNull(interactionArguments, "interactionArguments");
            if (!IsOverlayInitialized) return new InteractiveResult();

            OnMapKeyDown(new MapKeyEventArgs(interactionArguments));
            return new InteractiveResult();
        }*/


        protected void OnMapMouseDown(MapMouseEventArgs e)
        {
            EventHandler<MapMouseEventArgs> handler = MapMouseDown;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected void OnMapMouseMove(MapMouseEventArgs e)
        {
            EventHandler<MapMouseEventArgs> handler = MapMouseMove;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected void OnMapMouseUp(MapMouseEventArgs e)
        {
            EventHandler<MapMouseEventArgs> handler = MapMouseUp;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected void OnMapMouseClick(MapMouseEventArgs e)
        {
            EventHandler<MapMouseEventArgs> handler = MapMouseClick;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected void OnMapMouseDoubleClick(MapMouseEventArgs e)
        {
            EventHandler<MapMouseEventArgs> handler = MapMouseDoubleClick;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected void OnMapMouseWheel(MapMouseEventArgs e)
        {
            EventHandler<MapMouseEventArgs> handler = MapMouseWheel;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected void OnMapMouseLeave(MapMouseEventArgs e)
        {
            EventHandler<MapMouseEventArgs> handler = MapMouseLeave;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected void OnMapMouseEnter(MapMouseEventArgs e)
        {
            EventHandler<MapMouseEventArgs> handler = MapMouseEnter;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected void OnMapKeyUp(MapKeyEventArgs e)
        {
            EventHandler<MapKeyEventArgs> handler = MapKeyUp;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected void OnMapKeyDown(MapKeyEventArgs e)
        {
            EventHandler<MapKeyEventArgs> handler = MapKeyDown;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected override void DrawCore(RectangleShape targetExtent, OverlayRefreshType overlayRefreshType) { }
    }
}

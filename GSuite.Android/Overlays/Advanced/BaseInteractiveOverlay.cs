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
        /*public event EventHandler<MapMouseEventArgs> MapMouseDown;

        public event EventHandler<MapMouseEventArgs> MapMouseMove;

        public event EventHandler<MapMouseEventArgs> MapMouseUp;

        public event EventHandler<MapMouseEventArgs> MapMouseClick;

        public event EventHandler<MapMouseEventArgs> MapMouseDoubleClick;

        public event EventHandler<MapMouseEventArgs> MapMouseWheel;

        public event EventHandler<MapMouseEventArgs> MapMouseLeave;

        public event EventHandler<MapMouseEventArgs> MapMouseEnter;*/

        public event EventHandler<MapKeyEventArgs> MapKeyDown;

        public event EventHandler<MapKeyEventArgs> MapKeyUp;

        protected BaseInteractiveOverlay(Context context)
            : base(context)
        { }

        public InteractiveResult MotionDown(MapMotionEventArgs motionArgs)
        {
            Validators.CheckParameterIsNotNull(motionArgs, "interactionArguments");
            if (!IsOverlayInitialized) return new InteractiveResult();

            //OnMapMouseDown(new MapMouseEventArgs(interactionArguments));
            return MotionDownCore(motionArgs);
        }

        protected virtual InteractiveResult MotionDownCore(MapMotionEventArgs motionArgs)
        {
            return new InteractiveResult();
        }

        public InteractiveResult MotionMove(MapMotionEventArgs motionArgs)
        {
            Validators.CheckParameterIsNotNull(motionArgs, "motionArgs");
            if (!IsOverlayInitialized) return new InteractiveResult();

            //OnMapMotionMove(new MapMotionEventArgs(motionArgs))

            return MotionMoveCore(motionArgs);
        }

        protected virtual InteractiveResult MotionMoveCore(MapMotionEventArgs motionArgs)
        {
            return new InteractiveResult();
        }

        public InteractiveResult MotionUp(MapMotionEventArgs interactionArguments)
        {
            Validators.CheckParameterIsNotNull(interactionArguments, "interactionArguments");
            if (!IsOverlayInitialized) return new InteractiveResult();

            //OnMapMouseUp(new MapMouseEventArgs(interactionArguments));
            return MotionUpCore(interactionArguments);
        }

        protected virtual InteractiveResult MotionUpCore(MapMotionEventArgs interactionArguments)
        {
            return new InteractiveResult();
        }

        public InteractiveResult DoubleTap(MapMotionEventArgs motionArgs)
        {
            Validators.CheckParameterIsNotNull(motionArgs, "motionArgs");
            if (!IsOverlayInitialized) return new InteractiveResult();

            //OnMapMotionMove(new MapMotionEventArgs(motionArgs))

            return DoubleTapCore(motionArgs);
        }

        protected virtual InteractiveResult DoubleTapCore(MapMotionEventArgs motionArgs)
        {
            return new InteractiveResult();
        }

        public InteractiveResult Pinch(MapMotionEventArgs motionArgs)
        {
            Validators.CheckParameterIsNotNull(motionArgs, "motionArgs");
            if (!IsOverlayInitialized) return new InteractiveResult();

            //OnMapMotionMove(new MapMotionEventArgs(motionArgs))

            return PinchCore(motionArgs);
        }

        protected virtual InteractiveResult PinchCore(MapMotionEventArgs motionArgs)
        {
            return new InteractiveResult();
        }

        public InteractiveResult PinchEnd(MapMotionEventArgs motionArgs)
        {
            Validators.CheckParameterIsNotNull(motionArgs, "motionArgs");
            if (!IsOverlayInitialized) return new InteractiveResult();

            //OnMapMotionMove(new MapMotionEventArgs(motionArgs))

            return PinchEndCore(motionArgs);
        }

        protected virtual InteractiveResult PinchEndCore(MapMotionEventArgs motionArgs)
        {
            return new InteractiveResult();
        }

        public InteractiveResult MotionPointerDown(MapMotionEventArgs motionArgs)
        {
            Validators.CheckParameterIsNotNull(motionArgs, "motionArgs");
            if (!IsOverlayInitialized) return new InteractiveResult();

            return MotionPointerDownCore(motionArgs);
        }

        protected virtual InteractiveResult MotionPointerDownCore(MapMotionEventArgs motionArgs)
        {
            return new InteractiveResult();
        }

        /*protected void OnMapMouseDown(MapMouseEventArgs e)
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
        }*/

        protected override void DrawCore(RectangleShape targetExtent, OverlayRefreshType overlayRefreshType) { }
    }
}

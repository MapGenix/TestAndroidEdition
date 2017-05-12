using System;
using System.Windows;
using Mapgenix.Shapes;
using Mapgenix.Layers;
using Android.Widget;
using Android.Content;
using Android.Views;

namespace Mapgenix.GSuite.Android
{
    /// <summary>
    /// Base class of all overlays.
    /// </summary>
    [Serializable]
    public abstract class BaseOverlay : RelativeLayout, IDisposable
    {
        [NonSerialized]
        private MapLayout _overlayCanvas;

        public event EventHandler<ExtentEventArgs> Drawing;

        public event EventHandler<ExtentEventArgs> Drawn;

        protected BaseOverlay(Context context)
            :base(context)
        {
            OverlayCanvas = new MapLayout(context);
        }

        public string Name { get; set; }

        public MapArguments MapArguments { get; set; }

        public MapLayout OverlayCanvas { get { return _overlayCanvas; } set { _overlayCanvas = value; } }

        protected RectangleShape PreviousExtent { get; set; }

        public DrawingExceptionMode DrawingExceptionMode { get; set; }

        public virtual bool IsVisible
        {
            get { return OverlayCanvas.Visibility == ViewStates.Visible; }
            set
            {
                OverlayCanvas.Visibility = value ? ViewStates.Visible : ViewStates.Gone;
                
            }
        }

        protected bool IsOverlayInitialized { get; set; }

      
        public void Initialize(MapArguments mapArguments)
        {
            InitializeCore(mapArguments);
        }

        protected virtual void InitializeCore(MapArguments mapArguments)
        {
            MapArguments = mapArguments;
            IsOverlayInitialized = true;
        }


        public void Refresh()
        {
            RefreshCore();
        }

        protected virtual void RefreshCore()
        {
            Draw(MapArguments.CurrentExtent, OverlayRefreshType.Redraw);
        }


        public void Draw(RectangleShape targetExtent)
        {
            Draw(targetExtent, OverlayRefreshType.Redraw);
        }

        public void Draw(RectangleShape targetExtent, OverlayRefreshType refreshType)
        {
            if (!IsVisible) { return; }

            
            ExtentEventArgs args = new ExtentEventArgs(targetExtent);
            OnDrawing(args);
            if (args.Cancel) { return; }

            try
            {
                DrawCore(targetExtent, refreshType);
            }
            catch(Exception ex)
            {
                Toast.MakeText(Context, this.GetType().ToString() + " ERROR:" + ex.Message + " SOURCE: " + ex.Source, ToastLength.Short).Show();
            }            
            PreviousExtent = targetExtent;
        }

        protected abstract void DrawCore(RectangleShape targetExtent, OverlayRefreshType overlayRefreshType);

        public void PanTo(RectangleShape targetExtent)
        {
            PanToCore(targetExtent);
            PreviousExtent = targetExtent;
        }

        protected virtual void PanToCore(RectangleShape targetExtent) { }

        public virtual bool IsEmpty { get { return false; } }

        protected void OnDrawing(ExtentEventArgs e)
        {
            EventHandler<ExtentEventArgs> handler = Drawing;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected void OnDrawn(ExtentEventArgs e)
        {
            EventHandler<ExtentEventArgs> handler = Drawn;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public RectangleShape GetBoundingBox()
        {
            return GetBoundingBoxCore();
        }

        protected virtual RectangleShape GetBoundingBoxCore()
        {
            if (MapArguments != null)
            {
                return MapUtil.GetDefaultMaxExtent(MapArguments.MapUnit);
            }
            return null;
        }
 
        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing) { }
    }
}

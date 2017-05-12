using System;
using System.ComponentModel;
using System.Diagnostics;
//using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows;
using Mapgenix.Canvas;
using Mapgenix.Shapes;
using Mapgenix.Layers;
using Android.Widget;
using Android.Content;
using System.Threading;
using NativeAndroid = Android;
using Android.Graphics;
using System.Threading.Tasks;
using Android.OS;
using System.Collections.ObjectModel;

namespace Mapgenix.GSuite.Android
{
    /// <summary>
    /// The base class of tile for forming tile overlay.
    /// </summary>
    [Serializable]
    [ToolboxItem(false)]
    [DesignTimeVisible(false)]
    public partial class Tile : LinearLayout, IDisposable
    {
        //public static readonly DependencyProperty ImageSourceProperty = DependencyProperty.Register("ImageSource", typeof(ImageSource), typeof(Tile));
        [NonSerialized]
        private Bitmap _imageSource;

        [NonSerialized]
        private ImageView _view;
        private float _saveScale = 0f;
        private float _minScale = 590591790f;
        private float _maxScale = 1126.4644432067871f;
        private Matrix _matrix;
        public float[] m;


        public event EventHandler Opened;
        public event EventHandler<GeoCanvasEventArgs> Drawing;
        public event EventHandler<GeoCanvasEventArgs> Drawn;

        private bool _isOpened;
        private bool _disposed;

        [NonSerialized]
        private Bitmap watermarkCanvas;

        [NonSerialized]
        private TileAsyncTask _backgroundTask;
     
        public Tile(Context context)
            : base(context)
        {
            TargetExtent = null;
            IsAsync = true;
            HasWatermark = true;
            IsPartial = false;
            Focusable = false;
            _view = new ImageView(context);
            
            _matrix = new Matrix();
            _view.ImageMatrix = _matrix;
            m = new float[9];
            _view.SetScaleType(ImageView.ScaleType.Matrix);

            AddView(_view);            
        }

        #region properties

        internal ImageView View
        {
            get { return _view; }
        } 

        public Bitmap ImageSource
        {
            get { return _imageSource; }
            set { _imageSource = value; }
        }

        public RectangleShape TargetExtent { get; set; }

        public long RowIndex { get; set; }
        
        public long ColumnIndex { get; set; }

        public int ZoomLevelIndex { get; set; }

        public BaseBitmapTileCache TileCache { get; set; }

        public bool IsOpened
        {
            get { return _isOpened; }
            set
            {
                _isOpened = value;
                if (_isOpened)
                {
                    OnOpened(new EventArgs());
                }
            }
        }        

        protected virtual bool CancellationPending
        {
            get
            {
                //if (_backgroundWorker != null && _backgroundWorker.CancellationPending) { return true; }
                //else return false;
                return false;
            }
        }

        internal bool IsPartial { get; set; }

        public bool IsAsync { get; set; }

        public TimeSpan DrawingTime { get; set; }

        internal bool HasWatermark { get; set; }

        public DrawingExceptionMode DrawingExceptionMode { get; set; }

        internal float MinScale
        {
            get { return _minScale; }
            set { _minScale = value; }
        }

        internal float MaxScale
        {
            get { return _maxScale; }
            set { _maxScale = value; }
        }

        internal float Scale
        {
            get { return _saveScale; }
            set { _saveScale = value; }
        }

        #endregion

        #region public methods

        public void CommitDrawing(BaseGeoCanvas geoCanvas, object nativeImage)
        {
            CommitDrawingCore(geoCanvas, nativeImage);
        }

        public Bitmap ToImageSource(object nativeImage)
        {
            return ToImageSourceCore(nativeImage);
        }

        public void Draw(GdiPlusAndroidGeoCanvas geoCanvas)
        {
            try
            {
                OnDrawing(new GeoCanvasEventArgs(geoCanvas));
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                if (TileCache == null || geoCanvas is DrawingVisualGeoCanvas)
                {
                    DrawCore(geoCanvas);
                    geoCanvas.Flush();
                }
                else if (IsPartial)
                {
                    //DrawPartial(geoCanvas);
                }
                else
                {
                    DrawFull(geoCanvas);
                }
                stopwatch.Stop();
                DrawingTime = stopwatch.Elapsed;
                OnDrawn(new GeoCanvasEventArgs(geoCanvas, false));
            }
            catch (Exception ex)
            {
                DrawException(geoCanvas, ex);
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public void DrawAsync(GdiPlusAndroidGeoCanvas geoCanvas)
        {
            /*await Task.Factory.StartNew
            (
                () => TaskStart(geoCanvas)
            )
            .ContinueWith(task =>
            {
                using (var h = new Handler(Context.MainLooper))
                {
                    h.Post(() =>
                    {
                        TaskComplete(this, task);
                    });
                }
            });*/

            if(_backgroundTask == null )
                _backgroundTask = new TileAsyncTask();

            if (_backgroundTask.GetStatus() == AsyncTask.Status.Running || _backgroundTask.GetStatus() == AsyncTask.Status.Finished)
                return;

            Func<GdiPlusAndroidGeoCanvas, object> task = TaskStart;
            Action<object> complete = (Action<object>)((object taskResult) =>
            {
                using (var h = new Handler(Context.MainLooper))
                {
                    h.Post(() =>
                    {
                        TaskComplete(taskResult);
                    });
                }
            });
            _backgroundTask.Execute(task, geoCanvas, complete);
        }

        #endregion

        #region private methods

        private object TaskStart(GdiPlusAndroidGeoCanvas geoCanvas)
        {
            object result;
            try
            {
                object imageSource = geoCanvas.NativeImage;
                Draw(geoCanvas);
                geoCanvas.EndDrawing();

                Bitmap bitmap = imageSource as Bitmap;
                if (bitmap != null)
                {
                    MemoryStream memoryStream = new MemoryStream();
                    bitmap.Compress(Bitmap.CompressFormat.Png, 1, memoryStream);
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    imageSource = memoryStream;
                    bitmap.Dispose();
                }
                result = new TileAsyncResult() { GeoCanvas = geoCanvas, ImageSource = imageSource };
            }
            catch (Exception ex)
            {
                result = null;
            }

            return result;
        }

        private void TaskComplete(object sender, Task<object> e)
        {
            if (e.IsCanceled)
            {
                return;
            }
            else if (e.Exception == null && e.Result != null)
            {
                TileAsyncResult result = (TileAsyncResult)e.Result;
                CommitDrawing(result.GeoCanvas, result.ImageSource);
            }
        }

        private void TaskComplete(object taskResult)
        {
            if (_backgroundTask == null)
                return;

            if (_backgroundTask.IsCancelled)
            {
                return;
            }
            else if (taskResult != null)
            {
                TileAsyncResult result = (TileAsyncResult)taskResult;
                CommitDrawing(result.GeoCanvas, result.ImageSource);
            }
        }

        private void DrawWatermark()
        {
            if (watermarkCanvas != null)
            {
                //InstallerHelper.CheckInstaller(watermarkCanvas, Width, Height);
            }
        }
                
        #endregion

        #region protected method

        protected virtual void CommitDrawingCore(BaseGeoCanvas geoCanvas, object imageSource)
        {
            ImageSource = ToImageSourceCore(imageSource);
            _view.SetImageBitmap(ImageSource);
            IsOpened = true;            
        }

        protected virtual Bitmap ToImageSourceCore(object imageSource)
        {
            Stream streamSource = imageSource as Stream;
            NativeAndroid.Graphics.Bitmap renderTargetBitmap = imageSource as NativeAndroid.Graphics.Bitmap;
            if (streamSource != null)
            {
                NativeAndroid.Graphics.Bitmap bitmapSource = NativeAndroid.Graphics.BitmapFactory.DecodeStream(streamSource);
                streamSource.Dispose();
                return bitmapSource;
            }
            else if (renderTargetBitmap != null)
            {
                return renderTargetBitmap;
            }
            else
            {
                return null;
            }
        }

        protected virtual void DrawCore(BaseGeoCanvas geoCanvas)
        { }

        protected void DrawException(BaseGeoCanvas geoCanvas, Exception exception)
        {
            switch (DrawingExceptionMode)
            {
                case DrawingExceptionMode.DrawException:
                    DrawExceptionCore(geoCanvas, exception);
                    break;
                default: throw exception;
            }
        }

        protected virtual void DrawExceptionCore(BaseGeoCanvas canvas, Exception exception)
        {
            using (Bitmap exceptionImage = Bitmap.CreateBitmap(256,256, Bitmap.Config.Argb8888))
            {
                canvas.Clear(new GeoSolidBrush(GeoColor.FromArgb(128, 255, 192, 203)));
                MemoryStream streamSource = new MemoryStream();
                using (GeoImage geoImage = new GeoImage(streamSource))
                {
                    canvas.DrawScreenImageWithoutScaling(geoImage, canvas.Width * .5f, canvas.Height * .5f, DrawingLevel.LevelOne, 0f, 0f, 0f);
                }
            }
        }

        protected virtual void OnOpened(EventArgs args)
        {
            EventHandler handler = Opened;
            if (handler != null)
            {
                handler(this, args);
            }
        }

        protected virtual void OnDrawing(GeoCanvasEventArgs args)
        {
            EventHandler<GeoCanvasEventArgs> handler = Drawing;
            if (handler != null)
            {
                handler(this, args);
            }
        }

        protected virtual void OnDrawn(GeoCanvasEventArgs args)
        {
            EventHandler<GeoCanvasEventArgs> handler = Drawn;
            if (handler != null)
            {
                handler(this, args);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                ImageSource = null;                
                if(_backgroundTask != null)
                {
                    _backgroundTask.Cancel(true);
                    _backgroundTask = null;
                }
                _disposed = true;
                _view.Dispose();
            }
        }

        protected virtual Tile Clone()
        {
            Tile tile = new Tile(Context);
            tile.View.SetImageBitmap(this.ImageSource);
            tile.LayoutParameters = new LinearLayout.LayoutParams(tile.LayoutParameters);
            tile.TargetExtent = new RectangleShape(this.TargetExtent.UpperLeftPoint, this.TargetExtent.LowerRightPoint);
            tile.RowIndex = this.RowIndex;
            tile.ColumnIndex = this.ColumnIndex;
            tile.ZoomLevelIndex = this.ZoomLevelIndex;
            return tile;
        }

        #endregion
    }

    public class TileAsyncResult
    {
        public TileAsyncResult()
            : this(null, null)
        { }

        public TileAsyncResult(BaseGeoCanvas geoCanvas, object imageStream)
        {
            GeoCanvas = geoCanvas;
            ImageSource = imageStream;
        }

        public BaseGeoCanvas GeoCanvas { get; set; }

        public object ImageSource { get; set; }
    }

    public class TileAsyncTask : AsyncTask<object, object, object>
    {

        Action<object> _actionComplete;

        protected override void OnPreExecute()
        {
            base.OnPreExecute();
        }

        protected override void OnCancelled()
        {
            base.OnCancelled();
        }

        protected override void OnPostExecute(object result)
        {
            base.OnPostExecute(result);
            _actionComplete(result);
        }

        protected override object RunInBackground(params object[] @params)
        {
            Func<GdiPlusAndroidGeoCanvas, object> action = (Func<GdiPlusAndroidGeoCanvas, object>)@params[0];
            _actionComplete = (Action<object>)@params[2];
            return action((GdiPlusAndroidGeoCanvas)@params[1]);
        }
    }

}

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

        public event EventHandler Opened;

        public event EventHandler<GeoCanvasEventArgs> Drawing;

        public event EventHandler<GeoCanvasEventArgs> Drawn;

        private bool _isOpened;
        private bool _disposed;

        [NonSerialized]
        private ImageView watermarkCanvas;
        

        [NonSerialized]
        private BackgroundWorker _backgroundWorker;
     
        public Tile(Context context)
            : base(context)
        {
            TargetExtent = null;
            IsAsync = true;
            HasWatermark = true;
            IsPartial = false;
            Focusable = false;
            InitializeBackgroundWorkder();
            _view = new ImageView(context);
            AddView(_view);
        }

        private void InitializeBackgroundWorkder()
        {
            _backgroundWorker = new BackgroundWorker();
            _backgroundWorker.WorkerSupportsCancellation = true;
            //_backgroundWorker.DoWork += backgroundWorker_DoWork;
            //_backgroundWorker.RunWorkerCompleted += backgroundWorker_RunWorkerCompleted;
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
                if (_backgroundWorker != null && _backgroundWorker.CancellationPending) { return true; }
                else return false;
            }
        }

        internal bool IsPartial { get; set; }

        public void CommitDrawing(BaseGeoCanvas geoCanvas, object nativeImage)
        {
            CommitDrawingCore(geoCanvas, nativeImage);
        }

        protected virtual void CommitDrawingCore(BaseGeoCanvas geoCanvas, object imageSource)
        {
            _view.SetImageBitmap(ToImageSourceCore(imageSource));
            IsOpened = true;
        }

        public Bitmap ToImageSource(object nativeImage)
        {
            return ToImageSourceCore(nativeImage);
        }

        protected virtual Bitmap ToImageSourceCore(object imageSource)
        {            
            Stream streamSource = imageSource as Stream;
            NativeAndroid.Graphics.Bitmap renderTargetBitmap = imageSource as NativeAndroid.Graphics.Bitmap;
            if (streamSource != null)
            {
                /*BitmapImage bitmapSource = new BitmapImage();
                bitmapSource.BeginInit();
                bitmapSource.StreamSource = streamSource;
                bitmapSource.EndInit();
                MapUtil.FreezeElement(bitmapSource);
                return bitmapSource;*/

                NativeAndroid.Graphics.Bitmap bitmapSource = NativeAndroid.Graphics.BitmapFactory.DecodeStream(streamSource);
                return bitmapSource;
            }
            else if (renderTargetBitmap != null)
            {
                //MapUtil.FreezeElement(renderTargetBitmap);
                return renderTargetBitmap;
            }
            else
            {
                return null;
            }
        }

        public bool IsAsync { get; set; }

        public TimeSpan DrawingTime { get; set; }

        internal bool HasWatermark { get; set; }

        public DrawingExceptionMode DrawingExceptionMode { get; set; }

        /*public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            watermarkCanvas = (System.Windows.Controls.Canvas)GetTemplateChild("WatermarkCanvas");
           
            if (HasWatermark)
            {
                DrawWatermark();
            }
        }*/

        public void DrawAsync(GdiPlusAndroidGeoCanvas geoCanvas)
        {

            Task.Factory.StartNew
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
            });
        }

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
                    bitmap.Compress(Bitmap.CompressFormat.Png, 0, memoryStream);
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    imageSource = memoryStream;
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

            if (_disposed && _backgroundWorker != null)
            {
                _backgroundWorker.Dispose();
                _backgroundWorker = null;
            }
        }

        public void DrawAsync(DrawingVisualGeoCanvas geoCanvas)
        {

            /*new Thread(() => (geoCanvas)
                {
                    
                }
            );*/
            _backgroundWorker.CancelAsync();
            while (_backgroundWorker != null && _backgroundWorker.IsBusy)
            {
                //System.Windows.Forms.Application.DoEvents();
            }

            if (_backgroundWorker == null) InitializeBackgroundWorkder();
            _backgroundWorker.RunWorkerAsync(geoCanvas);
        }

        public void Draw(DrawingVisualGeoCanvas geoCanvas)
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



        public void Draw(GdiPlusGeoCanvas geoCanvas)
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

        /*private void DrawPartial(DrawingVisualGeoCanvas geoCanvas)
        {
            BitmapTile imageTile = null;
            lock (TileCache)
            {
                double targetScale = MapUtil.GetScale(geoCanvas.MapUnit, geoCanvas.CurrentWorldExtent, geoCanvas.Width, geoCanvas.Height);
                TileCache.TileMatrix.Scale = targetScale;
                TileCache.TileMatrix.Id = targetScale.ToString(CultureInfo.InvariantCulture);
                try
                {
                    imageTile = TileCache.GetTile(geoCanvas.CurrentWorldExtent);
                }
                catch { }
            }

            if (imageTile != null && imageTile.Bitmap != null)
            {
                //Bitmap cachedBitmap = imageTile.Bitmap;
                MemoryStream cachedStream = new MemoryStream();
                try
                {
                    //cachedBitmap.Compress(Bitmap.CompressFormat.Png, 100, cachedStream);
                    using (GeoImage cachedImage = new GeoImage(cachedStream))
                    {
                        geoCanvas.DrawScreenImage(cachedImage,
                            geoCanvas.Width * .5f,
                            geoCanvas.Height * .5f,
                            geoCanvas.Width,
                            geoCanvas.Height,
                            DrawingLevel.LevelOne,
                            0f, 0f, 0f);
                    }
                }
                finally
                {
                    //if (cachedBitmap != null) { cachedBitmap.Dispose(); }
                }
            }
            else
            {
                DrawCore(geoCanvas);
                Bitmap nativeImage = (Bitmap)geoCanvas.NativeImage;
                RectangleShape currentWorldExtent = geoCanvas.CurrentWorldExtent;
                GeographyUnit mapUnit = geoCanvas.MapUnit;

                geoCanvas.EndDrawing();
                geoCanvas.BeginDrawing(nativeImage, currentWorldExtent, mapUnit);

                if (!CancellationPending)
                {
                    if (imageTile == null)
                    {
                        imageTile = new BitmapTile(geoCanvas.CurrentWorldExtent, TileCache.TileMatrix.Scale);
                    }

                    imageTile.Bitmap = Bitmap.CreateScaledBitmap(nativeImage, nativeImage.Width, nativeImage.Height, true);
                    lock (TileCache)
                    {
                        try
                        {
                            TileCache.SaveTile(imageTile);
                        }
                        catch { }
                    }
                }
            }
        }


        private void DrawPartial(GdiPlusGeoCanvas geoCanvas)
        {
            BitmapTile imageTile = null;
            lock (TileCache)
            {
                double targetScale = MapUtil.GetScale(geoCanvas.MapUnit, geoCanvas.CurrentWorldExtent, geoCanvas.Width, geoCanvas.Height);
                TileCache.TileMatrix.Scale = targetScale;
                TileCache.TileMatrix.Id = targetScale.ToString(CultureInfo.InvariantCulture);
                try
                {
                    imageTile = TileCache.GetTile(geoCanvas.CurrentWorldExtent);
                }
                catch { }
            }

            if (imageTile != null && imageTile.Bitmap != null)
            {
                Bitmap cachedBitmap = imageTile.Bitmap;
                MemoryStream cachedStream = new MemoryStream();
                try
                {
                    cachedBitmap.Save(cachedStream, System.Drawing.Imaging.ImageFormat.Png);
                    using (GeoImage cachedImage = new GeoImage(cachedStream))
                    {
                        geoCanvas.DrawScreenImage(cachedImage,
                            geoCanvas.Width * .5f,
                            geoCanvas.Height * .5f,
                            geoCanvas.Width,
                            geoCanvas.Height,
                            DrawingLevel.LevelOne,
                            0f, 0f, 0f);
                    }
                }
                finally
                {
                    if (cachedBitmap != null) { cachedBitmap.Dispose(); }
                }
            }
            else
            {
                DrawCore(geoCanvas);
                Bitmap nativeImage = (Bitmap)geoCanvas.NativeImage;
                RectangleShape currentWorldExtent = geoCanvas.CurrentWorldExtent;
                GeographyUnit mapUnit = geoCanvas.MapUnit;
               
                geoCanvas.EndDrawing();
                geoCanvas.BeginDrawing(nativeImage, currentWorldExtent, mapUnit);

                if (!CancellationPending)
                {
                    if (imageTile == null)
                    {
                        imageTile = new BitmapTile(geoCanvas.CurrentWorldExtent, TileCache.TileMatrix.Scale);
                    }
                    imageTile.Bitmap = (Bitmap)nativeImage.Clone();
                    lock (TileCache)
                    {
                        try
                        {
                            TileCache.SaveTile(imageTile);
                        }
                        catch { }
                    }
                }
            }
        }*/

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
                    //exceptionImage.Save(streamSource, System.Drawing.Imaging.ImageFormat.Png);
                    //streamSource.Seek(0, SeekOrigin.Begin);
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

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && _backgroundWorker != null)
            {
                /*if (ImageSource != null && ImageSource is BitmapImage && ((BitmapImage)ImageSource).StreamSource != null)
                {
                    Stream streamSource = ((BitmapImage)ImageSource).StreamSource;
                    streamSource.Close();
                    streamSource.Dispose();
                    streamSource = null;
                }*/

                ImageSource = null;
                if (_backgroundWorker != null && _backgroundWorker.IsBusy)
                {
                    _backgroundWorker.CancelAsync();
                    _backgroundWorker.Dispose();
                }
                _disposed = true;
            }
        }

        private void DrawWatermark()
        {
            if (watermarkCanvas != null)
            {
                //InstallerHelper.CheckInstaller(watermarkCanvas, Width, Height);
            }
        }

        /*private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            if (_backgroundWorker == null)
            {
                return;
            }
            else if (_backgroundWorker.CancellationPending)
            {
                e.Cancel = true;
            }
            else
            {
                GdiPlusAndroidGeoCanvas geoCanvas = (GdiPlusAndroidGeoCanvas)e.Argument;
                object imageSource = geoCanvas.NativeImage;
                Draw(geoCanvas);
                geoCanvas.EndDrawing();

                Bitmap bitmap = imageSource as Bitmap;
                if (bitmap != null)
                {
                    MemoryStream memoryStream = new MemoryStream();
                    bitmap.Compress(Bitmap.CompressFormat.Png, 0, memoryStream);
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    imageSource = memoryStream;
                }

                e.Result = new TileAsyncResult() { GeoCanvas = geoCanvas, ImageSource = imageSource };
                e.Cancel = (_backgroundWorker == null || _backgroundWorker.CancellationPending);
            }
        }

        private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                return;
            }
            else if (e.Error == null && e.Result != null)
            {
                TileAsyncResult result = (TileAsyncResult)e.Result;
                CommitDrawing(result.GeoCanvas, result.ImageSource);
            }

            if (_disposed && _backgroundWorker != null)
            {
                _backgroundWorker.Dispose();
                _backgroundWorker = null;
            }
        }*/
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
}

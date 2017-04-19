using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Mapgenix.Canvas;
using Mapgenix.Layers.Properties;
using Mapgenix.Shapes;
using Mapgenix.Utils;

namespace Mapgenix.Layers
{
    /// <summary>Base class for vector and raster layers</summary>
    /// <remarks>For your own layers, we strongly recommend you inherit from a higher level class such as 
    /// FeatureLayer or RasterLayer.</remarks>
    [Serializable]
    public abstract class BaseLayer
    {
        TimeSpan _drawingTime;
      
        bool _isOpen;
        DrawingExceptionMode _drawingExceptionMode;

        public event EventHandler<ProgressEventArgs> DrawingProgressChanged;

        public bool IsOpen
        {
            get { return IsOpenCore; }
        }

		public bool HasBoundingBox { get; set; }
        
        protected virtual bool IsOpenCore
        {
            get { return _isOpen; }
            set { _isOpen = value; }
        }

        public TimeSpan DrawingTime
        {
            get { return _drawingTime; }
            protected set { _drawingTime = value; }
        }

        public string Name { get; set; }
        
        public virtual bool IsVisible { get; set; }
        
        public DrawingExceptionMode DrawingExceptionMode { get; set; }
        
        public void Open()
        {
            if (!IsOpen)
            {
                _isOpen = true;
                OpenCore();
            }
        }

        protected virtual void OpenCore()
        {
        }

        public void Close()
        {
            if (IsOpen)
            {
                _isOpen = false;
                CloseCore();
            }
        }

        protected virtual void CloseCore()
        {
        }

        public RectangleShape GetBoundingBox()
        {
            Validators.CheckLayerIsOpened(IsOpen);
            Validators.CheckLayerHasBoundingBox(HasBoundingBox);

            return GetBoundingBoxCore();
        }

        protected virtual RectangleShape GetBoundingBoxCore()
        {
            Validators.CheckLayerIsOpened(IsOpen);
            Validators.CheckLayerHasBoundingBox(HasBoundingBox);

            return new RectangleShape(-180, 90, 180, -90);
        }

        public void Draw(BaseGeoCanvas canvas, Collection<SimpleCandidate> labelsInAllLayers)
        {
            Validators.CheckParameterIsNotNull(canvas, "canvas");
            Validators.CheckParameterIsNotNull(labelsInAllLayers, "labeledInLayers");
            try
            {
                canvas.DrawingProgressChanged += canvas_ProgressDrawing;
                Stopwatch watch = Stopwatch.StartNew();

                if (IsVisible)
                {
                    switch (_drawingExceptionMode)
                    {
                        case DrawingExceptionMode.DrawException:
                            try
                            {
                                DrawCore(canvas, labelsInAllLayers);
                            }
                            catch (Exception ex)
                            {
                                DrawException(canvas, ex);
                            }
                            break;
                        case DrawingExceptionMode.Default:
                        case DrawingExceptionMode.ThrowException:
                        default:
                            DrawCore(canvas, labelsInAllLayers);
                            break;
                    }
                }

                watch.Stop();
                _drawingTime = watch.Elapsed;
            }
            finally
            {
                canvas.DrawingProgressChanged -= canvas_ProgressDrawing;
            }
        }

        protected void DrawException(BaseGeoCanvas canvas, Exception e)
        {
            DrawExceptionCore(canvas, e);
        }

        protected virtual void DrawExceptionCore(BaseGeoCanvas canvas, Exception e)
        {
            Validators.CheckParameterIsNotNull(canvas, "canvas");

            canvas.Clear(new GeoSolidBrush(GeoColor.FromArgb(128, 255, 192, 203)));
            Bitmap tempImage = Resources.DrawExceptionRedCross;
            MemoryStream stream = new MemoryStream();
            tempImage.Save(stream, ImageFormat.Png);
            stream.Seek(0, SeekOrigin.Begin);
            tempImage.Dispose();
            canvas.DrawScreenImageWithoutScaling(new GeoImage(stream), canvas.Width / 2, canvas.Height / 2, DrawingLevel.LabelLevel, 0, 0, 0);
        }

        protected abstract void DrawCore(BaseGeoCanvas canvas, Collection<SimpleCandidate> labelsInAllLayers);

        protected void OnDrawingProgressChanged(ProgressEventArgs e)
        {
            EventHandler<ProgressEventArgs> handler = DrawingProgressChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        void canvas_ProgressDrawing(object sender, ProgressEventArgs e)
        {
            OnDrawingProgressChanged(e);
        }
       
    }
}

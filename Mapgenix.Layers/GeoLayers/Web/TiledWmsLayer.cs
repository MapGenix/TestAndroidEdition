using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using Mapgenix.Canvas;
using Mapgenix.RasterSource;

namespace Mapgenix.Layers
{
    /// <summary>Layer for tiled WMS Server.</summary>
    [Serializable]
    public class TiledWmsLayer : BaseLayer
    {
  
        BaseBitmapTileCache _tileCache;

        public event EventHandler<WebRequestEventArgs> SendingRequest;
        public event EventHandler<WebResponseEventArgs> SentRequest;
        private WmsPictureFormat _outputFormat;


        public string ClientId
        {
            get
            {
                return ImageSource.ClientId;
            }
            set
            {
                ImageSource.ClientId = value;
            }
        }

        public GeoColor KeyColor
        {
            get;
            set;
        }



        public Collection<GeoColor> KeyColors
        {
            get;
            set;
        }


        public string PrivateKey
        {
            get
            {
                return ImageSource.PrivateKey;
            }
            set
            {
                ImageSource.PrivateKey = value;
            }
        }

        public WmsPictureFormat OutputFormat
        {
            get
            {
                return this._outputFormat;
            }
            set
            {
                this._outputFormat = value;
                if (ImageSource != null)
                    ImageSource.OutputFormat = ProcessOutputFormat();
            }
        }

        private string ProcessOutputFormat()
        {
            switch(this._outputFormat)
            {
                case WmsPictureFormat.Gif:
                    return "image/gif";
                case WmsPictureFormat.Jpeg:
                    return "image/jpeg";
                default:
                    return "image/png";
            }
        }


        public void ImageSource_SentRequest(object sender, WebResponseEventArgs e)
        {
            OnSentRequest(e);
        }
     

        protected virtual void OnSendingRequest(WebRequestEventArgs e)
        {
            EventHandler<WebRequestEventArgs> handler = SendingRequest;

            if (handler != null)
            {
                handler(this, e);
            }
        }
            

        protected virtual void OnSentRequest(WebResponseEventArgs e)
        {
            EventHandler<WebResponseEventArgs> handler = SentRequest;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        public Uri ServerUri
        {
            get
            {
                return ImageSource.ServerUris[0];
            }
            set
            {
                Validators.CheckUriIsValid(value);
                ImageSource.ServerUris[0] = value;
            }
        }


        public void ClearCache()
        {
            if (_tileCache != null)
            {
                _tileCache.ClearCache();
            }
        }

        public WebProxy WebProxy
        {
            get { return ImageSource.Proxy; }
            set { ImageSource.Proxy = value; }
        }


        protected override void OpenCore()
        {
            if (!Parameters.ContainsKey("ClientId"))
            {
                Parameters.Add("ClientId", ClientId);
            }
            ImageSource.Open();
        }

        public BaseBitmapTileCache TileCache
        {
            get { return _tileCache; }
            set { _tileCache = value; }
        }

        internal TiledWmsRasterSource ImageSource
        {
            get;
            set; 
        }

        public double UpperScale
        {
            get; set; }

       
        public double LowerScale
        {
            get; 
            set; 
        }


        private Dictionary<string, string> Parameters
        {
            get { return ImageSource.Parameters; }
        }

		protected override bool IsOpenCore
        {
            get { return ImageSource.IsOpen; }
        }

        protected override void CloseCore()
        {
            if (Parameters.ContainsKey("ClientId"))
            {
                Parameters.Remove("ClientId");
            }
            ImageSource.Close();
        }

        protected override void DrawCore(BaseGeoCanvas canvas, Collection<SimpleCandidate> labelsInAllLayers)
        {
			WebDrawHelper.DrawTiledWms(this, canvas);
        }

        protected override void DrawExceptionCore(BaseGeoCanvas canvas, Exception e)
        {
			WebDrawHelper.DrawException(canvas, e);
        }
    }
}
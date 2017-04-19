using Mapgenix.Canvas;
using Mapgenix.Shapes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Mapgenix.Layers
{
    internal class WmsRequestBuilder
    {
        #region private properties

        private Collection<string> _layers = new Collection<string>();
        private Dictionary<string, string> _parameters = new Dictionary<string, string>();
        private Uri _wmsUri;
        private WmsPictureFormat _pictureFormat;
        private string _srs;
        private string _version = "1.1.0";
        private WebProxy _webProxy;
        private Collection<string> _styles;
        private bool _isTransparent;
        #endregion

        #region public properties

        public Collection<string> Layers
        {
            get { return _layers; }
            set { _layers = value; }
        }

        public Dictionary<string,string> Parameters
        {
            get { return _parameters; }
            set { _parameters = value; }
        }

        public string WmsUrl
        {
            get
            {
                if (_wmsUri != null)
                    return _wmsUri.AbsoluteUri;
                else
                    return String.Empty;
            }
            set
            {
                _wmsUri = new Uri(value);
            }
        }

        public string Srs
        {
            get
            {
                return _srs;
            }
            set
            {
                _srs = value;
            }
        }

        public string Version
        {
            get
            {
                return _version;
            }
            set
            {
                _version = value;
            }
        }

        public WebProxy WebProxy
        {
            get { return _webProxy; }
            set { _webProxy = value; }
        }

        public WmsPictureFormat PictureFormat
        {
            get { return _pictureFormat; }
            set { _pictureFormat = value; }
        }

        public Collection<string> Styles
        {
            get { return _styles; }
            set { _styles = value; }
        }

        public string ServiceName
        {
            get { return _wmsUri.Authority; }
        }

        public bool IsTransparent
        {
            get { return _isTransparent; }
            set { _isTransparent = value; }
        }

        #endregion

        #region public methods

        public string GetRequestUrl(RectangleShape worldExtent, int canvasWidth, int canvasHeight)
        {
            StringBuilder requestString = new StringBuilder(this._wmsUri.AbsoluteUri);

            if (!requestString.ToString().Contains("?"))
            {
                requestString.Append("?");
            }
            if (!requestString.ToString().EndsWith("&", StringComparison.OrdinalIgnoreCase) && !requestString.ToString().EndsWith("?", StringComparison.OrdinalIgnoreCase))
            {
                requestString.Append("&");
            }

            requestString.AppendFormat(CultureInfo.InvariantCulture, "REQUEST=GetMap&BBOX={0},{1},{2},{3}", worldExtent.UpperLeftPoint.X, worldExtent.LowerRightPoint.Y, worldExtent.LowerRightPoint.X, worldExtent.UpperLeftPoint.Y);
            requestString.AppendFormat(CultureInfo.InvariantCulture, "&WIDTH={0}&HEIGHT={1}", canvasWidth, canvasHeight);
            requestString.Append("&LAYERS=");

            foreach(string layer in _layers)
            {
                requestString.AppendFormat(CultureInfo.InvariantCulture, "{0},", layer);
            }
            requestString.Remove(requestString.Length - 1, 1);

            requestString.AppendFormat(CultureInfo.InvariantCulture, "&FORMAT={0}", ProcessOutputFormat());

            if (_isTransparent)
            {
                requestString.AppendFormat(CultureInfo.InvariantCulture, "&TRANSPARENT=TRUE");
            }

            if (_version == "1.3.0")
            {
                requestString.AppendFormat(CultureInfo.InvariantCulture, "&CRS={0}", _srs);
            }
            else
            {
                requestString.AppendFormat(CultureInfo.InvariantCulture, "&SRS={0}", _srs);
            }

            if (_styles != null)
                if(_styles.Count > 0)
                {
                    requestString.Append("&STYLES=");
                    foreach(string style in _styles)
                    {
                        requestString.AppendFormat(CultureInfo.InvariantCulture, "{0},", style);
                    }
                    requestString.Remove(requestString.Length - 1, 1);
                }

            if (_version != null && _version != String.Empty)
                requestString.AppendFormat(CultureInfo.InvariantCulture, "&VERSION={0}", _version);

            string result = requestString.ToString();
            if(_parameters.Count > 0)
            {
                result = ProcessParameters(result);
            }

            return result;
        }

        public Bitmap FindImage(string requestString)
        {

            Bitmap image = null;
            WebRequest request = HttpWebRequest.Create(requestString);
            request.Proxy = WebProxy;
            using (Stream imageStream01 = request.GetResponse().GetResponseStream())
            {
                image = new Bitmap(imageStream01);
            }

            return image;
        }

        #endregion

        #region private methods
        private string ProcessParameters(string requestString)
        {
            foreach (string key in _parameters.Keys)
            {
                string keyPattern = "&" + key + "=";
                if (requestString.IndexOf(keyPattern, StringComparison.OrdinalIgnoreCase) != -1)
                {
                    int index = requestString.IndexOf(keyPattern, StringComparison.OrdinalIgnoreCase) + keyPattern.Length;
                    string part1 = requestString.Substring(0, index);
                    string part2 = string.Empty;
                    if (requestString.IndexOf("&", index, StringComparison.OrdinalIgnoreCase) != -1)
                    {
                        part2 = requestString.Substring(requestString.IndexOf("&", index, StringComparison.OrdinalIgnoreCase));
                    }

                    requestString = part1 + _parameters[key] + part2;
                }
                else
                {
                    requestString += keyPattern + _parameters[key];
                }
            }
            return requestString;
        }

        private string ProcessOutputFormat()
        {
            switch (this._pictureFormat)
            {
                case WmsPictureFormat.Gif:
                    return "IMAGE/GIF";
                case WmsPictureFormat.Jpeg:
                    return "IMAGE/JPEG";
                case WmsPictureFormat.Tiff:
                    return "IMAGE/TIFF";
                case WmsPictureFormat.Png8:
                    return "IMAGE/PNG; MODE=8BIT";
                default:
                    return "IMAGE/PNG";
            }
        }
        #endregion




    }
}

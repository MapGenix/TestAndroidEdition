using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using Mapgenix.Canvas;
using Android.Content;
using Android.Graphics;

namespace Mapgenix.GSuite.Android
{
    /// <summary>
    /// Tile from an Uri.
    /// </summary>
    [Serializable]
    [ToolboxItem(false)]
    public class UriTile : Tile
    {
        public event EventHandler UriFormatted;

        public UriTile(Context context)
            : base(context)
        {
            TimeoutInSeconds = 100;
        }

        public Uri Uri { get; set; }

        public WebProxy WebProxy { get; set; }

        public int TimeoutInSeconds { get; set; }

        internal object ThreadLocker { get; set; }

        public UriTileMode UriTileMode { get; set; }
               
        protected override void DrawCore(BaseGeoCanvas geoCanvas)
        {
            HttpWebResponse response = null;
            Bitmap bitmap = null;
            GeoImage geoImage = null;
            MemoryStream imageStream = null;

            try
            {
                lock (ThreadLocker)
                {
                    if(UriTileMode == UriTileMode.Default)
                    {
                        FormatUri(geoCanvas);
                        OnUriFormatted();
                    }
                    
                    WebRequest request = HttpWebRequest.Create(this.Uri);
                    request.Proxy = WebProxy;
                    if (TimeoutInSeconds > 0) request.Timeout = TimeoutInSeconds * 1000;
                    response = (HttpWebResponse)request.GetResponse();
                    Stream stream = response.GetResponseStream();
                    bitmap = BitmapFactory.DecodeStream(stream);
                }

                imageStream = new MemoryStream();
                bitmap.Compress(Bitmap.CompressFormat.Png, 0, imageStream);
                geoImage = new GeoImage(imageStream);
                geoCanvas.DrawScreenImage(geoImage,
                    geoCanvas.Width * .5f,
                    geoCanvas.Height * .5f,
                    geoCanvas.Width,
                    geoCanvas.Height, DrawingLevel.LevelOne, 0f, 0f, 0f);
            }
            catch(Exception ex)
            {
                if (ex != null)
                {

                }
            }
            finally
            {
                if (response != null) { response.Close(); response = null; }
                if (bitmap != null) { bitmap.Dispose(); bitmap = null; }
                if (geoImage != null) { geoImage.Dispose(); geoImage = null; }
            }
        }

       
        protected virtual void OnUriFormatted()
        {
            EventHandler handler = UriFormatted;
            if (handler != null)
            {
                handler(this, new EventArgs());
            }
        }

        private void FormatUri(BaseGeoCanvas geoCanvas)
        {
            string query = Uri.Query.Trim('&', '?');
            string[] querySectors = query.Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
            StringBuilder queryBuilder = new StringBuilder();
            foreach (string querySector in querySectors)
            {
                string[] nameValue = querySector.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                if (nameValue.Length == 2)
                {
                    string name = nameValue[0];
                    string value = nameValue[1];
                    if (name.Equals("BBOX", StringComparison.OrdinalIgnoreCase))
                        value = String.Format(CultureInfo.InvariantCulture,
                         "{0},{1},{2},{3}",
                         geoCanvas.CurrentWorldExtent.LowerLeftPoint.X,
                         geoCanvas.CurrentWorldExtent.LowerLeftPoint.Y,
                         geoCanvas.CurrentWorldExtent.UpperRightPoint.X,
                         geoCanvas.CurrentWorldExtent.UpperRightPoint.Y);
                    else if (name.Equals("WIDTH", StringComparison.OrdinalIgnoreCase))
                        value = geoCanvas.Width.ToString(CultureInfo.InvariantCulture);
                    else if (name.Equals("HEIGHT", StringComparison.OrdinalIgnoreCase))
                        value = geoCanvas.Height.ToString(CultureInfo.InvariantCulture);

                    queryBuilder.AppendFormat(CultureInfo.InvariantCulture, "&{0}={1}", name, value);
                }
            }

            UriBuilder uriBuilder = new UriBuilder(Uri);
            uriBuilder.Query = queryBuilder.ToString().Trim('&');
            Uri = uriBuilder.Uri;
        }
    }
}

using System;

namespace Mapgenix.Layers
{
    [Serializable]
    public class CreatingRequestGoogleMapsLayerEventArgs : EventArgs
    {
        private Uri requestUri;

        public CreatingRequestGoogleMapsLayerEventArgs()
            : this(null)
        {
        }

        public CreatingRequestGoogleMapsLayerEventArgs(Uri requestUri)
        {
            this.requestUri = requestUri;
        }

        public Uri RequestUri
        {
            get { return requestUri; }
            set { requestUri = value; }
        }
    }
}

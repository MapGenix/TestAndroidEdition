using System;

namespace Mapgenix.FeatureSource
{
    [Serializable]
    public class WfsResponseEventArgs : EventArgs
    {
        private readonly string xmlResponse;
        private readonly string serviceUrl;

       
        public WfsResponseEventArgs()
            : this(string.Empty, string.Empty)
        { }

       
        public WfsResponseEventArgs(string serviceUrl, string xmlResponse)
        {
            this.serviceUrl = serviceUrl;
            this.xmlResponse = xmlResponse;
        }

       
        public string XmlResponse
        {
            get { return xmlResponse; }
        }

      
        public string ServiceUrl
        {
            get { return serviceUrl; }
        }
    }
}

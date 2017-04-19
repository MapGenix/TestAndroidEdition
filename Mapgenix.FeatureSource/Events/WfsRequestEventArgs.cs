using System;

namespace Mapgenix.FeatureSource
{
    [Serializable]
    public class WfsRequestEventArgs : EventArgs
    {
        private string serviceUrl;
        private bool overrideResponse;
        private string xmlResponse;

       
        public WfsRequestEventArgs()
            : this(null, false, null)
        { }

       
        public WfsRequestEventArgs(string serviceUrl, bool overrideResponse, string xmlResponse)
        {
            this.serviceUrl = serviceUrl;
            this.overrideResponse = overrideResponse;
            this.xmlResponse = xmlResponse;
        }

       
        public string ServiceUrl
        {
            get { return serviceUrl; }
            set { serviceUrl = value; }
        }

       
        public bool OverrideResponse
        {
            get { return overrideResponse; }
            set { overrideResponse = value; }
        }

      
        public string XmlResponse
        {
            get { return xmlResponse; }
            set { xmlResponse = value; }
        }
    }
}

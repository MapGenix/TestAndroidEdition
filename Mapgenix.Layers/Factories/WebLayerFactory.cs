using System;
using System.Net;
using Mapgenix.Canvas;
using Mapgenix.FeatureSource;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Mapgenix.Layers
{
    /// <summary>
    /// Factory class for web map services layers.
    /// </summary>
    public static class WebLayerFactory
    {
        public static WfsFeatureLayer CreateWfsFeatureLayer(string serviceLocationUrl, string typeName)
        {
            return new WfsFeatureLayer
            {
                FeatureSource = FeatureSourceFactory.CreateWfsFeatureSource(serviceLocationUrl, typeName),
                Name = string.Empty,
				HasBoundingBox = true,
                IsVisible = true
            };
        }


        public static OpenStreetMapLayer CreateOpenStreetMapLayer(string cacheDirectory)
        {
            return CreateOpenStreetMapLayer(null, cacheDirectory);
        }

        public static OpenStreetMapLayer CreateOpenStreetMapLayer(WebProxy webProxy, string cacheDirectory)
        {
            return new OpenStreetMapLayer(webProxy)
            {
                CacheDirectory =  cacheDirectory,
            };
        }

		public static OpenStreetMapLayer CreateOpenStreetMapLayer(WebProxy webProxy)
		{
            return new OpenStreetMapLayer(webProxy);
			
		}

        public static GoogleMapsLayer CreateGoogleMapsLayer(string cacheDirectory, string clientId, string privateKey)
        {
            return CreateGoogleMapsLayer(string.Empty, cacheDirectory, clientId, privateKey, null);
        }

        public static GoogleMapsLayer CreateGoogleMapsLayer(String cacheDirectory, String clientId, String privateKey, WebProxy webProxy)
        {
            return CreateGoogleMapsLayer(string.Empty, cacheDirectory, clientId, privateKey, webProxy);
        }

        public static GoogleMapsLayer CreateGoogleMapsLayer(string licenseKey, string cacheDirectory, string clientId, string privateKey, WebProxy webProxy)
        {
            GoogleRequestBuilder builder = new GoogleRequestBuilder
                {
                    LicenseKey = licenseKey,
                    PrivateKey = privateKey,
                    ClientId = clientId,
                    WebProxy = webProxy,
                    TimeoutInSeconds = 20,
                };
            return new GoogleMapsLayer(licenseKey,cacheDirectory,clientId,privateKey,webProxy);
        }

        public static HEREMapsLayer CreateHEREMapsLayer(string licenseKey, string appCode)
        {
            return CreateHEREMapsLayer(licenseKey, appCode, HEREMapsMapType.Normal, String.Empty, HEREMapsPictureFormat.Png, null);
        }

        public static HEREMapsLayer CreateHEREMapsLayer(string licenseKey, string appCode, string cacheDirectory)
        {
            return CreateHEREMapsLayer(licenseKey, appCode, HEREMapsMapType.Normal, cacheDirectory, HEREMapsPictureFormat.Png, null);
        }

        public static HEREMapsLayer CreateHEREMapsLayer(string licenseKey, string appCode, string cacheDirectory, HEREMapsMapType mapType)
        {
            return CreateHEREMapsLayer(licenseKey, appCode, mapType, cacheDirectory, HEREMapsPictureFormat.Png, null);
        }

        public static HEREMapsLayer CreateHEREMapsLayer(string licenseKey, string appCode, string cacheDirectory, HEREMapsMapType mapType, HEREMapsPictureFormat pictureFormat)
        {
            return CreateHEREMapsLayer(licenseKey, appCode, mapType, cacheDirectory, pictureFormat, null);
        }

        public static HEREMapsLayer CreateHEREMapsLayer(string appID, string appCode, HEREMapsMapType mapType, string cacheDirectory, HEREMapsPictureFormat pictureFormat, WebProxy webProxy)
        {
            return new HEREMapsLayer(appID, appCode, mapType, cacheDirectory, pictureFormat, webProxy);
        }

        public static BingMapsLayer CreateBingMapsLayer(string licenseKey)
        {
            return CreateBingMapsLayer(licenseKey, BingMapsMapType.Road, String.Empty, BingMapsPictureFormat.Png, null);
        }

        public static BingMapsLayer CreateBingMapsLayer(string licenseKey, BingMapsMapType mapType)
        {
            return CreateBingMapsLayer(licenseKey, mapType, String.Empty, BingMapsPictureFormat.Png, null);
        }

        public static BingMapsLayer CreateBingMapsLayer(string licenseKey, BingMapsMapType mapType, string cacheDirectory)
        {
            return CreateBingMapsLayer(licenseKey, mapType, cacheDirectory, BingMapsPictureFormat.Png, null);
        }

        public static BingMapsLayer CreateBingMapsLayer(string licenseKey, BingMapsMapType mapType, string cacheDirectory, BingMapsPictureFormat pictureFormat)
        {
            return CreateBingMapsLayer(licenseKey, mapType, cacheDirectory, pictureFormat, null);
        }

        public static BingMapsLayer CreateBingMapsLayer(string licenseKey, BingMapsMapType mapType, string cacheDirectory, BingMapsPictureFormat pictureFormat, WebProxy webProxy)
        {
            return new BingMapsLayer(licenseKey, mapType, cacheDirectory, pictureFormat, webProxy);
        }


        public static WmsLayer CreateWmsLayer(string serverUri, string layer, string srs, Dictionary<string,string> parameters)
        {
            Collection<string> layers = new Collection<string>();
            layers.Add(layer);
            Collection<string> styles = new Collection<string>();

            return CreateWmsLayer(serverUri, layers, srs, parameters, styles, WmsPictureFormat.Png, GeoColor.StandardColors.Black, String.Empty, String.Empty, null);
        }

        public static WmsLayer CreateWmsLayer(string serverUri, Collection<string> layers, string srs, Dictionary<string, string> parameters, 
                Collection<string> styles, WmsPictureFormat pictureFormat, GeoColor keyColor, string clientID, string privateKey, WebProxy webProxy)
        {

            Collection<GeoColor> keyColors = new Collection<GeoColor>();
            keyColors.Add(keyColor);

            return new WmsLayer
            {
                RequestBuilder = new WmsRequestBuilder
                {
                    Layers = layers,
                    Parameters = parameters,
                    Srs = srs,
                    Styles = styles,
                    PictureFormat = pictureFormat,
                    WebProxy = webProxy,
                    WmsUrl = serverUri
                },

                KeyColor = keyColor,
                KeyColors = keyColors,
                WebProxy = webProxy,
                PrivateKey = privateKey,
                ClientId = clientID,
                IsVisible = true

            };
        }

        public static ArcGISRestFeatureLayer CreateArcGISRestFeatureLayer(string mapServerUrl, string layerId)
        {

            return new ArcGISRestFeatureLayer
            {
                FeatureSource = FeatureSourceFactory.CreateArcGISRestFeatureSource(mapServerUrl, layerId),
                Name = string.Empty,
                HasBoundingBox = true,
                IsVisible = true
            };
        }

    }
}

using System.Collections.Generic;
using System.Net;
using Mapgenix.Layers;
using Mapgenix.Canvas;
using System;
using System.Collections.ObjectModel;
using Android.Content;

namespace Mapgenix.GSuite.Android
{
    /// <summary>Factory for creating overlays</summary>
    public static class OverlayFactory
    {
        /// <summary>
        /// Factory method for creating an OpenStreetMapOverlay
        /// </summary>
        public static OpenStreetMapOverlay CreateOpenStreetMapOverlay(Context context, string cacheDirectory)
        {
            return CreateOpenStreetMapOverlay(context, null, cacheDirectory);
        }

        /// <summary>
        /// Factory method for creating an OpenStreetMapOverlay
        /// </summary>
		public static OpenStreetMapOverlay CreateOpenStreetMapOverlay(Context context, WebProxy webProxy, string cacheDirectory)
        {
            OpenStreetMapOverlay overlay = new OpenStreetMapOverlay(context, webProxy, cacheDirectory)
            {
                CacheDirectory = cacheDirectory,
                DrawingExceptionMode = DrawingExceptionMode.DrawException,
            };
            return overlay;
        }
        /*
        /// <summary>
        /// Factory method for creating a SimpleMarkerOverlay
        /// </summary>
        public static SimpleMarkerOverlay CreateSimpleMarkerOverlay()
        {
            SimpleMarkerOverlay overlay = new SimpleMarkerOverlay
            {
                DragMode = MarkerDragMode.None
            };
            return overlay;
        }

        /// <summary>
        /// Factory method for creating a SimpleMarkerOverlay
        /// </summary>
        public static SimpleMarkerOverlay CreateSimpleMarkerOverlay(IEnumerable<Marker> markers)
        {

            SimpleMarkerOverlay overlay = new SimpleMarkerOverlay
            {
                DragMode = MarkerDragMode.None
            };
            foreach (Marker marker in markers)
            {
                overlay.Markers.Add(marker);
            }
            return overlay;
        }*/

        /// <summary>
        /// Factory method for creating an HEREMapsOverlay
        /// </summary>
        public static HEREMapsOverlay CreateHEREMapsOverlay(Context context)
        {
            return CreateHEREMapsOverlay(context, String.Empty, String.Empty, HEREMapsMapType.Normal, String.Empty, HEREMapsPictureFormat.Png, null);
        }

        /// <summary>
        /// Factory method for creating an HEREMapsOverlay
        /// </summary>
        public static HEREMapsOverlay CreateHEREMapsOverlay(Context context, string appID, string appCode)
        {
            return CreateHEREMapsOverlay(context, appID, appCode, HEREMapsMapType.Normal, String.Empty, HEREMapsPictureFormat.Png, null);
        }

        /// <summary>
        /// Factory method for creating an HEREMapsOverlay
        /// </summary>
        public static HEREMapsOverlay CreateHEREMapsOverlay(Context context, string appID, string appCode, HEREMapsMapType mapType)
        {
            return CreateHEREMapsOverlay(context, appID, appCode, mapType, String.Empty, HEREMapsPictureFormat.Png, null);
        }

        /// <summary>
        /// Factory method for creating an HEREMapsOverlay
        /// </summary>
        public static HEREMapsOverlay CreateHEREMapsOverlay(Context context, string appID, string appCode, HEREMapsMapType mapType, string cacheDirectory)
        {
            return CreateHEREMapsOverlay(context, appID, appCode, mapType, cacheDirectory, HEREMapsPictureFormat.Png, null);
        }

        /// <summary>
        /// Factory method for creating an HEREMapsOverlay
        /// </summary>
        public static HEREMapsOverlay CreateHEREMapsOverlay(Context context, string appID, string appCode, HEREMapsMapType mapType, string cacheDirectory, HEREMapsPictureFormat pictureFormat)
        {
            return CreateHEREMapsOverlay(context, appID, appCode, mapType, cacheDirectory, pictureFormat, null);
        }

        /// <summary>
        /// Factory method for creating an HEREMapsOverlay
        /// </summary>
		public static HEREMapsOverlay CreateHEREMapsOverlay(Context context, string appID, string appCode, HEREMapsMapType mapType, string cacheDirectory, HEREMapsPictureFormat pictureFormat, WebProxy webProxy)
        {
            HEREMapsOverlay overlay = new HEREMapsOverlay(context)
            {
                AppID = appID,
                AppCode = appCode,
                MapType = mapType,
                CacheDirectory = cacheDirectory,
                CachePictureFormat = pictureFormat,
                WebProxy = webProxy,
                DrawingExceptionMode = DrawingExceptionMode.DrawException,

            };
            return overlay;
        }
        
        /// <summary>
        /// Factory method for creating an BingMapsOverlay
        /// </summary>
        public static BingMapsOverlay CreateBingMapsOverlay(Context context, string LicenseKey)
        {
            return CreateBingMapsOverlay(context, LicenseKey, BingMapsMapType.Road, String.Empty, BingMapsPictureFormat.Png, null);
        }

        /// <summary>
        /// Factory method for creating an BingMapsOverlay
        /// </summary>
        public static BingMapsOverlay CreateBingMapsOverlay(Context context, string licenseKey, BingMapsMapType mapType)
        {
            return CreateBingMapsOverlay(context, licenseKey, mapType, String.Empty, BingMapsPictureFormat.Png, null);
        }

        /// <summary>
        /// Factory method for creating an BingMapsOverlay
        /// </summary>
        public static BingMapsOverlay CreateBingMapsOverlay(Context context, string licenseKey, BingMapsMapType mapType, string cacheDirectory)
        {
            return CreateBingMapsOverlay(context, licenseKey, mapType, cacheDirectory, BingMapsPictureFormat.Png, null);
        }

        /// <summary>
        /// Factory method for creating an BingMapsOverlay
        /// </summary>
        public static BingMapsOverlay CreateBingMapsOverlay(Context context, string licenseKey, BingMapsMapType mapType, string cacheDirectory, BingMapsPictureFormat pictureFormat)
        {
            return CreateBingMapsOverlay(context, licenseKey, mapType, cacheDirectory, pictureFormat, null);
        }

        /// <summary>
        /// Factory method for creating an BingMapsOverlay
        /// </summary>
		public static BingMapsOverlay CreateBingMapsOverlay(Context context, string licenseKey, BingMapsMapType mapType, string cacheDirectory, BingMapsPictureFormat pictureFormat, WebProxy webProxy)
        {
            BingMapsOverlay overlay = new BingMapsOverlay(context)
            {
                LicenseKey = licenseKey,
                MapType = mapType,
                CacheDirectory = cacheDirectory,
                PictureFormat = pictureFormat,
                WebProxy = webProxy,
                DrawingExceptionMode = DrawingExceptionMode.DrawException,

            };
            return overlay;
        }

        /// <summary>
        /// Factory method for creating an GoogleMapsOverlay
        /// </summary>
        public static GoogleMapsOverlay CreateGoogleMapsOverlay(Context context, string licenseKey)
        {
            return CreateGoogleMapsOverlay(context, licenseKey, String.Empty, String.Empty, String.Empty, null);
        }

        /// <summary>
        /// Factory method for creating an GoogleMapsOverlay
        /// </summary>
        public static GoogleMapsOverlay CreateGoogleMapsOverlay(Context context, string licenseKey, string cacheDirectory)
        {
            return CreateGoogleMapsOverlay(context, licenseKey, cacheDirectory, String.Empty, String.Empty, null);
        }

        /// <summary>
        /// Factory method for creating an GoogleMapsOverlay
        /// </summary>
        public static GoogleMapsOverlay CreateGoogleMapsOverlay(Context context, string licenseKey, string cacheDirectory, string clientId)
        {
            return CreateGoogleMapsOverlay(context, licenseKey, cacheDirectory, clientId, String.Empty, null);
        }

        /// <summary>
        /// Factory method for creating an GoogleMapsOverlay
        /// </summary>
        public static GoogleMapsOverlay CreateGoogleMapsOverlay(Context context, string licenseKey, string cacheDirectory, string clientId, string privateKey)
        {
            return CreateGoogleMapsOverlay(context, licenseKey, cacheDirectory, clientId, privateKey, null);
        }

        /// <summary>
        /// Factory method for creating an GoogleMapsOverlay
        /// </summary>
        public static GoogleMapsOverlay CreateGoogleMapsOverlay(Context context, string licenseKey, string cacheDirectory,
            string clientId, string privateKey, WebProxy webProxy)
        {
            GoogleMapsOverlay overlay = new GoogleMapsOverlay(context)
            {
                LicenseKey = licenseKey,
                CacheDirectory = cacheDirectory,
                ClientID = clientId,
                PrivateKey = privateKey,
                WebProxy = webProxy
            };
            return overlay;
        }
        
        /// <summary>
        /// Factory method for creating an WmsOverlayLite
        /// </summary>
        public static WmsOverlayLite CreateWmsOverlayLite(Context context)
        {
            return CreateWmsOverlayLite(context, new Collection<Uri>(), null);
        }

        /// <summary>
        /// Factory method for creating an WmsOverlayLite
        /// </summary>
        public static WmsOverlayLite CreateWmsOverlayLite(Context context, Uri serverUri)
        {
            return CreateWmsOverlayLite(context, new Collection<Uri>() { serverUri }, null);
        }

        /// <summary>
        /// Factory method for creating an WmsOverlayLite
        /// </summary>
        public static WmsOverlayLite CreateWmsOverlayLite(Context context, IEnumerable<Uri> serverUris)
        {
            return CreateWmsOverlayLite(context,serverUris, null);
        }

        /// <summary>
        /// Factory method for creating an WmsOverlayLite
        /// </summary>
        public static WmsOverlayLite CreateWmsOverlayLite(Context context, IEnumerable<Uri> serverUris, WebProxy webProxy)
        {
            WmsOverlayLite overlay = new WmsOverlayLite(context)
            {
                ServerUris = (Collection<Uri>)serverUris,
                WebProxy = webProxy
            };
            return overlay;
        }
    }
}

using System;
using System.Collections.ObjectModel;
using System.Net;
using Mapgenix.FeatureSource;

namespace Mapgenix.Layers
{
    /// <summary>Feature layer for OGC WFS(Web Feature Service)</summary>
    [Serializable]
	public class WfsFeatureLayer : BaseFeatureLayer
	{

        /// <summary>Gets Capabilities for WFS server.</summary>
        /// <param name="serverUri"></param>
        /// <returns>String xml with server WFS capabilities.</returns>
		public static string GetCapabilities(Uri serverUri)
		{
			Validators.CheckParameterIsNotNull(serverUri, "serverUri");

			return WfsFeatureSourceHelper.GetCapabilities(serverUri);
		}
        /// <summary>Gets layers available in wfs server.</summary>
        /// <param name="serverUri"></param>
        /// <param name="webProxy"></param>
        /// <returns></returns>
		public static Collection<string> GetLayers(Uri serverUri, WebProxy webProxy)
		{
			Validators.CheckParameterIsNotNull(serverUri, "serverUri");

			return WfsFeatureSourceHelper.GetLayers(serverUri, webProxy);
		}

        /// <summary>Gets layers available in wfs server.</summary>
        /// <param name="serverUri"></param>
        /// <returns></returns>
		public static Collection<string> GetLayers(Uri serverUri)
		{
			Validators.CheckParameterIsNotNull(serverUri, "serverUri");

			return GetLayers(serverUri);
		}
	}
}
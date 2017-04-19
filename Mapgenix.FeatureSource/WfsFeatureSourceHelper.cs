using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml;

namespace Mapgenix.FeatureSource
{
    /// <summary>Static class for operations on OGS WFS services.</summary>
    public static class WfsFeatureSourceHelper
	{
		private const string GetCapabiltiesQuery = "?SERVICE=WFS&VERSION=1.0.0&REQUEST=GetCapabilities";

        /// <summary>Get capabilities from a WFS service url.</summary>
        /// <returns>XML representing capabilities of the WFS server.</returns>
        /// <param name="serviceLocationUrl">Url of the WFS service.</param>
        /// <remarks><para>Every OGC Web Service, including a Web Feature Service, has the ability to describe its capabilities 
        /// by returning service metadata in response to a GetCapabilities request. 
        /// Specifically, every WFS must support the KVP encoded form of the GetCapabilities request over HTTP GET in order to let 
        /// a client know how to obtain a capabilities document.</para>
        /// <para>
        /// The capabilities response document contains the following sections:
        ///1. Service Identification section
        ///The service identification section provides information about the WFS service itself.
        ///2. Service Provider section
        ///The service provider section provides metadata about the organization operating the WFS server.
        ///3. Operation Metadata section
        ///The operations metadata section provides metadata about the operations defined in this specification and implemented by a particular WFS server. This metadata includes the DCP, parameters and constraints for each operation.
        ///4. FeatureType list section
        ///This section defines the list of feature types (and operations on each feature type) that are available from a web feature service. Additional information, such as the default SRS, any other supported SRSs, or no SRS whatsoever (for non-spatial feature types), for WFS requests is provided for each feature type.
        ///5. ServesGMLObjectType list section
        ///This section defines the list of GML Object types, not derived from gml:AbstractFeatureType, that are available from a web feature service that supports the GetGMLObject operation. These types may be defined in a base GML schema, or in an application schema using its own namespace.
        ///6. SupportsGMLObjectType list section
        ///The Supports GML Object Type section defines the list of GML Object types that a WFS server would be capable of serving if it was deployed to serve data.
        ///described by an application schema that either used those GML Object types directly (for non-abstract types), or defined derived types based on those types.
        ///7. Filter capabilities section
        ///The schema of the Filter Capabilities Section is defined in the Filter Encoding Implementation Specification [3]. This is an optional section. If it exists, then the WFS should support the operations advertised therein. If the Filter Capabilities Section is not defined, then the client should assume that the server only supports the minimum default set of filter operators.
        /// </para>
        /// </remarks>
		public static string GetCapabilities(string serviceLocationUrl)
		{
			Validators.CheckParameterIsNotNullOrEmpty(serviceLocationUrl, "serviceLocationUrl");

			return GetCapabilities(new Uri(serviceLocationUrl), null);
		}

        /// <summary>Get capabilities from a WFS service url.</summary>
        /// <returns>XML representing capabilities of the WFS server.</returns>
        /// <param name="serverUri">Url of the WFS service.</param>
        /// <remarks><para>Every OGC Web Service, including a Web Feature Service, has the ability to describe its capabilities 
        /// by returning service metadata in response to a GetCapabilities request. 
        /// Specifically, every WFS must support the KVP encoded form of the GetCapabilities request over HTTP GET in order to let 
        /// a client know how to obtain a capabilities document.</para>
        /// <para>
        /// The capabilities response document contains the following sections:
        ///1. Service Identification section
        ///The service identification section provides information about the WFS service itself.
        ///2. Service Provider section
        ///The service provider section provides metadata about the organization operating the WFS server.
        ///3. Operation Metadata section
        ///The operations metadata section provides metadata about the operations defined in this specification and implemented by a particular WFS server. This metadata includes the DCP, parameters and constraints for each operation.
        ///4. FeatureType list section
        ///This section defines the list of feature types (and operations on each feature type) that are available from a web feature service. Additional information, such as the default SRS, any other supported SRSs, or no SRS whatsoever (for non-spatial feature types), for WFS requests is provided for each feature type.
        ///5. ServesGMLObjectType list section
        ///This section defines the list of GML Object types, not derived from gml:AbstractFeatureType, that are available from a web feature service that supports the GetGMLObject operation. These types may be defined in a base GML schema, or in an application schema using its own namespace.
        ///6. SupportsGMLObjectType list section
        ///The Supports GML Object Type section defines the list of GML Object types that a WFS server would be capable of serving if it was deployed to serve data.
        ///described by an application schema that either used those GML Object types directly (for non-abstract types), or defined derived types based on those types.
        ///7. Filter capabilities section
        ///The schema of the Filter Capabilities Section is defined in the Filter Encoding Implementation Specification [3]. This is an optional section. If it exists, then the WFS should support the operations advertised therein. If the Filter Capabilities Section is not defined, then the client should assume that the server only supports the minimum default set of filter operators.
        /// </para>
        /// </remarks>
		public static string GetCapabilities(Uri serverUri)
		{
			Validators.CheckParameterIsNotNull(serverUri, "serverUri");

			return GetCapabilities(serverUri, null);
		}

        /// <summary>Get capabilities from a WFS service url.</summary>
        /// <returns>XML representing capabilities of the WFS server.</returns>
        /// <param name="serverUri">Url of the WFS service.</param>
        /// <param name="webProxy">Proxy of the WFS service.</param>
        /// <remarks><para>Every OGC Web Service, including a Web Feature Service, has the ability to describe its capabilities 
        /// by returning service metadata in response to a GetCapabilities request. 
        /// Specifically, every WFS must support the KVP encoded form of the GetCapabilities request over HTTP GET in order to let 
        /// a client know how to obtain a capabilities document.</para>
        /// <para>
        /// The capabilities response document contains the following sections:
        ///1. Service Identification section
        ///The service identification section provides information about the WFS service itself.
        ///2. Service Provider section
        ///The service provider section provides metadata about the organization operating the WFS server.
        ///3. Operation Metadata section
        ///The operations metadata section provides metadata about the operations defined in this specification and implemented by a particular WFS server. This metadata includes the DCP, parameters and constraints for each operation.
        ///4. FeatureType list section
        ///This section defines the list of feature types (and operations on each feature type) that are available from a web feature service. Additional information, such as the default SRS, any other supported SRSs, or no SRS whatsoever (for non-spatial feature types), for WFS requests is provided for each feature type.
        ///5. ServesGMLObjectType list section
        ///This section defines the list of GML Object types, not derived from gml:AbstractFeatureType, that are available from a web feature service that supports the GetGMLObject operation. These types may be defined in a base GML schema, or in an application schema using its own namespace.
        ///6. SupportsGMLObjectType list section
        ///The Supports GML Object Type section defines the list of GML Object types that a WFS server would be capable of serving if it was deployed to serve data.
        ///described by an application schema that either used those GML Object types directly (for non-abstract types), or defined derived types based on those types.
        ///7. Filter capabilities section
        ///The schema of the Filter Capabilities Section is defined in the Filter Encoding Implementation Specification [3]. This is an optional section. If it exists, then the WFS should support the operations advertised therein. If the Filter Capabilities Section is not defined, then the client should assume that the server only supports the minimum default set of filter operators.
        /// </para>
        /// </remarks>
		public static string GetCapabilities(Uri serverUri, WebProxy webProxy)
		{
			Validators.CheckParameterIsNotNull(serverUri, "serverUri");

			string capabilities;
			string requestString = serverUri.AbsoluteUri + GetCapabiltiesQuery;
			WebRequest request = WebRequest.Create(new Uri(requestString));

			StreamReader reader = null;
			try
			{
				request.Proxy = webProxy;
				Stream netStream = request.GetResponse().GetResponseStream();
				Debug.Assert(netStream != null, "netStream != null");
				reader = new StreamReader(netStream);
				capabilities = reader.ReadToEnd();
			}
			finally
			{
				if (reader != null)
				{
					reader.Close();
				}
			}

			return capabilities;
		}

        /// <summary>Gets layer names from a WFS service url.</summary>
        /// <param name="serviceLocationUrl">Url of the WFS service.</param>
        /// <returns>Collection of layer names.</returns>
		public static Collection<string> GetLayers(string serviceLocationUrl)
		{
			Validators.CheckParameterIsNotNullOrEmpty(serviceLocationUrl, "serviceLocationUrl");

			return GetLayers(new Uri(serviceLocationUrl), null);
		}

        /// <summary>Gets layer names from a WFS service url.</summary>
        /// <param name="serverUri">Url of the WFS service.</param>
        /// <returns>Collection of layer names.</returns>
		public static Collection<string> GetLayers(Uri serverUri)
		{
			Validators.CheckParameterIsNotNull(serverUri, "serverUri");

			return GetLayers(serverUri, null);
		}

        /// <summary>Gets layer names from a WFS service url.</summary>
        /// <param name="serverUri">Url of the WFS service.</param>
        /// <param name="webProxy">Proxy of the WFS service.</param>
        /// <returns>Collection of layer names.</returns>
		public static Collection<string> GetLayers(Uri serverUri, WebProxy webProxy)
		{
			Validators.CheckParameterIsNotNull(serverUri, "serverUri");

			Collection<string> availabeLayers = new Collection<string>();
			string requestString = serverUri.AbsoluteUri + GetCapabiltiesQuery;
			WebRequest request = WebRequest.Create(new Uri(requestString));
			Stream netStream = null;
			try
			{
				request.Proxy = webProxy;
				netStream = request.GetResponse().GetResponseStream();

				XmlDocument document = new XmlDocument();

				document.Load(netStream);
				foreach (XmlNode parentNode in document.ChildNodes.Cast<XmlNode>().Where(parentNode => string.Equals(parentNode.LocalName, "WFS_Capabilities", StringComparison.OrdinalIgnoreCase)))
				{
					foreach (XmlNode node in parentNode.ChildNodes.Cast<XmlNode>().Where(node => string.Equals(node.LocalName, "FeatureTypeList", StringComparison.OrdinalIgnoreCase)))
					{
						foreach (XmlNode childNode in node.ChildNodes.Cast<XmlNode>().Where(childNode => string.Equals(childNode.LocalName, "FeatureType", StringComparison.OrdinalIgnoreCase)))
						{
							foreach (XmlNode sunNode in childNode.ChildNodes.Cast<XmlNode>().Where(sunNode => string.Equals(sunNode.LocalName, "Name", StringComparison.OrdinalIgnoreCase)))
							{
								availabeLayers.Add(sunNode.FirstChild.Value.Trim());
								break;
							}
						}
						break;
					}
					break;
				}
			}
			finally
			{
				if (netStream != null)
				{
					netStream.Close();
				}
			}

			return availabeLayers;
		}


		public static string GetWktForPoint(XmlReader reader, string shapeTypeName)
		{
			string resultString = shapeTypeName + "(";

			while (!(!reader.IsStartElement() && reader.LocalName == "Point"))
			{
				if (reader.IsStartElement() && reader.LocalName == "coordinates")
				{
					reader.Read();
					resultString += ConvertString(reader.Value.Trim());
				}
				reader.Read();
			}
			resultString += ")";
			return resultString;
		}

		public static string GetWktForLineString(XmlReader reader, string shapeTypeName)
		{
			string resultString = shapeTypeName + "(";

			while (!(!reader.IsStartElement() && reader.LocalName == "LineString"))
			{
				if (reader.IsStartElement() && reader.LocalName == "coordinates")
				{
					reader.Read();
					resultString += ConvertString(reader.Value.Trim());
				}
				reader.Read();
			}
			resultString += ")";
			return resultString;
		}

		public static string GetWktForPolygon(XmlReader reader, string shapeTypeName)
		{
			string resultString = shapeTypeName + "(";

			while (!(!reader.IsStartElement() && reader.LocalName == "Polygon"))
			{
				if (reader.IsStartElement() && reader.LocalName == "outerBoundaryIs")
				{
					resultString += "(";
				}
				else if (reader.IsStartElement() && reader.LocalName == "innerBoundaryIs")
				{
					resultString += "),(";
				}
				else if (reader.IsStartElement() && reader.LocalName == "coordinates")
				{
					reader.Read();
					resultString += ConvertString(reader.Value.Trim());
				}
				reader.Read();
			}
			resultString += "))";
			return resultString;
		}

		public static string GetWktForMultiPoint(XmlReader reader, string shapeTypeName)
		{
			string resultString = shapeTypeName + "(";
			int originalLength = resultString.Length;

			while (!(!reader.IsStartElement() && reader.LocalName == "MultiPoint"))
			{
				if (reader.IsStartElement() && reader.LocalName == "pointMember")
				{
					if (resultString.Length > originalLength)
					{
						resultString += ",";
					}
				}
				else if (reader.IsStartElement() && reader.LocalName == "coordinates")
				{
					reader.Read();
					resultString += ConvertString(reader.Value.Trim());
				}
				reader.Read();
			}
			resultString += ")";
			return resultString;
		}

		public static string GetWktForMultiLineString(XmlReader reader, string shapeTypeName)
		{
			string resultString = shapeTypeName + "(";
			int originalLength = resultString.Length;

			while (!(!reader.IsStartElement() && reader.LocalName == "MultiLineString"))
			{
				if (reader.IsStartElement() && reader.LocalName == "lineStringMember")
				{
					if (resultString.Length > originalLength)
					{
						resultString += "),(";
					}
					else
					{
						resultString += "(";
					}
				}
				else if (reader.IsStartElement() && reader.LocalName == "coordinates")
				{
					reader.Read();
					resultString += ConvertString(reader.Value.Trim());
				}
				reader.Read();
			}
			resultString += "))";
			return resultString;
		}

		public static string GetWktForMultiPolygon(XmlReader reader, string shapeTypeName)
		{
			string resultString = shapeTypeName + "(";
			int originalLength = resultString.Length;

			while (!(!reader.IsStartElement() && reader.LocalName == "MultiPolygon"))
			{
				if (reader.IsStartElement() && reader.LocalName == "outerBoundaryIs")
				{
					resultString += "(";
				}
				else if (reader.IsStartElement() && reader.LocalName == "innerBoundaryIs")
				{
					resultString += "),(";
				}
				else if (reader.IsStartElement() && reader.LocalName == "polygonMember")
				{
					if (resultString.Length > originalLength)
					{
						resultString += ")),(";
					}
					else
					{
						resultString += "(";
					}
				}
				else if (reader.IsStartElement() && reader.LocalName == "coordinates")
				{
					reader.Read();
					resultString += ConvertString(reader.Value.Trim());
				}
				reader.Read();
			}
			resultString += ")))";
			return resultString;
		}

		public static string ConvertString(string tempString)
		{
			tempString = tempString.Replace(",", ":");
			tempString = tempString.Replace(" ", ",");
			tempString = tempString.Replace(":", " ");

			return tempString;
		}

		public static string GetShapeTypeName(string shapeType)
		{
			switch (shapeType)
			{
				case "MultiPolygon":
				case "MultiLineString":
				case "PointPropertyType":
				case "LineString":
				case "Polygon":
				case "MultiPointPropertyType":
				case "Point":
				return shapeType.ToUpper(CultureInfo.InvariantCulture);

				case "Curve":
				return "LINESTRING";

				case "SurfacePropertyType":
				return "POLYGON";

				case "MultiCurvePropertyType":
				return "MULTILINESTRING";

				case "MultiSurfacePropertyType":
				return "MULTIPOLYGON";

				default:
				return null;
			}
		}
	}
}

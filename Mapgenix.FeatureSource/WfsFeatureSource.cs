using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Xml;
using Mapgenix.Shapes;
using System.Linq;

namespace Mapgenix.FeatureSource
{
    /// <summary>FeatureSource from WFS (Web Feature Service)</summary>
    /// <remarks>To get features from an WFS server.</remarks>
    [Serializable]
    public class WfsFeatureSource : BaseFeatureSource
    {
        #region constants
        private const string GetDescribeFeatureQuery = "?SERVICE=WFS&VERSION=1.0.0&REQUEST=DescribeFeatureType";
        private const string GetFeatureQuery = "?SERVICE=WFS&VERSION=1.0.0&REQUEST=GetFeature";
        #endregion

        #region private attributes
        private WebProxy _webProxy;

        [NonSerialized]
        private Collection<FeatureSourceColumn> _columns;

        [NonSerialized]
        private Collection<string> _columnsName;

        [NonSerialized]
        private string _geometryName = "msGeometry";

        [NonSerialized]
        private bool _featuresLoaded = false;

        /// <summary>
        /// Features load from wfs GetFeature request
        /// </summary>
        [NonSerialized]
        private Collection<Feature> _inMemoryFeatures;
        #endregion

        /// <summary>Url of WFS service.</summary>
        public string ServiceLocationUrl { get; set; }

        public WfsNamespace WfsNamespace { get; set; }

        /// <summary>Typename of the WFS service.</summary>
        public string TypeName { get; set; }

        /// <summary>Last response in XML format. It is passed out by RequestedData event as a parameter.</summary>
        public string LastXmlResponse { get; private set; }

        /// <summary>Timeout of the web request in seconds.  The default timeout value is 20 seconds.</summary>
        public int TimeoutInSeconds { get; set; }

        /// <summary>Gets or sets the proxy used for requesting a Web Response.</summary>
        public WebProxy WebProxy
        {
            get { return _webProxy; }
            set { _webProxy = value; }
        }

        /// <summary>Called before the requesting data by url from the WFS server.</summary>
        /// <remarks>This event is called before requesting data by url from the WFS server.</remarks>
        public event EventHandler<WfsRequestEventArgs> RequestingData;

        /// <returns>None</returns>
        /// <summary>To raise the RequestingData event from a derived class.</summary>
        /// <param name="wfsRequestEventArgs">Event arguments defining the parameters passed to the
        /// recipient of the event.</param>
        protected virtual void OnRequestingData(WfsRequestEventArgs wfsRequestEventArgs)
        {
            EventHandler<WfsRequestEventArgs> handler = RequestingData;
            if (handler != null)
            {
                handler(this, wfsRequestEventArgs);
            }
        }

        /// <summary>Called after requesting data by url from the WFS server.</summary>
        public event EventHandler<WfsResponseEventArgs> RequestedData;

        /// <summary>Raises the RequestedData event from a derived class.</summary>
        /// <param name="wfsResponseEventArgs">Event arguments defining the parameters passed to the
        /// recipient of the event.</param>
        protected virtual void OnRequestedData(WfsResponseEventArgs wfsResponseEventArgs)
        {
            EventHandler<WfsResponseEventArgs> handler = RequestedData;
            if (handler != null)
            {
                handler(this, wfsResponseEventArgs);
            }
        }

        /// <summary>Opens the WfsFeatureSource to have it ready to use.</summary>
        /// <returns>None</returns>
        protected override void OpenCore()
        {
            if (!_featuresLoaded)
            {
                GetColumnsCore();
                LoadInMemoryFeatures();
                _featuresLoaded = true;
            }
        }

        /// <summary>Closes the WfsFeatureSource.</summary>
        protected override void CloseCore()
        {
        }

        /// <summary>Returns the bounding box encompassing all of the features in the WfsFeatureSource.</summary>
        /// <returns>Bounding box encompassing all of the features in the WfsFeatureSource.</returns>
        protected override RectangleShape GetBoundingBoxCore()
        {
            RectangleShape boundingBox = null;

            if(_inMemoryFeatures.Count > 0)
            {
                boundingBox = _inMemoryFeatures[0].GetBoundingBox();
            }

            return boundingBox;
        }

        /// <summary>Returns the columns available for the WfsFeatureSource.</summary>
        ///<returns>Columns available for the WfsFeatureSource.</returns>
        protected override Collection<FeatureSourceColumn> GetColumnsCore()
        {
            if (_columns == null)
            {
                _columnsName = new Collection<string>();
                _columns = new Collection<FeatureSourceColumn>();

                string requestString = GetDescribeFeatureTypeQueryString();

                Stream stream = RequestStreamWithEvents(requestString);
                XmlReaderSettings settings = new XmlReaderSettings();
                settings.IgnoreWhitespace = true;
                XmlReader reader = XmlReader.Create(stream, settings);

                while (!reader.EOF)
                {
                    reader.Read();
                    if (reader.IsStartElement() && reader.LocalName == "complexType")
                    {
                        while (!reader.EOF)
                        {
                            reader.Read();

                            if (!reader.IsStartElement())
                            {
                                if (reader.LocalName == "complexType")
                                {
                                    break;
                                }
                                continue;
                            }

                            if (reader.IsStartElement() && reader.LocalName == "element")
                            {
                                if (reader.HasAttributes)
                                {
                                    string columnName = reader.GetAttribute("name");
                                    string columnType = reader.GetAttribute("type");
                                    if (columnName != null)
                                    {
                                        if (columnType != null)
                                        {
                                            if (columnType.Contains("gml"))
                                            {
                                                _geometryName = columnName;
                                            }
                                            else
                                            {
                                                if (columnType.Contains(reader.Prefix))
                                                {
                                                    columnType = columnType.Remove(0, reader.Prefix.Length + 1);
                                                }

                                                if (!_columnsName.Contains(columnName))
                                                {
                                                    _columnsName.Add(columnName);
                                                    _columns.Add(new FeatureSourceColumn(columnName, columnType, int.MaxValue));
                                                }
                                            }
                                        }
                                        else
                                        {
                                            XmlReader subReader = reader.ReadSubtree();
                                            if (subReader != null)
                                            {
                                                string subcolumnType = null;
                                                int maxLength = 0;
                                                while (!subReader.EOF)
                                                {
                                                    subReader.Read();
                                                    if (subReader.IsStartElement() && subReader.LocalName == "restriction")
                                                    {
                                                        subcolumnType = subReader.GetAttribute("base");
                                                    }
                                                    if (subReader.IsStartElement() && subReader.LocalName == "maxLength")
                                                    {
                                                        int.TryParse(subReader.GetAttribute("value"), NumberStyles.Any, CultureInfo.InvariantCulture, out maxLength);
                                                    }
                                                }
                                                if (subcolumnType != null && maxLength > 0)
                                                {
                                                    if (subcolumnType.Contains(reader.Prefix))
                                                    {
                                                        subcolumnType = subcolumnType.Remove(0, reader.Prefix.Length + 1);
                                                    }

                                                    if (!_columnsName.Contains(columnName))
                                                    {
                                                        _columnsName.Add(columnName);
                                                        _columns.Add(new FeatureSourceColumn(columnName, subcolumnType, maxLength));
                                                    }
                                                }

                                                subReader.Close();
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else if (!reader.IsStartElement() && reader.LocalName == "complexType")
                    {
                        break;
                    }
                }

                stream.Dispose();
            }

            return _columns;
        }

        /// <summary>Returns a collection of features based on Ids.</summary>
        /// <returns>Collection of features based on Ids.</returns>
        /// <param name="ids">Ids uniquely identifying the features in the WfsFeatureSource.</param>
        /// <param name="returningColumnNames">Field names of the column data to return with each Feature.</param>
        protected override Collection<Feature> GetFeaturesByIdsCore(IEnumerable<string> ids, IEnumerable<string> returningColumnNames)
        {
            Validators.CheckParameterIsNotNull(ids, "ids");

            Collection<Feature> features = new Collection<Feature>();

            foreach (Feature feature in _inMemoryFeatures)
            {
                if(ids.Contains(feature.Id))
                {
                    features.Add(feature.CloneDeep(returningColumnNames));
                }
            }

            return features;
        }

        /// <summary>Returns a collection of all the features in the WfsFeatureSource.</summary>
        /// <returns>Collection of all the features in the WfsFeatureSource.</returns>
        /// <param name="returningColumnNames">Field names of the column data to return with each Feature.</param>
        protected override Collection<Feature> GetAllFeaturesCore(IEnumerable<string> returningColumnNames)
        {
            Collection<Feature> features = new Collection<Feature>();

            foreach (Feature feature in _inMemoryFeatures)
            {
                features.Add(feature.CloneDeep(returningColumnNames));
            }

            return features;
        }

        /// <summary>Returns a collection of features of the WfsFeatureSource inside a bounding box.</summary>
        /// <returns>Collection of features of the WfsFeatureSource inside a bounding box.</returns>
        /// <param name="boundingBox">Bounding box to find the features inside.</param>
        /// <param name="returningColumnNames">Field names of the column data to return with each Feature.</param>
        protected override Collection<Feature> GetFeaturesInsideBoundingBoxCore(RectangleShape boundingBox, IEnumerable<string> returningColumnNames)
        {
            Collection<Feature> features = new Collection<Feature>();

            foreach (Feature feature in _inMemoryFeatures)
            {
                if (feature.GetShape().Intersects(boundingBox))
                {
                    features.Add(feature.CloneDeep(returningColumnNames));
                }
            }

            return features;
        }

        private Stream GetStream(RectangleShape boundingBox, IEnumerable<string> returningColumnNames)
        {
            string queryString = GetFeatureQueryString(boundingBox);
            queryString += "&propertyname=";

            if (_geometryName != null)
            {
                queryString += _geometryName;
            }

            if (returningColumnNames != null)
            {
                queryString = returningColumnNames.Aggregate(queryString, (current, columnName) => current + ("," + columnName));
            }

            Stream memoryStream = RequestStreamWithEvents(queryString);

            return memoryStream;

        }

        private Stream RequestStreamWithEvents(string requestString)
        {
            Stream memoryStream = null;

            WfsRequestEventArgs wfsRequestEventArgs = new WfsRequestEventArgs(requestString, false, string.Empty);
            OnRequestingData(wfsRequestEventArgs);

            string xmlResponse = wfsRequestEventArgs.XmlResponse;
            if (!wfsRequestEventArgs.OverrideResponse)
            {

                WebRequest request = WebRequest.Create(requestString);
                request.Timeout = TimeoutInSeconds * 1000;
                request.Proxy = _webProxy;

                memoryStream = request.GetResponse().GetResponseStream();
            }
            else
            {
                LastXmlResponse = xmlResponse;
                memoryStream = new MemoryStream();

                StreamWriter writer = new StreamWriter(memoryStream);
                writer.Write(xmlResponse);
                writer.Flush();
                memoryStream.Seek(0, SeekOrigin.Begin);
            }

            WfsResponseEventArgs wfsResponseEventArgs = new WfsResponseEventArgs(wfsRequestEventArgs.ServiceUrl, LastXmlResponse);
            OnRequestedData(wfsResponseEventArgs);

            return memoryStream;
        }

        private Feature GetFeature(XmlReader reader, IEnumerable<string> columnNames)
        {
            string id = null;
            string wkt = null;
            Dictionary<string, string> columnValues = new Dictionary<string, string>();

            if (columnNames != null)
            {
                foreach (string columnName in columnNames)
                {
                    columnValues.Add(columnName, string.Empty);
                }
            }

            reader.Read();
            while (!reader.EOF && reader.LocalName != "featureMember")
            {
                if (!reader.IsStartElement())
                {
                    reader.Read();
                    continue;
                }

                if (IsShapeType(reader.LocalName))
                {
                    reader.Read();
                    string shapeTypeName = WfsFeatureSourceHelper.GetShapeTypeName(reader.LocalName);
                    switch (shapeTypeName)
                    {
                        case "MULTIPOLYGON":
                            wkt = WfsFeatureSourceHelper.GetWktForMultiPolygon(reader, shapeTypeName);
                            break;

                        case "MULTILINESTRING":
                            wkt = WfsFeatureSourceHelper.GetWktForMultiLineString(reader, shapeTypeName);
                            break;

                        case "MULTIPOINT":
                            wkt = WfsFeatureSourceHelper.GetWktForMultiPoint(reader, shapeTypeName);
                            break;

                        case "POINT":
                            wkt = WfsFeatureSourceHelper.GetWktForPoint(reader, shapeTypeName);
                            break;

                        case "LINESTRING":
                            wkt = WfsFeatureSourceHelper.GetWktForLineString(reader, shapeTypeName);
                            break;

                        case "POLYGON":
                            wkt = WfsFeatureSourceHelper.GetWktForPolygon(reader, shapeTypeName);
                            break;

                        default:
                            break;
                    }
                }
                else if (string.Equals(reader.LocalName, TypeName, StringComparison.OrdinalIgnoreCase)
                         || string.Equals(reader.Name, TypeName, StringComparison.OrdinalIgnoreCase))
                {
                    if (reader.HasAttributes)
                    {
                        string temp = reader.GetAttribute(0);
                        id = temp;
                    }
                    reader.Read();
                }
                else
                {
                    if (string.Equals(reader.Prefix, "gml", StringComparison.OrdinalIgnoreCase))
                    {
                        reader.Skip();
                    }
                    else
                    {
                        string readColumnName = reader.LocalName;

                        string columnValue = reader.ReadElementString();
                        if (columnValues.ContainsKey(readColumnName))
                        {
                            columnValues[readColumnName] = columnValue;
                        }
                    }
                }
            }

            if (wkt == null)
            {
                return new Feature(new byte[] { }, id, columnValues);
            }
            return new Feature(wkt, id, columnValues);
        }

		private string GetDescribeFeatureTypeQueryString()
        {
            return ServiceLocationUrl + GetDescribeFeatureQuery + "&TYPENAME=" + TypeName;
        }

        private string GetFeatureQueryString(RectangleShape boundingBox)
        {
            string queryString = ServiceLocationUrl + GetFeatureQuery + "&TYPENAME=" + TypeName;

            string rectangleString = string.Empty;
            if (boundingBox != null)
            {
                rectangleString += boundingBox.LowerLeftPoint.X.ToString(CultureInfo.InvariantCulture) + ",";
                rectangleString += boundingBox.LowerLeftPoint.Y.ToString(CultureInfo.InvariantCulture) + ",";
                rectangleString += boundingBox.UpperRightPoint.X.ToString(CultureInfo.InvariantCulture) + ",";
                rectangleString += boundingBox.UpperRightPoint.Y.ToString(CultureInfo.InvariantCulture);
                queryString += "&BBOX=" + rectangleString;
            }

            return queryString;
        }

        private bool IsShapeType(string shapeType)
        {
            if (string.Equals(shapeType, _geometryName, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            switch (shapeType)
            {
                case "multiPolygonProperty":
                case "multiLineStringProperty":
                case "pointProperty":
                case "lineStringProperty":
                case "polygonProperty":
                case "curveProperty":
                case "surfaceProperty":
                case "multiPointProperty":
                case "multiCurveProperty":
                case "multiSurfaceProperty":

                case "multiPolygonMember":
                case "multiLineStringMember":
                case "pointMember":
                case "lineStringMember":
                case "polygonMember":
                case "curveMember":
                case "surfaceMember":
                case "multiPointMember":
                case "multiCurveMember":
                case "multiSurfaceMember":
                    return true;

                default:
                    return false;
            }
        }

        /// <summary>Gets collection of features by passing a columnName and a specified columValue.</summary>
        /// <returns>Collection of features matching the columnValue.</returns>
        /// <param name="columnName">Column name  to match the column value.</param>
        /// <param name="columnValue">Column value to match those returning features.</param>
        /// <param name="returningColumnNames">Columns for each feature.</param>
        protected override Collection<Feature> GetFeaturesByColumnValueCore(string columnName, string columnValue, IEnumerable<string> returningColumnNames)
        {
            Collection<Feature> features = new Collection<Feature>();           
            foreach (Feature feature in _inMemoryFeatures)
            {
                if (feature.ColumnValues[columnName].ToString().Equals(columnValue))
                {
                    features.Add(feature.CloneDeep(returningColumnNames));
                }
            }

            return features;
        }

        private void LoadInMemoryFeatures()
        {
            _inMemoryFeatures = new Collection<Feature>();

            string queryString = GetFeatureQueryString(null);
            WebRequest request = WebRequest.Create(queryString);
            request.Timeout = TimeoutInSeconds * 1000;
            request.Proxy = _webProxy;

            Stream stream = request.GetResponse().GetResponseStream();
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreWhitespace = true;
            XmlReader reader = XmlReader.Create(stream, settings);

            while (!reader.EOF)
            {
                reader.Read();
                if (reader.LocalName == "featureMember" && reader.IsStartElement())
                {
                    _inMemoryFeatures.Add(GetFeature(reader, _columnsName));
                }
            }

            

        }
    }
}
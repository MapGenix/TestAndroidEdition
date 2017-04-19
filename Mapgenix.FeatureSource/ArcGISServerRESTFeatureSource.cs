using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mapgenix.Shapes;
using System.Net;
using Newtonsoft.Json;

namespace Mapgenix.FeatureSource
{
    /// <summary>FeatureSource for ArcGIS Rest API </summary>
    [Serializable]
    public class ArcGISServerRESTFeatureSource : BaseFeatureSource
    {
        #region private attributes

        /// <summary>
        /// Features columns
        /// </summary>
        [NonSerialized]
        private Collection<FeatureSourceColumn> _columns;

        /// <summary>
        /// Indicates when features are loaded
        /// </summary>
        [NonSerialized]
        private bool _featuresLoaded = false;

        /// <summary>
        /// Features load from wfs GetFeature request
        /// </summary>
        [NonSerialized]
        private Collection<Feature> _inMemoryFeatures;

        /// <summary>
        /// HTTP proxy for ArcGIS resquest
        /// </summary>
        [NonSerialized]
        private WebProxy _webProxy;

        /// <summary>
        /// Map service URL
        /// </summary>
        [NonSerialized]
        private Uri _marpServiceURL;

        /// <summary>
        /// Layer Id for rest request
        /// </summary>
        [NonSerialized]
        private string _layerId;

        /// <summary>
        /// ESRI Geometry type 
        /// </summary>
        [NonSerialized]
        private string _geometryType;

        /// <summary>
        /// Feature column id
        /// </summary>
        [NonSerialized]
        private string _columnId;

        #endregion

        #region public properties

        /// <summary>
        /// Feature columns
        /// </summary>
        public Collection<FeatureSourceColumn> Columns
        {
            get {  return _columns; }
        }

        /// <summary>
        /// Get feature columns names
        /// </summary>
        public Collection<string> ColumnsNames
        {
            get
            {
                Collection<string> columnsNames = new Collection<string>();

                if(_columns != null)
                {
                    foreach(FeatureSourceColumn column in _columns)
                    {
                        columnsNames.Add(column.ColumnName);
                    }
                }

                return columnsNames;
            }
        }

        /// <summary>
        /// HTTP proxy for ArcGIS resquest
        /// </summary>
        public WebProxy WebProxy
        {
            get {  return _webProxy; }
            set { _webProxy = value; }
        }

        /// <summary>
        /// Map service URL
        /// </summary>
        public Uri MapServiceURL
        {
            get { return _marpServiceURL; }
            set { _marpServiceURL = value; }
        }

        /// <summary>
        /// Layer Id for rest request
        /// </summary>
        public string LayerId
        {
            get { return _layerId; }
            set { _layerId = value; }
        }

        #endregion

        #region protected methods

        /// <summary>
        /// Call rest service and load all features
        /// </summary>
        protected override void OpenCore()
        {
            if(!_featuresLoaded)
            {
                _featuresLoaded = true;
                LoadEsriGeoJSON();                
            }
        }

        protected override void CloseCore()
        {
            // Empty method
        }

        #endregion

        #region private methods

        /// <summary>
        /// Load columns and features from ESRIGeoJson
        /// </summary>
        private void LoadEsriGeoJSON()
        {
            string restRequest = _marpServiceURL.ToString() + "/" + _layerId;
            restRequest += "?f=json";
            ArcGISServerRESTLayerInfo data = new ArcGISServerRESTLayerInfo();
            using (WebClient request = new WebClient())
            {
                var json = request.DownloadString(restRequest);
                data = JsonConvert.DeserializeObject<ArcGISServerRESTLayerInfo>(json);
                _geometryType = data.GeometryType;
                if(data.Fields.Count > 0)
                {
                    _columns = new Collection<FeatureSourceColumn>();
                    foreach (ArcGISServerRESTLayerField field in data.Fields)
                    {
                        FeatureSourceColumn column = new FeatureSourceColumn(field.Name, field.Type, 0);
                        _columns.Add(column);
                    }
                }

                if (data.Extent != null)
                {
                    this.LoadFeaturesId(data.Extent);
                }
            }

        }

        /// <summary>
        /// POST request for load features id by full layer extent
        /// </summary>
        /// <param name="extent">ArcGIS layer extent extent</param>
        private void LoadFeaturesId(ArcGISServerRESTLayerExtent extent)
        {

            string parameters = "f=json&geometryType=esriGeometryEnvelope&returnIdsOnly=true&geometry=" + JsonConvert.SerializeObject(extent);
            string restRequest = _marpServiceURL.ToString() + "/" + _layerId + "/query";

            using (WebClient request = new WebClient())
            {
                request.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                var json = request.UploadString(restRequest, parameters);
                ArcGISServerRESTFeaturesId featureIds = JsonConvert.DeserializeObject<ArcGISServerRESTFeaturesId>(json);
                _columnId = featureIds.ObjectIdFieldName;

                if (featureIds.ObjectIds.Count > 0)
                {
                    _inMemoryFeatures = new Collection<Feature>();
                    var featuresIdsResquest = featureIds.Split<int>(featureIds.ObjectIds);
                    foreach (List<int> featuresGroup in featuresIdsResquest)
                    {
                        LoadFeaturesInfo(featuresGroup);
                    }
                }
            }            
        }

        /// <summary>
        /// POST request for load all features info by features id
        /// </summary>
        /// <param name="featuresIds"></param>
        private void LoadFeaturesInfo(List<int> featuresIds)
        {
            string parameters = "f=json&returnGeometry=true&outFields=" + String.Join(",", ColumnsNames.ToArray()) + "&objectIds=" + String.Join(",", featuresIds.ToArray());
            string restRequest = _marpServiceURL.ToString() + "/" + _layerId + "/query";

            using (WebClient request = new WebClient())
            {
                request.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                var json = request.UploadString(restRequest, parameters);
                ArcGISServerRESTLayerFeatures layerFeatures = JsonConvert.DeserializeObject<ArcGISServerRESTLayerFeatures>(json);
                
                foreach (ArcGISServerRESTFeature arcGisFeature in layerFeatures.Features)
                {
                    string wkt = String.Empty;
                    switch (layerFeatures.GeometryType)
                    {
                        case "esriGeometryPoint":
                            wkt = GetWKTPoint(arcGisFeature.Geometry);
                            break;
                        case "esriGeometryMultipoint":
                            wkt = GetWKTMultiPoint(arcGisFeature.Geometry);
                            break;
                        case "esriGeometryLine":
                        case "esriGeometryPolyline":
                            wkt = GetWKTPolyLine(arcGisFeature.Geometry);
                            break;
                        case "esriGeometryPolygon":
                            wkt = GetWKTPolygon(arcGisFeature.Geometry);
                            break;
                    }

                    Feature feature = new Feature(wkt, arcGisFeature.Attributes[_columnId], arcGisFeature.Attributes);
                    _inMemoryFeatures.Add(feature);

                }
            }
        }

        /// <summary>
        /// Get point wkt from EsriGeometry object
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns></returns>
        private string GetWKTPoint(ArcGISServerRESTGeometry geometry)
        {
            return "POINT(" + geometry.X + " " + geometry.Y + ")";
        }

        /// <summary>
        /// Get multipoint wkt from EsriGeometry object
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns></returns>
        private string GetWKTMultiPoint(ArcGISServerRESTGeometry geometry)
        {
            string wkt = "MULTIPOINT(";

            foreach(double[] point in geometry.Points)
            {
                wkt += point[0] + " " + point[1] + ",";
            }

            wkt = wkt.Remove(wkt.Length - 1);

            wkt += ")";

            return wkt;
        }

        /// <summary>
        /// Get polyline wkt from EsriGeometry object
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns></returns>
        private string GetWKTPolyLine(ArcGISServerRESTGeometry geometry)
        {
            string wkt = "MULTILINESTRING(";            
            foreach (List<double[]> segments in geometry.Paths)
            {
                wkt += "(";
                foreach(double[] segment in segments)
                {
                    wkt += segment[0] + " " + segment[1] + ",";
                }
                wkt = wkt.Remove(wkt.Length - 1);
                wkt += "),";
            }

            wkt = wkt.Remove(wkt.Length - 1);
            wkt += ")";

            return wkt;
        }

        /// <summary>
        /// Get polygon wkt from EsriGeometry object
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns></returns>
        private string GetWKTPolygon(ArcGISServerRESTGeometry geometry)
        {
            string wkt = "MULTIPOLYGON(";

            foreach (List<double[]> rings in geometry.Rings)
            {
                wkt += "((";
                foreach (double[] ring in rings)
                {
                    wkt += ring[0] + " " + ring[1] + ",";
                }
                wkt = wkt.Remove(wkt.Length - 1);
                wkt += ")),";
            }

            wkt = wkt.Remove(wkt.Length - 1);

            wkt += "))";
        
            return wkt;
        }

        #endregion

        protected override RectangleShape GetBoundingBoxCore()
        {
            RectangleShape boundingBox = null;

            if (_inMemoryFeatures.Count > 0)
            {
                boundingBox = _inMemoryFeatures[0].GetBoundingBox();
            }

            return boundingBox;
        }

        protected override Collection<Feature> GetAllFeaturesCore(IEnumerable<string> returningColumnNames)
        {
            Collection<Feature> features = new Collection<Feature>();

            foreach (Feature feature in _inMemoryFeatures)
            {
                features.Add(feature.CloneDeep(returningColumnNames));
            }

            return features;
        }

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

        protected override Collection<FeatureSourceColumn> GetColumnsCore()
        {
            return _columns;
        }

    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Mapgenix.FeatureSource
{
    /// <summary>
    /// JSON entity for ArcGIS Server REST layerFeatures Object
    /// </summary>
    class ArcGISServerRESTLayerFeatures
    {
        [JsonProperty("displayFieldName")]
        public string DisplayFieldName
        {
            get; set;
        }

        [JsonProperty("features")]
        public IList<ArcGISServerRESTFeature> Features
        {
            get; set;
        }

        [JsonProperty("geometryType")]
        public string GeometryType
        {
            get; set;
        }
    }
}

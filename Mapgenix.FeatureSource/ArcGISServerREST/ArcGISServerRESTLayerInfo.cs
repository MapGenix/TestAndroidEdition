using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Mapgenix.FeatureSource
{
    /// <summary>
    /// JSON entity for ArcGIS Server REST LayerInfo object
    /// </summary>
    internal class ArcGISServerRESTLayerInfo
    {
        [JsonProperty("capabilities")]
        public string Capabilities
        {
            get; set;
        }

        [JsonProperty("displayField")]
        public string DisplayField
        {
            get; set;
        }

        [JsonProperty("id")]
        public string Id
        {
            get; set;
        }

        [JsonProperty("name")]
        public string Name
        {
            get; set;
        }

        [JsonProperty("geometryType")]
        public string GeometryType
        {
            get; set;
        }

        [JsonProperty("extent")]
        public ArcGISServerRESTLayerExtent Extent
        {
            get; set;
        }

        [JsonProperty("fields")]
        public IList<ArcGISServerRESTLayerField> Fields
        {
            get; set;
        }

    }
}

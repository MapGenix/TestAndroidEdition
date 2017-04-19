using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Mapgenix.FeatureSource
{
    /// <summary>
    /// JSON entity for ArcGIS Server REST layerField object
    /// </summary>
    internal class ArcGISServerRESTLayerField
    {
        [JsonProperty("alias")]
        public string Alias
        {
            get; set;
        }

        [JsonProperty("name")]
        public string Name
        {
            get; set;
        }

        [JsonProperty("type")]
        public string Type
        {
            get; set;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Mapgenix.FeatureSource
{
    /// <summary>
    /// JSON entity for ArcGIS Server REST LayerExtent object
    /// </summary>
    internal class ArcGISServerRESTLayerExtent
    {
        [JsonProperty("spatialReference")]
        public dynamic SpatialReference
        {
            get; set;
        }

        [JsonProperty("xmin")]
        public double xMin
        {
            get; set;
        }

        [JsonProperty("xmax")]
        public double xMax
        {
            get; set;
        }

        [JsonProperty("ymin")]
        public double yMin
        {
            get; set;
        }

        [JsonProperty("ymax")]
        public double yMax
        {
            get; set;
        }
    }
}

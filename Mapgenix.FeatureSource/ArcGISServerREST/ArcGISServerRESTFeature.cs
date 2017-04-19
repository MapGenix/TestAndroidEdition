using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Mapgenix.FeatureSource
{
    /// <summary>
    /// JSON entity for ArcGIS Server REST Feature object                                    
    /// </summary>
    internal class ArcGISServerRESTFeature
    {
        [JsonProperty("attributes")]
        public Dictionary<string, string> Attributes
        {
            get; set;
        }

        [JsonProperty("geometry")]
        public ArcGISServerRESTGeometry Geometry
        {
            get; set;
        }

    }
}

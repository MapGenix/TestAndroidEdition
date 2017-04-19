using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Mapgenix.FeatureSource
{
    /// <summary>
    /// JSON entity for ArcGIS Server REST Geometry object
    /// </summary>
    internal class ArcGISServerRESTGeometry
    {
        [JsonProperty("x")]
        public double X
        {
            get; set;
        }

        [JsonProperty("y")]
        public double Y
        {
            get; set;
        }

        [JsonProperty("points")]
        public IList<double[]> Points
        {
            get; set;
        }

        [JsonProperty("paths")]
        public IList<IList<double[]>> Paths
        {
            get; set;
        }

        [JsonProperty("rings")]
        public IList<IList<double[]>> Rings
        {
            get; set;
        }

    }
}

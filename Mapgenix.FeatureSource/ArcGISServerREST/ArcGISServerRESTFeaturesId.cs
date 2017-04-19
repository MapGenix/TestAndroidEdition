using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Mapgenix.FeatureSource
{
    /// <summary>
    /// JSON entity for ArcGIS Server REST FeaturesId object
    /// </summary>
    internal class ArcGISServerRESTFeaturesId
    {
        [JsonProperty("objectIdFieldName")]
        public string ObjectIdFieldName
        {
            get; set;
        }

        [JsonProperty("objectIds")]
        public IList<int> ObjectIds
        {
            get; set;
        }

        public IList<List<T>> Split<T>(IList<T> list)
        {
            return list.Select((x, i) => new { Index = i, Value = x }).GroupBy(x => x.Index / 1000).Select(x => x.Select(v => v.Value).ToList()).ToList();
        }
    }
}

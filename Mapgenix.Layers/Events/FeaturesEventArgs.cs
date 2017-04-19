using System;
using System.Collections.ObjectModel;
using Mapgenix.Shapes;

namespace Mapgenix.Layers
{
    /// <summary>Event arguments class for the feature events of the FeatureLayer.</summary>
    [Serializable]
    public class FeaturesEventArgs : EventArgs
    {
        public FeaturesEventArgs()
            : this(new Collection<Feature>())
        {
            Features = new Collection<Feature>();
        }
        
        public FeaturesEventArgs(Collection<Feature> _features)
        {
            this.Features = _features;
        }

        public Collection<Feature> Features { get; set; }
    }
}

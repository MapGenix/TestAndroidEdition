using System;
using System.Collections.ObjectModel;
using Mapgenix.FeatureSource;

namespace Mapgenix.Layers
{
    /// <summary>Layer for multiple feature layers.</summary>
    [Serializable]
    public class MultipleFeatureLayer : BaseFeatureLayer
    {
        public Collection<FeatureSource.BaseFeatureSource> FeatureSources
        {
            get { return ((MultipleFeatureSource)FeatureSource).FeatureSources; }
        }


    }
}
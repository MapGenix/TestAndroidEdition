using System;
using Mapgenix.Shapes;

namespace Mapgenix.FeatureSource
{
    
    [Serializable]
    public abstract class BaseGridInterpolationModel
    {
        public double Interpolate(RectangleShape cellExtent, GridDefinition gridDefinition)
        {
            return InterpolateCore(cellExtent,gridDefinition);
        }

        protected abstract double InterpolateCore(RectangleShape cellExtent, GridDefinition gridDefinition);
    }
}

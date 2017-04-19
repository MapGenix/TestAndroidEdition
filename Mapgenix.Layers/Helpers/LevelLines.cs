using System.Collections.Generic;
using System.Collections.ObjectModel;
using Mapgenix.FeatureSource;

namespace Mapgenix.Layers
{
    public class LevelLines
    {
        public LevelLines()
        {
            Lines = new List<Collection<GridCell>>();
        }

        public List<Collection<GridCell>> Lines { get; set; }

        public double LevelValue { set; get; }
    }
}

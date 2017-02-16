using System;
using Mapgenix.Shapes;

namespace Mapgenix.GSuite.Android
{
    [Serializable]
    public class FeatureEventArgs : EventArgs
    {
        private Feature _selectedFeature;

        public FeatureEventArgs(Feature selectedFeature)
        {
            this._selectedFeature = selectedFeature;
        }

        public Feature Feature
        {
            get { return _selectedFeature; }
            set { _selectedFeature = value; }
        }
    }
}
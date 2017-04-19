using Mapgenix.Canvas;

namespace Mapgenix.Layers
{
    internal struct GraticuleLabel
    {
        private string label;
        private ScreenPointF location;

        public GraticuleLabel(string label, ScreenPointF location)
        {
            this.label = label;
            this.location = location;
        }

        public string Label
        {
            get { return label; }
            
        }

        public ScreenPointF Location
        {
            get { return location; }
            
        }
    }
}

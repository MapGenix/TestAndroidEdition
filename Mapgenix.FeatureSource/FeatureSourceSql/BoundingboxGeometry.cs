namespace Mapgenix.FeatureSource
{
    public class BoundingboxGeometry
    {

        public int srid { get; set; }
        public double minX { get; set; }
        public double minY { get; set; }
        public double maxX { get; set; }
        public double maxY { get; set; }
        public byte mbrEnd { get; set; }

    }
}

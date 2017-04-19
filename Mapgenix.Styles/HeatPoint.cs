namespace Mapgenix.Styles
{
    internal struct HeatPoint
    {
        public byte Intensity;
        public int X;
        public int Y;

        public HeatPoint(int x, int y, byte intensity)
        {
            X = x;
            Y = y;
            Intensity = intensity;
        }
    }
}
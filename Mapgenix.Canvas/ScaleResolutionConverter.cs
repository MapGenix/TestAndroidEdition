using Mapgenix.Shapes;

namespace Mapgenix.Canvas
{
    public static class ScaleResolutionConverter
    {
        private const int DotsPerInch = 96;

        public static double GetResolutionFromScale(double scale, GeographyUnit mapUnit)
        {
            var resolution = scale/(GetInchesPerUnit(mapUnit)*DotsPerInch);

            return resolution;
        }

        private static double GetInchesPerUnit(GeographyUnit mapUnit)
        {
            double inchesPerUnit = 0;

            switch (mapUnit)
            {
                case GeographyUnit.DecimalDegree:
                    inchesPerUnit = 4374754;
                    break;
                case GeographyUnit.Feet:
                    inchesPerUnit = 12;
                    break;
                case GeographyUnit.Meter:
                    inchesPerUnit = 39.3701;
                    break;
                case GeographyUnit.Unknown:
                default:
                    break;
            }

            return inchesPerUnit;
        }
    }
}
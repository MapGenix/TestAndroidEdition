using System;
using Mapgenix.Layers.Properties;
using Mapgenix.Shapes;

namespace Mapgenix.Layers
{
    [Serializable]
    public static class PrinterHelper
    {
        public static double GetGeographyUnitToPointRatio(GeographyUnit mapUnit)
        {
            double rtn = 12 * 96;  
            switch (mapUnit)
            {
                case GeographyUnit.Unknown:
                    throw new ArgumentOutOfRangeException("mapUnit", ExceptionDescription.EnumerationOutOfRange);
                case GeographyUnit.DecimalDegree:
                    throw new ArgumentOutOfRangeException("mapUnit", ExceptionDescription.PrinterLayerGeographyUnitDoesNotSupported);
                case GeographyUnit.Feet:
                    rtn = 12 * 96;
                    break;
                case GeographyUnit.Meter:
                    rtn = 3.2808399 * 12 * 96;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("mapUnit", ExceptionDescription.EnumerationOutOfRange);
            }

            return rtn;
        }

        public static double ConvertLength(double length, PrintingUnit fromUnit, PrintingUnit toUnit)
        {
            double lengthInInch = 0;
            switch (fromUnit)
            {
                case PrintingUnit.Millimeter:
                    lengthInInch = length * 0.0393700787;
                    break;
                case PrintingUnit.Inch:
                    lengthInInch = length * 1;
                    break;
                case PrintingUnit.Meter:
                    lengthInInch = length * 39.3700787;
                    break;
                case PrintingUnit.Centimeter:
                    lengthInInch = length * 0.393700787;
                    break;
                case PrintingUnit.Point:
                    lengthInInch = length / 96.0;
                    break;
                default:
                    break;
            }

            double rtn = lengthInInch;
            switch (toUnit)
            {
                case PrintingUnit.Millimeter:
                    rtn = lengthInInch * 25.4;
                    break;
                case PrintingUnit.Inch:
                    rtn = lengthInInch * 1;
                    break;
                case PrintingUnit.Meter:
                    rtn = lengthInInch * 0.0254;
                    break;
                case PrintingUnit.Centimeter:
                    rtn = lengthInInch * 2.54;
                    break;
                case PrintingUnit.Point:
                    rtn = lengthInInch * 96;
                    break;

                default:
                    break;
            }

            return rtn;
        }
    }
}
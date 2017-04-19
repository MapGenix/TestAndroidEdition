using System;
using System.Collections.Generic;


namespace Mapgenix.Shapes
{
    /// <summary>Static class for conversion operations to go to and from different units.
    /// </summary>
    [Serializable]
    public static class Conversion
    {
        private static Dictionary<DistanceUnit, double> distanceDictionary;
        private static Dictionary<AreaUnit, double> areaDictionary;

        /// <summary>Converts from one unit of measure to another.</summary>
        /// <returns>Length in the unit specified in the toUnit parameter.</returns>
        /// <remarks>None</remarks>
        /// <param name="amount">Length in the fromUnit parameter.</param>
        /// <param name="fromUnit">Unit of length for the amount parameter.</param>
        /// <param name="toUnit">Unit of length for the result.</param>
        public static double ConvertMeasureUnits(double amount, DistanceUnit fromUnit, DistanceUnit toUnit)
        {
        
            if (distanceDictionary == null)
            {
                InitMapDistanceUnits();
            }

            return (amount * distanceDictionary[toUnit] / distanceDictionary[fromUnit]);
        }

        /// <summary>Converts from one area unit of measure to another.</summary>
        /// <returns>Area in the unit specified in the toUnit parameter.</returns>
        /// <remarks>None</remarks>
        /// <param name="amount">Area size in the unit specified in the fromUnit parameter.</param>
        /// <param name="fromUnit">Unit of measure for the area in the amount parameter.</param>
        /// <param name="toUnit">Unit of measure for the result.</param>
        public static double ConvertMeasureUnits(double amount, AreaUnit fromUnit, AreaUnit toUnit)
        {
      
            if (areaDictionary == null)
            {
                InitMapAreaUnits();
            }

            return (amount / areaDictionary[fromUnit] * areaDictionary[toUnit]);
        }

        /// <summary>Returns a DistanceUnit converted from a GeographyUnit.</summary>
        /// <returns>DistanceUnit converted from a GeographyUnit.</returns>
        /// <remarks>None</remarks>
        /// <param name="unit">GeographyUnit to convert.</param>
        public static DistanceUnit ConvertGeographyUnitToDistanceUnit(GeographyUnit unit)
        {
            Validators.CheckGeographyUnitIsValid(unit, "unit");

            switch (unit)
            {
                case GeographyUnit.Feet:
                    return DistanceUnit.Feet;
                case GeographyUnit.Meter:
                    return DistanceUnit.Meter;
                case GeographyUnit.DecimalDegree:
                default:
                    throw new ArgumentException(ExceptionDescription.InputGeometryTypeIsNotValid, "unit");
            }
        }

        /// <summary>Returns degrees from radians.</summary>
        /// <returns>Degrees from radians.</returns>
        public static double DegreesToRadians(float degrees)
        {
            return (degrees / 180) * Math.PI;
        }

        private static void InitMapDistanceUnits()
        {
            distanceDictionary = new Dictionary<DistanceUnit, double>(6);


            distanceDictionary[DistanceUnit.Feet] = 3.2808399;
            distanceDictionary[DistanceUnit.Kilometer] = 0.001;
            distanceDictionary[DistanceUnit.Meter] = 1;
            distanceDictionary[DistanceUnit.Mile] = 0.000621371192;
            distanceDictionary[DistanceUnit.UsSurveyFeet] = 3.28083333;
            distanceDictionary[DistanceUnit.Yard] = 1.0936133;
        }

        private static void InitMapAreaUnits()
        {
            areaDictionary = new Dictionary<AreaUnit, double>(8);


            areaDictionary[AreaUnit.Acres] = 0.000247105381;
            areaDictionary[AreaUnit.Hectares] = 0.0001;
            areaDictionary[AreaUnit.SquareFeet] = 10.7639104;
            areaDictionary[AreaUnit.SquareKilometers] = 0.000001;
            areaDictionary[AreaUnit.SquareMeters] = 1;
            areaDictionary[AreaUnit.SquareMiles] = 0.000000386102159;
            areaDictionary[AreaUnit.SquareUsSurveyFeet] = 10.7638674;
            areaDictionary[AreaUnit.SquareYards] = 1.19599005;
        }
    }
}

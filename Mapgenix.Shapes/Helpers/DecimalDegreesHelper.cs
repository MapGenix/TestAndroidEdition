using System;
using System.Globalization;
using Mapgenix.Utils;

namespace Mapgenix.Shapes
{
    /// <summary>Static class for operation with decimal degrees (planar to angular coordinate systems).</summary>
    [Serializable]
    public static class DecimalDegreesHelper
    {
        private const double earthRadius = 6367.0;
        private const double radiantFactor = Math.PI / 180;
        private const double degreeFactor = 180 / Math.PI;
        private const int SecondsInOneMinute = 60;

        /// <summary>Returns a degrees, minutes and seconds structure from a decimal degree value.</summary>
        /// <returns>Degrees, minutes and seconds structure from a decimal degree value.</returns>
        /// <param name="decimalDegreesValue">Decimal degree value to convert.</param>
        public static DegreesMinutesSeconds GetDegreesMinutesSecondsFromDecimalDegree(double decimalDegreesValue)
        {
            Validators.CheckIfInputValueIsInRange(decimalDegreesValue, "value", -180, RangeCheckingInclusion.IncludeValue, 180, RangeCheckingInclusion.IncludeValue);

            DegreesMinutesSeconds returnDms = new DegreesMinutesSeconds(); 
            double absoluteDecimalDegreeValue = Math.Abs(decimalDegreesValue);

            if (decimalDegreesValue >= 0)
            {
                returnDms.Degrees = (short)(Math.Floor(decimalDegreesValue));
            }
            else
            {
                returnDms.Degrees = (short)(Math.Ceiling(decimalDegreesValue));
            }

            int decimals = 10;
            double tmpMinutes = Math.Round((absoluteDecimalDegreeValue - Math.Floor(absoluteDecimalDegreeValue)) * SecondsInOneMinute, decimals);
            returnDms.Minutes = (int)Math.Floor(tmpMinutes);
            returnDms.Seconds = Math.Round((tmpMinutes - Math.Floor(tmpMinutes)) * SecondsInOneMinute, decimals);

            if (decimalDegreesValue < 0)
            {
                returnDms.Degrees = -Math.Abs(returnDms.Degrees);
                returnDms.Minutes = -Math.Abs(returnDms.Minutes);
                returnDms.Seconds = -Math.Abs(returnDms.Seconds);
            }

            return returnDms;
        }

        /// <summary>Returns a string representation in degrees, minutes and seconds from a decimal degree value.</summary>
        /// <returns>String representation in degrees, minutes and seconds from a decimal degree value.</returns>
       /// <param name="decimalDegreesValue">Decimal degrees value to convert.</param>
        public static string GetDegreesMinutesSecondsStringFromDecimalDegree(double decimalDegreesValue)
        {
            Validators.CheckIfInputValueIsInRange(decimalDegreesValue, "decimalDegreeValue", -180, RangeCheckingInclusion.IncludeValue, 180, RangeCheckingInclusion.IncludeValue);

            DegreesMinutesSeconds degreesMinutesSeconds = GetDegreesMinutesSecondsFromDecimalDegree(decimalDegreesValue);
            return degreesMinutesSeconds.ToString();
        }

        /// <summary>Returns a string representation in degrees, minutes and seconds from a decimal degree value.</summary>
        /// <returns>String representation in degrees, minutes and seconds from a decimal degree value.</returns>
        /// <param name="decimalDegreesValue">Decimal degrees value to convert.</param>
        /// <param name="decimals">Number of decimals for the second.</param>
        public static string GetDegreesMinutesSecondsStringFromDecimalDegree(double decimalDegreesValue, int decimals)
        {
            Validators.CheckIfInputValueIsInRange(decimalDegreesValue, "decimalDegreeValue", -180, RangeCheckingInclusion.IncludeValue, 180, RangeCheckingInclusion.IncludeValue);
            Validators.CheckIfInputValueIsBiggerThan(decimals, "decimals", 0, RangeCheckingInclusion.IncludeValue);

            DegreesMinutesSeconds dms = GetDegreesMinutesSecondsFromDecimalDegree(decimalDegreesValue);
            return dms.ToString(decimals);
        }

        /// <summary>Returns a string representation in degrees, minutes and seconds from a decimal degree value.</summary>
        /// <returns>String representation in degrees, minutes and seconds from a decimal degree value.</returns>
        /// <param name="pointShape">Point to convert.</param>
        public static string GetDegreesMinutesSecondsStringFromDecimalDegreePoint(PointShape pointShape)
        {
            Validators.CheckParameterIsNotNull(pointShape, "pointShape");
            Validators.CheckParameterIsValid(pointShape, "pointShape");

            return GetDegreesMinutesSecondsStringFromDecimalDegreePoint(pointShape, 0);
        }

        /// <summary>Returns a string representation in degrees, minutes and seconds from a point in decimal degree coordinates.</summary>
        /// <returns>String representation in degrees, minutes and seconds from a a point decimal degree coordinates.</returns>
        /// <param name="pointShape">Point to convert.</param>
        /// <param name="decimals">Number of decimals for the second.</param>
        public static string GetDegreesMinutesSecondsStringFromDecimalDegreePoint(PointShape pointShape, int decimals)
        {
            Validators.CheckParameterIsNotNull(pointShape, "pointShape");
            Validators.CheckParameterIsValid(pointShape, "pointShape");
            Validators.CheckIfInputValueIsBiggerThan(decimals, "decimals", 0, RangeCheckingInclusion.IncludeValue);

            string yStr = GetDegreesMinutesSecondsStringFromDecimalDegree(pointShape.Y, decimals);
            if (pointShape.Y >= 0)
            {
                yStr = yStr + "N";
            }
            else
            {
                // Kill the leading minus.
                yStr = yStr.Substring(1) + "S";
            }

            string xStr = GetDegreesMinutesSecondsStringFromDecimalDegree(pointShape.X, decimals);
            if (pointShape.X >= 0)
            {
                xStr = xStr + "E";
            }
            else
            {
                xStr = xStr.Substring(1) + "W";
            }

            string result = string.Format(CultureInfo.InvariantCulture, "{0} {1}", xStr, yStr);
            return result;
        }

        /// <summary>Returns a string representation in degrees, minutes and seconds from a feature in decimal degree coordinates.</summary>
        /// <returns>String representation in degrees, minutes and seconds from a a feature decimal degree coordinates.</returns>
        /// <param name="pointShape">Point to convert.</param>
        /// <param name="decimals">Number of decimals for the second.</param>
        public static string GetDegreesMinutesSecondsStringFromDecimalDegreePoint(Feature point, int decimalPlaces)
        {
            Validators.CheckIfInputValueIsBiggerThan(decimalPlaces, "decimals", 0, RangeCheckingInclusion.IncludeValue);

            BaseShape baseShape = point.GetShape();
            Validators.CheckShapeIsPointShape(baseShape);

            PointShape pointShape = (PointShape)baseShape;
            return GetDegreesMinutesSecondsStringFromDecimalDegreePoint(pointShape, decimalPlaces);
        }

        /// <summary>Returns a string representation in degrees, minutes and seconds from a feature in decimal degree coordinates.</summary>
        /// <returns>String representation in degrees, minutes and seconds from a a feature decimal degree coordinates.</returns>
        /// <param name="pointShape">Point to convert.</param>
        public static string GetDegreesMinutesSecondsStringFromDecimalDegreePoint(Feature point)
        {
            BaseShape baseShape = point.GetShape();
            Validators.CheckShapeIsPointShape(baseShape);

            PointShape pointShape = (PointShape)baseShape;
            return GetDegreesMinutesSecondsStringFromDecimalDegreePoint(pointShape, 0);
        }

        /// <summary>Returns a decimal degree value based on a string containing degrees, minutes, and seconds.</summary>
        /// <returns>Decimal degree value based on a string containing degrees, minutes, and seconds.</returns>
        /// <param name="degreesMinutesSeconds">Degrees, minutes and seconds in a string to convert.</param>
        public static double GetDecimalDegreeFromDegreesMinutesSeconds(string degreesMinutesSeconds)
        {
            Validators.CheckParameterIsNotNullOrEmpty(degreesMinutesSeconds, "degreesMinutesSecondsString");

            if (string.IsNullOrEmpty(degreesMinutesSeconds))
            {
                throw new ArgumentException(ExceptionDescription.DecimalDegreeSecondsStringNull, "degreesMinutesSeconds");
            }

            double returnDegree = double.MaxValue;

            int pos1 = degreesMinutesSeconds.IndexOf("?", StringComparison.Ordinal);
            if (pos1 >= 0)
            {
                double degree = double.Parse(degreesMinutesSeconds.Substring(0, pos1), CultureInfo.InvariantCulture);
                int pos2 = degreesMinutesSeconds.IndexOf("'", StringComparison.Ordinal);
                if (pos2 >= 0)
                {
                    double minute = double.Parse(degreesMinutesSeconds.Substring(pos1 + 2, 2), CultureInfo.InvariantCulture);
                    int pos3 = degreesMinutesSeconds.IndexOf("''", StringComparison.Ordinal);
                    if (pos3 == -1)
                    {
                        pos3 = degreesMinutesSeconds.IndexOf("\"", StringComparison.Ordinal);
                    }
                    if (pos3 >= 0)
                    {
                        double second = double.Parse(degreesMinutesSeconds.Substring(pos2 + 2, 2), CultureInfo.InvariantCulture);
                        double decMin = minute + second / 60.0;
                        if (degree >= 0)
                        {
                            returnDegree = degree + decMin / 60.0;
                        }
                        else
                        {
                            returnDegree = degree - decMin / 60.0;
                        }
                    }
                }
            }

            return returnDegree;
        }

        /// <summary>Returns a decimal degree value based on a degree, minute and second structure.</summary>
        /// <returns>Decimal degree value based on a degree, minute and second structure.</returns>
        /// <param name="degreesMinutesSeconds">Degrees, minutes and seconds to convert.</param>
        public static double GetDecimalDegreeFromDegreesMinutesSeconds(DegreesMinutesSeconds degreesMinutesSeconds)
        {
            return GetDecimalDegreeFromDegreesMinutesSeconds(degreesMinutesSeconds.Degrees, degreesMinutesSeconds.Minutes, degreesMinutesSeconds.Seconds);
        }

        /// <summary>Returns a decimal degree value based on a set of degrees, minutes, and seconds.</summary>
        /// <returns>Decimal degree value based on a set of degrees, minutes, and seconds.</returns>
        /// <param name="degrees">Degree component of the degrees, minutes and seconds.</param>
        /// <param name="minutes">Minute component of the degrees, minutes and seconds.</param>
        /// <param name="seconds">Second component of the degrees, minutes and seconds.</param>
        public static double GetDecimalDegreeFromDegreesMinutesSeconds(int degrees, int minutes, double seconds)
        {
            Validators.CheckIfInputValueIsInRange(degrees, "degrees", -180, RangeCheckingInclusion.IncludeValue, 180, RangeCheckingInclusion.IncludeValue);
            Validators.CheckIfInputValueIsInRange(minutes, "minutes", -60, RangeCheckingInclusion.IncludeValue, 60, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckIfInputValueIsInRange(seconds, "seconds", -60, RangeCheckingInclusion.IncludeValue, 60, RangeCheckingInclusion.ExcludeValue);

            double returnDegree = double.MaxValue;

            double decMin = minutes + seconds / 60.0;
            if (degrees >= 0)
            {
                returnDegree = degrees + decMin / 60.0;
            }
            else
            {
                returnDegree = degrees - decMin / 60.0;
            }

            return returnDegree;
        }

        /// <summary>Returns the distance between two points in decimal degrees.</summary>
        /// <returns>Distance between two points in decimal degrees in the unit specified by the returning Unit parameter.</returns>
        /// <remarks>None</remarks>
        /// <param name="fromPoint">Point shape to measure from.</param>
        /// <param name="toPoint">Point shape to measure to.</param>
        /// <param name="returningUnit">Distance unit to get the result back in.</param>
        public static double GetDistanceFromDecimalDegrees(PointShape fromPoint, PointShape toPoint, DistanceUnit returningUnit)
        {
            Validators.CheckParameterIsNotNull(fromPoint, "fromPoint");
            Validators.CheckParameterIsNotNull(toPoint, "toPoint");
            Validators.CheckParameterIsValid(fromPoint, "fromPoint");
            Validators.CheckParameterIsValid(toPoint, "toPoint");
            Validators.CheckDecimalDegreeLongitudeIsInRange(fromPoint.X, "fromPoint.X");
            Validators.CheckDecimalDegreeLatitudeIsInRange(fromPoint.Y, "fromPoint.Y");
            Validators.CheckDecimalDegreeLongitudeIsInRange(toPoint.X, "toPoint.X");
            Validators.CheckDecimalDegreeLatitudeIsInRange(toPoint.Y, "toPoint.Y");

            return GetDistanceFromDecimalDegrees(fromPoint.X, fromPoint.Y, toPoint.X, toPoint.Y, returningUnit);
        }

        /// <summary>Returns the distance between two features in decimal degrees.</summary>
        /// <returns>Distance between two features in decimal degrees in the unit specified by the returning Unit parameter.</returns>
        /// <remarks>None</remarks>
        /// <param name="fromPointFeature">Feature to measure from.</param>
        /// <param name="toPointFeature">Feature to measure to.</param>
        /// <param name="returningUnit">Distance unit to get the result back in.</param>
        public static double GetDistanceFromDecimalDegrees(Feature fromPointFeature, Feature toPointFeature, DistanceUnit returningUnit)
        {
            BaseShape fromShape = fromPointFeature.GetShape();
            BaseShape toShape = toPointFeature.GetShape();
            Validators.CheckShapeIsPointShape(fromShape);
            Validators.CheckShapeIsPointShape(toShape);

            return GetDistanceFromDecimalDegrees((PointShape)fromShape, (PointShape)toShape, returningUnit);
        }

        /// <summary>Returns the distance between two points in decimal degrees.</summary>
        /// <returns>Distance between two points in decimal degrees.</returns>
        /// <remarks>None</remarks>
        /// <param name="fromLongitude">From longitude value.</param>
        /// <param name="fromLatitude">From latitude value.</param>
        /// <param name="toLongitude">To longitude value.</param>
        /// <param name="toLatitude">To latitude value.</param>
        /// <param name="returningUnit">Distance unit for the return value.</param>
        public static double GetDistanceFromDecimalDegrees(double fromLongitude, double fromLatitude, double toLongitude, double toLatitude, DistanceUnit returningUnit)
        {
            Validators.CheckDecimalDegreeLongitudeIsInRange(fromLongitude, "fromLongitude");
            Validators.CheckDecimalDegreeLongitudeIsInRange(toLongitude, "toLongitude");
            Validators.CheckDecimalDegreeLatitudeIsInRange(fromLatitude, "fromLatitude");
            Validators.CheckDecimalDegreeLatitudeIsInRange(toLatitude, "toLatitude");
          
            double returnDistance = double.MaxValue;
            if (fromLongitude == toLongitude && fromLatitude == toLatitude)
            {
                returnDistance = 0.0;
            }
            else
            {
                if ((fromLongitude < -180 && toLongitude > 180) || (toLongitude < -180 && fromLongitude > 180)
                    || (fromLatitude < -90 && toLatitude > 90) || (toLatitude < -90 && fromLatitude > 90))
                {
                    fromLongitude = 0;
                    fromLatitude = 0;
                    toLongitude = 180;
                    toLatitude = 0;
                }
                else if (((fromLongitude - toLongitude) >= 360) || ((fromLongitude - toLongitude) <= -360
                    || ((toLongitude - fromLongitude) >= 360) || ((fromLongitude - toLongitude) <= -360)
                    || ((fromLatitude - toLatitude) >= 180) || ((fromLatitude - toLatitude) <= -180)
                    || ((toLatitude - fromLatitude) >= 180) || ((toLatitude - fromLatitude) >= 180)))
                {
                    fromLongitude = 0;
                    fromLatitude = 0;
                    toLongitude = 180;
                    toLatitude = 0;
                }
                var earthRadius = 6371; // km
                double radius = Conversion.ConvertMeasureUnits(earthRadius, DistanceUnit.Kilometer, returningUnit);

               
                var dLat = (toLatitude - fromLatitude) * Math.PI / 180;
                var dLon = (toLongitude - fromLongitude) * Math.PI / 180;
                var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) + Math.Cos(fromLatitude * Math.PI / 180) * Math.Cos(toLatitude * Math.PI / 180) * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
                var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
                returnDistance = radius * c;
            }
            return returnDistance;
        }

        /// <summary>Calculates the longitude difference change given a certain distance and latitude.</summary>
        /// <returns>Longitude difference.</returns>
        /// <remarks>None</remarks>
        /// <param name="distance">Distance to get the change in longitude.</param>
        /// <param name="distanceUnit">Unit of the distance.</param>
        /// <param name="latitude">Latitude the distance is measured at.</param>
        public static double GetLongitudeDifferenceFromDistance(double distance, DistanceUnit distanceUnit, double latitude)
        {
            Validators.CheckDecimalDegreeLatitudeIsInRange(latitude, "latitude");
            double distanceInKm = Conversion.ConvertMeasureUnits(distance, distanceUnit, DistanceUnit.Kilometer);
           
            Validators.CheckIfInputValueIsSmallerThan(distanceInKm, "distance", 40076, RangeCheckingInclusion.IncludeValue);

            if (distance == 0)
            {
                return 0.0;
            }

            if (distanceUnit != DistanceUnit.Meter)
            {
                distance = Conversion.ConvertMeasureUnits(distance, distanceUnit, DistanceUnit.Meter);
            }

            if (distance < 0.2)
            {
                distance = 0.2;
            }

            double earthRadiusUnit = Conversion.ConvertMeasureUnits(earthRadius, DistanceUnit.Kilometer, DistanceUnit.Meter);
            double radiantLat = latitude * radiantFactor;
            double a = distance / earthRadiusUnit;
            double angleCos = Math.Cos((Math.PI / 2) - radiantLat);
            double angleSin = Math.Sin((Math.PI / 2) - radiantLat);
            double dLongCos = (Math.Cos(a) - Math.Pow(angleCos, 2)) / Math.Pow(angleSin, 2);

            double result = Math.Abs(Math.Acos(dLongCos) * degreeFactor);
            if (double.IsNaN(result))
            {
                result = 360.0;
            }
            if (distance < 0)
            {
                result = -result;
            }

            return result;
        }

        /// <summary>Calculates the longitude difference change given a certain distance and longitude.</summary>
        /// <returns>Double representing the distance.</returns>
        /// <remarks>None</remarks>
        /// <param name="distance">Distance over which to get the change in longitude.</param>
        /// <param name="distanceUnit">Unit for the distance.</param>
        /// <param name="longitude">Longitude the distance is measured at.</param>
        public static double GetLatitudeDifferenceFromDistance(double distance, DistanceUnit distanceUnit, double longitude)
        {
            Validators.CheckDecimalDegreeLongitudeIsInRange(longitude, "longitude");
            double distanceInKm = Conversion.ConvertMeasureUnits(distance, distanceUnit, DistanceUnit.Kilometer);
         
            Validators.CheckIfInputValueIsSmallerThan(distanceInKm, "distance", 40076, RangeCheckingInclusion.IncludeValue);

            if (distance == 0.0)
            {
                return 0.0;
            }

            double distKm = Conversion.ConvertMeasureUnits(distance, distanceUnit, DistanceUnit.Kilometer);
            double difference = (distKm / earthRadius) * degreeFactor;

            return difference;
        }

        

        internal static double GetLatitudeFromDistanceAndDegree(double fromLongitude, double fromLatitude, double distance, DistanceUnit distanceUnit, double degree)
        {
            Validators.CheckDecimalDegreeLongitudeIsInRange(fromLongitude, "fromLongitude");
            Validators.CheckDecimalDegreeLatitudeIsInRange(fromLatitude, "fromLatitude");
            double kmUnit = Conversion.ConvertMeasureUnits(distance, distanceUnit, DistanceUnit.Kilometer);
            Validators.CheckIfInputValueIsSmallerThan(kmUnit, "distance", 40076, RangeCheckingInclusion.IncludeValue);

            double earthRadiusInDistanceUnit = Conversion.ConvertMeasureUnits(earthRadius, DistanceUnit.Kilometer, distanceUnit);
            double beta = degree * radiantFactor;
            double alpha = distance / earthRadiusInDistanceUnit;

            double resultLatitude = fromLatitude + (alpha * Math.Cos(beta)) * degreeFactor;
            return resultLatitude;
        }

        internal static double GetLongitudeFromDistanceAndDegree(double fromLongitude, double fromLatitude, double distance, DistanceUnit distanceUnit, double degree)
        {
            Validators.CheckDecimalDegreeLongitudeIsInRange(fromLongitude, "fromLongitude");
            Validators.CheckDecimalDegreeLatitudeIsInRange(fromLatitude, "fromLatitude");
            double kmUnit = Conversion.ConvertMeasureUnits(distance, distanceUnit, DistanceUnit.Kilometer);
            Validators.CheckIfInputValueIsSmallerThan(kmUnit, "distance", 40076, RangeCheckingInclusion.IncludeValue);

            double earthRadiusInDistanceUnit = Conversion.ConvertMeasureUnits(earthRadius, DistanceUnit.Kilometer, distanceUnit);
            double beta = degree * radiantFactor;
            double alpha = distance / earthRadiusInDistanceUnit;

            double resultLatitude = fromLatitude + (alpha * Math.Cos(beta)) * degreeFactor;

            double cosDLon = (Math.Cos(alpha) - (Math.Sin(fromLatitude * radiantFactor) * Math.Sin(resultLatitude * radiantFactor))) /
                (Math.Cos(fromLatitude * radiantFactor) * Math.Cos(resultLatitude * radiantFactor));

            double dLon = 0;
            if (cosDLon <= 1)
            {
                dLon = Math.Acos(cosDLon) * degreeFactor;
            }

            double resultLongitude = double.MaxValue;
            if (degree >= 0 && degree <= 180)
            {
                resultLongitude = fromLongitude + dLon;
            }
            else
            {
                resultLongitude = fromLongitude - dLon;
            }

            return resultLongitude;
        }

        internal static MultilineShape GetGreatCircle(PointShape fromPoint, PointShape toPoint, int count)
        {
            double newX = 0;
            double newY = 0;

            double long1 = fromPoint.X * radiantFactor;
            double lat1 = fromPoint.Y * radiantFactor;
            double long2 = toPoint.X * radiantFactor;
            double lat2 = toPoint.Y * radiantFactor;

            double x1 = Math.Cos(long1) * Math.Cos(lat1);
            double x2 = Math.Cos(long2) * Math.Cos(lat2);
            double y1 = Math.Sin(long1) * Math.Cos(lat1);
            double y2 = Math.Sin(long2) * Math.Cos(lat2);
            double z1 = Math.Sin(lat1);
            double z2 = Math.Sin(lat2);

            double alpha = Math.Acos((x1 * x2) + (y1 * y2) + (z1 * z2));

            if (alpha == 0)
            {
                return new MultilineShape(new LineShape[] { new LineShape(new Vertex[] { new Vertex(fromPoint), new Vertex(toPoint) }) });
            }

            double x3 = (x2 - (x1 * Math.Cos(alpha))) / Math.Sin(alpha);
            double y3 = (y2 - (y1 * Math.Cos(alpha))) / Math.Sin(alpha);
            double z3 = (z2 - (z1 * Math.Cos(alpha))) / Math.Sin(alpha);

            MultilineShape returnMultiLine = new MultilineShape();
            LineShape lineShape1 = new LineShape();
            LineShape lineShape2 = new LineShape();
            lineShape1.Vertices.Add(new Vertex(fromPoint.X, fromPoint.Y));
            bool isDisconnected = false;
            for (int i = 1; i <= count; i++)
            {
                double xbm = (x1 * Math.Cos((i * alpha) / (count + 1))) + (x3 * Math.Sin((i * alpha) / (count + 1)));
                double ybm = (y1 * Math.Cos((i * alpha) / (count + 1))) + (y3 * Math.Sin((i * alpha) / (count + 1)));
                double zbm = (z1 * Math.Cos((i * alpha) / (count + 1))) + (z3 * Math.Sin((i * alpha) / (count + 1)));
                if (ybm < 0 && xbm < 0)
                {
                    newX = Math.Atan(ybm / xbm) * degreeFactor - 180;
                }
                else if (ybm > 0 && xbm < 0)
                {
                    newX = Math.Atan(ybm / xbm) * degreeFactor + 180;
                }
                else
                {
                    newX = Math.Atan(ybm / xbm) * degreeFactor;
                }
                newY = Math.Asin(zbm) * degreeFactor;
                if ((i > 2) && (isDisconnected == false))
                {
                    double lastDistance = Math.Sqrt(Math.Pow((lineShape1.Vertices[i - 2].X - lineShape1.Vertices[i - 3].X), 2) + Math.Pow((lineShape1.Vertices[i - 2].Y - lineShape1.Vertices[i - 3].Y), 2));
                    double currentDistance = Math.Sqrt(Math.Pow((newX - lineShape1.Vertices[i - 2].X), 2) + Math.Pow((newY - lineShape1.Vertices[i - 2].Y), 2));
                    if (currentDistance > (lastDistance * 6))
                    {
                        isDisconnected = true;
                    }
                }
                if (isDisconnected == false)
                {
                    lineShape1.Vertices.Add(new Vertex(newX, newY));
                }
                else
                {
                    lineShape2.Vertices.Add(new Vertex(newX, newY));
                }
            }

            if (isDisconnected == false)
            {
                lineShape1.Vertices.Add(new Vertex(toPoint.X, toPoint.Y));
                returnMultiLine.Lines.Add(lineShape1);
            }
            else
            {
                lineShape2.Vertices.Add(new Vertex(toPoint.X, toPoint.Y));
                returnMultiLine.Lines.Add(lineShape1);
                returnMultiLine.Lines.Add(lineShape2);
            }
            return returnMultiLine;
        }

        internal static double GetDistanceFromDecimalDegreesLine(double fromPointX, double fromPointY, double toPointX, double toPointY, PointShape pointShape, DistanceUnit lengthUnit)
        {
            double pointShape1LineX = fromPointX;
            double pointShape1LineY = fromPointY;
            double pointShape2LineX = toPointX;
            double pointShape2LineY = toPointY;

            if (pointShape1LineY == pointShape2LineY)
            {
                pointShape1LineY = pointShape1LineY * 1.0000000001;
            }
            if (pointShape1LineX == pointShape2LineX)
            {
                pointShape1LineX = pointShape1LineX * 1.0000000001;
            }
            double maxX = Math.Max(pointShape1LineX, pointShape2LineX);
            double minX = Math.Min(pointShape1LineX, pointShape2LineX);

            double a = (pointShape2LineY - pointShape1LineY) / (pointShape2LineX - pointShape1LineX);
            double b = pointShape1LineY - (a * pointShape1LineX);
            double a2 = -1 / a;
            double b2 = pointShape.Y - a2 * pointShape.X;

            PointShape pointShapeI = new PointShape();
            pointShapeI.X = (b2 - b) / (a - a2);

            double distance = double.MaxValue;
            if (pointShapeI.X >= minX && pointShapeI.X <= maxX)
            {
                pointShapeI.Y = a * pointShapeI.X + b;
                distance = GetDistanceFromDecimalDegrees(pointShape, pointShapeI, lengthUnit);
            }
            else
            {
                double dist1 = GetDistanceFromDecimalDegrees(pointShape.X, pointShape.Y, fromPointX, fromPointY, lengthUnit);
                double dist2 = GetDistanceFromDecimalDegrees(pointShape.X, pointShape.Y, toPointX, toPointY, lengthUnit);
                distance = Math.Min(dist1, dist2);
            }
            return distance;
        }

        internal static PointShape GetNearestPointFromPointShapeDecimalDegreesLine(double fromPointX, double fromPointY, double toPointX, double toPointY, PointShape pointShape)
        {
            double pointShape1LineX = fromPointX;
            double pointShape1LineY = fromPointY;
            double pointShape2LineX = toPointX;
            double pointShape2LineY = toPointY;

            if (pointShape1LineY == pointShape2LineY)
            {
                pointShape1LineY = pointShape1LineY * 1.0000000001;
            }
            if (pointShape1LineX == pointShape2LineX)
            {
                pointShape1LineX = pointShape1LineX * 1.0000000001;
            }

            double maxX = Math.Max(pointShape1LineX, pointShape2LineX);
            double minX = Math.Min(pointShape1LineX, pointShape2LineX);

            double a = (pointShape2LineY - pointShape1LineY) / (pointShape2LineX - pointShape1LineX);
            double b = pointShape1LineY - (a * pointShape1LineX);
            double a2 = -1 / a;
            double b2 = pointShape.Y - a2 * pointShape.X;

            PointShape pointShapeI = new PointShape();
            pointShapeI.X = (b2 - b) / (a - a2);

            PointShape nearestPointShape = new PointShape();
            if (pointShapeI.X >= minX && pointShapeI.X <= maxX)
            {
                pointShapeI.Y = a * pointShapeI.X + b;
                nearestPointShape = pointShapeI;
            }
            else
            {
                double dist1 = GetDistanceFromDecimalDegrees(pointShape.X, pointShape.Y, fromPointX, fromPointY, DistanceUnit.Meter);
                double dist2 = GetDistanceFromDecimalDegrees(pointShape.X, pointShape.Y, toPointX, toPointY, DistanceUnit.Meter);
                if (dist1 < dist2)
                {
                    nearestPointShape = new PointShape(fromPointX, fromPointY);
                }
                else
                {
                    nearestPointShape = new PointShape(toPointX, toPointY);
                }
            }
            return nearestPointShape;
        }

        internal static double GetXFromDegreeOnSphere(double degreeX, double degreeY, DistanceUnit distanceUnit)
        {
            double distanceRadius = Conversion.ConvertMeasureUnits(earthRadius, DistanceUnit.Kilometer, distanceUnit);

            double X = distanceRadius * Math.Cos(degreeY * radiantFactor) * (degreeX * radiantFactor);
            if (degreeX < 0)
            {
                X = -X;
            }

            return X;
        }

        internal static double GetYFromDegreeOnSphere(double degreeY, DistanceUnit distanceUnit)
        {
            double distanceRadius = Conversion.ConvertMeasureUnits(earthRadius, DistanceUnit.Kilometer, distanceUnit);

            double Y = distanceRadius * degreeY * radiantFactor;
            if (degreeY < 0)
            {
                Y = -Y;
            }
            return Y;
        }
    }
}

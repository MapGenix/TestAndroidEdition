using System;
using System.Globalization;

namespace Mapgenix.GSuite.Android
{
    public static class CoordinateHelper
    {
        public static string ToDms(double coordinate)
        {
            coordinate = Math.Abs(coordinate);

            double d = Math.Floor(coordinate);
            coordinate -= d;
            coordinate *= 60;
            double m = Math.Floor(coordinate);
            coordinate -= m;
            coordinate *= 60;
            double s = Math.Round(coordinate);

            char pad;
            bool flag = char.TryParse("0", out pad);

            if (flag)
            {
                string dd = d.ToString(CultureInfo.InvariantCulture);
                string mm = m.ToString(CultureInfo.InvariantCulture).PadLeft(2, pad);
                string ss = s.ToString(CultureInfo.InvariantCulture).PadLeft(2, pad);

                string dms = string.Format(CultureInfo.InvariantCulture, "{0}°{1}'{2}\"", dd, mm, ss);
                return dms;
            }
            else
            {
                return "Invalid coordinate.";
            }
        }
    }
}

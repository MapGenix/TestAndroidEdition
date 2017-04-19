using System;
using System.Globalization;
using Mapgenix.Utils;

namespace Mapgenix.Shapes
{
    /// <summary>Structure for degrees, minutes and seconds value.</summary>
    /// <remarks>To represent decimal degree numbers as degrees, minutes and seconds.</remarks>
    [Serializable]
    public struct DegreesMinutesSeconds
    {
        private int degrees;
        private int minutes;
        private double seconds;

        /// <summary>To create an instance of the class by specifying the degree, minute and second values.</summary>
        public DegreesMinutesSeconds(int degrees, int minutes, double seconds)
        {
            Validators.CheckIfInputValueIsInRange(degrees, "degrees", -180, RangeCheckingInclusion.IncludeValue, 180, RangeCheckingInclusion.IncludeValue);
            Validators.CheckIfInputValueIsInRange(minutes, "minutes", 0, RangeCheckingInclusion.IncludeValue, 60, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckIfInputValueIsInRange(seconds, "seconds", 0, RangeCheckingInclusion.IncludeValue, 60, RangeCheckingInclusion.ExcludeValue);

            double decimalDegrees = DegreesMinutesSecondsToDecimalDegree(degrees, minutes, seconds);
            if (decimalDegrees > 180 || decimalDegrees < -180)
            {
                throw new ArgumentOutOfRangeException(ExceptionDescription.DegreesMinutesSecondsInputError);
            }

            this.degrees = degrees;
            this.minutes = minutes;
            this.seconds = seconds;
        }

        private static double DegreesMinutesSecondsToDecimalDegree(int degrees, int minutes, double seconds)
        {
            double DecDegrees;

            double DecMin = minutes + (seconds / 60);
            if (degrees >= 0)
            {
                DecDegrees = degrees + (DecMin / 60);
            }
            else
            {
                DecDegrees = degrees - (DecMin / 60);
            }

            return DecDegrees;
        }

        /// <summary>Gets or sets the degrees portion of the structure.</summary>
        public int Degrees
        {
            get
            {
                return degrees;
            }
            set
            {
                degrees = value;
            }
        }

        /// <summary>Gets or sets the minute portion of the structure.</summary>
        public int Minutes
        {
            get
            {
                return minutes;
            }
            set
            {
                minutes = value;
            }
        }

        /// <summary>Gets or sets the second portion of the structure.</summary>
        public double Seconds
        {
            get
            {
                return seconds;
            }
            set
            {
                seconds = value;
            }
        }

        /// <summary>Overrides the hash function for the particular type.</summary>
        public override int GetHashCode()
        {
            return degrees.GetHashCode() ^ minutes.GetHashCode() ^ seconds.GetHashCode();
        }

        /// <summary>Compares current Degrees Minutes Seconds with an object.</summary>
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (obj is DegreesMinutesSeconds)
            {
                return Equals((DegreesMinutesSeconds)obj);
            }
            else
            {
                return false;
            }
        }

        private bool Equals(DegreesMinutesSeconds compareObj)
        {
            return ((degrees == compareObj.degrees) &&
                    (seconds == compareObj.seconds) &&
                    (minutes == compareObj.minutes));
        }

        /// <summary>Add two Degrees Minutes Seconds values together and return back the sum of the two.</summary>
        public DegreesMinutesSeconds Add(DegreesMinutesSeconds targetDegreesMinutesSeconds)
        {
            return (this + targetDegreesMinutesSeconds);
        }

        /// <summary>Operation + overloads for Degrees Minutes Seconds. Sum of two Degrees Minutes Seconds.</summary>
        public static DegreesMinutesSeconds operator +(DegreesMinutesSeconds degreesMinutesSeconds1, DegreesMinutesSeconds degreesMinutesSeconds2)
        {
            int degrees = degreesMinutesSeconds1.Degrees + degreesMinutesSeconds2.Degrees;
            int minutes = degreesMinutesSeconds1.Minutes + degreesMinutesSeconds2.Minutes;
            double seconds = degreesMinutesSeconds1.seconds + degreesMinutesSeconds2.Seconds;

            int secondsMinute = System.Convert.ToInt32(System.Math.Floor((seconds / 60.0)));

            double newSeconds = seconds - secondsMinute * 60;
            int newMinutes = (minutes + secondsMinute) % 60;
            int newDegrees = degrees + (minutes + secondsMinute) / 60;

            return new DegreesMinutesSeconds(newDegrees, newMinutes, newSeconds);
        }

       
        public static bool operator ==(DegreesMinutesSeconds degreesMinutesSeconds1, DegreesMinutesSeconds degreesMinutesSeconds2)
        {
            return degreesMinutesSeconds1.Equals(degreesMinutesSeconds2);
        }

      
        public static bool operator !=(DegreesMinutesSeconds degreesMinutesSeconds1, DegreesMinutesSeconds degreesMinutesSeconds2)
        {
            return !(degreesMinutesSeconds1 == degreesMinutesSeconds2);
        }

      
        public override string ToString()
        {
            if (this.degrees < 0 || this.minutes < 0 || this.seconds < 0)
            {
                return string.Format(CultureInfo.InvariantCulture, "-{0:D2}º {1:D2}' {2:D2}''", Math.Abs(this.Degrees), Math.Abs(this.Minutes), Math.Abs(System.Convert.ToInt32(this.Seconds)));
            }
            else
            {
                return string.Format(CultureInfo.InvariantCulture, "{0:D2}º {1:D2}' {2:D2}''", Math.Abs(this.Degrees), Math.Abs(this.Minutes), Math.Abs(System.Convert.ToInt32(this.Seconds)));
            }
        }

     
        public string ToString(int decimals)
        {
            double newSeconds = Math.Round(this.seconds, decimals);

            if (this.degrees < 0 || this.minutes < 0 || this.seconds < 0)
            {
                return string.Format(CultureInfo.InvariantCulture, "-{0:D2}º {1:D2}' {2}''", Math.Abs(this.Degrees), Math.Abs(this.Minutes), Math.Abs(newSeconds));
            }
            else
            {
                return string.Format(CultureInfo.InvariantCulture, "{0:D2}º {1:D2}' {2}''", Math.Abs(this.Degrees), Math.Abs(this.Minutes), Math.Abs(newSeconds));
            }            
        }
    }
}

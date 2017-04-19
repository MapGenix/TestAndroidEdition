using System;

namespace ThinkGeo.MapSuite.Core 
{
    /// <summary>This structure represents a single point in screen coordinates.</summary>
    /// <remarks>None</remarks>
    [Serializable]
    public struct ScreenPointF
    {
        // TODO: why these two properties cannot set.
        private float x;
        private float y;

        /// <summary>
        /// This constructor allows you to create a screen point by passing in an X &amp;
        /// Y.
        /// </summary>
        /// <remarks>None</remarks>
        /// <param name="x">This parameter represents the horizontal value of a screen point.</param>
        /// <param name="y">This parameter represents the vertical value of a screen point.</param>
        public ScreenPointF(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        /// <summary>This property returns the X, or horizontal, value of the screen point.</summary>
        /// <value>This property returns the X, or horizontal, value of the screen point.</value>
        /// <example>
        /// None
        /// </example>
        public float X
        {
            get { return x; }
        }

        /// <summary>This property returns the Y, or vertical, value of the screen point.</summary>
        /// <value>This property returns the Y, or vertical, value of the screen point.</value>
        /// <remarks>None</remarks>
        public float Y
        {
            get { return y; }
        }

        /// <summary>This method is an override of the == functionality.</summary>
        /// <remarks>None</remarks>
        /// <exception cref="System.ArgumentNullException" caption="ArgumentNullException">If you pass a null as the sourceScreenPoint, we will throw an ArgumentNullException.</exception>
        /// <exception cref="System.ArgumentNullException" caption="ArgumentNullException">If you pass a null as the targetScreenPoint, we will throw an ArgumentNullException.</exception>
        /// <returns>None</returns>
        /// <param name="screenPointF1">This parameter is the first screenPoint to compare with.</param>
        /// <param name="screenPointF2">This parameter is the second screenPoint to compare width.</param>
        public static bool operator ==(ScreenPointF screenPointF1, ScreenPointF screenPointF2)
        {
            bool result = false;

            if ((screenPointF1.x == screenPointF2.x) && (screenPointF1.y == screenPointF2.y))
            {
                result = true;
            }

            return result;
        }

        /// <summary>This method is an override of the != functionality.</summary>
        /// <remarks>None</remarks>
        /// <param name="screenPointF1">This parameter represents the first GeoColor to compare.</param>
        /// <param name="screenPointF2">This parameter represents the second GeoColor to compare.</param>
        /// <exception cref="System.ArgumentNullException" caption="ArgumentNullException">If you pass a null as the sourceScreenPoint, we will throw an ArgumentNullException.</exception>
        /// <exception cref="System.ArgumentNullException" caption="ArgumentNullException">If you pass a null as the targetScreenPoint, we will throw an ArgumentNullException.</exception>
        public static bool operator !=(ScreenPointF screenPointF1, ScreenPointF screenPointF2)
        {
            return !(screenPointF1 == screenPointF2);
        }

        /// <summary>This method is an override of the Equals functionality.</summary>
        /// <returns>This method returns the Equals functionality.</returns>
        /// <remarks>None</remarks>
        /// <param name="obj">
        /// This parameter is the object you want to check to see if it is equal to the current
        /// instance.
        /// </param>
        /// <exception cref="System.ArgumentNullException" caption="ArgumentNullException">If you pass a null as the obj, we will throw an ArgumentNullException.</exception>
        public override bool Equals(object obj)
        {
            bool restult = false;

            if (obj == null)
            {
                restult = false;
            }

            if (obj is ScreenPointF)
            {
                restult = Equals((ScreenPointF)obj);
            }

            return restult;
        }

        private bool Equals(ScreenPointF compareObj)
        {
            return (x == compareObj.x) && (y == compareObj.y);
        }

        /// <summary>This method is an override of the GetHashCode functionality.</summary>
        /// <returns>This method returns the hash code.</returns>
        /// <remarks>None</remarks>
        public override int GetHashCode()
        {
            return (x.GetHashCode()) ^ (y.GetHashCode());
        }
    }
}

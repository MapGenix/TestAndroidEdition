using System;

namespace Mapgenix.Canvas
{
    /// <summary>Represents a single point in screen coordinates.</summary>
    [Serializable]
    public struct ScreenPointF
    {
        private readonly float _x;
        private readonly float _y;

        /// <summary>Creates a screen point by passing in an X and a Y.</summary>
        /// <remarks>None</remarks>
        /// <param name="x">X (horizontal) value of a screen point.</param>
        /// <param name="y">Y (vertical) value of a screen point.</param>
        public ScreenPointF(float x, float y)
        {
            _x = x;
            _y = y;
        }

        /// <summary>Returns the X (horizontal) value of the screen point.</summary>
        /// <value>X (horizontal) value of the screen point.</value>
        public float X
        {
            get { return _x; }
        }

        /// <summary>Returns the Y (vertical) value of the screen point.</summary>
        /// <value>Y (vertical) value of the screen point.</value>
        public float Y
        {
            get { return _y; }
        }

        /// <summary>Overrides of the == operator.</summary>
        /// <remarks>None</remarks>
        /// <returns>None</returns>
        /// <param name="screenPointF1">First screenPoint to compare with.</param>
        /// <param name="screenPointF2">Second screenPoint to compare width.</param>
        public static bool operator ==(ScreenPointF screenPointF1, ScreenPointF screenPointF2)
        {
            var result = false;

            if ((screenPointF1._x == screenPointF2._x) && (screenPointF1._y == screenPointF2._y))
            {
                result = true;
            }

            return result;
        }

        /// <summary>Overrides of the != operator.</summary>
        /// <remarks>None</remarks>
        /// <param name="screenPointF1">First GeoColor to compare with.</param>
        /// <param name="screenPointF2">Second GeoColor to compare with.</param>
        public static bool operator !=(ScreenPointF screenPointF1, ScreenPointF screenPointF2)
        {
            return !(screenPointF1 == screenPointF2);
        }

        /// <summary>Overrides of the Equals method.</summary>
        /// <returns>Boolean result of Equals method.</returns>
        /// <remarks>None</remarks>
        /// <param name="obj">Object to check if it is equal to the current instance.</param>
        public override bool Equals(object obj)
        {
            var restult = false;

            if (obj != null && obj is ScreenPointF)
            {
                restult = Equals((ScreenPointF) obj);
            }

            return restult;
        }

        private bool Equals(ScreenPointF compareObj)
        {
            return (_x == compareObj._x) && (_y == compareObj._y);
        }

        /// <summary>Overrides the GetHashCode method.</summary>
        /// <returns>Hash code.</returns>
        public override int GetHashCode()
        {
            return (_x.GetHashCode()) ^ (_y.GetHashCode());
        }
    }
}
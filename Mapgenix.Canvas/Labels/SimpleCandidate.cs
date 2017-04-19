using System;
using Mapgenix.Shapes;

namespace Mapgenix.Canvas
{
    /// <summary>For storing label information used in the PositionStyle.</summary>
    /// <remarks>Used in PositionStyle for storing simple label information.</remarks>
    [Serializable]
    public class SimpleCandidate
    {
        private string _orginalText;
        private PolygonShape _simplePolygon;


        /// <summary>Constructor of the class.</summary>
        /// <overloads>Default constructor. Setting manually the properties is necessary.</overloads>
        /// <returns>None</returns>
        /// <remarks>None/// </remarks>
        public SimpleCandidate() : this(string.Empty, new PolygonShape())
        {
        }

        /// <summary>Constructor of the class.</summary>
        /// <overloads>Passes in the original text and a simple polygon in screen coordinates.</overloads>
        /// <returns>None</returns>
        /// <remarks>None</remarks>
        /// <param name="originalText">Text of the label.</param>
        /// <param name="simplePolygonInScreenCoordinate">Polygon in screen coordinates representing the area of the label.</param>
        public SimpleCandidate(string originalText, PolygonShape simplePolygonInScreenCoordinate)
        {
            _orginalText = originalText;
            _simplePolygon = simplePolygonInScreenCoordinate;
        }

        /// <summary>Gets and sets the text of the label.</summary>
        /// <value>Text of the label.</value>
        /// <remarks>None</remarks>
        public string OriginalText
        {
            get { return _orginalText; }
            set { _orginalText = value; }
        }

        /// <summary>Gets and sets the polygon in screen coordinates representing the area of the label.</summary>
        /// <value>Polygon in screen coordinates representing the area of the label.</value>
        /// <remarks>None</remarks>
        public PolygonShape SimplePolygonInScreenCoordinate
        {
            get { return _simplePolygon; }
            set { _simplePolygon = value; }
        }
    }
}
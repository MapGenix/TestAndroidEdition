using System;
using Mapgenix.Shapes;

namespace Mapgenix.Canvas
{
    /// <summary>Location information of a piece of a label.</summary>
    /// <remarks>Used in the labeling logic, specifically for the labeling
    /// candidate. Represents an entire label or, in the case of label spline a single character in the label.</remarks>
    [Serializable]
    public class LabelInformation
    {
        private PointShape _positionInScreenCoordinates;
        private double _rotationAngle;
        private string _text;

        /// <summary>Default constructor of the class.</summary>
        /// <remarks>Setting the various properties manually is necessary.</remarks>
        public LabelInformation()
            : this(new PointShape(0, 0), string.Empty, 0)
        {
        }

        /// <summary>Constructor passing position in screen, text and rotation angle.</summary>
        /// <overloads>This constructor setting all the properties.</overloads>
        /// <remarks>None</remarks>
        /// <returns>None</returns>
        /// <param name="positionInScreenCoordinates">Position of the label to draw.</param>
        /// <param name="text">Ttext of the label.</param>
        /// <param name="rotationAngle">Angle of rotation for the label.</param>
        public LabelInformation(PointShape positionInScreenCoordinates, string text, double rotationAngle)
        {
            _positionInScreenCoordinates = positionInScreenCoordinates;
            _text = text;
            _rotationAngle = rotationAngle;
        }

        /// <summary>Gets and sets the position of a piece of a label in screen coordinates.</summary>
        /// <value>Position of a piece of a label in screen coordinates.</value>
        public PointShape PositionInScreenCoordinates
        {
            get { return _positionInScreenCoordinates; }
            set { _positionInScreenCoordinates = value; }
        }

        /// <summary>Gets and sets the text of the label.</summary>
        /// <value>Text of the label.</value>
        /// <remarks>Text can be the entire label text or a single character.</remarks>
        public string Text
        {
            get { return _text; }
            set { _text = value; }
        }

        /// <summary>Gets and sets the angle of rotation of label.</summary>
        /// <value>Angle of rotation of label.</value>
        /// <remarks>Rotation of the entire label text, a single character, or a set of characters.</remarks>
        public double RotationAngle
        {
            get { return _rotationAngle; }
            set { _rotationAngle = value; }
        }
    }
}
using System;
using System.Collections.ObjectModel;
using Mapgenix.Shapes;

namespace Mapgenix.Canvas
{
    /// <summary>A candidate for labeling.</summary>
    /// <remarks>Used in the internals of the labeling system for keeping track every potential label according to labeling rules.</remarks>
    [Serializable]
    public class LabelingCandidate
    {
        private readonly Collection<LabelInformation> _labelInformation;
        private PointShape _centerPointInScreenCoordinates;
        private string _originalText;
        private PolygonShape _screenArea;

        /// <summary>Default constructor of the class.</summary>
        /// <remarks>Setting the properties manually is necessary.</remarks>
        public LabelingCandidate()
            : this(string.Empty, new PolygonShape(), new PointShape(0, 0))
        {
        }

        /// <summary>Constructor of the class.</summary>
        /// <overloads>Passes the text, area of the label and the center point.</overloads>
        /// <returns>None</returns>
        /// <param name="originalText">Text of the label before possible modification occurs.</param>
        /// <param name="simplePolygonInScreenCoordinates">Polygon (in screen coordinates) representing the area to be labeled.</param>
        /// <param name="centerPointInScreenCoordinates">Center of the polygon (in screen coordinates) representing the area to be labeled.</param>
        public LabelingCandidate(string originalText, PolygonShape simplePolygonInScreenCoordinates,
            PointShape centerPointInScreenCoordinates)
            : this(
                originalText, simplePolygonInScreenCoordinates, centerPointInScreenCoordinates,
                new Collection<LabelInformation>())
        {
        }

        /// <summary>Constructor of the class.</summary>
        /// <overloads>Passes the text, area of the label, center point and label information.</overloads>
        /// <returns>None</returns>
        /// <remarks>None</remarks>
        /// <param name="originalText">Text of the label before possible modification occurs.</param>
        /// <param name="simplePolygonInScreenCoordinates">Polygon (in screen coordinates) representing the area to be labeled.</param>
        /// <param name="centerPointInScreenCoordinates">Center of the polygon (in screen coordinates) representing the area to be labeled.</param>
        /// <param name="labelInformation">Labeling information for the labeling candidate.</param>
        public LabelingCandidate(string originalText, PolygonShape simplePolygonInScreenCoordinates,
            PointShape centerPointInScreenCoordinates, Collection<LabelInformation> labelInformation)
        {
            _originalText = originalText;
            _screenArea = simplePolygonInScreenCoordinates;
            _centerPointInScreenCoordinates = centerPointInScreenCoordinates;
            _labelInformation = labelInformation;
        }

        /// <summary>Gets and sets the original text of the label.</summary>
        /// <value>Original text of the label.</value>
        /// <remarks>None</remarks>
        public string OriginalText
        {
            get { return _originalText; }
            set { _originalText = value; }
        }

        /// <summary>Gets and sets the screen area encompassing the label.</summary>
        /// <value>Screen area encompassing the label.</value>
        /// <remarks>None</remarks>
        public PolygonShape ScreenArea
        {
            get { return _screenArea; }
            set { _screenArea = value; }
        }

        /// <summary>Gets and sets the center point for the center of the label.</summary>
        /// <value>Center point for the center of the label.</value>
        /// <remarks>None</remarks>
        public PointShape CenterPoint
        {
            get { return _centerPointInScreenCoordinates; }
            set { _centerPointInScreenCoordinates = value; }
        }

        /// <summary>Gets and sets the label information for the label.</summary>
        /// <value>Label information for this label.</value>
        /// <remarks>None</remarks>
        public Collection<LabelInformation> LabelInformation
        {
            get { return _labelInformation; }
        }
    }
}
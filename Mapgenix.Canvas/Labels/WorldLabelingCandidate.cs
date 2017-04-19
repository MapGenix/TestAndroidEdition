using System;
using Mapgenix.Shapes;

namespace Mapgenix.Canvas
{
    /// <summary>Labeling candidate with center point information in world coordinates.</summary>
    /// <remarks>Used in the labeling system keeping track of every potential label according to labeling rules.</remarks>
    [Serializable]
    public class WorldLabelingCandidate : LabelingCandidate
    {
        private PointShape _centerPointInWorldCoordinates;

        /// <summary>Default constructor of the class.</summary>
        /// <returns>None</returns>
        /// <remarks>Properties need to be set manually.</remarks>
        public WorldLabelingCandidate()
            : this(string.Empty, null)
        {
        }

        /// <summary>Constructor of the class.</summary>
        /// <overloads>Passes in the text of the label.</overloads>
        /// <returns>None</returns>
        /// <remarks>None</remarks>
        /// <param name="originalText">Text of the label before possible modification occurs.</param>
        public WorldLabelingCandidate(string originalText)
            : this(originalText, null)
        {
        }

        /// <summary>Constructor of the class.</summary>
        /// <overloads>Passes in the text of the label and the center point in world coordinates.</overloads>
        /// <returns>None</returns>
        /// <remarks>None</remarks>
        /// <param name="originalText">Text of the label before possible modification occurs.</param>
        /// <param name="centerPointInWorldCoordinates">Center of the polygon (in world coordinates) representing the area to be labeled.</param>
        public WorldLabelingCandidate(string originalText, PointShape centerPointInWorldCoordinates)
        {
            OriginalText = originalText;
            _centerPointInWorldCoordinates = centerPointInWorldCoordinates;
        }

        /// <summary>Gets or sets the center point position in world coordinates.</summary>
        public PointShape CenterPointInWorldCoordinates
        {
            get { return _centerPointInWorldCoordinates; }
            set { _centerPointInWorldCoordinates = value; }
        }
    }
}
using System;

namespace Mapgenix.Canvas
{
    /// <summary>Represents the custom drawing options for point styles.</summary>
    /// <remarks>Allows to set the CustomBrush to use custom brushes to achieve effects such as linear gradients and bitmap fills.</remarks>
    [Serializable]
    public class PointStyleCustom
    {
        private BaseGeoBrush _customBrush;

        /// <summary>Default constructor of the class.</summary>
        /// <remarks>None</remarks>
        public PointStyleCustom()
        {
        }

        /// <summary>Gets and sets a custom brush.</summary>
        /// <value>Custom brush.</value>
        /// <remarks>Allows to use custom brushes to achieve effects such as linear gradients and bitmap fills.</remarks>
        public BaseGeoBrush CustomBrush
        {
            get { return _customBrush; }
            set { _customBrush = value; }
        }
    }
}
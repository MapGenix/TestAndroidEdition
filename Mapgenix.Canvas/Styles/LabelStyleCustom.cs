using System;

namespace Mapgenix.Canvas
{
    /// <summary>Represents the custom drawing options for label style.</summary>
    /// <remarks>Allows to use custom brushes to achieve effects such as linear gradients and bitmap fills on label.
    [Serializable]
    public class LabelStyleCustom
    {
        private BaseGeoBrush _textCustomBrush;

        /// <summary>Gets and sets a custom  brush.</summary>
        /// <value>Custom brush.</value>
        /// <remarks>Allows  to use custom brushes to achieve effects such as linear gradients and bitmap fills.</remarks>
        public BaseGeoBrush TextCustomBrush
        {
            get { return _textCustomBrush; }
            set { _textCustomBrush = value; }
        }
    }
}
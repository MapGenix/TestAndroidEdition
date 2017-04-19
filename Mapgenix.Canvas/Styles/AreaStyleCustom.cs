using System;

namespace Mapgenix.Canvas
{
    /// <summary>Represents the custom drawing options for area styles.</summary>
    /// <remarks>Allows to set the FillCustomBrush to use custom brushes to achieve effects such as linear gradients and bitmap fills.</remarks>
    [Serializable]
    public class AreaStyleCustom
    {
        private BaseGeoBrush _fillCustomBrush;

        /// <summary>Gets and sets a custom fill brush.</summary>
        /// <value>Custom fill brush.</value>
        /// <remarks>Allows to use custom brushes to achieve effects such as linear gradients and bitmap fills.</remarks>
        public BaseGeoBrush FillCustomBrush
        {
            get { return _fillCustomBrush; }
            set { _fillCustomBrush = value; }
        }

        /// <summary>Creates a copy of AreaStyleAdvanced class using the deep clone technique.</summary>
        /// <returns>Cloned AreaStyleAdvanced.</returns>
        /// <remarks>Deep cloning copies the cloned object and all the objects within it.</remarks>
        public AreaStyleCustom CloneDeep()
        {
            var areaStyleCustom = new AreaStyleCustom();
            if (FillCustomBrush != null)
            {
                areaStyleCustom.FillCustomBrush = FillCustomBrush.CloneDeep();
            }
            return areaStyleCustom;
        }
    }
}
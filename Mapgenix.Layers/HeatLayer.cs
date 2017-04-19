using System;
using System.Collections.ObjectModel;
using Mapgenix.Canvas;
using Mapgenix.Styles;

namespace Mapgenix.Layers
{
    /// <summary>Layer for displaying spatial data as heat map.</summary>
    [Serializable]
    public class HeatLayer : BaseLayer
    {
        /// <summary>Gets or sets the feature source for the HeatLayer.</summary>
        /// <value>Feature source for the HeatLayer.</value>
        public FeatureSource.BaseFeatureSource FeatureSource { get; set; }

        /// <summary>Gets or sets the style of <strong>HeatLayer</strong>.</summary>
        public HeatStyle HeatStyle { get; set; }

        /// <summary>Gets or sets the upper threshold of the layer.</summary>
        public double UpperScale { get; set; }

        /// <summary>Gets or sets the lower threshold of the layer.</summary>
        public double LowerScale { get; set; }

        /// <summary>Opens the Layer to have it ready to use.</summary>
        /// <remarks>Abstract method called from the concrete public method Open.</remarks>
        /// <returns>None</returns>
        protected override void OpenCore()
        {
            base.OpenCore();
            FeatureSource.Open();
        }

        /// <summary>Closes the Layer and releases any resources used.</summary>
        /// <returns>None</returns>
        /// <remarks>Abstract method called from the concrete public method Close.</remarks>
        protected override void CloseCore()
        {
            base.CloseCore();
            FeatureSource.Close();
        }

        /// <summary>Draws the Layer.</summary>
        /// <remarks>Concrete wrapper of the abstract method DrawCore.</remarks>
        /// <returns>None</returns>
        /// <param name="canvas">Canvas to draw on.</param>
        /// <param name="labelsInAllLayers">Labels in all the layers.</param>
        protected override void DrawCore(BaseGeoCanvas canvas, Collection<SimpleCandidate> labelsInAllLayers)
        {
			DrawHelper.Draw(this, canvas, labelsInAllLayers);
        }
    }
}
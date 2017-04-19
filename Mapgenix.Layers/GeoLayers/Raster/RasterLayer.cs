using System;
using System.Collections.ObjectModel;
using Mapgenix.Canvas;
using Mapgenix.Shapes;
using Mapgenix.RasterSource;

namespace Mapgenix.Layers
{
    /// <summary>Abstract class for raster layers</summary>
    [Serializable]
    public class RasterLayer : BaseLayer
    {
        double _upperThreshold;
        double _lowerThreshold;
        BaseRasterSource _imageSource;
        GeoColor _keyColor;
        Collection<GeoColor> _keyColors;

        /// <summary>Constructor for the class. You need to set the properties manually.</summary>
        /// <returns>None</returns>
        public RasterLayer()
        {
            _upperThreshold = double.MaxValue;
            _lowerThreshold = double.MinValue;

            _keyColor = GeoColor.StandardColors.Transparent;
            _keyColors = new Collection<GeoColor>();
        }

        /// <summary>Gets and sets the source used by the RasterLayer.</summary>
        /// <value>Source used by the RasterLayer.</value>
        public BaseRasterSource ImageSource
        {
            get { return _imageSource; }
            set { _imageSource = value; }
        }

        /// <summary>Gets and sets the upper threshold of the scale to display the image.</summary>
        /// <value>Upper threshold of the scale to display the image.</value>
        public double UpperThreshold
        {
            get { return _upperThreshold; }
            set { _upperThreshold = value; }
        }

        /// <summary>Gets and sets the lower threshold of the scale to display the image.</summary>
        /// <value>Lower threshold of the scale to display the image.</value>
        public double LowerThreshold
        {
            get { return _lowerThreshold; }
            set { _lowerThreshold = value; }
        }

        /// <summary>Gets or set the key color of the Raster image.</summary>
        public GeoColor KeyColor
        {
            get { return _keyColor; }
            set { _keyColor = value; }
        }

        /// <summary>Gets or sets a collection of key colors. If HasKeyColor property is false, it throws exception.</summary>
        /// <remarks>It makes these colors transparent.</remarks>
        public Collection<GeoColor> KeyColors
        {
            get { return _keyColors; }
            set { _keyColors = value; }
        }

        /// <summary>Returns true if the RasterLayer is open and false if it is not.</summary>
        /// <value>True if the RasterLayer is open and false if it is not.</value>
        protected override bool IsOpenCore
        {
            get { return _imageSource.IsOpen; }
        }

        /// <summary>Returns the bounding box of the RasterLayer.</summary>
        /// <returns>Bounding box of the RasterLayer.</returns>
        protected override RectangleShape GetBoundingBoxCore()
        {
            Validators.CheckLayerIsOpened(IsOpen);
            Validators.CheckRasterSourceIsOpen(_imageSource.IsOpen);

            return _imageSource.GetBoundingBox();
        }

        /// <summary>Opens the RasterLayer to have it ready to use.</summary>
        /// <returns>None</returns>
        protected override void OpenCore()
        {
            ImageSource.Open();
        }

        /// <summary>Closes the RasterLayer. </summary>
        /// <returns>None</returns>
        protected override void CloseCore()
        {
            ImageSource.Close();
        }

        /// <summary>Draws the image from the GeoImage source based on the parameters provided.</summary>
        /// <returns>None</returns>
        /// <param name="canvas">GeoCanvas to Draw the RasterLayer on.</param>
		protected override void DrawCore(BaseGeoCanvas canvas, Collection<SimpleCandidate> labelsInAllLayers)
		{
			RasterDrawHelper.DrawRaster(this, canvas);
		}
        
    }
}

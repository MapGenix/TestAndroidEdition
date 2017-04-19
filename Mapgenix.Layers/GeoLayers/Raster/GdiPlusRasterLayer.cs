using System;
using System.Collections.ObjectModel;
using System.Drawing.Drawing2D;
using Mapgenix.Canvas;

namespace Mapgenix.Layers
{
    /// <summary>Non GIS image types to be drawn on the map, such as .BMP, .JPG, .PNG, etc.</summary>
    /// <remarks>The raster layer needs a world file associated with it. The world file contains
    /// coordinates info.</remarks>
    [Serializable]
    public class GdiPlusRasterLayer : RasterLayer
    {
        public InterpolationMode InterpolationMode { get; set; }

        /// <summary>Draws the image from the GeoImage source based on the parameters provided.</summary>
        /// <returns>None</returns>
        /// <param name="canvas">GeoCanvas used to Draw the RasterLayer.</param>
        /// <param name="labelsInAllLayers">Not used for raster layers.</param>
        protected override void DrawCore(BaseGeoCanvas canvas, Collection<SimpleCandidate> labelsInAllLayers)
        {
            GdiPlusGeoCanvas gdiPlusGeoCanvas = canvas as GdiPlusGeoCanvas;
            if (gdiPlusGeoCanvas != null)
            {
                gdiPlusGeoCanvas.InterpolationMode = InterpolationMode;
            }

			RasterDrawHelper.DrawRaster(this, canvas);
        }
    }
}
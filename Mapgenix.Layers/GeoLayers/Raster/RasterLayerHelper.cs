using Mapgenix.Canvas;
using Mapgenix.Shapes;

namespace Mapgenix.Layers
{
    /// <summary>Static class for operations on raster layers.</summary>
    public static class RasterLayerHelper
	{
        /// <summary>Rreturns whether the current extent is within the upper and lower thresholds.</summary>
        /// <returns>Whether the current extent is within the upper and lower thresholds.</returns>
        /// <param name="currentExtent">Current extent.</param>
        /// <param name="upperThreshold">Upper threshold to test the extent.</param>
        /// <param name="lowerThreshold">Lower threshold to test the extent.</param>
        /// <param name="canvasWidth">Width of the canvas.</param>
        /// <param name="mapUnit">Unit of the map.</param>
        /// <param name="dpi">Dpi (Dot per inch).</param>
		public static bool IsExtentWithinThreshold(RectangleShape currentExtent, double upperThreshold, double lowerThreshold, int canvasWidth, GeographyUnit mapUnit, float dpi)
		{
			Validators.CheckGeographyUnitIsValid(mapUnit, "mapUnit");
			double currentScale = ExtentHelper.GetScale(currentExtent, canvasWidth, mapUnit, dpi);

			bool isValid = false;

			if (currentScale <= upperThreshold && currentScale >= lowerThreshold)
			{
				isValid = true;
			}

			return isValid;
		}
	}
}

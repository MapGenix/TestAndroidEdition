using Mapgenix.Canvas;
using Mapgenix.RasterSource;
using System.Collections.ObjectModel;


namespace Mapgenix.Layers
{
    /// <summary>
    /// Factory class for raster layers.
    /// </summary>
    public static class RasterLayerFactory
    {
        
        public static GdiPlusRasterLayer CreateGdiPlusRasterLayer(string pathFilename)
        {
            return new GdiPlusRasterLayer
            {
                UpperThreshold = double.MaxValue,
                LowerThreshold = double.MinValue,
                KeyColor = GeoColor.StandardColors.Transparent,
                KeyColors = new Collection<GeoColor>(),
                ImageSource = RasterSourceFactory.CreateGdiPlusRasterSource(pathFilename),
                Name = string.Empty,
				HasBoundingBox = true,
                IsVisible = true
            };
        }

        public static GdiPlusRasterLayer CreateImageRasterLayer(string pathFilename)
        {
            return new GdiPlusRasterLayer
            {
                UpperThreshold = double.MaxValue,
                LowerThreshold = double.MinValue,
                KeyColor = GeoColor.StandardColors.Transparent,
                KeyColors = new Collection<GeoColor>(),
                ImageSource = RasterSourceFactory.CreateImageRasterSource(pathFilename),
                Name = string.Empty,
                HasBoundingBox = true,
                IsVisible = true
            };
        }


    }
}

using System.Collections.ObjectModel;
using Mapgenix.Shapes;
using Mapgenix.Canvas;
using Mapgenix.Styles;

namespace Mapgenix.Layers
{
    /// <summary>Static class for operations on zoom levels.</summary>
    public static class ZoomLevelHelper
	{
		public static int GetCurrentZoomLevelNumber(double newWidth, RectangleShape newTileExtent, GeographyUnit mapUnit, float dpi)
		{
			GoogleMapZoomLevelSet zoomlevelSet = new GoogleMapZoomLevelSet();
			ZoomLevel zoomlevel = zoomlevelSet.GetZoomLevel(newTileExtent, newWidth, mapUnit, dpi);
			Collection<ZoomLevel> zoomlevels = zoomlevelSet.GetZoomLevels();
			int zoomlevelNumber = 0;
			for (int i = 0; i < zoomlevels.Count; i++)
			{
				if (zoomlevel.Scale == zoomlevels[i].Scale)
				{
					zoomlevelNumber = i;
					break;
				}
			}
			return zoomlevelNumber;
		}

		public static int GetCurrentZoomLevelNumber(double newWidth, RectangleShape newTileExtent, GeographyUnit mapUnit)
		{
			HEREMapsMapZoomLevelSet zoomlevelSet = new HEREMapsMapZoomLevelSet();
			ZoomLevel zoomlevel = zoomlevelSet.GetZoomLevel(newTileExtent, newWidth, mapUnit);
			Collection<ZoomLevel> zoomlevels = zoomlevelSet.GetZoomLevels();
			int zoomlevelNumber = 0;
			for (int i = 0; i < zoomlevels.Count; i++)
			{
				if (zoomlevel.Scale == zoomlevels[i].Scale)
				{
					zoomlevelNumber = i;
					break;
				}
			}
			return zoomlevelNumber;
		}

		public static int GetCurrentZoomLevelNumberOpenStreet(double newWidth, RectangleShape newTileExtent, GeographyUnit mapUnit, float dpi)
		{
			OpenStreetMapsZoomLevelSet zoomlevelSet = new OpenStreetMapsZoomLevelSet();
			ZoomLevel zoomlevel = zoomlevelSet.GetZoomLevel(newTileExtent, newWidth, mapUnit, dpi);
			Collection<ZoomLevel> zoomlevels = zoomlevelSet.GetZoomLevels();
			int zoomlevelNumber = 0;
			for (int i = 0; i < zoomlevels.Count; i++)
			{
				if (zoomlevel.Scale == zoomlevels[i].Scale)
				{
					zoomlevelNumber = i;
					break;
				}
			}
			return zoomlevelNumber;
		}
	}
}

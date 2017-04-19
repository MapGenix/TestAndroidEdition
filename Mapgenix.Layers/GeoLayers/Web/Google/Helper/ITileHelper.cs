using Mapgenix.Canvas;

namespace Mapgenix.Layers
{
    public interface ITileHelper
	{
		void Process(BaseGeoCanvas canvas, int zoomLevelNumber, BaseProjection projection, GoogleMapsMapType mapType, GoogleMapsPictureFormat pictureFormat);
	}
}

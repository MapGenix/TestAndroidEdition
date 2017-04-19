using Mapgenix.Canvas;

namespace Mapgenix.Layers
{
	public static class HEREMapsAdapter
	{
		public static int GetPictureFormat(HEREMapsPictureFormat pictureFormat)
		{
			switch (pictureFormat)
			{
				case HEREMapsPictureFormat.Png:
					return 0;

				case HEREMapsPictureFormat.Gif:
					return 2;

				default:
					return 0;
			}
		}
	}
}

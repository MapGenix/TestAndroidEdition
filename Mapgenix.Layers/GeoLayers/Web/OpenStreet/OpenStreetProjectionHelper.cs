using Mapgenix.Canvas;
using Mapgenix.Shapes;
using Mapgenix.Styles;

namespace Mapgenix.Layers
{
    public static class OpenStreetProjectionHelper
	{
		const double InchPerDecimalDegree = 4374754;
		const double DivideDpi = 1.0 / 96.0;

		public static bool IsCompleteOutOfEarth(RectangleShape utmExtent, BaseProjection projection)
		{
			PointShape upperLeftPoint = (PointShape)projection.ConvertToInternalProjection(new PointShape(-180, 90));
			PointShape lowerRightPoint = (PointShape)projection.ConvertToInternalProjection(new PointShape(180, -90));

			if (utmExtent.UpperLeftPoint.X > lowerRightPoint.X ||
				utmExtent.LowerRightPoint.X < upperLeftPoint.X ||
				utmExtent.UpperLeftPoint.Y < lowerRightPoint.Y ||
				utmExtent.LowerRightPoint.Y > upperLeftPoint.Y)
			{
				return true;
			}
			return false;
		}

		public static RectangleShape GetExtentInDecimalDegree(BaseGeoCanvas canvas, BaseProjection projection)
		{
			PointShape centerPoint = ProjectionHelper.GetCenterPointInDecimalDegree(canvas, projection);
			float width = canvas.Width;
			float height = canvas.Height;

			OpenStreetMapsZoomLevelSet set = new OpenStreetMapsZoomLevelSet();
			RectangleShape newExtent = ExtentHelper.SnapToZoomLevel(canvas.CurrentWorldExtent, GeographyUnit.Meter, width, height, set);
			double currentScale = ExtentHelper.GetScale(newExtent, width, GeographyUnit.Meter, canvas.Dpi);
			double extentWidth = currentScale * width * DivideDpi / InchPerDecimalDegree;
			double extentHeight = extentWidth * height / width;
			RectangleShape resultExtent = new RectangleShape();
			resultExtent.UpperLeftPoint.X = centerPoint.X - extentWidth * 0.5;
			resultExtent.UpperLeftPoint.Y = centerPoint.Y + extentHeight * 0.5;
			resultExtent.LowerRightPoint.X = centerPoint.X + extentWidth * 0.5;
			resultExtent.LowerRightPoint.Y = centerPoint.Y - extentHeight * 0.5;

			return resultExtent;
		}
	}
}

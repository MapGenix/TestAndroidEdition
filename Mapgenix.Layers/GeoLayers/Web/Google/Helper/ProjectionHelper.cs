using Mapgenix.Canvas;
using Mapgenix.Shapes;
using Mapgenix.Styles;

namespace Mapgenix.Layers
{
    public static class ProjectionHelper
	{
		const double DivideDpi = 1.0 / 96.0;
		const double InchPerDecimalDegree = 4374754;
		const double GoogleLimitLatitud = 85.0;

		public static PointShape GetCenterPointInDecimalDegree(BaseGeoCanvas canvas, BaseProjection projection)
		{
			PointShape utmCenterPoint = canvas.CurrentWorldExtent.GetCenterPoint();
			PointShape result = (PointShape)(projection.ConvertToExternalProjection(utmCenterPoint));

			if (result.X < 0 && utmCenterPoint.X > 0)
			{
				while (result.X < 0)
				{
					result.X += 360;
				}
			}

			if (result.X > 0 && utmCenterPoint.X < 0)
			{
				while (result.X > 0)
				{
					result.X -= 360;
				}
			}

			if (result.Y > 0 && utmCenterPoint.Y < 0)
			{
				while (result.Y > 0)
				{
					result.Y -= 180;
				}
			}

			if (result.Y < 0 && utmCenterPoint.Y > 0)
			{
				while (result.Y < 0)
				{
					result.Y += 180;
				}
			}

			return result;
		}

		public static RectangleShape GetExtentInDecimalDegree(BaseGeoCanvas canvas, BaseProjection projection)
		{
			PointShape centerPoint = ProjectionHelper.GetCenterPointInDecimalDegree(canvas, projection);
			float width = canvas.Width;
			float height = canvas.Height;

			GoogleMapZoomLevelSet set = new GoogleMapZoomLevelSet();
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

		public static bool IsCompleteOutOfEarth(RectangleShape utmExtent, BaseProjection projection)
		{
			PointShape upperLeftPoint = (PointShape)projection.ConvertToExternalProjection(utmExtent.UpperLeftPoint);
			PointShape lowerRightPoint = (PointShape)projection.ConvertToExternalProjection(utmExtent.LowerRightPoint);

			if (-180 > lowerRightPoint.X ||
				180 < upperLeftPoint.X ||
				GoogleLimitLatitud < lowerRightPoint.Y ||
				-1 * GoogleLimitLatitud > upperLeftPoint.Y)
			{
				return true;
			}
			return false;
		}


	}
}

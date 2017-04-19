using System.Collections.Generic;
using Mapgenix.Canvas;
using Mapgenix.Shapes;

namespace Mapgenix.Layers
{
    public static class GeometryHelper
	{
		
		public static bool IsExtentWithinThreshold(RectangleShape currentExtent, double upperThreshold, double lowerThreshold, int canvasWidth, GeographyUnit mapUnit, float dpi)
		{
			double currentScale = ExtentHelper.GetScale(currentExtent, canvasWidth, mapUnit, dpi);

			bool isValid = false;

			if (currentScale <= upperThreshold && currentScale >= lowerThreshold)
			{
				isValid = true;
			}
			return isValid;
		}

		public static bool Contains(RectangleShape extent, RectangleShape targetExtent)
		{
			bool contains = false;
			if (extent.UpperLeftPoint.X <= targetExtent.UpperLeftPoint.X &&
				extent.UpperLeftPoint.Y >= targetExtent.UpperLeftPoint.Y &&
				extent.LowerRightPoint.X >= targetExtent.LowerRightPoint.X &&
				extent.LowerRightPoint.Y <= targetExtent.LowerRightPoint.Y)
			{
				contains = true;
			}
			return contains;
		}


		public static RectangleShape GetExpandToIncludeExtent(IEnumerable<TileMatrixCell> cells)
		{
			RectangleShape totalExtent = null;
			foreach (TileMatrixCell cell in cells)
			{
				if (totalExtent == null)
				{
					totalExtent = (RectangleShape)cell.BoundingBox.CloneDeep();
				}
				else
				{
					totalExtent.ExpandToInclude(cell.BoundingBox);
				}
			}

			return totalExtent;
		}

		public static double GetCenterX(RectangleShape renderExtent)
		{
			return (renderExtent.UpperLeftPoint.X + renderExtent.LowerRightPoint.X) * 0.5;
		}

		public static double GetCenterY(RectangleShape renderExtent)
		{
			return (renderExtent.UpperLeftPoint.Y + renderExtent.LowerRightPoint.Y) * 0.5;
		}

	}
}

using System;
using Mapgenix.Shapes;

namespace Mapgenix.Layers
{
	public static class RectangleShapeExtension
	{
		const int RectangleApartFrom = 0;
		const int RectangleContaining = 1; 
		const int RectangleContained = 2; 
		const int RectangleIntersect = 3;

		public static RectangleShape Intersection(RectangleShape sourceShape, RectangleShape targetShape)
		{

			int result = CheckContains(targetShape, sourceShape);

			switch (result)
			{
				case RectangleApartFrom:
					return new RectangleShape();
				case RectangleContaining:
					return (RectangleShape)targetShape.CloneDeep();
				case RectangleContained:
					return (RectangleShape)sourceShape.CloneDeep();
				default:
					return GetIntersectionShape(targetShape, sourceShape);
			}
		}


		public static int CheckContains(RectangleShape targetShape, RectangleShape sourceShape)
		{
			int result = RectangleIntersect;

			bool validWidth = targetShape.Width <= sourceShape.Width;
			bool validHeight = targetShape.Height <= sourceShape.Height;
			double targetCenterX = (targetShape.UpperLeftPoint.X + targetShape.LowerRightPoint.X) / 2;
			double targetCenterY = (targetShape.UpperLeftPoint.Y + targetShape.LowerRightPoint.Y) / 2;
			double sourceCenterX = (sourceShape.UpperLeftPoint.X + sourceShape.LowerRightPoint.X) / 2;
			double sourceCenterY = (sourceShape.UpperLeftPoint.Y + sourceShape.LowerRightPoint.Y) / 2;
			double distanceX = Math.Abs(targetCenterX - sourceCenterX);
			double distanceY = Math.Abs(targetCenterY - sourceCenterY);

			if (validWidth && validHeight)
			{
				double minDistanceWidth = Math.Abs((targetShape.Width - sourceShape.Width) / 2);
				double minDistanceHeight = Math.Abs((targetShape.Height - sourceShape.Height) / 2);
				if (distanceX <= minDistanceWidth && distanceY <= minDistanceHeight)
				{
					result = RectangleContaining;
				}
			}
			else if (!validWidth && !validHeight)
			{
				double minDistanceWidth = Math.Abs((targetShape.Width - sourceShape.Width) / 2);
				double minDistanceHeight = Math.Abs((targetShape.Height - sourceShape.Height) / 2);
				if (distanceX <= minDistanceWidth && distanceY <= minDistanceHeight)
				{
					result = RectangleContained;
				}
			}
			else
			{
				double maxDistanceWidth = Math.Abs((targetShape.Width + sourceShape.Width) / 2);
				double maxDistanceHeight = Math.Abs((targetShape.Height + sourceShape.Height) / 2);
				if (distanceX >= maxDistanceWidth || distanceY >= maxDistanceHeight)
				{
					return RectangleApartFrom;
				}
			}

			return result;
		}

		public static RectangleShape GetIntersectionShape(RectangleShape targetShape, RectangleShape sourceShape)
		{
			double resultUpperLeftX = double.MaxValue;
			double resultUpperLeftY = double.MinValue;
			double resultLowerRightX = double.MinValue;
			double resultLowerRightY = double.MaxValue;

			double targetUpperLeftX = targetShape.UpperLeftPoint.X;
			double targetUpperLeftY = targetShape.UpperLeftPoint.Y;
			double targetLowerRightX = targetShape.LowerRightPoint.X;
			double targetLowerRightY = targetShape.LowerRightPoint.Y;

			double sourceUpperLeftX = sourceShape.UpperLeftPoint.X;
			double sourceUpperLeftY = sourceShape.UpperLeftPoint.Y;
			double sourceLowerRightX = sourceShape.LowerRightPoint.X;
			double sourceLowerRightY = sourceShape.LowerRightPoint.Y;

			resultUpperLeftX = (targetUpperLeftX > sourceUpperLeftX && targetUpperLeftX < sourceLowerRightX) ? targetUpperLeftX : sourceUpperLeftX;
			resultUpperLeftY = (targetUpperLeftY > sourceLowerRightY && targetUpperLeftY < sourceUpperLeftY) ? targetUpperLeftY : sourceUpperLeftY;
			resultLowerRightX = (targetLowerRightX > sourceUpperLeftX && targetLowerRightX < sourceLowerRightX) ? targetLowerRightX : sourceLowerRightX;
			resultLowerRightY = (targetLowerRightY > sourceLowerRightY && targetLowerRightY < sourceUpperLeftY) ? targetLowerRightY : sourceLowerRightY;

			RectangleShape result = new RectangleShape(resultUpperLeftX, resultUpperLeftY, resultLowerRightX, resultLowerRightY);
			return result;
		}
	}
}

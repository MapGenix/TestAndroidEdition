using System;


namespace Mapgenix.Shapes
{
    [Serializable]
    internal static class WkbHelper
    {
        public static WellKnownType GetWellKnownTypeFromWkb(byte[] wkb)
        {
            if (!(wkb[2] == 0 && wkb[3] == 0 && (wkb[1] == 0 || wkb[4] == 0)))
            {
                throw new ArgumentException(ExceptionDescription.WkbIsInvalid, "wkb");
            }
         
            int shapeType = wkb[1] + wkb[4];

            return (WellKnownType)shapeType;
        }

     
        public static RectangleShape GetBoundingBoxFromWkb(byte[] wkb)
        {
            RectangleShape boundingBox = null;
            if (wkb[0] == 0)
            {
                BaseShape baseShape = BaseShape.CreateShapeFromWellKnownData(wkb);
                boundingBox = baseShape.GetBoundingBox();
            }
            else
            {
                WellKnownType type = GetWellKnownTypeFromWkb(wkb);
                switch (type)
                {
                    case WellKnownType.Invalid:
                        break;
                    case WellKnownType.Point:
                        boundingBox = GetBoundingBoxOfPointByLittleEndian(wkb);
                        break;
                    case WellKnownType.Line:
                        boundingBox = GetBoundingBoxOfLineByLittleEndian(wkb);
                        break;
                    case WellKnownType.Polygon:
                        boundingBox = GetBoundingBoxOfPolygonByLittleEndian(wkb);
                        break;
                    case WellKnownType.Multipoint:
                        boundingBox = GetBoundingBoxOfMultipointByLittleEndian(wkb);
                        break;
                    case WellKnownType.Multiline:
                        boundingBox = GetBoundingBoxOfMultilineByLittleEndian(wkb);
                        break;
                    case WellKnownType.Multipolygon:
                        boundingBox = GetBoundingBoxOfMultipolygonByLittleEndian(wkb);
                        break;
                    case WellKnownType.GeometryCollection:
                        BaseShape baseShape = BaseShape.CreateShapeFromWellKnownData(wkb);
                        boundingBox = baseShape.GetBoundingBox();
                        break;
                    default:
                        break;
                }
            }
            return boundingBox.GetBoundingBox();
        }

        private static RectangleShape GetBoundingBoxOfPointByLittleEndian(byte[] wkb)
        {
            double x = BitConverter.ToDouble(wkb, 5);
            double y = BitConverter.ToDouble(wkb, 13);

            return new RectangleShape(x, y, x, y);
        }

        private static RectangleShape GetBoundingBoxOfLineByLittleEndian(byte[] wkb)
        {
            double minX = double.MaxValue;
            double minY = double.MaxValue;
            double maxX = double.MinValue;
            double maxY = double.MinValue;

            int count = BitConverter.ToInt32(wkb, 5);
            int index = 9;

            for (int i = 0; i < count; i++)
            {
                double tempX = BitConverter.ToDouble(wkb, index); index += 8;
                double tempY = BitConverter.ToDouble(wkb, index); index += 8;
                if (minX > tempX) { minX = tempX; }
                if (minY > tempY) { minY = tempY; }
                if (maxX < tempX) { maxX = tempX; }
                if (maxY < tempY) { maxY = tempY; }
            }

            return new RectangleShape(minX, maxY, maxX, minY);
        }

        private static RectangleShape GetBoundingBoxOfPolygonByLittleEndian(byte[] wkb)
        {
            double minX = double.MaxValue;
            double minY = double.MaxValue;
            double maxX = double.MinValue;
            double maxY = double.MinValue;

            int ringCount = BitConverter.ToInt32(wkb, 5);
            int index = 9;

            for (int i = 0; i < ringCount; i++)
            {
                int pointCount = BitConverter.ToInt32(wkb, index); index += 4;
                for (int j = 0; j < pointCount; j++)
                {
                    double tempX = BitConverter.ToDouble(wkb, index); index += 8;
                    double tempY = BitConverter.ToDouble(wkb, index); index += 8;
                    if (minX > tempX) { minX = tempX; }
                    if (minY > tempY) { minY = tempY; }
                    if (maxX < tempX) { maxX = tempX; }
                    if (maxY < tempY) { maxY = tempY; }
                }
            }

            return new RectangleShape(minX, maxY, maxX, minY);
        }

        private static RectangleShape GetBoundingBoxOfMultipointByLittleEndian(byte[] wkb)
        {
            double minX = double.MaxValue;
            double minY = double.MaxValue;
            double maxX = double.MinValue;
            double maxY = double.MinValue;

            int count = BitConverter.ToInt32(wkb, 5);
            int index = 9;

            for (int i = 0; i < count; i++)
            {
                index += 5;
                double tempX = BitConverter.ToDouble(wkb, index); index += 8;
                double tempY = BitConverter.ToDouble(wkb, index); index += 8;
                if (minX > tempX) { minX = tempX; }
                if (minY > tempY) { minY = tempY; }
                if (maxX < tempX) { maxX = tempX; }
                if (maxY < tempY) { maxY = tempY; }
            }

            return new RectangleShape(minX, maxY, maxX, minY);
        }

        private static RectangleShape GetBoundingBoxOfMultilineByLittleEndian(byte[] wkb)
        {
            double minX = double.MaxValue;
            double minY = double.MaxValue;
            double maxX = double.MinValue;
            double maxY = double.MinValue;

            int lineCount = BitConverter.ToInt32(wkb, 5);
            int index = 9;

            for (int i = 0; i < lineCount; i++)
            {
                index += 5;
                double tempPointCount = BitConverter.ToInt32(wkb, index); index += 4;
                for (int j = 0; j < tempPointCount; j++)
                {
                    double tempX = BitConverter.ToDouble(wkb, index); index += 8;
                    double tempY = BitConverter.ToDouble(wkb, index); index += 8;
                    if (minX > tempX) { minX = tempX; }
                    if (minY > tempY) { minY = tempY; }
                    if (maxX < tempX) { maxX = tempX; }
                    if (maxY < tempY) { maxY = tempY; }
                }
            }

            return new RectangleShape(minX, maxY, maxX, minY);
        }

        private static RectangleShape GetBoundingBoxOfMultipolygonByLittleEndian(byte[] wkb)
        {
            double minX = double.MaxValue;
            double minY = double.MaxValue;
            double maxX = double.MinValue;
            double maxY = double.MinValue;

            int polygonCount = BitConverter.ToInt32(wkb, 5);
            int index = 9;

            for (int i = 0; i < polygonCount; i++)
            {
                index += 5;
                int ringCount = BitConverter.ToInt32(wkb, index); index += 4;

                for (int j = 0; j < ringCount; j++)
                {
                    int pointCount = BitConverter.ToInt32(wkb, index); index += 4;

                    for (int k = 0; k < pointCount; k++)
                    {
                        double tempX = BitConverter.ToDouble(wkb, index); index += 8;
                        double tempY = BitConverter.ToDouble(wkb, index); index += 8;
                        if (minX > tempX) { minX = tempX; }
                        if (minY > tempY) { minY = tempY; }
                        if (maxX < tempX) { maxX = tempX; }
                        if (maxY < tempY) { maxY = tempY; }
                    }
                }
            }

            return new RectangleShape(minX, maxY, maxX, minY);
        }
    }
}

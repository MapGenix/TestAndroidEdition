using System;
using System.Globalization;
using System.IO;
using GeoAPI.IO;

using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

namespace Mapgenix.Shapes
{
    [Serializable]
    internal static class ShapeConverter
    {
        private static WkbByteOrder wkbByteOrder = WkbByteOrder.LittleEndian;

        internal static BaseShape JtsShapeToShape(Geometry targetJtsShape)
        {
            Validators.CheckParameterIsNotNull(targetJtsShape, "targetJtsShape");

            WKBWriter wkbWriter = new WKBWriter(ByteOrder.LittleEndian);
            byte[] byteWkb = wkbWriter.Write(targetJtsShape);

            return GetBaseShapeFromBytes(byteWkb);
        }

        internal static Geometry ShapeToJtsShape(BaseShape targetShape)
        {
            Validators.CheckParameterIsNotNull(targetShape, "targetShape");
            Validators.CheckParameterIsValid(targetShape, "targetShape");

            byte[] targetShapeByte = targetShape.GetWellKnownBinary();

            return (Geometry)new WKBReader().Read(targetShapeByte);
        }

        internal static void WriteWkb(int value, WkbByteOrder byteOrder, BinaryWriter writer)
        {
            byte[] byteArray = BitConverter.GetBytes(value);
            if (byteOrder == WkbByteOrder.BigEndian)
            {
                Array.Reverse(byteArray, 0, byteArray.Length);
            }

            writer.Write(byteArray);
        }

        internal static void WriteWkb(double value, WkbByteOrder byteOrder, BinaryWriter writer)
        {
            switch (byteOrder)
            {
                case WkbByteOrder.LittleEndian:
                    writer.Write(BitConverter.GetBytes(value));
                    break;

                case WkbByteOrder.BigEndian:
                    byte[] byteArray = BitConverter.GetBytes(value);
                    Array.Reverse(byteArray, 0, byteArray.Length);
                    writer.Write(byteArray);
                    break;
                default:
                    break;
            }
        }

        internal static BaseShape GetBaseShapeFromBytes(byte[] wkb)
        {
            Stream stream = null;
            BinaryReader binaryReader = null;
            BaseShape result = null;
            try
            {
                stream = new MemoryStream(wkb);
                int tempByte = stream.ReadByte();
                if (tempByte == 0)
                {
                    wkbByteOrder = WkbByteOrder.BigEndian;
                }
                else
                {
                    wkbByteOrder = WkbByteOrder.LittleEndian;
                }

                binaryReader = new BinaryReader(stream);
                result = GetBaseShapeFromStream(binaryReader);
            }
            finally
            {
                if (binaryReader != null)
                {
                    binaryReader.Close();
                }
            }

            return result;
        }

        private static BaseShape GetBaseShapeFromStream(BinaryReader binaryReader)
        {
            int geometryType = GetFormatedInt(binaryReader);
            switch (geometryType)
            {
                case WkbShapeType.Point:
                    return GetPointShape(binaryReader);

                case WkbShapeType.LineString:
                    return GetLineShape(binaryReader);

                case WkbShapeType.Polygon:
                    return GetPolygonShape(binaryReader);

                case WkbShapeType.Multipoint:
                    return GetMultiPointShape(binaryReader);

                case WkbShapeType.Multiline:
                    return GetMultiLineShape(binaryReader);

                case WkbShapeType.Multipolygon:
                    return GetMultipolygonShape(binaryReader);

                case WkbShapeType.GeometryCollection:
                    return GetGeometryCollectionShape(binaryReader);

                default:
                    throw new NotSupportedException("Geometry Type " + geometryType + " is not supported now.");
               }
        }

        private static PointShape GetPointShape(BinaryReader binaryReader)
        {
            PointShape returnPointShape = new PointShape();
            returnPointShape.X = GetFormatedDouble(binaryReader);
            returnPointShape.Y = GetFormatedDouble(binaryReader);

            return returnPointShape;
        }

        private static Vertex GetVertex(BinaryReader binaryReader)
        {
            double x = GetFormatedDouble(binaryReader);
            double y = GetFormatedDouble(binaryReader);
            Vertex returnVertex = new Vertex(x, y);

            return returnVertex;
        }

        private static LineShape GetLineShape(BinaryReader binaryReader)
        {
            LineShape returnLineShape = new LineShape();

            int linePointsCount = GetFormatedInt(binaryReader);
            for (int i = 0; i < linePointsCount; i++)
            {
                returnLineShape.Vertices.Add(GetVertex(binaryReader));
            }

            return returnLineShape;
        }

        private static PolygonShape GetPolygonShape(BinaryReader binaryReader)
        {
            PolygonShape returnPolygonShape = new PolygonShape();
            int ringsCount = GetFormatedInt(binaryReader);

            returnPolygonShape.OuterRing = ReadRingShape(binaryReader);

            for (int i = 0; i < ringsCount - 1; i++)
            {
                returnPolygonShape.InnerRings.Add(ReadRingShape(binaryReader));
            }

            return returnPolygonShape;
        }

        private static MultipointShape GetMultiPointShape(BinaryReader binaryReader)
        {
            MultipointShape returnMultipoint = new MultipointShape();

            int pointsCount = GetFormatedInt(binaryReader);
            for (int i = 0; i < pointsCount; i++)
            {
                SetWkbOrderType(binaryReader);
                CheckWkbShapeType(binaryReader, WkbShapeType.Point);
                returnMultipoint.Points.Add(GetPointShape(binaryReader));
            }

            return returnMultipoint;
        }

        private static MultilineShape GetMultiLineShape(BinaryReader binaryReader)
        {
            MultilineShape returnMultiLine = new MultilineShape();

            int linesCount = GetFormatedInt(binaryReader);
            for (int i = 0; i < linesCount; i++)
            {
                SetWkbOrderType(binaryReader);
                CheckWkbShapeType(binaryReader, WkbShapeType.LineString);
                returnMultiLine.Lines.Add(GetLineShape(binaryReader));
            }

            return returnMultiLine;
        }

        private static GeometryCollectionShape GetGeometryCollectionShape(BinaryReader binaryReader)
        {
            GeometryCollectionShape returnGeometryCollection = new GeometryCollectionShape();

            int shapesCount = GetFormatedInt(binaryReader);
            for (int i = 0; i < shapesCount; i++)
            {
                SetWkbOrderType(binaryReader);
                returnGeometryCollection.Shapes.Add(GetBaseShapeFromStream(binaryReader));
            }

            return returnGeometryCollection;
        }

        private static MultipolygonShape GetMultipolygonShape(BinaryReader binaryReader)
        {
            MultipolygonShape returnMultipolygon = new MultipolygonShape();

            int polygonsCount = GetFormatedInt(binaryReader);
            for (int i = 0; i < polygonsCount; i++)
            {
                SetWkbOrderType(binaryReader);
                CheckWkbShapeType(binaryReader, WkbShapeType.Polygon);
                returnMultipolygon.Polygons.Add(GetPolygonShape(binaryReader));
            }

            return returnMultipolygon;
        }

        private static RingShape ReadRingShape(BinaryReader binaryReader)
        {
            RingShape returnRingShape = new RingShape();

            int pointsCount = GetFormatedInt(binaryReader);
            for (int i = 0; i < pointsCount; i++)
            {
                returnRingShape.Vertices.Add(GetVertex(binaryReader));
            }
            return returnRingShape;
        }

        private static void SetWkbOrderType(BinaryReader binaryReader)
        {
            if (binaryReader.ReadByte() == 0)
            { wkbByteOrder = WkbByteOrder.BigEndian; }
            else
            { wkbByteOrder = WkbByteOrder.LittleEndian; }
        }

        private static void CheckWkbShapeType(BinaryReader binaryReader, int wkbShapeType)
        {
            int geometryType = GetFormatedInt(binaryReader);
            if (geometryType != wkbShapeType)
            {
                throw new ArgumentException(ExceptionDescription.WkbTypeError, "wkbShapeType");
            }
        }

        private static int GetFormatedInt(BinaryReader binaryReader)
        {
            int result = 0;

            if (wkbByteOrder == WkbByteOrder.BigEndian)
            {
                byte[] bytes = new byte[4];
                binaryReader.Read(bytes, 0, 4);
                Array.Reverse(bytes);
                result = BitConverter.ToInt32(bytes, 0);
            }
            else
            {
                result = binaryReader.ReadInt32();
            }

            return result;
        }

        private static double GetFormatedDouble(BinaryReader binaryReader)
        {
            double result = 0;

            if (wkbByteOrder == WkbByteOrder.BigEndian)
            {
                byte[] bytes = new byte[8];
                binaryReader.Read(bytes, 0, 8);
                Array.Reverse(bytes);
                result = BitConverter.ToDouble(bytes, 0);
            }
            else
            {
                result = binaryReader.ReadDouble();
            }

            return result;
        }

        internal static Vertex GetVertexFormWkt(string oneVertexWkt)
        {
            Vertex returnVertex = new Vertex();
            int xBegin = oneVertexWkt.IndexOf("(", StringComparison.Ordinal);
            int yEnd = oneVertexWkt.Length;
            int space = oneVertexWkt.IndexOf(" ", StringComparison.Ordinal);

            while (oneVertexWkt.Substring(xBegin + 1, 1) == "(")
            {
                xBegin = xBegin + 1;
            }

            while (oneVertexWkt.Substring(yEnd - 1, 1) == (")"))
            {
                yEnd = yEnd - 1;
            }

            returnVertex.X = Convert.ToDouble(oneVertexWkt.Substring(xBegin + 1, space - xBegin - 1).Trim(), CultureInfo.InvariantCulture);
            returnVertex.Y = Convert.ToDouble(oneVertexWkt.Substring(space + 1, yEnd - space - 1).Trim(), CultureInfo.InvariantCulture);

            return returnVertex;
        }

        internal static string SetVertexToWkt(string oneVertexWkt, Vertex vertex)
        {
            int xBegin = oneVertexWkt.IndexOf("(", StringComparison.Ordinal);
            int yEnd = oneVertexWkt.Length;

            while (oneVertexWkt.Substring(xBegin + 1, 1) == "(")
            {
                xBegin = xBegin + 1;
            }
            while (oneVertexWkt.Substring(yEnd - 1, 1) == (")"))
            {
                yEnd = yEnd - 1;
            }

            string corrdinate = string.Format(CultureInfo.InvariantCulture, "{0} {1}", vertex.X.ToString(CultureInfo.InvariantCulture), vertex.Y.ToString(CultureInfo.InvariantCulture));

            return oneVertexWkt.Substring(0, xBegin + 1).Trim() + corrdinate + oneVertexWkt.Substring(yEnd).Trim();
        }
    }
}

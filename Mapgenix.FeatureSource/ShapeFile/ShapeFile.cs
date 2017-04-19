using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using Mapgenix.FeatureSource.Properties;
using Mapgenix.Shapes;
using Mapgenix.Utils;

namespace Mapgenix.FeatureSource
{
    [Serializable]
    public class ShapeFile
    {
        private const int ShapeTypeLength = 4;
        private const int IntLength = 4;
        private const int DoubleLength = 8;
        private const int BoundingBoxLength = 32;

        private string _shpPathFileName;
        private bool _isOpen;
        private Stream _shpFileStream;
        private readonly ShapeFileIndex _shx;
       
        public event EventHandler<StreamLoadingEventArgs> StreamLoading;

        public ShapeFile()
            : this(string.Empty)
        { }

        public ShapeFile(string pathFileName)
        {
            this._shpPathFileName = pathFileName;
            _shx = new ShapeFileIndex();
        }

        public string PathFilename
        {
            get { return _shpPathFileName; }
            set { _shpPathFileName = value; }
        }

        public ShapeFileIndex Shx
        {
            get { return _shx; }
        }

        public ShapeFileType ShapeFileType
        {
            get { return _shx.FileHeader.ShapeFileType; }
        }

        public virtual void OnStreamLoading(StreamLoadingEventArgs e)
        {
            EventHandler<StreamLoadingEventArgs> handler = StreamLoading;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        public string CheckRecordIsValid(int recordIndex)
        {
            string errorMessage = string.Empty;

            int offset = _shx.GetRecordOffset(recordIndex);
            int contentLength = _shx.GetRecordContentLength(recordIndex);

            if (contentLength > 4)
            {
                byte[] recordBytes = new byte[contentLength];
                _shpFileStream.Seek((long)offset + 8, SeekOrigin.Begin);
                _shpFileStream.Read(recordBytes, 0, recordBytes.Length);

                int recordType = ArrayHelper.GetIntFromByteArray(recordBytes, 0, 1);

                if (recordType == (int)ShapeFileType.Multipatch)
                {
                    errorMessage = ExceptionDescription.ShapeTypeNotImplement;
                    return errorMessage;
                }

                if (recordType != (int)ShapeFileType && recordType != (int)ShapeFileType.Null)
                {
                    errorMessage = string.Format(CultureInfo.InvariantCulture, "The Record type is: {0}, it should be the same as the shape file type: {1}.", ((ShapeFileType)recordType).ToString(), ShapeFileType.ToString());
                    return errorMessage;
                }

                try
                {
                    byte[] wkb = GetWkb(recordBytes, 0);
                    BaseShape.CreateShapeFromWellKnownData(wkb);
                }
                catch
                {
                    errorMessage = "The record content is not valid.";
                    return errorMessage;
                }
            }

            return errorMessage;
        }

        public int GetRecordCount()
        {
            return _shx.GetRecordCount();
        }

        public static void CreateShapeFile(string pathFileName, ShapeFileType shapeType)
        {
            string shxPathFileName = Path.ChangeExtension(pathFileName, ".shx");
            GenerateEmptyFile(shxPathFileName, shapeType);

            GenerateEmptyFile(pathFileName, shapeType);
        }

        private static void GenerateEmptyFile(string pathFileName, ShapeFileType shapeType)
        {
            Stream fileStream = new FileStream(pathFileName, FileMode.Create, FileAccess.ReadWrite);

            ShapeFileHeader header = new ShapeFileHeader(shapeType);
            header.WriteToStream(fileStream);

            fileStream.Flush();
            fileStream.Close();
            
        }

        public void Open(FileAccess fileAccess)
        {
            StreamLoadingEventArgs args = new StreamLoadingEventArgs(_shpPathFileName, "SHP File");
            OnStreamLoading(args);

            _shpFileStream = args.Stream;
            if (_shpFileStream == null)
            {
                Validators.CheckFileIsExist(_shpPathFileName);
                _shpFileStream = new FileStream(_shpPathFileName, FileMode.Open, fileAccess);
            }

            _shx.ShxPathFileName = Path.ChangeExtension(_shpPathFileName, ".shx");
            _shx.Open(fileAccess);

            _shx.FileHeader = ShapeFileHeader.GetHeaderFromStream(_shpFileStream);
            _isOpen = true;
        }

        public void Close()
        {
            if (_isOpen && _shpFileStream != null)
            {
                _shpFileStream.Close();
                _shpFileStream = null;

                if (_shx != null) { _shx.Close(); }
            }
            _isOpen = false;
        }

        public RectangleShape GetShapeBoundingBox()
        {
            BoundingBox boundingBox = _shx.FileHeader.BoundingBox;
            
            if (boundingBox.MinX == 0 && boundingBox.MaxY == -1 && boundingBox.MaxX == -1 && boundingBox.MinY == 0)
            {
                boundingBox.MinX = 0;
                boundingBox.MaxY = 0;
                boundingBox.MaxX = 0;
                boundingBox.MinY = 0;
            }

            return boundingBox.GetRectangleShape();
        }

        public ShapeFileType GetShapeFileType()
        {
            return _shx.FileHeader.ShapeFileType;
        }

        public RectangleShape GetBoundingBoxById(int recordIndex)
        {
            RectangleShape boundgingBox = null;

            int offset = _shx.GetRecordOffset(recordIndex);
            int length = _shx.GetRecordContentLength(recordIndex);
            if (length <= 4)
            {
                return boundgingBox;
            }

            _shpFileStream.Seek(offset + 8, SeekOrigin.Begin);

            int shapeType = ReadIntFromStream(_shpFileStream, WkbByteOrder.LittleEndian);
            if (shapeType == 1)
            {
                byte[] tempBytes = new byte[16];
                _shpFileStream.Read(tempBytes, 0, 16);
                double x = BitConverter.ToDouble(tempBytes, 0);
                double y = BitConverter.ToDouble(tempBytes, 8);

                boundgingBox = new RectangleShape(x, y, x, y);
            }
            else
            {
                byte[] tempBytes = new byte[32];
                _shpFileStream.Read(tempBytes, 0, 32);

                double minX = BitConverter.ToDouble(tempBytes, 0);
                double minY = BitConverter.ToDouble(tempBytes, 8);
                double maxX = BitConverter.ToDouble(tempBytes, 16);
                double maxY = BitConverter.ToDouble(tempBytes, 24);

                if (minX <= maxX && minY <= maxY)
                {
                    boundgingBox = new RectangleShape(minX, maxY, maxX, minY);
                }
            }
            return boundgingBox;
        }

        public void AddRecord(BaseShape targetShape)
        {
            int offset = (int)_shpFileStream.Length;
            int contentLength = AddRecordIntoShp(targetShape);

            _shx.AddRecord(offset, contentLength);

            BoundingBox currentShapeBoundindBox = GetShapeFileBoundingBox(targetShape.GetBoundingBox());
            _shx.FileHeader.BoundingBox.MergeBoundingBox(currentShapeBoundindBox);
            _shx.FileHeader.FileLength = (int)_shpFileStream.Length / 2;
        }

        public void DeleteRecord(int recordIndex)
        {
            int offset = _shx.GetRecordOffset(recordIndex);
            _shx.DeleteRecord(recordIndex);

            _shpFileStream.Seek(offset + 4, SeekOrigin.Begin);
            WriteIntToStream(0, _shpFileStream, WkbByteOrder.LittleEndian);

            WriteIntToStream(0, _shpFileStream, WkbByteOrder.LittleEndian);

        }

        public void UpdateRecord(int index, BaseShape targetShape)
        {
            int offset = (int)_shpFileStream.Length;

            int contentLength = AddRecordIntoShp(targetShape);

            _shx.UpdateRecord(index, offset, contentLength);

            BoundingBox currentShapeBoundindBox = GetShapeFileBoundingBox(targetShape.GetBoundingBox());
            _shx.FileHeader.BoundingBox.MergeBoundingBox(currentShapeBoundindBox);
            _shx.FileHeader.FileLength = (int)_shpFileStream.Length / 2;
        }

        public void Flush()
        {
            _shx.FileHeader.WriteToStream(_shpFileStream);
            _shpFileStream.Flush();

            _shx.Flush();
        }

        public byte[] ReadRecord(int recordIndex)
        {
            int offset = _shx.GetRecordOffset(recordIndex);
            int contentLength = _shx.GetRecordContentLength(recordIndex);

            byte[] wkb = null;
            try
            {
                if (contentLength <= 4)
                {
                    wkb = new byte[] { };
                }
                else if (_shx.FileHeader.ShapeFileType == ShapeFileType.Polygon
                        || _shx.FileHeader.ShapeFileType == ShapeFileType.PolygonM
                        || _shx.FileHeader.ShapeFileType == ShapeFileType.PolygonZ)
                {
                    byte[] memoryBytes = new byte[contentLength];

                    _shpFileStream.Seek((long)offset + 8, SeekOrigin.Begin);
                    _shpFileStream.Read(memoryBytes, 0, memoryBytes.Length);
                    wkb = ReadPolygon(memoryBytes, 0);
                }
                else
                {
                    byte[] dataBytes = null;
                    int startIndex = 0;
                    
                    dataBytes = new byte[contentLength];
                    _shpFileStream.Seek(offset + 8, SeekOrigin.Begin);
                    _shpFileStream.Read(dataBytes, 0, contentLength);
                    
                    wkb = GetWkb(dataBytes, startIndex);
                }
            }
            catch
            {
                throw new FileLoadException(ExceptionDescription.RecordIsInvalid + "Record index: " + recordIndex);
            }
            return wkb;
        }

        private byte[] GetWkb(byte[] dataBytes, int startIndex)
        {
            byte[] wkb = null;
            switch (_shx.FileHeader.ShapeFileType)
            {
                case ShapeFileType.Point:
                case ShapeFileType.PointM:
                case ShapeFileType.PointZ:
                    wkb = ReadPoint(dataBytes, startIndex);
                    break;
                case ShapeFileType.Multipoint:
                case ShapeFileType.MultipointM:
                case ShapeFileType.MultipointZ:
                    wkb = ReadMultipoint(dataBytes, startIndex);
                    break;
                case ShapeFileType.Polyline:
                case ShapeFileType.PolylineM:
                case ShapeFileType.PolylineZ:
                    wkb = ReadPolyline(dataBytes, startIndex);
                    break;
                case ShapeFileType.Polygon:
                case ShapeFileType.PolygonM:
                case ShapeFileType.PolygonZ:
                    wkb = ReadPolygon(dataBytes, startIndex);
                    break;
                case ShapeFileType.Null:
                case ShapeFileType.Multipatch:
                default:
                    throw new NotImplementedException(ExceptionDescription.ShapeTypeNotImplement + ": " + _shx.FileHeader.ShapeFileType.ToString());
            }

            return wkb;
        }

        private int AddRecordIntoShp(BaseShape targetShape)
        {
            _shpFileStream.Seek(0, SeekOrigin.End);

            int recordCount = _shx.GetRecordCount();
            WriteIntToStream(recordCount + 1, _shpFileStream, WkbByteOrder.BigEndian);

            int contentLength;
            WellKnownType targetShapeWellKnownType = targetShape.GetWellKnownType();
            switch (targetShapeWellKnownType)
            {
                case WellKnownType.Point:
                    contentLength = WritePointStream((PointShape)targetShape, _shpFileStream);
                    break;

                case WellKnownType.Multipoint:
                    contentLength = WriteMultipointStream((MultipointShape)targetShape, _shpFileStream);
                    break;

                case WellKnownType.Line:
                    MultilineShape multiline = new MultilineShape();
                    multiline.Lines.Add((LineShape)targetShape);
                    contentLength = WriteMultilineStream(multiline, _shpFileStream);
                    break;

                case WellKnownType.Multiline:
                    contentLength = WriteMultilineStream((MultilineShape)targetShape, _shpFileStream);
                    break;

                case WellKnownType.Polygon:
                    MultipolygonShape multipolygon = new MultipolygonShape();
                    multipolygon.Polygons.Add((PolygonShape)targetShape);
                    contentLength = WriteMultipolygonStream(multipolygon, _shpFileStream);
                    break;

                case WellKnownType.Multipolygon:
                    contentLength = WriteMultipolygonStream((MultipolygonShape)targetShape, _shpFileStream);
                    break;

                case WellKnownType.Invalid:
                default:
                    throw new NotSupportedException();
            }

            return contentLength;
        }

        private int WriteMultipolygonStream(MultipolygonShape targetShape, Stream stream)
        {
            List<RingShape> listRingShapes = GetRingsFromMultipolygon(targetShape);
            int pointsCount = 0;
            int partsCount = listRingShapes.Count;
            foreach (RingShape ringShape in listRingShapes)
            {
                pointsCount += ringShape.Vertices.Count;
            }
            
            int contentLength = (ShapeTypeLength + BoundingBoxLength + IntLength * 2 + IntLength * partsCount + (2 * DoubleLength) * pointsCount) / 2;
            WriteIntToStream(contentLength, stream, WkbByteOrder.BigEndian);

            int typeNumber = GetShapeFileTypeNumber(ShapeFileType.Polygon);
            WriteIntToStream(typeNumber, stream, WkbByteOrder.LittleEndian);

            BoundingBox currentBoundingBox = GetShapeFileBoundingBox(targetShape.GetBoundingBox());
            currentBoundingBox.WriteBoundingBox(stream, false);
            _shx.FileHeader.BoundingBox.MergeBoundingBox(currentBoundingBox);

            WriteIntToStream(partsCount, stream, WkbByteOrder.LittleEndian);
            WriteIntToStream(pointsCount, stream, WkbByteOrder.LittleEndian);

            int currentPointPos = 0;
            for (int i = 0; i < partsCount; i++)
            {
                WriteIntToStream(currentPointPos, stream, WkbByteOrder.LittleEndian);
                currentPointPos += listRingShapes[i].Vertices.Count;
            }

            foreach (RingShape ringshape in listRingShapes)
            {
                foreach (Vertex vertex in ringshape.Vertices)
                {
                    WriteDoubleToStream(vertex.X, stream, WkbByteOrder.LittleEndian);
                    WriteDoubleToStream(vertex.Y, stream, WkbByteOrder.LittleEndian);
                }
            }

            return contentLength;
        }

        private static List<RingShape> GetRingsFromMultipolygon(MultipolygonShape targetShape)
        {
            List<RingShape> returnRings = new List<RingShape>();
            foreach (PolygonShape polygon in targetShape.Polygons)
            {
                if (polygon.OuterRing.IsCounterClockwise())
                {
                    polygon.OuterRing.ReversePoints();
                }
                returnRings.Add(polygon.OuterRing);

                foreach (RingShape ringShape in polygon.InnerRings)
                {
                    if (!ringShape.IsCounterClockwise())
                    {
                        ringShape.ReversePoints();
                    }
                    returnRings.Add(ringShape);
                }
            }

            return returnRings;
        }

        private int WriteMultilineStream(MultilineShape targetShape, Stream stream)
        {
            int pointsCount = 0;
            int linesCount = targetShape.Lines.Count;
            foreach (LineShape line in targetShape.Lines)
            {
                pointsCount += line.Vertices.Count;
            }
            
            int contentLength = (ShapeTypeLength + BoundingBoxLength + IntLength * 2 + IntLength * linesCount + (2 * DoubleLength) * pointsCount) / 2;
            WriteIntToStream(contentLength, stream, WkbByteOrder.BigEndian);

            int typeNumber = GetShapeFileTypeNumber(ShapeFileType.Polyline);
            WriteIntToStream(typeNumber, stream, WkbByteOrder.LittleEndian);

            BoundingBox currentBoundingBox = GetShapeFileBoundingBox(targetShape.GetBoundingBox());
            currentBoundingBox.WriteBoundingBox(stream, false);
            _shx.FileHeader.BoundingBox.MergeBoundingBox(currentBoundingBox);

            WriteIntToStream(linesCount, stream, WkbByteOrder.LittleEndian);
            WriteIntToStream(pointsCount, stream, WkbByteOrder.LittleEndian);

            int currentPointPos = 0;
            for (int i = 0; i < linesCount; i++)
            {
                WriteIntToStream(currentPointPos, stream, WkbByteOrder.LittleEndian);
                currentPointPos += targetShape.Lines[i].Vertices.Count;
            }

            foreach (LineShape line in targetShape.Lines)
            {
                foreach (Vertex vertex in line.Vertices)
                {
                    WriteDoubleToStream(vertex.X, stream, WkbByteOrder.LittleEndian);
                    WriteDoubleToStream(vertex.Y, stream, WkbByteOrder.LittleEndian);
                }
            }

            return contentLength;
        }

        private int WriteMultipointStream(MultipointShape targetShape, Stream stream)
        {
            int pointsCount = targetShape.Points.Count;
            int contentLength = (ShapeTypeLength + BoundingBoxLength + IntLength + (2 * DoubleLength) * pointsCount) / 2;
            WriteIntToStream(contentLength, stream, WkbByteOrder.BigEndian);

            int typeNumber = GetShapeFileTypeNumber(ShapeFileType.Multipoint);
            WriteIntToStream(typeNumber, stream, WkbByteOrder.LittleEndian);

            BoundingBox currentBoundingBox = GetShapeFileBoundingBox(targetShape.GetBoundingBox());
            currentBoundingBox.WriteBoundingBox(stream, false);
            _shx.FileHeader.BoundingBox.MergeBoundingBox(currentBoundingBox);

            WriteIntToStream(pointsCount, stream, WkbByteOrder.LittleEndian);

            for (int i = 0; i < pointsCount; i++)
            {
                WriteDoubleToStream(targetShape.Points[i].X, stream, WkbByteOrder.LittleEndian);
                WriteDoubleToStream(targetShape.Points[i].Y, stream, WkbByteOrder.LittleEndian);
            }

            return contentLength;
        }

        private static int WritePointStream(PointShape targetShape, Stream stream)
        {
            int contentLengthInInt = (ShapeTypeLength + 2 * DoubleLength) / 2;
            WriteIntToStream(contentLengthInInt, stream, WkbByteOrder.BigEndian);

            int typeNumber = GetShapeFileTypeNumber(ShapeFileType.Point);
            WriteIntToStream(typeNumber, stream, WkbByteOrder.LittleEndian);

            WriteDoubleToStream(targetShape.X, stream, WkbByteOrder.LittleEndian);
            WriteDoubleToStream(targetShape.Y, stream, WkbByteOrder.LittleEndian);

            return contentLengthInInt;
        }

        private static byte[] ReadPolygon(byte[] dataBytes, int startIndex)
        {
            int currentIndex = startIndex;
            currentIndex += 4;
            currentIndex += 32; 

            int partsCount = BitConverter.ToInt32(dataBytes, currentIndex); currentIndex += 4;
            int pointsCount = BitConverter.ToInt32(dataBytes, currentIndex); currentIndex += 4;

            int[] partsPointPos = new int[partsCount];
            for (int i = 0; i < partsCount; i++)
            {
                partsPointPos[i] = BitConverter.ToInt32(dataBytes, currentIndex); currentIndex += 4;
            }

            RingShape[] rings = new RingShape[partsCount];
            for (int i = 0; i < partsCount; i++)
            {
                int startPointPos = partsPointPos[i];
                int endPointPos = pointsCount;
                if (i != partsCount - 1)
                {
                    endPointPos = partsPointPos[i + 1];
                }

                rings[i] = new RingShape();
                for (int j = startPointPos; j < endPointPos; j++)
                {
                    Vertex tmp = new Vertex();
                    tmp.X = BitConverter.ToDouble(dataBytes, currentIndex);
                    currentIndex += 8;
                    tmp.Y = BitConverter.ToDouble(dataBytes, currentIndex);
                    currentIndex += 8;
                    rings[i].Vertices.Add(tmp);
                }

                if (rings[i].Vertices[0].X != rings[i].Vertices[rings[i].Vertices.Count - 1].X ||
                    rings[i].Vertices[0].Y != rings[i].Vertices[rings[i].Vertices.Count - 1].Y)
                {
                    rings[i].Vertices.Add(rings[i].Vertices[0]);
                }
            }

            MultipolygonShape multipolygon = GetMultipolygonFromRings(rings); ;

            return GetMultipolygonWkb(multipolygon);
        }

        private static MultipolygonShape GetMultipolygonFromRings(RingShape[] ringShapes)
        {
            MultipolygonShape multiPolygonShape = new MultipolygonShape();
            List<RingShape> innerRings = new List<RingShape>();
            List<RectangleShape> polygonBoundingBoxes = new List<RectangleShape>();

            foreach (RingShape ringShape in ringShapes)
            {
                if (ringShape.IsCounterClockwise())
                {
                    innerRings.Add(ringShape);
                }
                else
                {
                    multiPolygonShape.Polygons.Add(new PolygonShape(ringShape));
                    polygonBoundingBoxes.Add(ringShape.GetBoundingBox());
                }
            }

            foreach (RingShape innerRing in innerRings)
            {
                AddInnerRingToPolygonShapes(innerRing, multiPolygonShape, polygonBoundingBoxes);
            }

            return multiPolygonShape;
        }

        private static void AddInnerRingToPolygonShapes(RingShape innerRing, MultipolygonShape multiPolygonShape, List<RectangleShape> polygonBoundingBoxes)
        {
            RectangleShape holeBoundingBox = innerRing.GetBoundingBox();
            List<int> polygonBelongsTos = new List<int>();

            for (int j = 0; j < multiPolygonShape.Polygons.Count; j++)
            {
                if (polygonBoundingBoxes[j].Contains(holeBoundingBox)) 
                {
                    polygonBelongsTos.Add(j);
                }
            }
            if (polygonBelongsTos.Count == 1)
            {
                multiPolygonShape.Polygons[polygonBelongsTos[0]].InnerRings.Add(innerRing);
            }
           
            else if (polygonBelongsTos.Count == 0)
            {
                
                throw new InvalidDataException(ExceptionDescription.ShapeFileInnerRingNotContainedInOuterRing);
            }
           
            else if (polygonBelongsTos.Count > 1)
            {
                foreach (int polygonBelongsTo in polygonBelongsTos)
                {
                    RingShape outerRing = multiPolygonShape.Polygons[polygonBelongsTo].OuterRing;
                    Vertex testVertex = innerRing.Vertices[0]; 
                    if (PointInList(testVertex, outerRing)
                        || IsPointInRing(testVertex, outerRing))
                    {
                        multiPolygonShape.Polygons[polygonBelongsTo].InnerRings.Add(innerRing);
                        break;
                    }
                }
            }
        }

        private static byte[] GetMultipolygonWkb(MultipolygonShape multipolygon)
        {
            int polygonsCount = multipolygon.Polygons.Count;
            Stream wkbStream = new MemoryStream();
            BinaryWriter wkbWriter = new BinaryWriter(wkbStream);

            byte[] wkb = null;

            try
            {
                byte byteOrder = 1;
                wkbWriter.Write(byteOrder);

                wkbWriter.Write(BitConverter.GetBytes(6));

                wkbWriter.Write(BitConverter.GetBytes(polygonsCount));

                for (int i = 0; i < polygonsCount; i++)
                {
                    byte[] polygonWkb = GetPolygonWkb(multipolygon.Polygons[i]);
                    wkbWriter.Write(polygonWkb);
                }

                wkb = new byte[wkbStream.Length];
                wkbStream.Seek(0, SeekOrigin.Begin);
                wkbStream.Read(wkb, 0, wkb.Length);
            }
            finally
            {
                wkbStream.Close();
                wkbWriter.Close();
            }

            return wkb;
        }

        private static byte[] GetPolygonWkb(PolygonShape polygonShape)
        {
            int length = 0;
            int ringsCount = 1 + polygonShape.InnerRings.Count;

            RingShape[] rings = new RingShape[ringsCount];
            rings[0] = polygonShape.OuterRing;
            length += 4 + 2 * 8 * rings[0].Vertices.Count;
            for (int i = 1; i < ringsCount; i++)
            {
                rings[i] = polygonShape.InnerRings[i - 1];
                length += 4 + 2 * 8 * rings[i].Vertices.Count;
            }

            byte[] wkb = new byte[9 + length];

            wkb[0] = 1;
            wkb[1] = 3;
            wkb[2] = 0;
            wkb[3] = 0;
            wkb[4] = 0;
            long index = 5;

            ArrayHelper.CopyToArray(BitConverter.GetBytes(ringsCount), wkb, index);
            index += 4;

            for (int i = 0; i < ringsCount; i++)
            {
                Collection<Vertex> tempVertices = rings[i].Vertices;
                int count = tempVertices.Count;
                ArrayHelper.CopyToArray(BitConverter.GetBytes(count), wkb, index);
                index += 4;

                for (int j = 0; j < count; j++)
                {
                    byte[] tempBytes = BitConverter.GetBytes(tempVertices[j].X);
                    for (int k = 0; k < 8; k++)
                    {
                        wkb[index] = tempBytes[k];
                        index++;
                    }

                    tempBytes = BitConverter.GetBytes(tempVertices[j].Y);
                    for (int k = 0; k < 8; k++)
                    {
                        wkb[index] = tempBytes[k];
                        index++;
                    }
                }
            }
            return wkb;
        }

        private static byte[] ReadPoint(byte[] dataBytes, int startIndex)
        {
            int currentIndex = startIndex;
            byte[] wellKnownBinary = new byte[21];

            wellKnownBinary[0] = 1;
            wellKnownBinary[1] = 1;
            wellKnownBinary[2] = 0;
            wellKnownBinary[3] = 0;
            wellKnownBinary[4] = 0;

            currentIndex += 4;
            Array.Copy(dataBytes, currentIndex, wellKnownBinary, 5, 16);

            return wellKnownBinary;
        }

        private static byte[] ReadMultipoint(byte[] dataBytes, int startIndex)
        {
            int currentIndex = startIndex;
            currentIndex += 36; 

            int pointsCount = BitConverter.ToInt32(dataBytes, currentIndex); currentIndex += 4;
            byte[] wellKnownBinary = new byte[9 + pointsCount * 21];

            wellKnownBinary[0] = 1;
            wellKnownBinary[1] = 4;
            wellKnownBinary[2] = 0;
            wellKnownBinary[3] = 0;
            wellKnownBinary[4] = 0;

            ArrayHelper.CopyToArray(BitConverter.GetBytes(pointsCount), wellKnownBinary, 5);
            int position = 9;

            for (int i = 0; i < pointsCount; i++)
            {
                wellKnownBinary[position++] = 1;
                wellKnownBinary[position++] = 1;
                wellKnownBinary[position++] = 0;
                wellKnownBinary[position++] = 0;
                wellKnownBinary[position++] = 0;
                Array.Copy(dataBytes, currentIndex, wellKnownBinary, position, 16); currentIndex += 16;
                position += 16;
            }

            return wellKnownBinary;
        }

        private static byte[] ReadPolyline(byte[] dataBytes, int startIndex)
        {
            byte[] wellKnownBinary = null;
            int currentIndex = startIndex;
            currentIndex += 36;
            int partsCount = BitConverter.ToInt32(dataBytes, currentIndex); currentIndex += 4;

            wellKnownBinary = ReadMultlineShape(partsCount, dataBytes, currentIndex);

            return wellKnownBinary;
        }

        private static byte[] ReadMultlineShape(int partsCount, byte[] dataBytes, int startIndex)
        {
            int currentIndex = startIndex;
            int totalPointsCount = BitConverter.ToInt32(dataBytes, currentIndex); currentIndex += 4;
            byte[] wellKnownBinary = new byte[9 + partsCount * 9 + totalPointsCount * 16];

            int position = 0;

            wellKnownBinary[0] = 1;
            wellKnownBinary[1] = 5;
            wellKnownBinary[2] = 0;
            wellKnownBinary[3] = 0;
            wellKnownBinary[4] = 0;
            ArrayHelper.CopyToArray(BitConverter.GetBytes(partsCount), wellKnownBinary, 5);
            position = 9;

            int[] parts = new int[partsCount];

            for (int i = 0; i < partsCount; i++)
            {
                parts[i] = BitConverter.ToInt32(dataBytes, currentIndex); currentIndex += 4;
            }

            for (int i = 0; i < partsCount; i++)
            {
                wellKnownBinary[position++] = 1;
                wellKnownBinary[position++] = 2;
                wellKnownBinary[position++] = 0;
                wellKnownBinary[position++] = 0;
                wellKnownBinary[position++] = 0;

                int length = (i != partsCount - 1) ? (parts[i + 1] - parts[i]) : (totalPointsCount - parts[i]);
                ArrayHelper.CopyToArray(BitConverter.GetBytes(length), wellKnownBinary, position);
                position += 4;

                length *= 16;
                Array.Copy(dataBytes, currentIndex, wellKnownBinary, position, length); currentIndex += length;
                position += length;
            }

            return wellKnownBinary;
        }


        

        internal static int ReadIntFromStream(Stream stream, WkbByteOrder byteOrder)
        {
            byte[] tempArray = new byte[4];
            stream.Read(tempArray, 0, 4);
            if (byteOrder == WkbByteOrder.BigEndian) 
            {
                Array.Reverse(tempArray);
            }

            return BitConverter.ToInt32(tempArray, 0);
        }

        internal static void WriteIntToStream(int value, Stream stream, WkbByteOrder byteOrder)
        {
            byte[] bytes = BitConverter.GetBytes(value);

            if (byteOrder == WkbByteOrder.BigEndian)
            {
                Array.Reverse(bytes);
            }

            stream.Write(bytes, 0, 4);
        }

        private static void WriteDoubleToStream(double value, Stream stream, WkbByteOrder byteOrder)
        {
            byte[] bytes = BitConverter.GetBytes(value);

            if (byteOrder == WkbByteOrder.BigEndian)
            {
                Array.Reverse(bytes);
            }

            stream.Write(bytes, 0, 8);
        }

        internal static int GetShapeFileTypeNumber(ShapeFileType shapeFileType)
        {
            switch (shapeFileType)
            {
                case ShapeFileType.Null: return 0;
                case ShapeFileType.Point: return 1;
                case ShapeFileType.Polyline: return 3;
                case ShapeFileType.Polygon: return 5;
                case ShapeFileType.Multipoint: return 8;
                case ShapeFileType.PointZ: return 11;
                case ShapeFileType.PolylineZ: return 13;
                case ShapeFileType.PolygonZ: return 15;
                case ShapeFileType.MultipointZ: return 18;
                case ShapeFileType.PointM: return 21;
                case ShapeFileType.PolylineM: return 23;
                case ShapeFileType.PolygonM: return 25;
                case ShapeFileType.MultipointM: return 28;
                case ShapeFileType.Multipatch: return 31;
                default: return 0;
            }
        }

        private static BoundingBox GetShapeFileBoundingBox(RectangleShape rectangleShape)
        {
            BoundingBox returnBBox = new BoundingBox();
            returnBBox.MinX = rectangleShape.UpperLeftPoint.X;
            returnBBox.MaxY = rectangleShape.UpperLeftPoint.Y;
            returnBBox.MaxX = rectangleShape.LowerRightPoint.X;
            returnBBox.MinY = rectangleShape.LowerRightPoint.Y;

            return returnBBox;
        }
 
        private static bool PointInList(Vertex testPoint, RingShape pointList)
        {
            foreach (Vertex p in pointList.Vertices)
                if (p == testPoint)
                    return true;
            return false;
        }

       
        private static bool IsPointInRing(Vertex p, RingShape ring)
        {
            int i;
            int i1;             
            double xInt;        
            int crossings = 0;  
            double x1;          
            double y1;
            double x2;
            double y2;
            int nPts = ring.Vertices.Count;

          
            for (i = 1; i < nPts; i++)
            {
                i1 = i - 1;
                Vertex p1 = ring.Vertices[i];
                Vertex p2 = ring.Vertices[i1];
                x1 = p1.X - p.X;
                y1 = p1.Y - p.Y;
                x2 = p2.X - p.X;
                y2 = p2.Y - p.Y;

                if (((y1 > 0) && (y2 <= 0)) || ((y2 > 0) && (y1 <= 0)))
                {
                    xInt = SignOfDet2X2(x1, y1, x2, y2) / (y2 - y1);
                    if (0.0 < xInt)
                        crossings++;
                }
            }

            if ((crossings % 2) == 1)
                return true;
            else return false;
        }

        private static int SignOfDet2X2(double x1, double y1, double x2, double y2)
        {
          
            int sign;
            double swap;
            double k;
            long count = 0;


            sign = 1;

         
            if ((x1 == 0.0) || (y2 == 0.0))
            {
                if ((y1 == 0.0) || (x2 == 0.0))
                {
                    return 0;
                }
                else if (y1 > 0)
                {
                    if (x2 > 0)
                    {
                        return -sign;
                    }
                    else
                    {
                        return sign;
                    }
                }
                else
                {
                    if (x2 > 0)
                    {
                        return sign;
                    }
                    else
                    {
                        return -sign;
                    }
                }
            }
            if ((y1 == 0.0) || (x2 == 0.0))
            {
                if (y2 > 0)
                {
                    if (x1 > 0)
                    {
                        return sign;
                    }
                    else
                    {
                        return -sign;
                    }
                }
                else
                {
                    if (x1 > 0)
                    {
                        return -sign;
                    }
                    else
                    {
                        return sign;
                    }
                }
            }

            if (0.0 < y1)
            {
                if (0.0 < y2)
                {
                    if (y1 <= y2)
                    {
                        ;
                    }
                    else
                    {
                        sign = -sign;
                        swap = x1;
                        x1 = x2;
                        x2 = swap;
                        swap = y1;
                        y1 = y2;
                        y2 = swap;
                    }
                }
                else
                {
                    if (y1 <= -y2)
                    {
                        sign = -sign;
                        x2 = -x2;
                        y2 = -y2;
                    }
                    else
                    {
                        swap = x1;
                        x1 = -x2;
                        x2 = swap;
                        swap = y1;
                        y1 = -y2;
                        y2 = swap;
                    }
                }
            }
            else
            {
                if (0.0 < y2)
                {
                    if (-y1 <= y2)
                    {
                        sign = -sign;
                        x1 = -x1;
                        y1 = -y1;
                    }
                    else
                    {
                        swap = -x1;
                        x1 = x2;
                        x2 = swap;
                        swap = -y1;
                        y1 = y2;
                        y2 = swap;
                    }
                }
                else
                {
                    if (y1 >= y2)
                    {
                        x1 = -x1;
                        y1 = -y1;
                        x2 = -x2;
                        y2 = -y2;
                        ;
                    }
                    else
                    {
                        sign = -sign;
                        swap = -x1;
                        x1 = -x2;
                        x2 = swap;
                        swap = -y1;
                        y1 = -y2;
                        y2 = swap;
                    }
                }
            }

            
            if (0.0 < x1)
            {
                if (0.0 < x2)
                {
                    if (x1 <= x2)
                    {
                        ;
                    }
                    else
                    {
                        return sign;
                    }
                }
                else
                {
                    return sign;
                }
            }
            else
            {
                if (0.0 < x2)
                {
                    return -sign;
                }
                else
                {
                    if (x1 >= x2)
                    {
                        sign = -sign;
                        x1 = -x1;
                        x2 = -x2;
                        ;
                    }
                    else
                    {
                        return -sign;
                    }
                }
            }

           
            while (true)
            {
                count = count + 1;
                k = Math.Floor(x2 / x1);
                x2 = x2 - k * x1;
                y2 = y2 - k * y1;

                if (y2 < 0.0)
                {
                    return -sign;
                }
                if (y2 > y1)
                {
                    return sign;
                }

                if (x1 > x2 + x2)
                {
                    if (y1 < y2 + y2)
                    {
                        return sign;
                    }
                }
                else
                {
                    if (y1 > y2 + y2)
                    {
                        return -sign;
                    }
                    else
                    {
                        x2 = x1 - x2;
                        y2 = y1 - y2;
                        sign = -sign;
                    }
                }
                if (y2 == 0.0)
                {
                    if (x2 == 0.0)
                    {
                        return 0;
                    }
                    else
                    {
                        return -sign;
                    }
                }
                if (x2 == 0.0)
                {
                    return sign;
                }

                k = Math.Floor(x1 / x2);
                x1 = x1 - k * x2;
                y1 = y1 - k * y2;

              
                if (y1 < 0.0)
                {
                    return sign;
                }
                if (y1 > y2)
                {
                    return -sign;
                }

              
                if (x2 > x1 + x1)
                {
                    if (y2 < y1 + y1)
                    {
                        return -sign;
                    }
                }
                else
                {
                    if (y2 > y1 + y1)
                    {
                        return sign;
                    }
                    else
                    {
                        x1 = x2 - x1;
                        y1 = y2 - y1;
                        sign = -sign;
                    }
                }
                if (y1 == 0.0)
                {
                    if (x1 == 0.0)
                    {
                        return 0;
                    }
                    else
                    {
                        return sign;
                    }
                }
                if (x1 == 0.0)
                {
                    return -sign;
                }
            }

        }
    }
}

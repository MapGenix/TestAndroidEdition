using System;
using System.IO;
using Mapgenix.Shapes;

namespace Mapgenix.FeatureSource
{
    [Serializable]
    public class ShapeFileHeader
    {
        private const int FileVersionNumber = 1000;
        private const long FileLengthInfoPosition = 24;

        private int _fileCodeNumber;
        private int _fileLength;
        private int _version;
        private BoundingBox _boundingBox;
        private ShapeFileType _shapeFileType;

        public ShapeFileHeader()
            : this(ShapeFileType.Null)
        {
        }

        public ShapeFileHeader(ShapeFileType shapeFileType)
        {
            _fileCodeNumber = 9994;
            _version = FileVersionNumber;
            this._shapeFileType = shapeFileType;
            _boundingBox = new BoundingBox();
        }

       
        public int FileLength
        {
            get { return _fileLength; }
            set { _fileLength = value; }
        }

        public ShapeFileType ShapeFileType
        {
            get { return _shapeFileType; }
            set { _shapeFileType = value; }
        }

        public BoundingBox BoundingBox
        {
            get { return _boundingBox; }
            set { _boundingBox = value; }
        }

        public static ShapeFileHeader GetHeaderFromStream(Stream stream)
        {
            ShapeFileHeader returnHeader = new ShapeFileHeader();

            stream.Seek(0, SeekOrigin.Begin);

            returnHeader._fileCodeNumber = ShapeFile.ReadIntFromStream(stream, WkbByteOrder.BigEndian);

            stream.Seek(FileLengthInfoPosition, SeekOrigin.Begin);
            returnHeader._fileLength = ShapeFile.ReadIntFromStream(stream, WkbByteOrder.BigEndian);


            returnHeader._version = ShapeFile.ReadIntFromStream(stream, WkbByteOrder.LittleEndian);

            returnHeader._shapeFileType = ConvertIntToShapeFileType(ShapeFile.ReadIntFromStream(stream, WkbByteOrder.LittleEndian));

            returnHeader._boundingBox = BoundingBox.GetHeaderBoundingBox(stream);

            return returnHeader;
        }

        public void WriteToStream(Stream targetFileStream)
        {
            targetFileStream.Seek(0, SeekOrigin.Begin);
            ShapeFile.WriteIntToStream(_fileCodeNumber, targetFileStream, WkbByteOrder.BigEndian);

            targetFileStream.Seek(FileLengthInfoPosition, SeekOrigin.Begin);
            int tempFileLength = (int)targetFileStream.Length / 2;
            ShapeFile.WriteIntToStream(tempFileLength, targetFileStream, WkbByteOrder.BigEndian);

            ShapeFile.WriteIntToStream(_version, targetFileStream, WkbByteOrder.LittleEndian);

            int typeNumber = ShapeFile.GetShapeFileTypeNumber(_shapeFileType);
            ShapeFile.WriteIntToStream(typeNumber, targetFileStream, WkbByteOrder.LittleEndian);

            _boundingBox.WriteBoundingBox(targetFileStream, true);
        }

        private static ShapeFileType ConvertIntToShapeFileType(int value)
        {
            switch (value)
            {
                case 0: return ShapeFileType.Null;
                case 1: return ShapeFileType.Point;
                case 3: return ShapeFileType.Polyline;
                case 5: return ShapeFileType.Polygon;
                case 8: return ShapeFileType.Multipoint;
                case 11: return ShapeFileType.PointZ;
                case 13: return ShapeFileType.PolylineZ;
                case 15: return ShapeFileType.PolygonZ;
                case 18: return ShapeFileType.MultipointZ;
                case 21: return ShapeFileType.PointM;
                case 23: return ShapeFileType.PolylineM;
                case 25: return ShapeFileType.PolygonM;
                case 28: return ShapeFileType.MultipointM;
                case 31: return ShapeFileType.Multipatch;
                default: return ShapeFileType.Null;
            }
        }
    }
}

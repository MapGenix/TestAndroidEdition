using System;
using System.IO;
using Mapgenix.Shapes;
using Mapgenix.Utils;

namespace Mapgenix.FeatureSource
{

    [Serializable]
    public class BoundingBox
    {
        private const int BoundingBoxStartPostion = 36;
        private const int DobleLength = 8;

        private double _minX;
        private double _maxX;
        private double _minY;
        private double _maxY;



        public double MinX
        {
            get { return _minX; }
            set { _minX = value; }
        }

        public double MaxX
        {
            get { return _maxX; }
            set { _maxX = value; }
        }

        public double MinY
        {
            get { return _minY; }
            set { _minY = value; }
        }

        public double MaxY
        {
            get { return _maxY; }
            set { _maxY = value; }
        }

        public static BoundingBox GetHeaderBoundingBox(Stream stream)
        {
            stream.Seek(BoundingBoxStartPostion, SeekOrigin.Begin);

            BoundingBox returnBoundingBox = new BoundingBox();

            BinaryReader br = new BinaryReader(stream);
            returnBoundingBox._minX = br.ReadDouble();
            returnBoundingBox._minY = br.ReadDouble();
            returnBoundingBox._maxX = br.ReadDouble();
            returnBoundingBox._maxY = br.ReadDouble();

            return returnBoundingBox;
        }

        
        public void MergeBoundingBox(BoundingBox targetBox)
        {
            if (_minX == 0 && _minY == 0 && _maxX == 0 && _maxY == 0)
            {
                _minX = targetBox._minX;
                _maxX = targetBox._maxX;
                _minY = targetBox._minY;
                _maxY = targetBox._maxY;
            }
            else
            {
                _minX = targetBox.MinX < _minX ? targetBox.MinX : _minX;
                _maxX = targetBox.MaxX > _maxX ? targetBox.MaxX : _maxX;
                _minY = targetBox.MinY < _minY ? targetBox.MinY : _minY;
                _maxY = targetBox.MaxY > _maxY ? targetBox.MaxY : _maxY;
            }
        }

        public void WriteBoundingBox(Stream stream, bool isHeaderBoundingBox)
        {
            byte[] doubleByte = ArrayHelper.GetByteArrayFromDouble(_minX, (byte)1);
            stream.Write(doubleByte, 0, DobleLength);

            doubleByte = ArrayHelper.GetByteArrayFromDouble(_minY, (byte)1);
            stream.Write(doubleByte, 0, DobleLength);

            doubleByte = ArrayHelper.GetByteArrayFromDouble(_maxX, (byte)1);
            stream.Write(doubleByte, 0, DobleLength);

            doubleByte = ArrayHelper.GetByteArrayFromDouble(_maxY, (byte)1);
            stream.Write(doubleByte, 0, DobleLength);

            if (isHeaderBoundingBox)
            {
                doubleByte = ArrayHelper.GetByteArrayFromDouble(0, (byte)1);
                stream.Write(doubleByte, 0, DobleLength);

                doubleByte = ArrayHelper.GetByteArrayFromDouble(0, (byte)1);
                stream.Write(doubleByte, 0, DobleLength);

                doubleByte = ArrayHelper.GetByteArrayFromDouble(0, (byte)1);
                stream.Write(doubleByte, 0, DobleLength);

                doubleByte = ArrayHelper.GetByteArrayFromDouble(0, (byte)1);
                stream.Write(doubleByte, 0, DobleLength);
            }
        }

       

        public RectangleShape GetRectangleShape()
        {
            Validators.CheckShapeFileBoundingBoxIsValid(this);

            return new RectangleShape(this.MinX, this.MaxY, this.MaxX, this.MinY);
        }
    }
}

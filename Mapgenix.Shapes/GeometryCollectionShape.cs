using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;


namespace Mapgenix.Shapes
{
    /// <summary>collection of shapes.</summary>
    [Serializable]
    public class GeometryCollectionShape : BaseShape
    {
        private Collection<BaseShape> shapes;

        /// <summary>Creates the GeometryCollectionShape.</summary>
        public GeometryCollectionShape()
            : this(new BaseShape[] { })
        { }

        /// <summary>Creates the GeometryCollectionShape.</summary>
        public GeometryCollectionShape(IEnumerable<BaseShape> baseShapes)
        {
            Validators.CheckParameterIsNotNull(baseShapes, "baseShapes");

            shapes = new Collection<BaseShape>();
            foreach (BaseShape baseShape in baseShapes)
            {
                shapes.Add(baseShape);
            }
        }

        /// <summary>Creates the GeometryCollectionShape.</summary>
        public GeometryCollectionShape(IEnumerable<Feature> features)
            : this()
        {
            Validators.CheckParameterIsNotNull(features, "features");
            shapes = new Collection<BaseShape>();
            foreach (Feature feature in features)
            {
                shapes.Add(feature.GetShape());
            }
        }

        /// <summary>Creates the GeometryCollectionShape.</summary>
        public GeometryCollectionShape(string wellKnownText)
            : this()
        {
            Validators.CheckParameterIsNotNull(wellKnownText, "wellKnownText");
            Validators.CheckWktStringIsValid(wellKnownText, "wellKnownText");

            LoadFromWellKnownData(wellKnownText);
        }

        /// <summary>Creates the GeometryCollectionShape.</summary>
        public GeometryCollectionShape(byte[] wellKnownBinary)
            : this()
        {
            Validators.CheckParameterIsNotNull(wellKnownBinary, "wellKnownBinary");
            Validators.CheckWkbIsValid(wellKnownBinary, "wellKnownBinary");

            LoadFromWellKnownData(wellKnownBinary);
        }

        /// <summary>Gets the collection of shapes.</summary>
        public Collection<BaseShape> Shapes
        {
            get { return shapes; }
        }

        protected override void LoadFromWellKnownDataCore(string wellKnownText)
        {
            Validators.CheckParameterIsNotNull(wellKnownText, "wellKnownText");
            Validators.CheckWktStringIsValid(wellKnownText, "wellKnownText");

            GeometryCollectionShape geometryCollection = (GeometryCollectionShape)BaseShape.CreateShapeFromWellKnownData(wellKnownText);
            CloneOneGeometryCollectionToCurrent(geometryCollection);
        }

        protected override string GetWellKnownTextCore()
        {
            Validators.CheckShapeIsValidForOperation(this);

            StringBuilder wellKnownText = new StringBuilder();
            wellKnownText.Append("GEOMETRYCOLLECTION(");

            for (int i = 0; i < shapes.Count; i++)
            {
                string baseShapeWkt = shapes[i].GetWellKnownText();
                wellKnownText.Append(baseShapeWkt);

                if (i < shapes.Count - 1)
                {
                    wellKnownText.Append(",");
                }
            }

            wellKnownText.Append(")");

            return wellKnownText.ToString();
        }

        protected override byte[] GetWellKnownBinaryCore(WkbByteOrder byteOrder)
        {
            Validators.CheckShapeIsValidForOperation(this);

            MemoryStream wkbStream = null;
            BinaryWriter wkbWriter = null;

            byte[] wkbArray = null;

            try
            {
                wkbStream = new MemoryStream();
                wkbWriter = new BinaryWriter(wkbStream);

                byte EndianByte = (byte)0;
                if (byteOrder == WkbByteOrder.LittleEndian)
                {
                    EndianByte = (byte)1;
                }
                else
                {
                    EndianByte = (byte)0;
                }

                wkbWriter.Write(EndianByte);

                ShapeConverter.WriteWkb(WkbShapeType.GeometryCollection, byteOrder, wkbWriter);
                ShapeConverter.WriteWkb(shapes.Count, byteOrder, wkbWriter);
                foreach (BaseShape shape in shapes)
                {
                    byte[] shapeBytes = shape.GetWellKnownBinary();
                    wkbWriter.Write(shapeBytes);
                }

                wkbWriter.Flush();
                wkbArray = wkbStream.ToArray();
            }
            finally
            {
                if (wkbWriter != null) { wkbWriter.Close(); }
                if (wkbStream != null) { wkbStream.Close(); }
            }

            return wkbArray;
        }

        protected override WellKnownType GetWellKnownTypeCore()
        {
            return WellKnownType.GeometryCollection;
        }

        private void CloneOneGeometryCollectionToCurrent(GeometryCollectionShape fromGeometryCollection)
        {
            for (int i = 0; i < fromGeometryCollection.Shapes.Count; i++)
            {
                Shapes.Add(fromGeometryCollection.Shapes[i]);
            }
        }

        protected override ShapeValidationResult ValidateCore(ShapeValidationMode validationMode)
        {
            return new ShapeValidationResult(true, String.Empty);
        }

        protected override MultipointShape GetCrossingCore(BaseShape targetShape)
        {
            Validators.CheckParameterIsNotNull(targetShape, "targetShape");
            Validators.CheckShapeIsValidForOperation(this);

            MultipointShape resultMultiPointShape = new MultipointShape();

            for (int i = 0; i < shapes.Count; i++)
            {
                MultipointShape multipointShape = shapes[i].GetCrossing(targetShape);

                for (int j = 0; j < multipointShape.Points.Count; j++)
                {
                    resultMultiPointShape.Points.Add(multipointShape.Points[j]);
                }
            }

            return resultMultiPointShape;
        }
    }
}

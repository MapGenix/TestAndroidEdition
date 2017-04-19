using System;
using System.Collections.Generic;
using System.Text;
using GeoAPI.Geometries;
using GeoAPI.IO;

using Mapgenix.Utils;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using GeoAPI.Operation.Buffer;

namespace Mapgenix.Shapes
{
    /// <summary>Abstract class for all shapes.</summary>
    /// <remarks>Lowest level shape in shape hierarchy. Directly
    /// inherited from BaseShape are BaseAreaShape, BaseLineShape and BasePointShape.</remarks>
    [Serializable]
    public abstract class BaseShape
    {
        private const int DefaultQuadrantSegments = 8;

        private string id;
        private object tag;

        /// <summary>Default constructor for BaseShape.</summary>    
        protected BaseShape()
        {
            id = Guid.NewGuid().ToString();
        }

        /// <summary>Id of the shape.</summary>
        public string Id
        {
            get { return id; }
            set { id = value; }
        }

        /// <summary>Tag of the shape.</summary>
        public object Tag
        {
            get { return tag; }
            set { tag = value; }
        }

        /// <summary>Creates a shape from a string of well-known text.</summary>
        /// <returns>Higher level shape constructed from the well-known text.
        /// It is necessary to cast to the higher level class.</returns>
        public static BaseShape CreateShapeFromWellKnownData(string wellKnownText)
        {
            Validators.CheckParameterIsNotNull(wellKnownText, "wellKnownText");
            Validators.CheckWktStringIsValid(wellKnownText, "wellKnownText");

            Geometry geometry = null;

            try
            {
                WKTReader reader = new WKTReader();
                geometry = (Geometry)reader.Read(wellKnownText);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(ExceptionDescription.WktFormatIsWrong, "wellKnownText", ex);
            }

            return ShapeConverter.JtsShapeToShape(geometry);
        }

       
        /// <summary>Hydrates the shape with its data from well-known text.</summary>
        /// <returns>None</returns>
        /// <param name="wellKnownText">Well-known text to hydrate the shape.</param>
        /// <remarks>As a concrete public method that wraps a Core method, Mapgenix reserves the right
        /// to add events and other logic returned by the Core version of the method.</remarks>
        public void LoadFromWellKnownData(string wellKnownText)
        {
            Validators.CheckParameterIsNotNull(wellKnownText, "wellKnownText");
            Validators.CheckWktStringIsValid(wellKnownText, "wellKnownText");

            try
            {
                LoadFromWellKnownDataCore(wellKnownText);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(ExceptionDescription.WktFormatIsWrong, "wellKnownText", ex);
            }
        }

        /// <summary>Hydrates the shape with its data from well-known text.</summary>
        /// <returns>None</returns>
        /// <param name="wellKnownText">Well-known text to hydrate the shape.</param>
        protected abstract void LoadFromWellKnownDataCore(string wellKnownText);

        /// <summary>Returns the well-known text representation of the shape.</summary>
        /// <returns>String representing the shape in well-known text.</returns>
        /// <remarks>As a concrete public method that wraps a Core method, Mapgenix reserves the right
        /// to add events and other logic returned by the Core version of the method.</remarks>
        public string GetWellKnownText()
        {
            Validators.CheckShapeIsValidForOperation(this);

            return GetWellKnownTextCore();
        }

        /// <summary>Returns the well-known text representation of the shape.</summary>
        /// <returns>String representing the shape in well-known text.</returns>
        protected abstract string GetWellKnownTextCore();

        /// <summary>Creates a shape from a string of well-known binary.</summary>
        /// <returns>Higher level shape constructed from the well-known binary.
        /// It is necessary to cast to the higher level class.</returns>
        /// <param name="wellKnownBinary">Array of bytes representing the geometry in well-known binary format.</param>
        public static BaseShape CreateShapeFromWellKnownData(byte[] wellKnownBinary)
        {
            Validators.CheckParameterIsNotNull(wellKnownBinary, "wellKnownBinary");

            BaseShape returnShape = null;

            if (wellKnownBinary.Length != 0)
            {
                try
                {
                    returnShape = ShapeConverter.GetBaseShapeFromBytes(wellKnownBinary);
                }
                catch (Exception ex)
                {
                    throw new ArgumentException(ExceptionDescription.WkbIsInvalid, "wellKnownBinary", ex);
                }
            }

            return returnShape;
        }

        /// <summary>Hydrates the shape with its data from well-known binary.</summary>
        /// <returns>None</returns>
        /// <param name="wellKnownBinary">Well-known binary to hydrate the shape.</param>
        /// <remarks>As a concrete public method that wraps a Core method, Mapgenix reserves the right
        /// to add events and other logic returned by the Core version of the method.</remarks>
        public void LoadFromWellKnownData(byte[] wellKnownBinary)
        {
            Validators.CheckParameterIsNotNull(wellKnownBinary, "wellKnownBinary");
            Validators.CheckWkbIsValid(wellKnownBinary, "wellKnownBinary");

            LoadFromWellKnownDataCore(wellKnownBinary);
        }

        /// <summary>Hydrates the shape with its data from well-known binary.</summary>
        /// <returns>None</returns>
        /// <param name="wellKnownBinary">Well-known binary to hydrate the shape.</param>
        protected virtual void LoadFromWellKnownDataCore(byte[] wellKnownBinary)
        {
            Validators.CheckParameterIsNotNull(wellKnownBinary, "wellKnownBinary");
            Validators.CheckWkbIsValid(wellKnownBinary, "wellKnownBinary");

            BaseShape baseShape = ShapeConverter.GetBaseShapeFromBytes(wellKnownBinary);
            string wkt = baseShape.GetWellKnownText();
            LoadFromWellKnownData(wkt);
        }

        /// <summary>Returns a byte array representing the shape in well-known binary.</summary>
        /// <returns>Byte array representing the shape in well-known binary.</returns>
        /// <remarks>As a concrete public method that wraps a Core method, Mapgenix reserves the right
        /// to add events and other logic returned by the Core version of the method.</remarks>
        public byte[] GetWellKnownBinary()
        {
            Validators.CheckShapeIsValidForOperation(this);

            return GetWellKnownBinaryCore(WkbByteOrder.LittleEndian);
        }

        /// <summary>Returns a byte array representing the shape in well-known binary.</summary>
        /// <returns>Byte array representing the shape in well-known binary.</returns>
        /// <remarks>As a concrete public method that wraps a Core method, Mapgenix reserves the right
        /// to add events and other logic returned by the Core version of the method.</remarks>
        /// <param name="byteOrder">Byte order to encode the well-known binary.</param>
        public byte[] GetWellKnownBinary(WkbByteOrder byteOrder)
        {
            Validators.CheckShapeIsValidForOperation(this);
   
            return GetWellKnownBinaryCore(byteOrder);
        }

        /// <summary>Returns a byte array representing the shape in well-known binary.</summary>
        /// <returns>Byte array representing the shape in well-known binary.</returns>
        /// <param name="byteOrder">Byte order to encode the well-known binary.</param>
        protected virtual byte[] GetWellKnownBinaryCore(WkbByteOrder byteOrder)
        {
            string wktString = this.GetWellKnownText();
            BaseShape tmpBaseShape = BaseShape.CreateShapeFromWellKnownData(wktString);
            Geometry geometry = ShapeConverter.ShapeToJtsShape(tmpBaseShape);

            ByteOrder order = GetByteOrderFromWkbByteOrder(byteOrder);
            WKBWriter wkbWriter = new WKBWriter(order);

            return wkbWriter.Write(geometry);
        }

        /// <summary>Calculates the rectangle encompassing the entire geometry.</summary>
        /// <returns>Rectangle encompassing the entire geometry.</returns>
        /// <remarks>As a concrete public method that wraps a Core method, Mapgenix reserves the right
        /// to add events and other logic returned by the Core version of the method.</remarks>
        public RectangleShape GetBoundingBox()
        {
            Validators.CheckShapeIsValidForOperation(this);

            return GetBoundingBoxCore();
        }

        /// <summary>Calculates the rectangle encompassing the entire geometry.</summary>
        /// <returns>Rectangle encompassing the entire geometry.</returns>
        protected virtual RectangleShape GetBoundingBoxCore()
        {
            Validators.CheckShapeIsValidForOperation(this);

            Geometry geometry = ShapeConverter.ShapeToJtsShape(this);
            Envelope envelope = (Envelope)geometry.EnvelopeInternal;
            RectangleShape boundingBox = new RectangleShape(envelope.MinX, envelope.MaxY, envelope.MaxX, envelope.MinY);

            return boundingBox;
        }

        /// <summary>returns the well-known type for the shape.</summary>
        /// <returns>Well-known type for the shape.</returns>
        /// <remarks>As a concrete public method that wraps a Core method, Mapgenix reserves the right
        /// to add events and other logic returned by the Core version of the method.</remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public WellKnownType GetWellKnownType()
        {
            return GetWellKnownTypeCore();
        }

        /// <summary>returns the well-known type for the shape.</summary>
        /// <returns>Well-known type for the shape.</returns>
        protected virtual WellKnownType GetWellKnownTypeCore()
        {
            byte[] wkb = this.GetWellKnownBinary();
            WellKnownType type = WkbHelper.GetWellKnownTypeFromWkb(wkb);

            return type;
        }

        /// <summary>Returns a complete copy of the shape.</summary>
        /// <returns>Complete copy of the shape.</returns>
        /// <remarks>As a concrete public method that wraps a Core method, Mapgenix reserves the right
        /// to add events and other logic returned by the Core version of the method.</remarks>
        public BaseShape CloneDeep()
        {
            return CloneDeepCore();
        }

        /// <summary>Returns a complete copy of the shape.</summary>
        /// <returns>Complete copy of the shape.</returns>
        protected virtual BaseShape CloneDeepCore()
        {
            string wkt = GetWellKnownText();

            return BaseShape.CreateShapeFromWellKnownData(wkt);
        }

        /// <summary>Returns a shape from one location to another based on an X and Y offset distance.</summary>
        /// <remarks>As a concrete public method that wraps a Core method, Mapgenix reserves the right
        /// to add events and other logic returned by the Core version of the method.</remarks> 
        /// <returns>Shape from one location to another based on an X and Y offset distance.</returns>
        /// <param name="targetShape">Shape to move.</param>
        /// <param name="xOffsetDistance">X offset distance.</param>
        /// <param name="yOffsetDistance">Y Offset distance.</param>
        /// <param name="shapeUnit">Geography unit of the shape.</param>
        /// <param name="distanceUnit">Distance unit.</param>
        public static BaseShape TranslateByOffset(BaseShape targetShape, double xOffsetDistance, double yOffsetDistance, GeographyUnit shapeUnit, DistanceUnit distanceUnit)
        {
            Validators.CheckParameterIsNotNull(targetShape, "targetShape");
            Validators.CheckGeographyUnitIsValid(shapeUnit, "shapeUnit");
            Validators.CheckShapeIsValidForOperation(targetShape);

            BaseShape returnBaseShape = targetShape.CloneDeepCore();
            returnBaseShape.TranslateByOffset(xOffsetDistance, yOffsetDistance, shapeUnit, distanceUnit);

            return returnBaseShape;
        }

        /// <summary>Returns a feature from one location to another based on an X and Y offset distance.</summary>
        /// <remarks>As a concrete public method that wraps a Core method, Mapgenix reserves the right
        /// to add events and other logic returned by the Core version of the method.</remarks> 
        /// <returns>Shape from one location to another based on an X and Y offset distance.</returns>
        /// <param name="targetFeature">Feature to move.</param>
        /// <param name="xOffsetDistance">X offset distance.</param>
        /// <param name="yOffsetDistance">Y Offset distance.</param>
        /// <param name="shapeUnit">Geography unit of the feature.</param>
        /// <param name="distanceUnit">Distance unit.</param>
        public static Feature TranslateByOffset(Feature targetFeature, double xOffsetDistance, double yOffsetDistance, GeographyUnit shapeUnit, DistanceUnit distanceUnit)
        {
            Validators.CheckGeographyUnitIsValid(shapeUnit, "shapeUnit");
         
            BaseShape targetShape = targetFeature.GetShape();
            Validators.CheckShapeIsValidForOperation(targetShape);

            BaseShape returnBaseShape = TranslateByOffset(targetShape, xOffsetDistance, yOffsetDistance, shapeUnit, distanceUnit);
            Feature returnFeature = new Feature(returnBaseShape, targetFeature.ColumnValues);

            return returnFeature;
        }

        /// <summary>Moves the shape from one location to another based on an X and Y offset distance.</summary>
        /// <remarks>As a concrete public method that wraps a Core method, Mapgenix reserves the right
        /// to add events and other logic returned by the Core version of the method.</remarks> 
        /// <returns>Shape from one location to another based on an X and Y offset distance.</returns>
        /// <param name="xOffsetDistance">X offset distance.</param>
        /// <param name="yOffsetDistance">Y Offset distance.</param>
        /// <param name="shapeUnit">Geography unit of the feature.</param>
        /// <param name="distanceUnit">Distance unit.</param>
        public void TranslateByOffset(double xOffsetDistance, double yOffsetDistance, GeographyUnit shapeUnit, DistanceUnit distanceUnit)
        {
            Validators.CheckGeographyUnitIsValid(shapeUnit, "shapeUnit");
            Validators.CheckShapeIsValidForOperation(this);

            TranslateByOffsetCore(xOffsetDistance, yOffsetDistance, shapeUnit, distanceUnit);
        }

        /// <summary>Moves the shape from one location to another based on an X and Y offset distance.</summary>
        /// <remarks>As a concrete public method that wraps a Core method, Mapgenix reserves the right
        /// to add events and other logic returned by the Core version of the method.</remarks> 
        /// <returns>Shape from one location to another based on an X and Y offset distance.</returns>
        /// <param name="xOffsetDistance">X offset distance.</param>
        /// <param name="yOffsetDistance">Y Offset distance.</param>
        public void TranslateByOffset(double xOffsetDistance, double yOffsetDistance)
        {
            Validators.CheckShapeIsValidForOperation(this);

            TranslateByOffsetCore(xOffsetDistance, yOffsetDistance, GeographyUnit.Meter, DistanceUnit.Meter);
        }

        /// <summary>Moves the shape from one location to another based on an X and Y offset distance.</summary>
        /// <param name="xOffsetDistance">X offset distance.</param>
        /// <param name="yOffsetDistance">Y Offset distance.</param>
        /// <param name="shapeUnit">Geography unit of the feature.</param>
        /// <param name="distanceUnit">Distance unit.</param>
        protected virtual void TranslateByOffsetCore(double xOffsetDistance, double yOffsetDistance, GeographyUnit shapeUnit, DistanceUnit distanceUnit)
        {
            Validators.CheckGeographyUnitIsValid(shapeUnit, "shapeUnit");
            Validators.CheckShapeIsValidForOperation(this);

            string[] wktArray = GetWellKnownText().Split(',');
            StringBuilder strBuild = new StringBuilder();

            for (int i = 0; i < wktArray.Length; i++)
            {
                string oneVertexWkt = wktArray[i].Trim();
                Vertex vertex = ShapeConverter.GetVertexFormWkt(oneVertexWkt);
                vertex.TranslateByOffset(xOffsetDistance, yOffsetDistance, shapeUnit, distanceUnit);
                wktArray[i] = ShapeConverter.SetVertexToWkt(oneVertexWkt, vertex);
            }

            for (int i = 0; i < wktArray.Length; i++)
            {
                strBuild.Append(wktArray[i]);
                if (i != wktArray.Length - 1)
                {
                    strBuild.Append(",");
                }
            }

            LoadFromWellKnownData(strBuild.ToString());
        }

        /// <summary>Returns a shape from one location to another based on a distance and a direction in degrees.</summary>
        /// <remarks>As a concrete public method that wraps a Core method, Mapgenix reserves the right
        /// to add events and other logic returned by the Core version of the method.</remarks> 
        /// <returns>Shape from one location to another based on a distance and a direction in degrees.</returns>
        /// <param name="targetShape">Shape to move.</param>
        /// <param name="distance">Distance to move.</param>
        /// <param name="angleInDegrees">Direction in degrees from 0 to 360.</param>
        /// <param name="shapeUnit">Geography unit of the shape.</param>
        /// <param name="distanceUnit">Distance unit.</param> 
       static BaseShape TranslateByDegree(BaseShape targetShape, double distance, double angleInDegrees, GeographyUnit shapeUnit, DistanceUnit distanceUnit)
        {
            Validators.CheckParameterIsNotNull(targetShape, "targetShape");
            Validators.CheckIfInputValueIsInRange(distance, "distance", 0, RangeCheckingInclusion.IncludeValue, double.MaxValue, RangeCheckingInclusion.IncludeValue);
            Validators.CheckIfInputValueIsInRange(angleInDegrees, "angleInDegrees", 0, RangeCheckingInclusion.IncludeValue, 360, RangeCheckingInclusion.IncludeValue);
            Validators.CheckGeographyUnitIsValid(shapeUnit, "shapeUnit");
            Validators.CheckShapeIsValidForOperation(targetShape);

            BaseShape returnBaseShape = targetShape.CloneDeepCore();
            returnBaseShape.TranslateByDegree(distance, angleInDegrees, shapeUnit, distanceUnit);

            return returnBaseShape;
        }

       /// <summary>Returns a shape from one location to another based on a distance and a direction in degrees.</summary>
        public static BaseShape TranslateByDegree(Feature targetFeature, double distance, double angleInDegrees, GeographyUnit shapeUnit, DistanceUnit distanceUnit)
        {
            Validators.CheckIfInputValueIsInRange(distance, "distance", 0, RangeCheckingInclusion.IncludeValue, double.MaxValue, RangeCheckingInclusion.IncludeValue);
            Validators.CheckIfInputValueIsInRange(angleInDegrees, "angleInDegrees", 0, RangeCheckingInclusion.IncludeValue, 360, RangeCheckingInclusion.IncludeValue);
            Validators.CheckGeographyUnitIsValid(shapeUnit, "shapeUnit");
         
            BaseShape targetShape = targetFeature.GetShape();
            BaseShape returnBaseShape = targetShape.CloneDeepCore();
            returnBaseShape.TranslateByDegree(distance, angleInDegrees, shapeUnit, distanceUnit);

            return returnBaseShape;
        }

        /// <summary>Moves a shape from one location to another based on a distance and a direction in degrees.</summary>
        public void TranslateByDegree(double distance, double angleInDegrees, GeographyUnit shapeUnit, DistanceUnit distanceUnit)
        {
            Validators.CheckIfInputValueIsInRange(distance, "distance", 0, RangeCheckingInclusion.IncludeValue, double.MaxValue, RangeCheckingInclusion.IncludeValue);
            Validators.CheckIfInputValueIsInRange(angleInDegrees, "angleInDegrees", 0, RangeCheckingInclusion.IncludeValue, 360, RangeCheckingInclusion.IncludeValue);
            Validators.CheckGeographyUnitIsValid(shapeUnit, "shapeUnit");
            Validators.CheckShapeIsValidForOperation(this);

            TranslateByDegreeCore(distance, angleInDegrees, shapeUnit, distanceUnit);
        }

        /// <summary>Moves a shape from one location to another based on a distance and a direction in degrees.</summary>
        public void TranslateByDegree(double distance, double angleInDegrees)
        {
            Validators.CheckIfInputValueIsInRange(distance, "distance", 0, RangeCheckingInclusion.IncludeValue, double.MaxValue, RangeCheckingInclusion.IncludeValue);
            Validators.CheckIfInputValueIsInRange(angleInDegrees, "angleInDegrees", 0, RangeCheckingInclusion.IncludeValue, 360, RangeCheckingInclusion.IncludeValue);
            Validators.CheckShapeIsValidForOperation(this);

            TranslateByDegreeCore(distance, angleInDegrees, GeographyUnit.Meter, DistanceUnit.Meter);
        }

        /// <summary>Moves a shape from one location to another based on a distance and a direction in degrees.</summary>
        protected virtual void TranslateByDegreeCore(double distance, double angleInDegrees, GeographyUnit shapeUnit, DistanceUnit distanceUnit)
        {
            Validators.CheckIfInputValueIsInRange(angleInDegrees, "angleInDegrees", 0, 360);
            Validators.CheckGeographyUnitIsValid(shapeUnit, "shapeUnit");
            Validators.CheckShapeIsValidForOperation(this);
            Validators.CheckIfInputValueIsBiggerThan(distance, "distance", 0, RangeCheckingInclusion.IncludeValue);

            string[] wktArray = this.GetWellKnownText().Split(',');
            StringBuilder strBuild = new StringBuilder();

            for (int i = 0; i < wktArray.Length; i++)
            {
                string onePointWkt = wktArray[i];
                Vertex vertex = ShapeConverter.GetVertexFormWkt(onePointWkt);
                vertex.TranslateByDegree(distance, angleInDegrees, shapeUnit, distanceUnit);
                wktArray[i] = ShapeConverter.SetVertexToWkt(onePointWkt, vertex);
            }

            for (int i = 0; i < wktArray.Length; i++)
            {
                strBuild.Append(wktArray[i]);
                if (i != wktArray.Length)
                {
                    strBuild.Append(",");
                }
            }

            LoadFromWellKnownData(strBuild.ToString());
        }

        /// <summary>Returns a shape rotated by a number of degrees based on a pivot point.</summary>
        public static BaseShape Rotate(BaseShape sourceBaseShape, PointShape pivotPoint, float degreeAngle)
        {
            Validators.CheckParameterIsNotNull(sourceBaseShape, "sourceBaseShape");
            Validators.CheckIfInputValueIsInRange(degreeAngle, "degreeAngle", 0, RangeCheckingInclusion.IncludeValue, 360, RangeCheckingInclusion.IncludeValue);
            Validators.CheckParameterIsNotNull(pivotPoint, "pivotPoint");
            Validators.CheckShapeIsValidForOperation(sourceBaseShape);

            BaseShape returnBaseShape = sourceBaseShape.CloneDeepCore();
            returnBaseShape.Rotate(pivotPoint, degreeAngle);

            return returnBaseShape;
        }

        /// <summary>Returns a shape rotated by a number of degrees based on a pivot point.</summary>
        public static BaseShape Rotate(Feature targetFeature, PointShape pivotPoint, float degreeAngle)
        {
            Validators.CheckIfInputValueIsInRange(degreeAngle, "degreeAngle", 0, RangeCheckingInclusion.IncludeValue, 360, RangeCheckingInclusion.IncludeValue);
            Validators.CheckParameterIsNotNull(pivotPoint, "pivotPoint");

            BaseShape targetShape = targetFeature.GetShape();

            return Rotate(targetShape, pivotPoint, degreeAngle);
        }

        /// <summary>Rotates the shape by a number of degrees based on a pivot point.</summary>
        public void Rotate(PointShape pivotPoint, float degreeAngle)
        {
            Validators.CheckIfInputValueIsInRange(degreeAngle, "degreeAngle", 0, RangeCheckingInclusion.IncludeValue, 360, RangeCheckingInclusion.IncludeValue);
            Validators.CheckParameterIsNotNull(pivotPoint, "pivotPoint");
            Validators.CheckShapeIsValidForOperation(this);

            RotateCore(pivotPoint, degreeAngle);
        }

        /// <summary>Rotates the shape by a number of degrees based on a pivot point.</summary>
        protected virtual void RotateCore(PointShape pivotPoint, float degreeAngle)
        {
            Validators.CheckIfInputValueIsInRange(degreeAngle, "degreeAngle", 0, RangeCheckingInclusion.IncludeValue, 360, RangeCheckingInclusion.IncludeValue);
            Validators.CheckParameterIsNotNull(pivotPoint, "pivotPoint");
            Validators.CheckShapeIsValidForOperation(this);
           
            throw new NotSupportedException(ExceptionDescription.RotateNotSupported);
        }

        /// <summary>Gets whether the shape can be rotated.</summary>
        public virtual bool CanRotate
        {
            get
            {
                return false;
            }
        }

        /// <summary>Returns the shortest line between this shape and the target shape.</summary>
        public MultilineShape GetShortestLineTo(BaseShape targetShape, GeographyUnit shapeUnit)
        {
            Validators.CheckParameterIsNotNull(targetShape, "targetShape");
            Validators.CheckShapeIsValidForOperation(this);
            Validators.CheckParameterIsValid(this, "targetShape");
            Validators.CheckGeographyUnitIsValid(shapeUnit, "shapeUnit");

            return GetShortestLineToCore(targetShape, shapeUnit);
        }

        /// <summary>Returns the shortest line between the shape and the target feature.</summary>
        public MultilineShape GetShortestLineTo(Feature targetFeature, GeographyUnit shapeUnit)
        {
            Validators.CheckShapeIsValidForOperation(this);
            BaseShape targetShape = targetFeature.GetShape();

            return GetShortestLineToCore(targetShape, shapeUnit);
        }

        /// <summary>Returns the shortest line between the shape and the target shape.</summary>
        protected virtual MultilineShape GetShortestLineToCore(BaseShape targetShape, GeographyUnit shapeUnit)
        {
            Validators.CheckGeographyUnitIsValid(shapeUnit, "shapeUnit");
            Validators.CheckParameterIsNotNull(targetShape, "targetShape");
            Validators.CheckShapeIsValidForOperation(this);
            Validators.CheckParameterIsValid(targetShape, "targetShape");

            MultilineShape multiline = null;

            if (targetShape.Intersects(this))
            {
                return multiline;
            }

            PointShape thisPoint = this.GetClosestPointTo(targetShape, shapeUnit);
            PointShape targetPoint = targetShape.GetClosestPointTo(this, shapeUnit);

            if (thisPoint == null || targetPoint == null)
            {
                return multiline;
            }

            if (shapeUnit == GeographyUnit.DecimalDegree)
            {
                multiline = DecimalDegreesHelper.GetGreatCircle(thisPoint, targetPoint, 1000);
            }
            else
            {
                multiline = new MultilineShape();
                LineShape line = new LineShape();
                line.Vertices.Add(new Vertex(thisPoint.X, thisPoint.Y));
                line.Vertices.Add(new Vertex(targetPoint.X, targetPoint.Y));
                multiline.Lines.Add(line);
            }

            return multiline;
        }

        /// <summary>Returns the point on  the shape closest to the target shape.</summary>
        public PointShape GetClosestPointTo(BaseShape targetShape, GeographyUnit shapeUnit)
        {
            Validators.CheckParameterIsNotNull(targetShape, "targetShape");
            Validators.CheckShapeIsValidForOperation(this);
            Validators.CheckParameterIsValid(targetShape, "targetShape");
            Validators.CheckGeographyUnitIsValid(shapeUnit, "shapeUnit");

            return GetClosestPointToCore(targetShape, shapeUnit);
        }

        /// <summary>Returns the point on  the shape closest to the target feature.</summary>
        public PointShape GetClosestPointTo(Feature targetFeature, GeographyUnit shapeUnit)
        {
            Validators.CheckShapeIsValidForOperation(this);
            BaseShape targetShape = targetFeature.GetShape();

            return GetClosestPointToCore(targetShape, shapeUnit);
        }

        /// <summary>Returns the point on  the shape closest to the target shape.</summary>
        protected virtual PointShape GetClosestPointToCore(BaseShape targetShape, GeographyUnit shapeUnit)
        {
            Validators.CheckParameterIsNotNull(targetShape, "targetShape");
            Validators.CheckShapeIsValidForOperation(this);
            Validators.CheckParameterIsValid(targetShape, "targetShape");
            Validators.CheckGeographyUnitIsValid(shapeUnit, "shapeUnit");

            string wkt = this.GetWellKnownText();
            BaseShape baseShape = BaseShape.CreateShapeFromWellKnownData(wkt);
            return baseShape.GetClosestPointToCore(targetShape, shapeUnit);
        }

        /// <summary>Returns the center point of the shape's bounding box.</summary>
        public PointShape GetCenterPoint()
        {
            Validators.CheckShapeIsValidForOperation(this);

            return GetCenterPointCore();
        }

        /// <summary>Returns the center point of the shape's bounding box.</summary>
        protected virtual PointShape GetCenterPointCore()
        {
            Validators.CheckShapeIsValidForOperation(this);
            Geometry geometry = ShapeConverter.ShapeToJtsShape(this);

            Point centerPoint = (Point)geometry.Centroid;

            PointShape result = new PointShape(centerPoint.X, centerPoint.Y);
            if (double.IsNaN(centerPoint.X) || double.IsNaN(centerPoint.Y))
            {
                result = GetBoundingBoxCore().GetCenterPoint();
            }

            return result;
        }

        /// <summary>Returns the area within a given distance from the shape.</summary>
        public MultipolygonShape Buffer(double distance, GeographyUnit shapeUnit, DistanceUnit distanceUnit)
        {
            Validators.CheckGeographyUnitIsValid(shapeUnit, "shapeUnit");
            Validators.CheckShapeIsValidForOperation(this);

            return BufferCore(distance, DefaultQuadrantSegments, BufferCapType.Round, shapeUnit, distanceUnit);
        }

        /// <summary>Returns the area within a given distance from the shape.</summary>
        public MultipolygonShape Buffer(double distance, int quadrantSegments, GeographyUnit shapeUnit, DistanceUnit distanceUnit)
        {
            Validators.CheckIfInputValueIsInRange(quadrantSegments, "quadrantSegments", 3, RangeCheckingInclusion.IncludeValue, 100, RangeCheckingInclusion.IncludeValue);
            Validators.CheckGeographyUnitIsValid(shapeUnit, "shapeUnit");
            Validators.CheckShapeIsValidForOperation(this);

            return BufferCore(distance, quadrantSegments, BufferCapType.Round, shapeUnit, distanceUnit);
        }

        /// <summary>Returns the area within a given distance from the shape.</summary>
        public MultipolygonShape Buffer(double distance, int quadrantSegments, BufferCapType bufferCapType, GeographyUnit shapeUnit, DistanceUnit distanceUnit)
        {
            Validators.CheckIfInputValueIsInRange(quadrantSegments, "quadrantSegments", 3, RangeCheckingInclusion.IncludeValue, 100, RangeCheckingInclusion.IncludeValue);
            Validators.CheckGeographyUnitIsValid(shapeUnit, "shapeUnit");
            Validators.CheckShapeIsValidForOperation(this);

            return BufferCore(distance, quadrantSegments, bufferCapType, shapeUnit, distanceUnit);
        }

        /// <summary>Returns the area within a given distance from the shape.</summary>
        protected virtual MultipolygonShape BufferCore(double distance, int quadrantSegments, BufferCapType bufferCapType, GeographyUnit shapeUnit, DistanceUnit distanceUnit)
        {
            Validators.CheckIfInputValueIsInRange(quadrantSegments, "quadrantSegments", 3, RangeCheckingInclusion.IncludeValue, 100, RangeCheckingInclusion.IncludeValue);
            Validators.CheckShapeIsValidForOperation(this);
            Validators.CheckGeographyUnitIsValid(shapeUnit, "shapeUnit");
         
            if (shapeUnit == GeographyUnit.DecimalDegree)
            {
                PointShape certerPoint = GetCenterPoint();
                distance = DecimalDegreesHelper.GetLongitudeDifferenceFromDistance(distance, distanceUnit, certerPoint.Y);
            }
            else
            {
                DistanceUnit unit = Conversion.ConvertGeographyUnitToDistanceUnit(shapeUnit);
                distance = Conversion.ConvertMeasureUnits(distance, distanceUnit, unit);
            }

            Geometry geometry = ShapeConverter.ShapeToJtsShape(this);
            BufferStyle bufferStyles = GetBufferStyles(bufferCapType);
            Geometry resultGeomery = (Geometry)geometry.Buffer(distance, quadrantSegments, bufferStyles);
            
            MultipolygonShape bufferedShape = null;
            if (resultGeomery.GetType().Name == "GeometryCollection")
            {
                return bufferedShape;
            }

            BaseShape resultShape = ShapeConverter.JtsShapeToShape(resultGeomery);

            switch (resultShape.GetType().Name)
            {
                case "PolygonShape":
                    bufferedShape = new MultipolygonShape();
                    bufferedShape.Polygons.Add((PolygonShape)resultShape);
                    break;

                case "MultipolygonShape":
                    bufferedShape = (MultipolygonShape)resultShape;
                    break;
                default:
                    break;
            }

            bufferedShape.Id = this.id;

            return bufferedShape;
        }

        /// <summary>Returns the distance between the shape and a target shape.</summary>
        public double GetDistanceTo(BaseShape targetShape, GeographyUnit shapeUnit, DistanceUnit distanceUnit)
        {
            Validators.CheckParameterIsNotNull(targetShape, "targetShape");
            Validators.CheckShapeIsValidForOperation(this);
            Validators.CheckParameterIsValid(targetShape, "targetShape");
            Validators.CheckGeographyUnitIsValid(shapeUnit, "shapeUnit");

            return GetDistanceToCore(targetShape, shapeUnit, distanceUnit);
        }

        /// <summary>Returns the distance between the shape and a target shape.</summary>
        public double GetDistanceTo(Feature targetFeature, GeographyUnit shapeUnit, DistanceUnit distanceUnit)
        {
            Validators.CheckGeographyUnitIsValid(shapeUnit, "shapeUnit");
            Validators.CheckShapeIsValidForOperation(this);

            BaseShape targetShape = targetFeature.GetShape();

            return GetDistanceToCore(targetShape, shapeUnit, distanceUnit);
        }

        /// <summary>Returns the distance between the shape and a target shape.</summary>
        protected virtual double GetDistanceToCore(BaseShape targetShape, GeographyUnit shapeUnit, DistanceUnit distanceUnit)
        {
            Validators.CheckGeographyUnitIsValid(shapeUnit, "shapeUnit");
            Validators.CheckParameterIsNotNull(targetShape, "targetShape");
            Validators.CheckParameterIsValid(targetShape, "targetShape");
            Validators.CheckShapeIsValidForOperation(this);

            double distance = 0;

            Geometry sourceGeometry = ShapeConverter.ShapeToJtsShape(this);
            Geometry targetGeometry = ShapeConverter.ShapeToJtsShape(targetShape);
            if (shapeUnit != GeographyUnit.DecimalDegree)
            {
                distance = sourceGeometry.Distance(targetGeometry);
                DistanceUnit shapeDistanceUnit = Conversion.ConvertGeographyUnitToDistanceUnit(shapeUnit);
                distance = Conversion.ConvertMeasureUnits(distance, shapeDistanceUnit, distanceUnit);
            }
            else
            {
                throw new NotSupportedException("Currently DecimalDegree are not supported.");
            }

            return distance;
        }

        /// <summary>Returns a shape registered from its original coordinate system to another based on two anchor points.</summary>
        public BaseShape Register(PointShape fromPoint, PointShape toPoint, DistanceUnit fromUnit, GeographyUnit toUnit)
        {
            Validators.CheckGeographyUnitIsValid(toUnit, "toUnit");
            Validators.CheckParameterIsNotNull(toPoint, "toPoint");
            Validators.CheckParameterIsNotNull(fromPoint, "fromPoint");
            Validators.CheckShapeIsValidForOperation(this);

            return RegisterCore(fromPoint, toPoint, fromUnit, toUnit);
        }

        /// <summary>Returns a shape registered from its original coordinate system to another based on two anchor points.</summary>
        public BaseShape Register(Feature fromPoint, Feature toPoint, DistanceUnit fromUnit, GeographyUnit toUnit)
        {
            Validators.CheckGeographyUnitIsValid(toUnit, "toUnit");
            Validators.CheckShapeIsValidForOperation(this);

            BaseShape fromShape = fromPoint.GetShape();
            BaseShape toShape = toPoint.GetShape();

            Validators.CheckShapeIsPointShape(fromShape);
            Validators.CheckShapeIsPointShape(toShape);

            return Register((PointShape)fromShape, (PointShape)toShape, fromUnit, toUnit);
        }

        /// <summary>Returns a shape registered from its original coordinate system to another based on two anchor points.</summary>
        protected virtual BaseShape RegisterCore(PointShape fromPoint, PointShape toPoint, DistanceUnit fromUnit, GeographyUnit toUnit)
        {
            Validators.CheckGeographyUnitIsValid(toUnit, "toUnit");

            string wkt = this.GetWellKnownText();
            BaseShape wktBaseShape = BaseShape.CreateShapeFromWellKnownData(wkt);
            BaseShape registerShape = wktBaseShape.RegisterCore(fromPoint, toPoint, fromUnit, toUnit);
            this.LoadFromWellKnownData(registerShape.GetWellKnownText());
            return this;
        }

        /// <summary>Returns a ShapeValidationResult based on a validation tests.</summary>
        public ShapeValidationResult Validate(ShapeValidationMode validationMode)
        {
         
            return ValidateCore(validationMode);
        }

        /// <summary>Returns a ShapeValidationResult based on a validation tests.</summary>
        protected virtual ShapeValidationResult ValidateCore(ShapeValidationMode validationMode)
        {
            ShapeValidationResult validationResult = new ShapeValidationResult(true, String.Empty);

            string wktStr = GetWellKnownTextCore();
            BaseShape tmpBaseShape = null;
            try
            {
                tmpBaseShape = BaseShape.CreateShapeFromWellKnownData(wktStr);
            }
            catch (ArgumentException)
            {
                return new ShapeValidationResult(false, ExceptionDescription.WkbIsInvalid);
            }

            WellKnownType shapeType = tmpBaseShape.GetWellKnownType();
            switch (shapeType)
            {
                case WellKnownType.Point:
                    break;

                case WellKnownType.Line:
                    if (((LineShape)tmpBaseShape).Vertices.Count < 2)
                    {
                        validationResult = new ShapeValidationResult(false, ExceptionDescription.ShapeIsInvalidForOperation);
                    }
                    break;

                case WellKnownType.Polygon:
                    PolygonShape polygonShape = (PolygonShape)tmpBaseShape;
                    validationResult = polygonShape.OuterRing.Validate(validationMode);
                    if (!validationResult.IsValid)
                    {
                        break;
                    }

                    foreach (RingShape innerRing in polygonShape.InnerRings)
                    {
                        validationResult = innerRing.Validate(validationMode);
                        if (!validationResult.IsValid)
                        {
                            break;
                        }
                    }
                    break;

                case WellKnownType.Multipoint:
                    break;

                case WellKnownType.Multiline:
                    MultilineShape multilineShape = (MultilineShape)tmpBaseShape;

                    if (multilineShape.Lines.Count == 0)
                    {
                        validationResult = new ShapeValidationResult(false, ExceptionDescription.ShapeIsInvalidForOperation);
                    }
                    else
                    {
                        foreach (LineShape lineShape in multilineShape.Lines)
                        {
                            validationResult = lineShape.Validate(validationMode);
                            if (!validationResult.IsValid)
                            {
                                break;
                            }
                        }
                    }
                    break;

                case WellKnownType.Multipolygon:
                    MultipolygonShape multiPolygonShape = (MultipolygonShape)tmpBaseShape;

                    if (multiPolygonShape.Polygons.Count == 0)
                    {
                        validationResult = new ShapeValidationResult(false, ExceptionDescription.ShapeIsInvalidForOperation);
                    }
                    foreach (PolygonShape polygon in multiPolygonShape.Polygons)
                    {
                        validationResult = polygon.Validate(validationMode);
                        if (!validationResult.IsValid)
                        {
                            break;
                        }
                    }
                    break;

                default:
                    break;
            }

            return validationResult;
        }

        /// <summary>Returns true if the shape and a target shape have no points in common.</summary>
        public bool IsDisjointed(BaseShape targetShape)
        {
            Validators.CheckParameterIsNotNull(targetShape, "targetShape");
            Validators.CheckShapeIsValidForOperation(this);
            Validators.CheckParameterIsValid(targetShape, "targetShape");

            return IsDisjointedCore(targetShape);
        }

        /// <summary>Returns true if the shape and a target feature's shape have no points in common.</summary>
        public bool IsDisjointed(Feature targetFeature)
        {
            Validators.CheckShapeIsValidForOperation(this);
            BaseShape targetShape = targetFeature.GetShape();

            return IsDisjointedCore(targetShape);
        }

        /// <summary>Returns true if the shape and a target shape have no points in common.</summary>
        protected virtual bool IsDisjointedCore(BaseShape targetShape)
        {
            Validators.CheckParameterIsNotNull(targetShape, "targetShape");
            Validators.CheckShapeIsValidForOperation(this);
            Validators.CheckParameterIsValid(targetShape, "targetShape");

            bool IsDisjointed = false;

            Geometry geometry01 = ShapeConverter.ShapeToJtsShape(this);
            Geometry geometry02 = ShapeConverter.ShapeToJtsShape(targetShape);
            IsDisjointed = geometry01.Disjoint(geometry02);

            return IsDisjointed;
        }

        /// <summary>Returns true if the shape and the target shape have at least one point in common.</summary>
        public bool Intersects(BaseShape targetShape)
        {
            Validators.CheckParameterIsNotNull(targetShape, "targetShape");
            Validators.CheckShapeIsValidForOperation(this);
            Validators.CheckParameterIsValid(targetShape, "targetShape");

            return IntersectsCore(targetShape);
        }

        /// <summary>Returns true if the shape and the target shape have at least one point in common.</summary>
        public bool Intersects(Feature targetFeature)
        {
            Validators.CheckShapeIsValidForOperation(this);
            BaseShape targetShape = targetFeature.GetShape();

            return IntersectsCore(targetShape);
        }

        /// <summary>Returns true if the shape and a target shape have at least one point in common.</summary>
        protected virtual bool IntersectsCore(BaseShape targetShape)
        {
            Validators.CheckParameterIsNotNull(targetShape, "targetShape");
            Validators.CheckShapeIsValidForOperation(this);
            Validators.CheckParameterIsValid(targetShape, "targetShape");

            Geometry thisGeometry = ShapeConverter.ShapeToJtsShape(this);
            Geometry targetGeometry = ShapeConverter.ShapeToJtsShape(targetShape);

            return thisGeometry.Intersects(targetGeometry);
        }

        /// <summary>Returns true if the shape and a target shape have at least one
        /// boundary point in common, but no interior points.</summary>
        public bool Touches(BaseShape targetShape)
        {
            Validators.CheckParameterIsNotNull(targetShape, "targetShape");
            Validators.CheckShapeIsValidForOperation(this);
            Validators.CheckParameterIsValid(targetShape, "targetShape");

            return TouchesCore(targetShape);
        }

        /// <summary>Returns true if the shape and the target feature's shape have at least one
        /// boundary point in common, but no interior points.</summary>
        public bool Touches(Feature targetFeature)
        {
            Validators.CheckShapeIsValidForOperation(this);
            BaseShape targetShape = targetFeature.GetShape();

            return TouchesCore(targetShape);
        }

        /// <summary>Returns true if the shape and a target shape have at least one
        /// boundary point in common, but no interior points.</summary>
        protected virtual bool TouchesCore(BaseShape targetShape)
        {
            Validators.CheckParameterIsNotNull(targetShape, "targetShape");
            Validators.CheckShapeIsValidForOperation(this);
            Validators.CheckParameterIsValid(targetShape, "targetShape");

            Geometry thisGeometry = ShapeConverter.ShapeToJtsShape(this);
            Geometry targetGeometry = ShapeConverter.ShapeToJtsShape(targetShape);

            return thisGeometry.Touches(targetGeometry);
        }

        /// <summary>Returns true if the shape and a target shape share some but not
        /// all interior points.</summary>
        public bool Crosses(BaseShape targetShape)
        {
            Validators.CheckParameterIsNotNull(targetShape, "targetShape");
            Validators.CheckShapeIsValidForOperation(this);
            Validators.CheckParameterIsValid(targetShape, "targetShape");

            return CrossesCore(targetShape);
        }

        /// <summary>Returns true if the shape and a target feature's shape share some but not
        /// all interior points.</summary>
        public bool Crosses(Feature targetFeature)
        {
            Validators.CheckShapeIsValidForOperation(this);
            BaseShape targetShape = targetFeature.GetShape();

            return CrossesCore(targetShape);
        }

        /// <summary>Returns true if the shape and a target shape share some but not
        /// all interior points.</summary>
        protected virtual bool CrossesCore(BaseShape targetShape)
        {
            Validators.CheckParameterIsNotNull(targetShape, "targetShape");
            Validators.CheckShapeIsValidForOperation(this);
            Validators.CheckParameterIsValid(targetShape, "targetShape");

            Geometry thisGeometry = ShapeConverter.ShapeToJtsShape(this);
            Geometry targetGeometry = ShapeConverter.ShapeToJtsShape(targetShape);

            return thisGeometry.Crosses(targetGeometry);
        }

        /// <summary>Returns true if the shape lies within the interior of a
        /// target shape.</summary>
        public bool IsWithin(BaseShape targetShape)
        {
            Validators.CheckParameterIsNotNull(targetShape, "targetShape");
            Validators.CheckShapeIsValidForOperation(this);
            Validators.CheckParameterIsValid(targetShape, "targetShape");

            return IsWithinCore(targetShape);
        }

        /// <summary>Returns true if the shape lies within the interior of a
        /// target feature's shape.</summary>
        public bool IsWithin(Feature targetFeature)
        {
            Validators.CheckShapeIsValidForOperation(this);
            BaseShape targetShape = targetFeature.GetShape();

            return IsWithinCore(targetShape);
        }

        /// <summary>Returns true if the shape lies within the interior of a
        /// target shape.</summary>
        protected virtual bool IsWithinCore(BaseShape targetShape)
        {
            Validators.CheckParameterIsNotNull(targetShape, "targetShape");
            Validators.CheckShapeIsValidForOperation(this);
            Validators.CheckParameterIsValid(targetShape, "targetShape");

            Geometry thisGeometry = ShapeConverter.ShapeToJtsShape(this);
            Geometry targetGeometry = ShapeConverter.ShapeToJtsShape(targetShape);

            return thisGeometry.Within(targetGeometry);
        }

        /// <summary>Returns true if a target shape lies within the interior of the shape.</summary>
        public bool Contains(BaseShape targetShape)
        {
            Validators.CheckParameterIsNotNull(targetShape, "targetShape");
            Validators.CheckShapeIsValidForOperation(this);
            Validators.CheckParameterIsValid(targetShape, "targetShape");

            return ContainsCore(targetShape);
        }

        /// <summary>Returns true if a target feature's shape lies within the interior of the shape.</summary>
        public bool Contains(Feature targetFeature)
        {
            Validators.CheckShapeIsValidForOperation(this);
            BaseShape targetShape = targetFeature.GetShape();

            return ContainsCore(targetShape);
        }

        /// <summary>Returns true if a target shape lies within the interior of the shape.</summary>
        protected virtual bool ContainsCore(BaseShape targetShape)
        {
            Validators.CheckParameterIsNotNull(targetShape, "targetShape");
            Validators.CheckShapeIsValidForOperation(this);
            Validators.CheckParameterIsValid(targetShape, "targetShape");

            Geometry thisGeometry = ShapeConverter.ShapeToJtsShape(this);
            Geometry targetGeometry = ShapeConverter.ShapeToJtsShape(targetShape);

            return thisGeometry.Contains(targetGeometry);
        }

        /// <summary>Returns true if the  shape and a target shape share some but not
        /// all points in common.</summary>
        public bool Overlaps(BaseShape targetShape)
        {
            Validators.CheckParameterIsNotNull(targetShape, "targetShape");
            Validators.CheckShapeIsValidForOperation(this);
            Validators.CheckParameterIsValid(targetShape, "targetShape");

            return OverlapsCore(targetShape);
        }

        /// <summary>Returns true if the  shape and a target feature's shape share some but not
        /// all points in common.</summary>
        public bool Overlaps(Feature targetFeature)
        {
            Validators.CheckShapeIsValidForOperation(this);
            BaseShape targetShape = targetFeature.GetShape();

            return OverlapsCore(targetShape);
        }

        /// <summary>Returns true if the  shape and a target shape share some but not
        /// all points in common.</summary>
        protected virtual bool OverlapsCore(BaseShape targetShape)
        {
            Validators.CheckParameterIsNotNull(targetShape, "targetShape");
            Validators.CheckShapeIsValidForOperation(this);
            Validators.CheckParameterIsValid(targetShape, "targetShape");

            Geometry thisGeometry = ShapeConverter.ShapeToJtsShape(this);
            Geometry targetGeometry = ShapeConverter.ShapeToJtsShape(targetShape);

            return thisGeometry.Overlaps(targetGeometry);
        }

        /// <summary>Returns true if the shape and a target shape are topologically equal.</summary>
        public bool IsTopologicallyEqual(BaseShape targetShape)
        {
            Validators.CheckParameterIsNotNull(targetShape, "targetShape");
            Validators.CheckShapeIsValidForOperation(this);
            Validators.CheckParameterIsValid(targetShape, "targetShape");

            return IsTopologicallyEqualCore(targetShape);
        }

        /// <summary>Returns true if the shape and a target feature's shape are topologically equal.</summary>
        public bool IsTopologicallyEqual(Feature targetFeature)
        {
            Validators.CheckShapeIsValidForOperation(this);
            BaseShape targetShape = targetFeature.GetShape();

            return IsTopologicallyEqualCore(targetShape);
        }

        /// <summary>Returns true if the shape and a target shape are topologically equal.</summary>
        protected virtual bool IsTopologicallyEqualCore(BaseShape targetShape)
        {
            Validators.CheckParameterIsNotNull(targetShape, "targetShape");
            Validators.CheckShapeIsValidForOperation(this);
            Validators.CheckParameterIsValid(targetShape, "targetShape");

            Geometry thisGeometry = ShapeConverter.ShapeToJtsShape(this);
            Geometry targetGeometry = ShapeConverter.ShapeToJtsShape(targetShape);

            return thisGeometry.Equals(targetGeometry);
        }

        /// <summary>Gets a feature with the same Id and shape as the current shape.</summary>
        public Feature GetFeature()
        {
            Validators.CheckShapeIsValidForOperation(this);

            return GetFeature(new Dictionary<string, string>());
        }

        /// <summary>Gets a feature with the same Id and shape as the current shape.</summary>
        public Feature GetFeature(IDictionary<string, string> columnValues)
        {
            Validators.CheckParameterIsNotNull(columnValues, "columnValues");
            Validators.CheckShapeIsValidForOperation(this);

            Feature returnValue = new Feature(this, columnValues);
            returnValue.Tag = tag;
            return returnValue;
        }

        /// <summary>Returns the crossing points between the shape and a target shape.</summary>
        public MultipointShape GetCrossing(BaseShape targetShape)
        {
            Validators.CheckParameterIsNotNull(targetShape, "targetShape");
            Validators.CheckShapeIsValidForOperation(this);

            return GetCrossingCore(targetShape);
        }

        /// <summary>Returns the crossing points between the shape and a target shape.</summary>
        protected virtual MultipointShape GetCrossingCore(BaseShape targetShape)
        {
            Validators.CheckParameterIsNotNull(targetShape, "targetShape");
            Validators.CheckShapeIsValidForOperation(this);

            return null;
        }

        /// <summary>Returns a shape with the size increased or decreased Increases or decreases by a scale.</summary>
        public static BaseShape ScaleTo(BaseShape baseShape, double scale)
        {
            Validators.CheckParameterIsNotNull(baseShape, "baseShape");
            Validators.CheckValueIsBiggerThanZero(scale, "scaleFactor");

            baseShape.ScaleTo(scale);

            return baseShape;
        }

        /// <summary>Increases or decreases the size of the shape by a scale factor.</summary>
        public void ScaleTo(double scale)
        {
            Validators.CheckValueIsBiggerThanZero(scale, "scaleFactor");

            ScaleToCore(scale);
        }

        /// <summary>Increases or decreases the size of the shape by a scale factor.</summary>
        protected virtual void ScaleToCore(double scale)
        {
            Validators.CheckValueIsBiggerThanZero(scale, "scaleFactor");

            if (scale >= 1)
            {
                if (this is RectangleShape)
                {
                    ((RectangleShape)this).ScaleUp((scale - 1) * 100);
                }
                else if (this is EllipseShape)
                {
                    ((EllipseShape)this).ScaleUp((scale - 1) * 100);
                }
                else if (this is RingShape)
                {
                    ((RingShape)this).ScaleUp((scale - 1) * 100);
                }
                else
                {
                    switch (GetWellKnownType())
                    {
                        case WellKnownType.Multipoint:
                            ((MultipointShape)this).ScaleUp((scale - 1) * 100);
                            break;
                        case WellKnownType.Line:
                            ((LineShape)this).ScaleUp((scale - 1) * 100);
                            break;
                        case WellKnownType.Multiline:
                            ((MultilineShape)this).ScaleUp((scale - 1) * 100);
                            break;
                        case WellKnownType.Polygon:
                            ((PolygonShape)this).ScaleUp((scale - 1) * 100);
                            break;
                        case WellKnownType.Multipolygon:
                            ((MultipolygonShape)this).ScaleUp((scale - 1) * 100);
                            break;
                        default:
                            break;
                    }
                }
            }
            else
            {
                if (this is RectangleShape)
                {
                    ((RectangleShape)this).ScaleDown((1 - scale) * 100);
                }
                else if (this is EllipseShape)
                {
                    ((EllipseShape)this).ScaleDown((1 - scale) * 100);
                }
                else if (this is RingShape)
                {
                    ((RingShape)this).ScaleDown((1 - scale) * 100);
                }
                else
                {
                    switch (GetWellKnownType())
                    {
                        case WellKnownType.Multipoint:
                            ((MultipointShape)this).ScaleDown((1 - scale) * 100);
                            break;
                        case WellKnownType.Line:
                            ((LineShape)this).ScaleDown((1 - scale) * 100);
                            break;
                        case WellKnownType.Multiline:
                            ((MultilineShape)this).ScaleDown((1 - scale) * 100);
                            break;
                        case WellKnownType.Polygon:
                            ((PolygonShape)this).ScaleDown((1 - scale) * 100);
                            break;
                        case WellKnownType.Multipolygon:
                            ((MultipolygonShape)this).ScaleDown((1 - scale) * 100);
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        private static BufferStyle GetBufferStyles(BufferCapType bufferCapStyle)
        {
            BufferStyle capStyle = BufferStyle.CapButt;
            switch (bufferCapStyle)
            {
                case BufferCapType.Round:
                    capStyle = BufferStyle.CapRound;
                    break;
                case BufferCapType.Square:
                    capStyle = BufferStyle.CapSquare;
                    break;
                case BufferCapType.Butt:
                    capStyle = BufferStyle.CapButt;
                    break;
                default:
                    break;
            }

            return capStyle;
        }

        private static ByteOrder GetByteOrderFromWkbByteOrder(WkbByteOrder wkbByteOrder)
        {
            ByteOrder byteOrder = ByteOrder.LittleEndian;

            if (wkbByteOrder == WkbByteOrder.BigEndian)
            {
                byteOrder = ByteOrder.BigEndian;
            }

            return byteOrder;
        }
    }
}
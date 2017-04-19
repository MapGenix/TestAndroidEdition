using System;
using System.Collections.Generic;
using Mapgenix.Canvas.Properties;
using Mapgenix.Shapes;
using Mapgenix.Utils;

namespace Mapgenix.Canvas
{
    /// <summary>Represents a cartographic projection to apply to BaseFeatureSource.</summary>
    /// <remarks>Sub projection classes are used on a BaseFeatureSource. Used to project coordinates from one projection to another and the other way around.
    /// Immplementation in both ways is necessary.</remarks>
    [Serializable]
    public abstract class BaseProjection
    {
        private const int PositionByteOrder = 0;
        private bool _isOpen;

        /// <summary>Gets the state of the projection (whether it is opened).</summary>
        /// <remarks>Reflects whether the projection is opened or closed. It is set in the
        /// concrete methods Open and Close, so if the sub class overrides Open and Close methods, it is not necessary to set this property.</remarks>
        public bool IsOpen
        {
            get { return _isOpen; }
            protected set { _isOpen = value; }
        }

        /// <summary>Indicates whether this projection should be used.</summary>
        protected virtual bool CanReproject
        {
            get { return true; }
        }

        /// <summary>Creates a copy of Projection using the deep clone technique.</summary>
        /// <returns>Cloned Projection.</returns>
        /// <remarks>Deep cloning copies the cloned object and all the objects within it.
        /// </remarks>
        public BaseProjection CloneDeep()
        {
            return CloneDeepCore();
        }

        /// <summary>Creates a copy of Projection using the deep clone technique.</summary>
        /// <returns>Cloned Projection.</returns>
        /// <remarks>Deep cloning copies the cloned object and all the objects within it.
        /// </remarks>
        protected virtual BaseProjection CloneDeepCore()
        {
            return SerializationHelper.CloneDeep(this);
        }

        /// <summary>Opens the projection and gets it ready to use.</summary>
        /// <remarks>Opens the projection and gets it ready to use.<br/><br/>
        /// As a concrete public method that wraps a Core method, events and other logic to pre- or post-process data returned by the Core version
        /// of the method can be added in the future.</remarks>
        /// <returns>None</returns>
        public void Open()
        {
            if (!_isOpen)
            {
                _isOpen = true;

                OpenCore();
            }
        }

        /// <returns>None</returns>
        /// <remarks>Core version of the Open method. To be overridden in an
        /// inherited version of the class. When overriding, getting the
        /// projection classes state ready for doing projections is necessary.</remarks>
        /// <summary>Opens the projection and gets it ready to use.</summary>
        protected virtual void OpenCore()
        {
        }

        /// <summary>Closes the projection and gets it ready for serialization if necessary.</summary>
        /// <returns>None</returns>
        /// <remarks> As a concrete public method that wraps a Core method, events and other logic to pre- or post-process data returned by the Core version
        /// of the method can be added in the future.</remarks>
        /// <returns>None</returns>
        public void Close()
        {
            if (_isOpen)
            {
                _isOpen = false;

                CloseCore();
            }
        }

        /// <summary>Closes the projection and gets it ready for serialization if necessary.</summary>
        /// <returns>None</returns>
        /// <remarks>
        /// Being the core version of the Close method, it is intended to be overridden in sub class. When overriding, it is necessary to free any
        /// state that are maintained and getting the class ready for serialization.
        /// Note that the object may be opened again, so to make sure it can open and close multiple times without any unintended effects.</remarks>
        protected virtual void CloseCore()
        {
        }

        /// <summary>Returns a projected vertex based on the coordinates passed in.</summary>
        /// <overloads>Projects a set of coordinates passed in as X and Y.</overloads>
        /// <returns>Projected vertex based on the coordinates passed in.</returns>
        /// <remarks>Returns a projected vertex based on the coordinates passed in.<br/><br/>
        /// As a concrete public method that wraps a Core method, events and other logic to pre- or post-process data returned by the Core version
        /// of the method can be added in the future.</remarks>
        /// </remarks>
        /// <param name="x">This parameter is the X of the point that will be projected.</param>
        /// <param name="y">This parameter is the Y of the point that will be projected.</param>
        public Vertex ConvertToExternalProjection(double x, double y)
        {
            return ConvertToExternalProjectionCore(new double[1] {x}, new double[1] {y})[0];
        }

        /// <summary>Returns a projected BaseShape based on the coordinates of a shape passed in.</summary>
        /// <returns>Projected BaseShape of a Feature passed in.</returns>
        /// <remarks>Returns a projected baseShape based on the BaseShape passed in.<br/><br/>
        /// As a concrete public method that wraps a Core method, events and other logic to pre- or post-process data returned by the Core version
        /// of the method can be added in the future.</remarks>
        /// </remarks>
        /// <param name="baseShape">BaseShape to be projected.</param>
        public BaseShape ConvertToExternalProjection(BaseShape baseShape)
        {
            Validators.CheckParameterIsNotNull(baseShape, "baseShape");
            Validators.CheckShapeIsValidForOperation(baseShape);

            var projectedFeature = new Feature(baseShape);
            UpdateToExternalProjection(projectedFeature);
            return projectedFeature.GetShape();
        }

        /// <summary>Returns a projected Feature based on the coordinates of a Feature passed in.</summary>
        /// <returns>Projected Feature of a Feature passed in.</returns>
        /// <remarks>Returns a projected Feature based on a Feature passed in.<br/><br/>
        /// Actually, what is reprojected is the BaseShape of the Feature.
        /// As a concrete public method that wraps a Core method, events and other logic to pre- or post-process data returned by the Core version
        /// of the method can be added in the future.</remarks>
        /// </remarks>
        /// <param name="feature">Feature that contains a BaseShape in world coordinates to be projected.
        /// </param>
        public Feature ConvertToExternalProjection(Feature feature)
        {
            if (CanReproject)
            {
                Validators.CheckProjectionIsOpen(_isOpen);
                Validators.CheckFeatureIsValid(feature);

                var wellKnownBinary = feature.GetWellKnownBinary();
                var projectedWellKnownBinary = new byte[wellKnownBinary.Length];
                Buffer.BlockCopy(wellKnownBinary, 0, projectedWellKnownBinary, 0, wellKnownBinary.Length);

                projectedWellKnownBinary = ConvertToExternalProjection(projectedWellKnownBinary);
                var projectedFeature = new Feature(projectedWellKnownBinary, feature.Id, feature.ColumnValues);

                projectedFeature.Tag = feature.Tag;
                return projectedFeature;
            }
            return feature;
        }

        public void UpdateToExternalProjection(Feature feature)
        {
            if (CanReproject)
            {
                Validators.CheckProjectionIsOpen(_isOpen);
                Validators.CheckFeatureIsValid(feature);

                var wellKnownBinary = feature.GetWellKnownBinary();
                ConvertToExternalProjection(wellKnownBinary);
            }
        }

       
        /// <summary>Returns a projected rectangle based on a rectangle.</summary>
        /// <overloads>This overload takes in a rectangle to project.</overloads>
        /// <returns>Projected rectangle based on a rectangle passed in.</returns>
        /// <param name="rectangleShape">Rectangle to project.</param>
        public RectangleShape ConvertToExternalProjection(RectangleShape rectangleShape)
        {
            Validators.CheckProjectionIsOpen(_isOpen);
            Validators.CheckParameterIsNotNull(rectangleShape, "rectangleShape");
            Validators.CheckShapeIsValidForOperation(rectangleShape);

            var xCoordinates = new double[4]
            {
                rectangleShape.UpperLeftPoint.X, rectangleShape.UpperRightPoint.X, rectangleShape.LowerLeftPoint.X,
                rectangleShape.LowerRightPoint.X
            };
            var yCoordinates = new double[4]
            {
                rectangleShape.UpperLeftPoint.Y, rectangleShape.UpperRightPoint.Y, rectangleShape.LowerLeftPoint.Y,
                rectangleShape.LowerRightPoint.Y
            };

            var resultVertex = ConvertToExternalProjectionCore(xCoordinates, yCoordinates);

            var multipointShape = new MultipointShape();
            multipointShape.Points.Add(new PointShape(resultVertex[0]));
            multipointShape.Points.Add(new PointShape(resultVertex[1]));
            multipointShape.Points.Add(new PointShape(resultVertex[2]));
            multipointShape.Points.Add(new PointShape(resultVertex[3]));

            var returnRectangle = multipointShape.GetBoundingBox();
            returnRectangle.Id = rectangleShape.Id;

            return returnRectangle;
        }

        /// <summary>Returns projected vertices based on the coordinates passed in.</summary>
        /// <returns>Projected vertices based on the coordinates passed in.</returns>
        /// <remarks>Returns a projected vertex based on the coordinates passed in. You need to override this method.</remarks>
        /// <param name="x">X values of the points to project.</param>
        /// <param name="y">Y values of the points to project.</param>
        protected abstract Vertex[] ConvertToExternalProjectionCore(double[] x, double[] y);

        /// <summary>Returns a reprojected vertex based on the coordinates passed in.</summary>
        /// <returns>Reprojected vertex based on the coordinates passed in. </returns>
        /// <remarks>Returns a reprojected vertex according to the internal projection.<br/><br/>
        /// As being a concrete public method that wraps a Core method, Mapgenix reserves the right
        /// to add events or other logic returned by the Core version of the method. That way, our framework stays open on our end, 
        /// but also allow developers to extend our logic. If you have questions about this,
        /// please contact our support team to help on extending the framework.
        /// </remarks>
        /// <overloads>Reprojects a set of coordinates passed in as X and Y.</overloads>
        /// <param name="x">X value of the point to be reprojected.</param>
        /// <param name="y">Y value of the point to be reprojected.</param>
        public Vertex ConvertToInternalProjection(double x, double y)
        {
            Validators.CheckProjectionIsOpen(_isOpen);

            return ConvertToInternalProjectionCore(new double[1] {x}, new double[1] {y})[0];
        }

        /// <summary>Returns a reprojected BaseShape based on the BaseShape passed in.</summary>
        /// <returns>Reprojected BaseShape based on the BaseShape passed in.</returns>
        /// <remarks>Returns a reprojected BaseShape according to the internal projection.<br/><br/>
        /// As being a concrete public method that wraps a Core method, Mapgenix reserves the right
        /// to add events or other logic returned by the Core version of the method. That way, our framework stays open on our end, 
        /// but also allow developers to extend our logic. If you have questions about this,
        /// please contact our support team to help on extending the framework.
        /// </remarks>
        /// <param name="baseShape">BaseShape to be reprojected.</param>
        public BaseShape ConvertToInternalProjection(BaseShape baseShape)
        {
            Validators.CheckProjectionIsOpen(_isOpen);
            Validators.CheckParameterIsNotNull(baseShape, "baseShape");
            Validators.CheckShapeIsValidForOperation(baseShape);

            var projectedFeature = new Feature(baseShape);
            UpdateToInternalProjection(projectedFeature);
            return projectedFeature.GetShape();
        }

        /// <summary>Returns a reprojected Feature based on the Feature passed in.</summary>
        /// <returns>Reprojected Feature based on the Feature passed in.</returns>
        /// <remarks>Returns a reprojected Feature according to the internal projection.<br/><br/>
        /// Actually, the BaseShape of the Feature is reprojected.
        /// As being a concrete public method that wraps a Core method, Mapgenix reserves the right
        /// to add events or other logic returned by the Core version of the method. That way, our framework stays open on our end, 
        /// but also allow developers to extend our logic. If you have questions about this,
        /// please contact our support team to help on extending the framework.
        /// </remarks>
        /// <param name="feature">Feature to be reprojected.</param>
        public Feature ConvertToInternalProjection(Feature feature)
        {
            if (CanReproject)
            {
                Validators.CheckProjectionIsOpen(_isOpen);
                Validators.CheckFeatureIsValid(feature);

                var wellKnownBinary = feature.GetWellKnownBinary();

                var projectedWellKnownBinary = new byte[wellKnownBinary.Length];
                Buffer.BlockCopy(wellKnownBinary, 0, projectedWellKnownBinary, 0, wellKnownBinary.Length);

                projectedWellKnownBinary = ConvertToInternalProjection(projectedWellKnownBinary);

                var projectedFeature = new Feature(projectedWellKnownBinary, feature.Id, feature.ColumnValues);
                projectedFeature.Tag = feature.Tag;

                return projectedFeature;
            }
            return feature;
        }

        public void UpdateToInternalProjection(Feature feature)
        {
            if (CanReproject)
            {
                Validators.CheckProjectionIsOpen(_isOpen);
                Validators.CheckFeatureIsValid(feature);

                var wellKnownBinary = feature.GetWellKnownBinary();
                ConvertToInternalProjection(wellKnownBinary);
            }
        }

        /// <overloads>Takes in a rectangle to reproject.</overloads>
        /// <summary>Returns a reprojected rectangle based on the rectangle passed in.</summary>
        /// <returns>Reprojected rectangle based on the rectangle passed in.</returns>
        /// <remarks>Reprojects to the internal projection.</remarks>
        /// <param name="rectangleShape">Rectangle to reproject.</param>
        public RectangleShape ConvertToInternalProjection(RectangleShape rectangleShape)
        {
            Validators.CheckProjectionIsOpen(_isOpen);
            Validators.CheckParameterIsNotNull(rectangleShape, "rectangleShape");
            Validators.CheckShapeIsValidForOperation(rectangleShape);

            var xCoordinates = new double[4]
            {
                rectangleShape.UpperLeftPoint.X, rectangleShape.UpperRightPoint.X, rectangleShape.LowerLeftPoint.X,
                rectangleShape.LowerRightPoint.X
            };
            var yCoordinates = new double[4]
            {
                rectangleShape.UpperLeftPoint.Y, rectangleShape.UpperRightPoint.Y, rectangleShape.LowerLeftPoint.Y,
                rectangleShape.LowerRightPoint.Y
            };

            var resultVertex = ConvertToInternalProjectionCore(xCoordinates, yCoordinates);

            var multipointShape = new MultipointShape();
            multipointShape.Points.Add(new PointShape(resultVertex[0]));
            multipointShape.Points.Add(new PointShape(resultVertex[1]));
            multipointShape.Points.Add(new PointShape(resultVertex[2]));
            multipointShape.Points.Add(new PointShape(resultVertex[3]));

            var returnRectangle = multipointShape.GetBoundingBox();
            returnRectangle.Id = rectangleShape.Id;

            return returnRectangle;
        }

        /// <summary>Returns reprojected vertices based on the coordinates passed in.</summary>
        /// <returns>Reprojected vertices based on the coordinates passed in.</returns>
        /// <remarks>Returns a projected vertex to the internal projection based on the coordinates passed in. You need to override this method.</remarks>
        /// <param name="x">X values of the points to reproject.</param>
        /// <param name="y">Y values of the points to reproject.</param>
        protected abstract Vertex[] ConvertToInternalProjectionCore(double[] x, double[] y);

        private byte[] ConvertToExternalProjection(byte[] wellKnownBinary)
        {
            var wellKnownType = GetWellKnownType(wellKnownBinary);
            byte[] projectedWkb;
            switch (wellKnownType)
            {
                case WellKnownType.Point:
                    projectedWkb = ConvertToExternalProjectionPoint(wellKnownBinary);
                    break;
                case WellKnownType.Multipoint:
                    projectedWkb = ConvertToExternalProjectionMultipoint(wellKnownBinary);
                    break;
                case WellKnownType.Line:
                    projectedWkb = ConvertToExternalProjectionLineshape(wellKnownBinary);
                    break;
                case WellKnownType.Multiline:
                    projectedWkb = ConvertToExternalProjectionMultilineShape(wellKnownBinary);
                    break;
                case WellKnownType.Polygon:
                    projectedWkb = ConvertToExternalProjectionPolygonShape(wellKnownBinary);
                    break;
                case WellKnownType.Multipolygon:
                    projectedWkb = ConvertToExternalProjectionMultipolygonShape(wellKnownBinary);
                    break;
                case WellKnownType.GeometryCollection:
                    var shape = new GeometryCollectionShape(wellKnownBinary);
                    var projectedShape = new GeometryCollectionShape();
                    foreach (var item in shape.Shapes)
                    {
                        var baseShape =
                            BaseShape.CreateShapeFromWellKnownData(ConvertToExternalProjection(item.GetWellKnownBinary()));
                        projectedShape.Shapes.Add(baseShape);
                    }
                    projectedWkb = projectedShape.GetWellKnownBinary();
                    Buffer.BlockCopy(projectedWkb, 0, wellKnownBinary, 0, projectedWkb.Length);
                    break;
                default:
                    throw new NotSupportedException(wellKnownType + ExceptionDescription.IsNotSupportedNowForProjection);
            }
            return projectedWkb;
        }

        private byte[] ConvertToInternalProjection(byte[] wellKnownBinary)
        {
            var wellKnownType = GetWellKnownType(wellKnownBinary);
            byte[] projectedWkb;
            switch (wellKnownType)
            {
                case WellKnownType.Point:
                    projectedWkb = ConvertToInternalProjectionPoint(wellKnownBinary);
                    break;
                case WellKnownType.Multipoint:
                    projectedWkb = ConvertToInternalProjectionMultipoint(wellKnownBinary);
                    break;
                case WellKnownType.Line:
                    projectedWkb = ConvertToInternalProjectionLineshape(wellKnownBinary);
                    break;
                case WellKnownType.Multiline:
                    projectedWkb = ConvertToInternalProjectionMultilineShape(wellKnownBinary);
                    break;
                case WellKnownType.Polygon:
                    projectedWkb = ConvertToInternalProjectionPolygonShape(wellKnownBinary);
                    break;
                case WellKnownType.Multipolygon:
                    projectedWkb = ConvertToInternalProjectionMultipolygonShape(wellKnownBinary);
                    break;
                case WellKnownType.GeometryCollection:
                    var shape = new GeometryCollectionShape(wellKnownBinary);
                    var projectedShape = new GeometryCollectionShape();
                    foreach (var item in shape.Shapes)
                    {
                        var baseShape =
                            BaseShape.CreateShapeFromWellKnownData(ConvertToInternalProjection(item.GetWellKnownBinary()));
                        projectedShape.Shapes.Add(baseShape);
                    }
                    projectedWkb = projectedShape.GetWellKnownBinary();
                    break;
                default:
                    throw new NotSupportedException(wellKnownType + ExceptionDescription.IsNotSupportedNowForProjection);
            }
            return projectedWkb;
        }

        private static WellKnownType GetWellKnownType(byte[] wkb)
        {
            var shapeType = ArrayHelper.GetShapeTypeFromByteArray(wkb, 1);
            var wellKnownType = WellKnownType.Invalid;
            switch (shapeType)
            {
                case 1:
                    wellKnownType = WellKnownType.Point;
                    break;
                case 2:
                    wellKnownType = WellKnownType.Line;
                    break;
                case 3:
                    wellKnownType = WellKnownType.Polygon;
                    break;
                case 4:
                    wellKnownType = WellKnownType.Multipoint;
                    break;
                case 5:
                    wellKnownType = WellKnownType.Multiline;
                    break;
                case 6:
                    wellKnownType = WellKnownType.Multipolygon;
                    break;
                case 7:
                    wellKnownType = WellKnownType.GeometryCollection;
                    break;
                default:
                    break;
            }
            return wellKnownType;
        }

        private byte[] ConvertToExternalProjectionPoint(byte[] wellKnownBinary)
        {
            var byteOrder = wellKnownBinary[PositionByteOrder];
            var x = ArrayHelper.GetDoubleFromByteArray(wellKnownBinary, 5, byteOrder);
            var y = ArrayHelper.GetDoubleFromByteArray(wellKnownBinary, 13, byteOrder);

            var projectdVertex = ConvertToExternalProjectionCore(new double[1] {x}, new double[1] {y})[0];

   
            var bytesX = ArrayHelper.GetByteArrayFromDouble(projectdVertex.X, byteOrder);
            Buffer.BlockCopy(bytesX, 0, wellKnownBinary, 5, 8);
            var bytesY = ArrayHelper.GetByteArrayFromDouble(projectdVertex.Y, byteOrder);
            Buffer.BlockCopy(bytesY, 0, wellKnownBinary, 13, 8);
            return wellKnownBinary;
        }

        private byte[] ConvertToExternalProjectionMultipoint(byte[] wellKnownBinary)
        {
            var byteOrder = wellKnownBinary[PositionByteOrder];
            var numberOfPoints = ArrayHelper.GetIntFromByteArray(wellKnownBinary, 5, byteOrder);


            var position = 9;

            var xCoordinates = new double[numberOfPoints];
            var yCoordinates = new double[numberOfPoints];

            for (var k = 0; k < numberOfPoints; k++)
            {
                position = position + 5;

                var x = ArrayHelper.GetDoubleFromByteArray(wellKnownBinary, position, byteOrder);
                var y = ArrayHelper.GetDoubleFromByteArray(wellKnownBinary, position + 8, byteOrder);

                xCoordinates[k] = x;
                yCoordinates[k] = y;

                position = position + 16;
            }

            var projectedVertices = ConvertToExternalProjectionCore(xCoordinates, yCoordinates);

            position = 9;

            for (var k = 0; k < numberOfPoints; k++)
            {

                position = position + 5;

                var bytesX = ArrayHelper.GetByteArrayFromDouble(projectedVertices[k].X, byteOrder);
                Buffer.BlockCopy(bytesX, 0, wellKnownBinary, position, 8);

                var bytesY = ArrayHelper.GetByteArrayFromDouble(projectedVertices[k].Y, byteOrder);
                Buffer.BlockCopy(bytesY, 0, wellKnownBinary, position + 8, 8);

                position = position + 16;
            }

            return wellKnownBinary;
        }

        private byte[] ConvertToExternalProjectionLineshape(byte[] wellKnownBinary)
        {
            var byteOrder = wellKnownBinary[PositionByteOrder];
            var numberOfPoints = ArrayHelper.GetIntFromByteArray(wellKnownBinary, 5, byteOrder);

            var position = 9;

            var xCoordinates = new double[numberOfPoints];
            var yCoordinates = new double[numberOfPoints];

            for (var k = 0; k < numberOfPoints; k++)
            {
                var x = ArrayHelper.GetDoubleFromByteArray(wellKnownBinary, position, byteOrder);
                var y = ArrayHelper.GetDoubleFromByteArray(wellKnownBinary, position + 8, byteOrder);

                xCoordinates[k] = x;
                yCoordinates[k] = y;

                position = position + 16;
            }

            var projectedVertices = ConvertToExternalProjectionCore(xCoordinates, yCoordinates);

            position = 9;

            for (var k = 0; k < numberOfPoints; k++)
            {
                var bytesX = ArrayHelper.GetByteArrayFromDouble(projectedVertices[k].X, byteOrder);
                Buffer.BlockCopy(bytesX, 0, wellKnownBinary, position, 8);

                var bytesY = ArrayHelper.GetByteArrayFromDouble(projectedVertices[k].Y, byteOrder);
                Buffer.BlockCopy(bytesY, 0, wellKnownBinary, position + 8, 8);

                position = position + 16;
            }

            return wellKnownBinary;
        }

        private byte[] ConvertToExternalProjectionMultilineShape(byte[] wellKnownBinary)
        {
            var byteOrder = wellKnownBinary[PositionByteOrder];
            var numberOfLines = ArrayHelper.GetIntFromByteArray(wellKnownBinary, 5, byteOrder);


            var position = 9;

            var xCoordinates = new List<double>();
            var yCoordinates = new List<double>();

            for (var k = 0; k < numberOfLines; k++)
            {
                var byteOrderOfEachLine = wellKnownBinary[position];
                var numberOfPoints = ArrayHelper.GetIntFromByteArray(wellKnownBinary, position + 5, byteOrderOfEachLine);

                position = position + 9;

                for (var i = 0; i < numberOfPoints; i++)
                {
                    var x = ArrayHelper.GetDoubleFromByteArray(wellKnownBinary, position, byteOrderOfEachLine);
                    var y = ArrayHelper.GetDoubleFromByteArray(wellKnownBinary, position + 8, byteOrderOfEachLine);

                    xCoordinates.Add(x);
                    yCoordinates.Add(y);

                    position = position + 16;
                }
            }

            var projectedVertices = ConvertToExternalProjectionCore(xCoordinates.ToArray(), yCoordinates.ToArray());

            position = 9;

            var vertexCount = 0;
            for (var k = 0; k < numberOfLines; k++)
            {
                var byteOrderOfEachLine = wellKnownBinary[position];

                var numberOfPoints = ArrayHelper.GetIntFromByteArray(wellKnownBinary, position + 5, byteOrderOfEachLine);


                position = position + 9;

                for (var i = 0; i < numberOfPoints; i++)
                {
                    var bytesX = ArrayHelper.GetByteArrayFromDouble(projectedVertices[vertexCount].X, byteOrderOfEachLine);
                    Buffer.BlockCopy(bytesX, 0, wellKnownBinary, position, 8);

                    var bytesY = ArrayHelper.GetByteArrayFromDouble(projectedVertices[vertexCount].Y, byteOrderOfEachLine);
                    Buffer.BlockCopy(bytesY, 0, wellKnownBinary, position + 8, 8);
                    position = position + 16;
                    vertexCount++;
                }
            }

            return wellKnownBinary;
        }

        private byte[] ConvertToExternalProjectionPolygonShape(byte[] wellKnownBinary)
        {
            var byteOrder = wellKnownBinary[PositionByteOrder];
            var numberOfRings = ArrayHelper.GetIntFromByteArray(wellKnownBinary, 5, byteOrder);

            var position = 9;
            var outRingPointCount = ArrayHelper.GetIntFromByteArray(wellKnownBinary, position, byteOrder);

            position = position + 4;

            var xCoordinates = new List<double>();
            var yCoordinates = new List<double>();

            for (var i = 0; i < outRingPointCount; i++)
            {
                var x = ArrayHelper.GetDoubleFromByteArray(wellKnownBinary, position, byteOrder);
                var y = ArrayHelper.GetDoubleFromByteArray(wellKnownBinary, position + 8, byteOrder);

                xCoordinates.Add(x);
                yCoordinates.Add(y);

                position = position + 16;
            }

            for (var k = 0; k < numberOfRings - 1; k++)
            {
                var innerRingPointCount = ArrayHelper.GetIntFromByteArray(wellKnownBinary, position, byteOrder);

                position = position + 4;

                for (var i = 0; i < innerRingPointCount; i++)
                {
                    var x = ArrayHelper.GetDoubleFromByteArray(wellKnownBinary, position, byteOrder);
                    var y = ArrayHelper.GetDoubleFromByteArray(wellKnownBinary, position + 8, byteOrder);

                    xCoordinates.Add(x);
                    yCoordinates.Add(y);

                    position = position + 16;
                }
            }

            var projectedVertices = ConvertToExternalProjectionCore(xCoordinates.ToArray(), yCoordinates.ToArray());

            position = 13;

            for (var i = 0; i < outRingPointCount; i++)
            {
                var bytesX = ArrayHelper.GetByteArrayFromDouble(projectedVertices[i].X, byteOrder);
                Buffer.BlockCopy(bytesX, 0, wellKnownBinary, position, 8);

                var bytesY = ArrayHelper.GetByteArrayFromDouble(projectedVertices[i].Y, byteOrder);
                Buffer.BlockCopy(bytesY, 0, wellKnownBinary, position + 8, 8);

                position = position + 16;
            }

            for (var k = 0; k < numberOfRings - 1; k++)
            {
                var innerRingPointCount = ArrayHelper.GetIntFromByteArray(wellKnownBinary, position, byteOrder);
                position = position + 4;

                for (var i = 0; i < innerRingPointCount; i++)
                {
                    var bytesX = ArrayHelper.GetByteArrayFromDouble(projectedVertices[outRingPointCount].X, byteOrder);
                    Buffer.BlockCopy(bytesX, 0, wellKnownBinary, position, 8);

                    var bytesY = ArrayHelper.GetByteArrayFromDouble(projectedVertices[outRingPointCount].Y, byteOrder);
                    Buffer.BlockCopy(bytesY, 0, wellKnownBinary, position + 8, 8);
                    position = position + 16;
                    outRingPointCount++;
                }
            }

            return wellKnownBinary;
        }

        private byte[] ConvertToExternalProjectionMultipolygonShape(byte[] wellKnownBinary)
        {
            var byteOrder = wellKnownBinary[PositionByteOrder];
            var numberOfPolygons = ArrayHelper.GetIntFromByteArray(wellKnownBinary, 5, byteOrder);


            var position = 9;

            var xCoordinates = new List<double>();
            var yCoordinates = new List<double>();

            for (var m = 0; m < numberOfPolygons; m++)
            {
                var byteOrderOfPolgyon = wellKnownBinary[position];

                var ringCountOfPolygon = ArrayHelper.GetIntFromByteArray(wellKnownBinary, position + 5, byteOrder);
                var outringPointCount = ArrayHelper.GetIntFromByteArray(wellKnownBinary, position + 9, byteOrderOfPolgyon);

                position = position + 13;
                for (var i = 0; i < outringPointCount; i++)
                {
                    var x = ArrayHelper.GetDoubleFromByteArray(wellKnownBinary, position, byteOrder);
                    var y = ArrayHelper.GetDoubleFromByteArray(wellKnownBinary, position + 8, byteOrder);

                    xCoordinates.Add(x);
                    yCoordinates.Add(y);

                    position = position + 16;
                }

                for (var k = 0; k < ringCountOfPolygon - 1; k++)
                {
                    var innerRingPointCount = ArrayHelper.GetIntFromByteArray(wellKnownBinary, position, byteOrder);

                    position = position + 4;

                    for (var i = 0; i < innerRingPointCount; i++)
                    {
                        var x = ArrayHelper.GetDoubleFromByteArray(wellKnownBinary, position, byteOrder);
                        var y = ArrayHelper.GetDoubleFromByteArray(wellKnownBinary, position + 8, byteOrder);

                        xCoordinates.Add(x);
                        yCoordinates.Add(y);

                        position = position + 16;
                    }
                }
            }

            var projectedVertices = ConvertToExternalProjectionCore(xCoordinates.ToArray(), yCoordinates.ToArray());

            position = 9;

            var vertexCount = 0;

            for (var m = 0; m < numberOfPolygons; m++)
            {
                var byteOrderOfPolgyon = wellKnownBinary[position];

                var ringCountOfPolygon = ArrayHelper.GetIntFromByteArray(wellKnownBinary, position + 5, byteOrder);
                var outringPointCount = ArrayHelper.GetIntFromByteArray(wellKnownBinary, position + 9, byteOrderOfPolgyon);
                position = position + 13;
                for (var i = 0; i < outringPointCount; i++)
                {
                    var bytesX = ArrayHelper.GetByteArrayFromDouble(projectedVertices[vertexCount].X, byteOrder);
                    Buffer.BlockCopy(bytesX, 0, wellKnownBinary, position, 8);

                    var bytesY = ArrayHelper.GetByteArrayFromDouble(projectedVertices[vertexCount].Y, byteOrder);
                    Buffer.BlockCopy(bytesY, 0, wellKnownBinary, position + 8, 8);

                    position = position + 16;
                    vertexCount++;
                }

                for (var k = 0; k < ringCountOfPolygon - 1; k++)
                {
                    var innerRingPointCount = ArrayHelper.GetIntFromByteArray(wellKnownBinary, position, byteOrder);
                    position = position + 4;

                    for (var i = 0; i < innerRingPointCount; i++)
                    {
                        var bytesX = ArrayHelper.GetByteArrayFromDouble(projectedVertices[vertexCount].X, byteOrder);
                        Buffer.BlockCopy(bytesX, 0, wellKnownBinary, position, 8);

                        var bytesY = ArrayHelper.GetByteArrayFromDouble(projectedVertices[vertexCount].Y, byteOrder);
                        Buffer.BlockCopy(bytesY, 0, wellKnownBinary, position + 8, 8);

                        position = position + 16;
                        vertexCount++;
                    }
                }
            }

            return wellKnownBinary;
        }

        private byte[] ConvertToInternalProjectionPoint(byte[] wellKnownBinary)
        {
            var byteOrder = wellKnownBinary[PositionByteOrder];
            var x = ArrayHelper.GetDoubleFromByteArray(wellKnownBinary, 5, byteOrder);
            var y = ArrayHelper.GetDoubleFromByteArray(wellKnownBinary, 13, byteOrder);

            var projectdVertex = ConvertToInternalProjectionCore(new double[1] {x}, new double[1] {y})[0];


            var bytesX = ArrayHelper.GetByteArrayFromDouble(projectdVertex.X, byteOrder);
            Buffer.BlockCopy(bytesX, 0, wellKnownBinary, 5, 8);

            var bytesY = ArrayHelper.GetByteArrayFromDouble(projectdVertex.Y, byteOrder);
            Buffer.BlockCopy(bytesY, 0, wellKnownBinary, 13, 8);
            return wellKnownBinary;
        }

        private byte[] ConvertToInternalProjectionMultipoint(byte[] wellKnownBinary)
        {
            var byteOrder = wellKnownBinary[PositionByteOrder];
            var numberOfPoints = ArrayHelper.GetIntFromByteArray(wellKnownBinary, 5, byteOrder);

            var position = 9;

            var xCoordinates = new double[numberOfPoints];
            var yCoordinates = new double[numberOfPoints];

            for (var k = 0; k < numberOfPoints; k++)
            {
                position = position + 5;

                var x = ArrayHelper.GetDoubleFromByteArray(wellKnownBinary, position, byteOrder);
                var y = ArrayHelper.GetDoubleFromByteArray(wellKnownBinary, position + 8, byteOrder);

                xCoordinates[k] = x;
                yCoordinates[k] = y;

                position = position + 16;
            }

            var projectedVertices = ConvertToInternalProjectionCore(xCoordinates, yCoordinates);

            position = 9;

            for (var k = 0; k < numberOfPoints; k++)
            {

                position = position + 5;

                var bytesX = ArrayHelper.GetByteArrayFromDouble(projectedVertices[k].X, byteOrder);
                Buffer.BlockCopy(bytesX, 0, wellKnownBinary, position, 8);

                var bytesY = ArrayHelper.GetByteArrayFromDouble(projectedVertices[k].Y, byteOrder);
                Buffer.BlockCopy(bytesY, 0, wellKnownBinary, position + 8, 8);

                position = position + 16;
            }
            return wellKnownBinary;
        }

        private byte[] ConvertToInternalProjectionLineshape(byte[] wellKnownBinary)
        {
            var byteOrder = wellKnownBinary[PositionByteOrder];
            var numberOfPoints = ArrayHelper.GetIntFromByteArray(wellKnownBinary, 5, byteOrder);

            var position = 9;

            var xCoordinates = new double[numberOfPoints];
            var yCoordinates = new double[numberOfPoints];

            for (var k = 0; k < numberOfPoints; k++)
            {
                var x = ArrayHelper.GetDoubleFromByteArray(wellKnownBinary, position, byteOrder);
                var y = ArrayHelper.GetDoubleFromByteArray(wellKnownBinary, position + 8, byteOrder);

                xCoordinates[k] = x;
                yCoordinates[k] = y;

                position = position + 16;
            }

            var projectedVertices = ConvertToInternalProjectionCore(xCoordinates, yCoordinates);

            position = 9;

            for (var k = 0; k < numberOfPoints; k++)
            {
                var bytesX = ArrayHelper.GetByteArrayFromDouble(projectedVertices[k].X, byteOrder);
                Buffer.BlockCopy(bytesX, 0, wellKnownBinary, position, 8);

                var bytesY = ArrayHelper.GetByteArrayFromDouble(projectedVertices[k].Y, byteOrder);
                Buffer.BlockCopy(bytesY, 0, wellKnownBinary, position + 8, 8);

                position = position + 16;
            }

            return wellKnownBinary;
        }

        private byte[] ConvertToInternalProjectionMultilineShape(byte[] wellKnownBinary)
        {
            var byteOrder = wellKnownBinary[PositionByteOrder];
            var numberOfLines = ArrayHelper.GetIntFromByteArray(wellKnownBinary, 5, byteOrder);


            var position = 9;

            var xCoordinates = new List<double>();
            var yCoordinates = new List<double>();

            for (var k = 0; k < numberOfLines; k++)
            {
                var byteOrderOfEachLine = wellKnownBinary[position];
                var numberOfPoints = ArrayHelper.GetIntFromByteArray(wellKnownBinary, position + 5, byteOrderOfEachLine);

                position = position + 9;

                for (var i = 0; i < numberOfPoints; i++)
                {
                    var x = ArrayHelper.GetDoubleFromByteArray(wellKnownBinary, position, byteOrderOfEachLine);
                    var y = ArrayHelper.GetDoubleFromByteArray(wellKnownBinary, position + 8, byteOrderOfEachLine);

                    xCoordinates.Add(x);
                    yCoordinates.Add(y);

                    position = position + 16;
                }
            }

            var projectedVertices = ConvertToInternalProjectionCore(xCoordinates.ToArray(), yCoordinates.ToArray());

            position = 9;

            var vertexCount = 0;
            for (var k = 0; k < numberOfLines; k++)
            {
                var byteOrderOfEachLine = wellKnownBinary[position];

                var numberOfPoints = ArrayHelper.GetIntFromByteArray(wellKnownBinary, position + 5, byteOrderOfEachLine);

                position = position + 9;

                for (var i = 0; i < numberOfPoints; i++)
                {
                    var bytesX = ArrayHelper.GetByteArrayFromDouble(projectedVertices[vertexCount].X, byteOrderOfEachLine);
                    Buffer.BlockCopy(bytesX, 0, wellKnownBinary, position, 8);

                    var bytesY = ArrayHelper.GetByteArrayFromDouble(projectedVertices[vertexCount].Y, byteOrderOfEachLine);
                    Buffer.BlockCopy(bytesY, 0, wellKnownBinary, position + 8, 8);
                    position = position + 16;
                    vertexCount++;
                }
            }
            return wellKnownBinary;
        }

        private byte[] ConvertToInternalProjectionPolygonShape(byte[] wellKnownBinary)
        {
            var byteOrder = wellKnownBinary[PositionByteOrder];
            var numberOfRings = ArrayHelper.GetIntFromByteArray(wellKnownBinary, 5, byteOrder);

    
            var position = 9;
            var outRingPointCount = ArrayHelper.GetIntFromByteArray(wellKnownBinary, position, byteOrder);
    
            position = position + 4;

            var xCoordinates = new List<double>();
            var yCoordinates = new List<double>();

            for (var i = 0; i < outRingPointCount; i++)
            {
                var x = ArrayHelper.GetDoubleFromByteArray(wellKnownBinary, position, byteOrder);
                var y = ArrayHelper.GetDoubleFromByteArray(wellKnownBinary, position + 8, byteOrder);

                xCoordinates.Add(x);
                yCoordinates.Add(y);

                position = position + 16;
            }

            for (var k = 0; k < numberOfRings - 1; k++)
            {
                var innerRingPointCount = ArrayHelper.GetIntFromByteArray(wellKnownBinary, position, byteOrder);

                position = position + 4;

                for (var i = 0; i < innerRingPointCount; i++)
                {
                    var x = ArrayHelper.GetDoubleFromByteArray(wellKnownBinary, position, byteOrder);
                    var y = ArrayHelper.GetDoubleFromByteArray(wellKnownBinary, position + 8, byteOrder);

                    xCoordinates.Add(x);
                    yCoordinates.Add(y);

                    position = position + 16;
                }
            }

            var projectedVertices = ConvertToInternalProjectionCore(xCoordinates.ToArray(), yCoordinates.ToArray());

            position = 13;

            for (var i = 0; i < outRingPointCount; i++)
            {
                var bytesX = ArrayHelper.GetByteArrayFromDouble(projectedVertices[i].X, byteOrder);
                Buffer.BlockCopy(bytesX, 0, wellKnownBinary, position, 8);
    
                var bytesY = ArrayHelper.GetByteArrayFromDouble(projectedVertices[i].Y, byteOrder);
                Buffer.BlockCopy(bytesY, 0, wellKnownBinary, position + 8, 8);
    
                position = position + 16;
            }

            for (var k = 0; k < numberOfRings - 1; k++)
            {
                var innerRingPointCount = ArrayHelper.GetIntFromByteArray(wellKnownBinary, position, byteOrder);
                position = position + 4;

                for (var i = 0; i < innerRingPointCount; i++)
                {
                    var bytesX = ArrayHelper.GetByteArrayFromDouble(projectedVertices[outRingPointCount].X, byteOrder);
                    Buffer.BlockCopy(bytesX, 0, wellKnownBinary, position, 8);
    
                    var bytesY = ArrayHelper.GetByteArrayFromDouble(projectedVertices[outRingPointCount].Y, byteOrder);
                    Buffer.BlockCopy(bytesY, 0, wellKnownBinary, position + 8, 8);
                    position = position + 16;
                    outRingPointCount++;
                }
            }

            return wellKnownBinary;
        }

        private byte[] ConvertToInternalProjectionMultipolygonShape(byte[] wellKnownBinary)
        {
            var byteOrder = wellKnownBinary[PositionByteOrder];
            var numberOfPolygons = ArrayHelper.GetIntFromByteArray(wellKnownBinary, 5, byteOrder);

    
            var position = 9;

            var xCoordinates = new List<double>();
            var yCoordinates = new List<double>();

            for (var m = 0; m < numberOfPolygons; m++)
            {
                var byteOrderOfPolgyon = wellKnownBinary[position];

                var ringCountOfPolygon = ArrayHelper.GetIntFromByteArray(wellKnownBinary, position + 5, byteOrder);
                var outringPointCount = ArrayHelper.GetIntFromByteArray(wellKnownBinary, position + 9, byteOrderOfPolgyon);

                position = position + 13;
                for (var i = 0; i < outringPointCount; i++)
                {
                    var x = ArrayHelper.GetDoubleFromByteArray(wellKnownBinary, position, byteOrder);
                    var y = ArrayHelper.GetDoubleFromByteArray(wellKnownBinary, position + 8, byteOrder);

                    xCoordinates.Add(x);
                    yCoordinates.Add(y);

                    position = position + 16;
                }

                for (var k = 0; k < ringCountOfPolygon - 1; k++)
                {
                    var innerRingPointCount = ArrayHelper.GetIntFromByteArray(wellKnownBinary, position, byteOrder);

                    position = position + 4;

                    for (var i = 0; i < innerRingPointCount; i++)
                    {
                        var x = ArrayHelper.GetDoubleFromByteArray(wellKnownBinary, position, byteOrder);
                        var y = ArrayHelper.GetDoubleFromByteArray(wellKnownBinary, position + 8, byteOrder);

                        xCoordinates.Add(x);
                        yCoordinates.Add(y);

                        position = position + 16;
                    }
                }
            }

            var projectedVertices = ConvertToInternalProjectionCore(xCoordinates.ToArray(), yCoordinates.ToArray());

            position = 9;

            var vertexCount = 0;

            for (var m = 0; m < numberOfPolygons; m++)
            {
                var byteOrderOfPolgyon = wellKnownBinary[position];

                var ringCountOfPolygon = ArrayHelper.GetIntFromByteArray(wellKnownBinary, position + 5, byteOrder);
                var outringPointCount = ArrayHelper.GetIntFromByteArray(wellKnownBinary, position + 9, byteOrderOfPolgyon);
              
            
                position = position + 13;
                for (var i = 0; i < outringPointCount; i++)
                {
                    var bytesX = ArrayHelper.GetByteArrayFromDouble(projectedVertices[vertexCount].X, byteOrder);
                    Buffer.BlockCopy(bytesX, 0, wellKnownBinary, position, 8);
            
                    var bytesY = ArrayHelper.GetByteArrayFromDouble(projectedVertices[vertexCount].Y, byteOrder);
                    Buffer.BlockCopy(bytesY, 0, wellKnownBinary, position + 8, 8);
                    position = position + 16;
                    vertexCount++;
                }

                for (var k = 0; k < ringCountOfPolygon - 1; k++)
                {
                    var innerRingPointCount = ArrayHelper.GetIntFromByteArray(wellKnownBinary, position, byteOrder);
                    position = position + 4;

                    for (var i = 0; i < innerRingPointCount; i++)
                    {
                        var bytesX = ArrayHelper.GetByteArrayFromDouble(projectedVertices[vertexCount].X, byteOrder);
                        Buffer.BlockCopy(bytesX, 0, wellKnownBinary, position, 8);
                        var bytesY = ArrayHelper.GetByteArrayFromDouble(projectedVertices[vertexCount].Y, byteOrder);
                        Buffer.BlockCopy(bytesY, 0, wellKnownBinary, position + 8, 8);
                        position = position + 16;
                        vertexCount++;
                    }
                }
            }

            return wellKnownBinary;
        }


        /// <summary>Returns the geographic unit of the internal projection.</summary>
        /// <returns>geographic unit of the internal projection.</returns>
        public GeographyUnit GetInternalGeographyUnit()
        {
            var geopraphyUnit = GetInternalGeographyUnitCore();
            return geopraphyUnit;
        }

        /// <summary>Returns the geographic unit of the internal projection.</summary>
        /// <returns>geographic unit of the internal projection.</returns>
        protected virtual GeographyUnit GetInternalGeographyUnitCore()
        {
            return GeographyUnit.Unknown;
        }

        /// <summary>Returns the geographic unit of the external projection.</summary>
        /// <returns>geographic unit of the external projection.</returns>
        public GeographyUnit GetExternalGeographyUnit()
        {
            var geopraphyUnit = GetExternalGeographyUnitCore();
            return geopraphyUnit;
        }

        /// <summary>Returns the geographic unit of the external projection.</summary>
        /// <returns>geographic unit of the external projection.</returns>
        protected virtual GeographyUnit GetExternalGeographyUnitCore()
        {
            return GeographyUnit.Unknown;
        }
    }
}
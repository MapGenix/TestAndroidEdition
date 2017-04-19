using System;
using System.IO;
using Mapgenix.Canvas;
using Mapgenix.Layers.Properties;
using Mapgenix.Shapes;
using Mapgenix.Utils;
using Mapgenix.FeatureSource;

namespace Mapgenix.Layers
{
    internal static class Validators
    {
        internal static void CheckGoogleMapUnit(GeographyUnit fromUnit, string parameterName)
        {
            if (fromUnit != GeographyUnit.Meter)
            {
                throw new ArgumentException(ExceptionDescription.GeographicUnitNotValidWithGoogle, parameterName);
            }
        }

        internal static void CheckOpenStreetMapUnit(GeographyUnit fromUnit, string parameterName)
        {
            if (fromUnit != GeographyUnit.Meter)
            {
                throw new ArgumentException(ExceptionDescription.GeographicUnitNotValidWithGoogle, parameterName);
            }
        }

        internal static void CheckParameterIsNotNull(Object objectToTest, string parameterName)
        {
           
            if (objectToTest == null)
            {
                string exceptionDescription = ExceptionDescription.ParameterIsNull;
                throw new ArgumentNullException(parameterName, exceptionDescription);
            }
        }


        internal static void CheckGeoCanvasIsInDrawing(bool isDrawing)
        {
            if (!isDrawing)
            {
                throw new InvalidOperationException(ExceptionDescription.GeocanvasIsNotInDrawing);
            }
        }

        internal static void CheckRebuildRecordIdModeIsValid(BuildIndexMode rebuildRecordIdMode, string parameterName)
        {
            switch (rebuildRecordIdMode)
            {
                case BuildIndexMode.Rebuild: break;
                case BuildIndexMode.DoNotRebuild: break;
                default:
                    string errorDescription = ExceptionDescription.EnumerationOutOfRange;
                    throw new ArgumentOutOfRangeException(parameterName, errorDescription);
            }
        }

        
        internal static void CheckShapeIsValidForOperation(BaseShape basheShapeToTest)
        {
            ShapeValidationResult validationResult = basheShapeToTest.Validate(ShapeValidationMode.Simple);
            if (!validationResult.IsValid)
            {
                string exceptionDescription = ExceptionDescription.ShapeIsInvalidForOperation;
                throw new InvalidOperationException(exceptionDescription + validationResult.ValidationErrors);
            }
        }

        internal static void CheckIfInputValueIsInRange(double inputValue, string parameterName, double minValue, RangeCheckingInclusion includeMinValue, double maxValue, RangeCheckingInclusion includeMaxValue)
        {
            bool bResult = false;
            if ((inputValue > minValue)
                || (inputValue == minValue && includeMinValue == RangeCheckingInclusion.IncludeValue))
            {
                if ((inputValue < maxValue)
                    || (inputValue == maxValue && includeMaxValue == RangeCheckingInclusion.IncludeValue))
                {
                    bResult = true;
                }
            }

            if (!bResult)
            {
                string errorDescription = ExceptionDescription.DoubleOutOfRange;
                throw new ArgumentOutOfRangeException(parameterName, errorDescription);
            }
        }


        internal static void CheckIfInputValueIsBiggerThan(double inputValue, string parameterName, double minValue, RangeCheckingInclusion includeMinValue)
        {
            CheckIfInputValueIsInRange(inputValue, parameterName, minValue, includeMinValue, double.MaxValue, RangeCheckingInclusion.IncludeValue);
        }


        internal static void CheckFeatureSourceIsOpen(bool isOpen)
        {
            if (!isOpen)
            {
                throw new InvalidOperationException(ExceptionDescription.FeatureSourceIsNotOpen);
            }
        }

        internal static void CheckRasterSourceIsOpen(bool isOpen)
        {
            if (!isOpen)
            {
                throw new InvalidOperationException(ExceptionDescription.ImageSourceIsNotOpen);
            }
        }

        internal static void CheckParameterIsNotNullOrEmpty(string value, string parameterName)
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName, ExceptionDescription.ParameterIsNull);
            }
            if (value.Trim() == string.Empty)
            {
                throw new ArgumentException(ExceptionDescription.ParameterIsEmpty, parameterName);
            }
        }



        internal static void CheckMapEngineExtentIsValid(RectangleShape worldExtent, string parameterName)
        {
            if (worldExtent == null)
            {
                throw new ArgumentNullException(ExceptionDescription.WorldExtentIsNotValid, parameterName);
            }
            ShapeValidationResult result = worldExtent.Validate(ShapeValidationMode.Simple);
            if (!result.IsValid)
            {
                throw new InvalidOperationException(ExceptionDescription.WorldExtentIsNotValid);
            }

            if (worldExtent.Width == 0 || worldExtent.Height == 0)
            {
                throw new InvalidOperationException(ExceptionDescription.CurrentExtentNotAssigned);
            }
        }

        internal static void CheckDistanceUnitIsValid(DistanceUnit distanceUnit, string parameterName)
        {
            switch (distanceUnit)
            {
                case DistanceUnit.Feet: break;
                case DistanceUnit.Kilometer: break;
                case DistanceUnit.Meter: break;
                case DistanceUnit.Mile: break;
                case DistanceUnit.UsSurveyFeet: break;
                case DistanceUnit.Yard: break;
                default:
                    string errorDescription = ExceptionDescription.EnumerationOutOfRange;
                    throw new ArgumentOutOfRangeException(parameterName, errorDescription);
            }
        }



        internal static void CheckFeatureIsValid(Feature feature)
        {
            if (!feature.IsValid())
            {
                throw new ArgumentException(ExceptionDescription.FeatureIsNotValid);
            }
        }


       
        internal static void CheckFileIsExist(string pathFileName)
        {
            if (!File.Exists(pathFileName))
            {
                throw new FileNotFoundException(ExceptionDescription.FileIsNotExist, pathFileName);
            }
        }

        internal static void CheckBuildIndexModeIsValid(BuildIndexMode buildIndexMode, string parameterName)
        {
            switch (buildIndexMode)
            {
                case BuildIndexMode.DoNotRebuild:
                    break;
                case BuildIndexMode.Rebuild:
                    break;
                default:
                    string errorDescription = ExceptionDescription.EnumerationOutOfRange;
                    throw new ArgumentOutOfRangeException(parameterName, errorDescription);
            }
        }

        internal static void CheckShapeFileNameIsValid(string pathFileName, string parameterName)
        {
            if (Path.GetExtension(pathFileName).ToUpperInvariant() != ".SHP")
            {
                throw new ArgumentException(ExceptionDescription.ShapeFileNameIsInvalid, parameterName);
            }
        }

        internal static void CheckShapeFileTypeIsValid(ShapeFileType shapeFileType, string parameterName)
        {
            switch (shapeFileType)
            {
                case ShapeFileType.Point: break;
                case ShapeFileType.Polyline: break;
                case ShapeFileType.Polygon: break;
                case ShapeFileType.Multipoint: break;

                case ShapeFileType.PointZ:
                case ShapeFileType.PolylineZ:
                case ShapeFileType.PolygonZ:
                case ShapeFileType.MultipointZ:
                case ShapeFileType.PointM:
                case ShapeFileType.PolylineM:
                case ShapeFileType.PolygonM:
                case ShapeFileType.MultipointM:
                case ShapeFileType.Multipatch:
                    break;
                
                default:
                    string errorDescription = ExceptionDescription.EnumerationOutOfRange;
                    throw new ArgumentOutOfRangeException(parameterName, errorDescription);
            }
        }

     
        internal static void CheckLayerIsOpened(bool isOpen)
        {
            if (!isOpen)
            {
                throw new InvalidOperationException(ExceptionDescription.CheckLayerIsOpened);
            }
        }

        
        internal static void CheckGeographyUnitIsValid(GeographyUnit geographyUnit, string parameterName)
        {
            switch (geographyUnit)
            {
                case GeographyUnit.DecimalDegree: break;
                case GeographyUnit.Feet: break;
                case GeographyUnit.Meter: break;
                default:
                    string errorDescription = ExceptionDescription.EnumerationOutOfRange;
                    throw new ArgumentOutOfRangeException(parameterName, errorDescription);
            }
        }


        internal static void CheckPanDirectionIsValid(PanDirection panDirection, string parameterName)
        {
            switch (panDirection)
            {
                case PanDirection.Up: break;
                case PanDirection.UpperRight: break;
                case PanDirection.Right: break;
                case PanDirection.LowerRight: break;
                case PanDirection.Down: break;
                case PanDirection.LowerLeft: break;
                case PanDirection.Left: break;
                case PanDirection.UpperLeft: break;
                default:
                    string errorDescription = ExceptionDescription.EnumerationOutOfRange;
                    throw new ArgumentOutOfRangeException(parameterName, errorDescription);
            }
        }

        
        internal static void CheckGroupLayerIsEmpty(SafeCollection<BaseLayer> layers)
        {
            if (layers.Count == 0)
            {
                throw new InvalidOperationException(ExceptionDescription.GroupLayerIsEmpty);
            }
        }

        internal static void DefaultAndCustomStyleDuplicateForRestrictedLayer(RestrictionLayer restrictedLayer)
        {
            if (restrictedLayer.RestrictionStyle != RestrictionStyle.UseCustomStyles && restrictedLayer.CustomStyles.Count > 0)
            {
                throw new InvalidOperationException(ExceptionDescription.DefaultAndCustomStyleDuplicateForRestrictedLayer);
            }
        }

        internal static void CheckLayerHasBoundingBox(bool hasBoundingBox)
        {
            if (!hasBoundingBox)
            {
                throw new NotSupportedException(ExceptionDescription.LayerDoesNotHaveBoundingBox);
            }
        }

        internal static void CheckOverwriteModeIsValid(OverwriteMode overwriteMode, string parameterName)
        {
            switch (overwriteMode)
            {
                case OverwriteMode.Overwrite:
                case OverwriteMode.DoNotOverwrite:
                    break;
                default:
                    string errorDescription = ExceptionDescription.EnumerationOutOfRange;
                    throw new ArgumentOutOfRangeException(parameterName, errorDescription);
            }
        }
      
        
        internal static void CheckUriIsValid(Uri uri)
        {
            if (String.IsNullOrEmpty(uri.AbsolutePath))
            {
                throw new UriFormatException(ExceptionDescription.UriNotValid);
            }
        }
        
        internal static void CheckStreamWritable(Stream outputStream, string parameterName)
        {
            if (!outputStream.CanWrite)
            {
                throw new ArgumentException(ExceptionDescription.StreamIsNotWritable, parameterName);
            }
        }
    }
}

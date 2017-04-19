using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using Mapgenix.FeatureSource.Properties;
using Mapgenix.Shapes;
using Mapgenix.Utils;

namespace Mapgenix.FeatureSource
{
    internal static class Validators
    {

        internal static void CheckColumnTypeAndDecimalLength(DbfColumnType columnType, int decimalLength)
        {

            if (columnType != DbfColumnType.Double && decimalLength != 0)
            {
                throw new ArgumentException(ExceptionDescription.DecimalLengthInValidForStringColumnType, "decimalLength");
            }
        }

        internal static void CheckMapEngineExtentIsValid(RectangleShape worldExtent, string parameterName)
        {

            if (worldExtent == null)
            {
                throw new ArgumentNullException(ExceptionDescription.WorldExtentIsNotValid, parameterName);
            }
            else
            {
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
        }

        internal static void CheckParameterIsNotNull(Object objectToTest, string parameterName)
        {
            

            if (objectToTest == null)
            {
                string exceptionDescription = ExceptionDescription.ParameterIsNull;
                throw new ArgumentNullException(parameterName, exceptionDescription);
            }
        }

        internal static void CheckDatabaseColumnsIsEmpty(IEnumerable<DbfColumn> databaseColumns, string parameterName)
        {
            

            int count = 0;
            foreach (DbfColumn dbfColumn in databaseColumns)
            {
                count++;
            }

            if (count == 0)
            {
                string exceptionDescription = ExceptionDescription.DbfColumnsIsEmpty;
                throw new ArgumentException(parameterName, exceptionDescription);
            }
        }

        internal static void CheckParameterIdsIsIntergers(IEnumerable<string> ids, string parameterName)
        {
           

            int tempId = 0;
            foreach (string id in ids)
            {
                if (!int.TryParse(id, NumberStyles.Any, CultureInfo.InvariantCulture, out tempId))
                {
                    string exceptionDescription = ExceptionDescription.ParameterIdsIsNotIntegers;
                    throw new ArgumentException(parameterName, exceptionDescription);
                }
            }
        }
        


       


        internal static void CheckValueIsBiggerThanZero(double value, string parameterName)
        {
           
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(parameterName, ExceptionDescription.TheValueShouldBeGreaterThanZero);
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

        internal static void CheckParameterIsValid(BaseShape basheShapeToTest, string parameterName)
        {
            

            ShapeValidationResult validationResult = basheShapeToTest.Validate(ShapeValidationMode.Simple);
            if (!validationResult.IsValid)
            {
                string exceptionDescription = ExceptionDescription.ParameterIsInvalid;
                throw new ArgumentException(exceptionDescription + validationResult.ValidationErrors, parameterName);
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

   

      
        internal static void CheckFeatureSourceIsEditable(bool isEditable)
        {
           
            if (!isEditable)
            {
                throw new InvalidOperationException(ExceptionDescription.FeatureSourceIsNotEditable);
            }
        }

       

        internal static void CheckFeatureSourceIsInTransaction(bool isInTransaction)
        {
           
            if (!isInTransaction)
            {
                throw new InvalidOperationException(ExceptionDescription.FeatureSourceIsNotInTransaction);
            }
        }

        internal static void CheckFeatureSourceIsAlreadyInTransaction(bool isInTransaction)
        {
           
            if (isInTransaction)
            {
                throw new InvalidOperationException(ExceptionDescription.FeatureSourceIsAlreadyInTransaction);
            }
        }

        internal static void CheckFeatureIsValid(Feature feature)
        {
           
            if (!feature.IsValid())
            {
                throw new ArgumentException(ExceptionDescription.FeatureIsNotValid);
            }
        }

        internal static void CheckShapeIsAreaBaseShape(BaseShape baseShape)
        {
           
            if (!(baseShape is BaseAreaShape))
            {
                throw new ArgumentException(ExceptionDescription.TargetShapeIsNotValidType);
            }
        }
      
        
        internal static void CheckFileIsSupportedByCommonImageSources(string extension)
        {
        
            extension = extension.ToUpper(CultureInfo.InvariantCulture);
            switch (extension)
            {
                case ".BMP":
                case ".GIF":
                case ".JPG":
                case ".PNG":
                case ".TIF":
                    break;

                case ".TIFF":
                    throw new NotSupportedException(ExceptionDescription.UseGeoTiffRasterInstead);
                case ".EXIG":
                default:
                    throw new NotSupportedException(ExceptionDescription.FileIsSupportedByCommonImageSource);
            }
        }

        internal static void CheckFileIsExist(string pathFileName)
        {
            
            if (!File.Exists(pathFileName))
            {
                throw new FileNotFoundException(ExceptionDescription.FileIsNotExist, pathFileName);
            }
        }

        internal static void CheckFileIsNotExist(string pathFileName)
        {
           
            if (File.Exists(pathFileName))
            {
                throw new ArgumentException(ExceptionDescription.FileAlreadyExists, pathFileName);
            }
        }
        

        internal static void CheckConnectStringIsNotNull(string connectionString)
        {
            

            if (connectionString == null)
            {
                throw new InvalidOperationException(ExceptionDescription.ConnectionStringCannotBeNull);
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
               
                case ShapeFileType.Null:
                default:
                    string errorDescription = ExceptionDescription.EnumerationOutOfRange;
                    throw new ArgumentOutOfRangeException(parameterName, errorDescription);
            }
        }

        internal static void CheckSqlStatementIsSupported(string sqlStatement)
        {
            

            if (!sqlStatement.Trim().StartsWith("update", StringComparison.CurrentCultureIgnoreCase))
            {
                throw new NotSupportedException(ExceptionDescription.NotSupportedSqlQueryMode);
            }
        }

        internal static void CheckShapeIsWriteable(ReadWriteMode readWriteMode)
        {
            
            switch (readWriteMode)
            {
                case ReadWriteMode.ReadWrite:
                    break;

                
                default:
                    throw new IOException(ExceptionDescription.FileAccessError);
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

    


        internal static void CheckWmsImageFormat(string format, Collection<string> outputFormats, string wmsUrl)
        {
            
            if (!outputFormats.Contains(format))
            {
                throw new ArgumentException(ExceptionDescription.WmsServerNotSupportFormat + wmsUrl, format);
            }

            bool isFormatSupported = false;
            foreach (ImageCodecInfo encoder in ImageCodecInfo.GetImageEncoders())
            {
                if (encoder.MimeType.ToUpperInvariant() == format.ToUpperInvariant())
                {
                    isFormatSupported = true;
                    break;
                }
            }

            if (!isFormatSupported)
            {
                throw new ArgumentException(ExceptionDescription.WmsServerNotSupportFormat + wmsUrl, format);
            }
        }

        internal static void CheckWmsLayerExists(Collection<string> activeLayerNames, Collection<string> serverLayerNames, string wmsUrl)
        {
           

            if (activeLayerNames.Count < 1)
            {
                throw new ArgumentException(ExceptionDescription.WmsLayerDoesNotExists);
            }

            bool isLayersUnlegal = false;
            string errorLayer = "";
            foreach (string layer in activeLayerNames)
            {
                if (!serverLayerNames.Contains(layer))
                {
                    isLayersUnlegal = true;
                    errorLayer = layer;
                    break;
                }
            }

            if (isLayersUnlegal)
            {
                throw new ArgumentException(ExceptionDescription.WmsServerNotSupportLayer + wmsUrl, errorLayer);
            }

        }

        internal static void CheckWmsStyleExists(Collection<string> activeStyleNames, Collection<string> serverStyleNames, string wmsUrl)
        {
            

            bool isStyleUnlegal = false;
            string errorStyle = "";
            foreach (string style in activeStyleNames)
            {
                if (!serverStyleNames.Contains(style))
                {
                    isStyleUnlegal = true;
                    errorStyle = style;
                    break;
                }
            }

            if (isStyleUnlegal)
            {
                throw new ArgumentException(ExceptionDescription.WmsServerNotSupportStyle + wmsUrl, errorStyle);
            }

        }

        internal static void CheckWmsCrsExists(string crs, Collection<string> wmsCrSs, string wmsUrl)
        {
            foreach (string item in wmsCrSs)
            {
                if (item.Contains(crs))
                {
                    return;
                }
            }

            if (!wmsCrSs.Contains(crs))
            {
                throw new ArgumentException(ExceptionDescription.WmsCRSNotSupport + wmsUrl);
            }
        }


        internal static void CheckIEnumerableIsEmptyOrNull(IEnumerable values)
        {
            if (values != null)
            {
                foreach (object item in values)
                {
                    return;
                }
            }

            throw new InvalidOperationException(ExceptionDescription.IEnumerableIsEmptyOrNull);
        }

        internal static void CheckFeatureSourceCollectionIsEmpty(Collection<BaseFeatureSource> featureSources)
        {
            if (featureSources.Count == 0)
            {
                throw new InvalidOperationException(ExceptionDescription.FeatureCollectionIsEmpty);
            }
        }


        internal static void CheckReturningColumnNamesTypeIsValid(ReturningColumnsType returningColumnNamesType, string parameterName)
        {
            switch (returningColumnNamesType)
            {
                case ReturningColumnsType.NoColumns:
                case ReturningColumnsType.AllColumns:
                    break;
                default:
                    string errorDescription = ExceptionDescription.EnumerationOutOfRange;
                    throw new ArgumentOutOfRangeException(parameterName, errorDescription);
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

        internal static void CheckSpatialDataTypeIsValid(SpatialDataType spatialDataType, string parameterName)
        {
            switch (spatialDataType)
            {
                case SpatialDataType.Geography:
                case SpatialDataType.Geometry:
                    break;
                default:
                    string errorDescription = ExceptionDescription.EnumerationOutOfRange;
                    throw new ArgumentOutOfRangeException(parameterName, errorDescription);
            }
        }

        internal static void CheckFeatureSourceCanExecuteSqlQuery(bool canExecuteSqlQuery)
        {
            if (!canExecuteSqlQuery)
            {
                throw new InvalidOperationException(ExceptionDescription.FeatureSourceCanNotExecuteSqlQuery);
            }
        }

        internal static void CheckRasterSourceHasProjectionText(bool hasProjectionText)
        {
            if (!hasProjectionText)
            {
                throw new InvalidOperationException(ExceptionDescription.RasterSourceNotContainsProjectionInformation);
            }
        }

        internal static void CheckShapeFileTypeIsSupported(ShapeFileType shapeFileType)
        {
            switch (shapeFileType)
            {
                case ShapeFileType.Null:
                case ShapeFileType.Point:
                case ShapeFileType.Multipoint:
                case ShapeFileType.Polyline:
                case ShapeFileType.Polygon:
                case ShapeFileType.PointZ:
                case ShapeFileType.PolylineZ:
                case ShapeFileType.PolygonZ:
                case ShapeFileType.MultipointZ:
                case ShapeFileType.PointM:
                case ShapeFileType.PolylineM:
                case ShapeFileType.PolygonM:
                case ShapeFileType.MultipointM:
                    break;
                case ShapeFileType.Multipatch:
                default:
                    throw new NotImplementedException(ExceptionDescription.ShapeTypeNotImplement);
            }
        }



        internal static void CheckShapeFileIsEditable(ShapeFileType shapeFileType)
        {
            switch (shapeFileType)
            {
                case ShapeFileType.Point:
                case ShapeFileType.Polyline:
                case ShapeFileType.Polygon:
                case ShapeFileType.Multipoint:
                    break;
                case ShapeFileType.Null:
                case ShapeFileType.PointZ:
                case ShapeFileType.PolylineZ:
                case ShapeFileType.PolygonZ:
                case ShapeFileType.MultipointZ:
                case ShapeFileType.PointM:
                case ShapeFileType.PolylineM:
                case ShapeFileType.PolygonM:
                case ShapeFileType.MultipointM:
                    break;
                case ShapeFileType.Multipatch:
                default:
                    throw new NotSupportedException(ExceptionDescription.UnsupportedEditingShapeType + shapeFileType);
            }
        }

      
        internal static void CheckStreamWritable(Stream outputStream, string parameterName)
        {
            if (!outputStream.CanWrite)
            {
                throw new ArgumentException(ExceptionDescription.StreamIsNotWritable, parameterName);
            }
        }

        internal static void CheckShapeFileBoundingBoxIsValid(BoundingBox boundingBox)
        {

            if (boundingBox.MaxX < boundingBox.MinX || boundingBox.MaxY < boundingBox.MinY)
            {
                throw new InvalidOperationException(ExceptionDescription.ShapeFileBoundingBoxIsValid);
            }
        }
    }
}

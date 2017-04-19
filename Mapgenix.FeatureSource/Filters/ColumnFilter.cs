using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Mapgenix.FeatureSource.Properties;
using Mapgenix.Shapes;
using System.Linq;

namespace Mapgenix.FeatureSource
{
    public static class ColumnFilter
	{

        public static string GetColumnNameAlias(string columnName, ICollection<string> columnNames)
        {
            return GetColumnNameAlias(columnName, columnNames, 2);
        }

        public static string GetColumnNameAlias(string columnName, ICollection<string> columnNames, int alias)
        {
            string result = columnName;
            if (columnNames.Contains(result))
            {
                string aliasString = alias.ToString();
                if (result.EndsWith(aliasString))
                {
                    int endNumber = int.Parse(result.Substring(result.Length - 1, 1));
                    endNumber++;
                    result = result.Substring(0, result.Length - 1) + endNumber;
                    alias++;
                }
                else
                {
                    result += aliasString;
                }

                result = GetColumnNameAlias(result, columnNames, alias);
            }
            return result;
        }

        public static Dictionary<string, Feature> GetFeaturesDictionaryFromCollecion(Collection<Feature> features)
        {
            Dictionary<string, Feature> returnDictionary = new Dictionary<string, Feature>(features.Count);
            foreach (Feature featue in features)
            {
                returnDictionary.Add(featue.Id, featue);
            }
            return returnDictionary;
        }

      
        public static Collection<Feature> GetFeaturesCollectionFromDictionary(Dictionary<string, Feature> features)
        {
            Collection<Feature> returnCollection = new Collection<Feature>();
            foreach (Feature featue in features.Values)
            {
                returnCollection.Add(featue);
            }
            return returnCollection;
        }



        public static Collection<Feature> CommitTransactionBufferOnFeatures(Collection<Feature> features, TransactionBuffer transactionBuffer, IEnumerable<string> returningIds)
        {
            Collection<Feature> returnFeatures = new Collection<Feature>();
            if (transactionBuffer != null)
            {
                TransactionResult result = new TransactionResult();

                Dictionary<string, Feature> dictionaryFeatures = GetFeaturesDictionaryFromCollecion(features);
         
                Dictionary<string, Feature> resultDictionaryFeatures = CommitTransactionBufferOnFeatures(dictionaryFeatures, transactionBuffer, out result);
                foreach (string id in returningIds.Where(id => resultDictionaryFeatures.ContainsKey(id)))
                {
                    returnFeatures.Add(resultDictionaryFeatures[id]);
                }
            }

            return returnFeatures;
        }

        public static Dictionary<string, Feature> CommitTransactionBufferOnFeatures(Dictionary<string, Feature> features, TransactionBuffer transactionBuffer, out TransactionResult transactionResult)
        {
            transactionResult = new TransactionResult();

            Dictionary<string, Feature> returnFeatures = new Dictionary<string, Feature>(features);
            bool isContains = false;
            foreach (string key in transactionBuffer.AddBuffer.Keys)
            {
                isContains = returnFeatures.ContainsKey(key);
                if (isContains)
                {
                    transactionResult.TotalSuccessCount += 1;
                }
                else
                {
                    returnFeatures.Add(key, transactionBuffer.AddBuffer[key]);
                    transactionResult.TotalFailureCount += 1;
                }
            }

            foreach (string key in transactionBuffer.DeleteBuffer)
            {
                isContains = returnFeatures.ContainsKey(key);
                if (isContains)
                {
                    returnFeatures.Remove(key);
                    transactionResult.TotalSuccessCount += 1;
                }
                else
                {
                    transactionResult.TotalFailureCount += 1;
                }
            }

            foreach (string key in transactionBuffer.EditBuffer.Keys)
            {
                isContains = returnFeatures.ContainsKey(key);
                if (isContains)
                {
                    returnFeatures[key] = transactionBuffer.EditBuffer[key];
                    transactionResult.TotalSuccessCount += 1;
                }
                else
                {
                    transactionResult.TotalFailureCount += 1;
                }
            }

            return returnFeatures;
        }

        public static Collection<string> SplitMultiFieldName(string multiFieldName)
        {
            Collection<string> fields = new Collection<string>();

            int startIndex = 0;

            while (true)
            {
                int openBlacketPosition = startIndex = multiFieldName.IndexOf('[', startIndex);
                if (startIndex == -1) { break; }
                int closeBlacketPosition = startIndex = multiFieldName.IndexOf(']', startIndex);
                if (startIndex == -1) { throw new NotSupportedException(ExceptionDescription.MultiFieldNamesNotSupportedError); }

                fields.Add(multiFieldName.Substring(openBlacketPosition + 1, closeBlacketPosition - openBlacketPosition - 1));
            }

            return fields;
        }


        public static Collection<string> SplitMultiFieldNameIncludeBlacket(string multiFieldName)
        {
            Collection<string> fields = new Collection<string>();

            int startIndex = 0;

            while (true)
            {
                int openBlacketPosition = startIndex = multiFieldName.IndexOf('[', startIndex);
                if (startIndex == -1) { break; }
                int closeBlacketPosition = startIndex = multiFieldName.IndexOf(']', startIndex);
                if (startIndex == -1) { throw new NotSupportedException(ExceptionDescription.MultiFieldNamesNotSupportedError); }

                fields.Add(multiFieldName.Substring(openBlacketPosition, closeBlacketPosition - openBlacketPosition + 1));
            }

            return fields;
        }

       
        public static Collection<string> SplitColumnNames(IEnumerable<string> returningColumnNames)
        {
            Validators.CheckParameterIsNotNull(returningColumnNames, "returningColumnNames");

            Collection<string> uniqColumnNames = new Collection<string>();

            foreach (string columnName in returningColumnNames)
            {
                if (columnName.IndexOf('[') == -1)
                {
                    string newColumnName = GetColumnNameAlias(columnName, uniqColumnNames);
                    uniqColumnNames.Add(newColumnName);
                }
                else
                {
                    Collection<string> fields = SplitMultiFieldName(columnName);
                    foreach (string newColumnName in fields.Select(field => field.Trim('[', ']')).Select(trimFieldName => GetColumnNameAlias(trimFieldName, uniqColumnNames)))
                    {
                        uniqColumnNames.Add(newColumnName);
                    }
                }
            }
            return uniqColumnNames;
        }

     
        public static Dictionary<string, string> CombineFieldValues(Dictionary<string, string> columnValues, IEnumerable<string> originalColumnNames)
        {
            Validators.CheckParameterIsNotNull(columnValues, "columnValues");
            Validators.CheckParameterIsNotNull(originalColumnNames, "originalColumnNames");

            Dictionary<string, string> combineColumnValues = new Dictionary<string, string>();

            foreach (string columnName in originalColumnNames)
            {
                if (columnName.IndexOf('[') == -1)
                {
                    combineColumnValues.Add(columnName, columnValues[columnName]);
                }
                else
                {
                    Collection<string> fields = SplitMultiFieldNameIncludeBlacket(columnName);
                    if (fields.Count == 0) { continue; }
                    string result = columnName;
                    foreach (string field in fields)
                    {
                        string trimFieldName = field.Trim('[', ']');
                        result = result.Replace(field, columnValues[trimFieldName].Trim());
                    }
                    combineColumnValues.Add(columnName, result.Trim());
                }
            }

            return combineColumnValues;
        }

        public static Collection<Feature> ReplaceColumnValues(Collection<Feature> orginalFeatures, IEnumerable<string> originalColumnNames)
        {
            Collection<Feature> resultFeatures = new Collection<Feature>();

            foreach (Feature tempFeature in orginalFeatures)
            {
                resultFeatures.Add(new Feature(tempFeature.GetWellKnownBinary(), tempFeature.Id, CombineFieldValues(tempFeature.ColumnValues, originalColumnNames)));
            }

            return resultFeatures;
        }

        public static int GetMaxNumberIndex(List<double> doubles)
        {
            int returnValue = 0;
            double maxNumber = doubles[0];

            for (int i = 1; i < doubles.Count; i++)
            {
                if (maxNumber < doubles[i])
                {
                    returnValue = i;
                    maxNumber = doubles[i];
                }
            }
            return returnValue;
        }

        public static bool HasComplicateFields(IEnumerable<string> originalComlumnNames)
        {
            return originalComlumnNames.Any(columnName => columnName.IndexOf('[') != -1);
        }

        public static MultipointShape GetGridFromRectangle(RectangleShape rectangleShape, int pointCountBySide)
        {
            MultipointShape multiPointShape = new MultipointShape();

            double maxSide;
            if (rectangleShape.Width > rectangleShape.Height) { maxSide = rectangleShape.Width; }
            else
            { maxSide = rectangleShape.Height; }

            double interval = maxSide / pointCountBySide;

            double x = rectangleShape.UpperLeftPoint.X;
            while (x < rectangleShape.UpperRightPoint.X)
            {
                double y = rectangleShape.UpperLeftPoint.Y;
                while (y > rectangleShape.LowerLeftPoint.Y)
                {
                    multiPointShape.Points.Add(new PointShape(x, y));
                    y = y - interval;
                }
                x = x + interval;
            }

            return multiPointShape;
        }

        public static Collection<Feature> GetFeaturesNearestFrom(Collection<Feature> possibleResults, BaseShape targetShape, GeographyUnit unitOfData, int maxItemsToFind)
        {
            List<Feature> nearestFeaturesInList = new List<Feature>(maxItemsToFind);
            List<double> distancesOfFeaturesInList = new List<double>(maxItemsToFind);
            double maxDistanceInList = double.MinValue;

            foreach (Feature feature in possibleResults)
            {
                BaseShape baseShapeInFeature = feature.GetShape();
                double distance = baseShapeInFeature.GetDistanceTo(targetShape, unitOfData, DistanceUnit.Meter);
                if (nearestFeaturesInList.Count < maxItemsToFind)
                {
                    nearestFeaturesInList.Add(feature);
                    distancesOfFeaturesInList.Add(distance);
                    if (distance > maxDistanceInList)
                    {
                        maxDistanceInList = distance;
                    }
                }
                else if (distance < maxDistanceInList)
                {
                    int indexOfMaxDistance = GetMaxNumberIndex(distancesOfFeaturesInList);
                    nearestFeaturesInList[indexOfMaxDistance] = feature;
                    distancesOfFeaturesInList[indexOfMaxDistance] = distance;

                    indexOfMaxDistance = GetMaxNumberIndex(distancesOfFeaturesInList);
                    maxDistanceInList = distancesOfFeaturesInList[indexOfMaxDistance];
                }
            }

            Collection<Feature> returningFeatures = new Collection<Feature>();

            foreach (Feature nearestFeature in nearestFeaturesInList)
            {
                returningFeatures.Add(nearestFeature);
            }

            return returningFeatures;
        }

        public static bool SpatialQueryIsValid(BaseShape targetShape, BaseShape sourceShape, QueryType queryType)
        {
            bool returnValue = false;

            switch (queryType)
            {
                case QueryType.Contains:
                    returnValue = sourceShape.Contains(targetShape);
                    break;
                case QueryType.Crosses:
                    returnValue = sourceShape.Crosses(targetShape);
                    break;
                case QueryType.Disjoint:
                    returnValue = sourceShape.IsDisjointed(targetShape);
                    break;
                case QueryType.Intersects:
                    returnValue = sourceShape.Intersects(targetShape);
                    break;
                case QueryType.TopologicalEqual:
                    returnValue = sourceShape.IsTopologicallyEqual(targetShape);
                    break;
                case QueryType.Overlaps:
                    returnValue = sourceShape.Overlaps(targetShape);
                    break;
                case QueryType.Touches:
                    returnValue = sourceShape.Touches(targetShape);
                    break;
                case QueryType.Within:
                    returnValue = sourceShape.IsWithin(targetShape);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("queryType", ExceptionDescription.EnumerationOutOfRange);
            }
            return returnValue;
        }

    }

}


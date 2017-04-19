using Mapgenix.Shapes;
using Mapgenix.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;


namespace Mapgenix.FeatureSource
{
    /// <summary>Wrapper class for executing queries.</summary>
    [Serializable]
    public class QueryTools
    {
        private BaseFeatureSource _featureSource;

        public QueryTools(BaseFeatureSource featureSource)
        {
            _featureSource = featureSource;
        }

        /// <summary>Whether FeatureSource can excute a SQL query or not.
        /// If it is false, it throws an exception when the following APIs are called: ExecuteScalar, ExecuteNonQuery, ExecuteQuery</summary>
        /// <remarks>The default implementation is false.</remarks>
        public bool CanExecuteSqlQuery
        {
            get
            {
                Validators.CheckFeatureSourceIsOpen(_featureSource.IsOpen);

                return _featureSource.CanExecuteSqlQuery;
            }
        }

        /// <summary>Executes a SQL statement against a connection object.</summary>
        /// <returns>Number of rows affected.</returns>
        /// <remarks>Use ExecuteNonQuery to perform catalog operations (for example, querying the structure of a database or creating database
        ///objects such as tables), or to change the data in a database by executing UPDATE, INSERT, or DELETE statements.</remarks>
        /// <param name="sqlStatement">The sqlStatement to be excuted.</param>
        public int ExecuteNonQuery(string sqlStatement)
        {
            Validators.CheckFeatureSourceIsOpen(_featureSource.IsOpen);
            Validators.CheckParameterIsNotNullOrEmpty(sqlStatement, "sqlStatement");
            Validators.CheckFeatureSourceCanExecuteSqlQuery(_featureSource.CanExecuteSqlQuery);

            return _featureSource.ExecuteNonQuery(sqlStatement);
        }

        /// <summary>Executes the query and returns the first column of the first row in the result
        /// set returned by the query. All other columns and rows are ignored.</summary>
        /// <returns>The first column of the first row in the result set.</returns>
        /// <remarks>Use the ExcuteScalar method to retrieve a single value from the database. This
        /// requires less code than use the ExcuteQuery method and then performing the operations
        /// necessary to generate the single value using the data.</remarks>
        /// <param name="sqlStatement">SQL statement to be excuted.</param>
        public object ExecuteScalar(string sqlStatement)
        {
            Validators.CheckFeatureSourceIsOpen(_featureSource.IsOpen);
            Validators.CheckParameterIsNotNullOrEmpty(sqlStatement, "sqlStatement");
            Validators.CheckFeatureSourceCanExecuteSqlQuery(_featureSource.CanExecuteSqlQuery);

            return _featureSource.ExecuteScalar(sqlStatement);
        }

        /// <summary>Returns a collection of features according to a column value.</summary>
        /// <returns>Collection of features according to a column value.</returns>
        /// <param name="columnName">Name of the column.</param>
        /// <param name="columnValue">Value of the column.</param>
        public Collection<Feature> GetFeaturesByColumnValue(string columnName, string columnValue)
        {
            Validators.CheckFeatureSourceIsOpen(_featureSource.IsOpen);
            Validators.CheckParameterIsNotNullOrEmpty(columnName, "columnName");
        
            Collection<Feature> returnFeatures = _featureSource.GetFeaturesByColumnValue(columnName, columnValue);

            return returnFeatures;
        }

        /// <summary>Returns collection of features containing the target shape.</summary>
        /// <returns>Collection of features containing the target shape</returns>
        /// <remarks>If there is a current transaction and it is marked as live, then the result
        /// includes any transaction Feature that applies.</remarks>
        /// <param name="targetShape">Target shape used in the spatial query.</param>
        /// <param name="returningColumnNames">Columns contained in the returned features.</param>
        public Collection<Feature> GetFeaturesContaining(BaseShape targetShape, IEnumerable<string> returningColumnNames)
        {
            Validators.CheckFeatureSourceIsOpen(_featureSource.IsOpen);
            Validators.CheckParameterIsNotNull(targetShape, "targetShape");
            Validators.CheckParameterIsNotNull(returningColumnNames, "returningColumnNames");

            return _featureSource.SpatialQuery(targetShape, QueryType.Contains, returningColumnNames);
        }

        /// <summary>Returns collection of features containing the target shape.</summary>
        /// <returns>Collection of features containing the target shape</returns>
        /// <remarks>If there is a current transaction and it is marked as live, then the result
        /// includes any transaction Feature that applies.</remarks>
        /// <param name="targetShape">Target shape used in the spatial query.</param>
        /// <param name="returningColumnsType">Column type.</param>
        public Collection<Feature> GetFeaturesContaining(BaseShape targetShape, ReturningColumnsType returningColumnsType)
        {
            Validators.CheckFeatureSourceIsOpen(_featureSource.IsOpen);
            Validators.CheckParameterIsNotNull(targetShape, "targetShape");
            Validators.CheckReturningColumnNamesTypeIsValid(returningColumnsType, "returningColumnNamesType");

            return _featureSource.SpatialQuery(targetShape, QueryType.Contains, returningColumnsType);
        }

        /// <summary>Returns collection of features containing the target shape.</summary>
        /// <returns>Collection of features containing the target shape</returns>
        /// <remarks>If there is a current transaction and it is marked as live, then the result
        /// includes any transaction Feature that applies.</remarks>
        /// <param name="targetShape">Target shape used in the spatial query.</param>
        /// <param name="returningColumnNames">Columns contained in the returned features.</param>
        public Collection<Feature> GetFeaturesContaining(Feature targetFeature, IEnumerable<string> returningColumnNames)
        {
            Validators.CheckFeatureSourceIsOpen(_featureSource.IsOpen);
            Validators.CheckParameterIsNotNull(returningColumnNames, "returningColumnNames");

            return _featureSource.SpatialQuery(targetFeature, QueryType.Contains, returningColumnNames);
        }

        /// <summary>Returns a collection of features that cross the target shape.</summary>
        /// <returns>Collection of features that cross the target shape.</returns>
        /// <param name="targetShape">Target shape used in the spatial query.</param>
        /// <param name="returningColumnNames">Columns contained in the returned features.</param>
        public Collection<Feature> GetFeaturesCrossing(BaseShape targetShape, IEnumerable<string> returningColumnNames)
        {
            Validators.CheckFeatureSourceIsOpen(_featureSource.IsOpen);
            Validators.CheckParameterIsNotNull(targetShape, "targetShape");
            Validators.CheckParameterIsNotNull(returningColumnNames, "returningColumnNames");

            return _featureSource.SpatialQuery(targetShape, QueryType.Crosses, returningColumnNames);
        }
        /// <summary>Returns a collection of features that cross the target shape.</summary>
        /// <returns>Collection of features that cross the target shape.</returns>
        /// <param name="targetShape">Target shape.</param>
        /// <param name="returningColumnNamesType">Column name type.</param>
        public Collection<Feature> GetFeaturesCrossing(BaseShape targetShape, ReturningColumnsType returningColumnsType)
        {
            Validators.CheckFeatureSourceIsOpen(_featureSource.IsOpen);
            Validators.CheckParameterIsNotNull(targetShape, "targetShape");
            Validators.CheckReturningColumnNamesTypeIsValid(returningColumnsType, "returningColumnNamesType");

            return _featureSource.SpatialQuery(targetShape, QueryType.Crosses, returningColumnsType);
        }

        /// <summary>Returns a collection of features that cross the target shape.</summary>
        /// <returns>Collection of features that cross the target shape.</returns>
        /// <param name="targetShape">Target shape used in the spatial query.</param>
        /// <param name="returningColumnNames">Columns contained in the returned features.</param>
        public Collection<Feature> GetFeaturesCrossing(Feature targetFeature, IEnumerable<string> returningColumnNames)
        {
            Validators.CheckFeatureSourceIsOpen(_featureSource.IsOpen);
            Validators.CheckParameterIsNotNull(returningColumnNames, "returningColumnNames");

            return _featureSource.SpatialQuery(targetFeature, QueryType.Crosses, returningColumnNames);
        }

        /// <summary>Returns a collection of features that disjoint the target Feature.</summary>
        /// <returns>Collection of features that disjoint the target Feature.</returns>
        /// <remarks>If there is a current transaction and it is marked as live, then the results
        /// will include any transaction Feature that applies.</remarks>
         /// <param name="targetShape">Target shape.</param>
        /// <param name="returningColumnNames">Columns contained in the returned features.</param>
        public Collection<Feature> GetFeaturesDisjointed(BaseShape targetShape, IEnumerable<string> returningColumnNames)
        {
            Validators.CheckFeatureSourceIsOpen(_featureSource.IsOpen);
            Validators.CheckParameterIsNotNull(targetShape, "targetShape");
            Validators.CheckParameterIsNotNull(returningColumnNames, "returningColumnNames");

            return _featureSource.SpatialQuery(targetShape, QueryType.Disjoint, returningColumnNames);
        }

        /// <summary>Returns a collection of features that disjoint the target Feature.</summary>
        /// <returns>Collection of features that disjoint the target Feature.</returns>
        /// <remarks>If there is a current transaction and it is marked as live, then the results
        /// will include any transaction Feature that applies.</remarks>
        /// <param name="targetShape">Target shape.</param>
        /// <param name="returningColumnType">Column type.</param>
        public Collection<Feature> GetFeaturesDisjointed(BaseShape targetShape, ReturningColumnsType returningColumnsType)
        {
            Validators.CheckFeatureSourceIsOpen(_featureSource.IsOpen);
            Validators.CheckParameterIsNotNull(targetShape, "targetShape");
            Validators.CheckReturningColumnNamesTypeIsValid(returningColumnsType, "returningColumnNamesType");

            return _featureSource.SpatialQuery(targetShape, QueryType.Disjoint, returningColumnsType);
        }

        /// <summary>Returns a collection of features that disjoint the target Feature.</summary>
        /// <returns>Collection of features that disjoint the target Feature.</returns>
        /// <remarks>If there is a current transaction and it is marked as live, then the results
        /// will include any transaction Feature that applies.</remarks>
        /// <param name="targetShape">Target shape.</param>
        /// <param name="returningColumnNames">Columns contained in the returned features.</param>
        public Collection<Feature> GetFeaturesDisjointed(Feature targetFeature, IEnumerable<string> returningColumnNames)
        {
            Validators.CheckFeatureSourceIsOpen(_featureSource.IsOpen);
            Validators.CheckParameterIsNotNull(returningColumnNames, "returningColumnNames");

            return _featureSource.SpatialQuery(targetFeature, QueryType.Disjoint, returningColumnNames);
        }

        /// <summary>Returns a collection of features that intersect the target Feature.</summary>
        /// <returns>Collection of features that intersect the target Feature.</returns>
        /// <remarks>If there is a current transaction and it is marked as live, then the results
        /// will include any transaction Feature that applies.</remarks>
        /// <param name="targetShape">Target shape.</param>
        /// <param name="returningColumnNames">Columns contained in the returned features.</param>
        public Collection<Feature> GetFeaturesIntersecting(BaseShape targetShape, IEnumerable<string> returningColumnNames)
        {
            Validators.CheckFeatureSourceIsOpen(_featureSource.IsOpen);
            Validators.CheckParameterIsNotNull(targetShape, "targetShape");
            Validators.CheckParameterIsNotNull(returningColumnNames, "returningColumnNames");

            return _featureSource.SpatialQuery(targetShape, QueryType.Intersects, returningColumnNames);
        }

        /// <summary>Returns a collection of features that intersect the target Feature.</summary>
        /// <returns>Collection of features that intersect the target Feature.</returns>
        /// <remarks>If there is a current transaction and it is marked as live, then the results
        /// will include any transaction Feature that applies.</remarks>
        /// <param name="targetShape">Target shape.</param>
        /// <param name="returningColumnType">Column type.</param>
        public Collection<Feature> GetFeaturesIntersecting(BaseShape targetShape, ReturningColumnsType returningColumnsType)
        {
            Validators.CheckFeatureSourceIsOpen(_featureSource.IsOpen);
            Validators.CheckParameterIsNotNull(targetShape, "targetShape");
            Validators.CheckReturningColumnNamesTypeIsValid(returningColumnsType, "returningColumnNamesType");

            return _featureSource.SpatialQuery(targetShape, QueryType.Intersects, returningColumnsType);
        }

        /// <summary>Returns a collection of features that intersect the target Feature.</summary>
        /// <returns>Collection of features that intersect the target Feature.</returns>
        /// <remarks>If there is a current transaction and it is marked as live, then the results
        /// will include any transaction Feature that applies.</remarks>
        /// <param name="targetShape">Target shape.</param>
        /// <param name="returningColumnNames">Columns contained in the returned features.</param>
        public Collection<Feature> GetFeaturesIntersecting(Feature targetFeature, IEnumerable<string> returningColumnNames)
        {
            Validators.CheckFeatureSourceIsOpen(_featureSource.IsOpen);
            Validators.CheckParameterIsNotNull(returningColumnNames, "returningColumnNames");

            return _featureSource.SpatialQuery(targetFeature, QueryType.Intersects, returningColumnNames);
        }

        /// <summary>Returns a collection of features that overlap the target Feature.</summary>
        /// <returns>Collection of features that overlap the target Feature.</returns>
        /// <remarks>If there is a current transaction and it is marked as live, then the results
        ///will include any transaction Feature that applies.</remarks>
        /// <param name="targetShape">Target shape.</param>
        /// <param name="returningColumnNames">Column values in the returned features.</param>
        public Collection<Feature> GetFeaturesOverlapping(BaseShape targetShape, IEnumerable<string> returningColumnNames)
        {
            Validators.CheckFeatureSourceIsOpen(_featureSource.IsOpen);
            Validators.CheckParameterIsNotNull(targetShape, "targetShape");
            Validators.CheckParameterIsNotNull(returningColumnNames, "returningColumnNames");

            return _featureSource.SpatialQuery(targetShape, QueryType.Overlaps, returningColumnNames);
        }

        /// <summary>Returns a collection of features that overlap the target Feature.</summary>
        /// <returns>Collection of features that overlap the target Feature.</returns>
        /// <remarks>If there is a current transaction and it is marked as live, then the results
        ///will include any transaction Feature that applies.</remarks>
        /// <param name="targetShape">Target shape.</param>
        /// <param name="returningColumnType">Column type.</param>
        public Collection<Feature> GetFeaturesOverlapping(BaseShape targetShape, ReturningColumnsType returningColumnsType)
        {
            Validators.CheckFeatureSourceIsOpen(_featureSource.IsOpen);
            Validators.CheckParameterIsNotNull(targetShape, "targetShape");
            Validators.CheckReturningColumnNamesTypeIsValid(returningColumnsType, "returningColumnNamesType");

            return _featureSource.SpatialQuery(targetShape, QueryType.Overlaps, returningColumnsType);
        }

        /// <summary>Returns a collection of features that overlap the target Feature.</summary>
        /// <returns>Collection of features that overlap the target Feature.</returns>
        /// <remarks>If there is a current transaction and it is marked as live, then the results
        ///will include any transaction Feature that applies.</remarks>
        /// <param name="targetShape">Target shape.</param>
        /// <param name="returningColumnNames">Column values in the returned features.</param>
        public Collection<Feature> GetFeaturesOverlapping(Feature targetFeature, IEnumerable<string> returningColumnNames)
        {
            Validators.CheckFeatureSourceIsOpen(_featureSource.IsOpen);
            Validators.CheckParameterIsNotNull(returningColumnNames, "returningColumnNames");

            return _featureSource.SpatialQuery(targetFeature, QueryType.Overlaps, returningColumnNames);
        }

        
        /// <summary>Returns collection of features topologically equal to the target shape.</summary>
        /// <returns>Collection of features topologically equal to the target shape.</returns>
        /// <remarks>If there is a current transaction and it is marked as live, then the results
        ///will include any transaction Feature that applies.</remarks>
        /// <param name="targetShape">Target shape.</param>
        /// <param name="returningColumnNames">Column values in the returned features.</param>
        public Collection<Feature> GetFeaturesTopologicalEqual(BaseShape targetShape, IEnumerable<string> returningColumnNames)
        {
            Validators.CheckFeatureSourceIsOpen(_featureSource.IsOpen);
            Validators.CheckParameterIsNotNull(targetShape, "targetShape");
            Validators.CheckParameterIsNotNull(returningColumnNames, "returningColumnNames");

            return _featureSource.SpatialQuery(targetShape, QueryType.TopologicalEqual, returningColumnNames);
        }

        /// <summary>Returns ollection of features topologically equal to the target shape.</summary>
        /// <returns>Collection of features topologically equal to the target shape.</returns>
        /// <remarks>If there is a current transaction and it is marked as live, then the results
        ///will include any transaction Feature that applies.</remarks>
        /// <param name="targetShape">Target shape.</param>
        /// <param name="returningColumnNamesType">Column values in the returned features.</param>
        public Collection<Feature> GetFeaturesTopologicalEqual(BaseShape targetShape, ReturningColumnsType returningColumnsType)
        {
            Validators.CheckFeatureSourceIsOpen(_featureSource.IsOpen);
            Validators.CheckParameterIsNotNull(targetShape, "targetShape");
            Validators.CheckReturningColumnNamesTypeIsValid(returningColumnsType, "returningColumnNamesType");

            return _featureSource.SpatialQuery(targetShape, QueryType.TopologicalEqual, returningColumnsType);
        }

        /// <summary>Returns ollection of features topologically equal to the target shape.</summary>
        /// <returns>Collection of features topologically equal to the target shape.</returns>
        /// <remarks>If there is a current transaction and it is marked as live, then the results
        ///will include any transaction Feature that applies.</remarks>
        /// <param name="targetShape">Target shape.</param>
        /// <param name="returningColumnNamesType">Column values in the returned features.</param>
        public Collection<Feature> GetFeaturesTopologicalEqual(Feature targetFeature, IEnumerable<string> returningColumnNames)
        {
            Validators.CheckFeatureSourceIsOpen(_featureSource.IsOpen);
            Validators.CheckParameterIsNotNull(returningColumnNames, "returningColumnNames");

            return _featureSource.SpatialQuery(targetFeature, QueryType.TopologicalEqual, returningColumnNames);
        }

        /// <summary>Returns a collection of features that touch the target shape.</summary>
        /// <returns>Collection of features that touch the target shape.</returns>
        /// <remarks>If there is a current transaction and it is marked as live, then the results
        /// will include any transaction Feature that applies.</remarks>
        /// <param name="targetShape">Target shape.</param>
        /// <param name="returningColumnNames">Column values in the returned features.</param>
        public Collection<Feature> GetFeaturesTouching(BaseShape targetShape, IEnumerable<string> returningColumnNames)
        {
            Validators.CheckFeatureSourceIsOpen(_featureSource.IsOpen);
            Validators.CheckParameterIsNotNull(targetShape, "targetShape");
            Validators.CheckParameterIsNotNull(returningColumnNames, "returningColumnNames");

            return _featureSource.SpatialQuery(targetShape, QueryType.Touches, returningColumnNames);
        }

        /// <summary>Returns a collection of features that touch the target shape.</summary>
        /// <returns>Collection of features that touch the target shape.</returns>
        /// <remarks>If there is a current transaction and it is marked as live, then the results
        /// will include any transaction Feature that applies.</remarks>
        /// <param name="targetShape">Target shape.</param>
        /// <param name="returningColumnNamesType">Column type.</param>
        public Collection<Feature> GetFeaturesTouching(BaseShape targetShape, ReturningColumnsType returningColumnsType)
        {
            Validators.CheckFeatureSourceIsOpen(_featureSource.IsOpen);
            Validators.CheckParameterIsNotNull(targetShape, "targetShape");
            Validators.CheckReturningColumnNamesTypeIsValid(returningColumnsType, "returningColumnNamesType");

            return _featureSource.SpatialQuery(targetShape, QueryType.Touches, returningColumnsType);
        }

        /// <summary>Returns a collection of features that touch the target shape.</summary>
        /// <returns>Collection of features that touch the target shape.</returns>
        /// <remarks>If there is a current transaction and it is marked as live, then the results
        /// will include any transaction Feature that applies.</remarks>
        /// <param name="targetFeature">Target feature.</param>
        /// <param name="returningColumnNames">Column names in the returned features.</param>
        public Collection<Feature> GetFeaturesTouching(Feature targetFeature, IEnumerable<string> returningColumnNames)
        {
            Validators.CheckFeatureSourceIsOpen(_featureSource.IsOpen);
            Validators.CheckParameterIsNotNull(returningColumnNames, "returningColumnNames");

            return _featureSource.SpatialQuery(targetFeature, QueryType.Touches, returningColumnNames);
        }

      
        /// <summary>Returns a collection of features that are within the target Feature.</summary>
        /// <returns>Collection of features that are within the target Feature.</returns>
        /// <remarks>If there is a current transaction and it is marked as live, then the results
        /// will include any transaction Feature that applies.</remarks>
        /// <param name="targetShape">Target shape.</param>
        /// <param name="returningColumnNames">Column names in the returned features.</param>
        public Collection<Feature> GetFeaturesWithin(BaseShape targetShape, IEnumerable<string> returningColumnNames)
        {
            Validators.CheckFeatureSourceIsOpen(_featureSource.IsOpen);
            Validators.CheckParameterIsNotNull(targetShape, "targetShape");
            Validators.CheckParameterIsNotNull(returningColumnNames, "returningColumnNames");

            return _featureSource.SpatialQuery(targetShape, QueryType.Within, returningColumnNames);
        }

        /// <summary>Returns a collection of features that are within the target Feature.</summary>
        /// <returns>Collection of features that are within the target Feature.</returns>
        /// <remarks>If there is a current transaction and it is marked as live, then the results
        /// will include any transaction Feature that applies.</remarks>
        /// <param name="targetShape">Target shape.</param>
        /// <param name="returningColumnNamesType">Column type.</param>
        public Collection<Feature> GetFeaturesWithin(BaseShape targetShape, ReturningColumnsType returningColumnsType)
        {
            Validators.CheckFeatureSourceIsOpen(_featureSource.IsOpen);
            Validators.CheckParameterIsNotNull(targetShape, "targetShape");
            Validators.CheckReturningColumnNamesTypeIsValid(returningColumnsType, "returningColumnNamesType");

            return _featureSource.SpatialQuery(targetShape, QueryType.Within, returningColumnsType);
        }

        /// <summary>Returns a collection of features that are within the target Feature.</summary>
        /// <returns>Collection of features that are within the target Feature.</returns>
        /// <remarks>If there is a current transaction and it is marked as live, then the results
        /// will include any transaction Feature that applies.</remarks>
        /// <param name="targetShape">Target shape.</param>
        /// <param name="returningColumnNames">Column names in the returned features.</param>
        public Collection<Feature> GetFeaturesWithin(Feature targetFeature, IEnumerable<string> returningColumnNames)
        {
            Validators.CheckFeatureSourceIsOpen(_featureSource.IsOpen);
            Validators.CheckParameterIsNotNull(returningColumnNames, "returningColumnNames");

            return _featureSource.SpatialQuery(targetFeature, QueryType.Within, returningColumnNames);
        }

        /// <summary>Returns a user defined number collection of features that are closest to the TargetShape.</summary>
        /// <returns>User defined number collection of features that are closest to the TargetShape.</returns>
        /// <remarks>It is important to note that the TargetShape and the FeatureSource
        ///     must be in the same unit, such as feet or meters. If there is a current transaction and it is marked as live, then
        ///     the results will include any transaction Feature that applies.</remarks>
        /// <param name="targetShape">Shape to find nearest features from.</param>
        /// <param name="unitOfData">Unit of data that the TargetShape and the FeatureSource are in, such as feet, meters, etc.</param>
        /// <param name="maxItemsToFind">Maximum number of features to find.</param>
        /// <param name="returningColumnNames">Column names to return with each Feature.</param>
        public Collection<Feature> GetFeaturesNearestTo(BaseShape targetShape, GeographyUnit unitOfData, int maxItemsToFind, IEnumerable<string> returningColumnNames)
        {
            Validators.CheckFeatureSourceIsOpen(_featureSource.IsOpen);
            Validators.CheckParameterIsNotNull(targetShape, "targetShape");
            Validators.CheckParameterIsNotNull(returningColumnNames, "returningColumnNames");
            Validators.CheckGeographyUnitIsValid(unitOfData, "unitOfData");
            Validators.CheckIfInputValueIsInRange(maxItemsToFind, "maxItemsToFind", 0, RangeCheckingInclusion.ExcludeValue, int.MaxValue, RangeCheckingInclusion.ExcludeValue);

            return _featureSource.GetFeaturesNearestTo(targetShape, unitOfData, maxItemsToFind, returningColumnNames);
        }

        /// <summary>Returns a user defined number collection of features that are closest to the TargetShape.</summary>
        /// <returns>User defined number collection of features that are closest to the TargetShape.</returns>
        /// <remarks>It is important to note that the TargetShape and the FeatureSource
        ///     must be in the same unit, such as feet or meters. If there is a current transaction and it is marked as live, then
        ///     the results will include any transaction Feature that applies.</remarks>
        /// <param name="targetShape">Shape to find nearest features from.</param>
        /// <param name="unitOfData">Unit of data that the TargetShape and the FeatureSource are in, such as feet, meters, etc.</param>
        /// <param name="maxItemsToFind">Maximum number of features to find.</param>
        /// <param name="returningColumnNamesType">Column type.</param>
        public Collection<Feature> GetFeaturesNearestTo(BaseShape targetShape, GeographyUnit unitOfData, int maxItemsToFind, ReturningColumnsType returningColumnNamesType)
        {
            Validators.CheckFeatureSourceIsOpen(_featureSource.IsOpen);
            Validators.CheckParameterIsNotNull(targetShape, "targetShape");
            Validators.CheckGeographyUnitIsValid(unitOfData, "unitOfData");
            Validators.CheckIfInputValueIsInRange(maxItemsToFind, "maxItemsToFind", 0, RangeCheckingInclusion.ExcludeValue, int.MaxValue, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckReturningColumnNamesTypeIsValid(returningColumnNamesType, "returningColumnNamesType");

            return _featureSource.GetFeaturesNearestTo(targetShape, unitOfData, maxItemsToFind, returningColumnNamesType);
        }

        /// <summary>Returns a user defined number collection of features that are closest to the TargetShape.</summary>
        /// <returns>User defined number collection of features that are closest to the TargetShape.</returns>
        /// <remarks>It is important to note that the TargetShape and the FeatureSource
        ///     must be in the same unit, such as feet or meters. If there is a current transaction and it is marked as live, then
        ///     the results will include any transaction Feature that applies.</remarks>
        /// <param name="targetShape">Shape to find nearest features from.</param>
        /// <param name="unitOfData">Unit of data that the TargetShape and the FeatureSource are in, such as feet, meters, etc.</param>
        /// <param name="maxItemsToFind">Maximum number of features to find.</param>
        /// <param name="returningColumnNames">Column names.</param>
        public Collection<Feature> GetFeaturesNearestTo(Feature targetFeature, GeographyUnit unitOfData, int maxItemsToFind, IEnumerable<string> returningColumnNames)
        {
            Validators.CheckFeatureSourceIsOpen(_featureSource.IsOpen);
            Validators.CheckGeographyUnitIsValid(unitOfData, "unitOfData");
            Validators.CheckIfInputValueIsInRange(maxItemsToFind, "maxItemsToFind", 0, RangeCheckingInclusion.ExcludeValue, int.MaxValue, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckParameterIsNotNull(returningColumnNames, "returningColumnNames");

            return _featureSource.GetFeaturesNearestTo(targetFeature, unitOfData, maxItemsToFind, returningColumnNames);
        }

        /// <summary>Returns a collection of features that are within a certain distance of a TargetShape.</summary>
        /// <returns>Collection of features that are within a certain distance of a TargetShape</returns>
        /// <remarks> It is important to note that the TargetShape and the
        ///     FeatureSource must use the same unit, such as feet or meters. If there is a current transaction and
        ///     it is marked as live, then the results will include any transaction Feature that
        ///     applies.</remarks>
        /// <param name="targetShape">Shape to find features within a distance of.</param>
        /// <param name="unitOfData">Unit of data that the FeatureSource and TargetShape are in.</param>
        /// <param name="distanceUnit">Unit of the distance parameter, such as feet, miles, kilometers, etc.</param>
        /// <param name="distance">Distance</param>
        /// <param name="returningColumnNames">Column names.</param>
        public Collection<Feature> GetFeaturesWithinDistanceOf(BaseShape targetShape, GeographyUnit unitOfData, DistanceUnit distanceUnit, double distance, IEnumerable<string> returningColumnNames)
        {
            Validators.CheckFeatureSourceIsOpen(_featureSource.IsOpen);
            Validators.CheckParameterIsNotNull(targetShape, "targetShape");
            Validators.CheckParameterIsNotNull(returningColumnNames, "returningColumnNames");
            Validators.CheckGeographyUnitIsValid(unitOfData, "unitOfData");
            Validators.CheckDistanceUnitIsValid(distanceUnit, "distanceUnit");

            return _featureSource.GetFeaturesWithinDistanceOf(targetShape, unitOfData, distanceUnit, distance, returningColumnNames);
        }

        /// <summary>Returns a collection of features that are within a certain distance of a TargetShape.</summary>
        /// <returns>Collection of features that are within a certain distance of a TargetShape</returns>
        /// <remarks> It is important to note that the TargetShape and the
        ///     FeatureSource must use the same unit, such as feet or meters. If there is a current transaction and
        ///     it is marked as live, then the results will include any transaction Feature that
        ///     applies.</remarks>
        /// <param name="targetShape">Shape to find features within a distance of.</param>
        /// <param name="unitOfData">Unit of data that the FeatureSource and TargetShape are in.</param>
        /// <param name="distanceUnit">Unit of the distance parameter, such as feet, miles, kilometers, etc.</param>
        /// <param name="distance">Distance</param>
        /// <param name="returningColumnNamesType">Column type.</param>
        public Collection<Feature> GetFeaturesWithinDistanceOf(BaseShape targetShape, GeographyUnit unitOfData, DistanceUnit distanceUnit, double distance, ReturningColumnsType returningColumnNamesType)
        {
            Validators.CheckFeatureSourceIsOpen(_featureSource.IsOpen);
            Validators.CheckParameterIsNotNull(targetShape, "targetShape");
            Validators.CheckGeographyUnitIsValid(unitOfData, "unitOfData");
            Validators.CheckDistanceUnitIsValid(distanceUnit, "distanceUnit");
            Validators.CheckReturningColumnNamesTypeIsValid(returningColumnNamesType, "returningColumnNamesType");

            return _featureSource.GetFeaturesWithinDistanceOf(targetShape, unitOfData, distanceUnit, distance, returningColumnNamesType);
        }

        /// <summary>Returns a collection of features that are within a certain distance of a TargetShape.</summary>
        /// <returns>Collection of features that are within a certain distance of a TargetShape</returns>
        /// <remarks> It is important to note that the TargetShape and the
        ///     FeatureSource must use the same unit, such as feet or meters. If there is a current transaction and
        ///     it is marked as live, then the results will include any transaction Feature that
        ///     applies.</remarks>
        /// <param name="targetShape">Shape to find features within a distance of.</param>
        /// <param name="unitOfData">Unit of data that the FeatureSource and TargetShape are in.</param>
        /// <param name="distanceUnit">Unit of the distance parameter, such as feet, miles, kilometers, etc.</param>
        /// <param name="distance">Distance</param>
        /// <param name="returningColumnNames">Column name.</param>
        public Collection<Feature> GetFeaturesWithinDistanceOf(Feature targetFeature, GeographyUnit unitOfData, DistanceUnit distanceUnit, double distance, IEnumerable<string> returningColumnNames)
        {
            Validators.CheckFeatureSourceIsOpen(_featureSource.IsOpen);
            Validators.CheckGeographyUnitIsValid(unitOfData, "unitOfData");
            Validators.CheckDistanceUnitIsValid(distanceUnit, "distanceUnit");
            Validators.CheckParameterIsNotNull(returningColumnNames, "returningColumnNames");

            return _featureSource.GetFeaturesWithinDistanceOf(targetFeature, unitOfData, distanceUnit, distance, returningColumnNames);
        }

        /// <summary>Returns an feature based on an Id provided.</summary>
        /// <returns>Feature based on an Id provided..</returns>
        /// <param name="id">Unique Id for the feature to find.</param>
        /// <param name="returningColumnNames">Column names returned with the Feature.</param>
        public Feature GetFeatureById(string id, IEnumerable<string> returningColumnNames)
        {
            Validators.CheckFeatureSourceIsOpen(_featureSource.IsOpen);
            Validators.CheckParameterIsNotNullOrEmpty(id, "id");
            Validators.CheckParameterIsNotNull(returningColumnNames, "returningColumnNames");

            return _featureSource.GetFeatureById(id, returningColumnNames);
        }

        /// <summary>Returns an feature based on an Id provided.</summary>
        /// <returns>Feature based on an Id provided..</returns>
        /// <param name="id">Unique Id for the feature to find.</param>
        /// <param name="returningColumnNamesType">Column type..</param>
        public Feature GetFeatureById(string id, ReturningColumnsType returningColumnNamesType)
        {
            Validators.CheckFeatureSourceIsOpen(_featureSource.IsOpen);
            Validators.CheckParameterIsNotNullOrEmpty(id, "id");
            Validators.CheckReturningColumnNamesTypeIsValid(returningColumnNamesType, "returningColumnNamesType");

            return _featureSource.GetFeatureById(id, returningColumnNamesType);
        }

       
        /// <summary>Returns a collection of features inside of the target rectangle.</summary>
        /// <returns>Collection of features inside of the target rectangle.</returns>
        /// <remarks>If there is a current transaction and it is marked as live, then the results
        ///     will include any transaction Feature that applies.</remarks>
        /// <param name="boundingBox">Target boundingBox.</param>
        /// <param name="returningColumnNames">Column names.</param>
        public Collection<Feature> GetFeaturesInsideBoundingBox(RectangleShape boundingBox, IEnumerable<string> returningColumnNames)
        {
            Validators.CheckFeatureSourceIsOpen(_featureSource.IsOpen);
            Validators.CheckParameterIsNotNull(boundingBox, "boundingBox");
            Validators.CheckParameterIsNotNull(returningColumnNames, "returningColumnNames");
            Validators.CheckParameterIsValid(boundingBox, "boundingBox");

            return _featureSource.GetFeaturesInsideBoundingBox(boundingBox, returningColumnNames);
        }

        /// <summary>Returns a collection of features inside of the target rectangle.</summary>
        /// <returns>Collection of features inside of the target rectangle.</returns>
        /// <remarks>If there is a current transaction and it is marked as live, then the results
        ///     will include any transaction Feature that applies.</remarks>
        /// <param name="boundingBox">Target boundingBox.</param>
        /// <param name="returningColumnNamesType">Column type.</param>
        public Collection<Feature> GetFeaturesInsideBoundingBox(RectangleShape boundingBox, ReturningColumnsType returningColumnNamesType)
        {
            Validators.CheckFeatureSourceIsOpen(_featureSource.IsOpen);
            Validators.CheckParameterIsNotNull(boundingBox, "boundingBox");
            Validators.CheckReturningColumnNamesTypeIsValid(returningColumnNamesType, "returningColumnNamesType");

            return _featureSource.GetFeaturesInsideBoundingBox(boundingBox, returningColumnNamesType);
        }

        /// <summary>Returns a collection of features outside of the target rectangle.</summary>
        /// <returns>Collection of features outside of the target rectangle.</returns>
        /// <remarks>If there is a current transaction and it is marked as live, then the results
        ///     will include any transaction Feature that applies.</remarks>
        /// <param name="boundingBox">Target boundingBox.</param>
        /// <param name="returningColumnNames">Column names.</param>
        public Collection<Feature> GetFeaturesOutsideBoundingBox(RectangleShape boundingBox, IEnumerable<string> returningColumnNames)
        {
            Validators.CheckFeatureSourceIsOpen(_featureSource.IsOpen);
            Validators.CheckParameterIsNotNull(boundingBox, "boundingBox");
            Validators.CheckParameterIsNotNull(returningColumnNames, "returningColumnNames");
            Validators.CheckParameterIsValid(boundingBox, "boundingBox");

            return _featureSource.GetFeaturesOutsideBoundingBox(boundingBox, returningColumnNames);
        }

        /// <summary>Returns a collection of features outside of the target rectangle.</summary>
        /// <returns>Collection of features outside of the target rectangle.</returns>
        /// <remarks>If there is a current transaction and it is marked as live, then the results
        ///     will include any transaction Feature that applies.</remarks>
        /// <param name="boundingBox">Target boundingBox.</param>
        /// <param name="returningColumnNamesType">Column type.</param>
        public Collection<Feature> GetFeaturesOutsideBoundingBox(RectangleShape boundingBox, ReturningColumnsType returningColumnNamesType)
        {
            Validators.CheckFeatureSourceIsOpen(_featureSource.IsOpen);
            Validators.CheckParameterIsNotNull(boundingBox, "boundingBox");
            Validators.CheckReturningColumnNamesTypeIsValid(returningColumnNamesType, "returningColumnNamesType");

            return _featureSource.GetFeaturesOutsideBoundingBox(boundingBox, returningColumnNamesType);
        }

        /// <summary>Returns the collection of columns for this FeatureSource.</summary>
        /// <returns>Collection of columns for this FeatureSource.</returns>
        public Collection<FeatureSourceColumn> GetColumns()
        {
            Validators.CheckFeatureSourceIsOpen(_featureSource.IsOpen);
            return _featureSource.GetColumns();
        }

        /// <summary>Returns the count of the features in  FeatureSource.</summary>
        /// <returns>Count of the features in  FeatureSource.</returns>
        public int GetCount()
        {
            Validators.CheckFeatureSourceIsOpen(_featureSource.IsOpen);
            return _featureSource.GetCount();
        }

        /// <remarks>Returns whatever is returned by the GetAllFeaturesCore method, along with any of the additions or
        /// subtractions made if you are in a transaction and that transaction is configured to be live.</remarks>
        /// <summary>Returns the collection of all the features in the FeatureSource.</summary>
        /// <returns>Collection of all the features in the FeatureSource.</returns>
        /// <param name="returningColumnNames">Column type</param>
        public Collection<Feature> GetAllFeatures(ReturningColumnsType returningColumnNamesType)
        {
            Validators.CheckFeatureSourceIsOpen(_featureSource.IsOpen);
            Validators.CheckReturningColumnNamesTypeIsValid(returningColumnNamesType, "returningColumnNamesType");

            return _featureSource.GetAllFeatures(returningColumnNamesType);
        }

        /// <remarks>Returns whatever is returned by the GetAllFeaturesCore method, along with any of the additions or
        /// subtractions made if you are in a transaction and that transaction is configured to be live.</remarks>
        /// <summary>Returns the collection of all the features in the FeatureSource.</summary>
        /// <returns>Collection of all the features in the FeatureSource.</returns>
        /// <param name="returningColumnNames">Column names</param>
        public Collection<Feature> GetAllFeatures(IEnumerable<string> returningColumnNames)
        {
            Validators.CheckFeatureSourceIsOpen(_featureSource.IsOpen);
            Validators.CheckParameterIsNotNull(returningColumnNames, "returningColumnNames");

            return _featureSource.GetAllFeatures(returningColumnNames);
        }

        /// <summary>Executes query and returns the result.</summary>
        /// <returns>Result set in the format of dataTable.</returns>
        /// <remarks>
        /// Use the ExcuteScalar method to retrieve a single value from the database. This
        /// requires less code than use the ExcuteQuery method and then performing the operations
        /// necessary to generate the single value using the data.
        /// </remarks>
        /// <param name="sqlStatement">The SQL statement to execute.</param>
        public DataTable ExecuteQuery(string sqlStatement)
        {
            Validators.CheckFeatureSourceIsOpen(_featureSource.IsOpen);
            Validators.CheckParameterIsNotNullOrEmpty(sqlStatement, "sqlStatement");
            Validators.CheckFeatureSourceCanExecuteSqlQuery(_featureSource.CanExecuteSqlQuery);

            return _featureSource.ExecuteQuery(sqlStatement);
        }

        /// <summary>Returns the bounding box for a feature with Id specified.</summary>
        /// <returns>Bounding box for a feature with Id specified.</returns>
        /// <param name="id">Unique Id of the feature to find the bounding box.</param>
        public RectangleShape GetBoundingBoxById(string id)
        {
            Validators.CheckFeatureSourceIsOpen(_featureSource.IsOpen);
            Validators.CheckParameterIsNotNullOrEmpty(id, "id");

            return _featureSource.GetBoundingBoxById(id);
        }

        /// <summary>Returns a collection of bounding boxes for the features with Id specified.</summary>
        /// <returns>Collection of bounding boxes for the features with Id specified.</returns>
        /// <param name="ids">Collection of unique Ids of the features to find the bounding box.</param>
        public Collection<RectangleShape> GetBoundingBoxesByIds(IEnumerable<string> ids)
        {
            Validators.CheckFeatureSourceIsOpen(_featureSource.IsOpen);
            Validators.CheckParameterIsNotNull(ids, "ids");

            return _featureSource.GetBoundingBoxesByIds(ids);
        }
    }
}

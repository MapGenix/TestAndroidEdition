using System;
using System.Collections.Generic;
using Mapgenix.Shapes;
using Mapgenix.Utils;

namespace Mapgenix.FeatureSource
{
    /// <summary>Wrapper class for FeatureLayer for the editing methods.</summary>
    [Serializable]
    public class EditTools
    {
        private BaseFeatureSource _featureSource;

        /// <summary>Constructor for the class.</summary>
        /// <overloads>Passes in the FeatureSource.</overloads>
        /// <returns>None</returns>
        /// <remarks>None</remarks>
        /// <param name="featureSource">FeatureSource for the edit operations.</param>
        public EditTools(BaseFeatureSource featureSource)
        {
            Validators.CheckParameterIsNotNull(featureSource, "featureSource");

            _featureSource = featureSource;
        }

        /// <summary>Starts a new transaction for the FeatureSource.</summary>
        /// <returns>None</returns>
        /// <remarks>Used to start a transaction. There are prerequisites before beginning a transaction, such as transaction not already in progress, 
        /// FeatureSource is open.<br/><br/>
        /// The Transaction System<br/>
        /// 	<br/>
        /// The transaction system is the same independently of the speficic FeatureSource (shapefile, in memory layer etc).
        /// First call BeginTransaction (Allocates a collection of in-memory change buffers to store changes before committing transactions).
        /// Call RollbackTransaction to revert before BeginTransaction.
        /// To commit, call CommitTransaction and the collection of changes are passed to CommitTransactionCore with 
        /// the logic of committting specific to a source.
        /// By default IsLiveTransaction property is set to false, meaning that until the changes are committed, any changes
        /// that are in the temporary editing buffer will not be reflected.<br/>
        /// 	<br/>
        /// In the case where the IsLiveTransaction is set to true, the live transaction concept means that all of the modifications
        /// performed during a transaction are live.<br/>
        /// 	<br/>
        /// As an example, a FeatureLayer has 20 records.
        /// Next, a transaction begins and then GetAllFeatures is called. The result is 20
        /// records. Delete one of the records and call the GetAllFeatures
        /// again.  Only 19 records are retrieved, even though the transaction has not yet been committed.<br/>
        /// <br/></remarks>
        public void BeginTransaction()
        {
            _featureSource.BeginTransaction();
        }

        ///<summary>Adds a new Feature to an existing transaction.</summary>
        ///<returns>ID uniquely identifying the Feature while in transaction.</returns>
        /// <overloads>Passes in a Feature.</overloads>
        /// <param name="feature">Feature to add to the transaction.</param>
        /// <remarks>
        /// The Transaction System<br/>
        /// 	<br/>
        /// The transaction system is the same independently of the speficic FeatureSource (shapefile, in memory layer etc).
        /// First call BeginTransaction (Allocates a collection of in-memory change buffers to store changes before committing transactions).
        /// Call RollbackTransaction to revert before BeginTransaction.
        /// To commit, call CommitTransaction and the collection of changes are passed to CommitTransactionCore with 
        /// the logic of committting specific to a source.
        /// By default IsLiveTransaction property is set to false, meaning that until the changes are committed, any changes
        /// that are in the temporary editing buffer will not be reflected.<br/>
        /// 	<br/>
        /// In the case where the IsLiveTransaction is set to true, the live transaction concept means that all of the modifications
        /// performed during a transaction are live.<br/>
        /// 	<br/>
        /// As an example, a FeatureLayer has 20 records.
        /// Next, a transaction begins and then GetAllFeatures is called. The result is 20
        /// records. Delete one of the records and call the GetAllFeatures
        /// again.  Only 19 records are retrieved, even though the transaction has not yet been committed.<br/>
        /// <br/></remarks>
        public string Add(Feature feature)
        {
            Validators.CheckParameterIsNotNull(feature, "feature");

            return _featureSource.AddFeature(feature);
        }

        ///<summary>Adds a new Feature with a shape to an existing transaction.</summary>
        ///<returns>ID uniquely identifying the Feature while in transaction.</returns>
        /// <overloads>Passes in a shape.</overloads>
        /// <param name="shape">Shape to add to the transaction.</param>
        /// <remarks>
        /// The Transaction System<br/>
        /// 	<br/>
        /// The transaction system is the same independently of the speficic FeatureSource (shapefile, in memory layer etc).
        /// First call BeginTransaction (Allocates a collection of in-memory change buffers to store changes before committing transactions).
        /// Call RollbackTransaction to revert before BeginTransaction.
        /// To commit, call CommitTransaction and the collection of changes are passed to CommitTransactionCore with 
        /// the logic of committting specific to a source.
        /// By default IsLiveTransaction property is set to false, meaning that until the changes are committed, any changes
        /// that are in the temporary editing buffer will not be reflected.<br/>
        /// 	<br/>
        /// In the case where the IsLiveTransaction is set to true, the live transaction concept means that all of the modifications
        /// performed during a transaction are live.<br/>
        /// 	<br/>
        /// As an example, a FeatureLayer has 20 records.
        /// Next, a transaction begins and then GetAllFeatures is called. The result is 20
        /// records. Delete one of the records and call the GetAllFeatures
        /// again.  Only 19 records are retrieved, even though the transaction has not yet been committed.<br/>
        /// <br/></remarks>
        public string Add(BaseShape shape)
        {
            Validators.CheckParameterIsNotNull(shape, "shape");

            return Add(new Feature(shape));
        }

        /// <summary>Adds a Feature with shape and column values.</summary>
        /// <remarks>
        /// The Transaction System<br/>
        /// 	<br/>
        /// The transaction system is the same independently of the speficic FeatureSource (shapefile, in memory layer etc).
        /// First call BeginTransaction (Allocates a collection of in-memory change buffers to store changes before committing transactions).
        /// Call RollbackTransaction to revert before BeginTransaction.
        /// To commit, call CommitTransaction and the collection of changes are passed to CommitTransactionCore with 
        /// the logic of committting specific to a source.
        /// By default IsLiveTransaction property is set to false, meaning that until the changes are committed, any changes
        /// that are in the temporary editing buffer will not be reflected.<br/>
        /// 	<br/>
        /// In the case where the IsLiveTransaction is set to true, the live transaction concept means that all of the modifications
        /// performed during a transaction are live.<br/>
        /// 	<br/>
        /// As an example, a FeatureLayer has 20 records.
        /// Next, a transaction begins and then GetAllFeatures is called. The result is 20
        /// records. Delete one of the records and call the GetAllFeatures
        /// again.  Only 19 records are retrieved, even though the transaction has not yet been committed.<br/>
        /// <br/></remarks>
        /// <param name="shape">Shape to be added to the transaction.</param>
        /// <param name="columnValues">DBF information of the shape.</param>
        /// <returns>ID uniquely identifying the shape while it is in a transaction.</returns>
        public string Add(BaseShape shape, Dictionary<string, string> columnValues)
        {
            Validators.CheckParameterIsNotNull(shape, "shape");
            Validators.CheckParameterIsNotNull(columnValues, "columnValues");

            return _featureSource.AddFeature(new Feature(shape, columnValues));
        }

        /// <summary>Increases the size of the feature by a percentage.</summary>
        /// <remarks>To easily edit features directly in the FeatureSource without having to retrieve them, convert them to a shape,
        /// manipulate them and put them back into the FeatureSource.<br/>
        /// </remarks>
        /// <param name="featureId">Id of the Feature to scale.</param>
        /// <param name="percentage">Percentage by which to increase the Feature size.</param>
        public void ScaleUp(string featureId, double percentage)
        {
            Validators.CheckParameterIsNotNullOrEmpty(featureId, "featureId");
            Validators.CheckIfInputValueIsBiggerThan(percentage, "percentage", 0, RangeCheckingInclusion.IncludeValue);
            Validators.CheckFeatureSourceIsInTransaction(_featureSource.IsInTransaction);

            Feature feature = _featureSource.GetFeatureById(featureId, new string[0]);
            BaseShape baseShape = feature.GetShape();
            BaseAreaShape areaBaseShape = baseShape as BaseAreaShape;

            if (areaBaseShape != null)
            {
                areaBaseShape.ScaleUp(percentage);
                areaBaseShape.Id = featureId;
                Update(areaBaseShape);
            }
        }

        /// <summary>Decreases the size of the feature by a percentage.</summary>
        /// <remarks>To easily edit features directly in the FeatureSource without having to retrieve them, convert them to a shape,
        /// manipulate them and put them back into the FeatureSource.<br/>
        /// </remarks>
        /// <param name="featureId">Id of the Feature to scale.</param>
        /// <param name="percentage">Percentage to decrease the Feature size.</param>
        public void ScaleDown(string featureId, double percentage)
        {
            Validators.CheckParameterIsNotNullOrEmpty(featureId, "featureId");
            Validators.CheckIfInputValueIsBiggerThan(percentage, "percentage", 0, RangeCheckingInclusion.IncludeValue);

            Feature feature = _featureSource.GetFeatureById(featureId, new string[0]);
            BaseShape baseShape = feature.GetShape();
            BaseAreaShape areaBaseShape = baseShape as BaseAreaShape;

            if (areaBaseShape != null)
            {
                areaBaseShape.ScaleDown(percentage);
                areaBaseShape.Id = featureId;
                Update(areaBaseShape);
            }
        }

        /// <summary>Moves the Feature from one location to another based on a X and Y offset distance.</summary>
        /// <remarks>
        /// <para>To easily edit features directly in the FeatureSource without having to retrieve them, convert them to a
        ///     shape, manipulate them and put them back into the FeatureSource.<br/>
        /// <br/></remarks>
        /// <returns>None</returns>
        /// <param name="featureId">Id of the Feature to move.</param>
        /// <param name="xOffset">X offset </param>
        /// <param name="yOffset">Y offset </param>
        /// <param name="shapeUnit">
        /// 	<para>GeographicUnit of the shape.</para>
        /// </param>
        /// <param name="offsetUnit">
        /// 	<para>DistanceUnit  for the offset.</para>
        /// </param>
        public void TranslateByOffset(string featureId, double xOffset, double yOffset, GeographyUnit shapeUnit, DistanceUnit offsetUnit)
        {
            Validators.CheckParameterIsNotNullOrEmpty(featureId, "featureId");
            Validators.CheckGeographyUnitIsValid(shapeUnit, "shapeUnit");
            Validators.CheckDistanceUnitIsValid(offsetUnit, "offsetUnit");

            Feature feature = _featureSource.GetFeatureById(featureId, new string[0]);
            BaseShape baseShape = feature.GetShape();
            baseShape.TranslateByOffset(xOffset, yOffset, shapeUnit, offsetUnit);
            Update(baseShape);
        }

        /// <summary>Moves the Feature from one location to another based on a distance and a direction in degrees.</summary>
        /// <returns>None</returns>
        /// <param name="featureId">Id of the Feature to move.</param>
        /// <param name="distance">Distance to move the feature by an angle.
        /// The distance is in the unit of DistanceUnit.</param>
        /// <param name="angleInDegrees">
        /// 	<para>Number between 0 and 360 degrees representing the direction to move the shape, with zero being up.</para>
        /// </param>
        /// <param name="shapeUnit">GeographicUnit of the shape to perform the move on.</param>
        /// <param name="distanceUnit">Distance unit for the distance.</param>
        public void TranslateByDegree(string featureId, double distance, double angleInDegrees, GeographyUnit shapeUnit, DistanceUnit distanceUnit)
        {
            Validators.CheckParameterIsNotNullOrEmpty(featureId, "featureId");
            Validators.CheckIfInputValueIsInRange(distance, "distance", 0, RangeCheckingInclusion.IncludeValue, double.MaxValue, RangeCheckingInclusion.IncludeValue);
            Validators.CheckIfInputValueIsInRange(angleInDegrees, "angleInDegrees", 0, RangeCheckingInclusion.IncludeValue, 360, RangeCheckingInclusion.IncludeValue);
            Validators.CheckGeographyUnitIsValid(shapeUnit, "shapeUnit");
            Validators.CheckDistanceUnitIsValid(distanceUnit, "distanceUnit");

            Feature feature = _featureSource.GetFeatureById(featureId, new string[0]);
            BaseShape baseShape = feature.GetShape();
            baseShape.TranslateByDegree(distance, angleInDegrees, shapeUnit, distanceUnit);
            Update(baseShape);
        }

        
        /// <summary>Returns the union of the Feature and the target shapes.</summary>
        /// <returns>None</returns>
        /// <remarks>To easily edit features directly in the FeatureSource without having to retrieve them, convert them to a shape,
        /// manipulate them and put them back into the FeatureSource.<br/>
        /// <br/></remarks>
        /// <param name="featureId">Id of the Feature to union.</param>
        /// <param name="targetShape">Shape to union with.</param>
        public void Union(string featureId, BaseAreaShape targetShape)
        {
            Validators.CheckParameterIsNotNullOrEmpty(featureId, "featureId");
            Validators.CheckParameterIsNotNull(targetShape, "targetShape");
            Validators.CheckParameterIsValid(targetShape, "targetShape");

            Feature feature = _featureSource.GetFeatureById(featureId, new string[0]);
            BaseAreaShape sourceShape = feature.GetShape() as BaseAreaShape;

            if (sourceShape != null)
            {
                MultipolygonShape unionResultShape = sourceShape.Union(targetShape);
                unionResultShape.Id = featureId;
                Update(unionResultShape);
            }
        }

        /// <summary>Returns the union of the Feature and the target feature.</summary>
        /// <returns>None</returns>
        /// <remarks>To easily edit features directly in the FeatureSource without having to retrieve them, convert them to a shape,
        /// manipulate them and put them back into the FeatureSource.<br/>
        /// <br/></remarks>
        /// <param name="featureId">Id of the Feature to union.</param>
        /// <param name="targetAreaFeature">Feature to union with.</param>
        public void Union(string featureId, Feature targetAreaFeature)
        {
            Validators.CheckParameterIsNotNullOrEmpty(featureId, "featureId");

            BaseShape targetShape = targetAreaFeature.GetShape();
            Validators.CheckParameterIsValid(targetShape, "targetAreaFeature");
            Validators.CheckShapeIsAreaBaseShape(targetShape);

            Union(featureId, (BaseAreaShape)targetShape);
        }

        /// <summary>Returns the difference between two shapes.</summary>
        /// <remarks>To easily edit features directly in the features without having to retrieve them, convert them to a shape,
        /// manipulate them and put them back into the FeatureSource.</remarks>
        /// <returns>None</returns>
        /// <param name="featureId">Id of Feature to get difference from.</param>
        /// <param name="targetShape">Shape to find the difference with the feature.</param>
        public void GetDifference(string featureId, BaseAreaShape targetShape)
        {
            Validators.CheckParameterIsNotNullOrEmpty(featureId, "featureId");
            Validators.CheckParameterIsNotNull(targetShape, "targetShape");
            Validators.CheckParameterIsValid(targetShape, "targetShape");

            Feature feature = _featureSource.GetFeatureById(featureId, new string[0]);
            BaseAreaShape sourceShape = feature.GetShape() as BaseAreaShape;
            if (sourceShape != null)
            {
                MultipolygonShape unionResultShape = sourceShape.GetDifference(targetShape);
                unionResultShape.Id = featureId;
                Update(unionResultShape);
            }
        }

        /// <summary>Returns the difference between two features.</summary>
        /// <returns>None</returns>
        /// <param name="featureId">Feature to remove area from.</param>
        /// <param name="targetAreaFeature">Feature to find the difference with.</param>
        public void GetDifference(string featureId, Feature targetAreaFeature)
        {
            Validators.CheckParameterIsNotNullOrEmpty(featureId, "featureId");

            BaseShape targetBaseShape = targetAreaFeature.GetShape();
            Validators.CheckShapeIsAreaBaseShape(targetBaseShape);
            Validators.CheckParameterIsValid(targetBaseShape, "targetAreaFeature");

            GetDifference(featureId, (BaseAreaShape)targetBaseShape);
        }

        /// <summary>Returns whether the FeatureLayer allows edits or is read only.</summary>
        /// <returns>Whether the FeatureLayer allows edits or is read only.</returns>
        /// <remarks>
        /// 	<para>Useful to check if a specific FeatureLayer accepts editing.
        /// 	Raises exception if BeginTransaction called with IsEditable set to false.</para>
        /// </remarks>
        public bool IsEditable
        {
            get { return _featureSource.IsEditable; }
        }

        /// <summary>Returns true if the FeatureLayer is in a transaction and false if it is not.</summary>
        /// <returns>True if the FeatureLayer is in a transaction and false if it is not.</returns>
        /// <remarks>To enter a transaction, call the BeginTransaction method. It is possible that some FeatureLayers are read only and do not allow
        /// edits. To end a transaction, call either CommitTransaction or RollbackTransaction.</remarks>
        public bool IsInTransaction
        {
            get { return _featureSource.IsInTransaction; }
        }

        /// <summary>Returns true if the features currently modified in a transaction
        /// are expected to reflect their state when calling other methods on the FeatureLayer, such as spatial queries.</summary>
        /// <returns>True if the features currently modified in a transaction
        /// are expected to reflect their state when calling other methods on the
        /// FeatureLayer.</returns>
        /// <remarks>A live transaction means that all of the modifications performed during
        /// a transaction are live from the standpoint of the querying methods on the object.</remarks>
        public bool IsTransactionLive
        {
            get { return _featureSource.IsTransactionLive; }
            set { _featureSource.IsTransactionLive = value; }
        }

        /// <summary>Gets and sets the transaction buffer.</summary>
        /// <value>None</value>
        public TransactionBuffer TransactionBuffer
        {
            get
            {
                return _featureSource.TransactionBuffer;
            }
            set
            {
                _featureSource.TransactionBuffer = value;
            }
        }

        /// <summary>Deletes a Feature from an existing transaction.</summary>
        /// <returns>None</returns>
        /// <param name="id">Id of the feature in the FeatureLayer to delete.</param>
        /// <remarks>Deletes a Feature from an existing transaction. Call BeginTransaction before.<br/>
        /// The Transaction System<br/>
        /// 	<br/>
        /// The transaction system is the same independently of the speficic FeatureSource (shapefile, in memory layer etc).
        /// First call BeginTransaction (Allocates a collection of in-memory change buffers to store changes before committing transactions).
        /// Call RollbackTransaction to revert before BeginTransaction.
        /// To commit, call CommitTransaction and the collection of changes are passed to CommitTransactionCore with 
        /// the logic of committting specific to a source.
        /// By default IsLiveTransaction property is set to false, meaning that until the changes are committed, any changes
        /// that are in the temporary editing buffer will not be reflected.<br/>
        /// 	<br/>
        /// In the case where the IsLiveTransaction is set to true, the live transaction concept means that all of the modifications
        /// performed during a transaction are live.<br/>
        /// 	<br/>
        /// As an example, a FeatureLayer has 20 records.
        /// Next, a transaction begins and then GetAllFeatures is called. The result is 20
        /// records. Delete one of the records and call the GetAllFeatures
        /// again.  Only 19 records are retrieved, even though the transaction has not yet been committed.<br/>
        /// <br/></remarks>
        public void Delete(string id)
        {
            Validators.CheckParameterIsNotNullOrEmpty(id, "id");

            _featureSource.DeleteFeature(id);
        }

        /// <summary>Updates a Feature in an existing transaction.</summary>
        /// <param name="shape">The shape to update in the transaction. Id of the Shape is the Id of the feature to update.</param>
        /// <remarks>Updates a Feature from an existing transaction. Call BeginTransaction before.<br/>
        /// The Transaction System<br/>
        /// 	<br/>
        /// The transaction system is the same independently of the speficic FeatureSource (shapefile, in memory layer etc).
        /// First call BeginTransaction (Allocates a collection of in-memory change buffers to store changes before committing transactions).
        /// Call RollbackTransaction to revert before BeginTransaction.
        /// To commit, call CommitTransaction and the collection of changes are passed to CommitTransactionCore with 
        /// the logic of committting specific to a source.
        /// By default IsLiveTransaction property is set to false, meaning that until the changes are committed, any changes
        /// that are in the temporary editing buffer will not be reflected.<br/>
        /// 	<br/>
        /// In the case where the IsLiveTransaction is set to true, the live transaction concept means that all of the modifications
        /// performed during a transaction are live.<br/>
        /// 	<br/>
        /// As an example, a FeatureLayer has 20 records.
        /// Next, a transaction begins and then GetAllFeatures is called. The result is 20
        /// records. Delete one of the records and call the GetAllFeatures
        /// again.  Only 19 records are retrieved, even though the transaction has not yet been committed.<br/>
        /// <br/></remarks>
        public void Update(BaseShape shape)
        {
            Validators.CheckParameterIsNotNull(shape, "shape");

            Update(new Feature(shape));
        }

        /// <summary>Updates a Feature in an existing transaction.</summary>
        /// <overloads>Passes in a Feature.</overloads>
        /// <param name="feature">The Feature to update in the transaction.</param>
        /// <remarks>Updates a Feature from an existing transaction. Call BeginTransaction before.<br/>
        /// The Transaction System<br/>
        /// 	<br/>
        /// The transaction system is the same independently of the speficic FeatureSource (shapefile, in memory layer etc).
        /// First call BeginTransaction (Allocates a collection of in-memory change buffers to store changes before committing transactions).
        /// Call RollbackTransaction to revert before BeginTransaction.
        /// To commit, call CommitTransaction and the collection of changes are passed to CommitTransactionCore with 
        /// the logic of committting specific to a source.
        /// By default IsLiveTransaction property is set to false, meaning that until the changes are committed, any changes
        /// that are in the temporary editing buffer will not be reflected.<br/>
        /// 	<br/>
        /// In the case where the IsLiveTransaction is set to true, the live transaction concept means that all of the modifications
        /// performed during a transaction are live.<br/>
        /// 	<br/>
        /// As an example, a FeatureLayer has 20 records.
        /// Next, a transaction begins and then GetAllFeatures is called. The result is 20
        /// records. Delete one of the records and call the GetAllFeatures
        /// again.  Only 19 records are retrieved, even though the transaction has not yet been committed.<br/>
        /// <br/></remarks>
        public void Update(Feature feature)
        {
            Validators.CheckParameterIsNotNull(feature, "feature");

            _featureSource.UpdateFeature(feature);
        }

        /// <summary>Updates a Feature in an existing transaction.</summary>
        /// <overloads>Passes in a shape and the columnValues to the Feature to update.</overloads>
        /// <param name="shape">Shape to update in the transaction. The Id of the Shape needs to be the same as the Id of the feature to update.</param>
        /// <param name="columnValues">Column values to update in the transaction.</param>
        /// <remarks>Updates a Feature from an existing transaction. Call BeginTransaction before.<br/>
        /// The Transaction System<br/>
        /// 	<br/>
        /// The transaction system is the same independently of the speficic FeatureSource (shapefile, in memory layer etc).
        /// First call BeginTransaction (Allocates a collection of in-memory change buffers to store changes before committing transactions).
        /// Call RollbackTransaction to revert before BeginTransaction.
        /// To commit, call CommitTransaction and the collection of changes are passed to CommitTransactionCore with 
        /// the logic of committting specific to a source.
        /// By default IsLiveTransaction property is set to false, meaning that until the changes are committed, any changes
        /// that are in the temporary editing buffer will not be reflected.<br/>
        /// 	<br/>
        /// In the case where the IsLiveTransaction is set to true, the live transaction concept means that all of the modifications
        /// performed during a transaction are live.<br/>
        /// 	<br/>
        /// As an example, a FeatureLayer has 20 records.
        /// Next, a transaction begins and then GetAllFeatures is called. The result is 20
        /// records. Delete one of the records and call the GetAllFeatures
        /// again.  Only 19 records are retrieved, even though the transaction has not yet been committed.<br/>
        /// <br/></remarks>
        public void Update(BaseShape shape, Dictionary<string, string> columnValues)
        {
            Validators.CheckParameterIsNotNull(shape, "shape");
            Validators.CheckParameterIsNotNull(columnValues, "columnValues");

            _featureSource.UpdateFeature(new Feature(shape, columnValues));
        }

       
        /// <summary>Commits the existing transaction to its underlying source of data.</summary>
        /// <returns>TransactionResult class giving the status of the commited transactions.</returns>
        /// <remarks>
        /// 	<para>Commits the existing transaction to its underlying source of data. Passes back the results of the commit, 
        /// 	including any error(s) received. Finally, frees up the internal memory cache of any features added, updated
        ///     or deleted. Call BeginTransaction first.
        ///     </para>The Transaction System<br/>
        /// 	<br/>
        /// The transaction system is the same independently of the speficic FeatureSource (shapefile, in memory layer etc).
        /// First call BeginTransaction (Allocates a collection of in-memory change buffers to store changes before committing transactions).
        /// Call RollbackTransaction to revert before BeginTransaction.
        /// To commit, call CommitTransaction and the collection of changes are passed to CommitTransactionCore with 
        /// the logic of committting specific to a source.
        /// By default IsLiveTransaction property is set to false, meaning that until the changes are committed, any changes
        /// that are in the temporary editing buffer will not be reflected.<br/>
        /// 	<br/>
        /// In the case where the IsLiveTransaction is set to true, the live transaction concept means that all of the modifications
        /// performed during a transaction are live.<br/>
        /// 	<br/>
        /// As an example, a FeatureLayer has 20 records.
        /// Next, a transaction begins and then GetAllFeatures is called. The result is 20
        /// records. Delete one of the records and call the GetAllFeatures
        /// again.  Only 19 records are retrieved, even though the transaction has not yet been committed.<br/>
        /// <br/>
        /// </remarks>
        public TransactionResult CommitTransaction()
        {
            return _featureSource.CommitTransaction();
        }

        /// <summary>Cancels an existing transaction. Frees up the internal
        /// memory cache of any feature added, updated or deleted.</summary>
        /// <returns>None</returns>
        /// <remarks>
        /// 	<para>Cancels an existing transaction. Frees up the internal
        ///     memory cache of any feature added, updated or deleted. Call BeginTransaction first.</para>
        ///</para>The Transaction System<br/>
        /// The transaction system is the same independently of the speficic FeatureSource (shapefile, in memory layer etc).
        /// First call BeginTransaction (Allocates a collection of in-memory change buffers to store changes before committing transactions).
        /// Call RollbackTransaction to revert before BeginTransaction.
        /// To commit, call CommitTransaction and the collection of changes are passed to CommitTransactionCore with 
        /// the logic of committting specific to a source.
        /// By default IsLiveTransaction property is set to false, meaning that until the changes are committed, any changes
        /// that are in the temporary editing buffer will not be reflected.<br/>
        /// 	<br/>
        /// In the case where the IsLiveTransaction is set to true, the live transaction concept means that all of the modifications
        /// performed during a transaction are live.<br/>
        /// 	<br/>
        /// As an example, a FeatureLayer has 20 records.
        /// Next, a transaction begins and then GetAllFeatures is called. The result is 20
        /// records. Delete one of the records and call the GetAllFeatures
        /// again.  Only 19 records are retrieved, even though the transaction has not yet been committed.<br/>
        /// <br/>
        /// </remarks>
        public void RollbackTransaction()
        {
            _featureSource.RollbackTransaction();
        }
    }
}

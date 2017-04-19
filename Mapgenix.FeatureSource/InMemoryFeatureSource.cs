using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using GeoAPI.Geometries;

//using NetTopologySuite.Index.Strtree;
using Mapgenix.Shapes;
using Mapgenix.FeatureSource.Properties;

namespace Mapgenix.FeatureSource
{
    /// <summary>FeatureSource in memory for dynamic features.</summary>
    /// <remarks>Usefull for example for real time GPS locations or for location clicked on the map by a user.</remarks>
    [Serializable]
    public class InMemoryFeatureSource : BaseFeatureSource
    {
        private Collection<FeatureSourceColumn> _columns;
       
        private int _maxRecordsToDraw;

        /*[NonSerialized]
        private STRtree<Feature> _rTree;*/

        /// <summary>Creates an instance of InMemoryFeatureSource class by passing FeatureSourceColumns.</summary>
        /// <param name="featureSourceColumns">FeaturesSourceColumns.</param>
        public InMemoryFeatureSource(IEnumerable<FeatureSourceColumn> featureSourceColumns, IEnumerable<Feature> features)
        {
            Validators.CheckParameterIsNotNull(featureSourceColumns, "featureSourceColumns");
            Validators.CheckParameterIsNotNull(features, "features");

            InMemoryFeatureSourceConstructor(featureSourceColumns, features);
        }

        /// <summary>Creates an instance of InMemoryFeatureSource class by passing FeatureSourceColumns and Features.</summary>
        /// <param name="featureSourceColumns">FeaturesSourceColumns to instance for InMemoryFeatureSource.</param>
        /// <param name="features">Features to instance for the InMemoryFeatureSource.</param>
        public InMemoryFeatureSource(IEnumerable<FeatureSourceColumn> featureSourceColumns, IEnumerable<BaseShape> shapes)
        {
            Validators.CheckParameterIsNotNull(shapes, "shapes");
            Validators.CheckParameterIsNotNull(featureSourceColumns, "featureSourceColumns");

            Collection<Feature> features = new Collection<Feature>();
            foreach (BaseShape shape in shapes)
            {
                features.Add(new Feature(shape));
            }

            InMemoryFeatureSourceConstructor(featureSourceColumns, features);
        }

        /// <summary>Gets or set the maximum number of features to draw.</summary>
        public int MaxRecordsToDraw
        {
            get { return _maxRecordsToDraw; }
            set { _maxRecordsToDraw = value; }
        }

        /// <summary>Returns the underlying Features stored in memory.</summary>
        /// <returns>Underlying Features stored in memory.</returns>
        /// <remarks>Allows to modify and inspect the InternalFeatures held by this
        /// FeatureSource in memory.</remarks>
        public ConcurrentDictionary<string,Feature> InternalFeatures { get; set; }

        /// <summary>Returns true if the FeatureSource allows edits or false if is read only.</summary>
        /// <returns>True if the FeatureSource allows edits or false if is read only.</returns>
        /// <remarks>
        /// 	<para>Useful to check if a specific FeatureSource accepts editing.
        ///     Raises exception if false and call BeginTransaction.<br/>
        /// 		<br/>
        ///     To create or extent a FeatureSource it is expected to override this virtual method to allows editing.</para>
        /// </remarks>
        public override bool IsEditable
        {
            get { return true; }
        }


        private void InMemoryFeatureSourceConstructor(IEnumerable<FeatureSourceColumn> featureSourceColumns, IEnumerable<Feature> features)
        {
            _columns = new Collection<FeatureSourceColumn>();
            foreach (FeatureSourceColumn featureSourceColumn in featureSourceColumns)
            {
                _columns.Add(featureSourceColumn);
            }

            InternalFeatures = new ConcurrentDictionary<string, Feature>();
            foreach (Feature feature in features)
            {
                InternalFeatures.TryAdd(feature.Id, feature);
            }
        }

        /// <summary>Builds spatial index to increase access speed.</summary>
        public void BuildIndex()
        {
            /*_rTree = new STRtree<Feature>();

            ICollection<string> keys = InternalFeatures.Keys;
            foreach (string key in keys)
            {
                RectangleShape boundingBox = InternalFeatures[key].GetBoundingBox();
                Envelope envelope = new Envelope(boundingBox.UpperLeftPoint.X, boundingBox.LowerRightPoint.X, boundingBox.UpperLeftPoint.Y, boundingBox.LowerRightPoint.Y);

                _rTree.Insert(envelope, InternalFeatures[key]);
            }

            _rTree.Build();*/
        }

        /// <summary>Returns the number of records in the FeatureSource.</summary>
        /// <returns>Number of records in the FeatureSource.</returns>
        /// <remarks>
        /// 	<para>This protected virtual method called from the concrete public method
        ///     GetCount. It does not take into account any transaction activity being the
        ///     responsibility of the concrete public method GetCount.<br/>
        /// 		<br/>
        ///     The default implementation of GetCountCore uses the GetAllRecordsCore. It is recommended
        ///     to provide one's own implementation to be more efficient<br/>
        /// 		<br/>
        ///     For example, in a ShapeFile the record count is in the main header of the file. 
        ///     For a spatial database , execute a simple query to get the count.</para>
        /// </remarks>
        protected override int GetCountCore()
        {
            Validators.CheckFeatureSourceIsOpen(IsOpen);
            return InternalFeatures.Count;
        }

        /// <summary>Commits the transaction to its underlying source of data.</summary>
        /// <returns>TransactionResult with status of the commited transaction including the succesfull updates,
        /// adds, and deletes and any error encountered during the committing of the transaction.</returns>
        /// <param name="transactions">Transaction buffer encapsulating all the transactions such as the adds, edits and deleted.</param>
        protected override TransactionResult CommitTransactionCore(TransactionBuffer transactions)
        {
            Validators.CheckParameterIsNotNull(transactions, "transactions");
            Validators.CheckFeatureSourceIsEditable(IsEditable);
            Validators.CheckFeatureSourceIsOpen(IsOpen);
            Validators.CheckFeatureSourceIsInTransaction(IsInTransaction);

            TransactionResult transactionResult = new TransactionResult();

            ProcessAddBuffer(transactions.AddBuffer, transactionResult);
            ProcessDeleteBuffer(transactions.DeleteBuffer, transactionResult);
            ProcessEditBuffer(transactions.EditBuffer, transactionResult);

            BuildIndex();

            return transactionResult;
        }

        private void ProcessAddBuffer(Dictionary<string, Feature> addBuffer, TransactionResult transactionResult)
        {
            foreach (string key in addBuffer.Keys)
            {
                if (InternalFeatures.ContainsKey(key))
                {
                    transactionResult.TotalFailureCount += 1;
                    transactionResult.FailureReasons.Add(key, ExceptionDescription.AddTransactionError);
                }
                else
                {
                    InternalFeatures.TryAdd(key, addBuffer[key]);
                    transactionResult.TotalSuccessCount += 1;
                }
            }
        }

        private void ProcessDeleteBuffer(Collection<string> deleteBuffer, TransactionResult transactionResult)
        {
            foreach (string key in deleteBuffer)
            {
                if (InternalFeatures.ContainsKey(key))
                {
                    Feature f;
                    InternalFeatures.TryRemove(key, out f);
                    transactionResult.TotalSuccessCount += 1;
                }
                else
                {
                    transactionResult.TotalFailureCount += 1;
                    transactionResult.FailureReasons.Add(key, ExceptionDescription.DeleteTransactionError);
                }
            }
        }

        private void ProcessEditBuffer(Dictionary<string, Feature> editBuffer, TransactionResult transactionResult)
        {
            foreach (string key in editBuffer.Keys)
            {
                if (InternalFeatures.ContainsKey(key))
                {
                    byte[] targetWellKnownBinary = editBuffer[key].GetWellKnownBinary();
                    Dictionary<string, string> targetColumnValues = InternalFeatures[key].ColumnValues;

                    foreach (KeyValuePair<string, string> item in editBuffer[key].ColumnValues)
                    {
                        targetColumnValues[item.Key] = item.Value;
                    }

                    Feature tmpFeature = new Feature(targetWellKnownBinary, editBuffer[key].Id, targetColumnValues)
                    {
                        Tag = editBuffer[key].Tag
                    };
                    InternalFeatures[key] = tmpFeature;
                    transactionResult.TotalSuccessCount += 1;
                }
                else
                {
                    transactionResult.TotalFailureCount += 1;
                    transactionResult.FailureReasons.Add(key, ExceptionDescription.UpdateTansactionError);
                }
            }
        }

        /// <summary>Returns collection of all the Features in the FeatureSource.</summary>
        /// <returns>Collection of all of the Features in the FeatureSource.</returns>
        /// <remarks>No need to consider anything about pending transactions as this is handled in the non Core
        /// version of the method.</remarks>
        /// <param name="returningColumnNames">Field names of the column data to return with each Feature.</param>
        protected override Collection<Feature> GetAllFeaturesCore(IEnumerable<string> returningColumnNames)
        {
            Validators.CheckParameterIsNotNull(returningColumnNames, "returningColumnNames");
            Validators.CheckFeatureSourceIsOpen(IsOpen);

            Collection<Feature> returnFeatures = new Collection<Feature>();
            foreach (Feature newFeature in InternalFeatures.Values.Select(feature => GetClonedFeature(feature, returningColumnNames)))
            {
                returnFeatures.Add(newFeature);
            }

            return returnFeatures;
        }

        /// <summary>Returns collection of Features inside a bounding box.</summary>
        /// <returns>Collection of Features inside a bounding box.</returns>
        /// <remarks>No need to consider anything about pending transactions as this is handled in the non Core
        /// version of the method.</remarks>
        /// <param name="boundingBox">Bounding box to find Features which are inside.</param>
        /// <param name="returningColumnNames">Field names of the column data to return with each Feature.</param>
        protected override Collection<Feature> GetFeaturesInsideBoundingBoxCore(RectangleShape boundingBox, IEnumerable<string> returningColumnNames)
        {
            Validators.CheckFeatureSourceIsOpen(IsOpen);
            Validators.CheckParameterIsNotNull(returningColumnNames, "returningColumnNames");
            Validators.CheckParameterIsNotNull(boundingBox, "boundingBox");
            Validators.CheckParameterIsValid(boundingBox, "boundingBox");

            /*if (_rTree == null || _rTree.IsEmpty)
            {*/
                return GetFeaturesOnNotCachedBoundingBoxes(boundingBox, returningColumnNames);
            //}*/
            //return GetFeaturesBasedOnCachedBoundingBoxes(boundingBox, returningColumnNames);
            
        }

        /// <summary>Returns collection of Features outside a bounding box.</summary>
        /// <returns>Collection of Features outside a bounding box.</returns>
        /// <remarks>No need to consider anything about pending transactions as this is handled in the non Core
        /// version of the method.</remarks>
        /// <param name="boundingBox">Bounding box to find Features which are outside.</param>
        /// <param name="returningColumnNames">Field names of the column data to return with each Feature.</param>
        protected override Collection<Feature> GetFeaturesOutsideBoundingBoxCore(RectangleShape boundingBox, IEnumerable<string> returningColumnNames)
        {
            Validators.CheckFeatureSourceIsOpen(IsOpen);
            Validators.CheckParameterIsNotNull(returningColumnNames, "returningColumnNames");
            Validators.CheckParameterIsNotNull(boundingBox, "boundingBox");
            Validators.CheckParameterIsValid(boundingBox, "boundingBox");

            Collection<Feature> returnRecords = new Collection<Feature>();
            RectangleShape featureBoundingBox = null;
            foreach (Feature feature in InternalFeatures.Values)
            {
                featureBoundingBox = feature.GetBoundingBox();

                if (!boundingBox.Contains(featureBoundingBox))
                {
                    Dictionary<string, string> columnValues = returningColumnNames.ToDictionary(columnName => columnName, columnName => feature.ColumnValues[columnName]);
                    Feature outsideFeature = new Feature(feature.GetWellKnownBinary(), feature.Id, columnValues)
                    {
                        Tag = feature.Tag
                    };
                    returnRecords.Add(outsideFeature);
                }
            }

            return returnRecords;
        }

        /// <summary>Returns the columns available for the FeatureSource.</summary>
        /// <returns>Columns available for the FeatureSource.</returns>
        protected override Collection<FeatureSourceColumn> GetColumnsCore()
        {
            Validators.CheckFeatureSourceIsOpen(IsOpen);

            return _columns;
        }

        private Collection<Feature> GetFeaturesOnNotCachedBoundingBoxes(RectangleShape boundingBox, IEnumerable<string> returningColumnNames)
        {
            Collection<Feature> returnRecords = new Collection<Feature>();
            foreach (Feature insideFeature in InternalFeatures.Values.Where(f => !f.GetBoundingBox().IsDisjointed(f.GetBoundingBox())))
            {
                Feature clone = GetClonedFeature(insideFeature, returningColumnNames);
                returnRecords.Add(clone);
            }

            return returnRecords;
        }

        private Collection<Feature> GetFeaturesBasedOnCachedBoundingBoxes(RectangleShape extent, IEnumerable<string> returningColumnNames)
        {
            /*Envelope envelope = new Envelope(extent.UpperLeftPoint.X, extent.LowerRightPoint.X, extent.UpperLeftPoint.Y, extent.LowerRightPoint.Y);
            IList<Feature> tempIds = _rTree.Query(envelope);*/
            //List<Feature> recordIds = new List<Feature>();

            /*if (tempIds.Count > MaxRecordsToDraw && MaxRecordsToDraw != 0)
            {
                int drawFeatureInterval = tempIds.Count / MaxRecordsToDraw;
                for (int i = 0; i < tempIds.Count; i += drawFeatureInterval)
                {
                    recordIds.Add(tempIds[i]);
                }
            }
            else
            {
                foreach (Feature f in tempIds)
                {
                    recordIds.Add(f);
                }
            }*/

            Collection<Feature> features = new Collection<Feature>();
            /*foreach (Feature insideFeature in recordIds)
            {
                features.Add(GetClonedFeature(insideFeature, returningColumnNames));
            }*/

            return features;
        }

        private static Feature GetClonedFeature(Feature feature, IEnumerable<string> returningColumnNames)
        {
            Dictionary<string, string> columnValues = returningColumnNames.ToDictionary(columnName => columnName, columnName => feature.ColumnValues.ContainsKey(columnName) ? feature.ColumnValues[columnName] : string.Empty);
           
            byte[] wellKnownBinary = feature.GetWellKnownBinary();
            byte[] projectedWellKnownBinary = new byte[wellKnownBinary.Length];
            Buffer.BlockCopy(wellKnownBinary, 0, projectedWellKnownBinary, 0, wellKnownBinary.Length);
            return new Feature(projectedWellKnownBinary, feature.Id, columnValues) {Tag = feature.Tag};
           
        }

        /// <summary>Returns a Feature based on a <paramref name="key"/></summary>
        /// <param name="key">Key of the Feature</param>
        /// <param name="returningColumnNamesType"></param>
        /// <returns>Specified Feature</returns>
        public Feature GetFeatureByKey(string key, ReturningColumnsType returningColumnNamesType)
        {
            Validators.CheckFeatureSourceIsOpen(IsOpen);
            Validators.CheckReturningColumnNamesTypeIsValid(returningColumnNamesType, "returningColumnNamesType");

            Collection<string> returningColumnNames = GetReturningColumnNames(returningColumnNamesType);

            return GetFeatureByKey(key, returningColumnNames);
        }

        /// <summary>Returns a Feature based on <paramref name="key"/></summary>
        /// <param name="key">Key of the Feature</param>
        /// <param name="returningColumnNames">Fields of the returned Feature</param>
        /// <returns>Specified Feature</returns>
        public Feature GetFeatureByKey(string key, IEnumerable<string> returningColumnNames)
        {

            Validators.CheckFeatureSourceIsOpen(IsOpen);
            Validators.CheckParameterIsNotNull(returningColumnNames, "returningColumnNames");
            Validators.CheckParameterIsNotNull(key, "key");

            Feature feature = new Feature();

            if (InternalFeatures.Count > 0)
            {
                if(InternalFeatures.ContainsKey(key))
                    feature = InternalFeatures[key].CloneDeep(returningColumnNames);
            }

            return feature;
        }

    }
}
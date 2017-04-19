using Mapgenix.Shapes;
using Mapgenix.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;


namespace Mapgenix.FeatureSource
{
    /// <summary>FeatureSource dealing with multiple FeatureSource of the same type.</summary>
    [Serializable]
    public class MultipleFeatureSource : BaseFeatureSource
    {
        private Collection<BaseFeatureSource> _featureSources;
        private Collection<int> _featureCounts;

        /// <summary>Default cosntructor of MultipleFeatureSource.</summary>
        /// <remarks>You need to add FeatureSources to MultipleFeatureSource with property FeatureSources.</remarks>
        /// <returns>None</returns>
        public MultipleFeatureSource()
            : this(new BaseFeatureSource[] { })
        {
        }

        /// <summary>Constructor by passing a group of FeatureSources.</summary>
        /// <param name="featureSources">Target FeatureSources to include in MultipleFeatureSource.</param>
        public MultipleFeatureSource(IEnumerable<BaseFeatureSource> featureSources)
        {
            Validators.CheckParameterIsNotNull(featureSources, "featureSources");

            _featureSources = new Collection<BaseFeatureSource>();

            foreach (BaseFeatureSource featureSource in featureSources)
            {
                _featureSources.Add(featureSource);
            }

            _featureCounts = new Collection<int>();
        }


        /// <summary>Collection of FeatureSources in MultipleFeatureSource.</summary>
        public Collection<BaseFeatureSource> FeatureSources
        {
            get { return _featureSources; }
        }

        /// <summary>Returns true if FeatureSource allows edits or false if is read only.</summary>
        /// <returns>true if FeatureSource allows edits or false if is read only.</returns>
        /// <remarks>
        /// 	<para>Useful to check if a specific FeatureSource accepts editing.
        ///     Raises exception if false and call BeginTransaction.<br/>
        /// 		<br/>
        ///     To create or extent a FeatureSource it is expected to override this virtual method to allows editing.</para>
        /// </remarks>
        public override bool IsEditable
        {
            get { return false; }
        }

        /// <summary>Returns Columns available for the FeatureSources in MultipleFeatureSource.</summary>
        /// <returns>Columns available for the FeatureSources in MultipleFeatureSource.</returns>
        protected override Collection<FeatureSourceColumn> GetColumnsCore()
        {
            Validators.CheckFeatureSourceIsOpen(IsOpen);

            Collection<FeatureSourceColumn> returnFeatureSourceColumns = new Collection<FeatureSourceColumn>();

            foreach (FeatureSourceColumn featureSourceColumn in _featureSources.Select(featureSource => featureSource.GetColumns()).SelectMany(featureSourceColumns => featureSourceColumns.Where(featureSourceColumn => !returnFeatureSourceColumns.Contains(featureSourceColumn))))
            {
                returnFeatureSourceColumns.Add(featureSourceColumn);
            }

            return returnFeatureSourceColumns;
        }

        /// <summary>Opens MultipleFeatureSource to be ready to use.</summary>
        /// <returns>None</returns>
        /// <remarks>Opens all the FeatureSources in the MultipleFeatureSource.</remarks>
        protected override void OpenCore()
        {
            _featureCounts.Clear();
            foreach (BaseFeatureSource featureSource in _featureSources)
            {
                featureSource.Open();
                _featureCounts.Add(featureSource.GetCount());
            }
        }

        /// <summary>Closes MultipleFeatureSource and releases any resources used.</summary>
        /// <remarks>Closes all the FeatureSources in the MultipleFeatureSource.
        /// </remarks>
        /// <returns>None</returns>
        protected override void CloseCore()
        {
            if (IsOpen)
            {
                foreach (BaseFeatureSource featureSource in _featureSources)
                {
                    featureSource.Close();
                }

                _featureCounts.Clear();
            }
        }

        /// <summary>Returns the total number of records in MultipleFeatureSource.</summary>
        /// <returns>Total number of records in MultipleFeatureSource.</returns>
        /// <remarks>Returning feature count stands for all the FeatureSources in the MultipleFeatureSource.</remarks>
        protected override int GetCountCore()
        {
            Validators.CheckFeatureSourceIsOpen(IsOpen);

            return _featureSources.Sum(featureSource => featureSource.GetCount());
        }

        /// <summary>Returns the bounding box encompassing all of the FeatureSources in the MultipleFeatureSource.</summary>
        /// <returns>Bounding box encompassing all of the FeatureSources in the MultipleFeatureSource.</returns>
        protected override RectangleShape GetBoundingBoxCore()
        {
            Validators.CheckFeatureSourceIsOpen(IsOpen);
            Validators.CheckFeatureSourceCollectionIsEmpty(_featureSources);

            RectangleShape boundingBox = null;
            foreach (BaseFeatureSource featureSource in _featureSources)
            {
                if (boundingBox == null)
                {
                    boundingBox = featureSource.GetBoundingBox();
                }
                else
                {
                    boundingBox.ExpandToInclude(featureSource.GetBoundingBox());
                }
            }

            return boundingBox;
        }

        /// <summary>Not supported in the concrete feature source MultipleFeatureSource.</summary>
        protected override TransactionResult CommitTransactionCore(TransactionBuffer transactions)
        {
            throw new NotSupportedException();
        }

        /// <summary>Returns a collection of all the Features in the MultipleFeatureSource.</summary>
        /// <returns>Collection of all the Features in the MultipleFeatureSource.</returns>
        /// <remarks>Returning collection of Features includes all the features in all the FeatureSources in the MultipleFeatureSource.</remarks>
        /// <param name="returningColumnNames">Field names of the column data to return with each Feature.</param>
        protected override Collection<Feature> GetAllFeaturesCore(IEnumerable<string> returningColumnNames)
        {
            Validators.CheckFeatureSourceIsOpen(IsOpen);
            Collection<Feature> returnFeatures = new Collection<Feature>();

            foreach (Feature feature in _featureSources.Select(featureSource => featureSource.GetAllFeatures(returningColumnNames)).SelectMany(features => features))
            {
                returnFeatures.Add(feature);
            }

            return returnFeatures;
        }

        /// <summary>Returns a collection of all the Features of the MultipleFeatureSource inside a bounding box.</summary>
        /// <remarks>Collection of all the Features of the MultipleFeatureSource inside a bounding box.</remarks>
        /// <param name="boundingBox">Bounding box to find Features inside of.</param>
        /// <param name="returningColumnNames">Field names of the column data to return with each Feature.</param>
        protected override Collection<Feature> GetFeaturesInsideBoundingBoxCore(RectangleShape boundingBox, IEnumerable<string> returningColumnNames)
        {
            Validators.CheckFeatureSourceIsOpen(IsOpen);
            Validators.CheckParameterIsNotNull(boundingBox, "boundingBox");
            Validators.CheckShapeIsValidForOperation(boundingBox);
            Validators.CheckParameterIsNotNull(returningColumnNames, "returningColumnNames");

            Collection<Feature> returnFeatures = new Collection<Feature>();

            foreach (Feature feature in _featureSources.Select(featureSource => featureSource.GetFeaturesInsideBoundingBox(boundingBox, returningColumnNames)).SelectMany(features => features))
            {
                returnFeatures.Add(feature);
            }

            return returnFeatures;
        }

        /// <summary>Returns a collection of Features based on Ids.</summary>
        /// <returns>Collection of Features based on Ids.</returns>
        /// <remarks>Returning collection of Features includes all the features with the passed in Ids insides 
        /// all the FeatureSources in the MultipleFeautureSource.</remarks>
        /// <param name="ids">Group of Ids uniquely identifying the Features in the FeatureSource.</param>
        /// <param name="returningColumnNames">Field names of the column data to return with each Feature.</param>
        protected override Collection<Feature> GetFeaturesByIdsCore(IEnumerable<string> ids, IEnumerable<string> returningColumnNames)
        {
            Validators.CheckFeatureSourceIsOpen(IsOpen);
            Validators.CheckParameterIsNotNull(ids, "ids");
            Validators.CheckParameterIsNotNull(returningColumnNames, "returningColumnNames");

            List<string> idsList = new List<string>(ids);
            if (idsList.Count == 1)
            {
                int id;
                if (int.TryParse(idsList[0], out id))
                {
                    int totalCount = 0;
                    for (int i = 0; i < _featureCounts.Count; i++)
                    {
                        if (id <= totalCount + _featureCounts[i])
                        {
                            idsList[0] = (id - totalCount).ToString();

                            Collection<Feature> features = _featureSources[i].GetFeaturesByIds(idsList, returningColumnNames);
                            return features;
                        }
                        totalCount += _featureCounts[i];
                    }
                }
            }

            Collection<Feature> returnFeatures = new Collection<Feature>();
            foreach (Feature feature in _featureSources.Select(featureSource => featureSource.GetFeaturesByIds(ids, returningColumnNames)).SelectMany(features => features))
            {
                returnFeatures.Add(feature);
            }

            return returnFeatures;
        }

       
        /// <summary>Returns a collection of Features used for drawing.</summary>
        /// <returns>Collection of Features used for drawing.</returns>
        /// <remarks>Returns a collection Features to draw in the MultipleFeatureSource inside a bounding box.</remarks>
        /// <param name="boundingBox">Bounding box where to draw the features.</param>
        /// <param name="screenWidth">Width in screen pixels of the canvas to draw on.</param>
        /// <param name="screenHeight">Height in screen pixels of the canvas to draw on.</param>
        /// <param name="returningColumnNames">Field names of the column data to return with each Feature.</param>
        protected override Collection<Feature> GetFeaturesForDrawingCore(RectangleShape boundingBox, double screenWidth, double screenHeight, IEnumerable<string> returningColumnNames)
        {
            Validators.CheckFeatureSourceIsOpen(IsOpen);
            Validators.CheckParameterIsNotNull(returningColumnNames, "returningColumnNames");
            Validators.CheckParameterIsNotNull(boundingBox, "boundingBox");
            Validators.CheckParameterIsValid(boundingBox, "boundingBox");
            Validators.CheckValueIsBiggerThanZero(screenWidth, "screenWidth");
            Validators.CheckValueIsBiggerThanZero(screenHeight, "screenHeight");

            Collection<Feature> returnFeatures = new Collection<Feature>();

            foreach (Feature feature in _featureSources.Select(featureSource => featureSource.GetFeaturesForDrawing(boundingBox, screenWidth, screenHeight, returningColumnNames)).SelectMany(features => features))
            {
                returnFeatures.Add(feature);
            }

            return returnFeatures;
        }

        /// <summary>Returns a collection of Features nearest to a TargetShape in the MultipleFeatureSource.</summary>
        /// <returns>Collection of Features nearest to a TargetShape in the MultipleFeatureSource.</returns>
        /// <param name="targetShape">Target shape to find nearest Features to.</param>
        /// <param name="unitOfData">Unit in which the Target Shape and the FeatureSource are in such as feet, meters, decimal degrees.</param>
        /// <param name="maxItemsToFind">Maximum number of features to find.</param>
        /// <param name="returningColumnNames">Field names of the column data to return with each Feature.</param>
        protected override Collection<Feature> GetFeaturesNearestToCore(BaseShape targetShape, GeographyUnit unitOfData, int maxItemsToFind, IEnumerable<string> returningColumnNames)
        {
            Validators.CheckFeatureSourceIsOpen(IsOpen);
            Validators.CheckIfInputValueIsBiggerThan(maxItemsToFind, "maxItemsToFind", 0, RangeCheckingInclusion.IncludeValue);
            Validators.CheckParameterIsNotNull(targetShape, "targetShape");
            Validators.CheckParameterIsValid(targetShape, "targetShape");
            Validators.CheckParameterIsNotNull(returningColumnNames, "returningColumnNames");
            Validators.CheckGeographyUnitIsValid(unitOfData, "unitOfData");

            Collection<Feature> returnFeatures = new Collection<Feature>();

            foreach (Feature feature in _featureSources.Select(featureSource => featureSource.GetFeaturesNearestTo(targetShape, unitOfData, maxItemsToFind, returningColumnNames)).SelectMany(features => features))
            {
                returnFeatures.Add(feature);
            }

            return returnFeatures;
        }

        /// <summary>Returns a collection of all the Features of the MultipleFeatureSource outside a bounding box.</summary>
        /// <remarks>Collection of all the Features of the MultipleFeatureSource outside a bounding box.</remarks>
        /// <param name="boundingBox">Bounding box to find Features outside of.</param>
        /// <param name="returningColumnNames">Field names of the column data to return with each Feature.</param>
        protected override Collection<Feature> GetFeaturesOutsideBoundingBoxCore(RectangleShape boundingBox, IEnumerable<string> returningColumnNames)
        {
            Validators.CheckFeatureSourceIsOpen(IsOpen);
            Validators.CheckParameterIsNotNull(returningColumnNames, "returningColumnNames");
            Validators.CheckParameterIsNotNull(boundingBox, "boundingBox");
            Validators.CheckParameterIsValid(boundingBox, "boundingBox");

            Collection<Feature> returnFeatures = new Collection<Feature>();

            foreach (Feature feature in _featureSources.Select(featureSource => featureSource.GetFeaturesOutsideBoundingBox(boundingBox, returningColumnNames)).SelectMany(features => features))
            {
                returnFeatures.Add(feature);
            }

            return returnFeatures;
        }

        /// <summary>Returns a collection of Features within a certain distance
        /// of a TargetShape. Applies to all featureSources in the MultipleFeatureSource.</summary>
        /// <returns>Collection of Features within a certain distance of a TargetShape.</returns>
        /// <param name="targetShape">Target shape to find features within a distance of.</param>
        /// <param name="unitOfData">Unit the FeatureSources and TargetShape are in such as meters, feet or decimal degrees.</param>
        /// <param name="distanceUnit">Unit of the distance parameter such as feet, miles or kilometers etc.</param>
        /// <param name="distance">Distance from the TargetShape to find Features</param>
        /// <param name="returningColumnNames">Field names of the column data to return with each Feature.</param>
        protected override Collection<Feature> GetFeaturesWithinDistanceOfCore(BaseShape targetShape, GeographyUnit unitOfData, DistanceUnit distanceUnit, double distance, IEnumerable<string> returningColumnNames)
        {
            Validators.CheckFeatureSourceIsOpen(IsOpen);
            Validators.CheckParameterIsNotNull(targetShape, "targetShape");
            Validators.CheckParameterIsValid(targetShape, "targetShape");
            Validators.CheckGeographyUnitIsValid(unitOfData, "unitOfData");
            Validators.CheckDistanceUnitIsValid(distanceUnit, "distanceUnit");
            Validators.CheckParameterIsNotNull(returningColumnNames, "returningColumnNames");

            Collection<Feature> returnFeatures = new Collection<Feature>();

            foreach (Feature feature in _featureSources.Select(featureSource => featureSource.GetFeaturesWithinDistanceOf(targetShape, unitOfData, distanceUnit, distance, returningColumnNames)).SelectMany(features => features))
            {
                returnFeatures.Add(feature);
            }

            return returnFeatures;
        }

        /// <summary>Returns a collection of Features in the MultiupleFeatureSource based on a target Feature and a spatial query type.</summary>
        /// <returns>Collection of Features in the MultiupleFeatureSource based on a target Feature and a spatial query type.</returns>
        /// <remarks>
        /// 		<br/>
        /// 		<strong>Spatial Query Types:</strong><br/>
        /// 		<br/>
        /// 		<strong>Disjoint</strong> - Returns a collection of features where the specific Feature
        ///     and the targetShape have no points in common.<br/>
        /// 		<br/>
        /// 		<strong>Intersects</strong> - Returns a collection of features where where the specific
        ///     Feature and the targetShape have at least one point in common.<br/>
        /// 		<br/>
        /// 		<strong>Touches</strong> - Returns a collection of features where the specific Feature
        ///     and the targetShape have at least one boundary point in common, but no interior
        ///     points.<br/>
        /// 		<br/>
        /// 		<strong>Crosses</strong> - Returns a collection of features where the specific Feature
        ///     and the targetShape share some but not all interior points.<br/>
        /// 		<br/>
        /// 		<strong>Within</strong> - Returns a collection of features where the specific Feature
        ///     lies within the interior of the targetShape.<br/>
        /// 		<br/>
        /// 		<strong>Contains</strong> - Returns a collection of features where the specific Feature
        ///     lies within the interior of the current shape.<br/>
        /// 		<br/>
        /// 		<strong>Overlaps</strong> - Returns a collection of features where the specific Feature
        ///     and the targetShape share some but not all points in common.<br/>
        /// 		<br/>
        /// 		<strong>TopologicalEqual</strong> - Returns a collection of features where the specific
        ///     Feature and the target Shape are topologically equal.<br/>
        /// 		<br/>
        ///     The default implementation of SpatialQueryCore uses
        ///     GetFeaturesInsideBoundingBoxCore method to pre-filter the spatial query. It is recommended to provide one's own implementation 
        ///     for this method that is more efficient. When you override this method, it is recommned to use a spatial index<br/>
        /// 		<br/>
        /// 		<br/>
        ///     As a concrete public method that wraps a Core method, Mapgenix reserves the right
        ///     to add events and other logic returned by the Core version of the method. </para>
        /// </remarks>
        /// <param name="targetShape">Target shape in spatial query.</param>
        /// <param name="queryType">Kind of spatial query to perform.</param>
        /// <param name="returningColumnNames">Column names.</param>  
        protected override Collection<Feature> SpatialQueryCore(BaseShape targetShape, QueryType queryType, IEnumerable<string> returningColumnNames)
        {
            Validators.CheckFeatureSourceIsOpen(IsOpen);
            Validators.CheckParameterIsNotNull(targetShape, "targetShape");
            Validators.CheckParameterIsValid(targetShape, "targetShape");
            Validators.CheckParameterIsNotNull(returningColumnNames, "returningColumnNames");

            Collection<Feature> returnFeatures = new Collection<Feature>();

            foreach (Feature feature in _featureSources.Select(featureSource => featureSource.SpatialQuery(targetShape, queryType, returningColumnNames)).SelectMany(features => features))
            {
                returnFeatures.Add(feature);
            }

            return returnFeatures;
        }
    }
}
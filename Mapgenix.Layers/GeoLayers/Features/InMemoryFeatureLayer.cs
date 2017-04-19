using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Mapgenix.Shapes;
using Mapgenix.FeatureSource;

namespace Mapgenix.Layers
{
    /// <summary>Layer in memory for dynamic features.</summary>
    /// <remarks>Typically used for dynamic temporary features coming, for example, from an
    /// external device such as a GPS unit, or any features not saved to disk or a database yet.</remarks>
    [Serializable]
    public class InMemoryFeatureLayer : BaseFeatureLayer
    {
        private const string existingFeatureColumnName = "state";
        private const string existingFeatureColumnValue = "selected";

        /// <summary>Gets a collection of the columns for the features in the in memory layer.</summary>
        /// <returns>Collection of the columns for the features in the in memory layer.</returns>
        public Collection<FeatureSourceColumn> Columns
        {
            get { return ((InMemoryFeatureSource)FeatureSource).GetColumns(); }
        }

        /// <summary>Gets the dictionary holding the features.</summary>
        /// <returns>Dictionary holding the features.</returns>
        /// <remarks>You can add, remove or edit features in the dictionary and this takes effect immediately in the layer.</remarks>
        private ConcurrentDictionary<string, Feature> InternalFeatures
        {
            get { return ((InMemoryFeatureSource)FeatureSource).InternalFeatures; }
        }

        /// <summary>Opens the in memory layer to get it ready to use.</summary>
        /// <remarks>Abstract method called from the concrete public method Open.</remarks>
        /// <returns>None</returns>
        protected override void OpenCore()
        {

            base.OpenCore();

            if (FeatureSource.Projection != null && !FeatureSource.Projection.IsOpen)
            {
                FeatureSource.Projection.Open();
            }
        }

        /// <summary>Adds a feature to the in memory feature layer.</summary>
        public void Add(Feature f)
        {
            InternalFeatures.TryAdd(Guid.NewGuid().ToString(), f);
        }

        /// <summary>Adds a feature with key to the in memory feature layer.</summary>
        public void Add(string key, Feature f)
        {
            InternalFeatures.TryAdd(key, f);
        }

        /// <summary>Removes a feature from the in memory feature layer.</summary>
        public void Remove(string key)
        {
            Feature f;
            InternalFeatures.TryRemove(key, out f);
        }

        /// <summary>Removes the last feature from the in memory feature layer.</summary>
        public void RemoveLast()
        {
            if (InternalFeatures.Count > 0)
            {
                Remove(InternalFeatures.Keys.Last());
            }
        }

        /// <summary>Checks by key if a feature exists in the in memory feature layer.</summary>
        public bool Contains(string key)
        {
            return InternalFeatures.ContainsKey(key);
        }

        /// <summary>Finds a feature by key in the in memory feature layer.</summary>
        public Feature Find(string key)
        {
            return InternalFeatures[key];
        }

        /// <summary>Checks if the in memory feature layer is empty of features.</summary>
        public bool IsEmpty()
        {
            return InternalFeatures.IsEmpty;
        }

        /// <summary>Returns all the keys of the features in the in memory feature layer.</summary>
        public List<string> AllKeys()
        {
            return InternalFeatures.Keys.ToList();
        }

        /// <summary>Replaces a feature by a key in the in memory feature layer.</summary>
        public void Replace(string key, Feature f)
        {
            InternalFeatures[key] = f;
        }

        /// <summary>Add and replaces a feature by a key in the in memory feature layer.</summary>
        public void AddReplace(string key, Feature f)
        {
            if (InternalFeatures.ContainsKey(key))
            {
                InternalFeatures[key] = f;
            }
            else
            {
                InternalFeatures.TryAdd(key, f);
            }
        }

        /// <summary>Finds the first feature in the in memory feature layer.</summary>
        public Feature First()
        {
            return InternalFeatures.Values.First();
        }

        /// <summary>Gets the number of features in the inmemory feature layer.</summary>
        public int NumFeatures()
        {
            return InternalFeatures.Count;
        }

        /// <summary>Finds the last feature in the in memory feature layer.</summary>
        public Feature Last()
        {
            return InternalFeatures.Values.Last();
        }

        /// <summary>Clears all the features in the in memory feature layer.</summary>
        public void Clear()
        {
            InternalFeatures.Clear();
        }

        /// <summary>Gets all the features in the in memory feature layer.</summary>
        public List<Feature> GetAll()
        {
            return InternalFeatures.Values.ToList();
        }

        /// <summary>Gets a collection of all the vertices of the features in the in memory feature layer.</summary>
        public Collection<PointShape> GetAllVertices()
        {
            Collection<PointShape> vertices = new Collection<PointShape>();

            foreach (Feature feature in GetAll())
            {
                foreach (PointShape pointShape in FeatureHelper.GetAllVerticesFromFeature(feature))
                {
                    vertices.Add(pointShape);
                }
            }

            return vertices;
        }

        /// <summary>Removes a vertex of a feature in the in memory feature layer.</summary>
        public bool RemoveVertex(Feature feature, PointShape pointShape)
        {
            bool success = false;
            foreach (string featureKey in AllKeys())
            {
                if (Find(featureKey).Id == feature.Id)
                {
                    Feature newFeature = FeatureHelper.CloneFeatureAndRemoveVertex(Find(featureKey), new Vertex(pointShape));
                    if (newFeature.Id != null)
                    {
                        Replace(featureKey, newFeature);
                        success = true;
                    }
                }
            }

            return success;
        }

        /// <summary>Moves a vertex of a feature in the in memory feature layer.</summary>
        public Feature MoveVertex(Feature sourceFeature, Vertex movingVertex, Vertex targetVertex)
        {
            Feature returnFeature = FeatureHelper.CloneFeatureAndMoveVertex(sourceFeature, movingVertex, targetVertex);

            foreach (string featureKey in AllKeys())
            {
                Feature feature = Find(featureKey);
                if (feature.ColumnValues != null)
                {
                    if (feature.ColumnValues[existingFeatureColumnName] == existingFeatureColumnValue)
                    {
                        PointShape pointShape = new PointShape(targetVertex);
                        Replace(featureKey, new Feature(pointShape.GetWellKnownBinary(), feature.Id, feature.ColumnValues));
                    }
                }
            }

            return returnFeature;
        }

        /// <summary>Deletes a feature by WKT in the in memory feature layer.</summary>
        public void Delete(string wktFeature)
        {
            Open();
            EditTools.BeginTransaction();
            EditTools.Delete(wktFeature);
            EditTools.CommitTransaction();
            Close();
        }

        /// <summary>Adds a feature in the in memory feature layer.</summary>
        public void AddFeature(BaseShape shape)
        {
            Open();
            EditTools.BeginTransaction();
            EditTools.Add(new Feature(shape));
            EditTools.CommitTransaction();
            Close();
        }

        /// <summary>Updates a feature in the in memory feature layer.</summary>
        public void UpdateFeature(Feature feature)
        {
            Open();
            EditTools.BeginTransaction();
            EditTools.Update(feature); 
            EditTools.CommitTransaction();
            Close();
        }

        /// <summary>Builds a sptial index for the in memory feature layer.</summary>
        public void BuildIndex()
        {
            ((InMemoryFeatureSource)FeatureSource).BuildIndex();
        }
    }
}
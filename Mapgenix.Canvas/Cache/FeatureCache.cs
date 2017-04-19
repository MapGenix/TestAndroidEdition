using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using GeoAPI.Geometries;
using Mapgenix.Shapes;
using NetTopologySuite.Index.Quadtree;

namespace Mapgenix.Canvas
{
    /// <summary>
    /// FeatureCache is a cache system used in FeatureSource to improve fetching of features
    /// </summary>
    /// <remarks>
    /// The FeatureCache system uses bounding box (RectangleShapes) as a mechanism to cache the features.
    /// </remarks>
    [Serializable]
    public class FeatureCache
    {
        private readonly Collection<RectangleShape> _cachedExtents;
        private readonly Dictionary<string, Feature> _cachedFeatures;

        private bool _isActive;

        [NonSerialized] private Quadtree<Feature> _quadtree;

        /// <summary>
        /// Default constructor that creates a new instance of FeatureCache.
        /// </summary>
        /// <returns>None</returns>
        /// <remarks>
        /// The cache system for features is not used by default. to use set
        /// the <strong>IsActive</strong> property to true.
        /// </remarks>
        public FeatureCache()
        {
            _quadtree = new Quadtree<Feature>();
            _cachedExtents = new Collection<RectangleShape>();
            _cachedFeatures = new Dictionary<string, Feature>();
            new Dictionary<RectangleShape, Collection<Feature>>();
        }

        /// <summary>Gets or sets the cache system to active or not.</summary>
        public bool IsActive
        {
            get { return _isActive; }
            set { _isActive = value; }
        }

        /// <summary>
        /// Returns a collection of Features in the feature cache system. The features returned
        /// are based on the BoundingBox and are retrieved from file system or DB system of a previous operation.
        /// </summary>
        /// <returns>
        /// Returns a collection of Features cached in the feature cache system.
        /// </returns>
        /// <remarks>
        /// This method is a concrete wrapper of the virtual method GetFeaturesCore. It
        /// returns whatever is returned by the GetBoundingBoxCore method. 
        /// </remarks>
        /// <exception cref="System.ArgumentNullException" caption="ArgumentNullException">If worldextent is null, it throws an ArgumentNullException.</exception>
        public Collection<Feature> GetFeatures(RectangleShape worldExtent)
        {
            Validators.CheckParameterIsNotNull(worldExtent, "worldExtent");

            return GetFeaturesCore(worldExtent);
        }

          /// <summary>
        /// Returns a Collection of Features in the feature cache system. The features returned
        /// are based on the bounding box and are retrieve from the file system or DB system of a previous operation.
        /// </summary>
        /// <returns>
        /// Collection of Features cached into the feature cache system.
        /// </returns>
        /// <exception cref="System.ArgumentNullException" caption="ArgumentNullException">If worldextent is null, it throws an ArgumentNullException.</exception>
        protected virtual Collection<Feature> GetFeaturesCore(RectangleShape worldExtent)
        {
            Validators.CheckParameterIsNotNull(worldExtent, "worldExtent");

            var returnValue = new Collection<Feature>();
            RectangleShape cachedExtent = null;

            foreach (var extent in _cachedExtents)
            {
                if (Contains(extent, worldExtent))
                {
                    cachedExtent = extent;
                    break;
                }
            }

            if (cachedExtent != null)
            {
                var envelope = new Envelope(cachedExtent.UpperLeftPoint.X, cachedExtent.LowerRightPoint.X,
                    cachedExtent.UpperLeftPoint.Y, cachedExtent.LowerRightPoint.Y);

                var recordIds = (ArrayList) _quadtree.Query(envelope);
                foreach (string recordId in recordIds)
                {
                    returnValue.Add(_cachedFeatures[recordId]);
                }
            }

            return returnValue;
        }

        /// <summary>
        /// Clears all of the cached items in this feature cache.
        /// </summary>
        public void Clear()
        {
            _quadtree = new Quadtree<Feature>();

            if (_cachedExtents != null)
            {
                _cachedExtents.Clear();
            }

            if (_cachedFeatures != null)
            {
                _cachedFeatures.Clear();
            }
        }

        /// <summary>
        /// Adds a collection of Features to the feature cache system based on a world extent.
        /// </summary>
        /// <returns>
        /// None.
        /// </returns>
        /// <remarks>
        /// This method is the concrete wrapper of the virtual method AddCore. It
        /// will return what is returned by the AddCore method. 
        /// </remarks>
        /// <exception cref="System.ArgumentNullException" caption="ArgumentNullException">If WorldExtent is null, it throws an ArgumentNullException.</exception>
        /// <exception cref="System.InvalidOperationException" caption="InvalidOperationException">if RectangleShape is invalid for the WorldExtent, it throws an InvalidOperationException.</exception>
        /// <exception cref="System.ArgumentNullException" caption="ArgumentNullException">If collection of features is null, it throws an ArgumentNullException.</exception>
        
        public void Add(RectangleShape worldExtent, Collection<Feature> features)
        {
            Validators.CheckMapEngineExtentIsValid(worldExtent, "worldExtent");
            Validators.CheckParameterIsNotNull(features, "features");

            AddCore(worldExtent, features);
        }

        /// <summary>
        /// This method adds a collection of features to the feature cache system based on world extent.
        /// </summary>
        /// <returns>
        /// None.
        /// </returns>
        /// <exception cref="System.ArgumentNullException" caption="ArgumentNullException">If WorldExtent is null, it throws an ArgumentNullException.</exception>
        /// <exception cref="System.InvalidOperationException" caption="InvalidOperationException">if RectangleShape is invalid for the WorldExtent, it throws an InvalidOperationException.</exception>
        /// <exception cref="System.ArgumentNullException" caption="ArgumentNullException">If collection of features is null, it throws an ArgumentNullException.</exception>
        protected virtual void AddCore(RectangleShape worldExtent, Collection<Feature> features)
        {
            Validators.CheckMapEngineExtentIsValid(worldExtent, "worldExtent");
            Validators.CheckParameterIsNotNull(features, "features");

            if (features.Count != 0)
            {
                if (!IsExtentCached(worldExtent))
                {
                    _cachedExtents.Add(worldExtent);
                    foreach (var feature in features)
                    {
                        if (!_cachedFeatures.ContainsKey(feature.Id))
                        {
                            _cachedFeatures.Add(feature.Id, feature);

                            var envelope = new Envelope(worldExtent.UpperLeftPoint.X, worldExtent.LowerRightPoint.X,
                                worldExtent.UpperLeftPoint.Y, worldExtent.LowerRightPoint.Y);
                            _quadtree.Insert(envelope, feature);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// The default implementation is to do nothing.
        /// </summary>
        /// <returns>
        /// None.
        /// </returns>
        /// <remarks>
        /// This method is the concrete wrapper of the virtual method OpenCore. It
        /// will return whatever is returned by the OpenCore method. 
        /// </remarks>
        public void Open()
        {
            OpenCore();
        }

        /// <summary>
        /// The default implementation is to do nothing.
        /// </summary>
        /// <returns>
        /// None.
        /// </returns>
        protected virtual void OpenCore()
        {
        }

        /// <summary>
        /// The default implementation is to do nothing.
        /// </summary>
        /// <returns>
        /// None.
        /// </returns>
        /// <remarks>
        /// This method is the concrete wrapper of the virtual method CloseCore. It
        /// will return whatever is returned by the CloseCore method. 
        /// </remarks>
        public void Close()
        {
            CloseCore();
        }

        /// <summary>
        /// The default implementation is to do nothing.
        /// </summary>
        /// <returns>
        /// None.
        /// </returns>
        protected virtual void CloseCore()
        {
        }

        /// <summary>
        /// Determines if the features within the world extent passed in is cached in the feature cache System.
        /// </summary>
        /// <param name="worldExtent">Target world extent used to determine if the features within it are cached or not.</param>
        /// <returns>True if the features within the specified world extent are cached. Otherwise, returns false.</returns>
        public bool IsExtentCached(RectangleShape worldExtent)
        {
            Validators.CheckMapEngineExtentIsValid(worldExtent, "worldExtent");

            var returnValue = false;

            if (_cachedExtents.Contains(worldExtent))
            {
                returnValue = true;
            }
            else
            {
                foreach (var extent in _cachedExtents)
                {
                    if (Contains(extent, worldExtent))
                    {
                        returnValue = true;
                        break;
                    }
                }
            }

            return returnValue;
        }

        private static bool Contains(RectangleShape extent, RectangleShape targetExtent)
        {
            var contains = false;

            if (extent.UpperLeftPoint.X <= targetExtent.UpperLeftPoint.X &&
                extent.UpperLeftPoint.Y >= targetExtent.UpperLeftPoint.Y &&
                extent.LowerRightPoint.X >= targetExtent.LowerRightPoint.X &&
                extent.LowerRightPoint.Y <= targetExtent.LowerRightPoint.Y)
            {
                contains = true;
            }

            return contains;
        }
    }
}
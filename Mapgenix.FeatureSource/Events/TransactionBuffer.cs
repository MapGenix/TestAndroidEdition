using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Mapgenix.Shapes;

namespace Mapgenix.FeatureSource
{
    /// <summary>Holding place for transactions that have not yet been committed.</summary>
    [Serializable]
    public class TransactionBuffer
    {
        readonly Dictionary<string, Feature> _addBuffer;
        readonly Collection<string> _deleteBuffer;
        readonly Dictionary<string, Feature> _updateBuffer;

        /// <summary>Class constructor.</summary>
        /// <overloads>Default constructor.</overloads>
        public TransactionBuffer()
        {
            _addBuffer = new Dictionary<string, Feature>(StringComparer.CurrentCultureIgnoreCase);
            _deleteBuffer = new Collection<string>();
            _updateBuffer = new Dictionary<string, Feature>(StringComparer.CurrentCultureIgnoreCase);
        }

        /// <summary>Class constructor.</summary>
        /// <overloads>Constructor passing in the necessary properties in the class.</overloads>
        /// <returns>None</returns>
        /// <remarks>None.</remarks>
        public TransactionBuffer(Dictionary<string, Feature> addBuffer, Collection<string> deleteBuffer, Dictionary<string, Feature> editBuffer)
        {
            this._addBuffer = addBuffer;
            this._deleteBuffer = deleteBuffer;
            this._updateBuffer = editBuffer;
        }

        /// <summary>Clears all the items in AddBuffer, EditBuffer and DeleteBuffer.</summary>
        /// <returns>None.</returns>
        public void Clear()
        {
            _addBuffer.Clear();
            _deleteBuffer.Clear();
            _updateBuffer.Clear();
        }

        /// <summary>Gets the dictionary buffer with the features to be added.</summary>
        /// <returns>dictionary buffer with the features to be added.</returns>
        /// <remarks>It is recommended that use this dictionary for reviewing and not for adding new items. </remarks>
        public Dictionary<string, Feature> AddBuffer
        {
            get { return _addBuffer; }
        }

        /// <summary>Gets the dictionary buffer with the features to be deleted.</summary>
        /// <returns>Dictionary buffer with the features to be deleted.</returns>
        /// <remarks>It is recommended that use this dictionary for reviewing and not for adding new items. </remarks>
        public Collection<string> DeleteBuffer
        {
            get { return _deleteBuffer; }
        }

        /// <summary>Gets the dictionary buffer with the features to be updated.</summary>
        /// <returns>Dictionary buffer with the features to be updated.</returns>
        public Dictionary<string, Feature> EditBuffer
        {
            get { return _updateBuffer; }
        }

        /// <summary>Adds a feature to the transaction buffer.</summary>
        /// <returns>None</returns>
        /// <remarks>None</remarks>
        /// <param name="feature">Feature to add to the transaction buffer.</param>
        public void AddFeature(Feature feature)
        {
            _addBuffer.Add(feature.Id, feature);
        }

        /// <summary>Adds a shape to the transaction buffer.</summary>
        /// <returns>None</returns>
        /// <remarks>None</remarks>
        /// <param name="baseShape">Shape to add to the transaction buffer. </param>
        public void AddFeature(BaseShape baseShape)
        {
            Validators.CheckParameterIsNotNull(baseShape, "baseShape");

            _addBuffer.Add(baseShape.Id, new Feature(baseShape));
        }

        /// <summary>Adds a shape to the transaction buffer.</summary>
        /// <returns>None</returns>
        /// <remarks>None</remarks>
        /// <param name="baseShape">Shape to add to the transaction buffer. </param>
        /// <param name="columnValues">Column values to add.</param>
        public void AddFeature(BaseShape baseShape, Dictionary<string, string> columnValues)
        {
            Validators.CheckParameterIsNotNull(baseShape, "baseShape");

            _addBuffer.Add(baseShape.Id, new Feature(baseShape, columnValues));
        }

        /// <summary>Adds a placeholder to represent a Feature to be deleted.</summary>
        /// <returns>None</returns>
        /// <remarks>Does not remove a feature from the TransactionBuffer immediately. Instead it adds it "to
        /// be deleted list". When the TransactionBuffer is processed we know what records need to be deleted.</remarks>
        /// <param name="featureId">Unique Id for the specific Feature to delete.</param>
        public void DeleteFeature(string featureId)
        {
            if (_addBuffer.ContainsKey(featureId))
            {
                _addBuffer.Remove(featureId);
            }
            else if (_deleteBuffer.Contains(featureId))
            {
                return;
            }
            else
            {
                _deleteBuffer.Add(featureId);
                if (_updateBuffer.ContainsKey(featureId))
                {
                    _updateBuffer.Remove(featureId);
                }
            }
        }

        /// <summary>Adds a Feature to be edited.</summary>
        /// <returns>None</returns>
        /// <remarks>None</remarks>
        /// <param name="feature">Feature to be edited.</param>
        public void EditFeature(Feature feature)
        {
            string id = feature.Id;
            object tag = feature.Tag;
            if (_deleteBuffer.Contains(id))
            {
                return;
            }
            else if (_addBuffer.ContainsKey(id))
            {
                _addBuffer[id] = feature;
            }
            else if (_updateBuffer.ContainsKey(id))
            {
                feature = new Feature(feature.GetWellKnownBinary(), id, feature.ColumnValues);
                feature.Tag = tag;
                _updateBuffer[id] = feature;
            }
            else
            {
                feature = new Feature(feature.GetWellKnownBinary(), id, feature.ColumnValues);
                feature.Tag = tag;
                _updateBuffer.Add(id, feature);
            }
        }

        /// <summary>Adds a shape to be edited.</summary>
        /// <returns>None</returns>
        /// <remarks>None</remarks>
        /// <param name="baseShape">Shape to be edited. 
        /// The shape ID needs to be the same as the feature ID.</param>
        public void EditFeature(BaseShape baseShape)
        {
            EditFeature(new Feature(baseShape));
        }

        /// <summary>Adds a shape to be edited.</summary>
        /// <returns>None</returns>
        /// <remarks>None</remarks>
        /// <param name="baseShape">Shape to be edited. 
        /// The shape ID needs to be the same as the feature ID.</param>
        /// <param name="columnValues">Column values to be updated. 
        /// The shape ID should be the same as the feature you are going to update.</param>
        public void EditFeature(BaseShape baseShape, Dictionary<string, string> columnValues)
        {
            EditFeature(new Feature(baseShape, columnValues));
        }
    }
}

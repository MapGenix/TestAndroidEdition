#define DEBUG

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;
using DotSpatial.Topology.Utilities;
using Mapgenix.Shapes;

namespace Mapgenix.FeatureSource
{
    /// <summary>Feature source for vector data from OGR</summary>
    /// <remarks>Use this class for vector formats such as tab files, KML, GML etc. It uses the library GDAL.</remarks>
    [Serializable]
	public class VectorFeatureSource : BaseFeatureSource
	{
		public string PathFilename { get; set; }

        /// <summary>Default constructor.</summary>
        public VectorFeatureSource(string filePath)
		{
			PathFilename = filePath;
		}

        /// <summary>Opens VectorFeatureSource to get it ready to use.</summary>
        /// <returns>None</returns>
		protected override void OpenCore()
		{
			Validators.CheckParameterIsNotNullOrEmpty(PathFilename, "PathFilename");
		}

        /// <summary>Returns collection of all the features in the VectorFeatureSource.</summary>
        /// <returns>Collection of all the features in the VectorFeatureSource
        /// </returns>
        /// <param name="returningColumnNames">Field names of the column data to return with each Feature.</param>
        protected override Collection<Feature> GetAllFeaturesCore(IEnumerable<string> returningColumnNames)
		{
			Collection<Feature> featureList;
			using (var reader = new OgrWrapper(PathFilename))
			{
				featureList = ExtractFeatures(returningColumnNames, reader);
			}
			return featureList;
		}

        /// <summary>Returns a collection of features inside of the target rectangle.</summary>
        /// <returns>Collection of features inside of the target rectangle.</returns>
        /// <param name="boundingBox">Target boundingBox.</param>
        /// <param name="returningColumnNames">Column names.</param>
		protected override Collection<Feature> GetFeaturesInsideBoundingBoxCore(RectangleShape boundingBox,
			IEnumerable<string> returningColumnNames)
		{
			Validators.CheckFeatureSourceIsOpen(IsOpen);
			Validators.CheckParameterIsNotNull(boundingBox, "boundingBox");
			Validators.CheckParameterIsValid(boundingBox, "boundingBox");
			Collection<Feature> featureList;

			using (var reader = new OgrWrapper(PathFilename))
			{
				reader.OgrLayer.SetSpatialFilterRect(boundingBox.UpperLeftPoint.X, boundingBox.UpperLeftPoint.Y,
					boundingBox.LowerRightPoint.X, boundingBox.LowerRightPoint.Y);
				featureList = ExtractFeatures(returningColumnNames, reader);
			}
			return featureList;
		}

        /// <summary>Returns the bounding box encompassing all of the features in VectorFeatureSource.</summary>
        /// <returns>Bounding box encompassing all of the features in VectorFeatureSource.</returns>
        protected override RectangleShape GetBoundingBoxCore()
		{
			var minX = double.MaxValue;
			var minY = double.MaxValue;
			var maxX = double.MinValue;
			var maxY = double.MinValue;
			using (var reader = new OgrWrapper(PathFilename))
			{
				var wkbReader = new WkbReader();
				while (reader.Read())
				{
					var wkbGeometry = (byte[])reader["Geometry"];
					if (wkbGeometry == null)
						continue;
					var geometry = wkbReader.Read(wkbGeometry);
					if (geometry == null)
						continue;
					minX = Math.Min(geometry.Envelope.Minimum.X, minX);
					minY = Math.Min(geometry.Envelope.Minimum.Y, minY);
					maxX = Math.Max(geometry.Envelope.Maximum.X, maxX);
					maxY = Math.Max(geometry.Envelope.Maximum.Y, maxY);
				}
			}
			return new RectangleShape(minX, maxY, maxX, minY);
		}

        /// <summary>Returns the collection of columns available for the feature source and caches them.</summary>
        /// <returns>Collection of columns available for the feature source.</returns>
        protected override Collection<FeatureSourceColumn> GetColumnsCore()
		{
			Validators.CheckFeatureSourceIsOpen(IsOpen);

			var returnValues = new Collection<FeatureSourceColumn>();
			var columnNames = new Collection<string>();
			using (var reader = new OgrWrapper(PathFilename))
			{
				foreach (DataRow row in reader.GetSchemaTable().Rows)
				{
					if (row["ColumnOrdinal"].ToString() != "0")
					{
						var columnName = ColumnFilter.GetColumnNameAlias(row["ColumnName"].ToString(), columnNames);
						columnNames.Add(columnName);
						var tmp = new FeatureSourceColumn(columnName, row["DataType"].ToString(), int.Parse(row["ColumnSize"].ToString()));
						returnValues.Add(tmp);
					}
				}
			}
			return returnValues;
		}

        /// <summary>Returns number of records in the vectorFeatureSource.</summary>
        /// <returns>Number of records in the VectorFeatureSource.</returns>
        protected override int GetCountCore()
		{
			int count;
			using (var reader = new OgrWrapper(PathFilename))
			{
				count = reader.FeaturesCount;
			}
			return count;
		}

		private Collection<Feature> ExtractFeatures(IEnumerable<string> returningColumnNames, OgrWrapper reader)
		{
			var featureList = new Collection<Feature>();
			var culture = CultureInfo.InvariantCulture;
			Dictionary<string, string> dictionary = null;
			var isColumnEmpty = ShapeFileFeatureSourceHelper.CheckIsNull(returningColumnNames);

			for (var i = 0; i <= reader.FeaturesCount; i++)
			{
				if (reader.Read())
				{
					if (!isColumnEmpty)
					{
						dictionary = GetDataFromDbf(i.ToString(culture), returningColumnNames, reader);
					}

					var wkbGeometry = (byte[])reader["Geometry"];
					if (wkbGeometry != null)
					{
						featureList.Add(new Feature(wkbGeometry, i.ToString(culture), dictionary));
					}
				}
			}
			return featureList;
		}

		private Dictionary<string, string> GetDataFromDbf(string id, IEnumerable<string> returningColumnNames,
			OgrWrapper reader)
		{
			Validators.CheckParameterIsNotNull(returningColumnNames, "columnNames");
			Validators.CheckParameterIsNotNull(id, "id");

			var dictionary = new Dictionary<string, string>();

			var feature = reader.OgrLayer.GetFeature(Convert.ToInt32(id, CultureInfo.InvariantCulture));
			foreach (var columnName in returningColumnNames)
			{
				var value = feature.GetFieldAsString(columnName);
				dictionary.Add(columnName, value);
			}
			return dictionary;
		}

	}
}
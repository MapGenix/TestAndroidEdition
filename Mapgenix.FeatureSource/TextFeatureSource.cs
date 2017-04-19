using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DotSpatial.Topology.Utilities;
using Mapgenix.Shapes;

namespace Mapgenix.FeatureSource
{
    public class TextFeatureSource : BaseFeatureSource
	{
		Stopwatch sw = new Stopwatch();
		#region Private fields

		private DataTable _dataTable;
		private TextFileSettings _settings;

		#endregion

		#region Properties

		public string PathFilename { get; set; }

		#endregion

		#region Constructors

		public TextFeatureSource(DataTable dataTable, TextFileSettings settings)
		{
			_dataTable = dataTable;
			_settings = settings;
		}

		#endregion

		#region FeatureSource implementation

		protected override Collection<Feature> GetAllFeaturesCore(IEnumerable<string> returningColumnNames)
		{
			return ExtractFeatures(returningColumnNames);
		}

		protected override Collection<Feature> GetFeaturesInsideBoundingBoxCore(RectangleShape boundingBox,
			IEnumerable<string> returningColumnNames)
		{
			Validators.CheckFeatureSourceIsOpen(IsOpen);
			Validators.CheckParameterIsNotNull(boundingBox, "boundingBox");
			Validators.CheckParameterIsValid(boundingBox, "boundingBox");
			List<Feature> list = new List<Feature>(_dataTable.Rows.Count);

			Collection<Feature> allRecordsInSource = GetAllFeaturesCore(returningColumnNames);
		
			allRecordsInSource.AsParallel().ForAll(feature =>
			{
				var featureBoundingBox = feature.GetBoundingBox();

				if (!boundingBox.IsDisjointed(featureBoundingBox))
				{
					list.Add(feature);
				}
			});

			return new Collection<Feature>(list);
		}

		protected override RectangleShape GetBoundingBoxCore()
		{
			var minX = double.MaxValue;
			var minY = double.MaxValue;
			var maxX = double.MinValue;
			var maxY = double.MinValue;
			var wkbReader = new WkbReader();
			foreach (DataRow row in _dataTable.Rows)
			{
				var wktGeometry = (string)row[_settings.GeometryFields[0]];
				if (wktGeometry == null)
					continue;

				Feature feature = new Feature(wktGeometry);

				byte[] wellKnownBinary = feature.GetWellKnownBinary();
				var geometry = wkbReader.Read(wellKnownBinary);
				if (geometry == null)
					continue;
				minX = Math.Min(geometry.Envelope.Minimum.X, minX);
				minY = Math.Min(geometry.Envelope.Minimum.Y, minY);
				maxX = Math.Max(geometry.Envelope.Maximum.X, maxX);
				maxY = Math.Max(geometry.Envelope.Maximum.Y, maxY);
			}
			return new RectangleShape(minX, maxY, maxX, minY);
		}

		protected override Collection<FeatureSourceColumn> GetColumnsCore()
		{
			Validators.CheckFeatureSourceIsOpen(IsOpen);

			var returnValues = new Collection<FeatureSourceColumn>();
			var columnNames = new Collection<string>();
			foreach (DataRow row in _dataTable.Rows)
			{
				if (row["ColumnOrdinal"].ToString() != "0")
				{
					var columnName = ColumnFilter.GetColumnNameAlias(row["ColumnName"].ToString(), columnNames);
					columnNames.Add(columnName);
					var tmp = new FeatureSourceColumn(columnName, row["DataType"].ToString(), int.Parse(row["ColumnSize"].ToString()));
					returnValues.Add(tmp);
				}
			}
			return returnValues;
		}

		protected override int GetCountCore()
		{
			return _dataTable.Rows.Count;
		}

		#endregion

		#region Private methods

		private Collection<Feature> ExtractFeatures(IEnumerable<string> returningColumnNames)
		{
			var culture = CultureInfo.InvariantCulture;
			Dictionary<string, string> dictionary = null;
			var isColumnEmpty = ShapeFileFeatureSourceHelper.CheckIsNull(returningColumnNames);

			List<Feature> list = new List<Feature>(_dataTable.Rows.Count);

			sw.Restart();
			int index = 0;
			Parallel.ForEach(_dataTable.AsEnumerable(), row =>
			{
				var currIndex = Interlocked.Increment(ref index);
				if (!isColumnEmpty)
				{
					dictionary = GetDataFromFile(currIndex.ToString(culture), returningColumnNames, row);
				}

				string wkbGeometry = (string)row[_settings.GeometryFields[0]];
				if (wkbGeometry != null)
				{
					var feature = new Feature(wkbGeometry, currIndex.ToString(culture), dictionary);
					list.Add(feature);

				}
			});


			sw.Stop();

			TimeSpan ts = sw.Elapsed;
			string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds,
				ts.Milliseconds / 10);
			Debug.WriteLine(string.Format("RunTime Extracting text features {0}", elapsedTime));

			return new Collection<Feature>(list);
			
		}

		private Dictionary<string, string> GetDataFromFile(string id, IEnumerable<string> returningColumnNames, DataRow row)
		{
			Validators.CheckParameterIsNotNull(returningColumnNames, "columnNames");
			Validators.CheckParameterIsNotNull(id, "id");

			var dictionary = new Dictionary<string, string>();
			foreach (var columnName in returningColumnNames)
			{
				var value = row[columnName].ToString();
				dictionary.Add(columnName, value);
			}
			return dictionary;
		}

		#endregion
	}
}

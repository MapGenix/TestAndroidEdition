using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.OleDb;
using System.Globalization;
using System.IO;
using Mapgenix.FeatureSource.Properties;
using Mapgenix.Index;
using Mapgenix.Shapes;
using Mapgenix.Utils;
using System.Linq;


namespace Mapgenix.FeatureSource
{
    /// <summary>FeatureSource for ESRI shapefile.</summary>
    [Serializable]
	public class ShapeFileFeatureSource : BaseFeatureSource
	{
		[NonSerialized]
        internal GeoDbf _dBaseEngine;

		private FileAccess _fileAccess;

		[NonSerialized] private RtreeSpatialIndex _rTreeIndex;

		[NonSerialized] internal ShapeFile _shapeFile;

		private ReadWriteMode _shapeFileReadWriteMode;

        /// <summary>Returns the path and file of the shapefile to use.</summary>
        /// <returns>Path and file of the shapefile to use.</returns>
        public string ShapePathFileName { get; set; }

        /// <summary>Gets and sets the path and file of the spatial index to use.</summary>
        /// <returns>Path and file of the spatial index to use.</returns>
		public string IndexPathFileName { get; set; }

		public ReadWriteMode ReadWriteMode
		{
			get { return _shapeFileReadWriteMode; }
			set { _shapeFileReadWriteMode = value; }
		}

        /// <summary>Returns true if ShapefileFeatureSource allows editing or false if is read only.</summary>
        /// <returns>True if ShapefileFeatureSource allows editing or false if is read only.</returns>
       public override bool IsEditable
		{
			get { return true; }
		}

       /// <summary>Can excute sql query or not.
       /// If it is false, it throws exception when callling the APIs:ExecuteScalar,ExecuteNonQuery,ExecuteQuery.</summary>
		protected override bool CanExecuteSqlQueryCore
		{
			get { return true; }
		}

        /// <summary>Gets and sets if shapefile requires spatial index when reading data. The default value is true.</summary>
		public bool RequireIndex { get; set; }

        /// <summary>Gets and sets the maximum number of records that can be drawn.</summary>
		public int MaxRecordsToDraw { get; set; }

        /// <summary>Allows to pass in own stream representing the files.</summary>
        public event EventHandler<StreamLoadingEventArgs> StreamLoading;

        /// <summary>Allows to raise the StreamLoading event.</summary>
        /// <returns>None</returns>
        /// <param name="e">Event arguments for the StreamLoading event.</param>
		protected virtual void OnStreamLoading(StreamLoadingEventArgs e)
		{
			var handler = StreamLoading;

			if (handler != null)
			{
				handler(this, e);
			}
		}

        /// <summary>Gets the dbf columns out from the shapefile featureSource.</summary>
        /// <returns>Collection of dbfColumns in the shapefile FeatureSource.</returns>
		public Collection<DbfColumn> GetDbfColumns()
		{
			Validators.CheckFeatureSourceIsOpen(IsOpen);

			var returnValues = new Collection<DbfColumn>();

			for (var i = 1; i <= _dBaseEngine.ColumnCount; i++)
			{
				var dbfColumn = _dBaseEngine.GetColumn(i);

				returnValues.Add(dbfColumn);
			}

			return returnValues;
		}

        /// <summary>Adds a new Integer column to the DBF file associated with the shapefile.</summary>
        /// <returns>None</returns>
        /// <remarks>None</remarks>
        /// <param name="columnName">Column to add.</param>
        /// <param name="length">Length of the integer.</param>
		public void AddColumnInteger(string columnName, int length)
		{
			Validators.CheckFeatureSourceIsOpen(IsOpen);
			Validators.CheckParameterIsNotNull(columnName, "columnName");
			Validators.CheckIfInputValueIsBiggerThan(length, "length", 0, RangeCheckingInclusion.ExcludeValue);

			_dBaseEngine.AddIntField(columnName, length);
		}

        /// <summary>Gets data directly from the DBF file associated with the shapefile.</summary>
        /// <overloads>Passes an Id and a column name.</overloads>
        /// <returns>Data directly from the DBF file associated with the shapefile.</returns>
        /// <param name="id">Id for the Feature to find.</param>
        /// <param name="columnName">Column name.</param>
		public string GetDataFromDbf(string id, string columnName)
		{
			Validators.CheckFeatureSourceIsOpen(IsOpen);
			Validators.CheckParameterIsNotNull(columnName, "columnName");
			Validators.CheckIfInputValueIsBiggerThan(Convert.ToInt32(id, CultureInfo.InvariantCulture), "id", 1,
				RangeCheckingInclusion.IncludeValue);

			return _dBaseEngine.ReadFieldAsString(Convert.ToInt32(id, CultureInfo.InvariantCulture), columnName);
		}

        /// <summary>Gets data directly from the DBF file associated with the shapefile.</summary>
        /// <returns>Dictionary holding all of the values from the DBF for the Id specified.</returns>
        /// <param name="id">Id of the Feature.</param>
		public Dictionary<string, string> GetDataFromDbf(string id)
		{
			Validators.CheckFeatureSourceIsOpen(IsOpen);
			Validators.CheckIfInputValueIsBiggerThan(Convert.ToInt32(id, CultureInfo.InvariantCulture), "id", 1,
				RangeCheckingInclusion.IncludeValue);

			return _dBaseEngine.ReadRecordAsString(Convert.ToInt32(id, CultureInfo.InvariantCulture));
		}

        /// <summary>Gets data directly from the DBF file associated with the shapefile.</summary>
		public Dictionary<string, string> GetDataFromDbf(string id, IEnumerable<string> returningColumnNames)
		{
			Validators.CheckFeatureSourceIsOpen(IsOpen);
			Validators.CheckParameterIsNotNull(returningColumnNames, "columnNames");
			Validators.CheckParameterIsNotNull(id, "id");
			Validators.CheckIfInputValueIsBiggerThan(Convert.ToInt32(id, CultureInfo.InvariantCulture), "id", 1,
				RangeCheckingInclusion.IncludeValue);

			var dictionary = new Dictionary<string, string>();

			foreach (var columnName in returningColumnNames)
			{
				var value = GetDataFromDbf(id, columnName);
				dictionary.Add(columnName, value);
			}

			return dictionary;
		}

        /// <summary>Gets data directly from the DBF file associated with the shapefile.</summary>
		public Collection<Dictionary<string, string>> GetDataFromDbf(IEnumerable<string> ids, IEnumerable<string> columnNames)
		{
			Validators.CheckFeatureSourceIsOpen(IsOpen);
			Validators.CheckParameterIsNotNull(ids, "ids");
			Validators.CheckParameterIsNotNull(columnNames, "columnNames");

			var returnValue = new Collection<Dictionary<string, string>>();

			Dictionary<string, string> tmp = null;
			foreach (var id in ids)
			{
				tmp = GetDataFromDbf(id, columnNames);
				returnValue.Add(tmp);
			}

			return returnValue;
		}

        /// <summary>Updates data in the DBF file associated with the shapefile.</summary>
        /// <returns>None</returns>
        /// <remarks>None</remarks>
        /// <param name="id">Id of the feature to update.</param>
        /// <param name="columnName">Column name to update.</param>
        /// <param name="value">Value to set.</param>
		public void UpdateDbfData(string id, string columnName, string value)
		{
			Validators.CheckFeatureSourceIsOpen(IsOpen);
			Validators.CheckParameterIsNotNull(columnName, "columnName");
			Validators.CheckParameterIsNotNull(value, "value");
			Validators.CheckParameterIsNotNull(id, "id");
			Validators.CheckIfInputValueIsInRange(Convert.ToInt32(id, CultureInfo.InvariantCulture), "id", 1,
				RangeCheckingInclusion.IncludeValue, _dBaseEngine.RecordCount, RangeCheckingInclusion.IncludeValue);

			_dBaseEngine.WriteField(Convert.ToInt32(id, CultureInfo.InvariantCulture), columnName, value);
			_dBaseEngine.Flush();
		}

        /// <summary>Executes a SQL query returning the number of records affected.</summary>
        /// <returns>Number of records affected by the SQL query.</returns>
        /// <param name="sqlStatement">SQL statement to execute.</param>
		protected override int ExecuteNonQueryCore(string sqlStatement)
		{
			Validators.CheckFeatureSourceIsOpen(IsOpen);
			Validators.CheckParameterIsNotNull(sqlStatement, "sqlStatement");
			Validators.CheckSqlStatementIsSupported(sqlStatement);

			Validators.CheckFeatureSourceCanExecuteSqlQuery(CanExecuteSqlQuery);

			OleDbCommand oleDbCommand = null;
			var result = -1;
			try
			{
				_dBaseEngine.Close();

				sqlStatement = ReplaceTableName(sqlStatement);

				oleDbCommand = GetOleDbCommand(sqlStatement);
				result = oleDbCommand.ExecuteNonQuery();

				_dBaseEngine.Open();
			}
			finally
			{
				if (oleDbCommand != null)
				{
					oleDbCommand.Dispose();
				}
			}

			return result;
		}

        /// <summary>Returns a DataTable based on the SQL statement provided.</summary>
        /// <returns>DataTable based on the SQL statement you provided.</returns>
        /// <param name="sqlStatement">SQL statement to execute.</param>
		protected override DataTable ExecuteQueryCore(string sqlStatement)
		{
			Validators.CheckFeatureSourceIsOpen(IsOpen);
			Validators.CheckParameterIsNotNull(sqlStatement, "sqlStatement");

			Validators.CheckFeatureSourceCanExecuteSqlQuery(CanExecuteSqlQuery);

			var dataTable = new DataTable {Locale = CultureInfo.InvariantCulture};
			OleDbCommand oleDbCommand = null;
			OleDbDataAdapter adapter = null;
			try
			{
				_dBaseEngine.Close();

				sqlStatement = ReplaceTableName(sqlStatement);
				oleDbCommand = GetOleDbCommand(sqlStatement);

				adapter = new OleDbDataAdapter(oleDbCommand);
				adapter.Fill(dataTable);

				_dBaseEngine.Open();
			}
			finally
			{
				if (oleDbCommand != null)
				{
					oleDbCommand.Dispose();
				}
				if (adapter != null)
				{
					adapter.Dispose();
				}
			}

			return dataTable;
		}

		private OleDbCommand GetOleDbCommand(string sqlStatement)
		{
			var connection =
				new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + Path.GetDirectoryName(ShapePathFileName) +
				                    "\\;Extended Properties=dBASE IV;User ID=Admin;Password=");

			if (connection.State != ConnectionState.Open)
			{
				connection.Open();
			}

			return new OleDbCommand(sqlStatement, connection);
		}

        /// <summary>Opens the ShapefileFeatureSource to have it ready to use.</summary>
        /// <returns>None</returns>
		protected override void OpenCore()
		{
			_fileAccess = FileAccess.Read;
			var rTreeFileAccess = RtreeSpatialIndexReadWriteMode.ReadOnly;
			if (_shapeFileReadWriteMode == ReadWriteMode.ReadWrite)
			{
				_fileAccess = FileAccess.ReadWrite;
				rTreeFileAccess = RtreeSpatialIndexReadWriteMode.ReadWrite;
			}

			OpenShapeFile();

			OpenDbfFile();

			OpenRtree(rTreeFileAccess);
		}

		private void OpenRtree(RtreeSpatialIndexReadWriteMode rTreeFileAccess)
		{
			if (_rTreeIndex == null)
			{
				_rTreeIndex = new RtreeSpatialIndex(IndexPathFileName, rTreeFileAccess);
			}

			_rTreeIndex.StreamLoading += ShapeFileFeatureSource_StreamLoading;
			_rTreeIndex.IdsEngine.StreamLoading += ShapeFileFeatureSource_StreamLoading;

			if (RequireIndex)
			{
				_rTreeIndex.Open();
			}

			if (!_rTreeIndex.HasIdx && RequireIndex)
			{
				throw new InvalidOperationException(ExceptionDescription.IndexFileNotExisted);
			}
		}

		private void OpenDbfFile()
		{
			if (_dBaseEngine == null)
			{
				_dBaseEngine = new GeoDbf();
			}

			_dBaseEngine.StreamLoading += ShapeFileFeatureSource_StreamLoading;

			var dbfPahtFileName = Path.ChangeExtension(ShapePathFileName, ".dbf");
			_dBaseEngine.PathFileName = dbfPahtFileName;
			var geoDbfReadWriteMode = DbfReadWriteMode.ReadOnly;

			if (_shapeFileReadWriteMode == ReadWriteMode.ReadWrite)
			{
				geoDbfReadWriteMode = DbfReadWriteMode.ReadWrite;
			}
			_dBaseEngine.ReadWriteMode = geoDbfReadWriteMode;
			_dBaseEngine.Open();
		}


		private void OpenShapeFile()
		{
			if (_shapeFile == null)
			{
				_shapeFile = new ShapeFile();
			}

			_shapeFile.StreamLoading += ShapeFileFeatureSource_StreamLoading;
			_shapeFile.Shx.StreamLoading += ShapeFileFeatureSource_StreamLoading;

			_shapeFile.PathFilename = ShapePathFileName;
			_shapeFile.Open(_fileAccess);
		}

        /// <summary>Closes the ShapefileFeatureSource.</summary>
        /// <returns>None</returns>
        protected override void CloseCore()
		{
			if (_shapeFile != null)
			{
				_shapeFile.Close();
			}
			if (_dBaseEngine != null)
			{
				_dBaseEngine.Close();
			}
			if (_rTreeIndex != null)
			{
				_rTreeIndex.Close();
				_rTreeIndex = null;
			}
		}

        /// <summary>Commits the existing transaction to its underlying source of data.</summary>
        /// <returns>TransactionResult with the status of the committed transaction including how many successfull updates,
        /// adds, and deletes and any error encountered.</returns>
        /// <param name="transactions">Transaction buffer encapsulating all of the actions (adds, edits and deletes) making up the
        /// transaction.</param>
		protected override TransactionResult CommitTransactionCore(TransactionBuffer transactions)
		{
			Validators.CheckParameterIsNotNull(transactions, "transactions");
			Validators.CheckShapeIsWriteable(_shapeFileReadWriteMode);
			Validators.CheckParameterIsNotNull(transactions, "transactionBuffer");
			Validators.CheckShapeFileIsEditable(_shapeFile.GetShapeFileType());
			
			var transactionResult = new TransactionResult();

			var addBuffer = transactions.AddBuffer;
			foreach (var feature in addBuffer.Values)
			{
				try
				{
					AddRecord(feature);
					transactionResult.TotalSuccessCount++;
				}
				catch (Exception ex)
				{
					transactionResult.TotalFailureCount++;
					transactionResult.FailureReasons.Add(feature.Id, ex.ToString());
				}
			}

			var deleteBuffer = transactions.DeleteBuffer;
			var deleteBufferIntergers =
				deleteBuffer.Select(id => Convert.ToInt32(id, CultureInfo.InvariantCulture)).ToList();

			deleteBufferIntergers.Sort();
			deleteBufferIntergers.Reverse();

			foreach (var id in deleteBufferIntergers)
			{
				try
				{
					DeleteRecord(id);
					transactionResult.TotalSuccessCount++;
				}
				catch (Exception ex)
				{
					transactionResult.TotalFailureCount++;
					transactionResult.FailureReasons.Add(id.ToString(CultureInfo.InvariantCulture), ex.ToString());
				}
			}

			var editBuffer = transactions.EditBuffer;
			foreach (var feature in editBuffer.Values)
			{
				try
				{
					UpdateRecord(feature);
					transactionResult.TotalSuccessCount++;
				}
				catch (Exception ex)
				{
					transactionResult.TotalFailureCount++;
					transactionResult.FailureReasons.Add(feature.Id, ex.ToString());
				}
			}

			transactionResult.TransactionResultStatus = transactionResult.TotalFailureCount == 0
				? TransactionResultStatus.Success
				: TransactionResultStatus.Failure;

			_shapeFile.Flush();
			_dBaseEngine.Flush();

			if (_rTreeIndex.HasIdx)
			{
				_rTreeIndex.Flush();
			}

			return transactionResult;
		}

        /// <summary>Returns a collection with all the features in the ShapefileFeatureSource.</summary>
        /// <returns>Collection with all the features in the ShapefileFeatureSource.</returns>
        /// <param name="returningColumnNames">Field names of the column data to return with each Feature.</param>
		protected override Collection<Feature> GetAllFeaturesCore(IEnumerable<string> returningColumnNames)
		{
			var returnValues = new Collection<Feature>();
			var recordCount = _shapeFile.GetRecordCount();
			var culture = CultureInfo.InvariantCulture;
			Dictionary<string, string> dictionary = null;
			var isColomnEmpty = ShapeFileFeatureSourceHelper.CheckIsNull(returningColumnNames);

			for (var i = 1; i <= recordCount; i++)
			{
				if (!isColomnEmpty)
				{
					dictionary = GetDataFromDbf(i.ToString(culture), returningColumnNames);
				}

				var wkb = _shapeFile.ReadRecord(i);
				returnValues.Add(new Feature(wkb, i.ToString(culture), dictionary));
			}

			return returnValues;
		}

        /// <summary>Returns the columns available for the ShapefileFeatureSource.</summary>
        /// <returns>Columns available in the ShapefileFeatureSource.</returns>
		protected override Collection<FeatureSourceColumn> GetColumnsCore()
		{
			Validators.CheckFeatureSourceIsOpen(IsOpen);

			var returnValues = new Collection<FeatureSourceColumn>();
			var columnNames = new Collection<string>();
			for (var i = 1; i <= _dBaseEngine.ColumnCount; i++)
			{
				
				var dbfColumn = _dBaseEngine.GetColumn(i);
				var columnName = ColumnFilter.GetColumnNameAlias(dbfColumn.ColumnName, columnNames);
				columnNames.Add(columnName);
				var tmp = new FeatureSourceColumn(columnName, dbfColumn.ColumnType.ToString(), dbfColumn.Length);
				returnValues.Add(tmp);
			}

			return returnValues;
		}

        /// <summary>Returns the bounding box encompassing all of the features in the ShapefileFeatureSource.</summary>
        /// <returns>Bounding box encompassing all of the features in the ShapefileFeatureSource.</returns>
		protected override RectangleShape GetBoundingBoxCore()
		{
			Validators.CheckFeatureSourceIsOpen(IsOpen);

			return _shapeFile.GetShapeBoundingBox();
		}

        /// <returns>Returns collection of Features of the ShapefileFeatureSource outside a bounding box.</returns>
        /// <summary>Collection of Features of the ShapefileFeatureSource outside a bounding box.</summary>
        /// <param name="boundingBox">Bounding box used to find the features outside.</param>
        /// <param name="returningColumnNames">Field names of the column data to return with each Feature.</param>
		protected override Collection<Feature> GetFeaturesOutsideBoundingBoxCore(RectangleShape boundingBox,
			IEnumerable<string> returningColumnNames)
		{
			Validators.CheckFeatureSourceIsOpen(IsOpen);
			Validators.CheckParameterIsNotNull(boundingBox, "boungingBox");
			Validators.CheckShapeIsValidForOperation(boundingBox);

			var returnValues = new Collection<Feature>();

			if (_rTreeIndex.HasIdx)
			{
				
				var fullBoundingBox = new RectangleShape(double.MinValue, double.MaxValue, double.MaxValue,
					double.MinValue);
				var allIds = _rTreeIndex.GetFeatureIdsIntersectingBoundingBox(fullBoundingBox);

				var insideIds = _rTreeIndex.GetFeatureIdsIntersectingBoundingBox(boundingBox);
				var containingIds = _rTreeIndex.GetFeatureIdsWithinBoundingBox(boundingBox);

				var resultIds = new Collection<string>();
				foreach (var id in allIds.Where(id => !insideIds.Contains(id) && !containingIds.Contains(id)))
				{
					resultIds.Add(id);
				}

				foreach (var feature in resultIds.Select(id => GetFeatureById(id, returningColumnNames)))
				{
					returnValues.Add(feature);
				}
			}
			else
			{
				returnValues = base.GetFeaturesOutsideBoundingBoxCore(boundingBox, returningColumnNames);
			}

			return returnValues;
		}

        /// <returns>Returns collection of Features of the ShapefileFeatureSource inside a bounding box.</returns>
        /// <summary>Collection of Features of the ShapefileFeatureSource inside a bounding box.</summary>
        /// <param name="boundingBox">Bounding box used to find the features inside.</param>
        /// <param name="returningColumnNames">Field names of the column data to return with each Feature.</param>
		protected override Collection<Feature> GetFeaturesInsideBoundingBoxCore(RectangleShape boundingBox,
			IEnumerable<string> returningColumnNames)
		{
			Validators.CheckFeatureSourceIsOpen(IsOpen);
			Validators.CheckParameterIsNotNull(boundingBox, "boungingBox");
			Validators.CheckShapeIsValidForOperation(boundingBox);

			var returnValues = new Collection<Feature>();
			if (_rTreeIndex.HasIdx)
			{
				var ids = _rTreeIndex.GetFeatureIdsIntersectingBoundingBox(boundingBox);
				var isColumnEmpty = ShapeFileFeatureSourceHelper.CheckIsNull(returningColumnNames);
				foreach (var id in ids)
				{
					var tmpId = Convert.ToInt32(id, CultureInfo.InvariantCulture);
					var wkb = _shapeFile.ReadRecord(tmpId);
					if (!isColumnEmpty)
					{
						var dict = GetDataFromDbf((tmpId).ToString(CultureInfo.InvariantCulture),
							returningColumnNames);
						returnValues.Add(new Feature(wkb, id, dict));
					}
					else
					{
						returnValues.Add(new Feature(wkb, id));
					}
				}
			}
			else
			{
				returnValues = base.GetFeaturesInsideBoundingBoxCore(boundingBox, returningColumnNames);
			}
			return returnValues;
		}

        /// <summary>Returns a collection of features used for drawing.</summary>
        /// <returns>Collection of features used for drawing.</returns>
        /// <param name="boundingBox">Bounding box of where to draw the features.</param>
        /// <param name="screenWidth">Width in screen pixels of the canvas to draw on.</param>
        /// <param name="screenHeight">Height in screen pixels of the canvas to draw on.</param>
        /// <param name="returningColumnNames">Field names of the column data to return with each Feature.</param>
		protected override Collection<Feature> GetFeaturesForDrawingCore(RectangleShape boundingBox, double screenWidth,
			double screenHeight, IEnumerable<string> returningColumnNames)
		{
			Validators.CheckFeatureSourceIsOpen(IsOpen);
			Validators.CheckParameterIsNotNull(returningColumnNames, "returningColumnNames");
			Validators.CheckParameterIsNotNull(boundingBox, "boundingBox");
			Validators.CheckParameterIsValid(boundingBox, "boundingBox");
			Validators.CheckValueIsBiggerThanZero(screenWidth, "screenWidth");
			Validators.CheckValueIsBiggerThanZero(screenHeight, "screenHeight");

			var returnValues = new Collection<Feature>();
			if (_rTreeIndex.HasIdx)
			{
				var recordIds = new List<int>();
				var tempIds = _rTreeIndex.GetFeatureIdsIntersectingBoundingBox(boundingBox);


				if (tempIds.Count > MaxRecordsToDraw && MaxRecordsToDraw != 0)
				{
					var drawFeatureInterval = tempIds.Count/MaxRecordsToDraw;
					for (var i = 0; i < tempIds.Count; i += drawFeatureInterval)
					{
						recordIds.Add(Convert.ToInt32(tempIds[i], CultureInfo.InvariantCulture));
					}
				}
				else
				{
					recordIds.AddRange(tempIds.Select(id => Convert.ToInt32(id, CultureInfo.InvariantCulture)));
				}

				tempIds.Clear();
				tempIds = null;

				var isColumnEmpty = ShapeFileFeatureSourceHelper.CheckIsNull(returningColumnNames);
				var shapefileType = _shapeFile.ShapeFileType;

				var args = new ProgressEventArgs();
				var featuresDrawn = 0;
				if (recordIds.Count < 50000 || shapefileType != ShapeFileType.Polygon)
				{
					foreach (var id in recordIds)
					{
						args = new ProgressEventArgs(featuresDrawn*100/recordIds.Count, null,
							recordIds.Count - featuresDrawn, featuresDrawn);
						OnDrawingProgressChanged(args);

						if (!args.Cancel)
						{
							var wkb = _shapeFile.ReadRecord(id);

							if (!isColumnEmpty)
							{
								var dict = GetDataFromDbf((id).ToString(CultureInfo.InvariantCulture),
									returningColumnNames);
								returnValues.Add(new Feature(wkb, id.ToString(CultureInfo.InvariantCulture), dict));
							}
							else
							{
								returnValues.Add(new Feature(wkb, id.ToString(CultureInfo.InvariantCulture)));
							}
						}
						else
						{
							break;
						}
						featuresDrawn++;
					}
				}
				else
				{
					var screenFactorX = screenWidth/boundingBox.Width;
					var screenFactorY = screenHeight/boundingBox.Height;
					var upperLeftPoint = boundingBox.UpperLeftPoint;

					foreach (var id in recordIds)
					{
						args = new ProgressEventArgs(featuresDrawn*100/recordIds.Count, null,
							recordIds.Count - featuresDrawn, featuresDrawn);
						OnDrawingProgressChanged(args);
						if (!args.Cancel)
						{
							var shapeBoundingBox = _shapeFile.GetBoundingBoxById(id);
							if (shapeBoundingBox != null)
							{
								byte[] wkb = null;
								wkb = ShapeFileFeatureSourceHelper.CheckBoundingBoxIsBigEnough(upperLeftPoint, shapeBoundingBox, screenFactorX,
									screenFactorY)
									? _shapeFile.ReadRecord(id)
									: ShapeFileFeatureSourceHelper.GetWellKnownBinaryForRectangle(shapeBoundingBox);

								if (!isColumnEmpty)
								{
									var dict = GetDataFromDbf((id).ToString(CultureInfo.InvariantCulture),
										returningColumnNames);
									returnValues.Add(new Feature(wkb, id.ToString(CultureInfo.InvariantCulture), dict));
								}
								else
								{
									returnValues.Add(new Feature(wkb, id.ToString(CultureInfo.InvariantCulture)));
								}
							}
						}
						else
						{
							break;
						}
						featuresDrawn++;
					}
				}
			}
			else
			{
				returnValues = base.GetFeaturesForDrawingCore(boundingBox, screenWidth, screenHeight, returningColumnNames);
			}

			return returnValues;
		}

        /// <summary>Returns a collection of features based on Ids.</summary>
        /// <returns>Collection of features based on Ids.</returns>
        /// <param name="ids">Ids uniquely identifying the features in the ShapefileFeatureSource.</param>
        /// <param name="returningColumnNames">Field names of the column data to return with each Feature.</param>
		protected override Collection<Feature> GetFeaturesByIdsCore(IEnumerable<string> ids,
			IEnumerable<string> returningColumnNames)
		{
			Validators.CheckFeatureSourceIsOpen(IsOpen);
			Validators.CheckParameterIsNotNull(ids, "ids");
			Validators.CheckParameterIdsIsIntergers(ids, "ids");
			Validators.CheckParameterIsNotNull(returningColumnNames, "returningColumnNames");

			var returnValues = new Collection<Feature>();
			var isColumnEmpty = ShapeFileFeatureSourceHelper.CheckIsNull(returningColumnNames);

			foreach (var id in ids)
			{
				var tmpId = Convert.ToInt32(id, CultureInfo.InvariantCulture);

				var wkb = _shapeFile.ReadRecord(tmpId);
				if (isColumnEmpty)
				{
					returnValues.Add(new Feature(wkb, id));
				}
				else
				{
					var dict = GetDataFromDbf((tmpId).ToString(CultureInfo.InvariantCulture),
						returningColumnNames);
					returnValues.Add(new Feature(wkb, id, dict));
				}
			}
			return returnValues;
		}

        /// <summary>Returns the number of records in the ShapefileFeatureSource.</summary>
        /// <returns>Number of records in the ShapefileFeatureSource.</returns>
		protected override int GetCountCore()
		{
			Validators.CheckFeatureSourceIsOpen(IsOpen);

			return _shapeFile.GetRecordCount();
		}

		private void ShapeFileFeatureSource_StreamLoading(object sender, StreamLoadingEventArgs e)
		{
			OnStreamLoading(e);
		}

		private void AddRecord(Feature feature)
		{
			var newId = (_shapeFile.GetRecordCount() + 1).ToString(CultureInfo.InvariantCulture);

			var targetShape = feature.GetShape();
			
			_shapeFile.AddRecord(targetShape);

			if (_rTreeIndex.HasIdx)
			{
				targetShape.Id = newId;
				_rTreeIndex.Add(targetShape);
			}

			if (feature.ColumnValues == null)
			{
				_dBaseEngine.AddEmptyRecord();
			}
			else
			{
				var fieldValues = FilterFieldValue(feature.ColumnValues);
				if (fieldValues.Count == 0)
				{
					_dBaseEngine.AddEmptyRecord();
				}
				else
				{
					foreach (var key in fieldValues.Keys)
					{
						var value = fieldValues[key];
						_dBaseEngine.WriteField(Convert.ToInt32(newId, CultureInfo.InvariantCulture), key, value);
					}
				}
			}
		}

		private Dictionary<string, string> FilterFieldValue(Dictionary<string, string> fieldValues)
		{
			
			return fieldValues.Keys.Where(key => _dBaseEngine.GetColumnNumber(key) != -1)
				.ToDictionary(key => key, key => fieldValues[key]);
		}

		private void DeleteRecord(int index)
		{
			
			if (_rTreeIndex.HasIdx)
			{
				var wkb = _shapeFile.ReadRecord(index);
				var targetShape = BaseShape.CreateShapeFromWellKnownData(wkb);
				if (targetShape != null)
				{
					_rTreeIndex.Delete(targetShape);
				}
			}

			_dBaseEngine.DeleteRecord(index);

			_shapeFile.DeleteRecord(index);
		}

		private void UpdateRecord(Feature targetFeature)
		{
			var index = Convert.ToInt32(targetFeature.Id, CultureInfo.InvariantCulture);
			var wkbSource = _shapeFile.ReadRecord(index);
			var sourceShape = BaseShape.CreateShapeFromWellKnownData(wkbSource);

			var targetShape = targetFeature.GetShape();

			_shapeFile.UpdateRecord(index, targetShape);

			if (_rTreeIndex.HasIdx)
			{
				_rTreeIndex.Delete(sourceShape);
				targetShape.Id = index.ToString(CultureInfo.InvariantCulture);
				_rTreeIndex.Add(targetShape);
			}

			if (targetFeature.ColumnValues != null)
			{
				foreach (var item in targetFeature.ColumnValues)
				{
					_dBaseEngine.WriteField(index, item.Key, item.Value);
				}
			}
		}

		private string ReplaceTableName(string sqlStatement)
		{
			sqlStatement += " ";
			var shortName = ShapeFileFeatureSourceHelper.GetShortFileName(ShapePathFileName);

			var sourceFileNameWithoutExtension = Path.GetFileNameWithoutExtension(ShapePathFileName).ToUpperInvariant();
			var shortNameWithoutExtension = Path.GetFileNameWithoutExtension(shortName);
			var position = sqlStatement.IndexOf(sourceFileNameWithoutExtension, StringComparison.OrdinalIgnoreCase);
			var tableName = sqlStatement.Substring(position, sourceFileNameWithoutExtension.Length);

			var substringIncludeTableName = sqlStatement.Substring(0, position + tableName.Length);
			sqlStatement = sqlStatement.Replace(substringIncludeTableName,
				substringIncludeTableName.Replace(tableName, shortNameWithoutExtension));


			if (!sqlStatement.Contains(" [" + shortNameWithoutExtension + "] "))
			{
				sqlStatement = sqlStatement.Replace(" " + shortNameWithoutExtension + " ", " [" + shortNameWithoutExtension + "] ");
			}

			return sqlStatement;
		}

        
    }
}
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Mapgenix.Index;
using Mapgenix.Shapes;
using Mapgenix.Canvas;

namespace Mapgenix.FeatureSource
{
    /// <summary>Static class for operations on ESRI shapefiles.</summary>
    public static class ShapeFileFeatureSourceHelper
	{
        /// <summary>Builds a spatial index to increase access speed.</summary>
		public static void BuildIndexFile(string shapePathFilename, BuildIndexMode buildIndexMode)
		{
			Validators.CheckParameterIsNotNullOrEmpty(shapePathFilename, "pathFileName");
			Validators.CheckShapeFileNameIsValid(shapePathFilename, "shapePathFileName");
			
			BuildIndexFile(shapePathFilename, Path.ChangeExtension(shapePathFilename, ".idx"), null, string.Empty, string.Empty,
				buildIndexMode);
		}

        /// <summary>Builds a spatial index to increase access speed.</summary>
		public static void BuildIndexFile(string shapePathFilename, string indexPathFilename,
            BaseProjection projection, string columnName, string regularExpression,
			BuildIndexMode buildIndexMode)
		{
			Validators.CheckParameterIsNotNullOrEmpty(shapePathFilename, "shapePathFileName");
			Validators.CheckShapeFileNameIsValid(shapePathFilename, "shapePathFileName");
			Validators.CheckParameterIsNotNullOrEmpty(indexPathFilename, "indexPathFileName");
			Validators.CheckParameterIsNotNull(columnName, "columnName");
			Validators.CheckParameterIsNotNull(regularExpression, "regularExpression");
			
			BuildIndexFile(shapePathFilename, indexPathFilename, projection, columnName, regularExpression, buildIndexMode,
				Encoding.Default);
		}

        /// <summary>Builds a spatial index to increase access speed.</summary>
		public static void BuildIndexFile(string shapePathFilename, string indexPathFilename,
            BaseProjection projection, string columnName, string regularExpression,
			BuildIndexMode buildIndexMode, Encoding encoding)
		{
			Validators.CheckParameterIsNotNullOrEmpty(shapePathFilename, "shapePathFileName");
			Validators.CheckShapeFileNameIsValid(shapePathFilename, "shapePathFileName");
			Validators.CheckParameterIsNotNullOrEmpty(indexPathFilename, "indexPathFileName");
			Validators.CheckParameterIsNotNull(columnName, "columnName");
			Validators.CheckParameterIsNotNull(regularExpression, "regularExpression");
			Validators.CheckParameterIsNotNull(encoding, "encoding");
		
			string tmpIndexPathFilename = Path.GetDirectoryName(indexPathFilename) + "\\TMP" +
										  Path.GetFileName(indexPathFilename);
			if (!(File.Exists(indexPathFilename) && File.Exists(Path.ChangeExtension(indexPathFilename, ".ids"))) ||
				buildIndexMode == BuildIndexMode.Rebuild)
			{
				RtreeSpatialIndex rTreeIndex = new RtreeSpatialIndex(tmpIndexPathFilename, RtreeSpatialIndexReadWriteMode.ReadWrite);

				ShapeFileFeatureSource featureSource = FeatureSourceFactory.CreateShapeFileFeatureSource(shapePathFilename, encoding);
				featureSource.RequireIndex = false;

				featureSource.Open();
				if (projection != null)
				{
					if (!projection.IsOpen)
					{
						projection.Open();
					}
				}
				try
				{
					ShapeFileType shapeType = ShapeFileType.Null;

					ShapeFileType currentRecordType = featureSource._shapeFile.GetShapeFileType();
					shapeType = currentRecordType;
					CreateIndexFile(tmpIndexPathFilename, currentRecordType);
					rTreeIndex.Open();

					int recordCount = featureSource.GetCount();
					DateTime startProcessTime = DateTime.Now;

					for (int i = 1; i <= recordCount; i++)
					{
						string id = i.ToString(CultureInfo.InvariantCulture);

						BaseShape currentShape = null;
						if (shapeType == ShapeFileType.Point || shapeType == ShapeFileType.PointZ || shapeType == ShapeFileType.PointM)
						{
							byte[] wkb = featureSource._shapeFile.ReadRecord(Convert.ToInt32(id, CultureInfo.InvariantCulture));
							currentShape = BaseShape.CreateShapeFromWellKnownData(wkb);
						}
						else
						{
							currentShape = featureSource._shapeFile.GetBoundingBoxById(Convert.ToInt32(id, CultureInfo.InvariantCulture));
						}

						if (currentShape != null)
						{
							bool isMatch = false;
							if (string.IsNullOrEmpty(columnName) && string.IsNullOrEmpty(regularExpression))
							{
								isMatch = true;
							}
							else
							{
								if (!string.IsNullOrEmpty(columnName))
								{
									string columnValue = featureSource.GetDataFromDbf(id.ToString(CultureInfo.InvariantCulture), columnName);
									isMatch = Regex.IsMatch(columnValue, regularExpression, RegexOptions.IgnoreCase);
								}
							}

							if (isMatch)
							{
								currentShape.Id = id;

								ShapeFileIndexEventArgs buildingIndexShapeFileFeatureSourceEventArgs =
									new ShapeFileIndexEventArgs(recordCount, i, new Feature(currentShape), startProcessTime,
										false, shapePathFilename);
								OnBuildingIndex(buildingIndexShapeFileFeatureSourceEventArgs);
								if (!buildingIndexShapeFileFeatureSourceEventArgs.Cancel)
								{
									BuildIndex(currentShape, rTreeIndex, projection);
								}
								else
								{
									break;
								}
							}
						}
					}
					rTreeIndex.Flush();
					rTreeIndex.Close();
				}
				finally
				{
					if (rTreeIndex != null)
					{
						rTreeIndex.Close();
					}
					if (featureSource != null)
					{
						featureSource.Close();
					}
					if (projection != null)
					{
						projection.Close();
					}

					MoveFile(tmpIndexPathFilename, indexPathFilename);
					MoveFile(Path.ChangeExtension(tmpIndexPathFilename, ".ids"), Path.ChangeExtension(indexPathFilename, ".ids"));

					DeleteFile(tmpIndexPathFilename);
					DeleteFile(Path.ChangeExtension(tmpIndexPathFilename, ".ids"));
				}
			}
		}


		private static void BuildIndex(BaseShape baseShape, RtreeSpatialIndex openedRtree,
            BaseProjection openedProjection)
		{
			if (baseShape != null)
			{
				BaseShape newShape = baseShape;
				if (openedProjection != null)
				{
					newShape = openedProjection.ConvertToExternalProjection(baseShape);
					newShape.Id = baseShape.Id;
				}

				openedRtree.Add(newShape);
			}
		}

        /// <summary>Creates a new shapefile.</summary>
		public static void CreateShapeFile(ShapeFileType shapeType, string pathFilename,
			IEnumerable<DbfColumn> databaseColumns, Encoding encoding, OverwriteMode overwriteMode)
		{
			Validators.CheckShapeFileTypeIsValid(shapeType, "shapeType");
			Validators.CheckParameterIsNotNullOrEmpty(pathFilename, "pathFileName");
			Validators.CheckParameterIsNotNull(databaseColumns, "databaseColumns");
			Validators.CheckDatabaseColumnsIsEmpty(databaseColumns, "databaseColumns");
			Validators.CheckOverwriteModeIsValid(overwriteMode, "overwriteMode");
			Validators.CheckParameterIsNotNull(encoding, "encoding");
			Validators.CheckShapeFileNameIsValid(pathFilename, "pathFileName");

			if (overwriteMode == OverwriteMode.DoNotOverwrite)
			{
				Validators.CheckFileIsNotExist(pathFilename);
			}

			string newPathFileName = Path.GetDirectoryName(pathFilename) + "\\TMP" + Path.GetFileName(pathFilename);

			try
			{
				ShapeFile.CreateShapeFile(newPathFileName, shapeType);

				GeoDbf.CreateDbfFile(Path.ChangeExtension(newPathFileName, ".dbf"), databaseColumns, OverwriteMode.DoNotOverwrite,
					encoding);

				CreateIndexFile(Path.ChangeExtension(newPathFileName, ".idx"), shapeType);

				MoveFile(newPathFileName, pathFilename);
				MoveFile(Path.ChangeExtension(newPathFileName, ".dbf"), Path.ChangeExtension(pathFilename, ".dbf"));
				MoveFile(Path.ChangeExtension(newPathFileName, ".dbt"), Path.ChangeExtension(pathFilename, ".dbt"));
				MoveFile(Path.ChangeExtension(newPathFileName, ".idx"), Path.ChangeExtension(pathFilename, ".idx"));
				MoveFile(Path.ChangeExtension(newPathFileName, ".ids"), Path.ChangeExtension(pathFilename, ".ids"));
				MoveFile(Path.ChangeExtension(newPathFileName, ".shx"), Path.ChangeExtension(pathFilename, ".shx"));
			}
			finally
			{
				DeleteFile(newPathFileName);
				DeleteFile(Path.ChangeExtension(newPathFileName, ".dbf"));
				DeleteFile(Path.ChangeExtension(newPathFileName, ".dbt"));
				DeleteFile(Path.ChangeExtension(newPathFileName, ".idx"));
				DeleteFile(Path.ChangeExtension(newPathFileName, ".ids"));
				DeleteFile(Path.ChangeExtension(newPathFileName, ".shx"));
			}
		}

        /// <summary>Clones the structure from a source shape file to a target shape file. 
        /// The targetShapeFile has the same type and dbf columns as the source shapefile but without any records.</summary>
		public static void CloneShapeFileStructure(string sourceShapePathFilename, string targetShapePathFilename,
			OverwriteMode overwriteMode)
		{
			Validators.CheckParameterIsNotNullOrEmpty(sourceShapePathFilename, "sourceShapePathFileName");
			Validators.CheckShapeFileNameIsValid(sourceShapePathFilename, "sourceShapePathFileName");
			Validators.CheckParameterIsNotNullOrEmpty(targetShapePathFilename, "targetShapePathFileName");
			Validators.CheckShapeFileNameIsValid(targetShapePathFilename, "targetShapePathFileName");
			Validators.CheckOverwriteModeIsValid(overwriteMode, "overwriteMode");

			CloneShapeFileStructure(sourceShapePathFilename, targetShapePathFilename, overwriteMode, Encoding.Default);
		}

        /// <summary>Clones the structure from a source shape file to a target shape file. 
        /// The targetShapeFile has the same type and dbf columns as the source shapefile but without any records.</summary>
		public static void CloneShapeFileStructure(string sourceShapePathFilename, string targetShapePathFilename,
			OverwriteMode overwriteMode, Encoding encoding)
		{
			Validators.CheckParameterIsNotNullOrEmpty(sourceShapePathFilename, "sourceShapePathFileName");
			Validators.CheckShapeFileNameIsValid(sourceShapePathFilename, "sourceShapePathFileName");
			Validators.CheckParameterIsNotNullOrEmpty(targetShapePathFilename, "targetShapePathFileName");
			Validators.CheckShapeFileNameIsValid(targetShapePathFilename, "targetShapePathFileName");
			Validators.CheckOverwriteModeIsValid(overwriteMode, "overwriteMode");
			Validators.CheckParameterIsNotNull(encoding, "encoding");


			ShapeFileFeatureSource sourceFeatureSource =
				FeatureSourceFactory.CreateShapeFileFeatureSource(sourceShapePathFilename);
			sourceFeatureSource.RequireIndex = false;

			Collection<DbfColumn> databaseColumns = new Collection<DbfColumn>();

			ShapeFileType shapeFileType;
			try
			{
				sourceFeatureSource.Open();
				databaseColumns = sourceFeatureSource.GetDbfColumns();
				shapeFileType = sourceFeatureSource._shapeFile.ShapeFileType;
			}
			finally
			{
				sourceFeatureSource.Close();
			}
			CreateShapeFile(shapeFileType, targetShapePathFilename, databaseColumns, encoding, overwriteMode);
		}

		public static byte[] GetWellKnownBinaryForRectangle(RectangleShape rectangleShape)
		{
			byte[] wkb = new byte[93];
			wkb[0] = 1;
			wkb[1] = 3;
			wkb[2] = 0;
			wkb[3] = 0;
			wkb[4] = 0;
			wkb[5] = 1;
			wkb[6] = 0;
			wkb[7] = 0;
			wkb[8] = 0;
			wkb[9] = 5;
			wkb[10] = 0;
			wkb[11] = 0;
			wkb[12] = 0;

			byte[] upperLeftXBytes = BitConverter.GetBytes(rectangleShape.UpperLeftPoint.X);
			byte[] upperLeftYBytes = BitConverter.GetBytes(rectangleShape.UpperLeftPoint.Y);
			byte[] lowerRightXBytes = BitConverter.GetBytes(rectangleShape.LowerRightPoint.X);
			byte[] lowerRightYBytes = BitConverter.GetBytes(rectangleShape.LowerRightPoint.Y);

			int startIndex = 13;

			CopyArray(upperLeftXBytes, wkb, startIndex);
			startIndex += 8;
			CopyArray(upperLeftYBytes, wkb, startIndex);
			startIndex += 8;

			CopyArray(lowerRightXBytes, wkb, startIndex);
			startIndex += 8;
			CopyArray(upperLeftYBytes, wkb, startIndex);
			startIndex += 8;

			CopyArray(lowerRightXBytes, wkb, startIndex);
			startIndex += 8;
			CopyArray(lowerRightYBytes, wkb, startIndex);
			startIndex += 8;

			CopyArray(upperLeftXBytes, wkb, startIndex);
			startIndex += 8;
			CopyArray(lowerRightYBytes, wkb, startIndex);
			startIndex += 8;

			CopyArray(upperLeftXBytes, wkb, startIndex);
			startIndex += 8;
			CopyArray(upperLeftYBytes, wkb, startIndex);
			startIndex += 8;

			return wkb;
		}

		private static void CopyArray(byte[] sourceBytes, byte[] destinateBytes, int startIndex)
		{
			for (int i = 0; i < sourceBytes.Length; i++)
			{
				destinateBytes[startIndex + i] = sourceBytes[i];
			}
		}

		public static bool CheckBoundingBoxIsBigEnough(PointShape upperLeftPoint, RectangleShape boundingBox,
			double screenFactorX, double screenFactorY)
		{
			int upperLeftX = (int)((boundingBox.UpperLeftPoint.X - upperLeftPoint.X) * screenFactorX);
			int upperLeftY = (int)((upperLeftPoint.Y - boundingBox.UpperLeftPoint.Y) * screenFactorY);
			int lowerRightX = (int)((boundingBox.LowerRightPoint.X - upperLeftPoint.X) * screenFactorX);
			int lowerRightY = (int)((upperLeftPoint.Y - boundingBox.LowerRightPoint.Y) * screenFactorY);

			bool isBigEnough = !(lowerRightX - upperLeftX < 2 && upperLeftY - lowerRightY < 2);

			return isBigEnough;
		}

		private static void RebuildDbfFile(string sourceDbfPathFileName)
		{
			GeoDbf dbf = new GeoDbf(sourceDbfPathFileName, DbfReadWriteMode.ReadWrite);
			dbf.Open();
			dbf.Pack();
			dbf.Close();
		}

		private static void RebuildShapeFile(string sourcePathFileName)
		{
			string targetPathFileName = Path.GetDirectoryName(sourcePathFileName) + "\\TMP" +
										Path.GetFileName(sourcePathFileName);

			ShapeFile targetShapeFile = null;
			ShapeFile sourceShapeFile = new ShapeFile(sourcePathFileName);
			sourceShapeFile.Open(FileAccess.Read);
			try
			{
				ShapeFile.CreateShapeFile(targetPathFileName, sourceShapeFile.ShapeFileType);

				targetShapeFile = new ShapeFile(targetPathFileName);
				targetShapeFile.Open(FileAccess.ReadWrite);

				int recordCount = sourceShapeFile.GetRecordCount();
				for (int i = 1; i < recordCount + 1; i++)
				{
					byte[] wkb = sourceShapeFile.ReadRecord(i);
					BaseShape currentShape = BaseShape.CreateShapeFromWellKnownData(wkb);

					if (currentShape != null)
					{
						targetShapeFile.AddRecord(currentShape);
					}
				}

				targetShapeFile.Flush();
			}
			finally
			{
				if (targetShapeFile != null)
				{
					targetShapeFile.Close();
				}
				if (sourceShapeFile != null)
				{
					sourceShapeFile.Close();
				}

				MoveFile(targetPathFileName, sourcePathFileName);
				MoveFile(Path.ChangeExtension(targetPathFileName, ".shx"), Path.ChangeExtension(sourcePathFileName, ".shx"));

				DeleteFile(targetPathFileName);
				DeleteFile(Path.ChangeExtension(targetPathFileName, ".shx"));
			}
		}


		private static void CreateIndexFile(string idxPathFileName, ShapeFileType shapeFileType)
		{
			if (shapeFileType == ShapeFileType.Point || shapeFileType == ShapeFileType.PointZ ||
				shapeFileType == ShapeFileType.PointM)
			{
				RtreeSpatialIndex.CreatePointSpatialIndex(idxPathFileName, RtreeSpatialIndexPageSize.EightKilobytes,
					RtreeSpatialIndexDataFormat.Float);
			}
			else
			{
				RtreeSpatialIndex.CreateRectangleSpatialIndex(idxPathFileName, RtreeSpatialIndexPageSize.EightKilobytes,
					RtreeSpatialIndexDataFormat.Float);
			}
		}


        private static void DeleteFile(string pathFileName)
		{
			if (File.Exists(pathFileName))
			{
				File.SetAttributes(pathFileName, FileAttributes.Normal);
				File.Delete(pathFileName);
			}
		}

		private static void MoveFile(string sourcePathFileName, string targetPathFileName)
		{
			DeleteFile(targetPathFileName);

			if (File.Exists(sourcePathFileName))
			{
				File.Move(sourcePathFileName, targetPathFileName);
			}
		}

		public static string GetShortFileName(string pathFileName)
		{
			StringBuilder sb = new StringBuilder(1024);
			UnsafeNativeMethods.GetShortPathName(pathFileName, sb, sb.Capacity);

			string shortPath = sb.ToString();

			return shortPath;
		}

		public static bool CheckIsNull(IEnumerable<string> values)
		{
			bool isEmptyOrNull = true;

			if (values != null)
			{
				if (values.Any())
				{
					isEmptyOrNull = false;
				}
			}

			return isEmptyOrNull;
		}

        /// <summary>Projects features in a shapefile into another projection and saves to a new shapefile.</summary>
        /// <param name="sourceShapeFile">Source shapefile to project.</param>
        /// <param name="targetShapeFile">Target shapefile to be saved for the projected features. </param>
        /// <param name="projection">Projection. The source shapefile is in the FromProjection of the Projection parameter, 
        /// and the target shapeFile is in the ToProjection of the Projection.</param>
        /// <param name="overwriteMode">Override mode when the target shapefile already exists.</param>
        public static void SaveToProjection(string sourceShapeFile, string targetShapeFile, Proj4Projection projection, OverwriteMode overwriteMode)
        {
            Validators.CheckShapeFileNameIsValid(sourceShapeFile, "sourceShapeFile");
            Validators.CheckShapeFileNameIsValid(targetShapeFile, "targetShapeFile");
            Validators.CheckParameterIsNotNull(projection, "projection");
            Validators.CheckOverwriteModeIsValid(overwriteMode, "overwriteMode");

            if (overwriteMode == OverwriteMode.DoNotOverwrite)
            {
                Validators.CheckFileIsNotExist(targetShapeFile);
            }

            ShapeFileFeatureSource sourceFeatureSouce = FeatureSourceFactory.CreateShapeFileFeatureSource(sourceShapeFile);
            sourceFeatureSouce.RequireIndex = false;

            try
            {
                sourceFeatureSouce.Open();

                CloneShapeFileStructure(sourceShapeFile, targetShapeFile, overwriteMode);
                ShapeFileFeatureSource targetFeatureSource = FeatureSourceFactory.CreateShapeFileFeatureSource(targetShapeFile, ReadWriteMode.ReadWrite);

                try
                {
                    targetFeatureSource.Open();
                    projection.Open();

                    int recordCount = sourceFeatureSouce.GetCount();
                    for (int i = 1; i <= recordCount; i++)
                    {
                        if (!targetFeatureSource.IsInTransaction)
                        {
                            targetFeatureSource.BeginTransaction();
                        }
                        string id = i.ToString(CultureInfo.InvariantCulture);
                        Feature projectedFeature = sourceFeatureSouce.GetFeatureById(id, ReturningColumnsType.AllColumns);
                        projection.UpdateToExternalProjection(projectedFeature);
                        targetFeatureSource.AddFeature(projectedFeature);
                        if (i % 10000 == 0)
                        {
                            targetFeatureSource.CommitTransaction();
                        }
                    }
                }
                finally
                {
                    if (targetFeatureSource.IsInTransaction)
                    {
                        targetFeatureSource.CommitTransaction();
                    }

                    projection.Close();
                    targetFeatureSource.Close();
                }

                BuildIndexFile(targetShapeFile, BuildIndexMode.Rebuild);
            }
            finally
            {
                sourceFeatureSouce.Close();
            }
        }

        /// <summary>Fired each time the index of a record is built using RTree index.
        /// Usefull to build a progress bar.</summary>
        public static event EventHandler<ShapeFileIndexEventArgs> BuildingIndex;

		internal static void OnBuildingIndex(ShapeFileIndexEventArgs e)
		{
			EventHandler<ShapeFileIndexEventArgs> handler = BuildingIndex;

			if (handler != null)
			{
				handler(null, e);
			}
		}

	}
}

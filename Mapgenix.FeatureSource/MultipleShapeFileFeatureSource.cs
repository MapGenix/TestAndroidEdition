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
using Mapgenix.FeatureSource.Properties;

namespace Mapgenix.FeatureSource
{
    /// <summary>FeatureSource for multiple ESRI shapefiles.</summary>
    [Serializable]
    public class MultipleShapeFileFeatureSource : BaseFeatureSource
    {
        private const int MaximumLoopCount = 100;

        private const char SlashCharacter = '\\';
        private const char WildcardQuestion = '?';
        private const char WildcardStar = '*';
        private Dictionary<string, GeoDbf> _dBaseEngines;

        private string _indexFilePattern;
        private bool _isBigMultipleIndex;
        private string _multipleShapeFilePattern;
        private Dictionary<string, RtreeSpatialIndex> _rTreeIndexs;

        private ReadWriteMode _shapeFileReadWriteMode;
        private Dictionary<string, ShapeFile> _shapeFiles;
        private Encoding _encoding;

        /// <summary>Default cosntructor of MultipleShapefileFeatureSource.</summary>
        /// <remarks>You need to add ShapefileFeatureSources to MultipleShapefileFeatureSource with property FeatureSources.</remarks>
        /// <returns>None</returns>
        public MultipleShapeFileFeatureSource()
            : this(string.Empty, string.Empty)
        {
        }


        public MultipleShapeFileFeatureSource(string multipleShapeFilePattern)
            : this(multipleShapeFilePattern, string.Empty)
        {
        }

        public MultipleShapeFileFeatureSource(string multipleShapeFilePattern, string indexFilePattern)
        {
            _multipleShapeFilePattern = multipleShapeFilePattern;
            _indexFilePattern = indexFilePattern;

            _shapeFiles = new Dictionary<string, ShapeFile>(StringComparer.OrdinalIgnoreCase);
            _dBaseEngines = new Dictionary<string, GeoDbf>(StringComparer.OrdinalIgnoreCase);
            _rTreeIndexs = new Dictionary<string, RtreeSpatialIndex>(StringComparer.OrdinalIgnoreCase);
            ShapeFiles = new Collection<string>();
            Indexes = new Collection<string>();
            _encoding = Encoding.Default;
        }

        public MultipleShapeFileFeatureSource(IEnumerable<string> shapeFiles)
        {
            Collection<string> multipleShapeFileIndexes = new Collection<string>();
            foreach (string indexFile in shapeFiles.Select(item => Path.ChangeExtension(item, ".midx")))
            {
                multipleShapeFileIndexes.Add(indexFile);
            }
            Init(shapeFiles, multipleShapeFileIndexes);
        }

        public MultipleShapeFileFeatureSource(IEnumerable<string> shapeFiles, IEnumerable<string> indexes)
        {
            Init(shapeFiles, indexes);
        }

        private void Init(IEnumerable<string> multipleShapeFiles, IEnumerable<string> multipleShapeFileIndexes)
        {
            Validators.CheckParameterIsNotNull(multipleShapeFiles, "multipleShapeFiles");
            Validators.CheckParameterIsNotNull(multipleShapeFileIndexes, "multipleShapeFileIndexes");

            _multipleShapeFilePattern = string.Empty;
            _indexFilePattern = string.Empty;

            _shapeFiles = new Dictionary<string, ShapeFile>(StringComparer.OrdinalIgnoreCase);
            _dBaseEngines = new Dictionary<string, GeoDbf>(StringComparer.OrdinalIgnoreCase);
            _rTreeIndexs = new Dictionary<string, RtreeSpatialIndex>(StringComparer.OrdinalIgnoreCase);
            ShapeFiles = new Collection<string>();
            Indexes = new Collection<string>();

            List<string> multipleShapeFilesList = new List<string>(multipleShapeFiles);
            List<string> multipleShapeFileIndexesList = new List<string>(multipleShapeFileIndexes);

            for (int i = 0; i < multipleShapeFilesList.Count; i++)
            {
                ShapeFiles.Add(multipleShapeFilesList[i].ToUpperInvariant());
                Indexes.Add(multipleShapeFileIndexesList[i].ToUpperInvariant());
            }
            _encoding = Encoding.Default;
        }

        public Collection<string> ShapeFiles { get; private set; }

        public Collection<string> Indexes { get; private set; }

       
        public string MultipleShapeFilePattern
        {
            get { return _multipleShapeFilePattern; }
            set { _multipleShapeFilePattern = value; }
        }

        public string IndexFilePattern
        {
            get { return _indexFilePattern; }
            set { _indexFilePattern = value; }
        }

        public Encoding Encoding
        {
            get { return _encoding; }
            set { _encoding = value; }
        }

        public override bool IsEditable
        {
            get { return false; }
        }

        public Collection<string> GetShapePathFileNames()
        {
            Collection<string> shapePathFileNames = new Collection<string>();
            if (string.IsNullOrEmpty(_multipleShapeFilePattern))
            {
                foreach (string key in ShapeFiles)
                {
                    shapePathFileNames.Add(key);
                }
            }
            else
            {
                string[] pathFileNames = GetPathFileNames(_multipleShapeFilePattern);
                foreach (string pathFileName in pathFileNames)
                {
                    shapePathFileNames.Add(pathFileName);
                }
            }
            return shapePathFileNames;
        }

        public Collection<string> GetIndexPathFileNames()
        {
            Collection<string> indexPathFileNames = new Collection<string>();
            if (string.IsNullOrEmpty(_multipleShapeFilePattern))
            {
                foreach (string key in Indexes)
                {
                    indexPathFileNames.Add(key);
                }
            }
            else
            {
                string[] pathFileNames = GetPathFileNames(_indexFilePattern);

                foreach (string pathFileName in pathFileNames)
                {
                    indexPathFileNames.Add(pathFileName);
                }
            }
            return indexPathFileNames;
        }

        public static void BuildIndex(string multipleShapeFilePattern)
        {
            Validators.CheckParameterIsNotNullOrEmpty(multipleShapeFilePattern, "multipleShapeFilePattern");

            string indexFilePatten = multipleShapeFilePattern.Replace(".shp", ".midx");
            BuildIndex(multipleShapeFilePattern, indexFilePatten, BuildIndexMode.DoNotRebuild);
        }

        public static void BuildIndex(string multipleShapeFilePattern, BuildIndexMode buildIndexMode)
        {
            Validators.CheckParameterIsNotNullOrEmpty(multipleShapeFilePattern, "multipleShapeFilePattern");
         
            string indexFilePatten = multipleShapeFilePattern.Replace(".shp", ".midx");
            BuildIndex(multipleShapeFilePattern, indexFilePatten, buildIndexMode);
        }

        public static void BuildIndex(string multipleShapeFilePattern, string indexFilePattern)
        {
            Validators.CheckParameterIsNotNullOrEmpty(multipleShapeFilePattern, "multipleShapeFilePattern");
            Validators.CheckParameterIsNotNullOrEmpty(indexFilePattern, "indexPathFileName");

            BuildIndex(multipleShapeFilePattern, indexFilePattern, BuildIndexMode.DoNotRebuild);
        }

        public static void BuildIndex(string multipleShapeFilePattern, string indexFilePattern, BuildIndexMode buildIndexMode)
        {
            Validators.CheckParameterIsNotNullOrEmpty(multipleShapeFilePattern, "multipleShapeFilePattern");
            Validators.CheckParameterIsNotNullOrEmpty(indexFilePattern, "indexPathFileName");
         
            Dictionary<string, string> shapeAndIdxPathFileNames = GetShapeAndIndexDictionary(multipleShapeFilePattern, indexFilePattern);

       
            bool build = CheckBuildIndexMode(buildIndexMode, shapeAndIdxPathFileNames.Values);

            if (build)
            {
                foreach (string shapePathFileName in shapeAndIdxPathFileNames.Keys)
                {
                    BuildIdxFile(shapePathFileName, shapeAndIdxPathFileNames[shapePathFileName], string.Empty, string.Empty);
                }
            }
        }

        public static void BuildIndex(string multipleShapeFilePattern, string columnName, string regularExpression, string indexFilename)
        {
            Validators.CheckParameterIsNotNullOrEmpty(multipleShapeFilePattern, "multipleShapeFilePattern");
            Validators.CheckParameterIsNotNullOrEmpty(columnName, "columnName");
            Validators.CheckParameterIsNotNullOrEmpty(regularExpression, "regularExpression");
            Validators.CheckParameterIsNotNullOrEmpty(indexFilename, "indexPathFileName");

            BuildIndex(multipleShapeFilePattern, columnName, regularExpression, indexFilename, BuildIndexMode.Rebuild);
        }

        public static void BuildIndex(string multipleShapeFilePattern, string columnName, string regularExpression, string indexPathFilename, BuildIndexMode buildIndexMode)
        {
            Validators.CheckParameterIsNotNullOrEmpty(multipleShapeFilePattern, "multipleShapeFilePattern");
            Validators.CheckParameterIsNotNullOrEmpty(columnName, "columnName");
            Validators.CheckParameterIsNotNullOrEmpty(regularExpression, "regularExpression");
            Validators.CheckParameterIsNotNullOrEmpty(indexPathFilename, "indexPathFileName");
            
            bool build = CheckBuildIndexMode(buildIndexMode, new[] { indexPathFilename });

            if (build)
            {
                Dictionary<string, string> shapeAndIdxPathFileNames = GetShapeAndIndexDictionary(multipleShapeFilePattern, indexPathFilename);
                foreach (string shapePathFileName in shapeAndIdxPathFileNames.Keys)
                {
                    BuildIdxFile(shapePathFileName, indexPathFilename, columnName, regularExpression);
                }
            }
        }

        public static void BuildIndex(string[] multipleShapeFiles, string[] multipleShapeFileIndexes)
        {
            Validators.CheckIEnumerableIsEmptyOrNull(multipleShapeFiles);
            Validators.CheckIEnumerableIsEmptyOrNull(multipleShapeFileIndexes);

            BuildIndex(multipleShapeFiles, multipleShapeFileIndexes, BuildIndexMode.DoNotRebuild);
        }

        public static void BuildIndex(string[] multipleShapeFiles, string[] multipleShapeFileIndexes, BuildIndexMode buildIndexMode)
        {
            Validators.CheckIEnumerableIsEmptyOrNull(multipleShapeFiles);
            Validators.CheckIEnumerableIsEmptyOrNull(multipleShapeFileIndexes);
           
            bool build = CheckBuildIndexMode(buildIndexMode, multipleShapeFileIndexes);

            if (build)
            {
                for (int i = 0; i < multipleShapeFiles.Length; i++)
                {
                    BuildIdxFile(multipleShapeFiles[i], multipleShapeFileIndexes[i], string.Empty, string.Empty);
                }
            }
        }

        private static bool CheckBuildIndexMode(BuildIndexMode buildIndexMode, IEnumerable<string> idxPathFileNames)
        {
            bool build = false;
            if (buildIndexMode == BuildIndexMode.Rebuild)
            {
                foreach (string idxFile in idxPathFileNames)
                {
                    DeleteFile(idxFile);
                    DeleteFile(Path.ChangeExtension(idxFile, ".mids"));
                }
                build = true;
            }
            else
            {
                foreach (string idxPathFileName in idxPathFileNames)
                {
                    build = !File.Exists(idxPathFileName);
                    if (!build)
                    {
                        break;
                    }
                }
            }
            return build;
        }

        private static Dictionary<string, string> GetShapeAndIndexDictionary(string multipleShapeFilePattern, string indexFilePattern)
        {
            Dictionary<string, string> idxPathFileNames = new Dictionary<string, string>();

            string[] fileNames = GetPathFileNames(multipleShapeFilePattern);

            string[] indexSubString = indexFilePattern.Split(new[] { '?', '*' }, StringSplitOptions.RemoveEmptyEntries);

            if (indexSubString.Length == 1)
            {
                foreach (string t in fileNames)
                {
                    idxPathFileNames.Add(t.ToUpperInvariant(), indexFilePattern);
                }

                return idxPathFileNames;
            }
            if (!string.IsNullOrEmpty(indexFilePattern))
            {
                foreach (string t in fileNames)
                {
                    idxPathFileNames.Add(t.ToUpperInvariant(), t.Substring(0, t.Length - 4) + ".midx");
                }

                return idxPathFileNames;
            }

            char[] idxFilePatternCharArray = indexFilePattern.ToCharArray();
            string[] substring = multipleShapeFilePattern.Split(new[] { '?', '*' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string fileName in fileNames)
            {
                string result = string.Empty;

                for (int j = 1; j < substring.Length; j++)
                {
                    int startIndex = fileName.ToUpperInvariant().LastIndexOf(substring[j - 1].ToUpperInvariant(), StringComparison.Ordinal) + substring[j - 1].Length;
                    int endIndex = fileName.ToUpperInvariant().LastIndexOf(substring[j].ToUpperInvariant(), StringComparison.Ordinal);
                    string insertedString = fileName.Substring(startIndex, endIndex - startIndex);

                    if (j >= indexSubString.Length)
                    {
                        break;
                    }

                    int idxStartIndex = indexFilePattern.LastIndexOf(indexSubString[j - 1], StringComparison.Ordinal) + indexSubString[j - 1].Length;
                    int idxEndIndex = indexFilePattern.LastIndexOf(indexSubString[j], StringComparison.Ordinal);

                    result += indexSubString[j - 1];

                    char[] tempCharArray = insertedString.ToCharArray();
                    bool isAddToEnd = false;

                    for (int k = idxStartIndex; k < idxEndIndex; k++)
                    {
                        if (isAddToEnd)
                        {
                            result += tempCharArray[k - idxStartIndex].ToString();
                        }
                        else if (idxFilePatternCharArray[k] == '?')
                        {
                            result += tempCharArray[k - idxStartIndex].ToString();
                        }
                        else if (idxFilePatternCharArray[k] == '*')
                        {
                            isAddToEnd = true;
                            result += tempCharArray[k - idxStartIndex].ToString();
                        }
                    }

                    if (j + 1 >= substring.Length)
                    {
                        result += indexSubString[j];
                    }
                }

                idxPathFileNames.Add(fileName.ToUpperInvariant(), result.ToUpperInvariant());
            }

            return idxPathFileNames;
        }

        private static void BuildIdxFile(string shapePathFilename, string idxFileName, string columnName, string regularExpression)
        {
            ShapeFile shapeFile = new ShapeFile();
            RtreeSpatialIndex rTreeIndex = new RtreeSpatialIndex(idxFileName, RtreeSpatialIndexReadWriteMode.ReadWrite);
            GeoDbf dbfEngine = new GeoDbf();

            try
            {
                shapeFile.PathFilename = shapePathFilename;
                shapeFile.Open(FileAccess.Read);
                int count = shapeFile.GetRecordCount();

                dbfEngine.PathFileName = shapePathFilename.ToLower(CultureInfo.InvariantCulture).Replace(".shp", ".dbf");
                dbfEngine.Open(FileMode.Open, FileAccess.Read);

                WellKnownType wellKnownType = GetWellKnownType(shapeFile.ShapeFileType);

                Regex regex = null;
                if (!string.IsNullOrEmpty(regularExpression))
                {
                    regex = new Regex(regularExpression);
                }
                for (int i = 1; i < count + 1; i++)
                {
                    if (IsFilteredByRegex(dbfEngine, i, columnName, regex))
                    {
                        continue;
                    }

                    CreateIndexFile(idxFileName, wellKnownType);

                    string id = string.Format(CultureInfo.InvariantCulture, "{0}:{1}", Path.GetFileNameWithoutExtension(shapePathFilename), i);
                    if (!rTreeIndex.IsOpen)
                    {
                        rTreeIndex.Open();
                    }

                    byte[] wkb = shapeFile.ReadRecord(i);
                    BaseShape baseShape = BaseShape.CreateShapeFromWellKnownData(wkb);
                    baseShape.Id = id;
                    rTreeIndex.Add(baseShape);
                }

                shapeFile.Close();
            }
            finally
            {
                shapeFile.Close();
                rTreeIndex.Close();
                dbfEngine.Close();
            }
        }

        private static bool IsFilteredByRegex(GeoDbf dbaseEngine, int recourdId, string columnName, Regex regex)
        {
            bool isFiltered = false;

            if (regex == null)
            {
                return false;
            }

			Collection<string> splitedColumns = ColumnFilter.SplitColumnNames(new[] { columnName });
            if (splitedColumns.Count != 0)
            {
                Dictionary<string, string> datas = GetDataFromDbf(dbaseEngine, recourdId.ToString(CultureInfo.InvariantCulture), splitedColumns);
                Dictionary<string, string> combinedDatas = ColumnFilter.CombineFieldValues(datas, new[] { columnName });
                string value = combinedDatas[columnName];
                
                isFiltered = !regex.IsMatch(value);
            }

            return isFiltered;
        }

        private static void CreateIndexFile(string idxPathFileName, WellKnownType wellKnownType)
        {
            if (!File.Exists(idxPathFileName))
            {
                if (wellKnownType == WellKnownType.Point)
                {
                    RtreeSpatialIndex.CreatePointSpatialIndex(idxPathFileName, RtreeSpatialIndexPageSize.EightKilobytes, RtreeSpatialIndexDataFormat.Double);
                }
                else
                {
                    RtreeSpatialIndex.CreateRectangleSpatialIndex(idxPathFileName, RtreeSpatialIndexPageSize.EightKilobytes, RtreeSpatialIndexDataFormat.Double);
                }
            }
        }

        protected override Collection<FeatureSourceColumn> GetColumnsCore()
        {
            Validators.CheckFeatureSourceIsOpen(IsOpen);

            Collection<FeatureSourceColumn> returnValues = new Collection<FeatureSourceColumn>();

            Collection<string> pathFileNames = GetShapePathFileNames();

            if (pathFileNames.Count != 0)
            {
                string key = Path.GetDirectoryName(pathFileNames[0]).Replace(":", "") + "\\" + Path.GetFileNameWithoutExtension(pathFileNames[0]);
                GeoDbf dBaseEngine = GetDBaseEngine(pathFileNames[0], key);
                for (int i = 1; i <= dBaseEngine.ColumnCount; i++)
                {

                    DbfColumn dbfColumn = dBaseEngine.GetColumn(i);
                    FeatureSourceColumn tmp = new FeatureSourceColumn(dbfColumn.ColumnName, dbfColumn.ColumnType.ToString(), dbfColumn.Length);
                    returnValues.Add(tmp);
                }
            }

            return returnValues;
        }

        protected override void OpenCore()
        {
            if (string.IsNullOrEmpty(_indexFilePattern) || _indexFilePattern.Contains("*") || _indexFilePattern.Contains("?"))
            {
                _isBigMultipleIndex = false;
            }
            else
            {
                _isBigMultipleIndex = true;
            }

            if (!string.IsNullOrEmpty(_multipleShapeFilePattern))
            {
                Dictionary<string, string> shapeAndIndexDictionary = GetShapeAndIndexDictionary(_multipleShapeFilePattern, _indexFilePattern);
                ShapeFiles = new Collection<string>();
                Indexes = new Collection<string>();
                foreach (string key in shapeAndIndexDictionary.Keys)
                {
                    ShapeFiles.Add(key);
                    Indexes.Add(shapeAndIndexDictionary[key.ToUpperInvariant()]);
                }
            }
        }

        protected override void CloseCore()
        {
            foreach (KeyValuePair<string, ShapeFile> item in _shapeFiles)
            {
                item.Value.Close();
            }

            foreach (KeyValuePair<string, GeoDbf> item in _dBaseEngines)
            {
                item.Value.Close();
            }

            foreach (KeyValuePair<string, RtreeSpatialIndex> item in _rTreeIndexs)
            {
                if (item.Value != null)
                {
                    item.Value.Close();
                }
            }

            _shapeFiles.Clear();
            _dBaseEngines.Clear();
            _rTreeIndexs.Clear();
        }

        protected override TransactionResult CommitTransactionCore(TransactionBuffer transactions)
        {
            throw new NotSupportedException(ExceptionDescription.NotSupported);
        }

        protected override Collection<Feature> GetAllFeaturesCore(IEnumerable<string> returningColumnNames)
        {
            Validators.CheckFeatureSourceIsOpen(IsOpen);
            Validators.CheckParameterIsNotNull(returningColumnNames, "columnNames");

            Collection<Feature> returnValues = new Collection<Feature>();
            List<string> returnColumnNames = new List<string>(returningColumnNames);

            foreach (Feature feature in from item in ShapeFiles let key = Path.GetDirectoryName(item).Replace(":", "") + "\\" + Path.GetFileNameWithoutExtension(item) select GetAllFeaturesByKey(Path.GetDirectoryName(item), key, returnColumnNames) into currentFeatures from feature in currentFeatures select feature)
            {
                returnValues.Add(feature);
            }

            return returnValues;
        }

        private Collection<string> GetRecordIdsByKey(string shapePathFileName, string key)
        {
            Collection<string> recordIds = new Collection<string>();

            RtreeSpatialIndex rTreeIndex = GetRTreeIndex(shapePathFileName, key);

            if (rTreeIndex == null)
            {
                ShapeFile shapeFile = GetShapeFile(shapePathFileName, key);
                int recordCount = shapeFile.GetRecordCount();
                for (int i = 1; i < recordCount + 1; i++)
                {
                    string tmpKey = key + ":" + i.ToString(CultureInfo.InvariantCulture);
                    recordIds.Add(tmpKey);
                }
            }
            else
            {
                RectangleShape boundingBox = new RectangleShape(double.MinValue, double.MaxValue, double.MaxValue, double.MinValue);
                Collection<string> allRecordIds = rTreeIndex.GetFeatureIdsIntersectingBoundingBox(boundingBox);

                foreach (string recordId in from recordId in allRecordIds let fullRecordId = shapePathFileName.Replace(":", "") + "\\" + recordId where fullRecordId.Contains(key) select recordId)
                {
                    recordIds.Add(recordId);
                }
            }

            return recordIds;
        }

        private Collection<Feature> GetAllFeaturesByKey(string shapePathFileName, string key, List<string> columnNames)
        {
            Collection<Feature> returnValues = new Collection<Feature>();
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            ShapeFile shapeFile = GetShapeFile(shapePathFileName, key);
            Collection<string> recordIds = GetRecordIdsByKey(Path.GetDirectoryName(shapeFile.PathFilename), key);

            CultureInfo cultureInfo = CultureInfo.InvariantCulture;

            foreach (string recordId in recordIds)
            {
                string tmpRecordId = recordId.Split(':')[1];
                byte[] wkb = shapeFile.ReadRecord(Convert.ToInt32(tmpRecordId, cultureInfo));

                if (columnNames.Count != 0)
                {
                    GeoDbf dBaseEngine = GetDBaseEngine(shapeFile.PathFilename, key);

                    dictionary = GetDataFromDbf(dBaseEngine, Convert.ToInt32(tmpRecordId, cultureInfo).ToString(cultureInfo), columnNames);
                }

                Feature feature = new Feature(wkb, recordId, dictionary);

                returnValues.Add(feature);
            }

            return returnValues;
        }

        private static Dictionary<string, string> GetDataFromDbf(GeoDbf dBaseEngine, string id, IEnumerable<string> columnNames)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();

            foreach (string columnName in columnNames)
            {
                string value = dBaseEngine.ReadFieldAsString(Convert.ToInt32(id, CultureInfo.InvariantCulture), columnName);
                dictionary.Add(columnName, value);
            }

            return dictionary;
        }

        protected override RectangleShape GetBoundingBoxCore()
        {
            Validators.CheckFeatureSourceIsOpen(IsOpen);

            RectangleShape boundingBox = null;

            foreach (RectangleShape currentBoundingBox in from item in ShapeFiles let key = Path.GetDirectoryName(item).Replace(":", "") + "\\" + Path.GetFileNameWithoutExtension(item) select GetBoundingBoxByKey(Path.GetDirectoryName(item), key))
            {
                if (boundingBox == null)
                {
                    boundingBox = currentBoundingBox;
                }
                else
                {
                    boundingBox.ExpandToInclude(currentBoundingBox);
                }
            }

            return boundingBox;
        }

        private RectangleShape GetBoundingBoxByKey(string shapePathFileName, string key)
        {
            ShapeFile shapeFile = GetShapeFile(shapePathFileName, key);
            return shapeFile.GetShapeBoundingBox();
        }

        protected override int GetCountCore()
        {
            Validators.CheckFeatureSourceIsOpen(IsOpen);

            return (from item in ShapeFiles let key = Path.GetDirectoryName(item).Replace(":", "") + "\\" + Path.GetFileNameWithoutExtension(item) select GetRecordIdsByKey(Path.GetDirectoryName(item), key) into recordIds select recordIds.Count).Sum();
        }

        protected override Collection<Feature> GetFeaturesByIdsCore(IEnumerable<string> ids, IEnumerable<string> returningColumnNames)
        {
            Validators.CheckFeatureSourceIsOpen(IsOpen);
            Validators.CheckParameterIsNotNull(ids, "ids");
            Validators.CheckParameterIsNotNull(returningColumnNames, "returningColumnNames");

            Collection<Feature> returnValues = new Collection<Feature>();
            List<string> columnNames = new List<string>(returningColumnNames);

            foreach (Feature feature in from item in ids let key = (item.Split(':'))[0] let recordId = (item.Split(':'))[1] select GetFeatureByKeyAndRecordId(item, key, recordId, columnNames))
            {
                returnValues.Add(feature);
            }

            return returnValues;
        }

        private Feature GetFeatureByKeyAndRecordId(string originalId, string key, string recordId, List<string> recordNames)
        {
            string shapeFilePathName = string.Empty;
            if (string.IsNullOrEmpty(_multipleShapeFilePattern))
            {
                foreach (string shapeKeyValue in from shapeKeyValue in ShapeFiles let currentKey = Path.GetDirectoryName(shapeKeyValue).Replace(":", "") + "\\" + Path.GetFileNameWithoutExtension(shapeKeyValue) where string.Equals(currentKey, key, StringComparison.OrdinalIgnoreCase) select shapeKeyValue)
                {
                    shapeFilePathName = Path.GetDirectoryName(shapeKeyValue);
                    break;
                }
            }
            else
            {
                shapeFilePathName = Path.GetDirectoryName(_multipleShapeFilePattern);
            }
            ShapeFile shapeFile = GetShapeFile(shapeFilePathName, key);
            CultureInfo cultureInfo = CultureInfo.InvariantCulture;
            byte[] wkb = shapeFile.ReadRecord(Convert.ToInt32(recordId, cultureInfo));

            Dictionary<string, string> columnValues = null;

            if (recordNames.Count != 0)
            {
                GeoDbf dBaseEngine = GetDBaseEngine(shapeFile.PathFilename, key);
                columnValues = GetDataFromDbf(dBaseEngine, Convert.ToInt32(recordId, cultureInfo).ToString(cultureInfo), recordNames);
            }

            return new Feature(wkb, originalId, columnValues);
        }

        protected override Collection<Feature> GetFeaturesInsideBoundingBoxCore(RectangleShape boundingBox, IEnumerable<string> returningColumnNames)
        {
            Validators.CheckFeatureSourceIsOpen(IsOpen);
            Validators.CheckParameterIsNotNull(boundingBox, "boundingBox");
            Validators.CheckShapeIsValidForOperation(boundingBox);
            Validators.CheckParameterIsNotNull(returningColumnNames, "returningColumnNames");

            Collection<Feature> returnValues = new Collection<Feature>();
            List<string> recordNames = new List<string>(returningColumnNames);

            if (_isBigMultipleIndex)
            {
                RtreeSpatialIndex rTreeIndex = null;
                if (_rTreeIndexs.ContainsKey(_indexFilePattern))
                {
                    rTreeIndex = _rTreeIndexs[_indexFilePattern];
                }
                else
                {
                    rTreeIndex = new RtreeSpatialIndex(_indexFilePattern, RtreeSpatialIndexReadWriteMode.ReadOnly);
                    rTreeIndex.Open();

                    _rTreeIndexs.Add(_indexFilePattern, rTreeIndex);
                }

                Collection<string> recordIds = rTreeIndex.GetFeatureIdsIntersectingBoundingBox(boundingBox);

                foreach (Feature feature in from item in recordIds select Path.GetDirectoryName(rTreeIndex.PathFileName).Replace(":", "") + "\\" + item into fullItem let tempItems = fullItem.Split(':') select GetFeatureByKeyAndRecordId(fullItem, tempItems[0], tempItems[1], recordNames))
                {
                    returnValues.Add(feature);
                }
            }
            else
            {
                foreach (Feature feature in from item in ShapeFiles let key = Path.GetDirectoryName(item).Replace(":", "") + "\\" + Path.GetFileNameWithoutExtension(item) select GetFeaturesInsideBoundingBoxByKey(Path.GetDirectoryName(item), key, boundingBox, recordNames) into features from feature in features select feature)
                {
                    returnValues.Add(feature);
                }
            }

            return returnValues;
        }

        private Collection<Feature> GetFeaturesInsideBoundingBoxByKey(string shapePathFileName, string key, RectangleShape boundingBox, List<string> returningColumnNames)
        {
            Collection<Feature> features = new Collection<Feature>();
            RtreeSpatialIndex rTreeIndex = GetRTreeIndex(shapePathFileName, key);

            if (rTreeIndex == null)
            {
                ShapeFile shapeFile = GetShapeFile(shapePathFileName, key);
                GeoDbf dBaseEngine = GetDBaseEngine(shapeFile.PathFilename, key);
       
                int recordConunt = shapeFile.GetRecordCount();

                for (int i = 1; i < recordConunt + 1; i++)
                {
                    byte[] wkb = shapeFile.ReadRecord(i);
                    BaseShape baseShape = BaseShape.CreateShapeFromWellKnownData(wkb);

                    if (boundingBox.Contains(baseShape))
                    {
                        Dictionary<string, string> dictionary = new Dictionary<string, string>();

                        if (returningColumnNames.Count != 0)
                        {
                            dictionary = GetDataFromDbf(dBaseEngine, i.ToString(CultureInfo.InvariantCulture), returningColumnNames);
                        }

                        baseShape.Id = key + ":" + i.ToString(CultureInfo.InvariantCulture);
                        Feature feature = new Feature(baseShape, dictionary);

                        features.Add(feature);
                    }
                }
            }
            else
            {
                Collection<string> recordIds = rTreeIndex.GetFeatureIdsIntersectingBoundingBox(boundingBox);

                foreach (Feature feature in from item in recordIds select Path.GetDirectoryName(rTreeIndex.PathFileName).Replace(":", "") + "\\" + item into fullItem let tempItems = fullItem.Split(':') where tempItems[0].ToUpperInvariant() == key let recordId = tempItems[1] select GetFeatureByKeyAndRecordId(fullItem, key, recordId, returningColumnNames))
                {
                    features.Add(feature);
                }
            }

            return features;
        }

        protected override Collection<Feature> GetFeaturesOutsideBoundingBoxCore(RectangleShape boundingBox, IEnumerable<string> returningColumnNames)
        {
            Validators.CheckFeatureSourceIsOpen(IsOpen);
            Validators.CheckParameterIsNotNull(boundingBox, "boundingBox");
            Validators.CheckShapeIsValidForOperation(boundingBox);
            Validators.CheckParameterIsNotNull(returningColumnNames, "returningColumnNames");

            Collection<Feature> returnValues = new Collection<Feature>();
            List<string> recordNames = new List<string>(returningColumnNames);

            foreach (Feature feature in from item in ShapeFiles let folder = Path.GetDirectoryName(item) let key = folder.Replace(":", "") + "\\" + Path.GetFileNameWithoutExtension(item) select GetFeaturesOutsideBoundingBoxByKey(folder, key, boundingBox, recordNames) into features from feature in features select feature)
            {
                returnValues.Add(feature);
            }

            return returnValues;
        }

        private Collection<Feature> GetFeaturesOutsideBoundingBoxByKey(string shapePathFileName, string key, RectangleShape boundingBox, List<string> returningColumnNames)
        {
            Collection<Feature> returnValues = new Collection<Feature>();

            RtreeSpatialIndex rTreeIndex = GetRTreeIndex(shapePathFileName, key);

            if (rTreeIndex == null)
            {
                ShapeFile shapeFile = GetShapeFile(shapePathFileName, key);
                GeoDbf dBaseEngine = GetDBaseEngine(shapeFile.PathFilename, key);

                int recordConunt = shapeFile.GetRecordCount();

                for (int i = 1; i < recordConunt + 1; i++)
                {
                    byte[] wkb = shapeFile.ReadRecord(i);
                    BaseShape baseShape = BaseShape.CreateShapeFromWellKnownData(wkb);

                    if (!boundingBox.Contains(baseShape) && !boundingBox.Intersects(baseShape))
                    {
                        Dictionary<string, string> dictionary = new Dictionary<string, string>();

                        if (returningColumnNames.Count != 0)
                        {
                            dictionary = GetDataFromDbf(dBaseEngine, i.ToString(CultureInfo.InvariantCulture), returningColumnNames);
                        }

                        Feature feature = new Feature(baseShape, dictionary);

                        returnValues.Add(feature);
                    }
                }
            }
            else
            {
                Collection<string> allRecrodIds = GetRecordIdsByKey(shapePathFileName, key);

                Collection<string> insideIds = rTreeIndex.GetFeatureIdsIntersectingBoundingBox(boundingBox);
                Collection<string> containingIds = rTreeIndex.GetFeatureIdsWithinBoundingBox(boundingBox);

                foreach (Feature feature in from id in allRecrodIds where !insideIds.Contains(id) && !containingIds.Contains(id) let index = id.IndexOf(':') select key + ":" + id.Remove(0, index + 1) into tmp select GetFeatureById(tmp, returningColumnNames))
                {
                    returnValues.Add(feature);
                }
            }

            return returnValues;
        }

        protected override Collection<Feature> GetFeaturesWithinDistanceOfCore(BaseShape targetShape, GeographyUnit unitOfData, DistanceUnit distanceUnit, double distance, IEnumerable<string> returningColumnNames)
        {
            Validators.CheckFeatureSourceIsOpen(IsOpen);
            Validators.CheckParameterIsNotNull(targetShape, "targetShape");
            Validators.CheckParameterIsValid(targetShape, "targetShape");
            Validators.CheckGeographyUnitIsValid(unitOfData, "unitOfData");
            Validators.CheckDistanceUnitIsValid(distanceUnit, "distanceUnit");
            Validators.CheckParameterIsNotNull(distance, "distance");

            Collection<Feature> returnValues = new Collection<Feature>();
            List<string> columnNames = new List<string>(returningColumnNames);

            foreach (Feature feature in from item in ShapeFiles let key = Path.GetDirectoryName(item).Replace(":", "") + "\\" + Path.GetFileNameWithoutExtension(item) select GetFeaturesWithinDistanceOfByKey(Path.GetDirectoryName(item), key, targetShape, unitOfData, distanceUnit, distance, columnNames) into currentFeatures from feature in currentFeatures select feature)
            {
                returnValues.Add(feature);
            }

            return returnValues;
        }

        private Collection<Feature> GetFeaturesWithinDistanceOfByKey(string shapeFilePathName, string key, BaseShape targetShape, GeographyUnit unitOfData, DistanceUnit distanceUnit, double distance, List<string> returningColumnNames)
        {
            Collection<Feature> features = new Collection<Feature>();

            RectangleShape boundingBox = targetShape.GetBoundingBox();
            if (Math.Abs(boundingBox.Width) < 1e-6)
            {
                double centerX = boundingBox.GetCenterPoint().X;
                boundingBox.UpperLeftPoint.X = centerX - 1e-6;
                boundingBox.LowerRightPoint.X = centerX + 1e-6;
            }
            if (Math.Abs(boundingBox.Height) < 1e-6)
            {
                double centerY = boundingBox.GetCenterPoint().Y;
                boundingBox.UpperLeftPoint.Y = centerY + 1e-6;
                boundingBox.LowerRightPoint.Y = centerY - 1e-6;
            }

            MultipolygonShape multiPlygonShape = boundingBox.Buffer(distance, unitOfData, distanceUnit);
            RectangleShape bufferedBoundingBox = multiPlygonShape.GetBoundingBox();

            Collection<Feature> allPossibleResults = GetFeaturesInsideBoundingBoxByKey(shapeFilePathName, key, bufferedBoundingBox, returningColumnNames);

            BaseShape baseShapeInFeature = null;
            foreach (Feature feature in allPossibleResults)
            {
                baseShapeInFeature = feature.GetShape();
                double currentDistance = baseShapeInFeature.GetDistanceTo(targetShape, unitOfData, distanceUnit);
                if (currentDistance < distance)
                {
                    features.Add(feature);
                }
            }

            return features;
        }


        protected override Collection<Feature> GetFeaturesNearestToCore(BaseShape targetShape, GeographyUnit unitOfData, int maxItemsToFind, IEnumerable<string> returningColumnNames)
        {
            
            return base.GetFeaturesNearestToCore(targetShape, unitOfData, maxItemsToFind, returningColumnNames);
        }

     

        private static string[] GetPathFileNames(string wildcardPathFileName)
        {
            int lastSlash = wildcardPathFileName.LastIndexOf(SlashCharacter);
            string pathName = wildcardPathFileName.Substring(0, lastSlash);
            string wildcardFileName = wildcardPathFileName.Substring(lastSlash + 1);

            string[] files = Directory.GetFiles(pathName, wildcardFileName, SearchOption.AllDirectories);

            return files;
        }

        private ShapeFile GetShapeFile(string shapePathFileName, string key)
        {
            ShapeFile shapeFile = null;

            if (_shapeFiles.ContainsKey(key))
            {
                shapeFile = _shapeFiles[key];
            }
            else
            {
                string fullPath = shapePathFileName + "\\" + Path.GetFileNameWithoutExtension(key) + ".shp";
            
                shapeFile = new ShapeFile(fullPath);

                FileAccess fileAccess = FileAccess.Read;
                if (_shapeFileReadWriteMode == ReadWriteMode.ReadWrite)
                {
                    fileAccess = FileAccess.ReadWrite;
                }
                shapeFile.Open(fileAccess);

                _shapeFiles.Add(key, shapeFile);
            }

            return shapeFile;
        }

        private GeoDbf GetDBaseEngine(string shapePathFileName, string key)
        {
            GeoDbf dbfEngine = null;

            if (_dBaseEngines.ContainsKey(key))
            {
                dbfEngine = _dBaseEngines[key];
            }
            else
            {
                string dbfPathFileName = Path.GetDirectoryName(shapePathFileName) + "\\" + Path.GetFileNameWithoutExtension(key) + ".dbf"; 
               
                DbfReadWriteMode geoDbfReadWriteMode = DbfReadWriteMode.ReadOnly;
                if (_shapeFileReadWriteMode == ReadWriteMode.ReadWrite)
                {
                    geoDbfReadWriteMode = DbfReadWriteMode.ReadWrite;
                }

                dbfEngine = new GeoDbf(dbfPathFileName, geoDbfReadWriteMode) {Encoding = _encoding};
                dbfEngine.Open();

                _dBaseEngines.Add(key, dbfEngine);
            }
            return dbfEngine;
        }

        private RtreeSpatialIndex GetRTreeIndex(string shapePathFileName, string key)
        {
            RtreeSpatialIndex rTreeIndex = null;

            if (_rTreeIndexs.ContainsKey(key))
            {
                rTreeIndex = _rTreeIndexs[key];
            }
            else
            {
                string keyInDictionary = shapePathFileName + "\\" + Path.GetFileNameWithoutExtension(key) + ".shp";

                string idxPathFileName = string.Empty;
                for (int i = 0; i < ShapeFiles.Count; i++)
                {
                    if (ShapeFiles[i] == keyInDictionary.ToUpperInvariant())
                    {
                        idxPathFileName = Indexes[i];
                        break;
                    }
                }
                ;

                if (File.Exists(idxPathFileName))
                {
                    RtreeSpatialIndexReadWriteMode fileAccess = RtreeSpatialIndexReadWriteMode.ReadOnly;
                    if (_shapeFileReadWriteMode == ReadWriteMode.ReadWrite)
                    {
                        fileAccess = RtreeSpatialIndexReadWriteMode.ReadWrite;
                    }
                    rTreeIndex = new RtreeSpatialIndex(idxPathFileName, fileAccess);

                    rTreeIndex.Open();
                }

                _rTreeIndexs.Add(key, rTreeIndex);
            }

            return rTreeIndex;
        }

        private static void DeleteFile(string pathFileName)
        {
            if (File.Exists(pathFileName))
            {
                File.SetAttributes(pathFileName, FileAttributes.Normal);
                File.Delete(pathFileName);
            }
        }

        private static WellKnownType GetWellKnownType(ShapeFileType shapeFileType)
        {
            WellKnownType wellKnownType = WellKnownType.Invalid;
            switch (shapeFileType)
            {
                case ShapeFileType.Null:
                    wellKnownType = WellKnownType.Invalid;
                    break;
                case ShapeFileType.Point:
                    wellKnownType = WellKnownType.Point;
                    break;
                case ShapeFileType.Polyline:
                    wellKnownType = WellKnownType.Line;
                    break;
                case ShapeFileType.Polygon:
                    wellKnownType = WellKnownType.Polygon;
                    break;
                case ShapeFileType.Multipoint:
                    wellKnownType = WellKnownType.Multipoint;
                    break;
                case ShapeFileType.PointZ:
                    wellKnownType = WellKnownType.Invalid;
                    break;
                case ShapeFileType.PolylineZ:
                    wellKnownType = WellKnownType.Invalid;
                    break;
                case ShapeFileType.PolygonZ:
                    wellKnownType = WellKnownType.Invalid;
                    break;
                case ShapeFileType.MultipointZ:
                    wellKnownType = WellKnownType.Invalid;
                    break;
                case ShapeFileType.PointM:
                    wellKnownType = WellKnownType.Invalid;
                    break;
                case ShapeFileType.PolylineM:
                    wellKnownType = WellKnownType.Invalid;
                    break;
                case ShapeFileType.PolygonM:
                    wellKnownType = WellKnownType.Invalid;
                    break;
                case ShapeFileType.MultipointM:
                    wellKnownType = WellKnownType.Invalid;
                    break;
                case ShapeFileType.Multipatch:
                    wellKnownType = WellKnownType.Invalid;
                    break;
                default:
                    break;
            }

            return wellKnownType;
        }
    }
}

using Mapgenix.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Text;

namespace Mapgenix.FeatureSource
{
    [Serializable]
    public class GeoDbf : IDisposable
    {
        private DbfReadWriteMode _readWriteMode;
        private Dictionary<string, int> _fieldNameCache;

        #region Const Define
        private const int FileHeadLength = 32;
        private const int FieldHeadLength = 32;
        private const int NoRecord = -1;
        private const int LeastHeaderLength = 33;

        private const byte Deleted = 0x2A;			
        private const byte Valid = 0x20;		   		
        private const byte FileEnd = 0x1A;  			
        private const byte Encrypted = 0x01;			
        private const byte DbtRecordEnd = 0x1A;		

        private const byte HeaderEndCharacter = 0x0D;
        private const byte HeaderTypeWithoutMemo = 0x03;
        private const byte HeaderTypeWithMemo = 0x8B;

        private const byte ByteBlankField = 0x00;
        private const byte ByteBlankRecord = 0x20;
        private const byte ByteL = (byte)'L';
        private const byte ByteM = (byte)'M';
        private const byte ByteN = (byte)'N';
        private const byte ByteC = (byte)'C';
        private const byte ByteF = (byte)'F';
        private const byte ByteD = (byte)'D';

        private const int EncryptLabelPosition = 15;	
        private const int MaxRecordsOnceRead = 200;		
        private const int BlockSize = 512;				
        private const int ByteValue = 256;				
        private const int BlockHeaderLength = 8;      

        private const string DbtSuffix = ".dbt";		
        private const string TmpFileSuffix = ".tmp";	

        #endregion

        #region Error Type Define
        private const string ErrorCreateReadConflict = "You can not use Read mode when create a file";
        private const string ErrorFileMode = "You didn't input the file mode";
        private const string ErrorIndexOverBound = "Your input index is out of bound";
        private const string ErrorArgumentsNumberIllegal = "The input Arguments number is illegal";
        private const string ErrorFieldNameIllegal = "The input Field name should be less than 10 characters";
        private const string ErrorFieldNameDuplicated = "The input Field name is duplicated";
        private const string ErrorFieldNameNotExist = "The Field name you want is not exist";
        private const string ErrorInputArguments = "Your input arguments are illegal, please check";
        private const string ErrorInputArgumentsArranged = "Your every arguments is legal, but the arranging is wrong";
        private const string ErrorInputLengthOutOfBound = "The Value you input is too long";
        private const string ErrorBlockIndexIllegal = "The Block index you input is illegal";
        private const string ErrorNoPasswordForEncryptedFile = "You are trying to open an encrypted file without a password";
        private const string ErrorPasswordForUnencryptedFile = "You are trying to open an enEncrypted file with a password";

        private const string ErrorFlushInCreateMode = "You can only Flush Record in OPEN mode";
        private const string ErrorWriteInCreateMode = "You can only Write Records in OPEN mode";
        private const string ErrorReadInCreateMode = "You can only Read Records in OPEN mode";
        private const string ErrorDbfIsReadonly = "You cannot do this operation with Access.Read Mode";
        private const string ErrorDeleteInCreateMode = "You can only delete record in OPEN mode";
        private const string ErrorPackInCreateMode = "You can only pack records in OPEN mode";
        private const string ErrorCheckRecordInCreateMode = "You can only check record status in OPEN mode";
        private const string ErrorFileHasClosed = "You can not operate when the file has closed";
        private const string ErrorDateFieldIllegal = "Date Field's length should be 8";
        private const string ErrorEncryptInCreateMode = "You can not set a password in Create mode";
        private const string ErrorOperateInDetachedMode = "You can not do operation while detached, please attached first";
        private const string ErrorIllegalOperationInEncryptedMode = "You can not do the operation in Encrypted format";
        private const string ErrorTypeNotSupported = "We only Supported Character(C), Number(N)," +
            " Logical(L), Date(D), Memo(M), Floating Point(F) now." +
            " You are using an Unsupported Format or your password is not right";

        private const string ErrorCannotGetRecord = "Cannot get the record";
        private const string ErrorNoFieldsExist = "You haven't add any fields yet";
        private const string ErrorOperateOnRemovedRecord = "You are operating a Record which has been removed";

        private const string ErrorDbfTypeNotSupported = "The dbf type you input is not supported."
            + "We only Supported (03)(File without DBT) and (8B)(dBASE IV w. memo) now";

        #endregion

        public event EventHandler<StreamLoadingEventArgs> StreamLoading;

        protected virtual void OnStreamLoading(StreamLoadingEventArgs e)
        {
            EventHandler<StreamLoadingEventArgs> handler = StreamLoading;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        public GeoDbf()
            : this(string.Empty, DbfReadWriteMode.ReadOnly, Encoding.Default, CultureInfo.CurrentCulture)
        {
        }

        public GeoDbf(string pathFileName)
            : this(pathFileName, DbfReadWriteMode.ReadOnly, Encoding.Default, CultureInfo.CurrentCulture)
        {
        }

        public GeoDbf(string pathFileName, DbfReadWriteMode readWriteMode)
            : this(pathFileName, readWriteMode, Encoding.Default, CultureInfo.CurrentCulture)
        {
        }

        public GeoDbf(string pathFileName, DbfReadWriteMode readWriteMode, Encoding encoding)
            : this(pathFileName, readWriteMode, encoding, CultureInfo.CurrentCulture)
        {
        }

        public GeoDbf(string pathFileName, DbfReadWriteMode readWriteMode, Encoding encoding, CultureInfo cultureInfo)
        {
            _pathFileName = pathFileName;
            _readWriteMode = readWriteMode;
            _encoding = encoding;
            _cultureInfo = cultureInfo;
            _isClosed = true;
            _fieldNameCache = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        }

        public bool IsOpen
        {
            get { return !_isClosed; }
        }

        public int ColumnCount
        {
            get { return _columnCount; }
        }

        public int RecordCount
        {
            get { return _recordCount; }
        }

        public string PathFileName
        {
            get { return _pathFileName; }
            set { _pathFileName = value; }
        }

        public DbfReadWriteMode ReadWriteMode
        {
            get { return _readWriteMode; }
            set { _readWriteMode = value; }
        }

        public Encoding Encoding
        {
            get { return _encoding; }
            set { _encoding = value; }
        }

        public CultureInfo CultureInfo
        {
            get { return _cultureInfo; }
            set { _cultureInfo = value; }
        }

        ~GeoDbf()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public static void CopyDbfHeader(string sourcePathFileName, string destinationPathFileName)
        {
            CopyDbfHeader(sourcePathFileName, destinationPathFileName, OverwriteMode.DoNotOverwrite);
        }

        public static void CopyDbfHeader(string sourcePathFileName, string destinationPathFileName, OverwriteMode overwriteMode)
        {
            GeoDbf dBaseEngine = new GeoDbf(sourcePathFileName);
            dBaseEngine.Open();
            Collection<DbfColumn> dbfColumns = new Collection<DbfColumn>();
            for (int i = 1; i <= dBaseEngine.ColumnCount; i++)
            {
                DbfColumn dbfColumn = dBaseEngine.GetColumn(i);

                dbfColumns.Add(dbfColumn);
            }
            dBaseEngine.Close();
            CreateDbfFile(destinationPathFileName, dbfColumns, overwriteMode, dBaseEngine.Encoding);
        }

        public static void CreateDbfFile(string dbfPathFileName, IEnumerable<DbfColumn> dbfColumns)
        {
            CreateDbfFile(dbfPathFileName, dbfColumns, OverwriteMode.DoNotOverwrite, Encoding.Default);
        }

        public static void CreateDbfFile(string dbfPathFileName, IEnumerable<DbfColumn> dbfColumns, OverwriteMode overwriteMode)
        {
            CreateDbfFile(dbfPathFileName, dbfColumns, overwriteMode, Encoding.Default);
        }

        public static void CreateDbfFile(string dbfPathFileName, IEnumerable<DbfColumn> dbfColumns, OverwriteMode overwriteMode, Encoding encoding)
        {
            if (File.Exists(dbfPathFileName))
            {
                if (overwriteMode == OverwriteMode.Overwrite)
                {
                    File.SetAttributes(dbfPathFileName, FileAttributes.Normal);
                    File.Delete(dbfPathFileName);
                }
                else
                {
                    return;
                }
            }

            GeoDbf dbfEngine = new GeoDbf(dbfPathFileName, DbfReadWriteMode.ReadWrite, encoding, CultureInfo.CurrentCulture);
            dbfEngine.CreateDBFFile(dbfPathFileName, FileAccess.ReadWrite);
            
            List<DbfColumn> columns = new List<DbfColumn>(dbfColumns);
            foreach (DbfColumn column in columns)
            {
                dbfEngine.AddField(column.ColumnName, column.ColumnType, column.Length, column.DecimalLength);
            }
            dbfEngine.Close();
        }

        public DbfColumn GetColumn(string columnName)
        {
            DbfColumnType type = GetFieldType(columnName);
            int length = GetFieldLength(columnName);
            int decimalLength = GetFieldDecimalLength(columnName);

            return new DbfColumn(columnName, type, length, decimalLength);
        }

        public DbfColumn GetColumn(int columnNumber)
        {
            string columnName = GetColumnName(columnNumber);

            DbfColumnType type = GetFieldType(columnNumber - 1);
            int length = GetFieldLength(columnNumber - 1);
            int decimalLength = GetFieldDecimalLength(columnNumber - 1);

            return new DbfColumn(columnName, type, length, decimalLength);
        }

        public Dictionary<string, object> ReadRecord(int recordNumber)
        {
            Dictionary<string, object> result = new Dictionary<string, object>();
            Dictionary<string, string> recordAsString = ReadRecordAsString(recordNumber);
            object value = null;
            foreach (string columnName in recordAsString.Keys)
            {
                DbfColumnType type = GetFieldType(columnName);
                switch (type)
                {
                    case DbfColumnType.String:
                        value = ReadFieldAsString(recordNumber, columnName);
                        break;
                    case DbfColumnType.Double:
                        value = ReadFieldAsDouble(recordNumber, columnName);
                        break;
                    case DbfColumnType.Logical:
                        value = ReadFieldAsBoolean(recordNumber, columnName);
                        break;
                    case DbfColumnType.Integer:
                        value = ReadFieldAsInteger(recordNumber, columnName);
                        break;
                    case DbfColumnType.Memo:
                        value = ReadFieldAsString(recordNumber, columnName);
                        break;
                    case DbfColumnType.Date:
                        value = ReadFieldAsDateTime(recordNumber, columnName);
                        break;
                    case DbfColumnType.Null:
                    default:
                        break;
                }
                result.Add(columnName, value);
            }

            return result;
        }

        public Boolean ReadFieldAsBoolean(int recordNumber, int columnNumber)
        {
            string value = ReadFieldAsString(recordNumber, columnNumber);

            return Boolean.Parse(value);
        }

        public Boolean ReadFieldAsBoolean(int recordNumber, string columnName)
        {
            string value = ReadFieldAsString(recordNumber, columnName);

            return Boolean.Parse(value);
        }

        public DateTime ReadFieldAsDateTime(int recordNumber, string columnName)
        {
            string value = ReadFieldAsString(recordNumber, columnName);

            return DateTime.Parse(value, CultureInfo);
        }

        public DateTime ReadFieldAsDateTime(int recordNumber, int columnNumber)
        {
            string value = ReadFieldAsString(recordNumber, columnNumber);

            return DateTime.Parse(value, CultureInfo);
        }

        public Double ReadFieldAsDouble(int recordNumber, string columnName)
        {
            string value = ReadFieldAsString(recordNumber, columnName);

            return Double.Parse(value, CultureInfo);
        }

        public Double ReadFieldAsDouble(int recordNumber, int columnNumber)
        {
            string value = ReadFieldAsString(recordNumber, columnNumber);

            return Double.Parse(value, CultureInfo);
        }

        public int ReadFieldAsInteger(int recordNumber, int columnNumber)
        {
            string value = ReadFieldAsString(recordNumber, columnNumber);

            return int.Parse(value, CultureInfo);
        }

        public int ReadFieldAsInteger(int recordNumber, string columnName)
        {
            string value = ReadFieldAsString(recordNumber, columnName);

            return int.Parse(value, CultureInfo);
        }

        public string ReadFieldAsString(int recordNumber, string columnName)
        {
            return ReadFieldValue(recordNumber, columnName);
        }

        public string ReadFieldAsString(int recordNumber, int columnNumber)
        {
            return ReadFieldValue(recordNumber, columnNumber - 1);
        }

        public void WriteField(int recordNumber, string columnName, Double value)
        {
            WriteFieldValue(recordNumber, columnName, value);
        }

        public void WriteField(int recordNumber, int columnNumber, int value)
        {
            WriteFieldValue(recordNumber, columnNumber - 1, value);
        }

        public void WriteField(int recordNumber, string columnName, Boolean value)
        {
            WriteFieldValue(recordNumber, columnName, value);
        }

        public void WriteField(int recordNumber, int columnNumber, Double value)
        {
            WriteFieldValue(recordNumber, columnNumber - 1, value);
        }

        public void WriteField(int recordNumber, string columnName, int value)
        {
            WriteFieldValue(recordNumber, columnName, value);
        }

        public void WriteField(int recordNumber, int columnNumber, string value)
        {
            WriteFieldValue(recordNumber, columnNumber - 1, value);
        }

        public void WriteField(int recordNumber, string columnName, string value)
        {
            WriteFieldValue(recordNumber, columnName, value);
        }

        public void WriteField(int recordNumber, int columnNumber, Boolean value)
        {
            WriteFieldValue(recordNumber, columnNumber - 1, value);
        }
        public void WriteField(int recordNumber, int columnNumber, DateTime value)
        {
            WriteFieldValue(recordNumber, columnNumber - 1, value);
        }

        public void WriteField(int recordNumber, string columnName, DateTime value)
        {
            WriteFieldValue(recordNumber, columnName, value);
        }


        public void Open()
        {
            if (_isClosed)
            {
                FileAccess fileAccess = FileAccess.Read;
                if (_readWriteMode == DbfReadWriteMode.ReadWrite)
                {
                    fileAccess = FileAccess.ReadWrite;
                }
                OpenDbfFile(PathFileName, fileAccess);
                _isClosed = false;
            }
        }

        public void Close()
        {
            if (!_isClosed)
            {
                if (_bFileDetached)
                {
                    throw new InvalidOperationException(ErrorOperateInDetachedMode);
                }
                if (_isCreating) 
                {
                    WriteHeader(_haveMemoField);
                    _dbfStream.WriteByte(FileEnd);

                    if (_haveMemoField)
                    {
                        GenerateDbtFile(); 
                    }
                }
                else		
                {
                    Flush();
                    if (_haveMemoField)
                    {
                        if (_dbtStream != null) _dbtStream.Close();
                        _dbtStream = null;
                    }
                }
                if (_dbfStream != null) _dbfStream.Close();
                _dbfStream = null;
                _isClosed = true;

                _recordCount = 0;
                _recordLength = 0;
                _headerLength = 0;
                _columnCount = 0;
                _fieldNameCache.Clear();
            }
        }

        public void Flush()
        {
            CheckOpening(ErrorFlushInCreateMode);

            if (_password != null)
            {
                return;
            }

            if (_isUpdated)
            {
                UpdateHeader();
            }

            if (_isFieldUpdated)
            {
                UpdateFields();
            }

            if (_deletedItems.Count != 0)
            {
                UpdateDeletedArray();
            }

            if (_isCurrentRecordModified & _currentRecord != NoRecord)
            {
                FlushCurrentRecord();
            }
            _dbfStream.Flush();
        }

        public void Pack()
        {
            CheckOpening(ErrorPackInCreateMode);
            if (_password != null)
            {
                throw new InvalidOperationException(ErrorIllegalOperationInEncryptedMode);
            }
            ArrayList memoFields;
            ArrayList deletedArray = GenDeletedIndexArray();
            if (_haveMemoField) 
            {
                memoFields = new ArrayList();
                for (int i = 0; i < _fieldsHeader.Count; i++)
                {
                    if (((DbfColumnInfo)_fieldsHeader[i]).ByteType == 'M') 
                    {
                        memoFields.Add(i);
                    }
                }
                PackDbtFile(deletedArray, memoFields);
                PackDbfFile(deletedArray);
                UpdateMemoRecord(memoFields);
            }
            else
            {
                PackDbfFile(deletedArray);
            }
        }

        public int GetColumnNumber(string columnName)
        {
            Validators.CheckParameterIsNotNull(columnName, "columnName");

            if (_fieldNameCache.ContainsKey(columnName))
            {
                return _fieldNameCache[columnName];
            }
            int nResult = NoRecord;

            if (_isClosed)
            {
                throw new InvalidOperationException(ErrorFileHasClosed);
            }

            for (int i = 1; i <= _fieldsHeader.Count; i++)
            {
                string tmp = GetColumnName(i).Trim().ToUpperInvariant();
                if (columnName.ToUpperInvariant().Trim() == tmp)
                {
                    nResult = i;
                    break;
                }
            }

            _fieldNameCache.Add(columnName, nResult);
            return nResult;
        }

        public string GetColumnName(int columnNumber)
        {
            int zeroBasedColumnNumber = columnNumber - 1;

            if (_isClosed)
            {
                throw new InvalidOperationException(ErrorFileHasClosed);
            }

            if ((zeroBasedColumnNumber < 0) | (zeroBasedColumnNumber >= _columnCount))
            {
                return null;
            }

            byte[] tmpFieldName = new byte[10];
            int nStartPos = FileHeadLength + FieldHeadLength * zeroBasedColumnNumber;

            for (int i = 0; i < tmpFieldName.Length; i++)
            {
                tmpFieldName[i] = _headerBytes[nStartPos + i];
            }
            return GetValidSubString(tmpFieldName);
        }

        public void DeleteRecord(int recordNumber)
        {
            int recordIndexZero = recordNumber - 1;

            CheckOpening(ErrorDeleteInCreateMode);

            if (_password != null)
            {
                throw new InvalidOperationException(ErrorIllegalOperationInEncryptedMode);
            }

            if (recordIndexZero < 0 || recordIndexZero >= _recordCount || _recordCount == 0)
            {
                throw new InvalidOperationException(ErrorIndexOverBound);
            }
            _isUpdated = true;
            if (!_deletedItems.Contains(recordIndexZero))
            {
                _deletedItems.Add(recordIndexZero);
            }
        }

        public void UpdateColumnName(int columnNumber, string newColumnName)
        {
            if (columnNumber >= _columnCount || columnNumber < 0)
            {
                throw new ArgumentOutOfRangeException("columnNumber", ErrorIndexOverBound);
            }

            if (_isClosed)
            {
                throw new InvalidOperationException(ErrorFileHasClosed);
            }

            string orginalColumnName = GetColumnName(columnNumber);

            int nStartPos = FileHeadLength + FieldHeadLength * (columnNumber - 1);

            if (_encoding == null)
            {
                _encoding = Encoding.Default;
            }
            byte[] arrFieldName = _encoding.GetBytes(newColumnName);

            if (arrFieldName.Length > 10)
            {
                throw new ArgumentException(ErrorFieldNameIllegal);
            }


            for (int i = 0; i < arrFieldName.Length; i++)
            {
                _headerBytes[nStartPos + i] = arrFieldName[i];
            }

            AddBlank(ref _headerBytes, nStartPos, arrFieldName.Length, 10, ByteBlankField);

            _fieldNameCache.Remove(orginalColumnName);
            _fieldNameCache.Add(newColumnName, columnNumber);
            _isFieldUpdated = true;
            
        }

        public void UndeleteRecord(int recordNumber)
        {
            int recordIndexZero = recordNumber - 1;

            CheckOpening(ErrorDeleteInCreateMode);

            if (_password != null)
            {
                throw new InvalidOperationException(ErrorIllegalOperationInEncryptedMode);
            }

            if (recordIndexZero < 0 || recordIndexZero >= _recordCount || _recordCount == 0)
            {
                throw new InvalidOperationException(ErrorIndexOverBound);
            }
            long lOffset = (long)recordIndexZero * _recordLength + _headerLength;
            _dbfStream.Seek(lOffset, SeekOrigin.Begin);
            _dbfStream.WriteByte(Valid);
            _dbfStream.Flush();

            if (_deletedItems.Contains(recordIndexZero))
            {
                _deletedItems.Remove(recordIndexZero);
            }
        }

        public bool IsRecordDeleted(int recordNumber)
        {
            int recordIndexZero = recordNumber - 1;

            CheckOpening(ErrorCheckRecordInCreateMode);

            if (recordIndexZero >= _recordCount)
            {
                return false;
            }

            int byteMark;
            long lStartPos;
            if (_password == null)
            {
                lStartPos = _headerLength + (long)_recordLength * recordIndexZero;
                _dbfStream.Seek(lStartPos, SeekOrigin.Begin);
                byteMark = _dbfStream.ReadByte();
            }
            else
            {
                byteMark = 0;
            }

            if (byteMark == Deleted)
            {
                return true;
            }
            return false;
        }

        public void AddEmptyRecord()
        {
            CheckOpening(ErrorWriteInCreateMode);
            if (_password != null)
            {
                throw new InvalidOperationException(ErrorIllegalOperationInEncryptedMode);
            }

            if (_columnCount < 1)
            {
                throw new InvalidOperationException(ErrorNoFieldsExist);
            }

            object[] blankObjects = new object[_columnCount];
            _recordCount++;
            WriteRecord(_recordCount, blankObjects);
        }

        public Dictionary<string, string> ReadRecordAsString(int recordNumber)
        {
            int recordIndexZero = recordNumber - 1;

            CheckOpening(ErrorReadInCreateMode);

            if (recordIndexZero >= _recordCount)
            {
                throw new InvalidOperationException(ErrorIndexOverBound);
            }

            byte[] byteNativeAttribute = ReadNativeAttributesInBytes(recordIndexZero);
            if (byteNativeAttribute == null)
            {
                throw new InvalidOperationException(ErrorCannotGetRecord);
            }

            Dictionary<string, string> returnDictionary = new Dictionary<string, string>();
           
            DbfColumnInfo field;
            string strResult;
            for (int i = 0; i < _columnCount; i++)
            {
                field = (DbfColumnInfo)_fieldsHeader[i];
                string fieldName = GetColumnName(i + 1);
                strResult = GetValidSubString(byteNativeAttribute, field.Offset, field.Size);
                if (GetFieldType(i) != DbfColumnType.Memo) 
                {
                    
                    strResult = strResult.Trim();

                    if (GetFieldType(i) == DbfColumnType.Double)
                    {
                        strResult = strResult.Replace(".", _cultureInfo.NumberFormat.NumberDecimalSeparator);
                    }
                    returnDictionary.Add(fieldName, strResult);
                }
                else 
                {
                    int iBlock = ConvertStr2Int32(strResult.Trim());
                    string dbtValue = ReadDbtRecord(iBlock);
                    returnDictionary.Add(fieldName, dbtValue);
                }
            }
            return returnDictionary;
        }

        public void WriteRecord(int recordNumber, IEnumerable<object> values)
        {
            Validators.CheckParameterIsNotNull(values, "values");

            Collection<object> tempValues = new Collection<object>();
            foreach (object item in values)
            {
                tempValues.Add(item);
            }
            int recordIndexZero = recordNumber - 1;

            CheckOpening(ErrorWriteInCreateMode);

            if (_password != null)
            {
                throw new InvalidOperationException(ErrorIllegalOperationInEncryptedMode);
            }

            if ((recordIndexZero < 0) | (recordIndexZero > _recordCount))
            {
                throw new InvalidOperationException(ErrorIndexOverBound);
            }
            if (IsRecordDeleted(recordNumber))
            {
                throw new InvalidOperationException(ErrorOperateOnRemovedRecord);
            }

            if (tempValues.Count != _fieldsHeader.Count)
            {
                throw new InvalidOperationException(ErrorArgumentsNumberIllegal);
            }

            _isCurrentRecordModified = true;
            _isUpdated = true;
            Switch2RightRecord(recordIndexZero);
            for (int i = 0; i < tempValues.Count; i++)
            {
                if (GetFieldType(i) == DbfColumnType.Memo && tempValues[i] != null) 
                {

                    byte[] byteTmp = new byte[4];
                    _dbtStream.Seek(0, SeekOrigin.Begin);
                    _dbtStream.Read(byteTmp, 0, 4);
                    int iNextAvailableBlock = ReadIntFromBytes(byteTmp, 0);

                    Write2CurrentRecord(i, iNextAvailableBlock);
                    Write2Dbt(tempValues[i], iNextAvailableBlock);
                }
                else  
                {
                    Write2CurrentRecord(i, tempValues[i]);
                }
            }
        }

        public void Open(FileMode fileMode, FileAccess fileAccess)
        {
           
            _cultureInfo = CultureInfo.CurrentCulture;

            if (fileMode == FileMode.CreateNew && _password != null)
            {
                throw new InvalidOperationException(ErrorEncryptInCreateMode);
            }

            if (fileMode == FileMode.Open)
            {
                OpenDbfFile(PathFileName, fileAccess);
            }
            else if (fileMode == FileMode.CreateNew)
            {
                CreateDBFFile(PathFileName, fileAccess);
            }
            else
            {
                throw new InvalidOperationException(ErrorFileMode);
            }

            _isClosed = false;
        }


        internal void AddStringField(string columnName, int length)
        {
            AddField(columnName, DbfColumnType.String, length, 0);
        }

        public void AddIntField(string columnName, int length)
        {
            AddField(columnName, DbfColumnType.Integer, length, 0);
        }

        public void AddDoubleColumn(string columnName, int length, int decimalLength)
        {
            AddField(columnName, DbfColumnType.Double, length, decimalLength);
        }

        public void AddBoolColumn(string columnName)
        {
            AddField(columnName, DbfColumnType.Logical, 1, 0);
        }

        internal void AddMemoField(string columnName, int length)
        {
            AddField(columnName, DbfColumnType.Memo, length, 0);
        }

        public void AddDateField(string columnName)
        {
            AddField(columnName, DbfColumnType.Date, 8, 0);
        }

        private void AddField(string columnName, DbfColumnType columnType, int length, int decimalLength)
        {    
            string strErrorMsg = GetInputErrorMsg(columnName, columnType, length, decimalLength);
            if (_readWriteMode == DbfReadWriteMode.ReadOnly)
            {
                strErrorMsg = ErrorDbfIsReadonly;
            }
            if (strErrorMsg != null)
            {
                Close();
                throw new InvalidOperationException(strErrorMsg);
            }

            if (_isCreating)
            {
                AddField2BlankFile(columnName, columnType, length, decimalLength);
            }
            else
            {
                AddField2ExistedFile(columnName, columnType, length, decimalLength);
            }
        }

        private DbfColumnType GetFieldType(string fieldName)
        {
            int index = GetColumnNumber(fieldName);
            if (index == -1)
            {
                throw new InvalidOperationException(ErrorFieldNameNotExist);
            }
            return GetFieldType(index - 1);
        }

        private DbfColumnType GetFieldType(int fieldIndex)
        {
            if (_isClosed)
            {
                throw new InvalidOperationException(ErrorFileHasClosed);
            }

            if ((fieldIndex < 0) | (fieldIndex >= _columnCount))
            {
                throw new ArgumentOutOfRangeException("fieldIndex", "message");
            }

            DbfColumnInfo tmpField = (DbfColumnInfo)_fieldsHeader[fieldIndex];

            DbfColumnType eResult;
            switch (tmpField.ByteType)
            {
                case ByteL: eResult = DbfColumnType.Logical;
                    break;
                case ByteM: eResult = DbfColumnType.Memo;
                    break;
                case ByteN:
                case ByteF: eResult = DbfColumnType.Double;
                    if (tmpField.Decimals > 0)
                    {
                        eResult = DbfColumnType.Double;
                    }
                    else
                    {
                        eResult = DbfColumnType.Integer;
                    }
                    break;
                case ByteD: eResult = DbfColumnType.Date;
                    break;
                case ByteC: eResult = DbfColumnType.String;
                    break;
                default:
                    throw new InvalidOperationException(ErrorTypeNotSupported);
            }
            return eResult;
        }

        private int GetFieldLength(string fieldName)
        {
            int index = GetColumnNumber(fieldName);
            if (index == -1)
            {
                throw new InvalidOperationException(ErrorFieldNameNotExist);
            }
            return GetFieldLength(index - 1);
        }

        private int GetFieldLength(int fieldIndex)
        {
            DbfColumnInfo fieldInfo = (DbfColumnInfo)_fieldsHeader[fieldIndex];

            return fieldInfo.Size;
        }

        private int GetFieldDecimalLength(string fieldName)
        {
            int index = GetColumnNumber(fieldName);
            if (index == -1)
            {
                throw new InvalidOperationException(ErrorFieldNameNotExist);
            }
            return GetFieldDecimalLength(index - 1);
        }

        private int GetFieldDecimalLength(int fieldIndex)
        {
            DbfColumnInfo fieldInfo = (DbfColumnInfo)_fieldsHeader[fieldIndex];

            return fieldInfo.Decimals;
        }

        private string ReadFieldValue(int recordIndex, int fieldIndex)
        {
            int recordIndexZero = recordIndex - 1;

            CheckOpening(ErrorReadInCreateMode);

            if (recordIndexZero >= _recordCount)
            {
                throw new InvalidOperationException(ErrorIndexOverBound);
            }

            DbfColumnType type = GetFieldType(fieldIndex);
		
            string strNativeAttribute = ReadNativeAttribute(recordIndexZero, fieldIndex);

            if (type != DbfColumnType.Memo) 
            {
                strNativeAttribute = strNativeAttribute.Trim();
                if (type == DbfColumnType.Double)
                {
                    strNativeAttribute = strNativeAttribute.Replace(".", _cultureInfo.NumberFormat.NumberDecimalSeparator);
                }
                return strNativeAttribute;
            }
            int iBlock = ConvertStr2Int32(strNativeAttribute.Trim());
            return ReadDbtRecord(iBlock);
        }

        private string ReadFieldValue(int recordIndex, string fieldName)
        {
            int index = GetColumnNumber(fieldName);
            if (index == -1)
            {
                throw new InvalidOperationException(ErrorFieldNameNotExist);
            }
            return ReadFieldValue(recordIndex, index - 1);
        }


        private void WriteFieldValue(int recordIndex, int fieldIndex, object value)
        {
            int recordIndexZero = recordIndex - 1;

            CheckOpening(ErrorWriteInCreateMode);
            if (_password != null)
            {
                throw new InvalidOperationException(ErrorIllegalOperationInEncryptedMode);
            }
            if ((recordIndexZero < 0) | (recordIndexZero > _recordCount) |
                (fieldIndex < 0) | (fieldIndex > _fieldsHeader.Count))
            {
                throw new InvalidOperationException(ErrorIndexOverBound);
            }
            if (IsRecordDeleted(recordIndex))
            {
                throw new InvalidOperationException(ErrorOperateOnRemovedRecord);
            }

            _isCurrentRecordModified = true;
            _isUpdated = true;

            Switch2RightRecord(recordIndexZero);
            if (GetFieldType(fieldIndex) == DbfColumnType.Memo && value != null) 
            {
                
                byte[] byteTmp = new byte[4];
                _dbtStream.Seek(0, SeekOrigin.Begin);
                _dbtStream.Read(byteTmp, 0, 4);
                int iNextAvailableBlock = ReadIntFromBytes(byteTmp, 0);

                Write2CurrentRecord(fieldIndex, iNextAvailableBlock);
                Write2Dbt(value, iNextAvailableBlock);
            }
            else
            {
                Write2CurrentRecord(fieldIndex, value);
            }
        }

        private void WriteFieldValue(int recordIndex, string fieldName, object value)
        {
            int zeroBasedFieldIndex = GetColumnNumber(fieldName) - 1;
            if (zeroBasedFieldIndex == -1)
            {
                throw new InvalidOperationException(ErrorFieldNameNotExist);
            }
            WriteFieldValue(recordIndex, zeroBasedFieldIndex, value);
        }

        private GeoDbf CopyDBaseStructure(string newPathFileName)
        {
            if (_isClosed)
            {
                throw new InvalidOperationException(ErrorFileHasClosed);
            }
            if (_password != null)
            {
                throw new InvalidOperationException(ErrorIllegalOperationInEncryptedMode);
            }

            GeoDbf newFile = new GeoDbf(newPathFileName, DbfReadWriteMode.ReadWrite, _encoding, CultureInfo);
            newFile.CreateDBFFile(newPathFileName, FileAccess.ReadWrite);
           
            newFile._headerBytes = _headerBytes;
            GetTime(ref _headerBytes);

            newFile._headerBytes[4] = 0;
            newFile._headerBytes[5] = 0;
            newFile._headerBytes[6] = 0;
            newFile._headerBytes[7] = 0;

            newFile._headerLength = _headerLength;
            newFile._recordLength = _recordLength;
            newFile._columnCount = _columnCount;

            InitFieldsHeaderInfo(newFile._headerBytes, newFile);

            newFile._dbfStream.Seek(0, SeekOrigin.Begin);
            newFile._dbfStream.Write(_headerBytes, 0, _headerBytes.Length);

            if (newFile._headerBytes[0] == HeaderTypeWithMemo)
            {
                newFile._haveMemoField = true;
            }
            return newFile;
        }
        

       
        private void OpenDbfFile(string fileName, FileAccess access)
        {
            _dbfStream = OpenDbfStream(fileName, FileMode.Open, access);

            byte[] tmpFileHeader = new byte[FileHeadLength];

            _dbfStream.Read(tmpFileHeader, 0, FileHeadLength);
            CheckEncryptFlag(tmpFileHeader[EncryptLabelPosition]);

            InitFileHeaderInfo(tmpFileHeader, this);

            if (tmpFileHeader[0] == HeaderTypeWithMemo)
            {
                _haveMemoField = true;
                _dbtFileName = Path.ChangeExtension(fileName, ".dbt");
                _dbtStream = OpenDbtStream(_dbtFileName, FileMode.Open, access);
            }
            else if (tmpFileHeader[0] == HeaderTypeWithoutMemo)
            {
                _haveMemoField = false;
            }
            else
            {
                throw new InvalidOperationException(ErrorDbfTypeNotSupported);
            }

            _headerBytes = GetHeaderBytes();

            _isClosed = false;
            InitFieldsHeaderInfo(_headerBytes, this);

            _bFileDetached = false;

            _deletedItems = new ArrayList();

            _isCreating = false;
            _isUpdated = false;
            _isClosed = false;

            _isCurrentRecordModified = false;
            _currentRecord = NoRecord;
            _currentRecords = new byte[_recordLength];

            for (int i = 1; i <= _fieldsHeader.Count; i++)
            {
                string columnName = GetColumnName(i).Trim().ToUpperInvariant();
                if (_fieldNameCache.ContainsKey(columnName))
                {
                    columnName = ColumnFilter.GetColumnNameAlias(columnName, _fieldNameCache.Keys);
                }

                _fieldNameCache.Add(columnName, i);
            }

        }

       

        private Stream OpenDbfStream(string fileName, FileMode fileMode, FileAccess fileAccess)
        {
            StreamLoadingEventArgs args = new StreamLoadingEventArgs(_pathFileName, "DBF File");
            OnStreamLoading(args);

            Stream newStream;
            if (args.Stream == null)
            {
                newStream = new FileStream(fileName, fileMode, fileAccess);
            }
            else
            {
                newStream = args.Stream;
            }
            _isClosed = false;
            _bFileDetached = false;
            return newStream;
        }

        private Stream OpenDbtStream(string fileName, FileMode fileMode, FileAccess fileAccess)
        {
            StreamLoadingEventArgs args = new StreamLoadingEventArgs(Path.ChangeExtension(_pathFileName, ".dbt"), "DBT File");
            OnStreamLoading(args);

            Stream newStream;
            if (args.Stream == null)
            {
                newStream = new FileStream(fileName, fileMode, fileAccess);
            }
            else
            {
                newStream = args.Stream;
            }

            return newStream;
        }


        #region Init Methods

       
        private void Init(string fileName, FileMode mode, FileAccess access, string strPassWord)
        {
            _password = strPassWord;
            _cultureInfo = CultureInfo.CurrentCulture;

            if (mode == FileMode.CreateNew && strPassWord != null)
            {
                throw new InvalidOperationException(ErrorEncryptInCreateMode);
            }

            if (mode == FileMode.Open)
            {
                OpenDbfFile(fileName, access);
            }
            else if (mode == FileMode.CreateNew)
            {
                CreateDBFFile(fileName, access);
            }
            else
            {
                throw new InvalidOperationException(ErrorFileMode);
            }
        }

        private void CreateDBFFile(string fileName, FileAccess access)
        {
            if (access == FileAccess.Read)
            {
                throw new InvalidOperationException(ErrorCreateReadConflict);
            }

            _dbfStream = OpenDbfStream(fileName, FileMode.CreateNew, access);
            _pathFileName = fileName;

            _recordCount = 0;
            _columnCount = 0;
            _recordLength = 1;
            _headerLength = LeastHeaderLength;

            _bFileDetached = false;
            _fieldsHeader = new ArrayList();
            _headerBytes = new byte[_headerLength];
           
            _haveMemoField = false;
            _isCreating = true;
            _isClosed = false;
        }


        private void GenerateDbtFile()
        {
            string strFileName = Path.GetDirectoryName(_pathFileName)
                + "\\" + Path.GetFileNameWithoutExtension(_pathFileName) + DbtSuffix;
            byte[] header = new byte[BlockSize];

            header[0] = 1; 
            header[21] = 2;

            FileAccess fileAccess = FileAccess.Read;
            if (_readWriteMode == DbfReadWriteMode.ReadWrite)
            {
                fileAccess = FileAccess.ReadWrite;
            }
            Stream fs = OpenDbtStream(strFileName, FileMode.Create, fileAccess);
            fs.Write(header, 0, header.Length);
            fs.Close();
        }


        private void InitFileHeaderInfo(byte[] arrFileHeader, GeoDbf dbfFile)
        {
            dbfFile._headerLength = arrFileHeader[8] + arrFileHeader[9] * ByteValue;
            dbfFile._recordLength = arrFileHeader[10] + arrFileHeader[11] * ByteValue;
          
            if (dbfFile._password == null)
            {
                if (dbfFile._recordLength != 1)
                {
                    dbfFile._recordCount = (int)((dbfFile._dbfStream.Length - dbfFile._headerLength)
                        / dbfFile._recordLength);
                }
                else
                {
                    dbfFile._recordCount = (int)(dbfFile._dbfStream.Length - dbfFile._headerLength - 1);
                }
            }
           
            else
            {
                dbfFile._recordCount = ReadIntFromBytes(arrFileHeader, 4);
            }
            dbfFile._columnCount = (_headerLength - FileHeadLength) / FileHeadLength;
        }


        private void InitFieldsHeaderInfo(byte[] arrHeader, GeoDbf dbfFile)
        {
            dbfFile._fieldsHeader = new ArrayList();

            int nStartPos;
            DbfColumnInfo tmpField = new DbfColumnInfo();
            for (int nField = 0; nField < _columnCount; nField++)
            {
                nStartPos = nField * FieldHeadLength + FileHeadLength;

                if ((arrHeader[nStartPos + 11] == 'N') |
                    (arrHeader[nStartPos + 11] == 'F'))
                {
                    tmpField.Size = arrHeader[nStartPos + 16];
                    tmpField.Decimals = arrHeader[nStartPos + 17];
                }
                else
                {
                    tmpField.Size = arrHeader[nStartPos + 16] +
                        arrHeader[nStartPos + 17] * ByteValue;
                    tmpField.Decimals = 0;
                }

                tmpField.ByteType = arrHeader[nStartPos + 11];

                if (nField == 0)
                {
                    tmpField.Offset = 1;
                }
                else
                {
                    tmpField.Offset = ((DbfColumnInfo)_fieldsHeader[nField - 1]).Offset +
                        ((DbfColumnInfo)_fieldsHeader[nField - 1]).Size;
                }

                dbfFile._fieldsHeader.Add(tmpField);
            }
        }


        #endregion

        #region Read Methods

     
        private string ReadNativeAttribute(int iRecord, int iField)
        {
            byte[] bytesResult = ReadNativeAttributeInBytes(iRecord, iField);

            return GetValidSubString(bytesResult);
        }

        private string GetValidSubString(byte[] bytesIn)
        {
            return GetValidSubString(bytesIn, 0, bytesIn.Length);
        }

        
        private string GetValidSubString(byte[] bytesIn, int nIndex, int length)
        {
            if (_encoding == null)
            {
                _encoding = Encoding.Default;
            }
            string tmp = _encoding.GetString(bytesIn, nIndex, length);

            int i = tmp.Length - 1;
            for (; i >= 0; i--)
            {
                if (tmp[i] != '\0')
                {
                    break;
                }
            }

            return tmp.Substring(0, i + 1);

        }


        private byte[] ReadNativeAttributeInBytes(int iRecord, int iField)
        {
            if (_password == null)
            {
                return ReadNativeAttributeInBytes_UnEncrypted(iRecord, iField);
            }
            return null;
        }


        private byte[] ReadNativeAttributeInBytes_UnEncrypted(int iRecord, int iField)
        {
            if ((iRecord < 0) | (iField < 0)
                | (iRecord >= _recordCount) | (iField >= _columnCount))
            {
                return null;
            }

            int nOffSet = ((DbfColumnInfo)_fieldsHeader[iField]).Offset;
            int nSize = ((DbfColumnInfo)_fieldsHeader[iField]).Size;

            long lRecordOffset = (long)_recordLength * iRecord +
                _headerLength + nOffSet;

            _dbfStream.Seek(lRecordOffset, SeekOrigin.Begin);
            byte[] arrResult = new byte[nSize];
            _dbfStream.Read(arrResult, 0, nSize);

            return arrResult;
        }


        private byte[] ReadNativeAttributesInBytes(int iRecord)
        {
            if ((iRecord < 0) | (iRecord >= _recordCount))
            {
                return null;
            }

            if (_password == null)
            {
                return ReadNativeAttributes_UnEncrypted(iRecord);
            }
            return null;
        }


        private byte[] ReadNativeAttributes_UnEncrypted(int iRecord)
        {
            long lRecordOffset = (long)_recordLength * iRecord + _headerLength;

            _dbfStream.Seek(lRecordOffset, SeekOrigin.Begin);
            byte[] arrResult = new byte[_recordLength];
            _dbfStream.Read(arrResult, 0, _recordLength);

            return arrResult;
        }


        private string ReadDbtRecord(int iBlock)
        {
            if (_password == null)
            {
                return ReadDBTRecord_UnEncrypted(iBlock);
            }
            return null;
        }


        private string ReadDBTRecord_UnEncrypted(int iBlock)
        {
            if (iBlock == 0)
            {
                return "";
            }
            if (iBlock < 0)
            {
                throw new InvalidOperationException(ErrorBlockIndexIllegal);
            }
            long lStartPos = (long)iBlock * BlockSize + 4;
            byte[] tmpLength = new byte[4];
            _dbtStream.Seek(lStartPos, SeekOrigin.Begin);
            _dbtStream.Read(tmpLength, 0, 4);

            int tmpRecordLength = ReadIntFromBytes(tmpLength, 0);
            int tmpValubleLength = tmpRecordLength - BlockHeaderLength;

            byte[] tmpRecord = new byte[tmpValubleLength];
            int count = _dbtStream.Read(tmpRecord, 0, tmpRecord.Length);

            if (_encoding == null)
            {
                _encoding = Encoding.Default;
            }
            return _encoding.GetString(tmpRecord, 0, count);
        }



        #endregion

        #region Write Methods

        private void FlushCurrentRecord()
        {
            _isCurrentRecordModified = false;
            long lRecordOffset = (long)_recordLength * _currentRecord + _headerLength;

            _dbfStream.Seek(lRecordOffset, SeekOrigin.Begin);
            _dbfStream.Write(_currentRecords,
                0, _currentRecords.Length);
            if (_currentRecord == _recordCount - 1)
            {
                _dbfStream.WriteByte(FileEnd);
            }
        }

       
        private static void AddBlank(ref byte[] arrUpdateBytes, int nOffset, int nWrittenBytes,
            int nSize, byte byteBlank)
        {
            for (int i = nWrittenBytes; i < nSize; i++)
            {
                arrUpdateBytes[nOffset + i] = byteBlank;
            }
        }

      
        private void Write2CurrentRecord(int iField, object objValue)
        {
            if (_encoding == null)
            {
                _encoding = Encoding.Default;
            }
            DbfColumnInfo field = (DbfColumnInfo)_fieldsHeader[iField];

            _isCurrentRecordModified = true;
            if (objValue == null)
            {
                _currentRecords[0] = Valid;
                AddBlank(ref _currentRecords, field.Offset, 0, field.Size, ByteBlankRecord);
                return;
            }
            DbfColumnType type = GetFieldType(iField);
            string strValue = GetStrByObj(iField, type, objValue).Trim();

            int nWholeSize = ((DbfColumnInfo)_fieldsHeader[iField]).Size;
            if ((type != DbfColumnType.Logical)
                && _encoding.GetByteCount(strValue) > nWholeSize) 
            {
                throw new InvalidOperationException(ErrorInputLengthOutOfBound);
            }

            byte[] buffer = _encoding.GetBytes(strValue);


            for (int i = 0; i < buffer.Length; i++)
            {
                _currentRecords[field.Offset + i] = buffer[i];
            }

            AddBlank(ref _currentRecords, field.Offset, buffer.Length, field.Size, ByteBlankRecord);
        }


        private void Write2Dbt(object value, int nOriginalBlock)
        {
            string strValue;
            if (value == null)
            {
                return;
            }
            strValue = value.ToString();
           
            if (_encoding == null)
            {
                _encoding = Encoding.Default;
            }
            byte[] buffer = _encoding.GetBytes(strValue);
            int contentLength = buffer.Length;

            int nLength = contentLength + BlockHeaderLength;
            int nBlock = (nLength + 1) / BlockSize + 1; 
            int newBlockNum = nBlock + nOriginalBlock;

            byte[] header = new byte[4];
            WriteInt2Bytes(ref header, 0, newBlockNum);

            _dbtStream.Seek(0, SeekOrigin.Begin);
            _dbtStream.Write(header, 0, 4); 

            byte[] byteRecord = new byte[nBlock * BlockSize];

            byteRecord[0] = 0xFF;
            byteRecord[1] = 0xFF;
            byteRecord[2] = 0x08;
            byteRecord[3] = 0x00;

            WriteInt2Bytes(ref byteRecord, 4, nLength);

            int i = 0;
            for (; i < contentLength; i++)
            {
                byteRecord[BlockHeaderLength + i] = buffer[i];
            }

            byteRecord[BlockHeaderLength + i] = DbtRecordEnd;
            byteRecord[BlockHeaderLength + i + 1] = DbtRecordEnd;

            _dbtStream.Seek(0, SeekOrigin.End);
            _dbtStream.Write(byteRecord, 0, byteRecord.Length);
            _dbtStream.Flush();
        }


        private void Switch2RightRecord(int iRecord)
        {
            if (iRecord == _recordCount)
            {
                Flush();
                AddEmptyRecord();
            }

            else if (_currentRecord != iRecord)
            {
                Flush();
                long lRecordOffset = (long)_recordLength * iRecord + _headerLength;

                _dbfStream.Seek(lRecordOffset, SeekOrigin.Begin);
                _dbfStream.Read(_currentRecords, 0, _recordLength);

                _currentRecord = iRecord;
            }
        }


        private void WriteHeader(bool bHaveMemoField)
        {
            if (bHaveMemoField)
            {
                _headerBytes[0] = HeaderTypeWithMemo;
            }
            else
            {
                _headerBytes[0] = HeaderTypeWithoutMemo;
            }

            _headerBytes[8] = (byte)(_headerLength % ByteValue);
            _headerBytes[9] = (byte)(_headerLength / ByteValue);
            _headerBytes[10] = (byte)(_recordLength % ByteValue);
            _headerBytes[11] = (byte)(_recordLength / ByteValue);

            GetTime(ref _headerBytes);

            _headerBytes[_headerBytes.Length - 1] = HeaderEndCharacter;

            _dbfStream.Seek(0, SeekOrigin.Begin);
            _dbfStream.Write(_headerBytes, 0, _headerBytes.Length);
        }


        private void UpdateHeader()
        {
            byte[] arrFileHeader = new byte[FileHeadLength];
            _dbfStream.Seek(0, SeekOrigin.Begin);
            _dbfStream.Read(arrFileHeader, 0, FileHeadLength);

            GetTime(ref arrFileHeader);

            WriteInt2Bytes(ref arrFileHeader, 4, _recordCount);

            _isUpdated = false;
            _dbfStream.Seek(0, SeekOrigin.Begin);
            _dbfStream.Write(arrFileHeader, 0, FileHeadLength);
        }

       
        private void UpdateFields()
        {
            _dbfStream.Seek(32, SeekOrigin.Begin);
            _dbfStream.Write(_headerBytes, 32, _headerBytes.Length - 32);
            _isFieldUpdated = false;
        }

        private static string GetDbtFile(string dbf)
        {
            return dbf.Replace(".dbf", ".dbt");
        }


        private static void MoveDbfAfterFieldOperation(string strSrcDbf, string strDestDbf)
        {
            File.Delete(strDestDbf);
            File.Move(strSrcDbf, strDestDbf);

            string strSrcDbt = GetDbtFile(strSrcDbf);
            string strDestDbt = GetDbtFile(strDestDbf);

            if (File.Exists(strSrcDbt) && File.Exists(strDestDbt))
            {
                File.Delete(strSrcDbt);
            }
            else if (File.Exists(strSrcDbt) && !File.Exists(strDestDbt))
            {
                File.Move(strSrcDbt, strDestDbt);
            }
            else if (!File.Exists(strSrcDbt) && File.Exists(strDestDbt))
            {
                File.Delete(strDestDbt);
            }
        }

        private static string GetTmpDbfFile()
        {
            string tempFile = Path.GetTempFileName();
            string strTmpFile = tempFile + ".dbf";
            try
            {
                if (File.Exists(strTmpFile))
                {
                    File.Delete(strTmpFile);
                }
                if (File.Exists(GetDbtFile(strTmpFile)))
                {
                    File.Delete(GetDbtFile(strTmpFile));
                }
            }
            finally
            {
                File.Delete(tempFile);
            }
            return strTmpFile;
        }

       
        private int AddField2ExistedFile(string fieldName, DbfColumnType type,
            int nWidth, int nDecimals)
        {
            if (_password != null)
            {
                throw new InvalidOperationException(ErrorIllegalOperationInEncryptedMode);
            }
            Flush();

            string strTmpFile = GetTmpDbfFile();

            GeoDbf newDbf = CopyDBaseStructure(strTmpFile);

            newDbf.AddField(fieldName, type, nWidth, nDecimals);
            newDbf.Close();

            newDbf = new GeoDbf(strTmpFile, _readWriteMode);
            newDbf.Open();
            byte[] tmpBytes = new byte[_recordLength];

            newDbf._dbfStream.Seek(-1, SeekOrigin.End);
            for (int i = 0; i < _recordCount; i++)
            {
                _dbfStream.Seek(_headerLength + i * _recordLength, SeekOrigin.Begin);
                _dbfStream.Read(tmpBytes, 0, tmpBytes.Length);
                newDbf._dbfStream.Write(tmpBytes, 0, tmpBytes.Length);
                newDbf._dbfStream.Write(new byte[nWidth], 0, nWidth);
            }
            newDbf._dbfStream.WriteByte(FileEnd);
            newDbf._isUpdated = true;
            newDbf._recordCount = _recordCount;
            newDbf.Close();

            string strDbf = _pathFileName;
            Close();

            MoveDbfAfterFieldOperation(strTmpFile, strDbf);

            FileAccess fileAccess = FileAccess.Read;
            if (_readWriteMode == DbfReadWriteMode.ReadWrite)
            {
                fileAccess = FileAccess.ReadWrite;
            }
            Init(strDbf, FileMode.Open, fileAccess, null);
            return _columnCount;
        }

        
        private int AddField2BlankFile(string fieldName, DbfColumnType type,
            int nWidth, int nDecimals)
        {
            DbfColumnInfo tmpField = GetNewField(type, nWidth, nDecimals);
            _fieldsHeader.Add(tmpField);
            if (type == DbfColumnType.Memo)
            {
                _haveMemoField = true;
            }
            _columnCount++;
            _recordLength += nWidth;
            _headerLength += FieldHeadLength;

            _headerBytes = GetNewHeaderArray(fieldName, tmpField);
            return _columnCount - 1;
        }

        #endregion

        #region Pack Methods

       
        private void PackDbfFile(ArrayList deletedArray)
        {
            if (deletedArray.Count != 0)
            {
                string strName = _pathFileName;
                string strTmpFile = Path.GetDirectoryName(_pathFileName) + "\\" +
                    Path.GetFileNameWithoutExtension(_pathFileName) + TmpFileSuffix;
                Stream fsTmpFile = OpenDbfStream(strTmpFile, FileMode.Create,
                    FileAccess.Write);

                CopyHeaderData_DBF(this, ref fsTmpFile);

                deletedArray.Sort();
                PackData_DBF(ref fsTmpFile, deletedArray);

                _recordCount = _recordCount - deletedArray.Count;
                _isUpdated = true;

                fsTmpFile.WriteByte(FileEnd);
                fsTmpFile.Close();
                _dbfStream.Close();

                File.Copy(strTmpFile, _pathFileName, true);
                File.Delete(strTmpFile);
                FileAccess fileAccess = FileAccess.Read;
                if (_readWriteMode == DbfReadWriteMode.ReadWrite)
                {
                    fileAccess = FileAccess.ReadWrite;
                }
                _dbfStream = OpenDbfStream(strName, FileMode.Open, fileAccess);

                Flush();
            }
        }


        private void PackDbtFile(ArrayList deletedArray, ArrayList memoFields)
        {
            Flush();
            ArrayList arrDeletedBlocks = GetDeletedBlocks(deletedArray, memoFields);

            _originalIndex = new ArrayList();
            _newIndex = new ArrayList();

            if (arrDeletedBlocks.Count != 0)
            {
                string strTmpFile = _dbtFileName + TmpFileSuffix;
                Stream fsTmpFile = OpenDbtStream(strTmpFile, FileMode.Create, FileAccess.ReadWrite);

                CopyHeaderData_DBT(_dbtStream, ref fsTmpFile);

                PackData_DBT(ref fsTmpFile, arrDeletedBlocks);

                fsTmpFile.Close();
                _dbtStream.Close();

                File.Copy(strTmpFile, _dbtFileName, true);
                File.Delete(strTmpFile);
                FileAccess fileAccess = FileAccess.Read;
                if (_readWriteMode == DbfReadWriteMode.ReadWrite)
                {
                    fileAccess = FileAccess.ReadWrite;
                }
                _dbtStream = OpenDbtStream(_dbtFileName, FileMode.Open, fileAccess);
            }
        }


        private void PackData_DBF(ref Stream fsDest, ArrayList deletedArray)
        {
            int iWriteTo = 0; 				
            int iStart = 0;   				
            int iEnd = (int)deletedArray[0] - 1;	  
            int nLength = iEnd + 1; 

            CopyData_DBF(_dbfStream, iStart, iEnd, ref fsDest, iWriteTo);

            for (int i = 0; i < deletedArray.Count; i++)
            {
                iStart = (int)deletedArray[i] + 1;
                iWriteTo += nLength;
                if (i < deletedArray.Count - 1) 
                {
                    iEnd = (int)deletedArray[i + 1] - 1;
                    nLength = iEnd - iStart + 1;
                }
                else 
                {
                    iEnd = _recordCount - 1;
                }
                CopyData_DBF(_dbfStream, iStart, iEnd, ref fsDest, iWriteTo);
            }
        }


       
        private void PackData_DBT(ref Stream fsDest, ArrayList deletedBlocks)
        {
            if (deletedBlocks == null || deletedBlocks.Count == 0)
            {
                return;
            }

            deletedBlocks.Sort();

            int iWriteTo = 1; 
            int iStart = 0;  
            int iEnd = (int)deletedBlocks[0] - 1;	  
            int nLength = 0; 

            for (int i = 0; i < deletedBlocks.Count + 1; i++)
            {
                iStart += 1 + nLength;
                iWriteTo += nLength;

                if (i < deletedBlocks.Count) 
                {
                    iEnd = (int)deletedBlocks[i] - 1;
                    nLength = GetBlockCount(iStart, iEnd);
                }
                else 
                {
                    iEnd = (int)(_dbtStream.Length / BlockSize) - 1;
                }
                CopyData_DBT(_dbtStream, iStart, iEnd, ref fsDest, iWriteTo);

                MakeContrastArrays(ref _originalIndex, ref _newIndex,
                    iStart, iEnd, iWriteTo);
            }
            UpdateDbtHeader(fsDest, deletedBlocks.Count);
        }


        private void UpdateMemoRecord(ArrayList memoFields)
        {
            int tmpOriginal;
            int tmpNew;
            string tmpStr;
            for (int j = 0; j < memoFields.Count; j++)
            {
                int iField = (int)memoFields[j];
                for (int m = 0; m < _recordCount; m++)
                {
                    tmpStr = ReadNativeAttribute(m, iField);
                    tmpOriginal = ConvertStr2Int32(tmpStr.Trim());
                    tmpNew = GetNewValue(tmpOriginal);
                    if (tmpNew == tmpOriginal) 
                    {
                        continue;
                    }

                    Switch2RightRecord(m);
                    Write2CurrentRecord(iField, tmpNew);
                }
            }
            Flush();
        }


        private void CopyData_DBF(Stream fsSrc, int iStart, int iEnd,
            ref Stream fsDest, int iWriteTo)
        {
            int nLength = (iEnd - iStart + 1) * _recordLength;
            if (nLength == 0) 
            {
                return;
            }
            byte[] buffer = new byte[nLength];

            fsSrc.Seek((long)_recordLength * iStart + _headerLength,
                SeekOrigin.Begin);
            fsSrc.Read(buffer, 0, nLength);

            fsDest.Seek((long)_recordLength * iWriteTo + _headerLength,
                SeekOrigin.Begin);
            fsDest.Write(buffer, 0, nLength);
        }


        private static void CopyData_DBT(Stream fsSrc, int iStart, int iEnd,
            ref Stream fsDest, int iWriteTo)
        {
            if (iEnd < iStart)
            {
                return;
            }
            int nLength = (iEnd - iStart + 1) * BlockSize;
            byte[] buffer = new byte[nLength];

            fsSrc.Seek((long)BlockSize * iStart, SeekOrigin.Begin);
            fsSrc.Read(buffer, 0, nLength);
            fsDest.Seek((long)BlockSize * iWriteTo, SeekOrigin.Begin);
            fsDest.Write(buffer, 0, nLength);
        }


        private static void UpdateDbtHeader(Stream fsDbt, int nDeletedBlocks)
        {
            byte[] arrFileHeader = new byte[4];
            fsDbt.Seek(0, SeekOrigin.Begin);
            fsDbt.Read(arrFileHeader, 0, 4);

            int nBlockNumber = ReadIntFromBytes(arrFileHeader, 0);
            nBlockNumber -= nDeletedBlocks;

            WriteInt2Bytes(ref arrFileHeader, 0, nBlockNumber);

            fsDbt.Seek(0, SeekOrigin.Begin);
            fsDbt.Write(arrFileHeader, 0, arrFileHeader.Length);
        }


        private int GetNewValue(int originalValue)
        {
            if (_originalIndex.Count == 0)
            {
                return originalValue;
            }
            int tmpIndex = _originalIndex.IndexOf(originalValue);
            if (tmpIndex == -1)
            {
                return originalValue;
            }
            return (int)_newIndex[tmpIndex];
        }


        private static void MakeContrastArrays(ref ArrayList arrOriginal, ref ArrayList arrNew,
            int iOriginalStart, int iOriginalEnd, int iNewStart)
        {
            if (iNewStart == iOriginalStart)
            {
                return;
            }
            int j = iNewStart;
            for (int i = iOriginalStart; i <= iOriginalEnd; i++)
            {
                arrOriginal.Add(i);
                arrNew.Add(j++);
            }
        }


        #endregion

        #region Delete Methods

       
        private ArrayList GetDeletedBlocks(ArrayList deletedArray, ArrayList memoFields)
        {
            ArrayList arrDeletedBlocks = new ArrayList();

            int iField;
            int iBlock;
            int iBlockNum;

            ArrayList arrValuableBlocks = new ArrayList();
            for (int i = 0; i < _recordCount; i++)
            {
                for (int j = 0; j < memoFields.Count; j++)
                {
                    iField = (int)memoFields[j];
                    string tmpStrBlock = ReadNativeAttribute(i, iField);
                    iBlock = ConvertStr2Int32(tmpStrBlock.Trim());

                    if (iBlock == 0) 
                    {
                        continue;
                    }

                    iBlockNum = GetBlockSizeOfRecord(_dbtStream, iBlock);
                    if (deletedArray.Contains(i))
                    {
                        for (int k = 0; k < iBlockNum; k++)
                        {
                            arrDeletedBlocks.Add(iBlock + k);
                        }
                        continue;
                    }

                    for (int k = 0; k < iBlockNum; k++)
                    {
                        arrValuableBlocks.Add(iBlock + k);
                    }
                }
            }

            byte[] byteBlocks = new byte[4];
            _dbtStream.Seek(0, SeekOrigin.Begin);
            _dbtStream.Read(byteBlocks, 0, 4);
            int nBlocks = byteBlocks[3] * ByteValue * ByteValue * ByteValue
                + byteBlocks[2] * ByteValue * ByteValue
                + byteBlocks[1] * ByteValue
                + byteBlocks[0] - 1;

            for (int i = 1; i <= nBlocks; i++)
            {
                if (arrValuableBlocks.Contains(i) || arrDeletedBlocks.Contains(i))
                {
                    continue;
                }
                arrDeletedBlocks.Add(i);
            }
            return arrDeletedBlocks;
        }


        private ArrayList GenDeletedIndexArray()
        {
            ArrayList arrResult = new ArrayList();
            int nMaxLength = _recordLength * MaxRecordsOnceRead;
            byte[] buffer = new byte[nMaxLength]; 

            int nStartRecord = 0; 
            _dbfStream.Seek((long)nStartRecord * _recordLength + _headerLength,
                SeekOrigin.Begin);

            while (nStartRecord < _recordCount)
            {
                int nLeftRecord = _recordCount - nStartRecord;
                int nRecordOffSet;
                if (nLeftRecord < MaxRecordsOnceRead)
                {
                    nRecordOffSet = nLeftRecord;
                }
                else
                {
                    nRecordOffSet = MaxRecordsOnceRead;
                }

                _dbfStream.Read(buffer, 0, nRecordOffSet * _recordLength);
                for (int i = 0; i < nRecordOffSet; i++)
                {
                    if (buffer[i * _recordLength] == Deleted) 
                    {
                        arrResult.Add(i + nStartRecord);
                    }
                }
                nStartRecord += nRecordOffSet;
            }
            return arrResult;
        }


        private void UpdateDeletedArray()
        {
            for (int i = 0; i < _deletedItems.Count; i++)
            {
                int tmpDeletedIndex = (int)_deletedItems[i];
                long lOffset = (long)tmpDeletedIndex * _recordLength + _headerLength;
                _dbfStream.Seek(lOffset, SeekOrigin.Begin);
                _dbfStream.WriteByte(Deleted);
                _dbfStream.Flush();
            }
            _deletedItems.Clear();
        }


        #endregion

        #region Get Properties Record

       
        private static int GetBlockCount(int iStart, int iEnd)
        {
            if (iStart > iEnd)
            {
                return 0;
            }
            return iEnd - iStart + 1;
        }

        private int GetBlockSizeOfRecord(Stream fsDbt, int iBlock)
        {
            fsDbt.Seek((long)iBlock * BlockSize + 4, SeekOrigin.Begin);

            byte[] tmpRecordLength = new byte[4];
            _dbtStream.Read(tmpRecordLength, 0, 4);

            int nByteSize = ReadIntFromBytes(tmpRecordLength, 0) + 2; 
            int nBlockSize = nByteSize / BlockSize + 1;
            return nBlockSize;
        }


        private byte[] GetHeaderBytes()
        {
            byte[] arrHeader = new byte[_headerLength];
            _dbfStream.Seek(0, SeekOrigin.Begin);
            _dbfStream.Read(arrHeader, 0, _headerLength);

            if (_password != null)
            {
                
            }
            return arrHeader;
        }


        private bool FieldsAreValid(ArrayList fields)
        {
            string fieldName;
            bool bResult = true;
            for (int i = 1; i <= fields.Count; i++)
            {
                fieldName = GetColumnName(i).Trim();
                if (!Char.IsLetter(fieldName[0]))
                {
                    bResult = false;
                    break;
                }

            }
            return bResult;
        }


        private void CheckEncryptFlag(byte flag)
        {
            if (flag == Encrypted)
            {
                if (_password == null)
                {
                    throw new InvalidOperationException(ErrorNoPasswordForEncryptedFile);
                }
            }
            else
            {
                if (_password != null)
                {
                    throw new InvalidOperationException(ErrorPasswordForUnencryptedFile);
                }
            }
        }


        private void CheckOpening(string errorMsg)
        {
            if (_isClosed)
            {
                throw new InvalidOperationException(ErrorFileHasClosed);
            }
            if (_bFileDetached)
            {
                throw new InvalidOperationException(ErrorOperateInDetachedMode);
            }
            if (_isCreating)
            {
                throw new InvalidOperationException(errorMsg);
            }
        }


        private DbfColumnInfo GetNewField(DbfColumnType type, int nWidth, int nDecimals)
        {
            DbfColumnInfo tmpField = new DbfColumnInfo();
            tmpField.Offset = _recordLength;
            tmpField.Size = nWidth;
            tmpField.Decimals = nDecimals;

            if (type == DbfColumnType.Logical)
            {
                tmpField.ByteType = ByteL;
            }
            else if (type == DbfColumnType.String)
            {
                tmpField.ByteType = ByteC;
            }
            else if (type == DbfColumnType.Memo)
            {
                tmpField.ByteType = ByteM;
            }
            else if (type == DbfColumnType.Date)
            {
                tmpField.ByteType = ByteD;
            }
            else
            {
                tmpField.ByteType = ByteN;
            }

            return tmpField;
        }


        private byte[] GetNewHeaderArray(string fieldName, DbfColumnInfo field)
        {
            byte[] newHeader = new byte[_headerBytes.Length + FileHeadLength];
            int nStartPos = FileHeadLength + FieldHeadLength * (_columnCount - 1);

            Array.Copy(_headerBytes, 0, newHeader, 0, nStartPos);

            if (_encoding == null)
            {
                _encoding = Encoding.Default;
            }
            byte[] arrFieldName = _encoding.GetBytes(fieldName);

            for (int i = 0; i < arrFieldName.Length; i++)
            {
                newHeader[nStartPos + i] = arrFieldName[i];
            }

            AddBlank(ref newHeader, nStartPos, arrFieldName.Length, 10, ByteBlankField);
            newHeader[nStartPos + 11] = field.ByteType;

            if (field.ByteType == ByteC) // String
            {
                newHeader[nStartPos + 16] = (byte)(field.Size % ByteValue);
                newHeader[nStartPos + 17] = (byte)(field.Size / ByteValue);
            }
            else
            {
                newHeader[nStartPos + 16] = (byte)(field.Size);
                newHeader[nStartPos + 17] = (byte)(field.Decimals);
            }
            return newHeader;
        }


        #endregion

        #region Other Methods

       
        private string GetStrByObj(int iField, DbfColumnType type, object objValue)
        {
            string strValue;
            int nDecimalSize = ((DbfColumnInfo)_fieldsHeader[iField]).Decimals;
            switch (type)
            {
                case (DbfColumnType.Integer):
                    strValue = ConvertStr2Int32Str(objValue.ToString());
                    break;
                case (DbfColumnType.Double):
                    strValue = GetDoubleStrByObj(objValue, nDecimalSize);
                    break;
                case (DbfColumnType.Logical):
                    string strLogic = objValue.ToString().ToUpperInvariant();
                    if (strLogic == "T" || strLogic == "TRUE" || strLogic == "Y" || strLogic == "YES" || strLogic == "1")
                    {
                        strValue = "T";
                    }
                    else if (strLogic == "F" || strLogic == "FALSE" || strLogic == "N" || strLogic == "NO" || strLogic == "0")
                    {
                        strValue = "F";
                    }
                    else
                    {
                        strValue = Convert.ToBoolean(strLogic, CultureInfo.InvariantCulture) ? "T" : "F";
                    }
                    break;
                default:
                    strValue = objValue.ToString();
                    break;
            }
            return strValue;
        }


        private static string GetDoubleStrByObj(object objValue, int nDecimalSize)
        {
            NumberFormatInfo info = new NumberFormatInfo();
            info.NumberDecimalSeparator = ".";
            info.NumberGroupSeparator = ",";

            string strValue;
            double dValue = Convert.ToDouble(objValue, CultureInfo.InvariantCulture);
            dValue = Math.Round(dValue, nDecimalSize);

            strValue = dValue.ToString(info);

            return strValue;
        }


        private string GetInputErrorMsg(string fieldName, DbfColumnType type,
            int nWidth, int nDecimals)
        {
            string strErrorMsg = null;

            if (nWidth < 1 | nWidth > ByteValue * ByteValue - 1)
            {
                strErrorMsg = ErrorInputArguments;
            }
            else if (nDecimals < 0 | nDecimals > nWidth)
            {
                strErrorMsg = ErrorInputArguments;
            }

            else if (type != DbfColumnType.Double && nDecimals != 0)
            {
                strErrorMsg = ErrorInputArgumentsArranged;
            }

           
            else if (_encoding.GetByteCount(fieldName) > 10)
            {
                strErrorMsg = ErrorFieldNameIllegal;
            }
           
            else if (type == DbfColumnType.Date && (nWidth != 8 || nDecimals != 0))
            {
                strErrorMsg = ErrorDateFieldIllegal;
            }
            else
            {
                for (int i = 1; i <= _columnCount; i++)
                {
                    string strTmp = GetColumnName(i);
                    if (strTmp == fieldName)
                    {
                        strErrorMsg = ErrorFieldNameDuplicated;
                    }
                }
            }
            return strErrorMsg;
        }


        private static int ConvertStr2Int32(string strNum)
        {
            string strResult = ConvertStr2Int32Str(strNum);
            if (string.IsNullOrEmpty(strResult))
            {
                return 0;
            }
            return Convert.ToInt32(strResult, CultureInfo.InvariantCulture);
        }


        private static string ConvertStr2Int32Str(string strNum)
        {
            if (strNum.Length == 0)
            {
                return "";
            }

            int i;
            for (i = 1; i < strNum.Length; i++)
            {
                if (!Char.IsDigit(strNum[i]))
                {
                    break;
                }
            }
            string strResult = strNum.Substring(0, i);

            if ((strNum[0] == '+' | strNum[0] == '-') & (strResult.Length == 1))
            {
                return "";
            }
            if (strNum[0] == '+' | strNum[0] == '-' | Char.IsDigit(strNum[0]))
            {
                return strResult;
            }
            return "";
        }


        private static void WriteInt2Bytes(ref byte[] bytes, int iStart, int intValue)
        {
            bytes[iStart] = (byte)(intValue % ByteValue);
            bytes[iStart + 1] = (byte)((intValue / ByteValue) % ByteValue);
            bytes[iStart + 2] = (byte)((intValue / (ByteValue * ByteValue)) % ByteValue);
            bytes[iStart + 3] = (byte)((intValue / (ByteValue * ByteValue * ByteValue))
                % ByteValue);
        }


        private static int ReadIntFromBytes(byte[] bytes, int iStart)
        {
            int nResult = bytes[iStart]
                + bytes[iStart + 1] * ByteValue
                + bytes[iStart + 2] * ByteValue * ByteValue
                + bytes[iStart + 3] * ByteValue * ByteValue * ByteValue;
            return nResult;
        }


        private static void GetTime(ref byte[] fileHeader)
        {
            fileHeader[1] = (byte)(DateTime.Now.Year % 100);	
            fileHeader[2] = (byte)DateTime.Now.Month;			
            fileHeader[3] = (byte)DateTime.Now.Day;				
        }


        private void CopyHeaderData_DBF(GeoDbf dbfFile, ref Stream fsDest)
        {
            byte[] arrFileHeader = new byte[dbfFile._headerLength];
            dbfFile._dbfStream.Seek(0, SeekOrigin.Begin);
            dbfFile._dbfStream.Read(arrFileHeader, 0, dbfFile._headerLength);

            GetTime(ref _headerBytes);

            WriteInt2Bytes(ref arrFileHeader, 4, _recordCount);

            fsDest.Seek(0, SeekOrigin.Begin);
            fsDest.Write(arrFileHeader, 0, dbfFile._headerLength);
        }


        private static void CopyHeaderData_DBT(Stream dbtFile, ref Stream fsDest)
        {
            byte[] arrFileHeader = new byte[BlockSize];

            dbtFile.Seek(0, SeekOrigin.Begin);
            dbtFile.Read(arrFileHeader, 0, BlockSize);

            fsDest.Seek(0, SeekOrigin.Begin);
            fsDest.Write(arrFileHeader, 0, BlockSize);
        }


        #endregion

        #region Private Fields

        private string _pathFileName;	
        private string _dbtFileName; 

        private Stream _dbfStream;		
        private Stream _dbtStream;	

        private int _recordCount;			
        private int _recordLength;		
        private int _headerLength;		
        private int _columnCount;		
        private bool _isClosed;		

        private ArrayList _fieldsHeader;  
        private byte[] _headerBytes;		 

        private bool _isCurrentRecordModified;
        private int _currentRecord;
        private byte[] _currentRecords;

        private ArrayList _deletedItems;

        private ArrayList _originalIndex;	
        private ArrayList _newIndex;		

        private string _password;				
        private bool _bFileDetached;

        private bool _haveMemoField;
        private bool _isUpdated;
        private bool _isFieldUpdated;
        private bool _isCreating;

        [NonSerialized]
        private Encoding _encoding;
        private CultureInfo _cultureInfo;

        #endregion

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                Close();
            }
            else
            {
                if (_dbfStream != null)
                {
                    _dbfStream.Dispose();
                }
                if (_dbtStream != null)
                {
                    _dbtStream.Dispose();
                }
            }
        }
    }
}

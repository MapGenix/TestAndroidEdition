using System;
using System.IO;
using Mapgenix.Shapes;
using Mapgenix.Utils;

namespace Mapgenix.FeatureSource
{
    [Serializable]
    public class ShapeFileIndex
    {
        private const int EachRecordLength = 8;
        private const int FileHeaderLength = 100;
        private const int CacheSize = 512;

        private string _shxPathFileName;
        private ShapeFileHeader _fileHeader;
        private Stream _shxStream;
        private long _startIndex;
        private long _endIndex;
        private byte[] _cache;
        private bool _isOpen;

        public event EventHandler<StreamLoadingEventArgs> StreamLoading;

        public ShapeFileIndex()
            : this(string.Empty)
        { }

        public ShapeFileIndex(string shxPathFileName)
        {
            this._shxPathFileName = shxPathFileName;
            _fileHeader = new ShapeFileHeader();
        }

        public string ShxPathFileName
        {
            get { return _shxPathFileName; }
            set { _shxPathFileName = value; }
        }

        public ShapeFileHeader FileHeader
        {
            get { return _fileHeader; }
            set { _fileHeader = value; }
        }

        internal virtual void OnStreamLoading(StreamLoadingEventArgs e)
        {
            EventHandler<StreamLoadingEventArgs> handler = StreamLoading;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        public void Open()
        {
            Open(FileAccess.Read);
        }

        public void Open(FileAccess fileAccess)
        {
            StreamLoadingEventArgs args = new StreamLoadingEventArgs(_shxPathFileName, "SHX File");
            args.FileAccess = fileAccess;
            OnStreamLoading(args);

            if (args.Stream == null)
            {
                Validators.CheckFileIsExist(_shxPathFileName);

                _shxStream = new FileStream(_shxPathFileName, FileMode.Open, fileAccess);
            }
            else
            {
                _shxStream = args.Stream;
            }

            _isOpen = true;
        }

       
        public void Close()
        {
            if (_isOpen && _shxStream != null)
            {
                _shxStream.Close();
                _shxStream = null;
            }
            _cache = null;
            _startIndex = 0;
            _endIndex = 0;
            _isOpen = false;
        }

        public int GetRecordCount()
        {
            return (int)((_shxStream.Length - FileHeaderLength) / EachRecordLength);
        }

        public int GetRecordOffset(int recordIndex)
        {
            int recordPosition = (recordIndex - 1) * EachRecordLength + FileHeaderLength;

            if (recordPosition + 4 >= _endIndex || recordPosition < _startIndex)
            {
                _startIndex = recordPosition;
                _endIndex = _startIndex + CacheSize;
                _cache = new byte[CacheSize];
                _shxStream.Seek(_startIndex, SeekOrigin.Begin);
                _shxStream.Read(_cache, 0, CacheSize);
            }

            int position = (int)(recordPosition - _startIndex);
            int offset = (((_cache[position] << 0x18) | (_cache[position + 1] << 0x10)) | (_cache[position + 2] << 8)) | _cache[position + 3];

            return offset * 2;
        }

        public int GetRecordContentLength(int recordIndex)
        {
            int recordPosition = (recordIndex - 1) * EachRecordLength + FileHeaderLength;
            int recordContentPosition = recordPosition + 4;

            if (recordContentPosition + 4 >= _endIndex || recordContentPosition < _startIndex)
            {
                _startIndex = recordContentPosition;
                _endIndex = _startIndex + CacheSize;
                _cache = new byte[CacheSize];
                _shxStream.Seek(_startIndex, SeekOrigin.Begin);
                _shxStream.Read(_cache, 0, CacheSize);
            }

            int position = (int)(recordContentPosition - _startIndex);
            int offset = (((_cache[position] << 0x18) | (_cache[position + 1] << 0x10)) | (_cache[position + 2] << 8)) | _cache[position + 3];

            return offset * 2;
        }

        public void UpdateRecord(int recordIndex, int offset, int contentLength)
        {
            offset = offset / 2;
            WriteRecord(recordIndex, offset, contentLength);
        }

        public void AddRecord(int offset, int contentLength)
        {
            offset = offset / 2;

            WriteRecord(GetRecordCount() + 1, offset, contentLength);
        }

        private void WriteRecord(int recordIndex, int offset, int contentLength)
        {
            _shxStream.Seek(FileHeaderLength + (recordIndex - 1) * 8, SeekOrigin.Begin);

            ShapeFile.WriteIntToStream(offset, _shxStream, WkbByteOrder.BigEndian);

            ShapeFile.WriteIntToStream(contentLength, _shxStream, WkbByteOrder.BigEndian);
        }

        public void DeleteRecord(int recordIndex)
        {
            int recordStartPosition = (recordIndex - 1) * EachRecordLength + FileHeaderLength;

            _shxStream.Seek(recordStartPosition + 4, SeekOrigin.Begin);

            ShapeFile.WriteIntToStream(0, _shxStream, WkbByteOrder.BigEndian);

            _startIndex = 0;
            _endIndex = 0;
        }

        public void Flush()
        {
            _fileHeader.WriteToStream(_shxStream);
            _shxStream.Flush();
        }
    }
}

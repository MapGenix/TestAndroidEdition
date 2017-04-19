using System;
using System.IO;

namespace Mapgenix.Utils
{
    /// <summary>Event arguments for the StreamLoading event in place of concrete files on the file system.</summary>
    [Serializable]
    public class StreamLoadingEventArgs : EventArgs
    {
        private string _streamName;
        private Stream _stream;
        private FileMode _fileMode;
        private FileAccess _fileAccess;
        private readonly string _streamType;

  
        public StreamLoadingEventArgs(string streamName, string streamType)
            : this(streamName, streamType, null, FileMode.Open, FileAccess.Read)
        { }

        public StreamLoadingEventArgs(string streamName, Stream stream, FileMode fileMode, FileAccess fileAccess)
            : this(streamName, string.Empty, stream, fileMode, fileAccess)
        {
        }

        public StreamLoadingEventArgs(string streamName, string streamType, Stream stream, FileMode fileMode, FileAccess fileAccess)
        {
            _streamName = streamName;
            _streamType = streamType;
            _stream = stream;
            _fileMode = fileMode;
            _fileAccess = fileAccess;
        }

        public String StreamName
        {
            get
            {
                return _streamName;
            }
            set
            {
                _streamName = value;
            }
        }

        public Stream Stream
        {
            get
            {
                return _stream;
            }
            set
            {
                _stream = value;
            }
        }

        public FileMode FileMode
        {
            get
            {
                return _fileMode;
            }
            set
            {
                _fileMode = value;
            }
        }

        public FileAccess FileAccess
        {
            get
            {
                return _fileAccess;
            }
            set
            {
                _fileAccess = value;
            }
        }

        public string StreamType
        {
            get { return _streamType; }
        }
    }
}

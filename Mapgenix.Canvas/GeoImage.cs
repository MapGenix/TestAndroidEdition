using System;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using Mapgenix.Utils;

namespace Mapgenix.Canvas
{
    /// <summary>Image for a GeoCanvas.</summary>
    /// <remarks>Usefull for any kind of stream, for example path and filename of image.
    //Usefull also for compressed streams, isolated storage and other stream-related storage mechanism.</remarks>
    [Serializable]
    public class GeoImage : IDisposable
    {
        private Guid _canvasImageFormat;

        private Stream _imageStream;
        private string _pathFilename;

        /// <overloads>To create an image without specifying a PathFileName.</overloads>
        /// <remarks>Set the PathFileName property.</remarks>
        /// <summary>To create a new GeoImage.</summary>
        public GeoImage()
            : this(string.Empty)
        {
        }

        /// <overloads>To create an image by specifying a PathFileName.</overloads>
        /// <summary>To create a GeoImage</summary>
        /// <remarks>When specifying the path and filename, it should be in the correct format; however, the file does not need to exist on the file system. 
        /// Streams supplied by the developer at runtime can be used. 
        /// Streamloading event is raised in case no file is found in the path specified. In that event handler, the stream needs to be supplied.</remarks>
        /// <param name="pathFileName">Path and filename of the image file. Note: it does not need to actually exist on the file system</param>
        public GeoImage(string pathFileName)
        {
            _pathFilename = pathFileName;
            _canvasImageFormat = new Guid("7B53FF9B-33EE-456E-AC1D-20FEC55E01F9");
            SetFileInfo();
        }

        /// <summary>To create a GeoImage.</summary>
        /// <overloads>Passes in a stream in TIFF format.</overloads>
        /// <returns>None</returns>
        /// <param name="imageStream">Stream in TIFF format.</param>
        public GeoImage(Stream imageStream)
        {
            _imageStream = imageStream;
        }

        /// <summary>To create a GeoImage</summary>
        /// <overloads>To create a GeoImage by specifying a width, height and a GeoCanvas.</overloads>
        /// <returns>None</returns>
        /// <param name="width">Width in pixels for the GeoImage.</param>
        /// <param name="height">Height in pixels for the GeoImage.</param>
        public GeoImage(int width, int height)
        {
            var stream = new MemoryStream();
            byte[] headerBytes =
            {
                0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a, 0x00, 0x00, 0x00, 0x0d, 0x49, 0x48,
                0x44, 0x52
            };
            stream.Write(headerBytes, 0, headerBytes.Length);

            var widthBytes = BitConverter.GetBytes(width);
            Array.Reverse(widthBytes);
            stream.Write(widthBytes, 0, widthBytes.Length);

            var heighBytes = BitConverter.GetBytes(height);
            Array.Reverse(heighBytes);
            stream.Write(heighBytes, 0, heighBytes.Length);

            byte[] endOfHeader = {0x08, 0x06, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00};
            stream.Write(endOfHeader, 0, endOfHeader.Length);

            byte[] sRgbAndcHrmBytes =
            {
                0x00, 0x00, 0x00, 0x01, 0x73, 0x52, 0x47, 0x42, 0x00, 0xae, 0xce, 0x1c, 0xe9,
                0x00, 0x00, 0x00, 0x04, 0x67, 0x41, 0x4d, 0x41, 0x00, 0x00, 0xb1, 0x8f, 0x0b, 0xfc, 0x61, 0x05, 0x00,
                0x00, 0x00, 0x20, 0x63, 0x48, 0x52, 0x4d, 0x00, 0x00, 0x7a, 0x26, 0x00, 0x00, 0x80, 0x84, 0x00, 0x00,
                0xfa, 0x00, 0x00, 0x00, 0x80, 0xe8, 0x00, 0x00, 0x75, 0x30, 0x00, 0x00, 0xea, 0x60, 0x00, 0x00, 0x3a,
                0x98, 0x00, 0x00, 0x17, 0x70, 0x9c, 0xba, 0x51, 0x3c
            };

            stream.Write(sRgbAndcHrmBytes, 0, sRgbAndcHrmBytes.Length);

            byte[] iDatData = {0x49, 0x44, 0x41, 0x54, 0x78, 0x53}; 

            var dataBytes = new byte[width*height*4 + height];
            var resultStream = new MemoryStream();
            var deflateStream = new DeflateStream(resultStream, CompressionMode.Compress);
            deflateStream.Write(dataBytes, 0, dataBytes.Length);
            dataBytes = new byte[resultStream.Length];
            resultStream.Seek(0, SeekOrigin.Begin);
            resultStream.Write(dataBytes, 0, dataBytes.Length);
            deflateStream.Close();

            stream.Write(BitConverter.GetBytes(dataBytes.Length), 0, 4);
            stream.Write(iDatData, 0, iDatData.Length);
            stream.Write(dataBytes, 0, dataBytes.Length);

            byte[] iEndBytes = {0x00, 0x00, 0x00, 0x00, 0x49, 0x45, 0x4e, 0x44, 0xae, 0x42, 0x60, 0x82};
            stream.Write(iEndBytes, 0, iEndBytes.Length);

            stream.Seek(0, SeekOrigin.Begin);

            _imageStream = stream;
        }

        /// <summary>Returns path and filename of the image to represent.</summary>
        /// <remarks>When specifying the path and filename, it should be in the correct format; however, the file does not need to exist on the file system. 
        /// Streams supplied by the developer at runtime can be used. 
        /// Streamloading event is raised in case no file is found in the path specified. In that event handler, the stream needs to be supplied.</remarks>
        public string PathFilename
        {
            get { return _pathFilename; }
            set
            {
                _pathFilename = value;

                SetFileInfo();
            }
        }

        private string PathName { get; set; }

        private string FileName { get; set; }

        /// <summary>Returns the image format.</summary>
        /// <returns>Image format.</returns>
        /// <remarks>None</remarks>
        public Guid CanvasImageFormat
        {
            get { return _canvasImageFormat; }
            set { _canvasImageFormat = value; }
        }

        /// <summary>Dispose for the class.</summary>
        /// <overloads>Disposes the stream in the GeoImage and releases its resources.</overloads>
        /// <returns>None</returns>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>Passes one's own stream to represent the GeoImage.</summary>
        /// <remarks>When specifying the path and filename, it should be in the correct format; however, the file does not need to exist on the file system. 
        /// Streams supplied by the developer at runtime can be used. 
        /// Streamloading event is raised in case no file is found in the path specified. In that event handler, the stream needs to be supplied.</remarks>
        public event EventHandler<StreamLoadingEventArgs> StreamLoading;

        /// <summary>Raises the StreamLoading event to specify one's own stream.</summary>
        /// <remarks>None</remarks>
        /// <param name="e">Event arguments for the StreamLoading event.</param>
        protected virtual void OnStreamLoading(StreamLoadingEventArgs e)
        {
            var handler = StreamLoading;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>Returns the stream of a GeoImage.</summary>
        /// <returns>Stream of a GeoImage.</returns>
        /// <remarks>Raises the event allowing to supply one's own stream.</remarks>
        public Stream GetImageStream(BaseGeoCanvas canvas)
        {
            Validators.CheckParameterIsNotNull(canvas, "canvas");

            if (_imageStream != null)
            {
                if (_imageStream.CanSeek)
                {
                    
                    _imageStream.Seek(0, SeekOrigin.Begin);
                }
            }
            else
            {
                var streamLoadingEventArgs = new StreamLoadingEventArgs(string.Empty, "GeoImage");
                OnStreamLoading(streamLoadingEventArgs);
                _imageStream = streamLoadingEventArgs.Stream;

                if (_imageStream == null)
                {
                    _imageStream = GetImageStreamCore(canvas);
                }
            }

            if (_imageStream == null)
            {
                var exceptionDescription = string.Format(CultureInfo.InvariantCulture,
                    "Image from image path:{0} is missing or invalid.", _pathFilename);
                throw new ArgumentException(exceptionDescription);
            }

            return _imageStream;
        }

        /// <summary>Returns the stream of a GeoImage.</summary>
        /// <returns>Stream of a GeoImage.</returns>
        /// <remarks>Raises the event allowing to supply one's own stream.</remarks>
        protected Stream GetImageStreamCore(BaseGeoCanvas canvas)
        {
            Validators.CheckParameterIsNotNull(canvas, "canvas");
            
            return canvas.GetStreamFromGeoImage(this);
        }

        /// <summary>Gets the width in pixel of the image.</summary>
        /// <returns>Width in pixel of the image.</returns>
        /// <remarks>None.</remarks>
        public int GetWidth()
        {
            var isFromPath = false;
            if (!string.IsNullOrEmpty(_pathFilename) && _imageStream == null)
            {
                _imageStream = File.Open(_pathFilename, FileMode.Open, FileAccess.Read);
                isFromPath = true;
            }

            if (_imageStream == null)
            {
                return 0;
            }

            var reader = new BinaryReader(_imageStream);
            reader.BaseStream.Seek(0, SeekOrigin.Begin);

            var beginBytes = new byte[32];
            reader.Read(beginBytes, 0, beginBytes.Length);

            var width = 0;

            // PNG
            if (beginBytes[1] == 0x50 && beginBytes[2] == 0x4e && beginBytes[3] == 0x47)
            {
                var tempBytes = new byte[4];
                Array.Copy(beginBytes, 16, tempBytes, 0, 4);
                Array.Reverse(tempBytes);
                width = BitConverter.ToInt32(tempBytes, 0);
            }
            //BMP
            else if (beginBytes[0] == 0x42 && beginBytes[1] == 0x4d)
            {
                width = BitConverter.ToInt32(beginBytes, 18);
            }
            //TIFF (Little endian)
            else if (beginBytes[0] == 0x49 && beginBytes[1] == 0x49)
            {
                reader.BaseStream.Seek(4, SeekOrigin.Begin);
                var idfOffset = reader.ReadInt32();
                reader.BaseStream.Seek(idfOffset + 14, SeekOrigin.Begin);
                var infomation = new byte[24];
                reader.Read(infomation, 0, infomation.Length);
                if (BitConverter.ToInt16(infomation, 0) == 256)
                {
                    width = BitConverter.ToInt32(infomation, 8);
                }
            }
            // TIFF (Big endian)
            else if (beginBytes[0] == 0x4D && beginBytes[1] == 0x4D)
            {
                reader.BaseStream.Seek(4, SeekOrigin.Begin);
                var idfOffset = ArrayHelper.GetIntFromByteArray(BitConverter.GetBytes(reader.ReadInt32()), 0, 0);
                reader.BaseStream.Seek(idfOffset + 14, SeekOrigin.Begin);
                var infomation = new byte[24];
                reader.Read(infomation, 0, infomation.Length);
                if (ArrayHelper.GetInt16FromByteArray(infomation, 0, 0) == 256)
                {
                    width = ArrayHelper.GetInt16FromByteArray(infomation, 8, 0);
                }
            }
            // GIF
            else if (beginBytes[0] == 0x47 && beginBytes[1] == 0x49 && beginBytes[2] == 0x46)
            {
                width = BitConverter.ToInt16(beginBytes, 6);
            }
            // JPG
            else if (beginBytes[0] == 255 && beginBytes[1] == 216)
            {
                width = GetWidthOrHeightOfJpeg(reader, beginBytes, true);
            }

            if (isFromPath)
            {
                _imageStream.Dispose();
                _imageStream = null;
            }

            return width;
        }

        /// <summary>Gets the height in pixel of the image.</summary>
        /// <returns>Height in pixel of the image.</returns>
        /// <remarks>None.</remarks>
        public int GetHeight()
        {
            var isFromPath = false;
            if (!string.IsNullOrEmpty(_pathFilename) && _imageStream == null)
            {
                _imageStream = File.Open(_pathFilename, FileMode.Open, FileAccess.Read);
                isFromPath = true;
            }

            if (_imageStream == null)
            {
                return 0;
            }

            var reader = new BinaryReader(_imageStream);
            reader.BaseStream.Seek(0, SeekOrigin.Begin);

            var beginBytes = new byte[32];
            reader.Read(beginBytes, 0, beginBytes.Length);

            var height = 0;

            //PNG
            if (beginBytes[1] == 0x50 && beginBytes[2] == 0x4e && beginBytes[3] == 0x47)
            {
                var tempBytes = new byte[4];
                Array.Copy(beginBytes, 20, tempBytes, 0, 4);
                Array.Reverse(tempBytes);
                height = BitConverter.ToInt32(tempBytes, 0);
            }
            //BMP
            else if (beginBytes[0] == 0x42 && beginBytes[1] == 0x4d)
            {
                height = BitConverter.ToInt32(beginBytes, 22);
            }
            //TIFF(Little endian)
            else if (beginBytes[0] == 0x49 && beginBytes[1] == 0x49)
            {
                reader.BaseStream.Seek(4, SeekOrigin.Begin);
                var idfOffset = reader.ReadInt32();
                reader.BaseStream.Seek(idfOffset + 14, SeekOrigin.Begin);
                var infomation = new byte[24];
                reader.Read(infomation, 0, infomation.Length);

                if (BitConverter.ToInt16(infomation, 12) == 257)
                {
                    height = BitConverter.ToInt32(infomation, 20);
                }
            }
            // TIFF(Big endian)
            else if (beginBytes[0] == 0x4D && beginBytes[1] == 0x4D)
            {
                reader.BaseStream.Seek(4, SeekOrigin.Begin);
                var idfOffset = ArrayHelper.GetIntFromByteArray(BitConverter.GetBytes(reader.ReadInt32()), 0, 0);
                reader.BaseStream.Seek(idfOffset + 14, SeekOrigin.Begin);
                var infomation = new byte[24];
                reader.Read(infomation, 0, infomation.Length);

                if (ArrayHelper.GetInt16FromByteArray(infomation, 12, 0) == 257)
                {
                    height = ArrayHelper.GetInt16FromByteArray(infomation, 20, 0);
                }
            }
            //GIF
            else if (beginBytes[0] == 0x47 && beginBytes[1] == 0x49 && beginBytes[2] == 0x46)
            {
                height = BitConverter.ToInt16(beginBytes, 8);
            }
            // JPG
            else if (beginBytes[0] == 255 && beginBytes[1] == 216)
            {
                height = GetWidthOrHeightOfJpeg(reader, beginBytes, false);
            }

            if (isFromPath)
            {
                _imageStream.Dispose();
                _imageStream = null;
            }

            return height;
        }

        private static int GetWidthOrHeightOfJpeg(BinaryReader reader, byte[] beginBytes, bool isWidth)
        {
            var widthOrHeight = 0;
            reader.BaseStream.Seek(6, SeekOrigin.Begin);

            var blockLength = beginBytes[4]*256 + beginBytes[5];

            var i = 4;
            while (i < reader.BaseStream.Length)
            {
                i += blockLength;
                reader.BaseStream.Seek(i, SeekOrigin.Begin);
                if (i > reader.BaseStream.Length)
                {
                    break;
                }
                if (reader.ReadByte() != 255)
                {
                    break;
                }
                if (reader.ReadByte() == 192)
                {
                    if (isWidth)
                    {
                        reader.BaseStream.Seek(i + 7, SeekOrigin.Begin);
                        widthOrHeight = reader.ReadByte()*256 + reader.ReadByte();
                    }
                    else
                    {
                        reader.BaseStream.Seek(i + 5, SeekOrigin.Begin);
                        widthOrHeight = reader.ReadByte()*256 + reader.ReadByte();
                    }
                    break;
                }
                i += 2;
                reader.BaseStream.Seek(i, SeekOrigin.Begin);
                blockLength = reader.ReadByte()*256 + reader.ReadByte();
            }

            return widthOrHeight;
        }

        private void SetFileInfo()
        {
            if (string.IsNullOrEmpty(_pathFilename))
            {
                _pathFilename = string.Empty;
                PathName = string.Empty;
                FileName = string.Empty;

                return;
            }

            if (_pathFilename.IndexOf(":", StringComparison.CurrentCultureIgnoreCase) == -1)
            {
                _pathFilename = Path.GetFullPath(_pathFilename);
            }

            FileName = Path.GetFileName(_pathFilename);
            PathName = Path.GetDirectoryName(_pathFilename);
        }

        /// <summary>Closes the GeoImage.</summary>
        /// <returns>None</returns>
        public void Close()
        {
            if (_imageStream != null)
            {
                _imageStream.Close();
                _imageStream = null;
            }
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                Close();
            }
        }

        
    }
}
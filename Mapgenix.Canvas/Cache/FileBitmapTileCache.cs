using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using Mapgenix.Shapes;

namespace Mapgenix.Canvas
{
    /// <summary>
    /// Concrete class inheriting from BaseBitmapTileCache.
    /// </summary>
    /// <remarks>Tile cache helps to improve map drawing in case of large data or remote server images.</remarks>
    [Serializable]
    public class FileBitmapTileCache : BaseBitmapTileCache
    {
        private readonly float _resolution;
        private string _cacheDirectory;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <remarks>CacheDirectory, CacheID, TileImageFormat and TileMatrix properties need to be set manually.</remarks>
        public FileBitmapTileCache()
            : this(GetTemporaryFolder())
        {
        }

        /// <summary>
        /// Overloaded constructor.
        /// </summary>
        /// <param name="cacheDirectory">Cache directory of the tile cache.</param>
        /// <remarks>Other properties are set with default values.</remarks>
        public FileBitmapTileCache(string cacheDirectory)
            : this(cacheDirectory, Guid.NewGuid().ToString())
        {
        }

        /// <summary>
        /// Overloaded constructor.
        /// </summary>
        /// <param name="cacheDirectory">Cache directory of the tile cache.</param>
        /// <param name="cacheId">Cache id of the tile cache.</param>
        /// <remarks>Other properties are set with default values.</remarks>
        public FileBitmapTileCache(string cacheDirectory, string cacheId)
            : this(cacheDirectory, cacheId, TileImageFormat.Png, new TileMatrix(590591790))
        {
        }

        /// <summary>
        /// Overloaded constructor.
        /// </summary>
        /// <param name="cacheDirectory">Cache directory of the tile cache.</param>
        /// <param name="cacheId">Cache id of the tile cache.</param>
        /// <param name="imageFormat">Image format of the tile cache.</param>
        /// <param name="tileMatrix">Tile matrix of the tile cache.</param>
       public FileBitmapTileCache(string cacheDirectory, string cacheId, TileImageFormat imageFormat,
            TileMatrix tileMatrix)
            : base(cacheId, imageFormat, tileMatrix)
        {
            _cacheDirectory = cacheDirectory;
            Bitmap testBitmap = null;
            try
            {
                testBitmap = new Bitmap(1, 1);
                _resolution = testBitmap.HorizontalResolution;
            }
            finally
            {
                if (testBitmap != null)
                {
                    testBitmap.Dispose();
                }
            }
        }

       /// <summary>
       /// Gets or sets the cache directory.
       /// </summary>
        public string CacheDirectory
        {
            get { return _cacheDirectory; }
            set { _cacheDirectory = value; }
        }

        /// <summary>
        /// Returns the BitmapTile based on a row and a column. 
        /// </summary>
        /// <param name="row">Target row for the tile to retrieve.</param>
        /// <param name="column">Target column for the tile to fetch.</param>
        /// <returns>Returns the BitmapTile based on a row and a column.</returns>
       protected override BitmapTile GetTileCore(long row, long column)
        {
            var tileImageFileName = GetTileImageFileName(row, column, TileMatrix.Scale);
            var cell = TileMatrix.GetCell(row, column);
            var tile = new BitmapTile(cell.BoundingBox, TileMatrix.Scale);

            if (File.Exists(tileImageFileName))
            {
                FileStream fileStream = null;
                try
                {
                    var os = Environment.OSVersion;
                    if (os.Version.Major <= 5 && _resolution != 96)
                    {
                        fileStream = new FileStream(tileImageFileName, FileMode.Open, FileAccess.Read);
                        tile.Bitmap = new Bitmap(fileStream);
                    }
                    else
                    {
                        tile.Bitmap = UnsafeHelper.FastLoadImageFromFile(tileImageFileName);
                    }
                }
                finally
                {
                    if (fileStream != null)
                    {
                        fileStream.Dispose();
                    }
                }
            }
            return tile;
        }

       /// <summary>
       /// Saves the target tile passed in.
       /// </summary>
       /// <param name="tile">Target tile to be saved.</param>
       protected override void SaveTileCore(BaseTile tile)
        {
            Validators.CheckParameterIsNotNull(tile, "tile");

            var currentScale = TileMatrix.Scale;
            TileMatrix.Scale = tile.Scale;
            var cell = TileMatrix.GetCell(tile.BoundingBox.GetCenterPoint());
            TileMatrix.Scale = currentScale;
            var tileImageFileName = GetTileImageFileName(cell.Row, cell.Column, tile.Scale);

            var bitmapTile = tile as BitmapTile;
            if (bitmapTile != null)
            {
                var bitmapArray = GetBinariesFromBitmap(bitmapTile.Bitmap);

                var directory =
                    tileImageFileName.Remove(tileImageFileName.LastIndexOf(@"\", StringComparison.OrdinalIgnoreCase));

                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                if (!File.Exists(tileImageFileName))
                {
                    File.WriteAllBytes(tileImageFileName, bitmapArray);
                }
            }
        }

       /// <summary>
       /// Deletes the target tile passed in.
       /// </summary>
       /// <param name="tile">Target tile to be deleted.</param>
       /// <remarks>Does not take effect if ReadOnly is set to true.</remarks>
        protected override void DeleteTileCore(BaseTile tile)
        {
            Validators.CheckParameterIsNotNull(tile, "tile");

            var cell = TileMatrix.GetCell(tile.BoundingBox.GetCenterPoint());
            var tileImageFileName = GetTileImageFileName(cell.Row, cell.Column, tile.Scale);
            if (File.Exists(tileImageFileName))
            {
                File.Delete(tileImageFileName);
            }
        }

        private byte[] GetBinariesFromBitmap(Bitmap bitmap)
        {
            var memoryStream = new MemoryStream();
            try
            {
                if (ImageFormat == TileImageFormat.Jpeg)
                {
                    var encoderParameter = new EncoderParameter(Encoder.Quality, (long) JpegQuality);
                    var encoderParameters = new EncoderParameters(1);
                    encoderParameters.Param[0] = encoderParameter;
                    var jpegCodecInfo = GetEncoder(System.Drawing.Imaging.ImageFormat.Jpeg);

                    bitmap.Save(memoryStream, jpegCodecInfo, encoderParameters);
                }
                else if (ImageFormat == TileImageFormat.Png)
                {
                    bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
                }
                var buffer = memoryStream.GetBuffer();
                return buffer;
            }
            finally
            {
                memoryStream.Dispose();
            }
        }

        private static ImageCodecInfo GetEncoder(ImageFormat format)
        {
            var codecs = ImageCodecInfo.GetImageDecoders();

            foreach (var codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }

        /// <summary>
        /// Clears all the tiles in the tile cache.
        /// </summary>
        /// <remarks>Does not take effect if ReadOnly is set to true.</remarks>
        protected override void ClearCacheCore()
        {
            var deleteFolder = _cacheDirectory;

            if (!deleteFolder.EndsWith(@"\", StringComparison.OrdinalIgnoreCase))
            {
                deleteFolder += @"\";
            }

            if (!string.IsNullOrEmpty(CacheId))
            {
                deleteFolder += CacheId;
            }

            if (Directory.Exists(deleteFolder))
            {
                var result = VerifyDeletingFolder(deleteFolder);
                if (result)
                {
                    Directory.Delete(deleteFolder, true);
                }
            }
        }

        private static bool VerifyDeletingFolder(string folder)
        {
            var createdByGSuite = true;

            var allSubFolderNames = Directory.GetDirectories(folder, "*", SearchOption.AllDirectories);

            foreach (var folderName in allSubFolderNames)
            {
                double result;

                var splittedFolderName = folderName.Split('\\');

                var isDouble = double.TryParse(splittedFolderName[splittedFolderName.Length - 1], NumberStyles.Any,
                    CultureInfo.InvariantCulture, out result);
                if (!isDouble)
                {
                    createdByGSuite = false;
                    break;
                }
            }

            return createdByGSuite;
        }

        /// <summary>
        /// Overrides the logic of its base class BaseBitmapTileCache.
        /// Returns the higher scale BitmapTile used for drawing effects when zooming in or out.
        /// </summary>
        /// <param name="tileBoundingBox">Current tile bounding box.</param>
        /// <param name="tileScale">Current tile scale.</param>
        /// <returns>Returns the higher scale(higher zoomLevel) bitmap tile.</returns>
        protected override BitmapTile GetHigherScaleTileCore(RectangleShape tileBoundingBox, double tileScale)
        {
            BitmapTile higherScaleTile = null;

            var dir = Path.Combine(CacheDirectory, CacheId);
            if (Directory.Exists(dir))
            {
                var root = new DirectoryInfo(dir);

                var directoryInfos = root.GetDirectories("*", SearchOption.TopDirectoryOnly);

                var scales = new double[directoryInfos.Length];
                for (var i = 0; i < directoryInfos.Length; i++)
                {
                    scales[i] = double.Parse(directoryInfos[i].Name, CultureInfo.InvariantCulture);
                }
                Array.Sort(scales);

                foreach (var higherZoomLevelScale in scales)
                {
                    if (higherZoomLevelScale/tileScale < 33 && higherZoomLevelScale/tileScale > 1)
                    {
                        TileMatrix.Scale = higherZoomLevelScale;
                        var tempTiles = GetTiles(tileBoundingBox);
                        TileMatrix.Scale = tileScale;

                        var tempTileBitmap = MergeBitmaps(tempTiles, higherZoomLevelScale);

                        if (tempTileBitmap != null && tempTileBitmap.Bitmap != null)
                        {
                            higherScaleTile = tempTileBitmap;
                            break;
                        }
                    }
                }
            }

            return higherScaleTile;
        }

        /// <summary>
        /// Gets the name of the tile image file based on a row and a column.
        /// </summary>
        public string GetTileImageFileName(long row, long column)
        {
            return GetTileImageFileName(row, column, TileMatrix.Scale);
        }

        private string GetTileImageFileName(long row, long column, double scale)
        {
            var tileImageFileName = _cacheDirectory;

            if (!tileImageFileName.EndsWith(@"\", StringComparison.OrdinalIgnoreCase))
            {
                tileImageFileName += @"\";
            }

            if (!string.IsNullOrEmpty(CacheId))
            {
                tileImageFileName += CacheId + @"\";
            }

            var foundAproximiateScaleDirectory = false;

            if (Directory.Exists(_cacheDirectory + "\\" + CacheId))
            {
                var targetDirectories = Directory.GetDirectories(_cacheDirectory + "\\" + CacheId, "*.*",
                    SearchOption.TopDirectoryOnly);
                foreach (var targetDirectory in targetDirectories)
                {
                    var directoryInfo = new DirectoryInfo(targetDirectory);
                    var scaleFolder = directoryInfo.Name;
                    var targetDirectoryScale = Convert.ToDouble(scaleFolder, CultureInfo.InvariantCulture);

                    if (Math.Abs(scale - targetDirectoryScale) < 1e-4)
                    {
                        tileImageFileName = targetDirectory + @"\";
                        foundAproximiateScaleDirectory = true;
                        break;
                    }
                }
            }

            if (!foundAproximiateScaleDirectory)
            {
                tileImageFileName += scale.ToString(CultureInfo.InvariantCulture) + @"\";
            }

            tileImageFileName += row.ToString(CultureInfo.InvariantCulture) + @"\";

            tileImageFileName += column.ToString(CultureInfo.InvariantCulture);

            var extension = ImageFormat.ToString().ToLowerInvariant();
            tileImageFileName += "." + extension;

            return tileImageFileName;
        }

        private static string GetTemporaryFolder()
        {
            var returnValue = string.Empty;
            if (string.IsNullOrEmpty(returnValue))
            {
                returnValue = Environment.GetEnvironmentVariable("Temp");
            }

            if (string.IsNullOrEmpty(returnValue))
            {
                returnValue = Environment.GetEnvironmentVariable("Tmp");
            }

            if (string.IsNullOrEmpty(returnValue))
            {
                returnValue = @"c:\GSuiteTemp";
            }
            else
            {
                returnValue = returnValue + "\\GSuite";
            }

            returnValue = returnValue + "\\PersistentCaches";

            return returnValue;
        }
    }
}
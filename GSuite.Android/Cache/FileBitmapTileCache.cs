using Mapgenix.Canvas;
using System;
using System.Globalization;
using System.IO;

namespace Mapgenix.GSuite.Android
{
    /// <summary>
    /// Concrete class inheriting from BaseNativeImageTileCache.
    /// Tiles are saved in disk.
    /// </summary>
    [Serializable]
    public class FileNativeImageTileCache : BaseNativeImageTileCache
    {
        private string _cacheDirectory;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public FileNativeImageTileCache()
            : this(string.Empty, string.Empty, TileImageFormat.Png, new TileMatrix(590591790))
        {
        }

        /// <summary>
        /// Overloaded constructor.
        /// </summary>
        /// <param name="cacheDirectory">Cache directory of the tile cache.</param>
        /// <param name="cacheId">Cache id of the tile cache.</param>
        /// <param name="imageFormat">Image format of the tile cahce.</param>
        /// <param name="tileMatrix">Tile matrix of the tile cache.</param>
        public FileNativeImageTileCache(string cacheDirectory, string cacheId, TileImageFormat imageFormat,
            TileMatrix tileMatrix)
            : base(cacheId, imageFormat, tileMatrix)
        {
            CacheDirectory = cacheDirectory;
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
        /// Returns a NativeImageTile based on a row and a column. 
        /// </summary>
        /// <param name="row">Target row for the tile to retrieve.</param>
        /// <param name="column">Target column for the tile to retrieve.</param>
        /// <returns>A NativeImageTile based on a row and a column.</returns>
        protected override NativeImageTile GetTileCore(long row, long column)
        {
            var tileImageFileName = GetTileImageFileName(row, column);

            var cell = TileMatrix.GetCell(row, column);
            var tile = new NativeImageTile(cell.BoundingBox, TileMatrix.Scale);

            if (File.Exists(tileImageFileName))
            {
                tile.NativeImage = File.ReadAllBytes(tileImageFileName);
            }
            return tile;
        }

        /// <summary>
        /// Saves the target tile.
        /// </summary>
        /// <param name="tile">Target tile to be saved.</param>
        protected override void SaveTileCore(BaseTile tile)
        {
            var nativeImageTile = tile as NativeImageTile;
            if (nativeImageTile != null)
            {
                var cell = TileMatrix.GetCell(tile.BoundingBox.GetCenterPoint());
                var tileImageFileName = GetTileImageFileName(cell.Row, cell.Column);
                var directory =
                    tileImageFileName.Remove(tileImageFileName.LastIndexOf(@"\", StringComparison.OrdinalIgnoreCase));

                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                if (!File.Exists(tileImageFileName))
                {
                    File.WriteAllBytes(tileImageFileName, nativeImageTile.NativeImage);
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
            var tileImageFileName = GetTileImageFileName(cell.Row, cell.Column);
            if (File.Exists(tileImageFileName))
            {
                File.Delete(tileImageFileName);
            }
        }

        private string GetTileImageFileName(long row, long column)
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

            if (!string.IsNullOrEmpty(TileMatrix.Id))
            {
                tileImageFileName += TileMatrix.Id + @"\";
            }

            tileImageFileName += row.ToString(CultureInfo.InvariantCulture) + @"\";

            tileImageFileName += column.ToString(CultureInfo.InvariantCulture);

            var extension = ImageFormat.ToString().ToLowerInvariant();
            tileImageFileName += "." + extension;

            return tileImageFileName;
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
                Directory.Delete(deleteFolder, true);
            }
        }
    }
}
using System;
using System.Drawing;
using Mapgenix.Canvas.Properties;
using Mapgenix.Shapes;

namespace Mapgenix.Canvas
{
    /// <summary>
    /// Base class for the tile cache system.
    /// </summary>
    /// <remarks>The tile cache system helps to improve the performance of
    /// map drawing, especially in case of large data or remote server images.</remarks>
    [Serializable]
    public abstract class BaseTileCache
    {
        private string _cacheId;
        private TileImageFormat _imageFormat;
        private short _jpegQuality;

        [NonSerialized] private Bitmap _loadingTileImage;

        [NonSerialized] private Bitmap _noDataTileImage;

        private TileAccessMode _tileAccessMode;
        private TileMatrix _tileMatrix;

        /// <summary>
        /// Constructor of the class.
        /// </summary>
        /// <param name="cacheId">Cache identifier.</param>
        /// <param name="imageFormat">Image format.</param>
        /// <param name="tileMatrix">Tile matrix system used for calculating tiles.</param>
        protected BaseTileCache(string cacheId, TileImageFormat imageFormat, TileMatrix tileMatrix)
        {
            _cacheId = cacheId;
            _imageFormat = imageFormat;
            _jpegQuality = 100; 
            _tileMatrix = tileMatrix;
            _tileAccessMode = TileAccessMode.Default;
        }

        /// <summary>
        /// Gets or sets the TileMatrix used in calculation of tiles.
        /// </summary>
       public TileMatrix TileMatrix
        {
            get { return _tileMatrix; }
            set { _tileMatrix = value; }
        }

       /// <summary>
       /// Gets or sets the Mode for the TileCache access of the tiles. The Default value is ReadAddDelete
       /// </summary>
       /// <remarks>In order to take affect, set the ReadOnly property to false.</remarks>
        public TileAccessMode TileAccessMode
        {
            get { return _tileAccessMode; }
            set { _tileAccessMode = value; }
        }

        /// <summary>
        /// Returns a preset image when tile data is missing.
        /// </summary>
        public Bitmap NoDataTileImage
        {
            get
            {
                if (_noDataTileImage == null)
                {
                    _noDataTileImage = GetCompressedOrStretchedImage(Resources.NoImageTile, _tileMatrix.TileWidth,
                        _tileMatrix.TileHeight);
                }
                return (Bitmap) _noDataTileImage.Clone();
            }
        }

        /// <summary>
        /// Returns a preset image when a tile is loading.
        /// </summary>
        public Bitmap LoadingTileImage
        {
            get
            {
                if (_loadingTileImage == null)
                {
                    _loadingTileImage = GetCompressedOrStretchedImage(Resources.LoadingTile, _tileMatrix.TileWidth,
                        _tileMatrix.TileHeight);
                }
                return (Bitmap) _loadingTileImage.Clone();
            }
        }

        /// <summary>
        /// Gets or sets the tile image format.
        /// </summary>
        public TileImageFormat ImageFormat
        {
            get { return _imageFormat; }
            set { _imageFormat = value; }
        }

        /// <summary>
        /// Gets or sets the Jpeg quality. This property only takes effect when setting 
        /// the ImageFormat to Jpeg.
        /// </summary>
        public short JpegQuality
        {
            get { return _jpegQuality; }
            set
            {
                _jpegQuality = value;
            }
        }

        /// <summary>
        /// Gets or sets the id of the TileCache.
        /// </summary>
        public string CacheId
        {
            get { return _cacheId; }
            set { _cacheId = value; }
        }

        /// <summary>
        /// Saves the target tile passed in.
        /// </summary>
        /// <param name="tile">Target tile to be saved.</param>
        /// <remarks>This method does not take effect when ReadOnly is set to true.</remarks>
        /// <exception cref="System.ArgumentNullException" caption="ArgumentNullException">When target tile is null, it throws an ArgumentNullException.</exception>
        public void SaveTile(BaseTile tile)
        {
            Validators.CheckParameterIsNotNull(tile, "tile");

            if (_tileAccessMode == TileAccessMode.Default || _tileAccessMode == TileAccessMode.ReadAdd ||
                _tileAccessMode == TileAccessMode.ReadAddDelete)
            {
                SaveTileCore(tile);
            }
        }

        /// <summary>
        /// Saves the bitmap based on an extent into tiles.
        /// </summary>
        /// <param name="bitmap">Target bitmap to be saved into tiles.</param>
        /// <param name="bitmapExtent">Target extent for the bitmap.</param>
        /// <exception cref="System.ArgumentNullException" caption="ArgumentNullException">When the target bitmap is null, it throws an ArgumentNullException.</exception>
        public void SaveTiles(Bitmap bitmap, RectangleShape bitmapExtent)
        {
            Validators.CheckParameterIsNotNull(bitmap, "bitmap");

            var cells = _tileMatrix.GetIntersectingCells(bitmapExtent);

            try
            {
                foreach (var cell in cells)
                {
                    var cellBitmap = ExtractBitmap(bitmap, bitmapExtent, cell.BoundingBox, _tileMatrix.TileWidth,
                        _tileMatrix.TileHeight);


                    var savingBitmapTile = new BitmapTile(cellBitmap, cell.BoundingBox, _tileMatrix.Scale);
                    SaveTile(savingBitmapTile);
                }
            }
            finally
            {
                if (bitmap != null)
                {
                    bitmap.Dispose();
                }
            }
        }

        /// <summary>
        /// Deletes the tiles based on a world extent.
        /// </summary>
        /// <param name="worldExtent">Target extent to delete all tiles that are inside.</param>
        public void DeleteTiles(RectangleShape worldExtent)
        {
            var cells = _tileMatrix.GetIntersectingCells(worldExtent);

            foreach (var cell in cells)
            {
                var bitmapTile = new BitmapTile(cell.BoundingBox, _tileMatrix.Scale);
                DeleteTile(bitmapTile);
            }
        }

        /// <summary>
        /// Deletes the target tile passed in.
        /// </summary>
        /// <param name="tile">Target tile to be deleted.</param>
        /// <remarks>This method does not take effect if ReadOnly is set to true.</remarks>
        /// <exception cref="System.ArgumentNullException" caption="ArgumentNullException">When the target tile is null, it throws an ArgumentNullException.</exception>
        public void DeleteTile(BaseTile tile)
        {
            Validators.CheckParameterIsNotNull(tile, "tile");
            if (_tileAccessMode == TileAccessMode.Default || _tileAccessMode == TileAccessMode.ReadAddDelete)
            {
                DeleteTileCore(tile);
            }
        }

        /// <summary>
        /// Clears all the tiles in the tile cache.
        /// </summary>
        /// <remarks>This method does not take effect if ReadOnly is set to true.</remarks>
        public void ClearCache()
        {
            if (_tileAccessMode == TileAccessMode.Default || _tileAccessMode == TileAccessMode.ReadAddDelete)
            {
                ClearCacheCore();
            }
        }

        /// <summary>
        /// This abstract method clears all the tiles in the tile cache. The 
        /// sub BaseTileCache class must implement this method.
        /// </summary>
        /// <remarks>This method does not take effect if ReadOnly is set to true.</remarks>
       protected abstract void ClearCacheCore();

       /// <summary>
       /// This abstract method saves the target tile passed in. The 
       /// sub BaseTileCache class must implement this method.
       /// </summary>
       /// <param name="tile">Target tile to be saved.</param>
       /// <remarks>This method does not take effect when ReadOnly is set to true.</remarks>
        protected abstract void SaveTileCore(BaseTile tile);

        /// <summary>
        /// This abstract method deletes the target tile passed in. The
        /// sub BaseTileCache class must implement this method.
        /// </summary>
        /// <param name="tile">The target tile to be deleted.</param>
        /// <remarks>This method does not take effect if ReadOnly is set to true.</remarks>
        protected abstract void DeleteTileCore(BaseTile tile);


        private static Bitmap ExtractBitmap(Bitmap sourceImage, RectangleShape sourceImageExtent,
            RectangleShape tileExtent, int tileImageWidth, int tileImageHeight)
        {
            Bitmap resultBitmap = null;

            if (sourceImage != null)
            {
                var upperLeftPoint = ExtentHelper.ToScreenCoordinate(sourceImageExtent, tileExtent.UpperLeftPoint,
                    Convert.ToSingle(sourceImage.Width), Convert.ToSingle(sourceImage.Height));
                resultBitmap = new Bitmap(tileImageWidth, tileImageHeight);


                using (var graphics = Graphics.FromImage(resultBitmap))
                {
                    graphics.DrawImageUnscaled(sourceImage, -(int) Math.Round(upperLeftPoint.X),
                        -(int) Math.Round(upperLeftPoint.Y));
                }
            }

            return resultBitmap;
        }

        private static Bitmap GetCompressedOrStretchedImage(Image offScaleImage, int tileWidth, int tileHeight)
        {
            Bitmap resultImage = null;
            if (offScaleImage != null)
            {
                Graphics graphics = null;

                try
                {
                    var sourceRectangle = new Rectangle(0, 0, offScaleImage.Width, offScaleImage.Height);
                    var destinationRectangle = new Rectangle(0, 0, tileWidth, tileHeight);

                    resultImage = new Bitmap(tileWidth, tileHeight);
                    graphics = Graphics.FromImage(resultImage);
                    graphics.DrawImage(offScaleImage, destinationRectangle, sourceRectangle, GraphicsUnit.Pixel);
                }
                finally
                {
                    if (graphics != null)
                    {
                        graphics.Dispose();
                    }
                    offScaleImage.Dispose();
                }
            }

            return resultImage;
        }
    }
}
using System;
using System.Collections.ObjectModel;
using Mapgenix.Shapes;

namespace Mapgenix.Canvas
{
    /// <summary>
    /// Base class for the native tile cache system which inherits from BaseTileCache class.
    /// </summary>
    /// <remarks>The tile Cache system helps to improve the performance of
    /// map drawing, especially in case of large data or remote server images.</remarks>
    [Serializable]
    public abstract class BaseNativeImageTileCache : BaseTileCache
    {
        /// <summary>
        /// Constructor of the class.
        /// </summary>
        /// <param name="cacheId">Cache identifier</param>
        /// <param name="imageFormat">Image format.</param>
        /// <param name="tileMatrix">Tile matrix system used for calculating tiles.</param>
        protected BaseNativeImageTileCache(string cacheId, TileImageFormat imageFormat, TileMatrix tileMatrix)
            : base(cacheId, imageFormat, tileMatrix)
        {
        }

        /// <summary>
        /// Returns the NativeImageTile based on the bounding box passed in.
        /// </summary>
        /// <param name="tileBoundingBox">Target bounding box for the tile to retrieve.</param>
        /// <returns>NativeImageTile based on the bounding box passed in.</returns>
          public NativeImageTile GetTile(RectangleShape tileBoundingBox)
        {
            Validators.CheckMapEngineExtentIsValid(tileBoundingBox, "tileExtent");

            var cell = TileMatrix.GetCell(tileBoundingBox.GetCenterPoint());
            return GetTileCore(cell.Row, cell.Column);
        }

          /// <summary>
          /// Returns the NativeImageTile based on a row and a column.
          /// </summary>
          /// <param name="row">Target row for the tile to retrieve.</param>
          /// <param name="column">Target column for the tile to retrieve.</param>
          /// <returns>NativeImageTile based on a row and a column passed in.</returns>
        public NativeImageTile GetTile(long row, long column)
        {
            return GetTileCore(row, column);
        }

        /// <summary>
        /// Returns a collection of NativeImageTile based on a  world extent.
        /// </summary>
        /// <param name="extent">Target world extent to retrieve the collection of NativeImageTile.</param>
        /// <returns>Collection of NativeImageTiles based on a world extent passed in.</returns>
        public Collection<NativeImageTile> GetTiles(RectangleShape extent)
        {
            Validators.CheckMapEngineExtentIsValid(extent, "extent");

            var cells = TileMatrix.GetIntersectingCells(extent);

            var tiles = new Collection<NativeImageTile>();
            foreach (var cell in cells)
            {
                var tile = GetTileCore(cell.Row, cell.Column);
                tiles.Add(tile);
            }

            return tiles;
        }

        /// <summary>
        /// Abstract method returning the NativeImageTile based on a row and a column. 
        /// Each concrete TileCache needs to implement its logic to retrieve an image tile from a row and a column.
        /// </summary>
        /// <param name="row">Target row for the tile to retrieve.</param>
        /// <param name="column">Target column for the tile to retrieve.</param>
        /// <returns>NativeImageTile based on a row and a column passed in.</returns>
        protected abstract NativeImageTile GetTileCore(long row, long column);
    }
}
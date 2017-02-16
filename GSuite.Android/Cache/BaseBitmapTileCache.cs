using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Mapgenix.Shapes;
using Mapgenix.Canvas;
using Android.Graphics;
using NativeAndroid = Android;
using Android.Graphics.Drawables.Shapes;

namespace Mapgenix.GSuite.Android
{
    /// <summary>
    /// Base class for BitmapTileCache system which inherits from BaseTileCache class.
    /// </summary>
    /// <remarks>Tile cache is an efficient way to improve drawing performance especially in case of large data or remote server images.</remarks>
    [Serializable]
    public abstract class BaseBitmapTileCache : BaseTileCache
    {
        /// <summary>
        /// Constructor of the class.
        /// </summary>
        /// <param name="cacheId">Cache identifier.</param>
        /// <param name="imageFormat">Image Format showing.</param>
        /// <param name="tileMatrix">Tile matrix system for calculating tiles.</param>
        protected BaseBitmapTileCache(string cacheId, TileImageFormat imageFormat, TileMatrix tileMatrix)
            : base(cacheId, imageFormat, tileMatrix)
        {
        }

        /// <summary>
        /// Returns the BitmapTile based on the bounding box passed in.
        /// </summary>
        /// <param name="tileBoundingBox">The target bounding box for the tile to retrieve.</param>
        /// <returns>Returns a BitmapTile based on the bounding box passed in.</returns>
        public BitmapTile GetTile(RectangleShape tileBoundingBox)
        {
            Validators.CheckMapEngineExtentIsValid(tileBoundingBox, "tileBoundingBox");

            var cell = TileMatrix.GetCell(tileBoundingBox.GetCenterPoint());
            return GetTileCore(cell.Row, cell.Column);
        }

        /// <summary>
        /// Returns the BitmapTile based on a row and a column.
        /// </summary>
        /// <param name="row">The target row for the tile to retrieve.</param>
        /// <param name="column">The target column for the tile to retrieve.</param>
        /// <returns>Returns a BitmapTile based on a row and a column passed in.</returns>
        public BitmapTile GetTile(long row, long column)
        {
            return GetTileCore(row, column);
        }

        /// <summary>
        /// Returns a collection of BitmapTiles based on a bounding box.
        /// </summary>
        /// <param name="worldExtent">The target bounding box for the tile to retrieve.</param>
        /// <returns>Returns a collection of BitmapTiles based on a bounding box passed in.</returns>
        public Collection<BitmapTile> GetTiles(RectangleShape worldExtent)
        {
            Validators.CheckMapEngineExtentIsValid(worldExtent, "worldExtent");

            var returnCells = TileMatrix.GetIntersectingCells(worldExtent);

            var tiles = new Collection<BitmapTile>();
            foreach (var extentCell in returnCells)
            {
                var tile = GetTileCore(extentCell.Row, extentCell.Column);
                tiles.Add(tile);
            }


            return tiles;
        }

        /// <summary>
        /// Returns the BitmapTile based on a row and a column. 
        /// The concrete class inheriting from BaseTileCache needs to implement the logic to retrieve a BitmapTile tile based on a row and a column.
        /// </summary>
        /// <param name="row">The target row for the tile to retrieve.</param>
        /// <param name="column">The target column for the tile to retrieve.</param>
        /// <returns>Returns a BitmapTile based on a row and a column passed in.</returns>
        protected abstract BitmapTile GetTileCore(long row, long column);


        /// <summary>
        /// Returns the higher scale (or higher zoom level) BitmapTile used for preview effects when Zooming in or out.
        /// </summary>
        /// <param name="tileBoundingBox">The target bounding box for the tile to retrieve.</param>
        /// <param name="tileScale">The current tile scale.</param>
        /// <returns>Returns the higher scale(or higher zoomLevel) bitmap tile.</returns>
        public BitmapTile GetHigherScaleTile(RectangleShape tileBoundingBox, double tileScale)
        {
            return GetHigherScaleTileCore(tileBoundingBox, tileScale);
        }

        /// <summary>
        /// Can be overriden by its sub classes as a virtual method.
        /// Returns the higher scale (or higher zoom level) BitmapTile used for preview effects when Zooming in or out.
        /// </summary>
        /// <param name="tileBoundingBox">The target bounding box for the tile to retrieve.</param>
        /// <param name="tileScale">The current tile scale.</param>
        /// <returns>Returns the higher scale(or higher zoomLevel) bitmap tile.</returns>
        protected virtual BitmapTile GetHigherScaleTileCore(RectangleShape tileBoundingBox, double tileScale)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Merges the collection of bitmap tiles passed in into a larger bitmap tile.
        /// </summary>
        /// <param name="tilesToMerge">The collection of bitmap tiles to be merged.</param>
        /// <param name="scale">The target scale for resulting larger merged bitmap tile.</param>
        /// <returns>Returns a larger merged bitmap tile.</returns>
        protected BitmapTile MergeBitmaps(IEnumerable<BitmapTile> tilesToMerge, double scale)
        {
            Validators.CheckParameterIsNotNull(tilesToMerge, "tilesToMerge");

            var totalTilesExtent = GetExpandToIncludeExtent(tilesToMerge);
            var entireWidth = 0;
            var entireHeight = 0;
            foreach (var tile in tilesToMerge)
            {
                if (tile.Bitmap != null)
                {
                    entireWidth = (int)Math.Round(totalTilesExtent.Width * tile.Bitmap.Width / tile.BoundingBox.Width);
                    entireHeight = (int)Math.Round(totalTilesExtent.Height * tile.Bitmap.Height / tile.BoundingBox.Height);
                    break;
                }
            }
            if (entireWidth == 0 || entireHeight == 0)
            {
                return null;
            }


            var sourceRectangle = new Rect(0, 0, entireWidth, entireHeight);
            var mergedBitmap = Bitmap.CreateBitmap(sourceRectangle.Width(), sourceRectangle.Height(), Bitmap.Config.Argb4444);
            NativeAndroid.Graphics.Canvas graphics = null;

            try
            {
                graphics = new NativeAndroid.Graphics.Canvas();

                foreach (var bitmapTile in tilesToMerge)
                {
                    var drawingUpperLeftX = (bitmapTile.BoundingBox.UpperLeftPoint.X - totalTilesExtent.UpperLeftPoint.X) /
                                            totalTilesExtent.Width * mergedBitmap.Width;
                    var drawingUpperLeftY = (totalTilesExtent.UpperLeftPoint.Y - bitmapTile.BoundingBox.UpperLeftPoint.Y) /
                                            totalTilesExtent.Height * mergedBitmap.Height;

                    if (bitmapTile.Bitmap != null)
                    {
                        graphics.DrawBitmap(bitmapTile.Bitmap, Convert.ToSingle(Math.Round(drawingUpperLeftX)), Convert.ToSingle(Math.Round(drawingUpperLeftY)), null);
                    }
                }
            }
            finally
            {
                if (graphics != null)
                {
                    graphics.Dispose();
                }
            }
            return new BitmapTile(mergedBitmap, totalTilesExtent, scale);
        }

        private static RectangleShape GetExpandToIncludeExtent(IEnumerable<BitmapTile> tiles)
        {
            RectangleShape totalExtent = null;
            foreach (var tile in tiles)
            {
                if (totalExtent == null)
                {
                    totalExtent = (RectangleShape)tile.BoundingBox.CloneDeep();
                }
                else
                {
                    totalExtent.ExpandToInclude(tile.BoundingBox);
                }
            }

            return totalExtent;
        }
    }
}
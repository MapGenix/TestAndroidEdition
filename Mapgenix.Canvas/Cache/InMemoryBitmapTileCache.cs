using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using Mapgenix.Shapes;
using Mapgenix.Utils;

namespace Mapgenix.Canvas
{
    /// <summary>
    /// Concrete class inheriting from BaseBitmapTileCache.
    /// Tiles are saved in memory.
    /// </summary>
    [Serializable]
    public class InMemoryBitmapTileCache : BaseBitmapTileCache
    {
        private readonly SafeCollection<Dictionary<string, BitmapTile>> _cachedTilesByScale;
        private int _maximumTilesCount;
        private Queue<KeyValuePair<string, string>> _referenceTiles;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public InMemoryBitmapTileCache()
            : base(Guid.NewGuid().ToString(), TileImageFormat.Png, new TileMatrix(590591790))
        {
            _cachedTilesByScale = new SafeCollection<Dictionary<string, BitmapTile>>();
            _maximumTilesCount = 1000;
            _referenceTiles = new Queue<KeyValuePair<string, string>>();
        }

        /// <summary>
        /// Gets or sets the maximum tiles count.
        /// </summary>
        public int MaximumTilesCount
        {
            get { return _maximumTilesCount; }
            set { _maximumTilesCount = value; }
        }

        /// <summary>
        /// Gets the cached tiles. Each item is a Dictionary with scale as key.
        /// </summary>
        public SafeCollection<Dictionary<string, BitmapTile>> CachedTilesByScale
        {
            get { return _cachedTilesByScale; }
        }

        /// <summary>
        /// Returns a BitmapTile based on row and a column. 
        /// </summary>
        /// <param name="row">Target row for the tile to retrieve.</param>
        /// <param name="column">Target column for the tile to retrieve.</param>
        /// <returns>Returns a BitmapTile based on a row and a column.</returns>
        protected override BitmapTile GetTileCore(long row, long column)
        {
            var rowAndColumn = string.Format(CultureInfo.InvariantCulture, "{0}:{1}", row, column);

            var extentCell = TileMatrix.GetCell(row, column);
            var bitmapTile = new BitmapTile(extentCell.BoundingBox, TileMatrix.Scale);

            var scaleKey = bitmapTile.Scale.ToString(CultureInfo.InvariantCulture);

            if (_cachedTilesByScale.Contains(scaleKey))
            {
                var value = _cachedTilesByScale[scaleKey];

                if (value.ContainsKey(rowAndColumn))
                {
                    if (value[rowAndColumn].Bitmap != null)
                    {
                        bitmapTile.Bitmap = (Bitmap) value[rowAndColumn].Bitmap.Clone();
                    }
                }
            }

            return bitmapTile;
        }

        /// <summary>
        /// Saves the target tile passed in.
        /// </summary>
        /// <param name="tile">Target tile to be saved.</param>
        /// <remarks>Does not take effect if ReadOnly is set to true.</remarks>
        protected override void SaveTileCore(BaseTile tile)
        {
            if (_referenceTiles.Count == _maximumTilesCount)
            {
                var scaleRowColumn = _referenceTiles.Dequeue();
                var scale = scaleRowColumn.Key;
                var rowColumn = scaleRowColumn.Value;
                var removingTile = _cachedTilesByScale[scale][rowColumn];
                if (removingTile.Bitmap != null)
                {
                   
                    removingTile.Bitmap.Dispose();
                    removingTile.Bitmap = null;
                }
                _cachedTilesByScale[scale].Remove(rowColumn);
            }

            var bitmapTile = tile as BitmapTile;
            if (bitmapTile != null)
            {
                var cell = TileMatrix.GetCell(tile.BoundingBox.GetCenterPoint());
                var scaleKey = tile.Scale.ToString(CultureInfo.InvariantCulture);
                var rowAndColumn = string.Format(CultureInfo.InvariantCulture, "{0}:{1}", cell.Row, cell.Column);

                if (_cachedTilesByScale.Contains(scaleKey))
                {
                    var value = _cachedTilesByScale[scaleKey];

                    if (value.ContainsKey(rowAndColumn))
                    {
                        if (value[rowAndColumn].Bitmap != null)
                        {
                            value[rowAndColumn].Bitmap.Dispose();
                            value[rowAndColumn].Bitmap = null;
                        }
                        value.Remove(rowAndColumn);
                    }

                    _cachedTilesByScale[scaleKey].Add(rowAndColumn, bitmapTile);
                }
                else
                {
                    var newValue = new Dictionary<string, BitmapTile>();
                    newValue.Add(rowAndColumn, bitmapTile);
                    _cachedTilesByScale.Add(scaleKey, newValue);
                }

                var isExisted = false;
                for (var i = 0; i < _referenceTiles.Count; i++)
                {
                    var tempValue = _referenceTiles.Dequeue();
                    if (string.CompareOrdinal(tempValue.Key, scaleKey) == 0 &&
                        string.CompareOrdinal(tempValue.Value, rowAndColumn) == 0)
                    {
                        isExisted = true;
                    }
                    _referenceTiles.Enqueue(tempValue);
                }

                if (!isExisted)
                {
                    _referenceTiles.Enqueue(new KeyValuePair<string, string>(scaleKey, rowAndColumn));
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
            var bitmapTile = tile as BitmapTile;
            if (bitmapTile != null)
            {
                var scaleKey = tile.Scale.ToString(CultureInfo.InvariantCulture);
                var cell = TileMatrix.GetCell(tile.BoundingBox.GetCenterPoint());
                var rowAndColumn = string.Format(CultureInfo.InvariantCulture, "{0}:{1}", cell.Row, cell.Column);

                if (_cachedTilesByScale.Contains(scaleKey))
                {
                    var value = _cachedTilesByScale[scaleKey];

                    if (value.ContainsKey(rowAndColumn))
                    {
                        if (value[rowAndColumn].Bitmap != null)
                        {
                            value[rowAndColumn].Bitmap.Dispose();
                            value[rowAndColumn].Bitmap = null;
                        }
                        value.Remove(rowAndColumn);

                        var tempReferenceTiles = new Queue<KeyValuePair<string, string>>();
                        for (var i = 0; i < _referenceTiles.Count; i++)
                        {
                            var item = _referenceTiles.Dequeue();
                            if (item.Key != scaleKey || item.Value != rowAndColumn)
                            {
                                tempReferenceTiles.Enqueue(item);
                            }
                        }
                        _referenceTiles = tempReferenceTiles;
                    }
                }
            }
        }

        /// <summary>
        /// Clears all the tiles in the tile cache.
        /// </summary>
        /// <remarks>Does not take effect if ReadOnly is set to true.</remarks>
        protected override void ClearCacheCore()
        {
            foreach (var key in _cachedTilesByScale.GetKeys())
            {
                foreach (var bitmapTile in _cachedTilesByScale[key].Values)
                {
                    if (bitmapTile.Bitmap != null)
                    {
                        bitmapTile.Bitmap.Dispose();
                        bitmapTile.Bitmap = null;
                    }
                }
            }
            _cachedTilesByScale.Clear();
            _referenceTiles.Clear();
        }

        /// <summary>
        /// Overrides the logic of its base class BaseBitmapTileCache.
        /// Returns the higher scale BitmapTile used for drawing effects when zooming in or out.
        /// </summary>
        /// <param name="tileBoundingBox">Current tile bounding box.</param>
        /// <param name="tileScale">Current tile scale.</param>
        /// <returns>The higher scale(higher zoomLevel) bitmap tile.</returns>
        protected override BitmapTile GetHigherScaleTileCore(RectangleShape tileBoundingBox, double tileScale)
        {
            var scaleStrings = CachedTilesByScale.GetKeys();
            var scales = new double[scaleStrings.Count];
            for (var i = 0; i < scaleStrings.Count; i++)
            {
                scales[i] = double.Parse(scaleStrings[i], CultureInfo.InvariantCulture);
            }

            BitmapTile higherScaleTile = null;

            for (var i = scales.Length - 1; i >= 0; i--)
            {
                var higherZoomLevelScale = scales[i];
                if (higherZoomLevelScale/tileScale < 33 && higherZoomLevelScale/tileScale > 1)
                    // keep the different of zoomlevels not bigger than 5.
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

            return higherScaleTile;
        }
    }
}
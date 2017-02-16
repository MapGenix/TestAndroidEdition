using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using Mapgenix.Canvas;
using Mapgenix.Shapes;
using Android.Graphics;
using NativeAndroid = Android;

namespace Mapgenix.GSuite.Android
{
    /// <summary>
    /// The base class of tile for forming tile overlay.
    /// </summary>
    public partial class Tile
    {
        private double _targetResolution;
        private void DrawFull(BaseGeoCanvas geoCanvas)
        {
            lock (TileCache)
            {
                double targetScale = MapUtil.GetScale(geoCanvas.MapUnit, geoCanvas.CurrentWorldExtent, geoCanvas.Width, geoCanvas.Height);
                _targetResolution = MapUtil.GetResolutionFromScale(targetScale, geoCanvas.MapUnit);
                TileCache.TileMatrix.Scale = targetScale;
                TileCache.TileMatrix.Id = targetScale.ToString(CultureInfo.InvariantCulture);
                Collection<TileMatrixCell> drawingCells = TileCache.TileMatrix.GetIntersectingCells(geoCanvas.CurrentWorldExtent);

                Collection<TileMatrixCell> uncachedCells = new Collection<TileMatrixCell>();
                Dictionary<TileMatrixCell, BitmapTile> cachedCells = new Dictionary<TileMatrixCell, BitmapTile>();
                foreach (TileMatrixCell tempCell in drawingCells)
                {
                    try
                    {
                        BitmapTile tempTile = TileCache.GetTile(tempCell.BoundingBox);
                        if (tempTile != null && tempTile.Bitmap != null)
                        {
                            cachedCells.Add(tempCell, tempTile);
                        }
                        else
                        {
                            uncachedCells.Add(tempCell);
                        }
                    }
                    catch { }
                }

                RectangleShape uncachedExtent = GetUncachedExtent(uncachedCells);
                if (uncachedExtent != null)
                {
                    cachedCells = ExcludeCachedCellsInsideOfUncachedArea(uncachedExtent, cachedCells);

                    DrawUncachedImage(geoCanvas, uncachedExtent);
                }

                DrawCachedImages(geoCanvas, cachedCells);
            }
        }

        private void DrawUncachedImage(BaseGeoCanvas geoCanvas, RectangleShape uncachedExtent)
        {
            double uncachedTileWidth = uncachedExtent.Width / _targetResolution;
            double uncachedTileHeight = uncachedExtent.Height / _targetResolution;

            Bitmap uncachedBitmap = Bitmap.CreateBitmap((int)uncachedTileWidth, (int)uncachedTileHeight, null);
            GeoImage uncachedGeoImage = null;
            MemoryStream uncachedImageStream = null;

            try
            {
                GdiPlusGeoCanvas tempGeoCanvas = new GdiPlusGeoCanvas();
                GdiPlusGeoCanvas originGdiPlusGeoCanvas = geoCanvas as GdiPlusGeoCanvas;
                if (originGdiPlusGeoCanvas != null)
                {
                    tempGeoCanvas.DrawingQuality = originGdiPlusGeoCanvas.DrawingQuality;
                    tempGeoCanvas.CompositingQuality = originGdiPlusGeoCanvas.CompositingQuality;
                    tempGeoCanvas.SmoothingMode = originGdiPlusGeoCanvas.SmoothingMode;
                }

                tempGeoCanvas.BeginDrawing(uncachedBitmap, uncachedExtent, geoCanvas.MapUnit);
                DrawCore(tempGeoCanvas);
                tempGeoCanvas.EndDrawing();

                if (!CancellationPending) SaveLatestCaches(uncachedBitmap, uncachedExtent);

                uncachedImageStream = new MemoryStream();
                uncachedBitmap.Compress(Bitmap.CompressFormat.Png, 0, uncachedImageStream);
                uncachedGeoImage = new GeoImage(uncachedImageStream);
                PointShape uncachedWorldCenter = uncachedExtent.GetCenterPoint();
                geoCanvas.DrawWorldImageWithoutScaling(uncachedGeoImage, uncachedWorldCenter.X, uncachedWorldCenter.Y, DrawingLevel.LevelOne);
            }
            finally
            {
                if (uncachedBitmap != null) { uncachedBitmap.Dispose(); uncachedBitmap = null; }
                if (uncachedGeoImage != null) { uncachedGeoImage.Dispose(); uncachedGeoImage = null; }
            }
        }

        private void SaveLatestCaches(Bitmap uncachedBitmap, RectangleShape uncachedExtent)
        {
            Collection<TileMatrixCell> uncachedCells = TileCache.TileMatrix.GetContainedCells(uncachedExtent);
            foreach (TileMatrixCell tempCell in uncachedCells)
            {
                int cellScreenWidth = TileCache.TileMatrix.TileWidth;
                int cellScreenHeight = TileCache.TileMatrix.TileHeight;
                Bitmap partialBitmap = Bitmap.CreateBitmap(cellScreenWidth, cellScreenHeight, null);
                NativeAndroid.Graphics.Canvas parialGraphics = new NativeAndroid.Graphics.Canvas(partialBitmap);
                double left = (tempCell.BoundingBox.UpperLeftPoint.X - uncachedExtent.UpperLeftPoint.X) / _targetResolution;
                double top = (uncachedExtent.UpperLeftPoint.Y - tempCell.BoundingBox.UpperLeftPoint.Y) / _targetResolution;

                try
                {
                    Rect sourceRectangle = new Rect((int)left, (int)top, cellScreenWidth, cellScreenHeight);
                    parialGraphics.DrawBitmap(uncachedBitmap, sourceRectangle, sourceRectangle, null);
                    parialGraphics.Dispose();
                    /*
                    parialGraphics.DrawImage(uncachedBitmap, 0f, 0f, sourceRectangle, GraphicsUnit.Pixel);
                    parialGraphics.Flush();*/

                    BitmapTile imageTile = new BitmapTile(partialBitmap, tempCell.BoundingBox, TileCache.TileMatrix.Scale);
                    TileCache.SaveTile(imageTile);
                }
                catch
                {
                }
                finally
                {
                    if (partialBitmap != null) { partialBitmap.Dispose(); partialBitmap = null; }
                    if (parialGraphics != null) { parialGraphics.Dispose(); parialGraphics = null; }
                }
            }
        }

        private static void DrawCachedImages(BaseGeoCanvas geoCanvas, Dictionary<TileMatrixCell, BitmapTile> cachedCells)
        {
            foreach (KeyValuePair<TileMatrixCell, BitmapTile> tempCell in cachedCells)
            {
                GeoImage cachedGeoImage = null;
                try
                {
                    MemoryStream imageStream = new MemoryStream();
                    tempCell.Value.Bitmap.Compress(Bitmap.CompressFormat.Png, 0, imageStream);
                    cachedGeoImage = new GeoImage(imageStream);
                    PointShape drawingWorldPoint = tempCell.Key.BoundingBox.GetCenterPoint();
                    geoCanvas.DrawWorldImageWithoutScaling(cachedGeoImage, drawingWorldPoint.X, drawingWorldPoint.Y, DrawingLevel.LevelOne);
                }
                finally
                {
                    if (cachedGeoImage != null) { cachedGeoImage.Dispose(); cachedGeoImage = null; }
                }
            }
        }

        private static Dictionary<TileMatrixCell, BitmapTile> ExcludeCachedCellsInsideOfUncachedArea(RectangleShape uncachedExtent, Dictionary<TileMatrixCell, BitmapTile> cachedCells)
        {
            Dictionary<TileMatrixCell, BitmapTile> usefullCaches = new Dictionary<TileMatrixCell, BitmapTile>();
            foreach (KeyValuePair<TileMatrixCell, BitmapTile> tempCell in cachedCells)
            {
                if (!uncachedExtent.Contains(tempCell.Key.BoundingBox))
                {
                    usefullCaches.Add(tempCell.Key, tempCell.Value);
                }
            }

            return usefullCaches;
        }

        private static RectangleShape GetUncachedExtent(Collection<TileMatrixCell> uncachedCells)
        {
            RectangleShape uncachedExtent = null;
            foreach (TileMatrixCell tempCell in uncachedCells)
            {
                if (uncachedExtent == null) uncachedExtent = tempCell.BoundingBox;
                else uncachedExtent.ExpandToInclude(tempCell.BoundingBox);
            }

            return uncachedExtent;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Mapgenix.Shapes;
using Mapgenix.Canvas;
using Android.Widget;
using Android.Content;
using Android.Graphics.Drawables;
using Android.Graphics;
using Android.Views;

namespace Mapgenix.GSuite.Android
{
    /// <summary>
    /// Overlay made of tiles.
    /// </summary>
    [Serializable]
    public abstract partial class BaseTileOverlay : BaseOverlay
    {
        public event EventHandler<TimedProgressEventArgs> DrawTilesProgressChanged;

        public event EventHandler<TileEventArgs> DrawingTile;

        public event EventHandler<TileEventArgs> DrawnTile;

        [NonSerialized]
        private FrameLayout _drawingCanvas;
        [NonSerialized]
        private FrameLayout _stretchCanvas;
        /*[NonSerialized]
        private TranslateTransform _translateTransform;*/
        private TileType _tileType;
        private int _tileBuffer;
        private bool _isDrawing;
        private int _tileWidth;
        private int _tileHeight;

        /*[NonSerialized]
        private DispatcherTimer _panningTimer;*/
        private RectangleShape _targetExtentForPanning;
        private object _lockerObject;

        protected BaseTileOverlay(Context context)
            :base (context)
        {
            TileWidth = Convert.ToInt32(LayoutUnitsUtil.convertDpToPixel(128, Context.Resources.DisplayMetrics.Xdpi));
            TileHeight = Convert.ToInt32(LayoutUnitsUtil.convertDpToPixel(128, Context.Resources.DisplayMetrics.Xdpi)); ;
            ImageFormat = TileImageFormat.Png;
            JpegQuality = 80;
            _lockerObject = new object();

            TileType = TileType.MultipleTile;
            TransitionEffect = TransitionEffect.Stretch;

            _drawingCanvas = new FrameLayout(context);
            _drawingCanvas.Elevation = ZIndexes.DrawingTileCanvas;
            OverlayCanvas.AddView(_drawingCanvas);

            _stretchCanvas = new FrameLayout(context);
            _stretchCanvas.Elevation = ZIndexes.StretchTileCanvas;
            OverlayCanvas.AddView(_stretchCanvas);

            /*_translateTransform = new TranslateTransform();
            OverlayCanvas.RenderTransform = _translateTransform;*/

            /*_panningTimer = new DispatcherTimer(DispatcherPriority.Background);
            _panningTimer.Interval = TimeSpan.FromMilliseconds(200);
            _panningTimer.Tick += panningTimer_Tick;*/
        }

       
        public TileType TileType
        {
            get { return _tileType; }
            set
            {
                _tileType = value;
                if (value != TileType.SingleTile && DrawingCanvas != null && StretchCanvas != null)
                {
                    ClearTiles(ClearTilesMode.AllTiles);
                }
            }
        }

      
        public int TileWidth
        {
            get { return _tileWidth; }
            set
            {
                Validators.CheckTileSizeIsValid(value, "TileWidth");
                _tileWidth = value;
              
                ClearTiles(ClearTilesMode.AllTiles);
            }
        }

      
        public int TileHeight
        {
            get { return _tileHeight; }
            set
            {
                Validators.CheckTileSizeIsValid(value, "TileHeight");
                _tileHeight = value;
              
                ClearTiles(ClearTilesMode.AllTiles);
            }
        }

        
        public TransitionEffect TransitionEffect { get; set; }

       
        public BaseBitmapTileCache TileCache { get; set; }

      
        public TileImageFormat ImageFormat { get; set; }

       
        public int JpegQuality { get; set; }

       
        public int TileBuffer
        {
            get { return _tileBuffer; }
            set
            {
                Validators.CheckValueIsGreaterOrEqualToZero(value, "TileBuffer");
                _tileBuffer = value;
            }
        }

       
        public RectangleShape MaxExtent { get; set; }

       
        protected FrameLayout DrawingCanvas { get { return _drawingCanvas; } }

      
        protected FrameLayout StretchCanvas { get { return _stretchCanvas; } }

     
        protected override void DrawCore(RectangleShape targetExtent, OverlayRefreshType overlayRefreshType)
        {
            if (overlayRefreshType == OverlayRefreshType.Pan)
            {
                if (TileType == TileType.SingleTile)
                {
                    PanSingleTile(targetExtent);
                }
                else if (TileType == TileType.MultipleTile)
                {
                    DrawMultipleTiles(targetExtent, overlayRefreshType);
                }
                else
                {
                    /*if (_panningTimer.IsEnabled)
                    {
                        _panningTimer.Stop();
                    }*/

                    DrawMultipleTiles(targetExtent, overlayRefreshType, true);
                    _targetExtentForPanning = targetExtent;
                    //_panningTimer.Start();
                }
            }
            else
            {
                _isDrawing = true;
                /*_translateTransform.X = 0;
                _translateTransform.Y = 0;*/

                if (TileType == TileType.SingleTile)
                {
                    DrawSingleTile(targetExtent);
                }
                else if (TileType == TileType.MultipleTile || TileType == TileType.HybridTile)
                {
                    if (TileType != TileType.SingleTile && overlayRefreshType == OverlayRefreshType.Redraw)
                    {

                        for(int i = 0; i < DrawingCanvas.ChildCount; i++)
                        {
                            Tile tile = (Tile)DrawingCanvas.GetChildAt(i);
                            tile.IsOpened = false;
                        }

                        OnDrawTilesProgressChanged(new TimedProgressEventArgs(0, 0, null));
                    }

                    DrawMultipleTiles(targetExtent, overlayRefreshType);
                }
            }
        }

      
        protected override void RefreshCore()
        {
            ClearTiles(ClearTilesMode.StretchedTiles);
            DrawCore(MapArguments.CurrentExtent, OverlayRefreshType.Redraw);
        }

        protected virtual void OnDrawTilesProgressChanged(TimedProgressEventArgs args)
        {
            EventHandler<TimedProgressEventArgs> handler = DrawTilesProgressChanged;
            if (handler != null)
            {
                handler(this, args);
            }
        }

        protected virtual void OnDrawingTile(TileEventArgs args)
        {
            EventHandler<TileEventArgs> handler = DrawingTile;
            if (handler != null)
            {
                handler(this, args);
            }
        }

        protected virtual void OnDrawnTile(TileEventArgs args)
        {
            EventHandler<TileEventArgs> handler = DrawnTile;
            if (handler != null)
            {
                handler(this, args);
            }
        }

        protected abstract Tile GetTileCore();

        protected void DrawTile(Tile tile, RectangleShape targetExtent)
        {
            if (IsVisible)
            {
                RectangleShape targetDrawingExtent = targetExtent;
                if (WrapDatelineMode == WrapDatelineMode.WrapDateline)
                {
                    targetDrawingExtent = AdjustExtentForWrapDateline(targetDrawingExtent);
                }

                DrawTileCore(tile, targetDrawingExtent);
            }
        }

        protected abstract void DrawTileCore(Tile tile, RectangleShape targetExtent);

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {

                ClearTiles(ClearTilesMode.AllTiles);
            }
        }

        protected override void InitializeCore(MapArguments mapArguments)
        {
            base.InitializeCore(mapArguments);

            if (TileCache != null)
            {
                TileCache.TileMatrix.BoundingBoxUnit = mapArguments.MapUnit;
            }
        }

        protected Dictionary<string, TileMatrixCell> GetDrawingCells(RectangleShape targetExtent)
        {
            SaveOrRestoreMaxExtent();
            return GetDrawingCellsCore(targetExtent);
        }

        protected virtual Dictionary<string, TileMatrixCell> GetDrawingCellsCore(RectangleShape targetExtent)
        {
            double resolution = MapUtil.GetResolution(targetExtent, MapArguments.ActualWidth, MapArguments.ActualHeight);
            double targetScale = MapUtil.GetScale(MapArguments.MapUnit, targetExtent, MapArguments.ActualWidth, MapArguments.ActualHeight);
            TileMatrix matrix = new TileMatrix(targetScale, TileWidth, TileHeight, MapArguments.MapUnit);
            if (MaxExtent != null) { matrix.BoundingBox = MaxExtent; }

            Collection<TileMatrixCell> matrixCells = null;
            try
            {
                RectangleShape bufferedTargetExtent = GetBufferedExtent(targetExtent, resolution);
                matrixCells = matrix.GetIntersectingCells(bufferedTargetExtent);

                if (TileBuffer != 0)
                {
                    matrixCells = GetSortedCells(matrixCells, targetExtent);
                }
            }
            catch
            {
                matrixCells = new Collection<TileMatrixCell>();
            }

            int currentZoomLevelIndex = MapArguments.GetSnappedZoomLevelIndex(targetExtent);
            Dictionary<string, TileMatrixCell> cells = new Dictionary<string, TileMatrixCell>();
            foreach (TileMatrixCell matrixCell in matrixCells)
            {
                cells.Add(GetTileKey(currentZoomLevelIndex, matrixCell.Row, matrixCell.Column), matrixCell);
            }

            return cells;
        }

        private Collection<TileMatrixCell> GetSortedCells(Collection<TileMatrixCell> cells, RectangleShape displayExtent)
        {
            long drawnMinRow = long.MaxValue, drawnMinCol = long.MaxValue, drawnMaxRow = long.MinValue, drawnMaxCol = long.MinValue;
            long minRow = long.MaxValue, minCol = long.MaxValue, maxRow = long.MinValue, maxCol = long.MinValue;
            Collection<TileMatrixCell> returnedCells = new Collection<TileMatrixCell>();
            foreach (TileMatrixCell cell in cells)
            {
                minCol = minCol > cell.Column ? cell.Column : minCol;
                maxCol = maxCol < cell.Column ? cell.Column : maxCol;
                minRow = minRow > cell.Row ? cell.Row : minRow;
                maxRow = maxRow < cell.Row ? cell.Row : maxRow;

                if (!displayExtent.IsDisjointed(cell.BoundingBox))
                {
                    drawnMinCol = drawnMinCol > cell.Column ? cell.Column : drawnMinCol;
                    drawnMaxCol = drawnMaxCol < cell.Column ? cell.Column : drawnMaxCol;
                    drawnMinRow = drawnMinRow > cell.Row ? cell.Row : drawnMinRow;
                    drawnMaxRow = drawnMaxRow < cell.Row ? cell.Row : drawnMaxRow;
                    returnedCells.Add(cell);
                }
            }

            long colCount = maxCol - minCol + 1;
            for (int bufferIndex = 1; bufferIndex <= TileBuffer; bufferIndex++)
            {
                long borderMinCol = drawnMinCol - bufferIndex;
                long borderMaxCol = drawnMaxCol + bufferIndex;
                long borderMinRow = drawnMinRow - bufferIndex;
                long borderMaxRow = drawnMaxRow + bufferIndex;

                Action<long, long> addToReturnedCells = (targetRowIndex, targetColumnIndex) =>
                {
                    if (targetRowIndex < minRow || targetRowIndex > maxRow || targetColumnIndex < minCol || targetColumnIndex > maxCol)
                    {
                        return;
                    }

                    long targetLongCellIndex = (targetRowIndex - minRow) * colCount + targetColumnIndex - minCol;
                    int targetCellIndex = (int)targetLongCellIndex;
                    if (targetCellIndex >= 0 && cells.Count > targetCellIndex)
                    {
                        returnedCells.Add(cells[targetCellIndex]);
                    }
                };

                for (long rowIndex = borderMinRow; rowIndex <= borderMaxRow; rowIndex++)
                {
                    if (rowIndex == borderMinRow || rowIndex == borderMaxRow)
                    {
                        for (long colIndex = borderMinCol; colIndex <= borderMaxCol; colIndex++)
                        {
                            addToReturnedCells(rowIndex, colIndex);
                        }
                    }
                    else
                    {
                        addToReturnedCells(rowIndex, borderMinCol);
                        addToReturnedCells(rowIndex, borderMaxCol);
                    }
                }
            }

            return returnedCells;
        }

        private RectangleShape GetBufferedExtent(RectangleShape targetExtent, double resolution)
        {
            if (TileBuffer == 0)
            {
                return targetExtent;
            }
            else
            {
                double bufferedWidth = resolution * TileWidth * TileBuffer;
                double bufferedHeight = resolution * TileHeight * TileBuffer;
                double minX = targetExtent.UpperLeftPoint.X - bufferedWidth;
                double maxX = targetExtent.LowerRightPoint.X + bufferedWidth;
                double minY = targetExtent.LowerRightPoint.Y - bufferedHeight;
                double maxY = targetExtent.UpperLeftPoint.Y + bufferedHeight;
                return new RectangleShape(minX, maxY, maxX, minY);
            }
        }

        private Tile GetTile(RectangleShape targetExtent, double tileScreenWidth, double tileScreenHeight, long tileColumnIndex, long tileRowIndex, int zoomLevelIndex)
        {            
            Tile newTile = GetTileCore();

            LayoutParams p = new LayoutParams((int)tileScreenWidth, (int)tileScreenHeight);
            newTile.LayoutParameters = p;

            newTile.LayoutParameters.Width = Convert.ToInt32(tileScreenWidth);
            newTile.LayoutParameters.Height = Convert.ToInt32(tileScreenHeight);
            if(newTile.LayoutParameters.Width == 0)
            {
                newTile.LayoutParameters.Width = 64;
            }
            newTile.IsAsync = (TileType != TileType.SingleTile);
            newTile.IsPartial = (TileType != TileType.SingleTile);
            newTile.RowIndex = tileRowIndex;
            newTile.ColumnIndex = tileColumnIndex;
            newTile.TargetExtent = targetExtent;
            newTile.ZoomLevelIndex = zoomLevelIndex;
            newTile.DrawingExceptionMode = DrawingExceptionMode;
            newTile.Background = new ColorDrawable(Color.Transparent);

            newTile.Opened += newTile_Opened;
            newTile.Drawing += newTile_Drawing;
            newTile.Drawn += newTile_Drawn;
            return newTile;
        }

        private void newTile_Drawn(object sender, GeoCanvasEventArgs e)
        {
            OnDrawnTile(new TileEventArgs((Tile)sender, e.GeoCanvas));
        }

        private void newTile_Drawing(object sender, GeoCanvasEventArgs e)
        {
            OnDrawingTile(new TileEventArgs((Tile)sender, e.GeoCanvas));
        }

        private bool CheckTileIsOutOfBounds(Tile tile, RectangleShape targetExtent)
        {
            bool isOutOfBounds = false;
            double resolution = MapUtil.GetResolution(targetExtent, MapArguments.ActualWidth, MapArguments.ActualHeight);
            RectangleShape bufferedExtent = GetBufferedExtent(targetExtent, resolution);
            if (tile.TargetExtent.IsDisjointed(bufferedExtent))
            {
                isOutOfBounds = true;
            }
            return isOutOfBounds;
        }

        protected virtual void DrawStretchTiles(RectangleShape targetExtent)
        {
            double currentResolution = MapArguments.CurrentResolution;

            for(int i = 0; i < DrawingCanvas.ChildCount; i++)
            {
                Tile tile = (Tile)DrawingCanvas.GetChildAt(i);
                if (!CheckTileIsOutOfBounds(tile, targetExtent))
                {
                    if (tile.ImageSource != null)
                    {
                        Tile newTile = new Tile(Context);
                        newTile.HasWatermark = false;
                        newTile.TargetExtent = tile.TargetExtent;
                        newTile.ZoomLevelIndex = tile.ZoomLevelIndex;
                        newTile.ImageSource = tile.ImageSource;

                        DrawStretchTile(newTile, targetExtent, currentResolution);
                        StretchCanvas.AddView(newTile);
                    }
                }
            }
        }

        private void ShiftAndRemoveStretchTiles(RectangleShape targetExtent, double currentResolution, bool isPanning)
        {
            Collection<Tile> removeQueries = new Collection<Tile>();

            for(int i = 0; i < StretchCanvas.ChildCount; i++)
            {
                Tile tile = (Tile)StretchCanvas.GetChildAt(i);
                bool isOutOfBounds = CheckTileIsOutOfBounds(tile, targetExtent);
                if (!isPanning && isOutOfBounds)
                {
                    removeQueries.Add(tile);
                }

                if (!isOutOfBounds)
                {
                    ShiftStretchTile(tile, targetExtent, currentResolution);
                }
            }

            foreach (Tile tile in removeQueries)
            {
                if (StretchCanvas.IndexOfChild(tile) != -1)
                {
                    StretchCanvas.RemoveView(tile);
                    tile.Dispose();
                }
            }
        }

        private void newTile_Opened(object sender, EventArgs e)
        {
            Tile currentTile = (Tile)sender;
            int totalCount = _drawingCanvas.ChildCount;
            int loadedCount = 0;

            RemoveCurrentGhostTile(currentTile.TargetExtent);

            int elapsedMilliseconds = 0;
            for(int i = 0; i < totalCount; i++)
            {
                Tile tile = (Tile)_drawingCanvas.GetChildAt(i);
                if (tile.IsOpened)
                {
                    loadedCount++;
                    elapsedMilliseconds += tile.DrawingTime.Milliseconds;
                }
            }

            int downloadPercentage = loadedCount * 100 / totalCount;
            TimedProgressEventArgs args = new TimedProgressEventArgs(elapsedMilliseconds, downloadPercentage, null);
            OnDrawTilesProgressChanged(args);

            if (downloadPercentage == 100)
            {
                ClearTiles(ClearTilesMode.StretchedTiles);
                OnDrawn(new ExtentEventArgs(MapArguments.CurrentExtent));
                _isDrawing = false;
            }
        }

        private void DrawSingleTile(RectangleShape targetExtent)
        {
            DrawingCanvas.RemoveAllViews();
            double resolution = MapArguments.CurrentResolution;
            RectangleShape bufferedTargetExtent = GetBufferedExtent(targetExtent, resolution);

            double mapScreenWidth = MapArguments.ActualWidth;
            double mapScreenHeight = MapArguments.ActualHeight;

            if (TileBuffer != 0)
            {
                mapScreenWidth += 2 * TileBuffer * TileWidth;
                mapScreenHeight += 2 * TileBuffer * TileHeight;
            }

            Tile newTile = GetTile(bufferedTargetExtent, mapScreenWidth, mapScreenHeight, 0, 0, MapArguments.GetSnappedZoomLevelIndex(targetExtent));
            LayoutParams p = new LayoutParams(newTile.Width, newTile.Height);
            p.LeftMargin = -TileBuffer * TileWidth;
            p.TopMargin = -TileBuffer * TileHeight;

            DrawingCanvas.AddView(newTile, p);
            DrawTile(newTile, bufferedTargetExtent);
        }

        private void DrawMultipleTiles(RectangleShape targetExtent, OverlayRefreshType refreshType)
        {
            DrawMultipleTiles(targetExtent, refreshType, false);
        }

        private void DrawMultipleTiles(RectangleShape targetExtent, OverlayRefreshType refreshType, bool isPanning)
        {
            lock (_lockerObject)
            {
                if (PreviousExtent != null)
                {
                    double targetScale = MapUtil.GetScale(MapArguments.MapUnit, targetExtent, MapArguments.ActualWidth, MapArguments.ActualHeight);
                    double previousScale = MapUtil.GetScale(MapArguments.MapUnit, PreviousExtent, MapArguments.ActualWidth, MapArguments.ActualHeight);

                    if (!MapUtil.IsFuzzyEqual(targetScale, previousScale))
                    {
                        ClearTiles(ClearTilesMode.StretchedTiles);
                        if (TransitionEffect == TransitionEffect.Stretch)
                        {
                            DrawStretchTiles(targetExtent);
                        }
                        ClearTiles(ClearTilesMode.DrawingTiles);
                    }
                    else
                    {
                        ShiftAndRemoveStretchTiles(targetExtent, MapArguments.CurrentResolution, isPanning);
                    }
                }

                Dictionary<string, TileMatrixCell> cells = GetDrawingCells(targetExtent);
                for (int i = DrawingCanvas.ChildCount - 1; i >= 0; i--)
                {
                    if (i >= DrawingCanvas.ChildCount) continue;

                    Tile tile = (Tile)DrawingCanvas.GetChildAt(i);
                    if (CheckTileIsOutOfBounds(tile, targetExtent) && !isPanning)
                    {
                        DrawingCanvas.RemoveView(tile);
                        DisposeTile(tile);
                    }
                    else
                    {
                        if (tile.ZoomLevelIndex != MapArguments.GetSnappedZoomLevelIndex(targetExtent))
                        {
                            DrawingCanvas.RemoveView(tile);
                            continue;
                        }

                        double offsetX = Math.Round((tile.TargetExtent.UpperLeftPoint.X - targetExtent.UpperLeftPoint.X) / tile.TargetExtent.Width * TileWidth);
                        double offsetY = Math.Round((targetExtent.UpperLeftPoint.Y - tile.TargetExtent.UpperLeftPoint.Y) / tile.TargetExtent.Height * TileHeight);

                        FrameLayout.LayoutParams p = new FrameLayout.LayoutParams(tile.LayoutParameters.Width, tile.LayoutParameters.Height);
                        p.LeftMargin = Convert.ToInt32(offsetX);
                        p.TopMargin = Convert.ToInt32(offsetY);
                        tile.LayoutParameters = p;

                        string currentCellSimpleExtentString = GetTileKey(tile.ZoomLevelIndex, tile.RowIndex, tile.ColumnIndex);
                        if (cells.ContainsKey(currentCellSimpleExtentString))
                        {
                            cells.Remove(currentCellSimpleExtentString);
                        }

                        if (refreshType == OverlayRefreshType.Redraw)
                        {
                            DrawTile(tile, tile.TargetExtent);
                        }
                    }
                }

                int currentZoomLevel = MapArguments.GetSnappedZoomLevelIndex(targetExtent);
                if (!isPanning)
                {
                    foreach (string key in cells.Keys)
                    {
                        TileMatrixCell cell = cells[key];
                        Tile tile = GetTile(cell.BoundingBox, TileWidth, TileHeight, cell.Column, cell.Row, currentZoomLevel);

                        double resolution = 1 / MapUtil.GetResolution(targetExtent, MapArguments.ActualWidth, MapArguments.ActualHeight);
                        double offsetX = Math.Round((cell.BoundingBox.UpperLeftPoint.X - targetExtent.UpperLeftPoint.X) * resolution);
                        double offsetY = Math.Round((targetExtent.UpperLeftPoint.Y - cell.BoundingBox.UpperLeftPoint.Y) * resolution);
                        FrameLayout.LayoutParams p = new FrameLayout.LayoutParams(tile.LayoutParameters.Width, tile.LayoutParameters.Height);
                        p.LeftMargin = Convert.ToInt32(offsetX);
                        p.TopMargin = Convert.ToInt32(offsetY);
                        tile.LayoutParameters = p;

                        if (tile.ZoomLevelIndex == currentZoomLevel)
                        {
                            DrawTile(tile, cell.BoundingBox);
                            DrawingCanvas.AddView(tile);
                        }
                    }
                }
            }
        }

        private void PanSingleTile(RectangleShape targetExtent)
        {
            if (PreviousExtent != null)
            {
                double resolution = MapArguments.CurrentResolution;
                double worldOffsetX = targetExtent.UpperLeftPoint.X - PreviousExtent.UpperLeftPoint.X;
                double worldOffsetY = targetExtent.UpperLeftPoint.Y - PreviousExtent.UpperLeftPoint.Y;
                double screenOffsetX = worldOffsetX / resolution;
                double screenOffsetY = worldOffsetY / resolution;

                /*_translateTransform.X -= screenOffsetX;
                _translateTransform.Y += screenOffsetY;*/
            }
        }

        private void RemoveCurrentGhostTile(RectangleShape tileTargetExtent)
        {
            Collection<Tile> ghostTiles = new Collection<Tile>();

            for(int i = 0; i < StretchCanvas.ChildCount; i++)
            {
                Tile ghostTile = (Tile)StretchCanvas.GetChildAt(i);
                ghostTiles.Add(ghostTile);
            }

            Tile selectedGhostTile = ghostTiles.FirstOrDefault<Tile>((tile) => { return tile.TargetExtent.Contains(tileTargetExtent); });
            if (selectedGhostTile != null)
            {
                bool removeGhostTile = true;

                for(int i = 0; i < DrawingCanvas.ChildCount; i++)
                {
                    Tile drawingTile = (Tile)DrawingCanvas.GetChildAt(i);
                    if (selectedGhostTile.TargetExtent.Contains(drawingTile.TargetExtent) && !drawingTile.IsOpened)
                    {
                        removeGhostTile = false;
                        break;
                    }
                }

                if (removeGhostTile)
                {
                    StretchCanvas.RemoveView(selectedGhostTile);
                }
            }
        }

        private void panningTimer_Tick(object sender, EventArgs e)
        {
            DrawMultipleTiles(_targetExtentForPanning, OverlayRefreshType.Pan);
            //_panningTimer.Stop();
        }

        private static string GetTileKey(int level, long row, long column)
        {
            return String.Format(CultureInfo.InvariantCulture, "{0},{1},{2}", level, row, column);
        }

        private static void ShiftStretchTile(Tile targetTile, RectangleShape targetExtent, double currentResolution)
        {
            double offsetX = (targetTile.TargetExtent.UpperLeftPoint.X - targetExtent.UpperLeftPoint.X) / currentResolution;
            double offsetY = (targetExtent.UpperLeftPoint.Y - targetTile.TargetExtent.UpperLeftPoint.Y) / currentResolution;

            FrameLayout.LayoutParams p = new FrameLayout.LayoutParams(targetTile.Width, targetTile.Height);
            p.LeftMargin = Convert.ToInt32(offsetX);
            p.TopMargin = Convert.ToInt32(offsetY);
            targetTile.LayoutParameters = p;
        }

        public void ClearTiles(ClearTilesMode clearTilesMode)
        {
            Action<ViewGroup> clearTiles = canvas =>
            {
                if (canvas != null)
                {
                    for(int i = 0; i < canvas.ChildCount; i++)
                    {
                        Tile tile = (Tile)canvas.GetChildAt(i);
                        DisposeTile(tile);
                    }

                    canvas.RemoveAllViews();
                }
            };

            switch (clearTilesMode)
            {
                case ClearTilesMode.StretchedTiles:
                    clearTiles(StretchCanvas);
                    break;
                case ClearTilesMode.AllTiles:
                    clearTiles(StretchCanvas);
                    clearTiles(DrawingCanvas);
                    break;
                case ClearTilesMode.Default:
                case ClearTilesMode.DrawingTiles:
                default:
                    clearTiles(DrawingCanvas);
                    break;
            }
        }

        private void DisposeTile(Tile tile)
        {
            tile.Opened -= newTile_Opened;
            tile.Drawing -= newTile_Drawing;
            tile.Drawn -= newTile_Drawn;
            tile.Dispose();
        }

        private static void DrawStretchTile(Tile tile, RectangleShape targetExtent, double currentResolution)
        {
            tile.HasWatermark = false;
            double newTileWidth = tile.TargetExtent.Width / currentResolution;
            double newTileHeight = tile.TargetExtent.Height / currentResolution;
            double offsetX = (tile.TargetExtent.UpperLeftPoint.X - targetExtent.UpperLeftPoint.X) / currentResolution;
            double offsetY = (targetExtent.UpperLeftPoint.Y - tile.TargetExtent.UpperLeftPoint.Y) / currentResolution;
            FrameLayout.LayoutParams p = new FrameLayout.LayoutParams(Convert.ToInt32(newTileWidth), Convert.ToInt32(newTileHeight));
            p.LeftMargin = Convert.ToInt32(offsetX);
            p.TopMargin = Convert.ToInt32(offsetY);
            p.Width = Convert.ToInt32(newTileWidth);
            p.Height = Convert.ToInt32(newTileHeight);

        }

        internal void SetMatrixTiles(Matrix matrix)
        {
            for(var i = 0; i < DrawingCanvas.ChildCount; i++)
            {
                Tile tile = (Tile)DrawingCanvas.GetChildAt(i);
                if(tile != null)
                    tile.ImageSource.ImageMatrix = matrix;
            }
        }
    }
}

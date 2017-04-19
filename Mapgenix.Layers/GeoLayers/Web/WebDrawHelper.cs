using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Globalization;
using System.Text;
using Mapgenix.Canvas;
using Mapgenix.Shapes;
using Mapgenix.RasterSource;

namespace Mapgenix.Layers
{
    public static class WebDrawHelper
	{
		
		public static void DrawException(BaseGeoCanvas canvas, Exception e)
		{
			int indexOfParameterName = e.Message.IndexOf("Parameter name", StringComparison.Ordinal);
			int origLength = e.Message.Length;

			if (indexOfParameterName != -1)
			{
				origLength = indexOfParameterName;
			}
			int splitNum = origLength / 50;
			if (origLength % 50 > 0)
			{
				splitNum += 1;
			}
			StringBuilder splits = new StringBuilder();
			for (int index = 0; index < splitNum; index++)
			{
				int startPosition = index * 50;
				int endPosition = startPosition + 50;
				if (endPosition > origLength)
				{
					endPosition = origLength;
				}

				string lineMessage = e.Message.Substring(startPosition, (endPosition - startPosition)) + Environment.NewLine;
				splits.AppendFormat(CultureInfo.InvariantCulture, lineMessage);
			}
			if (indexOfParameterName != -1)
			{
				splits.AppendFormat(CultureInfo.InvariantCulture, e.Message.Substring(indexOfParameterName));
			}
			GeoFont warterMarkFont = new GeoFont("Arial", 9);

			canvas.DrawTextWithScreenCoordinate(splits.ToString(), warterMarkFont, new GeoSolidBrush(GeoColor.FromArgb(255, 0, 0, 0)), canvas.Width / 2, canvas.Height / 2, DrawingLevel.LabelLevel);
			canvas.Flush();
		}

	


		public static void DrawImage(TiledWmsLayer layer, BaseGeoCanvas canvas)
		{
			Collection<GeoColor> tempKeyColors = new Collection<GeoColor>();
			if (canvas.HasKeyColor)
			{
				tempKeyColors = canvas.KeyColors;

				canvas.KeyColors.Clear();
				foreach (GeoColor oneKeyColor in layer.KeyColors)
				{
					canvas.KeyColors.Add(oneKeyColor);
				}
			}

			GeoImage tempImage = null;
			try
			{
				int canvasWidth = (int)canvas.Width;

				if (GeometryHelper.IsExtentWithinThreshold(canvas.CurrentWorldExtent, layer.UpperScale, layer.LowerScale, canvasWidth, canvas.MapUnit, canvas.Dpi))
				{
					PointShape centerPointShape;
					if (layer.ImageSource is TiledWmsRasterSource)
					{
						centerPointShape = canvas.CurrentWorldExtent.GetCenterPoint();
					}
					else
					{
						RectangleShape imageExtent = layer.GetBoundingBox();
						if (!imageExtent.Intersects(canvas.CurrentWorldExtent))
						{
							return;
						}
						RectangleShape overlapShape = RectangleShapeExtension.Intersection(imageExtent, canvas.CurrentWorldExtent);
						centerPointShape = overlapShape.GetCenterPoint();
					}
					tempImage = layer.ImageSource.GetMap(canvas);
					canvas.DrawWorldImageWithoutScaling(tempImage, centerPointShape.X, centerPointShape.Y, DrawingLevel.LevelOne);
				}
			}
			finally
			{
				if (tempImage != null) { tempImage.Close(); }
			}

			if (canvas.HasKeyColor)
			{
				canvas.KeyColors.Clear();
				foreach (GeoColor oneKeyColor in tempKeyColors)
				{
					canvas.KeyColors.Add(oneKeyColor);
				}
			}
		}

		

		public static Bitmap GetBitMapCoveringUncachingTiles(TiledWmsLayer layer, BaseGeoCanvas canvas, Collection<TileMatrixCell> notExistingCells, RectangleShape drawingExtent)
		{
			int drawingBitmapWidth = layer.TileCache.TileMatrix.TileWidth * (int)Math.Round(drawingExtent.Width / notExistingCells[0].BoundingBox.Width);
			int drawingBitmapHeight = layer.TileCache.TileMatrix.TileHeight * (int)Math.Round(drawingExtent.Height / notExistingCells[0].BoundingBox.Height);
			Bitmap drawingBitmap = new Bitmap(drawingBitmapWidth, drawingBitmapHeight);

			GdiPlusGeoCanvas drawingCanvas = new GdiPlusGeoCanvas();
			drawingCanvas.DrawingQuality = canvas.DrawingQuality;

			try
			{
				drawingCanvas.BeginDrawing(drawingBitmap, drawingExtent, canvas.MapUnit);
				WebDrawHelper.DrawImage(layer, drawingCanvas);
				drawingCanvas.Flush();
			}
			finally
			{
				drawingCanvas.EndDrawing();
			}

			PointShape drawingExtentCenterPoint = drawingExtent.GetCenterPoint();
			canvas.DrawWorldImageWithoutScaling(canvas.ToGeoImage(drawingBitmap), drawingExtentCenterPoint.X, drawingExtentCenterPoint.Y, DrawingLevel.LevelOne);
			return drawingBitmap;
		}


		
	

		public static void SetNotExistingAndExistingCells(TiledWmsLayer layer, BaseGeoCanvas canvas, double canvasScale, Collection<TileMatrixCell> notExistingCells, Collection<TileMatrixCell> existingCells)
		{
			if (layer.TileCache != null)
			{
				lock (layer.TileCache)
				{
					layer.TileCache.TileMatrix.Scale = canvasScale;
					Collection<TileMatrixCell> cells = layer.TileCache.TileMatrix.GetIntersectingCells(canvas.CurrentWorldExtent);

					foreach (TileMatrixCell cell in cells)
					{
						BitmapTile tile = layer.TileCache.GetTile(cell.BoundingBox);

						if (tile == null || tile.Bitmap == null)
						{
							notExistingCells.Add(cell);
						}
						else
						{
							existingCells.Add(cell);

							tile.Bitmap.Dispose();
							tile.Bitmap = null;
						}
					}
				}
			}
		}



		public static void DrawTiledWms(TiledWmsLayer layer, BaseGeoCanvas canvas)
		{
			if (canvas.MapUnit == GeographyUnit.Meter && layer.TileCache != null)
			{
				layer.TileCache.TileMatrix.BoundingBoxUnit = GeographyUnit.Meter;
				layer.TileCache.TileMatrix.BoundingBox = new RectangleShape(-20037508, 20037508.34, 20037508, -20037508);
			}
			if (layer.TileCache == null)
			{
                DrawImage(layer, canvas);
			}
			else
			{
				Collection<TileMatrixCell> notExistingCells = new Collection<TileMatrixCell>();
				Collection<TileMatrixCell> existingCells = new Collection<TileMatrixCell>();

				double canvasScale = ExtentHelper.GetScale(canvas.CurrentWorldExtent, canvas.Width, canvas.MapUnit);
                SetNotExistingAndExistingCells(layer, canvas, canvasScale, notExistingCells, existingCells);

				RectangleShape drawingExtent = null;
				if (notExistingCells.Count > 0)
				{
					drawingExtent = GeometryHelper.GetExpandToIncludeExtent(notExistingCells);
					Bitmap drawingBitmap = GetBitMapCoveringUncachingTiles(layer, canvas, notExistingCells, drawingExtent);

					double saveScale = ExtentHelper.GetScale(drawingExtent, drawingBitmap.Width, canvas.MapUnit);
					if (IsEqual(saveScale, canvasScale) && !layer.ImageSource.IsTimeOut && !layer.ImageSource.IsError && layer.TileCache != null)
					{
						layer.TileCache.TileMatrix.Scale = canvasScale;
						layer.TileCache.SaveTiles(drawingBitmap, drawingExtent);
					}
				}

				foreach (TileMatrixCell cell in existingCells)
				{
					if (drawingExtent == null || !GeometryHelper.Contains(drawingExtent, cell.BoundingBox))
					{
						BitmapTile tile = layer.TileCache.GetTile(cell.BoundingBox);
						if (tile != null && tile.Bitmap != null)
						{
							try
							{
								PointShape centerPoint = tile.BoundingBox.GetCenterPoint();
								canvas.DrawWorldImageWithoutScaling(canvas.ToGeoImage(tile.Bitmap), centerPoint.X, centerPoint.Y, DrawingLevel.LevelOne);
							}
							finally
							{
								tile.Bitmap.Dispose();
							}
						}
					}
				}
			}
		}

		internal static bool IsEqual(double firstValue, double secondValue)
		{
			bool result = false;

			double relativeSubtraction = Math.Abs(firstValue - secondValue);
			if ((relativeSubtraction / firstValue) < 10E-6)
			{
				result = true;
			}

			return result;
		}

	}
}

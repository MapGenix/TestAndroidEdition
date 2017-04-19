using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing.Imaging;
using System.IO;
using Mapgenix.Canvas;
using Mapgenix.Layers.Properties;
using Mapgenix.Shapes;
using Mapgenix.Styles;

namespace Mapgenix.Layers
{
    public static class DrawHelper
	{

		public static void Draw(HeatLayer self, BaseGeoCanvas canvas, Collection<SimpleCandidate> labelsInAllLayers)
		{
			Validators.CheckParameterIsNotNull(canvas, "canvas");

            RectangleShape currentWorldExtent;
			double currentScale = ExtentHelper.GetScale(canvas.CurrentWorldExtent, canvas.Width, canvas.MapUnit, canvas.Dpi);

			if (currentScale <= self.UpperScale && currentScale >= self.LowerScale)
			{
                if (canvas.MapUnit == GeographyUnit.DecimalDegree)
                {
                    RectangleShape worldExtent = new RectangleShape(-180, 90, 180, -90);
                    currentWorldExtent = worldExtent.GetIntersection(canvas.CurrentWorldExtent);
                }
                else
                {
                    currentWorldExtent = canvas.CurrentWorldExtent;
                }
                RectangleShape bufferedExtent = currentWorldExtent.Buffer(self.HeatStyle.PointRadius, canvas.MapUnit, self.HeatStyle.PointRadiusUnit).GetBoundingBox();
                
                Collection<Feature> features = self.FeatureSource.GetFeaturesInsideBoundingBox(bufferedExtent, self.HeatStyle.GetRequiredColumnNames());
				self.HeatStyle.Draw(features, canvas, new Collection<SimpleCandidate>(), new Collection<SimpleCandidate>());
			}
		}


		public static void DrawLayer(RestrictionLayer self, BaseGeoCanvas canvas, Collection<SimpleCandidate> labelsInAllLayers)
		{
			Validators.CheckParameterIsNotNull(canvas, "canvas");
			Validators.CheckParameterIsNotNull(labelsInAllLayers, "labeledInLayers");
			Validators.CheckGeoCanvasIsInDrawing(canvas.IsDrawing);
			Validators.CheckLayerIsOpened(self.IsOpen);
			Validators.DefaultAndCustomStyleDuplicateForRestrictedLayer(self);

			double currentScale = ExtentHelper.GetScale(canvas.CurrentWorldExtent, canvas.Width, canvas.MapUnit, canvas.Dpi);

			switch (self.RestrictionMode)
			{
				case RestrictionMode.ShowZones:
					if (currentScale > self.UpperScale || currentScale < self.LowerScale)
					{
						HideTheWholeMap(self, canvas);
					}
					else
					{
						ShowZones(self, canvas);
					}
					break;

				case RestrictionMode.HideZones:
					if (currentScale <= self.UpperScale && currentScale >= self.LowerScale)
					{
						HideZones(self, canvas);
					}
					break;

				default:
					break;
			}
		}

		static void ShowZones(RestrictionLayer self, BaseGeoCanvas canvas)
		{
			BaseAreaShape entireShape = null;

			foreach (BaseAreaShape zone in self.Zones)
			{
				if (zone.Contains(canvas.CurrentWorldExtent))
				{
					return;
				}

				if (entireShape == null)
				{
					entireShape = canvas.CurrentWorldExtent;
				}
				entireShape = entireShape.GetDifference(zone);
			}

			if (entireShape != null)
			{
				Feature feature = new Feature(entireShape);
				DrawImage(self, new[] { feature }, canvas);
			}
		}

		static void HideTheWholeMap(RestrictionLayer self, BaseGeoCanvas canvas)
		{
			DrawImage(self, new[] { new Feature(canvas.CurrentWorldExtent) }, canvas);
		}

		static void HideZones(RestrictionLayer self, BaseGeoCanvas canvas)
		{
			Collection<Feature> features = new Collection<Feature>();

			foreach (BaseAreaShape zone in self.Zones)
			{
				features.Add(new Feature(zone));
			}

			if (features.Count > 0)
			{
				DrawImage(self, features, canvas);
			}
		}

		public static BaseStyle GetDefaultStyle(RestrictionLayer self)
		{
			BaseStyle style = self.DefaultStyle;
			switch (self.RestrictionStyle)
			{
				case RestrictionStyle.HatchPattern:
					style = GetHatchPattern();
					break;

				case RestrictionStyle.CircleWithSlashImage:
					using(Stream stream = new MemoryStream())
					{
						Resources.NoData.Save(stream, ImageFormat.Png);
						style = GetCircleWithSlashImage(stream);
					}
					break;

				case RestrictionStyle.UseCustomStyles:
					style = null;
					break;

			}
			return style;

		} 

		static void DrawImage(RestrictionLayer self, IEnumerable<Feature> features, BaseGeoCanvas canvas)
		{
			BaseStyle defaultStyle = GetDefaultStyle(self);
				

			Collection<SimpleCandidate> labeledInLayer = new Collection<SimpleCandidate>();
			Collection<SimpleCandidate> labelsInAllLayers = new Collection<SimpleCandidate>();
			DrawingQuality tempDrawingQuality = canvas.DrawingQuality;
			canvas.DrawingQuality = DrawingQuality.HighSpeed;
			if (defaultStyle != null)
			{
				defaultStyle.Draw(features, canvas, labeledInLayer, labelsInAllLayers);
			}
			else
			{
				foreach (BaseStyle customStyle in self.CustomStyles)
				{
					customStyle.Draw(features, canvas, labeledInLayer, labelsInAllLayers);
				}
			}
			canvas.DrawingQuality = tempDrawingQuality;
			
			
		}

		static BaseStyle GetCircleWithSlashImage(Stream stream)
		{
			AreaStyle returnStyle = new AreaStyle();

			GeoImage image = new GeoImage(stream);
			GeoTextureBrush brush = new GeoTextureBrush(image);

            returnStyle.AreaStyleCustom.FillCustomBrush = brush;

			return returnStyle;
		}

		static BaseStyle GetHatchPattern()
		{
			return AreaStyles.CreateHatchStyle(GeoHatchStyle.Percent80, GeoColor.StandardColors.LightGray, GeoColor.StandardColors.Black, GeoColor.StandardColors.Transparent, 0, LineDashStyle.Solid, 0, 0);
		}
	}
}

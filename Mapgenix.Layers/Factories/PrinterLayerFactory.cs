using System.Collections.Generic;
using System.Data;
using Mapgenix.Canvas;

namespace Mapgenix.Layers
{
    /// <summary>
    /// Factory class for printer layers.
    /// </summary>
    public static class PrinterLayerFactory
	{

		public static CompassPrinterLayer CreateCompassPrinterLayer(GeoImage needleImage, GeoImage frameImage)
        {
			return new CompassPrinterLayer
			{
				NeedleImage = needleImage,
				FrameImage = frameImage,
	            ResizeMode = PrinterResizeMode.MaintainAspectRatio,
				BackgroundMask = new AreaStyle(),
				DragMode = PrinterDragMode.Dragable,
				HasBoundingBox = true
			};
            
        }

		public static DataGridPrinterLayer CreateDataGridPrinterLayer(DataTable dataTable, GeoFont textFont)
        {
			return new DataGridPrinterLayer
			{
				DataTable = dataTable,
				FontBrush = new GeoSolidBrush(GeoColor.StandardColors.Black),
				CellBorderPen = new GeoPen(new GeoSolidBrush(GeoColor.StandardColors.Black)),
				TextFont = textFont,
				CellTextJustification = LabelTextJustification.Left,

				BackgroundMask = new AreaStyle(),
				ResizeMode = PrinterResizeMode.Resizable,
				DragMode = PrinterDragMode.Dragable,
				HasBoundingBox = true
			};
            
        }

		public static LabelPrinterLayer CreateLabelPrinterLayer()
		{
			return CreateLabelPrinterLayer("", new GeoFont("Arial", 10), new GeoSolidBrush(GeoColor.StandardColors.Black));
		}

        public static LabelPrinterLayer CreateLabelPrinterLayer(string text, GeoFont font, BaseGeoBrush fontBrush)
        {
			return new LabelPrinterLayer
			{
				Text = text,
				Font = font,
				FontBrush = fontBrush,

				BackgroundMask = new AreaStyle(),
				ResizeMode = PrinterResizeMode.Resizable,
				DragMode = PrinterDragMode.Dragable,
				HasBoundingBox = true
			};
            
        }

		public static MapPrinterLayer CreateMapPrinterLayer()
		{

			return new MapPrinterLayer
			{
				BackgroundMask = new AreaStyle(),
				ResizeMode = PrinterResizeMode.Resizable,
				DragMode = PrinterDragMode.Dragable,
				HasBoundingBox = true
			};

    	}

		public static PagePrinterLayer CreatePagePrinterLayer()
        {
			return CreatePagePrinterLayer(PrinterPageSize.AnsiA, PrinterOrientation.Portrait);
		}

        public static PagePrinterLayer CreatePagePrinterLayer(PrinterPageSize pageSize, PrinterOrientation orientation)
        {
			return new PagePrinterLayer
			{
                BackgroundMask = new AreaStyle(new GeoPen(GeoColor.StandardColors.Black, 2), new GeoSolidBrush(GeoColor.StandardColors.WhiteSmoke)),
				PageSize = pageSize,
	            Orientation = orientation,
				CustomWidth = 0,
				CustomHeight = 0,

				ResizeMode = PrinterResizeMode.Fixed,
				DragMode = PrinterDragMode.Fixed,

				HasBoundingBox = true

			};
            
        }

		public static ScaleBarPrinterLayer CreateScaleBarPrinterLayer(MapPrinterLayer mapPrinterLayer)
        {
			Dictionary<string, string> displayUnits = new Dictionary<string,string>(); 
			displayUnits.Add("Meter", "Meters");
			displayUnits.Add("Feet", "Feet");
			displayUnits.Add("Kilometer", "KM");
			displayUnits.Add("Mile", "Miles");


			return new ScaleBarPrinterLayer
			{
				TextColor = GeoColor.StandardColors.Black,
				BarBrush = new GeoSolidBrush(GeoColor.StandardColors.Black),
				MaskBrush = new GeoSolidBrush(GeoColor.StandardColors.White),
				MaskContour = new GeoPen(GeoColor.StandardColors.Black),
				Font = new GeoFont("Microsoft Sans Serif", 7F, DrawingFontStyles.Bold),
				Thickness = 8,

				MaxWidth = 392,
				Width = 392,

				MapPrinterLayer = mapPrinterLayer,
				DisplayUnitString = displayUnits,
				
				BackgroundMask = new AreaStyle(),
				ResizeMode = PrinterResizeMode.Resizable,
				DragMode = PrinterDragMode.Dragable,
				HasBoundingBox = true
			};
        }

		public static ScaleLinePrinterLayer CreateScaleLinePrinterLayer(MapPrinterLayer mapPrinterLayer)
        {
			Dictionary<string, string> displayUnits = new Dictionary<string, string>();
			displayUnits.Add("Meter", "m");
			displayUnits.Add("Feet", "ft");
			displayUnits.Add("Kilometer", "km");
			displayUnits.Add("Mile", "mi");
			displayUnits.Add("UsSurveyFeet", "usf");
			displayUnits.Add("Yard", "f");


			return new ScaleLinePrinterLayer
			{
				ScaleLineHeightPixel = 14,
				ScaleLineWidthPixel = 150,
            
				Font = new GeoFont("Arial", 6.5f),
                Brush = new GeoSolidBrush(GeoColor.StandardColors.Black),
                Pen = new GeoPen(GeoColor.StandardColors.Black, 2f),
				BackPen = new GeoPen(GeoColor.StandardColors.White, 4f),

				MapPrinterLayer = mapPrinterLayer,
				DisplayUnits = displayUnits,
				BackgroundMask = new AreaStyle(),
				ResizeMode = PrinterResizeMode.Resizable,
				DragMode = PrinterDragMode.Dragable,
				HasBoundingBox = true

			};
        }
	}
}

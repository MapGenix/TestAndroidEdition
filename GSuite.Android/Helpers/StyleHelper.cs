using Mapgenix.Canvas;
using System;
using System.Windows;
using System.Windows.Media;

namespace Mapgenix.GSuite.Wpf
{
    internal static class StyleHelper
    {
        public static Brush GetWpfBrushFromGeoBrush(BaseGeoBrush geoBrush)
        {
            
            Brush brush = null;

            if (geoBrush != null)
            {
                string brushName = geoBrush.GetType().Name;
                switch (brushName)
                {
                    case "GeoSolidBrush":
                        SolidColorBrush wpfSolidbrush = new SolidColorBrush();
                        GeoSolidBrush solidGeoBrush = geoBrush as GeoSolidBrush;
                        wpfSolidbrush.Color = Color.FromArgb(solidGeoBrush.Color.AlphaComponent, solidGeoBrush.Color.RedComponent, solidGeoBrush.Color.GreenComponent, solidGeoBrush.Color.BlueComponent);
                        brush = wpfSolidbrush;
                        break;
                    default:
                        break;
                }
            }

            return brush;
        }


        public static PenLineJoin GetLineJoinFromGeoLineJoin(DrawingLineJoin drawingLineJoin)
        {
            PenLineJoin penLineJoin = PenLineJoin.Round;
            switch (drawingLineJoin)
            {
                case DrawingLineJoin.Bevel:
                    penLineJoin = PenLineJoin.Bevel;
                    break;
                case DrawingLineJoin.Miter:
                    penLineJoin = PenLineJoin.Miter;
                    break;
                case DrawingLineJoin.Round:
                    penLineJoin = PenLineJoin.Round;
                    break;
                default:
                    break;
            }
            return penLineJoin;
        }

        public static Pen GetWpfPenFromGeoPen(GeoPen geoPen)
        {
            Pen pen = null;

            if (geoPen != null)
            {
                pen = new Pen();
                pen.Brush = GetWpfBrushFromGeoBrush(geoPen.Brush);
                pen.DashCap = GetPenLineCapFromGeoDashCap(geoPen.DashCap);
                pen.Thickness = geoPen.Width;
                pen.DashStyle = GetDashStyleFromGeoDashStyle(geoPen.DashStyle);


                pen.EndLineCap = GetPenLineCapFromDrawingLineCap(geoPen.EndCap);
                pen.LineJoin = GetLineJoinFromGeoLineJoin(geoPen.LineJoin);
                pen.MiterLimit = geoPen.MiterLimit;
                pen.StartLineCap = GetPenLineCapFromDrawingLineCap(geoPen.StartCap);

                if (pen.DashStyle != null)
                {
                    foreach (float dash in geoPen.DashPattern)
                    {
                        pen.DashStyle.Dashes.Add((double)dash);
                    }
                }
            }

            return pen;
        }

        private static DashStyle GetDashStyleFromGeoDashStyle(LineDashStyle geoDashStyle)
        {
            DashStyle dashStyle = DashStyles.Solid;
            switch (geoDashStyle)
            {
                case LineDashStyle.Dash:
                    dashStyle = DashStyles.Dash;
                    break;
                case LineDashStyle.DashDot:
                    dashStyle = DashStyles.DashDot;
                    break;
                case LineDashStyle.DashDotDot:
                    dashStyle = DashStyles.DashDotDot;
                    break;
                case LineDashStyle.Dot:
                    dashStyle = DashStyles.Dot;
                    break;
                default: break;
            }

            return dashStyle;
        }

        public static PenLineCap GetPenLineCapFromGeoDashCap(GeoDashCap dashCap)
        {
            PenLineCap penLineCap = PenLineCap.Round;
            switch (dashCap)
            {
                case GeoDashCap.Round:
                    penLineCap = PenLineCap.Round;
                    break;
                case GeoDashCap.Flat:
                    penLineCap = PenLineCap.Flat;
                    break;
                case GeoDashCap.Triangle:
                    penLineCap = PenLineCap.Triangle;
                    break;
                default:
                    break;
            }
            return penLineCap;
        }

        private static PenLineCap GetPenLineCapFromDrawingLineCap(DrawingLineCap lineCap)
        {
            PenLineCap result = PenLineCap.Flat;
            switch (lineCap)
            {
                case DrawingLineCap.Flat:
                    result = PenLineCap.Flat;
                    break;
                case DrawingLineCap.Round:
                    result = PenLineCap.Round;
                    break;
                case DrawingLineCap.Square:
                    result = PenLineCap.Square;
                    break;
                case DrawingLineCap.Triangle:
                    result = PenLineCap.Triangle;
                    break;
                default: throw new NotSupportedException();
            }

            return result;
        }

        internal static Typeface GetTypeface(GeoFont font)
        {
            FontStyle fontStyle = FontStyles.Normal;
            if (font.IsItalic) { fontStyle = FontStyles.Italic; }

            FontWeight fontWeight = FontWeights.Normal;
            if (font.IsBold) { fontWeight = FontWeights.Bold; }

            Typeface typeface = new Typeface(new FontFamily(font.FontName), fontStyle, fontWeight, FontStretches.Normal);
            return typeface;
        }

    }
}

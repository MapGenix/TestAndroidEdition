using System;
using System.Collections.ObjectModel;
using Mapgenix.Canvas;


namespace Mapgenix.Styles
{
    /// <summary>
    /// Factory for creating different types of LineStyles.
    /// </summary>
    [Serializable]
    public static class LineStyles
    {
        /// <summary>Returns a LineStyle.</summary>
       /// <returns>LineStyle.</returns>
        /// <remarks>None</remarks>
        /// <overloads>Simple line with an optional round cap.</overloads>
        public static LineStyle CreateSimpleLineStyle(GeoColor centerlineColor, float centerlineWidth, bool roundCap)
        {
            return CreateSimpleLineStyle(centerlineColor, centerlineWidth, LineDashStyle.Solid, GeoColor.StandardColors.Transparent, 1, LineDashStyle.Solid, GeoColor.StandardColors.Transparent, 1, LineDashStyle.Solid, roundCap);
        }

        /// <summary>Returns a LineStyle.</summary>
       /// <returns>LineStyle.</returns>
        /// <remarks>None</remarks>
        /// <overloads>Dashed line with an optional round cap.</overloads>
        /// <param name="centerlineColor">Center line color.</param>
        /// <param name="centerlineWidth">Center line width.</param>
        /// <param name="centerlineDashStyle">Dash style for the center line.</param>
        /// <param name="roundCap">Optional rounded end cap.</param>
        public static LineStyle CreateSimpleLineStyle(GeoColor centerlineColor, float centerlineWidth, LineDashStyle centerlineDashStyle, bool roundCap)
        {
            return CreateSimpleLineStyle(centerlineColor, centerlineWidth, centerlineDashStyle, GeoColor.StandardColors.Transparent, 1, LineDashStyle.Solid, GeoColor.StandardColors.Transparent, 1, LineDashStyle.Solid, roundCap);
        }

        /// <summary>Returns a LineStyle.</summary>
        /// <returns>LineStyle.</returns>
        /// <remarks>None</remarks>
        /// <overloads>Line with an inner and outer area and optional round cap.</overloads>
        /// <param name="innerLineColor">Inner line color.</param>
        /// <param name="innerLineWidth">Inner line width.</param>
        /// <param name="outerLineColor">Outer line color.</param>
        /// <param name="outerLineWidth">Outer line width.</param>
        /// <param name="roundCap">Rounded end cap.</param>
        public static LineStyle CreateSimpleLineStyle(GeoColor innerLineColor, float innerLineWidth, GeoColor outerLineColor, float outerLineWidth, bool roundCap)
        {
            return CreateSimpleLineStyle(GeoColor.StandardColors.Transparent, 1, LineDashStyle.Solid, innerLineColor, innerLineWidth, LineDashStyle.Solid, outerLineColor, outerLineWidth, LineDashStyle.Solid, roundCap);
        }

        /// <summary>Returns a LineStyle.</summary>
       /// <returns>LineStyle.</returns>
        /// <remarks>None</remarks>
        /// <param name="innerLineColor">Inner line color.</param>
        /// <param name="innerLineWidth">Inner line width.</param>
        /// <param name="innerLineDashStyle">Inner line dash style.</param>
        /// <param name="outerLineColor">Outer line color.</param>
        /// <param name="outerLineWidth">Outer line width.</param>
        /// <param name="outerLineDashStyle">Outer line dash style.</param>
        /// <param name="roundCap">Optional rounded end cap.</param>
        public static LineStyle CreateSimpleLineStyle(GeoColor innerLineColor, float innerLineWidth, LineDashStyle innerLineDashStyle, GeoColor outerLineColor, float outerLineWidth, LineDashStyle outerLineDashStyle, bool roundCap)
        {
            return CreateSimpleLineStyle(GeoColor.StandardColors.Transparent, 1, LineDashStyle.Solid, innerLineColor, innerLineWidth, innerLineDashStyle, outerLineColor, outerLineWidth, outerLineDashStyle, roundCap);
        }

        /// <summary>Returns a LineStyle.</summary>
       /// <returns>LineStyle.</returns>
        /// <remarks>None</remarks>
        /// <overloads>Line with a solid centerline.</overloads>
        /// <param name="centerlineColor">Center line color.</param>
        /// <param name="centerlineWidth">Center line width.</param>
        /// <param name="innerLineColor">Inner line color.</param>
        /// <param name="innerLineWidth">Inner line width.</param>
        /// <param name="outerLineColor">Outer line color.</param>
        /// <param name="outerLineWidth">Outer line width.</param>
        /// <param name="roundCap">Optional rounded end cap.</param>
        public static LineStyle CreateSimpleLineStyle(GeoColor centerlineColor, float centerlineWidth, GeoColor innerLineColor, float innerLineWidth, GeoColor outerLineColor, float outerLineWidth, bool roundCap)
        {
            return CreateSimpleLineStyle(centerlineColor, centerlineWidth, LineDashStyle.Solid, innerLineColor, innerLineWidth, LineDashStyle.Solid, outerLineColor, outerLineWidth, LineDashStyle.Solid, roundCap);
        }

        /// <summary>Returns a LineStyle.</summary>
       /// <returns>LineStyle.</returns>
        /// <remarks>None</remarks>
        /// <overloads>Line with a dashed centerline.</overloads>
        /// <param name="centerlineColor">Center line color.</param>
        /// <param name="centerlineWidth">Center line width.</param>
        /// <param name="centerlineDashStyle">Center line dash style.</param>
        /// <param name="innerLineColor">Inner line color.</param>
        /// <param name="innerLineWidth">Inner line width.</param>
        /// <param name="innerLineDashStyle">Inner line dash style.</param>
        /// <param name="outerLineColor">Outer line color.</param>
        /// <param name="outerLineWidth">Outer line width.</param>
        /// <param name="outerLineDashStyle">Outer line dash style.</param>
        /// <param name="roundCap">Optional rounded end cap.</param>
        public static LineStyle CreateSimpleLineStyle(GeoColor centerlineColor, float centerlineWidth, LineDashStyle centerlineDashStyle, GeoColor innerLineColor, float innerLineWidth, LineDashStyle innerLineDashStyle, GeoColor outerLineColor, float outerLineWidth, LineDashStyle outerLineDashStyle, bool roundCap)
        {
            GeoPen centerPen = new GeoPen(centerlineColor, centerlineWidth);
            centerPen.DashStyle = centerlineDashStyle;
            GeoPen innerPen = new GeoPen(innerLineColor, innerLineWidth);
            innerPen.DashStyle = innerLineDashStyle;
            GeoPen outerPen = new GeoPen(outerLineColor, outerLineWidth);
            outerPen.DashStyle = outerLineDashStyle;

            if (roundCap)
            {
                centerPen.StartCap = DrawingLineCap.Round;
                centerPen.EndCap = DrawingLineCap.Round;
                innerPen.StartCap = DrawingLineCap.Round;
                innerPen.EndCap = DrawingLineCap.Round;
                outerPen.StartCap = DrawingLineCap.Round;
                outerPen.EndCap = DrawingLineCap.Round;
            }

            return new LineStyle(outerPen, innerPen, centerPen);
        }

        private static LineStyle CreateRailWayStyle(GeoColor innerPenColor, float innerPenWidth, GeoColor outerPenColor, float outerPenWidth, GeoColor centerPenColor, float centerPenWidth, Collection<float> dashPattern)
        {
            LineStyle lineStyle = new LineStyle();
            lineStyle.InnerPen = new GeoPen(innerPenColor, innerPenWidth);
            lineStyle.OuterPen = new GeoPen(outerPenColor, outerPenWidth);

            GeoPen centerPen = new GeoPen(centerPenColor, centerPenWidth);
            centerPen.DashPattern.Add(dashPattern[0]);
            centerPen.DashPattern.Add(dashPattern[1]);
            lineStyle.CenterPen = centerPen;

            return lineStyle;
        }
    }
}
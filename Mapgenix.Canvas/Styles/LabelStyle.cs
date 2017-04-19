using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Mapgenix.Shapes;

namespace Mapgenix.Canvas
{
    /// <summary>Class used to label features on the map.</summary>
    /// <remarks>Used for labeling feature based layers on the map. Many properties are available to control the appearance and behavior
    /// of the label on the map.</remarks>
    [Serializable]
    public class LabelStyle : BaseLabelStyle
    {
        private const double DefaultFittingPolygonFactor = 1;
        private const int DefaultGridSize = 100;
        private const double DefaultTextLineSegmentRatio = 0.9;

        /// <summary>Constructor of the class.</summary>
        /// <overloads>
        /// This is the default constructor for the class. If you use this constructor, you
        /// should set the required properties manually.</overloads>
        /// <returns>None</returns>
        public LabelStyle()
            : this(string.Empty, new GeoFont(), new GeoSolidBrush())
        {
        }

        /// <summary>Constructor of the class.</summary>
        /// <returns>None</returns>
        /// <overloads>To pass in the minimum required fields for labeling. Explore other properties for more control on labeling.</overloads>
        /// <param name="textColumnName">Name of the column containing the text to label.</param>
        /// <param name="textFont">Font of the label.</param>
        /// <param name="textSolidBrush">GeoSolidBrush of the label.</param>
        public LabelStyle(string textColumnName, GeoFont textFont, GeoSolidBrush textSolidBrush)
        {
            TextColumnName = textColumnName;
            Font = textFont;
            TextSolidBrush = textSolidBrush;
            GridSize = DefaultGridSize;
            TextLineSegmentRatio = DefaultTextLineSegmentRatio;
            FittingPolygonFactor = DefaultFittingPolygonFactor;
        }

        /// <summary>Gets and sets the mode determining a polygon labeling location.</summary>
        /// <value>Mode determining a polygon labeling location.</value>
        /// <remarks>The two ways to establish a polygon's labeling location area:
        /// 1) Polygon's centroid 2) Polygon's boungdingbox center.</remarks>
        public new PolygonLabelingLocationMode PolygonLabelingLocationMode
        {
            get { return base.PolygonLabelingLocationMode; }
            set { base.PolygonLabelingLocationMode = value; }
        }

        /// <summary>Gets and sets the X offset in pixel for drawing labels.</summary>
        /// <value>X pixel offset in pixel for drawing each labels.</value>
        /// <remarks>Useful to adjust the position of a label, for example, in relation to the point it represents.</remarks>
        public new float XOffsetInPixel
        {
            get { return base.XOffsetInPixel; }
            set { base.XOffsetInPixel = value; }
        }

        /// <summary>Gets and sets the Y offset in pixel for drawing labels.</summary>
        /// <value>Y pixel offset in pixel for drawing each labels.</value>
        /// <remarks>Useful to adjust the position of a label, for example, in relation to the point it represents.</remarks>
        public new float YOffsetInPixel
        {
            get { return base.YOffsetInPixel; }
            set { base.YOffsetInPixel = value; }
        }

        /// <summary>Gets and sets the grid size used for deterministic labeling.</summary>
        /// <value>Grid size for deterministic labeling.</value>
        /// <remarks>Grid size determines how many labels will be considered as candidates for drawing by grid. The
        /// smaller the grid size, the higher the density of candidates. The smaller the grid, 
        /// the more the drawing speed performance is negatively impacted.</remarks>
        public new int GridSize
        {
            get { return base.GridSize; }
            set
            {
                base.GridSize = value;
                Validators.CheckValueIsBiggerThanZero(base.GridSize, "gridSize");
            }
        }

        ///<summary>Gets and sets whether the labeler intends to fit the label on the visible part of a line on the screen.</summary>
        ///<value>Whether the labeler intends to fit the label on the visible part of the line or not.</value>
        public new bool FittingLineInScreen
        {
            get { return base.FittingLineInScreen; }
            set { base.FittingLineInScreen = value; }
        }

        /// <summary>Returns a collection of label styles to stack multiple label styles on top of each other.</summary>
        /// <value>collection of label styles.</value>
        public new Collection<LabelStyle> CustomTextStyles
        {
            get { return base.CustomTextStyles; }
        }

        /// <summary>Gets and sets the rotation angle of the label being positioned.</summary>
        /// <value>Rotation angle of the item being positioned.</value>
        /// <remarks>None</remarks>
        public new double RotationAngle
        {
            get { return base.RotationAngle; }
            set { base.RotationAngle = value; }
        }

        /// <summary>Gets and sets the format for the text of the label.</summary>
        /// <value>Format for the text of the label.</value>
        public new string TextFormat
        {
            get { return base.TextFormat; }
            set { base.TextFormat = value; }
        }

        /// <summary>Gets and sets the numeric format for the text of the label.</summary>
        /// <value>Numeric format for the text of the label.</value>
        public new string NumericFormat
        {
            get { return base.NumericFormat; }
            set { base.NumericFormat = value; }
        }

        /// <summary>Gets and sets the date format for the text of the label.</summary>
        /// <value>Date format for the text of the label.</value>
        public new string DateFormat
        {
            get { return base.DateFormat; }
            set { base.DateFormat = value; }
        }

        /// <summary>Gets and sets the SolidBrush to draw the text of the label.</summary>
        /// <value>This property gets the SolidBrush that will be used to draw the text.</value>
        /// <remarks>To draw a solid color. For other brushes, use Advanced property.</remarks>
        public new GeoSolidBrush TextSolidBrush
        {
            get { return base.TextSolidBrush; }
            set { base.TextSolidBrush = value; }
        }

        /// <summary>Gets and sets the font to draw the text of the label.</summary>
        /// <value>Font to draw the text of the label.</value>
        /// <remarks>None</remarks>
        public new GeoFont Font
        {
            get { return base.Font; }
            set { base.Font = value; }
        }

        /// <summary>Gets and sets a pen to draw a halo around the text of the label.</summary>
        /// <value>Pen to draw a halo around the text of the label</value>
        /// <remarks>Halo effect makes the text stand out.</remarks>
        public new GeoPen HaloPen
        {
            get { return base.HaloPen; }
            set { base.HaloPen = value; }
        }

        /// <summary>Gets and sets the column name in the data to get the text from.</summary>
        /// <value>Column name in the data to get the text from.</value>
        /// <remarks>To retrieve text from a feature specifying the name of the column that contains the text to draw.</remarks>
        public new string TextColumnName
        {
            get { return base.TextColumnName; }
            set { base.TextColumnName = value; }
        }

        /// <summary>Gets and sets the AreaStyle to draw a mask behind the text.</summary>
        /// <value>AreaStyle to draw a mask behind the text.</value>
        /// <remarks>Halo and mask are similar in their effect of making highlight the text.</remarks>
        public new AreaStyle Mask
        {
            get { return base.Mask; }
            set { base.Mask = value; }
        }

        /// <summary>Gets and sets the margin around the text for the mask.</summary>
        /// <value>Margin around the text for the mask.</value>
        /// <remarks>Determines how much larger the mask is than the text.</remarks>
        public new int MaskMargin
        {
            get { return base.MaskMargin; }
            set { base.MaskMargin = value; }
        }

        /// <summary>Gets the custom properties of LabelStyle.</summary>
        /// <value>Gets he custom properties of LabelStyle.</value>
        public new LabelStyleCustom Advanced
        {
            get { return base.Advanced; }
        }

        /// <summary>Gets and sets the rule determining how duplication of labels is handled.</summary>
        /// <value>Rule determining how duplication of labels is handled.</value>
        public new LabelDuplicateRule DuplicateRule
        {
            get { return base.DuplicateRule; }
            set { base.DuplicateRule = value; }
        }

        /// <summary>Gets and sets the rule determining how overlapping labels are handled.</summary>
        /// <value>Rule determining how overlapping labels are handled.</value>
        public new LabelOverlappingRule OverlappingRule
        {
            get { return base.OverlappingRule; }
            set { base.OverlappingRule = value; }
        }

        /// <summary>Gets and sets whether the labeler allows carriage returns.</summary>
        /// <value>Whether the labeler allows carriage returns.</value>
        /// <remarks>Allows the labeler to split long labels into multiple lines.</remarks>
        public new bool AllowLineCarriage
        {
            get { return base.AllowLineCarriage; }
            set { base.AllowLineCarriage = value; }
        }

        /// <summary>Gets and sets whether the labeler  forces carriage returns.</summary>
        /// <value>Whether the labeler forces carriage returns.</value>
        public new bool ForceLineCarriage
        {
            get { return base.ForceLineCarriage; }
            set { base.ForceLineCarriage = value; }
        }

        /// <summary>Gets and sets whether the labeler intends to fit the label within the boundary of polygon.</summary>
        /// <value>Whether the labeler intends to fit the label within the boundary of polygon.</value>
        /// <remarks>None</remarks>
        public new bool FittingPolygon
        {
            get { return base.FittingPolygon; }
            set { base.FittingPolygon = value; }
        }

        /// <summary>Gets and sets whether the labeler labels every part of a multi-part polygon.</summary>
        /// <value>Whether the labeler labels every part of a multi-part polygon.</value>
        public new bool LabelAllPolygonParts
        {
            get { return base.LabelAllPolygonParts; }
            set { base.LabelAllPolygonParts = value; }
        }

        /// <summary>Gets and sets whether the labeler lebels every part of a multi-part line.</summary>
        /// <value>Whether the labeler labels every part of a multi-part line.</value>
        public new bool LabelAllLineParts
        {
            get { return base.LabelAllLineParts; }
            set { base.LabelAllLineParts = value; }
        }

        /// <summary>Gets and sets whether labeling for lines is horizontal.</summary>
        /// <value>Whether labeling for lines is horizontal.</value>
        /// <remarks>Normally lines are labeled in the direction of the line.</remarks>
        public new bool ForceHorizontalLabelForLine
        {
            get { return base.ForceHorizontalLabelForLine; }
            set { base.ForceHorizontalLabelForLine = value; }
        }

        /// <summary>Gets and sets the factor keeping the label inside of the polygon.</summary>
        /// <value>Factor keeping the label inside of the polygon.</value>
        /// <remarks>None</remarks>
        public new double FittingPolygonFactor
        {
            get { return base.FittingPolygonFactor; }
            set { base.FittingPolygonFactor = value; }
        }

        /// <summary>Gets and sets whether a partial label in the current extent is drawn or not.</summary>
        /// <remarks>Provides a solution to the "cut off" label issue when multiple tiles exist.</remarks>
        public new bool SuppressPartialLabels
        {
            get { return base.SuppressPartialLabels; }
            set { base.SuppressPartialLabels = value; }
        }

        /// <summary>Gets and sets the ratio label length / line length.</summary>
        /// <value>Ratio label length / line length.</value>
        /// <remarks>Allows to suppress labels to avoid label length exceeding greatly the line length.</remarks>
        public new double TextLineSegmentRatio
        {
            get { return base.TextLineSegmentRatio; }
            set { base.TextLineSegmentRatio = value; }
        }

        /// <summary>Gets and sets whether the labeler changes the label position to avoid overlapping for point-based features.</summary>
        /// <value>Whether the labeler changes the label position to avoid overlapping for point-based features.</value>
        public new bool BestPlacement
        {
            get { return base.BestPlacement; }
            set { base.BestPlacement = value; }
        }

        /// <summary>Gets and sets the location of the label for point features in relation to the point.</summary>
        /// <value>Location of the label for point features in relation to the point.</value>
        public new PointPlacement PointPlacement
        {
            get { return base.PointPlacement; }
            set { base.PointPlacement = value; }
        }

        /// <summary>Gets or sets the SplineType for labeling.</summary>
        public new SplineType SplineType
        {
            get { return base.SplineType; }
            set { base.SplineType = value; }
        }

        /// <summary>Gets or sets the DrawingLavel for the label.</summary>
        public new DrawingLevel DrawingLevel
        {
            get { return base.DrawingLevel; }
            set { base.DrawingLevel = value; }
        }

        /// <summary>Returns a value representing a keyValuepair (feature id and label position of a features).</summary>
        public new Dictionary<string, WorldLabelingCandidate> LabelPositions
        {
            get { return base.LabelPositions; }
        }



        /// <summary>Draws a sample feature on the canvas passed in.</summary>
        /// Can be used to display a legend or other sample area.</remarks>
        /// <returns>None</returns>
        /// <param name="canvas">Canvas you want to draw the features on.</param>
        /// <param name="drawingExtent">Extent of the drawing.</param>
        protected override void DrawSampleCore(BaseGeoCanvas canvas, DrawingRectangleF drawingExtent)
        {
            Validators.CheckParameterIsNotNull(canvas, "canvas");

            if (Mask != null)
            {
                var rectangle = canvas.MeasureText(TextColumnName, Font);

                if (RotationAngle == 0)
                {
                    Mask.DrawSample(canvas,
                        new DrawingRectangleF(drawingExtent.CenterX, drawingExtent.CenterY,
                            rectangle.Width + MaskMargin*2, rectangle.Height + MaskMargin*2));
                }
                else
                {
                    var outlineWidth = Mask.OutlinePen == null ? 0 : Mask.OutlinePen.Width;
                    var maskWidth = rectangle.Width + outlineWidth*2 + MaskMargin*2;
                    var maskHeight = rectangle.Height + outlineWidth*2 + MaskMargin*2;
                    rectangle = new DrawingRectangleF(maskWidth*.5f, maskHeight*.5f, maskWidth, maskHeight);

                    var resolution = Math.Max(canvas.CurrentWorldExtent.Width/canvas.Width,
                        canvas.CurrentWorldExtent.Height/canvas.Height);
                    var left = canvas.CurrentWorldExtent.UpperLeftPoint.X +
                               (rectangle.CenterX - rectangle.Width*.5)*resolution;
                    var top = canvas.CurrentWorldExtent.UpperLeftPoint.Y -
                              (rectangle.CenterY - rectangle.Height*.5)*resolution;
                    var right = left + rectangle.Width*resolution;
                    var bottom = top - rectangle.Height*resolution;

                    var nativeImage = new Bitmap((int) rectangle.Width + 2, (int) rectangle.Height + 2);
                    var streamSource = new MemoryStream();

                    try
                    {
                        var tmpGeoCanvas = new GdiPlusGeoCanvas();
                        tmpGeoCanvas.BeginDrawing(nativeImage, new RectangleShape(left, top, right, bottom),
                            canvas.MapUnit);
                        Mask.DrawSample(tmpGeoCanvas,
                            new DrawingRectangleF(rectangle.CenterX, rectangle.CenterY
                                , rectangle.Width + 1, rectangle.Height + 1));

                        tmpGeoCanvas.EndDrawing();
                        nativeImage.Save(streamSource, ImageFormat.Png);
                        canvas.DrawScreenImageWithoutScaling(new GeoImage(streamSource), drawingExtent.CenterX,
                            drawingExtent.CenterY, DrawingLevel.LevelOne, 0, 0, (float) RotationAngle);
                    }
                    finally
                    {
                        if (nativeImage != null) nativeImage.Dispose();
                        if (streamSource != null) streamSource.Dispose();
                    }
                }
            }

            canvas.DrawText(TextColumnName, Font, TextSolidBrush, HaloPen,
                new[] {new ScreenPointF(drawingExtent.CenterX, drawingExtent.CenterY)}, DrawingLevel, 0, 0,
                (float) RotationAngle);
        }
    }
}
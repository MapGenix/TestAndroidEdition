using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Mapgenix.Shapes;


namespace Mapgenix.Canvas
{
    /// <summary>Style used to draw line based features.</summary>
    /// <remarks>Set an inner,outer and center pen for line features such as roads. The outer pen draws first, inner second and center last.</remarks>
    [Serializable]
    public class LineStyle : BaseStyle
    {
        private readonly Collection<LineStyle> _customLineStyles;
        private GeoPen _centerPen;
        private DrawingLevel _centerPenDrawingLevel;
        private GeoPen _innerPen;
        private DrawingLevel _innerPenDrawingLevel;
        private GeoPen _outerPen;
        private DrawingLevel _outerPenDrawingLevel;
        private float _xOffsetInPixel;
        private float _yOffsetInPixel;

        /// <summary>Constructor of the class.</summary>
        /// <returns>None</returns>
        public LineStyle()
            : this(new GeoPen(), new GeoPen(), new GeoPen())
        {
        }

        /// <summary>Constructor of the class.</summary>
        /// <returns>None</returns>
        /// <overloads>Pass in a single outer pen.</overloads>
        /// <remarks>Use this when no additional pens to draw the feature is needed.</remarks>
        /// <param name="outerPen">Outer pen to draw the feature.</param>
        public LineStyle(GeoPen outerPen)
            : this(outerPen, new GeoPen(), new GeoPen())
        {
        }

        /// <summary>Constructor of the class.</summary>
        /// <returns>None</returns>
        /// <overloads>Pass in an outer and inner pen. common for drawing roads, for example.</overloads>
        /// <param name="outerPen">Outer pen to draw the feature.</param>
        /// <param name="innerPen">Inner pen to draw the feature.</param>
        public LineStyle(GeoPen outerPen, GeoPen innerPen)
            : this(outerPen, innerPen, new GeoPen())
        {
        }

        /// <summary>Constructor of the class.</summary>
        /// <returns>None</returns>
        /// <overloads>Pass in an outer, inner and center line pen. common for drawing highways, for example.</overloads>
        /// <param name="outerPen">Outer pen to draw the feature.</param>
        /// <param name="innerPen">Inner pen to draw the feature.</param>
        /// <param name="centerPen">Center pen to draw the feature.</param>
        public LineStyle(GeoPen outerPen, GeoPen innerPen, GeoPen centerPen)
        {
            _outerPen = outerPen;
            _innerPen = innerPen;
            _centerPen = centerPen;
            _customLineStyles = new Collection<LineStyle>();
            _outerPenDrawingLevel = DrawingLevel.LevelOne;
            _innerPenDrawingLevel = DrawingLevel.LevelTwo;
            _centerPenDrawingLevel = DrawingLevel.LevelThree;
        }


        /// <summary>Gets and sets the X offset in pixel unit for the features to draw.</summary>
        /// <value>X offset in pixel unit for the features to draw.</value>
        /// <remarks>Combining with a Y offset, it allows effects such as drop shadows.</remarks>
        public float XOffsetInPixel
        {
            get { return _xOffsetInPixel; }
            set { _xOffsetInPixel = value; }
        }

        /// <summary>Gets and sets the Y offset in pixel unit for the features to draw.</summary>
        /// <value>Y offset in pixel unit for the features to draw.</value>
        /// <remarks>Combining with a X offset, it allows effects such as drop shadows.</remarks>
        public float YOffsetInPixel
        {
            get { return _yOffsetInPixel; }
            set { _yOffsetInPixel = value; }
        }

        /// <summary>Returns a collection of line styles to stack multiple line styles on top of each other.</summary>
        /// <value>Collection of line styles.</value>
        /// <remarks>Use these stacks to create drop shadow effects, multiple colored outlines, etc.</remarks>
        public Collection<LineStyle> CustomLineStyles
        {
            get { return _customLineStyles; }
        }

        /// <summary>Gets and sets the outer pen for the line.</summary>
        /// <value>Gets the outer pen for the line.</value>
        public GeoPen OuterPen
        {
            get { return _outerPen; }
            set { _outerPen = value; }
        }

        /// <summary>Gets and sets the onner pen for the line.</summary>
        /// <value>Gets the inner pen for the line.</value>
        public GeoPen InnerPen
        {
            get { return _innerPen; }
            set { _innerPen = value; }
        }

        /// <summary>Gets and sets the center pen for the line.</summary>
        /// <value>Gets the center pen for the line.</value>
        public GeoPen CenterPen
        {
            get { return _centerPen; }
            set { _centerPen = value; }
        }

        /// <summary>Gets and sets the drawing level for the outer pen.</summary>
        public DrawingLevel OuterPenDrawingLevel
        {
            get { return _outerPenDrawingLevel; }
            set { _outerPenDrawingLevel = value; }
        }

        /// <summary>Gets and sets the drawing level for the inner pen.</summary>
        public DrawingLevel InnerPenDrawingLevel
        {
            get { return _innerPenDrawingLevel; }
            set { _innerPenDrawingLevel = value; }
        }

        /// <summary>Gets and sets the drawing level for the center pen.</summary>
        public DrawingLevel CenterPenDrawingLevel
        {
            get { return _centerPenDrawingLevel; }
            set { _centerPenDrawingLevel = value; }
        }

        public bool IsDefault
        {
            get
            {
                if (!IsActive)
                {
                    return true;
                }

                var isDefault = false;

                if (_outerPen.Color.IsTransparent && _innerPen.Color.IsTransparent && _centerPen.Color.IsTransparent)
                {
                    if (_outerPen.Brush is GeoSolidBrush && _innerPen.Brush is GeoSolidBrush &&
                        _centerPen.Brush is GeoSolidBrush)
                    {
                        isDefault = true;
                    }
                }

                if (_customLineStyles.Count > 0)
                {
                    isDefault = false;
                }

                return isDefault;
            }
        }

        /// <summary>Draws the features on the canvas passed in.</summary>
        /// <remarks>Called from the concrete public method Draw. 
        /// When overriding this method, consider each feature and its column data values.If column data for a
        /// feature is needed, be sure to override GetRequiredColumnNamesCore and add the columns to the collection.</remarks>
        /// <returns>None</returns>
        /// <param name="features">Features to draw on the canvas.</param>
        /// <param name="canvas">Canvas to draw the features on.</param>
        /// <param name="labelsInThisLayer">Labels to draw in the current layer only.</param>
        /// <param name="labelsInAllLayers">Labels to drawn in all layers.</param>
        protected override void DrawCore(IEnumerable<Feature> features, BaseGeoCanvas canvas,
            Collection<SimpleCandidate> labelsInThisLayer, Collection<SimpleCandidate> labelsInAllLayers)
        {
            Validators.CheckParameterIsNotNull(features, "features");
            Validators.CheckParameterIsNull(_outerPen, "outerPen");
            Validators.CheckParameterIsNull(_innerPen, "innerPen");
            Validators.CheckParameterIsNull(_centerPen, "centerPen");
            Validators.CheckParameterIsNotNull(canvas, "canvas");
            Validators.CheckParameterIsNotNull(labelsInThisLayer, "labeledInLayer");
            Validators.CheckParameterIsNotNull(labelsInAllLayers, "labelsInAllLayers");
            Validators.CheckGeoCanvasIsInDrawing(canvas.IsDrawing);

            if (!IsDefault)
            {
                var featuresList = new List<Feature>(features);

                if (featuresList.Count < 10)
                {
                    var newFeatureList = RegenerateFeatures(canvas.CurrentWorldExtent, featuresList);
                    if (newFeatureList != null)
                    {
                        featuresList = newFeatureList;
                    }
                }

                for (var i = 0; i < featuresList.Count; i++)
                {
                    var shapeWellKnownType = featuresList[i].GetWellKnownType();
                    if (shapeWellKnownType != WellKnownType.Line && shapeWellKnownType != WellKnownType.Multiline &&
                        shapeWellKnownType != WellKnownType.GeometryCollection)
                    {
                        continue;
                    }

                    if (!_outerPen.Color.IsTransparent || !(_outerPen.Brush is GeoSolidBrush))
                    {
                        canvas.DrawLine(featuresList[i], _outerPen, _outerPenDrawingLevel, XOffsetInPixel,
                            YOffsetInPixel);
                    }
                    if (!_innerPen.Color.IsTransparent || !(_innerPen.Brush is GeoSolidBrush))
                    {
                        canvas.DrawLine(featuresList[i], _innerPen, _innerPenDrawingLevel, XOffsetInPixel,
                            YOffsetInPixel);
                    }
                    if (!_centerPen.Color.IsTransparent || !(_centerPen.Brush is GeoSolidBrush))
                    {
                        canvas.DrawLine(featuresList[i], _centerPen, _centerPenDrawingLevel, XOffsetInPixel,
                            YOffsetInPixel);
                    }

                    foreach (var lineStyle in _customLineStyles)
                    {
                        var tmpFeatures = new Feature[1] {featuresList[i]};
                        lineStyle.Draw(tmpFeatures, canvas, labelsInThisLayer, labelsInAllLayers);
                    }
                }
            }
        }

        /// <summary>Draws a sample feature on the canvas passed in.</summary>
        /// <remarks>Virtual method called from the concrete public method Draw. Can be used to
        /// display a legend or other sample area.</remarks>
        /// <returns>None</returns>
        /// <param name="canvas">Canvas to draw the features on.</param>
        protected override void DrawSampleCore(BaseGeoCanvas canvas, DrawingRectangleF drawingExtent)
        {
            Validators.CheckParameterIsNotNull(canvas, "canvas");
            Validators.CheckGeoCanvasIsInDrawing(canvas.IsDrawing);
            var rectangle = canvas.ToWorldCoordinate(drawingExtent);
            var line = GetSampleLine(rectangle);
            var features = new Feature[1] {new Feature(line)};
            Draw(features, canvas, new Collection<SimpleCandidate>(), new Collection<SimpleCandidate>());
        }

        private static LineShape GetSampleLine(RectangleShape rectangle)
        {
            var result = new LineShape();

            var centerPoint = rectangle.GetCenterPoint();
            var halfWidth = rectangle.Width*0.4;
            var x1 = centerPoint.X - halfWidth;
            var x2 = centerPoint.X + halfWidth;

            result.Vertices.Add(new Vertex(x1, centerPoint.Y));
            result.Vertices.Add(new Vertex(x2, centerPoint.Y));

            return result;
        }

        private static List<Feature> RegenerateFeatures(RectangleShape currentExtent, List<Feature> featureList)
        {
            var isToRegenerate = false;

            for (var i = 0; i < featureList.Count; i++)
            {
                var boundingBox = featureList[i].GetBoundingBox();
                if (boundingBox.Width > currentExtent.Width*1024 || boundingBox.Height > currentExtent.Height*1024)
                {
                    isToRegenerate = true;
                    break;
                }
            }

            List<Feature> newFeatureList = null;

            if (isToRegenerate)
            {
                var newExtent = (RectangleShape) currentExtent.CloneDeep();
                newExtent.ScaleUp(100);

                newFeatureList = new List<Feature>();
                for (var i = 0; i < featureList.Count; i++)
                {
                    var shape = featureList[i].GetShape();
                    var line = shape as LineShape;
                    if (line != null)
                    {
                        var tempMultiline = OverlappingForLine(line, newExtent);
                        if (tempMultiline.Lines.Count != 0)
                        {
                            newFeatureList.Add(new Feature(tempMultiline.GetWellKnownBinary(), featureList[i].Id));
                        }
                    }
                    else
                    {
                        var multiline = shape as MultilineShape;
                        if (multiline != null)
                        {
                            var tempMultiline = OverlappingForMultiline(multiline, newExtent);
                            if (tempMultiline.Lines.Count != 0)
                            {
                                newFeatureList.Add(new Feature(tempMultiline.GetWellKnownBinary(), featureList[i].Id));
                            }
                        }
                    }
                }
            }

            return newFeatureList;
        }

        private static MultilineShape OverlappingForLine(LineShape line, RectangleShape rectangle)
        {
            var resultShape = new MultilineShape();

            if (line.Vertices.Count < 2)
            {
                return resultShape;
            }

            var upperLeftX = rectangle.UpperLeftPoint.X;
            var upperLeftY = rectangle.UpperLeftPoint.Y;
            var lowerRightX = rectangle.LowerRightPoint.X;
            var lowerRightY = rectangle.LowerRightPoint.Y;

            var startVertex = line.Vertices[0];
            var newLine = new LineShape();
            var startVertexInRectangle = (startVertex.X >= upperLeftX && startVertex.Y <= upperLeftY &&
                                          startVertex.X <= lowerRightX && startVertex.Y >= lowerRightY);
            var isReCreateLineShape = false;

            for (var i = 1; i < line.Vertices.Count; i++)
            {
                var endVertex = line.Vertices[i];
                var endVertexInRectangle = (endVertex.X >= upperLeftX && endVertex.Y <= upperLeftY &&
                                            endVertex.X <= lowerRightX && endVertex.Y >= lowerRightY);

                if (!startVertexInRectangle && endVertexInRectangle)
                {
                    var crossingVertex = GetCrossingVertex(startVertex, endVertex, rectangle);
                    newLine.Vertices.Add(crossingVertex);
                    newLine.Vertices.Add(endVertex);
                    if (i == line.Vertices.Count - 1)
                    {
                        resultShape.Lines.Add(newLine);
                    }
                }
                else if (startVertexInRectangle && !endVertexInRectangle)
                {
                    var crossingVertex = GetCrossingVertex(startVertex, endVertex, rectangle);
                    if (newLine.Vertices.Count == 0)
                    {
                        newLine.Vertices.Add(startVertex);
                    }
                    newLine.Vertices.Add(crossingVertex);
                    resultShape.Lines.Add(newLine);
                    isReCreateLineShape = true;
                }
                else if (startVertexInRectangle)
                {
                    if (newLine.Vertices.Count == 0)
                    {
                        newLine.Vertices.Add(startVertex);
                    }
                    newLine.Vertices.Add(endVertex);
                }
                else
                {
                    var tempLine = OverlappingForLineWithTwoPoints(startVertex, endVertex, rectangle);

                    if (tempLine != null && tempLine.Vertices.Count == 2)
                    {
                        resultShape.Lines.Add(tempLine);
                        isReCreateLineShape = true;
                    }
                }

                if (isReCreateLineShape)
                {
                    isReCreateLineShape = false;
                    newLine = new LineShape();
                }

                startVertexInRectangle = endVertexInRectangle;
                startVertex = endVertex;
            }

            if ((resultShape.Lines.Count == 0 && newLine.Vertices.Count > 1))
            {
                resultShape.Lines.Add(newLine);
            }

            return resultShape;
        }

        private static MultilineShape OverlappingForMultiline(MultilineShape multiline, RectangleShape rectangle)
        {
            var resultMultiline = new MultilineShape();

            foreach (var line in multiline.Lines)
            {
                var tempMultiline = OverlappingForLine(line, rectangle);
                foreach (var tempLine in tempMultiline.Lines)
                {
                    resultMultiline.Lines.Add(tempLine);
                }
            }

            return resultMultiline;
        }

        private static LineShape OverlappingForLineWithTwoPoints(Vertex startPoint, Vertex endPoint,
            RectangleShape rectangle)
        {
            var startX = startPoint.X;
            var startY = startPoint.Y;
            var endX = endPoint.X;
            var endY = endPoint.Y;

            var isHorizontal = ((endY - startY) == 0);
            var isVertical = ((endX - startX) == 0);

            var upperLeftX = rectangle.UpperLeftPoint.X;
            var upperLeftY = rectangle.UpperLeftPoint.Y;
            var lowerRightX = rectangle.LowerRightPoint.X;
            var lowerRightY = rectangle.LowerRightPoint.Y;

            LineShape lineShape = null;

            if (isHorizontal)
            {
                if (IsInRange(upperLeftX, startX, endX) && IsInRange(endY, upperLeftY, lowerRightY))
                {
                    lineShape = new LineShape();
                    lineShape.Vertices.Add(new Vertex(upperLeftX, endY));
                }
                else if (IsInRange(lowerRightX, startX, endX) && IsInRange(endY, upperLeftY, lowerRightY))
                {
                    lineShape = new LineShape();
                    lineShape.Vertices.Add(new Vertex(lowerRightX, endY));
                }
            }
            else if (isVertical)
            {
                if (IsInRange(upperLeftY, startY, endY) && IsInRange(endX, upperLeftX, lowerRightX))
                {
                    lineShape = new LineShape();
                    lineShape.Vertices.Add(new Vertex(endX, upperLeftY));
                }
                else if (IsInRange(lowerRightY, startY, endY) && IsInRange(endX, upperLeftX, lowerRightX))
                {
                    lineShape = new LineShape();
                    lineShape.Vertices.Add(new Vertex(endX, lowerRightY));
                }
            }
            else
            {
                var tangent = (endY - startY)/(endX - startX);

                if (IsInRange(upperLeftY, startY, endY))
                {
                    var newX = startX + ((upperLeftY - startY)/tangent);
                    if (IsInRange(newX, upperLeftX, lowerRightX) && IsInRange(newX, startX, endX))
                    {
                        lineShape = new LineShape();
                        lineShape.Vertices.Add(new Vertex(newX, upperLeftY));
                    }
                }

                if (IsInRange(lowerRightY, startY, endY))
                {
                    var newX = startX + ((lowerRightY - startY)/tangent);
                    if (IsInRange(newX, upperLeftX, lowerRightX) && IsInRange(newX, startX, endX))
                    {
                        if (lineShape == null)
                        {
                            lineShape = new LineShape();
                        }
                        lineShape.Vertices.Add(new Vertex(newX, lowerRightY));
                    }
                }

                if (IsInRange(upperLeftX, startX, endX))
                {
                    var newY = startY + ((upperLeftX - startX)*tangent);
                    if (IsInRange(newY, upperLeftY, lowerRightY) && IsInRange(newY, startY, endY))
                    {
                        if (lineShape == null)
                        {
                            lineShape = new LineShape();
                        }
                        lineShape.Vertices.Add(new Vertex(upperLeftX, newY));
                    }
                }

                if (IsInRange(lowerRightX, startX, endX))
                {
                    var newY = startY + ((lowerRightX - startX)*tangent);
                    if (IsInRange(newY, upperLeftY, lowerRightY) && IsInRange(newY, startY, endY))
                    {
                        if (lineShape == null)
                        {
                            lineShape = new LineShape();
                        }
                        lineShape.Vertices.Add(new Vertex(lowerRightX, newY));
                    }
                }
            }

            return lineShape;
        }

        private static Vertex GetCrossingVertex(Vertex startPoint, Vertex endPoint, RectangleShape rectangle)
        {
            var startX = startPoint.X;
            var startY = startPoint.Y;
            var endX = endPoint.X;
            var endY = endPoint.Y;

            var isHorizontal = ((endY - startY) == 0);
            var isVertical = ((endX - startX) == 0);

            var upperLeftX = rectangle.UpperLeftPoint.X;
            var upperLeftY = rectangle.UpperLeftPoint.Y;
            var lowerRightX = rectangle.LowerRightPoint.X;
            var lowerRightY = rectangle.LowerRightPoint.Y;

            if (isHorizontal)
            {
                if (IsInRange(upperLeftX, startX, endX) && IsInRange(endY, upperLeftY, lowerRightY))
                {
                    return new Vertex(upperLeftX, endY);
                }
                if (IsInRange(lowerRightX, startX, endX) && IsInRange(endY, upperLeftY, lowerRightY))
                {
                    return new Vertex(lowerRightX, endY);
                }
            }
            else if (isVertical)
            {
                if (IsInRange(upperLeftY, startY, endY) && IsInRange(endX, upperLeftX, lowerRightX))
                {
                    return new Vertex(endX, upperLeftY);
                }
                if (IsInRange(lowerRightY, startY, endY) && IsInRange(endX, upperLeftX, lowerRightX))
                {
                    return new Vertex(endX, lowerRightY);
                }
            }
            else
            {
                var tangent = (endY - startY)/(endX - startX);

                if (IsInRange(upperLeftY, startY, endY))
                {
                    var newX = startX + ((upperLeftY - startY)/tangent);
                    if (IsInRange(newX, upperLeftX, lowerRightX) && IsInRange(newX, startX, endX))
                    {
                        return new Vertex(newX, upperLeftY);
                    }
                }

                if (IsInRange(lowerRightY, startY, endY))
                {
                    var newX = startX + ((lowerRightY - startY)/tangent);
                    if (IsInRange(newX, upperLeftX, lowerRightX) && IsInRange(newX, startX, endX))
                    {
                        return new Vertex(newX, lowerRightY);
                    }
                }

                if (IsInRange(upperLeftX, startX, endX))
                {
                    var newY = startY + ((upperLeftX - startX)*tangent);
                    if (IsInRange(newY, upperLeftY, lowerRightY) && IsInRange(newY, startY, endY))
                    {
                        return new Vertex(upperLeftX, newY);
                    }
                }

                if (IsInRange(lowerRightX, startX, endX))
                {
                    var newY = startY + ((lowerRightX - startX)*tangent);
                    if (IsInRange(newY, upperLeftY, lowerRightY) && IsInRange(newY, startY, endY))
                    {
                        return new Vertex(lowerRightX, newY);
                    }
                }
            }

            return new Vertex(double.MaxValue, double.MaxValue);
        }

        private static bool IsInRange(double value, double start, double end)
        {
            return (value >= start && value <= end) || (value <= start && value >= end);
        }
    }
}
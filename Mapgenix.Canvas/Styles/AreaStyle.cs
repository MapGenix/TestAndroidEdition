using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Mapgenix.Shapes;


namespace Mapgenix.Canvas
{
    /// <summary>Style to draw area based features</summary>
    /// <remarks>To draw area features. Fill brush to draw the interior. Outline pen to draw the border.</remarks>
    [Serializable]
    public class AreaStyle : BaseStyle
    {
        private readonly AreaStyleCustom _areaStyleCustom;
        private readonly Collection<AreaStyle> _customAreaStyles;
        private DrawingLevel _drawingLevel;
        private GeoSolidBrush _fillSolidBrush;
        private GeoPen _outlinePen;
        private PenBrushDrawingOrder _penBrushDrawingOrder;
        private float _xOffsetInPixel;
        private float _yOffsetInPixel;

        /// <summary>Constructor of the class.</summary>
        /// <returns>None</returns>
        /// <remarks>Properties for the fill brush and outline pen need to be set manually.</remarks>
        public AreaStyle()
            : this(new GeoPen(), new GeoSolidBrush(), PenBrushDrawingOrder.BrushFirst)
        {
        }

        /// <summary>Constructor of the class.</summary>
        /// <overloads>Fill brush without an outline.</overloads>
        /// <returns>None</returns>
        /// <remarks>The area does not have an outline.</remarks>
        /// <param name="fillSolidBrush">Solid brush to fill the area with.</param>
        public AreaStyle(GeoSolidBrush fillSolidBrush)
            : this(new GeoPen(), fillSolidBrush, PenBrushDrawingOrder.BrushFirst)
        {
        }

        /// <summary>Constructor of the class.</summary>
        /// <remarks>The area has an outline but no fill.</remarks>
        /// <overloads>Outline pen without a filled-in area.</overloads>
        /// <returns>None</returns>
        /// <param name="outlinePen">Outline pen to outline the area.</param>
        public AreaStyle(GeoPen outlinePen)
            : this(outlinePen, new GeoSolidBrush(), PenBrushDrawingOrder.BrushFirst)
        {
        }

        /// <summary>Constructor of the class.</summary>
        /// <overloads>Outline pen with a filled-in area.</overloads>
        /// <returns>None</returns>
        /// <remarks>Fill brush and an outline pen.</remarks>
        /// <param name="outlinePen">Outline pen to outline the area.</param>
        /// <param name="fillSolidBrush">Solid brush to fill the area with.</param>
        public AreaStyle(GeoPen outlinePen, GeoSolidBrush fillSolidBrush)
            : this(outlinePen, fillSolidBrush, PenBrushDrawingOrder.BrushFirst)
        {
        }

        /// <summary>Constructor of the class.</summary>
        /// <overloads>Outline pen with a filled-in area. To specify pen or brush to draw first.</overloads>
        /// <returns>None</returns>
        /// <param name="outlinePen">Outline pen to outline the area.</param>
        /// <param name="fillSolidBrush">Solid brush to fill the area with.</param>
        /// <param name="penBrushDrawingOrder">Outline pen or the fill brush is drawn first.</param>
        public AreaStyle(GeoPen outlinePen, GeoSolidBrush fillSolidBrush, PenBrushDrawingOrder penBrushDrawingOrder)
        {
            _outlinePen = outlinePen;
            _fillSolidBrush = fillSolidBrush;
            _penBrushDrawingOrder = penBrushDrawingOrder;
            _areaStyleCustom = new AreaStyleCustom();
            _customAreaStyles = new Collection<AreaStyle>();
            _drawingLevel = DrawingLevel.LevelOne;
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

        /// <summary>Gets and sets the solid brush to fill in the area features.</summary>
        /// <value>Solid brush to use to fill in the area features.</value>
        /// <remarks>The default solid brush is transparent.</remarks>
        public GeoSolidBrush FillSolidBrush
        {
            get { return _fillSolidBrush; }
            set { _fillSolidBrush = value; }
        }

        /// <summary>Returns a collection of area styles to stack multiple area styles on top of each other.</summary>
        /// <value>Collection of area styles.</value>
        /// <remarks>Use these stacks to create drop shadow effects, multiple colored outlines, etc.</remarks>
        public Collection<AreaStyle> CustomAreaStyles
        {
            get { return _customAreaStyles; }
        }

        /// <summary>Gets and sets the outline pen to outline the features.</summary>
        /// <value>Gets the outline pen to outline the features.</value>
        /// <remarks>The default outline pen color is transparent.</remarks>
        public GeoPen OutlinePen
        {
            get { return _outlinePen; }
            set { _outlinePen = value; }
        }

        /// <summary>Gets and sets the pen and brush drawing order.</summary>
        /// <value>Pen and brush drawing order.</value>
        /// <remarks>Controls whether the outline pen or the fill brush draws first. By default fill brush draws first.</remarks>
        public PenBrushDrawingOrder PenBrushDrawingOrder
        {
            get { return _penBrushDrawingOrder; }
            set { _penBrushDrawingOrder = value; }
        }

      
        public AreaStyleCustom AreaStyleCustom
        {
            get { return _areaStyleCustom ; }
        }

        /// <summary>Gets and sets the drawing level of the style.</summary>
        public DrawingLevel DrawingLevel
        {
            get { return _drawingLevel; }
            set { _drawingLevel = value; }
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

                if (_outlinePen.Color.IsTransparent && _fillSolidBrush.Color.IsTransparent &&
                    _outlinePen.Brush is GeoSolidBrush)
                {
                    isDefault = true;
                }

                if (isDefault && _areaStyleCustom.FillCustomBrush != null)
                {
                    isDefault = false;
                }

                if (isDefault && _customAreaStyles.Count != 0)
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
            Validators.CheckParameterIsNotNull(_outlinePen, "outlinePen");
            Validators.CheckParameterIsNotNull(_fillSolidBrush, "fillSolidBrush");
            Validators.CheckParameterIsNotNull(canvas, "canvas");
            Validators.CheckParameterIsNotNull(labelsInThisLayer, "labeledInLayer");
            Validators.CheckParameterIsNotNull(labelsInAllLayers, "labeledInLayers");
            Validators.CheckGeoCanvasIsInDrawing(canvas.IsDrawing);

            if (!IsDefault)
            {
                var drawingGeoPen = GetDrawingGeoPen();
                var drawingGeoBrush = GetDrawingGeoBrush();

                if (canvas.CurrentWorldExtent.Width == 0)
                {
                    return;
                }

                var featureList = new List<Feature>(features);

                if (featureList.Count < 10)
                {
                    var newFeatureList = RegenerateFeatures(canvas.CurrentWorldExtent, featureList);
                    if (newFeatureList != null)
                    {
                        featureList = newFeatureList;
                    }
                }

                for (var i = 0; i < featureList.Count; i++)
                {
                    var shapeWellKnownType = featureList[i].GetWellKnownType();
                    if (shapeWellKnownType != WellKnownType.Polygon && shapeWellKnownType != WellKnownType.Multipolygon &&
                        shapeWellKnownType != WellKnownType.GeometryCollection)
                    {
                        continue;
                    }

                    canvas.DrawArea(featureList[i], drawingGeoPen, drawingGeoBrush, _drawingLevel, XOffsetInPixel,
                        YOffsetInPixel, _penBrushDrawingOrder);

                    foreach (var areaStyle in _customAreaStyles)
                    {
                        areaStyle.Draw(new Feature[1] {featureList[i]}, canvas, labelsInThisLayer, labelsInAllLayers);
                    }
                }
            }
        }

        private BaseGeoBrush GetDrawingGeoBrush()
        {
            var drawingGeoBrush = _areaStyleCustom.FillCustomBrush;
            if (drawingGeoBrush == null && !_fillSolidBrush.Color.IsTransparent)
            {
                drawingGeoBrush = _fillSolidBrush;
            }
            return drawingGeoBrush;
        }

        private GeoPen GetDrawingGeoPen()
        {
            var result = _outlinePen;
            if (result.Color.IsTransparent && result.Brush is GeoSolidBrush)
            {
                result = null;
            }
            return result;
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

            var drawingRectangle = canvas.ToWorldCoordinate(drawingExtent);
            var tmpFeatures = new Feature[1] {new Feature(drawingRectangle)};
            Draw(tmpFeatures, canvas, new Collection<SimpleCandidate>(), new Collection<SimpleCandidate>());
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
                    var currentAreaShape = featureList[i].GetShape() as BaseAreaShape;
                    if (currentAreaShape != null)
                    {
                        var shapeIsComplex = CheckShapeShapeIsComplex(currentAreaShape);

                        if (!shapeIsComplex)
                        {
                            MultipolygonShape multipolygon = null;
                            try
                            {
                                //An error occurs in some cases in GetIntersection due to the validation of the resulting polygon (Val)
                                //multipolygon = currentAreaShape.GetIntersection(newExtent);
                                //multipolygon = (MultipolygonShape)currentAreaShape;
                            }
                            catch (Exception)
                            {
                            }

                            if (multipolygon != null)
                            {
                                if (featureList[i].ColumnValues == null)
                                {
                                    newFeatureList.Add(new Feature(multipolygon.GetWellKnownBinary(), featureList[i].Id));
                                }
                                else
                                {
                                    newFeatureList.Add(new Feature(multipolygon.GetWellKnownBinary(), featureList[i].Id,
                                        featureList[i].ColumnValues));
                                }
                            }
                            else
                            {
                                newFeatureList.Add(featureList[i]);
                            }
                        }
                    }
                }
            }

            return newFeatureList;
        }

        private static bool CheckShapeShapeIsComplex(BaseAreaShape currentAreaShape)
        {
            var shpaeIsComplex = false;
            var polygonShape = currentAreaShape as PolygonShape;
            if (polygonShape != null)
            {
                if (polygonShape.InnerRings.Count > 1000)
                {
                    shpaeIsComplex = true;
                }
            }

            return shpaeIsComplex;
        }
    }
}
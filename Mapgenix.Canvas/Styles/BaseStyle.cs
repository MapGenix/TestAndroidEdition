using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Mapgenix.Shapes;

namespace Mapgenix.Canvas
{
    /// <summary>Abstract class as the root for all styles used for drawing features.</summary>
    /// <remarks>In order to extend one's own style, it is suggested to first consider extending an existoing concrete style. 
    /// The most important method to override is DrawCore.</remarks>
    [Serializable]
    public abstract class BaseStyle
    {
        private readonly Collection<string> _requiredColumnNames;
        private bool _isActive;
        private string _name;

        /// <summary>Default constructor called by sub classes.</summary>
        /// <returns>None</returns>
        protected BaseStyle()
        {
            _name = string.Empty;
            _isActive = true;
            _requiredColumnNames = new Collection<string>();
        }

        /// <summary>Gets and sets the name of the style.</summary>
        /// <value>Name of the style.</value>
        /// <remarks>This property is not used internally. It can be used by the developper, for example to generate a legend.</remarks>
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        /// <summary>Gets and sets the active status of the style.</summary>
        /// <value>Active status of the style.</value>
        /// <remarks>If the style is not active, it does not draw.</remarks>
        public bool IsActive
        {
            get { return _isActive; }
            set { _isActive = value; }
        }

        /// <summary>Gets a collection of fields required for the style.</summary>
        /// <value>Collection of fields required for the style.</value>
        /// <remarks>Gets a collection of fields required for the style.</remarks>
        public Collection<string> RequiredColumnNames
        {
            get { return _requiredColumnNames; }
        }

        /// <summary>Creates a copy of style using the deep clone technique.</summary>
        /// <returns>Cloned style.</returns>
        /// <remarks>Deep cloning copies the cloned object and all the objects within it.</remarks>
        public BaseStyle CloneDeep()
        {
            return CloneDeepCore();
        }

        /// <summary>Create a copy of style using the deep clone technique.</summary>
        /// <returns>A cloned style.</returns>
        /// <remarks>Deep cloning copies the cloned object and all the objects within it.</remarks>
        protected virtual BaseStyle CloneDeepCore()
        {
            return (BaseStyle) SerializeCloneDeep(this);
        }

        /// <summary>Draws the features on the canvas passed in.</summary>
        /// <returns>None</returns>
        /// <remarks>
        /// 	<para>Concrete wrapper of the abstract method DrawCore.<br/><br/>
        ///     As a concrete public method that wraps a Core method, Mapgenix reserves the right
        ///     to add events and other logic returned by the Core version of the method. </para>
        /// </remarks>
        /// <param name="features">Features to draw on the canvas.</param>
        /// <param name="canvas">Canvas to draw the features on.</param>
        /// <param name="labelsInThisLayer">Labels to draw in the current layer only.</param>
        /// <param name="labelsInAllLayers">Labels to draw in all layers.</param>
        public void Draw(IEnumerable<Feature> features, BaseGeoCanvas canvas, Collection<SimpleCandidate> labelsInThisLayer,
            Collection<SimpleCandidate> labelsInAllLayers)
        {
            Validators.CheckParameterIsNotNull(canvas, "canvas");
            Validators.CheckGeoCanvasIsInDrawing(canvas.IsDrawing);
            Validators.CheckParameterIsNotNull(features, "features");
            Validators.CheckParameterIsNotNull(labelsInThisLayer, "labelsInThisLayer");
            Validators.CheckParameterIsNotNull(labelsInAllLayers, "labeledInLayers");

            if (IsActive)
            {
                DrawCore(features, canvas, labelsInThisLayer, labelsInAllLayers);
            }
        }

        /// <summary>Draws the shapes on the canvas passed in.</summary>
        /// <returns>None</returns>
        /// <remarks>
        /// 	<para>Concrete wrapper of the abstract method DrawCore.<br/><br/>
        ///     As a concrete public method that wraps a Core method, Mapgenix reserves the right
        ///     to add events and other logic returned by the Core version of the method.</para>
        /// </remarks>
        /// <param name="shapes">Shapes to draw on the canvas.</param>
        /// <param name="canvas">Canvas to draw the shapes on.</param>
        /// <param name="labelsInThisLayer">Labels to draw in the current layer only.</param>
        /// <param name="labelsInAllLayers">Labels to draw in all layers.</param>
        public void Draw(IEnumerable<BaseShape> shapes, BaseGeoCanvas canvas, Collection<SimpleCandidate> labelsInThisLayer,
            Collection<SimpleCandidate> labelsInAllLayers)
        {
            Validators.CheckParameterIsNotNull(canvas, "canvas");
            Validators.CheckParameterIsNotNull(shapes, "shapes");
            Validators.CheckGeoCanvasIsInDrawing(canvas.IsDrawing);
            Validators.CheckParameterIsNotNull(labelsInThisLayer, "labelsInThisLayer");
            Validators.CheckParameterIsNotNull(labelsInAllLayers, "labeledInLayers");

            var features = new Collection<Feature>();
            foreach (var shape in shapes)
            {
                features.Add(new Feature(shape));
            }

            Draw(features, canvas, labelsInThisLayer, labelsInAllLayers);
        }

        /// <summary>Draws the features on the canvas passed in.</summary>
        /// <remarks>Abstract method called from the concrete public method Draw.</remarks>
        /// <returns>None</returns>
        /// <param name="features">Features to draw on the canvas.</param>
        /// <param name="canvas">Canvas to draw the features on.</param>
        /// <param name="labelsInThisLayer">Labels to draw in the current layer only.</param>
        /// <param name="labelsInAllLayers">Labels to draw in all layers.</param>
        protected abstract void DrawCore(IEnumerable<Feature> features, BaseGeoCanvas canvas,
            Collection<SimpleCandidate> labelsInThisLayer, Collection<SimpleCandidate> labelsInAllLayers);

        /// <summary>Returns the column data required for the style to draw.</summary>
        /// <returns>Collection of column names requiered by the style.</returns>
        /// <remarks>As a concrete public method that wraps a Core method, Mapgenix reserves the right
        ///to add events and other logic returned by the Core version of the method.</remarks>
        public Collection<string> GetRequiredColumnNames()
        {
            var returnValue = new Collection<string>();
            if (IsActive)
            {
                returnValue = GetRequiredColumnNamesCore();
            }

            foreach (var columnName in _requiredColumnNames)
            {
                if (!returnValue.Contains(columnName))
                {
                    returnValue.Add(columnName);
                }
            }

            return returnValue;
        }

        /// <summary>Returns the column data required for the style to draw.</summary>
        /// <remarks>Abstract method called from the concrete public method GetRequiredFieldNames.</remarks>
        /// <returns>Returns a collection of column names required by the style.</returns>
        protected virtual Collection<string> GetRequiredColumnNamesCore()
        {
            return new Collection<string>();
        }

        /// <summary>Draws a sample feature on the canvas passed in.</summary>
        /// <returns>None</returns>
        /// <remarks>Concrete wrapper for the abstract method DrawSampleCore.
        ///     Can be used to display a legend or other sample area.
        ///     As a concrete public method that wraps a Core method, Mapgenix reserves the right
        ///     to add events and other logic returned by the Core version of the method.</remarks>
        public void DrawSample(BaseGeoCanvas canvas, DrawingRectangleF drawingExtent)
        {
            Validators.CheckParameterIsNotNull(canvas, "canvas");
            Validators.CheckGeoCanvasIsInDrawing(canvas.IsDrawing);
            Validators.CheckParameterIsNotNull(drawingExtent, "drawingExtent");

            DrawSampleCore(canvas, drawingExtent);
        }

        /// <summary>Draws a sample feature on the canvas passed in.</summary>
        /// <remarks>Virtual method called from the concrete public method Draw. 
        /// Can be used to display a legend or other sample area.</remarks>
        /// <returns>None</returns>
        /// <param name="canvas">Canvas you want to draw the features on.</param>
        /// <param name="drawingExtent">Extent of the drawing.</param>
        protected virtual void DrawSampleCore(BaseGeoCanvas canvas, DrawingRectangleF drawingExtent)
        {
            Validators.CheckParameterIsNotNull(canvas, "canvas");
            Validators.CheckGeoCanvasIsInDrawing(canvas.IsDrawing);
            Validators.CheckParameterIsNotNull(drawingExtent, "drawingExtent");
        }

        private static object SerializeCloneDeep(object instance)
        {
            var stream = new MemoryStream();
            var formatter = new BinaryFormatter();
            formatter.Serialize(stream, instance);
            stream.Seek(0, SeekOrigin.Begin);
            return formatter.Deserialize(stream);
        }
    }
}
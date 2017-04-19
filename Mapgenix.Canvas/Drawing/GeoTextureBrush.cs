using System;

namespace Mapgenix.Canvas
{
    /// <summary>Brush to fill an area with a image texture.</summary>
    [Serializable]
    public class GeoTextureBrush : BaseGeoBrush
    {
        private readonly GeoImage _geoImage;
        private DrawingRectangleF _drawingRectangleF;
        private GeoWrapMode _geoWrapMode;

        /// <summary>Constructor passing in an image.</summary>
        /// <overloads>Image for the texture.</overloads>
        /// <returns>None</returns>
        /// <remarks>To use the Tile wrap mode and to have image fill the entire shape.
        /// </remarks>
        /// <param name="image">Image for the texture.</param>
        public GeoTextureBrush(GeoImage image)
            : this(image, GeoWrapMode.Tile, new DrawingRectangleF())
        {
        }

        /// <summary>Constructor passing in image and screen rectangle.</summary>
        /// <overloads>To pass the image with a screen rectangle to determine how much area to fill.</overloads>
        /// <returns>None</returns>
        /// <param name="image">Image to use as the texture.</param>
        /// <param name="rectangleF">Rectangle (in screen coordinates) for the area to use for the texture.</param>
        public GeoTextureBrush(GeoImage image, DrawingRectangleF rectangleF)
            : this(image, GeoWrapMode.Tile, rectangleF)
        {
        }

        /// <summary>Constructor passing in image and wrapmode.</summary>
        /// <overloads>Image and the wrap mode.</overloads>
        /// <returns>None</returns>
        /// <remarks>None</remarks>
        /// <param name="image">Image you want to use as the texture.</param>
        /// <param name="wrapMode">Wrap mode to determine the way the texture wraps when the area to fill is larger than the texture image.
        /// </param>
        public GeoTextureBrush(GeoImage image, GeoWrapMode wrapMode)
            : this(image, wrapMode, new DrawingRectangleF())
        {
        }

        /// <summary>Constructor of the class passing in an image, the wrapmode and a screen rectangle .</summary>
        /// <overloads>To pass in the image as well as a screen rectangle to determine how much of the area to filled according to the wrap mode.</overloads>
        /// <returns>None</returns>
        /// <remarks>None</remarks>
        /// <param name="image">Image to use as the texture.</param>
        /// <param name="wrapMode">Wrap mode to determine the way the texture wraps when the area to fill is larger than the image texture.</param>
        /// <param name="rectangleF">Rectangle (in screen coordinates) for the area to apply the texture.</param>
        public GeoTextureBrush(GeoImage image, GeoWrapMode wrapMode, DrawingRectangleF rectangleF)
        {
            _geoImage = image;
            _geoWrapMode = wrapMode;
            _drawingRectangleF = rectangleF;
        }

        /// <summary>Gets and sets the GeoImage used for the texture fill.</summary>
        /// <value>Gets the GeoImage for the texture fill.</value>
        /// <remarks>None</remarks>
        public GeoImage GeoImage
        {
            get { return _geoImage; }
        }

        /// <summary>Gets and sets the screen rectangle to fill the texture.</summary>
        /// <value>Screen rectangle to fill the texture.</value>
        public DrawingRectangleF DrawingRectangleF
        {
            get { return _drawingRectangleF; }
            set { _drawingRectangleF = value; }
        }

        /// <summary>Gets and sets the wrap mode to determine how an area is filled if the area is larger than the texture image.</summary>
        /// <value>Wrap mode to determine how an area is filled if the area is larger than the texture image.</value>
        public GeoWrapMode GeoWrapMode
        {
            get { return _geoWrapMode; }
            set { _geoWrapMode = value; }
        }
    }
}
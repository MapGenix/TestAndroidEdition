using System;
 
namespace ThinkGeo.MapSuite.Core 
{
    /// <summary>This class is used to fill an area with a image texture.</summary>
    /// <remarks>
    /// This class is used to fill an area with a image texture. For example, you may have
    /// a small bitmap that looks like marble, which you can then use to fill in an area
    /// feature.
    /// </remarks>
    [Serializable]
    public class GeoTextureBrush : GeoBrush
    {
        private GeoImage geoImage;
        private GeoWrapMode geoWrapMode;
        private DrawingRectangleF drawingRectangleF;

        //TODO: David:  This while thing witht the screen rectangle is not what we want.
        // Cant we always in the canvas just use the height and width of the canvas area.
        //  I dont think users want ot mess witht his and I dont think it makes much sense to expose this to them.

        //TODO when use this constructor, the DrawingRectangleF is 0,0,0,0, it will throw an exception 
        // when g.Draw(...)
        /// <summary>This is a constructor for the class.</summary>
        /// <overloads>This overload allows you to pass in the image for the texture.</overloads>
        /// <returns>None</returns>
        /// <remarks>
        /// You will use this constructor when you want to use the Tile wrap mode and you
        /// want the image fill to encompass the entire shape.
        /// </remarks>
        /// <param name="image">This parameter is the image you want to use as the texture.</param>
        public GeoTextureBrush(GeoImage image)
            : this(image, GeoWrapMode.Tile, new DrawingRectangleF())
        { }

        //TODO when use this constructor, the DrawingRectangleF is 0,0,0,0, it will throw an exception 
        // when g.Draw(...)
        /// <summary>This is a constructor for the class.</summary>
        /// <overloads>
        /// This constructor allows you to pass in the image as well as a screen rectangle that
        /// determines how much of the area is filled.
        /// </overloads>
        /// <returns>None</returns>
        /// <remarks>
        /// This method allows you to pass in a rectangle in screen coordinates to determine
        /// how much of the area is textured.
        /// </remarks>
        /// <param name="image">This parameter is the image you want to use as the texture.</param>
        /// <param name="rectangleF">
        /// This parameter is a rectangle (in screen coordinates) that specifies the area you want
        /// to use for the texture.
        /// </param>
        public GeoTextureBrush(GeoImage image, DrawingRectangleF rectangleF)
            : this(image, GeoWrapMode.Tile, rectangleF)
        { }

        /// <summary>This is a constructor for the class.</summary>
        /// <overloads>This constructor allows you to pass in the image and the wrap mode.</overloads>
        /// <returns>None</returns>
        /// <remarks>None</remarks>
        /// <param name="image">This parameter is the image you want to use as the texture.</param>
        /// <param name="wrapMode">
        /// This parameter determines the way the texture wraps when the area to fill is
        /// larger than the texture.
        /// </param>
        public GeoTextureBrush(GeoImage image, GeoWrapMode wrapMode)
            : this(image, wrapMode, new DrawingRectangleF())
        { }

        /// <summary>This is a constructor for the class.</summary>
        /// <overloads>
        /// This constructor allows you to pass in the image as well as a screen rectangle that
        /// determines how much of the area is filled along with the wrap mode.
        /// </overloads>
        /// <returns>None</returns>
        /// <remarks>None</remarks>
        /// <param name="image">This parameter is the image you want to use as the texture.</param>
        /// <param name="wrapMode">
        /// This parameter determines the way the texture wraps when the area to fill is
        /// larger than the texture.
        /// </param>
        /// <param name="rectangleF">
        /// This parameter is a rectangle (in screen coordinates) that specifies the area you want
        /// to use for the texture.
        /// </param>
        public GeoTextureBrush(GeoImage image, GeoWrapMode wrapMode, DrawingRectangleF rectangleF)
        {
            this.geoImage = image;
            this.geoWrapMode = wrapMode;
            this.drawingRectangleF = rectangleF;
        }

        //TODO: David:  Why is this get only??  Doesnt seem to make sense...
        /// <summary>This property gets and sets the GeoImage used for the texture fill.</summary>
        /// <value>This property gets the GeoImage used for the texture fill.</value>
        /// <remarks>None</remarks>
        public GeoImage GeoImage
        {
            get { return geoImage; }
        }

        /// <summary>This property gets and sets the screen rectangle used to fill the texture.</summary>
        /// <value>This property gets the screen rectangle used to fill the texture.</value>
        /// <remarks>None</remarks>
        public DrawingRectangleF DrawingRectangleF
        {
            get { return drawingRectangleF; }
            set { drawingRectangleF = value; }
        }

        /// <summary>
        /// This property gets and sets the wrap mode that is used to determine how an area
        /// is filled if the area is larger than the texture.
        /// </summary>
        /// <value>
        /// This property gets the wrap mode that is used to determine how an area is filled
        /// if the area is larger than the texture.
        /// </value>
        /// <remarks>
        /// This property gets and sets the wrap mode that is used to determine how an area
        /// is filled if the area is larger than the texture.
        /// </remarks>
        public GeoWrapMode GeoWrapMode
        {
            get { return geoWrapMode; }
            set { geoWrapMode = value; }
        }
    }
}
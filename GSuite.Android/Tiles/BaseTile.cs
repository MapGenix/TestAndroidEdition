using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Mapgenix.Shapes;

namespace Mapgenix.GSuite.Android
{
    /// <summary>
    /// 	<para>Abstract class for tile. Tile is used as a rectangle image saved in Memory or Hard disk to improve drawing performance.</para>
    /// 	<para></para>
    /// 	<para>The Tile cache system is the pilar of the drawing technique for many mapping solutions such as OpenStreetMaps, GoogleMaps, HEREMaps,
    /// 	BingsMaps, etc.</para>
    /// </summary>
    /// <remarks>Abstract BaseTile is the lowest level class in the Tile hierarchy. Directly
    /// inherited from Tile are BitmapTile, NativeImageTile. The methods and
    /// properties on BaseTile are the lowest common denominator for the inherited Tiles.</remarks>
    [Serializable]
    public abstract class BaseTile
    {
        private RectangleShape _boundingBox;
        private double _scale;

        /// <summary>Default constructor of BaseTile.</summary>
        public BaseTile()
            : this(new RectangleShape(), 0.0)
        {
        }

        /// <summary>Passing a boundingBox and a scale.
        /// </summary>
        public BaseTile(RectangleShape boundingBox, double scale)
        {
            Validators.CheckParameterIsNotNull(boundingBox, "boundingBox");

            _boundingBox = boundingBox;
            _scale = scale;
        }

        /// <summary>The scale describing the Tile.</summary>
        public double Scale
        {
            get { return _scale; }
            set { _scale = value; }
        }

        /// <summary>The Bounding box describing the Tile.</summary>
        public RectangleShape BoundingBox
        {
            get { return _boundingBox; }
            set
            {
                Validators.CheckParameterIsNotNull(value, "BoundingBox");
                _boundingBox = value;
            }
        }

        /// <summary>To deep clone a BaseTile object.</summary>
        /// <returns>Cloned BaseTile object.</returns>
        public BaseTile CloneDeep()
        {
            return CloneDeepCore();
        }

        /// <summary>To deep clone a BaseTile object.</summary>
        /// <returns>Cloned BaseTile object.</returns>
        protected virtual BaseTile CloneDeepCore()
        {
            return (BaseTile)SerializeCloneDeep(this);
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
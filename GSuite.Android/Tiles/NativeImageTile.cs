using System;
using Mapgenix.Shapes;

namespace Mapgenix.GSuite.Android
{
    /// <summary>Represents a native image tile defined as scale, RectangleBox and a byte array.</summary>
    [Serializable]
    public class NativeImageTile : BaseTile
    {
        private byte[] _nativeImage;

        /// <summary>Default constructor without passing any parameters. Properties needs to be set manually.</summary>
        public NativeImageTile()
            : this(null, new RectangleShape(), 0.0)
        {
        }

        /// <summary>Constructor for Tile passing a boundingBox and a scale.</summary>
        public NativeImageTile(RectangleShape boundingBox, double scale)
            : this(null, boundingBox, scale)
        {
        }

        /// <summary>Constructor for Tile passing an image bitmap array, a boundingBox and a scale.</summary>
        public NativeImageTile(byte[] nativeImage, RectangleShape boundingBox, double scale)
            : base(boundingBox, scale)
        {
            _nativeImage = nativeImage;
        }

        /// <summary>Gets and sets the byte array showing the image of the tile.</summary>
        public byte[] NativeImage
        {
            get { return _nativeImage; }
            set { _nativeImage = value; }
        }
    }
}
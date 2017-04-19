using System;
using System.Drawing;
using Mapgenix.Shapes;

namespace Mapgenix.Canvas
{
    /// <summary>
    /// NativeImageTile defined as scale, RectangleBox
    /// and a byte array.
    /// </summary>
    [Serializable]
    public class BitmapTile : BaseTile, IDisposable
    {
        private Bitmap _bitmap;

        /// <summary>Default constructor without passing any parameters. Properties need to be set manually.</summary>
        public BitmapTile()
            : this(null, new RectangleShape(), 0.0)
        {
        }

        /// <summary>Constructor for Tile passing a boundingBox and a scale.</summary>
        public BitmapTile(RectangleShape boundingBox, double scale)
            : this(null, boundingBox, scale)
        {
        }

        /// <summary>Constructor for Tile passing an image bitmap, a boundingBox and a scale.</summary>
        public BitmapTile(Bitmap bitmap, RectangleShape boundingBox, double scale)
            : base(boundingBox, scale)
        {
            _bitmap = bitmap;
        }

        /// <summary>Gets and sets the Bitmap showing the tile image.</summary>
        public Bitmap Bitmap
        {
            get { return _bitmap; }
            set { _bitmap = value; }
        }

        /// <summary>Releases and resets unmanaged resources.</summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~BitmapTile()
        {
            Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_bitmap != null)
                {
                    _bitmap.Dispose();
                }
            }
        }
    }
}
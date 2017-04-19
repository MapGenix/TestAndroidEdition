using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mapgenix.Layers
{
    class WmsLayerTileInfo
    {
        private readonly int _imageId;
        private readonly int _tileX;
        private readonly int _tileY;

        public WmsLayerTileInfo()
            : this(0, 0, 0)
        { }

        public WmsLayerTileInfo(int imageId, int tileX, int tileY)
        {
            this._imageId = imageId;
            this._tileX = tileX;
            this._tileY = tileY;
        }

        public int ImageId
        {
            get { return _imageId; }
        }

        public int TileX
        {
            get { return _tileX; }
        }

        public int TileY
        {
            get { return _tileY; }
        }
    }
}

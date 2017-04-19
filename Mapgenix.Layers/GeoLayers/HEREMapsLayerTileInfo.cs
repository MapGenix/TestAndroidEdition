namespace Mapgenix.Layers
{
    internal class HEREMapsLayerTileInfo
    {
        private readonly int _imageId;
        private readonly int _tileX;
        private readonly int _tileY;

        public HEREMapsLayerTileInfo()
            : this(0, 0, 0)
        { }

        public HEREMapsLayerTileInfo(int imageId, int tileX, int tileY)
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

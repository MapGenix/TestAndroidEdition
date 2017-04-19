namespace Mapgenix.Layers
{
    public class GoogleMapsLayerTileInfo
    {
        private int imageId;
        private int tileX;
        private int tileY;

        public GoogleMapsLayerTileInfo()
            : this(0, 0, 0)
        {
        }

        public GoogleMapsLayerTileInfo(int imageId, int tileX, int tileY)
        {
            this.imageId = imageId;
            this.tileX = tileX;
            this.tileY = tileY;
        }

        public int ImageId
        {
            get { return imageId; }
        }

        public int TileX
        {
            get { return tileX; }
        }

        public int TileY
        {
            get { return tileY; }
        }
    }
}
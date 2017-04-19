namespace Mapgenix.FeatureSource
{
    public class GeoData
    {
        public string ogr_name { get; set; }
        public int ogr_fid { get; set; }
        public byte[] ogr_geometry { get; set; }
        public int ogr_srid { get; set; }
       
    }
}

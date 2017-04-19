namespace Mapgenix.FeatureSource
{
	public class TextFileSettings
	{
		public char Delimiter { get; set; }
		public bool FirstRowIsHeader { get; set; }
		public bool SkipEmptyLines { get; set; }
		public TextFileType FileType { get; set; }
		public string[] GeometryFields { get; set; }
		public string LayerName { get; set; }

	}

	public enum TextFileType
	{
		Csv,
		Wkt
	}
}

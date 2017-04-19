using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Mapgenix.FeatureSource
{
    /// <summary>
    /// Factory for creating FeatureSource
    /// </summary>
    public static class FeatureSourceFactory
	{
        public static WfsFeatureSource CreateWfsFeatureSource(string serviceLocationUrl, string typeName)
        {
            return new WfsFeatureSource
            {
                ServiceLocationUrl = serviceLocationUrl,
                TypeName = typeName,
                TimeoutInSeconds = 20,
                WfsNamespace = WfsNamespace.Null
            };
        }

        public static GridFeatureSource CreateGridFeatureSource(string gridPathFilename)
        {
			return new GridFeatureSource
			{
				InitializedWithGridCellMatrix = false,
	            PathFilename = gridPathFilename

			};
        }

		public static GridFeatureSource CreateGridFeatureSource(GridCell[,] gridCellMatrix)
        {
			return new GridFeatureSource
			{
				InitializedWithGridCellMatrix = true,
				GridCellMatrix = gridCellMatrix
			};
        }


        public static ShapeFileFeatureSource CreateShapeFileFeatureSource()
        {
            return CreateShapeFileFeatureSource(string.Empty, string.Empty, ReadWriteMode.ReadOnly);
        }


        public static ShapeFileFeatureSource CreateShapeFileFeatureSource(string shapePathFilename)
        {
            return CreateShapeFileFeatureSource(shapePathFilename, Path.ChangeExtension(shapePathFilename, ".idx"), ReadWriteMode.ReadOnly);
        }


        public static ShapeFileFeatureSource CreateShapeFileFeatureSource(string shapePathFilename, Encoding encoding)
        {
            return CreateShapeFileFeatureSource(shapePathFilename, Path.ChangeExtension(shapePathFilename, ".idx"), ReadWriteMode.ReadOnly, encoding);
        }


        public static ShapeFileFeatureSource CreateShapeFileFeatureSource(string shapePathFilename, ReadWriteMode readWriteMode)
        {
            return CreateShapeFileFeatureSource(shapePathFilename, Path.ChangeExtension(shapePathFilename, ".idx"), readWriteMode);
        }


        public static ShapeFileFeatureSource CreateShapeFileFeatureSource(string shapePathFilename, string indexPathFilename)
        {
            return CreateShapeFileFeatureSource(shapePathFilename, indexPathFilename, ReadWriteMode.ReadOnly);
        }


        public static ShapeFileFeatureSource CreateShapeFileFeatureSource(string shapePathFilename, string indexPathFilename, ReadWriteMode readWriteMode)
        {
            return CreateShapeFileFeatureSource(shapePathFilename, indexPathFilename, readWriteMode, Encoding.Default);
        }


        public static ShapeFileFeatureSource CreateShapeFileFeatureSource(string shapePathFilename, string indexPathFilename, ReadWriteMode readWriteMode, Encoding encoding)
        {
            GeoDbf dbf = new GeoDbf();
            dbf.Encoding = encoding;
            return new ShapeFileFeatureSource
            {
                ShapePathFileName = shapePathFilename,
                IndexPathFileName = indexPathFilename,
                ReadWriteMode = readWriteMode,
                RequireIndex = true,
                _dBaseEngine = dbf
            };
        }


        /*public static BaseFeatureSource CreateVectorFeatureSource(string filePath)
        {
            return new VectorFeatureSource(filePath);
        }*/

        /// <summary>
        /// Create ArcGISRestFeatureSource element
        /// </summary>
        /// <param name="mapServerUrl">ArcGIS map server url example: http://sampleserver1.arcgisonline.com/ArcGIS/rest/services/Specialty/ESRI_StatesCitiesRivers_USA/MapServer/ </param>
        /// <param name="layerId">Map server layer ID</param>
        /// <returns>ArcGISServerRESTFeatureSource</returns>
        public static ArcGISServerRESTFeatureSource CreateArcGISRestFeatureSource(string mapServerUrl, string layerId)
        {
            return new ArcGISServerRESTFeatureSource
            {
                MapServiceURL = new System.Uri(mapServerUrl),
                LayerId = layerId
            };
        }

    }
}

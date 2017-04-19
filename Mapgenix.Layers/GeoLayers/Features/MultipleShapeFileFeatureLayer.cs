using System;
using System.Collections.ObjectModel;
using System.Text;
using Mapgenix.FeatureSource;

namespace Mapgenix.Layers
{
    /// <summary>Layer for multiple shapefiles of the same geometric type.</summary>
    /// <remarks>Represents multiple shapefiles of the same geometric type.<br/>
    /// 	<br/>
    /// Notice you can generate single large index for all the shapefiles included.</remarks>
    [Serializable]
    public class MultipleShapeFileFeatureLayer : BaseFeatureLayer
    {
        public Collection<string> ShapeFiles
        {
            get { return ((MultipleShapeFileFeatureSource)FeatureSource).ShapeFiles; }
        }

        public Collection<string> Indexes
        {
            get { return ((MultipleShapeFileFeatureSource)FeatureSource).Indexes; }
        }

        /// <summary>Gets and sets the shapefile pattern making up the layer.</summary>
        /// <value>Shapefile pattern making up the layer.</value>
        /// <remarks>None</remarks>
        public string MultipleShapeFilePattern
        {
            get { return ((MultipleShapeFileFeatureSource)FeatureSource).MultipleShapeFilePattern; }
            set { ((MultipleShapeFileFeatureSource)FeatureSource).MultipleShapeFilePattern = value; }
        }

        /// <summary>Gets and sets the spatial index file pattern making up the layer.</summary>
        /// <value>Spatial index file pattern making up the layer.</value>
        /// <remarks>None</remarks>
        public string IndexFilePattern
        {
            get { return ((MultipleShapeFileFeatureSource)FeatureSource).IndexFilePattern; }
            set { ((MultipleShapeFileFeatureSource)FeatureSource).IndexFilePattern = value; }
        }

        /// <summary>Gets and sets the encoding information of the DBF.</summary>
        public Encoding Encoding
        {
            get { return ((MultipleShapeFileFeatureSource)FeatureSource).Encoding; }
            set { ((MultipleShapeFileFeatureSource)FeatureSource).Encoding = value; }
        }

        /// <summary>Builds a spatial index for the multiple layer.</summary>
        /// <overloads>Allows to pass in a shapefile path and filename pattern. Generates
        /// indexes for each shapefile in the multiple layer, matching the index file names to the shapefile names.
        /// </overloads>
        /// <returns>None</returns>
        /// <param name="multipleShapeFilePattern">Matching pattern defining which shapefiles to include.</param>
        public static void BuildIndex(string multipleShapeFilePattern)
        {
            MultipleShapeFileFeatureSource.BuildIndex(multipleShapeFilePattern);
        }

        /// <summary>Builds a spatial index for the multiple layer.</summary>
        /// <overloads>Allows to pass in a shapefile path and filename pattern. Generates
        /// indexes for each shapefile in the multiple layer, matching the index file names to the shapefile names.
        /// </overloads>
        /// <returns>None</returns>
        /// <param name="multipleShapeFilePattern">Matching pattern defining which shapefiles to include.</param>
        /// <param name="buildIndexMode">Mode whether to rebuilt index file if it already exists.</param>
        public static void BuildIndex(string multipleShapeFilePattern, BuildIndexMode buildIndexMode)
        {
            MultipleShapeFileFeatureSource.BuildIndex(multipleShapeFilePattern, buildIndexMode);
        }

        /// <summary>Builds a spatial index for the multiple layer.</summary>
       public static void BuildIndex(string multipleShapeFilePattern, string indexFilePattern)
        {
            MultipleShapeFileFeatureSource.BuildIndex(multipleShapeFilePattern, indexFilePattern);
        }

        /// <summary>Builds a spatial index for the multiple layer.</summary>
        public static void BuildIndex(string multipleShapeFilePattern, string indexFilePattern, BuildIndexMode buildIndexMode)
        {
            MultipleShapeFileFeatureSource.BuildIndex(multipleShapeFilePattern, indexFilePattern, buildIndexMode);
        }

        /// <summary>Builds a spatial index for the multiple layer.</summary>
        public static void BuildIndex(string multipleShapeFilePattern, string columnName, string regularExpression, string indexFilename)
        {
            MultipleShapeFileFeatureSource.BuildIndex(multipleShapeFilePattern, columnName, regularExpression, indexFilename);
        }

        /// <summary>Builds a spatial index for the multiple layer.</summary>
        public static void BuildIndex(string multipleShapeFilePattern, string columnName, string regularExpression, string indexFilename, BuildIndexMode buildIndexMode)
        {
            MultipleShapeFileFeatureSource.BuildIndex(multipleShapeFilePattern, columnName, regularExpression, indexFilename, buildIndexMode);
        }

        /// <summary>Builds a spatial index for the multiple layer.</summary>
        public static void BuildIndex(string[] multipleShapeFiles, string[] multipleShapeFileIndexes)
        {
            MultipleShapeFileFeatureSource.BuildIndex(multipleShapeFiles, multipleShapeFileIndexes);
        }

        /// <summary>Builds a spatial index for the multiple layer.</summary>
        public static void BuildIndex(string[] multipleShapeFiles, string[] multipleShapeFileIndexes, BuildIndexMode buildIndexMode)
        {
            MultipleShapeFileFeatureSource.BuildIndex(multipleShapeFiles, multipleShapeFileIndexes, buildIndexMode);
        }

        /// <summary>Returns a collection of the shapefiles and their paths making up the multiple layer.</summary>
        /// <returns>Collection of the shapefiles and their paths making up the multiple layer.</returns>
        /// <remarks>None</remarks>
        public Collection<string> GetShapePathFileNames()
        {
            return ((MultipleShapeFileFeatureSource)FeatureSource).GetShapePathFileNames();
        }

        /// <summary>Returns a collection of the index files and their paths making up the multiple layer.</summary>
        /// <returns>Collection of the index files and their paths making up the multiple layer.</returns>
        /// <remarks>None</remarks>
        public Collection<string> GetIndexPathFileNames()
        {
            return ((MultipleShapeFileFeatureSource)FeatureSource).GetIndexPathFileNames();
        }
    }
}
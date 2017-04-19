using System;
using Mapgenix.FeatureSource;
using Mapgenix.Utils;

namespace Mapgenix.Layers
{
    /// <summary>Feature layer for an ESRI shapefile.</summary>
    [Serializable]
    public class ShapeFileFeatureLayer : BaseFeatureLayer
    {
        /// <summary>Allows to pass in one's own stream to represent the files.</summary>
        /// <remarks>The stream can come from a variety of places such as isolated storage, a compressed file,
        /// and encrypted stream.</remarks>
        public event EventHandler<StreamLoadingEventArgs> StreamLoading
        {
            add
            {
                ((ShapeFileFeatureSource)FeatureSource).StreamLoading += value;
            }
            remove
            {
                ((ShapeFileFeatureSource)FeatureSource).StreamLoading -= value;
            }
        }

       

        public int MaxRecordsToDraw
        {
            get { return ((ShapeFileFeatureSource)FeatureSource).MaxRecordsToDraw; }
            set { ((ShapeFileFeatureSource)FeatureSource).MaxRecordsToDraw = value; }
        }


        /// <summary>Builds a spatial index for the layer.</summary>
        /// <overloads>Allows to pass in the shapefile and the rebuild mode.</overloads>
        /// <returns>None</returns>
       /// <param name="pathFilename">Matching pattern defining which shapefile to include.</param>
        /// <param name="rebuildExistingIndexMode">Mode to rebuild index file if it already exists.</param>
        public static void BuildIndexFile(string pathFilename, BuildIndexMode rebuildExistingIndexMode)
        {
            Validators.CheckParameterIsNotNull(pathFilename, "pathFileName");
            Validators.CheckShapeFileNameIsValid(pathFilename, "pathFileName");
            Validators.CheckBuildIndexModeIsValid(rebuildExistingIndexMode, "rebuildExistingIndexMode");

            ShapeFileFeatureSourceHelper.BuildIndexFile(pathFilename, rebuildExistingIndexMode);
        }



    }
}
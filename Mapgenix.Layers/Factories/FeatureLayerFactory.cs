using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using Mapgenix.FeatureSource;
using Mapgenix.Shapes;
using Mapgenix.Canvas;
using Mapgenix.Styles;

namespace Mapgenix.Layers
{
    /// <summary>
    /// Factory class for feature layers
    /// </summary>
    public static class FeatureLayerFactory
    {
        public static GridFeatureLayer CreateGridFeatureLayer(string gridPathFilename)
        {
            return new GridFeatureLayer
            {
                FeatureSource = FeatureSourceFactory.CreateGridFeatureSource(gridPathFilename),
                DrawingQuality = DrawingQuality.HighSpeed,
                Name = string.Empty,
				HasBoundingBox = true,
                IsVisible = true
            }; 
        }

        public static GridFeatureLayer CreateGridFeatureLayer(GridCell[,] gridCellMatrix)
        {
            return new GridFeatureLayer
            {
                FeatureSource = FeatureSourceFactory.CreateGridFeatureSource(gridCellMatrix),
                DrawingQuality = DrawingQuality.HighSpeed,
                Name = string.Empty,
				HasBoundingBox = true,
                IsVisible = true
            };
        }

   
        public static InMemoryFeatureLayer CreateInMemoryFeatureLayer()
        {
            return CreateInMemoryFeatureLayer(new FeatureSourceColumn[] { }, new Feature[] { });
        }

        public static InMemoryFeatureLayer CreateInMemoryFeatureLayer(IEnumerable<FeatureSourceColumn> featureSourceColumns, IEnumerable<Feature> features)
        {
            InMemoryFeatureLayer inMemoryFeatureLayer = new InMemoryFeatureLayer();
            inMemoryFeatureLayer.FeatureSource = new InMemoryFeatureSource(featureSourceColumns, features);
            inMemoryFeatureLayer.Name = string.Empty;
            inMemoryFeatureLayer.IsVisible = true;
           
            return inMemoryFeatureLayer;
        }

        public static InMemoryFeatureLayer CreateInMemoryFeatureLayer(IEnumerable<FeatureSourceColumn> featureSourceColumns, IEnumerable<BaseShape> shapes)
        {
            Validators.CheckParameterIsNotNull(shapes, "shapes");

            Collection<Feature> features = new Collection<Feature>();
            foreach (BaseShape shape in shapes)
            {
                features.Add(new Feature(shape));
            }

            InMemoryFeatureLayer inMemoryFeatureLayer = new InMemoryFeatureLayer();
            inMemoryFeatureLayer.FeatureSource = new InMemoryFeatureSource(featureSourceColumns, features);
            inMemoryFeatureLayer.Name = string.Empty;
            inMemoryFeatureLayer.IsVisible = true;

            return inMemoryFeatureLayer;
        }
      
        
        public static MultipleFeatureLayer CreateMultipleFeatureLayer(IEnumerable<FeatureSource.BaseFeatureSource> featureSources)
        {
            return new MultipleFeatureLayer
            {
                FeatureSource = new MultipleFeatureSource(featureSources),
                Name = string.Empty,
                IsVisible = true
            }; 
        }


        public static MultipleShapeFileFeatureLayer CreateMultipleShapeFileFeatureLayer(string multipleShapeFilePattern)
        {
            return CreateMultipleShapeFileFeatureLayer(multipleShapeFilePattern, string.Empty);
        }

		public static ShapeFileFeatureLayer CreateShapeFileFeatureLayer(string shapePathFilename, string layerName = null)
		{
			return CreateShapeFileFeatureLayer(shapePathFilename, Path.ChangeExtension(shapePathFilename, ".idx"),
				ReadWriteMode.ReadOnly, layerName);
		}

        public static MultipleShapeFileFeatureLayer CreateMultipleShapeFileFeatureLayer(string multipleShapeFilePattern, string indexFilePattern)
        {
            return new MultipleShapeFileFeatureLayer
            {
                FeatureSource = new MultipleShapeFileFeatureSource(multipleShapeFilePattern, indexFilePattern),
                Name = string.Empty,
                IsVisible = true
            };            
        }

        public static MultipleShapeFileFeatureLayer CreateMultipleShapeFileFeatureLayer(IEnumerable<string> shapeFiles)
            
        {
            Collection<string> multipleShapeFileIndexes = new Collection<string>();
            foreach (string item in shapeFiles)
            {
                string indexFile = Path.ChangeExtension(item, ".midx");
                multipleShapeFileIndexes.Add(indexFile);
            }
            return new MultipleShapeFileFeatureLayer
            {
                FeatureSource = new MultipleShapeFileFeatureSource(shapeFiles, multipleShapeFileIndexes),
                Name = string.Empty,
                IsVisible = true,
				HasBoundingBox = true
            };            
        }

        public static MultipleShapeFileFeatureLayer CreateMultipleShapeFileFeatureLayer(IEnumerable<string> shapeFiles, IEnumerable<string> indexes)
        {
            return new MultipleShapeFileFeatureLayer
            {
                FeatureSource = new MultipleShapeFileFeatureSource(shapeFiles, indexes),
                Name = string.Empty,
                IsVisible = true,
				HasBoundingBox = true
            };
        }
        
        

        public static ShapeFileFeatureLayer CreateShapeFileFeatureLayer(string shapePathFilename)
        {
            return CreateShapeFileFeatureLayer(shapePathFilename, Path.ChangeExtension(shapePathFilename, ".idx"), ReadWriteMode.ReadOnly);
        }

        public static ShapeFileFeatureLayer CreateShapeFileFeatureLayer(string shapePathFilename, Encoding encoding)
        {
            return CreateShapeFileFeatureLayer(shapePathFilename, Path.ChangeExtension(shapePathFilename, ".idx"), ReadWriteMode.ReadOnly, encoding);
        }


        public static ShapeFileFeatureLayer CreateShapeFileFeatureLayer(string shapePathFilename, ReadWriteMode readWriteMode)
        {
            return CreateShapeFileFeatureLayer(shapePathFilename, Path.ChangeExtension(shapePathFilename, ".idx"), readWriteMode);
        }

     
        public static ShapeFileFeatureLayer CreateShapeFileFeatureLayer(string shapePathFilename, string indexPathFilename, ReadWriteMode readWriteMode)
        {
            return CreateShapeFileFeatureLayer(shapePathFilename, indexPathFilename, readWriteMode, Encoding.Default);
        }

        public static ShapeFileFeatureLayer CreateShapeFileFeatureLayer(string shapePathFilename, string indexPathFilename,
            ReadWriteMode readWriteMode, string layerName = null)
        {
            return CreateShapeFileFeatureLayer(shapePathFilename, indexPathFilename, readWriteMode, Encoding.Default, layerName);
        }


        public static ShapeFileFeatureLayer CreateShapeFileFeatureLayer(string shapePathFilename, string indexPathFilename,
        ReadWriteMode readWriteMode, Encoding encoding, string layerName = null)
        {
            ShapeFileFeatureLayer shapeFileFeatureLayer = new ShapeFileFeatureLayer();
            ShapeFileFeatureSource shapeFileFeatureSource = FeatureSourceFactory.CreateShapeFileFeatureSource(shapePathFilename, indexPathFilename, readWriteMode, encoding);
            shapeFileFeatureLayer.FeatureSource = shapeFileFeatureSource;
            shapeFileFeatureLayer.Name = layerName;
            shapeFileFeatureLayer.HasBoundingBox = true;
            shapeFileFeatureLayer.IsVisible = true;

            return shapeFileFeatureLayer;
        }

        public static BaseFeatureLayer CreateVectorFeatureLayer(string filePath)
        {
            return new InMemoryFeatureLayer
            {
                //FeatureSource = FeatureSourceFactory.CreateVectorFeatureSource(filePath),
                IsVisible = true
            };
        }
        
    }
}

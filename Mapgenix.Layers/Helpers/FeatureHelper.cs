using Mapgenix.Shapes;
using System.Collections.ObjectModel;


namespace Mapgenix.Layers
{
    /// <summary>Static class for operations on features.</summary>
    public static class FeatureHelper
    {
        
        public static Feature CloneFeatureAndRemoveVertex(Feature feature, Vertex vertex)
        {
            Feature newFeature = new Feature();
            bool deleteSucceed = false;
            WellKnownType wkt = feature.GetWellKnownType();

            switch (wkt)
            {
                case WellKnownType.Line:
                    {
                        LineShape lineShape = (LineShape)feature.GetShape();
                        deleteSucceed = LineShape.RemoveVertex(vertex, lineShape);
                        if (deleteSucceed)
                        {
                            newFeature = new Feature(lineShape.GetWellKnownBinary(), feature.Id, feature.ColumnValues);
                        }
                    }
                    break;
                case WellKnownType.Polygon:
                    {
                        PolygonShape polygonShape = (PolygonShape)feature.GetShape();
                        deleteSucceed = PolygonShape.RemoveVertex(vertex, polygonShape);
                        if (deleteSucceed)
                        {
                            newFeature = new Feature(polygonShape.GetWellKnownBinary(), feature.Id, feature.ColumnValues);
                        }
                    }
                    break;
                case WellKnownType.Multipoint:
                    MultipointShape multipointShape = (MultipointShape)feature.GetShape();
                    deleteSucceed = MultipointShape.RemoveVertex(vertex, multipointShape);
                    if (deleteSucceed)
                    {
                        newFeature = new Feature(multipointShape.GetWellKnownBinary(), feature.Id, feature.ColumnValues);
                    }
                    break;
                case WellKnownType.Multiline:
                    MultilineShape multilineShape = (MultilineShape)feature.GetShape();
                    deleteSucceed = MultilineShape.RemoveVertex(vertex, multilineShape);
                    if (deleteSucceed)
                    {
                        newFeature = new Feature(multilineShape.GetWellKnownBinary(), feature.Id, feature.ColumnValues);
                    }
                    break;
                case WellKnownType.Multipolygon:
                    MultipolygonShape multipolygonShape = (MultipolygonShape)feature.GetShape();
                    deleteSucceed = MultipolygonShape.RemoveVertex(vertex, multipolygonShape);
                    if (deleteSucceed)
                    {
                        newFeature = new Feature(multipolygonShape.GetWellKnownBinary(), feature.Id, feature.ColumnValues);
                    }
                    break;
            }

            return newFeature;
        }

        public static Collection<PointShape> GetAllVerticesFromFeature(Feature feature)
        {
            WellKnownType wellKnowType = feature.GetWellKnownType();
            Collection<PointShape> returnValues = new Collection<PointShape>();

            switch (wellKnowType)
            {
                case WellKnownType.Multipoint:
                    returnValues = GetAllVerticesFromMultipointTypeFeature(feature);
                    break;
                case WellKnownType.Line:
                    returnValues = GetAllVerticesFromLineTypeFeature(feature);
                    break;
                case WellKnownType.Multiline:
                    returnValues = GetAllVerticesFromMultilineTypeFeature(feature);
                    break;
                case WellKnownType.Polygon:
                    returnValues = GetAllVerticesFromPolygonTypeFeature(feature);
                    break;
                case WellKnownType.Multipolygon:
                    returnValues = GetAllVerticesFromMultipolygonTypeFeature(feature);
                    break;
            }

            return returnValues;
        }

        public static Collection<PointShape> GetAllVerticesFromMultipointTypeFeature(Feature feature)
        {
            Collection<PointShape> returnValues = new Collection<PointShape>();

            MultipointShape multiPointShape = feature.GetShape() as MultipointShape;
            foreach (PointShape t in multiPointShape.Points)
            {
                returnValues.Add(t);
            }

            return returnValues;
        }

        public static Collection<PointShape> GetAllVerticesFromLineTypeFeature(Feature feature)
        {
            Collection<PointShape> returnValues = new Collection<PointShape>();

            LineShape lineShape = feature.GetShape() as LineShape;
            foreach (Vertex t in lineShape.Vertices)
            {
                returnValues.Add(new PointShape(t));
            }

            return returnValues;
        }

        public static Collection<PointShape> GetAllVerticesFromMultilineTypeFeature(Feature feature)
        {
            Collection<PointShape> returnValues = new Collection<PointShape>();

            MultilineShape multiLineShape = feature.GetShape() as MultilineShape;
            
            foreach (LineShape lineShape in multiLineShape.Lines)
            {
                foreach (Vertex t in lineShape.Vertices)
                {
                    returnValues.Add(new PointShape(t));
                }
            }

            return returnValues;
        }

        public static Collection<PointShape> GetAllVerticesFromPolygonTypeFeature(Feature feature)
        {
            Collection<PointShape> returnValues = new Collection<PointShape>();

            PolygonShape polygonShape = feature.GetShape() as PolygonShape;
            RingShape outerRing = polygonShape.OuterRing;

            foreach (Vertex t in outerRing.Vertices)
            {
                returnValues.Add(new PointShape(t));
            }

            foreach (RingShape innerRing in polygonShape.InnerRings)
            {
                foreach (Vertex t in innerRing.Vertices)
                {
                    returnValues.Add(new PointShape(t));
                }
            }

            return returnValues;
        }

        public static Collection<PointShape> GetAllVerticesFromMultipolygonTypeFeature(Feature feature)
        {
            Collection<PointShape> returnValues = new Collection<PointShape>();
            MultipolygonShape multiPolygonShape = feature.GetShape() as MultipolygonShape;
            
            foreach (PolygonShape polygonShape in multiPolygonShape.Polygons)
            {
                RingShape outerRing = polygonShape.OuterRing;

                foreach (Vertex t in outerRing.Vertices)
                {
                    returnValues.Add(new PointShape(t));
                }

                foreach (RingShape innerRing in polygonShape.InnerRings)
                {
                    foreach (Vertex t in innerRing.Vertices)
                    {
                        returnValues.Add(new PointShape(t));
                    }
                }
            }
            return returnValues;
        }

        public static Feature CloneFeatureAndMoveVertex(Feature feature, Vertex movingVertex, Vertex targetVertex)
        {
            Feature returnFeature = new Feature();

            WellKnownType wellKnowType = feature.GetWellKnownType();

            switch (wellKnowType)
            {
                case WellKnownType.Multipoint:
                    returnFeature = MoveVertexForMultipointTypeFeature(feature, new PointShape(movingVertex), new PointShape(targetVertex));
                    break;
                case WellKnownType.Line:
                    returnFeature = MoveVertexForLineTypeFeature(feature, new PointShape(movingVertex), new PointShape(targetVertex));
                    break;
                case WellKnownType.Multiline:
                    returnFeature = MoveVertexForMultiLineTypeFeature(feature, new PointShape(movingVertex), new PointShape(targetVertex));
                    break;
                case WellKnownType.Polygon:
                    returnFeature = MoveVertexForPolygonTypeFeature(feature, new PointShape(movingVertex), new PointShape(targetVertex));
                    break;
                case WellKnownType.Multipolygon:
                    returnFeature = MoveVertexForMultipolygonTypeFeature(feature, new PointShape(movingVertex), new PointShape(targetVertex));
                    break;

            }

            return returnFeature;
        }

        public static Feature MoveVertexForMultipointTypeFeature(Feature sourceFeature, PointShape sourceControlPoint, PointShape targetControlPoint)
        {
            MultipointShape originalShape = sourceFeature.GetShape() as MultipointShape;
            originalShape.Id = sourceFeature.Id;

            foreach (PointShape t in originalShape.Points)
            {
                PointShape currentPoint = t;

                if (currentPoint.X == sourceControlPoint.X && currentPoint.Y == sourceControlPoint.Y)
                {
                    t.X = targetControlPoint.X;
                    t.Y = targetControlPoint.Y;
                }
            }

            return new Feature(originalShape, sourceFeature.ColumnValues);
        }

        public static Feature MoveVertexForLineTypeFeature(Feature sourceFeature, PointShape sourceControlPoint, PointShape targetControlPoint)
        {
            LineShape originalShape = sourceFeature.GetShape() as LineShape;
            originalShape.Id = sourceFeature.Id;

            for (int i = 0; i < originalShape.Vertices.Count; i++)
            {
                if (originalShape.Vertices[i].X == sourceControlPoint.X && originalShape.Vertices[i].Y == sourceControlPoint.Y)
                {
                    originalShape.Vertices[i] = new Vertex(targetControlPoint);
                }
            }

            return new Feature(originalShape, sourceFeature.ColumnValues);
        }

        public static Feature MoveVertexForMultiLineTypeFeature(Feature sourceFeature, PointShape sourceControlPoint, PointShape targetControlPoint)
        {
            MultilineShape originalShape = sourceFeature.GetShape() as MultilineShape;
            originalShape.Id = sourceFeature.Id;

            foreach (LineShape t in originalShape.Lines)
            {
                for (int j = 0; j < t.Vertices.Count; j++)
                {
                    Vertex currentVertex = t.Vertices[j];

                    if (currentVertex.X == sourceControlPoint.X && currentVertex.Y == sourceControlPoint.Y)
                    {
                        t.Vertices[j] = new Vertex(targetControlPoint);
                    }
                }
            }

            return new Feature(originalShape, sourceFeature.ColumnValues);
        }

        public static Feature MoveVertexForPolygonTypeFeature(Feature sourceFeature, PointShape sourceControlPoint, PointShape targetControlPoint)
        {
            PolygonShape originalShape = sourceFeature.GetShape() as PolygonShape;
            originalShape.Id = sourceFeature.Id;

            for (int i = 0; i < originalShape.OuterRing.Vertices.Count; i++)
            {
                Vertex currentVertex = originalShape.OuterRing.Vertices[i];

                if (currentVertex.X == sourceControlPoint.X && currentVertex.Y == sourceControlPoint.Y)
                {
                    originalShape.OuterRing.Vertices[i] = new Vertex(targetControlPoint);
                }
            }

            foreach (RingShape t in originalShape.InnerRings)
            {
                for (int j = 0; j < t.Vertices.Count; j++)
                {
                    Vertex currentVertex = t.Vertices[j];

                    if (currentVertex.X == sourceControlPoint.X && currentVertex.Y == sourceControlPoint.Y)
                    {
                        t.Vertices[j] = new Vertex(targetControlPoint);
                    }
                }
            }

            return new Feature(originalShape, sourceFeature.ColumnValues);
        }

        public static Feature MoveVertexForMultipolygonTypeFeature(Feature sourceFeature, PointShape sourceControlPoint, PointShape targetControlPoint)
        {
            MultipolygonShape originalShape = sourceFeature.GetShape() as MultipolygonShape;
            originalShape.Id = sourceFeature.Id;

            foreach (PolygonShape t in originalShape.Polygons)
            {
                for (int i = 0; i < t.OuterRing.Vertices.Count; i++)
                {
                    Vertex currentVertex = t.OuterRing.Vertices[i];

                    if (currentVertex.X == sourceControlPoint.X && currentVertex.Y == sourceControlPoint.Y)
                    {
                        t.OuterRing.Vertices[i] = new Vertex(targetControlPoint);
                    }
                }

                foreach (RingShape t1 in t.InnerRings)
                {
                    for (int i = 0; i < t1.Vertices.Count; i++)
                    {
                        Vertex currentVertex = t1.Vertices[i];

                        if (currentVertex.X == sourceControlPoint.X && currentVertex.Y == sourceControlPoint.Y)
                        {
                            t1.Vertices[i] = new Vertex(targetControlPoint);
                        }
                    }
                }
            }

            return new Feature(originalShape, sourceFeature.ColumnValues);
        }
    }
}

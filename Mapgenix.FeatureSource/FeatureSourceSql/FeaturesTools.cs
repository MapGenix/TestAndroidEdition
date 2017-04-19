using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;

using DotSpatial.Topology.Utilities;
using Npgsql;
using Mapgenix.Shapes;
using Mapgenix.Canvas;


namespace Mapgenix.FeatureSource
{
    public static class FeaturesTools
    {
        public static Collection<Feature> GetFeatures(List<GeoData> dataList, IEnumerable<string> columnNames, SqlFeatureSource self)
        {
            int num = 0;
       
            Collection<Feature> features = new Collection<Feature>();
            Dictionary<string, string> fieldValues = new Dictionary<string, string>();
            List<string> fieldsList = (columnNames == null) ? new List<string>() : new List<string>(columnNames);

            foreach (GeoData data in dataList) 
            {
                string id = null;
                if (data.GetType().GetProperties()[0].Name.Equals(self.FeatureIdColumn, StringComparison.CurrentCultureIgnoreCase))
                {
                    id = data.GetType().GetProperties()[0].GetValue(data,null).ToString();
                    fieldValues.Add(data.GetType().GetProperties()[0].Name, data.GetType().GetProperties()[0].ToString());
                }
                else if (!data.GetType().GetProperties()[0].Name.Equals(self.GeomColumnName, StringComparison.CurrentCultureIgnoreCase))
                {
                    fieldValues.Add(fieldsList[num], data.GetType().GetProperties()[0].GetValue(data, null).ToString());
                }

                if (data.GetType().GetProperties()[0] != null)
                {
                    features.Add(new Feature((byte[])data.GetType().GetProperties()[1].GetValue(data, null), id, fieldValues));
                }
                else
                {
                    features.Add(new Feature(new byte[] { }, id, fieldValues));
                }

                num++;
            }

            return features;
        }

        public static Collection<Feature> GetFeatures(List<GeoData> dataList, SqlFeatureSource self) 
        { 
            Collection<Feature> features = new Collection<Feature>();
            Dictionary<string, string> fieldValues = new Dictionary<string, string>();

            switch (self.DataBaseType) 
            { 
                case(SqlDataBaseType.SqlServer):
                    foreach (GeoData data in dataList)
                    {
                        string id = null;
                        if (data.GetType().GetProperties()[1].Name.Equals(self.FeatureIdColumn, StringComparison.CurrentCultureIgnoreCase))
                        {
                            id = data.GetType().GetProperties()[1].GetValue(data, null).ToString();
                            fieldValues.Add(data.GetType().GetProperties()[1].Name, data.GetType().GetProperties()[1].GetValue(data, null).ToString());
                        }
                        else if (data.GetType().GetProperties()[1].Name.Equals(self.GeomColumnName, StringComparison.CurrentCultureIgnoreCase))
                        {
                            fieldValues.Add(data.GetType().GetProperties()[1].Name, data.GetType().GetProperties()[1].GetValue(data, null).ToString());
                        }

                        if (data.ogr_geometry != null)
                        {
                            features.Add(new Feature((byte[])data.GetType().GetProperties()[2].GetValue(data, null), id, fieldValues));
                        }
                        else
                        {
                            features.Add(new Feature(new byte[] { }, id, fieldValues));
                        }

                        fieldValues = new Dictionary<string, string>();
                    }
                    break;

                case(SqlDataBaseType.PostGis):
                    foreach (GeoData data in dataList)
                    {
                        string id = null;
                        int x = 0;


                        if (int.TryParse(data.GetType().GetProperties()[1].GetValue(data, null).ToString(), out x))
                        {
                            id = data.GetType().GetProperties()[1].GetValue(data, null).ToString();
                            fieldValues.Add(data.GetType().GetProperties()[1].Name, data.GetType().GetProperties()[1].GetValue(data, null).ToString());

                        }
                        else if (data.GetType().GetProperties()[1].Name.Equals(self.GeomColumnName, StringComparison.CurrentCultureIgnoreCase))
                        {
                            fieldValues.Add(data.GetType().GetProperties()[1].Name, data.GetType().GetProperties()[1].GetValue(data, null).ToString());
                        }

                        if ((byte[])data.GetType().GetProperties()[2].GetValue(data, null) != null)
                        {
                            features.Add(new Feature((byte[])data.GetType().GetProperties()[2].GetValue(data, null), id, fieldValues));
                        }
                        else
                        {
                            features.Add(new Feature(new byte[] { }, id, fieldValues));
                        }

                        fieldValues = new Dictionary<string, string>();
                    }
                    break;
            }

            return features;
        }

        public static Collection<Feature> GetFeatures(SqlFeatureSource self, DataTable featDataTable) 
        {
            Collection<Feature> features = new Collection<Feature>();
            Dictionary<string, string> fieldValues = new Dictionary<string, string>();
            List<string> colNames = new List<string>();

            var wkbr = new SqliteReader();
            Int64 id = 0;
            byte[] geom = null;
            
                    
                    foreach(DataColumn c in featDataTable.Columns) 
                    {
                        colNames.Add(c.ColumnName);
                    }

                    foreach(DataRow dataRows in featDataTable.Rows)
                    {
                        fieldValues = new Dictionary<string, string>();
                        foreach (string name in colNames) 
                        {
                            if (name.ToLower().Equals(self.FeatureIdColumn.ToLower()))
                            {
                                if (self.DataBaseType.Equals(SqlDataBaseType.PostGis) || self.DataBaseType.Equals(SqlDataBaseType.SqlServer))
                                {
                                    id = Int64.Parse(dataRows.Field<int>(name).ToString());
                                }
                                else if(self.DataBaseType.Equals(SqlDataBaseType.Oracle))
                                {
                                    id = Convert.ToInt64(dataRows.Field<object>(name));
                                }
                                else
                                {
                                    id = dataRows.Field<Int64>(name);
                                }
                                fieldValues.Add(name, id.ToString());
                            }
                            else if (name.ToLower().Equals(self.GeomColumnName.ToLower()))
                            {
                                if (self.DataBaseType.Equals(SqlDataBaseType.PostGis) || self.DataBaseType.Equals(SqlDataBaseType.SqlServer) 
                                    || self.DataBaseType.Equals(SqlDataBaseType.Oracle))
                                {
                                    geom = dataRows.Field<byte[]>(name);
                                }
                                else
                                {
                                    geom = wkbr.Read(dataRows.Field<byte[]>(name)).ToBinary();
                                }

                                fieldValues.Add(name, geom.ToString());
                            }
                            else 
                            {
                                fieldValues.Add(name, dataRows[name].ToString());
                            }
                        
                        }

                        features.Add(new Feature(geom, id.ToString(), fieldValues));
                    }

           return features;
        }

        public static Collection<Feature> GetFeatures(DataTable featDataTable, string featIdColumn, string geoColumnName, SqlDataBaseType dBType) 
        {
            
            Collection<Feature> features = new Collection<Feature>();
            var wkbr = new SqliteReader();
            byte[] geom = null;
            Int64 id = 0;

            List<string> rowName = new List<string>();


            foreach (DataColumn c in featDataTable.Columns)
            {
                rowName.Add(c.ColumnName);
            }

            foreach (DataRow dataRows in featDataTable.Rows)
            {
                Dictionary<string, string> fieldValues = new Dictionary<string, string>();

                foreach (string name in rowName)
                {

                    if (name.ToLower().Equals(featIdColumn.ToLower()))
                    {
                        if (dBType.Equals(SqlDataBaseType.PostGis) || dBType.Equals(SqlDataBaseType.SqlServer)
                            || dBType.Equals(SqlDataBaseType.Oracle))
                        {
                            id = Int64.Parse(dataRows.Field<int>(0).ToString());
                        }
                        else 
                        {
                            id = dataRows.Field<Int64>(0);
                        }
                       
                        fieldValues.Add(name, id.ToString());
                    }
                    else if (name.ToLower().Equals(geoColumnName.ToLower()))
                    {
                        if (!dBType.Equals(SqlDataBaseType.Sqlite))
                        {
                            geom = dataRows.Field<byte[]>(name);
                        }
                        else
                        {
                            geom = wkbr.Read(dataRows.Field<byte[]>(name)).ToBinary();
                        }

                        fieldValues.Add(name, geom.ToString());
                    }
                    else
                    {
                        fieldValues.Add(name, dataRows[name].ToString());
                    
                    }
                }


                features.Add(new Feature(geom, id.ToString(), fieldValues));
            }

            return features;
        }

        public static byte[] GetCorrectRingOrientationWkb(RectangleShape boundingBox)
        {
            PolygonShape polygon = new PolygonShape();

            polygon.OuterRing.Vertices.Add(new Vertex(boundingBox.UpperLeftPoint.X, boundingBox.UpperLeftPoint.Y));
            polygon.OuterRing.Vertices.Add(new Vertex(boundingBox.UpperLeftPoint.X, boundingBox.LowerRightPoint.Y));
            polygon.OuterRing.Vertices.Add(new Vertex(boundingBox.LowerRightPoint.X, boundingBox.LowerRightPoint.Y));
            polygon.OuterRing.Vertices.Add(new Vertex(boundingBox.LowerRightPoint.X, boundingBox.UpperLeftPoint.Y));
            polygon.OuterRing.Vertices.Add(new Vertex(boundingBox.UpperLeftPoint.X, boundingBox.UpperLeftPoint.Y));

            return polygon.GetWellKnownBinary();
        }

        public static SpatialDataType GetSpatialDataType(string type) 
        {
            switch (type)
            {
                case "geometry":
                    return SpatialDataType.Geometry;
                case "geography":
                    return SpatialDataType.Geography;
                default:
                    return SpatialDataType.Geometry;
            }
        }

        public static DataTable FromListToDataTable<T>(List<T> items) 
        {
            DataTable dataTable = new DataTable(typeof(T).Name);
            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo prop in Props)
            {
                dataTable.Columns.Add(prop.Name);
            }
            foreach (T item in items)
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {
                    values[i] = Props[i].GetValue(item, null);
                }

                dataTable.Rows.Add(values);
            }

            return dataTable;
        }

        public static Collection<Feature> GetFeaturesContains(BaseFeatureSource self, BaseShape targetShape, IEnumerable<string> returningColumnNames)
        {
            return GetFeatures(self, QueryType.Contains, targetShape, returningColumnNames);
        }

        public static Collection<Feature> GetFeaturesCrosses(BaseFeatureSource self, BaseShape targetShape, IEnumerable<string> returningColumnNames)
        {
            return GetFeatures(self, QueryType.Crosses, targetShape, returningColumnNames);
        }

        public static Collection<Feature> GetFeaturesDisjoint(BaseFeatureSource self, BaseShape targetShape, IEnumerable<string> returningColumnNames)
        {
            return GetFeatures(self, QueryType.Disjoint, targetShape, returningColumnNames);
        }

        public static Collection<Feature> GetFeaturesIntersects(BaseFeatureSource self, BaseShape targetShape, IEnumerable<string> returningColumnNames)
        {
            return GetFeatures(self, QueryType.Intersects, targetShape, returningColumnNames);
        }

        public static Collection<Feature> GetFeaturesIsTopologicalEqual(BaseFeatureSource self, BaseShape targetShape, IEnumerable<string> returningColumnNames)
        {
            return GetFeatures(self, QueryType.TopologicalEqual, targetShape, returningColumnNames);
        }

        public static Collection<Feature> GetFeaturesOverlaps(BaseFeatureSource self, BaseShape targetShape, IEnumerable<string> returningColumnNames)
        {
            return GetFeatures(self, QueryType.Overlaps, targetShape, returningColumnNames);
        }

        public static Collection<Feature> GetFeaturesTouches(BaseFeatureSource self, BaseShape targetShape, IEnumerable<string> returningColumnNames)
        {
            return GetFeatures(self, QueryType.Touches, targetShape, returningColumnNames);
        }

        public static Collection<Feature> GetFeaturesWithin(BaseFeatureSource self, BaseShape targetShape, IEnumerable<string> returningColumnNames)
        {
            return GetFeatures(self, QueryType.Within, targetShape, returningColumnNames);
        }

        private static Collection<Feature> GetFeatures(BaseFeatureSource self, QueryType required, BaseShape targetShape, IEnumerable<string> returningColumnNames)
        {
            RectangleShape boundingBox = targetShape.GetBoundingBox();
            Collection<Feature> allPossibleFeatures = self.GetFeaturesByBoundingBox(boundingBox, returningColumnNames);
            Collection<Feature> returnFeatures = new Collection<Feature>();

            switch (required)
            {
                case (QueryType.Contains):
                    foreach (Feature feature in allPossibleFeatures.Where(feature => feature.GetShape().Contains(targetShape)))
                    {
                        returnFeatures.Add(feature);
                    }
                    break;
                case (QueryType.Crosses):
                    foreach (Feature feature in allPossibleFeatures.Where(feature => feature.GetShape().Crosses(targetShape)))
                    {
                        returnFeatures.Add(feature);
                    }
                    break;
                case (QueryType.Disjoint):
                    foreach (Feature feature in allPossibleFeatures.Where(feature => feature.GetShape().IsDisjointed(targetShape)))
                    {
                        returnFeatures.Add(feature);
                    }
                    break;
                case (QueryType.Intersects):
                    foreach (Feature feature in allPossibleFeatures.Where(feature => feature.GetShape().Intersects(targetShape)))
                    {
                        returnFeatures.Add(feature);
                    }
                    break;
                case (QueryType.TopologicalEqual):
                    foreach (Feature feature in allPossibleFeatures.Where(feature => feature.GetShape().IsTopologicallyEqual(targetShape)))
                    {
                        returnFeatures.Add(feature);
                    }
                    break;
                case (QueryType.Overlaps):
                    foreach (Feature feature in allPossibleFeatures.Where(feature => feature.GetShape().Overlaps(targetShape)))
                    {
                        returnFeatures.Add(feature);
                    }
                    break;
                case (QueryType.Touches):
                    foreach (Feature feature in allPossibleFeatures.Where(feature => feature.GetShape().Touches(targetShape)))
                    {
                        returnFeatures.Add(feature);
                    }
                    break;
                case (QueryType.Within):
                    foreach (Feature feature in allPossibleFeatures.Where(feature => feature.GetShape().IsWithin(targetShape)))
                    {
                        returnFeatures.Add(feature);
                    }
                    break;
            }

            return returnFeatures;
        }

        public static Collection<NpgsqlParameter> GetSpatialColumnNames(Feature feature, string featIdCol) 
        {
            Collection<string> spetialColumnNames = new Collection<string>();
            Collection<NpgsqlParameter> parameters = new Collection<NpgsqlParameter>();

            foreach (string columnName in feature.ColumnValues.Keys)
            {
                string columnValue = null;
                if (String.Compare(columnName, featIdCol, true) == 0)
                {
                    columnValue = feature.Id;
                }
                else
                {
                    columnValue = feature.ColumnValues[columnName];
                }
                if (columnValue.Contains(","))
                {
                    spetialColumnNames.Add(columnName);
                }
            }

            foreach (string spetialColumnName in spetialColumnNames)
            {
                parameters.Add(new NpgsqlParameter(spetialColumnName, feature.ColumnValues[spetialColumnName]));
            
            
            }

            return parameters;
        }

        public static BoundingboxGeometry GetSQLiteGeometry(byte[] data) 
        {
            BoundingboxGeometry bounding = new BoundingboxGeometry();
            Stream stream = new MemoryStream(data);

            BinaryReader reader = null;
            var startByte = stream.ReadByte(); 
            var byteOrder = (ByteOrder)stream.ReadByte();

            try
            {
                reader = (byteOrder == ByteOrder.BigEndian) ? new BeBinaryReader(stream) : new BinaryReader(stream);
               
                bounding.srid = reader.ReadInt32();
                bounding.minX = reader.ReadDouble();
                bounding.minY = reader.ReadDouble();
                bounding.maxX = reader.ReadDouble();
                bounding.maxY = reader.ReadDouble();
                bounding.mbrEnd = reader.ReadByte();

                return bounding;

            }
            finally
            {
                reader.Close();
            }

        }

        public static RectangleShape GetBoundingBoxbyFeatures(List<byte[]> data, SqlDataBaseType dataBase) 
        {
            var wkbr = new SqliteReader();
            Collection<RectangleShape> boundingBoxes = new Collection<RectangleShape>();

            if (dataBase.Equals(SqlDataBaseType.PostGis))
            {
                foreach (byte[] x in data)
                {
                    RectangleShape boundingBox = new RectangleShape();
                    boundingBox.LoadFromWellKnownData(x);
                    boundingBoxes.Add(boundingBox);
                }
            }
            else 
            {
                foreach (byte[] x in data)
                {
                    RectangleShape boundingBox = new RectangleShape();
                    boundingBox.LoadFromWellKnownData(wkbr.Read(x).ToBinary());
                    boundingBoxes.Add(boundingBox);
                }
            
            }

            return ExtentHelper.GetBoundingBoxOfItems(boundingBoxes);
        }

        public static Collection<FeatureSourceColumn> GetColumnsCore(DataTable data) 
        {
            Collection<FeatureSourceColumn> columnsCore = new Collection<FeatureSourceColumn>();
            List<string> rowName = new List<string>();

            foreach (DataColumn col in data.Columns)
            {
                rowName.Add(col.ColumnName);
            }

            foreach (DataRow dataRows in data.Rows)
            {
                string colName = string.Empty;
                string coltype = string.Empty;
                int maxlength = 0;

                foreach (string name in rowName)
                {
                    if (name.ToLower().Contains("name"))
                    {
                        colName = dataRows.Field<string>(name);
                    }

                    if (name.ToLower().Contains("type"))
                    {
                        coltype = dataRows.Field<string>(name);
                    }

                    if (name.ToLower().Contains("maxlength"))
                    {
                        maxlength = Convert.ToInt16(dataRows.Field<object>(name));
                    }
                }

                if (!columnsCore.Any(x => x.ColumnName.Equals(colName)))
                {
                    columnsCore.Add(new FeatureSourceColumn(colName, coltype, maxlength));
                }

            }

            return columnsCore;
        }
    }
    
}

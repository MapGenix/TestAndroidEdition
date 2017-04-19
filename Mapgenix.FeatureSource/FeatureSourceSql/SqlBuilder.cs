namespace Mapgenix.FeatureSource
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Globalization;
    using System.Collections.ObjectModel;
    using System.Data;
    using Mapgenix.Shapes;


    public static class SqlBuilder
    {
        public static string GetColumnTypeQuery(string tableName) 
        {
            return "SELECT allTypes.name FROM sys.columns allColumns, sys.types allTypes, sys.all_views allViews " +
                    "WHERE allColumns.object_id=allViews.object_id AND allColumns.user_type_id=allTypes.user_type_id " +
                    "AND allTypes.system_type_id=240 AND allViews.name='" + tableName + "' union SELECT allTypes.name " +
                    "FROM sys.columns allColumns, sys.tables allTables, sys.types allTypes WHERE allColumns.object_id=allTables.object_id " +
                    "AND allColumns.user_type_id=allTypes.user_type_id AND allTypes.system_type_id=240 AND allTables.name='" + tableName + "'";
        }

        public static string GetColumnNameByTable(SpatialDataType spatialDataType, string tableName) 
        {
            return "SELECT allColumns.name FROM sys.columns allColumns, sys.tables allTables, sys.types allTypes " +
                                                  "WHERE allColumns.object_id=allTables.object_id " +
                                                  "AND allColumns.user_type_id=allTypes.user_type_id " +
                                                  "AND allTypes.name='" + spatialDataType + "' " +
                                                  "AND allTables.name='" + tableName + "'";
        }

        public static string GetColumnNameByView(SpatialDataType spatialDataType, string tableName) 
        {

            return "SELECT allColumns.name FROM sys.columns allColumns, sys.all_views allTables, sys.types allTypes " +
                                                                                "WHERE allColumns.object_id=allTables.object_id " +
                                                                                "AND allColumns.user_type_id=allTypes.user_type_id " +
                                                                                "AND allTypes.name='" + spatialDataType + "' " +
                                                                                "AND allTables.name='" + tableName + "'";
        }

        public static string GetGeometryColumnName(string tableName, string schemaName) 
        {
            string sqlStatement = "select pg_attribute.attname from pg_attribute,pg_class,pg_type,pg_namespace where" +
                                   "pg_type.typname='geometry' and pg_attribute.attrelid=pg_class.oid" + 
                                   " and pg_class.relname='" + tableName + "' and pg_attribute.atttypid=pg_type.oid and pg_attribute.attnum>0;";

            if (!string.IsNullOrEmpty(schemaName))
            {
                sqlStatement += string.Format(" and pg_class.relnamespace=pg_namespace.oid and pg_namespace.nspname='{0}'", schemaName);
            }

            return sqlStatement;
        }

        public static string GetGeometryColumnName(SqlDataBaseType dbType, string tableName, string schema) 
        {
            if (dbType == SqlDataBaseType.PostGis) 
            {
                return "select f_geometry_column from geometry_columns where f_table_name='" + tableName + "'";
            }
            else if(dbType == SqlDataBaseType.Sqlite)
            {
                return "SELECT f_geometry_column FROM geometry_columns WHERE f_table_name = '" + tableName.ToLower() + "';";
            }
            else if(dbType == SqlDataBaseType.Oracle)
            {
                return string.Format("SELECT column_name FROM all_tab_columns WHERE table_name = '{0}' AND owner = '{1}' AND DATA_TYPE = 'SDO_GEOMETRY'", tableName.ToUpper(), schema);
            }

            return null;
        }

        public static string GetSrid(SqlDataBaseType dbType, string tableName) 
        {
            if(dbType == SqlDataBaseType.Oracle)
            {
                return string.Format("select srid from user_sdo_geom_metadata where table_name='{0}'", tableName);
            }
            else
            {
                return "select srid from geometry_columns where f_table_name='" + tableName + "'";
            }            
        }

        public static string GetProxCount(string tableName, string schemaName)
        {
            string sqlStatement = "select pg_class.reltuples from pg_class where pg_class.relname='" + tableName + "'";
            
            if (!string.IsNullOrEmpty(schemaName))
            {
                schemaName += string.Format(" and pg_class.relnamespace='{0}'", schemaName);
            }

            return sqlStatement;
        }

        public static string GetGeometryType(SqlFeatureSource self) 
        {
            if (self.DataBaseType.Equals(SqlDataBaseType.SqlServer))
            {
                return string.Format(CultureInfo.InvariantCulture, "SELECT TOP 1 {0}.STAsBinary() FROM {1};", self.GeomColumnName, self.TableName);
            }
            else if (self.DataBaseType.Equals(SqlDataBaseType.Sqlite))
            {
                return "SELECT '" + self.GeomColumnName + "'  FROM '" + self.TableName + "' LIMIT 1;";
            }
            else if (self.DataBaseType.Equals(SqlDataBaseType.Oracle))
            {
                return string.Format("SELECT SDO_UTIL.TO_WKBGEOMETRY (g.{0}) FROM {1} g", self.GeomColumnName, self.TableName);
            }
            else
            {
                string prefix = string.IsNullOrEmpty(self.SchemaName) ? string.Empty : self.SchemaName + ".";
                return string.Format("select AsBinary({0}) from {1} limit 1;", self.GeomColumnName, prefix + self.TableName);
            }            
        }

        public static string GetSpatialQueryCondition(SpatialDataType spatialDataType, int queryType, int srid, string geomColumnName) 
        {
           string sql = null;

               switch (spatialDataType)
               {
                   case SpatialDataType.Geometry:
                       sql = string.Format(CultureInfo.InvariantCulture, "(geometry::STGeomFromWKB(@geometry,{0}).STIntersects({1})=1)", srid, geomColumnName);
                       break;
                   case SpatialDataType.Geography:
                       sql = string.Format(CultureInfo.InvariantCulture, "(geography::STGeomFromText(@geography,{0}).STIntersects({1})=1)", srid, geomColumnName);

                       break;
                   default:
                       sql = geomColumnName;
                       break;
               }

               switch (queryType)
               {
                   case 1:
                       return string.Format(CultureInfo.InvariantCulture, "WHERE {0}", sql);
                   case 2:
                       return string.Format(CultureInfo.InvariantCulture, "WHERE NOT ({0})", sql);
                   default:
                       return null;
               }
           
        }

        public static string GetSpatialQueryCondition(SqlFeatureSource self, int queryType, int srid, string geomColumnName, string parameter) 
        {

            if (self.DataBaseType.Equals(SqlDataBaseType.PostGis))
            {
                switch (queryType)
                {
                    case 1:
                        return "where ST_GeomFromWKB(:" + parameter + "," + srid + ") && " + MakeCaseSensitive(geomColumnName);
                    case 2:
                        return "where not (ST_GeomFromWKB(:" + parameter + "," + srid + ") && " + MakeCaseSensitive(geomColumnName) + ")";
                    default:
                        return null;
                }
            }
            else if(self.DataBaseType.Equals(SqlDataBaseType.Oracle))
            {
                switch (queryType)
                {
                    case 1:
                        return string.Format("WHERE SDO_FILTER({0}, SDO_UTIL.FROM_WKBGEOMETRY(:{1})) = 'TRUE'", geomColumnName, parameter);
                    case 2:
                        return string.Format("WHERE SDO_FILTER({0}, SDO_UTIL.FROM_WKBGEOMETRY(:{1})) <> 'TRUE'", geomColumnName, parameter);
                    default:
                        return null; 
                }
            }

            return null;
                      
        }

        public static string GetSpatialJoinQuery(string featIdColumn, string vitualTableName, RectangleShape boundingBox)
        {
        
            return " AS ft JOIN '"+ vitualTableName +"' AS vt ON ft."+ featIdColumn +" = vt.id WHERE vt.minx < '" + boundingBox.UpperRightPoint.X + "' AND vt.maxx > '" 
                + boundingBox.LowerLeftPoint.X + "' AND vt.miny < '" + boundingBox.UpperRightPoint.Y + "' AND vt.maxy > '" + boundingBox.LowerLeftPoint.Y + "'";
        
        
        }

        public static string BuildSelectColumnsString(IEnumerable<string> columnNames, string featIdColumn, string geomColumnName)
        {
            bool isContainGeometryColumn = false;
            bool isContainFeatureIdColumn = false;
            bool isNotFirstFieldName = false;

            StringBuilder builder = new StringBuilder();
            if (!columnNames.All(x => string.IsNullOrEmpty(x)))
            {
                foreach (string columnName in columnNames)
                {
                    if (isNotFirstFieldName)
                    {
                        builder.Append(",");
                    }

                    if (columnName.Equals(featIdColumn, StringComparison.CurrentCultureIgnoreCase))
                    {
                        isContainFeatureIdColumn = true;
                        builder.Append(columnName);
                    }
                    else if (!columnName.Equals(geomColumnName, StringComparison.CurrentCultureIgnoreCase))
                    {
                        builder.Append(columnName);
                    }
                    else
                    {
                        isContainGeometryColumn = true;
                        builder.Append(string.Format(CultureInfo.InvariantCulture, "{0}.STAsBinary() as {1}", columnName, geomColumnName));
                    }

                    isNotFirstFieldName = true;
                }
            }

            if (!isContainGeometryColumn)
            {
                if (builder.Length != 0)
                {
                    builder.Append(",");
                }
                builder.Append(string.Format(CultureInfo.InvariantCulture, "{0}.STAsBinary() as {1}", geomColumnName, geomColumnName));
            }

            if (!isContainFeatureIdColumn && !string.IsNullOrEmpty(featIdColumn))
            {
                if (builder.Length != 0)
                {
                    builder.Append(",");
                }
                builder.Append(string.Format(CultureInfo.InvariantCulture, "{0}", featIdColumn));
            }

            return builder.ToString();
        }

        public static string BuildSelectColumnsStringPost(IEnumerable<string> columnNames, string featIdColumn, string geomColumnName) 
        {
            List<string> fieldsList = (columnNames == null) ? new List<string>() : new List<string>(columnNames);
            fieldsList.Add(geomColumnName);

            bool isContainGeometryColumn = false;
            bool isContainFeatureIdColumn = false;
            bool isNotFirstFieldName = false;

            StringBuilder builder = new StringBuilder();

            if (!fieldsList.All(x => string.IsNullOrEmpty(x)))
            {
                foreach (string columnName in fieldsList)
                {
                    if (isNotFirstFieldName) { builder.Append(","); }

                    if (columnName.Equals(featIdColumn, StringComparison.CurrentCultureIgnoreCase))
                    {
                        isContainFeatureIdColumn = true;
                        builder.Append(MakeCaseSensitive(columnName));
                    }
                    else if (!columnName.Equals(geomColumnName, StringComparison.CurrentCultureIgnoreCase))
                    {
                        builder.Append(MakeCaseSensitive(columnName));
                    }
                    else
                    {
                        isContainGeometryColumn = true;
                        builder.Append(string.Format(CultureInfo.InvariantCulture, "ST_AsBinary({0}) as " + MakeCaseSensitive(columnName), MakeCaseSensitive(columnName), MakeCaseSensitive(geomColumnName)));
                    }

                    isNotFirstFieldName = true;
                }
            }

            if (!isContainGeometryColumn)
            {
                if (builder.Length != 0) { builder.Append(","); }
                builder.Append(string.Format(CultureInfo.InvariantCulture, "ST_AsBinary({0}) as " + MakeCaseSensitive(geomColumnName), MakeCaseSensitive(geomColumnName), MakeCaseSensitive(geomColumnName)));
            }

            if (!isContainFeatureIdColumn && !string.IsNullOrEmpty(featIdColumn))
            {
                if (builder.Length != 0) { builder.Append(","); }
                builder.Append(featIdColumn + " ");
            }

            return builder.ToString();
        }

        public static string BuildSelectColumnsStringSqlite(IEnumerable<string> columnsName) 
        {
            StringBuilder query = new StringBuilder();

            foreach (string name in columnsName)
            {
                query.Append(name + ",");
            }

            query.Remove(query.Length - 1, 1);
 
            return query.ToString();
        }

        public static string BuildSelectColumnsStringSqlite(IEnumerable<string> columnsName, SqlFeatureSource self)
        {
            StringBuilder query = new StringBuilder();

            if (!columnsName.Contains(self.FeatureIdColumn) && !columnsName.Contains(self.GeomColumnName)) 
            {
                query.Append(self.FeatureIdColumn + "," + self.GeomColumnName + ",");
            }
                
            foreach (string name in columnsName)
            {
                query.Append(name + ",");
            }

            query.Remove(query.Length - 1, 1);

            return query.ToString();
        }

        public static string BuildSelectColumnsStringOracle(IEnumerable<string> columnNames, string featIdColumn, string geomColumnName, bool forTempTable)
        {
            List<string> fieldsList = (columnNames == null) ? new List<string>() : new List<string>(columnNames);

            bool isContainGeometryColumn = false;
            bool isContainFeatureIdColumn = false;
            bool isNotFirstFieldName = false;

            StringBuilder builder = new StringBuilder();

            if (!fieldsList.All(x => string.IsNullOrEmpty(x)))
            {
                foreach (string columnName in fieldsList)
                {
                    if (isNotFirstFieldName) { builder.Append(","); }

                    if (columnName.Equals(featIdColumn, StringComparison.CurrentCultureIgnoreCase))
                    {
                        isContainFeatureIdColumn = true;
                        builder.Append(columnName);
                    }
                    else if (!columnName.Equals(geomColumnName, StringComparison.CurrentCultureIgnoreCase))
                    {
                        builder.Append(columnName);
                    }
                    else
                    {
                        isContainGeometryColumn = true;
                        if(forTempTable)
                            builder.Append(string.Format(CultureInfo.InvariantCulture, "SDO_UTIL.TO_WKBGEOMETRY({0}) as B{0}, GEOM", columnName));
                        else
                            builder.Append(string.Format(CultureInfo.InvariantCulture, "B{0} AS GEOM", columnName));
                    }

                    isNotFirstFieldName = true;
                }
            }

            if (!isContainGeometryColumn)
            {
                if (builder.Length != 0) { builder.Append(","); }
                if(forTempTable)
                    builder.Append(string.Format(CultureInfo.InvariantCulture, "SDO_UTIL.TO_WKBGEOMETRY({0}) as B{0}, GEOM", geomColumnName));
                else
                    builder.Append(string.Format(CultureInfo.InvariantCulture, "B{0} AS GEOM", geomColumnName));
            }

            if (!isContainFeatureIdColumn && !string.IsNullOrEmpty(featIdColumn))
            {
                if (builder.Length != 0) { builder.Append(","); }
                builder.Append(featIdColumn + " ");
            }

            return builder.ToString();
        }

        public static string GetStringQuery(IEnumerable<string> columnNames, SqlFeatureSource self, string condition) 
        {
            string sqlColumns = string.Empty;

            if (self.DataBaseType.Equals(SqlDataBaseType.SqlServer))
            {
                sqlColumns = BuildSelectColumnsString(columnNames, self.FeatureIdColumn, self.GeomColumnName);
                return GetStringQuery(sqlColumns, self.TableName, condition);
            }
            else if (self.DataBaseType.Equals(SqlDataBaseType.PostGis))
            {
                sqlColumns = BuildSelectColumnsStringPost(columnNames, self.FeatureIdColumn, self.GeomColumnName);
                return GetStringQuery(sqlColumns, self.TableName, self.SchemaName, condition);
            }
            else if(self.DataBaseType.Equals(SqlDataBaseType.Oracle))
            {
                sqlColumns = BuildSelectColumnsStringOracle(columnNames, self.FeatureIdColumn, self.GeomColumnName, false);
                string sql = string.Empty;
                if (self.CreateTempTable)
                    sql = GetStringQuery(sqlColumns, "temp_" + self.TableName, condition);
                else
                    sql = GetStringQuery(sqlColumns, self.TableName, condition);

                if (sql.Substring(sql.Length - 1).Equals(";"))
                    return sql.Substring(0, sql.Length - 1);
                else
                    return sql;
            }
            else 
            {                  
                sqlColumns = BuildSelectColumnsStringSqlite(columnNames, self);
                return sqlColumns = GetStringQuery(sqlColumns, self.TableName, condition);
            }
           
        }

        public static string GetStringQuery(string columns, string tableName, string condition)
        {
            if (string.IsNullOrEmpty(condition)) 
            {
                return "SELECT " + columns + " FROM " + tableName + ";";
            }
            else if (string.IsNullOrEmpty(columns) && string.IsNullOrEmpty(condition))
            {
                return "SELECT * FROM " + tableName + ";";
            }
            else if (string.IsNullOrEmpty(columns))
            {
                if (condition.Contains("WHERE"))
                {
                    return "SELECT * FROM " + tableName + " WHERE " + condition + ";";
                }
                else 
                {
                    return "SELECT * FROM " + tableName +" "+ condition + ";";
                }
                
            }
            else 
            {
                if (condition.Contains("WHERE"))
                {
                    return "SELECT " + columns + " FROM " + tableName + " " + condition + "";
                }
                else
                {
                    return "SELECT " + columns + " FROM " + tableName + " WHERE " + condition + ";";
                }
            }
        }

        public static string GetStringQuery(string columns, string tableName, string schemaName, string condition)
        {
            string prefix = string.IsNullOrEmpty(schemaName) ? string.Empty : schemaName + ".";
            return "select " + columns + " from " + prefix + MakeCaseSensitive(tableName) + " " + condition + ";";
            
        }

        public static string IsTableExist(string schemaName, string tableName, SqlDataBaseType dbType)
        {
            string prefix = string.IsNullOrEmpty(schemaName) ? string.Empty : schemaName + ".";

            if (dbType.Equals(SqlDataBaseType.SqlServer))
            {
                return string.Format(CultureInfo.InvariantCulture, "SELECT ISNULL(OBJECTPROPERTY(OBJECT_ID('{0}'), 'IsUserTable'), 0)", tableName);
            }
            else if (dbType.Equals(SqlDataBaseType.Sqlite))
            {
                return string.Format(CultureInfo.InvariantCulture, "SELECT count(*) FROM sqlite_master WHERE type = 'table' AND name LIKE '{0}'", tableName);
            }
            else if (dbType.Equals(SqlDataBaseType.Oracle))
            {
                return string.Format(CultureInfo.InvariantCulture, "select COUNT(*) from all_objects where object_type in ('TABLE') and object_name = '{0}' AND owner = '{1}'", tableName.ToUpper(), schemaName.ToUpper());
            }
            else 
            {
                return string.Format(CultureInfo.InvariantCulture, "select exists(select * from information_schema.tables where table_name = '{0}')", prefix + tableName);
            }
            
        }

        public static string DropTable(string tableName) 
        { 
            return "DROP TABLE " + tableName;
        }

        public static string CreateTable(string tableName, SpatialDataType spatialDataType, IEnumerable<FeatureSourceColumn> columns) 
        {
            StringBuilder sqlStatement = new StringBuilder(string.Format(CultureInfo.InvariantCulture, "CREATE TABLE {0} (id int primary key identity(1,1), {1} {2}", tableName, "geom", spatialDataType));
            
            return BuildSelectColumnsString(columns, sqlStatement);
        }

        private static string BuildSelectColumnsString(IEnumerable<FeatureSourceColumn> columns, StringBuilder sqlStatement) 
        {
            foreach (var item in columns)
            {
                if (string.IsNullOrEmpty(item.TypeName))
                {
                    sqlStatement.AppendFormat(CultureInfo.InvariantCulture, ", {0} varchar(255)", item.ColumnName);
                }
                else
                {
                    if (item.MaxLength != 0)
                        sqlStatement.AppendFormat(CultureInfo.InvariantCulture, ", {0} {1}({2})", item.ColumnName, item.TypeName, item.MaxLength);
                    else
                        sqlStatement.AppendFormat(CultureInfo.InvariantCulture, ", {0} {1}", item.ColumnName, item.TypeName);
                }
            }

            sqlStatement.AppendFormat(CultureInfo.InvariantCulture, ")");

            return sqlStatement.ToString().Replace(";", " ");
        }

        public static string DropIndexes(Collection<string> indexNames, string tableName) 
        {
            string sqlStatement = "DROP INDEX";

            for (int i = 0; i < indexNames.Count; i++)
            {
                if (i != 0)
                {
                    sqlStatement += ",";
                }

                sqlStatement += " " + indexNames[i] + " ON " + tableName;
            }

            sqlStatement += ";";

            return null;
        }

        public static string GetIndexName(string tableName) 
        {
            return string.Format(CultureInfo.InvariantCulture, "SELECT sys.indexes.name as name from sys.indexes, sys.tables where sys.tables.name='{0}' AND sys.tables.object_id=sys.indexes.object_id AND sys.indexes.type_desc='SPATIAL';", tableName); 
        }

        public static string GetBuildIndex(string tableName, string geomColumnName, RectangleShape boundingBox) 
        {
            if (boundingBox != null) 
            {
                return string.Format(CultureInfo.InvariantCulture, 
                    "CREATE SPATIAL INDEX geom_sidx ON {0}({1}) WITH (BOUNDING_BOX=({2},{3},{4},{5}),GRIDS=(LOW,LOW,MEDIUM,HIGH));", tableName, geomColumnName, boundingBox.UpperLeftPoint.X, boundingBox.LowerRightPoint.Y, boundingBox.LowerRightPoint.X, boundingBox.UpperLeftPoint.Y);
            }
            else 
            {
                return string.Format(CultureInfo.InvariantCulture, 
                    "CREATE SPATIAL INDEX geom_sidx ON {0}({1}) WITH (GRIDS=(LOW,LOW,MEDIUM,HIGH));", tableName, geomColumnName);
            }
        }

        public static string IsIndexExist(string tableName)
        {
            return string.Format(CultureInfo.InvariantCulture, 
                "SELECT sys.tables.name from sys.spatial_index_tessellations, sys.tables where sys.tables.name='{0}' AND sys.tables.object_id=sys.spatial_index_tessellations.object_id", tableName);
        }

        public static string GetStringQuery(string sqlStatement, string whereClause,ExecutingSqlStatementType sqlStatementType) 
        {
            if (sqlStatementType != ExecutingSqlStatementType.GetSpatialDataType
               && sqlStatementType != ExecutingSqlStatementType.GetColumns
               && sqlStatementType != ExecutingSqlStatementType.BuildIndex)
            {
                if (!string.IsNullOrEmpty(whereClause))
                {
                    sqlStatement = sqlStatement.Replace(';', ' ');
                    if (sqlStatement.ToUpper(CultureInfo.InvariantCulture).Contains("WHERE"))
                    {
                        sqlStatement = sqlStatement + " " + whereClause.ToUpper(CultureInfo.InvariantCulture).Replace("WHERE", "AND");
                    }
                    else
                    {
                        sqlStatement = sqlStatement + " " + whereClause;
                    }
                    sqlStatement += ";";
                }

            }
            return sqlStatement;
        }

        public static string GetBoundingBoxQueryByGeometry(SqlFeatureSource self)
        {
            StringBuilder sqlStatement = new StringBuilder();

            //TODO: need make sure geometry here what the different with geography
            // TODO at least, SqlGeography doesn't have STEnvelope() function
            sqlStatement.Append("DECLARE @polygon01 geometry;");
            sqlStatement.Append("DECLARE @polygon02 geometry;");

            sqlStatement.Append(string.IsNullOrEmpty(self.WhereClause)
                ? string.Format(CultureInfo.InvariantCulture, "DECLARE mycursor cursor for select {0} from {1};",
                    self.GeomColumnName, self.TableName)
                : string.Format(CultureInfo.InvariantCulture, "DECLARE mycursor cursor for select {0} from {1} {2};",
                     self.GeomColumnName, self.TableName, self.WhereClause));

            sqlStatement.Append("open mycursor;");
            sqlStatement.Append("while(@polygon01 is null)");
            sqlStatement.Append("fetch mycursor into @polygon01;");
            sqlStatement.Append("while(@polygon02 is null)");
            sqlStatement.Append("fetch mycursor into @polygon02;");
            sqlStatement.Append("if (@polygon02 Is not NULL) ");
            sqlStatement.Append("begin ");
            sqlStatement.Append("set @polygon01 = @polygon01.STUnion(@polygon02.STEnvelope()).STEnvelope();");
            sqlStatement.Append("fetch mycursor into @polygon02;");
            sqlStatement.Append("while @@FETCH_STATUS = 0 ");
            sqlStatement.Append("begin ");
            sqlStatement.Append("if(@polygon02 is not null)");
            sqlStatement.Append("set @polygon01 = @polygon01.STUnion(@polygon02.STEnvelope()).STEnvelope();");
            sqlStatement.Append("fetch mycursor into @polygon02;");
            sqlStatement.Append("end;");
            sqlStatement.Append("end;");
            sqlStatement.Append("SELECT @polygon01.STAsBinary();");
            sqlStatement.Append("CLOSE myCursor;");
            sqlStatement.Append("DEALLOCATE myCursor;");

            return sqlStatement.ToString();
        }

        public static string GetBoundingBoxQuery(SqlFeatureSource self) 
        { 
            if(self.DataBaseType.Equals(SqlDataBaseType.PostGis))
            {
                return "SELECT  ST_AsBinary(" + MakeCaseSensitive(self.GeomColumnName) + ") FROM " + self.TableName + ";";
            }
            else if(self.DataBaseType.Equals(SqlDataBaseType.Oracle))
            {
                return string.Format("SELECT SDO_TUNE.EXTENT_OF('{0}', '{1}').get_wkt() AS bbox FROM dual", self.TableName.ToUpper(), self.GeomColumnName.ToUpper() );
            }
            else 
            {
                return "SELECT " + self.GeomColumnName + " FROM '" + self.TableName + "';";
            }
        }

        public static string GetBoundingBoxQueryByGeography(SqlFeatureSource self)
        {
            StringBuilder sqlStatement = new StringBuilder();

            sqlStatement.Append("DECLARE @geography1 geography;");
            sqlStatement.Append("DECLARE @geography2 geography;");
            sqlStatement.Append("DECLARE @polygon01 geometry;");
            sqlStatement.Append("DECLARE @polygon02 geometry;");
            if (string.IsNullOrEmpty(self.WhereClause))
            {
                sqlStatement.AppendFormat(CultureInfo.InvariantCulture, "DECLARE mycursor cursor for select {0} from {1};", self.GeomColumnName, self.TableName);
            }
            else
            {
                sqlStatement.AppendFormat(CultureInfo.InvariantCulture, "DECLARE mycursor cursor for select {0} from {1} {2};", self.GeomColumnName, self.TableName, self.WhereClause);
            }
            sqlStatement.Append("open mycursor;");
            sqlStatement.Append("fetch mycursor into @geography1;");
            sqlStatement.Append("fetch mycursor into @geography2;");
            sqlStatement.AppendFormat(CultureInfo.InvariantCulture, "set @polygon01 = geometry::STGeomFromWKB(@geography1.STAsBinary(),{0});", self.Srid);
            sqlStatement.AppendFormat(CultureInfo.InvariantCulture, "set @polygon02 = geometry::STGeomFromWKB(@geography2.STAsBinary(),{0});", self.Srid);
            sqlStatement.Append("if (@polygon02 Is not NULL) ");
            sqlStatement.Append("begin ");
            sqlStatement.Append("set @polygon01 = @polygon01.STUnion(@polygon02.STEnvelope()).STEnvelope();");
            sqlStatement.Append("fetch mycursor into @geography2;");
            sqlStatement.Append("while @@FETCH_STATUS = 0 ");
            sqlStatement.Append("begin ");
            sqlStatement.AppendFormat(CultureInfo.InvariantCulture, "set @polygon02 = geometry::STGeomFromWKB(@geography2.STAsBinary(),{0});", self.Srid);
            sqlStatement.Append("set @polygon01 = @polygon01.STUnion(@polygon02.STEnvelope()).STEnvelope();");
            sqlStatement.Append("fetch mycursor into @geography2;");
            sqlStatement.Append("end;");
            sqlStatement.Append("end;");
            sqlStatement.Append("SELECT @polygon01.STAsBinary();");
            sqlStatement.Append("CLOSE myCursor;");
            sqlStatement.Append("DEALLOCATE myCursor;");

            return sqlStatement.ToString();
        }

        public static string GetSqlQueryToValidate(SqlFeatureSource self) 
        {
            return string.Format(CultureInfo.InvariantCulture, "SELECT TOP 1 {0} from {1} where {2}.STIsValid()=0;", self.FeatureIdColumn, self.TableName, self.GeomColumnName);
        
        }

        public static string GetSqlQueryToValidate(string tableName, string geomColumnName) 
        {
            return string.Format(CultureInfo.InvariantCulture, "SELECT * from {0} where {1}.STIsValid()=0;", tableName, geomColumnName);
        }

        public static string GetValidGeometryQuery(string tableName, string geomColumnName) 
        {
            return string.Format(CultureInfo.InvariantCulture, "UPDATE {0} SET {1}={1}.MakeValid() WHERE {1}.STIsValid()=0;", tableName, geomColumnName);
        }

        public static string GetQueryToColumsCore(SqlFeatureSource self) 
        {
            if (self.DataBaseType.Equals(SqlDataBaseType.SqlServer))
            {
                return string.Format(CultureInfo.InvariantCulture, "SELECT allColumns.name as Name, allTypes.name as Type, allTypes.max_length as MaxLength " +
                    "FROM sys.columns allColumns, sys.tables allTables, sys.types allTypes" +
                    " WHERE allColumns.object_id=allTables.object_id AND allColumns.user_type_id=allTypes.user_type_id AND allTables.name='{0}';", self.TableName);

            }
            else if (self.DataBaseType.Equals(SqlDataBaseType.PostGis))
            {

                string prefix = string.IsNullOrEmpty(self.SchemaName) ? string.Empty : self.SchemaName + ".";
                string sqlStatement = "select pg_attribute.attname as Name, pg_type.typname as Type, pg_attribute.attlen"+
                    " as MaxLength from pg_attribute,pg_class,pg_type,pg_namespace where pg_attribute.attrelid=pg_class.oid and pg_class.relname='" + self.TableName + 
                    "' and pg_attribute.atttypid=pg_type.oid and pg_attribute.attnum>0";

                if (!string.IsNullOrEmpty(self.SchemaName))
                {
                    sqlStatement += string.Format(" and pg_class.relnamespace=pg_namespace.oid and pg_namespace.nspname='{0}'", self.SchemaName);
                }

                return sqlStatement;
            }
            else if (self.DataBaseType.Equals(SqlDataBaseType.Sqlite))
            {
                return "PRAGMA table_info('" + self.TableName + "');";

            }
            else if(self.DataBaseType.Equals(SqlDataBaseType.Oracle))
            {
                return string.Format("SELECT column_name \"Name\", data_type \"Type\", CAST(data_length AS integer) \"MaxLength\"  FROM all_tab_columns WHERE table_name = '{0}' and owner = '{1}'", self.TableName.ToUpper(), self.SchemaName.ToUpper());
            }
            else
            {
                return string.Empty;
            }
            
        }

        public static string GetFeaturesByIdsCore(IEnumerable<string> ids, IEnumerable<string> columnsName, SqlFeatureSource self) 
        {
            StringBuilder builder = new StringBuilder();
            string columns = string.Empty;
           

            if (self.DataBaseType == SqlDataBaseType.SqlServer)
            {
                builder.Append(self.FeatureIdColumn + "='" + ids.ToList()[0] + "'");

                if (ids.Count() > 1)
                {
                    for (int i = 0; i < ids.Count(); i++)
                    {
                       
                       builder.Append(" OR " + self.FeatureIdColumn + "='" + ids.ToList()[i] + "'");

                    }
                }
                builder.Append(";");
            }
            else if (self.DataBaseType == SqlDataBaseType.PostGis)
            {
                builder.Append(" where " + MakeCaseSensitive(self.FeatureIdColumn) + " in ('" + ids.ToList()[0] + "'");
                if (ids.Count() > 1)
                {
                    builder.Append(",");
                }

                for (int i = 1; i < ids.Count(); i++)
                {
                    builder.Append("'" + ids.ToList()[i] + "'");
                    if (i != ids.Count() - 1)
                    {
                        builder.Append(",");
                    }
                }
                builder.Append(")");
            }
            else if(self.DataBaseType == SqlDataBaseType.Oracle)
            {
                builder.Append(self.FeatureIdColumn + " in ('" + ids.ToList()[0] + "'");
                if (ids.Count() > 1)
                {
                    builder.Append(",");
                }

                for (int i = 1; i < ids.Count(); i++)
                {
                    builder.Append("'" + ids.ToList()[i] + "'");
                    if (i != ids.Count() - 1)
                    {
                        builder.Append(",");
                    }
                }
                builder.Append(")");
            }
            else 
            {
                int countIds = 0;

                foreach (string id in ids)
                {
                    countIds++;
                    builder.Append( self.FeatureIdColumn + " = '" + id + "' ");

                    if (!ids.Count().Equals(countIds))
                    {
                        builder.Append("OR ");
                    }
                }
      
            }

            return GetStringQuery(columnsName, self, builder.ToString()); 
         }

        public static string GetFeaturesByIdsCore(IEnumerable<string> ids, SqlFeatureSource self)
        {
            StringBuilder builder = new StringBuilder();
            int countIds = 0;

            builder.Append("SELECT '" + self.FeatureIdColumn + "', '" + self.GeomColumnName + "' FROM '" + self.TableName + "' ");

            if (!ids.Count().Equals(0))
            {
                builder.Append("WHERE ");

                foreach (string id in ids) 
                {
                    countIds++;
                    builder.Append("'" + self.FeatureIdColumn + "' = '" + id + "' ");

                    if (!ids.Count().Equals(countIds))
                    {
                        builder.Append("OR ");
                    }
                }

                builder.Append(";");
            }
            else
            {
                builder.Append(";");
            }

            return builder.ToString();
        }

        public static string GetUpdateFeatureQuery(SqlFeatureSource self, SpatialDataType spatialDataType, string id, Feature feature) 
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append(string.Format(CultureInfo.InvariantCulture, "UPDATE {0} SET {1}={2}::STGeomFromWKB(@{2},{3})", self.TableName, self.GeomColumnName, spatialDataType, self.Srid));
            if (feature.ColumnValues != null)
            {
                foreach (string columnName in feature.ColumnValues.Keys.Where(columnName => String.Compare(columnName, self.GeomColumnName, true, CultureInfo.InvariantCulture) != 0))
                {
                    if (String.Compare(columnName, self.FeatureIdColumn, true, CultureInfo.InvariantCulture) == 0)
                    {
                        stringBuilder.Append("," + columnName + "='" + feature.Id + "'");
                    }
                    else
                    {
                        stringBuilder.Append("," + columnName + "='" + feature.ColumnValues[columnName] + "'");
                    }
                }
            }
            stringBuilder.Append(" WHERE " + self.FeatureIdColumn + "='" + id + "'");
            stringBuilder.Append(";");
 

         return stringBuilder.ToString();;
        }

        public static string GetUpdateFeatureQuery(SqlFeatureSource self, Feature feature, string geomColumnName, string id, string paramName) 
        {
            StringBuilder stringBuilder = new StringBuilder();
            string prefix = string.IsNullOrEmpty(self.SchemaName) ? string.Empty : self.SchemaName + ".";
            stringBuilder.Append("update " + prefix + MakeCaseSensitive(self.TableName) + " set ");

            stringBuilder.Append(MakeCaseSensitive( geomColumnName) + "=" + "ST_GeomFromWKB(:" + paramName + "," + self.Srid + ")");

            foreach (string columnName in feature.ColumnValues.Keys)
            {
                if (String.Compare(columnName, geomColumnName, true) == 0)
                {
                    continue;
                }
                string columnValue = null;
                if (String.Compare(columnName, self.FeatureIdColumn, true) == 0)
                {
                    columnValue = feature.Id;
                }
                else
                {
                    columnValue = feature.ColumnValues[columnName];
                }
                if (columnValue.Contains("'"))
                {
                    stringBuilder.Append("," + MakeCaseSensitive(columnName) + "=:" + columnName);
                }
                else
                {
                    stringBuilder.Append("," + MakeCaseSensitive(columnName) + "='" + columnValue + "'");
                }
            }
            stringBuilder.Append(" where " + MakeCaseSensitive(self.FeatureIdColumn) + "='" + id + "'");
            stringBuilder.Append(";");

            return stringBuilder.ToString();
        }

        public static string GetUpdateFeatureQuery(SqlFeatureSource self, string geomColumnName, string paramName, string id) 
        {
            StringBuilder builder = new StringBuilder();

            builder.Append("UPDATE '" + self.TableName + "' SET '" + geomColumnName + "' = '" + paramName + "' WHERE " + self.FeatureIdColumn + " = '"+ id +"' ;");


            return builder.ToString();
        }

        public static string GetInsertFeatureQuery(SqlFeatureSource self, Feature feature, SpatialDataType spatialDataType) 
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append(string.Format(CultureInfo.InvariantCulture, "INSERT INTO {0} ({1}", self.TableName, self.GeomColumnName));

            if (feature.ColumnValues != null)
            {
                foreach (string columnName in feature.ColumnValues.Keys.Where(columnName => String.Compare(columnName, self.GeomColumnName, true, CultureInfo.InvariantCulture) != 0))
                {
                    stringBuilder.Append("," + columnName);
                }
            }

            stringBuilder.Append(string.Format(CultureInfo.InvariantCulture, ") VALUES({0}::STGeomFromWKB(@{0},{1})", spatialDataType, self.Srid));
            if (feature.ColumnValues != null)
            {
                foreach (string columnName in feature.ColumnValues.Keys.Where(columnName => String.Compare(columnName, self.GeomColumnName, true, CultureInfo.InvariantCulture) != 0))
                {
                    if (String.Compare(columnName, self.FeatureIdColumn, true, CultureInfo.InvariantCulture) == 0)
                    {
                        stringBuilder.Append(",'" + feature.Id + "'");
                    }
                    else
                    {
                        stringBuilder.Append(",'" + feature.ColumnValues[columnName] + "'");
                    }
                }
            }
            stringBuilder.Append(");");

            return stringBuilder.ToString();
        }

        public static string GetInsertFeatureQuery(SqlFeatureSource self, Feature feature, string parameterName, string geomFieldName, Collection<FeatureSourceColumn> featSourceCols) 
        {
            StringBuilder builder = new StringBuilder();
            string prefix = string.IsNullOrEmpty(self.SchemaName) ? string.Empty : self.SchemaName + ".";
            builder.Append(string.Format(CultureInfo.InvariantCulture, "insert into {0} ({1}", prefix + MakeCaseSensitive(self.TableName), MakeCaseSensitive(geomFieldName)));

            foreach (string columnName in feature.ColumnValues.Keys)
            {
                if (String.Compare(columnName, geomFieldName, true) == 0)
                {
                    continue;
                }
                builder.Append("," + MakeCaseSensitive(columnName));
            }

            builder.Append(") values(ST_GeomFromWKB(:" + parameterName + "," + self.Srid + ")");

            foreach (string columnName in feature.ColumnValues.Keys)
            {
                if (String.Compare(columnName, geomFieldName, true) == 0)
                {
                    continue;
                }
                string columnValue = null;
                if (String.Compare(columnName, self.FeatureIdColumn, true) == 0)
                {
                    columnValue = feature.Id;
                }
                else
                {
                    columnValue = feature.ColumnValues[columnName];
                }
                if (columnValue.Contains(","))
                {

                    builder.Append(",:" + columnName);
                }
                else
                {
                    foreach (FeatureSourceColumn featureSourceColumn in featSourceCols)
                    {
                        if (string.CompareOrdinal(featureSourceColumn.ColumnName, columnName) == 0)
                        {
                            if (string.CompareOrdinal(featureSourceColumn.TypeName, "int8") == 0)
                            {
                                builder.Append(",null");
                            }
                            else
                            {
                                builder.Append(",'" + columnValue + "'");
                            }
                        }
                    }
                }
            }
            builder.Append(");");

           return builder.ToString();
        }

        public static string GetInsertFeatureQuery(SqlFeatureSource self, string GeomParameterName, Feature feature) 
        {
            StringBuilder builder = new StringBuilder();

            builder.Append("INSERT INTO '" + self.TableName + "' ('" + self.GeomColumnName + "') VALUES (" + GeomParameterName + ");");
           
            return builder.ToString();
        }

        public static string GetInsertFeatureQueryOracle(SqlFeatureSource self, Feature feature, string parameterName, string geomFieldName)
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append(string.Format(CultureInfo.InvariantCulture, "INSERT INTO {0} ({1}", self.TableName, self.GeomColumnName));

            if (feature.ColumnValues != null)
            {
                foreach (string columnName in feature.ColumnValues.Keys.Where(columnName => String.Compare(columnName, self.GeomColumnName, true, CultureInfo.InvariantCulture) != 0))
                {
                    stringBuilder.Append("," + columnName);
                }
            }

            stringBuilder.Append(string.Format(") values(SDO_CS.TRANSFORM(SDO_UTIL.FROM_WKBGEOMETRY(:{0}),{1})", parameterName, self.Srid));
            if (feature.ColumnValues != null)
            {
                foreach (string columnName in feature.ColumnValues.Keys.Where(columnName => String.Compare(columnName, self.GeomColumnName, true, CultureInfo.InvariantCulture) != 0))
                {
                    if (String.Compare(columnName, self.FeatureIdColumn, true, CultureInfo.InvariantCulture) == 0)
                    {
                        stringBuilder.Append(",'" + feature.Id + "'");
                    }
                    else
                    {
                        stringBuilder.Append(",'" + feature.ColumnValues[columnName] + "'");
                    }
                }
            }
            stringBuilder.Append(");");

            return stringBuilder.ToString();
        }
        
        public static string GetDeleteQuery(SqlFeatureSource self, Collection<string> idsToDelete) 
        {
            StringBuilder builder = new StringBuilder();
            int countIds = 0;
           
            if(self.DataBaseType.Equals(SqlDataBaseType.Oracle))
            {
                builder.Append("DELETE FROM " + self.TableName + " ");
            }
            else if (!self.DataBaseType.Equals(SqlDataBaseType.PostGis)) 
            {
                 builder.Append("DELETE FROM '" + self.TableName + "' ");
            }
            else 
            {
                string prefix = string.IsNullOrEmpty(self.SchemaName) ? string.Empty : self.SchemaName + ".";
                builder.Append("DELETE FROM " + prefix + MakeCaseSensitive(self.TableName) + " ");
            }
                
            if (!idsToDelete.Count().Equals(0))
            {
                builder.Append("WHERE ");

                foreach (string id in idsToDelete) 
                {
                    countIds++;
                    builder.Append( "'" + self.FeatureIdColumn + "' = '" + id + "' ");

                    if (!id.Count().Equals(countIds))
                    {
                        builder.Append("OR ");
                    }
                }

                builder.Append(";");
            
            }

            return builder.ToString();
        }

        private static string MakeCaseSensitive(string columnName)
        {
            return "\"" + columnName + "\"";
        }

        public static string GetTableName(string schema, string tableName) 
        {
            string prefix = string.IsNullOrEmpty(schema) ? string.Empty : schema + ".";

            return prefix + MakeCaseSensitive(tableName);
        }

        public static string CreateVirtualTableQuery(string tableName) 
        {
            return "CREATE VIRTUAL TABLE '"+tableName+"' USING rtree('id', 'minx', 'maxx', 'miny', 'maxy')";
        }

        public static string GetInsert(IEnumerable<string> columnsName, DataTable data, string tableName)
        {
            StringBuilder query = new StringBuilder();
            query.Append("INSERT INTO "+ tableName +" (");

            foreach (string name in columnsName) 
            {
                query.Append(name + ",");
            }

            query.Remove(query.Length - 1, 1);
            query.Append(") VALUES ");

            BoundingboxGeometry read = new BoundingboxGeometry();

            foreach (DataRow dataRows in data.Rows)
            {
                read = FeaturesTools.GetSQLiteGeometry(dataRows.Field<byte[]>(1));
                query.Append("(" + dataRows.Field<Int64>(0) + "," + read.minX + "," + read.maxX + "," + read.minY + "," + read.maxY + "),");
            }


            query.Remove(query.Length - 1, 1);
            query.Append(";");

            return query.ToString();
        }

        public static string GetCount(SqlFeatureSource self) 
        {
            if (self.DataBaseType.Equals(SqlDataBaseType.Oracle))
            {
                return "SELECT COUNT(*) FROM " + self.TableName;
            }
            else if (!self.DataBaseType.Equals(SqlDataBaseType.PostGis))
            {
                return "SELECT COUNT(*) FROM " + self.TableName + " ;";
            }
            else
            {
                string prefix = string.IsNullOrEmpty(self.SchemaName) ? string.Empty : self.SchemaName + ".";
                return "select count(*) from " + prefix + MakeCaseSensitive(self.TableName);

            }
          
        }

        public static string GetCount(string tableName)
        {
           return "SELECT COUNT(*) FROM " + tableName + " ;";
        }

        public static string CreateTempTableOracle(string schema, string tableName, string selectQuery)
        {
            return string.Format("CREATE TABLE {0}.temp_{1} AS {2}", schema, tableName, selectQuery);
        }

        public static string InsertSpatialMetadataTempTableOracle(string tableName)
        {
            return string.Format("INSERT INTO USER_SDO_GEOM_METADATA SELECT 'TEMP_{0}' AS TABLE_NAME, COLUMN_NAME, DIMINFO, SRID FROM USER_SDO_GEOM_METADATA WHERE TABLE_NAME = '{0}'", tableName.ToUpper());
        }

        public static string CreateSpatialTempIndex(string schema, string tableName, string geomColumn)
        {
            return string.Format("CREATE INDEX {0}.TEMP_{1}_{2}_SPX ON {0}.TEMP_{1}({2}) INDEXTYPE IS MDSYS.SPATIAL_INDEX", schema, tableName, geomColumn);
        }

        public static string DropTempTableOracle(string schema, string tableName)
        {
            return string.Format("DROP TABLE {0}.temp_{1}", schema, tableName);
        }

        public static string DeleteSpatialMetadataTempTableOracle(string tableName)
        {
            return string.Format("DELETE FROM USER_SDO_GEOM_METADATA WHERE TABLE_NAME='TEMP_{0}'", tableName.ToUpper());
        }

        public static string DropSpatialTempIndex(string schema, string tableName, string geomColumn)
        {
            return string.Format("DROP INDEX {0}.TEMP_{1}_{2}_SPX", schema, tableName, geomColumn);
        }

        public static string BuildSelectQueryTempTableOracle(IEnumerable<string> columnNames, SqlFeatureSource self)
        {
            string sqlColumns = BuildSelectColumnsStringOracle(columnNames, self.FeatureIdColumn, self.GeomColumnName, true);
            string sql = GetStringQuery(sqlColumns, self.TableName, "1=0");
            if (sql.Substring(sql.Length - 1).Equals(";"))
                return sql.Substring(0, sql.Length - 1);
            else
                return sql;
        }

        public static string InsertQueryTemTableOracle(IEnumerable<string> columnNames, SqlFeatureSource self, int page)
        {
            int limit = (page + 1) * 10000;
            int start = page * 10000;
            string sqlColumns = BuildSelectColumnsStringOracle(columnNames, self.FeatureIdColumn, self.GeomColumnName, true);
            return string.Format("insert into temp_{0} SELECT {1} FROM (SELECT ROWNUM rnum,a.* FROM (SELECT * FROM {0}) a WHERE ROWNUM <= {2}) a WHERE rnum > {3}", self.TableName, sqlColumns, limit, start);
        }

    }
}


using Mapgenix.FeatureSource;
using System.Data.Entity;
using System.Data.SQLite;

namespace Mapgenix.Layers
{
    /// <summary>
    /// Factory class for spatial data base layers.
    /// </summary>
    public static class FeatureSqlLayerFactory
    {

        private const int invalidSrid = -999;

        /// <summary>
        /// Sql Server Spatial Feature Layer
        /// </summary>
        /// <param name="nameOrConnectionString">This parameter refers to connectionString or name of connectionString in the "App.config" </param>
        /// <param name="tableName">Set the table name of the database that you want to work.</param>
        /// <param name="featIdColumn">Set the name of the column where are defined the "id" of each Feature.</param>
        /// <param name="geomColumnName">Set the name of the column where the geometric data of the table is stored.</param>
        /// <param name="srid">This last parameter is placed the SRID</param>
        /// <returns>All features from the Sql Server data base table</returns>
        public static BaseFeatureLayer CreateSqlServerSpatialFeatureLayer(string nameOrConnectionString, string tableName,
            string featIdColumn, string geomColumnName, int srid)
        {
            return CreateSqlServerSpatialFeatureLayer(new DbContext(nameOrConnectionString), tableName, featIdColumn, geomColumnName, srid);
        }

        /// <summary>
        /// Sql Server Spatial Feature Layer
        /// </summary>
        /// <param name="context">This parameter refers to SourceContext, which must be created using the "connectionString" in the "App.config" </param>
        /// <param name="tableName">Set the table name of the database that you want to work.</param>
        /// <param name="featIdColumn">Set the name of the column where are defined the "id" of each Feature.</param>
        /// <param name="geomColumnName">Set the name of the column where the geometric data of the table is stored.</param>
        /// <param name="srid">This last parameter is placed the SRID</param>
        /// <returns>All features from the Sql Server data base table</returns>
        public static BaseFeatureLayer CreateSqlServerSpatialFeatureLayer(DbContext context, string tableName,
            string featIdColumn, string geomColumnName, int srid)
        {
            return new InMemoryFeatureLayer
            {
                //FeatureSource = new SqlFeatureSource(SqlDataBaseType.SqlServer,
                    //context, tableName, featIdColumn, geomColumnName, srid),
                Name = string.Empty,
                HasBoundingBox = true,
                IsVisible = true
            };
        }


        /// <summary>
        /// PostGIS Feature Layer
        /// </summary>
        /// <param name="nameOrConnectionString">This parameter refers to connectionString or name of connectionString in the "App.config" </param>
        /// <param name="schemaName">SchemaName refers to the name of the schema where is located the table</param>
        /// <param name="tableName">Set the table name of the database that you want to work.</param>
        /// <param name="featIdColumn">Set the name of the column where are defined the "id" of each Feature.</param>
        /// <param name="geomColumName">Set the name of the column where the geometric data of the table is stored.</param>
        /// <param name="srid">This last parameter is placed the SRID</param>
        /// <returns>All features from the PostGIS data base table</returns>
        public static BaseFeatureLayer CreatePostGisFeatureLayer(string nameOrConnectionString, string schemaName,
            string tableName, string featIdColumn, string geomColumName, int srid)
        {
            int validSrid = 0;

            if (srid.Equals(null))
            {
                validSrid = invalidSrid;
            }
            else
            {
                validSrid = srid;
            }

            InMemoryFeatureLayer postLayer = new InMemoryFeatureLayer
            {
                //FeatureSource = new SqlFeatureSource(SqlDataBaseType.PostGis, new DbContext(nameOrConnectionString),
                    //schemaName, tableName, featIdColumn, geomColumName, validSrid),
                Name = string.Empty,
                HasBoundingBox = true,
                IsVisible = true
            };
            postLayer.SetupTools();
            return postLayer;
        }

        /// <summary>
        /// PostGIS Feature Layer
        /// </summary>
        /// <param name="context">This parameter refers to SourceContext, which must be created using the "connectionString" in the "App.config". </param>
        /// <param name="schemaName">SchemaName refers to the name of the schema where is located the table</param>
        /// <param name="tableName">Set the table name of the database that you want to work.</param>
        /// <param name="featIdColumn">Set the name of the column where are defined the "id" of each Feature.</param>
        /// <param name="geomColumName">Set the name of the column where the geometric data of the table is stored.</param>
        /// <param name="srid">This last parameter is placed the SRID</param>
        /// <returns>All features from the PostGIS data base table</returns>
        public static BaseFeatureLayer CreatePostGisFeatureLayer(DbContext context, string schemaName,
            string tableName, string featIdColumn, string geomColumName, int srid)
        {
            int validSrid = 0;

            if (srid.Equals(null))
            {
                validSrid = invalidSrid;
            }
            else 
            {
                validSrid = srid;
            }

            InMemoryFeatureLayer postLayer = new InMemoryFeatureLayer
            {
                //FeatureSource = new SqlFeatureSource(SqlDataBaseType.PostGis, context,
                    //schemaName, tableName, featIdColumn, geomColumName, validSrid),
                Name = string.Empty,
                HasBoundingBox = true,
                IsVisible = true
            };
            postLayer.SetupTools();
            return postLayer;
        }

        /// <summary>
        /// SpatiaLite Feature Layer
        /// </summary>
        /// <param name="nameOrConnectionString">This parameter refers to connectionString or name of connectionString in the "App.config"</param>
        /// <param name="dataBaseSource">Set the path where the database file is located.</param>
        /// <param name="tableName">Set the table name of the database that you want to work.</param>
        /// <param name="featIdColumn">Set the name of the column where are defined the "id" of each Feature.</param>
        /// <param name="geomColumnName">Set the name of the column where the geometric data of the table is stored.</param>
        /// <returns>All features from the SpatiaLite data base table</returns>
        public static BaseFeatureLayer CreateSpatiaLiteFeatureLayer(string nameOrConnectionString, string dataBaseSource,
            string tableName, string featIdColumn, string geomColumnName)
        {

            DbContext context = null;
            if (nameOrConnectionString.Contains("Data Source="))
                context = new DbContext(new SQLiteConnection(nameOrConnectionString), true);
            else
                context = new DbContext(nameOrConnectionString);

            return CreateSpatiaLiteFeatureLayer(context, dataBaseSource, tableName, featIdColumn, geomColumnName);
        }

        /// <summary>
        /// SpatiaLite Feature Layer
        /// </summary>
        /// <param name="context">This parameter refers to SourceContext, which must be created using the "connectionString" in the "App.config".</param>
        /// <param name="dataBaseSource">Set the path where the database file is located.</param>
        /// <param name="tableName">Set the table name of the database that you want to work.</param>
        /// <param name="featIdColumn">Set the name of the column where are defined the "id" of each Feature.</param>
        /// <param name="geomColumnName">Set the name of the column where the geometric data of the table is stored.</param>
        /// <returns>All features from the SpatiaLite data base table</returns>
        public static BaseFeatureLayer CreateSpatiaLiteFeatureLayer(DbContext context, string dataBaseSource,
            string tableName, string featIdColumn, string geomColumnName) 
        {
            return new InMemoryFeatureLayer 
            {
                //FeatureSource = new SqlFeatureSource(SqlDataBaseType.Sqlite, context, dataBaseSource,
                    //tableName, featIdColumn, geomColumnName),
                    Name = string.Empty,
                    HasBoundingBox = true,
                    IsVisible = true
            };
        }

        /// <summary>
        /// Oracle Spatial Feature Layer
        /// </summary>
        /// <param name="nameOrConnectionString">This parameter refers to connectionString or name of connectionString in the "App.config" </param>
        /// <param name="owner">owner datatabase</param>
        /// <param name="tableName">Set the table name of the database that you want to work.</param>
        /// <param name="featIdColumn">Set the name of the column where are defined the "id" of each Feature.</param>
        /// <param name="geomColumnName">Set the name of the column where the geometric data of the table is stored.</param>
        /// <param name="srid">This last parameter is placed the SRID</param>
        /// <returns>All features from the Oracle data base table</returns>
        public static BaseFeatureLayer CreateOracleSpatialFeatureLayer(string nameOrConnectionString, string owner, string tableName,
            string featIdColumn, string geomColumnName, int srid)
        {
            return CreateOracleSpatialFeatureLayer(new DbContext(nameOrConnectionString), owner, tableName, featIdColumn, geomColumnName, srid, true);
        }

        /// <summary>
        /// Oracle Spatial Feature Layer
        /// </summary>
        /// <param name="nameOrConnectionString">This parameter refers to connectionString or name of connectionString in the "App.config" </param>
        /// <param name="owner">owner datatabase</param>
        /// <param name="tableName">Set the table name of the database that you want to work.</param>
        /// <param name="featIdColumn">Set the name of the column where are defined the "id" of each Feature.</param>
        /// <param name="geomColumnName">Set the name of the column where the geometric data of the table is stored.</param>
        /// <param name="srid">This last parameter is placed the SRID</param>
        /// <param name="createTempTable">Create temp table improve spatial querys performance</param>
        /// <returns>All features from the Oracle data base table</returns>
        public static BaseFeatureLayer CreateOracleSpatialFeatureLayer(string nameOrConnectionString, string owner, string tableName,
            string featIdColumn, string geomColumnName, int srid, bool createTempTable)
        {
            return CreateOracleSpatialFeatureLayer(new DbContext(nameOrConnectionString), owner, tableName, featIdColumn, geomColumnName, srid, createTempTable);
        }

        /// <summary>
        /// Oracle Spatial Feature Layer
        /// </summary>
        /// <param name="context">This parameter refers to SourceContext, which must be created using the "connectionString" in the "App.config".</param>
        /// <param name="owner">SchemaName refers to the name of the schema where is located the table</param>
        /// <param name="tableName">Set the table name of the database that you want to work.</param>
        /// <param name="featIdColumn">Set the name of the column where are defined the "id" of each Feature.</param>
        /// <param name="geomColumnName">Set the name of the column where the geometric data of the table is stored.</param>
        /// <param name="srid">This last parameter is placed the SRID</param>
        /// <returns>All features from the Oracle data base table</returns>
        public static BaseFeatureLayer CreateOracleSpatialFeatureLayer(DbContext context, string owner, string tableName,
            string featIdColumn, string geomColumnName, int srid)
        {
            return CreateOracleSpatialFeatureLayer(context, owner, tableName, featIdColumn, geomColumnName, srid, true);
        }

        /// <summary>
        /// Oracle Spatial Feature Layer
        /// </summary>
        /// <param name="context">This parameter refers to SourceContext, which must be created using the "connectionString" in the "App.config".</param>
        /// <param name="owner">SchemaName refers to the name of the schema where is located the table</param>
        /// <param name="tableName">Set the table name of the database that you want to work.</param>
        /// <param name="featIdColumn">Set the name of the column where are defined the "id" of each Feature.</param>
        /// <param name="geomColumnName">Set the name of the column where the geometric data of the table is stored.</param>
        /// <param name="srid">This last parameter is placed the SRID</param>
        /// <param name="createTempTable">Create temp table improve spatial querys performance</param>
        /// <returns>All features from the Oracle data base table</returns>
        public static BaseFeatureLayer CreateOracleSpatialFeatureLayer(DbContext context, string owner, string tableName, 
            string featIdColumn, string geomColumnName, int srid, bool createTempTable )
        {
            int validSrid = 0;

            if (srid.Equals(null))
            {
                validSrid = invalidSrid;
            }
            else
            {
                validSrid = srid;
            }

            InMemoryFeatureLayer oracleLayer = new InMemoryFeatureLayer
            {
                //FeatureSource = new SqlFeatureSource(SqlDataBaseType.Oracle, context, owner,
                    // tableName, featIdColumn, geomColumnName, validSrid, createTempTable),
                Name = string.Empty,
                HasBoundingBox = true,
                IsVisible = true
            };
            oracleLayer.SetupTools();
            return oracleLayer;
        }
    }
}

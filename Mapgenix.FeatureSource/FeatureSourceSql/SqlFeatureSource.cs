using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using Npgsql;
using System.Data;
using Mapgenix.Shapes;
using Mapgenix.FeatureSource.Properties;
using System.Data.SQLite;
using System.Data.Entity;
using Mapgenix.Canvas;
using Mapgenix.Utils;
using Oracle.ManagedDataAccess.Client;

namespace Mapgenix.FeatureSource
{
    public class SqlFeatureSource : BaseFeatureSource
    {
        public event EventHandler<SqlServerEvent> _sqlServerStatement;
        public event EventHandler<PostGisEvent> _postGisStatement;
        public event EventHandler<SqliteEvent> _sqliteStatement;
        public event EventHandler<OracleEvent> _oracleStatement;

        #region Variables&Const
        SpatialDataType _spatialDataType;
        Collection<FeatureSourceColumn> _featureSourceColumns;
        string _geomFieldName;
        string _virtualTableName;
        const int InvalidSrid = -999;
        const string ParameterName = "geometry";
        private bool _SourceInitialized = false;
        private object _databaseLook = new object();
           
        #endregion

        #region Constructors
        /// <summary>
        /// SqlServer Constructor
        /// </summary>
        /// <param name="dataBaseType"></param>
        /// <param name="newContext"></param>
        /// <param name="tableName"></param>
        /// <param name="featIdColumn"></param>
        /// <param name="srid"></param>
        public SqlFeatureSource(SqlDataBaseType dataBaseType, DbContext newContext,
            string tableName, string featIdColumn, string geomColumnName, int srid)
                : this(dataBaseType, newContext, string.Empty, string.Empty, tableName, featIdColumn, geomColumnName, srid, string.Empty) { }


        /// <summary>
        /// PostGis / Oracle Constructor 
        /// </summary>
        /// <param name="dataBaseType"></param>
        /// <param name="newContext"></param>
        /// <param name="schemaName"></param>
        /// <param name="tableName"></param>
        /// <param name="featIdColumn"></param>
        /// <param name="srid"></param>
        public SqlFeatureSource(SqlDataBaseType dataBaseType, DbContext newContext,
            string schemaName, string tableName, string featIdColumn, string geomColumnName, int srid)
            : this(dataBaseType, newContext, string.Empty, schemaName, tableName, featIdColumn, geomColumnName, srid, string.Empty) { }


        /// <summary>
        /// Sqlite Constructor
        /// </summary>
        /// <param name="dataBaseType"></param>
        /// <param name="newContext"></param>
        /// <param name="dataBaseSource"></param>
        /// <param name="tableName"></param>
        /// <param name="geomColumnName"></param>
        /// <param name="featIdColumn"></param>
        public SqlFeatureSource(SqlDataBaseType dataBaseType, DbContext newContext, string dataBaseSource,
            string tableName, string featIdColumn, string geomColumnName)
            : this(dataBaseType, newContext, dataBaseSource, string.Empty, tableName, featIdColumn, geomColumnName, 0, string.Empty) { }


        /// <summary>
        /// Oracle Constructor 
        /// </summary>
        /// <param name="dataBaseType"></param>
        /// <param name="newContext"></param>
        /// <param name="schemaName"></param>
        /// <param name="tableName"></param>
        /// <param name="featIdColumn"></param>
        /// <param name="geomColumnName"></param>
        /// <param name="srid"></param>
        /// <param name="createTempTable"></param>
        public SqlFeatureSource(SqlDataBaseType dataBaseType, DbContext newContext,
            string schemaName, string tableName, string featIdColumn, string geomColumnName, int srid, bool createTempTable)
            : this(dataBaseType, newContext, string.Empty, schemaName, tableName, featIdColumn, geomColumnName, srid, string.Empty)
        {
            CreateTempTable = createTempTable;
        }

        /// <summary>
        /// SqlFeature
        /// </summary>
        /// <param name="dataBaseType"></param>
        /// <param name="tableName"></param>
        /// <param name="featIdColumn"></param>
        /// <param name="srid"></param>
        /// <param name="newContext"></param>
        public SqlFeatureSource(SqlDataBaseType dataBaseType, DbContext newContext,  string dataBaseSource, string schemaName,
            string tableName, string featIdColumn, string geoColumnName ,int srid, string whereClause)
        {
            DataBaseType = dataBaseType;
            DataBaseSource = string.IsNullOrEmpty(dataBaseSource) ? newContext.Database.Connection.ConnectionString : dataBaseSource; 
            SchemaName = schemaName;
            TableName = tableName;
            FeatureIdColumn = featIdColumn;
            GeomColumnName = geoColumnName;
            TimeOut = 30;
            Srid = srid;
            WhereClause = whereClause;
            Context = newContext;//new DbContext("SqlConnection");
        }
        #endregion

        #region Attributes

        public SqlDataBaseType DataBaseType { get; set; }

        public string DataBaseSource { get; set; }

        public string SchemaName {  get; set; }

        public string TableName {  get; set; }

        public string FeatureIdColumn { get; set; }

        public string GeomColumnName { get; set; }

        public string WhereClause { get; set; }

        public int Srid { get; set; }

        public int TimeOut { get; set; }

        public bool CreateTempTable { get; set; }

        public DbContext Context { get;  private set; }
        #endregion

        #region PublicsMethods
        public WellKnownType GetFirstGeometryType()
        {
            WellKnownType type = WellKnownType.Invalid;
            byte[] data = null;

            try
            {
                if (DataBaseType.Equals(SqlDataBaseType.SqlServer)) 
                {
                    data = Context.Database.SqlQuery<byte[]>(GetSqlEvent(SqlBuilder.GetGeometryType(this),
                    ExecutingSqlStatementType.GetFirstGeometryType)).First<byte[]>();

                    if (data.Count() != 0)
                    {
                        if (data != null)
                        {
                            type = new Feature(data).GetWellKnownType();
                        }
                    }                
                }
                else if (DataBaseType.Equals(SqlDataBaseType.PostGis)) 
                {
                    type = Context.Database.SqlQuery<WellKnownType>(GetSqlEvent(SqlBuilder.GetGeometryType(this),
                    ExecutingSqlStatementType.GetFirstGeometryType)).First<WellKnownType>();
                }
                else if (DataBaseType.Equals(SqlDataBaseType.Sqlite))
                {
                   DataTable dataTable = new DataTable();

                   using(SQLiteDataAdapter dataLite = 
                    new SQLiteDataAdapter(GetSqlEvent(SqlBuilder.GetGeometryType(this),
                        ExecutingSqlStatementType.GetFirstGeometryType), @DataBaseSource))
                    {
                        dataLite.Fill(dataTable);
                    }

                   foreach (DataRow dataRows in dataTable.Rows)
                   {
                       var wkbr = new SqliteReader();
                       var geom = wkbr.Read(dataRows.Field<byte[]>(1));

                       type = new Feature(geom.ToBinary()).GetWellKnownType();
                   }
                }else if(DataBaseType.Equals(SqlDataBaseType.Oracle))
                {
                    data = Context.Database.SqlQuery<byte[]>(GetSqlEvent(SqlBuilder.GetGeometryType(this),
                    ExecutingSqlStatementType.GetFirstGeometryType)).First<byte[]>();

                    if (data.Count() != 0)
                    {
                        if (data != null)
                        {
                            type = new Feature(data).GetWellKnownType();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }

            return type;
        }

        public int GetProximateCount()
        {
            int count = 0;

            try
            {
                count = Context.Database.SqlQuery<int>(GetSqlEvent(SqlBuilder.GetProxCount(TableName, SchemaName), 
                    ExecutingSqlStatementType.GetCount)).First<int>();
            }
            catch (Exception e)
            {
                throw e;
            }

            return count;
        }

        public void BuildIndex(BuildIndexMode buildIndexMode)
        {
            Validators.CheckFeatureSourceIsOpen(IsOpen);

            switch (buildIndexMode)
            {
                case BuildIndexMode.DoNotRebuild:
                    if (!CheckIndexExist())
                    {
                        BuildIndex();
                    }
                    break;
                case BuildIndexMode.Rebuild:
                    DropIndexes(GetIndexName());
                    BuildIndex();
                    break;
                default:
                    break;
            }
        }

        public void CreateTable(string tableName, SpatialDataType spatialDataType, 
            IEnumerable<FeatureSourceColumn> columns, OverwriteMode overwriteMode)
        {
            Validators.CheckParameterIsNotNullOrEmpty(tableName, "tableName");
            Validators.CheckParameterIsNotNull(columns, "columns");
            Validators.CheckSpatialDataTypeIsValid(spatialDataType, "spatialDataType");
            Validators.CheckOverwriteModeIsValid(overwriteMode, "overWriteMode");

            Context.Database.ExecuteSqlCommand(SqlBuilder.CreateTable(tableName, spatialDataType, columns));
        }

        public Dictionary<string, string> Validate()
        {
            Validators.CheckFeatureSourceIsOpen(IsOpen);
            Dictionary<string, string> description = new Dictionary<string, string>();

            if (_spatialDataType == SpatialDataType.Geometry)
            {
                string sqlQuery = GetSqlEvent(SqlBuilder.GetSqlQueryToValidate(this), ExecutingSqlStatementType.Validate);
                int rows = Context.Database.SqlQuery<string>(sqlQuery).Count();

                try
                {
                    if (rows > 0)
                    {
                        string infoSqlStatement = SqlBuilder.GetSqlQueryToValidate(TableName, GeomColumnName);
                        description.Add("1", ExceptionDescription.GeometriesInvalidExist + infoSqlStatement);
                    }
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
            return description;
        }

        public void MakeAllGeometriesValid()
        {
            Validators.CheckFeatureSourceIsOpen(IsOpen);

            if (_spatialDataType == SpatialDataType.Geometry)
            {
                try
                {
                    Context.Database.ExecuteSqlCommand(GetSqlEvent(SqlBuilder.GetValidGeometryQuery(TableName, GeomColumnName),
                        ExecutingSqlStatementType.MakeAllGeometriesValid));
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }
        #endregion 

        #region OverrideMethods

        protected override void OpenCore()
        {
            if (string.IsNullOrEmpty(DataBaseSource))
                return;

            if (_SourceInitialized)
                return;

            int tempSrid = Int32.MinValue;

            switch (DataBaseType) 
            { 
                case SqlDataBaseType.SqlServer:
           
                    _spatialDataType = GetSpatialDataType();

                    if (string.IsNullOrEmpty(GeomColumnName))
                    {
                        GeomColumnName = GetGeometryColumnName(DataBaseType, 1);

                        if (string.IsNullOrEmpty(GeomColumnName))
                        {
                            GeomColumnName = GetGeometryColumnName(DataBaseType, 2);
                        }

                        if (string.IsNullOrEmpty(GeomColumnName))
                        {
                            throw new ArgumentException(string.Format("Can not find the geometry column in table : '{0}'", TableName));
                        }
                    }

                    try
                    {
                        tempSrid = Context.Database.SqlQuery<int>(GetSqlEvent(SqlBuilder.GetSrid(DataBaseType, TableName), ExecutingSqlStatementType.GetColumns)).First<int>();
                    }
                    catch (Exception e)
                    {
                        throw e;
                    }

                    if (Srid == InvalidSrid || Srid == 0)
                    {

                        if (tempSrid == Int32.MinValue)
                        {
                            throw new ArgumentException("InvalidSrid", "Srid");
                        }
                        else
                        {
                            Srid = tempSrid;
                        }
                    }

                    _SourceInitialized = true;

                    break;

                case SqlDataBaseType.PostGis:                

                    if (!string.IsNullOrEmpty(_geomFieldName))
                    {
                        if (Srid == InvalidSrid)throw new ArgumentException("Invalid Srid", "Srid");

                        _geomFieldName = GeomColumnName;
                        return;
                    }

                    try
                    {
                         _geomFieldName = Context.Database.SqlQuery<string>(GetSqlEvent(SqlBuilder.GetGeometryColumnName(DataBaseType,TableName, SchemaName), 
                             ExecutingSqlStatementType.GetColumns)).First<string>();

                         tempSrid = Context.Database.SqlQuery<int>(GetSqlEvent(SqlBuilder.GetSrid(DataBaseType, TableName), ExecutingSqlStatementType.GetColumns)).FirstOrDefault<int>();

                    }
                    catch (Exception e)
                    {
                        throw e;
                    }

                    if (Srid == InvalidSrid)
                    {

                        if (tempSrid == Int32.MinValue)
                        {
                            throw new ArgumentException("InvalidSrid", "Srid");
                        }
                        else
                        {
                            Srid = tempSrid;
                        }
                    }

                    if (string.IsNullOrEmpty(_geomFieldName))
                        _geomFieldName = GetGeometryColumnName(DataBaseType, 0);


                    if (string.IsNullOrEmpty(_geomFieldName))
                        throw new ArgumentException(string.Format("Can not find the geometry column in table : '{0}'", TableName));

                    _SourceInitialized = true;

                break;

                case SqlDataBaseType.Oracle:
                    if (!string.IsNullOrEmpty(_geomFieldName))
                    {
                        if (Srid == InvalidSrid) throw new ArgumentException("Invalid Srid", "Srid");

                        _geomFieldName = GeomColumnName;
                        return;
                    }

                    try
                    {
                        _geomFieldName = Context.Database.SqlQuery<string>(GetSqlEvent(SqlBuilder.GetGeometryColumnName(DataBaseType, TableName, SchemaName),
                            ExecutingSqlStatementType.GetColumns)).First<string>();

                        tempSrid = Context.Database.SqlQuery<int>(GetSqlEvent(SqlBuilder.GetSrid(DataBaseType, TableName), ExecutingSqlStatementType.GetColumns)).FirstOrDefault<int>();

                    }
                    catch (Exception e)
                    {
                        throw e;
                    }

                    if (Srid == InvalidSrid)
                    {

                        if (tempSrid == Int32.MinValue)
                        {
                            throw new ArgumentException("InvalidSrid", "Srid");
                        }
                        else
                        {
                            Srid = tempSrid;
                        }
                    }

                    if (string.IsNullOrEmpty(_geomFieldName))
                        _geomFieldName = GetGeometryColumnName(DataBaseType, 0);


                    if (string.IsNullOrEmpty(_geomFieldName))
                        throw new ArgumentException(string.Format("Can not find the geometry column in table : '{0}'", TableName));

                    if (CreateTempTable)
                    {
                        if (IsTableExist("temp_" + this.TableName))
                        {
                            DropOracleTempSpatialTable();
                        }

                        CreateOracleTempSpatialTable();
                    }

                    _SourceInitialized = true;
                break;

                case SqlDataBaseType.Sqlite:

                    try
                    {

                        if (string.IsNullOrEmpty(GeomColumnName))
                            GeomColumnName = GetGeometryColumnName(DataBaseType, 0);


                        if (string.IsNullOrEmpty(GeomColumnName))
                            throw new ArgumentException(string.Format("Can not find the geometry column in table : '{0}'", TableName));


                        _virtualTableName = "idx_" + TableName + "_geometry";

                        if (!IsTableExist(_virtualTableName))
                        {
                            CreateSqliteBoundingTable(_virtualTableName);
                            FillBoundingTable(_virtualTableName);
                        }
                        else
                        {
                            if (!Context.Database.SqlQuery<int>(SqlBuilder.GetCount(this)).First().Equals(
                                Context.Database.SqlQuery<int>(SqlBuilder.GetCount(_virtualTableName)).First()))
                            {
                                Context.Database.ExecuteSqlCommand(SqlBuilder.GetDeleteQuery(this, null));
                                FillBoundingTable(_virtualTableName);
                            }
                        }

                        _SourceInitialized = true;

                    }
                    catch(Exception e)
                    {
                        System.Diagnostics.Debug.WriteLine(e.Message + " InnerException: " + ((e.InnerException != null) ? e.InnerException.Message : ""));
                        throw e;
                    }

                break;
            }
           
        }

        protected override Collection<Feature> GetFeaturesInsideBoundingBoxCore(RectangleShape boundingBox, IEnumerable<string> columnNames)
        {
            Validators.CheckFeatureSourceIsOpen(IsOpen);
            Validators.CheckParameterIsNotNull(boundingBox, "boundingBox");
            Validators.CheckParameterIsValid(boundingBox, "boundingBox");

            return GetFeaturesByBoundingBox(boundingBox, 1, columnNames);
        }

        protected override Collection<Feature> GetFeaturesOutsideBoundingBoxCore(RectangleShape boundingBox, IEnumerable<string> columnNames)
        {
            Validators.CheckFeatureSourceIsOpen(IsOpen);
            Validators.CheckParameterIsNotNull(boundingBox, "boundingBox");
            Validators.CheckParameterIsValid(boundingBox, "boundingBox");

            return GetFeaturesByBoundingBox(boundingBox, 2, columnNames);
        }

        protected override Collection<Feature> GetAllFeaturesCore(IEnumerable<string> columnNames)
        {
            Validators.CheckFeatureSourceIsOpen(IsOpen);

            Collection<Feature> features = new Collection<Feature>();
            DataTable tableFeature = new DataTable();

            string sql = string.Empty;

            try
            {
                if (DataBaseType.Equals(SqlDataBaseType.SqlServer))
                {

                    sql = SqlBuilder.GetStringQuery(columnNames, this, string.Empty);

                    using (SqlDataAdapter dataFeatures = new SqlDataAdapter(GetSqlEvent(sql,
                        ExecutingSqlStatementType.GetAllFeatures), @DataBaseSource))
                    {
                        dataFeatures.Fill(tableFeature);
                    }
                }
                else if (DataBaseType.Equals(SqlDataBaseType.PostGis))
                {
                    string cols = SqlBuilder.BuildSelectColumnsStringPost(columnNames, FeatureIdColumn, GeomColumnName);
                    sql = SqlBuilder.GetStringQuery(columnNames,this, string.Empty);

                    using (NpgsqlDataAdapter dataFeatures = new NpgsqlDataAdapter(GetSqlEvent(sql,
                        ExecutingSqlStatementType.GetAllFeatures), @DataBaseSource))
                    {
                        dataFeatures.Fill(tableFeature);
                    }
                }
                else if (DataBaseType.Equals(SqlDataBaseType.Sqlite))
                {
                    sql = SqlBuilder.GetStringQuery(columnNames, this, string.Empty);

                    using (SQLiteDataAdapter dataFeatures = new SQLiteDataAdapter(GetSqlEvent(sql,
                        ExecutingSqlStatementType.GetAllFeatures), @DataBaseSource))
                    {
                        dataFeatures.Fill(tableFeature);
                    }
                }
                else if(DataBaseType.Equals(SqlDataBaseType.Oracle))
                {
                    sql = SqlBuilder.GetStringQuery(columnNames, this, string.Empty);

                    using (OracleDataAdapter dataFeatures = new OracleDataAdapter(GetSqlEvent(sql,
                        ExecutingSqlStatementType.GetAllFeatures), @DataBaseSource))
                    {
                        dataFeatures.Fill(tableFeature);
                    }
                }

            }
            catch (Exception e)
            {
                throw e;
            }


            features = FeaturesTools.GetFeatures(tableFeature, FeatureIdColumn, GeomColumnName, DataBaseType);

            return features;
        }

        protected override Collection<Feature> GetFeaturesByIdsCore(IEnumerable<string> ids, IEnumerable<string> columnNames)
        {
            Validators.CheckFeatureSourceIsOpen(IsOpen);
            Validators.CheckParameterIsNotNull(ids, "ids");

            List<string> idsList = new List<string>(ids);
            Collection<Feature> features = new Collection<Feature>();
            List<GeoData> dataList = new List<GeoData>();
            DataTable featDataTable = new DataTable();
            
            string sql = string.Empty;

            sql = GetSqlEvent(SqlBuilder.GetFeaturesByIdsCore(ids, columnNames, this),
                    ExecutingSqlStatementType.GetFeaturesByIds);

            try
            {
                if(DataBaseType.Equals(SqlDataBaseType.PostGis))
                {
                    using (NpgsqlDataAdapter data = new NpgsqlDataAdapter(sql, @DataBaseSource)) 
                    {
                        data.Fill(featDataTable);
                    }
                }
                else if (DataBaseType.Equals(SqlDataBaseType.Sqlite))
                {
                    using (SQLiteDataAdapter data =
                    new SQLiteDataAdapter(sql, @DataBaseSource))
                    {
                        data.Fill(featDataTable);
                    }                    
                }
                else if(DataBaseType.Equals(SqlDataBaseType.Oracle))
                {
                    using (OracleDataAdapter data = new OracleDataAdapter(sql, @DataBaseSource))
                    {
                        data.Fill(featDataTable);
                    }
                }
                else 
                {

                    using (SqlDataAdapter data = new SqlDataAdapter(sql, @DataBaseSource))
                    {
                        data.Fill(featDataTable);
                    }
                
                }

                features = FeaturesTools.GetFeatures(this, featDataTable);
            }
            catch (Exception e)
            {
                throw e;
            }

            return features;
        }

        protected override int ExecuteNonQueryCore(string sqlStatement)
        {
            Validators.CheckParameterIsNotNullOrEmpty(sqlStatement, "sqlStatement");
            Validators.CheckFeatureSourceIsOpen(IsOpen);

            int rows = 0;

            try
            {
               rows = Context.Database.SqlQuery<int>(GetSqlEvent(sqlStatement, ExecutingSqlStatementType.ExecuteNonQuery)).First<int>();
         
            }
            catch (Exception e)
            {
                throw e;
            }

            return rows;
        }

        protected override RectangleShape GetBoundingBoxCore()
        {
            Validators.CheckFeatureSourceIsOpen(IsOpen);

            RectangleShape boundingBoxRectangleShape = new RectangleShape();
            Collection<RectangleShape> boundingBoxes = new Collection<RectangleShape>();
            string sql = string.Empty;
            List<byte[]> data = new List<byte[]>();
            var wkbr = new SqliteReader();

            try 
            {
                switch (DataBaseType)
                {
                    case (SqlDataBaseType.SqlServer):

                        switch (_spatialDataType)
                        {
                            case SpatialDataType.Geometry:
                                sql = GetSqlEvent(SqlBuilder.GetBoundingBoxQueryByGeometry(this), ExecutingSqlStatementType.GetBoundingBox);
                                break;
                            case SpatialDataType.Geography:
                                sql = GetSqlEvent(SqlBuilder.GetBoundingBoxQueryByGeography(this), ExecutingSqlStatementType.GetBoundingBox);
                                break;
                            default:
                                sql = GetSqlEvent(SqlBuilder.GetBoundingBoxQueryByGeometry(this), ExecutingSqlStatementType.GetBoundingBox);
                                break;
                        }

                        data = Context.Database.SqlQuery<byte[]>(sql).ToList<byte[]>();

                        foreach (byte[] x in data)
                        {
                            boundingBoxRectangleShape.LoadFromWellKnownData(x);
                        }

                        break;

                    case (SqlDataBaseType.PostGis):


                        data = Context.Database.SqlQuery<byte[]>(GetSqlEvent(SqlBuilder.GetBoundingBoxQuery(this),
                            ExecutingSqlStatementType.GetBoundingBox)).ToList<byte[]>();

                       boundingBoxRectangleShape = FeaturesTools.GetBoundingBoxbyFeatures(data, DataBaseType);

                        break;

                    case (SqlDataBaseType.Sqlite):

                        data = Context.Database.SqlQuery<byte[]>(GetSqlEvent(SqlBuilder.GetBoundingBoxQuery(this),
                            ExecutingSqlStatementType.GetBoundingBox)).ToList<byte[]>();

                        boundingBoxRectangleShape = FeaturesTools.GetBoundingBoxbyFeatures(data, DataBaseType);

                        break;

                    case (SqlDataBaseType.Oracle):
                        
                        var wktList = Context.Database.SqlQuery<string>(GetSqlEvent(SqlBuilder.GetBoundingBoxQuery(this),
                            ExecutingSqlStatementType.GetBoundingBox)).ToList<string>();

                        boundingBoxRectangleShape.LoadFromWellKnownData(wktList[0]);
                        break;
                }
            
            }
            catch (Exception e) 
            {
                throw e;
            }

            return boundingBoxRectangleShape;
        }

        protected override int GetCountCore()
        {
            Validators.CheckFeatureSourceIsOpen(IsOpen);
            string sql = SqlBuilder.GetCount(this);
            int count = 0;

            try
            {
                count = Context.Database.SqlQuery<int>(GetSqlEvent(sql,
                    ExecutingSqlStatementType.GetCount)).First<int>();
            }
            catch(Exception e)
            {
                throw e;
            }

            return count;
        }

        protected override Collection<FeatureSourceColumn> GetColumnsCore()
        {
            Validators.CheckFeatureSourceIsOpen(IsOpen);
            
            DataTable data = new DataTable();

            try
            {
                switch (DataBaseType) 
                { 
                    case (SqlDataBaseType.SqlServer):
                        using (SqlDataAdapter dataAdapter = new SqlDataAdapter(GetSqlEvent(SqlBuilder.GetQueryToColumsCore(this),
                            ExecutingSqlStatementType.GetColumns), @DataBaseSource))
                        {
                            dataAdapter.Fill(data);
                        }
                        
                        break;

                    case (SqlDataBaseType.PostGis):
                       
                        using (NpgsqlDataAdapter dataAdapter = new NpgsqlDataAdapter(GetSqlEvent(SqlBuilder.GetQueryToColumsCore(this),
                            ExecutingSqlStatementType.GetColumns), @DataBaseSource)) 
                            {
                                dataAdapter.Fill(data);
                            }

                        break;

                    case (SqlDataBaseType.Sqlite):

                        using (SQLiteDataAdapter dataAdapter = new SQLiteDataAdapter(GetSqlEvent(SqlBuilder.GetQueryToColumsCore(this), 
                            ExecutingSqlStatementType.GetColumns), @DataBaseSource))
                        {
                            dataAdapter.Fill(data);
                        }
                        break;

                    case (SqlDataBaseType.Oracle):
                        using (OracleDataAdapter dataAdapter = new OracleDataAdapter(GetSqlEvent(SqlBuilder.GetQueryToColumsCore(this),
                            ExecutingSqlStatementType.GetColumns), @DataBaseSource))
                        {
                            dataAdapter.Fill(data);
                        }
                        break;
                }

            }
            catch (Exception e)
            {
                throw e;
            }

            return FeaturesTools.GetColumnsCore(data);
        }

        protected override void CloseCore()
        { 
        }

        protected override TransactionResult CommitTransactionCore(TransactionBuffer transactions)
        {
            Validators.CheckFeatureSourceIsInTransaction(IsInTransaction);

            TransactionResult transactionResult = new TransactionResult();

            ProcessAddBuffer(transactions.AddBuffer, transactionResult);
            ProcessDeleteBuffer(transactions.DeleteBuffer, transactionResult);
            ProcessEditBuffer(transactions.EditBuffer, transactionResult);

            return transactionResult;
        }

        protected override DataTable ExecuteQueryCore(string sqlStatement)
        {

            Validators.CheckParameterIsNotNullOrEmpty(sqlStatement, "sqlStatement");
            Validators.CheckFeatureSourceIsOpen(IsOpen);
            Validators.CheckFeatureSourceCanExecuteSqlQuery(CanExecuteSqlQuery);

            DataTable tableFeature = new DataTable();
           
            try
            {
                if (DataBaseType.Equals(SqlDataBaseType.SqlServer))
                {
                    using (SqlDataAdapter dataFeatures = new SqlDataAdapter(GetSqlEvent(sqlStatement,
                        ExecutingSqlStatementType.ExecuteQuery), @DataBaseSource))
                    {
                        dataFeatures.Fill(tableFeature);
                    }
                }
                else if (DataBaseType.Equals(SqlDataBaseType.PostGis))
                {
                    using (NpgsqlDataAdapter dataFeatures = new NpgsqlDataAdapter(GetSqlEvent(sqlStatement,
                        ExecutingSqlStatementType.ExecuteQuery), @DataBaseSource))
                    {
                        dataFeatures.Fill(tableFeature);
                    }
                }
                else if (DataBaseType.Equals(SqlDataBaseType.Sqlite))
                {
                    using (SQLiteDataAdapter dataFeatures = new SQLiteDataAdapter(GetSqlEvent(sqlStatement,
                        ExecutingSqlStatementType.ExecuteQuery), @DataBaseSource))
                    {
                        dataFeatures.Fill(tableFeature);
                    }
                }
                else if(DataBaseType.Equals(SqlDataBaseType.Oracle))
                {
                    using (OracleDataAdapter dataFeatures = new OracleDataAdapter(GetSqlEvent(sqlStatement,
                        ExecutingSqlStatementType.ExecuteQuery), @DataBaseSource))
                    {
                        dataFeatures.Fill(tableFeature);
                    }
                }

            }
            catch (Exception e)
            {
                throw e;
            }

            return tableFeature;
        }

        #endregion 

        #region PrivateMethods

        private SpatialDataType GetSpatialDataType()
        {
            return FeaturesTools.GetSpatialDataType(Context.Database.SqlQuery<string>(SqlBuilder.GetColumnTypeQuery(TableName)).First());
        }

        private string GetSqlEvent(string sqlStatement, ExecutingSqlStatementType sqlStatementType)
        {
            string sql = SqlBuilder.GetStringQuery(sqlStatement, WhereClause, sqlStatementType);

            if (DataBaseType.Equals(SqlDataBaseType.SqlServer)) 
            {
                SqlServerEvent e = new SqlServerEvent(sql, sqlStatementType);
                OnExecutingSqlStatement(e);
            }
            else if (DataBaseType.Equals(SqlDataBaseType.PostGis)) 
            {
                PostGisEvent e = new PostGisEvent(sql, sqlStatementType);
                OnExecutingSqlStatement(e);
            }
            else if (DataBaseType.Equals(SqlDataBaseType.Sqlite))
            {
                SqliteEvent e = new SqliteEvent(sql, sqlStatementType);
                OnExecutingSqlStatement(e);
            }
            else if(DataBaseType.Equals(SqlDataBaseType.Oracle))
            {
                OracleEvent e = new OracleEvent(sql, sqlStatementType);
                OnExecutingSqlStatement(e);
            }

            return sql;
        }

        private string GetGeometryColumnName(SqlDataBaseType sqlDb, int source)
        {
            string tempGeomColumnName = null;

            try
            {
                if (sqlDb == SqlDataBaseType.PostGis)
                {
                    tempGeomColumnName = Context.Database.SqlQuery<string>(SqlBuilder.GetGeometryColumnName(TableName, SchemaName)).First<string>();
                }
                else if (sqlDb == SqlDataBaseType.SqlServer && source == 1)
                {
                    tempGeomColumnName = Context.Database.SqlQuery<string>(SqlBuilder.GetColumnNameByTable(_spatialDataType, TableName)).First();
                }
                else if (sqlDb == SqlDataBaseType.SqlServer && source == 2) 
                {
                    tempGeomColumnName = Context.Database.SqlQuery<string>(SqlBuilder.GetColumnNameByView(_spatialDataType, TableName)).First();
                }
                else if (sqlDb == SqlDataBaseType.Sqlite)
                {
                    tempGeomColumnName = Context.Database.SqlQuery<string>(SqlBuilder.GetGeometryColumnName(sqlDb, TableName, SchemaName)).FirstOrDefault();
                }
                else if (sqlDb == SqlDataBaseType.Oracle)
                {
                    tempGeomColumnName = Context.Database.SqlQuery<string>(SqlBuilder.GetGeometryColumnName(sqlDb, TableName, SchemaName)).FirstOrDefault();
                }
                
            }
            catch (Exception e)
            {
                throw e;
            }


            return tempGeomColumnName;
        }

        protected virtual void OnExecutingSqlStatement(SqlServerEvent e)
        {
            EventHandler<SqlServerEvent> handler = _sqlServerStatement;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnExecutingSqlStatement(PostGisEvent e)
        {
            EventHandler<PostGisEvent> handler = _postGisStatement;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnExecutingSqlStatement(SqliteEvent e)
        {
            EventHandler<SqliteEvent> handler = _sqliteStatement;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnExecutingSqlStatement(OracleEvent e)
        {
            EventHandler<OracleEvent> handler = _oracleStatement;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        private Collection<Feature> GetFeaturesByBoundingBox(RectangleShape boundingBox, int queryType, IEnumerable<string> columnNames)
        {
            Collection<Feature> features = new Collection<Feature>();
            string condition = string.Empty;
            string sqlStatement = string.Empty;
            string sql = string.Empty;
            List<GeoData> dataList = new List<GeoData>();
            DataTable featDataTable = new DataTable();

            List<string> fieldsList = (columnNames == null) ? new List<string>() : new List<string>(columnNames);


            if(!fieldsList.Any(x => FeatureIdColumn.ToLower().Contains(x.ToLower())))
            {
                fieldsList.Add(FeatureIdColumn);
            }

            if (!fieldsList.Any(x => GeomColumnName.ToLower().Contains(x.ToLower())))
            {
                fieldsList.Add(GeomColumnName);
            }

            switch (DataBaseType)
            {
                case (SqlDataBaseType.SqlServer):


                    _spatialDataType = GetSpatialDataType();

                    condition = SqlBuilder.GetSpatialQueryCondition(_spatialDataType, queryType, Srid, GeomColumnName);
                    sqlStatement = SqlBuilder.GetStringQuery(fieldsList, this, condition);

                    try
                    {
                        sql = GetSqlEvent(sqlStatement, queryType == 1 ? ExecutingSqlStatementType.GetFeaturesInsideBoundingBoxEx : ExecutingSqlStatementType.GetFeaturesOutsideBoundingBox);

                        SqlCommand commandSql = new SqlCommand(sql, new SqlConnection(@DataBaseSource));
                        commandSql.Parameters.Add(new SqlParameter(ParameterName, boundingBox.GetWellKnownBinary()));
                        commandSql.Connection.Open();
                        featDataTable.Load(commandSql.ExecuteReader(CommandBehavior.CloseConnection));

                        features = FeaturesTools.GetFeatures(this, featDataTable);

                    }
                    catch (Exception e)
                    {
                        if (e.Message.Contains("MakeValid") || (e.InnerException != null && e.InnerException.Message.Contains("MakeValid")))
                        {
                            throw new ArgumentException(ExceptionDescription.InvalidGeometryInMsSqlForSpatialQuery, e.InnerException);
                        }
                        else
                        {
                            throw e;
                        }
                    }
                    break;

                case (SqlDataBaseType.PostGis):

                    condition = SqlBuilder.GetSpatialQueryCondition(this, queryType, Srid, _geomFieldName, ParameterName);
                    sqlStatement = SqlBuilder.GetStringQuery(fieldsList, this, condition);

                    try
                    {
                       
                        sql = GetSqlEvent(sqlStatement, queryType == 1 ? ExecutingSqlStatementType.GetFeaturesInsideBoundingBoxEx : ExecutingSqlStatementType.GetFeaturesOutsideBoundingBox);
                        NpgsqlCommand commandSql = new NpgsqlCommand(sql,new NpgsqlConnection(@DataBaseSource));
                        commandSql.Parameters.Add(new NpgsqlParameter(ParameterName, boundingBox.GetWellKnownBinary()));
                        commandSql.Connection.Open();

                        featDataTable.Load(commandSql.ExecuteReader(CommandBehavior.CloseConnection));

                        features = FeaturesTools.GetFeatures(this, featDataTable);
                    }
                    catch (Exception e)
                    {
                        if (e.Message.Contains("MakeValid") || (e.InnerException != null && e.InnerException.Message.Contains("MakeValid")))
                        {
                            throw new ArgumentException(ExceptionDescription.InvalidGeometryInMsSqlForSpatialQuery, e.InnerException);
                        }
                        else
                        {
                            throw e;
                        }
                    }

                    break;

                case (SqlDataBaseType.Sqlite):

                    condition = SqlBuilder.GetSpatialJoinQuery(FeatureIdColumn, _virtualTableName, boundingBox);
                    sqlStatement = (SqlBuilder.GetStringQuery(fieldsList, this, condition));

                    sql = GetSqlEvent(sqlStatement, queryType == 1 ? ExecutingSqlStatementType.GetFeaturesInsideBoundingBoxEx : ExecutingSqlStatementType.GetFeaturesOutsideBoundingBox);

                    try 
                    {
                        lock (_databaseLook)
                        {
                            using (SQLiteDataAdapter data = new SQLiteDataAdapter(sql, new SQLiteConnection(@DataBaseSource)))
                            {
                                data.Fill(featDataTable);
                            }
                        }
                        
                    }
                    catch (Exception e) 
                    {
                        if (e.Message.Contains("MakeValid") || (e.InnerException != null && e.InnerException.Message.Contains("MakeValid")))
                        {
                            throw new ArgumentException(ExceptionDescription.InvalidGeometryInMsSqlForSpatialQuery, e.InnerException);
                        }
                        else
                        {
                            //throw e;
                        }
                    
                    }

                    features = FeaturesTools.GetFeatures(this, featDataTable);
                    break;

                case (SqlDataBaseType.Oracle):

                    condition = SqlBuilder.GetSpatialQueryCondition(this, queryType, Srid, _geomFieldName, ParameterName);
                    sqlStatement = SqlBuilder.GetStringQuery(fieldsList, this, condition);

                    try
                    {

                        sql = GetSqlEvent(sqlStatement, queryType == 1 ? ExecutingSqlStatementType.GetFeaturesInsideBoundingBoxEx : ExecutingSqlStatementType.GetFeaturesOutsideBoundingBox);
                        OracleCommand commandSql = new OracleCommand(sql, new OracleConnection(@DataBaseSource));
                        commandSql.Parameters.Add(new OracleParameter(ParameterName, boundingBox.GetWellKnownBinary()));
                        commandSql.Connection.Open();

                        featDataTable.Load(commandSql.ExecuteReader(CommandBehavior.CloseConnection));

                        features = FeaturesTools.GetFeatures(this, featDataTable);
                        Collection<Feature> returnRecords = new Collection<Feature>();

                        foreach (Feature insideFeature in features.Where(f => !f.GetBoundingBox().IsDisjointed(boundingBox)))
                        {
                            Feature clone = insideFeature.CloneDeep(columnNames);
                            returnRecords.Add(clone);
                        }

                        return returnRecords;

                    }
                    catch (Exception e)
                    {
                        if (e.Message.Contains("MakeValid") || (e.InnerException != null && e.InnerException.Message.Contains("MakeValid")))
                        {
                            throw new ArgumentException(ExceptionDescription.InvalidGeometryInMsSqlForSpatialQuery, e.InnerException);
                        }
                        else
                        {
                            throw e;
                        }
                    }

                    break;

            }

            return features;
        }

        private void DropIndexes(Collection<string> indexNames)
        {
            if (!DataBaseType.Equals(SqlDataBaseType.SqlServer)) return;

            if (indexNames.Count == 0) return;
            Context.Database.ExecuteSqlCommand(SqlBuilder.DropIndexes(indexNames, TableName));
        }

        private bool IsTableExist(string tableName)
        {
            if (!DataBaseType.Equals(SqlDataBaseType.SqlServer) && !DataBaseType.Equals(SqlDataBaseType.Sqlite) && !DataBaseType.Equals(SqlDataBaseType.Oracle)) return false;

            bool answer = false;

            if (DataBaseType.Equals(SqlDataBaseType.SqlServer))
            {
                answer = Context.Database.SqlQuery<bool>(SqlBuilder.IsTableExist(SchemaName, tableName, DataBaseType)).First<bool>();
            }
            else if (DataBaseType.Equals(SqlDataBaseType.Sqlite))
            {
                int num = Context.Database.SqlQuery<int>(SqlBuilder.IsTableExist(SchemaName, tableName, DataBaseType)).First<int>();
                if (num != 0)
                {
                    Context.Database.ExecuteSqlCommand(SqlBuilder.DropTable(tableName));
                }
            }
            else if(DataBaseType.Equals(SqlDataBaseType.Oracle))
            {
                int num = Context.Database.SqlQuery<int>(SqlBuilder.IsTableExist(SchemaName, tableName, DataBaseType)).First<int>();
                return Convert.ToBoolean(num);
            }
            else 
            {
                answer = Context.Database.SqlQuery<bool>(SqlBuilder.IsTableExist(SchemaName, tableName, DataBaseType)).First<bool>();
            }
            return answer;
        }

        private void DropTable(string tableName)
        {
            if (!DataBaseType.Equals(SqlDataBaseType.SqlServer)) return;

            Context.Database.ExecuteSqlCommand(SqlBuilder.DropTable(tableName));
        }
       
        private Collection<string> GetIndexName()
        {
            Collection<string> names = new Collection<string>();

            foreach (string x in Context.Database.SqlQuery<string>(SqlBuilder.GetIndexName(TableName)).ToList())
            {
                names.Add(x);
            }

            return names;
        }

        private void ProcessAddBuffer(Dictionary<string, Feature> addBuffer, TransactionResult result)
        {
            string sql = string.Empty;

            foreach (Feature feature in addBuffer.Values)
            {
                try
                {
                    if (DataBaseType.Equals(SqlDataBaseType.SqlServer)) 
                    {
                        sql = GetSqlEvent(SqlBuilder.GetInsertFeatureQuery(this, feature, _spatialDataType), ExecutingSqlStatementType.CommitTransactionEx);
                        
                        Context.Database.ExecuteSqlCommand(sql,
                            new SqlParameter(_spatialDataType.ToString(), feature.GetWellKnownBinary()));
                    }
                    else if (DataBaseType.Equals(SqlDataBaseType.PostGis))
                    {
                        if (_featureSourceColumns == null)
                        {
                            _featureSourceColumns = GetColumns();
                        }

                        Collection<NpgsqlParameter> parameters = FeaturesTools.GetSpatialColumnNames(feature, FeatureIdColumn);
                        parameters.Add(new NpgsqlParameter(ParameterName, feature.GetWellKnownBinary()));

                        sql = GetSqlEvent(SqlBuilder.GetInsertFeatureQuery(this, feature, ParameterName, _geomFieldName, _featureSourceColumns),
                            ExecutingSqlStatementType.CommitTransactionEx);

                        Context.Database.ExecuteSqlCommand(sql, parameters);

                    }
                    else if (DataBaseType.Equals(SqlDataBaseType.Sqlite)) 
                    {
                        sql = GetSqlEvent(SqlBuilder.GetInsertFeatureQuery(this, ParameterName, feature), ExecutingSqlStatementType.CommitTransactionEx);
                        Context.Database.ExecuteSqlCommand(sql, new SQLiteParameter(ParameterName,feature.GetWellKnownBinary()));
                       
                    }else if(DataBaseType.Equals(SqlDataBaseType.Oracle))
                    {
                        Collection<OracleParameter> parameters = new Collection<OracleParameter>();
                        parameters.Add(new OracleParameter(ParameterName, feature.GetWellKnownBinary()));

                        sql = GetSqlEvent(SqlBuilder.GetInsertFeatureQueryOracle(this, feature, ParameterName, _geomFieldName),
                            ExecutingSqlStatementType.CommitTransactionEx);

                        Context.Database.ExecuteSqlCommand(sql, parameters);
                    }

                    result.TotalSuccessCount++;
                }
                catch (Exception e)
                {
                    result.FailureReasons.Add(feature.Id, e.Message);
                    result.TotalFailureCount++;
                    throw e;
                }
            }
        }

        private void ProcessEditBuffer(Dictionary<string, Feature> editBuffer, TransactionResult transactionResult)
        {
            string sql = string.Empty;

            foreach (string id in editBuffer.Keys)
            {
                try
                {
                    if (DataBaseType.Equals(SqlDataBaseType.SqlServer)) 
                    {

                        sql = GetSqlEvent(SqlBuilder.GetUpdateFeatureQuery(this, _spatialDataType, id, editBuffer[id]), ExecutingSqlStatementType.CommitTransactionEx);
                    
                        Context.Database.ExecuteSqlCommand(sql,
                            new SqlParameter(_spatialDataType.ToString(), editBuffer[id].GetWellKnownBinary()));
                    }
                    else if (DataBaseType.Equals(SqlDataBaseType.PostGis))
                    {
                        Collection<string> spetialColumnNames = new Collection<string>();
                        sql = SqlBuilder.GetUpdateFeatureQuery(this, editBuffer[id], _geomFieldName, id, ParameterName);

                        Collection<NpgsqlParameter> parameters = FeaturesTools.GetSpatialColumnNames(editBuffer[id], FeatureIdColumn);
                        parameters.Add(new NpgsqlParameter(ParameterName, editBuffer[id].GetWellKnownBinary()));

                        Context.Database.ExecuteSqlCommand(sql, parameters);
                    }

                    transactionResult.TotalSuccessCount++;
                }
                catch (Exception e)
                {
                    transactionResult.FailureReasons.Add(id, e.Message);
                    transactionResult.TotalFailureCount++;
                    throw e;
                }
            }
        }

        private void ProcessDeleteBuffer(Collection<string> deleteBuffer, TransactionResult transactionResult)
        {
            if (deleteBuffer.Count == 0) return;

            string sql = GetSqlEvent(SqlBuilder.GetDeleteQuery(this, deleteBuffer), ExecutingSqlStatementType.CommitTransactionEx);

            try
            { 
                Context.Database.ExecuteSqlCommand(sql);
            }
            catch (Exception e)
            {
                transactionResult.TotalSuccessCount += deleteBuffer.Count;
                throw e;
            }
        }

        private void BuildIndex()
        {
            if (_spatialDataType == SpatialDataType.Geometry)
            {
                RectangleShape boundingBox = GetBoundingBoxCore();
                Context.Database.ExecuteSqlCommand(SqlBuilder.GetBuildIndex(TableName, GeomColumnName, boundingBox));
            }
            else
            {
                Context.Database.ExecuteSqlCommand(SqlBuilder.GetBuildIndex(TableName, GeomColumnName, null));
            }
        }

        private bool CheckIndexExist()
        {
            return Context.Database.SqlQuery<bool>(SqlBuilder.IsIndexExist(TableName)).First<bool>();
        }

        private void CreateSqliteBoundingTable(string tableName) 
        {
           System.Diagnostics.Debug.WriteLine(tableName);
           Context.Database.ExecuteSqlCommand(SqlBuilder.CreateVirtualTableQuery(tableName));
        }

        private void FillBoundingTable(string tableName) 
        {
            DataTable data = new DataTable();

            using (SQLiteDataAdapter dataAdapter = new SQLiteDataAdapter(SqlBuilder.GetStringQuery(new List<string> { FeatureIdColumn , GeomColumnName }, this, string.Empty), 
                                                       @DataBaseSource))
            {
                dataAdapter.Fill(data);
            }
            string query = SqlBuilder.GetInsert(new List<string> { "id", "minx", "maxx", "miny", "maxy" }, data, tableName);

            Context.Database.ExecuteSqlCommand(query);
        }

        #endregion

        #region private Oracle utilities

        private void CreateOracleTempSpatialTable()
        {
            var allColumns = this.GetColumnsCore();
            List<string> columns = new List<string>();
            foreach(FeatureSourceColumn column in allColumns)
            {
                columns.Add(column.ColumnName);
            }

            List<string> fieldsList = (columns == null) ? new List<string>() : new List<string>(columns);

            if (!fieldsList.Any(x => FeatureIdColumn.ToLower().Contains(x.ToLower())))
            {
                fieldsList.Add(FeatureIdColumn);
            }

            if (!fieldsList.Any(x => _geomFieldName.ToLower().Contains(x.ToLower())))
            {
                fieldsList.Add(_geomFieldName);
            }

            string sql = SqlBuilder.BuildSelectQueryTempTableOracle(fieldsList, this);
            
            Context.Database.ExecuteSqlCommand(SqlBuilder.CreateTempTableOracle(this.SchemaName, this.TableName, sql));
            this.FillOracleTempSpatialTable(fieldsList);
            Context.Database.ExecuteSqlCommand(SqlBuilder.InsertSpatialMetadataTempTableOracle(this.TableName));
            Context.Database.ExecuteSqlCommand(SqlBuilder.CreateSpatialTempIndex(this.SchemaName, this.TableName, _geomFieldName));
        }

        private void FillOracleTempSpatialTable(IEnumerable<string> columnNames)
        {
            int countFeatures = this.GetCountCore();
            int pages = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(Convert.ToDouble(countFeatures) / 10000)));

            for(int i = 0; i < pages; i++)
            {
                string sqlInsert = SqlBuilder.InsertQueryTemTableOracle(columnNames, this, i);
                Context.Database.ExecuteSqlCommand(sqlInsert);
            }
        }


        private void DropOracleTempSpatialTable()
        {
            Context.Database.ExecuteSqlCommand(SqlBuilder.DropSpatialTempIndex(this.SchemaName, this.TableName, _geomFieldName));
            Context.Database.ExecuteSqlCommand(SqlBuilder.DeleteSpatialMetadataTempTableOracle(this.TableName));            
            Context.Database.ExecuteSqlCommand(SqlBuilder.DropTempTableOracle(this.SchemaName, this.TableName));
        }

        #endregion

    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;

using Mapgenix.Canvas;
using Mapgenix.FeatureSource.Properties;
using Mapgenix.Shapes;
using Mapgenix.Utils;

namespace Mapgenix.FeatureSource
{
    /// <summary>
    /// 	Abstract class for all feature sources. Feature
    ///     sources represent feature data to be integrated into GSuite.
    ///     <para>Abstract class from which all other feature
    ///     sources are derived. It encapsulates the logic for handling
    ///     transactions and ensuring the data is consistent regardless of the projections used.</para>
    /// </summary>
    [Serializable]
    public abstract class BaseFeatureSource
    {
        private const int MaximumLoopCount = 100;
        private const int ProgressDrawingRaisingFrenquency = 200;
        private int _progressDrawingRaisedCount;
        private bool _isOpen;
        private bool _isInTransaction;
        private bool _isTransactionLive;
        private TransactionBuffer _transactionBuffer;
        private Collection<FeatureSourceColumn> _featureSourceColumns;
        private Collection<string> _featureIdsToExclude;
        private BaseProjection _projection;
        private FeatureCache _geoCache;
        public event EventHandler<ProgressEventArgs> DrawingProgressChanged;

        /// <summary>Default constructor for the abstract BaseFeatureSource class.</summary>
        /// <returns>None</returns>
        /// <remarks>As protected, add code to this method if you override it from an inheriting class.</remarks>
        protected BaseFeatureSource()
        {
            _progressDrawingRaisedCount = 0;
            _geoCache = new FeatureCache();
            _transactionBuffer = new TransactionBuffer();
            _featureIdsToExclude = new Collection<string>();
        }

        /// <summary>Raised when fields are requested in a feature source method that do
        /// not exist in the feature source. It allows to supplement the data from an outside
        /// source.</summary>
        /// <remarks><br/>Used primarily when you have data relating to a particular feature or set of
        /// features that is not within source of the data.<br/>
        /// 	<br/>
        /// To integrate this SQL data, you simply create a file name that does not exist in the
        /// .dbf file.  Whenever GSuite is queried to return records that specifically require
        /// this field, the BaseFeatureSource will raise this event and allow the developer to
        /// supply the data. In this way, you can query the SQL table and store the
        /// data in some sort of collection, and then when the event is raised, simply supply that
        /// data.<br/>
        /// 	<br/>
        /// As this is an event, it will raise for each feature and field combination requested.
        /// This means that the event can be raised quite often, and we suggest that you cache the
        /// data you wish to supply in memory. We recommend against sending out a new SQL query
        /// each time this event is raised. 
        /// </remarks>
        public event EventHandler<ColumnFetchEventArgs> CustomColumnFetch;

        /// <summary>Raised after the CommitTransaction method is called, but before the
        /// CommitTransactionCore is called.  This allows access to the TransactionBuffer before the
        /// transaction is committed. It also allows to cancel the pending commit of the
        /// transaction.
        /// </summary>
        /// <remarks>
        /// This event is raised before the CommitTransactionCore is called and allows
        /// access to the TransactionBuffer before the transaction is committed. It also allows
        /// to cancel the pending transaction. The TransactionBuffer is the object that stores all
        /// of the pending transactions and is accessible through this event to allow to either
        /// add, remove or modify transactions.<br/>
        /// 	<br/>
        /// In the event that you cancel the CommitTransaction method, the transaction remains intact and
        /// you will still be editing. This makes it a nice place to possibly check for
        /// connectivity before the TransactionCore code is run, which is where the records are
        /// actually committed. Calling the RollBackTransaction method is the only way to
        /// permanently cancel a pending transaction without committing it.
        /// </remarks>
        public event EventHandler<CommitTransactionEventArgs> CommittingTransaction;

        /// <summary>Raised after the CommitTransaction and the CommitTransactionCore
        /// are called and allows access to the TransactionBuffer and the TransactionResults
        /// object before CommitTransaction method is returned.</summary>
        /// <remarks>
        /// 	<para>This event is raised after the CommitTransactionCore is called and allows
        ///     access to the TransactionBuffer and the TransactionResults object before
        ///     CommitTransaction method is returned.<br/>
        /// 		<br/>
        ///     With this event, you can analyse the results of the transaction and do any cleanup
        ///     code necessary. In the event some of the records did not commit, you can handle
        ///     those items here. The TransactionResults object is passed out of the
        ///     CommitTransaction method so you could analyze it then; however, this is the only
        ///     place where you have access to both the TransactionResults object and the
        ///     TransactionBuffer object at the same time. These are useful together to try and
        ///     determine what went wrong and possibly try and re-commit them.<br/>
        /// 		<br/>
        ///     At the time of this event you will technically be out of the current
        ///     transaction.</para>
        /// </remarks>
        public event EventHandler<CommitTransactionEventArgs> CommittedTransaction;

        /// <summary>Raised before the opening of the FeatureSource.</summary>
        /// <remarks>Raised before the opening of the FeatureSource. Technically, this
        /// event is called after the calling of the Open method on the BaseFeatureSource, but before
        /// the protected OpenCore method.<br/>
        /// 	<br/>
        /// It is typical that the BaseFeatureSource may be opened and closed may times during the life
        /// cycle of your application.</remarks>
        public event EventHandler<EventArgs> OpeningFeatureSource;

        /// <summary>Raised after the opening of the BaseFeatureSource.</summary>
        /// <remarks>
        /// This event is called after the opening of the BaseFeatureSource. Technically, this
        /// event is called after the calling of the Open method on the BaseFeatureSource and after the
        /// protected OpenCore method is called.<br/>
        /// 	<br/>
        /// It is typical that the BaseFeatureSource may be opened and closed may times during the life
        /// cycle of your application. </remarks>
        public event EventHandler<EventArgs> OpenedFeatureSource;

        /// <summary>Raised before the closing of the BaseFeatureSource.</summary>
        /// <remarks>
        /// This event is called before the closing of the BaseFeatureSource. Technically, this
        /// event is called after the calling of the Close method on the BaseFeatureSource, but before
        /// the protected CloseCore method.<br/>
        /// 	<br/>
        /// It is typical that the FeatureSource may be opened and closed may times during the life
        /// cycle of your application.</remarks>
        public event EventHandler<EventArgs> ClosingFeatureSource;

        /// <summary>Raised after the closing of the BaseFeatureSource.</summary>
        /// <remarks>
        /// This event is called after the closing of the BaseFeatureSource. Technically, this
        /// event is called after the calling of the Close method on the BaseFeatureSource and after
        /// the protected CloseCore method.<br/>
        /// 	<br/>
        /// It is typical that the BaseFeatureSource may be opened and closed may times during the life
        /// cycle of your application.</remarks>
        public event EventHandler<EventArgs> ClosedFeatureSource;

        /// <summary>Allows to raise the CustomColumnFetch event from a derived class.</summary>
        /// <returns>None</returns>
        /// <remarks>
        /// You can call this method from a derived class to enable it to raise the
        /// CustomColumnFetch event. This may be useful if you plan to extend BaseFeatureSource and
        /// you need access to user-definable field data.<br/>
        /// 	<br/>
        /// Details on the event:<br/>
        /// 	<br/>
        /// This event is raised when fields are requested in a feature source method that do not
        /// exist in the feature source. It allows you supplement the data from any outside source
        /// you may have.<br/>
        /// 	<br/>
        /// It is used primarily when you have data relating to a particular feature or set of
        /// features that is not within source of the data. For example, you may have a shapefile of countries
        /// whose .dbf component describes some characterisctis of each country.
        /// Additionally, in an outside SQL Server table, you may also have data about the countries,
        /// and it is this data that you wish to use for determining how you want to color
        /// each country.<br/>
        /// 	<br/>
        /// To integrate this SQL data, you simply create a file name that does not exist in the
        /// .dbf file.  Whenever GSuite is queried to return records that specifically require
        /// this field, the FeatureSource will raise this event and allow the developer to
        /// supply the data. In this way, you can query the SQL table and store the
        /// data in some sort of collection, and then when the event is raised, simply supply that
        /// data.<br/>
        /// 	<br/>
        /// As this is an event, it will raise for each feature and field combination requested.
        /// This means that the event can be raised quite often, and we suggest that you cache the
        /// data you wish to supply in memory. We recommend against sending out a new SQL query
        /// each time this event is raised.</remarks>
        /// <param name="e">
        /// This parameter is the event arguments which define the parameters passed to the
        /// recipient of the event.
        /// </param>
        protected virtual void OnCustomColumnFetch(ColumnFetchEventArgs e)
        {
            EventHandler<ColumnFetchEventArgs> handler = CustomColumnFetch;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>Allows to raise the CommittingTransaction event from a derived class.</summary>
        /// <returns>None</returns>
        /// <remarks>
        /// You can call this method from a derived class to enable it to raise the
        /// CommittingTransaction event. This may be useful if you plan to extend the FeatureSource
        /// and you need access to the event.<br/>
        /// 	<br/>
        /// Details on the event:<br/>
        /// 	<br/>
        /// This event is raised before the CommitTransactionCore is called and allows you access
        /// to the TransactionBuffer before the transaction is committed. It also allows to
        /// cancel the pending transaction. The TransactionBuffer is the object that stores all of
        /// the pending transactions and is accessible through this event to allow to either add,
        /// remove or modify transactions.<br/>
        /// 	<br/>
        /// In the event that you cancel the CommitTransaction method, the transaction remains intact and
        /// you will still be editing. This makes it a nice place to possibly check for
        /// connectivity before the TransactionCore code is run, which is where the records are
        /// actually committed. Calling the RollBackTransaction method is the only way to
        /// permanently cancel a pending transaction without committing it.
        /// </remarks>
        /// <param name="e">Arguments which define the parameters passed to the recipient of the event.</param>
        protected virtual void OnCommittingTransaction(CommitTransactionEventArgs e)
        {
            EventHandler<CommitTransactionEventArgs> handler = CommittingTransaction;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <returns>None</returns>
        /// <summary>Allows to raise the CommittedTransaction event from a derived class.</summary>
        /// <remarks>
        ///     You can call this method from a derived class to enable it to raise the
        ///     CommittedTransaction event. This may be useful if you plan to extend the
        ///     FeatureSource and you need access to the event.<br/>
        /// 	<br/>
        ///     Details on the event:<br/>
        /// 	<br/>
        /// 	<para>This event is raised after the CommitTransactionCore is called and allows
        ///     access to the TransactionBuffer and the TransactionResults object before
        ///     CommitTransaction method is returned.<br/>
        /// 		<br/>
        ///     With this event, you can analyze the results of the transaction and do any cleanup
        ///     code necessary. In the event some of the records did not commit, you can handle
        ///     these items here. The TransactionResults object is passed out of the
        ///     CommitTransaction method so you could analyze it then; however, this is the only
        ///     place where you have access to both the TransactionResults object and the
        ///     TransactionBuffer object at the same time. These are useful together to try and
        ///     determine what went wrong and possibly try and re-commit them.<br/>
        /// 		<br/>
        ///     At the time of this event, you will technically be out of the current
        ///     transaction.</para>
        /// </remarks>
        /// <param name="e">Event arguments which define the parameters passed to the
        /// recipient of the event.
        /// </param>
        protected virtual void OnCommittedTransaction(CommitTransactionEventArgs e)
        {
            EventHandler<CommitTransactionEventArgs> handler = CommittedTransaction;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <returns>None</returns>
        /// <summary>Allows to raise the OpeningFeatureSource event from a derived
        /// class.
        /// </summary>
        /// <remarks>
        /// You can call this method from a derived class to enable it to raise the
        /// OpeningFeatureSource event. Usefull to extend BaseFeatureSource
        /// and you need access to the event.<br/>
        /// 	<br/>
        /// Details on the event:<br/>
        /// 	<br/>
        /// This event is called before the opening of the BaseFeatureSource. Technically, this event is
        /// called after the calling of the Open method on BaseFeatureSource, but before the
        /// protected OpenCore method.<br/>
        /// 	<br/>
        /// It is typical that the FeatureSource may be opened and closed may times during the life
        /// cycle of your application.</remarks>
        /// <param name="e">Event arguments defining the parameters passed to the
        /// recipient of the event.</param>
        protected virtual void OnOpeningFeatureSource(EventArgs e)
        {
            EventHandler<EventArgs> handler = OpeningFeatureSource;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <returns>None</returns>
        /// <summary>Allows to raise the OpenedFeatureSource event from a derived class.</summary>
        /// <remarks>Call this method from a derived class to enable it to raise the
        /// OpenedFeatureSource event. Usefull to extend BaseFeatureSource and you need access to the event.<br/>
        /// 	<br/>
        /// Details on the event:<br/>
        /// 	<br/>
        /// Raised after the opening of BaseFeatureSource. Technically, this event is
        /// raised after the calling of the Open method on BaseFeatureSource and after
        /// protected OpenCore method is called.<br/>
        /// 	<br/>
        /// It is typical that the FeatureSource may be opened and closed may times during the life
        /// cycle of an application.</remarks>
        /// <param name="e">Event arguments defining the parameters passed to the recipient of the event.</param>
        protected virtual void OnOpenedFeatureSource(EventArgs e)
        {
            EventHandler<EventArgs> handler = OpenedFeatureSource;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <returns>None</returns>
        /// <summary>Allows to raise the ClosingFeatureSource event from a derived class.</summary>
        /// <remarks>
        /// You can call this method from a derived class to enable it to raise the
        /// ClosingFeatureSource event. Useful to extend the FeatureSource
        /// and you need access to the event.<br/>
        /// 	<br/>
        /// Details on the event:<br/>
        /// 	<br/>Raised before the closing of the FeatureSource. Technically, this event is
        /// called after the calling of the Close method on the FeatureSource, but before the
        /// protected CloseCore method.<br/>
        /// 	<br/>
        /// It is typical that the FeatureSource may be opened and closed may times during the life
        /// cycle of your application. 
        /// </remarks>
        /// <param name="e">Event arguments defining the parameters passed to the recipient of the event.</param>
        protected virtual void OnClosingFeatureSource(EventArgs e)
        {
            EventHandler<EventArgs> handler = ClosingFeatureSource;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <returns>None</returns>
        /// <summary>Allows to raise the ClosedFeatureSource event from a derived class.</summary>
        /// <remarks>Call this method from a derived class to enable it to raise the
        /// ClosedFeatureSource event. Useful to extend the FeatureSource
        /// and you need access to the event.<br/>
        /// 	<br/>
        /// Details on the event:<br/>
        /// 	<br/>
        /// Raised after the closing of BaseFeatureSource. Technically, this event is
        /// called after the calling of the Close method on the FeatureSource and after the
        /// protected CloseCore method.<br/>
        /// 	<br/>
        /// It is typical that the FeatureSource may be opened and closed may times during the life
        /// cycle of your application. 
        /// </remarks>
        /// <param name="e">Event arguments defining the parameters passed to the
        /// recipient of the event.
        /// </param>
        protected virtual void OnClosedFeatureSource(EventArgs e)
        {
            EventHandler<EventArgs> handler = ClosedFeatureSource;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>Specifies whether BaseFeatureSource can excute a SQL query or not.
        /// If it is false, it throws an exception when these APIs are called: ExecuteScalar, ExecuteNonQuery, ExecuteQuery</summary>
        /// <remarks>Default implementation is false.</remarks>
        public bool CanExecuteSqlQuery
        {
            get { return CanExecuteSqlQueryCore; }
        }

        /// <summary>Specifies whether BaseFeatureSource can excute a SQL query or not.
        /// If it is false, it throws an exception when these APIs are calleds: ExecuteScalar, ExecuteNonQuery, ExecuteQuery
        /// </summary>
        /// <remarks>Default implementation is false.</remarks>
        protected virtual bool CanExecuteSqlQueryCore
        {
            get { return false; }
        }

        /// <summary>Returns true if BaseFeatureSource is open and false if it is not.</summary>
        /// <returns>True if the FeatureSource is open.</returns>
        /// <remarks>
        /// Various methods on BaseFeatureSource require it to be in an open state. If one
        /// of those methods is called when the state is not open, the method throws an
        /// exception. To enter the open state, call BaseFeatureSource Open method. The
        /// method raises an exception if the current BaseFeatureSource is already open.
        /// </remarks>
        public bool IsOpen
        {
            get
            {
                return IsOpenCore;
            }
        }

        /// <summary>Returns true if BaseFeatureSource is open.</summary>
        /// <returns>True if the FeatureSource is open.</returns>
        /// <remarks>
        /// Various methods on BaseFeatureSource require that it be in an open state. If one
        /// of those methods is called when the state is not open, the method throws an
        /// exception. To enter the open state, call BaseFeatureSource Open method. The
        /// method raises an exception if the current FeatureSource is already open.
        /// </remarks>
        protected virtual bool IsOpenCore
        {
            get
            {
                return _isOpen;
            }
            set
            {
                _isOpen = value;
            }
        }

        /// <summary>Gets a collection of columns of the feature source.</summary>
        /// <returns>Collection of columns of the feature source.</returns>
        /// <remarks>None.</remarks>
        protected Collection<FeatureSourceColumn> FeatureSourceColumns
        {
            get
            {
                return _featureSourceColumns;
            }
        }

        /// <summary>Returns true if BaseFeatureSource is in a transaction.</summary>
        /// <returns>True if BaseFeatureSource is in a transaction.</returns>
        /// <remarks>
        /// To enter a transaction, call BeginTransaction method of BaseFeatureSource. To end a transaction, call either CommitTransaction or
        /// RollbackTransaction.</remarks>
        public bool IsInTransaction
        {
            get
            {
                return _isInTransaction;
            }
        }

        /// <summary>Returns true if the features currently modified in a transaction
        /// are expected to reflect their state when calling other methods on BaseFeatureSource,
        /// such as spatial queries.</summary>
        /// <returns>True if the features currently modified in a transaction
        /// are expected to reflect their state when calling other methods on the
        /// FeatureSource.</returns>
        /// <remarks>
        /// The live transaction means that all of the modifications performed during
        /// a transaction are live from the standpoint of the querying methods on the object.</remarks>
        public bool IsTransactionLive
        {
            get
            {
                return _isTransactionLive;
            }
            set
            {
                _isTransactionLive = value;
            }
        }

        /// <summary>Returns whether BaseFeatureSource allows editing or is read-only.</summary>
        /// <returns>Whether BaseFeatureSource allows editing or is read-only.</returns>
        /// <remarks>
        /// 	<para>This property is useful to check if a specific FeatureSource accepts editing.
        ///     If you call BeginTransaction and this property is false, then an exception will
        ///     be raised.<br/>
        /// 		<br/>
        ///     For developers who are creating or extending BaseFeatureSource, it is expected to override this virtual method 
        ///     if the new BaseFeatureSource created allows editing. By default, it is false, meaning that to allow editing you
        ///     must override this method and return true.</para>
        /// </remarks>
        public virtual bool IsEditable
        {
            get { return false; }
        }


        /// <summary>Holds the projection object used within BaseFeatureSource to ensure that features inside are projected.</summary>
        /// <returns>Map projections selected by the developer for the feature source.</returns>
        /// <remarks>
        /// By default this property is null, meaning that the data being passed back from any
        /// methods on BaseFeatureSource is in the coordinate system of the raw data. When you
        /// specify a projection object in the property, all incoming and outgoing method
        /// calls will subject the features to projection.<br/>
        /// </remarks>
        public BaseProjection Projection
        {
            get
            {
                return _projection;
            }
            set
            {
                _projection = value;
            }
        }

        /// <summary>Cache system.</summary>
        /// <remarks>Set IsActive to true for the Cache system. By default it is not active.</remarks>
        public FeatureCache GeoCache
        {
            get { return _geoCache; }
            set { _geoCache = value; }
        }

        /// <summary>Collection of strings representing record id of features to exclude from the Layer.</summary>
       public Collection<string> FeatureIdsToExclude
        {
            get { return _featureIdsToExclude; }
        }

       /// <summary>TransactionBuffer used in the Transaction System.</summary>
       /// <remarks>
       /// 	<br/>
       /// The Transaction System<br/>
       /// 	<br/>
       /// The transaction system of a BaseFeatureSource is on top of the inherited implementation
       /// of any specific source. In this way, it functions the same way for every feature source. 
       /// First call BeginTransaction. This allocates a collection of in-memory change buffers that are used to store changes until
       /// the transaction is commited. So, for example, when Add, Delete or Update
       /// method are called, the changes to the feature are stored in memory only. If you
       /// choose to abandon the transaction, call RollbackTransaction at any time and the
       /// in-memory buffer will be deleted and the changes will be lost. When you are ready to
       /// commit the transaction, call CommitTransaction and the collections of changes
       /// are then passed to the CommitTransactionCore method and the implementer of the specific
       /// BaseFeatureSource is responsible for integrating the changes into the underlying
       /// BaseFeatureSource. By default the IsLiveTransaction property is set to false, meaning
       /// that until the changes are committed, the BaseFeatureSource APIs will not reflect any changes
       /// that are in the temporary editing buffer.<br/>
       /// 	<br/>
       /// In  case where the IsLiveTransaction is set to true, then it is a bit different.
       /// The live transaction means that all of the modifications
       /// performed during a transaction are live from the standpoint of the querying methods on
       /// the object.<br/>
       /// </remarks>
        public TransactionBuffer TransactionBuffer
        {
            get
            {
                return _transactionBuffer;
            }
            set
            {
                _transactionBuffer = value;
            }
        }

        /// <summary>Executes a SQL statement against a connection object.</summary>
        /// <returns>Number of rows affected.</returns>
        /// <param name="sqlStatement">sqlStatement to execute.</param>
        public int ExecuteNonQuery(string sqlStatement)
        {
            Validators.CheckFeatureSourceIsOpen(IsOpen);
            Validators.CheckParameterIsNotNullOrEmpty(sqlStatement, "sqlStatement");
            Validators.CheckFeatureSourceCanExecuteSqlQuery(CanExecuteSqlQuery);

            return ExecuteNonQueryCore(sqlStatement);
        }

        /// <summary>Executes a SQL statement against a connection object.</summary>
        /// <returns>Number of rows affected.</returns>
        /// <param name="sqlStatement">sqlStatement to execute.</param>
        protected virtual int ExecuteNonQueryCore(string sqlStatement)
        {
            throw new InvalidOperationException(ExceptionDescription.FeatureSourceCanNotExecuteSqlQuery);
        }

        /// <summary>Gets collection of features by passing a columnName and a specified columValue.</summary>
        /// <returns>Collection of features matching the columnValue.</returns>
        /// <param name="columnName">Column name  to match the column value.</param>
        /// <param name="columnValue">Column value to match those returning features.</param>
        /// <param name="returningColumnType">Type of ReturningColumnsType for each Feature.</param>
        public Collection<Feature> GetFeaturesByColumnValue(string columnName, string columnValue, ReturningColumnsType returningColumnType)
        {
            Validators.CheckFeatureSourceIsOpen(IsOpen);
            Validators.CheckParameterIsNotNullOrEmpty(columnName, "columnName");
            Validators.CheckReturningColumnNamesTypeIsValid(returningColumnType, "returningColumnType");

            Collection<string> returningColumnNames = GetReturningColumnNames(returningColumnType);

            return GetFeaturesByColumnValue(columnName, columnValue, returningColumnNames);

        }

        /// <summary>Gets collection of features by passing a columnName and a specified columValue.</summary>
        /// <returns>Collection of features matching the columnValue.</returns>
        /// <param name="columnName">Column name  to match the column value.</param>
        /// <param name="columnValue">Column value to match those returning features.</param>
        /// <param name="returningColumnNames">Columns for each feature.</param>
        public Collection<Feature> GetFeaturesByColumnValue(string columnName, string columnValue, IEnumerable<string> returningColumnNames)
        {
            Validators.CheckFeatureSourceIsOpen(IsOpen);
            Validators.CheckParameterIsNotNullOrEmpty(columnName, "columnName");
            Validators.CheckParameterIsNotNull(returningColumnNames, "returningColumnNames");

            Collection<Feature> returnFeatures = GetFeaturesByColumnValueCore(columnName, columnValue, returningColumnNames);

            if (Projection != null)
            {
                returnFeatures = ConvertToExternalProjection(returnFeatures);
            }

            return returnFeatures;
        }

        /// <summary>Gets collection of features by passing a columnName and a specified columValue.</summary>
        /// <returns>Collection of features matching the columnValue.</returns>
        /// <param name="columnName">Column name  to match the column value.</param>
        /// <param name="columnValue">Column value to match those returning features.</param>
        public Collection<Feature> GetFeaturesByColumnValue(string columnName, string columnValue)
        {
            Validators.CheckFeatureSourceIsOpen(IsOpen);
            Validators.CheckParameterIsNotNullOrEmpty(columnName, "columnName");

            Collection<string> colNames = GetReturningColumnNames(ReturningColumnsType.AllColumns);
            Collection<Feature> returnFeatures = GetFeaturesByColumnValue(columnName, columnValue, colNames);

            return returnFeatures;
        }

        /// <summary>Gets collection of features by passing a columnName and a specified columValue.</summary>
        /// <returns>Collection of features matching the columnValue.</returns>
        /// <param name="columnName">Column name  to match the column value.</param>
        /// <param name="columnValue">Column value to match those returning features.</param>
        /// <param name="returningColumnNames">Columns for each feature.</param>
        protected virtual Collection<Feature> GetFeaturesByColumnValueCore(string columnName, string columnValue, IEnumerable<string> returningColumnNames)
        {
            Validators.CheckFeatureSourceIsOpen(IsOpen);
            Validators.CheckParameterIsNotNullOrEmpty(columnName, "columnName");
            Validators.CheckParameterIsNotNull(returningColumnNames, "returningColumnNames");

            Collection<Feature> returnFeatures = new Collection<Feature>();

            List<string> columnNames = new List<string>(returningColumnNames);
            if (columnNames.FindIndex(x => x.Equals(columnName, StringComparison.OrdinalIgnoreCase)) == -1)
            {
                columnNames.Add(columnName);
            }

            Collection<Feature> allFeatures = GetAllFeaturesCore(columnNames);

            for (int i = 0; i < allFeatures.Count; i++)
            {
                if (allFeatures[i].GetWellKnownBinary() != null && String.CompareOrdinal(allFeatures[i].ColumnValues[columnName], columnValue) == 0)
                {
                    returnFeatures.Add(allFeatures[i]);
                }
            }

            return returnFeatures;
        }

        /// <summary>
        /// Executes the query and returns the first column of the first row in the result
        /// set returned by the query.
        /// </summary>
        /// <returns>First column of the first row in the result set.</returns>
        /// <remarks>
        /// Use the ExcuteScalar method to retrieve a single value from the database. 
        /// </remarks>
        /// <param name="sqlStatement">sqlStatement to execute.</param>
        public object ExecuteScalar(string sqlStatement)
        {
            Validators.CheckFeatureSourceIsOpen(IsOpen);
            Validators.CheckParameterIsNotNullOrEmpty(sqlStatement, "sqlStatement");
            Validators.CheckFeatureSourceCanExecuteSqlQuery(CanExecuteSqlQuery);

            return ExecuteScalarCore(sqlStatement);
        }

        /// <summary>
        /// Executes the query and returns the first column of the first row in the result
        /// set returned by the query.
        /// </summary>
        /// <returns>First column of the first row in the result set.</returns>
        /// <remarks>
        /// Use ExcuteScalar method to retrieve a single value from the database. 
        /// </remarks>
        /// <param name="sqlStatement">sqlStatement to execute.</param>
        protected virtual object ExecuteScalarCore(string sqlStatement)
        {
            Validators.CheckFeatureSourceIsOpen(IsOpen);
            Validators.CheckParameterIsNotNullOrEmpty(sqlStatement, "sqlStatement");
            Validators.CheckFeatureSourceCanExecuteSqlQuery(CanExecuteSqlQuery);

            DataTable dt = ExecuteQuery(sqlStatement);

            object returnValue = null;
            if (dt.Rows.Count > 0)
            {
                returnValue = dt.Rows[0][0];
            }

            return returnValue;
        }

        /// <summary>
        /// Executes the query and returns the result of the query.
        /// </summary>
        /// <returns>Result set in the format of dataTable.</returns>
        /// <remarks>
        /// Use ExecuteScalar method to retrieve a single value from the database. 
        /// </remarks>
        /// <param name="sqlStatement">SqlStatement to execute.</param>
        public DataTable ExecuteQuery(string sqlStatement)
        {
            Validators.CheckFeatureSourceIsOpen(IsOpen);
            Validators.CheckParameterIsNotNullOrEmpty(sqlStatement, "sqlStatement");
            Validators.CheckFeatureSourceCanExecuteSqlQuery(CanExecuteSqlQuery);

            return ExecuteQueryCore(sqlStatement);
        }

        /// <summary>
        /// Executes the query and returns the result of the query.
        /// </summary>
        /// <returns>Result set in the format of dataTable.</returns>
        /// <remarks>
        /// Use ExecuteScalar method to retrieve a single value from the database. 
        /// </remarks>
        /// <param name="sqlStatement">SqlStatement to execute.</param>
        protected virtual DataTable ExecuteQueryCore(string sqlStatement)
        {
            throw new InvalidOperationException(ExceptionDescription.FeatureSourceCanNotExecuteSqlQuery);
        }

        /// <summary>Returns the collection of columns available for the feature source and caches
        /// them.</summary>
        /// <returns>Collection of columns available for the feature source.</returns>
        /// <remarks>
        /// As the concrete method wrapping GetColumnsCore, note that this method will cache the results to GetColumnsCore. 
        /// It means that the first time this method is called it calls GetCollumnsCore, which is protected,
        /// and cache the results. The next time this method is called it does not call
        /// GetColumnsCore again.<br/>
        /// 	<br/>
        /// As a concrete public method that wraps a Core method, Mapgenix reserves the right
        /// to add events and other logic returned by the Core version
        /// of the method.</remarks>
        public Collection<FeatureSourceColumn> GetColumns()
        {
            Validators.CheckFeatureSourceIsOpen(IsOpen);

            if (_featureSourceColumns == null)
            {
                _featureSourceColumns = GetColumnsCore();
            }
            return _featureSourceColumns;
        }

        /// <summary>Returns the collection of columns available for the feature source and caches
        /// them.</summary>
        /// <returns>Collection of columns available for the feature source.</returns>
        /// <remarks>
        /// As the concrete method wrapping GetColumnsCore, note that this method will cache the results to GetColumnsCore. 
        /// It means that the first time this method is called it calls GetCollumnsCore, which is protected,
        /// and cache the results. The next time this method is called it does not call
        /// GetColumnsCore again.<br/>
        /// 	<br/>
        /// As a concrete public method that wraps a Core method, Mapgenix reserves the right
        /// to add events and other logic returned by the Core version
        /// of the method.</remarks>
        protected virtual Collection<FeatureSourceColumn> GetColumnsCore()
        {
            return new Collection<FeatureSourceColumn>();
        }

        /// <summary>Refreshes the columns of BaseFeatureSource and caches them.</summary>
        /// <remarks>
        /// None.
        /// </remarks>
        public void RefreshColumns()
        {
            Validators.CheckFeatureSourceIsOpen(IsOpen);

            _featureSourceColumns = GetColumnsCore();
        }

        /// <summary>Returns the count of the number of records in the BaseFeatureSource.</summary>
        /// <returns>Count of the number of records in the FeatureSource.</returns>
        /// <remarks>Concrete wrapper pf virtual method GetCountCore.<br/>
        /// 	<br/>
        /// The default implementation of GetCountCore uses the GetAllRecordsCore method to
        /// calculate how many records in BaseFeatureSource.<br/>
        /// 	<br/>
        /// As a concrete public method that wraps a Core method, Mapgenix reserves the right
        /// to add events and other logic returned by the Core version of the method.</remarks>
        public int GetCount()
        {
            Validators.CheckFeatureSourceIsOpen(IsOpen);

            int count = GetCountCore();
            if (IsInTransaction && IsTransactionLive)
            {
                count += _transactionBuffer.AddBuffer.Count;
                count -= _transactionBuffer.DeleteBuffer.Count;
            }

            if (count < 0) { count = 0; }

            return count;
        }

        /// <summary>Returns number of records in the BaseFeatureSource.</summary>
        /// <returns>Number of records in the BaseFeatureSource.</returns>
        /// <remarks>
        /// 	<para>Protected virtual method called from concrete public method
        ///     GetCount. It does not take into account any transaction activity, being handled by the concrete public method GetCount.<br/>
        /// 		<br/>
        ///     The default implementation of GetCountCore uses the GetAllRecordsCore method to
        ///     calculate how many records there are in BaseFeatureSource.<br/>
        /// 		<br/>
        ///     If you do not override this method, it gets the count by calling the
        ///     GetAllFeatureCore method and counting each feature. It is inefficient way
        ///     to get the count in most data sources. It is recommended to override
        ///     this method and replace it with a more optimized method.</para>
        /// </remarks>
        protected virtual int GetCountCore()
        {
            Validators.CheckFeatureSourceIsOpen(IsOpen);

            Collection<Feature> allRecordsInDb = GetAllFeaturesCore(new string[] { });
            return allRecordsInDb.Count;
        }

        /// <summary>Returns the bounding box encompassing all of the features in BaseFeatureSource.</summary>
        /// <returns>Bounding box encompassing all of the features in BaseFeatureSource.</returns>
        /// <remarks>Concrete wrapper of virtual method GetBoundingBoxCore. <br/>
        /// 	<br/>
        /// The default implementation of GetBoundingBoxCore uses the GetAllRecordsCore method to
        /// calculate the bounding box of the FeatureSource. It is recommended to provide
        /// one's own implementation for this method to be more efficient.<br/>
        /// 	<br/>
        /// As a concrete public method that wraps a Core method, Mapgenix reserves the right
        /// to add events and other logic returned by the Core version of the method. </remarks>
        public RectangleShape GetBoundingBox()
        {
            Validators.CheckFeatureSourceIsOpen(IsOpen);

            RectangleShape boundingBox = GetBoundingBoxCore();
            if (IsInTransaction && IsTransactionLive)
            {
                foreach (Feature feature in _transactionBuffer.AddBuffer.Values)
                {
                    boundingBox.ExpandToInclude(feature.GetBoundingBox());
                }
                foreach (Feature feature in _transactionBuffer.EditBuffer.Values)
                {
                    boundingBox.ExpandToInclude(feature.GetBoundingBox());
                }
            }

            if (Projection != null)
            {
                boundingBox = ConvertToExternalProjection(boundingBox);
            }

            return boundingBox;
        }

        /// <summary>Returns the bounding box encompassing all of the features in BaseFeatureSource.</summary>
        /// <returns>Bounding box encompassing all of the features in BaseFeatureSource.</returns>
        /// <remarks>
        /// 	<para>Protected virtual method called from the concrete public method
        ///     GetBoundingBox.<br/>
        /// 		<br/>
        ///     The default implementation of GetBoundingBoxCore uses GetAllRecordsCore method
        ///     to calculate the bounding box of BaseFeatureSource. It is recommended to
        ///     provide one's own implementation for this method to be more efficient.<br/>
        /// <br/></para>
        /// </remarks>
        protected virtual RectangleShape GetBoundingBoxCore()
        {
            Validators.CheckFeatureSourceIsOpen(IsOpen);

            Collection<Feature> features = GetAllFeaturesCore(new string[] { });

            RectangleShape boundingBox = new RectangleShape();
            if (features.Count > 0)
            {
                boundingBox = features[0].GetBoundingBox();
            }

            foreach (Feature feature in features)
            {
                boundingBox.ExpandToInclude(feature.GetBoundingBox());
            }
            return boundingBox;
        }

        /// <summary>Opens BaseFeatureSource to have it ready to use.</summary>
        /// <returns>None</returns>
        /// <remarks>
        /// 	<para>Concrete wrapper of abstract method OpenCore. It is responsible for initializing BaseFeatureSource.<br/>
        ///     As a concrete public method that wraps a Core method, Mapgenix reserves the right
        ///     to add events and other logic returned by the Core version of the method.</para>
        /// </remarks>
        public void Open()
        {
            if (!IsOpen)
            {
                OnOpeningFeatureSource(new EventArgs());

                _isOpen = true;
                OpenCore();


                if (_isOpen)
                {
                    if (_projection != null)
                    {
                        _projection.Open();
                    }

                    OnOpenedFeatureSource(new EventArgs());
                }
            }
        }

        /// <summary>Opens BaseFeatureSource to get it ready to use.</summary>
        /// <remarks>Protected virtual method called from the concrete public method Open.
        /// It is responsible for initializing BaseFeatureSource.<br/>
        /// 	<br/>
        /// When implementing this virtual method ,consider opening files for file-based sources,
        /// connecting to databases in the database-based feature sources and so on. Close these in the Close method of BaseFeatureSource.
        /// </remarks>
        /// <returns>None</returns>
        protected virtual void OpenCore()
        {

        }

       
        /// <summary>Closes BaseFeatureSource and releases any resources used by it.
        /// </summary>
        /// <returns>None</returns>
        /// <remarks>Concrete wrapper for the abstract method CloseCore.
        /// As  a concrete public method that wraps a Core method, Mapgenix reserves the right
        /// to add events and other logic returned by the Core version of the method.</remarks>
        public void Close()
        {
            if (IsOpen)
            {
                OnClosingFeatureSource(new EventArgs());

                _isOpen = false;
                CloseCore();

                
                OnClosedFeatureSource(new EventArgs());
            }
        }

        /// <summary>Closes BaseFeatureSource and releases any resources used by it.</summary>
        /// <returns>None</returns>
        /// <remarks>Protected virtual method called from the concrete public method Close.</remarks>
        protected virtual void CloseCore()
        {
        }

        /// <summary>Starts a new transaction in case BaseFeatureSource allows editing.</summary>
        /// <returns>None</returns>
        /// <remarks>Used to start a transaction, in case BaseFeatureSource allows
        /// editing. BaseFeatureSource must be open.<br/>
        /// 	<br/>
        /// 	 /// The Transaction System<br/>
        /// 	<br/>
        /// The transaction system is the same independently of the speficic FeatureSource (shapefile, in memory layer etc).
        /// First call BeginTransaction (Allocates a collection of in-memory change buffers to store changes before committing transactions).
        /// Call RollbackTransaction to revert before BeginTransaction.
        /// To commit, call CommitTransaction and the collection of changes are passed to CommitTransactionCore with 
        /// the logic of committting specific to a source.
        /// By default IsLiveTransaction property is set to false, meaning that until the changes are committed, any changes
        /// that are in the temporary editing buffer will not be reflected.<br/>
        /// 	<br/>
        /// In the case where the IsLiveTransaction is set to true, the live transaction concept means that all of the modifications
        /// performed during a transaction are live.<br/>
        /// 	<br/>
        /// As an example, a FeatureLayer has 20 records.
        /// Next, a transaction begins and then GetAllFeatures is called. The result is 20
        /// records. Delete one of the records and call the GetAllFeatures
        /// again.  Only 19 records are retrieved, even though the transaction has not yet been committed.<br/>
        /// <br/>
        /// </remarks>
        public void BeginTransaction()
        {
            Validators.CheckFeatureSourceIsEditable(IsEditable);
            Validators.CheckFeatureSourceIsOpen(IsOpen);
            Validators.CheckFeatureSourceIsAlreadyInTransaction(IsInTransaction);

            _transactionBuffer.Clear();
            _isInTransaction = true;
        }

        /// <summary>Adds a Feature to an existing transaction.</summary>
        /// <remarks>Adds a Feature to an existing transaction. Call BeginTransaction first.<br/>
        /// 	<br/>
        /// 	<br/>
        /// 	 /// The Transaction System<br/>
        /// 	<br/>
        /// The transaction system is the same independently of the speficic FeatureSource (shapefile, in memory layer etc).
        /// First call BeginTransaction (Allocates a collection of in-memory change buffers to store changes before committing transactions).
        /// Call RollbackTransaction to revert before BeginTransaction.
        /// To commit, call CommitTransaction and the collection of changes are passed to CommitTransactionCore with 
        /// the logic of committting specific to a source.
        /// By default IsLiveTransaction property is set to false, meaning that until the changes are committed, any changes
        /// that are in the temporary editing buffer will not be reflected.<br/>
        /// 	<br/>
        /// In the case where the IsLiveTransaction is set to true, the live transaction concept means that all of the modifications
        /// performed during a transaction are live.<br/>
        /// 	<br/>
        /// As an example, a FeatureLayer has 20 records.
        /// Next, a transaction begins and then GetAllFeatures is called. The result is 20
        /// records. Delete one of the records and call the GetAllFeatures
        /// again.  Only 19 records are retrieved, even though the transaction has not yet been committed.<br/>
        /// <br/>
        /// </remarks>
        /// <returns>ID string uniquely identifying the Feature while in transaction.</returns>
        /// <param name="feature">Feature to add to the transaction.</param>
        public string AddFeature(Feature feature)
        {
            Validators.CheckFeatureSourceIsEditable(IsEditable);
            Validators.CheckFeatureSourceIsOpen(IsOpen);
            Validators.CheckFeatureSourceIsInTransaction(IsInTransaction);
            Validators.CheckFeatureIsValid(feature);

            if (_projection != null)
            {
                feature = _projection.ConvertToInternalProjection(feature);
            }

            _transactionBuffer.AddFeature(feature);

            return feature.Id;
        }

        /// <summary>Adds a Feature with a shape to an existing transaction.</summary>
        /// <remarks>Adds a Feature with a shape to an existing transaction. Call BeginTransaction first.<br/>
        /// 	<br/>
        /// 	<br/>
        /// 	 /// The Transaction System<br/>
        /// 	<br/>
        /// The transaction system is the same independently of the speficic FeatureSource (shapefile, in memory layer etc).
        /// First call BeginTransaction (Allocates a collection of in-memory change buffers to store changes before committing transactions).
        /// Call RollbackTransaction to revert before BeginTransaction.
        /// To commit, call CommitTransaction and the collection of changes are passed to CommitTransactionCore with 
        /// the logic of committting specific to a source.
        /// By default IsLiveTransaction property is set to false, meaning that until the changes are committed, any changes
        /// that are in the temporary editing buffer will not be reflected.<br/>
        /// 	<br/>
        /// In the case where the IsLiveTransaction is set to true, the live transaction concept means that all of the modifications
        /// performed during a transaction are live.<br/>
        /// 	<br/>
        /// As an example, a FeatureLayer has 20 records.
        /// Next, a transaction begins and then GetAllFeatures is called. The result is 20
        /// records. Delete one of the records and call the GetAllFeatures
        /// again.  Only 19 records are retrieved, even though the transaction has not yet been committed.<br/>
        /// <br/>
        /// </remarks>
        /// <returns>ID string uniquely identifying the Feature while in transaction.</returns>
        /// <param name="shape">BaseShape used to create the Feature to add to the transaction.</param>
        public string AddFeature(BaseShape shape)
        {
            Validators.CheckFeatureSourceIsEditable(IsEditable);
            Validators.CheckFeatureSourceIsOpen(IsOpen);
            Validators.CheckFeatureSourceIsInTransaction(IsInTransaction);
            Validators.CheckParameterIsNotNull(shape, "shape");

            return AddFeature(new Feature(shape));
        }

        /// <summary>Adds a Feature with a shape to an existing transaction.</summary>
        /// <remarks>Adds a Feature with a shape to an existing transaction. Call BeginTransaction first.<br/>
        /// 	<br/>
        /// 	<br/>
        /// 	 /// The Transaction System<br/>
        /// 	<br/>
        /// The transaction system is the same independently of the speficic FeatureSource (shapefile, in memory layer etc).
        /// First call BeginTransaction (Allocates a collection of in-memory change buffers to store changes before committing transactions).
        /// Call RollbackTransaction to revert before BeginTransaction.
        /// To commit, call CommitTransaction and the collection of changes are passed to CommitTransactionCore with 
        /// the logic of committting specific to a source.
        /// By default IsLiveTransaction property is set to false, meaning that until the changes are committed, any changes
        /// that are in the temporary editing buffer will not be reflected.<br/>
        /// 	<br/>
        /// In the case where the IsLiveTransaction is set to true, the live transaction concept means that all of the modifications
        /// performed during a transaction are live.<br/>
        /// 	<br/>
        /// As an example, a FeatureLayer has 20 records.
        /// Next, a transaction begins and then GetAllFeatures is called. The result is 20
        /// records. Delete one of the records and call the GetAllFeatures
        /// again.  Only 19 records are retrieved, even though the transaction has not yet been committed.<br/>
        /// <br/>
        /// </remarks>
        /// <returns>ID string uniquely identifying the Feature while in transaction.</returns>
        /// <param name="shape">BaseShape used to create the Feature to add to the transaction.</param>
        /// <param name="columnValues">Column name value pairs for the feature to add to the transaction..</param>
        public string AddFeature(BaseShape shape, IDictionary<string, string> columnValues)
        {
            Validators.CheckFeatureSourceIsEditable(IsEditable);
            Validators.CheckFeatureSourceIsOpen(IsOpen);
            Validators.CheckFeatureSourceIsInTransaction(IsInTransaction);
            Validators.CheckParameterIsNotNull(shape, "shape");
            Validators.CheckParameterIsNotNull(columnValues, "columnValues");

            return AddFeature(new Feature(shape, columnValues));
        }

        
        /// <summary>This method deletes a Feature from an existing transaction.</summary>
        /// <remarks>Deletes a Feature from an existing transaction. Call BeginTransaction first.<br/>
        /// 	<br/>
        /// 	<br/>
        /// 	The Transaction System<br/>
        /// 	<br/>
        /// The transaction system is the same independently of the speficic FeatureSource (shapefile, in memory layer etc).
        /// First call BeginTransaction (Allocates a collection of in-memory change buffers to store changes before committing transactions).
        /// Call RollbackTransaction to revert before BeginTransaction.
        /// To commit, call CommitTransaction and the collection of changes are passed to CommitTransactionCore with 
        /// the logic of committting specific to a source.
        /// By default IsLiveTransaction property is set to false, meaning that until the changes are committed, any changes
        /// that are in the temporary editing buffer will not be reflected.<br/>
        /// 	<br/>
        /// In the case where the IsLiveTransaction is set to true, the live transaction concept means that all of the modifications
        /// performed during a transaction are live.<br/>
        /// 	<br/>
        /// As an example, a FeatureLayer has 20 records.
        /// Next, a transaction begins and then GetAllFeatures is called. The result is 20
        /// records. Delete one of the records and call the GetAllFeatures
        /// again.  Only 19 records are retrieved, even though the transaction has not yet been committed.<br/>
        /// <br/>
        /// </remarks>
        /// <returns>None</returns>
        /// <param name="id">Id of the feature in the FeatureSource to delete.</param>
        public void DeleteFeature(string id)
        {
            Validators.CheckFeatureSourceIsEditable(IsEditable);
            Validators.CheckFeatureSourceIsOpen(IsOpen);
            Validators.CheckFeatureSourceIsInTransaction(IsInTransaction);
            Validators.CheckParameterIsNotNull(id, "id");

            _transactionBuffer.DeleteFeature(id);
        }

      
       
        /// <summary>Updates a Feature in an existing transaction.</summary>
        /// <returns>None</returns>
        /// <param name="feature">Feature to update in the transaction.</param>
        /// <remarks>
        /// 	<para>Updates a Feature in an existing transaction. Call BeginTransaction first.</para>
        /// 	The Transaction System<br/>
        /// 	<br/>
        /// The transaction system is the same independently of the speficic FeatureSource (shapefile, in memory layer etc).
        /// First call BeginTransaction (Allocates a collection of in-memory change buffers to store changes before committing transactions).
        /// Call RollbackTransaction to revert before BeginTransaction.
        /// To commit, call CommitTransaction and the collection of changes are passed to CommitTransactionCore with 
        /// the logic of committting specific to a source.
        /// By default IsLiveTransaction property is set to false, meaning that until the changes are committed, any changes
        /// that are in the temporary editing buffer will not be reflected.<br/>
        /// 	<br/>
        /// In the case where the IsLiveTransaction is set to true, the live transaction concept means that all of the modifications
        /// performed during a transaction are live.<br/>
        /// 	<br/>
        /// As an example, a FeatureLayer has 20 records.
        /// Next, a transaction begins and then GetAllFeatures is called. The result is 20
        /// records. Delete one of the records and call the GetAllFeatures
        /// again.  Only 19 records are retrieved, even though the transaction has not yet been committed.<br/>
        /// <br/>
        /// </remarks>
        public void UpdateFeature(Feature feature)
        {
            Validators.CheckFeatureSourceIsEditable(IsEditable);
            Validators.CheckFeatureSourceIsOpen(IsOpen);
            Validators.CheckFeatureSourceIsInTransaction(IsInTransaction);
            Validators.CheckFeatureIsValid(feature);

            if (_projection != null)
            {
                feature = _projection.ConvertToInternalProjection(feature);
            }

            _transactionBuffer.EditFeature(feature);
        }

        /// <summary>Cancels an existing transaction. It frees up the internal memory cache of any feature added, updated or deleted.</summary>
        /// <returns>None</returns>
        /// <remarks>
        /// 	<para>Cancels an existing transaction. It frees up the internal
        ///     memory cache of any features added, updated or deleted. Call BeginTransaction first.</para>
        /// 	The Transaction System<br/>
        /// 	<br/>
        /// The transaction system is the same independently of the speficic FeatureSource (shapefile, in memory layer etc).
        /// First call BeginTransaction (Allocates a collection of in-memory change buffers to store changes before committing transactions).
        /// Call RollbackTransaction to revert before BeginTransaction.
        /// To commit, call CommitTransaction and the collection of changes are passed to CommitTransactionCore with 
        /// the logic of committting specific to a source.
        /// By default IsLiveTransaction property is set to false, meaning that until the changes are committed, any changes
        /// that are in the temporary editing buffer will not be reflected.<br/>
        /// 	<br/>
        /// In the case where the IsLiveTransaction is set to true, the live transaction concept means that all of the modifications
        /// performed during a transaction are live.<br/>
        /// 	<br/>
        /// As an example, a FeatureLayer has 20 records.
        /// Next, a transaction begins and then GetAllFeatures is called. The result is 20
        /// records. Delete one of the records and call the GetAllFeatures
        /// again.  Only 19 records are retrieved, even though the transaction has not yet been committed.<br/>
        /// <br/>
        /// </remarks>
        public void RollbackTransaction()
        {
            Validators.CheckFeatureSourceIsEditable(IsEditable);
            Validators.CheckFeatureSourceIsOpen(IsOpen);
            Validators.CheckFeatureSourceIsInTransaction(IsInTransaction);

            _isInTransaction = false;
            _transactionBuffer.Clear();
        }

        /// <summary>Commits the existing transaction to its underlying source of data.</summary>
        /// <returns>Value of this method is a TransactionResult class with status of the committed transaction.</returns>
        /// <remarks>
        /// 	<para>Concrete wrapper of virtual method CommitTransactionCore. Call BeginTransaction first.</para>
        /// 	The Transaction System<br/>
        /// 	<br/>
        /// The transaction system is the same independently of the speficic FeatureSource (shapefile, in memory layer etc).
        /// First call BeginTransaction (Allocates a collection of in-memory change buffers to store changes before committing transactions).
        /// Call RollbackTransaction to revert before BeginTransaction.
        /// To commit, call CommitTransaction and the collection of changes are passed to CommitTransactionCore with 
        /// the logic of committting specific to a source.
        /// By default IsLiveTransaction property is set to false, meaning that until the changes are committed, any changes
        /// that are in the temporary editing buffer will not be reflected.<br/>
        /// 	<br/>
        /// In the case where the IsLiveTransaction is set to true, the live transaction concept means that all of the modifications
        /// performed during a transaction are live.<br/>
        /// 	<br/>
        /// As an example, a FeatureLayer has 20 records.
        /// Next, a transaction begins and then GetAllFeatures is called. The result is 20
        /// records. Delete one of the records and call the GetAllFeatures
        /// again.  Only 19 records are retrieved, even though the transaction has not yet been committed.<br/>
        /// <br/>
        /// </remarks>
        public TransactionResult CommitTransaction()
        {
            Validators.CheckFeatureSourceIsEditable(IsEditable);
            Validators.CheckFeatureSourceIsOpen(IsOpen);
            Validators.CheckFeatureSourceIsInTransaction(IsInTransaction);

            CommitTransactionEventArgs eventArgs = new CommitTransactionEventArgs(_transactionBuffer);
            eventArgs.Cancel = false;
            OnCommittingTransaction(eventArgs);

            TransactionResult result = new TransactionResult();
            if (eventArgs.Cancel)
            {
                result.TransactionResultStatus = TransactionResultStatus.Cancel;
            }
            else
            {
                result = CommitTransactionCore(_transactionBuffer);
                _isInTransaction = false;

                OnCommittedTransaction(new CommitTransactionEventArgs(_transactionBuffer));

                _transactionBuffer.Clear();
            }
            return result;
        }

       
        /// <summary>Commits the existing transaction to its underlying source of data.</summary>
        /// <returns>TransactionResult class with status of the committed transaction.</returns>
        /// <param name="transactions">Transaction with the adds, edits and deletes that make up the
        /// transaction. To write the changes to the underlying data source.</param>
        /// <remarks>The Transaction System<br/>
        /// 	<br/>
        /// The transaction system is the same independently of the speficic FeatureSource (shapefile, in memory layer etc).
        /// First call BeginTransaction (Allocates a collection of in-memory change buffers to store changes before committing transactions).
        /// Call RollbackTransaction to revert before BeginTransaction.
        /// To commit, call CommitTransaction and the collection of changes are passed to CommitTransactionCore with 
        /// the logic of committting specific to a source.
        /// By default IsLiveTransaction property is set to false, meaning that until the changes are committed, any changes
        /// that are in the temporary editing buffer will not be reflected.<br/>
        /// 	<br/>
        /// In the case where the IsLiveTransaction is set to true, the live transaction concept means that all of the modifications
        /// performed during a transaction are live.<br/>
        /// 	<br/>
        /// As an example, a FeatureLayer has 20 records.
        /// Next, a transaction begins and then GetAllFeatures is called. The result is 20
        /// records. Delete one of the records and call the GetAllFeatures
        /// again.  Only 19 records are retrieved, even though the transaction has not yet been committed.<br/>
        /// <br/>
        /// </remarks>
        protected virtual TransactionResult CommitTransactionCore(TransactionBuffer transactions)
        {
            throw new NotSupportedException(ExceptionDescription.EditingNotSupported);
        }

        /// <summary>Returns collection of all the features in the FeatureSource.</summary>
        /// <returns>Collection of all the features in the FeatureSource
        /// </returns>
        /// <param name="returningColumnNames">Field names of the column data to return with each Feature.</param>
        /// <remarks>Returns collection of features in BaseFeatureSource. Returns
        /// what is returned by GetAllFeaturesCore method, with any of the additions or
        /// subtractions made if in a live transaction.<br/>
        ///<br/>
        /// As a concrete public method that wraps a Core method, Mapgenix reserves the right
        /// to add events and other logic returned by the Core version of the method. </remarks>
        public Collection<Feature> GetAllFeatures(IEnumerable<string> returningColumnNames)
        {
            Validators.CheckFeatureSourceIsOpen(IsOpen);
            Validators.CheckParameterIsNotNull(returningColumnNames, "columnNames");

			Collection<string> uniqColumnNames = ColumnFilter.SplitColumnNames(returningColumnNames);
            Collection<string> fieldNamesInsideSource = GetColumnNamesInsideFeatureSource(uniqColumnNames);
            Collection<string> fieldNamesOutsideOfSource = GetColumnNamesOutsideFeatureSource(uniqColumnNames);

            Collection<Feature> returnFeatures = GetAllFeaturesCore(fieldNamesInsideSource);

            returnFeatures = RaiseCustomColumnFetchEvent(returnFeatures, fieldNamesOutsideOfSource);

			if (ColumnFilter.HasComplicateFields(returningColumnNames))
            {
				returnFeatures = ColumnFilter.ReplaceColumnValues(returnFeatures, returningColumnNames);
            }

            returnFeatures = ApplyTransactionLive(returnFeatures);

            if (Projection != null)
            {
                returnFeatures = ConvertToExternalProjection(returnFeatures);
            }

            RemoveEmptyAndExcludeFeatures(returnFeatures);

            return returnFeatures;
        }

        private Collection<Feature> RaiseCustomColumnFetchEvent(Collection<Feature> sourceFeatures, Collection<string> fieldNamesOutsideOfSource)
        {
            if (fieldNamesOutsideOfSource.Count > 0)
            {
                for (int i = 0; i < sourceFeatures.Count; i++)
                {
                    foreach (string columnName in fieldNamesOutsideOfSource)
                    {
                        ColumnFetchEventArgs e = new ColumnFetchEventArgs(columnName, sourceFeatures[i].Id);
                        OnCustomColumnFetch(e);
                        if (!sourceFeatures[i].ColumnValues.ContainsKey(e.ColumnName))
                        {
                            sourceFeatures[i].ColumnValues.Add(columnName, e.ColumnValue);
                        }
                    }
                }
            }

            return sourceFeatures;
        }

        private Collection<Feature> ApplyTransactionLive(Collection<Feature> sourceFeatures)
        {
            if (IsInTransaction && IsTransactionLive)
            {
				Dictionary<string, Feature> dictionaryFeatures = ColumnFilter.GetFeaturesDictionaryFromCollecion(sourceFeatures);

                foreach (Feature feature in _transactionBuffer.AddBuffer.Values)
                {
                    dictionaryFeatures.Add(feature.Id, feature);
                }

                foreach (Feature feature in _transactionBuffer.EditBuffer.Values)
                {
                    if (dictionaryFeatures.ContainsKey(feature.Id))
                    {
                        dictionaryFeatures.Remove(feature.Id);
                    }

                    dictionaryFeatures.Add(feature.Id, feature);
                }

                foreach (string id in _transactionBuffer.DeleteBuffer)
                {
                    if (dictionaryFeatures.ContainsKey(id))
                    {
                        dictionaryFeatures.Remove(id);
                    }
                }

                sourceFeatures = ColumnFilter.GetFeaturesCollectionFromDictionary(dictionaryFeatures);
            }

            return sourceFeatures;
        }

        /// <summary>Returns collection of all the features in the FeatureSource.</summary>
        /// <returns>Collection of all the features in the FeatureSource
        /// </returns>
        /// <param name="returningColumnNamesType">Column name type.</param>
        /// <remarks>Returns collection of features in BaseFeatureSource. Returns
        /// what is returned by GetAllFeaturesCore method, with any of the additions or
        /// subtractions made if in a live transaction.<br/>
        ///<br/>
        /// As a concrete public method that wraps a Core method, Mapgenix reserves the right
        /// to add events and other logic returned by the Core version of the method. </remarks>
        public Collection<Feature> GetAllFeatures(ReturningColumnsType returningColumnNamesType)
        {
            Validators.CheckFeatureSourceIsOpen(IsOpen);
            Validators.CheckReturningColumnNamesTypeIsValid(returningColumnNamesType, "returningColumnNamesType");

            Collection<string> returningColumnNames = GetReturningColumnNames(returningColumnNamesType);

            return GetAllFeatures(returningColumnNames);
        }

        /// <summary>Returns collection of all the features in the FeatureSource.</summary>
        /// <returns>Collection of all the features in the FeatureSource
        /// </returns>
        /// <param name="returningColumnNames">Field names of the column data to return with each Feature.</param>
        /// <remarks>Returns collection of features in BaseFeatureSource. Returns
        /// what is returned by GetAllFeaturesCore method, with any of the additions or
        /// subtractions made if in a live transaction.<br/></remarks>
        protected abstract Collection<Feature> GetAllFeaturesCore(IEnumerable<string> returningColumnNames);

        /// <summary>Returns a collection of features outside of the target rectangle.</summary>
        /// <returns>Collection of features outside of the target rectangle.</returns>
        /// <remarks>If there is a current transaction and it is marked as live, then the results
        ///     will include any transaction Feature that applies.</remarks>
        /// <param name="boundingBox">Target boundingBox.</param>
        /// <param name="returningColumnNames">Column names.</param>
        public Collection<Feature> GetFeaturesOutsideBoundingBox(RectangleShape boundingBox, IEnumerable<string> returningColumnNames)
        {
            Validators.CheckFeatureSourceIsOpen(IsOpen);
            Validators.CheckParameterIsNotNull(returningColumnNames, "returningColumnNames");
            Validators.CheckParameterIsNotNull(boundingBox, "boundingBox");
            Validators.CheckParameterIsValid(boundingBox, "boundingBox");

            RectangleShape unProjectedBoundingBox = boundingBox;
            if (Projection != null)
            {
                unProjectedBoundingBox = Projection.ConvertToInternalProjection(boundingBox);
            }

			Collection<string> uniqColumnNames = ColumnFilter.SplitColumnNames(returningColumnNames);
            Collection<string> fieldNamesInsideSource = GetColumnNamesInsideFeatureSource(uniqColumnNames);
            Collection<string> fieldNamesOutsideOfSource = GetColumnNamesOutsideFeatureSource(uniqColumnNames);

            Collection<Feature> returnFeatures = GetFeaturesOutsideBoundingBoxCore(unProjectedBoundingBox, fieldNamesInsideSource);

            returnFeatures = RaiseCustomColumnFetchEvent(returnFeatures, fieldNamesOutsideOfSource);

            if (IsInTransaction && IsTransactionLive)
            {
                foreach (Feature feature in _transactionBuffer.AddBuffer.Values)
                {
                    if (!unProjectedBoundingBox.Contains(feature.GetBoundingBox()))
                    {
                        returnFeatures.Add(feature);
                    }
                }

                Dictionary<string, Feature> recordsInDictionary = new Dictionary<string, Feature>();
                if (_transactionBuffer.EditBuffer.Count > 0 || _transactionBuffer.DeleteBuffer.Count > 0)
                {
					recordsInDictionary = ColumnFilter.GetFeaturesDictionaryFromCollecion(returnFeatures);
                }

                foreach (Feature feature in _transactionBuffer.EditBuffer.Values)
                {
                    if (!unProjectedBoundingBox.Contains(feature.GetBoundingBox()) && recordsInDictionary.ContainsKey(feature.Id))
                    {
                        Feature oringalFeature = recordsInDictionary[feature.Id];
                        returnFeatures.Remove(oringalFeature);
                        returnFeatures.Add(feature);
                    }
                }

                foreach (string id in _transactionBuffer.DeleteBuffer)
                {
                    if (recordsInDictionary.ContainsKey(id))
                    {
                        if (!unProjectedBoundingBox.Contains(recordsInDictionary[id].GetBoundingBox()) && recordsInDictionary.ContainsKey(id))
                        {
                            Feature feature = recordsInDictionary[id];
                            returnFeatures.Remove(feature);
                        }
                    }
                }
            }

            if (Projection != null)
            {
                returnFeatures = ConvertToExternalProjection(returnFeatures);
            }

            RemoveEmptyAndExcludeFeatures(returnFeatures);

            return returnFeatures;
        }

        /// <summary>Returns a collection of features outside of the target rectangle.</summary>
        /// <returns>Collection of features outside of the target rectangle.</returns>
        /// <remarks>If there is a current transaction and it is marked as live, then the results
        ///     will include any transaction Feature that applies.</remarks>
        /// <param name="boundingBox">Target boundingBox.</param>
        /// <param name="returningColumnNamesType">Column type.</param>
        public Collection<Feature> GetFeaturesOutsideBoundingBox(RectangleShape boundingBox, ReturningColumnsType returningColumnNamesType)
        {
            Validators.CheckFeatureSourceIsOpen(IsOpen);
            Validators.CheckReturningColumnNamesTypeIsValid(returningColumnNamesType, "returningColumnNamesType");

            Collection<string> returningColumnNames = GetReturningColumnNames(returningColumnNamesType);

            return GetFeaturesOutsideBoundingBox(boundingBox, returningColumnNames);
        }

        /// <summary>Returns a collection of features outside of the target rectangle.</summary>
        /// <returns>Collection of features outside of the target rectangle.</returns>
        /// <remarks>If there is a current transaction and it is marked as live, then the results
        ///     will include any transaction Feature that applies.</remarks>
        /// <param name="boundingBox">Target boundingBox.</param>
        /// <param name="returningColumnNames">Column names.</param>
        protected virtual Collection<Feature> GetFeaturesOutsideBoundingBoxCore(RectangleShape boundingBox, IEnumerable<string> returningColumnNames)
        {
            Validators.CheckFeatureSourceIsOpen(IsOpen);
            Validators.CheckParameterIsNotNull(returningColumnNames, "returningColumnNames");
            Validators.CheckParameterIsNotNull(boundingBox, "boundingBox");
            Validators.CheckParameterIsValid(boundingBox, "boundingBox");

            Collection<Feature> allRecordsInDatabase = GetAllFeaturesCore(returningColumnNames);

            Collection<Feature> recordsOutsideOfBoundingBox = new Collection<Feature>();

            foreach (Feature feature in allRecordsInDatabase)
            {
                if (!boundingBox.Contains(feature.GetBoundingBox()))
                {
                    recordsOutsideOfBoundingBox.Add(feature);
                }
            }

            return recordsOutsideOfBoundingBox;
        }

        /// <summary>Returns a collection of features inside of the target rectangle.</summary>
        /// <returns>Collection of features inside of the target rectangle.</returns>
        /// <remarks>If there is a current transaction and it is marked as live, then the results
        ///     will include any transaction Feature that applies.</remarks>
        /// <param name="boundingBox">Target boundingBox.</param>
        /// <param name="returningColumnNames">Column names.</param>
        public Collection<Feature> GetFeaturesInsideBoundingBox(RectangleShape boundingBox, IEnumerable<string> returningColumnNames)
        {
            Validators.CheckFeatureSourceIsOpen(IsOpen);
            Validators.CheckParameterIsNotNull(returningColumnNames, "returningColumnNames");
            Validators.CheckParameterIsNotNull(boundingBox, "boundingBox");
            Validators.CheckParameterIsValid(boundingBox, "boundingBox");

            RectangleShape projectedBoundingBox = new RectangleShape(boundingBox.UpperLeftPoint.X, boundingBox.UpperLeftPoint.Y, boundingBox.LowerRightPoint.X, boundingBox.LowerRightPoint.Y);
            if (Projection != null)
            {
                projectedBoundingBox = ConvertToInternalProjection(boundingBox);
            }

			Collection<string> uniqColumnNames = ColumnFilter.SplitColumnNames(returningColumnNames);
            Collection<string> fieldNamesInsideSource = GetColumnNamesInsideFeatureSource(uniqColumnNames);
            Collection<string> fieldNamesOutsideOfSource = GetColumnNamesOutsideFeatureSource(uniqColumnNames);
            Collection<Feature> returnFeatures = GetFeaturesInsideBoundingBoxCore(projectedBoundingBox, fieldNamesInsideSource);

            returnFeatures = RaiseCustomColumnFetchEvent(returnFeatures, fieldNamesOutsideOfSource);

			if (ColumnFilter.HasComplicateFields(returningColumnNames))
            {
				returnFeatures = ColumnFilter.ReplaceColumnValues(returnFeatures, returningColumnNames);
            }

            if (IsInTransaction && IsTransactionLive)
            {
                foreach (Feature feature in _transactionBuffer.AddBuffer.Values)
                {
                    if (projectedBoundingBox.Intersects(feature.GetBoundingBox()))
                    {
                        returnFeatures.Add(feature);
                    }
                }

                Dictionary<string, Feature> recordsInDictionary = new Dictionary<string, Feature>();
                if (_transactionBuffer.EditBuffer.Count > 0 || _transactionBuffer.DeleteBuffer.Count > 0)
                {
					recordsInDictionary = ColumnFilter.GetFeaturesDictionaryFromCollecion(returnFeatures);
                }

                foreach (Feature feature in _transactionBuffer.EditBuffer.Values)
                {
                    bool isContained = projectedBoundingBox.Intersects(feature.GetBoundingBox());
                    if (isContained)
                    {
                        if (recordsInDictionary.ContainsKey(feature.Id))
                        {
                            Feature oringalFeature = recordsInDictionary[feature.Id];
                            returnFeatures.Remove(oringalFeature);
                        }
                        returnFeatures.Add(feature);
                    }
                    else if (!isContained && recordsInDictionary.ContainsKey(feature.Id))
                    {
                        Feature oringalFeature = recordsInDictionary[feature.Id];
                        returnFeatures.Remove(oringalFeature);
                    }
                }

                foreach (string id in _transactionBuffer.DeleteBuffer)
                {
                    if (recordsInDictionary.ContainsKey(id))
                    {
                        if (projectedBoundingBox.Intersects(recordsInDictionary[id].GetBoundingBox()) && recordsInDictionary.ContainsKey(id))
                        {
                            Feature feature = recordsInDictionary[id];
                            returnFeatures.Remove(feature);
                        }
                    }
                }
            }

            if (Projection != null)
            {
                returnFeatures = ConvertToExternalProjection(returnFeatures);
            }

            RemoveEmptyAndExcludeFeatures(returnFeatures);

            return returnFeatures;
        }

        /// <summary>Returns a collection of features inside of the target rectangle.</summary>
        /// <returns>Collection of features inside of the target rectangle.</returns>
        /// <remarks>If there is a current transaction and it is marked as live, then the results
        ///     will include any transaction Feature that applies.</remarks>
        /// <param name="boundingBox">Target boundingBox.</param>
        /// <param name="returningColumnNamesType">Column type.</param>
        public Collection<Feature> GetFeaturesInsideBoundingBox(RectangleShape boundingBox, ReturningColumnsType returningColumnNamesType)
        {
            Validators.CheckFeatureSourceIsOpen(IsOpen);
            Validators.CheckReturningColumnNamesTypeIsValid(returningColumnNamesType, "returningColumnNamesType");

            Collection<string> returningColumnNames = GetReturningColumnNames(returningColumnNamesType);

            return GetFeaturesInsideBoundingBox(boundingBox, returningColumnNames);
        }

        /// <summary>Returns a collection of features inside of the target rectangle.</summary>
        /// <returns>Collection of features inside of the target rectangle.</returns>
        /// <remarks>If there is a current transaction and it is marked as live, then the results
        ///     will include any transaction Feature that applies.</remarks>
        /// <param name="boundingBox">Target boundingBox.</param>
        /// <param name="returningColumnNames">Column names.</param>
       protected virtual Collection<Feature> GetFeaturesInsideBoundingBoxCore(RectangleShape boundingBox, IEnumerable<string> returningColumnNames)
        {
            Validators.CheckFeatureSourceIsOpen(IsOpen);
            Validators.CheckParameterIsNotNull(returningColumnNames, "returningColumnNames");
            Validators.CheckParameterIsNotNull(boundingBox, "boundingBox");
            Validators.CheckParameterIsValid(boundingBox, "boundingBox");

            Collection<Feature> allRecordsInSource = GetAllFeaturesCore(returningColumnNames);

            Collection<Feature> returnRecords = new Collection<Feature>();
            RectangleShape featureBoundingBox = null;
            foreach (Feature feature in allRecordsInSource)
            {
                featureBoundingBox = feature.GetBoundingBox();

                if (!boundingBox.IsDisjointed(featureBoundingBox))
                {
                    returnRecords.Add(feature);
                }
            }

            return returnRecords;
        }

       /// <summary>Returns a collection of features used for drawing.</summary>
       /// <returns>Collection of features used for drawing..</returns>
       /// <param name="boundingBox">Bounding box of the features to draw.</param>
       /// <param name="screenWidth">Width of the canvas, in screen pixels, to draw on.</param>
       /// <param name="screenHeight">Height of the canvas, in screen pixels, to draw on.
       /// </param>
       /// <param name="returningColumnNames">Field names of the column data.</param>
        public Collection<Feature> GetFeaturesForDrawing(RectangleShape boundingBox, double screenWidth, double screenHeight, IEnumerable<string> returningColumnNames)
        {
            Validators.CheckFeatureSourceIsOpen(IsOpen);
            Validators.CheckParameterIsNotNull(returningColumnNames, "returningColumnNames");
            Validators.CheckParameterIsNotNull(boundingBox, "boundingBox");
            Validators.CheckParameterIsValid(boundingBox, "boundingBox");
            Validators.CheckValueIsBiggerThanZero(screenWidth, "screenWidth");
            Validators.CheckValueIsBiggerThanZero(screenHeight, "screenHeight");

            Collection<Feature> returnFeatures = new Collection<Feature>();
			Collection<string> uniqColumnNames = ColumnFilter.SplitColumnNames(returningColumnNames);
            Collection<string> fieldNamesInsideSource = GetColumnNamesInsideFeatureSource(uniqColumnNames);
            Collection<string> fieldNamesOutsideOfSource = GetColumnNamesOutsideFeatureSource(uniqColumnNames);

            if (Projection != null)
            {
                boundingBox = ConvertToInternalProjection(boundingBox);
            }

            if (_geoCache.IsActive)
            {
                returnFeatures = _geoCache.GetFeatures(boundingBox);

                if (returnFeatures.Count == 0)
                {
                    returnFeatures = GetFeaturesForDrawingCore(boundingBox, screenWidth, screenHeight, fieldNamesInsideSource);
                    _geoCache.Add(boundingBox, returnFeatures);
                }
            }
            else
            {
                returnFeatures = GetFeaturesForDrawingCore(boundingBox, screenWidth, screenHeight, fieldNamesInsideSource);
            }

            returnFeatures = RaiseCustomColumnFetchEvent(returnFeatures, fieldNamesOutsideOfSource);

			if (ColumnFilter.HasComplicateFields(returningColumnNames))
            {
				returnFeatures = ColumnFilter.ReplaceColumnValues(returnFeatures, returningColumnNames);
            }

            if (IsInTransaction && IsTransactionLive)
            {
                foreach (Feature feature in _transactionBuffer.AddBuffer.Values)
                {
                    if (boundingBox.Intersects(feature.GetBoundingBox()))
                    {
                        returnFeatures.Add(feature);
                    }
                }

                Dictionary<string, Feature> recordsInDictionary = new Dictionary<string, Feature>();
                if (_transactionBuffer.EditBuffer.Count > 0 || _transactionBuffer.DeleteBuffer.Count > 0)
                {
					recordsInDictionary = ColumnFilter.GetFeaturesDictionaryFromCollecion(returnFeatures);
                }

                foreach (Feature feature in _transactionBuffer.EditBuffer.Values)
                {
                    bool isContained = boundingBox.Intersects(feature.GetBoundingBox());
                    if (isContained)
                    {
                        if (recordsInDictionary.ContainsKey(feature.Id))
                        {
                            Feature oringalFeature = recordsInDictionary[feature.Id];
                            returnFeatures.Remove(oringalFeature);
                        }
                        returnFeatures.Add(feature);
                    }
                    else if (!isContained && recordsInDictionary.ContainsKey(feature.Id))
                    {
                        Feature oringalFeature = recordsInDictionary[feature.Id];
                        returnFeatures.Remove(oringalFeature);
                    }
                }

                foreach (string id in _transactionBuffer.DeleteBuffer)
                {
                    if (recordsInDictionary.ContainsKey(id))
                    {
                        if (boundingBox.Intersects(recordsInDictionary[id].GetBoundingBox()) && recordsInDictionary.ContainsKey(id))
                        {
                            Feature feature = recordsInDictionary[id];
                            returnFeatures.Remove(feature);
                        }
                    }
                }
            }

            if (Projection != null)
            {
                returnFeatures = ConvertToExternalProjection(returnFeatures);
            }

            RemoveEmptyAndExcludeFeatures(returnFeatures);

            return returnFeatures;
        }

        internal Collection<Feature> GetWrappingFeatures(RectangleShape boundingBox, double screenWidth, double screenHeight, IEnumerable<string> returningColumnNames, RectangleShape wrappingExtent)
        {
            Validators.CheckFeatureSourceIsOpen(IsOpen);
            Validators.CheckParameterIsNotNull(returningColumnNames, "returningColumnNames");
            Validators.CheckParameterIsNotNull(boundingBox, "boundingBox");
            Validators.CheckParameterIsValid(boundingBox, "boundingBox");
            Validators.CheckValueIsBiggerThanZero(screenWidth, "screenWidth");
            Validators.CheckValueIsBiggerThanZero(screenHeight, "screenHeight");
            Validators.CheckParameterIsNotNull(wrappingExtent, "wrappingExtent");
            Validators.CheckParameterIsValid(wrappingExtent, "wrappingExtent");

            double maxX = boundingBox.LowerRightPoint.X;
            double minX = boundingBox.UpperLeftPoint.X;

            if (minX >= wrappingExtent.LowerLeftPoint.X && maxX <= wrappingExtent.LowerRightPoint.X)
            {
                return new Collection<Feature>();
            }
            if (maxX > wrappingExtent.LowerRightPoint.X)
            {
                minX = wrappingExtent.LowerLeftPoint.X;
                maxX = minX + maxX - wrappingExtent.LowerRightPoint.X;
            }
            else if (minX < wrappingExtent.LowerLeftPoint.X)
            {
                maxX = wrappingExtent.LowerRightPoint.X;
                minX = maxX - (wrappingExtent.LowerLeftPoint.X - minX);
            }


            RectangleShape newMarginWorldExtent = new RectangleShape(minX, boundingBox.UpperLeftPoint.Y, maxX, boundingBox.LowerRightPoint.Y);

            Collection<Feature> newfeatures = GetFeaturesForDrawing(newMarginWorldExtent, screenWidth, screenHeight, returningColumnNames);

            return newfeatures;
        }

        public Collection<Feature> GetWrappingFeaturesLeft(RectangleShape boundingBox, double screenWidth, double screenHeight, IEnumerable<string> returningColumnNames, RectangleShape wrappingExtent)
        {
            Validators.CheckFeatureSourceIsOpen(IsOpen);
            Validators.CheckParameterIsNotNull(returningColumnNames, "returningColumnNames");
            Validators.CheckParameterIsNotNull(boundingBox, "boundingBox");
            Validators.CheckParameterIsValid(boundingBox, "boundingBox");
            Validators.CheckValueIsBiggerThanZero(screenWidth, "screenWidth");
            Validators.CheckValueIsBiggerThanZero(screenHeight, "screenHeight");
            Validators.CheckParameterIsNotNull(wrappingExtent, "wrappingExtent");
            Validators.CheckParameterIsValid(wrappingExtent, "wrappingExtent");

            double maxX = boundingBox.LowerRightPoint.X;
            double minX = boundingBox.UpperLeftPoint.X;

            if (minX >= wrappingExtent.LowerLeftPoint.X)
            {
                return new Collection<Feature>();
            }
            if (minX < wrappingExtent.LowerLeftPoint.X)
            {
                maxX = wrappingExtent.LowerRightPoint.X;
                minX = maxX - (wrappingExtent.LowerLeftPoint.X - minX);
            }

            RectangleShape newMarginWorldExtent = new RectangleShape(minX, boundingBox.UpperLeftPoint.Y, maxX, boundingBox.LowerRightPoint.Y);

            Collection<Feature> newfeatures = GetFeaturesForDrawing(newMarginWorldExtent, screenWidth, screenHeight, returningColumnNames);

            return newfeatures;
        }

        public Collection<Feature> GetWrappingFeaturesRight(RectangleShape boundingBox, double screenWidth, double screenHeight, IEnumerable<string> returningColumnNames, RectangleShape wrappingExtent)
        {
            Validators.CheckFeatureSourceIsOpen(IsOpen);
            Validators.CheckParameterIsNotNull(returningColumnNames, "returningColumnNames");
            Validators.CheckParameterIsNotNull(boundingBox, "boundingBox");
            Validators.CheckParameterIsValid(boundingBox, "boundingBox");
            Validators.CheckValueIsBiggerThanZero(screenWidth, "screenWidth");
            Validators.CheckValueIsBiggerThanZero(screenHeight, "screenHeight");
            Validators.CheckParameterIsNotNull(wrappingExtent, "wrappingExtent");
            Validators.CheckParameterIsValid(wrappingExtent, "wrappingExtent");

            double maxX = boundingBox.LowerRightPoint.X;
            double minX = boundingBox.UpperLeftPoint.X;

            if (maxX <= wrappingExtent.LowerRightPoint.X)
            {
                return new Collection<Feature>();
            }
            if (maxX > wrappingExtent.LowerRightPoint.X)
            {
                minX = wrappingExtent.LowerLeftPoint.X;
                maxX = minX + maxX - wrappingExtent.LowerRightPoint.X;
            }

            RectangleShape newMarginWorldExtent = new RectangleShape(minX, boundingBox.UpperLeftPoint.Y, maxX, boundingBox.LowerRightPoint.Y);

            Collection<Feature> newfeatures = GetFeaturesForDrawing(newMarginWorldExtent, screenWidth, screenHeight, returningColumnNames);

            return newfeatures;
        }

     
        public Collection<Feature> GetFeaturesForDrawing(RectangleShape boundingBox, double screenWidth, double screenHeight, ReturningColumnsType returningColumnNamesType)
        {
            Validators.CheckFeatureSourceIsOpen(IsOpen);
            Validators.CheckReturningColumnNamesTypeIsValid(returningColumnNamesType, "returningColumnNamesType");

            Collection<string> returningColumnNames = GetReturningColumnNames(returningColumnNamesType);

            return GetFeaturesForDrawing(boundingBox, screenWidth, screenHeight, returningColumnNames);
        }

        /// <summary>Returns collection of features for drawing.</summary>
        /// <returns>Collection of features for drawing.</returns>
        /// <param name="boundingBox">Bounding box of the features to draw.</param>
        /// <param name="screenWidth">Width of the canvas, in screen pixels, to draw on.</param>
        /// <param name="screenHeight">Height of the canvas, in screen pixels, to draw on.</param>
        /// <param name="returningColumnNamesType">Returning Columns Type.</param> 
        protected virtual Collection<Feature> GetFeaturesForDrawingCore(RectangleShape boundingBox, double screenWidth, double screenHeight, IEnumerable<string> returningColumnNames)
        {
            Validators.CheckFeatureSourceIsOpen(IsOpen);
            Validators.CheckParameterIsNotNull(returningColumnNames, "returningColumnNames");
            Validators.CheckParameterIsNotNull(boundingBox, "boundingBox");
            Validators.CheckParameterIsValid(boundingBox, "boundingBox");
            Validators.CheckValueIsBiggerThanZero(screenWidth, "screenWidth");
            Validators.CheckValueIsBiggerThanZero(screenHeight, "screenHeight");

            return GetFeaturesInsideBoundingBoxCore(boundingBox, returningColumnNames);
        }

        /// <summary>Returns a collection of features based on the target Feature and the spatial query type specified.</summary>
        /// <returns>Collection of features based on the target Feature and the spatial query type specified.</returns>
        /// <remarks>
        /// 	<para>Returns a collection of features based on the target Feature and the
        ///     spatial query type specified below. If there is a current transaction and it is
        ///     marked as live, then the results will include any transaction Feature that
        ///     applies.<br/>
        /// 		<br/>
        /// 		<strong>Spatial Query Types:</strong><br/>
        /// 		<br/>
        /// 		<strong>Disjoint</strong> - Returns a collection of features where the specific Feature
        ///     and the targetShape have no points in common.<br/>
        /// 		<br/>
        /// 		<strong>Intersects</strong> - Returns a collection of features where where the specific
        ///     Feature and the targetShape have at least one point in common.<br/>
        /// 		<br/>
        /// 		<strong>Touches</strong> - Returns a collection of features where the specific Feature
        ///     and the targetShape have at least one boundary point in common, but no interior
        ///     points.<br/>
        /// 		<br/>
        /// 		<strong>Crosses</strong> - Returns a collection of features where the specific Feature
        ///     and the targetShape share some but not all interior points.<br/>
        /// 		<br/>
        /// 		<strong>Within</strong> - Returns a collection of features where the specific Feature
        ///     lies within the interior of the targetShape.<br/>
        /// 		<br/>
        /// 		<strong>Contains</strong> - Returns a collection of features where the specific Feature
        ///     lies within the interior of the current shape.<br/>
        /// 		<br/>
        /// 		<strong>Overlaps</strong> - Returns a collection of features where the specific Feature
        ///     and the targetShape share some but not all points in common.<br/>
        /// 		<br/>
        /// 		<strong>TopologicalEqual</strong> - Returns a collection of features where the specific
        ///     Feature and the target Shape are topologically equal.<br/>
        /// 		<br/>
        ///     The default implementation of SpatialQueryCore uses
        ///     GetFeaturesInsideBoundingBoxCore method to pre-filter the spatial query. It is recommended to provide one's own implementation 
        ///     for this method that is more efficient. When you override this method, it is recommned to use a spatial index<br/>
        /// 		<br/>
        /// 		<br/>
        ///     As a concrete public method that wraps a Core method, Mapgenix reserves the right
        ///     to add events and other logic returned by the Core version of the method. </para>
        /// </remarks>
        /// <param name="targetShape">Target shape in spatial query.</param>
        /// <param name="queryType">Kind of spatial query to perform.</param>
        /// <param name="returningColumnNames">Field names of the column data to return with each Feature.</param>
        public Collection<Feature> SpatialQuery(BaseShape targetShape, QueryType queryType, IEnumerable<string> returningColumnNames)
        {
            Validators.CheckFeatureSourceIsOpen(IsOpen);
            Validators.CheckParameterIsNotNull(targetShape, "targetShape");
            Validators.CheckParameterIsValid(targetShape, "targetShape");
            
            Validators.CheckParameterIsNotNull(returningColumnNames, "returningColumnNames");

            BaseShape projectedTargetShape = targetShape;
            if (Projection != null)
            {
                projectedTargetShape = ConvertToInternalProjection(targetShape);
            }

			Collection<string> uniqColumnNames = ColumnFilter.SplitColumnNames(returningColumnNames);
            Collection<string> fieldNamesInsideSource = GetColumnNamesInsideFeatureSource(uniqColumnNames);
            Collection<string> fieldNamesOutsideOfSource = GetColumnNamesOutsideFeatureSource(uniqColumnNames);
            Collection<Feature> returnFeatures = SpatialQueryCore(projectedTargetShape, queryType, fieldNamesInsideSource);

            returnFeatures = RaiseCustomColumnFetchEvent(returnFeatures, fieldNamesOutsideOfSource);

			if (ColumnFilter.HasComplicateFields(returningColumnNames))
            {
				returnFeatures = ColumnFilter.ReplaceColumnValues(returnFeatures, returningColumnNames);
            }

            if (IsInTransaction && IsTransactionLive)
            {
                foreach (Feature feature in _transactionBuffer.AddBuffer.Values)
                {
					if (ColumnFilter.SpatialQueryIsValid(targetShape, feature.GetShape(), queryType))
                    {
                        returnFeatures.Add(feature);
                    }
                }

                Dictionary<string, Feature> dictionaryFeatures = new Dictionary<string, Feature>();
                if (_transactionBuffer.EditBuffer.Count > 0 || _transactionBuffer.DeleteBuffer.Count > 0)
                {
					dictionaryFeatures = ColumnFilter.GetFeaturesDictionaryFromCollecion(returnFeatures);
                }

                foreach (Feature feature in _transactionBuffer.EditBuffer.Values)
                {
					bool isSatisfiedSpatialQuery = ColumnFilter.SpatialQueryIsValid(targetShape, feature.GetShape(), queryType);
                    if (isSatisfiedSpatialQuery)
                    {
                        if (dictionaryFeatures.ContainsKey(feature.Id))
                        {
                            Feature oringalFeature = dictionaryFeatures[feature.Id];
                            returnFeatures.Remove(oringalFeature);
                        }
                        returnFeatures.Add(feature);
                    }
                    else if (!isSatisfiedSpatialQuery && dictionaryFeatures.ContainsKey(feature.Id))
                    {
                        Feature oringalFeature = dictionaryFeatures[feature.Id];
                        returnFeatures.Remove(oringalFeature);
                    }
                }

                foreach (string id in _transactionBuffer.DeleteBuffer)
                {
                    if (dictionaryFeatures.ContainsKey(id))
                    {
                        Feature feature = dictionaryFeatures[id];
                        returnFeatures.Remove(feature);
                    }
                }
            }

            if (Projection != null)
            {
                returnFeatures = ConvertToExternalProjection(returnFeatures);
            }

            RemoveEmptyAndExcludeFeatures(returnFeatures);

            return returnFeatures;
        }

        /// <summary>Returns a collection of features based on the target Feature and the spatial query type specified.</summary>
        /// <returns>Collection of features based on the target Feature and the spatial query type specified.</returns>
        /// <remarks>
        /// 	<para>Returns a collection of features based on the target Feature and the
        ///     spatial query type specified below. If there is a current transaction and it is
        ///     marked as live, then the results will include any transaction Feature that
        ///     applies.<br/>
        /// 		<br/>
        /// 		<strong>Spatial Query Types:</strong><br/>
        /// 		<br/>
        /// 		<strong>Disjoint</strong> - Returns a collection of features where the specific Feature
        ///     and the targetShape have no points in common.<br/>
        /// 		<br/>
        /// 		<strong>Intersects</strong> - Returns a collection of features where where the specific
        ///     Feature and the targetShape have at least one point in common.<br/>
        /// 		<br/>
        /// 		<strong>Touches</strong> - Returns a collection of features where the specific Feature
        ///     and the targetShape have at least one boundary point in common, but no interior
        ///     points.<br/>
        /// 		<br/>
        /// 		<strong>Crosses</strong> - Returns a collection of features where the specific Feature
        ///     and the targetShape share some but not all interior points.<br/>
        /// 		<br/>
        /// 		<strong>Within</strong> - Returns a collection of features where the specific Feature
        ///     lies within the interior of the targetShape.<br/>
        /// 		<br/>
        /// 		<strong>Contains</strong> - Returns a collection of features where the specific Feature
        ///     lies within the interior of the current shape.<br/>
        /// 		<br/>
        /// 		<strong>Overlaps</strong> - Returns a collection of features where the specific Feature
        ///     and the targetShape share some but not all points in common.<br/>
        /// 		<br/>
        /// 		<strong>TopologicalEqual</strong> - Returns a collection of features where the specific
        ///     Feature and the target Shape are topologically equal.<br/>
        /// 		<br/>
        ///     The default implementation of SpatialQueryCore uses
        ///     GetFeaturesInsideBoundingBoxCore method to pre-filter the spatial query. It is recommended to provide one's own implementation 
        ///     for this method that is more efficient. When you override this method, it is recommned to use a spatial index<br/>
        /// 		<br/>
        /// 		<br/>
        ///     As a concrete public method that wraps a Core method, Mapgenix reserves the right
        ///     to add events and other logic returned by the Core version of the method. </para>
        /// </remarks>
        /// <param name="targetShape">Target shape in spatial query.</param>
        /// <param name="queryType">Kind of spatial query to perform.</param>
        /// <param name="returningColumnNamesType">Column type.</param>  
        public Collection<Feature> SpatialQuery(BaseShape targetShape, QueryType queryType, ReturningColumnsType returningColumnNamesType)
        {
            Validators.CheckFeatureSourceIsOpen(IsOpen);
            Validators.CheckReturningColumnNamesTypeIsValid(returningColumnNamesType, "returningColumnNamesType");

            Collection<string> returningColumnNames = GetReturningColumnNames(returningColumnNamesType);

            return SpatialQuery(targetShape, queryType, returningColumnNames);
        }

        /// <summary>Returns a collection of features based on the target Feature and the spatial query type specified.</summary>
        /// <returns>Collection of features based on the target Feature and the spatial query type specified.</returns>
        /// <remarks>
        /// 	<para>Returns a collection of features based on the target Feature and the
        ///     spatial query type specified below. If there is a current transaction and it is
        ///     marked as live, then the results will include any transaction Feature that
        ///     applies.<br/>
        /// 		<br/>
        /// 		<strong>Spatial Query Types:</strong><br/>
        /// 		<br/>
        /// 		<strong>Disjoint</strong> - Returns a collection of features where the specific Feature
        ///     and the targetShape have no points in common.<br/>
        /// 		<br/>
        /// 		<strong>Intersects</strong> - Returns a collection of features where where the specific
        ///     Feature and the targetShape have at least one point in common.<br/>
        /// 		<br/>
        /// 		<strong>Touches</strong> - Returns a collection of features where the specific Feature
        ///     and the targetShape have at least one boundary point in common, but no interior
        ///     points.<br/>
        /// 		<br/>
        /// 		<strong>Crosses</strong> - Returns a collection of features where the specific Feature
        ///     and the targetShape share some but not all interior points.<br/>
        /// 		<br/>
        /// 		<strong>Within</strong> - Returns a collection of features where the specific Feature
        ///     lies within the interior of the targetShape.<br/>
        /// 		<br/>
        /// 		<strong>Contains</strong> - Returns a collection of features where the specific Feature
        ///     lies within the interior of the current shape.<br/>
        /// 		<br/>
        /// 		<strong>Overlaps</strong> - Returns a collection of features where the specific Feature
        ///     and the targetShape share some but not all points in common.<br/>
        /// 		<br/>
        /// 		<strong>TopologicalEqual</strong> - Returns a collection of features where the specific
        ///     Feature and the target Shape are topologically equal.<br/>
        /// 		<br/>
        ///     The default implementation of SpatialQueryCore uses
        ///     GetFeaturesInsideBoundingBoxCore method to pre-filter the spatial query. It is recommended to provide one's own implementation 
        ///     for this method that is more efficient. When you override this method, it is recommned to use a spatial index<br/>
        /// 		<br/>
        /// 		<br/>
        ///     As a concrete public method that wraps a Core method, Mapgenix reserves the right
        ///     to add events and other logic returned by the Core version of the method. </para>
        /// </remarks>
        /// <param name="targetShape">Target shape in spatial query.</param>
        /// <param name="queryType">Kind of spatial query to perform.</param>
        /// <param name="returningColumnNames">Column names.</param>  
        public Collection<Feature> SpatialQuery(Feature feature, QueryType queryType, IEnumerable<string> returningColumnNames)
        {
            Validators.CheckFeatureSourceIsOpen(IsOpen);
            Validators.CheckParameterIsNotNull(feature, "feature");
            Validators.CheckParameterIsNotNull(queryType, "queryType");
            
            Validators.CheckParameterIsNotNull(returningColumnNames, "returningColumnNames");

            BaseShape targetBaseShape = feature.GetShape();

            return SpatialQuery(targetBaseShape, queryType, returningColumnNames);
        }

        /// <summary>Returns a collection of features based on the target Feature and the spatial query type specified.</summary>
        /// <returns>Collection of features based on the target Feature and the spatial query type specified.</returns>
        /// <remarks>
        /// 	<para>Returns a collection of features based on the target Feature and the
        ///     spatial query type specified below. If there is a current transaction and it is
        ///     marked as live, then the results will include any transaction Feature that
        ///     applies.<br/>
        /// 		<br/>
        /// 		<strong>Spatial Query Types:</strong><br/>
        /// 		<br/>
        /// 		<strong>Disjoint</strong> - Returns a collection of features where the specific Feature
        ///     and the targetShape have no points in common.<br/>
        /// 		<br/>
        /// 		<strong>Intersects</strong> - Returns a collection of features where where the specific
        ///     Feature and the targetShape have at least one point in common.<br/>
        /// 		<br/>
        /// 		<strong>Touches</strong> - Returns a collection of features where the specific Feature
        ///     and the targetShape have at least one boundary point in common, but no interior
        ///     points.<br/>
        /// 		<br/>
        /// 		<strong>Crosses</strong> - Returns a collection of features where the specific Feature
        ///     and the targetShape share some but not all interior points.<br/>
        /// 		<br/>
        /// 		<strong>Within</strong> - Returns a collection of features where the specific Feature
        ///     lies within the interior of the targetShape.<br/>
        /// 		<br/>
        /// 		<strong>Contains</strong> - Returns a collection of features where the specific Feature
        ///     lies within the interior of the current shape.<br/>
        /// 		<br/>
        /// 		<strong>Overlaps</strong> - Returns a collection of features where the specific Feature
        ///     and the targetShape share some but not all points in common.<br/>
        /// 		<br/>
        /// 		<strong>TopologicalEqual</strong> - Returns a collection of features where the specific
        ///     Feature and the target Shape are topologically equal.<br/>
        /// 		<br/>
        ///     The default implementation of SpatialQueryCore uses
        ///     GetFeaturesInsideBoundingBoxCore method to pre-filter the spatial query. It is recommended to provide one's own implementation 
        ///     for this method that is more efficient. When you override this method, it is recommned to use a spatial index<br/>
        /// 		<br/>
        /// 		<br/>
        ///     As a concrete public method that wraps a Core method, Mapgenix reserves the right
        ///     to add events and other logic returned by the Core version of the method. </para>
        /// </remarks>
        /// <param name="targetShape">Target shape in spatial query.</param>
        /// <param name="queryType">Kind of spatial query to perform.</param>
        /// <param name="returningColumnNamesType">Column type.</param>  
        public Collection<Feature> SpatialQuery(Feature feature, QueryType queryType, ReturningColumnsType returningColumnNamesType)
        {
            Validators.CheckFeatureSourceIsOpen(IsOpen);
            Validators.CheckReturningColumnNamesTypeIsValid(returningColumnNamesType, "returningColumnNamesType");

            Collection<string> returningColumnNames = GetReturningColumnNames(returningColumnNamesType);

            return SpatialQuery(feature, queryType, returningColumnNames);
        }


        /// <summary>Returns a collection of features based on the target Feature and the spatial query type specified.</summary>
        /// <returns>Collection of features based on the target Feature and the spatial query type specified.</returns>
        /// <remarks>
        /// 	<para>Returns a collection of features based on the target Feature and the
        ///     spatial query type specified below. If there is a current transaction and it is
        ///     marked as live, then the results will include any transaction Feature that
        ///     applies.<br/>
        /// 		<br/>
        /// 		<strong>Spatial Query Types:</strong><br/>
        /// 		<br/>
        /// 		<strong>Disjoint</strong> - Returns a collection of features where the specific Feature
        ///     and the targetShape have no points in common.<br/>
        /// 		<br/>
        /// 		<strong>Intersects</strong> - Returns a collection of features where where the specific
        ///     Feature and the targetShape have at least one point in common.<br/>
        /// 		<br/>
        /// 		<strong>Touches</strong> - Returns a collection of features where the specific Feature
        ///     and the targetShape have at least one boundary point in common, but no interior
        ///     points.<br/>
        /// 		<br/>
        /// 		<strong>Crosses</strong> - Returns a collection of features where the specific Feature
        ///     and the targetShape share some but not all interior points.<br/>
        /// 		<br/>
        /// 		<strong>Within</strong> - Returns a collection of features where the specific Feature
        ///     lies within the interior of the targetShape.<br/>
        /// 		<br/>
        /// 		<strong>Contains</strong> - Returns a collection of features where the specific Feature
        ///     lies within the interior of the current shape.<br/>
        /// 		<br/>
        /// 		<strong>Overlaps</strong> - Returns a collection of features where the specific Feature
        ///     and the targetShape share some but not all points in common.<br/>
        /// 		<br/>
        /// 		<strong>TopologicalEqual</strong> - Returns a collection of features where the specific
        ///     Feature and the target Shape are topologically equal.<br/>
        /// 		<br/>
        ///     The default implementation of SpatialQueryCore uses
        ///     GetFeaturesInsideBoundingBoxCore method to pre-filter the spatial query. It is recommended to provide one's own implementation 
        ///     for this method that is more efficient. When you override this method, it is recommned to use a spatial index<br/>
        /// 		<br/>
        /// 		<br/>
        ///     As a concrete public method that wraps a Core method, Mapgenix reserves the right
        ///     to add events and other logic returned by the Core version of the method. </para>
        /// </remarks>
        /// <param name="targetShape">Target shape in spatial query.</param>
        /// <param name="queryType">Kind of spatial query to perform.</param>
        /// <param name="returningColumnNames">Column names.</param>  
        protected virtual Collection<Feature> SpatialQueryCore(BaseShape targetShape, QueryType queryType, IEnumerable<string> returningColumnNames)
        {
            Validators.CheckFeatureSourceIsOpen(IsOpen);
            Validators.CheckParameterIsNotNull(targetShape, "targetShape");
            Validators.CheckParameterIsValid(targetShape, "targetShape");
            
            Validators.CheckParameterIsNotNull(returningColumnNames, "returningColumnNames");

            Collection<Feature> returnValues = new Collection<Feature>();
            switch (queryType)
            {
                case QueryType.Contains:
                    returnValues = GetFeaturesContains(targetShape, returningColumnNames);
                    break;
                case QueryType.Crosses:
                    returnValues = GetFeaturesCrosses(targetShape, returningColumnNames);
                    break;
                case QueryType.Disjoint:
                    returnValues = GetFeaturesDisjoint(targetShape, returningColumnNames);
                    break;
                case QueryType.Intersects:
                    returnValues = GetFeaturesIntersects(targetShape, returningColumnNames);
                    break;
                case QueryType.TopologicalEqual:
                    returnValues = GetFeaturesIsTopologicalEqual(targetShape, returningColumnNames);
                    break;
                case QueryType.Overlaps:
                    returnValues = GetFeaturesOverlaps(targetShape, returningColumnNames);
                    break;
                case QueryType.Touches:
                    returnValues = GetFeaturesTouches(targetShape, returningColumnNames);
                    break;
                case QueryType.Within:
                    returnValues = GetFeaturesWithin(targetShape, returningColumnNames);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("queryType", ExceptionDescription.EnumerationOutOfRange);
            }
            return returnValues;
        }

        #region SpatialQuery

        private Collection<Feature> GetFeaturesContains(BaseShape targetShape, IEnumerable<string> returningColumnNames)
        {
            RectangleShape boundingBox = targetShape.GetBoundingBox();
            Collection<Feature> allPossibleFeatures = GetFeaturesInsideBoundingBoxCore(boundingBox, returningColumnNames);

            Collection<Feature> returnFeatures = new Collection<Feature>();
            foreach (Feature feature in allPossibleFeatures)
            {
                if (feature.GetShape().Contains(targetShape))
                {
                    returnFeatures.Add(feature);
                }
            }
            return returnFeatures;
        }

        private Collection<Feature> GetFeaturesCrosses(BaseShape targetShape, IEnumerable<string> returningColumnNames)
        {
            RectangleShape boundingBox = targetShape.GetBoundingBox();
            Collection<Feature> allPossibleFeatures = GetFeaturesInsideBoundingBoxCore(boundingBox, returningColumnNames);

            Collection<Feature> returnFeatures = new Collection<Feature>();
            foreach (Feature feature in allPossibleFeatures)
            {
                if (feature.GetShape().Crosses(targetShape))
                {
                    returnFeatures.Add(feature);
                }
            }
            return returnFeatures;
        }

        private Collection<Feature> GetFeaturesDisjoint(BaseShape targetShape, IEnumerable<string> returningColumnNames)
        {
            RectangleShape boundingBox = targetShape.GetBoundingBox();
            Collection<Feature> outsideBoundingBoxFeatures = GetFeaturesOutsideBoundingBoxCore(boundingBox, returningColumnNames);
            Collection<Feature> insideBoundingBoxFeatures = GetFeaturesInsideBoundingBoxCore(boundingBox, returningColumnNames);

            Collection<Feature> returnFeatures = new Collection<Feature>();

            foreach (Feature feature in outsideBoundingBoxFeatures)
            {
                if (feature.GetShape().IsDisjointed(targetShape))
                {
                    returnFeatures.Add(feature);
                }
            }


            foreach (Feature feature in insideBoundingBoxFeatures)
            {
                if (feature.GetShape().IsDisjointed(targetShape))
                {
                    returnFeatures.Add(feature);
                }
            }
            return returnFeatures;
        }

        private Collection<Feature> GetFeaturesIntersects(BaseShape targetShape, IEnumerable<string> returningColumnNames)
        {
            RectangleShape boundingBox = targetShape.GetBoundingBox();
            Collection<Feature> allPossibleFeatures = GetFeaturesInsideBoundingBoxCore(boundingBox, returningColumnNames);

            Collection<Feature> returnFeatures = new Collection<Feature>();
            foreach (Feature feature in allPossibleFeatures)
            {
                if (feature.GetShape().Intersects(targetShape))
                {
                    returnFeatures.Add(feature);
                }
            }
            return returnFeatures;
        }

        private Collection<Feature> GetFeaturesIsTopologicalEqual(BaseShape targetShape, IEnumerable<string> returningColumnNames)
        {
            RectangleShape boundingBox = targetShape.GetBoundingBox();
            Collection<Feature> allPossibleFeatures = GetFeaturesInsideBoundingBoxCore(boundingBox, returningColumnNames);

            Collection<Feature> returnFeatures = new Collection<Feature>();
            foreach (Feature feature in allPossibleFeatures)
            {
                if (feature.GetShape().IsTopologicallyEqual(targetShape))
                {
                    returnFeatures.Add(feature);
                }
            }
            return returnFeatures;
        }

        private Collection<Feature> GetFeaturesOverlaps(BaseShape targetShape, IEnumerable<string> returningColumnNames)
        {
            RectangleShape boundingBox = targetShape.GetBoundingBox();
            Collection<Feature> allPossibleFeatures = GetFeaturesInsideBoundingBoxCore(boundingBox, returningColumnNames);

            Collection<Feature> returnFeatures = new Collection<Feature>();
            foreach (Feature feature in allPossibleFeatures)
            {
                if (feature.GetShape().Overlaps(targetShape))
                {
                    returnFeatures.Add(feature);
                }
            }
            return returnFeatures;
        }

        private Collection<Feature> GetFeaturesTouches(BaseShape targetShape, IEnumerable<string> returningColumnNames)
        {
            RectangleShape boundingBox = targetShape.GetBoundingBox();
            Collection<Feature> allPossibleFeatures = GetFeaturesInsideBoundingBoxCore(boundingBox, returningColumnNames);

            Collection<Feature> returnFeatures = new Collection<Feature>();
            foreach (Feature feature in allPossibleFeatures)
            {
                if (feature.GetShape().Touches(targetShape))
                {
                    returnFeatures.Add(feature);
                }
            }
            return returnFeatures;
        }

        private Collection<Feature> GetFeaturesWithin(BaseShape targetShape, IEnumerable<string> returningColumnNames)
        {
            RectangleShape boundingBox = targetShape.GetBoundingBox();
            Collection<Feature> allPossibleFeatures = GetFeaturesInsideBoundingBoxCore(boundingBox, returningColumnNames);

            Collection<Feature> returnFeatures = new Collection<Feature>();
            foreach (Feature feature in allPossibleFeatures)
            {
                if (feature.GetShape().IsWithin(targetShape))
                {
                    returnFeatures.Add(feature);
                }
            }
            return returnFeatures;
        }

       
        #endregion

        /// <summary>Returns a user defined number collection of features that are closest to the TargetShape.</summary>
        /// <returns>User defined number collection of features that are closest to the TargetShape.</returns>
        /// <remarks>It is important to note that the TargetShape and the FeatureSource
        ///     must be in the same unit, such as feet or meters. If there is a current transaction and it is marked as live, then
        ///     the results will include any transaction Feature that applies.</remarks>
        /// <param name="targetShape">Shape to find nearest features from.</param>
        /// <param name="unitOfData">Unit of data that the TargetShape and the FeatureSource are in, such as feet, meters, etc.</param>
        /// <param name="maxItemsToFind">Maximum number of features to find.</param>
        /// <param name="returningColumnNames">Column names to return with each Feature.</param>
        public Collection<Feature> GetFeaturesNearestTo(BaseShape targetShape, GeographyUnit unitOfFeatureSource, int maxItemsToFind, IEnumerable<string> returningColumnNames)
        {
            Validators.CheckFeatureSourceIsOpen(IsOpen);
            Validators.CheckIfInputValueIsBiggerThan(maxItemsToFind, "maxItemsToFind", 0, RangeCheckingInclusion.IncludeValue);
            Validators.CheckParameterIsNotNull(targetShape, "targetShape");
            Validators.CheckParameterIsValid(targetShape, "targetShape");
            Validators.CheckParameterIsNotNull(returningColumnNames, "returningColumnNames");
            Validators.CheckGeographyUnitIsValid(unitOfFeatureSource, "unitOfFeatureSource");

            BaseShape projectedShape = targetShape;
            GeographyUnit internalGeographyUnit = unitOfFeatureSource;
            if (Projection != null)
            {
                internalGeographyUnit = Projection.GetInternalGeographyUnit();
                if (internalGeographyUnit == GeographyUnit.Unknown)
                {
                    throw new InvalidOperationException(ExceptionDescription.ProjectionInternalGeographyIsUnknown);
                }
                projectedShape = ConvertToInternalProjection(targetShape);
            }

            int numberOfItemsToFindConsiderBuffer = maxItemsToFind;
            if (IsInTransaction && IsTransactionLive)
            {
                numberOfItemsToFindConsiderBuffer += _transactionBuffer.DeleteBuffer.Count + _transactionBuffer.EditBuffer.Count;
            }

			Collection<string> uniqColumnNames = ColumnFilter.SplitColumnNames(returningColumnNames);
            Collection<string> fieldNamesInsideSource = GetColumnNamesInsideFeatureSource(uniqColumnNames);
            Collection<string> fieldNamesOutsideOfSource = GetColumnNamesOutsideFeatureSource(uniqColumnNames);

            Collection<Feature> returnFeatures = GetFeaturesNearestToCore(projectedShape, internalGeographyUnit, numberOfItemsToFindConsiderBuffer, fieldNamesInsideSource);

            returnFeatures = RaiseCustomColumnFetchEvent(returnFeatures, fieldNamesOutsideOfSource);

			if (ColumnFilter.HasComplicateFields(returningColumnNames))
            {
				returnFeatures = ColumnFilter.ReplaceColumnValues(returnFeatures, returningColumnNames);
            }

            if (IsInTransaction && IsTransactionLive)
            {
				Dictionary<string, Feature> dictionaryFeaturesConsiderBuffer = ColumnFilter.GetFeaturesDictionaryFromCollecion(returnFeatures);

                foreach (Feature feature in _transactionBuffer.AddBuffer.Values)
                {
                    dictionaryFeaturesConsiderBuffer.Add(feature.Id, feature);
                }

                foreach (Feature feature in _transactionBuffer.EditBuffer.Values)
                {
                    if (dictionaryFeaturesConsiderBuffer.ContainsKey(feature.Id))
                    {
                        dictionaryFeaturesConsiderBuffer.Remove(feature.Id);
                    }

                    dictionaryFeaturesConsiderBuffer.Add(feature.Id, feature);
                }

                foreach (string id in _transactionBuffer.DeleteBuffer)
                {
                    if (dictionaryFeaturesConsiderBuffer.ContainsKey(id))
                    {
                        dictionaryFeaturesConsiderBuffer.Remove(id);
                    }
                }
                Collection<Feature> collectionFeatruesConsiderBuffer = ColumnFilter.GetFeaturesCollectionFromDictionary(dictionaryFeaturesConsiderBuffer);
                returnFeatures = ColumnFilter.GetFeaturesNearestFrom(collectionFeatruesConsiderBuffer, projectedShape, internalGeographyUnit, maxItemsToFind);
            }

            RemoveEmptyAndExcludeFeatures(returnFeatures);

            if (Projection != null)
            {
                returnFeatures = ConvertToExternalProjection(returnFeatures);
            }

            return returnFeatures;
        }

       
        public Collection<Feature> GetFeaturesNearestTo(BaseShape targetShape, GeographyUnit unitOfData, int maxItemsToFind, ReturningColumnsType returningColumnNamesType)
        {
            Validators.CheckFeatureSourceIsOpen(IsOpen);
            Validators.CheckReturningColumnNamesTypeIsValid(returningColumnNamesType, "returningColumnNamesType");

            Collection<string> returningColumnNames = GetReturningColumnNames(returningColumnNamesType);

            return GetFeaturesNearestTo(targetShape, unitOfData, maxItemsToFind, returningColumnNames);
        }


        /// <summary>Returns a user defined number collection of features that are closest to the TargetShape.</summary>
        /// <returns>User defined number collection of features that are closest to the TargetShape.</returns>
        /// <remarks>It is important to note that the TargetShape and the FeatureSource
        ///     must be in the same unit, such as feet or meters. If there is a current transaction and it is marked as live, then
        ///     the results will include any transaction Feature that applies.</remarks>
        /// <param name="targetShape">Shape to find nearest features from.</param>
        /// <param name="unitOfData">Unit of data that the TargetShape and the FeatureSource are in, such as feet, meters, etc.</param>
        /// <param name="maxItemsToFind">Maximum number of features to find.</param>
        /// <param name="returningColumnNames">Column names.</param>
        public Collection<Feature> GetFeaturesNearestTo(Feature targetFeature, GeographyUnit unitOfData, int maxItemsToFind, IEnumerable<string> returningColumnNames)
        {
            Validators.CheckFeatureSourceIsOpen(IsOpen);
            Validators.CheckIfInputValueIsBiggerThan(maxItemsToFind, "maxItemsToFind", 0, RangeCheckingInclusion.IncludeValue);
            Validators.CheckParameterIsNotNull(returningColumnNames, "returningColumnNames");
            Validators.CheckGeographyUnitIsValid(unitOfData, "unitOfData");

            BaseShape targetShape = targetFeature.GetShape();

            return GetFeaturesNearestTo(targetShape, unitOfData, maxItemsToFind, returningColumnNames);
        }

        /// <summary>Returns a user defined number collection of features that are closest to the TargetShape.</summary>
        /// <returns>User defined number collection of features that are closest to the TargetShape.</returns>
        /// <remarks>It is important to note that the TargetShape and the FeatureSource
        ///     must be in the same unit, such as feet or meters. If there is a current transaction and it is marked as live, then
        ///     the results will include any transaction Feature that applies.</remarks>
        /// <param name="targetFeature">Feature to find nearest features from.</param>
        /// <param name="unitOfData">Unit of data that the TargetShape and the FeatureSource are in, such as feet, meters, etc.</param>
        /// <param name="maxItemsToFind">Maximum number of features to find.</param>
        /// <param name="returningColumnNamesType">Column type.</param>
        public Collection<Feature> GetFeaturesNearestTo(Feature targetFeature, GeographyUnit unitOfData, int maxItemsToFind, ReturningColumnsType returningColumnNamesType)
        {
            Validators.CheckFeatureSourceIsOpen(IsOpen);
            Validators.CheckReturningColumnNamesTypeIsValid(returningColumnNamesType, "returningColumnNamesType");

            Collection<string> returningColumnNames = GetReturningColumnNames(returningColumnNamesType);

            return GetFeaturesNearestTo(targetFeature, unitOfData, maxItemsToFind, returningColumnNames);
        }

        /// <summary>Returns a user defined number collection of features that are closest to the TargetShape.</summary>
        /// <returns>User defined number collection of features that are closest to the TargetShape.</returns>
        /// <remarks>It is important to note that the TargetShape and the FeatureSource
        ///     must be in the same unit, such as feet or meters. If there is a current transaction and it is marked as live, then
        ///     the results will include any transaction Feature that applies.</remarks>
        /// <param name="targetShape">Shape to find nearest features from.</param>
        /// <param name="unitOfData">Unit of data that the TargetShape and the FeatureSource are in, such as feet, meters, etc.</param>
        /// <param name="maxItemsToFind">Maximum number of features to find.</param>
        /// <param name="returningColumnNames">Column names.</param>
        /// <param name="searchRadius">Maximim search radius</param>
        /// <param name="unitOfSearchRadius">Distance unit of the search radius</param>
        public Collection<Feature> GetFeaturesNearestTo(BaseShape targetShape, GeographyUnit unitOfData, int maxItemsToFind, IEnumerable<string> returningColumnNames, double searchRadius, DistanceUnit unitOfSearchRadius)
        {
            Validators.CheckFeatureSourceIsOpen(IsOpen);
            Validators.CheckIfInputValueIsBiggerThan(maxItemsToFind, "maxItemsToFind", 0, RangeCheckingInclusion.IncludeValue);
            Validators.CheckParameterIsNotNull(targetShape, "targetShape");
            Validators.CheckParameterIsValid(targetShape, "targetShape");
            Validators.CheckParameterIsNotNull(returningColumnNames, "returningColumnNames");
            Validators.CheckGeographyUnitIsValid(unitOfData, "unitOfData");

            Collection<Feature> features = GetFeaturesWithinDistanceOf(targetShape, unitOfData, unitOfSearchRadius, searchRadius, returningColumnNames);
            Collection<Feature> newFeatures = new Collection<Feature>();
            Collection<double> distances = new Collection<double>();
            foreach (Feature feature in features)
            {
                BaseShape tempShape = feature.GetShape();
                double tempDistance = targetShape.GetDistanceTo(tempShape, unitOfData, unitOfSearchRadius);
                if (tempDistance <= searchRadius)
                {
                    int i = 0;
                    for (i = 0; i < distances.Count; i++)
                    {
                        if (distances[i] > tempDistance)
                        {
                            break;
                        }
                    }
                    distances.Insert(i, tempDistance);
                    newFeatures.Insert(i, feature);
                }
            }

            Collection<Feature> result = new Collection<Feature>();
            if (newFeatures.Count <= maxItemsToFind)
            {
                result = newFeatures;
            }
            else
            {
                for (int i = 0; i < maxItemsToFind; i++)
                {
                    result.Add(newFeatures[i]);
                }
            }
            return result;
        }

        /// <summary>Returns a user defined number collection of features that are closest to the TargetShape.</summary>
        /// <returns>User defined number collection of features that are closest to the TargetShape.</returns>
        /// <remarks>It is important to note that the TargetShape and the FeatureSource
        ///     must be in the same unit, such as feet or meters. If there is a current transaction and it is marked as live, then
        ///     the results will include any transaction Feature that applies.</remarks>
        /// <param name="targetFeature">Featuree to find nearest features from.</param>
        /// <param name="unitOfData">Unit of data that the TargetShape and the FeatureSource are in, such as feet, meters, etc.</param>
        /// <param name="maxItemsToFind">Maximum number of features to find.</param>
        /// <param name="returningColumnNames">Column names.</param>
        /// <param name="searchRadius">Maximim search radius</param>
        /// <param name="unitOfSearchRadius">Distance unit of the search radius</param>
        public Collection<Feature> GetFeaturesNearestTo(Feature targetFeature, GeographyUnit unitOfData, int maxItemsToFind, IEnumerable<string> returningColumnNames, double searchRadius, DistanceUnit unitOfSearchRadius)
        {
            BaseShape targetShape = targetFeature.GetShape();

            return GetFeaturesNearestTo(targetShape, unitOfData, maxItemsToFind, returningColumnNames, searchRadius, unitOfSearchRadius);
        }

        /// <summary>Returns a user defined number collection of features that are closest to the TargetShape.</summary>
        /// <returns>User defined number collection of features that are closest to the TargetShape.</returns>
        /// <remarks>It is important to note that the TargetShape and the FeatureSource
        ///     must be in the same unit, such as feet or meters. If there is a current transaction and it is marked as live, then
        ///     the results will include any transaction Feature that applies.</remarks>
        /// <param name="targetShape">Shape to find nearest features from.</param>
        /// <param name="unitOfData">Unit of data that the TargetShape and the FeatureSource are in, such as feet, meters, etc.</param>
        /// <param name="maxItemsToFind">Maximum number of features to find.</param>
        /// <param name="returningColumnNames">Column names.</param>
        protected virtual Collection<Feature> GetFeaturesNearestToCore(BaseShape targetShape, GeographyUnit unitOfData, int maxItemsToFind, IEnumerable<string> returningColumnNames)
        {
            Validators.CheckFeatureSourceIsOpen(IsOpen);
            Validators.CheckIfInputValueIsBiggerThan(maxItemsToFind, "maxItemsToFind", 0, RangeCheckingInclusion.IncludeValue);
            Validators.CheckParameterIsNotNull(targetShape, "targetShape");
            Validators.CheckParameterIsValid(targetShape, "targetShape");
            Validators.CheckParameterIsNotNull(returningColumnNames, "returningColumnNames");
            Validators.CheckGeographyUnitIsValid(unitOfData, "unitOfData");

            RectangleShape boundingBox = targetShape.GetBoundingBox();
            if (Math.Abs(boundingBox.Width) < 1e-6)
            {
                double centerX = boundingBox.GetCenterPoint().X;
                boundingBox.UpperLeftPoint.X = centerX - 1e-6;
                boundingBox.LowerRightPoint.X = centerX + 1e-6;
            }
            if (Math.Abs(boundingBox.Height) < 1e-6)
            {
                double centerY = boundingBox.GetCenterPoint().Y;
                boundingBox.UpperLeftPoint.Y = centerY + 1e-6;
                boundingBox.LowerRightPoint.Y = centerY - 1e-6;
            }

            Collection<Feature> allPossibleResults = new Collection<Feature>();
            int maxCount = GetCountCore();
            if (maxItemsToFind >= maxCount)
            {
                allPossibleResults = GetAllFeaturesCore(returningColumnNames);
            }
            else
            {
                for (int i = 0; i < MaximumLoopCount; i++)
                {
                    if (allPossibleResults.Count >= maxItemsToFind)
                    {
                        break;
                    }
                    allPossibleResults = GetFeaturesInsideBoundingBoxCore(boundingBox, returningColumnNames);
                    boundingBox.ScaleUp(200);
                }
            }

			Collection<Feature> results = ColumnFilter.GetFeaturesNearestFrom(allPossibleResults, targetShape, unitOfData, maxItemsToFind);
            return results;
        }

        /// <summary>Returns a collection of features that are within a certain distance of a TargetShape.</summary>
        /// <returns>Collection of features that are within a certain distance of a TargetShape</returns>
        /// <remarks> It is important to note that the TargetShape and the
        ///     FeatureSource must use the same unit, such as feet or meters. If there is a current transaction and
        ///     it is marked as live, then the results will include any transaction Feature that
        ///     applies.</remarks>
        /// <param name="targetShape">Shape to find features within a distance of.</param>
        /// <param name="unitOfData">Unit of data that the FeatureSource and TargetShape are in.</param>
        /// <param name="distanceUnit">Unit of the distance parameter, such as feet, miles, kilometers, etc.</param>
        /// <param name="distance">Distance</param>
        /// <param name="returningColumnNames">Column names.</param>
        public Collection<Feature> GetFeaturesWithinDistanceOf(BaseShape targetShape, GeographyUnit unitOfData, DistanceUnit distanceUnit, double distance, IEnumerable<string> returningColumnNames)
        {
            Validators.CheckFeatureSourceIsOpen(IsOpen);
            Validators.CheckParameterIsNotNull(targetShape, "targetShape");
            Validators.CheckParameterIsValid(targetShape, "targetShape");
            Validators.CheckGeographyUnitIsValid(unitOfData, "unitOfData");
            Validators.CheckDistanceUnitIsValid(distanceUnit, "distanceUnit");
            Validators.CheckParameterIsNotNull(returningColumnNames, "returningColumnNames");

			Collection<string> uniqColumnNames = ColumnFilter.SplitColumnNames(returningColumnNames);
            Collection<string> fieldNamesInsideSource = GetColumnNamesInsideFeatureSource(uniqColumnNames);
            Collection<string> fieldNamesOutsideOfSource = GetColumnNamesOutsideFeatureSource(uniqColumnNames);

            BaseShape projectedTargetShape = targetShape;

            GeographyUnit internalGeographyUnit = unitOfData;
            if (Projection != null)
            {
                internalGeographyUnit = Projection.GetInternalGeographyUnit();
                if (internalGeographyUnit == GeographyUnit.Unknown)
                {
                    throw new InvalidOperationException(ExceptionDescription.ProjectionInternalGeographyIsUnknown);
                }
                projectedTargetShape = ConvertToInternalProjection(targetShape);
            }

            Collection<Feature> returnFeatures = GetFeaturesWithinDistanceOfCore(projectedTargetShape, internalGeographyUnit, distanceUnit, distance, fieldNamesInsideSource);

            returnFeatures = RaiseCustomColumnFetchEvent(returnFeatures, fieldNamesOutsideOfSource);

			if (ColumnFilter.HasComplicateFields(returningColumnNames))
            {
				returnFeatures = ColumnFilter.ReplaceColumnValues(returnFeatures, returningColumnNames);
            }

            if (IsInTransaction && IsTransactionLive)
            {
                BaseShape baseShapeInFeature = null;
                foreach (Feature feature in _transactionBuffer.AddBuffer.Values)
                {
                    baseShapeInFeature = feature.GetShape();
                    double currentDistance = baseShapeInFeature.GetDistanceTo(targetShape, internalGeographyUnit, distanceUnit);
                    if (currentDistance < distance)
                    {
                        returnFeatures.Add(feature);
                    }
                }

                Dictionary<string, Feature> recordsInDictionary = new Dictionary<string, Feature>();
                if (_transactionBuffer.EditBuffer.Count > 0 || _transactionBuffer.DeleteBuffer.Count > 0)
                {
					recordsInDictionary = ColumnFilter.GetFeaturesDictionaryFromCollecion(returnFeatures);
                }

                foreach (Feature feature in _transactionBuffer.EditBuffer.Values)
                {
                    baseShapeInFeature = feature.GetShape();
                    double currentDistance = baseShapeInFeature.GetDistanceTo(targetShape, internalGeographyUnit, distanceUnit);
                    if (currentDistance < distance)
                    {
                        if (recordsInDictionary.ContainsKey(feature.Id))
                        {
                            Feature oringalFeature = recordsInDictionary[feature.Id];
                            returnFeatures.Remove(oringalFeature);
                        }
                        returnFeatures.Add(feature);
                    }
                    else if (currentDistance > distance && recordsInDictionary.ContainsKey(feature.Id))
                    {
                        Feature oringalFeature = recordsInDictionary[feature.Id];
                        returnFeatures.Remove(oringalFeature);
                    }
                }

                foreach (string id in _transactionBuffer.DeleteBuffer)
                {
                    if (recordsInDictionary.ContainsKey(id))
                    {
                        BaseShape baseShape = recordsInDictionary[id].GetShape();
                        double currentDistance = baseShape.GetDistanceTo(targetShape, internalGeographyUnit, distanceUnit);
                        if (currentDistance < distance && recordsInDictionary.ContainsKey(id))
                        {
                            Feature feature = recordsInDictionary[id];
                            returnFeatures.Remove(feature);
                        }
                    }
                }
            }

            RemoveEmptyAndExcludeFeatures(returnFeatures);

            if (Projection != null)
            {
                returnFeatures = ConvertToExternalProjection(returnFeatures);
            }

            return returnFeatures;
        }

        /// <summary>Returns a collection of features that are within a certain distance of a TargetShape.</summary>
        /// <returns>Collection of features that are within a certain distance of a TargetShape</returns>
        /// <remarks> It is important to note that the TargetShape and the
        ///     FeatureSource must use the same unit, such as feet or meters. If there is a current transaction and
        ///     it is marked as live, then the results will include any transaction Feature that
        ///     applies.</remarks>
        /// <param name="targetShape">Shape to find features within a distance of.</param>
        /// <param name="unitOfData">Unit of data that the FeatureSource and TargetShape are in.</param>
        /// <param name="distanceUnit">Unit of the distance parameter, such as feet, miles, kilometers, etc.</param>
        /// <param name="distance">Distance</param>
        /// <param name="returningColumnNamesType">Column type.</param>
        public Collection<Feature> GetFeaturesWithinDistanceOf(BaseShape targetShape, GeographyUnit unitOfData, DistanceUnit distanceUnit, double distance, ReturningColumnsType returningColumnNamesType)
        {
            Validators.CheckFeatureSourceIsOpen(IsOpen);
            Validators.CheckReturningColumnNamesTypeIsValid(returningColumnNamesType, "returningColumnNamesType");

            Collection<string> returningColumnNames = GetReturningColumnNames(returningColumnNamesType);

            return GetFeaturesWithinDistanceOf(targetShape, unitOfData, distanceUnit, distance, returningColumnNames);
        }

        /// <summary>Returns a collection of features that are within a certain distance of a TargetShape.</summary>
        /// <returns>Collection of features that are within a certain distance of a TargetShape</returns>
        /// <remarks> It is important to note that the TargetShape and the
        ///     FeatureSource must use the same unit, such as feet or meters. If there is a current transaction and
        ///     it is marked as live, then the results will include any transaction Feature that
        ///     applies.</remarks>
        /// <param name="targetShape">Shape to find features within a distance of.</param>
        /// <param name="unitOfData">Unit of data that the FeatureSource and TargetShape are in.</param>
        /// <param name="distanceUnit">Unit of the distance parameter, such as feet, miles, kilometers, etc.</param>
        /// <param name="distance">Distance</param>
        /// <param name="returningColumnNames">Column name.</param>
        public Collection<Feature> GetFeaturesWithinDistanceOf(Feature targetFeature, GeographyUnit unitOfData, DistanceUnit distanceUnit, double distance, IEnumerable<string> returningColumnNames)
        {
            Validators.CheckFeatureSourceIsOpen(IsOpen);
            Validators.CheckGeographyUnitIsValid(unitOfData, "unitOfData");
            Validators.CheckDistanceUnitIsValid(distanceUnit, "distanceUnit");
            Validators.CheckParameterIsNotNull(returningColumnNames, "returningColumnNames");

            BaseShape targetShape = targetFeature.GetShape();

            return GetFeaturesWithinDistanceOf(targetShape, unitOfData, distanceUnit, distance, returningColumnNames);
        }

        /// <summary>Returns a collection of features that are within a certain distance of a TargetShape.</summary>
        /// <returns>Collection of features that are within a certain distance of a TargetShape</returns>
        /// <remarks> It is important to note that the TargetShape and the
        ///     FeatureSource must use the same unit, such as feet or meters. If there is a current transaction and
        ///     it is marked as live, then the results will include any transaction Feature that
        ///     applies.</remarks>
        /// <param name="targetShape">Shape to find features within a distance of.</param>
        /// <param name="unitOfData">Unit of data that the FeatureSource and TargetShape are in.</param>
        /// <param name="distanceUnit">Unit of the distance parameter, such as feet, miles, kilometers, etc.</param>
        /// <param name="distance">Distance</param>
        /// <param name="returningColumnNamesType">Column type.</param>
        public Collection<Feature> GetFeaturesWithinDistanceOf(Feature targetFeature, GeographyUnit unitOfData, DistanceUnit distanceUnit, double distance, ReturningColumnsType returningColumnNamesType)
        {
            Validators.CheckFeatureSourceIsOpen(IsOpen);
            Validators.CheckReturningColumnNamesTypeIsValid(returningColumnNamesType, "returningColumnNamesType");

            Collection<string> returningColumnNames = GetReturningColumnNames(returningColumnNamesType);

            return GetFeaturesWithinDistanceOf(targetFeature, unitOfData, distanceUnit, distance, returningColumnNames);
        }

        /// <summary>Returns a collection of features that are within a certain distance of a TargetShape.</summary>
        /// <returns>Collection of features that are within a certain distance of a TargetShape</returns>
        /// <remarks> It is important to note that the TargetShape and the
        ///     FeatureSource must use the same unit, such as feet or meters. If there is a current transaction and
        ///     it is marked as live, then the results will include any transaction Feature that
        ///     applies.</remarks>
        /// <param name="targetShape">Shape to find features within a distance of.</param>
        /// <param name="unitOfData">Unit of data that the FeatureSource and TargetShape are in.</param>
        /// <param name="distanceUnit">Unit of the distance parameter, such as feet, miles, kilometers, etc.</param>
        /// <param name="distance">Distance</param>
        /// <param name="returningColumnNames">Column name.</param>
        protected virtual Collection<Feature> GetFeaturesWithinDistanceOfCore(BaseShape targetShape, GeographyUnit unitOfData, DistanceUnit distanceUnit, double distance, IEnumerable<string> returningColumnNames)
        {
            Validators.CheckFeatureSourceIsOpen(IsOpen);
            Validators.CheckParameterIsNotNull(targetShape, "targetShape");
            Validators.CheckParameterIsValid(targetShape, "targetShape");
            Validators.CheckGeographyUnitIsValid(unitOfData, "unitOfData");
            Validators.CheckDistanceUnitIsValid(distanceUnit, "distanceUnit");
            Validators.CheckParameterIsNotNull(returningColumnNames, "returningColumnNames");

            RectangleShape boundingBox = targetShape.GetBoundingBox();
            if (Math.Abs(boundingBox.Width) < 1e-6)
            {
                double centerX = boundingBox.GetCenterPoint().X;
                boundingBox.UpperLeftPoint.X = centerX - 1e-6;
                boundingBox.LowerRightPoint.X = centerX + 1e-6;
            }
            if (Math.Abs(boundingBox.Height) < 1e-6)
            {
                double centerY = boundingBox.GetCenterPoint().Y;
                boundingBox.UpperLeftPoint.Y = centerY + 1e-6;
                boundingBox.LowerRightPoint.Y = centerY - 1e-6;
            }

            MultipolygonShape multiPlygonShape = boundingBox.Buffer(distance, unitOfData, distanceUnit);
            RectangleShape bufferedBoundingBox = multiPlygonShape.GetBoundingBox();
            Collection<Feature> allPossibleResultsInDatabase = GetFeaturesInsideBoundingBoxCore(bufferedBoundingBox, returningColumnNames);

            Collection<Feature> featuesWithinDistance = new Collection<Feature>();
            BaseShape baseShapeInFeature = null;
            foreach (Feature feature in allPossibleResultsInDatabase)
            {
                baseShapeInFeature = feature.GetShape();
                double currentDistance = baseShapeInFeature.GetDistanceTo(targetShape, unitOfData, distanceUnit);
                if (currentDistance < distance)
                {
                    featuesWithinDistance.Add(feature);
                }
            }
            return featuesWithinDistance;
        }

        /// <summary>Returns an feature based on an Id provided.</summary>
        /// <returns>Feature based on an Id provided..</returns>
        /// <param name="id">Unique Id for the feature to find.</param>
        /// <param name="returningColumnNames">Column names returned with the Feature.</param>
       public Feature GetFeatureById(string id, IEnumerable<string> returningColumnNames)
        {
            Validators.CheckFeatureSourceIsOpen(IsOpen);
            Validators.CheckParameterIsNotNull(returningColumnNames, "returningColumnNames");
            Validators.CheckParameterIsNotNull(id, "id");

            string[] ids = { id };

			Collection<string> uniqColumnNames = ColumnFilter.SplitColumnNames(returningColumnNames);
            Collection<string> fieldNamesInsideSource = GetColumnNamesInsideFeatureSource(uniqColumnNames);
            Collection<string> fieldNamesOutsideOfSource = GetColumnNamesOutsideFeatureSource(uniqColumnNames);

            Collection<Feature> features = GetFeaturesByIdsCore(ids, fieldNamesInsideSource);

            features = RaiseCustomColumnFetchEvent(features, fieldNamesOutsideOfSource);

			if (ColumnFilter.HasComplicateFields(returningColumnNames))
            {
				features = ColumnFilter.ReplaceColumnValues(features, returningColumnNames);
            }

            Collection<Feature> returnFeatures = new Collection<Feature>(features);
            if (IsInTransaction && IsTransactionLive)
            {
                returnFeatures = ColumnFilter.CommitTransactionBufferOnFeatures(features, _transactionBuffer, ids);
            }

            if (Projection != null)
            {
                returnFeatures = ConvertToExternalProjection(returnFeatures);
            }

            RemoveEmptyAndExcludeFeatures(returnFeatures);

            if (returnFeatures.Count == 0)
            {
                returnFeatures.Add(new Feature());
            }

            return returnFeatures[0];
        }

       /// <summary>Returns an feature based on an Id provided.</summary>
       /// <returns>Feature based on an Id provided..</returns>
       /// <param name="id">Unique Id for the feature to find.</param>
       /// <param name="returningColumnNamesType">Column type..</param>
       public Feature GetFeatureById(string id, ReturningColumnsType returningColumnNamesType)
        {
            Validators.CheckFeatureSourceIsOpen(IsOpen);
            Validators.CheckReturningColumnNamesTypeIsValid(returningColumnNamesType, "returningColumnNamesType");

            Collection<string> returningColumnNames = GetReturningColumnNames(returningColumnNamesType);

            return GetFeatureById(id, returningColumnNames);
        }

       /// <summary>Returns a collection of features based on Ids provided.</summary>
       /// <returns>Collection of features based on Ids provided.</returns>
       /// <param name="ids">Unique Ids for the features to find.</param>
       /// <param name="returningColumnNames">Column names returned with the Features.</param>
        public Collection<Feature> GetFeaturesByIds(IEnumerable<string> ids, IEnumerable<string> returningColumnNames)
        {
            Validators.CheckFeatureSourceIsOpen(IsOpen);
            Validators.CheckParameterIsNotNull(returningColumnNames, "returningColumnNames");
            Validators.CheckParameterIsNotNull(ids, "ids");

			Collection<string> uniqColumnNames = ColumnFilter.SplitColumnNames(returningColumnNames);
            Collection<string> fieldNamesInsideSource = GetColumnNamesInsideFeatureSource(uniqColumnNames);
            Collection<string> fieldNamesOutsideOfSource = GetColumnNamesOutsideFeatureSource(uniqColumnNames);

            Collection<Feature> features = GetFeaturesByIdsCore(ids, fieldNamesInsideSource);

            features = RaiseCustomColumnFetchEvent(features, fieldNamesOutsideOfSource);

			if (ColumnFilter.HasComplicateFields(returningColumnNames))
            {
				features = ColumnFilter.ReplaceColumnValues(features, returningColumnNames);
            }

            Collection<Feature> returnFeatures = new Collection<Feature>(features);
            if (IsInTransaction && IsTransactionLive)
            {
				returnFeatures = ColumnFilter.CommitTransactionBufferOnFeatures(features, _transactionBuffer, ids);
            }

            for (int i = 0; i < returnFeatures.Count; i++)
            {
                foreach (string columnName in fieldNamesOutsideOfSource)
                {
                    ColumnFetchEventArgs e = new ColumnFetchEventArgs(columnName, returnFeatures[i].Id);
                    OnCustomColumnFetch(e);
                    if (!returnFeatures[i].ColumnValues.ContainsKey(e.ColumnName))
                    {
                        returnFeatures[i].ColumnValues.Add(columnName, e.ColumnValue);
                    }
                }
            }

            if (Projection != null)
            {
                returnFeatures = ConvertToExternalProjection(returnFeatures);
            }

            RemoveEmptyAndExcludeFeatures(returnFeatures);

            return returnFeatures;
        }

        /// <summary>Returns a collection of features based on Ids provided.</summary>
        /// <returns>Collection of features based on Ids provided.</returns>
        /// <param name="ids">Unique Ids for the features to find.</param>
        /// <param name="returningColumnNamesType">Column type.</param>
        public Collection<Feature> GetFeaturesByIds(IEnumerable<string> ids, ReturningColumnsType returningColumnNamesType)
        {
            Validators.CheckFeatureSourceIsOpen(IsOpen);
            Validators.CheckReturningColumnNamesTypeIsValid(returningColumnNamesType, "returningColumnNamesType");

            Collection<string> returningColumnNames = GetReturningColumnNames(returningColumnNamesType);

            return GetFeaturesByIds(ids, returningColumnNames);
        }

        /// <summary>Returns the bounding box for a feature with Id specified.</summary>
        /// <returns>Bounding box for a feature with Id specified.</returns>
        /// <param name="id">Unique Id of the feature to find the bounding box.</param>
        public RectangleShape GetBoundingBoxById(string id)
        {
            Validators.CheckFeatureSourceIsOpen(IsOpen);
            Validators.CheckParameterIsNotNull(id, "id");

            Collection<Feature> features = GetFeaturesByIdsCore(new[] { id }, new List<string>());

            RectangleShape rectangle = null;

            if (features.Count > 0)
            {
                rectangle = features[0].GetBoundingBox();
                if (Projection != null)
                {
                    rectangle = ConvertToExternalProjection(rectangle);
                }
            }

            return rectangle;
        }

        /// <summary>Returns a bounding box for the features with Id specified.</summary>
        /// <returns>Bounding box for the features with Id specified.</returns>
        /// <param name="ids">Collection of unique Ids of the features to find the bounding box.</param>
        public RectangleShape GetBoundingBoxByIds(IEnumerable<string> ids)
        {
            Validators.CheckFeatureSourceIsOpen(IsOpen);
            Validators.CheckIEnumerableIsEmptyOrNull(ids);

            RectangleShape resultRectangleShape = null;
            foreach (string id in ids)
            {
                RectangleShape currentRectangleShape = GetBoundingBoxById(id);
                if (resultRectangleShape == null)
                {
                    resultRectangleShape = currentRectangleShape;
                }
                else
                {
                    resultRectangleShape.ExpandToInclude(currentRectangleShape);
                }
            }
            return resultRectangleShape;
        }

        /// <summary>Returns a collection of bounding boxes for the features with Id specified.</summary>
        /// <returns>Collection of bounding boxes for the features with Id specified.</returns>
        /// <param name="ids">Collection of unique Ids of the features to find the bounding box.</param>
        public Collection<RectangleShape> GetBoundingBoxesByIds(IEnumerable<string> ids)
        {
            Validators.CheckFeatureSourceIsOpen(IsOpen);
            Validators.CheckParameterIsNotNull(ids, "ids");

            Collection<RectangleShape> rectangles = new Collection<RectangleShape>();

            RectangleShape rectangle = null;
            foreach (string id in ids)
            {
                rectangle = GetBoundingBoxById(id);
                rectangles.Add(rectangle);
            }

            return rectangles;
        }

        /// <summary>Returns the well known type representing the first feature of the FeatureSource.</summary>
        /// <returns>Well known type representing the first feature of the FeatureSource.</returns>
        /// <remarks>Concrete wrapper for the virtual method GetFirstFeaturesWellKnownTypeCore.<br/>
        /// 	<br/>
        /// The default implementation of GetFirstFeaturesWellKnownTypeCore uses the GetAllFeaturesCore method to
        /// get WellKnownType of the first feature from all features. It is recommended to implement one's own more efficient method.<br/>
        /// 	<br/>
        /// As a concrete public method that wraps a Core method, Mapgenix reserves the right
        /// to add events and other logic returned by the Core version
        /// of the method.</remarks>
        public WellKnownType GetFirstFeaturesWellKnownType()
        {
            Validators.CheckFeatureSourceIsOpen(IsOpen);

            return GetFirstFeaturesWellKnownTypeCore();
        }


        /// <summary>Returns the well known type representing the first feature of the FeatureSource.</summary>
        /// <returns>Well known type representing the first feature of the FeatureSource.</returns>
        /// <remarks> Protected virtual method called from the concrete public method
        ///     GetFirstFeaturesWellKnownType.<br/>
        /// 	<br/>
        /// The default implementation of GetFirstFeaturesWellKnownTypeCore uses the GetAllFeaturesCore method to
        /// get WellKnownType of the first feature from all features. It is recommended to implement one's own more efficient method.<br/>
        ///</remarks>
        protected virtual WellKnownType GetFirstFeaturesWellKnownTypeCore()
        {
            Validators.CheckFeatureSourceIsOpen(IsOpen);

            WellKnownType featureType = WellKnownType.Invalid;

            Collection<Feature> allFeatures = GetAllFeatures(ReturningColumnsType.NoColumns);
            if (allFeatures.Count > 0)
            {
                featureType = allFeatures[0].GetWellKnownType();
            }

            return featureType;
        }

        /// <summary>Returns a collection of features from Ids.</summary>
        /// <returns>Collection of features from Ids.</returns>
        /// <remarks>The internal implementation calls the GetAllFeaturesCore. It is recommended to override
        /// GetFeaturesByIdsCore method for more efficient method.</remarks>
        /// <param name="ids">Ids which uniquely identify features in the FeatureSource.</param>
        /// <param name="returningColumnNames">Column names</param>
        protected virtual Collection<Feature> GetFeaturesByIdsCore(IEnumerable<string> ids, IEnumerable<string> returningColumnNames)
        {
            Validators.CheckFeatureSourceIsOpen(IsOpen);
            Validators.CheckParameterIsNotNull(ids, "ids");
            Validators.CheckParameterIsNotNull(returningColumnNames, "returningColumnNames");

            Collection<Feature> allFeatures = GetAllFeaturesCore(returningColumnNames);

			Dictionary<string, Feature> allFeaturesIds = ColumnFilter.GetFeaturesDictionaryFromCollecion(allFeatures);
            Collection<Feature> returnFeatures = new Collection<Feature>();
            foreach (string id in ids)
            {
                if (allFeaturesIds.ContainsKey(id))
                {
                    returnFeatures.Add(allFeaturesIds[id]);
                }
            }
            return returnFeatures;
        }

        
        protected Collection<RectangleShape> ConvertToExternalProjection(IEnumerable<RectangleShape> rectangles)
        {
            Validators.CheckParameterIsNotNull(rectangles, "rectangles");

            Collection<RectangleShape> returnRectangles = new Collection<RectangleShape>();
            RectangleShape newRectangle = null;
            foreach (RectangleShape rectangle in rectangles)
            {
                newRectangle = ConvertToExternalProjection(rectangle);
                returnRectangles.Add(newRectangle);
            }

            return returnRectangles;
        }

        /// <summary>Projects rectangles based on the internal Projection of BaseFeatureSource.</summary>
        /// <overloads>Projects rectangles based on the internal Projection of BaseFeatureSource.</overloads>
        /// <returns>Rectangles based on the internal Projection of BaseFeatureSource.</returns>
        /// <remarks>Protected method to help developers to implement or extend one of our BaseFeatureSources.</remarks>
        /// <param name="rectangles">Rectangles to project.</param>
       protected Collection<RectangleShape> ConvertToInternalProjection(IEnumerable<RectangleShape> rectangles)
        {
            Validators.CheckParameterIsNotNull(rectangles, "rectangles");

            Collection<RectangleShape> returnRectangles = new Collection<RectangleShape>();
            RectangleShape newRectangle = null;
            foreach (RectangleShape rectangle in rectangles)
            {
                newRectangle = ConvertToInternalProjection(rectangle);
                returnRectangles.Add(newRectangle);
            }

            return returnRectangles;
        }

       /// <summary>Projects rectangles based on the external Projection of BaseFeatureSource.</summary>
       /// <overloads>Projects rectangles based on the external Projection of BaseFeatureSource.</overloads>
       /// <returns>Rectangles based on the external Projection of BaseFeatureSource.</returns>
       /// <remarks>Protected method to help developers to implement or extend one of our BaseFeatureSources.</remarks>
       /// <param name="rectangles">Rectangles to project.</param>
        protected RectangleShape ConvertToExternalProjection(RectangleShape rectangle)
        {
            RectangleShape newRectangle = rectangle;

            if (_projection != null)
            {
                newRectangle = _projection.ConvertToExternalProjection(rectangle);
            }

            return newRectangle;
        }

        /// <summary>Projects a rectangle based on the internal Projection of BaseFeatureSource.</summary>
        /// <overloads>Projects a rectangle based on the internal Projection of BaseFeatureSource.</overloads>
        /// <returns>Rectangle based on the internal Projection of BaseFeatureSource.</returns>
        /// <remarks>Protected method to help developers to implement or extend one of our BaseFeatureSources.</remarks>
        /// <param name="rectangle">Rectangle to project.</param>
        protected virtual RectangleShape ConvertToInternalProjection(RectangleShape rectangle)
        {
            RectangleShape newRectangle = rectangle;

            if (_projection != null)
            {
				MultipointShape multiPoint = ColumnFilter.GetGridFromRectangle(rectangle, 20);
                multiPoint = (MultipointShape)_projection.ConvertToInternalProjection(multiPoint);
                newRectangle = multiPoint.GetBoundingBox();
            }

            return newRectangle;
        }

        /// <summary>Projects BaseShape based on the internal Projection of BaseFeatureSource.</summary>
        /// <overloads>Projects BaseShape based on the internal Projection of BaseFeatureSource.</overloads>
        /// <returns>BaseShape based on the internal Projection of BaseFeatureSource.</returns>
        /// <remarks>Protected method to help developers to implement or extend one of our BaseFeatureSources.</remarks>
        /// <param name="baseShape">BaseShape to project.</param>
        protected BaseShape ConvertToInternalProjection(BaseShape baseShape)
        {
            BaseShape newBaseShape = baseShape;
            if (_projection != null)
            {
                newBaseShape = _projection.ConvertToInternalProjection(baseShape);

            }

            return newBaseShape;
        }

        /// <summary>Projects a collection of features based on the external Projection of BaseFeatureSource.</summary>
        /// <overloads>Projects a collection of features based on the external Projection of BaseFeatureSource.</overloads>
        /// <returns>Collection of features based on the external Projection of BaseFeatureSource.</returns>
        /// <remarks>Protected method to help developers to implement or extend one of our BaseFeatureSources.</remarks>
        /// <param name="features">Collection of features to project.</param>
        protected Collection<Feature> ConvertToExternalProjection(IEnumerable<Feature> features)
        {
            Validators.CheckParameterIsNotNull(features, "features");

            Collection<Feature> returnFeatures = new Collection<Feature>();

            foreach (Feature feature in features)
            {
                if (feature.GetWellKnownBinary().Length != 0)
                {
                    returnFeatures.Add(ConvertToExternalProjection(feature));
                }
                else
                {
                    returnFeatures.Add(feature);
                }
            }

            return returnFeatures;
        }

        /// <summary>Projects a collection of features based on the internal Projection of BaseFeatureSource.</summary>
        /// <overloads>Projects a collection of features based on the internal Projection of BaseFeatureSource.</overloads>
        /// <returns>Collection of features based on the internal Projection of BaseFeatureSource.</returns>
        /// <remarks>Protected method to help developers to implement or extend one of our BaseFeatureSources.</remarks>
        /// <param name="features">Collection of features to project.</param>
       protected Collection<Feature> ConvertToInternalProjection(IEnumerable<Feature> features)
        {
            Validators.CheckParameterIsNotNull(features, "features");

            Collection<Feature> returnFeatures = new Collection<Feature>();

            foreach (Feature feature in features)
            {
                returnFeatures.Add(ConvertToInternalProjection(feature));
            }

            return returnFeatures;
        }

       /// <summary>Projects a feature based on the external Projection of BaseFeatureSource.</summary>
       /// <overloads>Projects a feature based on the external Projection of BaseFeatureSource.</overloads>
       /// <returns>Feature based on the external Projection of BaseFeatureSource.</returns>
       /// <remarks>Protected method to help developers to implement or extend one of our BaseFeatureSources.</remarks>
       /// <param name="feature">Feature to project.</param>
       protected Feature ConvertToExternalProjection(Feature feature)
        {
            Feature newFeature = feature;

            if (_projection != null)
            {
                newFeature = _projection.ConvertToExternalProjection(feature);
            }

            return newFeature;
        }

       /// <summary>Projects a feature based on the internal Projection of BaseFeatureSource.</summary>
       /// <overloads>Projects a feature based on the internal Projection of BaseFeatureSource.</overloads>
       /// <returns>Feature based on the internal Projection of BaseFeatureSource.</returns>
       /// <remarks>Protected method to help developers to implement or extend one of our BaseFeatureSources.</remarks>
       /// <param name="feature">Feature to project.</param>
       protected Feature ConvertToInternalProjection(Feature feature)
        {
            if (_projection != null)
            {
                _projection.UpdateToInternalProjection(feature);
            }

            return feature;
        }

       /// <summary>Returns column names in BaseFeatureSource from a list of field names.</summary>
       /// <returns>Column names in BaseFeatureSource from a list of field names</returns>
       /// <remarks>Protected method to help developers to implement or extend one of our BaseFeatureSources.</remarks>
       /// <param name="returningColumnNames">Column names.</param>
        protected Collection<string> GetColumnNamesInsideFeatureSource(IEnumerable<string> returningColumnNames)
        {
            Validators.CheckParameterIsNotNull(returningColumnNames, "returningColumnNames");

            Collection<string> fieldNamesInFeatureSource = new Collection<string>();

            if (new List<string>(returningColumnNames).Count != 0)
            {
                Collection<FeatureSourceColumn> columns = GetColumns();
                Dictionary<string, int> allFieldNamesInFeatureSource = new Dictionary<string, int>(StringComparer.CurrentCultureIgnoreCase);

                for (int i = 0; i < columns.Count; i++)
                {
					string newColumnName = ColumnFilter.GetColumnNameAlias(columns[i].ColumnName, allFieldNamesInFeatureSource.Keys);
                    allFieldNamesInFeatureSource.Add(newColumnName, i);
                }

                foreach (string columnName in returningColumnNames)
                {
                    if (allFieldNamesInFeatureSource.ContainsKey(columnName))
                    {
                        fieldNamesInFeatureSource.Add(columnName);
                    }
                }
            }

            return fieldNamesInFeatureSource;
        }


        /// <summary>Returns column names outside of BaseFeatureSource from a list of field names.</summary>
        /// <returns>Column names outside of BaseFeatureSource from a list of field names</returns>
        /// <remarks>Protected method to help developers to implement or extend one of our BaseFeatureSources.</remarks>
        /// <param name="returningColumnNames">Column names.</param>
        protected Collection<string> GetColumnNamesOutsideFeatureSource(IEnumerable<string> returningColumnNames)
        {
            Validators.CheckParameterIsNotNull(returningColumnNames, "returningColumnNames");

            Collection<string> fieldNamesOutsideFeatureSource = new Collection<string>();

            if (new List<string>(returningColumnNames).Count != 0)
            {
                Collection<FeatureSourceColumn> columns = GetColumns();
                Dictionary<string, int> allFieldNamesInFeatureSource = new Dictionary<string, int>(StringComparer.CurrentCultureIgnoreCase);

                for (int i = 0; i < columns.Count; i++)
                {
					string newColumnName = ColumnFilter.GetColumnNameAlias(columns[i].ColumnName, allFieldNamesInFeatureSource.Keys);
                    allFieldNamesInFeatureSource.Add(newColumnName, i);
                }

                foreach (string columnName in returningColumnNames)
                {
                    if (!allFieldNamesInFeatureSource.ContainsKey(columnName))
                    {
                        fieldNamesOutsideFeatureSource.Add(columnName);
                    }
                }
            }

            return fieldNamesOutsideFeatureSource;
        }

        /// <summary>Returns columnNames based on returningColumnType.</summary>
        /// <param name="returningColumnNamesType">ReturningColumnType.</param>
        /// <returns>ColumnNames based on the given returningColumnNamesType.</returns>
        ///<remarks>Concrete sub class from BaseFeatureSource can override this logic.</remarks>
        protected Collection<string> GetReturningColumnNames(ReturningColumnsType returningColumnNamesType)
        {
            Collection<string> columnNames = new Collection<string>();

            switch (returningColumnNamesType)
            {
                case ReturningColumnsType.NoColumns:
                    break;
                case ReturningColumnsType.AllColumns:

                    Collection<FeatureSourceColumn> columns = GetColumns();

                    foreach (FeatureSourceColumn column in columns)
                    {
						string newColumnName = ColumnFilter.GetColumnNameAlias(column.ColumnName, columnNames);
                        columnNames.Add(newColumnName);
                    }

                    break;
                default:
                    break;
            }

            return columnNames;
        }


		private void RemoveEmptyAndExcludeFeatures(Collection<Feature> sourceFeatures)
		{

			Collection<int> features = new Collection<int>();
			for (int i = 0; i < sourceFeatures.Count; i++)
			{
				if (sourceFeatures[i].GetWellKnownBinary().Length == 0 || _featureIdsToExclude.Contains(sourceFeatures[i].Id))
				{
					features.Add(i);
				}
			}

			for (int i = features.Count - 1; i >= 0; i--)
			{
				sourceFeatures.RemoveAt(features[i]);
			}
		}

      

        protected virtual void OnDrawingProgressChanged(ProgressEventArgs e)
        {
            _progressDrawingRaisedCount++;
            if (_progressDrawingRaisedCount == ProgressDrawingRaisingFrenquency)
            {
                _progressDrawingRaisedCount = 0;
                EventHandler<ProgressEventArgs> handler = DrawingProgressChanged;
                if (handler != null)
                {
                    handler(this, e);
                }
            }
        }

        public Collection<Feature> GetFeaturesByBoundingBox(RectangleShape boundingBox, IEnumerable<string> returningColumnNames)
        {
            return GetFeaturesInsideBoundingBoxCore(boundingBox, returningColumnNames);
        }
        
    }
}

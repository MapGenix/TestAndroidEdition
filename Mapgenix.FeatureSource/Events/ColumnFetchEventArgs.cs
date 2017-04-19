using System;

namespace Mapgenix.FeatureSource
{
    /// <summary>Event parameters passed in through the CustomFieldFetch event.</summary>
    /// <remarks>
    /// CustomColumnFetch Event Background<br/>
    /// 	<br/>
    /// Used primarily when you have data relating to a particular feature or set of
    /// features that is not within source of the data.For example, for a shapefile
    /// you may want to link to an outside SQL Server table.<br/>
    /// 	<br/>
    /// To integrate this SQL data, you simply create a column name that does not exist in the
    /// .dbf file of the shapefile.  Whenever GSuite is queried to return records that specifically require
    /// this column (field), the FeatureSource raises the event to supply the data. This way the SQL table is queried and the
    /// data is stored in a collection.<br/>
    /// 	<br/>
    /// Being an event, it gets raised for each feature meaning the event can be raised a lot of times.
    /// That is why we recommend caching the data.</remarks>
    [Serializable]
    public class ColumnFetchEventArgs : EventArgs
    {
        private readonly string columnName;
        private readonly string id;
        private string columnValue;

        /// <summary>To create the event arguments.</summary>
        /// <returns>None</returns>
        /// <remarks>None</remarks>
        /// <param name="columnName">Column name.</param>
        /// <param name="id">Id of the feature.</param>
        public ColumnFetchEventArgs(string columnName, string id)
            
        {
            this.columnName = columnName;
            this.id = id;
            this.columnValue = string.Empty;
        }

        /// <summary>Returns the column name needed to return data.</summary>
        /// <returns>Column name needed to return data.</returns>
        /// <remarks>
        /// 	<para>Look up the Id in your external data source and find the column of the data.<br/>
        /// 		<br/>
        /// 		<br/>
        /// CustomColumnFetch Event Background<br/>
        /// 	<br/>
        /// Used primarily when you have data relating to a particular feature or set of
        /// features that is not within source of the data.For example, for a shapefile
        /// you may want to link to an outside SQL Server table.<br/>
        /// 	<br/>
        /// To integrate this SQL data, you simply create a column name that does not exist in the
        /// .dbf file of the shapefile.  Whenever GSuite is queried to return records that specifically require
        /// this column (field), the FeatureSource raises the event to supply the data. This way the SQL table is queried and the
        /// data is stored in a collection.<br/>
        /// 	<br/>
        /// Being an event, it gets raised for each feature meaning the event can be raised a lot of times.
        /// That is why we recommend caching the data.</remarks>
        /// </remarks>
        public string ColumnName
        {
            get { return columnName; }
        }

        /// <summary>Returns the Id needed to return data.</summary>
        /// <returns>Id needed to return data.</returns>
        /// <remarks>Look up the Id in your external data source and find the field of data.<br/>
        /// 	<br/>
        /// CustomColumnFetch Event Background<br/>
        /// 	<br/>
        /// Used primarily when you have data relating to a particular feature or set of
        /// features that is not within source of the data.For example, for a shapefile
        /// you may want to link to an outside SQL Server table.<br/>
        /// 	<br/>
        /// To integrate this SQL data, you simply create a column name that does not exist in the
        /// .dbf file of the shapefile.  Whenever GSuite is queried to return records that specifically require
        /// this column (field), the FeatureSource raises the event to supply the data. This way the SQL table is queried and the
        /// data is stored in a collection.<br/>
        /// 	<br/>
        /// Being an event, it gets raised for each feature meaning the event can be raised a lot of times.
        /// That is why we recommend caching the data.
        /// </remarks>
        public string Id
        {
            get { return this.id; }
        }

        /// <summary>Returns the field's value. To be set in the event.</summary>
        /// <returns>Field's value. To be set in the event.</returns>
        /// <remarks>CustomColumnFetch Event Background<br/>
        /// 	<br/>
        /// Used primarily when you have data relating to a particular feature or set of
        /// features that is not within source of the data.For example, for a shapefile
        /// you may want to link to an outside SQL Server table.<br/>
        /// 	<br/>
        /// To integrate this SQL data, you simply create a column name that does not exist in the
        /// .dbf file of the shapefile.  Whenever GSuite is queried to return records that specifically require
        /// this column (field), the FeatureSource raises the event to supply the data. This way the SQL table is queried and the
        /// data is stored in a collection.<br/>
        /// 	<br/>
        /// Being an event, it gets raised for each feature meaning the event can be raised a lot of times.
        /// That is why we recommend caching the data.
        /// </remarks>
        public string ColumnValue
        {
            get { return this.columnValue; }
            set { this.columnValue = value; }
        }

    }
}

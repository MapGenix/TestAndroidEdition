using System;

namespace Mapgenix.FeatureSource
{
    public class PostGisEvent
        : EventArgs
    {
        public PostGisEvent(string sqlStatement)
            : this(sqlStatement, ExecutingSqlStatementType.Unknown)
        {
            SqlStatement = sqlStatement;
        }

        public PostGisEvent(string sqlStatement, ExecutingSqlStatementType sqlStatementType)
            : base()
        {
            SqlStatement = sqlStatement;
            ExcutingSqlStatementType = sqlStatementType;
        }

        /// <summary>
        /// 
        /// </summary>
        public string SqlStatement { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public ExecutingSqlStatementType ExcutingSqlStatementType { get; set; }
    }
}

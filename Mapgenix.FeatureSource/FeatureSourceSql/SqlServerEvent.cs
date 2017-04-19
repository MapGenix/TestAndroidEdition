using System;

namespace Mapgenix.FeatureSource
{
    /// <summary>Parameters passed in through the ExecutingSqlStatement event in MsSql2008FeatureSource.</summary>
    /// <remarks>None</remarks>
    [Serializable]
    public class SqlServerEvent
        : EventArgs
    {
        //private string sqlStatement;
        //private ExecutingSqlStatementType executingStatementType;

        /// <summary>Constructor for the class.</summary>
        /// <returns>None</returns>
        /// <remarks>None</remarks>
        /// <param name="sqlStatement">SqlStatement passed in to execute.</param>
        public SqlServerEvent(string sqlStatement)
            : this(sqlStatement, ExecutingSqlStatementType.Unknown)
        {
           SqlStatement = sqlStatement;
        }

        /// <summary>Constructor for the class.</summary>
        /// <returns>None</returns>
        /// <remarks>None</remarks>
        /// <param name="sqlStatement">SqlStatement passed in to execute.</param>
        /// <param name="sqlStatementType">SqlStatementType of the sqlStatement to execute.</param>
        public SqlServerEvent(string sqlStatement,ExecutingSqlStatementType sqlStatementType)
            : base()
        {
            SqlStatement = sqlStatement;
            ExecutingSqlStatementType = sqlStatementType;
        }

        /// <summary>Gets and sets the SqlStatement to execute.</summary>
        /// <remarks>None</remarks>
        public string SqlStatement{ get; set; }

        /// <summary>Gets and sets the SqlStatementType of the SqlStatement to execute.</summary>
        /// <remarks>None</remarks>
        public ExecutingSqlStatementType ExecutingSqlStatementType{ get; set; }
    }
}

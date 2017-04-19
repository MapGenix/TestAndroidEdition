using System;

namespace Mapgenix.FeatureSource
{

    [Serializable]
    public class SqlEventArgs : EventArgs
    {
        private string sqlStatement;
        private ExecutingSqlStatementType executingStatementType;

       
        public SqlEventArgs(string sqlStatement)
            : this(sqlStatement, ExecutingSqlStatementType.Unknown)
        {
            this.sqlStatement = sqlStatement;
        }

       
        public SqlEventArgs(string sqlStatement, ExecutingSqlStatementType sqlStatementType)
        {
            this.sqlStatement = sqlStatement;
            this.executingStatementType = sqlStatementType;
        }

        public string SqlStatement
        {
            get { return sqlStatement; }
            set { sqlStatement = value; }
        }

        public ExecutingSqlStatementType ExecutingSqlStatementType
        {
            get { return executingStatementType; }
            set { executingStatementType = value; }
        }
    }
}

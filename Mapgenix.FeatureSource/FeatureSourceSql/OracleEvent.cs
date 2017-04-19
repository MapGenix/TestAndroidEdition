using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mapgenix.FeatureSource
{
    public class OracleEvent
        : EventArgs
    {
        public OracleEvent(string sqlStatement)
            : this(sqlStatement, ExecutingSqlStatementType.Unknown)
        {
            SqlStatement = sqlStatement;
        }

        public OracleEvent(string sqlStatement, ExecutingSqlStatementType sqlStatementType)
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

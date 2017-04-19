using System;

namespace Mapgenix.FeatureSource
{
    /// <summary>Column information specific to a DBF column.</summary>
    [Serializable]
    public class DbfColumn : FeatureSourceColumn
    {
        string columnName;
        DbfColumnType columnType;
        int length;
        int decimalLength;

        /// <summary>Constructor of DbfColumn.</summary>
        /// <overloads>Default constructor.</overloads>
        /// <returns>None</returns>
        /// <remarks>None</remarks>
        protected DbfColumn()
            : this(string.Empty, DbfColumnType.String, 0, 0)
        {
        }

        /// <summary>Constructor of DbfColumn.</summary>
        /// <returns>None</returns>
        /// <remarks>None</remarks>
        /// <param name="columnName">Name of the column in the DBF.</param>
        /// <param name="columnType">Type of the column in the DBF.</param>
        /// <param name="length">Length of the column in the DBF.</param>
        /// <param name="decimalLength">Number of decimal places of the column if numeric type.</param>
        public DbfColumn(string columnName, DbfColumnType columnType, int length, int decimalLength)
        {
            Validators.CheckColumnTypeAndDecimalLength(columnType, decimalLength);

            this.columnName = columnName;
            this.columnType = columnType;
            this.length = length;
            this.decimalLength = decimalLength;
        }

        /// <summary>Returns the name of the column in the DBF.</summary>
        /// <returns>Name of the column in the DBF.</returns>
        /// <remarks>None</remarks>
        public new string ColumnName
        {
            get
            {
                return columnName;
            }
            set
            {
                columnName = value;
            }
        }

        /// <summary>Returns the type of the column in the DBF.</summary>
        /// <returns>Type of the column in the DBF.</returns>
        /// <remarks>None</remarks>
        public DbfColumnType ColumnType
        {
            get
            {
                return columnType;
            }
            set
            {
                columnType = value;
            }
        }

        /// <summary>Returns the length of the column in the DBF.</summary>
        /// <returns>Length of the column in the DBF.</returns>
        /// <remarks>None</remarks>
        public int Length
        {
            get
            {
                return length;
            }
            set
            {
                length = value;
            }
        }

        /// <summary>Returns the decimal length of the column in the DBF.</summary>
        /// <decimalDegreesValue>Decimal length of the column in the DBF.</decimalDegreesValue>
        /// <remarks>Number of decimal places the number represents if column type is numeric.</remarks>
        public int DecimalLength
        {
            get
            {
                return decimalLength;
            }
            set
            {
                decimalLength = value;
            }
        }
    }
}

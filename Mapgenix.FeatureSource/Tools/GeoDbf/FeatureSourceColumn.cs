using System;

namespace Mapgenix.FeatureSource
{
    /// <summary>Column contained within a FeatureSource.</summary>
    /// <remarks>A FeatureSource is represented by a collection of features with their column data.</remarks>
    [Serializable]
    public class FeatureSourceColumn
    {
        private string columnName;
        private string typeName;
        private int maxLength;

        /// <summary>Constructor to create a FeatureSourceColumn.</summary>
        /// <overloads>Creates a FeatureSourceColumn that is empty.</overloads>
        /// <returns>None</returns>
        public FeatureSourceColumn()
            :this(string.Empty, string.Empty, 0)
        {}

        /// <summary>Constructor to create a FeatureSourceColumn.</summary>
        /// <overloads>Creates a FeatureSourceColumn with the column's name only.</overloads>
        /// <returns>None</returns>
        /// <param name="columnName">Name of the column.</param>
        public FeatureSourceColumn(string columnName)
            : this(columnName, string.Empty, 0)
        {}

        /// <summary>Constructor to create a FeatureSourceColumn.</summary>
        /// <overloads>Creates a FeatureSourceColumn with the column's name, type and max length.</overloads>
        /// <returns>None</returns>
        /// <param name="columnName">Name of the column.</param>
        /// <param name="typeName">Type name of the column.</param>
        /// <param name="maxLength">Maximum length of the column.</param>
        public FeatureSourceColumn(string columnName, string typeName, int maxLength)
        {
            this.columnName = columnName;
            this.typeName = typeName;
            this.maxLength = maxLength;
        }

        /// <summary>Returns the name of the column.</summary>
        /// <decimalDegreesValue>Name of the column.</decimalDegreesValue>
        /// <remarks>None</remarks>
        public string ColumnName
        {
            get
            {
                return columnName;
            }
        }

        /// <summary>Returns the type name of the column.</summary>
        /// <returns>Type name of the column.</returns>
        public string TypeName
        {
            get
            {
                return typeName;
            }
        }

        /// <summary>Returns the maximum length of the column.</summary>
        /// <decimalDegreesValue>Maximum length of the column.</decimalDegreesValue>
       public int MaxLength
        {
            get
            {
                return maxLength;
            }
        }

       /// <summary>Returns column name of FeatureSourceColumn</summary>
       /// <returns>Column name of FeatureSourceColumn</returns>
        public override string ToString()
        {
            return columnName;
        }
    }
}

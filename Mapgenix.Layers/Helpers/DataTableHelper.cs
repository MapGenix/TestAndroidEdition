using Mapgenix.Shapes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;


namespace Mapgenix.Layers
{
    /// <summary>Static class for operations on Data Tables.</summary>
    public static class DataTableHelper
    {
        /// <summary>Gets DataTable of information about a collection of features with the specified
        /// returningColumns.</summary>
        /// <param name="features">Collection of features.</param>
        /// <param name="returningColumnNames">ColumnNames for the features.</param>
        /// <returns>DataTable with information about the features and the returning columnNames.</returns>
        public static DataTable LoadDataTable(Collection<Feature> features, IEnumerable<string> returningColumnNames)
        {
            Validators.CheckParameterIsNotNull(returningColumnNames, "returningColumnNames");

            DataTable dataTable = null;

            if (features != null)
            {
                dataTable = new DataTable {Locale = CultureInfo.InvariantCulture};

                foreach (string columnName in returningColumnNames)
                {
                    dataTable.Columns.Add(columnName);
                }

                foreach (Feature feature in features)
                {
                    DataRow dataRow = dataTable.NewRow();

                    for (int i = 0; i < dataTable.Columns.Count; i++)
                    {
                        if (feature.ColumnValues.ContainsKey(dataTable.Columns[i].ColumnName))
                        {
                            dataRow[i] = feature.ColumnValues[dataTable.Columns[i].ColumnName];
                        }
                        else
                        {
                            dataRow[i] = String.Empty;
                        }
                    }

                    dataTable.Rows.Add(dataRow);
                }
            }

            return dataTable;
        }

    }
}

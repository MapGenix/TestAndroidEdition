using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace Mapgenix.Canvas
{
    internal class CsvFileHelper
    {
        internal static string CsvFileFolder;

        private static Dictionary<string, CsvTable> _csvFileTables;

        internal static string[] CsvScanFileByName(string fileName, string keyFieldName, string value,
            CsvCompareCriteria criteria)
        {
            var targetTable = GetTable(fileName);
            if (targetTable == null || targetTable.DataTable == null)
                return null;
            if (!targetTable.DataTable.Columns.Contains(keyFieldName))
                return null;
            string[] targetRowData = null;
            for (var i = 0; i < targetTable.DataTable.Rows.Count && targetRowData == null; i++)
            {
                var cellValue = (string) targetTable.DataTable.Rows[i][keyFieldName];
                switch (criteria)
                {
                    case CsvCompareCriteria.ApproxString:
                        if (cellValue.Contains(value))
                        {
                            var dataList = new ArrayList(targetTable.DataTable.Rows[i].ItemArray);
                            targetRowData = (string[]) dataList.ToArray(Type.GetType("System.String"));
                            targetTable.CurrentIndex = i;
                        }
                        break;
                    case CsvCompareCriteria.ExactString:
                        if (cellValue.Equals(value, StringComparison.OrdinalIgnoreCase))
                        {
                            var dataList = new ArrayList(targetTable.DataTable.Rows[i].ItemArray);
                            targetRowData = (string[]) dataList.ToArray(Type.GetType("System.String"));
                            targetTable.CurrentIndex = i;
                        }
                        break;
                    case CsvCompareCriteria.Integer:
                        if (cellValue.Equals(value, StringComparison.OrdinalIgnoreCase))
                        {
                            var cellIntValue = 0;
                            if (int.TryParse(cellValue, out cellIntValue))
                            {
                                var dataList = new ArrayList(targetTable.DataTable.Rows[i].ItemArray);
                                targetRowData = (string[]) dataList.ToArray(Type.GetType("System.String"));
                                targetTable.CurrentIndex = i;
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
            return targetRowData;
        }

        internal static int CsvGetFileFieldId(string fileName, string keyFieldName)
        {
            return GetTable(fileName).DataTable.Columns.IndexOf(keyFieldName);
        }

        internal static string GetCsvFilename(string baseName)
        {
            return baseName;
        }

        internal static string CsvGetField(string fileName, string keyFieldName, string keyFieldValue,
            CsvCompareCriteria criteria, string targetField)
        {
            var records = CsvScanFileByName(fileName, keyFieldName, keyFieldValue, criteria);
            if (records == null)
                return string.Empty;

            var targetFieldIndex = GetTable(fileName).DataTable.Columns.IndexOf(targetField);
            if (targetFieldIndex > 0)
            {
                return records[targetFieldIndex];
            }
            return string.Empty;
        }

        internal static string[] CsvGetNextLine(string fileName)
        {
            var targetTable = GetTable(fileName);
            if (targetTable == null || targetTable.DataTable == null)
            {
                return null;
            }
            var targetIndex = targetTable.CurrentIndex + 1;
            if (targetIndex > targetTable.DataTable.Rows.Count - 1)
            {
                return null;
            }
            var itemArrayList = new ArrayList(targetTable.DataTable.Rows[targetIndex].ItemArray);
            return (string[]) itemArrayList.ToArray(Type.GetType("System.String"));
        }

        private static string CsvFileName(string baseName)
        {
            return CsvFileFolder + baseName;
        }

        internal static string[] GetMappingTable(string csvFileName, string[] columnNames)
        {
            var records = new List<string>();
            var fileFullName = CsvFileName(csvFileName);
            if (File.Exists(fileFullName))
            {
                using (var datumStream = new FileStream(fileFullName, FileMode.Open, FileAccess.Read))
                {
                    using (TextReader tr = new StreamReader(datumStream))
                    {
                        var columns = tr.ReadLine();
                        var fullColumnNames =
                            new List<string>(columns.ToUpper()
                                .Split(new[] {',', '"'}, StringSplitOptions.RemoveEmptyEntries));
                        var columnIndexes = new Dictionary<string, int>();
                        foreach (var columnName in columnNames)
                        {
                            if (fullColumnNames.Contains(columnName))
                            {
                                columnIndexes.Add(columnName, fullColumnNames.IndexOf(columnName));
                            }
                            else
                            {
                                return null;
                            }
                        }
                        while (tr.Peek() != -1)
                        {
                            var recordLine = tr.ReadLine();
                            if (!string.IsNullOrEmpty(recordLine))
                            {
                                if (recordLine.StartsWith("Example", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    continue;
                                }
                                while (!recordLine.EndsWith(",") && !recordLine.EndsWith("\""))
                                {
                                    recordLine += tr.ReadLine();
                                }
                                try
                                {
                                    var contents = GetRowContents(recordLine);
                                    foreach (var item in columnIndexes.Values)
                                    {
                                        records.Add(contents[item].Trim('"'));
                                    }
                                }
                                catch
                                {
                                }
                            }
                        }
                    }
                }
            }
            records.AddRange(new string[] {null, null, null});
            var returnValue = records.ToArray();
            return returnValue;
        }

        private static List<string> GetRowContents(string recordLine)
        {
            recordLine += ",";
            var items = new List<string>();
            var contents = recordLine.ToCharArray();
            var isInQuote = false;
            var index = 0;
            var indexForItem = index;
            while (index < contents.Length)
            {
                if (contents[index].Equals('"'))
                {
                    if (index + 2 < contents.Length)
                    {
                        if (contents[index + 1].Equals('"') && contents[index + 2].Equals('"'))
                        {
                            isInQuote = !isInQuote;
                        }
                    }
                    if (!(contents[index + 1].Equals('"') || contents[index - 1].Equals('"')))
                        isInQuote = !isInQuote;
                }
                if (contents[index].Equals(',') && !isInQuote)
                {
                    var itemChars = new char[index - indexForItem];
                    Array.Copy(contents, indexForItem, itemChars, 0, itemChars.Length);
                    items.Add(new string(itemChars));
                    indexForItem = ++index;
                    continue;
                }
                index++;
            }
            return items;
        }

        private static CsvTable GetTable(string fileName)
        {
            if (_csvFileTables == null)
                _csvFileTables = new Dictionary<string, CsvTable>();
            if (_csvFileTables.ContainsKey(fileName) && _csvFileTables[fileName] != null &&
                _csvFileTables[fileName].DataTable != null)
            {
                return _csvFileTables[fileName];
            }
            var fileTable = new CsvTable();
            fileTable.FileFullName = CsvFileName(fileName);
            fileTable.DataTable = LoadCsvFileTable(fileTable.FileFullName);
            fileTable.FileName = fileName;
            fileTable.CurrentIndex = 0;
            if (fileTable.DataTable != null)
            {
                _csvFileTables.Add(fileName, fileTable);
            }
            return fileTable;
        }

        private static DataTable LoadCsvFileTable(string fileName)
        {
            DataTable csvFileTable = null;

            if (File.Exists(fileName))
            {
                csvFileTable = new DataTable();
                using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                {
                    using (TextReader tr = new StreamReader(fs))
                    {
                        var columnNames = tr.ReadLine();
                        var nameList = columnNames.Split(',');

                        foreach (var columnName in nameList)
                        {
                            var col = new DataColumn(columnName.Trim('"'));
                            csvFileTable.Columns.Add(col);
                        }
                        while (tr.Peek() != -1)
                        {
                            var contentRow = tr.ReadLine();
                            var contents = contentRow.Split(',');
                            if (contents.Length != nameList.Length)
                            {
                                contents = GetRowContents(contentRow).ToArray();
                                if (contents.Length != nameList.Length)
                                {
                                    continue;
                                }
                            }
                            else
                            {
                                for (var i = 0; i < contents.Length; i++)
                                {
                                    contents[i] = contents[i].Trim('"');
                                }
                            }
                            csvFileTable.Rows.Add(contents);
                        }
                    }
                }
            }
            return csvFileTable;
        }
    }
}
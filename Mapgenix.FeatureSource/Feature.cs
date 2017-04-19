using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;


namespace Mapgenix.Shapes
{

    [Serializable]
    public struct Feature
    {
        private byte[] wellKnownBinary;
        private string id;
        private object tag;
        private Dictionary<string, string> columnValues;
        private RectangleShape boundingBox;

        public Feature(BaseShape baseShape)
            : this(baseShape.GetWellKnownBinary(), baseShape.Id, (IEnumerable<string>)null, baseShape.Tag)
        { }

       
        public Feature(byte[] wellKnownBinary)
            : this(wellKnownBinary, Guid.NewGuid().ToString(), (IEnumerable<string>)null, null)
        { }

        
        public Feature(byte[] wellKnownBinary, string id)
            : this(wellKnownBinary, id, (IEnumerable<string>)null, null)
        { }

     
        public Feature(string wellKnownText)
            : this(BaseShape.CreateShapeFromWellKnownData(wellKnownText).GetWellKnownBinary(), Guid.NewGuid().ToString(), (IEnumerable<string>)null, null)
        { }

        
        public Feature(string wellKnownText, string id)
            : this(BaseShape.CreateShapeFromWellKnownData(wellKnownText).GetWellKnownBinary(), id, (IEnumerable<string>)null, null)
        { }

        
        public Feature(BaseShape baseShape, IDictionary<string, string> columnValues)
            : this(baseShape.GetWellKnownBinary(), baseShape.Id, columnValues, baseShape.Tag)
        { }

       
        public Feature(string wellKnownText, string id, IDictionary<string, string> columnValues)
            : this(BaseShape.CreateShapeFromWellKnownData(wellKnownText).GetWellKnownBinary(), id, columnValues, null)
        {
        }

        public Feature(string wellKnownText, string id, IEnumerable<string> columnValues)
            : this(BaseShape.CreateShapeFromWellKnownData(wellKnownText).GetWellKnownBinary(), id, columnValues, null)
        {
        }

       
        public Feature(byte[] wellKnownBinary, string id, IEnumerable<string> columnValues)
            : this(wellKnownBinary, id, columnValues, null)
        {
        }

       
        private Feature(byte[] wellKnownBinary, string id, IEnumerable<string> columnValues, object tag)
        {
            this.columnValues = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            if (columnValues != null)
            {
                foreach (string columnValue in columnValues)
                {
                    Validators.CheckFeatureColumnValueContainsColon(columnValue, "columnValues");

                    string key = columnValue.Split(':')[0].Trim();
                    string value = columnValue.Substring(columnValue.IndexOf(':') + 1).Trim();
                    this.columnValues.Add(key, value);
                }
            }

            this.wellKnownBinary = wellKnownBinary;
            this.id = id;
            this.boundingBox = null;
            this.tag = tag;
        }

       
        public Feature(byte[] wellKnownBinary, string id, IDictionary<string, string> columnValues)
            : this(wellKnownBinary, id, columnValues, null)
        {
        }

        private Feature(byte[] wellKnownBinary, string id, IDictionary<string, string> columnValues, object tag)
        {
            this.wellKnownBinary = wellKnownBinary;
            this.id = id;
            if (columnValues == null)
            {
                this.columnValues = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }
            else
            {
                this.columnValues = new Dictionary<string, string>(columnValues, StringComparer.OrdinalIgnoreCase);
            }
            boundingBox = null;
            this.tag = tag;
        }

       
        public Feature(Vertex vertex)
            : this(vertex, Guid.NewGuid().ToString(), null)
        {
        }

       
        public Feature(Vertex vertex, string id)
            : this(vertex, id, null)
        {
        }

       
        public Feature(Vertex vertex, string id, IEnumerable<string> columnValues)
            : this(vertex.X, vertex.Y, id, columnValues)
        {
        }

       
        public Feature(double x, double y)
            : this(x, y, Guid.NewGuid().ToString(), null)
        {
        }

      
        public Feature(double x, double y, string id)
            : this(x, y, id, null)
        {
        }

       
        public Feature(double x, double y, string id, IEnumerable<string> columnValues)
            : this(new PointShape(x, y).GetWellKnownBinary(), id, columnValues)
        {
        }

       
        public byte[] GetWellKnownBinary()
        {
            return wellKnownBinary;
        }

       
        public string Id
        {
            get { return id; }
        }

       
        public object Tag
        {
            get { return tag; }
            set { tag = value; }
        }

       
        public Dictionary<string, string> ColumnValues
        {
            get { return columnValues; }
        }

       
        public WellKnownType GetWellKnownType()
        {
            return WkbHelper.GetWellKnownTypeFromWkb(wellKnownBinary);
        }

      
        public BaseShape GetShape()
        {
            BaseShape baseShape = null;
            if (wellKnownBinary != null)
            {
                baseShape = BaseShape.CreateShapeFromWellKnownData(wellKnownBinary);
                baseShape.Id = id;
                baseShape.Tag = tag;
            }
            return baseShape;
        }

       
        public string GetWellKnownText()
        {
            BaseShape shape = GetShape();
            return shape.GetWellKnownText();
        }

      
        public RectangleShape GetBoundingBox()
        {
            if (boundingBox == null)
            {
                boundingBox = WkbHelper.GetBoundingBoxFromWkb(wellKnownBinary);
            }
            return (RectangleShape)boundingBox.CloneDeep();
        }

      
        public Feature CloneDeep(IEnumerable<string> returningColumnNames)
        {
            if (returningColumnNames == null)
            {
                return new Feature(wellKnownBinary, Id);
            }
            else
            {
                Dictionary<string, string> fields = new Dictionary<string, string>();

                foreach (string columnName in returningColumnNames)
                {
                    if (this.columnValues != null && this.columnValues.ContainsKey(columnName))
                    {
                        fields.Add(columnName, this.columnValues[columnName]);
                    }
                }
                return new Feature(wellKnownBinary, Id, fields);
            }
        }

        public Feature CloneDeep(ReturningColumnsType returningColumnNamesType)
        {
            
            Collection<string> returningColumnNames = new Collection<string>();

            switch (returningColumnNamesType)
            {
                case ReturningColumnsType.NoColumns:
                    break;
                case ReturningColumnsType.AllColumns:
                    foreach (string columnName in columnValues.Keys)
                    {
                        returningColumnNames.Add(columnName);
                    }
                    break;
    
            }

            return CloneDeep(returningColumnNames);
        }

     
        public bool IsValid()
        {
            bool returnValue = false;
            if (wellKnownBinary != null)
            {
                returnValue = true;
            }
            return returnValue;
        }

     
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (obj is Feature)
            {
                return Equals((Feature)obj);
            }
            else
            {
                return false;
            }
        }

        private bool Equals(Feature compareObj)
        {
            if (id != compareObj.id)
            {
                return false;
            }

            if (wellKnownBinary == null)
            {
                if (compareObj.wellKnownBinary != null)
                {
                    return false;
                }
            }
            else
            {
                if (compareObj.wellKnownBinary == null)
                {
                    return false;
                }
                for (int i = 0; i < wellKnownBinary.Length; i++)
                {
                    if (wellKnownBinary[i] != compareObj.wellKnownBinary[i])
                    {
                        return false;
                    }
                }
            }

            if (columnValues != null)
            {
                if (columnValues.Count != compareObj.columnValues.Count)
                {
                    return false;
                }
                foreach (string key in columnValues.Keys)
                {
                    string value = columnValues[key];
                    if (!compareObj.columnValues.ContainsKey(key))
                    {
                        return false;
                    }

                    string compareObjValue = compareObj.columnValues[key];
                    if (value != compareObjValue)
                    {
                        return false;
                    }
                }
                foreach (string key in compareObj.columnValues.Keys)
                {
                    string compareObjValue = compareObj.columnValues[key];
                    if (!columnValues.ContainsKey(key))
                    {
                        return false;
                    }

                    string value = columnValues[key];
                    if (value != compareObjValue)
                    {
                        return false;
                    }
                }
            }
            else
            {
                if (compareObj.columnValues != null)
                {
                    return false;
                }
            }

            return true;
        }

      
        public override int GetHashCode()
        {
            int wkbHashCode = (wellKnownBinary == null ? 0 : wellKnownBinary.Length);
            int idHashCode = (id == null? 0:id.GetHashCode());
            int fieldValuesHashCode = 0;
            if (columnValues != null)
            {
                fieldValuesHashCode = columnValues.Count;
            }

            return wkbHashCode ^ idHashCode ^ fieldValuesHashCode;
        }

      
        public static bool operator ==(Feature feature1, Feature feature2)
        {
            return feature1.Equals(feature2);
        }

       
        public static bool operator !=(Feature feature1, Feature feature2)
        {
            return !(feature1 == feature2);
        }
    }
}

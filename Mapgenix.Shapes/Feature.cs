using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;


namespace Mapgenix.Shapes
{
    /// <summary>Basic data unit with an ID for vector data made of attributes and a shape.</summary>
    [Serializable]
    public struct Feature
    {
        private byte[] wellKnownBinary;
        private string id;
        private object tag;
        private Dictionary<string, string> columnValues;
        private RectangleShape boundingBox;

        /// <summary>Constructor of the Feature.</summary>
        /// <overloads>To pass in a BaseShape to construct the Feature.</overloads>
        /// <returns>None</returns>
        /// <remarks>You need to specify the ID of the BaseShape.</remarks>
        /// <param name="baseShape">Shape to use as the basis of the Feature.</param>
        public Feature(BaseShape baseShape)
            : this(baseShape.GetWellKnownBinary(), baseShape.Id, (IEnumerable<string>)null, baseShape.Tag)
        { }

        /// <summary>Constructor of the Feature.</summary>
        /// <overloads>To pass a  well-known binary.</overloads>
        /// <returns>None</returns>
        /// <remarks>To create a feature using well-known binary. The Id for the Feature is a random GUID.</remarks>
        /// <param name="wellKnownBinary">Well-known binary to create the feature.</param>
        public Feature(byte[] wellKnownBinary)
            : this(wellKnownBinary, Guid.NewGuid().ToString(), (IEnumerable<string>)null, null)
        { }

        /// <summary>Constructor of the Feature.</summary>
        /// <overloads>To create a feature using well-known binary and an Id.</overloads>
        /// <returns>None</returns>
        /// <param name="wellKnownBinary">Well-known binary to create the Feature.</param>
        /// <param name="id">Id for the Feature.</param>
        public Feature(byte[] wellKnownBinary, string id)
            : this(wellKnownBinary, id, (IEnumerable<string>)null, null)
        { }

        // <summary>Cconstructor of the Feature.</summary>
        /// <overloads>To create a feature using well-known text.</overloads>
        /// <returns>None</returns>
        /// <remarks>The Id for the Feature is a random GUID.</remarks>
        /// <param name="wellKnownText">Well-known text to create the Feature.</param>
        public Feature(string wellKnownText)
            : this(BaseShape.CreateShapeFromWellKnownData(wellKnownText).GetWellKnownBinary(), Guid.NewGuid().ToString(), (IEnumerable<string>)null, null)
        { }

        /// <summary>Constructor of the Feature.</summary>
        /// <overloads>To create the Feature from well-known text and an Id.</overloads>
        /// <returns>None</returns>
        /// <param name="wellKnownText">Well-known text to create the Feature.</param>
        /// <param name="id">Id used for the Feature.</param>
        public Feature(string wellKnownText, string id)
            : this(BaseShape.CreateShapeFromWellKnownData(wellKnownText).GetWellKnownBinary(), id, (IEnumerable<string>)null, null)
        { }

        /// <summary>Constructor of the Feature.</summary>
        /// <overloads>To create the Feature from a baseShape and column values.</overloads>
        /// <returns>None</returns>
        /// <param name="baseShape">BaseShape to create the Feature.</param>
        /// <param name="columnValues">ColumnValues used in the Feature.</param>
        public Feature(BaseShape baseShape, IDictionary<string, string> columnValues)
            : this(baseShape.GetWellKnownBinary(), baseShape.Id, columnValues, baseShape.Tag)
        { }

        /// <summary>Constructor of the Feature.</summary>
        public Feature(string wellKnownText, string id, IDictionary<string, string> columnValues)
            : this(BaseShape.CreateShapeFromWellKnownData(wellKnownText).GetWellKnownBinary(), id, columnValues, null)
        {
        }

        /// <summary>Constructor of the Feature.</summary>
        public Feature(string wellKnownText, string id, IEnumerable<string> columnValues)
            : this(BaseShape.CreateShapeFromWellKnownData(wellKnownText).GetWellKnownBinary(), id, columnValues, null)
        {
        }

        /// <summary>Constructor of the Feature.</summary>
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

        /// <summary>Constructor of the Feature.</summary>
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

        /// <summary>Constructor of the Feature.</summary>
        public Feature(Vertex vertex)
            : this(vertex, Guid.NewGuid().ToString(), null)
        {
        }

        /// <summary>Constructor of the Feature.</summary>
        public Feature(Vertex vertex, string id)
            : this(vertex, id, null)
        {
        }

        /// <summary>Constructor of the Feature.</summary>
        public Feature(Vertex vertex, string id, IEnumerable<string> columnValues)
            : this(vertex.X, vertex.Y, id, columnValues)
        {
        }

        /// <summary>Constructor of the Feature.</summary>
        public Feature(double x, double y)
            : this(x, y, Guid.NewGuid().ToString(), null)
        {
        }

        /// <summary>Constructor of the Feature.</summary>
        public Feature(double x, double y, string id)
            : this(x, y, id, null)
        {
        }

        /// <summary>Constructor of the Feature.</summary>
        public Feature(double x, double y, string id, IEnumerable<string> columnValues)
            : this(new PointShape(x, y).GetWellKnownBinary(), id, columnValues)
        {
        }

        /// <summary>Returns the well-known binary representing the shape of the Feature.</summary>
        public byte[] GetWellKnownBinary()
        {
            return wellKnownBinary;
        }

        /// <summary>Gets the Id of the Feature.</summary>
        public string Id
        {
            get { return id; }
        }

        /// <summary>Tag of the Feature.</summary>
        public object Tag
        {
            get { return tag; }
            set { tag = value; }
        }

        /// <summary>Gets a dictionary of column names and values representing the column data related to the Feature.</summary>
        public Dictionary<string, string> ColumnValues
        {
            get { return columnValues; }
        }

        /// <summary>Returns the well known type representing the shape of the Feature.</summary>
        public WellKnownType GetWellKnownType()
        {
            return WkbHelper.GetWellKnownTypeFromWkb(wellKnownBinary);
        }

        /// <summary>Returns the shape representing the Feature.</summary>
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

        /// <summary>Returns the well-known text representing the shape of the Feature.</summary>
        public string GetWellKnownText()
        {
            BaseShape shape = GetShape();
            return shape.GetWellKnownText();
        }

        /// <summary>Returns the bounding box of the Feature.</summary>
        public RectangleShape GetBoundingBox()
        {
            if (boundingBox == null)
            {
                boundingBox = WkbHelper.GetBoundingBoxFromWkb(wellKnownBinary);
            }
            return (RectangleShape)boundingBox.CloneDeep();
        }

        /// <summary>Clones the entire structure as a separate copy.</summary>
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

        /// <summary>Clones the entire structure as a separate copy.</summary>
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

        /// <summary>Returns true if the results of some simple validity tests on the Feature has passed.</summary>
        public bool IsValid()
        {
            bool returnValue = false;
            if (wellKnownBinary != null)
            {
                returnValue = true;
            }
            return returnValue;
        }

        /// <summary>Compares two InternalFeatures to see if they are equal.</summary>
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

        /// <summary>Returns a hash code for the Feature.</summary>
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

        /// <summary>Returns true if two Features are equal.</summary>
        public static bool operator ==(Feature feature1, Feature feature2)
        {
            return feature1.Equals(feature2);
        }

        /// <summary>Returns true if two Features are not equal.</summary>
        public static bool operator !=(Feature feature1, Feature feature2)
        {
            return !(feature1 == feature2);
        }
    }
}

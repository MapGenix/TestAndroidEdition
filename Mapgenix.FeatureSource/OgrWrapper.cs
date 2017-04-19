using System;
using System.Data;
using System.Data.Common;
using OSGeo.OGR;
using DotSpatial.Data;

namespace Mapgenix.FeatureSource
{
    public class OgrWrapper : IDisposable
	{

        private OgrDataReader _reader;
	
	 
		public int NumberOfLayers
		{
			get
			{
				return _reader.NumberOfLayers;
			}
		}

		public int FieldCount
		{
			get { return _reader.FieldCount; }
		}



		public string LayerName
		{
			get { return _reader.LayerName; }
			set
			{
                _reader.LayerName = value;
			}
		}

		public string GetFieldName(int i)
		{
			return (string)_reader.GetSchemaTable().Rows[i][SchemaTableColumn.ColumnName];
		}

		public Type GetFieldType(int i)
		{
			return _reader.GetFieldType(i);
		}

		public Layer OgrLayer { get { return _reader.MainLayer; } }

		public int FeaturesCount { get; private set; }

		public OgrWrapper(string sDataSource)
		{
            _reader = new OgrDataReader(sDataSource);
            FeaturesCount = _reader.MainLayer.GetFeatureCount(1);

        }

        public OgrWrapper(string sDataSource, string sLayer)
		{
            _reader = new OgrDataReader(sDataSource);

            FeaturesCount = _reader.MainLayer.GetFeatureCount(1);
        }

        public OgrWrapper(string sDataSource, int layerNum)
		{
            _reader = new OgrDataReader(sDataSource);

            FeaturesCount = _reader.MainLayer.GetFeatureCount(1);
        }


        public bool Read()
		{
            return _reader.Read();
		}

		public object this[string name]
		{
			get
			{
				var i = GetOrdinal(name);
				return this[i];
			}
		}

		public object this[int i]
		{
			get { return GetValue(i); }
		}

		public object GetValue(int i)
		{
            return _reader.GetValue(i);
			
		}

		public int GetOrdinal(string name)
		{
            return _reader.GetOrdinal(name);
		}

		public DataTable GetSchemaTable()
		{
            return _reader.GetSchemaTable();
		}


		public void Dispose()
		{
			Dispose(true);
		}

		~OgrWrapper()
		{
			Dispose(false);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
                _reader.Dispose();
				
			}
		}

	}
}
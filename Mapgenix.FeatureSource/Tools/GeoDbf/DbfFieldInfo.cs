using System;

namespace Mapgenix.FeatureSource
{
    [Serializable]
    public struct DbfColumnInfo
    {
        private int offset;
        private int size;
        private int decimals;
        private byte byteType;

        public int Offset
        {
            get { return offset; }
            set { offset = value; }
        }

        public int Size
        {
            get { return size; }
            set { size = value; }
        }

        public int Decimals
        {
            get { return decimals; }
            set { decimals = value; }
        }

        public byte ByteType
        {
            get { return byteType; }
            set { byteType = value; }
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (obj is DbfColumnInfo)
            {
                return Equals((DbfColumnInfo)obj);
            }
            else
            {
                return false;
            }
        }

        private bool Equals(DbfColumnInfo dbfColumnInfo)
        {
            if (this.offset != dbfColumnInfo.Offset) return false;
            if (this.size != dbfColumnInfo.Size) return false;
            if (this.decimals != dbfColumnInfo.Decimals) return false;
            if (this.byteType != dbfColumnInfo.ByteType) return false;

            return true;
        }

        public override int GetHashCode()
        {
            return offset.GetHashCode() ^ size.GetHashCode() ^ decimals.GetHashCode() ^ byteType.GetHashCode();
        }

        public static DbfColumnInfo operator +(DbfColumnInfo dbfColumnInfo1, DbfColumnInfo dbfColumnInfo2)
        {
            DbfColumnInfo dbfColumnInfo = new DbfColumnInfo();

            dbfColumnInfo.Offset = dbfColumnInfo1.offset + dbfColumnInfo2.offset;
            dbfColumnInfo.Size = dbfColumnInfo1.Size + dbfColumnInfo2.Size;
            dbfColumnInfo.Decimals = dbfColumnInfo1.Decimals + dbfColumnInfo2.Decimals;
            dbfColumnInfo.ByteType = (byte)(dbfColumnInfo1.ByteType + dbfColumnInfo2.ByteType);

            return dbfColumnInfo;
        }

        public static bool operator ==(DbfColumnInfo dbfColumnInfo1, DbfColumnInfo dbfColumnInfo2)
        {
            return dbfColumnInfo1.Equals(dbfColumnInfo2);
        }

       
        public static bool operator !=(DbfColumnInfo dbfColumnInfo1, DbfColumnInfo dbfColumnInfo2)
        {
            return !(dbfColumnInfo1 == dbfColumnInfo2);
        }
    }
}

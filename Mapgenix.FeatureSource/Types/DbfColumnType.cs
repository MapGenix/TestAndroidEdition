namespace Mapgenix.FeatureSource
{
    /// <summary>Column types in the DBF.</summary>
    public enum DbfColumnType
    {
        /// <summary>Null</summary>
        Null = 0,
        /// <summary>A string type.</summary>
        String = 1,
        /// <summary>8 byte numeric type.</summary>
        Double = 2,
        /// <summary>Logical</summary>
        Logical = 3,
        /// <summary>8 byte numeric type.</summary>
        Integer = 4,
        /// <summary>10 digit pointer to memo file.</summary>
        Memo = 5,
        /// <summary>Date in format - YYYYMMDD</summary>
        Date = 6
    }
}

namespace Mapgenix.Shapes
{
    /// <summary>Byte orders a well-known binary is written in.</summary>
    public enum WkbByteOrder
    {
        /// <summary>Least significant byte value is at the lowest address.</summary>
        LittleEndian = 0,

        /// <summary>Most significant byte value is at the lowest address.</summary>
        BigEndian = 1
    }
}

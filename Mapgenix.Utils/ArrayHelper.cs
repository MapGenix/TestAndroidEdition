using System;

namespace Mapgenix.Utils
{
    public static class ArrayHelper
    {
        public static int GetIntFromByteArray(byte[] wkb, int startIndex, byte byteOrder)
        {
            switch (byteOrder)
            {
                case 1:
                    return BitConverter.ToInt32(wkb, startIndex);

                case 0:
                    var tempArray = new byte[4];
                    Array.Copy(wkb, startIndex, tempArray, 0, tempArray.Length);
                    Array.Reverse(tempArray);
                    return BitConverter.ToInt32(tempArray, 0);

                default:
                    return BitConverter.ToInt32(wkb, startIndex);
            }
        }

        public static short GetInt16FromByteArray(byte[] wkb, int startIndex, byte byteOrder)
        {
            switch (byteOrder)
            {
                case 1:
                    return BitConverter.ToInt16(wkb, startIndex);

                case 0:
                    var tempArray = new byte[2];
                    Array.Copy(wkb, startIndex, tempArray, 0, tempArray.Length);
                    Array.Reverse(tempArray);
                    return BitConverter.ToInt16(tempArray, 0);

                default:
                    return BitConverter.ToInt16(wkb, startIndex);
            }
        }

        public static double GetDoubleFromByteArray(byte[] wkb, int startIndex, byte byteOrder)
        {
            double result;

            if (byteOrder == 1)
            {
                result = BitConverter.ToDouble(wkb, startIndex);
            }
            else
            {
                var tempArray = new byte[8];
                Array.Copy(wkb, startIndex, tempArray, 0, tempArray.Length);
                Array.Reverse(tempArray);

                result = BitConverter.ToDouble(tempArray, 0);
            }

            return result;
        }

        public static int GetShapeTypeFromByteArray(byte[] wkb, int startIndex)
        {
            var tempArray = new byte[4];
            Array.Copy(wkb, startIndex, tempArray, 0, tempArray.Length);

            if (!(tempArray[1] == 0 && tempArray[2] == 0 && (tempArray[0] == 0 || tempArray[3] == 0)))
            {
                throw new ArgumentException(ExceptionDescription.WkbIsInvalid, "wkb");
            }

            return tempArray[0] + tempArray[3];

        }

        public static void CopyToArray(byte[] sourceArray, byte[] destinateArray, long destinateIndex)
        {
            for (int i = 0; i < sourceArray.Length; i++)
            {
                destinateArray[destinateIndex + i] = sourceArray[i];
            }
        }

        public static byte[] GetByteArrayFromDouble(double doubleValue, byte byteOrder)
        {
            byte[] bytes = BitConverter.GetBytes(doubleValue);

            if (byteOrder == 0)
            {
                Array.Reverse(bytes);
            }

            return bytes;
        }

    }
}

using System.IO;

namespace Mapgenix.Utils
{
    public static class StreamHelper
    {
        public static void CopyStream(Stream sourceStream, Stream targetStream)
        {
            int sizeOfBlock = 1024;
            int numberOfBlock = (int)(sourceStream.Length / sizeOfBlock);
            int left = (int)(sourceStream.Length % sizeOfBlock);
            sourceStream.Seek(0, SeekOrigin.Begin);
            targetStream.Seek(0, SeekOrigin.Begin);
            byte[] bytes = null;

            for (int i = 0; i < numberOfBlock; i++)
            {
                bytes = new byte[sizeOfBlock];
                sourceStream.Read(bytes, 0, sizeOfBlock);
                targetStream.Write(bytes, 0, sizeOfBlock);
            }

            bytes = new byte[left];
            sourceStream.Read(bytes, 0, left);
            targetStream.Write(bytes, 0, left);
        }



    }


}

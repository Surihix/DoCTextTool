using Ionic.Zlib;
using System.IO;

namespace DoCTextTool.LinesClasses
{
    internal static class ZlibLibrary
    {
        public static void ZlibDecompress(this Stream cmpStreamName, Stream outStreamName)
        {
            using (ZlibStream decompressor = new ZlibStream(cmpStreamName, CompressionMode.Decompress, true))
            {
                decompressor.CopyTo(outStreamName);
            }
        }
    }
}
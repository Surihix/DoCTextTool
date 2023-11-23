using System.IO;

namespace DoCTextTool
{
    internal class TextEncryptor
    {
        public static void EncryptProcess(string inTxtFile)
        {
            var outFile = Path.Combine(Path.GetDirectoryName(inTxtFile), $"{Path.GetFileNameWithoutExtension(inTxtFile)}.enc");
        }
    }
}
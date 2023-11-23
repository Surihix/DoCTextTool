using System;
using System.IO;
using static DoCTextTool.SupportClasses.ToolHelpers;

namespace DoCTextTool
{
    internal class TxtEncryptor
    {
        public static void EncryptProcess(string inFile)
        {
            Console.WriteLine($"Decrypting '{Path.GetFileName(inFile)}' data....");
            Console.WriteLine("");

            var outFile = Path.Combine(Path.GetDirectoryName(inFile), $"{Path.GetFileNameWithoutExtension(inFile)}.enc");

            using (var )
            {

            }


            ExitType.Success.ExitProgram($"Finished copying encrypted data to '{Path.GetFileName(outFile)}' file");
        }
    }
}
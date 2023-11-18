using DoCTextTool.SupportClasses;
using System;
using System.IO;
using static DoCTextTool.SupportClasses.ToolHelpers;

namespace DoCTextTool.DoCTextTool
{
    internal class TextConverter
    {
        public static void ConvertProcess(string inTxtFile)
        {
            var outFile = Path.Combine(Path.GetDirectoryName(inTxtFile), $"{Path.GetFileNameWithoutExtension(inTxtFile)}.bin");

            using (var inFileStream = new FileStream(inTxtFile, FileMode.Open, FileAccess.Read))
            {
                using (var inFileReader = new BinaryReader(inFileStream))
                {
                    Console.WriteLine("");

                    ExitType.Success.ExitProgram("Finish message");
                }
            }
        }
    }
}
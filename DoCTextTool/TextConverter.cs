using DoCTextTool.LineClasses;
using DoCTextTool.SupportClasses;
using System;
using System.IO;
using System.Text;
using static DoCTextTool.SupportClasses.ToolHelpers;

namespace DoCTextTool.DoCTextTool
{
    internal class TextConverter
    {
        public static void ConvertProcess(string inTxtFile)
        {
            Console.WriteLine("Converting text file data....");
            Console.WriteLine("");

            using (var inFileStream = new FileStream(inTxtFile, FileMode.Open, FileAccess.Read))
            {
                using (var inFileReader = new StreamReader(inFileStream, Encoding.UTF8))
                {

                    using (var bodyStream = new MemoryStream())
                    {
                        using (var bodyWriter = new BinaryWriter(bodyStream))
                        {
                            ushort lineCount = 0;
                            LinesConverter.ConvertLines(inFileReader, bodyWriter, ref lineCount);
                            var decryptBtmTxt = LinesConverter.LargestLineData(inFileReader, lineCount);


                            using (var headerStream = new MemoryStream())
                            {
                                using (var headerWriter = new BinaryWriter(headerStream))
                                {
                                    var header = new FileStructs.Header();
                                    header.SeedValueA = 1;
                                    header.SeedValueB = 0;

                                    headerStream.Seek(0, SeekOrigin.Begin);
                                    headerStream.InsertEmptyBytes(32);

                                    header.LineCount = lineCount;
                                    header.Reserved = 0;
                                    header.DcmpFlag = 1;

                                    header.DecryptBtmTxtSize = (uint)decryptBtmTxt.Length;
                                    header.DcmpBodySize = (uint)bodyStream.Length;
                                    header.HeaderSize = 24;

                                    headerWriter.BaseStream.Position = 0;
                                    headerWriter.Write(header.SeedValueA);
                                    headerWriter.Write(header.SeedValueB);
                                    headerWriter.Write(header.LineCount);
                                    headerWriter.Write(header.Reserved);
                                    headerWriter.Write(header.DcmpFlag);
                                    headerWriter.Write(header.DecryptBtmTxtSize);
                                    headerWriter.Write(header.DcmpBodySize);

                                    headerWriter.BaseStream.Position = 24;
                                    headerWriter.Write(header.HeaderSize);

                                    // Debugging purpose
                                }
                            }
                        }

                        var outFile = Path.Combine(Path.GetDirectoryName(inTxtFile), $"{Path.GetFileNameWithoutExtension(inTxtFile)}.bin");
                    }
                }
            }

            ExitType.Success.ExitProgram("Finished converting text data to valid Dirge Of Cerberus text file");
        }
    }
}
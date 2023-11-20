using DoCTextTool.SupportClasses;
using System;
using System.IO;
using System.Text;
using static DoCTextTool.SupportClasses.ToolHelpers;

namespace DoCTextTool.LineClasses
{
    internal class LinesConverter
    {
        public static void ConvertLines(StreamReader inFileReader, BinaryWriter bodyWriter, ref ushort lineCount)
        {
            lineCount = ushort.Parse(inFileReader.ReadLine());
            Console.WriteLine($"Line count: {lineCount}");
            Console.WriteLine("");

            if (lineCount > 1000)
            {
                Console.WriteLine("");
                ExitType.Error.ExitProgram("Line count is invalid");
            }

            bodyWriter.BaseStream.Seek(0, SeekOrigin.Begin);
            bodyWriter.BaseStream.InsertEmptyBytes(lineCount * 12);


            using (var linesStream = new MemoryStream())
            {
                using (var linesWriter = new BinaryWriter(linesStream))
                {
                    Console.WriteLine("Parsing lines....");
                    Console.WriteLine("");


                    // Process lines and unknownIds
                    var lineOffsets = new FileStructs.LineOffsets();
                    long bodySectionWritePos = 0;
                    lineOffsets.LineOffset = 0;
                    uint lineOffsetAbsoluteStartPos = (uint)(lineCount * 12) + 32;

                    for (int l = 0; l < lineCount; l++)
                    {
                        var currentLineData = inFileReader.ReadLine().Split(new string[] { " || " }, StringSplitOptions.None);
                        lineOffsets.UnknownId = uint.Parse(currentLineData[0]);
                        var currentLine = EncodingShift(currentLineData[2]);

                        lineOffsets.LineOffset = (uint)linesStream.Length;
                        linesWriter.BaseStream.Position = lineOffsets.LineOffset;
                        linesWriter.Write(currentLine);
                        linesWriter.Write((byte)0);

                        bodyWriter.BaseStream.Position = bodySectionWritePos;
                        bodyWriter.Write(lineOffsets.UnknownId);

                        bodyWriter.BaseStream.Position = bodySectionWritePos + 8;
                        lineOffsets.LineOffset += lineOffsetAbsoluteStartPos;
                        bodyWriter.Write(lineOffsets.LineOffset);

                        bodySectionWritePos += 12;
                    }


                    Console.WriteLine("Parsing line ids....");
                    Console.WriteLine("");

                    // Process line ids
                    inFileReader.BaseStream.Seek(0, SeekOrigin.Begin);
                    _ = inFileReader.ReadLine();

                    bodySectionWritePos = 0;

                    for (int li = 0; li < lineCount; li++)
                    {
                        var currentLineIdData = inFileReader.ReadLine().Split(new string[] { " || " }, StringSplitOptions.None);
                        var currentLineId = EncodingShift(currentLineIdData[1]);

                        lineOffsets.LineIdOffset = (uint)linesStream.Length;
                        linesWriter.BaseStream.Position = lineOffsets.LineIdOffset;
                        linesWriter.Write(currentLineId);
                        linesWriter.Write((byte)0);

                        bodyWriter.BaseStream.Position = bodySectionWritePos + 4;
                        lineOffsets.LineIdOffset += lineOffsetAbsoluteStartPos;
                        bodyWriter.Write(lineOffsets.LineIdOffset);

                        bodySectionWritePos += 12;
                    }


                    // Copy processed lines and
                    // line id streams to body
                    // stream
                    linesStream.Seek(0, SeekOrigin.Begin);
                    bodyWriter.BaseStream.Seek(bodyWriter.BaseStream.Length, SeekOrigin.Begin);
                    linesStream.CopyTo(bodyWriter.BaseStream);


                    // Debugging purpose
                }
            }
        }

        static byte[] EncodingShift(string inputString)
        {
            var stringsUTF8Array = Encoding.UTF8.GetBytes(inputString);
            var stringsShiftJSArray = Encoding.Convert(Encoding.UTF8, Encoding.GetEncoding("shift-jis"), stringsUTF8Array);

            return stringsShiftJSArray;
        }


        public static byte[] LargestLineData(StreamReader inFileReader, ushort lineCount)
        {
            inFileReader.BaseStream.Seek(0, SeekOrigin.Begin);
            _ = inFileReader.ReadLine();

            byte[] largestLine = new byte[] { };
            var prevSize = 0;
            var currentSize = 0;

            for (int lil = 0; lil < lineCount; lil++)
            {

            }
            return largestLine;
        }
    }
}
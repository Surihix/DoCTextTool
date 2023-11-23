using DoCTextTool.SupportClasses;
using System;
using System.Collections.Generic;
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

                    // Process lines and unknownIds
                    Console.WriteLine("Parsing lines....");

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


                    // Process line ids
                    Console.WriteLine("Parsing line ids....");

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
                    var processedDataStartPos = bodyWriter.BaseStream.Length;
                    linesStream.Seek(0, SeekOrigin.Begin);
                    bodyWriter.BaseStream.Seek(processedDataStartPos, SeekOrigin.Begin);
                    linesStream.CopyTo(bodyWriter.BaseStream);
                }
            }
        }

        static byte[] EncodingShift(string inputString)
        {
            var stringsUTF8Array = Encoding.UTF8.GetBytes(inputString);
            var stringsShiftJSArray = Encoding.Convert(Encoding.UTF8, Encoding.GetEncoding("shift-jis"), stringsUTF8Array);

            return stringsShiftJSArray;
        }


        public static byte[] GetLongestLine(StreamReader inFileReader, ushort lineCount)
        {
            inFileReader.BaseStream.Seek(0, SeekOrigin.Begin);
            _ = inFileReader.ReadLine();

            byte[] longestLineArray = new byte[] { };
            var prevLargestSize = 0;

            for (int i = 0; i < lineCount; i++)
            {
                var currentLineIdData = inFileReader.ReadLine().Split(new string[] { " || " }, StringSplitOptions.None);
                var currentLine = EncodingShift(currentLineIdData[2]);

                var currentSize = currentLine.Length;

                if (currentSize >= prevLargestSize)
                {
                    prevLargestSize = currentSize;
                    longestLineArray = currentLine;
                }
            }

            var padNulls = ((long)prevLargestSize).PadCheckDivisibility(8);

            if (padNulls > 0)
            {
                var adjustedList = new List<byte>();
                foreach (var b in longestLineArray)
                {
                    adjustedList.Add(b);
                }

                for (int a = 0; a < padNulls; a++)
                {
                    adjustedList.Add(0);
                }

                longestLineArray = adjustedList.ToArray();
            }

            return longestLineArray;
        }
    }
}
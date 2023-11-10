using DoCTextTool.SupportClasses;
using Ionic.Zlib;
using System;
using System.Collections.Generic;
using System.IO;

namespace DoCTextTool.LineClasses
{
    internal class LinesParser
    {
        public static void ExtractLines(Stream decryptedStream, bool isCompressed, uint bodySize, ushort lineCount, string outFile)
        {
            using (var outStream = new MemoryStream())
            {
                using (var outStreamReader = new BinaryReader(outStream))
                {
                    decryptedStream.Seek(0, SeekOrigin.Begin);
                    decryptedStream.CopyInBuffers(outStream, 32);

                    decryptedStream.Seek(32, SeekOrigin.Begin);

                    switch (isCompressed)
                    {
                        case true:
                            using (ZlibStream decompressor = new ZlibStream(decryptedStream, CompressionMode.Decompress, true))
                            {
                                decompressor.CopyTo(outStream);
                            }
                            break;

                        case false:
                            decryptedStream.CopyInBuffers(outStream, bodySize);
                            break;
                    }

                    using (var outFileStream = new FileStream(outFile, FileMode.Append, FileAccess.Write))
                    {
                        using (var outFileWriter = new BinaryWriter(outFileStream))
                        {

                            var lineOffsets = new FileStructs.LineOffsets();
                            var readPos = 32;
                            uint writePos = 0;

                            for (int l = 0; l < lineCount; l++)
                            {
                                // Get offsets
                                outStreamReader.BaseStream.Position = readPos;
                                lineOffsets.UnknownId = outStreamReader.ReadUInt32();

                                outStreamReader.BaseStream.Position = readPos + 4;
                                lineOffsets.LineIdOffset = outStreamReader.ReadUInt32();

                                outStreamReader.BaseStream.Position = readPos + 8;
                                lineOffsets.LineOffset = outStreamReader.ReadUInt32();

                                // Write UnkID
                                var unkId = Convert.ToString(lineOffsets.UnknownId);
                                var unkIdList = new List<byte>();
                                foreach (var num in unkId)
                                {
                                    unkIdList.Add(Convert.ToByte(num));
                                }
                                WriteEachByte(unkIdList, outFileWriter, ref writePos);
                                WriteSeparator(outFileWriter, writePos);
                                writePos = (uint)outFileWriter.BaseStream.Position;

                                // Write Line ID
                                outStreamReader.BaseStream.Position = lineOffsets.LineIdOffset;
                                var lineIdBytesList = outStreamReader.ReadBytesTillNull();
                                WriteEachByte(lineIdBytesList, outFileWriter, ref writePos);
                                WriteSeparator(outFileWriter, writePos);
                                writePos = (uint)outFileWriter.BaseStream.Position;

                                // Write Line
                                outStreamReader.BaseStream.Position = lineOffsets.LineOffset;
                                var lineBytesList = outStreamReader.ReadBytesTillNull();
                                WriteEachByte(lineBytesList, outFileWriter, ref writePos);
                                writePos = (uint)outFileWriter.BaseStream.Position;

                                // Move to next line
                                outFileWriter.BaseStream.Position = writePos;
                                outFileWriter.Write((byte)13);
                                outFileWriter.Write((byte)10);
                                writePos = (uint)outFileWriter.BaseStream.Position;

                                readPos += 12;
                            }
                        }
                    }
                }
            }
        }

        static void WriteEachByte(List<byte> stringBytesList, BinaryWriter outFileWriter, ref uint writePos)
        {
            foreach (var stringByte in stringBytesList)
            {
                outFileWriter.BaseStream.Position = writePos;
                outFileWriter.Write(stringByte);
                writePos += 1;
            }

            writePos = (uint)outFileWriter.BaseStream.Position;
        }

        static void WriteSeparator(BinaryWriter outFileWriter, uint writePos)
        {
            outFileWriter.BaseStream.Position = writePos;
            outFileWriter.Write((byte)32);
            outFileWriter.Write((byte)124);
            outFileWriter.Write((byte)124);
            outFileWriter.Write((byte)32);
        }
    }
}
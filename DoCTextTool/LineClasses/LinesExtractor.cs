using DoCTextTool.SupportClasses;
using Ionic.Zlib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DoCTextTool.LineClasses
{
    internal class LinesExtractor
    {
        public static void ExtractLines(Stream decryptedStream, bool isCompressed, uint bodySize, ushort lineCount, string outFile)
        {
            outFile.IfFileExistsDel();

            using (var outBinStream = new MemoryStream())
            {
                using (var outBinReader = new BinaryReader(outBinStream))
                {
                    decryptedStream.Seek(0, SeekOrigin.Begin);
                    decryptedStream.CopyInBuffers(outBinStream, 32);

                    decryptedStream.Seek(32, SeekOrigin.Begin);

                    switch (isCompressed)
                    {
                        case true:
                            using (ZlibStream decompressor = new ZlibStream(decryptedStream, CompressionMode.Decompress, true))
                            {
                                decompressor.CopyTo(outBinStream);
                            }
                            break;

                        case false:
                            decryptedStream.CopyInBuffers(outBinStream, bodySize);
                            break;
                    }

                    using (var outTxtBinStream = new FileStream(outFile, FileMode.Append, FileAccess.Write))
                    {
                        using (var outTxtBinWriter = new BinaryWriter(outTxtBinStream))
                        {

                            var lineOffsets = new FileStructs.LineOffsets();
                            var readPos = 32;
                            uint writePos = 0;

                            for (int l = 0; l < lineCount; l++)
                            {
                                // Get offsets
                                outBinReader.BaseStream.Position = readPos;
                                lineOffsets.UnknownId = outBinReader.ReadUInt32();

                                outBinReader.BaseStream.Position = readPos + 4;
                                lineOffsets.LineIdOffset = outBinReader.ReadUInt32();

                                outBinReader.BaseStream.Position = readPos + 8;
                                lineOffsets.LineOffset = outBinReader.ReadUInt32();

                                // Write UnkID
                                var unkId = Convert.ToString(lineOffsets.UnknownId);
                                var unkIdList = new List<byte>();
                                foreach (var num in unkId)
                                {
                                    unkIdList.Add(Convert.ToByte(num));
                                }
                                WriteEachByte(unkIdList, outTxtBinWriter, ref writePos);
                                WriteSeparator(outTxtBinWriter, writePos);
                                writePos = (uint)outTxtBinWriter.BaseStream.Position;

                                // Write Line ID
                                outBinReader.BaseStream.Position = lineOffsets.LineIdOffset;
                                var lineIdBytesList = outBinReader.ReadBytesTillNull();
                                WriteEachByte(lineIdBytesList, outTxtBinWriter, ref writePos);
                                WriteSeparator(outTxtBinWriter, writePos);
                                writePos = (uint)outTxtBinWriter.BaseStream.Position;

                                // Write Line
                                outBinReader.BaseStream.Position = lineOffsets.LineOffset;
                                var lineBytesList = outBinReader.ReadBytesTillNull();
                                WriteEachByte(lineBytesList, outTxtBinWriter, ref writePos);
                                writePos = (uint)outTxtBinWriter.BaseStream.Position;

                                // Move to next line
                                outTxtBinWriter.BaseStream.Position = writePos;
                                outTxtBinWriter.Write((byte)13);
                                outTxtBinWriter.Write((byte)10);
                                writePos = (uint)outTxtBinWriter.BaseStream.Position;

                                readPos += 12;
                            }
                        }
                    }

                    // Convert the txt file to UTF-8
                    var outTxtBinArray = File.ReadAllBytes(outFile);
                    var outTxtBinConverted = Encoding.Convert(Encoding.GetEncoding("shift-jis"), Encoding.UTF8, outTxtBinArray);

                    File.Delete(outFile);
                    File.WriteAllBytes(outFile, outTxtBinConverted);
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
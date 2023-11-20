using DoCTextTool.CryptographyClasses;
using DoCTextTool.LineClasses;
using DoCTextTool.SupportClasses;
using Ionic.Zlib;
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

                            var decryptedFooterTxt = LinesConverter.GetLongestLine(inFileReader, lineCount);
                            Console.WriteLine("Generated bottom decrypted text");
                            Console.WriteLine("");

                            var bodySize = bodyStream.Length;
                            var padNulls = bodySize.CheckDivisibility(8);

                            if (padNulls > 0)
                            {
                                bodyStream.Seek(bodySize, SeekOrigin.Begin);
                                bodyStream.InsertEmptyBytes(padNulls);
                            }

                            using (var headerStream = new MemoryStream())
                            {
                                using (var headerReader = new BinaryReader(headerStream))
                                {
                                    using (var headerWriter = new BinaryWriter(headerStream))
                                    {
                                        // Create the base header
                                        // offsets
                                        var header = new FileStructs.Header();
                                        header.SeedValueA = 1;
                                        header.SeedValueB = 0;

                                        headerStream.Seek(0, SeekOrigin.Begin);
                                        headerStream.InsertEmptyBytes(32);

                                        header.LineCount = lineCount;
                                        header.Reserved = 0;
                                        header.DcmpFlag = 1;

                                        header.DecryptedFooterTxtSize = (uint)decryptedFooterTxt.Length;
                                        header.DcmpBodySize = (uint)bodyStream.Length;

                                        headerWriter.BaseStream.Position = 0;
                                        headerWriter.Write(header.SeedValueA);
                                        headerWriter.Write(header.SeedValueB);
                                        headerWriter.Write(header.LineCount);
                                        headerWriter.Write(header.Reserved);
                                        headerWriter.Write(header.DcmpFlag);
                                        headerWriter.Write(header.DecryptedFooterTxtSize);
                                        headerWriter.Write(header.DcmpBodySize);


                                        // Debugging purpose
                                        //bodyStream.Seek(0, SeekOrigin.Begin);
                                        //File.WriteAllBytes("testDataBody", bodyStream.ToArray());

                                        //File.WriteAllBytes("testDataFooterTxt", decryptedFooterTxt);


                                        // Prepare a stream for holding the
                                        // compressed body section
                                        using (var cmpBodyStream = new MemoryStream())
                                        {
                                            using (var cmpBodyReader = new BinaryReader(cmpBodyStream))
                                            {
                                                using (var cmpBodyWriter = new BinaryWriter(cmpBodyStream))
                                                {
                                                    // Compress body section
                                                    Console.WriteLine("Compressing text data....");
                                                    Console.WriteLine("");

                                                    bodyStream.Seek(0, SeekOrigin.Begin);

                                                    using (var zlibCmpStream = new ZlibStream(bodyStream, CompressionMode.Compress, CompressionLevel.Level5, true))
                                                    {
                                                        cmpBodyWriter.Seek(0, SeekOrigin.Begin);
                                                        zlibCmpStream.CopyTo(cmpBodyStream);
                                                    }

                                                    // Pad null byte to make the 
                                                    // compressed body divisible
                                                    // by 8
                                                    var cmpbodySize = cmpBodyStream.Length;
                                                    padNulls = cmpbodySize.CheckDivisibility(8);

                                                    if (padNulls > 0)
                                                    {
                                                        cmpBodyStream.Seek(cmpbodySize, SeekOrigin.Begin);
                                                        cmpBodyStream.InsertEmptyBytes(padNulls);
                                                    }

                                                    // Insert footer bytes for the
                                                    // body section and update these
                                                    // footer offsets accordingly
                                                    var bodyFooter = new FileStructs.BodyFooter();
                                                    bodyFooter.BodyDataSize = (uint)(cmpbodySize + padNulls);
                                                    bodyFooter.CompressedDataCheckSum = cmpBodyReader.ComputeCheckSum();

                                                    cmpBodyWriter.BaseStream.Position = cmpbodySize + padNulls;
                                                    cmpBodyStream.InsertEmptyBytes(8);

                                                    cmpBodyWriter.BaseStream.Position = cmpbodySize + padNulls;
                                                    cmpBodyWriter.Write(bodyFooter.BodyDataSize);
                                                    cmpBodyWriter.Write(bodyFooter.CompressedDataCheckSum);


                                                    // Do one final update for the header
                                                    // offsets
                                                    header.TotalFileSize = 32 + bodyFooter.BodyDataSize + 8 + header.DecryptedFooterTxtSize;
                                                    headerWriter.BaseStream.Position = 20;
                                                    headerWriter.Write(header.TotalFileSize);

                                                    header.HeaderCheckSum = headerReader.ComputeCheckSum();
                                                    header.HeaderSize = 24;
                                                    headerWriter.BaseStream.Position = 24;
                                                    headerWriter.Write(header.HeaderSize);
                                                    headerWriter.Write(header.HeaderCheckSum);


                                                    // Debugging purpose
                                                    //headerStream.Seek(0, SeekOrigin.Begin);
                                                    //File.WriteAllBytes("testDataHeader", headerStream.ToArray());

                                                    //cmpBodyStream.Seek(0, SeekOrigin.Begin);
                                                    //File.WriteAllBytes("testDataBodyCmp", cmpBodyStream.ToArray());


                                                    // Create the final output file
                                                    // with this stream
                                                    var outFile = Path.Combine(Path.GetDirectoryName(inTxtFile), $"{Path.GetFileNameWithoutExtension(inTxtFile)}.bin");
                                                    outFile.IfFileExistsDel();

                                                    using (var outFileStream = new FileStream(outFile, FileMode.Append, FileAccess.Write))
                                                    {
                                                        using (var outFileWriter = new BinaryWriter(outFileStream))
                                                        {
                                                            // Encrypt header section
                                                            Console.WriteLine("Encrypting header section....");
                                                            Console.WriteLine("");

                                                            headerStream.Seek(0, SeekOrigin.Begin);
                                                            Encryption.EncryptSection(KeyArrays.KeyblocksHeader, 4, 0, 0, headerReader, outFileWriter);


                                                            // Encrypt body section
                                                            Console.WriteLine("Encrypting body section....");
                                                            Console.WriteLine("");

                                                            cmpBodyStream.Seek(0, SeekOrigin.Begin);
                                                            var blockCount = (uint)cmpBodyStream.Length / 8;
                                                            Encryption.EncryptSection(KeyArrays.KeyBlocksMainBody, blockCount, 0, 32, cmpBodyReader, outFileWriter);


                                                            // Copy the decrypted bottom text
                                                            outFileWriter.Write(decryptedFooterTxt);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            ExitType.Success.ExitProgram("Finished converting text data to bin file");
        }
    }
}
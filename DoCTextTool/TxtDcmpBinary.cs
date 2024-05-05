using DoCTextTool.CryptoClasses;
using DoCTextTool.SupportClasses;
using Ionic.Zlib;
using System;
using System.IO;
using static DoCTextTool.SupportClasses.ToolHelpers;

namespace DoCTextTool.DoCTextTool
{
    internal class TxtDcmpBinary
    {
        public static void BinaryProcess(string inFile)
        {
            Console.WriteLine($"Extracting decompressed binary data from '{Path.GetFileName(inFile)}'....");
            Console.WriteLine("");

            var outFile = Path.Combine(Path.GetDirectoryName(inFile), $"{Path.GetFileNameWithoutExtension(inFile)}_dcmp.bin");

            using (var inFileReader = new BinaryReader(File.Open(inFile, FileMode.Open, FileAccess.Read)))
            {
                inFileReader.BaseStream.Length.HeaderLengthCheck();

                inFileReader.BaseStream.Position = 0;
                var headerValue = inFileReader.ReadUInt64();

                headerValue.HeaderValueCheck();


                using (var decryptedStream = new MemoryStream())
                {
                    using (var decryptedStreamBinWriter = new BinaryWriter(decryptedStream))
                    {
                        uint readPos = 0;
                        uint writePos = 0;
                        uint blockCount = 0;

                        // Header
                        Console.WriteLine("Generating header keyblock table....");
                        Console.WriteLine("");

                        var headerSeedArray = new byte[] { 136, 86, 49, 149, 241, 163, 137, 87 };
                        var keyblocksTableHeader = Generators.GenerateKeyblocksTable(headerSeedArray, false);

                        Console.WriteLine("Decrypting header section....");
                        Console.WriteLine("");
                        Decryption.DecryptBlocks(keyblocksTableHeader, 4, readPos, writePos, inFileReader, decryptedStreamBinWriter, false);

                        using (var decryptedStreamBinReader = new BinaryReader(decryptedStream))
                        {
                            var header = new ToolVariables.Header();
                            decryptedStreamBinReader.BaseStream.Position = 0;
                            header.SeedValue = decryptedStreamBinReader.ReadUInt64();

                            decryptedStreamBinReader.BaseStream.Position = 28;
                            header.HeaderCheckSum = decryptedStreamBinReader.ReadUInt32();

                            if (header.HeaderCheckSum != decryptedStreamBinReader.ComputeCheckSum(24 / 4, 0))
                            {
                                ExitType.Error.ExitProgram("Header section was not decrypted properly");
                            }

                            var bodyFooter = new ToolVariables.BodyFooter();

                            decryptedStreamBinReader.BaseStream.Position = 11;
                            header.DcmpFlag = decryptedStreamBinReader.ReadByte();
                            header.DecryptedFooterTxtSize = decryptedStreamBinReader.ReadUInt32();
                            header.DcmpBodySize = decryptedStreamBinReader.ReadUInt32();
                            bodyFooter.BodyDataSize = header.DcmpBodySize;

                            var isCompressed = true;
                            var doHashCompute = true;
                            if (header.DcmpFlag == 0)
                            {
                                isCompressed = false;
                                doHashCompute = false;
                            }


                            // Body
                            Console.WriteLine("Generating body keyblock table....");
                            Console.WriteLine("");
                            var keyblocksTableBody = Generators.GenerateKeyblocksTable(BitConverter.GetBytes(header.SeedValue), false);

                            Console.WriteLine("Decrypting body section....");
                            Console.WriteLine("");

                            var decryptionBodySize = new FileInfo(inFile).Length - header.DecryptedFooterTxtSize - 32;
                            ((uint)decryptionBodySize).CryptoLengthCheck();
                            blockCount = (uint)decryptionBodySize / 8;

                            readPos = 32;
                            writePos = 32;

                            Decryption.DecryptBlocks(keyblocksTableBody, blockCount, readPos, writePos, inFileReader, decryptedStreamBinWriter, false);

                            // Debugging purpose
                            //File.WriteAllBytes("DecryptedDataTest", decryptedStream.ToArray());

                            decryptedStreamBinReader.BaseStream.Position = decryptedStreamBinReader.BaseStream.Length - 4;
                            bodyFooter.CompressedDataCheckSum = decryptedStreamBinReader.ReadUInt32();

                            if (bodyFooter.CompressedDataCheckSum != decryptedStreamBinReader.ComputeCheckSum(((uint)decryptionBodySize - 8) / 4, readPos))
                            {
                                ExitType.Error.ExitProgram("Body section was not decrypted properly");
                            }

                            outFile.IfFileExistsDel();


                            using (var outFileStream = new FileStream(outFile, FileMode.Append, FileAccess.Write))
                            {
                                // Copy header data
                                decryptedStream.Seek(0, SeekOrigin.Begin);
                                decryptedStream.ExCopyTo(outFileStream, 32);

                                // Decompress body data
                                // if its compressed.
                                // If not compressed, then 
                                // copy the body data directly
                                // into the outfile.
                                if (isCompressed)
                                {
                                    Console.WriteLine("Decompressing and copying data....");
                                    Console.WriteLine("");

                                    using (var zlibBodyData = new MemoryStream())
                                    {
                                        decryptedStream.Seek(32, SeekOrigin.Begin);
                                        decryptedStream.ExCopyTo(zlibBodyData, decryptionBodySize - 8);

                                        zlibBodyData.Seek(0, SeekOrigin.Begin);

                                        using (var decompressor = new ZlibStream(zlibBodyData, CompressionMode.Decompress, true))
                                        {
                                            decompressor.CopyTo(outFileStream);
                                        }
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("Copying decompressed data....");
                                    Console.WriteLine("");

                                    decryptedStream.Seek(32, SeekOrigin.Begin);
                                    decryptedStream.ExCopyTo(outFileStream, decryptionBodySize - 8);
                                }

                                // Copy body footer data
                                decryptedStream.Seek((32 + decryptionBodySize) - 8, SeekOrigin.Begin);
                                decryptedStream.ExCopyTo(outFileStream, 8);

                                // Copy decrypted txt snippet
                                inFileReader.BaseStream.Seek(decryptionBodySize + 32, SeekOrigin.Begin);
                                inFileReader.BaseStream.CopyTo(outFileStream);
                            }

                            // Update cmpflag, size, and
                            // hash offsets if the doHashCompute
                            // flag was set
                            if (doHashCompute)
                            {
                                Console.WriteLine("Updating offsets....");
                                Console.WriteLine("");

                                using (var outFileWriter = new BinaryWriter(File.Open(outFile, FileMode.Open, FileAccess.Write)))
                                {
                                    header.TotalFileSize = (uint)outFileWriter.BaseStream.Length;

                                    outFileWriter.BaseStream.Position = 11;
                                    outFileWriter.Write((byte)0);

                                    outFileWriter.BaseStream.Position = 20;
                                    outFileWriter.Write(header.TotalFileSize);

                                    outFileWriter.BaseStream.Position = 32 + header.DcmpBodySize;
                                    outFileWriter.Write(bodyFooter.BodyDataSize);
                                }

                                using (var outFileReader = new BinaryReader(File.Open(outFile, FileMode.Open, FileAccess.Read)))
                                {
                                    header.HeaderCheckSum = outFileReader.ComputeCheckSum(24 / 4, 0);
                                    bodyFooter.CompressedDataCheckSum = outFileReader.ComputeCheckSum(bodyFooter.BodyDataSize / 4, 32);
                                }

                                using (var outFileWriter2 = new BinaryWriter(File.Open(outFile, FileMode.Open, FileAccess.Write)))
                                {
                                    outFileWriter2.BaseStream.Position = 28;
                                    outFileWriter2.Write(header.HeaderCheckSum);

                                    outFileWriter2.BaseStream.Position = 32 + bodyFooter.BodyDataSize + 4;
                                    outFileWriter2.Write(bodyFooter.CompressedDataCheckSum);
                                }
                            }
                        }
                    }
                }
            }

            Console.WriteLine("");
            ExitType.Success.ExitProgram($"Finished extracting binary data to '{Path.GetFileName(outFile)}' file");
        }
    }
}
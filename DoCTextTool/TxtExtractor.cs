using DoCTextTool.CryptoClasses;
using DoCTextTool.LineClasses;
using DoCTextTool.SupportClasses;
using System;
using System.IO;
using static DoCTextTool.SupportClasses.ToolHelpers;

namespace DoCTextTool
{
    internal class TxtExtractor
    {
        public static void ExtractProcess(string inFile)
        {
            Console.WriteLine($"Extracting text data from '{Path.GetFileName(inFile)}'....");
            Console.WriteLine("");

            Console.WriteLine($"Encoding Specified: {ToolVariables.TxtEncoding.ToString().Replace("lt", "Latin").Replace("jp", "Japanese")}");
            Console.WriteLine("");

            var outFile = Path.Combine(Path.GetDirectoryName(inFile), $"{Path.GetFileNameWithoutExtension(inFile)}.txt");

            using (var inFileReader = new BinaryReader(File.Open(inFile, FileMode.Open, FileAccess.Read)))
            {
                inFileReader.BaseStream.Length.HeaderLengthCheck();

                inFileReader.BaseStream.Position = 0;
                var headerValue = inFileReader.ReadUInt64();

                headerValue.HeaderValueCheck();

                using (var decryptedStream = new MemoryStream())
                {
                    using (var decryptedStreamBinReader = new BinaryReader(decryptedStream))
                    {
                        using (var decryptedStreamBinWriter = new BinaryWriter(decryptedStream))
                        {

                            // Header
                            Console.WriteLine("Generating header keyblock table....");
                            Console.WriteLine("");

                            var headerSeedArray = new byte[] { 136, 86, 49, 149, 241, 163, 137, 87 };
                            var keyblocksTableHeader = Generators.GenerateKeyblocksTable(headerSeedArray, false);

                            Console.WriteLine("Decrypting header section....");
                            Console.WriteLine("");
                            Decryption.DecryptBlocks(keyblocksTableHeader, 4, 0, 0, inFileReader, decryptedStreamBinWriter, false);

                            var header = new ToolVariables.Header();

                            decryptedStreamBinReader.BaseStream.Position = 28;
                            header.HeaderCheckSum = decryptedStreamBinReader.ReadUInt32();

                            if (header.HeaderCheckSum != decryptedStreamBinReader.ComputeCheckSum(24 / 4, 0))
                            {
                                ExitType.Error.ExitProgram("Header section was not decrypted properly");
                            }

                            decryptedStreamBinReader.BaseStream.Position = 0;
                            header.SeedValue = decryptedStreamBinReader.ReadUInt64();

                            decryptedStreamBinReader.BaseStream.Position = 8;
                            header.LineCount = decryptedStreamBinReader.ReadUInt16();

                            decryptedStreamBinReader.BaseStream.Position = 11;
                            header.DcmpFlag = decryptedStreamBinReader.ReadByte();
                            header.DecryptedFooterTxtSize = decryptedStreamBinReader.ReadUInt32();

                            var isCompressed = true;
                            if (header.DcmpFlag == 0)
                            {
                                isCompressed = false;
                            }

                            Console.WriteLine("");
                            Console.WriteLine($"Line count: {header.LineCount}");
                            Console.WriteLine($"Decrypted bottom text Size: {header.DecryptedFooterTxtSize}");
                            Console.WriteLine($"Compression Flag: {isCompressed}");
                            Console.WriteLine("");
                            Console.WriteLine("");


                            // Body
                            Console.WriteLine("Generating body keyblock table....");
                            Console.WriteLine("");
                            var keyblocksTableBody = Generators.GenerateKeyblocksTable(BitConverter.GetBytes(header.SeedValue), false);

                            Console.WriteLine("Decrypting body section....");
                            Console.WriteLine("");

                            var decryptionBodySize = new FileInfo(inFile).Length - header.DecryptedFooterTxtSize - 32;
                            ((uint)decryptionBodySize).CryptoLengthCheck();

                            var blockCount = (uint)decryptionBodySize / 8;
                            Decryption.DecryptBlocks(keyblocksTableBody, blockCount, 32, 32, inFileReader, decryptedStreamBinWriter, false);

                            // Debugging purpose
                            //File.WriteAllBytes("DecryptedDataTest", decryptedStream.ToArray());

                            var bodyFooter = new ToolVariables.BodyFooter();
                            decryptedStreamBinReader.BaseStream.Position = decryptedStreamBinReader.BaseStream.Length - 4;
                            bodyFooter.CompressedDataCheckSum = decryptedStreamBinReader.ReadUInt32();

                            if (bodyFooter.CompressedDataCheckSum != decryptedStreamBinReader.ComputeCheckSum(((uint)decryptionBodySize - 8) / 4, 32))
                            {
                                ExitType.Error.ExitProgram("Body section was not decrypted properly");
                            }

                            // Lines
                            Console.WriteLine("");
                            Console.WriteLine("Extracting lines....");

                            LinesExtractor.ExtractLines(decryptedStream, isCompressed, (uint)decryptionBodySize, header.LineCount, outFile);

                            Console.WriteLine("");
                            ExitType.Success.ExitProgram($"Finished extracting text data to '{Path.GetFileName(outFile)}' file");
                        }
                    }
                }
            }
        }
    }
}
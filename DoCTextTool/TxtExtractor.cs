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

            var outFile = Path.Combine(Path.GetDirectoryName(inFile), $"{Path.GetFileNameWithoutExtension(inFile)}.txt");

            Console.WriteLine("Generating Keyblocks tables....");
            Console.WriteLine("");

            var headerSeedArray = new byte[] { 136, 86, 49, 149, 241, 163, 137, 87 };
            var bodySeedArray = new byte[] { 1, 0, 0, 0, 0, 0, 0, 0 };

            var keyblocksTableHeader = Generators.GenerateKeyblocksTable(headerSeedArray, false);
            var keyblocksTableBody = Generators.GenerateKeyblocksTable(bodySeedArray, false);


            using (var inFileReader = new BinaryReader(File.Open(inFile, FileMode.Open, FileAccess.Read)))
            {
                inFileReader.BaseStream.Length.HeaderLengthCheck();

                inFileReader.BaseStream.Position = 0;
                var headerValue = inFileReader.ReadUInt64();

                headerValue.HeaderValueCheck(false);

                using (var decryptedStream = new MemoryStream())
                {
                    using (var decryptedStreamBinReader = new BinaryReader(decryptedStream))
                    {
                        using (var decryptedStreamBinWriter = new BinaryWriter(decryptedStream))
                        {

                            // Header
                            Console.WriteLine("Decrypting header section....");
                            Console.WriteLine("");

                            Decryption.DecryptBlocks(keyblocksTableHeader, 4, 0, 0, inFileReader, decryptedStreamBinWriter, false);

                            var header = new FileStructs.Header();
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

                            Console.WriteLine($"Line count: {header.LineCount}");
                            Console.WriteLine($"Decrypted bottom text Size: {header.DecryptedFooterTxtSize}");
                            Console.WriteLine($"Compression Flag: {isCompressed}");


                            // Body
                            Console.WriteLine("");
                            Console.WriteLine("Decrypting body section....");

                            var decryptionBodySize = new FileInfo(inFile).Length - header.DecryptedFooterTxtSize - 32;
                            ((uint)decryptionBodySize).CryptoLengthCheck();

                            var blockCount = (uint)decryptionBodySize / 8;

                            Decryption.DecryptBlocks(keyblocksTableBody, blockCount, 32, 32, inFileReader, decryptedStreamBinWriter, false);

                            // Debugging purpose
                            //File.WriteAllBytes("DecryptedDataTest", decryptedStream.ToArray());


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
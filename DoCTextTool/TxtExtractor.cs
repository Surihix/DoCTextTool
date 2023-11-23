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

            using (var inFileStream = new FileStream(inFile, FileMode.Open, FileAccess.Read))
            {
                using (var inFileReader = new BinaryReader(inFileStream))
                {
                    inFileStream.Length.HeaderLengthCheck();

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

                                Decryption.DecryptSection(KeyArrays.KeyblocksHeader, 4, 0, 0, inFileReader, decryptedStreamBinWriter, false);

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
                                var blockCount = (uint)decryptionBodySize / 8;

                                // Check if the length of the
                                // body section to decrypt is
                                // valid
                                decryptionBodySize.LengthCheck();

                                Decryption.DecryptSection(KeyArrays.KeyBlocksMainBody, blockCount, 32, 32, inFileReader, decryptedStreamBinWriter, false);

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
}
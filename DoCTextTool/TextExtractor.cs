using DoCTextTool.CryptographyClasses;
using DoCTextTool.LineClasses;
using DoCTextTool.SupportClasses;
using System;
using System.IO;
using static DoCTextTool.SupportClasses.ToolHelpers;

namespace DoCTextTool
{
    internal class TextExtractor
    {
        public static void ExtractProcess(string inFile)
        {
            var outFile = Path.Combine(Path.GetDirectoryName(inFile), $"{Path.GetFileNameWithoutExtension(inFile)}.txt");

            using (var inFileStream = new FileStream(inFile, FileMode.Open, FileAccess.Read))
            {
                using (var inFileReader = new BinaryReader(inFileStream))
                {
                    // Check header
                    if (inFileStream.Length < 32)
                    {
                        ExitType.Error.ExitProgram("Header length is not valid");
                    }

                    inFileReader.BaseStream.Position = 0;
                    var headerValue = inFileReader.ReadUInt64();

                    // If its not encrypted
                    // header, then throw the
                    // appropriate exit message
                    if (headerValue != 10733845617377775685)
                    {
                        if (headerValue == 1)
                        {
                            ExitType.Error.ExitProgram("File is already decrypted. this may not be a valid Dirge Of Cerberus text file.");
                        }
                        else
                        {
                            ExitType.Error.ExitProgram("This is not a valid Dirge Of Cerberus text file.");
                        }
                    }

                    using (var decryptedStream = new MemoryStream())
                    {
                        using (var decryptedStreamBinReader = new BinaryReader(decryptedStream))
                        {
                            using (var decryptedStreamBinWriter = new BinaryWriter(decryptedStream))
                            {

                                // Header
                                Console.WriteLine("");
                                Console.WriteLine("Decrypting header section....");
                                Console.WriteLine("");

                                Decryption.DecryptSection(KeyArrays.KeyblocksHeader, 4, 0, 0, inFileReader, decryptedStreamBinWriter);

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
                                if (decryptionBodySize % 8 != 0)
                                {
                                    ExitType.Error.ExitProgram("Length of the body to decrypt is not valid");
                                }

                                Decryption.DecryptSection(KeyArrays.KeyBlocksMainBody, blockCount, 32, 32, inFileReader, decryptedStreamBinWriter);

                                // Debugging purpose
                                //File.WriteAllBytes("DecryptedDataTest", decryptedStream.ToArray());


                                // Lines
                                Console.WriteLine("");
                                Console.WriteLine("Extracting lines....");

                                LinesExtractor.ExtractLines(decryptedStream, isCompressed, (uint)decryptionBodySize, header.LineCount, outFile);

                                Console.WriteLine("");
                                ExitType.Success.ExitProgram("Finished extracting lines to text file");
                            }
                        }
                    }
                }
            }
        }
    }
}
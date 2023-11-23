using DoCTextTool.CryptographyClasses;
using System;
using System.IO;
using static DoCTextTool.SupportClasses.ToolHelpers;

namespace DoCTextTool
{
    internal class TextDecryptor
    {
        public static void DecryptProcess(string inFile)
        {
            Console.WriteLine($"Decrypting '{Path.GetFileName(inFile)}' data....");
            Console.WriteLine("");

            var outFile = Path.Combine(Path.GetDirectoryName(inFile), $"{Path.GetFileNameWithoutExtension(inFile)}.dec");

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
                            ExitType.Error.ExitProgram("File is already decrypted.");
                        }
                        else
                        {
                            ExitType.Error.ExitProgram("This is not a valid Dirge Of Cerberus text file.");
                        }
                    }

                    using (var decryptedStream = new MemoryStream())
                    {
                        using (var decryptedStreamBinWriter = new BinaryWriter(decryptedStream))
                        {
                            // Header
                            Decryption.DecryptSection(KeyArrays.KeyblocksHeader, 4, 0, 0, inFileReader, decryptedStreamBinWriter, true);
                            Console.WriteLine("");

                            using (var decryptedStreamBinReader = new BinaryReader(decryptedStream))
                            {
                                decryptedStreamBinReader.BaseStream.Position = 12;
                                uint decryptedFooterTxtSize = decryptedStreamBinReader.ReadUInt32();

                                // Body
                                var decryptionBodySize = new FileInfo(inFile).Length - decryptedFooterTxtSize - 32;
                                var blockCount = (uint)decryptionBodySize / 8;

                                // Check if the length of the
                                // body section to decrypt is
                                // valid
                                decryptionBodySize.LengthCheck();

                                Decryption.DecryptSection(KeyArrays.KeyBlocksMainBody, blockCount, 32, 32, inFileReader, decryptedStreamBinWriter, true);
                                Console.WriteLine("");

                                outFile.IfFileExistsDel();

                                using (var outFileStream = new FileStream(outFile, FileMode.Append, FileAccess.Write))
                                {
                                    decryptedStream.Seek(0, SeekOrigin.Begin);
                                    decryptedStream.CopyTo(outFileStream);

                                    inFileStream.Seek(decryptionBodySize + 32, SeekOrigin.Begin);
                                    inFileStream.CopyTo(outFileStream);
                                }
                            }
                        }
                    }
                }
            }

            ExitType.Success.ExitProgram($"Finished copying decrypted data to '{Path.GetFileName(outFile)}' file");
        }
    }
}
using DoCTextTool.CryptoClasses;
using System;
using System.IO;
using static DoCTextTool.SupportClasses.ToolHelpers;

namespace DoCTextTool
{
    internal class TxtDecryptor
    {
        public static void DecryptProcess(string inFile)
        {
            var inFileName = Path.GetFileName(inFile);
            var outFile = Path.Combine(Path.GetDirectoryName(inFile), $"{Path.GetFileNameWithoutExtension(inFile)}.dec");

            using (var inFileStream = new FileStream(inFile, FileMode.Open, FileAccess.Read))
            {
                using (var inFileReader = new BinaryReader(inFileStream))
                {
                    inFileStream.Length.HeaderLengthCheck();

                    inFileReader.BaseStream.Position = 0;
                    var headerValue = inFileReader.ReadUInt64();

                    headerValue.HeaderValueCheck(true);

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

                                decryptionBodySize.CryptoLengthCheck();

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

            File.Delete(inFile);
            File.Move(outFile, Path.Combine(Path.GetDirectoryName(outFile), inFileName));
            
            ExitType.Success.ExitProgram($"Finished decrypting file");
        }
    }
}
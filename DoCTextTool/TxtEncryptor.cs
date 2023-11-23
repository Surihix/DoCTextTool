using DoCTextTool.CryptoClasses;
using System;
using System.IO;
using static DoCTextTool.SupportClasses.ToolHelpers;

namespace DoCTextTool
{
    internal class TxtEncryptor
    {
        public static void EncryptProcess(string inFile)
        {
            var inFileName = Path.GetFileName(inFile);
            var outFile = Path.Combine(Path.GetDirectoryName(inFile), $"{Path.GetFileNameWithoutExtension(inFile)}.enc");

            using (var inFileStream = new FileStream(inFile, FileMode.Open, FileAccess.Read))
            {
                using (var inFileReader = new BinaryReader(inFileStream))
                {
                    inFileStream.Length.HeaderLengthCheck();

                    inFileReader.BaseStream.Position = 0;
                    var headerValue = inFileReader.ReadUInt64();

                    if (headerValue != 1)
                    {
                        ExitType.Error.ExitProgram("File is not decrypted or may not be a Dirge of Cerberus text bin file.");
                    }

                    using (var encryptedStream = new MemoryStream())
                    {
                        using (var encryptedStreamBinWriter = new BinaryWriter(encryptedStream))
                        {
                            Encryption.EncryptSection(KeyArrays.KeyblocksHeader, 4, 0, 0, inFileReader, encryptedStreamBinWriter, true);
                            Console.WriteLine("");

                            inFileReader.BaseStream.Position = 12;
                            var encryptionBodySize = new FileInfo(inFile).Length - inFileReader.ReadUInt32() - 32;
                            var blockCount = (uint)encryptionBodySize / 8;

                            encryptionBodySize.CryptoLengthCheck();

                            Encryption.EncryptSection(KeyArrays.KeyBlocksMainBody, blockCount, 32, 32, inFileReader, encryptedStreamBinWriter, true);
                            Console.WriteLine("");

                            outFile.IfFileExistsDel();

                            using (var outFileStream = new FileStream(outFile, FileMode.Append, FileAccess.Write))
                            {
                                encryptedStream.Seek(0, SeekOrigin.Begin);
                                encryptedStream.CopyTo(outFileStream);

                                inFileStream.Seek(encryptionBodySize + 32, SeekOrigin.Begin);
                                inFileStream.CopyTo(outFileStream);
                            }
                        }
                    }
                }
            }

            File.Delete(inFile);
            File.Move(outFile, Path.Combine(Path.GetDirectoryName(outFile), inFileName));
            
            ExitType.Success.ExitProgram($"Finished encrypting file");
        }
    }
}
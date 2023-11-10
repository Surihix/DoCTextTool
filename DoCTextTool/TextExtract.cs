using DoCTextTool.DecryptionClasses;
using DoCTextTool.LineClasses;
using DoCTextTool.SupportClasses;
using System;
using System.IO;
using static DoCTextTool.SupportClasses.CmnMethods;

namespace DoCTextTool
{
    internal class TextExtract
    {
        public static void ExtractProcess(string inFile)
        {
            var outFile = Path.Combine(Path.GetDirectoryName(inFile), $"{Path.GetFileNameWithoutExtension(inFile)}.txt");

            using (var inFileStream = new FileStream(inFile, FileMode.Open, FileAccess.Read))
            {
                using (var inFileReader = new BinaryReader(inFileStream))
                {
                    // Perform checks to ensure that
                    // the length of the decryption
                    // body is valid and the read
                    // length is 32
                    inFileStream.Length.LengthCheck();
                    ((uint)inFileStream.Length).ReadLengthCheck();

                    // Check header
                    inFileReader.BaseStream.Position = 0;
                    var headerValue = inFileReader.ReadUInt64();

                    // If its not the encrypted
                    // header, then check if its
                    // the decrypted header
                    if (headerValue != 10733845617377775685)
                    {
                        if (headerValue != 1)
                        {
                            ExitType.Error.ExitProgram("This is not a valid Dirge Of Cerberus text file");
                        }
                    }

                    // Set a bool to decrypt
                    var isEncrypted = true;
                    if (headerValue == 1)
                    {
                        isEncrypted = false;
                    }

                    using (var decryptedStream = new MemoryStream())
                    {
                        using (var decryptedStreamReader = new BinaryReader(decryptedStream))
                        {
                            using (var decryptedStreamWriter = new BinaryWriter(decryptedStream))
                            {
                                // If not encrypted, copy
                                // the data directly to
                                // the decrypted stream
                                if (!isEncrypted)
                                {
                                    inFileStream.Seek(0, SeekOrigin.Begin);
                                    inFileStream.CopyTo(decryptedStream);
                                }

                                if (isEncrypted)
                                {
                                    // Header
                                    Decryption.DecryptSection(KeyArrays.KeyblocksHeader, 4, 0, 0, inFileReader, decryptedStreamWriter);
                                }

                                var header = new FileStructs.Header();
                                decryptedStreamReader.BaseStream.Position = 8;
                                header.LineCount = decryptedStreamReader.ReadUInt16();

                                decryptedStreamReader.BaseStream.Position = 11;
                                header.DcmpFlag = decryptedStreamReader.ReadByte();
                                header.UnencTxtSize = decryptedStreamReader.ReadUInt32();

                                var isCompressed = true;
                                if (header.DcmpFlag == 0)
                                {
                                    isCompressed = false;
                                }

                                Console.WriteLine($"Line count: {header.LineCount}");
                                Console.WriteLine($"Unencrypted Text Size: {header.UnencTxtSize}");
                                Console.WriteLine($"Compression Flag: {isCompressed}");


                                // Body
                                Console.WriteLine("");
                                Console.WriteLine("Decrypting body section....");

                                var decryptionBodySize = new FileInfo(inFile).Length - header.UnencTxtSize - 32;
                                var blockCount = (uint)decryptionBodySize / 8;

                                // Check if the length of the
                                // body is valid
                                decryptionBodySize.LengthCheck();

                                if (isEncrypted)
                                {
                                    Decryption.DecryptSection(KeyArrays.KeyBlocksMainBody, blockCount, 32, 32, inFileReader, decryptedStreamWriter);
                                }

                                // Debugging purpose
                                //File.WriteAllBytes("DecryptedDataTest", decryptedStream.ToArray());


                                // Lines
                                Console.WriteLine("");
                                Console.WriteLine("Extracting lines....");

                                LinesParser.ExtractLines(decryptedStream, isCompressed, (uint)decryptionBodySize, header.LineCount, outFile);

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
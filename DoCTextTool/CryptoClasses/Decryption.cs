using System;
using System.IO;

namespace DoCTextTool.CryptoClasses
{
    internal class Decryption
    {
        public static void DecryptBlocks(byte[] keyblocksTable, uint blockCount, uint readPos, uint writePos, BinaryReader inFileReader, BinaryWriter decryptedStreamBinWriter, bool logDisplay)
        {
            uint blockCounter = 0;

            for (int i = 0; i < blockCount; i++)
            {
                // Setup BlockCounter according
                // to the currentBlockId and read
                // 8 bytes (a block) to decrypt
                var currentBlockId = blockCounter >> 3;

                inFileReader.BaseStream.Position = readPos;
                var currentBytes = inFileReader.ReadBytes(8);


                // Setup BlockCounter variables
                uint tableOffset = 0;
                CryptoBase.BlockCounterSetup(blockCounter, ref tableOffset);


                // Shift all of the byte
                // value 8 times and 
                // perform a XOR operation
                var decryptedByte1 = ((currentBlockId ^ 69) & 255) ^ currentBytes[0];
                decryptedByte1 = decryptedByte1.LoopAByte(keyblocksTable, tableOffset);

                var decryptedByte2 = (uint)currentBytes[0] ^ currentBytes[1];
                decryptedByte2 = decryptedByte2.LoopAByte(keyblocksTable, tableOffset);

                var decryptedByte3 = (uint)currentBytes[1] ^ currentBytes[2];
                decryptedByte3 = decryptedByte3.LoopAByte(keyblocksTable, tableOffset);

                var decryptedByte4 = (uint)currentBytes[2] ^ currentBytes[3];
                decryptedByte4 = decryptedByte4.LoopAByte(keyblocksTable, tableOffset);

                var decryptedByte5 = (uint)currentBytes[3] ^ currentBytes[4];
                decryptedByte5 = decryptedByte5.LoopAByte(keyblocksTable, tableOffset);

                var decryptedByte6 = (uint)currentBytes[4] ^ currentBytes[5];
                decryptedByte6 = decryptedByte6.LoopAByte(keyblocksTable, tableOffset);

                var decryptedByte7 = (uint)currentBytes[5] ^ currentBytes[6];
                decryptedByte7 = decryptedByte7.LoopAByte(keyblocksTable, tableOffset);

                var decryptedByte8 = (uint)currentBytes[6] ^ currentBytes[7];
                decryptedByte8 = decryptedByte8.LoopAByte(keyblocksTable, tableOffset);


                // Setup decrypted byte variables
                var decryptedBytesArray = new byte[] { (byte)decryptedByte5, (byte)decryptedByte6, (byte)decryptedByte7,
                    (byte)decryptedByte8, (byte)decryptedByte1, (byte)decryptedByte2, (byte)decryptedByte3, (byte)decryptedByte4 };

                var decryptedBytesHigherVal = BitConverter.ToUInt32(decryptedBytesArray, 0);
                var decryptedBytesLowerVal = BitConverter.ToUInt32(decryptedBytesArray, 4);


                // Setup keyblock variables
                uint keyblockLowerVal = 0;
                uint keyblockHigherVal = 0;
                CryptoBase.KeyblockSetup(keyblocksTable, tableOffset, ref keyblockLowerVal, ref keyblockHigherVal);


                // Setup SpecialKey variables
                uint carryFlag = 0;
                long specialKey1 = 0;
                long specialKey2 = 0;
                CryptoBase.SpecialKeySetup(ref carryFlag, ref specialKey1, ref specialKey2);


                // Process bytes with the SpecialKey
                // and keyblock variables
                long decryptedBytesLongLowerVal = decryptedBytesLowerVal;
                long decryptedBytesLongHigherVal = decryptedBytesHigherVal;

                if (decryptedBytesLongLowerVal < keyblockLowerVal)
                {
                    carryFlag = 1;
                }
                else
                {
                    carryFlag = 0;
                }

                decryptedBytesLongLowerVal -= keyblockLowerVal;
                decryptedBytesLongHigherVal -= keyblockHigherVal;
                decryptedBytesLongHigherVal -= carryFlag;

                decryptedBytesLongLowerVal ^= specialKey1;
                decryptedBytesLongHigherVal ^= specialKey2;

                decryptedBytesLongLowerVal ^= keyblockLowerVal;
                decryptedBytesLongHigherVal ^= keyblockHigherVal;


                // Store the bytes in a array
                // and write it to the stream
                var decryptedByteLowerArray = BitConverter.GetBytes((uint)decryptedBytesLongLowerVal);
                var decryptedByteHigherArray = BitConverter.GetBytes((uint)decryptedBytesLongHigherVal);

                decryptedStreamBinWriter.BaseStream.Position = writePos;
                decryptedStreamBinWriter.Write(decryptedByteHigherArray);

                decryptedStreamBinWriter.BaseStream.Position = writePos + 4;
                decryptedStreamBinWriter.Write(decryptedByteLowerArray);


                if (logDisplay)
                {
                    Console.Write($"Block: {i}  ");

                    Console.Write(decryptedByteHigherArray[0].ToString("X2") + " " +
                        decryptedByteHigherArray[1].ToString("X2") + " " + decryptedByteHigherArray[2].ToString("X2") + " " +
                        decryptedByteHigherArray[3].ToString("X2") + " ");

                    Console.WriteLine(decryptedByteLowerArray[0].ToString("X2") + " " +
                        decryptedByteLowerArray[1].ToString("X2") + " " + decryptedByteLowerArray[2].ToString("X2") + " " +
                        decryptedByteLowerArray[3].ToString("X2"));
                }


                // Move to next block
                blockCounter += 8;
                readPos += 8;
                writePos += 8;
            }
        }
    }
}
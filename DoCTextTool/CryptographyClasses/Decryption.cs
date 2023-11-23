using System;
using System.IO;

namespace DoCTextTool.CryptographyClasses
{
    internal class Decryption
    {
        public static void DecryptSection(byte[] currentKeyBlock, uint blockCount, int readPos, int writePos, BinaryReader inFileReader, BinaryWriter decryptedStreamBinWriter, bool logDisplay)
        {
            uint blockByteCounter = 0;

            for (int i = 0; i < blockCount; i++)
            {
                // Setup BlockByteCounter according
                // to the currentBlockId and read
                // 8 bytes (a block) to decrypt
                var currentBlockId = blockByteCounter >> 3;

                inFileReader.BaseStream.Position = readPos;
                var currentBytes = inFileReader.ReadBytes(8);


                // Setup BlockByteCounter variables
                uint keyBlockOffset = 0;
                uint blockByteCounter_Eval = 0;
                uint blockByteCounter_Fval = 0;
                CryptographyHelpers.BlockByteCounterSetup(blockByteCounter, ref keyBlockOffset, ref blockByteCounter_Eval, ref blockByteCounter_Fval);


                // Shift all of the byte
                // value 8 times and 
                // perform a XOR operation
                var decryptedByte1 = ((currentBlockId.XOR(69)) & 255).XOR(currentBytes[0]);
                decryptedByte1 = decryptedByte1.LoopAByte(currentKeyBlock, keyBlockOffset);

                var decryptedByte2 = ((uint)currentBytes[0]).XOR(currentBytes[1]);
                decryptedByte2 = decryptedByte2.LoopAByte(currentKeyBlock, keyBlockOffset);

                var decryptedByte3 = ((uint)currentBytes[1]).XOR(currentBytes[2]);
                decryptedByte3 = decryptedByte3.LoopAByte(currentKeyBlock, keyBlockOffset);

                var decryptedByte4 = ((uint)currentBytes[2]).XOR(currentBytes[3]);
                decryptedByte4 = decryptedByte4.LoopAByte(currentKeyBlock, keyBlockOffset);

                var decryptedByte5 = ((uint)currentBytes[3]).XOR(currentBytes[4]);
                decryptedByte5 = decryptedByte5.LoopAByte(currentKeyBlock, keyBlockOffset);

                var decryptedByte6 = ((uint)currentBytes[4]).XOR(currentBytes[5]);
                decryptedByte6 = decryptedByte6.LoopAByte(currentKeyBlock, keyBlockOffset);

                var decryptedByte7 = ((uint)currentBytes[5]).XOR(currentBytes[6]);
                decryptedByte7 = decryptedByte7.LoopAByte(currentKeyBlock, keyBlockOffset);

                var decryptedByte8 = ((uint)currentBytes[6]).XOR(currentBytes[7]);
                decryptedByte8 = decryptedByte8.LoopAByte(currentKeyBlock, keyBlockOffset);


                // Setup decrypted byte variables
                var decryptedBytes_LowerArray = new byte[] { (byte)decryptedByte4, (byte)decryptedByte3, (byte)decryptedByte2, (byte)decryptedByte1 };
                var decryptedBytesLowerVal = decryptedBytes_LowerArray.ArrayToUIntHexNum();

                var decryptedBytes_HigherArray = new byte[] { (byte)decryptedByte8, (byte)decryptedByte7, (byte)decryptedByte6, (byte)decryptedByte5 };
                var decryptedBytesHigherVal = decryptedBytes_HigherArray.ArrayToUIntHexNum();


                // Setup KeyBlock variables
                uint keyBlockActiveLowerValue = 0;
                uint keyBlockActiveHigherValue = 0;
                CryptographyHelpers.KeyBlockSetup(currentKeyBlock, keyBlockOffset, ref keyBlockActiveLowerValue, ref keyBlockActiveHigherValue);


                // Setup SpecialKey variables
                uint carryFlag = 0;
                long specialKey1 = 0;
                long specialKey2 = 0;
                CryptographyHelpers.SpecialKeySetup(ref carryFlag, blockByteCounter_Eval, blockByteCounter_Fval, ref specialKey1, ref specialKey2);


                // Process bytes with the SpecialKey
                // and Keyblock variables
                long decryptBytesLowerVal = decryptedBytesLowerVal;
                long decryptBytesHigherVal = decryptedBytesHigherVal;

                if (decryptBytesLowerVal < keyBlockActiveLowerValue)
                {
                    carryFlag = 1;
                }
                else
                {
                    carryFlag = 0;
                }

                decryptBytesLowerVal -= keyBlockActiveLowerValue;
                decryptBytesHigherVal -= keyBlockActiveHigherValue;
                decryptBytesHigherVal -= carryFlag;

                decryptBytesLowerVal ^= specialKey1;
                decryptBytesHigherVal ^= specialKey2;

                decryptBytesLowerVal ^= keyBlockActiveLowerValue;
                decryptBytesHigherVal ^= keyBlockActiveHigherValue;


                // Store the bytes in a array
                // and write it to the stream
                var decryptedByteLowerArray = decryptBytesLowerVal.LongHexToUIntHexArray();
                var decryptedByteHigherArray = decryptBytesHigherVal.LongHexToUIntHexArray();

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

                    Console.Write(decryptedByteLowerArray[0].ToString("X2") + " " +
                        decryptedByteLowerArray[1].ToString("X2") + " " + decryptedByteLowerArray[2].ToString("X2") + " " +
                        decryptedByteLowerArray[3].ToString("X2"));

                    Console.WriteLine("");
                }


                // Move to next block
                blockByteCounter += 8;
                readPos += 8;
                writePos += 8;
            }
        }
    }
}
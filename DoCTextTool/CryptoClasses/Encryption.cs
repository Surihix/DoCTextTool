using System;
using System.IO;

namespace DoCTextTool.CryptoClasses
{
    internal class Encryption
    {
        public static void EncryptSection(byte[] currentKeyBlock, uint blockCount, uint readPos, uint writePos, BinaryReader inFileReader, BinaryWriter encryptedStreamBinWriter, bool logDisplay)
        {
            uint blockByteCounter = 0;

            for (int i = 0; i < blockCount; i++)
            {
                var currentBlockId = blockByteCounter >> 3;

                inFileReader.BaseStream.Position = readPos;
                var bytesToEncrypt = inFileReader.ReadBytes(8);
                var bytesToEncryptLowerArray = new byte[] { bytesToEncrypt[7], bytesToEncrypt[6], bytesToEncrypt[5], bytesToEncrypt[4] };
                var bytesToEncryptHigherArray = new byte[] { bytesToEncrypt[3], bytesToEncrypt[2], bytesToEncrypt[1], bytesToEncrypt[0] };

                long bytesToEncryptLowerVal = bytesToEncryptLowerArray.ArrayToLongHexNum();
                long bytesToEncryptHigherVal = bytesToEncryptHigherArray.ArrayToLongHexNum();


                // Setup BlockByteCounter variables
                uint keyBlockOffset = 0;
                uint blockByteCounter_Eval = 0;
                uint blockByteCounter_Fval = 0;
                CryptoBase.BlockByteCounterSetup(blockByteCounter, ref keyBlockOffset, ref blockByteCounter_Eval, ref blockByteCounter_Fval);


                // Setup KeyBlock variables
                uint keyBlockActiveLowerValue = 0;
                uint keyBlockActiveHigherValue = 0;
                CryptoBase.KeyBlockSetup(currentKeyBlock, keyBlockOffset, ref keyBlockActiveLowerValue, ref keyBlockActiveHigherValue);


                // Setup SpecialKey variables
                uint carryFlag = 0;
                long specialKey1 = 0;
                long specialKey2 = 0;
                CryptoBase.SpecialKeySetup(ref carryFlag, blockByteCounter_Eval, blockByteCounter_Fval, ref specialKey1, ref specialKey2);


                // XOR the bytes to encrypt 
                // with the KeyBlock variables
                bytesToEncryptLowerVal ^= keyBlockActiveLowerValue;
                bytesToEncryptHigherVal ^= keyBlockActiveHigherValue;


                // XOR the bytes to encrypt 
                // with the SpecialKey
                // variables and increase the
                // bytes with the KeyBlock variables
                bytesToEncryptLowerVal ^= specialKey1;
                bytesToEncryptHigherVal ^= specialKey2;

                bytesToEncryptLowerVal += keyBlockActiveLowerValue;
                bytesToEncryptHigherVal += keyBlockActiveHigherValue;


                // Adjust the lower value bytes
                // to be a positive value in hex
                // and compare that adjusted value
                // to determine the carryFlag value.
                // After that increase the higher 
                // value bytes with the carryFlag value
                var bytesToEncLowerValFixedHex = bytesToEncryptLowerVal.ToString("X16");
                var bytesToEncLowerValFixedHexNum = bytesToEncLowerValFixedHex[8] + "" + bytesToEncLowerValFixedHex[9] +
                    "" + bytesToEncLowerValFixedHex[10] + "" + bytesToEncLowerValFixedHex[11] +
                    "" + bytesToEncLowerValFixedHex[12] + "" + bytesToEncLowerValFixedHex[13] +
                    "" + bytesToEncLowerValFixedHex[14] + "" + bytesToEncLowerValFixedHex[15];
                long bytesToEncryptLowerValFixed = Convert.ToInt64(bytesToEncLowerValFixedHexNum, 16);

                if (bytesToEncryptLowerValFixed < keyBlockActiveLowerValue)
                {
                    carryFlag = 1;
                }
                else
                {
                    carryFlag = 0;
                }

                bytesToEncryptHigherVal += carryFlag;


                // Arrange the bytes in such a way
                // that only the rightmost digits
                // are remaining
                var computedBytesHexHighVal = bytesToEncryptHigherVal.ToString("X16");
                var b1 = Convert.ToUInt32(computedBytesHexHighVal[14] + "" + computedBytesHexHighVal[15], 16);
                var b2 = Convert.ToUInt32(computedBytesHexHighVal[12] + "" + computedBytesHexHighVal[13], 16);
                var b3 = Convert.ToUInt32(computedBytesHexHighVal[10] + "" + computedBytesHexHighVal[11], 16);
                var b4 = Convert.ToUInt32(computedBytesHexHighVal[8] + "" + computedBytesHexHighVal[9], 16);

                var computedBytesHexLowVal = bytesToEncryptLowerVal.ToString("X16");
                var b5 = Convert.ToUInt32(computedBytesHexLowVal[14] + "" + computedBytesHexLowVal[15], 16);
                var b6 = Convert.ToUInt32(computedBytesHexLowVal[12] + "" + computedBytesHexLowVal[13], 16);
                var b7 = Convert.ToUInt32(computedBytesHexLowVal[10] + "" + computedBytesHexLowVal[11], 16);
                var b8 = Convert.ToUInt32(computedBytesHexLowVal[8] + "" + computedBytesHexLowVal[9], 16);

                var computedBytesArray = new byte[] { (byte)b5, (byte)b6, (byte)b7, (byte)b8,
                        (byte)b1, (byte)b2, (byte)b3, (byte)b4 };


                // Reverse shift all of
                // the byte value 8 times
                // and perform a XOR
                // operation
                var encryptedByte1 = computedBytesArray[0].LoopAByteReverse(currentKeyBlock, keyBlockOffset);
                encryptedByte1 = ((currentBlockId.XOR(69)) & 255).XOR(encryptedByte1);

                var encryptedByte2 = computedBytesArray[1].LoopAByteReverse(currentKeyBlock, keyBlockOffset);
                encryptedByte2 = encryptedByte1.XOR(encryptedByte2);

                var encryptedByte3 = computedBytesArray[2].LoopAByteReverse(currentKeyBlock, keyBlockOffset);
                encryptedByte3 = encryptedByte2.XOR(encryptedByte3);

                var encryptedByte4 = computedBytesArray[3].LoopAByteReverse(currentKeyBlock, keyBlockOffset);
                encryptedByte4 = encryptedByte3.XOR(encryptedByte4);

                var encryptedByte5 = computedBytesArray[4].LoopAByteReverse(currentKeyBlock, keyBlockOffset);
                encryptedByte5 = encryptedByte4.XOR(encryptedByte5);

                var encryptedByte6 = computedBytesArray[5].LoopAByteReverse(currentKeyBlock, keyBlockOffset);
                encryptedByte6 = encryptedByte5.XOR(encryptedByte6);

                var encryptedByte7 = computedBytesArray[6].LoopAByteReverse(currentKeyBlock, keyBlockOffset);
                encryptedByte7 = encryptedByte6.XOR(encryptedByte7);

                var encryptedByte8 = computedBytesArray[7].LoopAByteReverse(currentKeyBlock, keyBlockOffset);
                encryptedByte8 = encryptedByte7.XOR(encryptedByte8);


                // Store the bytes in a array
                // and write it to the stream
                var encryptedByteArray = new byte[] { (byte)encryptedByte1, (byte)encryptedByte2, (byte)encryptedByte3,
                    (byte)encryptedByte4, (byte)encryptedByte5, (byte)encryptedByte6, (byte)encryptedByte7, (byte)encryptedByte8 };

                encryptedStreamBinWriter.BaseStream.Position = writePos;
                encryptedStreamBinWriter.Write(encryptedByteArray);


                if (logDisplay)
                {
                    Console.Write($"Block: {i}  ");

                    Console.WriteLine(encryptedByteArray[0].ToString("X2") + " " + encryptedByteArray[1].ToString("X2") + " " +
                        encryptedByteArray[2].ToString("X2") + " " + encryptedByteArray[3].ToString("X2") + " " +
                        encryptedByteArray[4].ToString("X2") + " " + encryptedByteArray[5].ToString("X2") + " " +
                        encryptedByteArray[6].ToString("X2") + " " + encryptedByteArray[7].ToString("X2"));
                }


                // Move to next block
                readPos += 8;
                blockByteCounter += 8;
                writePos += 8;
            }
        }
    }
}
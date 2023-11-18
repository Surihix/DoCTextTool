using System;
using System.IO;

namespace DoCTextTool.EncryptionClasses
{
    internal class Encryption
    {
        public static void EncryptSection(byte[] currentKeyBlock, uint blockCount, uint readPos, uint writePos, BinaryReader inFileReader, BinaryWriter encryptedStreamBinWriter)
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


                var blockByteCounterHex = blockByteCounter.ToString("X8");
                var hex1 = Convert.ToUInt32(blockByteCounterHex[6] + "" + blockByteCounterHex[7], 16);
                var hex2 = Convert.ToUInt32(blockByteCounterHex[4] + "" + blockByteCounterHex[5], 16);
                var hex3 = Convert.ToUInt32(blockByteCounterHex[2] + "" + blockByteCounterHex[3], 16);
                var hex4 = Convert.ToUInt32(blockByteCounterHex[0] + "" + blockByteCounterHex[1], 16);
                var blockByteCounterArray = new byte[4] { (byte)hex1, (byte)hex2, (byte)hex3, (byte)hex4 };
                var blockByteCounterLowerArray = new byte[] { blockByteCounterArray[1], blockByteCounterArray[0] };

                var blockByteCounterLowerMostHex = blockByteCounterLowerArray[0].ToString("X2") +
                    "" + blockByteCounterLowerArray[1].ToString("X2");
                var blockByteCounterLowerMost = Convert.ToUInt16(blockByteCounterLowerMostHex, 16);

                blockByteCounterLowerMost >>= 3;
                blockByteCounterLowerMost <<= 3;
                var keyBlockOffset = (uint)blockByteCounterLowerMost & 248;


                var keyBlockActiveLowerArray = new byte[] { currentKeyBlock[keyBlockOffset + 3],
                            currentKeyBlock[keyBlockOffset + 2], currentKeyBlock[keyBlockOffset + 1],
                            currentKeyBlock[keyBlockOffset + 0] };

                var keyBlockActiveHigherArray = new byte[] { currentKeyBlock[keyBlockOffset + 7],
                            currentKeyBlock[keyBlockOffset + 6], currentKeyBlock[keyBlockOffset + 5],
                            currentKeyBlock[keyBlockOffset + 4] };

                var keyBlockActiveLowerValue = keyBlockActiveLowerArray.ArrayToUIntHexNum();
                var keyBlockActiveHigherValue = keyBlockActiveHigherArray.ArrayToUIntHexNum();


                long blockByteCounter_AB_shiftVal = (long)blockByteCounter << 10;
                long blockByteCounter_CD_shiftVal = (long)blockByteCounter << 20;
                long blockByteCounter_EF_shiftVal = (long)blockByteCounter << 30;

                var blockByteCounter_AB_array = blockByteCounter_AB_shiftVal.LongHexToArray();
                var blockByteCounter_A_array = new byte[] { blockByteCounter_AB_array[3], blockByteCounter_AB_array[2], blockByteCounter_AB_array[1], blockByteCounter_AB_array[0] };
                var blockByteCounter_B_array = new byte[] { blockByteCounter_AB_array[7], blockByteCounter_AB_array[6], blockByteCounter_AB_array[5], blockByteCounter_AB_array[4] };
                var blockByteCounter_Aval = blockByteCounter_A_array.ArrayToUIntHexNum();
                var blockByteCounter_Bval = blockByteCounter_B_array.ArrayToUIntHexNum();

                var blockByteCounter_CD_array = blockByteCounter_CD_shiftVal.LongHexToArray();
                var blockByteCounter_C_array = new byte[] { blockByteCounter_CD_array[3], blockByteCounter_CD_array[2], blockByteCounter_CD_array[1], blockByteCounter_CD_array[0] };
                var blockByteCounter_D_array = new byte[] { blockByteCounter_CD_array[7], blockByteCounter_CD_array[6], blockByteCounter_CD_array[5], blockByteCounter_CD_array[4] };
                var blockByteCounter_Cval = blockByteCounter_C_array.ArrayToUIntHexNum();
                var blockByteCounter_Dval = blockByteCounter_D_array.ArrayToUIntHexNum();

                blockByteCounter_Cval |= blockByteCounter_Aval;
                blockByteCounter_Dval |= blockByteCounter_Bval;

                var blockByteCounter_EF_array = blockByteCounter_EF_shiftVal.LongHexToArray();
                var blockByteCounter_E_array = new byte[] { blockByteCounter_EF_array[3], blockByteCounter_EF_array[2], blockByteCounter_EF_array[1], blockByteCounter_EF_array[0] };
                var blockByteCounter_F_array = new byte[] { blockByteCounter_EF_array[7], blockByteCounter_EF_array[6], blockByteCounter_EF_array[5], blockByteCounter_EF_array[4] };
                var blockByteCounter_Eval = blockByteCounter_E_array.ArrayToUIntHexNum();
                var blockByteCounter_Fval = blockByteCounter_F_array.ArrayToUIntHexNum();

                blockByteCounter_Eval |= blockByteCounter;
                blockByteCounter_Fval |= 0;

                blockByteCounter_Eval |= blockByteCounter_Cval;
                blockByteCounter_Fval |= blockByteCounter_Dval;


                uint carryFlag;
                if (blockByteCounter_Eval > 1587207352)
                {
                    carryFlag = 1;
                }
                else
                {
                    carryFlag = 0;
                }

                long specialKey1 = (long)blockByteCounter_Eval + 2707759943;
                long specialKey2 = (long)blockByteCounter_Fval + carryFlag;


                // Step 1
                bytesToEncryptLowerVal ^= keyBlockActiveLowerValue;
                bytesToEncryptHigherVal ^= keyBlockActiveHigherValue;


                // Step 2
                bytesToEncryptLowerVal ^= specialKey1;
                bytesToEncryptHigherVal ^= specialKey2;

                bytesToEncryptLowerVal += keyBlockActiveLowerValue;
                bytesToEncryptHigherVal += keyBlockActiveHigherValue;


                // Step 3
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


                // Step 4
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


                // Step 5
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


                // Final step
                var encryptedByteArray = new byte[] { (byte)encryptedByte1, (byte)encryptedByte2, (byte)encryptedByte3,
                    (byte)encryptedByte4, (byte)encryptedByte5, (byte)encryptedByte6, (byte)encryptedByte7, (byte)encryptedByte8 };

                encryptedStreamBinWriter.BaseStream.Position = writePos;
                encryptedStreamBinWriter.Write(encryptedByteArray);


                // Debugging purpose
                //Console.Write($"Block: {i}  ");

                //Console.WriteLine(encryptedByte1.ToString("X2") + " " + encryptedByte2.ToString("X2") + " " +
                //    encryptedByte3.ToString("X2") + " " + encryptedByte4.ToString("X2") + " " +
                //    encryptedByte5.ToString("X2") + " " + encryptedByte6.ToString("X2") + " " +
                //    encryptedByte7.ToString("X2") + " " + encryptedByte8.ToString("X2"));
                //Console.WriteLine("");


                // Move to next block
                readPos += 8;
                blockByteCounter += 8;
                writePos += 8;
            }
        }
    }
}
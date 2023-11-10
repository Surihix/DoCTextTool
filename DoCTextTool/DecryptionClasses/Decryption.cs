using System;
using System.IO;

namespace DoCTextTool.DecryptionClasses
{
    internal class Decryption
    {
        public static void DecryptSection(byte[] currentKeyBlock, uint blockCount, int readPos, int writePos, BinaryReader inFileReader, BinaryWriter decryptedStreamWriter)
        {
            uint blockByteCounter = 0;

            for (var i = 0; i < blockCount; i++)
            {
                var currentBlockId = blockByteCounter >> 3;

                inFileReader.BaseStream.Position = readPos;
                var currentBytes = inFileReader.ReadBytes(8);

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


                // Shift all of the byte
                // value 8 times
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


                // Get decrypted byte
                // values
                var decryptedBytes_LowerArray = new byte[] { (byte)decryptedByte4, (byte)decryptedByte3, (byte)decryptedByte2, (byte)decryptedByte1 };
                var decryptedBytesLowerVal = decryptedBytes_LowerArray.ArrayToUIntHexNum();

                var decryptedBytes_HigherArray = new byte[] { (byte)decryptedByte8, (byte)decryptedByte7, (byte)decryptedByte6, (byte)decryptedByte5 };
                var decryptedBytesHigherVal = decryptedBytes_HigherArray.ArrayToUIntHexNum();


                // Setup blockByteCounter
                // variables
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


                // Special key part
                long decryptBytesLowerVal = decryptedBytesLowerVal;
                long decryptBytesHigherVal = decryptedBytesHigherVal;

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

                var keyBlockActiveLowerArray = new byte[] { currentKeyBlock[keyBlockOffset + 3],
                            currentKeyBlock[keyBlockOffset + 2], currentKeyBlock[keyBlockOffset + 1],
                            currentKeyBlock[keyBlockOffset + 0] };
                var keyBlockActiveHigherArray = new byte[] { currentKeyBlock[keyBlockOffset + 7],
                            currentKeyBlock[keyBlockOffset + 6], currentKeyBlock[keyBlockOffset + 5],
                            currentKeyBlock[keyBlockOffset + 4] };

                var keyBlockActiveLowerValue = keyBlockActiveLowerArray.ArrayToUIntHexNum();
                var keyBlockActiveHigherValue = keyBlockActiveHigherArray.ArrayToUIntHexNum();

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


                // Final steps
                decryptBytesLowerVal ^= keyBlockActiveLowerValue;
                decryptBytesHigherVal ^= keyBlockActiveHigherValue;

                var decryptedByteLowerArray = decryptBytesLowerVal.LongHexToUIntHexArray();
                var decryptedByteHigherArray = decryptBytesHigherVal.LongHexToUIntHexArray();

                decryptedStreamWriter.BaseStream.Position = writePos;
                decryptedStreamWriter.Write(decryptedByteHigherArray);

                decryptedStreamWriter.BaseStream.Position = writePos + 4;
                decryptedStreamWriter.Write(decryptedByteLowerArray);


                // Debugging purpose
                //Console.Write($"Block: {i}  ");

                //Console.Write(decryptedByteHigherArray[0].ToString("X2") + " " +
                //    decryptedByteHigherArray[1].ToString("X2") + " " + decryptedByteHigherArray[2].ToString("X2") + " " +
                //    decryptedByteHigherArray[3].ToString("X2") + " ");

                //Console.Write(decryptedByteLowerArray[0].ToString("X2") + " " +
                //    decryptedByteLowerArray[1].ToString("X2") + " " + decryptedByteLowerArray[2].ToString("X2") + " " +
                //    decryptedByteLowerArray[3].ToString("X2"));

                //Console.WriteLine("");


                // Move to next block
                blockByteCounter += 8;
                readPos += 8;
                writePos += 8;
            }
        }
    }
}
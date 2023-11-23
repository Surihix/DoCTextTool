using System;

namespace DoCTextTool.CryptoClasses
{
    internal class CryptoBase
    {
        public static void BlockByteCounterSetup(uint blockByteCounter, ref uint keyBlockOffset, 
            ref uint blockByteCounter_Eval, ref uint blockByteCounter_Fval)
        {
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
            keyBlockOffset = (uint)blockByteCounterLowerMost & 248;

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
            
            blockByteCounter_Eval = blockByteCounter_E_array.ArrayToUIntHexNum();
            blockByteCounter_Fval = blockByteCounter_F_array.ArrayToUIntHexNum();

            blockByteCounter_Eval |= blockByteCounter;
            blockByteCounter_Fval |= 0;

            blockByteCounter_Eval |= blockByteCounter_Cval;
            blockByteCounter_Fval |= blockByteCounter_Dval;
        }


        public static void KeyBlockSetup(byte[] currentKeyBlock, uint keyBlockOffset, ref uint keyBlockActiveLowerValue, 
            ref uint keyBlockActiveHigherValue)
        {
            var keyBlockActiveLowerArray = new byte[] { currentKeyBlock[keyBlockOffset + 3],
                            currentKeyBlock[keyBlockOffset + 2], currentKeyBlock[keyBlockOffset + 1],
                            currentKeyBlock[keyBlockOffset + 0] };

            var keyBlockActiveHigherArray = new byte[] { currentKeyBlock[keyBlockOffset + 7],
                            currentKeyBlock[keyBlockOffset + 6], currentKeyBlock[keyBlockOffset + 5],
                            currentKeyBlock[keyBlockOffset + 4] };

            keyBlockActiveLowerValue = keyBlockActiveLowerArray.ArrayToUIntHexNum();
            keyBlockActiveHigherValue = keyBlockActiveHigherArray.ArrayToUIntHexNum();
        }


        public static void SpecialKeySetup(ref uint carryFlag, uint blockByteCounter_Eval, uint blockByteCounter_Fval, 
            ref long specialKey1, ref long specialKey2)
        {
            if (blockByteCounter_Eval > 1587207352)
            {
                carryFlag = 1;
            }
            else
            {
                carryFlag = 0;
            }

            specialKey1 = (long)blockByteCounter_Eval + 2707759943;
            specialKey2 = (long)blockByteCounter_Fval + carryFlag;
        }        
    }
}
using System;

namespace DoCTextTool.CryptoClasses
{
    internal class CryptoBase
    {
        static uint BlockCounterEval { get; set; }

        static uint BlockCounterFval { get; set; }

        public static void BlockCounterSetup(uint blockCounter, ref uint tableOffset)
        {
            var blockCounterLowerMost = (ushort)(blockCounter & 0xFFFF);

            blockCounterLowerMost >>= 3;
            blockCounterLowerMost <<= 3;
            tableOffset = (uint)blockCounterLowerMost & 0xF8;

            long blockCounterABshiftVal = (long)blockCounter << 10;
            long blockCounterCDshiftVal = (long)blockCounter << 20;
            long blockCounterEFshiftVal = (long)blockCounter << 30;

            var blockCounterAval = (uint)(blockCounterABshiftVal & 0xFFFFFFFF);
            var blockCounterBval = (uint)(blockCounterABshiftVal >> 32);

            var blockCounterCval = (uint)(blockCounterCDshiftVal & 0xFFFFFFFF);
            var blockCounterDval = (uint)(blockCounterCDshiftVal >> 32);

            blockCounterCval |= blockCounterAval;
            blockCounterDval |= blockCounterBval;

            BlockCounterEval = (uint)(blockCounterEFshiftVal & 0xFFFFFFFF);
            BlockCounterFval = (uint)(blockCounterEFshiftVal >> 32);

            BlockCounterEval |= blockCounter;
            BlockCounterFval |= 0;

            BlockCounterEval |= blockCounterCval;
            BlockCounterFval |= blockCounterDval;
        }

        public static void KeyblockSetup(byte[] keyblocksTable, uint tableOffset, ref uint keyblockLowerVal, ref uint keyblockHigherVal)
        {
            var currentKeyblock = new byte[] { keyblocksTable[tableOffset + 0], keyblocksTable[tableOffset + 1],
                keyblocksTable[tableOffset + 2], keyblocksTable[tableOffset + 3], keyblocksTable[tableOffset + 4],
                keyblocksTable[tableOffset + 5], keyblocksTable[tableOffset + 6], keyblocksTable[tableOffset + 7] };

            keyblockLowerVal = BitConverter.ToUInt32(currentKeyblock, 0);
            keyblockHigherVal = BitConverter.ToUInt32(currentKeyblock, 4);
        }

        public static void SpecialKeySetup(ref uint carryFlag, ref long specialKey1, ref long specialKey2)
        {
            if (BlockCounterEval > ~0xA1652347)
            {
                carryFlag = 1;
            }
            else
            {
                carryFlag = 0;
            }

            specialKey1 = (long)BlockCounterEval + 0xA1652347;
            specialKey2 = (long)BlockCounterFval + carryFlag;
        }
    }
}
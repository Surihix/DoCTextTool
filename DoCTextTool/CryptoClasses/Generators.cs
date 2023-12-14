using System;
using System.Linq;

namespace DoCTextTool.CryptoClasses
{
    internal class Generators
    {
        public static byte[] GenerateKeyblocksTable(byte[] seedArray, bool logDisplay)
        {
            var finalKeyblocksTable = new byte[256];

            var seedHalfA = BitConverter.ToUInt32(seedArray, 0);
            var seedHalfB = BitConverter.ToUInt32(seedArray, 4);

            seedHalfA = (seedHalfA << 0x08) | (seedHalfA >> 0x18);
            seedHalfB = (seedHalfB >> 0x10) | (seedHalfB << 0x10);

            var keyblock = BitConverter.GetBytes(seedHalfB).Concat(BitConverter.GetBytes(seedHalfA)).ToArray();
            keyblock[0] += 0x45;

            // Loop 1
            int i = 1;
            int tmp;
            while (i < 8)
            {
                tmp = keyblock[i] + 0xD4 + keyblock[i - 1];
                tmp ^= (keyblock[i - 1]) << 2;
                tmp ^= 0x45;

                keyblock[i] = (byte)tmp;
                i++;
            }

            Array.ConstrainedCopy(keyblock, 0, finalKeyblocksTable, 0, keyblock.Length);

            if (logDisplay)
            {
                Console.WriteLine($"Block 0: {keyblock[0]:X2} {keyblock[1]:X2} {keyblock[2]:X2} {keyblock[3]:X2} " +
                    $"{keyblock[4]:X2} {keyblock[5]:X2} {keyblock[6]:X2} {keyblock[7]:X2}");
            }

            // Loop 2
            i = 1;
            ulong previousKeyBlock = BitConverter.ToUInt64(keyblock, 0);
            ulong tmpKeyBlock;
            uint tmpBlockHalfA, tmpBlockHalfB, blockHalfA, blockHalfB, cf;
            var copyIndex = 8;

            while (i < 0x20)
            {
                tmpKeyBlock = previousKeyBlock << 2;

                tmpBlockHalfA = (uint)(tmpKeyBlock & 0xFFFFFFFF);
                tmpBlockHalfB = (uint)(tmpKeyBlock >> 32);

                blockHalfA = (uint)(previousKeyBlock & 0xFFFFFFFF);
                blockHalfB = (uint)(previousKeyBlock >> 32);

                if ((long)tmpBlockHalfA + blockHalfA > 0xFFFFFFFF)
                {
                    cf = 1;
                }
                else
                {
                    cf = 0;
                }

                tmpBlockHalfA += blockHalfA;
                tmpBlockHalfB += blockHalfB;
                tmpBlockHalfB += cf;

                keyblock = BitConverter.GetBytes(tmpBlockHalfA).Concat(BitConverter.GetBytes(tmpBlockHalfB)).ToArray();

                if (logDisplay)
                {
                    Console.WriteLine($"Block {i}: {keyblock[0]:X2} {keyblock[1]:X2} {keyblock[2]:X2} {keyblock[3]:X2} " +
                        $"{keyblock[4]:X2} {keyblock[5]:X2} {keyblock[6]:X2} {keyblock[7]:X2}");
                }
                Array.ConstrainedCopy(keyblock, 0, finalKeyblocksTable, copyIndex, keyblock.Length);

                previousKeyBlock = BitConverter.ToUInt64(keyblock, 0);
                i++;
                copyIndex += 8;
            }

            //File.WriteAllBytes("KeysDump", finalKeyblocksTable);

            return finalKeyblocksTable;
        }
    }
}
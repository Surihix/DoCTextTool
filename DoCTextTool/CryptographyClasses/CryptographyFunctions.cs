using System;
using static DoCTextTool.SupportClasses.ToolHelpers;

namespace DoCTextTool.CryptographyClasses
{
    internal static class CryptographyFunctions
    {
        public static void LengthCheck(this long decryptionBodySize)
        {
            var isValid = true;
            if (decryptionBodySize % 8 != 0)
            {
                isValid = false;
            }
            if (decryptionBodySize <= 0)
            {
                isValid = false;
            }
            if (!isValid)
            {
                ExitType.Error.ExitProgram("Length of the body to decrypt is not valid");
            }
        }


        public static void ReadLengthCheck(this uint readSize)
        {
            if (readSize < 32)
            {
                ExitType.Error.ExitProgram("Header length is too small for reading");
            }
        }


        public static uint XOR(this uint leftVal, uint rightVal)
        {
            var computedXOR = leftVal ^ rightVal;

            return Convert.ToUInt32(computedXOR.ToString("X2"), 16);
        }


        public static uint LoopAByte(this uint decryptedByte, byte[] currentKeyBlock, uint currentKeyBlockOffset)
        {
            var byteIterator = 0;

            while (byteIterator < 8)
            {
                int integerVal = KeyArrays.Integers[decryptedByte];

                var keyBlockByte = currentKeyBlock[currentKeyBlockOffset + byteIterator];
                var computedValue = integerVal - keyBlockByte;

                if (computedValue < 0)
                {
                    var computeValHex = computedValue.ToString("X");
                    decryptedByte = Convert.ToUInt32(computeValHex.Substring(computeValHex.Length - 2), 16);
                }
                else
                {
                    decryptedByte = (uint)computedValue;
                }

                byteIterator++;
            }

            return decryptedByte;
        }


        public static byte[] LongHexToArray(this long value)
        {
            var computedHex = value.ToString("X16");
            var b1 = Convert.ToUInt32(computedHex[14] + "" + computedHex[15], 16);
            var b2 = Convert.ToUInt32(computedHex[12] + "" + computedHex[13], 16);
            var b3 = Convert.ToUInt32(computedHex[10] + "" + computedHex[11], 16);
            var b4 = Convert.ToUInt32(computedHex[8] + "" + computedHex[9], 16);
            var b5 = Convert.ToUInt32(computedHex[6] + "" + computedHex[7], 16);
            var b6 = Convert.ToUInt32(computedHex[4] + "" + computedHex[5], 16);
            var b7 = Convert.ToUInt32(computedHex[2] + "" + computedHex[3], 16);
            var b8 = Convert.ToUInt32(computedHex[0] + "" + computedHex[1], 16);
            var hexNumArray = new byte[] { (byte)b1, (byte)b2, (byte)b3, (byte)b4, (byte)b5, (byte)b6, (byte)b7, (byte)b8 };

            return hexNumArray;
        }


        public static byte[] LongHexToUIntHexArray(this long value)
        {
            var computedLongHex = value.ToString("X16");
            var b1 = Convert.ToUInt32(computedLongHex[14] + "" + computedLongHex[15], 16);
            var b2 = Convert.ToUInt32(computedLongHex[12] + "" + computedLongHex[13], 16);
            var b3 = Convert.ToUInt32(computedLongHex[10] + "" + computedLongHex[11], 16);
            var b4 = Convert.ToUInt32(computedLongHex[8] + "" + computedLongHex[9], 16);
            var hexNumArray = new byte[] { (byte)b1, (byte)b2, (byte)b3, (byte)b4 };

            return hexNumArray;
        }


        public static uint ArrayToUIntHexNum(this byte[] byteArray)
        {
            var hexValue = byteArray[0].ToString("X2") + "" + byteArray[1].ToString("X2") + "" +
                byteArray[2].ToString("X2") + "" + byteArray[3].ToString("X2");

            return Convert.ToUInt32(hexValue, 16);
        }


        public static long ArrayToLongHexNum(this byte[] byteArray)
        {
            var hexValue = "FFFFFFFF";
            hexValue += byteArray[0].ToString("X2") + "" + byteArray[1].ToString("X2") + "" + byteArray[2].ToString("X2") +
                "" + byteArray[3].ToString("X2");

            return Convert.ToInt64(hexValue, 16);
        }


        public static uint LoopAByteReverse(this byte byteToEncrypt, byte[] currentKeyBlock, uint currentKeyBlockOffset)
        {
            var byteIterator = 7;

            while (byteIterator > -1)
            {
                var keyBlockByte = currentKeyBlock[currentKeyBlockOffset + byteIterator];
                var integerValUsed = keyBlockByte + byteToEncrypt;

                if (integerValUsed > 255)
                {
                    var negativeHexVal = "FFFFFF";
                    negativeHexVal += byteToEncrypt.ToString("X2");

                    integerValUsed = Convert.ToInt32(negativeHexVal, 16) + keyBlockByte;
                    byteToEncrypt = (byte)Array.IndexOf(KeyArrays.Integers, (byte)integerValUsed);
                }
                else
                {
                    byteToEncrypt = (byte)Array.IndexOf(KeyArrays.Integers, (byte)integerValUsed);
                }

                byteIterator--;
            }

            return byteToEncrypt;
        }
    }
}
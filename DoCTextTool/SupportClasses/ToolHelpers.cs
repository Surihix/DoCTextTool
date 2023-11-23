using System;
using System.Collections.Generic;
using System.IO;

namespace DoCTextTool.SupportClasses
{
    internal static class ToolHelpers
    {
        public static void ExitProgram(this ExitType typeCode, string exitMsg)
        {
            var exitType = "";
            switch (typeCode)
            {
                case ExitType.Error:
                    exitType = "Error";
                    break;

                case ExitType.Warning:
                    exitType = "Warning";
                    break;

                case ExitType.Success:
                    exitType = "Success";
                    break;
            }

            Console.WriteLine($"{exitType}: {exitMsg}");
            Console.ReadLine();
            Environment.Exit(0);
        }

        public enum ExitType
        {
            Error,
            Warning,
            Success
        }

        public static void IfFileExistsDel(this string fileToDelete)
        {
            if (File.Exists(fileToDelete))
            {
                File.Delete(fileToDelete);
            }
        }

        public static void HeaderLengthCheck(this long headerLength)
        {
            if (headerLength < 32)
            {
                ExitType.Error.ExitProgram("Header length is not valid");
            }
        }

        public static void HeaderValueCheck(this ulong headerValue, bool isCryptProcess)
        {
            if (headerValue != 10733845617377775685)
            {
                if (headerValue == 1)
                {
                    switch (isCryptProcess)
                    {
                        case true:
                            ExitType.Error.ExitProgram("File is already decrypted.");
                            break;

                        case false:
                            ExitType.Error.ExitProgram("File is already decrypted. Ensure that the text bin file is encrypted before extracting it.");
                            break;
                    }         
                }
                else
                {
                    ExitType.Error.ExitProgram("This is not a valid Dirge Of Cerberus text file.");
                }
            }
        }

        public static void ExCopyTo(this Stream inStream, Stream outStream, long size)
        {
            int bufferSize = 81920;
            long amountRemaining = size;

            while (amountRemaining > 0)
            {
                long arraySize = Math.Min(bufferSize, amountRemaining);
                byte[] copyArray = new byte[arraySize];

                _ = inStream.Read(copyArray, 0, (int)arraySize);
                outStream.Write(copyArray, 0, (int)arraySize);

                amountRemaining -= arraySize;
            }
        }

        public static List<byte> ReadBytesTillNull(this BinaryReader reader)
        {
            var byteList = new List<byte>();
            byte currentValue;
            while ((currentValue = reader.ReadByte()) != default)
            {
                byteList.Add(currentValue);
            }

            return byteList;
        }

        public static void InsertEmptyBytes(this Stream inStream, long padAmount)
        {
            for (int p = 0; p < padAmount; p++)
            {
                inStream.WriteByte(0);
            }
        }

        public static long CheckDivisibility(this long valueToCheck, int divisibilityValue)
        {
            long padNulls = 0;

            if (valueToCheck % divisibilityValue != 0)
            {
                var remainder = valueToCheck % divisibilityValue;
                var increaseBytes = divisibilityValue - remainder;
                var newPos = valueToCheck + increaseBytes;
                padNulls = newPos - valueToCheck;
            }
            return padNulls;
        }
    }
}
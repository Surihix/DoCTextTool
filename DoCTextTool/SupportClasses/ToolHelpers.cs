using System;
using System.Collections.Generic;
using System.IO;

namespace DoCTextTool.SupportClasses
{
    internal static class ToolHelpers
    {
        public static string ExampleMsg = "Examples:" +
                    "\nDoCTextTool.exe -d \"string_us.bin\"" + "\nDoCTextTool.exe -e \"string_us.dec\"" +
                    "\nDoCTextTool.exe -x \"string_us.bin\"" + "\nDoCTextTool.exe -c \"string_us.txt\"";

        public static string ActionSwitchesMsg = "Action Switches:" +
            "\n-d = to decrypt\n-e = to encrypt\n-x = to extract\n-c = to convert";

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

        public static void CopyInBuffers(this Stream inStream, Stream outStream, long size)
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
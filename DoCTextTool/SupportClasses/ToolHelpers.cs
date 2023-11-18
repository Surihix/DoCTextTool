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
    }
}
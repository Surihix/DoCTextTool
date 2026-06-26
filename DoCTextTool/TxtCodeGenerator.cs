using System;
using System.Text;
using static DoCTextTool.SupportClasses.ToolHelpers;

namespace DoCTextTool
{
    internal class TxtCodeGenerator
    {
        public static void GenerateProcess(string inputStringID)
        {
            Console.WriteLine($"Generating code for '{inputStringID}'....");
            Console.WriteLine("");

            var stringID = inputStringID;
            var padding = stringID.Length % 4;

            if (padding != 0)
            {
                var sb = new StringBuilder();
                _ = sb.Append(stringID);

                for (int i = 0; i < 4 - padding; i++)
                {
                    _ = sb.Append("\0");
                }

                stringID = sb.ToString();
            }

            var stringIDbytes = Encoding.ASCII.GetBytes(stringID);

            var stringIDcodeBytes = new byte[4];
            var index = 0;

            for (int i = 0; i < 4; i++)
            {
                var rowValue = stringIDbytes[index];

                for (int j = index; j < stringIDbytes.Length; j += 4)
                {
                    if (j + 4 >= stringIDbytes.Length)
                    {
                        break;
                    }

                    rowValue ^= stringIDbytes[j + 4];
                }

                stringIDcodeBytes[i] = rowValue;
                index++;
            }

            ExitType.Success.ExitProgram($"{BitConverter.ToUInt32(stringIDcodeBytes, 0)}");
        }
    }
}
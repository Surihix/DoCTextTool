using DoCTextTool.SupportClasses;
using System;
using System.IO;
using static DoCTextTool.SupportClasses.ToolHelpers;

namespace DoCTextTool
{
    internal class Core
    {
        static void Main(string[] args)
        {
            var exampleMsgArray = new string[]
            {
                "Examples:", "DoCTextTool.exe -x \"string_us.bin\"", "DoCTextTool.exe -c \"string_us.txt\"", "",
                "Important:", "Change the filename mentioned in the example to the name or path of" +
                "\nthe file that you are trying to extract or convert.", ""
            };

            var actionSwitchesMsgArray = new string[]
            {
                "Action Switches:", "-x = To Extract", "-c = To Convert"
            };

            Console.WriteLine("");


            // Check launch arguments
            if (args.Length < 2)
            {
                ExitType.Warning.ExitProgram($"Enough arguments not specified\n\n{string.Join("\n", actionSwitchesMsgArray)}\n\n{string.Join("\n", exampleMsgArray)}");
            }

            var toolActionSwitch = new ActionSwitches();
            if (Enum.TryParse(args[0].Replace("-", ""), false, out ActionSwitches convertedActionSwitch))
            {
                toolActionSwitch = convertedActionSwitch;
            }
            else
            {
                ExitType.Error.ExitProgram($"Invalid or no action switch specified\n\n{string.Join("\n", actionSwitchesMsgArray)}");
            }

            if (!File.Exists(args[1]))
            {
                ExitType.Error.ExitProgram("Specified file is missing");
            }

            try
            {
                switch (toolActionSwitch)
                {
                    case ActionSwitches.x:
                        TxtExtractor.ExtractProcess(args[1]);
                        break;

                    case ActionSwitches.c:
                        TxtConverter.ConvertProcess(args[1]);
                        break;
                }
            }
            catch (Exception ex)
            {
                ExitType.Error.ExitProgram($"An Exception has occured\n{ex}");
            }
        }

        enum ActionSwitches
        {
            x,
            c
        }
    }
}
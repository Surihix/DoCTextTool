using DoCTextTool.DoCTextTool;
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
                "Examples:", "DoCTextTool.exe -x -lt \"string_us.bin\"", "DoCTextTool.exe -c -lt \"string_us.txt\"",
                "DoCTextTool.exe -b \"string_us.bin\"", "",
                "Important:", "Change the filename mentioned in the example to the name or path of" +
                "\nthe file that you are trying to extract or convert.", ""
            };

            var actionSwitchesMsgArray = new string[]
            {
                "Action Switches:", "-x = To Extract", "-c = To Convert", "-b = To Extract decompressed binary data",
                "-h or -? = To display tool instructions"
            };

            var encodingSwitchesMsgArray = new string[]
            {
                "Encoding Switches:", "-lt = For English and Latin text bin files", "-jp = For Japanese text bin files"
            };

            Console.WriteLine("");


            // Check launch arguments
            if (args.Length == 1)
            {
                if (args[0] == "-h" || args[0] == "-?")
                {
                    Console.WriteLine($"\n{string.Join("\n", actionSwitchesMsgArray)}\n\n{string.Join("\n", encodingSwitchesMsgArray)}\n\n{string.Join("\n", exampleMsgArray)}");
                    Console.ReadLine();
                    Environment.Exit(0);
                }
            }

            if (args.Length < 2)
            {
                ExitType.Warning.ExitProgram($"Enough arguments not specified\n\n{string.Join("\n", actionSwitchesMsgArray)}\n\n{string.Join("\n", encodingSwitchesMsgArray)}\n\n{string.Join("\n", exampleMsgArray)}");
            }

            var toolActionSwitch = new ActionSwitches();
            if (Enum.TryParse(args[0].Replace("-", ""), false, out ActionSwitches convertedActionSwitch))
            {
                toolActionSwitch = convertedActionSwitch;
            }
            else
            {
                ExitType.Error.ExitProgram($"Invalid action switch specified\n\n{string.Join("\n", actionSwitchesMsgArray)}");
            }

            // Determine the encoding switch
            // depending on the action switch
            var inFile = args[1];

            if (toolActionSwitch != ActionSwitches.b)
            {
                if (args.Length < 3)
                {
                    ExitType.Warning.ExitProgram("Enough arguments not specified for this tool action");
                }

                inFile = args[2];

                if (Enum.TryParse(args[1].Replace("-", ""), false, out ToolVariables.EncodingSwitches convertedEncodingSwitch))
                {
                    ToolVariables.TxtEncoding = convertedEncodingSwitch;
                }
                else
                {
                    ExitType.Error.ExitProgram($"Invalid encoding switch specified\n\n{string.Join("\n", encodingSwitchesMsgArray)}");
                }
            }

            if (!File.Exists(inFile))
            {
                ExitType.Error.ExitProgram("Specified file is missing");
            }

            try
            {
                switch (toolActionSwitch)
                {
                    case ActionSwitches.x:
                        TxtExtractor.ExtractProcess(inFile);
                        break;

                    case ActionSwitches.c:
                        TxtConverter.ConvertProcess(inFile);
                        break;

                    case ActionSwitches.b:
                        TxtDcmpBinary.BinaryProcess(inFile);
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
            b,
            c,
            x
        }
    }
}
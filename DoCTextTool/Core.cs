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
            Console.WriteLine("");

            if (args.Length < 2)
            {
                var warnMsg = "Enough arguments not specified\n\n";
                var warnMsgExample = "Examples:\nDoCTextTool.exe -e \"string_us.bin\"\nDoCTextTool.exe -c \"string_us.txt\"";
                ExitType.Warning.ExitProgram(warnMsg + warnMsgExample);
            }

            var toolActionSwitch = new ActionSwitches();
            if (Enum.TryParse(args[0].Replace("-", ""), false, out ActionSwitches convertedActionSwitch))
            {
                toolActionSwitch = convertedActionSwitch;
            }
            else
            {
                ExitType.Error.ExitProgram("Invalid action switch specified. Must be \"-e\" or \"-c\"");
            }

            try
            {
                switch (toolActionSwitch)
                {
                    case ActionSwitches.e:
                        if (!File.Exists(args[1]))
                        {
                            ExitType.Error.ExitProgram("Specified file is missing");
                        }

                        TextExtractor.ExtractProcess(args[1]);
                        break;

                    case ActionSwitches.c:
                        if (!File.Exists(args[1]))
                        {
                            ExitType.Error.ExitProgram("Specified file is missing");
                        }

                        TextConverter.ConvertProcess(args[1]);
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
            e,
            c
        }
    }
}
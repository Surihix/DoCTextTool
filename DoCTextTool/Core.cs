using DoCTextTool.DoCTextTool;
using DoCTextTool.SupportClasses;
using System;
using System.IO;
using System.Security.Cryptography;
using static DoCTextTool.SupportClasses.ToolHelpers;

namespace DoCTextTool
{
    internal class Core
    {
        static void Main(string[] args)
        {
            // Check DotNetZip dll file
            Console.WriteLine("Checking 'DotNetZip.dll' file....");

            if (!File.Exists("DotNetZip.dll"))
            {
                Console.WriteLine("");
                ExitType.Error.ExitProgram("Missing 'DotNetZip.dll' file present next to this program");
            }
            else
            {
                using (var dllStream = new FileStream("DotNetZip.dll", FileMode.Open, FileAccess.Read))
                {
                    using (var dllHash = SHA256.Create())
                    {
                        var hashArray = dllHash.ComputeHash(dllStream);
                        var hash = BitConverter.ToString(hashArray).Replace("-", "").ToLower();

                        if (!hash.Equals("8e9c0362e9bfb3c49af59e1b4d376d3e85b13aed0fbc3f5c0e1ebc99c07345f3"))
                        {
                            Console.WriteLine("");
                            ExitType.Error.ExitProgram("'DotNetZip.dll' file is corrupt.\nPlease ensure that this dll file present next to this program, is valid.");
                        }
                    }
                }
            }

            Console.Clear();
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
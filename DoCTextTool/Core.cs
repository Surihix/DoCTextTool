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

            var exampleMsgArray = new string[]
            {
                "Examples:", "DoCTextTool.exe -d \"string_us.bin\"", "DoCTextTool.exe -e \"string_us.bin\"", 
                "DoCTextTool.exe -x \"string_us.bin\"", "DoCTextTool.exe -c \"string_us.txt\""
            };

            var actionSwitchesMsgArray = new string[] 
            {
                "Action Switches:", "-d = To Decrypt", "-e = To Encrypt", "-x = To Extract", "-c = To Convert"
            };

            Console.Clear();
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
                    case ActionSwitches.d:
                        TxtDecryptor.DecryptProcess(args[1]);
                        break;

                    case ActionSwitches.e:
                        TxtEncryptor.EncryptProcess(args[1]);
                        break;

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
            d,
            e,
            x,
            c
        }
    }
}
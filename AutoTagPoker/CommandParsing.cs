using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AutoTagPoker.CommandExecution;

namespace AutoTagPoker
{
    internal class CommandParsing
    {
        public static List<Command> Commands = new List<Command>();
        public static void GetCommandsFromString(string consoleinput, string command)
        {
            Command com = new Command();
            List<string> args = new List<string>();
            int split = 0;
            string consoleArgs = "";
            string strcommand = command;
            if (string.IsNullOrEmpty(consoleinput) && string.IsNullOrEmpty(command)) return;

            if (consoleinput != "")
            {
                strcommand = consoleinput;
            }
            if (strcommand.Contains("//"))
            {
                ApplyGameMod(strcommand, true);
            }
            else
            {
                for (var i = 0; i < strcommand.Length; i++)
                {
                    string tmp = strcommand.ElementAt(i).ToString();
                    if (tmp == ".")
                    {                                             //------| Check through command string for the first "."
                        string test = strcommand.Split('.')[split];
                        if (split == 0)
                        {
                            com.TagName = test.Replace("/", "");  //------| Setting the string as tagtype
                        }
                        else if (split == 1)
                        {
                            com.TagType = test.Replace("/", "");  //------| Check through command string for the second "." set this as tagname
                        }
                        else if (split == 2 && strcommand.Contains("["))
                        {
                            string tmpArg = test.Replace("/", "") + strcommand.Split('.')[split + 1];
                            //Get final square bracket
                            int bracketindex = tmpArg.LastIndexOf(']');
                            tmpArg = tmpArg.Substring(0, bracketindex + 1);
                            //Parse arguments

                            string arg = "";
                            for (var j = 0; j < tmpArg.Length; j++)
                            {
                                if (tmpArg.ElementAt(j) == ']')
                                {
                                    //end of argument
                                    arg += tmpArg.ElementAt(j);
                                    args.Add(arg);
                                    consoleArgs += arg;
                                    arg = "";
                                }
                                else
                                {
                                    arg += tmpArg.ElementAt(j);
                                }
                            }
                            com.Args = args.ToArray();
                        }
                        else if (split == 2 && !strcommand.Contains("["))
                        {
                            string tmpArg = test;
                            //Get final square bracket
                            //Parse arguments
                            args.Add(tmpArg);
                            com.Args = args.ToArray();
                        }
                        split += 1;
                    }
                    if (tmp == "=")
                    {
                        break;
                    }
                }


                com.Method = strcommand.Split('.')[split].Split('=')[0];
                string finalLine = strcommand.Split('=')[1].Split('\n')[0];
                com.value = finalLine;
                Commands.Add(com);
                var currentcommand = Commands[Commands.Count - 1];
                Globals.CE.c = currentcommand;
                Globals.CE.TagBlockProcessing();
                string output =
                    "Tag Type: " + "\t" + com.TagType + "\n" +
                    "Tag Name: " + "\t" + com.TagName + "\n" +
                    "Tag Blocks: " + "\t" + consoleArgs + "\n" +
                    "Edit Value: " + "\t" + com.Method + "\n" +
                    "New value: " + "\t" + com.value;


                Console.WriteLine(output);
                Console.WriteLine(strcommand);
            }

        }
        public static void ApplyGameMod(string command, bool on)
        {
            string fullCommand = string.Empty;
            string prefix = string.Empty;
            if (command.Contains(" ")) // Example Enable://WireFrame true
            {
                prefix = command.Split(' ')[1];
                string[] commandParts = command.Split(' ');

                fullCommand = commandParts.Length >= 3 ? commandParts[2] : "";
                command = command.Split(' ')[0];
                if (prefix.ToLower() == "true") on = true;
                if (prefix.ToLower() == "false") on = false;
            }
            switch (command.ToLower())
            {
                case "//tp"://tp player KillSwitch:x:y:z or //tp player KillSwitch:location:location1fromjson
                    {
                        string arg = prefix.Split(" ")[0];
                        if (arg.ToLower() == "player")
                        {
                            string playerName = fullCommand.Split(":")[0];
                            float x = float.Parse(fullCommand.Split(":")[1]);
                            float y = float.Parse(fullCommand.Split(':')[2]);
                            float z = float.Parse(fullCommand.Split(':')[3]);

                            break;
                        }
                        else if (arg.ToLower() == "create")
                        {

                            break;
                        }
                        break;
                    }

            }
        }
    }
}

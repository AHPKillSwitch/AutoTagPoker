using System;
using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using static AutoTagPoker.MemoryAllocator;
using static AutoTagPoker.RuntimeMemory;

namespace AutoTagPoker
{
    public class Program
    {

        static async Task Main(string[] args)
        {
            //Console.InputEncoding = System.Text.Encoding.UTF8;
            Globals.SetGlobalVariables();
            Console.CancelKeyPress += Console_CancelKeyPress;
            string command;
            do
            {
                Console.WriteLine("Enter a command after map has loaded (or type 'exit' to quit): ");
                command = Console.ReadLine();
                if (command == "reload commands")
                {
                    Console.WriteLine("Reloading Commands");
                    await Task.Delay(1000);
                    Console.Clear();
                    Globals.pokedCommandCount = 0;
                    PullCommands.ReadAndApplyCommands();
                }


                // Process the command here...
                Console.WriteLine("You entered: " + command);

            } while (command != "exit");
            await Task.Delay(-1);
        }
        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            foreach (var aMem in Globals.assignedMemory)
            {
                VirtualFreeEx(Globals.myProcess.Handle, aMem, 0, FreeType.Release);
            }
            // Free the allocated memory

            Console.WriteLine("Memory freed. Exiting...");
        }
    }
    public static class Globals
    {

        public static Process myProcess;
        public static IntPtr[] assignedMemory = null;
        public static Dictionary<string, string> tagList = new Dictionary<string, string>();
        public static Dictionary<string, StringID> StringIDs = new Dictionary<string, StringID>();
        public static RuntimeMemory RM;
        public static int pokedCommandCount = 0;
        public static bool t_loaded = false;
        public static CommandExecution CE;
        public static string puginpath = "Plugins/";

        public static void SetGlobalVariables()
        {
            var procs = Process.GetProcessesByName("eldorado");
            if (procs.Length > 0)
            {
                myProcess = procs[0];
            }
            var tagsLoaded = GetTagStatus.TagStatus();
            TagListReader tagListReader = new TagListReader();
            tagList = tagListReader.ReadTagList();
        }

        public static StringID GetStringID(string sIDName)
        {
            try
            {
                StringID sID = StringIDs[sIDName];
                return sID;
            }
            catch
            {
                StringID nullTagData = new StringID("", "");
                return nullTagData;
            }
        }
    }

    public static class GetTagStatus
    {
        static bool tagscurrentlyloaded;
        public static async Task TagStatus()
        {
            await Task.Run(async () =>
            {
                string input = "Monitoring Tag State";
                int i = 0;
                while (true)
                {
                    await Task.Delay(500); // Adjust the delay as needed
                    Globals.t_loaded = await GetTagsLoadedStatus();
                    if (!Globals.t_loaded)
                    {
                        // Reset the input string
                        input = "Monitoring Tag State";

                        // Add dots to the input string
                        for (int dotCount = 0; dotCount < i % 4; dotCount++)
                        {
                            input += " .";
                        }

                        // Clear the current line and write the updated input string
                        Console.CursorLeft = 0;
                        Console.Write(input.PadRight(Console.WindowWidth)); // Ensure the line is fully cleared
                        Console.CursorLeft = 0;
                    }
                    i++;
                }


                static async Task<bool> GetTagsLoadedStatus()
                {

                    IntPtr intPtr = Addresses.GetIntPtrFromAddress(Addresses.mapsHeader + Offsets.scnr_Name);
                    bool tags_Loaded_Status = MemoryEditor.ReadString(Addresses.GetIntPtrFromAddress(Addresses.mapsHeader + Offsets.scnr_Name), 8) != "mainmenu";

                    if (!tagscurrentlyloaded && tags_Loaded_Status) //Check if tags are loaded if not load them
                    {
                        tagscurrentlyloaded = true;
                        await Task.Delay(800);
                        Globals.RM = new RuntimeMemory();
                        Globals.CE = new CommandExecution();
                        Console.WriteLine("Tags Loaded: Applying Commands...");
                        PullCommands.ReadAndApplyCommands();
                    }
                    else if (tagscurrentlyloaded && !tags_Loaded_Status)
                    {
                        tagscurrentlyloaded = false;
                        Console.Clear();
                    }
                    return tags_Loaded_Status;
                }

            });
        }
    }
}
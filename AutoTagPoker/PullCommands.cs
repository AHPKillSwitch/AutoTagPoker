using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTagPoker
{
    public static class PullCommands
    {
        public static void ReadAndApplyCommands()
        {
            string url = MemoryEditor.ReadString(Addresses.GetIntPtrFromAddress(Addresses.gameTypeDescription), 127);
            Console.WriteLine("GameType Description: " + url);
            //string url = "[autopoker]mongoose-race-battle-tag-pokes-12857436 trail=20";

            string consoleinput = "";
            if (url is null)
            {
                //ModsEnabled = false;
                //Main.SetPokingStatusLabel(ModsEnabled);
                //Main.PrintToConsole("-------------------- Mods Disabled --------------------" + "\n");
                //Main.PrintToConsole("Make sure the game type description contains TSI:PBLink" + "\n");
            }

            else if (url.Contains("[autopoker]"))
            {
                List<string> result = new List<string>();
                try
                {
                    string pbinurl1 = "https://commandlist.supportforum.co/post/" + url.Split("[autopoker]")[1];
                    result = PullCommandsFromURL(pbinurl1);
                    if (result.Count == 0)
                    {

                    }
                    //else PrintToConsole("GameType Mods Found. Applying... ");
                    foreach (string command in result)
                    {
                        try
                        {
                            CommandParsing.GetCommandsFromString(consoleinput, command);
                        }
                        catch
                        {
                            Console.WriteLine("Failed to apply command. " + command);
                        }
                    }
                }
                catch
                {
                    foreach (string line in System.IO.File.ReadLines("Content/Commands/" + url.Split(':')[1] + ".txt"))
                    {
                        string command = CleanString(line);
                        result.Add(command);
                    }
                    foreach (string command in result)
                    {
                        if (command.Contains("//"))
                        {
                            CommandParsing.ApplyGameMod(command, true);
                        }
                        else
                        {
                            CommandParsing.GetCommandsFromString("", command);
                        }
                    }
                    foreach (string command in result)
                    {
                        CommandParsing.GetCommandsFromString(consoleinput, command);
                    }
                }
            }

        }
        public static List<string> PullCommandsFromURL(string url)
        {
            url = url.Replace(" ", "?");

            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.3");
            var html = httpClient.GetStringAsync(url).Result;

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(html);
            var commandboxHtml = htmlDocument.DocumentNode.Descendants("span")
                .Where(node => node.GetAttributeValue("id", "")
                .Contains("post_message")).ToList();

            var commandlineList = commandboxHtml[0].Descendants("ol")
            .SelectMany(ol => ol.Descendants("li"))
            .ToList();
            string[] STRList = commandlineList.Select(s => CleanString(s.InnerHtml)).ToArray();
            STRList = STRList.Where(s => s != "").ToArray();
            httpClient.Dispose();
            return STRList.ToList();
        }
        public static string CleanString(string str)
        {
            if (str.Contains("/"))
            {
                return str.Replace("\r", "");
            }
            else
            {
                return str;
            }
        }
    }
}

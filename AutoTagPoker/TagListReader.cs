using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTagPoker
{
    public class TagListReader
    {
        public Dictionary<string, string> ReadTagList()
        {
            Dictionary<string, string> lookupDictionary = new Dictionary<string, string>();

            try
            {
                string[] lines = File.ReadAllLines("tag_list.csv");

                foreach (string line in lines)
                {
                    string[] parts = line.Split(','); // Assuming comma-separated, you can change the delimiter accordingly

                    if (parts.Length == 2)
                    {
                        string salt = parts[0].Trim();
                        string tagName = parts[1].Trim();



                        // Add to dictionary using salt as key
                        lookupDictionary[salt] = tagName;

                        // Add to dictionary using tag name as key
                        //lookupDictionary[tagName] = salt;
                    }
                    else
                    {
                        Console.WriteLine($"Invalid line format: {line}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while reading the file: {ex.Message}");
            }

            return lookupDictionary;
        }
    }
}

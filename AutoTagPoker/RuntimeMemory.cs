using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Linq;
using System.ComponentModel;
using System.Threading.Tasks;

namespace AutoTagPoker
{
    public class RuntimeMemory
    {
        public BackgroundWorker bGW = new BackgroundWorker();
        public TagHolder TH = new TagHolder();
        public StringID SI = new StringID();
        public static CommandExecution CE;
        public int tagsloadedstatus;
        string RawHex = string.Empty;
        public int tagCount;
        string tag_instances_address;
        public List<string> outPutStrings = new List<string>();

        public RuntimeMemory()
        {
            tagCount = MemoryEditor.ReadInt32((Addresses.GetIntPtrFromAddress(Addresses.g_cache_file_globals) + Offsets.tag_loaded_count));
            ParseTagInstanceList();
        }

        private bool ParseTagInstanceList()
        {
            try
            {

                TagHolder newHolder = new TagHolder();
                TH.tagTypeDict.Clear();
                for (var i = 0x0; i < tagCount; i++)
                {
                    IntPtr tagInstancesMemory = (IntPtr)MemoryEditor.ReadInt32(Addresses.GetIntPtrFromAddress(Addresses.g_cache_file_globals) + Offsets.tag_instances);
                    IntPtr tag_index_absolute_mapping_Address = (IntPtr)MemoryEditor.ReadInt32(Addresses.GetIntPtrFromAddress(Addresses.g_cache_file_globals) + Offsets.tag_index_absolute_mapping);

                    //Address Setting
                    int tagIndexInstance = MemoryEditor.ReadInt32((tag_index_absolute_mapping_Address + (i * 0x4)));
                    IntPtr tagAbsoluteIndexInstance = (IntPtr)(IntPtr)MemoryEditor.ReadInt32((tagInstancesMemory + (tagIndexInstance * 0x4)));

                    if ((uint)tagIndexInstance != 0xFFFFFFFF)
                    {
                        
                        //Get Group Name
                        string tagtype = MemoryEditor.ReadString((tagAbsoluteIndexInstance + Instance_Offsets.tagGroupName), 4);
                        byte[] bytes = Encoding.ASCII.GetBytes(tagtype);
                        Array.Reverse(bytes);
                        tagtype = Encoding.ASCII.GetString(bytes);
                        //Get Tag Name
                        string tagname = GetTagName(i);

                        TH.AddElement(tagtype, tagname, tagAbsoluteIndexInstance, tagAbsoluteIndexInstance, "0x" + i.ToString("X8"), "shared");
                    }
                }
                //TH.PrintTagTypes();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public int sharedTagAddressBase;
        public int loadedMapAddressBase;


        public class TagHolder
        {

            public TagHolder()
            {
            }

            // dict of Keys to Tag Types
            public Dictionary<string, TagType> tagTypeDict = new Dictionary<string, TagType>();

            public void GetChild(string tagTypeName, string tagName)
            {
                TagType tagType = tagTypeDict[tagTypeName];
                TagData tag = tagType.childTags[tagName];

                //tag.Print();
            }

            // for writing a new tag
            public void AddElement(string tagTypeName, string tagName, TagData tagData)
            {
                if (!tagTypeDict.ContainsKey(tagTypeName))
                {
                    tagTypeDict.Add(tagTypeName, new TagType(tagTypeName));
                }

                TagType tagType = tagTypeDict[tagTypeName];

                if (!tagType.childTags.ContainsKey(tagName))
                {
                    tagType.childTags.Add(tagName, tagData);
                }
                else
                {
                    tagType.childTags[tagTypeName] = tagData;
                }
            }

            public void AddElement(string tagTypeName, string tagName, IntPtr tag_runtime_address, IntPtr runtimeAddress, string datumAddress, string map)
            {
                TagData tagData = new TagData(tagName, tag_runtime_address, runtimeAddress, datumAddress, map);
                AddElement(tagTypeName, tagName, tagData);
            }
            public TagData GetTag(string tagTypeName, string tagName)
            {
                List<string> keys = new List<string>();
                
                if (tagName.Contains("?"))
                {
                    TagType sectetedtagType = tagTypeDict[tagTypeName];
                    foreach (KeyValuePair<string, TagData> typePair in sectetedtagType.childTags)
                    {
                        keys.Add(typePair.Key.ToString());
                    }
                    try
                    {
                        int i = 0;
                        if (tagName == "?+") i = keys.Count - 1;
                        else i = int.Parse(tagName.Split('?')[1]);
                        TagData tag = sectetedtagType.childTags[keys[i]];
                        return tag;
                    }
                    catch
                    {
                        TagData nullTagData = new TagData("null", (IntPtr)0, (IntPtr)0, "null", "null");
                        Console.WriteLine("Cound not find Key " + tagTypeName + "in Dictinary - GetPlayerColourTag");
                        return nullTagData;
                    }
                }
                foreach (KeyValuePair<string, TagType> typePair in tagTypeDict)
                {
                    TagType tagType = typePair.Value;

                    //tagType.PrintChildren();
                }
                try
                {
                    
                    TagType tagType = tagTypeDict[tagTypeName];
                    try
                    {
                        TagData tag = tagType.childTags[tagName];
                        return tag;
                    }
                    catch
                    {
                        TagData nullTagData = new TagData("null", (IntPtr)0, (IntPtr)0, "null", "null");
                        Console.WriteLine("Cound not find Key: " + tagName + "in Dictinary group: " + tagTypeName);
                        return nullTagData;
                    }
                }
                catch
                {
                    TagData nullTagData = new TagData("null", (IntPtr)0, (IntPtr)0, "null", "null");
                    Console.WriteLine("Cound not find Key " + tagTypeName + "in Dictinary");
                    return nullTagData;
                }
            }
            
            public TagData GetPlayerColourTag(string tagTypeName, int i)
            {
                List<string> keys = new List<string>();
                TagType tagType = tagTypeDict["cont"];
                //TagData tagData = tagType.childTags["cont"];
                foreach (KeyValuePair<string, TagData> typePair in tagType.childTags)
                {
                    keys.Add(typePair.Key.ToString());
                }
                try
                {
                    TagData tag = tagType.childTags[keys[i]];
                    return tag;
                }
                catch
                {
                    TagData nullTagData = new TagData("null", (IntPtr)0, (IntPtr)0, "null", "null");
                    Console.WriteLine("Cound not find Key " + tagTypeName + "in Dictinary - GetPlayerColourTag");
                    return nullTagData;
                }
            }


            public TagType GetTagType(string tagTypeName)
            {
                TagType tagType = tagTypeDict[tagTypeName];

                return tagType;
            }

            public void PrintTagTypes()
            {
                foreach (KeyValuePair<string, TagType> typePair in tagTypeDict)
                {
                    TagType tagType = typePair.Value;
                }
            }
            [System.Diagnostics.Conditional("DEBUG")]
            public void PrintTags()
            {
                foreach (KeyValuePair<string, TagType> typePair in tagTypeDict)
                {
                    TagType tagType = typePair.Value;

                    tagType.PrintChildren();
                }
            }
        }
        public class TagType
        {
            public string displayName = "Weapon";


            public void PrintChildren()
            {
                foreach (KeyValuePair<string, TagData> tagPair in childTags)
                {
                    tagPair.Value.Print();
                }
            }
            public Dictionary<string, TagData> childTags = new Dictionary<string, TagData>();

            public TagType(string displayName)
            {
                this.displayName = displayName;
            }
        }
        public struct TagData
        {
            public string tagname;
            public IntPtr runTimeAddress;
            public IntPtr runtimeValue;
            public string tagRef;
            public string map;

            public TagData(string tagname, IntPtr runTimeAddress, IntPtr runtimeValue, string datumAddress, string map)
            {
                this.tagname = tagname;
                this.runTimeAddress = runTimeAddress;
                this.runtimeValue = runtimeValue;
                this.tagRef = datumAddress;
                this.map = map;
            }

            public void Print()
            {
                Console.WriteLine("         Runtime Address:" + runTimeAddress);
            }


        }

        private string GetTagName(int index) // open .map file, read tagname string table, read tagname table. Dont read the whole file
        {
            string key = index.ToString("X");
            string tagNameForSalt = string.Empty;
            if (Globals.tagList.ContainsKey("0x" + index.ToString("X8")))
            {
                tagNameForSalt = Globals.tagList["0x" + index.ToString("X8")];
            }
            else
            {
                tagNameForSalt = "0x" + index.ToString("X8");
            }
            
            return tagNameForSalt;
        }
        public static bool CheckFileExists(string folderPath, string fileName)
        {
            string filePath = Path.Combine(folderPath, fileName);

            if (File.Exists(filePath))
            {
                // File exists
                return true;
            }
            else
            {
                // File does not exist
                return false;
            }
        }

        public struct StringID
        {
            public string stringID;
            public string index;

            public StringID(string stringID, string count)
            {
                this.stringID = stringID;
                this.index = count;
            }
        }

        public byte[] ReadBytesAtOffset(string filePath, long offset, int length)
        {
            byte[] test = new byte[length];
            try
            {
                using (BinaryReader reader = new BinaryReader(new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read)))
                {
                    reader.BaseStream.Seek(offset, SeekOrigin.Begin);
                    reader.Read(test, 0, length);
                }
            }
            catch
            {
                outPutStrings.Add("Cant find maps folder, Please check your maps path in the config file!");
            }
            return test;
        }
        public byte[] ReadBytesAtOffset(string filePath, long offset)
        {
            byte[] test = new byte[128];
            using (BinaryReader reader = new BinaryReader(new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read)))
            {
                reader.BaseStream.Seek(offset, SeekOrigin.Begin);
                reader.Read(test, 0, 128);
            }
            return test;
        }


        #region Byte Conversion
        // convert byte array from file to hex values
        public static string ConvertByteToHex(byte[] byteData)
        {
            string hexValues = BitConverter.ToString(byteData).Replace("-", "");

            return hexValues;
        }
        // convert hex values of file back to bytes
        public static byte[] ConvertHexToByteArray(string hexString)
        {
            byte[] byteArray = new byte[hexString.Length / 2];

            for (int index = 0; index < byteArray.Length; index++)
            {
                string byteValue = hexString.Substring(index * 2, 2);
                byteArray[index] = byte.Parse(byteValue, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            }

            return byteArray;
        }
        //convert hex to Unicode string
        public static string ConvertHexToUnicode(string Hex)
        {
            byte[] textBytes = ConvertHexToByteArray(Hex);
            Hex = System.Text.Encoding.UTF8.GetString(textBytes);
            return Hex;
        }
        //convert bytes to unicode string
        public static string ConvertHexToUnicode(byte[] Hex)
        {
            return System.Text.Encoding.UTF8.GetString(Hex);
        }
        public int ReadSizeFromHex(string hex)
        {
            byte[] tmp = ConvertHexToByteArray(hex);
            return BitConverter.ToInt32(tmp, 0);
        }
        public int ReadSizeFromHex(byte[] b)
        {
            //byte[] tmp = ConvertHexToByteArray(b);
            return BitConverter.ToInt32(b, 0);
        }
        public Int16 ReadSizeFrom2ByteHex(string hex)
        {
            byte[] tmp = ConvertHexToByteArray(hex);
            return BitConverter.ToInt16(tmp, 0);
        }
        public int ReadSizeFrom3ByteHex(string hex)
        {
            byte[] tmp = ConvertHexToByteArray(hex);
            int value = tmp[0] + (tmp[1] << 8) + (tmp[2] << 16);
            return value;
        }
        public int ReadSizeFrom1ByteHex(string hex)
        {
            byte[] tmp = ConvertHexToByteArray(hex);
            int value = tmp[0];
            return value;
        }
        #endregion

    }
}

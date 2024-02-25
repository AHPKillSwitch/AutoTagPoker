using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using static AutoTagPoker.RuntimeMemory;

namespace AutoTagPoker
{
    public class CommandExecution
    {
        List<string> valueTypes = new List<string> { "/ascii", "/point2", "/point3", "/degree", "/degree2", "/degree3", "/flags8", "/flags16", "/flags32", "/int8", "/int16", "/int32", "/enum8", "/enum16", "/enum32", "/tagRef", "/float32", "/rangef", "/ranged", "/vector3", "/vector4", "/stringId", "/colorf" };
        List<RuntimeMemory.TagData> AllTagData = new List<RuntimeMemory.TagData>();
        RuntimeMemory.TagData TD = new RuntimeMemory.TagData();
        XmlDocument doc;
        RuntimeMemory.TagHolder TH;
        RuntimeMemory.StringID stringID = new RuntimeMemory.StringID();
        public Command c;

        public CommandExecution()
        {
            TH = Globals.RM.TH;
            stringID = Globals.RM.SI;
        }

        public void TagBlockProcessing()
        {
            doc = new XmlDocument();
            doc.Load(Globals.puginpath + c.TagType + ".xml");
            if (c.value.Contains("?*"))
            {
                int i = int.Parse(c.value.Split('.')[1]);
                TD = TH.GetPlayerColourTag(c.TagType, i);
            }
            else if (c.TagName.Contains("?*"))
            {
                int i = int.Parse(c.TagName.Split('*')[1]);
                TD = TH.GetPlayerColourTag(c.TagType, i);
            }
            else
            {
                TD = TH.GetTag(c.TagType, c.TagName);
            }
            TD.ToString();
            if (TD.tagRef != "null") // tag not found in dict
            {
                Console.WriteLine("\n \n----------------- Processing Command " + Globals.pokedCommandCount.ToString() + " ----------------");
                CommandParsing(c.Args, c.Method, c.value);
                Globals.pokedCommandCount++;
            }
            else
            {
                ErrorOutput_TagNotFound(c.TagName, c.TagType);
            }
        }
        public int RunTimeAddress;
        public string offset;
        public string tagblock = "/reflexive";
        public string basexml;
        public int CopyPasteBlockData;
        public string method1;

        private void CommandParsing(string[] args, string method, string value)
        {
            basexml = "/plugin";
            RunTimeAddress = (int)TD.runTimeAddress;
            string test = TD.tagRef;
            if (args != null)
            {
                string argument1;
                for (int i = 0; i < args.Length; i++)
                {
                    if (i + 1 == args.Length) method1 = method;
                    else method1 = "blank";
                    string argument = args.ElementAt(i);
                    if (argument.Contains("["))
                    {
                        basexml = basexml + tagblock;
                        argument1 = argument.Replace("[", "").Replace("]", "");
                        RunTimeAddress = GetTagBlockData( basexml, argument1, method1, RunTimeAddress, value);
                    }
                }
                RunTimeAddress = GetTagData(basexml, method, value);
            }
            else
            {
                RunTimeAddress = GetTagData(basexml, method, value);
            }
        }
        public struct Command
        {
            //  /weap.objects\weapons\rifle\assault_rifle.[Barrels].[Firing_effect].Prediction Type=None|
            public string TagType;
            public string TagName;
            public string[] Args;
            public string Method;
            public string value;
        }


        private int GetTagBlockData(string xmlpath, string argument, string method1, int RunTimeAddress, string value)
        {
            XmlNodeList nodeList = doc.SelectNodes(xmlpath);
            for (var nc = 0; nc < nodeList.Count; nc++)
            {
                int blockOffset = MemoryEditor.ReadInt32((IntPtr)RunTimeAddress + Instance_Offsets.tagBlockOffset);
                string argument1 = argument.Split(':')[0];
                int CmdBlockIndex = int.Parse(argument.Split(':')[1]);
                if (nodeList[nc].Attributes[0].Value == argument1)
                {
                    offset = nodeList[nc].Attributes[1].InnerText; //get off set from xml
                    int offs = Convert.ToInt32(offset, 16); // convert to int
                    string BlockIndexSize = nodeList[nc].Attributes[3].InnerText; //get index size from xml
                    int Bindex = Convert.ToInt32(BlockIndexSize, 16);
                    if (method1 == "copy")
                    {
                        int addrToRead1 = (int)(RunTimeAddress + blockOffset + offs + 4); //
                        //string mm1 = addrToRead1.ToString("X");
                        CopyPasteBlockData = MemoryEditor.ReadInt32((IntPtr)addrToRead1);
                        Console.WriteLine("copied Data: " + CopyPasteBlockData.ToString() + "\n");

                    }
                    else if (method1 == "paste")
                    {
                        IntPtr addrToRead1 = (IntPtr)RunTimeAddress + blockOffset + offs + 4;
                        WriteInt32(addrToRead1, CopyPasteBlockData);
                        Console.WriteLine("Pasted Data: " + CopyPasteBlockData.ToString() + "\n");
                    }
                    else if (method1 == "delete" || method1 == "add" || method1 == "clear" || method1 == "set")
                    {
                        IntPtr addrToRead1 = (IntPtr)RunTimeAddress + blockOffset + offs;
                        int blockindexcount = MemoryEditor.ReadInt32((IntPtr)(RunTimeAddress + offs));
                        if (method1 == "add")
                        {
                            IntPtr allocatedMemory = MemoryAllocator.AllocateMemory(Globals.myProcess.Handle, Convert.ToUInt32(BlockIndexSize, 16));
                            blockindexcount += 1;
                            WriteInt32(addrToRead1, blockindexcount);
                            WriteInt32(addrToRead1 + 0x4, (int)allocatedMemory);
                            WriteInt32(addrToRead1 + 0x10, 0);
                            Console.WriteLine("Added +1 to tag index, Current index = : " + blockindexcount.ToString() + "\n");
                        }
                        if (method1 == "delete")
                        {
                            blockindexcount -= 1;
                            WriteInt32(addrToRead1, blockindexcount);
                            Console.WriteLine("Added -1 to tag index, Current index = : " + blockindexcount.ToString() + "\n");
                        }
                        if (method1 == "clear")
                        {
                            WriteInt32(addrToRead1,0);
                            Console.WriteLine("Tag Block Index`s Cleared " + "\n");
                        }
                        if (method1 == "set")
                        {
                            WriteInt32(addrToRead1, int.Parse(value));
                            Console.WriteLine("Tag Block Index`s Cleared " + "\n");
                        }
                    }
                    else
                    {
                        if (blockOffset == 0xFFFFFFFF) blockOffset = 0;
                        int addrToRead = RunTimeAddress + blockOffset + offs + 4; // runtime tag address + offset to Selected [block] + 4 to get to block address
                        string mm = addrToRead.ToString("X");
                        RunTimeAddress = MemoryEditor.ReadInt32((IntPtr)addrToRead);
                        Bindex = CmdBlockIndex * Bindex;
                        RunTimeAddress += Bindex;
                        string mk = RunTimeAddress.ToString("X");
                        return RunTimeAddress;
                    }
                }
            }
            Console.WriteLine("Command Error!!");
            return 0;
        }

        private int GetTagData(string xmlpath, string method, string value)
        {
            int RunTimeAddress1 = RunTimeAddress;
            foreach (string type in valueTypes)
            {
                XmlNodeList innerNodes = doc.SelectNodes(xmlpath + type);
                foreach (XmlNode _node in innerNodes)
                {

                    if (_node.Attributes[0].Value == method)
                    {
                        //in the right node
                        string hexOffset = _node.Attributes[1].Value;
                        int off = Convert.ToInt32(hexOffset, 16);
                        IntPtr Tagruntimeaddress = (IntPtr)RunTimeAddress + off;
                        string mk2 = Tagruntimeaddress.ToString("X");
                        //MessageBox.Show(node.InnerXml);
                        switch (type)
                        {
                            case "/ascii":
                                { // Code flow ----->
                                    WriteASCII(Tagruntimeaddress, value);
                                    break;
                                }
                            case "/rangef":
                                { // Code flow ----->
                                    float value1 = float.Parse(value.Split(':')[0]); WriteFloat(Tagruntimeaddress, value1);
                                    float value2 = float.Parse(value.Split(':')[1]); Tagruntimeaddress += 4; WriteFloat(Tagruntimeaddress, value2);
                                    break;
                                }
                            case "/ranged":
                                { // Code flow ----->
                                    float value1 = float.Parse(value.Split(':')[0]); WriteFloat(Tagruntimeaddress, value1);
                                    float value2 = float.Parse(value.Split(':')[1]); Tagruntimeaddress += 4; WriteFloat(Tagruntimeaddress, value2);
                                    break;
                                }
                            case "/colorf":
                                { // Code flow ----->
                                    IntPtr runtimeaddress = (IntPtr)Tagruntimeaddress;
                                    WriteFloat(runtimeaddress, GetColorF(value.Substring(0, 2))); //alpha
                                    WriteFloat((runtimeaddress += 0x4), GetColorF(value.Substring(2, 2))); //R
                                    WriteFloat((runtimeaddress += 0x4), GetColorF(value.Substring(4, 2))); //G
                                    WriteFloat((runtimeaddress += 0x4), GetColorF(value.Substring(6, 2))); //B
                                    break;
                                }
                            case "/tagRef":
                                {
                                    string tagname = string.Empty;
                                    string tagtype = string.Empty;
                                    if (value != "null")
                                    {

                                        if (c.value.Contains("?*"))
                                        {
                                            int i = int.Parse(c.value.Split('.')[1]);
                                            TD = TH.GetPlayerColourTag(c.TagType, i);
                                        }
                                        else if (c.TagName.Contains("?*"))
                                        {
                                            int i = int.Parse(c.TagName.Split('?')[1]);
                                            TD = TH.GetPlayerColourTag(c.TagType, i);
                                        }
                                        else
                                        {
                                            tagname = value.Split('.')[0];
                                            tagtype = value.Split('.')[1];
                                            TD = TH.GetTag(tagtype, tagname);
                                        }
                                        TD.ToString();
                                        string datumindex = TD.tagRef;
                                        if (datumindex != "null")
                                        {
                                            RTETagRef(tagtype, Tagruntimeaddress, datumindex);
                                        }
                                        else
                                        {
                                            ErrorOutput_TagNotFound(tagname, tagtype);
                                        }
                                    }
                                    else
                                    {
                                        IntPtr runtimeaddress = Tagruntimeaddress;
                                        WriteInt32(runtimeaddress, Convert.ToInt32("0xFFFFFFFF"));
                                        runtimeaddress += 0xC;
                                        WriteInt32(runtimeaddress, Convert.ToInt32("0xFFFFFFFF"));
                                    }
                                    break;
                                }
                            case "/int8":
                                {

                                    WriteByte(Tagruntimeaddress, byte.Parse(value));

                                    //WriteMemory(Tagruntimeaddress.ToString("X"), "int", value);
                                    break;
                                }
                            case "/int16":
                                {
                                    string output = string.Empty;
                                    byte[] bytes = BitConverter.GetBytes(Convert.ToInt16(value));
                                    for (int i = 0; i < bytes.Length; i++)
                                    {
                                        output += "0x" + bytes[i].ToString("X");
                                        if (i != bytes.Length - 1) output += ",";
                                    }
                                    WriteBytes(Tagruntimeaddress, bytes);

                                    //WriteMemory(Tagruntimeaddress.ToString("X"), "int", value);
                                    break;
                                }
                            case "/int32":
                                {
                                    WriteInt32((IntPtr)Tagruntimeaddress, int.Parse(value));
                                    break;
                                }
                            case "/enum8":
                                {
                                    foreach (var off1 in from XmlNode node in innerNodes
                                                         where node.Attributes[0].Value == method
                                                         from XmlNode option in node
                                                         where _node.Attributes[0].Value == method && option.Attributes[0].Value == value
                                                         let tmpflagindex = option.Attributes[1].Value
                                                         let off1 = Convert.ToInt32(tmpflagindex, 16)
                                                         select off1)
                                    {
                                        WriteInt32((IntPtr)Tagruntimeaddress, off1);
                                        return 0;
                                    }

                                    break;
                                }
                            case "/enum16":
                                {
                                    foreach (var off1 in from XmlNode node in innerNodes
                                                         where node.Attributes[0].Value == method
                                                         from XmlNode option in node
                                                         where _node.Attributes[0].Value == method && option.Attributes[0].Value == value
                                                         let tmpflagindex = option.Attributes[1].Value
                                                         let off1 = Convert.ToInt32(tmpflagindex, 16)
                                                         select off1)
                                    {
                                        WriteInt32((IntPtr)Tagruntimeaddress, off1);
                                        return 0;
                                    }

                                    break;
                                }
                            case "/enum32":
                                {
                                    foreach (var off1 in from XmlNode node in innerNodes
                                                         where node.Attributes[0].Value == method
                                                         from XmlNode option in node
                                                         where _node.Attributes[0].Value == method && option.Attributes[0].Value == value
                                                         let tmpflagindex = option.Attributes[1].Value
                                                         let off1 = Convert.ToInt32(tmpflagindex, 16)
                                                         select off1)
                                    {
                                        WriteInt32((IntPtr)Tagruntimeaddress, off1);
                                        return 0;
                                    }

                                    break;
                                }
                            case "/float32":
                                {
                                    float v1 = float.Parse(value);
                                    WriteFloat((IntPtr)Tagruntimeaddress, v1);
                                    break;
                                }
                            case "/point2":
                                { // Code flow ----->
                                    float v1 = float.Parse(value.Split(':')[0]); WriteFloat((IntPtr)Tagruntimeaddress, v1);
                                    float v2 = float.Parse(value.Split(':')[1]); Tagruntimeaddress += 4; WriteFloat((IntPtr)Tagruntimeaddress, v2);
                                    break;
                                }
                            case "/point3":
                                { // Code flow ----->
                                    float v1 = float.Parse(value.Split(':')[0]); WriteFloat((IntPtr)Tagruntimeaddress, v1);
                                    float v2 = float.Parse(value.Split(':')[1]); Tagruntimeaddress += 4; WriteFloat((IntPtr)Tagruntimeaddress, v2);
                                    float v3 = float.Parse(value.Split(':')[1]); Tagruntimeaddress += 4; WriteFloat((IntPtr)Tagruntimeaddress, v3);
                                    break;
                                }
                            case "/degree":
                                { // Code flow ----->
                                    int v1 = int.Parse(value.Split(':')[0]); float v1_2 = ConvertDegreesToRadians(v1); WriteFloat((IntPtr)Tagruntimeaddress, v1_2);
                                    break;
                                }
                            case "/degree2":
                                { // Code flow ----->
                                    int v1 = int.Parse(value.Split(':')[0]); float v1_2 = ConvertDegreesToRadians(v1); WriteFloat((IntPtr)Tagruntimeaddress, v1_2);
                                    int v2 = int.Parse(value.Split(':')[1]); float v2_2 = ConvertDegreesToRadians(v2); Tagruntimeaddress += 4; WriteFloat((IntPtr)Tagruntimeaddress, v2_2);
                                    break;
                                }
                            case "/vector3":
                                { // Code flow ----->
                                    float v1 = float.Parse(value.Split(':')[0]); WriteFloat((IntPtr)Tagruntimeaddress, v1);
                                    float v2 = float.Parse(value.Split(':')[1]); Tagruntimeaddress += 4; WriteFloat((IntPtr)Tagruntimeaddress, v2);
                                    float v3 = float.Parse(value.Split(':')[2]); Tagruntimeaddress += 4; WriteFloat((IntPtr)Tagruntimeaddress, v3);
                                    break;
                                }
                            case "/vector4":
                                { // Code flow ----->
                                    float v1 = float.Parse(value.Split(':')[0]); WriteFloat((IntPtr)Tagruntimeaddress, v1);
                                    float v2 = float.Parse(value.Split(':')[1]); Tagruntimeaddress += 4; WriteFloat((IntPtr)Tagruntimeaddress, v2);
                                    float v3 = float.Parse(value.Split(':')[2]); Tagruntimeaddress += 4; WriteFloat((IntPtr)Tagruntimeaddress, v3);
                                    float v4 = float.Parse(value.Split(':')[3]); Tagruntimeaddress += 4; WriteFloat((IntPtr)Tagruntimeaddress, v4);
                                    break;
                                }
                            case "/stringId":
                                { // Code flow ----->

                                    stringID = Globals.GetStringID(value);
                                    if (stringID.index == "")
                                    {
                                        WriteInt32((IntPtr)Tagruntimeaddress, 0);
                                    }
                                    else
                                    {
                                        WriteInt32((IntPtr)Tagruntimeaddress, 0); // need to fix
                                    }
                                    break;
                                }
                            case "/flags8": //
                                {
                                    string myOption = value.Split(':')[0];
                                    bool settrue = bool.Parse(value.Split(':')[1]);

                                    int flagindex = 0;
                                    foreach (var (Tagruntimeaddress1, tmpflagindex) in from XmlNode node in innerNodes
                                                                                       where node.Attributes[0].Value == method
                                                                                       from XmlNode option in node
                                                                                       where _node.Attributes[0].Value == method && option.Attributes[0].Value == myOption
                                                                                       let hexOffset1 = node.Attributes[1].Value
                                                                                       let off1 = Convert.ToInt32(hexOffset1, 16)
                                                                                       let Tagruntimeaddress1 = RunTimeAddress1 + off1
                                                                                       let tmpflagindex = option.Attributes[1].Value
                                                                                       select (Tagruntimeaddress1, tmpflagindex))
                                    {
                                        flagindex = int.Parse(tmpflagindex);
                                        int readFlags = (int)MemoryEditor.ReadByte((IntPtr)Tagruntimeaddress1);
                                        RTEFlags(Tagruntimeaddress1, readFlags, flagindex, settrue);
                                        return 0;
                                    }

                                    break;
                                }
                            case "/flags16": //
                                {
                                    string myOption = value.Split(':')[0];
                                    bool settrue = bool.Parse(value.Split(':')[1]);

                                    int flagindex = 0;
                                    foreach (var (Tagruntimeaddress1, tmpflagindex) in from XmlNode node in innerNodes
                                                                                       where node.Attributes[0].Value == method
                                                                                       from XmlNode option in node
                                                                                       where _node.Attributes[0].Value == method && option.Attributes[0].Value == myOption
                                                                                       let hexOffset1 = node.Attributes[1].Value
                                                                                       let off1 = Convert.ToInt32(hexOffset1, 16)
                                                                                       let Tagruntimeaddress1 = RunTimeAddress1 + off1
                                                                                       let tmpflagindex = option.Attributes[1].Value
                                                                                       select (Tagruntimeaddress1, tmpflagindex))
                                    {
                                        flagindex = int.Parse(tmpflagindex);
                                        int readFlags = (int)MemoryEditor.ReadInt16((IntPtr)Tagruntimeaddress1);
                                        RTEFlags(Tagruntimeaddress1, readFlags, flagindex, settrue);
                                        return 0;
                                    }

                                    break;
                                }
                            case "/flags32": //
                                {
                                    string myOption = value.Split(':')[0];
                                    bool settrue = bool.Parse(value.Split(':')[1]);

                                    int flagindex = 0;
                                    foreach (var (Tagruntimeaddress1, tmpflagindex) in from XmlNode node in innerNodes
                                                                                       where node.Attributes[0].Value == method
                                                                                       from XmlNode option in node
                                                                                       where _node.Attributes[0].Value == method && option.Attributes[0].Value == myOption
                                                                                       let hexOffset1 = node.Attributes[1].Value
                                                                                       let off1 = Convert.ToInt32(hexOffset1, 16)
                                                                                       let Tagruntimeaddress1 = RunTimeAddress1 + off1
                                                                                       let tmpflagindex = option.Attributes[1].Value
                                                                                       select (Tagruntimeaddress1, tmpflagindex))
                                    {
                                        flagindex = int.Parse(tmpflagindex);
                                        int readFlags = (int)MemoryEditor.ReadInt32((IntPtr)Tagruntimeaddress1);
                                        RTEFlags(Tagruntimeaddress1, readFlags, flagindex, settrue);
                                        return 0;
                                    }

                                    break;
                                }
                        }
                    }
                }
            }
            return 0;
        }

        public void WriteASCII(IntPtr Tagruntimeaddress, string value)
        {
            Console.WriteLine("\nWriting Memory - Address: " + Tagruntimeaddress + "       Value: " + value);
            MemoryEditor.WriteString(Tagruntimeaddress, value);
        }
        public void WriteInt32(IntPtr Tagruntimeaddress, int value)
        {
            Console.WriteLine("\nWriting Memory - Address: " + Tagruntimeaddress + "       Value: " + value);
            MemoryEditor.WriteInt32(Tagruntimeaddress, value);
        }
        public void WriteFloat(IntPtr Tagruntimeaddress, float value)
        {
            Console.WriteLine("\nWriting Memory - Address: " + Tagruntimeaddress + "       Value: " + value);
            MemoryEditor.WriteFloat(Tagruntimeaddress, value);
        }
        public void WriteByte(IntPtr Tagruntimeaddress, byte value)
        {
            Console.WriteLine("\nWriting Memory - Address: " + Tagruntimeaddress + "       Value: " + value);
            MemoryEditor.WriteByte(Tagruntimeaddress, value);
        }
        public void WriteBytes(IntPtr Tagruntimeaddress, byte[] value)
        {
            Console.WriteLine("\nWriting Memory - Address: " + Tagruntimeaddress + "       Value: " + value);
            MemoryEditor.WriteBytes(Tagruntimeaddress, value);
        }

        private void ErrorOutput_TagNotFound(string tagname, string tagtype)
        {
            Console.WriteLine("=================================== COMMAND ERROR ===================================");
            Console.WriteLine("Failed to find Tagname: " + tagname);
            Console.WriteLine("Tag Dictinary: " + tagtype);
            Console.WriteLine("Check the below command for spelling or formatting errors!!");
            Console.WriteLine("Note: Shared.map tags are not loaded on all maps, check if tag is loaded on the .map");
            Console.WriteLine("===================================================================================");
        }

        public float ConvertDegreesToRadians(int degrees)
        {
            float radians = (float)((Math.PI / 180) * degrees);
            return (radians);
        }

        public void RTETagRef(string tagtype, IntPtr runtimeaddress, string datumindex)
        {
            datumindex = datumindex.Replace("0x", "");
            int tagRef = int.Parse(datumindex, System.Globalization.NumberStyles.HexNumber);
            byte[] bytes = Encoding.ASCII.GetBytes(tagtype);
            Array.Reverse(bytes);
            string Rtagtype = Encoding.ASCII.GetString(bytes);
            WriteASCII(runtimeaddress, Rtagtype);
            WriteInt32((runtimeaddress + 0xC), tagRef);
        }
        public void RTEFlags(int runtimeaddress, int currentFlags, int option, bool setTrue)
        {
            string input = Convert.ToString(currentFlags, 2);
            int output = Convert.ToInt32(input, 2);
            if (setTrue == true)
            {
                int intValue = output | (1 << option);

                WriteInt32((IntPtr)runtimeaddress, intValue);
            }
            else
            {
                int intValue = output &= ~(1 << option);

                WriteInt32((IntPtr)runtimeaddress, intValue);
            }
        }
        private float GetColorF(string hex)
        {
            int x = int.Parse(hex, System.Globalization.NumberStyles.HexNumber);
            float f = x / 255;

            return f;
        }
    }
}

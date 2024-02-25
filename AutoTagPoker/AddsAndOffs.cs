using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTagPoker
{
    public static class Addresses
    {
        public static string mapsHeader = "eldorado.exe+01EB7FA8";
        public static string g_cache_file_globals = "eldorado.exe+1EAAFE8";
        public static string gameTypeDescription = "eldorado.exe+1EC0500";

        public static string OffsetAddress(string address, string offset)
        {
            string[] parts = address.Split('+');
            if (parts.Length != 2)
            {
                Console.WriteLine("Invalid base address format");
                return "invalid";
            }
            string moduleName = parts[0];
            string hexAddressStr = parts[1];

            if (!int.TryParse(hexAddressStr, System.Globalization.NumberStyles.HexNumber, null, out int baseAddress))
            {
                Console.WriteLine("Invalid hex address format");
                return "invalid";
            }
            // Calculate the new address
            int newAddress = baseAddress + int.Parse(offset.Replace(",", ""), NumberStyles.HexNumber);

            // Convert the new address back to string
            string newAddressStr = moduleName + "+" + newAddress.ToString("X");
            return newAddressStr;
        }
        //halo2.exe+0x435fe3,0x10,0x22,0x12
        public static IntPtr GetIntPtrFromAddress(string address)
        {
            // Split the address string by comma and plus sign
            string[] parts = address.Split(new char[] { ',', '+' }, StringSplitOptions.RemoveEmptyEntries);
            string[] offsetparts = address.Split('+');
            int[] offsets = null;

            if (offsetparts.Length == 2) // Ensure the string has the correct format
            {
                string[] offsetParts = parts.Skip(2).ToArray(); // Skip the first element
                offsets = offsetParts.Select(part => Convert.ToInt32(part.Trim(), 16)).ToArray();

                // Now 'offsets' array contains the parsed offsets
            }

            // Parse the base address and offset
            if (parts.Length >= 3 && parts[0] == "eldorado.exe")
            {
                if (int.TryParse(parts[1], System.Globalization.NumberStyles.HexNumber, null, out int baseAddressOffset) &&
                    int.TryParse(parts[2].Substring(2), System.Globalization.NumberStyles.HexNumber, null, out int offset))
                {
                    // Calculate the final address by adding the base address and offset
                    IntPtr baseAddress = Globals.myProcess.MainModule.BaseAddress;
                    IntPtr currentAddress = (IntPtr)(baseAddress.ToInt64() + baseAddressOffset);

                    if (offsets != null)
                    {
                        foreach (int off in offsets)
                        {
                            int Pointer = MemoryEditor.ReadInt32(currentAddress);


                            currentAddress = (IntPtr)Pointer + off;
                        }
                    }

                    // Return the final address as IntPtr
                    return currentAddress;
                }
            }
            else
            {
                if (int.TryParse(parts[1], System.Globalization.NumberStyles.HexNumber, null, out int baseAddressOffset))
                {
                    IntPtr baseAddress = Globals.myProcess.MainModule.BaseAddress;
                    IntPtr currentAddress = (IntPtr)(baseAddress.ToInt64() + baseAddressOffset);

                    return currentAddress;
                }
            }

            // Return IntPtr.Zero if the address format is invalid
            return IntPtr.Zero;
        }



    }
    public static class Offsets
    {
        //mapsHeader
        public static string scnr_Name = ",1A4";
        //g_cache_file_globals
        public static int tag_index_absolute_mapping = 0x14;
        public static int absolute_index_tag_mapping = 0x18;
        public static int tag_loaded_count = 0x1C;
        public static int tag_total_count = 0x20;
        public static int tag_instances = 0x10;
    }
    public static class Instance_Offsets
    {
        public static int tagGroupName = 0x14;
        public static int tagBlockOffset = 0x10;
    }
}

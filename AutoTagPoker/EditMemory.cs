using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AutoTagPoker
{

    public static class MemoryEditor
    {
        // Constants for process access rights
        private const int PROCESS_VM_READ = 0x0010;
        private const int PROCESS_VM_WRITE = 0x0020;
        private const int PROCESS_VM_OPERATION = 0x0008;

        // Handle to the target process
        private static IntPtr processHandle = Globals.myProcess.Handle;

        // Read memory from the target process
        public static bool ReadMemory(IntPtr address, byte[] buffer, int bytesToRead, out int bytesRead)
        {
            return ReadProcessMemory(processHandle, address, buffer, bytesToRead, out bytesRead);
        }

        // Write memory to the target process
        public static bool WriteMemory(IntPtr address, byte[] buffer, int bytesToWrite, out int bytesWritten)
        {
            return WriteProcessMemory(processHandle, address, buffer, bytesToWrite, out bytesWritten);
        }

        // Read an integer from memory
        public static int ReadInt32(IntPtr address)
        {
            byte[] buffer = new byte[4];
            ReadMemory(address, buffer, buffer.Length, out _);
            return BitConverter.ToInt32(buffer, 0);
        }
        public static int ReadInt16(IntPtr address)
        {
            byte[] buffer = new byte[2];
            ReadMemory(address, buffer, buffer.Length, out _);
            return BitConverter.ToInt32(buffer, 0);
        }
        public static int ReadInt8(IntPtr address)
        {
            byte[] buffer = new byte[1];
            ReadMemory(address, buffer, buffer.Length, out _);
            return BitConverter.ToInt32(buffer, 0);
        }

        // Read a float from memory
        public static float ReadFloat(IntPtr address)
        {
            byte[] buffer = new byte[4];
            ReadMemory(address, buffer, buffer.Length, out _);
            return BitConverter.ToSingle(buffer, 0);
        }

        // Read a string from memory (ASCII)
        public static string ReadString(IntPtr address, int maxLength = 256)
        {
            byte[] buffer = new byte[maxLength];
            ReadMemory(address, buffer, buffer.Length, out int bytesRead);

            // Find the index of the first null character (if any)
            int nullTerminatorIndex = Array.IndexOf(buffer, (byte)0);

            // If no null terminator is found, use the maximum length of bytes read
            if (nullTerminatorIndex == -1)
            {
                nullTerminatorIndex = bytesRead;
            }

            // Decode the byte array up to the null terminator (or maximum bytes read)
            string result = Encoding.ASCII.GetString(buffer, 0, nullTerminatorIndex);

            // Remove any trailing whitespace
            result = result.Trim();

            return result;
        }

        // Read a Unicode string from memory
        public static string ReadUnicodeString(IntPtr address, int maxLength = 256)
        {
            byte[] buffer = new byte[maxLength * 2];
            ReadMemory(address, buffer, buffer.Length, out int bytesRead);
            return Encoding.Unicode.GetString(buffer, 0, bytesRead);
        }

        // Write an integer to memory
        public static bool WriteInt32(IntPtr address, int value)
        {
            byte[] buffer = BitConverter.GetBytes(value);
            return WriteMemory(address, buffer, buffer.Length, out _);
        }

        // Write a float to memory
        public static bool WriteFloat(IntPtr address, float value)
        {
            byte[] buffer = BitConverter.GetBytes(value);
            return WriteMemory(address, buffer, buffer.Length, out _);
        }

        // Write a string to memory (ASCII)
        public static bool WriteString(IntPtr address, string value)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(value);
            return WriteMemory(address, buffer, buffer.Length, out _);
        }

        // Write a Unicode string to memory
        public static bool WriteUnicodeString(IntPtr address, string value)
        {
            byte[] buffer = Encoding.Unicode.GetBytes(value);
            return WriteMemory(address, buffer, buffer.Length, out _);
        }

        public static byte ReadByte(IntPtr address)
        {
            byte[] buffer = new byte[1];
            ReadMemory(address, buffer, buffer.Length, out _);
            return buffer[0];
        }

        public static byte[] ReadBytes(IntPtr address, int length)
        {
            byte[] buffer = new byte[length];
            ReadMemory(address, buffer, buffer.Length, out _);
            return buffer;
        }

        public static bool WriteByte(IntPtr address, byte value)
        {
            byte[] buffer = { value };
            return WriteMemory(address, buffer, buffer.Length, out _);
        }

        public static bool WriteBytes(IntPtr address, byte[] values)
        {
            return WriteMemory(address, values, values.Length, out _);
        }

        // Import the necessary functions from kernel32.dll
        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, out int lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int nSize, out int lpNumberOfBytesWritten);

        // Import CloseHandle function from kernel32.dll
        [DllImport("kernel32.dll")]
        private static extern bool CloseHandle(IntPtr hObject);
    }
}

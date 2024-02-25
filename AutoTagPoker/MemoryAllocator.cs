using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AutoTagPoker
{
    public static class MemoryAllocator
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, AllocationType flAllocationType, MemoryProtection flProtect);

        [Flags]
        private enum AllocationType : uint
        {
            Commit = 0x1000,
            Reserve = 0x2000,
            Decommit = 0x4000,
            Release = 0x8000,
        }

        [Flags]
        private enum MemoryProtection : uint
        {
            ExecuteReadWrite = 0x40,
            ExecuteWriteCopy = 0x80,
            ExecuteRead = 0x20,
            Execute = 0x10,
            ReadOnly = 0x02,
            ReadWrite = 0x04,
            WriteCopy = 0x08,
            NoAccess = 0x01,
        }

        public static IntPtr AllocateMemory(IntPtr processHandle, uint size)
        {
            IntPtr intPtr = VirtualAllocEx(processHandle, IntPtr.Zero, size, AllocationType.Commit | AllocationType.Reserve, MemoryProtection.ReadWrite);
            if (Globals.assignedMemory == null)
            {
                Globals.assignedMemory = new IntPtr[0];
            }

            // Resize the array to accommodate the new IntPtr value
            Array.Resize(ref Globals.assignedMemory, Globals.assignedMemory.Length + 1);

            // Assign the new IntPtr value to the last element of the array
            Globals.assignedMemory[Globals.assignedMemory.Length - 1] = intPtr;
            return VirtualAllocEx(processHandle, IntPtr.Zero, size, AllocationType.Commit | AllocationType.Reserve, MemoryProtection.ReadWrite);
        }
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool VirtualFreeEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, FreeType dwFreeType);

        public enum FreeType : uint
        {
            Release = 0x8000
        }
    }
}

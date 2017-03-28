using System.Runtime.InteropServices;

namespace Goodbye_World {
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct IMAGE_SECTION_HEADER {
        public const int IMAGE_SIZEOF_SHORT_NAME = 8;

        [StructLayout(LayoutKind.Explicit)]
        public struct MISC {
            [FieldOffset(0)]
            public uint PhysicalAddress;
            [FieldOffset(0)]
            public uint VirtualSize;
        }

        public ulong Name;
        public MISC Misc;
        public uint VirtualAddress;
        public uint SizeOfRawData;
        public uint PointerToRawData;
        public uint PointerToRelocations;
        public uint PointerToLinenumbers;
        public ushort NumberOfRelocations;
        public ushort NumberOfLinenumbers;
        public uint Characteristics;
    }
}

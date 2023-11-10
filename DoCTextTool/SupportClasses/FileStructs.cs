namespace DoCTextTool.SupportClasses
{
    internal class FileStructs
    {
        public struct Header
        {
            public uint SeedValueA;
            public uint SeedValueB;
            public ushort LineCount;
            public int Reserved;
            public int DcmpFlag;
            public uint UnencTxtSize;
            public uint DcmpBodySize;
            public uint TotalFileSize;
            public uint HeaderSize;
            public uint HeaderCheckSum;
        }

        public struct CompressedDataFooter
        {
            public uint CompressedDataSize;
            public uint CompressedDataCheckSum;
        }

        public struct LineOffsets
        {
            public uint UnknownId;
            public uint LineIdOffset;
            public uint LineOffset;
        }
    }
}
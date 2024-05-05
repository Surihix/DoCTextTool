namespace DoCTextTool.SupportClasses
{
    internal class ToolVariables
    {
        public static EncodingSwitches TxtEncoding { get; set; }

        public enum EncodingSwitches
        {
            lt,
            jp
        }

        public class Header
        {
            public ulong SeedValue { get; set; }
            public ushort LineCount { get; set; }
            public byte Reserved { get; set; }
            public byte DcmpFlag { get; set; }
            public uint DecryptedFooterTxtSize { get; set; }
            public uint DcmpBodySize { get; set; }
            public uint TotalFileSize { get; set; }
            public uint HeaderSize { get; set; }
            public uint HeaderCheckSum { get; set; }
        }

        public class BodyFooter
        {
            public uint BodyDataSize { get; set; }
            public uint CompressedDataCheckSum { get; set; }
        }

        public class LineOffsets
        {
            public uint UnknownId { get; set; }
            public uint LineIdOffset { get; set; }
            public uint LineOffset { get; set; }
        }
    }
}
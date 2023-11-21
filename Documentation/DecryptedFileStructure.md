


#### Header Section
| Offset | Size | Type | Description |
| --- | --- | --- | --- |
| 0x0 | 0x4 | UInt32 | Seed Value A, always 1 |
| 0x4 | 0x4 | UInt32 | Seed Value B, always 0 |
| 0x8 | 0x2 | UInt16 | Line Count |
| 0xA | 0x1 | UInt8 | Reserved, always null |
| 0xB | 0x1 | UInt8 | Compression Flag |
| 0xC | 0x4 | UInt32 | Decrypted text snippet size |
| 0x10 | 0x4 | UInt32 | Decompressed Body Section size |
| 0x14 | 0x4 | UInt32 | File size |

#### Header Footer Section
| Offset | Size | Type | Description |
| --- | --- | --- | --- |
| 0x18 | 0x4 | UInt32 | Header Section size, always 24 |
| 0x1C | 0x4 | UInt32 | Header [Checksum](https://github.com/Surihix/DoCTextTool/master/Documentation/DecryptedFileStructure.md#checksum) |

<br>

#### Body Footer Section
| Offset | Size | Type | Description |
| --- | --- | --- | --- |
| 0x18 | 0x4 | UInt32 | Body Section size |
| 0x1C | 0x4 | UInt32 | Body [Checksum](https://github.com/Surihix/DoCTextTool/master/Documentation/DecryptedFileStructure.md#checksum) |

<br>

#### Checksum

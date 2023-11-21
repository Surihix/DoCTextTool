First off majority of the credits goes to Shademp for researching this format and the decryption method.

The text bin files are encrypted with a custom encryption scheme and would have to be decrypted to reveal usable data. these files are always encrypted by default and begin with this following header: 
<br>``45 90 AF F9 B7 47 F6 94``

The file is split into three main parts. a [Header Section](https://github.com/Surihix/DoCTextTool/blob/master/Documentation/DecryptedFileStructure.md#header-section), a [Body Section](https://github.com/Surihix/DoCTextTool/blob/master/Documentation/DecryptedFileStructure.md#body-section) and a decrypted text snippet section at the end of the file. the header and the body sections each have its own footer sections which would contain the size and checksum values of its associated sections. 

The decrypted text snippet section can contain either a decrypted line or a line id from the body section. at first, it was assumed that the longest line in the text bin file would directly be pasted into this section. but after some observations with other files, it looks like parts of different lines are sometimes mixed and pasted here with the size of the data being that of the longest line's size. 

If in case the text bin file's size is not divisible by 8, then null bytes are padded to reach the closest divisible by 8 size and the decrypted text snippet size value in the [Header Section](https://github.com/Surihix/DoCTextTool/blob/master/Documentation/DecryptedFileStructure.md#header-section) would take those null bytes into account along with longest line's size.

<br>

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
| 0x1C | 0x4 | UInt32 | Header Section [Checksum](https://github.com/Surihix/DoCTextTool/blob/master/Documentation/DecryptedFileStructure.md#checksum) |

<br>


#### Body Section
In most of the text bin files, the body section is compressed with zlib compression and the footer offsets for this section would contain the size and checksum value of the compressed data. the size of this section would always be divisible by 8 and if its not, then null bytes are padded to make it reach the closest divisible by 8 size. do note that the decompressed body section size in the [Header Section](https://github.com/Surihix/DoCTextTool/blob/master/Documentation/DecryptedFileStructure.md#header-section) will not take these null bytes into account as it only reflects the size of the decompressed body section data.

You can determine whether the body section is compressed or not by seeing the Compression Flag value in the [Header Section](https://github.com/Surihix/DoCTextTool/blob/master/Documentation/DecryptedFileStructure.md#header-section). if its `1`, then the body section is compressed. if its `0`, then its not compressed. 

If the body section is not compressed, then the footer offsets would contain the size and checksum of the decompressed data. to put it in simpler terms, the footer offsets reflect the state of the section's data when its present in the text bin file. 

The [Decompressed Body Section](https://github.com/Surihix/DoCTextTool/blob/master/Documentation/DecryptedFileStructure.md#decompressed-body-section) has its own offset table that would contain a numerical id, an offset to a line id and an offset to the line. there would be 12 offsets in total for each line in the file and you can refer to the Line Count value given in the [Header Section](https://github.com/Surihix/DoCTextTool/blob/master/Documentation/DecryptedFileStructure.md#header-section) to compute the total number of offsets present in this section. 

The line strings would begin after the offset table and the line id strings would begin after the end of all the lines strings. each line and line id string bytes would terminate with a single null byte and some null bytes are padded after the last line id string bytes, to ensure that the size value of the decompressed body section is divisible by 8.

#### Decompressed Body Section Table
| Offset | Size | Type | Description |
| --- | --- | --- | --- |
| 0x0 | 0x4 | UInt32 | Unknown numerical Id |
| 0x4 | 0x4 | UInt32 | Line Id position, absolute |
| 0x8 | 0x4 | UInt32 | Line position, absolute |


#### Body Footer Section
| Offset | Size | Type | Description |
| --- | --- | --- | --- |
| 0x18 | 0x4 | UInt32 | Body Section size (when on the text bin file) |
| 0x1C | 0x4 | UInt32 | Body Section [Checksum](https://github.com/Surihix/DoCTextTool/blob/master/Documentation/DecryptedFileStructure.md#checksum) (when on the text bin file) |

<br>

#### Checksum
The checksum is calculated by adding every 4th byte from the 0th byte of a section till the last 4th byte of that section. the footer section is excluded from this calculation.

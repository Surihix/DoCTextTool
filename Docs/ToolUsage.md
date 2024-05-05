# Instructions
The program should be launched from a command prompt terminal with few argument switches.

Action switches:
<br>``-x`` Extracts text data from a text bin file into a normal text file
<br>``-c`` Converts the extracted normal text file data to the game's text bin format
<br>``-b`` Extracts the compressed data section from a text bin file along with the necessary offsets into a new binary file
<br>``-h`` or ``-?`` Displays the tool's usage instructions

<br>Encoding switches
<br>``-lt`` For English and other Latin language based text bin files
<br>``-jp`` For Japanese text bin files

<br>Commandline usage examples:
<br>``DoCTextTool.exe -x -lt "string_us.bin" ``
<br>``DoCTextTool.exe -c -lt "string_us.txt" ``
<br>``DoCTextTool.exe -b -lt "string_us.txt" ``

## Important Notes
- Use the appropriate encoding switch when extracting or converting a text bin file. the filename should give an idea on what encoding to use but the vast majority of text bin files do not have a virtual path specified in the main KEL.DAT archive. you can use the [DoCPathGenerator](https://github.com/Surihix/DoCPathsGenerator) program to generate paths for these pathless text bin files which should help in determining what encoding switch to use. alternatively, you can experiment with both the switches and see which one gives you the correct text data in the extracted text file.
- The very first line in the extracted text file will have a number which indicates the number of lines present in the text file. change this number only when you are increasing or decreasing the lines in the text file before converting back to the game's text file format.
- The extracted text file will have two different Ids. a numerical Id and a string based Id. each line will be neatly separated with a `` || `` symbol along with the Ids and will look something like this:
<br>`` 1781806664 || MES_70352CT || $hctVincent. You should probably$rthink of getting some rest. ``

- There will usually be eight text files for each cutscene and event in the game, making them collectively a set. each text file from the set is supposed to have the text data for a different UI language but from what I have noticed, this probably applies
only for the PAL version text file sets. most of the NTSC-U version's text bin files, contain the english text data in all the eight files and sometimes would contain only the Japanese text data in that set.
- If you are adding a new line, then ensure that both the numerical and the string based Ids are present before the line data along with the `` || `` symbol. as of now, I don't know how both the Ids are generated, but all text files in the same set do share the same ids and so if there is a situation where a different UI language's text data from the set contains an extra line or two, then you can take the Ids from those lines and put it in your new line. do note that the order of the numerical Ids have to be sequential. for example, you can't have ``1580148303`` after ``1781806664``.
- Some of the english and latin language based text bin files would have Japanese characters in one or more lines. so if you encounter some weird looking symbols, characters or numbers in a line, then assume that those are japanese characters. from what I have noticed, these Japanese characters are usually developer comments stating that a line is unused and so it can be safely considered as an unused line.
- The ``-b`` switch will extract a text bin file's compressed data section into a new binary file and will also copy all of the necessary offsets in their decrypted state, onto the new file. this option is useful if in case you want to view the text data present on the file without being obscured by the encryption.

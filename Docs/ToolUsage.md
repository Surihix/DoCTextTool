# Instructions
The program should be launched from a command prompt terminal with any one of these following argument switches, along with the input file:
<br>``-x`` Extracts text data from a text bin file into a normal text file
<br>``-c`` Converts the extracted normal text file data to the game's text bin format
<br>``-b`` Extracts the compressed data section from a text bin file along with the necessary offsets into a new binary file

Commandline usage examples:
<br>``DoCTextTool.exe -x "string_us.bin" ``
<br>``DoCTextTool.exe -c "string_us.txt" ``
<br>``DoCTextTool.exe -b "string_us.txt" ``

## Important Notes
- The very first line in the extracted text file will have a number which indicates the number of lines present in the text file. change this number only when you are increasing or decreasing the lines in the text file before converting back to the game's text file format.
- The extracted text file will have two different Ids. a numerical Id and a string based Id. each line will be neatly separated with a `` || `` symbol along with the Ids and will look something like this:
<br>`` 1781806664 || MES_70352CT || $hctVincent. You should probably$rthink of getting some rest. ``

- There will usually be eight text files for each cutscene and event in the game, making them collectively a set. each text file from the set is supposed to have the text data for a different UI language but from what I have noticed, this probably applies
only for the PAL version text file sets. most of the NTSC-U version's text bin files, contain the english text data in all the eight files and sometimes would contain only the Japanese text data in that set.
- If you are adding a new line, then ensure that both the numerical and the string based Ids are present before the line data along with the `` || `` symbol. as of now, I don't know how both the Ids are generated, but all text files in the same set do share the same ids and so if there is a situation where a different UI language's text data from the set contains an extra line or two, then you can take the Ids from those lines and put it in your new line. do note that the order of the numerical Ids have to be sequential. for example, you can't have ``1580148303`` after ``1781806664``.
- The ``-b`` switch will extract a text bin file's compressed data section into a new binary file and will also copy all of the necessary offsets in their decrypted state, onto the new file. this option is useful if in case you want to view the text data present on the file without being obscured by the encryption.

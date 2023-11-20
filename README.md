# DoCTextTool
A small tool that allows you to extract and convert Dirge of Cerberus's text files. 

The program should be launched from a command prompt terminal with these following argument switches along with the input file:
<br>``-e`` Extracts text data from the game's propertiary text files into a general text file
<br>``-c`` Converts the general text file data back into game's propertiary text file format

Commandline usage examples:
<br>``DoCTextTool.exe -e "string_us.bin" ``
<br>``DoCTextTool.exe -c "string_us.txt" ``

## Important Notes
- The very first line in the extracted text file will have the number of lines present in the text file. change this number only if you are increasing or decreasing the lines in the text file before converting back to the game's text file format.
the number should match the number of lines present in the text file.
- The extracted text file will have two different IDs. a numerical Id and a string based Id. each line will be neatly separated with a `` || `` symbol along with the Ids and they will look something like this:
<br>`` 1781806664 || MES_70352CT || $hctVincent. You should probably$rthink of getting some rest. ``

- There will usually be eight text files for each cutscene and event in the game, making them collectively a set. each text file from the set is supposed to have the text data for a different UI language but from what I have noticed, this probably applies
only for the PAL version text file sets. most of the NTSC-U version files, contain the english text data in all the eight files and sometimes would also contain the Japanese text data in that set.
- If you are adding a new line, then ensure that both the numerical and the string based Ids are present before the line data along with the `` || `` symbol. as of now, I don't know how both the Ids are generated, but all text files in the same set do share
  the same ids. so if there is a situation where a different language's text data from the same set, contains an extra line or two, then you can take the ids from those lines and put it into your text file's new line. the numerical Ids order has to be
  sequential. for instance, you can't have ``1580148303`` after ``1781806664``.


# Credits
[Shademp](https://github.com/Shademp) - Decryption assembly algorithm and general support

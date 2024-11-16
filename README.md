# NeedleTextureStreamingUtility
Packs, unpacks and analyzes Hedgehog Engine NTSP/NTSI files.

```
Usages:

   unpack <NTSI DDS file/directory> [NTSP directory]
      Restores a DDS file from an NTSI file and its NTSP package.
      If no NTSP directory is specified, current directory is used.
      Unpacks either just the selected NTSI file, or all NTSI files in a directory

   pack <DDS file directory> [Output Name]
      Creates an NTSP file and NTSI DDS files from DDS files inside a folder.
      If no name is specified, the directory name will be used.
      Files are outputs to <DDS file directory>/NTSP

   info <NTSI DDS file | NTSP file>
      Prints info about the specified file
```
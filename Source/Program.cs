using System;

namespace NeedleTextureStreamingUtility
{
    public class Program
    {
        static void Main(string[] args)
        {
            if(args.Length == 0)
            {
                PrintHelp();
                return;
            }

            switch(args[0])
            {
                case "unpack":
                    if(args.Length < 2)
                    {
                        Console.WriteLine("No file specified");
                        return;
                    }

                    string ntsiFile = args[1];
                    string ntspDirectory = string.Empty;

                    if(args.Length > 2)
                    {
                        ntspDirectory = args[2];
                    }

                    Unpack.Run(ntsiFile, ntspDirectory);
                    break;

                case "pack":
                    if(args.Length < 2)
                    {
                        Console.WriteLine("No input directory specified");
                        return;
                    }

                    string inputDirectory = args[1];
                    string outputName = string.Empty;

                    if(args.Length > 2)
                    {
                        outputName = args[2];
                    }

                    Pack.Run(inputDirectory, outputName);
                    break;

                case "info":
                    if(args.Length < 2)
                    {
                        Console.WriteLine("No file specified");
                        return;
                    }

                    Info.Run(args[1]);
                    break;

                default:
                    PrintHelp();
                    break;
            }
        }

        private static void PrintHelp()
        {
            Console.WriteLine("""
            Needle Texture Streaming Utility - @Justin113D
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
            """);
        }

    }
}

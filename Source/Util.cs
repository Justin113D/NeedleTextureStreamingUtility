using SharpNeedle.IO;
using SharpNeedle.Utilities;
using System;
using System.IO;
using System.Linq;

namespace NeedleTextureStreamingUtility
{
    public static class Util
    {
        public const uint NTSPSignature = 0x5053544E;
        public const uint NTSISignature = 0x4E545349;
        public const uint DDSSignature = 0x44445320;

        public static uint GetSignature(IFile file)
        {
            Stream filestream = file.Open();

            uint signature = GetSignature(filestream.ReadBytes(4));
            filestream.Position = 0;

            return signature;
        }

        public static uint GetSignature(byte[] file)
        {
            return (uint)((file[0] << 24) | (file[1] << 16) | (file[2] << 8) | file[3]);
        }

        public static void PrintStringTable(string[][] table, string[] headers, bool[] alignment)
        {
            int[] widths = new int[headers.Length];

            for(int i = 0; i < widths.Length; i++)
            {
                widths[i] = int.Max(headers[i].Length, table.Max(x => x[i].Length));
            }

            Console.Write(" Index  ");

            for(int i = 0; i < widths.Length; i++)
            {
                Console.Write(' ');
                Console.Write(headers[i]);
                Console.Write(new string(' ', widths[i] - headers[i].Length + 2));
            }

            Console.WriteLine();

            for(int i = 0; i < table.Length; i++)
            {
                string index = i.ToString();
                Console.Write(new string(' ', 5 - index.Length + 1));
                Console.Write(index);
                Console.Write("  ");

                for(int j = 0; j < widths.Length; j++)
                {
                    if(alignment[j])
                    {
                        Console.Write(new string(' ', widths[j] - table[i][j].Length + 1));
                        Console.Write(table[i][j]);
                        Console.Write("  ");
                    }
                    else
                    {
                        Console.Write(' ');
                        Console.Write(table[i][j]);
                        Console.Write(new string(' ', widths[j] - table[i][j].Length + 2));
                    }
                }
                Console.WriteLine();
            }
        }
    }
}

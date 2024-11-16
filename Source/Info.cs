using Amicitia.IO.Binary;
using Amicitia.IO.Streams;
using SharpNeedle;
using SharpNeedle.HedgehogEngine.Needle.TextureStreaming;
using SharpNeedle.IO;
using System;
using System.IO;
using System.Linq;
using NTSI = SharpNeedle.HedgehogEngine.Needle.TextureStreaming.Info;

namespace NeedleTextureStreamingUtility
{
    public class Info
    {
        public static void Run(string filepath)
        {
            if(!File.Exists(filepath))
            {
                Console.WriteLine("File not found!");
                return;
            }


            IFile file = FileSystem.Open(filepath);
            uint signature = Util.GetSignature(file);

            if(signature == Util.NTSPSignature)
            {
                PrintNTSP(file);
            }
            else if(signature == Util.NTSISignature)
            {
                PrintNTSI(file);
            }
            else
            {
                Console.WriteLine("File is not an NTSI or NTSP file");
            }

        }
    
        private static void PrintNTSP(IFile file)
        {
            Package package = ResourceManager.Instance.Open<Package>(file);

            Console.WriteLine(
                $"""
                    ===== NTSP file =====
                    Entries: {package.Entries.Count}
                    Blocks: {package.Blocks.Count}

                    ===== Entries =====
                    """);

            string[][] entryInfo = package.Entries.Select<Package.EntryInfo, string[]>(x => [
                x.Name,
                    x.BlockIndex.ToString(),
                    x.Width.ToString(),
                    x.Height.ToString(),
                    x.MipLevels.ToString(),
                    x.Hash.ToString("X8")
            ]).ToArray();

            Util.PrintStringTable(
                entryInfo,
                ["Name", "Blockindex", "Width", "Height", "Mipmap levels", "Hash"],
                [false, true, true, true, true, true]);

            Console.WriteLine();
            Console.WriteLine("===== Blocks =====");

            string[][] blockInfo = package.Blocks.Select<DataBlock, string[]>(x => [
                x.Position.ToString("X"),
                    x.Length.ToString()
            ]).ToArray();

            Util.PrintStringTable(blockInfo, ["Position", "Length"], [true, true]);
        }

        private static void PrintNTSI(IFile file)
        {
            NTSI info = ResourceManager.Instance.Open<NTSI>(file);

            MemoryStream ddsStream = new(info.DdsHeader);
            BinaryObjectReader ddsReader = new(ddsStream, StreamOwnership.Retain, Endianness.Little);
            ddsReader.Skip(4);
            DDSHeader ddsHeader = DDSHeader.Read(ddsReader);

            Console.WriteLine(
                $"""
                    ===== NTSI file =====
                    Name: {Path.GetFileNameWithoutExtension(info.Name)}
                    Package Name: {info.PackageName}
                    Mip 4x4 index: {info.Mip4x4Index}
                    Mip 4x4 data size: {info.Mip4x4.Length}

                    ===== DDS file header =====
                    Size: {ddsHeader.Size}
                    Flags: {ddsHeader.Flags}
                    Height: {ddsHeader.Height}
                    Width: {ddsHeader.Width}
                    Pitch/Linear Size: {ddsHeader.PitchOrLinearSize}
                    Depth: {ddsHeader.Depth}
                    Mipmap Count: {ddsHeader.MipMapCount}

                        ===== Pixel format =====
                        Size: {ddsHeader.PixelFormat.Size}
                        Flags: {ddsHeader.PixelFormat.Flags}
                        FourCC: {ddsHeader.PixelFormat.FourCC:X8}
                        RGBBitCount: {ddsHeader.PixelFormat.RGBBitCount:X8}
                        RBitMask: {ddsHeader.PixelFormat.RBitMask:X8}
                        GBitMask: {ddsHeader.PixelFormat.GBitMask:X8}
                        BBitMask: {ddsHeader.PixelFormat.BBitMask:X8}
                        ABitMask: {ddsHeader.PixelFormat.ABitMask:X8}
                    

                    Caps: {ddsHeader.Caps}
                    Caps2: {ddsHeader.Caps2}
                    Caps3: {ddsHeader.Caps3:X8}
                    Caps4: {ddsHeader.Caps4:X8}
                    """);

            if(ddsHeader.DXT10Extension.HasValue)
            {
                Console.WriteLine(
                    $"""

                        ===== DXT10 extension =====
                        Format: {ddsHeader.DXT10Extension.Value.Format}
                        Resource Dimension: {ddsHeader.DXT10Extension.Value.ResourceDimension}
                        Misc Flag: {ddsHeader.DXT10Extension.Value.MiscFlag}
                        Array Size: {ddsHeader.DXT10Extension.Value.ArraySize}
                        Alpha Mode: {ddsHeader.DXT10Extension.Value.AlphaMode}
                        """);
            }
        }
    }
}

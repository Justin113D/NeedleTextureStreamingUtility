using Amicitia.IO.Binary;
using Amicitia.IO.Streams;
using SharpNeedle.HedgehogEngine.Needle.TextureStreaming;
using SharpNeedle.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NTSI = SharpNeedle.HedgehogEngine.Needle.TextureStreaming.Info;

namespace NeedleTextureStreamingUtility
{
    public static class Pack
    {
        public static void Run(string inputDirectory, string outputName)
        {
            if(!Directory.Exists(inputDirectory))
            {
                Console.WriteLine("Input directory does not exist!");
                return;
            }

            if(string.IsNullOrWhiteSpace(outputName))
            {
                outputName = Path.GetFileName(inputDirectory);
            }

            Package package = new();
            List<NTSI> infos = [];

            foreach(string filepath in Directory.GetFiles(inputDirectory, "*.dds"))
            {
                string filename = Path.GetFileName(filepath);
                byte[] file = File.ReadAllBytes(filepath);
                uint signature = Util.GetSignature(file);

                if(signature != Util.DDSSignature)
                {
                    Console.WriteLine($"File \"{filename}\" is not a DDS file!");
                    continue;
                }

                MemoryStream stream = new(file);
                BinaryObjectReader reader = new(stream, StreamOwnership.Retain, Endianness.Little);
                reader.Skip(4);
                DDSHeader header = DDSHeader.Read(reader);

                uint headerSize = header.Size + 4;
                if(header.DXT10Extension.HasValue)
                {
                    headerSize += 20;
                }

                if(!VerifyDDS(header, filename, out uint mip4x4index))
                {
                    return;
                }

                List<DataBlock> blocks = [];

                void CreateDataBlock(uint size)
                {
                    DataBlock block = new()
                    {
                        Length = size
                    };
                    block.EnsureData(null, false);

                    block.DataStream.Write(reader.ReadArray<byte>((int)size));
                    block.DataStream.Position = 0;
                    blocks.Add(block);
                }

                uint width = header.Width;
                for(int i = 0; i < header.MipMapCount; i++, width >>= 1)
                {
                    CreateDataBlock(header.CalculateImageSize(width));
                }

                NTSI info = new()
                {
                    Name = filename,
                    PackageName = outputName,
                    Mip4x4Index = (int)mip4x4index,
                    Mip4x4 = blocks[(int)mip4x4index].DataStream.ToArray(),
                    DdsHeader = file[..(int)headerSize]
                };

                Package.EntryInfo entry = new()
                {
                    Name = Path.GetFileNameWithoutExtension(filename),
                    Width = (ushort)header.Width,
                    Height = (ushort)header.Height,
                    MipLevels = (int)header.MipMapCount,
                    BlockIndex = package.Blocks.Count,
                };

                entry.Hash = Package.EntryInfo.ComputeHash(entry.Name);

                infos.Add(info);
                package.Entries.Add(entry);
                package.Blocks.AddRange(blocks);
            }

            string outDir = Path.Join(inputDirectory, "Output");
            Directory.CreateDirectory(outDir);

            package.Write(Path.Join(outDir, outputName + ".ntsp"));

            foreach(NTSI info in infos)
            {
                info.Write(Path.Join(outDir, info.Name));
            }
        }

        private static bool VerifyDDS(DDSHeader header, string filename, out uint mip4x4index)
        {
            mip4x4index = 0;

            if(header.Width > 0xFFFF)
            {
                Console.WriteLine($"DDS file \"{filename}\" too large! Must be smaller than 65536x65536");
                return false;
            }
            else if(header.Flags.HasFlag(DDSFlags.Depth))
            {
                Console.WriteLine($"DDS texture \"{filename}\" is 3D! Only 2D allowed!");
                return false;
            }
            else if(header.Width != header.Height)
            {
                Console.WriteLine($"DDS texture \"{filename}\" is not squared!");
                return false;
            }

            uint mip4x4size = header.Width;

            if(mip4x4size < 4)
            {
                Console.WriteLine($"DDS file \"{filename}\" is too small! Needs to be at least 4x4 in size!");
                return false;
            }

            while(mip4x4size > 4)
            {
                mip4x4size >>= 1;
                mip4x4index++;
            }

            if(mip4x4index >= header.MipMapCount)
            {
                Console.WriteLine($"DDS file \"{filename}\" has an invalid mipmap count of {header.MipMapCount}! Needs to be at least {mip4x4index + 1} to contain a 4x4 image!");
                return false;
            }

            return true;
        }
    }
}

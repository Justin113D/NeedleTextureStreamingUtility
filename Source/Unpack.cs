using SharpNeedle;
using SharpNeedle.Utilities;
using SharpNeedle.HedgehogEngine.Needle.TextureStreaming;
using System;
using System.IO;
using SharpNeedle.IO;
using System.Collections.Generic;
using NTSI = SharpNeedle.HedgehogEngine.Needle.TextureStreaming.Info;


namespace NeedleTextureStreamingUtility
{
    public static class Unpack
    {
        public static void Run(string ntsiFile, string ntspDirectory)
        {
            string[] ntsiFiles = [];

            if(File.Exists(ntsiFile))
            {
                ntsiFiles = [ntsiFile];
            }
            else if(Directory.Exists(ntsiFile))
            {
                ntsiFiles = Directory.GetFiles(ntsiFile, "*.dds");
            }
            else
            {
                Console.WriteLine("Specified NTSI DDS file/directory does not exist!");
                return;
            }

            Dictionary<string, Package?> packages = [];

            foreach(string ntsiFilePath in ntsiFiles)
            {
                IFile file = FileSystem.Open(ntsiFilePath);
                uint signature = Util.GetSignature(file);

                if(signature != Util.NTSISignature)
                {
                    Console.WriteLine($"\"{Path.GetFileName(ntsiFilePath)}\" is not an NTSI file");
                    continue;
                }

                NTSI info = ResourceManager.Instance.Open<NTSI>(file);
                info.BaseFile.Dispose();

                if(!packages.TryGetValue(info.PackageName, out Package? package))
                {
                    string ntspFile = info.PackageName + ".ntsp";

                    if(!string.IsNullOrEmpty(ntspDirectory))
                    {
                        ntspFile = Path.Join(ntspDirectory, ntspFile);
                    }

                    if(File.Exists(ntspFile))
                    {
                        package = ResourceManager.Instance.Open<Package>(ntspFile);
                    }

                    packages[info.PackageName] = package;
                }

                if(package == null)
                {
                    Console.WriteLine($"The NTSP file \"{info.PackageName}\" specified in \"{Path.GetFileName(ntsiFilePath)}\" was not found");
                    continue;
                }

                byte[] ddsFile = info.UnpackDDS(package);
                File.WriteAllBytes(ntsiFilePath, ddsFile);
                Console.WriteLine($"Successfully unpacked NTSI file \"{Path.GetFileName(ntsiFilePath)}\"");
            }
        }

    }
}

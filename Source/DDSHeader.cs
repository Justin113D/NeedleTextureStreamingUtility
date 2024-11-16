using Amicitia.IO.Binary;
using System;
using System.Linq;
using static SharpNeedle.HedgehogEngine.Mirage.SampleChunkNode;

namespace NeedleTextureStreamingUtility
{
    public struct DDSHeader
    {
        public const uint DXT10Signature = 0x30315844;

        public uint Size { get; set; }
        public DDSFlags Flags { get; set; }
        public uint Height { get; set; }
        public uint Width { get; set; }
        public uint PitchOrLinearSize { get; set; }
        public uint Depth { get; set; }
        public uint MipMapCount { get; set; }
        public DDSPixelFormat PixelFormat { get; set; }
        public DDSCaps Caps { get; set; }
        public DDSCaps2 Caps2 { get; set; }
        public uint Caps3 { get; set; }
        public uint Caps4 { get; set; }

        public DDSDXT10Header? DXT10Extension { get; set; }

        public static DDSHeader Read(BinaryObjectReader reader)
        {
            DDSHeader result = default;

            result.Size = reader.ReadUInt32();
            result.Flags = (DDSFlags)reader.ReadUInt32();
            result.Height = reader.ReadUInt32();
            result.Width = reader.ReadUInt32();
            result.PitchOrLinearSize = reader.ReadUInt32();
            result.Depth = reader.ReadUInt32();
            result.MipMapCount = reader.ReadUInt32();
            reader.Skip(44);
            result.PixelFormat = DDSPixelFormat.Read(reader);
            result.Caps = (DDSCaps)reader.ReadUInt32();
            result.Caps2 = (DDSCaps2)reader.ReadUInt32();
            result.Caps3 = reader.ReadUInt32();
            result.Caps4 = reader.ReadUInt32();
            reader.Skip(4);

            if(result.PixelFormat.Flags.HasFlag(DDSPixelFormatFlags.FourCC) && result.PixelFormat.FourCC == DXT10Signature)
            {
                result.DXT10Extension = DDSDXT10Header.Read(reader);
            }

            return result;
        }

        public uint CalculateImageSize(uint width)
        {
            return DXT10Extension.HasValue 
                ? DXT10Extension.Value.CalculateImageSize(width) 
                : PixelFormat.CalculateImageSize(width);
        }
    }

    [Flags]
    public enum DDSFlags : uint
    {
        Caps = 0x1,
        Height = 0x2,
        Width = 0x4,
        Pitch = 0x8,
        Pixelformat = 0x1000,
        MipmapCount = 0x20000,
        LinearSize = 0x80000,
        Depth = 0x800000
    }

    [Flags]
    public enum DDSCaps : uint
    {
        Complex = 0x8,
        Mipmap = 0x1000,
        Texture = 0x400000,
    }

    [Flags]
    public enum DDSCaps2 : uint
    {
        Cubemap = 0x200,
        CubemapPositiveX = 0x400,
        CubemapNegativeX = 0x800,
        CubemapPositiveY = 0x1000,
        CubemapNegativeY = 0x2000,
        CubemapPositiveZ = 0x4000,
        CubemapNegativeZ = 0x8000,
        Volume = 0x200000,
    }

    public struct DDSPixelFormat
    {
        private const uint _dxt1Signature = 0x31545844;
        private const uint _dxt2Signature = 0x32545844;
        private const uint _dxt3Signature = 0x33545844;
        private const uint _dxt4Signature = 0x34545844;
        private const uint _dxt5Signature = 0x35545844;
        private const uint _bc4uSignature = 0x55344342;
        private const uint _bc4sSignature = 0x53344342;
        private const uint _ati2Signature = 0x32495441;
        private const uint _bc5sSignature = 0x53354342;


        public uint Size { get; set; }
        public DDSPixelFormatFlags Flags { get; set; }
        public uint FourCC { get; set; }
        public uint RGBBitCount { get; set; }
        public uint RBitMask { get; set; }
        public uint GBitMask { get; set; }
        public uint BBitMask { get; set; }
        public uint ABitMask { get; set; }

        public static DDSPixelFormat Read(BinaryObjectReader reader)
        {
            return new DDSPixelFormat()
            {
                Size = reader.ReadUInt32(),
                Flags = (DDSPixelFormatFlags)reader.ReadUInt32(),
                FourCC = reader.ReadUInt32(),
                RGBBitCount = reader.ReadUInt32(),
                RBitMask = reader.ReadUInt32(),
                GBitMask = reader.ReadUInt32(),
                BBitMask = reader.ReadUInt32(),
                ABitMask = reader.ReadUInt32(),
            };
        }

        public uint CalculateImageSize(uint width)
        {
            ulong slice, nbw;

            switch(FourCC)
            {
                case _dxt1Signature:
                    nbw = ulong.Max(1u, (((ulong)width) + 3u) / 4u);
                    slice = nbw * nbw * 8u;
                    break;
                case _dxt2Signature:
                case _dxt3Signature:
                case _dxt4Signature:
                case _dxt5Signature:
                case _bc4uSignature:
                case _bc4sSignature:
                case _ati2Signature:
                case _bc5sSignature:
                    nbw = ulong.Max(1u, (((ulong)width) + 3u) / 4u);
                    slice = nbw * nbw * 16u;
                    break;
                default:
                    slice = ((((ulong)width) * RGBBitCount) + 7u) / 8u * width;
                    break;
            }

            return (uint)slice;
        }
    }

    [Flags]
    public enum DDSPixelFormatFlags : uint
    {
        AlphaPixels = 0x1,
        Alpha = 0x2,
        FourCC = 0x4,
        RGB = 0x40,
        YUV = 0x200,
        Luminance = 0x20000
    }

    public struct DDSDXT10Header
    {
        public DXGIFormat Format { get; set; }
        public DXGIResourceDimension ResourceDimension { get; set; }
        public DXGIMiscFlags MiscFlag { get; set; }
        public uint ArraySize { get; set; }
        public DXGIAlphaMode AlphaMode { get; set; }

        public static DDSDXT10Header Read(BinaryObjectReader reader)
        {
            return new DDSDXT10Header()
            {
                Format = (DXGIFormat)reader.ReadUInt32(),
                ResourceDimension = (DXGIResourceDimension)reader.ReadUInt32(),
                MiscFlag = (DXGIMiscFlags)reader.ReadUInt32(),
                ArraySize = reader.ReadUInt32(),
                AlphaMode = (DXGIAlphaMode)reader.ReadUInt32(),
            };
        }

        public uint CalculateImageSize(uint width)
        {
            // sourced from https://github.com/microsoft/DirectXTex/blob/main/DirectXTex/DirectXTexUtil.cpp
            ulong pitch, slice, nbw;

            switch(Format)
            {
                case DXGIFormat.UNKNOWN:
                    throw new InvalidOperationException("Invalid DXGI format");

                case DXGIFormat.BC1_TYPELESS:
                case DXGIFormat.BC1_UNORM:
                case DXGIFormat.BC1_UNORM_SRGB:
                case DXGIFormat.BC4_TYPELESS:
                case DXGIFormat.BC4_UNORM:
                case DXGIFormat.BC4_SNORM:
                    nbw = ulong.Max(1u, (((ulong)width) + 3u) / 4u);
                    slice = nbw * nbw * 8u;
                    break;

                case DXGIFormat.BC2_TYPELESS:
                case DXGIFormat.BC2_UNORM:
                case DXGIFormat.BC2_UNORM_SRGB:
                case DXGIFormat.BC3_TYPELESS:
                case DXGIFormat.BC3_UNORM:
                case DXGIFormat.BC3_UNORM_SRGB:
                case DXGIFormat.BC5_TYPELESS:
                case DXGIFormat.BC5_UNORM:
                case DXGIFormat.BC5_SNORM:
                case DXGIFormat.BC6H_TYPELESS:
                case DXGIFormat.BC6H_UF16:
                case DXGIFormat.BC6H_SF16:
                case DXGIFormat.BC7_TYPELESS:
                case DXGIFormat.BC7_UNORM:
                case DXGIFormat.BC7_UNORM_SRGB:
                    nbw = ulong.Max(1u, (((ulong)width) + 3u) / 4u);
                    slice = nbw * nbw * 16u;
                    break;

                case DXGIFormat.R8G8_B8G8_UNORM:
                case DXGIFormat.G8R8_G8B8_UNORM:
                case DXGIFormat.YUY2:
                    pitch = ((((ulong)width) + 1u) >> 1) * 4u;
                    slice = pitch * width;
                    break;

                case DXGIFormat.Y210:
                case DXGIFormat.Y216:
                    pitch = ((((ulong)width) + 1u) >> 1) * 8u;
                    slice = pitch * width;
                    break;

                case DXGIFormat.NV12:
                case DXGIFormat.OPAQUE_420:
                    if((width % 2) != 0)
                    {
                        throw new InvalidOperationException("Requires a width alignment of 2.");
                    }

                    pitch = ((((ulong)width) + 1u) >> 1) * 2u;
                    slice = pitch * (width + ((((ulong)width) + 1u) >> 1));
                    break;

                case DXGIFormat.P010:
                case DXGIFormat.P016:
                    if((width % 2) != 0)
                    {
                        throw new InvalidOperationException("Requires a width alignment of 2.");
                    }

                    pitch = ((((ulong)width) + 1u) >> 1) * 4u;
                    slice = pitch * (width + ((((ulong)width) + 1u) >> 1));
                    break;

                case DXGIFormat.NV11:
                    pitch = ((((ulong)width) + 3u) >> 2) * 4u;
                    slice = pitch * width * 2u;
                    break;

                case DXGIFormat.P208:
                    pitch = ((((ulong)width) + 1u) >> 1) * 2u;
                    slice = pitch * width * 2u;
                    break;

                case DXGIFormat.V208:
                    if((width % 2) != 0)
                    {
                        throw new InvalidOperationException("Requires a width alignment of 2.");
                    }

                    slice = width * (width + (((((ulong)width) + 1u) >> 1) * 2u));
                    break;

                case DXGIFormat.V408:
                    slice = width * (width + ((ulong)(width >> 1) * 4u));
                    break;

                default:
                    uint bpp = GetBitsPerPixel();
                    pitch = ((((ulong)width) * bpp) + 7u) / 8u;
                    slice = pitch * width;
                    break;
            }

            return (uint)slice;
        }

        public uint GetBitsPerPixel()
        {
            // sourced from https://github.com/microsoft/DirectXTex/blob/main/DirectXTex/DirectXTexUtil.cpp

            switch(Format)
            {
                case DXGIFormat.R32G32B32A32_TYPELESS:
                case DXGIFormat.R32G32B32A32_FLOAT:
                case DXGIFormat.R32G32B32A32_UINT:
                case DXGIFormat.R32G32B32A32_SINT:
                    return 128;

                case DXGIFormat.R32G32B32_TYPELESS:
                case DXGIFormat.R32G32B32_FLOAT:
                case DXGIFormat.R32G32B32_UINT:
                case DXGIFormat.R32G32B32_SINT:
                    return 96;

                case DXGIFormat.R16G16B16A16_TYPELESS:
                case DXGIFormat.R16G16B16A16_FLOAT:
                case DXGIFormat.R16G16B16A16_UNORM:
                case DXGIFormat.R16G16B16A16_UINT:
                case DXGIFormat.R16G16B16A16_SNORM:
                case DXGIFormat.R16G16B16A16_SINT:
                case DXGIFormat.R32G32_TYPELESS:
                case DXGIFormat.R32G32_FLOAT:
                case DXGIFormat.R32G32_UINT:
                case DXGIFormat.R32G32_SINT:
                case DXGIFormat.R32G8X24_TYPELESS:
                case DXGIFormat.D32_FLOAT_S8X24_UINT:
                case DXGIFormat.R32_FLOAT_X8X24_TYPELESS:
                case DXGIFormat.X32_TYPELESS_G8X24_UINT:
                case DXGIFormat.Y416:
                case DXGIFormat.Y210:
                case DXGIFormat.Y216:
                    return 64;

                case DXGIFormat.R10G10B10A2_TYPELESS:
                case DXGIFormat.R10G10B10A2_UNORM:
                case DXGIFormat.R10G10B10A2_UINT:
                case DXGIFormat.R11G11B10_FLOAT:
                case DXGIFormat.R8G8B8A8_TYPELESS:
                case DXGIFormat.R8G8B8A8_UNORM:
                case DXGIFormat.R8G8B8A8_UNORM_SRGB:
                case DXGIFormat.R8G8B8A8_UINT:
                case DXGIFormat.R8G8B8A8_SNORM:
                case DXGIFormat.R8G8B8A8_SINT:
                case DXGIFormat.R16G16_TYPELESS:
                case DXGIFormat.R16G16_FLOAT:
                case DXGIFormat.R16G16_UNORM:
                case DXGIFormat.R16G16_UINT:
                case DXGIFormat.R16G16_SNORM:
                case DXGIFormat.R16G16_SINT:
                case DXGIFormat.R32_TYPELESS:
                case DXGIFormat.D32_FLOAT:
                case DXGIFormat.R32_FLOAT:
                case DXGIFormat.R32_UINT:
                case DXGIFormat.R32_SINT:
                case DXGIFormat.R24G8_TYPELESS:
                case DXGIFormat.D24_UNORM_S8_UINT:
                case DXGIFormat.R24_UNORM_X8_TYPELESS:
                case DXGIFormat.X24_TYPELESS_G8_UINT:
                case DXGIFormat.R9G9B9E5_SHAREDEXP:
                case DXGIFormat.R8G8_B8G8_UNORM:
                case DXGIFormat.G8R8_G8B8_UNORM:
                case DXGIFormat.B8G8R8A8_UNORM:
                case DXGIFormat.B8G8R8X8_UNORM:
                case DXGIFormat.R10G10B10_XR_BIAS_A2_UNORM:
                case DXGIFormat.B8G8R8A8_TYPELESS:
                case DXGIFormat.B8G8R8A8_UNORM_SRGB:
                case DXGIFormat.B8G8R8X8_TYPELESS:
                case DXGIFormat.B8G8R8X8_UNORM_SRGB:
                case DXGIFormat.AYUV:
                case DXGIFormat.Y410:
                case DXGIFormat.YUY2:
                    return 32;

                case DXGIFormat.P010:
                case DXGIFormat.P016:
                case DXGIFormat.V408:
                    return 24;

                case DXGIFormat.R8G8_TYPELESS:
                case DXGIFormat.R8G8_UNORM:
                case DXGIFormat.R8G8_UINT:
                case DXGIFormat.R8G8_SNORM:
                case DXGIFormat.R8G8_SINT:
                case DXGIFormat.R16_TYPELESS:
                case DXGIFormat.R16_FLOAT:
                case DXGIFormat.D16_UNORM:
                case DXGIFormat.R16_UNORM:
                case DXGIFormat.R16_UINT:
                case DXGIFormat.R16_SNORM:
                case DXGIFormat.R16_SINT:
                case DXGIFormat.B5G6R5_UNORM:
                case DXGIFormat.B5G5R5A1_UNORM:
                case DXGIFormat.A8P8:
                case DXGIFormat.B4G4R4A4_UNORM:
                case DXGIFormat.P208:
                case DXGIFormat.V208:
                    return 16;

                case DXGIFormat.NV12:
                case DXGIFormat.OPAQUE_420:
                case DXGIFormat.NV11:
                    return 12;

                case DXGIFormat.R8_TYPELESS:
                case DXGIFormat.R8_UNORM:
                case DXGIFormat.R8_UINT:
                case DXGIFormat.R8_SNORM:
                case DXGIFormat.R8_SINT:
                case DXGIFormat.A8_UNORM:
                case DXGIFormat.BC2_TYPELESS:
                case DXGIFormat.BC2_UNORM:
                case DXGIFormat.BC2_UNORM_SRGB:
                case DXGIFormat.BC3_TYPELESS:
                case DXGIFormat.BC3_UNORM:
                case DXGIFormat.BC3_UNORM_SRGB:
                case DXGIFormat.BC5_TYPELESS:
                case DXGIFormat.BC5_UNORM:
                case DXGIFormat.BC5_SNORM:
                case DXGIFormat.BC6H_TYPELESS:
                case DXGIFormat.BC6H_UF16:
                case DXGIFormat.BC6H_SF16:
                case DXGIFormat.BC7_TYPELESS:
                case DXGIFormat.BC7_UNORM:
                case DXGIFormat.BC7_UNORM_SRGB:
                case DXGIFormat.AI44:
                case DXGIFormat.IA44:
                case DXGIFormat.P8:
                    return 8;

                case DXGIFormat.R1_UNORM:
                    return 1;

                case DXGIFormat.BC1_TYPELESS:
                case DXGIFormat.BC1_UNORM:
                case DXGIFormat.BC1_UNORM_SRGB:
                case DXGIFormat.BC4_TYPELESS:
                case DXGIFormat.BC4_UNORM:
                case DXGIFormat.BC4_SNORM:
                    return 4;

                default:
                    return 0;
            }
        }
    }

    public enum DXGIFormat : uint
    {
        UNKNOWN = 0,
        R32G32B32A32_TYPELESS = 1,
        R32G32B32A32_FLOAT = 2,
        R32G32B32A32_UINT = 3,
        R32G32B32A32_SINT = 4,
        R32G32B32_TYPELESS = 5,
        R32G32B32_FLOAT = 6,
        R32G32B32_UINT = 7,
        R32G32B32_SINT = 8,
        R16G16B16A16_TYPELESS = 9,
        R16G16B16A16_FLOAT = 10,
        R16G16B16A16_UNORM = 11,
        R16G16B16A16_UINT = 12,
        R16G16B16A16_SNORM = 13,
        R16G16B16A16_SINT = 14,
        R32G32_TYPELESS = 15,
        R32G32_FLOAT = 16,
        R32G32_UINT = 17,
        R32G32_SINT = 18,
        R32G8X24_TYPELESS = 19,
        D32_FLOAT_S8X24_UINT = 20,
        R32_FLOAT_X8X24_TYPELESS = 21,
        X32_TYPELESS_G8X24_UINT = 22,
        R10G10B10A2_TYPELESS = 23,
        R10G10B10A2_UNORM = 24,
        R10G10B10A2_UINT = 25,
        R11G11B10_FLOAT = 26,
        R8G8B8A8_TYPELESS = 27,
        R8G8B8A8_UNORM = 28,
        R8G8B8A8_UNORM_SRGB = 29,
        R8G8B8A8_UINT = 30,
        R8G8B8A8_SNORM = 31,
        R8G8B8A8_SINT = 32,
        R16G16_TYPELESS = 33,
        R16G16_FLOAT = 34,
        R16G16_UNORM = 35,
        R16G16_UINT = 36,
        R16G16_SNORM = 37,
        R16G16_SINT = 38,
        R32_TYPELESS = 39,
        D32_FLOAT = 40,
        R32_FLOAT = 41,
        R32_UINT = 42,
        R32_SINT = 43,
        R24G8_TYPELESS = 44,
        D24_UNORM_S8_UINT = 45,
        R24_UNORM_X8_TYPELESS = 46,
        X24_TYPELESS_G8_UINT = 47,
        R8G8_TYPELESS = 48,
        R8G8_UNORM = 49,
        R8G8_UINT = 50,
        R8G8_SNORM = 51,
        R8G8_SINT = 52,
        R16_TYPELESS = 53,
        R16_FLOAT = 54,
        D16_UNORM = 55,
        R16_UNORM = 56,
        R16_UINT = 57,
        R16_SNORM = 58,
        R16_SINT = 59,
        R8_TYPELESS = 60,
        R8_UNORM = 61,
        R8_UINT = 62,
        R8_SNORM = 63,
        R8_SINT = 64,
        A8_UNORM = 65,
        R1_UNORM = 66,
        R9G9B9E5_SHAREDEXP = 67,
        R8G8_B8G8_UNORM = 68,
        G8R8_G8B8_UNORM = 69,
        BC1_TYPELESS = 70,
        BC1_UNORM = 71,
        BC1_UNORM_SRGB = 72,
        BC2_TYPELESS = 73,
        BC2_UNORM = 74,
        BC2_UNORM_SRGB = 75,
        BC3_TYPELESS = 76,
        BC3_UNORM = 77,
        BC3_UNORM_SRGB = 78,
        BC4_TYPELESS = 79,
        BC4_UNORM = 80,
        BC4_SNORM = 81,
        BC5_TYPELESS = 82,
        BC5_UNORM = 83,
        BC5_SNORM = 84,
        B5G6R5_UNORM = 85,
        B5G5R5A1_UNORM = 86,
        B8G8R8A8_UNORM = 87,
        B8G8R8X8_UNORM = 88,
        R10G10B10_XR_BIAS_A2_UNORM = 89,
        B8G8R8A8_TYPELESS = 90,
        B8G8R8A8_UNORM_SRGB = 91,
        B8G8R8X8_TYPELESS = 92,
        B8G8R8X8_UNORM_SRGB = 93,
        BC6H_TYPELESS = 94,
        BC6H_UF16 = 95,
        BC6H_SF16 = 96,
        BC7_TYPELESS = 97,
        BC7_UNORM = 98,
        BC7_UNORM_SRGB = 99,
        AYUV = 100,
        Y410 = 101,
        Y416 = 102,
        NV12 = 103,
        P010 = 104,
        P016 = 105,
        OPAQUE_420 = 106,
        YUY2 = 107,
        Y210 = 108,
        Y216 = 109,
        NV11 = 110,
        AI44 = 111,
        IA44 = 112,
        P8 = 113,
        A8P8 = 114,
        B4G4R4A4_UNORM = 115,
        P208 = 130,
        V208 = 131,
        V408 = 132,
        SAMPLER_FEEDBACK_MIN_MIP_OPAQUE,
        SAMPLER_FEEDBACK_MIP_REGION_USED_OPAQUE,
        FORCE_UINT = 0xffffffff
    };

    public enum DXGIResourceDimension : uint
    {
        Unknown = 0,
        Buffer = 1,
        Texture1D = 2,
        Texture2D = 3,
        Texture3D = 4
    };

    [Flags]
    public enum DXGIMiscFlags : uint
    {
        GenerateMips = 0x1,
        Shared = 0x2,
        Texturecube = 0x4,
        DrawindirectArgs = 0x10,
        BufferAllowRawViews = 0x20,
        BufferStructured = 0x40,
        ResourceClamp = 0x80,
        SharedKeyedmutex = 0x100,
        GdiCompatible = 0x200,
        SharedNthandle = 0x800,
        RestrictedContent = 0x1000,
        RestrictSharedResource = 0x2000,
        RestrictSharedResourceDriver = 0x4000,
        Guarded = 0x8000,
        Unknown = 0x10000,
        TilePool = 0x20000,
        Tiled = 0x40000,
        HWProtected = 0x80000,
    }

    public enum DXGIAlphaMode : uint
    {
        Unknown = 0,
        Straight = 1,
        Premultiplied = 2,
        Opaque = 3,
        Custom = 4
    }
}

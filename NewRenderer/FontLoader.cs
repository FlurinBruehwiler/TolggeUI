using System.Reflection;
using System.Runtime.InteropServices;
using StbTrueTypeSharp;

namespace NewRenderer;

public struct GlyphBoundingBox
{
    public int x0;
    public int y0;
    public int x1;
    public int y1;
}

public class Font
{
    public required string Name;
    public required byte[] AtlasBitmap;
    public required int AtlasWidth;
    public required int AtlasHeight;
    public required Dictionary<char, GlyphInfo> GlyphInfos;

    public required float Ascent;
    public required float Descent;
    public required float LineGap;
}

public struct GlyphInfo
{
    public required int XAtlasOffset; //the TopLeft Coordinate of the Glyph within the FontAtlas
    public required int Width; //Width of the glyph
    public required int Height; //height of the glyph
    public required int XOff;
    public required int YOff;

    public required float AdvanceWidth;
    public required float LeftSideBearing;

    public required GlyphBoundingBox GlyphBoundingBox;
}

public unsafe struct CharInfo
{
    public required byte* Bitmap;
    public required int Width;
    public required int Height;
    public required int XOff;
    public required int YOff;
    public required char Char;


    public Span<byte> BitmapAsSpan()
    {
        return new Span<byte>(Bitmap, Width * Height);
    }
}

public class FontLoader
{
    public static unsafe Font LoadFont(string name)
    {
        var info = new StbTrueType.stbtt_fontinfo();

        var asm = Assembly.GetExecutingAssembly();
        using var stream = asm.GetManifestResourceStream($"NewRenderer.{name}");
        using MemoryStream ms = new MemoryStream();
        stream!.CopyTo(ms);
        var fontData = ms.ToArray();

        var ptr = Marshal.AllocHGlobal(fontData.Length);
        var unmanagedSpan = new Span<byte>((byte*)ptr, fontData.Length);
        fontData.CopyTo(unmanagedSpan);

        StbTrueType.stbtt_InitFont(info, (byte*)ptr, 0);

        CharInfo[] charInfos = new CharInfo['~' - ' '];

        var scale = StbTrueType.stbtt_ScaleForPixelHeight(info, 20);

        int ascent = 0;
        int descent = 0;
        int lineGap = 0;
        StbTrueType.stbtt_GetFontVMetrics(info, &ascent, &descent, &lineGap);

        for (char i = ' '; i < '~'; i += '\x1')
        {
            int width = 0;
            int height = 0;
            int xOff = 0;
            int yOff = 0;
            var bitmap = StbTrueType.stbtt_GetCodepointBitmap(info, 0, scale, i, &width, &height, &xOff, &yOff);

            charInfos[i - ' '] = new CharInfo
            {
                Bitmap = bitmap,
                Width = width,
                Height = height,
                XOff = xOff,
                YOff = yOff,
                Char = i,
            };
        }

        var maxHeight = 1000; //charInfos.Max(x => x.Height);
        var totalWidth = 1000; charInfos.Sum(x => x.Width);

        byte[] fontAtlasBitmap = new byte[maxHeight * totalWidth];
        var glyphInfos = new Dictionary<char, GlyphInfo>(charInfos.Length);

        int currentXOffset = 0;

        for (var i = 0; i < charInfos.Length; i++)
        {
            var c = charInfos[i];

            var bb = new GlyphBoundingBox();

            StbTrueType.stbtt_GetCodepointBitmapBox(info, i, 0, scale, &bb.x0, &bb.y0, &bb.x1, &bb.y1);

            int advanceWidth = 0;
            int leftSideBearing = 0;

            StbTrueType.stbtt_GetCodepointHMetrics(info, i, &advanceWidth, &leftSideBearing);

            glyphInfos[c.Char] = new GlyphInfo
            {
                Height = c.Height,
                Width = c.Width,
                XOff = c.XOff,
                YOff = c.YOff,
                XAtlasOffset = currentXOffset,
                GlyphBoundingBox = bb,
                AdvanceWidth = advanceWidth * scale,
                LeftSideBearing = leftSideBearing * scale
            };

            var bitmap = c.BitmapAsSpan();

            for (int j = 0; j < c.Height; j++)
            {
                var destSpan = fontAtlasBitmap.AsSpan(j * totalWidth + currentXOffset);
                var srcSpan = bitmap.Slice(j * c.Width, c.Width);

                srcSpan.CopyTo(destSpan);
            }

            currentXOffset += c.Width + 1;
        }

        return new Font
        {
            Name = name,
            AtlasBitmap = fontAtlasBitmap,
            AtlasWidth = totalWidth,
            AtlasHeight = maxHeight,
            GlyphInfos = glyphInfos,
            Ascent = ascent * scale,
            Descent = descent * scale,
            LineGap = lineGap * scale
        };
    }
}
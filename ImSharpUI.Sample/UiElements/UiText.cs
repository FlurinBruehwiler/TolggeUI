﻿using SkiaSharp;

namespace ImSharpUISample.UiElements;

public class UiText : UiElement
{
    public string Content { get; set; }

    public float PSize { get; set; } = 15;

    public ColorDefinition PColor { get; set; } = new(255, 255, 255);

    public TextAlign PhAlign { get; set; } = TextAlign.Start;

    public TextAlign PvAlign { get; set; } = TextAlign.Start;

    private static readonly SKPaint Paint = new()
    {
        IsAntialias = true,
        Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyleWeight.Normal, SKFontStyleWidth.Normal,
            SKFontStyleSlant.Upright)
    };

    public override void Render(SKCanvas canvas)
    {
        if (Content == string.Empty)
            return;

        Paint.TextSize = PSize;
        Paint.Color = new SKColor(PColor.Red, PColor.Green, PColor.Blue, PColor.Appha);

        var path = Paint.GetTextPath(Content, ComputedX, ComputedY);
        path.GetBounds(out var rect);

        Paint.GetFontMetrics(out var metrics);

        var actualX = ComputedX;
        var actualY = ComputedY;

        actualY += PvAlign switch
        {
            TextAlign.Start => PSize,
            TextAlign.End => ComputedHeight,
            TextAlign.Center => ComputedHeight / 2 - (metrics.Ascent + metrics.Descent) / 2,
            _ => throw new ArgumentOutOfRangeException()
        };

        actualX += PhAlign switch
        {
            TextAlign.End => ComputedWidth - rect.Width,
            TextAlign.Center => ComputedWidth / 2 - rect.Width / 2,
            TextAlign.Start => 0,
            _ => throw new ArgumentOutOfRangeException()
        };

        canvas.DrawText(Content, actualX, actualY, Paint);
    }

    public override void Layout(UiWindow uiWindow)
    {
    }

    public override bool LayoutHasChanged()
    {
        throw new NotImplementedException();
    }

    public override bool HasChanges()
    {
        throw new NotImplementedException();
    }

    public UiText Width(float width, SizeKind sizeKind = SizeKind.Pixel)
    {
        PWidth = new SizeDefinition(width, sizeKind);
        return this;
    }

    public UiText Height(float height, SizeKind sizeKind = SizeKind.Pixel)
    {
        PHeight = new SizeDefinition(height, sizeKind);
        return this;
    }

    public UiText Color(byte red, byte green, byte blue, byte transparency = 255)
    {
        PColor = new ColorDefinition(red, green, blue, transparency);
        return this;
    }

    public UiText Color(ColorDefinition color)
    {
        PColor = color;
        return this;
    }

    public UiText Size(float size)
    {
        PSize = size;
        return this;
    }

    public UiText HAlign(TextAlign textAlign)
    {
        PhAlign = textAlign;
        return this;
    }

    public UiText VAlign(TextAlign textAlign)
    {
        PvAlign = textAlign;
        return this;
    }
}

public enum TextAlign
{
    Start,
    Center,
    End
}

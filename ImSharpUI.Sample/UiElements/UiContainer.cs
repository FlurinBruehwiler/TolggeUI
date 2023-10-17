﻿using System.Numerics;
using SkiaSharp;
using EnumXAlign = ImSharpUISample.XAlign;
using EnumMAlign = ImSharpUISample.MAlign;
using EnumDir = ImSharpUISample.Dir;

namespace ImSharpUISample.UiElements;

public partial class UiContainer : UiElementContainer, IUiContainerBuilder
{

    public bool FocusIn { get; set; }
    public bool FocusOut { get; set; }

    public bool Clicked
    {
        get
        {
            if (Ui.Window is null)
                throw new Exception();

            if (!Ui.Window.IsMouseButtonPressed(MouseButtonKind.Left))
                return false;

            if (DivContainsPoint(Ui.Window.MousePosition))
            {
                return true;
            }

            return false;
        }
    }

    public int PZIndex { get; set; }
    public bool PFocusable { get; set; }
    public bool IsNew { get; set; } = true;
    public ColorDefinition? PColor { get; set; }
    public ColorDefinition? PBorderColor { get; set; }
    public Quadrant PPadding { get; set; } = new(0, 0, 0, 0);
    public int PGap { get; set; }
    public int PRadius { get; set; }
    public int PBorderWidth { get; set; }
    public EnumDir PDir { get; set; } = EnumDir.Vertical;
    public MAlign PmAlign { get; set; } = EnumMAlign.FlexStart;
    public XAlign PxAlign { get; set; } = EnumXAlign.FlexStart;
    public Action? POnClick { get; set; }
    public bool PAutoFocus { get; set; }
    public bool PAbsolute { get; set; }
    public bool DisablePositioning { get; set; }
    public UiContainer? AbsoluteContainer { get; set; }

    public bool PHidden { get; set; }

    public Quadrant PAbsolutePosition { get; set; } = new(0, 0, 0, 0);

    public bool IsHovered
    {
        get
        {
            if (Ui.Window is null)
                throw new Exception();

            if (DivContainsPoint(Ui.Window.MousePosition))
            {
                return true;
            }

            return false;
        }
    }

    public bool HasFocusWithin
    {
        get
        {
            if (IsActive)
                return true;

            foreach (var uiElement in OldChildrenById)
            {
                if (uiElement.Value is UiContainer { IsActive: true })
                    return true;
            }

            return false;
        }
    }


    public bool IsActive { get; set; }
    public bool PCanScroll { get; set; }
    public float ScrollPos { get; set; }
    public bool IsClipped { get; set; }

    private bool DivContainsPoint(Vector2 pos)
    {
        return ComputedX <= pos.X && ComputedX + PComputedWidth >= pos.X && ComputedY <= pos.Y &&
               ComputedY + PComputedHeight >= pos.Y;
    }

    public override void CloseElement()
    {
        if (PAbsolute)
        {
            Ui.AbsoluteDivs.Add(this);
        }
    }

    public override void Render(SKCanvas canvas)
    {
        if (PColor is { } color)
        {
            if (PRadius != 0)
            {
                canvas.DrawRoundRect(ComputedX, ComputedY, PComputedWidth, PComputedHeight, PRadius, PRadius,
                    GetColor(color));
            }
            else
            {
                canvas.DrawRect(ComputedX, ComputedY, PComputedWidth, PComputedHeight,
                    GetColor(color));
            }
        }
        if (PBorderWidth != 0 && PBorderColor is {} borderColor)
        {
            canvas.Save();

            if (PRadius != 0)
            {
                float borderRadius = PRadius + PBorderWidth;

                canvas.ClipRoundRect(
                    new SKRoundRect(SKRect.Create(ComputedX, ComputedY, PComputedWidth, PComputedHeight), PRadius), SKClipOperation.Difference,
                    antialias: true);

                canvas.DrawRoundRect(ComputedX - PBorderWidth,
                    ComputedY - PBorderWidth,
                    PComputedWidth + 2 * PBorderWidth,
                    PComputedHeight + 2 * PBorderWidth,
                    borderRadius,
                    borderRadius,
                    GetColor(borderColor));
            }
            else
            {
                canvas.ClipRect(SKRect.Create(ComputedX, ComputedY, PComputedWidth, PComputedHeight), SKClipOperation.Difference, true);

                canvas.DrawRect(ComputedX - PBorderWidth, ComputedY - PBorderWidth,
                    PComputedWidth + 2 * PBorderWidth, PComputedHeight + 2 * PBorderWidth,
                    GetColor(borderColor));
            }

            canvas.Restore();
        }

        canvas.Save();

        if (PCanScroll || IsClipped)
        {
            if (PRadius != 0)
            {
                canvas.ClipRoundRect(
                    new SKRoundRect(SKRect.Create(ComputedX, ComputedY, PComputedWidth, PComputedHeight), PRadius),
                    antialias: true);
            }
            else
            {
                canvas.ClipRect(SKRect.Create(ComputedX, ComputedY, PComputedWidth, PComputedHeight));
            }
        }

        foreach (var childElement in Children)
        {
            if (childElement is UiContainer { PHidden: true })
            {
                continue;
            }

            //if differenz Z-index, defer rendering
            if (childElement is UiContainer uiContainer && uiContainer.PZIndex != 0)
            {
                Ui.DeferedRenderedContainers.Add(uiContainer);
                continue;
            }

            childElement.Render(canvas);
        }

        canvas.Restore();
    }


    public override void Layout(UiWindow uiWindow)
    {
        IsNew = false;

        ComputeSize();

        var contentSize = ComputePosition();

        if (PCanScroll)
        {
            if (contentSize > PComputedHeight)
            {
                ScrollPos = Math.Clamp(ScrollPos + uiWindow.ScrollDelta * 20, 0, contentSize - PComputedHeight);
            }
            else
            {
                ScrollPos = 0;
            }
        }

        foreach (var childElement in Children)
        {
            if (childElement is UiContainer { PHidden: true })
            {
                continue;
            }

            childElement.ComputedY -= ScrollPos;
            childElement.Layout(uiWindow);
        }
    }

    public bool ContainsPoint(double x, double y)
    {
        return ComputedX <= x && ComputedX + PComputedWidth >= x && ComputedY <= y &&
               ComputedY + PComputedHeight >= y;
    }
}

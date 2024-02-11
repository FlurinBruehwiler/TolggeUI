﻿using SkiaSharp;
using EnumXAlign = Flamui.XAlign;
using EnumMAlign = Flamui.MAlign;
using EnumDir = Flamui.Dir;

namespace Flamui.UiElements;

public partial class UiContainer
{
    private static readonly SKPaint SPaint = new()
    {
        IsAntialias = true
    };

    private static readonly SKPaint SBlurPaint = new()
    {
        IsAntialias = true
    };

    private void ComputeSize()
    {
        //1. layout() fixed size and shrinkable children
        //2. Calculate size of remaining percentage based children
        //3. layout() percentage children

        foreach (var child in Children)
        {
            if (child.PShrinkHeight)
            {
                child.Layout();
            }
        }

        switch (PDir)
        {
            case EnumDir.Horizontal:
                ComputeRowSize();
                break;
            case EnumDir.Vertical:
                ComputeColumnSize();
                break;
        }

        foreach (var child in Children)
        {
            if (!child.PShrinkHeight)
            {
                child.Layout();
            }
        }
    }

    private Size ComputePosition()
    {
        switch (PmAlign)
        {
            case EnumMAlign.FlexStart:
                return RenderFlexStart();
            case EnumMAlign.FlexEnd:
                return RenderFlexEnd();
            case EnumMAlign.Center:
                return RenderCenter();
            case EnumMAlign.SpaceBetween:
                return RenderSpaceBetween();
            case EnumMAlign.SpaceAround:
                return RenderSpaceAround();
            case EnumMAlign.SpaceEvenly:
                return RenderSpaceEvenly();
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private float GetCrossAxisOffset(UiElement item)
    {
        return PxAlign switch
        {
            EnumXAlign.FlexStart => 0,
            EnumXAlign.FlexEnd => GetCrossAxisLength() - GetItemCrossAxisLength(item),
            EnumXAlign.Center => GetCrossAxisLength() / 2 - GetItemCrossAxisLength(item) / 2,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private float GetMainAxisLength()
    {
        return PDir switch
        {
            EnumDir.Horizontal => ComputedBounds.W - (PPadding.Left + PPadding.Right),
            EnumDir.Vertical => ComputedBounds.H - (PPadding.Top + PPadding.Bottom),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private float GetCrossAxisLength()
    {
        return PDir switch
        {
            EnumDir.Horizontal => ComputedBounds.H - (PPadding.Top + PPadding.Bottom),
            EnumDir.Vertical => ComputedBounds.W - (PPadding.Left + PPadding.Right),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private float GetItemMainAxisLength(UiElement item)
    {
        return PDir switch
        {
            EnumDir.Horizontal => item.ComputedBounds.W,
            EnumDir.Vertical => item.ComputedBounds.H,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private float GetItemMainAxisFixedLength(UiElement item)
    {
        switch (PDir)
        {
            case EnumDir.Horizontal:
                if (item.PWidth.Kind == SizeKind.Percentage)
                    return 0;

                if (item.PShrinkHeight)
                    return item.ComputedBounds.W;

                return item.PWidth.GetDpiAwareValue();
            case EnumDir.Vertical:
                if (item.PHeight.Kind == SizeKind.Percentage)
                    return 0;

                if (item.PShrinkHeight)
                    return item.ComputedBounds.W;

                return item.PHeight.GetDpiAwareValue();
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private float GetItemCrossAxisLength(UiElement item)
    {
        return PDir switch
        {
            EnumDir.Horizontal => item.ComputedBounds.H,
            EnumDir.Vertical => item.ComputedBounds.W,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private void ComputeColumnSize()
    {
        var remainingSize = RemainingMainAxisFixedSize();

        var totalPercentage = 0f;

        foreach (var child in Children)
        {
            if (child is not UiContainer { PAbsolute: true })
            {
                if (child.PHeight.Kind == SizeKind.Percentage)
                {
                    totalPercentage += child.PHeight.Value;
                }
            }
        }

        float sizePerPercent;

        if (totalPercentage > 100)
        {
            sizePerPercent = remainingSize / totalPercentage;
        }
        else
        {
            sizePerPercent = remainingSize / 100;
        }

        foreach (var item in Children)
        {
            if (item is UiContainer { PAbsolute: true })
            {
                CalculateAbsoluteSize(item);
                continue;
            }

            if (!item.PShrinkHeight)
            {
                item.ComputedBounds.H = item.PHeight.Kind switch
                {
                    SizeKind.Percentage => item.PHeight.Value * sizePerPercent,
                    SizeKind.Pixel => item.PHeight.GetDpiAwareValue(),
                    _ => throw new ArgumentOutOfRangeException()
                };
            }

            if (!item.PShirnkWidth)
            {
                item.ComputedBounds.W = item.PWidth.Kind switch
                {
                    SizeKind.Pixel => item.PWidth.GetDpiAwareValue(),
                    SizeKind.Percentage => (float)((ComputedBounds.W - (PPadding.Left + PPadding.Right)) *
                                                   item.PWidth.Value * 0.01),
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
        }
    }

    private void ComputeRowSize()
    {
        var remainingSize = RemainingMainAxisFixedSize();
        var totalPercentage = 0f;

        foreach (var child in Children)
        {
            if (child is UiContainer { PAbsolute: true })
                continue;

            if (child.PWidth.Kind == SizeKind.Percentage)
            {
                totalPercentage += child.PWidth.Value;
            }
        }

        float sizePerPercent;

        if (totalPercentage > 100)
        {
            sizePerPercent = remainingSize / totalPercentage;
        }
        else
        {
            sizePerPercent = remainingSize / 100;
        }

        foreach (var item in Children)
        {
            if (item is UiContainer { PAbsolute: true })
            {
                CalculateAbsoluteSize(item);
                continue;
            }

            if (!item.PShirnkWidth)
            {
                item.ComputedBounds.W = item.PWidth.Kind switch
                {
                    SizeKind.Percentage => item.PWidth.Value * sizePerPercent,
                    SizeKind.Pixel => item.PWidth.GetDpiAwareValue(),
                    _ => throw new ArgumentOutOfRangeException()
                };
            }

            if (!item.PShrinkHeight)
            {
                item.ComputedBounds.H = item.PHeight.Kind switch
                {
                    SizeKind.Pixel => item.PHeight.GetDpiAwareValue(),
                    SizeKind.Percentage => (float)(ComputedBounds.H * item.PHeight.Value * 0.01 -
                                                   (PPadding.Top + PPadding.Bottom)),
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
        }
    }

    private void PositionAbsoluteItem(UiContainer item)
    {
        if (item.DisablePositioning)
            return;

        var parent = this;
        if (item is { AbsoluteContainer: not null } p)
            parent = p.AbsoluteContainer;

        var horizontalOffset = 0f;

        if (item.PAbsolutePosition.Left != null)
        {
            horizontalOffset = item.PAbsolutePosition.Left.Value;
        }

        if (item.PAbsolutePosition.Right != null)
        {
            horizontalOffset = parent.ComputedBounds.W + item.PAbsolutePosition.Right.Value;
        }
        item.ComputedBounds.X = parent.ComputedBounds.X + horizontalOffset;
        item.ComputedBounds.Y = parent.ComputedBounds.Y + (item.PAbsolutePosition.Top ?? 0);
    }

    private void CalculateAbsoluteSize(UiElement item)
    {
        var parent = this;
        if (item is UiContainer { AbsoluteContainer: not null } p)
            parent = p.AbsoluteContainer;

        item.ComputedBounds.W = item.PWidth.Kind switch
        {
            SizeKind.Percentage => item.PWidth.Value * (parent.ComputedBounds.W / 100),
            SizeKind.Pixel => item.PWidth.GetDpiAwareValue(),
            _ => throw new ArgumentOutOfRangeException()
        };
        item.ComputedBounds.H = item.PHeight.Kind switch
        {
            SizeKind.Pixel => item.PHeight.GetDpiAwareValue(),
            SizeKind.Percentage => item.PHeight.Value * (parent.ComputedBounds.H / 100),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private float RemainingMainAxisFixedSize()
    {
        var childSum = 0f;

        foreach (var child in Children)
        {
            if (child is UiContainer { PAbsolute: true })
                continue;

            childSum += GetItemMainAxisFixedLength(child);
        }

        return GetMainAxisLength() - childSum - GetGapSize();
    }

    private float GetGapSize()
    {
        if (Children.Count <= 1)
            return 0;

        return (Children.Count - 1) * PGap;
    }

    private float RemainingMainAxisSize()
    {
        var sum = 0f;

        foreach (var child in Children)
        {
            if (child is UiContainer { PAbsolute: true })
                continue;

            sum += GetItemMainAxisLength(child);
        }

        sum += PGap * (Children.Count - 1);

        return GetMainAxisLength() - sum;
    }

    private Size RenderFlexStart()
    {
        var mainOffset = 0f;
        var crossSize = 0f;

        foreach (var child in Children)
        {
            if (child is UiContainer { PAbsolute: true } divChild)
            {
                PositionAbsoluteItem(divChild);
                continue;
            }

            DrawWithMainOffset(mainOffset, child);
            mainOffset += GetItemMainAxisLength(child) + PGap;

            var childCrossSize = GetItemCrossAxisLength(child);
            if (childCrossSize > crossSize)
                crossSize += childCrossSize;
        }

        var mainSize = mainOffset - PGap;

        var vertialPadding = PPadding.Top + PPadding.Bottom;
        var horizontalPadding = PPadding.Left + PPadding.Right;

        return PDir switch
        {
            EnumDir.Horizontal => new Size(mainSize + vertialPadding, crossSize + horizontalPadding),
            EnumDir.Vertical => new Size(crossSize + vertialPadding, mainSize + horizontalPadding),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private Size RenderFlexEnd()
    {
        var mainOffset = RemainingMainAxisSize();

        foreach (var child in Children)
        {
            if (child is UiContainer { PAbsolute: true } divChild)
            {
                PositionAbsoluteItem(divChild);
                continue;
            }

            DrawWithMainOffset(mainOffset, child);
            mainOffset += GetItemMainAxisLength(child) + PGap;
        }

        return new Size();
    }

    private Size RenderCenter()
    {
        var mainOffset = RemainingMainAxisSize() / 2;

        foreach (var child in Children)
        {
            if (child is UiContainer { PAbsolute: true } divChild)
            {
                PositionAbsoluteItem(divChild);
                continue;
            }

            DrawWithMainOffset(mainOffset, child);
            mainOffset += GetItemMainAxisLength(child) + PGap;
        }

        return new Size();
    }

    private Size RenderSpaceBetween()
    {
        var totalRemaining = RemainingMainAxisSize();
        var space = totalRemaining / (Children.Count - 1);

        var mainOffset = 0f;

        foreach (var child in Children)
        {
            if (child is UiContainer { PAbsolute: true } divChild)
            {
                PositionAbsoluteItem(divChild);
                continue;
            }

            DrawWithMainOffset(mainOffset, child);
            mainOffset += GetItemMainAxisLength(child) + space + PGap;
        }

        return new Size();
    }

    private Size RenderSpaceAround()
    {
        var totalRemaining = RemainingMainAxisSize();
        var space = totalRemaining / Children.Count / 2;

        var mainOffset = 0f;

        foreach (var child in Children)
        {
            if (child is UiContainer { PAbsolute: true } divChild)
            {
                PositionAbsoluteItem(divChild);
                continue;
            }

            mainOffset += space;
            DrawWithMainOffset(mainOffset, child);
            mainOffset += GetItemMainAxisLength(child) + space + PGap;
        }

        return new Size();
    }

    private Size RenderSpaceEvenly()
    {
        var totalRemaining = RemainingMainAxisSize();
        var space = totalRemaining / (Children.Count + 1);

        var mainOffset = space;

        foreach (var child in Children)
        {
            if (child is UiContainer { PAbsolute: true } divChild)
            {
                PositionAbsoluteItem(divChild);
                continue;
            }

            DrawWithMainOffset(mainOffset, child);
            mainOffset += GetItemMainAxisLength(child) + space + PGap;
        }

        return new Size();
    }

    private void DrawWithMainOffset(float mainOffset, UiElement item)
    {
        switch (PDir)
        {
            case EnumDir.Horizontal:
                item.ComputedBounds.X = mainOffset;
                item.ComputedBounds.Y = GetCrossAxisOffset(item);
                break;
            case EnumDir.Vertical:
                item.ComputedBounds.X = GetCrossAxisOffset(item);
                item.ComputedBounds.Y = mainOffset;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        item.ComputedBounds.X += ComputedBounds.X + PPadding.Left;
        item.ComputedBounds.Y += ComputedBounds.Y + PPadding.Top;
    }

    public override bool LayoutHasChanged()
    {
        throw new NotImplementedException();
    }

    public override bool HasChanges()
    {
        throw new NotImplementedException();
    }
}

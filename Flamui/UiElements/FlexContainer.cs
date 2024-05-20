﻿using Flamui.Layouting;

namespace Flamui.UiElements;

public partial class FlexContainer : UiElementContainer
{
    public bool FocusIn { get; } //todo
    public bool FocusOut { get; } //todo

    public FlexContainerInfo Info = new();

    public override void Reset()
    {
        Info = new();
        base.Reset();
    }

    public bool IsClicked
    {
        get
        {
            if (Window is null)
                throw new Exception();

            if (!Window.IsMouseButtonPressed(MouseButtonKind.Left))
                return false;

            if (Window.HoveredElements.Contains(this))
            {
                return true;
            }

            return false;
        }
    }

    public bool IsDoubleClicked //todo
    {
        get
        {
            return true;
        }
    }



    public bool IsHovered
    {
        get
        {
            if (Window is null)
                throw new Exception();

            return Window.HoveredElements.Contains(this);
        }
    }

    public bool IsNewlyHovered
    {
        get
        {
            if (Window is null)
                throw new Exception();

            return !Window.OldHoveredElements.Contains(this) && Window.HoveredElements.Contains(this);
        }
    }

    public bool IsNewlyUnHovered
    {
        get
        {
            if (Window is null)
                throw new Exception();

            return Window.OldHoveredElements.Contains(this) && !Window.HoveredElements.Contains(this);
        }
    }

    public bool HasFocusWithin
    {
        get
        {
            if (IsActive)
                return true;

            foreach (var uiElement in DataStore.OldDataById) //ToDo, maybe old children again
            {
                if (uiElement.Value is FlexContainer { IsActive: true })
                    return true;
            }

            return false;
        }
    }

    public override void Render(RenderContext renderContext, Point offset)
    {
        FlexContainerRenderer.Render(renderContext, this, offset);
    }

    public BoxSize ContentSize;

    private readonly EmptyStack _scrollBarContainer = new();


    private float LayoutScrollbar()
    {
        //TODO pls refactor this very ugly code!!!!!!!!!!!!!!!

        var scrollbar = Window.Ui.GetData(Id, this, static (_, _, scrollContainer) =>
        {
            var comp = new Scrollbar(new ScrollService(scrollContainer), new ScrollbarSettings
            {
                Width = 10,
                MinTrackSize = 50,
                ThumbColor = new ColorDefinition(92, 92, 92),
                TrackColor = C.Transparent,
                Padding = 5, //ToDo padding doesn't work
                ThumbHoverColor = new ColorDefinition(92, 92, 92),
                TrackHoverColor = new ColorDefinition(52, 52, 52)
            });
            comp.OnInitialized();
            return comp;
        });

        _scrollBarContainer.DataStore.Reset();

        Window.Ui.OpenElementStack.Push(_scrollBarContainer);
        scrollbar.Build(Window.Ui);
        Window.Ui.OpenElementStack.Pop();

        if (_scrollBarContainer.UiElement is null)
            return 0;

        var shadowParent = new FlexContainer
        {
            Id = default,
            Window = Window,
            // ComputedBounds = ComputedBounds,
            // PmAlign = EnumMAlign.FlexEnd,
            // Direction = EnumDir.Horizontal
        };

        shadowParent.AddChild(_scrollBarContainer.UiElement);
        // shadowParent.Layout();

        return _scrollBarContainer.UiElement.BoxSize.Width;
    }

    public override void PrepareLayout(Dir dir)
    {
        if (Info.GetMainSizeKind(dir) == SizeKind.Percentage)
        {
            FlexibleChildConfig = new FlexibleChildConfig
            {
                Percentage = Info.GetSizeInDirection(dir)
            };
        }
        else
        {
            FlexibleChildConfig = null;
        }
    }

    public override BoxSize Layout(BoxConstraint constraint)
    {
        TightenConstraint(ref constraint);

        BoxSize = FlexSizeCalculator.ComputeSize(constraint, Children, Info).ApplyConstraint(constraint);

        var actualSizeTakenUpByChildren = FlexPositionCalculator.ComputePosition(Children, BoxSize, Info);

        AbsoluteLayouter.LayoutAbsoluteChildren(Children, BoxSize);

        return BoxSize;
    }

    private void TightenConstraint(ref BoxConstraint constraint)
    {
        //width
        if (!constraint.IsWidthTight())
        {
            if(Info.WidthKind == SizeKind.Percentage && !float.IsInfinity(constraint.MaxWidth))
            {
                var width = constraint.MaxWidth * (0.01f * Info.WidthValue);
                constraint.MaxWidth = width;
                constraint.MinWidth = width;

                //todo check that we don't comply with the constraints
            }
            else if (Info.WidthKind == SizeKind.Pixel)
            {
                var width = Info.WidthValue;
                constraint.MaxWidth = width;
                constraint.MinWidth = width;

                //todo check that we don't comply with the constraints
            }
            else if (Info.WidthKind == SizeKind.Shrink)
            {
                constraint.MinWidth = Info.MinWidth;
            }
        }

        //height
        if (!constraint.IsHeightTight())
        {
            if(Info.HeightKind == SizeKind.Percentage && !float.IsInfinity(constraint.MaxHeight))
            {
                var height = constraint.MaxHeight * (0.01f * Info.HeightValue);
                constraint.MaxHeight = height;
                constraint.MinHeight = height;

                //todo check that we don't comply with the constraints
            }
            else if (Info.HeightKind == SizeKind.Pixel)
            {
                var height = Info.HeightValue;
                constraint.MaxHeight = height;
                constraint.MinHeight = height;

                //todo check that we don't comply with the constraints
            }
            else if (Info.HeightKind == SizeKind.Shrink)
            {
                constraint.MinHeight = Info.MinHeight;
            }
        }
    }
}

﻿using ImSharpUISample.ToolWindows;
using ImSharpUISample.UiElements;
using static SDL2.SDL;
using static ImSharpUISample.Ui;

namespace ImSharpUISample;

public abstract class UiComponent
{
    public virtual void OnInitialized()
    {

    }

    public abstract void Build();
}

public class Sidebar : UiComponent
{
    private SidebarSide _side;
    private int _windowWidth = 300;
    private ToolWindowDefinition? _selectedToolWindow;
    private readonly List<ToolWindowDefinition> _toolWindowDefinitions = new();
    private bool _isDragging = false;

    //ToDo free cursor with https://wiki.libsdl.org/SDL2/SDL_FreeCursor
    private IntPtr _resizeCursor = SDL_CreateSystemCursor(SDL_SystemCursor.SDL_SYSTEM_CURSOR_SIZEWE);
    private IntPtr _normalCursor = SDL_CreateSystemCursor(SDL_SystemCursor.SDL_SYSTEM_CURSOR_ARROW);

    public override void OnInitialized()
    {
        _selectedToolWindow = _toolWindowDefinitions.FirstOrDefault();
    }

    public override void Build()
    {
        //Sidebar
        DivStart(_side.ToString()).Color(43, 45, 48).Width(40).Padding(5).Gap(10);

            foreach (var toolWindowDefinition in _toolWindowDefinitions)
            {
                SidebarIcon(toolWindowDefinition);
            }

            //Window
            if (_selectedToolWindow is {} selectedToolWindow)
            {
                DivStart(out var toolWindow, _side.ToString()).ZIndex(1).Color(43, 45, 48).BorderWidth(1).BorderColor(0, 0, 0);

                    DivStart(out var dragZone).Absolute().Width(4).BlockHit();
                        if (_side == SidebarSide.Left)
                            dragZone.Absolute(right: -2);
                        else
                            dragZone.Absolute(left: -2);

                        HandleWindowResize(dragZone);
                    DivEnd();

                    toolWindow.Width(_windowWidth);

                    if (_side == SidebarSide.Left)
                        toolWindow.Absolute(left: 40);
                    else
                        toolWindow.Absolute(left: -_windowWidth);

                    var comp = (UiComponent)GetComponent(selectedToolWindow.WindowComponent, selectedToolWindow.Path);
                    comp.Build();
                DivEnd();
            }

        DivEnd();

        _toolWindowDefinitions.Clear();
    }

    public void ToolWindow<T>(string icon) where T : UiComponent
    {
        //ToDo remove memory allocation
        _toolWindowDefinitions.Add(new ToolWindowDefinition($"./Icons/{icon}.svg", typeof(T)));
    }

    public void Side(SidebarSide side)
    {
        _side = side;
    }

    private void HandleWindowResize(UiContainer dragZone)
    {
        if (dragZone.IsNewlyHovered)
            SDL_SetCursor(_resizeCursor);
        if (dragZone.IsNewlyUnHovered)
            SDL_SetCursor(_normalCursor);

        if (dragZone.IsHovered && Window.IsMouseButtonPressed(MouseButtonKind.Left))
        {
            _isDragging = true;
        }

        if (_isDragging && Window.IsMouseButtonReleased(MouseButtonKind.Left))
        {
            _isDragging = false;
        }

        if (_isDragging)
        {
            if (_side == SidebarSide.Left)
            {
                _windowWidth += (int)Window.MouseDelta.X;
            }
            else
            {
                _windowWidth -= (int)Window.MouseDelta.X;
            }
        }
    }

    private void SidebarIcon(ToolWindowDefinition toolWindowDefinition)
    {
        DivStart(out var toolbar, toolWindowDefinition.Path).Radius(5).Padding(3).Height(30).Color(0, 0, 0, 0);
            if (toolbar.Clicked)
            {
                if (toolWindowDefinition == _selectedToolWindow)
                {
                    _selectedToolWindow = null;
                }
                else
                {
                    _selectedToolWindow = toolWindowDefinition;
                }
            }

            if (toolWindowDefinition == _selectedToolWindow || toolbar.IsHovered)
            {
                toolbar.Color(78, 81, 87);
            }
            SvgImage(toolWindowDefinition.Path);
        DivEnd();
    }
}

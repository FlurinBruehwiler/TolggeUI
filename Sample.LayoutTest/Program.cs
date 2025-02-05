﻿using Flamui;
using Flamui.Components;
using Flamui.Drawing;

/*
 * Todo
 * - Scroll
 * - Text Selection/Editing etc
 * - Fix Border
 * - Fix Text
 * - Only rerender when changed
 */

var builder = FlamuiApp.CreateBuilder();

var app = builder.Build();

app.CreateWindow<LayoutTest>("Sample.LayoutTest", new FlamuiWindowOptions());
app.CreateWindow<LayoutTest>("Sample.LayoutTest2", new FlamuiWindowOptions());

app.Run();

public class LayoutTest : FlamuiComponent
{
    private ColorDefinition cc = new(43, 45, 48);
    private ColorDefinition c2 = new(30, 31, 34);
    private ColorDefinition c3 = new(75, 76, 79);

    private const string loremIpsum =
        "Lorem ipsum dolor sit amet, consectetur adipiscing elit.";

    private string input = "";

    public override void Build(Ui ui)
    {
        ui.CascadingValues.TextColor = new ColorDefinition(188, 190, 196);
        ui.CascadingValues.TextSize = 17;

        using (ui.Div().Color(cc).Padding(10).Gap(10))
        {
            using (var div = ui.Div().Color(c2).Rounded(2).Border(1, ColorPalette.BorderColor).Padding(20).Direction(Dir.Vertical).Gap(10))
            {
                /*
                ui.Defered(() =>
                {

                });

                ui.Canvas.DrawText();
                ui.Canvas.DrawRect();
                */

                ui.StyledInput(ref input);

                ui.Text(loremIpsum).Size(20).Multiline().Selectable();

                ui.Text(loremIpsum).Size(40).Multiline().Selectable();

                // ui.Text(loremIpsum).Size(40);
            }
        }
    }

}
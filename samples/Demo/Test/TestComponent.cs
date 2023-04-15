﻿using Demo.Test.RenderObject;
using Modern.WindowKit.Input;

namespace Demo.Test;

public class TestComponent : UiComponent
{
    private string _text = string.Empty;

    public override Div Render()
    {
        return new Div
            {
                new Img("./battery.svg"),
                new Txt(_text)
                    .Size(30)
                    .VAlign(TextAlign.Center)
            }.Color(40,40, 40)
            .OnKeyDown(key =>
            {
                if (key != Key.Back) return;
                if (_text.Length != 0)
                {
                    _text = _text.Remove(_text.Length - 1);
                }
            })
            .OnTextInput(s =>
            {
                _text += s;
            })
            .XAlign(XAlign.Center)
            .Padding(10)
            .Gap(10)
            .Dir(Dir.Row);
    }
}
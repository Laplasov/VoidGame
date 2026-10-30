using System.Collections;
using System.Diagnostics;
using Void.Engine;
using Void.Engine.Assets.Loaders.Fonts;
using Void.Engine.Coroutines.Routines.Time;
using Void.Engine.Graphics;
using Void.Engine.Inputs.Mouses;
using Void.Engine.Systems;
using VoidGame.Shaders;
using VoidGame.UI.UIElements;

namespace VoidGame.UI;

public class View : ViewBase
{
    private Panel _panel;
    private Panel _childPanel;

    private Panel _scrollTest;
    private ScrollView _scroller;

    private UiFill RedBoy;

    private HoverBox _hoverBox = new HoverBox().SetFill(Color.Yellow).SetOutlineSize(2f);

    private GradientStep _sky = new GradientStep(Color.DarkGray, new Vect2(0, 1));

    private static Label CreateText() =>
        new Label(Label.Lorem)
            .SetAlign(TextAlignment.TopLeft)
            .SetWrap(TextWrapMode.Word)
            .SetScale(2.5f)
            .SetSnap(false)
            .SetPadding(2f)
            .SetClip(false);

    private Label _text = CreateText();
    private Label _scrollText = CreateText();

    public View()
    {
        RedBoy = Color.Red.ToFill().Add(_sky);
        var grid = new Grid(rows: 6, columns: 6)
            .SetGap(2f)
            .SetPadding(3f)
            .Add(new Panel().AddChild(_text), row: 0, column: 0, rowSpan: 2, columnSpan: 2) // takes 4 cells
            .Add(new Panel(), row: 0, column: 2, columnSpan: 3)
            .Add(new Panel(), row: 1, column: 2, rowSpan: 2, columnSpan: 4);

        _panel = new Panel()
            .SetSize(new Vect2(0.5f, 0.5f))
            .SetDock(Anchor.BottomCenter)
            .SetFills(Color.Blue, Color.Black, Color.Cyan, RedBoy);
        //_childPanel = new Panel().SetSize(new Vect2(0.5f, 0.5f)).SetDock(Anchor.MiddleCenter);

        //_panel.AddChild(_childPanel);
        _panel.AddChild(grid);

        _scroller = new ScrollView();

        _scrollTest = new Panel().SetSize(new Vect2(0.5f, 0.5f)).SetDock(Anchor.TopCenter);

        _scrollTest.AddChild(_scroller);

        _scroller.ScrollSpace = new Vect2(100f, 300f);
        var panelFoScroll = new Panel()
            .SetSize(new Vect2(1f, 1f))
            .SetFills(Color.Blue, Color.Black, Color.Cyan, RedBoy);

        _scroller.AddChild(_scrollText);

        RootElement.AddChild(_panel);
        RootElement.AddChild(_scrollTest);
    }

    protected override void UpdateExtension(FrameTime frameTime)
    {
        _scroller.ScrollBy(new Vect2(0, Delta));
    }
}

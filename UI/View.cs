using System.Collections;
using Void.Engine;
using Void.Engine.Coroutines;
using Void.Engine.Coroutines.Routines.Time;
using Void.Engine.Graphics;
using Void.Engine.Systems;
using VoidGame.Shaders;
using VoidGame.UI.UIElements;

namespace VoidGame.UI;

public class View
{
    private Panel _panel;
    private Panel _childPanel;

    private UiFill RedBoy;

    private GradientStep _sky = new GradientStep(Color.DarkGray, new Vect2(0, 1));

    public View()
    {
        RedBoy = Color
            .Red.ToFill()
            .Add(_sky)
            .HorizontalGradient(Color.White)
            .SetRoundedCorners(new Vect4(0f, 15f, 15f, 15f));
        ;

        _panel = new Panel()
            .SetSize(new Vect2(0.5f, 0.5f))
            .SetDock(Anchor.MiddleCenter)
            .SetFills(Color.Blue, Color.Black, Color.Cyan, RedBoy);

        _childPanel = new Panel().SetSize(new Vect2(0.5f, 0.5f)).SetDock(Anchor.MiddleCenter);

        _panel.AddChild(_childPanel);
    }

    float _delta = 0f;

    public virtual void OnDraw(SpriteBatcher batcher, FrameTime frameTime)
    {
        var viewport = GameSettings.Instance.Viewport;

        // The root viewport acts as the "parent content" for the top-level UI element
        var rootRect = new Rect2(0, 0, viewport.X, viewport.Y);

        // 1. Calculate all layouts (cascades down to children)
        _panel.UpdateLayout(rootRect);

        _delta += frameTime.DeltaTime * 1.0f;
        if (_delta >= 1f)
            _delta = 0f;

        _sky.To = _sky.To.WithAlpha(_delta);

        // 2. Draw everything (cascades down to children)
        _panel.Draw(batcher, frameTime);
    }
}

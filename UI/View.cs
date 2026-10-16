using Void.Engine;
using Void.Engine.Graphics;
using Void.Engine.Systems;
using VoidGame.UI.UIElements;

namespace VoidGame.UI;

public class View
{
    private Panel _panel;
    private Panel _childPanel;

    public View()
    {
        _panel = new Panel().SetSize(new Vect2(0.5f, 0.5f)).SetDock(Anchor.BottomLeft);

        _childPanel = new Panel().SetSize(new Vect2(0.5f, 0.5f)).SetDock(Anchor.TopLeft);

        _panel.AddChild(_childPanel);
    }

    public virtual void OnDraw(SpriteBatcher batcher, FrameTime frameTime)
    {
        var viewport = GameSettings.Instance.Viewport;

        // The root viewport acts as the "parent content" for the top-level UI element
        var rootRect = new Rect2(0, 0, viewport.X, viewport.Y);

        // 1. Calculate all layouts (cascades down to children)
        _panel.UpdateLayout(rootRect);

        // 2. Draw everything (cascades down to children)
        _panel.Draw(batcher, frameTime);
    }
}

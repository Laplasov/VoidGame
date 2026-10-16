using Void.Engine;
using Void.Engine.Graphics;
using Void.Engine.Systems;
using VoidGame.UI.UiElements;

namespace VoidGame.UI;

public class View
{
    private Panel _panel = new Panel(GameSettings.Instance.Viewport)
        .SetSize(new Vect2(0.5f, 0.5f))
        .SetDock(Anchor.MiddleCenter);

    public virtual void OnDraw(SpriteBatcher batcher, FrameTime frameTime)
    {
        _panel.Draw(batcher, frameTime);
    }
}

using Void.Engine;
using Void.Engine.Graphics;
using Void.Engine.Inputs.Mouses;
using Void.Engine.Systems;
using VoidGame.UI.UIElements;

namespace VoidGame.UI;

public class ViewBase
{
    protected HoverBox HoverBoxElement = new HoverBox().SetFill(Color.Yellow).SetOutlineSize(2f);
    protected UIElement RootElement = new Root();
    protected float LimitDelta = 0f;
    protected float Delta = 0f;
    protected bool IsHoverBoxActive = true;

    protected ViewBase() { }

    public virtual void OnEnter() { }

    protected virtual void DeltaUpdate(FrameTime frameTime)
    {
        LimitDelta += frameTime.DeltaTime * 1.0f;
        if (LimitDelta >= 1f)
            LimitDelta = 0f;
        Delta += frameTime.DeltaTime * 1.0f;
    }

    public virtual void OnUpdate(FrameTime frameTime)
    {
        var viewport = GameSettings.Instance.Viewport;
        DeltaUpdate(frameTime);
        MouseState state = Mouse.GetState();
        var window = GameSettings.Instance.Window;

        Vect2 mousePos = new Vect2(
            state.Position.X * viewport.X / window.X,
            state.Position.Y * viewport.Y / window.Y
        );

        var rootRect = new Rect2(0, 0, viewport.X, viewport.Y);

        RootElement.UpdateLayout(rootRect);
        var hoverElement = RootElement.MouseUpdate(mousePos);
        HoverBoxElement.Target = hoverElement;
        UpdateExtension(frameTime);
    }

    protected virtual void UpdateExtension(FrameTime frameTime) { }

    public virtual void OnDraw(SpriteBatcher batcher, FrameTime frameTime)
    {
        RootElement.Draw(batcher, frameTime);
        if (IsHoverBoxActive)
            HoverBoxElement.Draw(batcher, frameTime);
    }
}

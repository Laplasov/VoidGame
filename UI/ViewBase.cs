using Void.Engine;
using Void.Engine.Graphics;
using Void.Engine.Inputs.Mouses;
using Void.Engine.Systems;
using VoidGame.Shaders;
using VoidGame.UI.UIElements;

namespace VoidGame.UI;

public class ViewBase
{
    protected TargetBox HoverBoxElement = new TargetBox().SetFill(Color.Yellow).SetOutlineSize(2f);
    protected TargetBox FocusBoxElement = new TargetBox().SetFill(Color.Blue).SetOutlineSize(2f);

    protected Root RootElement = new Root();
    protected float LimitDelta = 0f;
    protected float Delta = 0f;
    protected bool IsHoverBoxActive = true;
    protected bool IsFocusBoxActive = true;
    protected int CurrentFocusIndex = 0;
    protected int CurrentRootFocusIndex = 0;
    protected List<UiIcon> IconList = new List<UiIcon>();

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
        DeltaUpdate(frameTime);
        MouseState state = Mouse.GetState();
        TargetPack mouse = new TargetPack(Mouse.GetState());
        var viewport = GameSettings.Instance.Viewport;
        var window = GameSettings.Instance.Window;
        var rootRect = new Rect2(0, 0, viewport.X, viewport.Y);

        RootElement.UpdateLayout(rootRect);

        var hoverElement = RootElement.TargetUpdate(mouse);
        HoverBoxElement.Target = hoverElement;

        if (CurrentFocusIndex < CurrentRootFocusIndex)
        {
            CurrentFocusIndex = RootElement.MaxIndex();
            if (CurrentFocusIndex < CurrentRootFocusIndex)
                CurrentFocusIndex = CurrentRootFocusIndex;
        }

        UIElement.OnFocuse = RootElement.FindByIndex(CurrentFocusIndex);
        FocusBoxElement.Target = UIElement.OnFocuse;

        if (UIElement.OnFocuse == null)
            CurrentFocusIndex = CurrentRootFocusIndex;

        foreach (UiIcon icon in IconList)
            UpdateExtension(frameTime);
    }

    protected virtual void UpdateExtension(FrameTime frameTime) { }

    public virtual void OnDraw(SpriteBatcher batcher, FrameTime frameTime)
    {
        RootElement.Draw(batcher, frameTime);
        if (IsHoverBoxActive)
            HoverBoxElement.Draw(batcher, frameTime);
        if (IsFocusBoxActive && UIElement.OnFocuse != null)
            FocusBoxElement.Draw(batcher, frameTime);
    }
}

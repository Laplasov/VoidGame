using Void.Engine.Graphics;
using Void.Engine.Systems;

namespace VoidGame.UI.UIElements;

public class Root : UIElement<Root>
{
    public override void UpdateLayout(Rect2 parentContent)
    {
        Bounds = parentContent;
        Content = parentContent;

        foreach (var child in Children)
            child.UpdateLayout(Content);
    }

    public override void Draw(SpriteBatcher batcher, FrameTime frameTime)
    {
        foreach (var child in Children)
            child.Draw(batcher, frameTime);
    }

    public override UIElement TargetUpdate(TargetPack mouse)
    {
        var hit = base.TargetUpdate(mouse);
        return hit == this ? null : hit;
    }
}

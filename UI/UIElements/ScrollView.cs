using Void.Engine.Graphics;
using Void.Engine.Systems;
using VoidGame.Shaders;

namespace VoidGame.UI.UIElements;

public class ScrollView : UIElement<ScrollView>
{
    public ScrollView()
    {
        Size = new Vect2(1f, 1f);
        ScrollTool = new ScrollBar();
    }

    public ScrollBar ScrollTool { get; set; }

    public Vect2 Offset { get; set; }

    public void ScrollTo(Vect2 offset) => Offset = offset;

    public void ScrollBy(Vect2 delta) => ScrollTo(Offset + delta);

    public void SetThumbFill(UiFill fill) => ScrollTool.Thumb.Fill = fill;

    public void SetBarFill(UiFill fill) => ScrollTool.Fill = fill;

    public override bool Hoverable { get; set; } = false;

    public Vect2 ScrollSpace { get; set; }
    public bool AutoHeight { get; set; }

    public ScrollView SetAutoHeight(bool auto = true)
    {
        AutoHeight = auto;
        return this;
    }

    public override UIElement MouseUpdate(Vect2 mousePosition)
    {
        bool isInside =
            mousePosition.X >= Bounds.X
            && mousePosition.X <= Bounds.X + Bounds.Width
            && mousePosition.Y >= Bounds.Y
            && mousePosition.Y <= Bounds.Y + Bounds.Height;

        if (!Content.Contains(mousePosition))
            return isInside && Hoverable ? this : null;

        // The bar is drawn on top of the content, so it gets the first chance
        return ScrollTool.MouseUpdate(mousePosition) ?? base.MouseUpdate(mousePosition);
    }

    public override void UpdateLayout(Rect2 parentContent)
    {
        Bounds = new Rect2(
            parentContent.X + Position.X * parentContent.Width,
            parentContent.Y + Position.Y * parentContent.Height,
            Size.X * parentContent.Width,
            Size.Y * parentContent.Height
        );

        Content = new Rect2(Bounds.X, Bounds.Y, Bounds.Width, Bounds.Height);

        if (AutoHeight)
        {
            float spaceX = ScrollSpace.X > 0 ? ScrollSpace.X : Content.Width;
            float maxH = 0f;
            foreach (var child in Children)
            {
                if (child == ScrollTool)
                    continue;
                float h = child.MeasureHeight(spaceX);
                if (h > maxH)
                    maxH = h;
            }
            ScrollSpace = new Vect2(spaceX, maxH);
        }

        float maxX = MathF.Max(0f, ScrollSpace.X - Content.Width);
        float maxY = MathF.Max(0f, ScrollSpace.Y - Content.Height);

        // Clamp the single source of truth
        Offset = new Vect2(
            MathF.Min(MathF.Max(0f, Offset.X), maxX),
            MathF.Min(MathF.Max(0f, Offset.Y), maxY)
        );

        // Pass the scroll state to the ScrollTool (no binding, just a function call)
        ScrollTool.SetScrollState(maxY, Offset.Y);
        ScrollTool.VisibleRatio = ScrollSpace.Y > 0 ? Content.Height / ScrollSpace.Y : 1f;

        // Layout the ScrollTool
        ScrollTool.SetSize(new Vect2(0.05f, 1f)).SetPosition(new Vect2(0.95f, 0f));
        ScrollTool.UpdateLayout(Content);

        // Layout children
        var scrollSpace = new Rect2(
            Content.X - Offset.X,
            Content.Y - Offset.Y,
            ScrollSpace.X,
            ScrollSpace.Y
        );

        foreach (var child in Children)
        {
            if (child == ScrollTool)
                continue;
            child.UpdateLayout(scrollSpace);
        }
    }

    public override void Draw(SpriteBatcher batcher, FrameTime frameTime)
    {
        Clips.Push(batcher, Content);
        foreach (var child in Children)
        {
            if (child == ScrollTool)
                continue;
            child.Draw(batcher, frameTime);
        }
        Clips.Pop(batcher);

        ScrollTool.Draw(batcher, frameTime);
    }
}

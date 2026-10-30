using Void.Engine.Graphics;
using Void.Engine.Systems;
using VoidGame.Shaders;

namespace VoidGame.UI.UIElements;

public class Box : UIElement<Box>
{
    public UiFill Fill { get; set; } = Color.White;

    public override bool Hoverable { get; set; } = false;

    public Box SetFill(UiFill fill)
    {
        Fill = fill;
        return this;
    }

    public void SetBounds(Rect2 bounds) => Bounds = bounds;

    public void SetContent(Rect2 content) => Content = content;

    public override void UpdateLayout(Rect2 parentContent)
    {
        float w = Size.X * parentContent.Width;
        float h = Size.Y * parentContent.Height;
        float x = parentContent.X + Position.X * parentContent.Width;
        float y = parentContent.Y + Position.Y * parentContent.Height;

        Bounds = new Rect2(x, y, w, h);
        Content = Bounds; // A simple box has no padding/border

        foreach (var child in Children)
            child.UpdateLayout(Content);
    }

    public override void Draw(SpriteBatcher batcher, FrameTime frameTime)
    {
        FillRect(batcher, Bounds, Fill);

        foreach (var child in Children)
            child.Draw(batcher, frameTime);
    }

    protected static void FillRect(SpriteBatcher batcher, Rect2 rect, UiFill fill)
    {
        if (rect.Width <= 0 || rect.Height <= 0)
            return;

        var shader = FillPrograms.For(fill);

        batcher.SetShader(shader);
        shader.SetUniform("uCutRect", new Vect4(0, 0, 0, 0));
        shader.SetUniform("uCutRadii", new Vect4(0, 0, 0, 0));
        shader.SetUniform("uRect", new Vect4(rect.X, rect.Y, rect.Width, rect.Height));

        // Use the fill's custom corners if set, otherwise default (0,0,0,0)
        shader.SetUniform("uCornerRadii", fill.RoundedCorners ?? new Vect4(0, 0, 0, 0));
        shader.SetUniform("uFillRotation", fill.Rotation);
        shader.SetUniform("uFillOffset", fill.Offset);
        shader.SetUniform("uColor", fill.Color);

        FillPrograms.Apply(fill, shader);

        batcher.DrawBypassAtlas(
            UIElement.WhitePixel,
            rect,
            UIElement.WhitePixel.Bounds,
            fill.Color
        );

        batcher.Flush();
        batcher.ClearShader();
    }
}

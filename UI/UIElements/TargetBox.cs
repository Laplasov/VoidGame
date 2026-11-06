using Void.Engine.Graphics;
using Void.Engine.Systems;
using VoidGame.Shaders;

namespace VoidGame.UI.UIElements;

public class TargetBox : Box
{
    public float OutlineSize { get; set; } = 2f;
    public UIElement Target { get; set; }

    public TargetBox SetOutlineSize(float size)
    {
        OutlineSize = size;
        return this;
    }

    public new TargetBox SetFill(UiFill fill)
    {
        Fill = fill;
        return this;
    }

    public TargetBox SetTarget(UIElement target)
    {
        Target = target;
        return this;
    }

    public override void Draw(SpriteBatcher batcher, FrameTime frameTime)
    {
        if (Target == null)
            return;

        // 1. Calculate expanded bounds (additional size from bound)
        var expandedBounds = new Rect2(
            Target.Bounds.X - OutlineSize,
            Target.Bounds.Y - OutlineSize,
            Target.Bounds.Width + (OutlineSize * 2),
            Target.Bounds.Height + (OutlineSize * 2)
        );

        // 2. Use the Target's original bounds as the cutout
        var cutBounds = Target.Bounds;

        // 3. Draw with the cutout
        FillRectWithCut(batcher, expandedBounds, Fill, cutBounds);
    }

    private static void FillRectWithCut(SpriteBatcher batcher, Rect2 rect, UiFill fill, Rect2 toCut)
    {
        if (rect.Width <= 0 || rect.Height <= 0)
            return;

        var shader = FillPrograms.For(fill);

        batcher.SetShader(shader);

        // Provide the cut bounds to the shader!
        shader.SetUniform("uCutRect", new Vect4(toCut.X, toCut.Y, toCut.Width, toCut.Height));
        shader.SetUniform("uCutRadii", new Vect4(0, 0, 0, 0)); // Sharp inner edge
        shader.SetUniform("uRect", new Vect4(rect.X, rect.Y, rect.Width, rect.Height));
        shader.SetUniform("uCornerRadii", fill.RoundedCorners ?? new Vect4(0, 0, 0, 0));

        FillPrograms.Apply(fill, shader);

        batcher.DrawBypassAtlas(fill.Image.GetTexture, rect, fill.Image.GetSource, fill.Color);

        batcher.Flush();
        batcher.ClearShader();
    }
}

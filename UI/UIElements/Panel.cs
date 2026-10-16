using Void.Engine.Graphics;
using Void.Engine.Systems;
using VoidGame.Shaders;

namespace VoidGame.UI.UIElements;

public record struct PanelColors(
    Color ShadowColor,
    Color BorderColor,
    Color PaddingColor,
    Color ContentColor
);

public class Panel : UIElement<Panel>
{
    public float BorderWidth { get; set; } = 1f;
    public float PaddingWidth { get; set; } = 2f;
    public float ShadowSize { get; set; } = 0f;

    public PanelColors Colors { get; set; } =
        new PanelColors(Color.Blue, Color.Black, Color.Cyan, Color.Gray);

    public Vect2 ShadowOffset { get; set; } = new(2, 2);
    public Anchor Anchor { get; set; } = Anchor.TopLeft;

    public Panel SetColors(PanelColors colors)
    {
        Colors = colors;
        return this;
    }

    public Panel SetDock(Anchor anchor)
    {
        float x = anchor switch
        {
            Anchor.TopLeft or Anchor.MiddleLeft or Anchor.BottomLeft => 0f,
            Anchor.TopCenter or Anchor.MiddleCenter or Anchor.BottomCenter => 0.5f - Size.X / 2f,
            Anchor.TopRight or Anchor.MiddleRight or Anchor.BottomRight => 1f - Size.X,
            _ => 0f,
        };

        float y = anchor switch
        {
            Anchor.TopLeft or Anchor.TopCenter or Anchor.TopRight => 0f,
            Anchor.MiddleLeft or Anchor.MiddleCenter or Anchor.MiddleRight => 0.5f - Size.Y / 2f,
            Anchor.BottomLeft or Anchor.BottomCenter or Anchor.BottomRight => 1f - Size.Y,
            _ => 0f,
        };

        Position = new Vect2(x, y);
        return this;
    }

    public override void UpdateLayout(Rect2 parentContent)
    {
        float w = Size.X * parentContent.Width;
        float h = Size.Y * parentContent.Height;

        float x = Position.X * parentContent.Width;
        float y = Position.Y * parentContent.Height;

        // Apply Anchors relative to the parent
        switch (Anchor)
        {
            case Anchor.TopCenter:
            case Anchor.MiddleCenter:
            case Anchor.BottomCenter:
                x = (parentContent.Width - w) / 2f;
                break;
            case Anchor.TopRight:
            case Anchor.MiddleRight:
            case Anchor.BottomRight:
                x = parentContent.Width - w;
                break;
        }

        switch (Anchor)
        {
            case Anchor.MiddleLeft:
            case Anchor.MiddleCenter:
            case Anchor.MiddleRight:
                y = (parentContent.Height - h) / 2f;
                break;
            case Anchor.BottomLeft:
            case Anchor.BottomCenter:
            case Anchor.BottomRight:
                y = parentContent.Height - h;
                break;
        }

        // Offset by the parent's absolute position
        x += parentContent.X;
        y += parentContent.Y;

        Bounds = new Rect2(x, y, w, h);

        var bw = BorderWidth;
        var bwm = -bw * 2;
        var paddingRect = new Rect2(x + bw, y + bw, w + bwm, h + bwm);

        var pw = PaddingWidth;
        var pwm = -pw * 2;
        Content = new Rect2(
            paddingRect.X + pw,
            paddingRect.Y + pw,
            paddingRect.Width + pwm,
            paddingRect.Height + pwm
        );

        // CRITICAL: Cascade the layout update to all children, passing OUR Content area
        foreach (var child in Children)
        {
            child.UpdateLayout(this.Content);
        }
    }

    public override void Draw(SpriteBatcher batcher, FrameTime frameTime)
    {
        var borderRect = Bounds;
        var paddingRect = new Rect2(
            borderRect.X + BorderWidth,
            borderRect.Y + BorderWidth,
            borderRect.Width - (BorderWidth * 2),
            borderRect.Height - (BorderWidth * 2)
        );

        var shadowRect = new Rect2(
            borderRect.X - ShadowSize + ShadowOffset.X,
            borderRect.Y - ShadowSize + ShadowOffset.Y,
            borderRect.Width + (ShadowSize * 2),
            borderRect.Height + (ShadowSize * 2)
        );

        Fill(batcher, shadowRect, Colors.ShadowColor, paddingRect);
        //Fill(batcher, borderRect, Colors.BorderColor, paddingRect);
        Fill(batcher, paddingRect, Colors.PaddingColor, Content);
        //Fill(batcher, Content, Colors.ContentColor, Rect2.Empty);

        // Draw all children
        foreach (var child in Children)
        {
            child.Draw(batcher, frameTime);
        }
    }

    private static void Fill(SpriteBatcher batcher, Rect2 rect, Color color, Rect2 toCut)
    {
        if (rect.Width <= 0 || rect.Height <= 0)
            return;

        float cutX = toCut.X;
        float cutY = toCut.Y;
        float cutW = toCut.Width;
        float cutH = toCut.Height;

        batcher.SetShader(UiShaders.Cutout);
        UiShaders.Cutout.SetUniform("uCutRect", new Vect4(cutX, cutY, cutW, cutH));
        UiShaders.Cutout.SetUniform("uColor", color);
        batcher.DrawBypassAtlas(WhitePixel, rect, WhitePixel.Bounds, color);

        batcher.Flush();
        batcher.ClearShader();
    }
}

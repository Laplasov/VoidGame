using Void.Engine.Graphics;
using Void.Engine.Systems;
using VoidGame.Shaders;

namespace VoidGame.UI.UiElements;

public class Panel(Vect2 ParrentSize) : UiElement<Panel>
{
    public float BorderWidth { get; set; } = 1f;
    public float PaddingWidth { get; set; } = 2f;
    public float ShadowSize { get; set; } = 0f;
    public Vect2 ShadowOffset { get; set; } = new(2, 2);

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

    public void Draw(SpriteBatcher batcher, FrameTime frameTime)
    {
        var size = ParrentSize;

        float x = Position.X * size.X;
        float y = Position.Y * size.Y;
        float w = Size.X * size.X;
        float h = Size.Y * size.Y;

        var bw = BorderWidth;
        var bwm = -bw * 2;
        var borderRect = new Rect2(x, y, w, h);
        var paddingRect = new Rect2(x + bw, y + bw, w + bwm, h + bwm);

        var shadowRect = new Rect2(
            borderRect.X - ShadowSize + ShadowOffset.X,
            borderRect.Y - ShadowSize + ShadowOffset.Y,
            borderRect.Width + (ShadowSize * 2),
            borderRect.Height + (ShadowSize * 2)
        );

        var pw = PaddingWidth;
        var pwm = -pw * 2;
        Content = new Rect2(
            paddingRect.X + pw,
            paddingRect.Y + pw,
            paddingRect.Width + pwm,
            paddingRect.Height + pwm
        );

        Fill(batcher, shadowRect, Color.DarkBlue, paddingRect);
        Fill(batcher, borderRect, Color.Black, paddingRect);
        Fill(batcher, paddingRect, Color.White, Content);
        Fill(batcher, Content, Color.Red, Rect2.Empty);
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

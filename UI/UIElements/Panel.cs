using Void.Engine.Graphics;
using Void.Engine.Systems;
using VoidGame.Shaders;

namespace VoidGame.UI.UIElements;

public class PanelFills
{
    public UiFill ShadowColor { get; set; }
    public UiFill BorderColor { get; set; }
    public UiFill PaddingColor { get; set; }
    public UiFill ContentColor { get; set; }
}

public class PanelProportions
{
    public float BorderWidth { get; set; }
    public float PaddingWidth { get; set; }
    public float ShadowSize { get; set; }
    public Vect2 ShadowOffset { get; set; }
    public Vect4 RoundedCorners { get; set; }
}

public class Panel : UIElement<Panel>
{
    public Rect2 OuterBounds { get; private set; }

    public PanelFills Fills { get; set; } =
        new PanelFills()
        {
            ShadowColor = Color.Blue,
            BorderColor = Color.Black,
            PaddingColor = Color.Cyan,
            ContentColor = Color.Gray,
        };

    public PanelProportions Prop { get; set; } =
        new PanelProportions()
        {
            BorderWidth = 1f,
            PaddingWidth = 2f,
            ShadowSize = 0f,
            ShadowOffset = new(2, 2),
            RoundedCorners = new(0f, 0f, 0f, 0f),
        };

    public Panel SetFills(UiFill shadow, UiFill border, UiFill padding, UiFill content)
    {
        Fills.ShadowColor = shadow;
        Fills.BorderColor = border;
        Fills.PaddingColor = padding;
        Fills.ContentColor = content;
        return this;
    }

    public Panel SetProportions(
        float borderWidth,
        float paddingWidth,
        float shadowSize,
        Vect2 shadowOffset,
        Vect4 roundedCorners
    )
    {
        Prop.BorderWidth = borderWidth;
        Prop.PaddingWidth = paddingWidth;
        Prop.ShadowSize = shadowSize;
        Prop.ShadowOffset = shadowOffset;
        Prop.RoundedCorners = roundedCorners;
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

    private Vect4 ShadowOverhang()
    {
        return new Vect4(
            MathF.Max(0, Prop.ShadowSize - Prop.ShadowOffset.X),
            MathF.Max(0, Prop.ShadowSize - Prop.ShadowOffset.Y),
            MathF.Max(0, Prop.ShadowSize + Prop.ShadowOffset.X),
            MathF.Max(0, Prop.ShadowSize + Prop.ShadowOffset.Y)
        );
    }

    public override void UpdateLayout(Rect2 parentContent)
    {
        float w = Size.X * parentContent.Width;
        float h = Size.Y * parentContent.Height;

        // Position is a percentage of the parent's Content, offset by its absolute origin
        float x = parentContent.X + Position.X * parentContent.Width;
        float y = parentContent.Y + Position.Y * parentContent.Height;

        OuterBounds = new Rect2(x, y, w, h);

        var o = ShadowOverhang();
        float sox = x + o.X;
        float soby = y + o.Y;
        float sobw = MathF.Max(0, w - o.X - o.Z);
        float sobh = MathF.Max(0, h - o.Y - o.W);
        Bounds = new Rect2(sox, soby, sobw, sobh);

        var bw = Prop.BorderWidth;
        //var bwm = -bw * 2;
        //var paddingRect = new Rect2(x + bw, y + bw, w + bwm, h + bwm);
        var paddingRect = new Rect2(sox + bw, soby + bw, sobw - bw * 2, sobh - bw * 2);

        var pw = Prop.PaddingWidth;
        var pwm = -pw * 2;
        Content = new Rect2(
            paddingRect.X + pw,
            paddingRect.Y + pw,
            paddingRect.Width + pwm,
            paddingRect.Height + pwm
        );

        foreach (var child in Children)
        {
            child.UpdateLayout(this.Content);
        }
    }

    public override void Draw(SpriteBatcher batcher, FrameTime frameTime)
    {
        var borderRect = Bounds;
        var paddingRect = new Rect2(
            borderRect.X + Prop.BorderWidth,
            borderRect.Y + Prop.BorderWidth,
            borderRect.Width - (Prop.BorderWidth * 2),
            borderRect.Height - (Prop.BorderWidth * 2)
        );

        var shadowRect = new Rect2(
            borderRect.X - Prop.ShadowSize + Prop.ShadowOffset.X,
            borderRect.Y - Prop.ShadowSize + Prop.ShadowOffset.Y,
            borderRect.Width + (Prop.ShadowSize * 2),
            borderRect.Height + (Prop.ShadowSize * 2)
        );

        var corners = Prop.RoundedCorners;

        var borderRadii = ResolveCorners(Fills.BorderColor, Prop.RoundedCorners);
        var paddingRadii = ResolveCorners(Fills.PaddingColor, Inset(borderRadii, Prop.BorderWidth));
        var contentRadii = ResolveCorners(
            Fills.ContentColor,
            Inset(paddingRadii, Prop.PaddingWidth)
        );
        var shadowRadii = ResolveCorners(Fills.ShadowColor, borderRadii);

        //   rect         fill                 cutRect       outer radii    cut radii
        Fill(batcher, shadowRect, Fills.ShadowColor, paddingRect, shadowRadii, paddingRadii);
        Fill(batcher, borderRect, Fills.BorderColor, paddingRect, borderRadii, paddingRadii);
        Fill(batcher, paddingRect, Fills.PaddingColor, Content, paddingRadii, contentRadii);
        Fill(batcher, Content, Fills.ContentColor, Rect2.Empty, contentRadii, default); // Draw all children
        foreach (var child in Children)
        {
            child.Draw(batcher, frameTime);
        }
    }

    private static Vect4 ResolveCorners(UiFill fill, Vect4 fallback) =>
        fill.RoundedCorners ?? fallback;

    private static Vect4 Inset(Vect4 r, float d) =>
        new(
            MathF.Max(0, r.X - d),
            MathF.Max(0, r.Y - d),
            MathF.Max(0, r.Z - d),
            MathF.Max(0, r.W - d)
        );

    private static void Fill(
        SpriteBatcher batcher,
        Rect2 rect,
        UiFill fill,
        Rect2 toCut,
        Vect4 corners,
        Vect4 cutCorners
    )
    {
        if (rect.Width <= 0 || rect.Height <= 0)
            return;

        var shader = FillPrograms.For(fill);

        batcher.SetShader(shader);
        shader.SetUniform("uCutRect", new Vect4(toCut.X, toCut.Y, toCut.Width, toCut.Height));
        shader.SetUniform("uCutCornerRadii", cutCorners); // new
        shader.SetUniform("uRect", new Vect4(rect.X, rect.Y, rect.Width, rect.Height));
        shader.SetUniform("uCornerRadii", corners);

        FillPrograms.Apply(fill, shader);

        batcher.DrawBypassAtlas(fill.Image.GetTexture, rect, fill.Image.GetSource, fill.Color);

        batcher.Flush();
        batcher.ClearShader();
    }
}

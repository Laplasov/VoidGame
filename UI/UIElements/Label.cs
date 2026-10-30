using Void.Engine;
using Void.Engine.Assets.Loaders.Fonts;
using Void.Engine.Graphics;
using Void.Engine.Systems;

namespace VoidGame.UI.UIElements;

/// <summary>How a Label decides its text scale.</summary>
public enum LabelSizing
{
    /// <summary>Use <see cref="Label.Scale"/> directly.</summary>
    Fixed,

    /// <summary>One line of text takes <see cref="Label.RelativeSize"/> of the container's height.</summary>
    Relative,

    /// <summary>Largest scale at which the whole (wrapped) text fits inside the container.</summary>
    Fit,
}

/// <summary>
/// Draws text inside its parent's Content rect.
/// Position/Size are fractions of the parent's content, same as Panel.
/// Defaults to Size (1,1), so a bare Label fills the parent's content.
/// Alignment, wrapping and drawing are done by the engine's SpriteBatcher.DrawText.
/// </summary>
/// <remarks>
/// Final scale = (scale from <see cref="Sizing"/>) + <see cref="ScaleOffset"/>,
/// then rounded to a whole number when <see cref="SnapScale"/> is on (best for pixel fonts).
/// Explicit '\n' always starts a new line; word wrap happens on top of that.
/// </remarks>
public class Label : UIElement<Label>
{
    private readonly record struct SizeKey(
        string Text,
        Font Font,
        float Width,
        float Height,
        TextWrapMode Wrap,
        LabelSizing Sizing,
        float Scale,
        float Relative,
        float Offset,
        bool Snap,
        float Max,
        float Min
    );

    private SizeKey _key;
    private bool _hasKey;
    private float _scale = 1f;

    public override bool Hoverable { get; set; } = false;

    public Label(string text = "", Font font = null)
    {
        Text = text ?? "";
        Font = font;
        Size = new Vect2(1f, 1f);
    }

    public Label(Font font, string text = "")
        : this(text, font) { }

    /// <summary>Text to draw. Null or empty draws nothing.</summary>
    public string Text { get; set; }

    /// <summary>Font to use. When null, the engine's built-in default font (Game.Instance.Font) is used.</summary>
    public Font Font { get; set; }

    public Color Color { get; set; } = Color.White;

    /// <summary>Where the text block sits inside the label's bounds.</summary>
    public TextAlignment Align { get; set; } = TextAlignment.TopLeft;

    /// <summary>Word (default) wraps at spaces, Character wraps anywhere, None only breaks on '\n'.</summary>
    public TextWrapMode Wrap { get; set; } = TextWrapMode.Word;

    /// <summary>How the base scale is chosen.</summary>
    public LabelSizing Sizing { get; set; } = LabelSizing.Fixed;

    /// <summary>Scale used when <see cref="Sizing"/> is Fixed. 1 is the font's native pixel size.</summary>
    public float Scale { get; set; } = 1f;

    /// <summary>For Relative sizing: fraction of the container height one line should take (0.1 = 10%).</summary>
    public float RelativeSize { get; set; } = 0.1f;

    /// <summary>Added to the computed scale. Positive grows the text, negative shrinks it.</summary>
    public float ScaleOffset { get; set; }

    /// <summary>Round the final scale to a whole number (min 1). Keeps pixel fonts crisp.</summary>
    public bool SnapScale { get; set; } = true;

    /// <summary>Upper limit for Fit sizing.</summary>
    public float MaxScale { get; set; } = 16f;

    /// <summary>Lower limit for Fit sizing when <see cref="SnapScale"/> is off.</summary>
    public float MinScale { get; set; } = 0.5f;

    /// <summary>Clip to the label's bounds (catches a single word wider than the container).</summary>
    public bool Clip { get; set; } = true;

    /// <summary>The scale used on the last layout, after sizing, offset and snapping.</summary>
    public float EffectiveScale => _scale;

    public Vect4 Padding { get; set; } = new Vect4(0, 0, 0, 0);

    public Label SetText(string text)
    {
        Text = text;
        return this;
    }

    public Label SetFont(Font font)
    {
        Font = font;
        return this;
    }

    public Label SetColor(Color color)
    {
        Color = color;
        return this;
    }

    public Label SetAlign(TextAlignment align)
    {
        Align = align;
        return this;
    }

    public Label SetWrap(TextWrapMode wrap = TextWrapMode.Word)
    {
        Wrap = wrap;
        return this;
    }

    public Label SetClip(bool clip = true)
    {
        Clip = clip;
        return this;
    }

    public Label SetSnap(bool snap = true)
    {
        SnapScale = snap;
        return this;
    }

    public Label SetScaleOffset(float offset)
    {
        ScaleOffset = offset;
        return this;
    }

    /// <summary>Fixed scale.</summary>
    public Label SetScale(float scale)
    {
        Sizing = LabelSizing.Fixed;
        Scale = scale;
        return this;
    }

    /// <summary>One line = this fraction of the container height.</summary>
    public Label SetRelativeSize(float fraction)
    {
        Sizing = LabelSizing.Relative;
        RelativeSize = fraction;
        return this;
    }

    /// <summary>Biggest scale that still fits the container.</summary>
    public Label SetFit(float maxScale = 16f)
    {
        Sizing = LabelSizing.Fit;
        MaxScale = maxScale;
        return this;
    }

    public Label SetPadding(float padding)
    {
        Padding = new Vect4(padding, padding, padding, padding);
        return this;
    }

    public Label SetPadding(float x, float y, float z, float w)
    {
        Padding = new Vect4(x, y, z, w);
        return this;
    }

    public override void UpdateLayout(Rect2 parentContent)
    {
        float w = Size.X * parentContent.Width;
        float h = Size.Y * parentContent.Height;
        float x = parentContent.X + Position.X * parentContent.Width;
        float y = parentContent.Y + Position.Y * parentContent.Height;

        Bounds = new Rect2(x, y, w, h);
        Content = new Rect2(
            Bounds.X + Padding.X,
            Bounds.Y + Padding.Y,
            Bounds.Width - Padding.X - Padding.Z,
            Bounds.Height - Padding.Y - Padding.W
        );

        Font font = Font ?? Game.Instance?.Font;
        if (font is not null)
        {
            float cw = Content.Width;
            float ch = Content.Height;
            // Recompute only when something that affects the scale changed
            var key = new SizeKey(
                Text,
                font,
                w,
                h,
                Wrap,
                Sizing,
                Scale,
                RelativeSize,
                ScaleOffset,
                SnapScale,
                MaxScale,
                MinScale
            );
            if (!_hasKey || key != _key)
            {
                _scale = ComputeScale(font, cw, ch);
                _key = key;
                _hasKey = true;
            }
        }

        foreach (var child in Children)
            child.UpdateLayout(Content);
    }

    public override void Draw(SpriteBatcher batcher, FrameTime frameTime)
    {
        Font font = Font ?? Game.Instance?.Font;

        if (font is not null && !string.IsNullOrEmpty(Text))
        {
            if (Clip)
                Clips.Push(batcher, Content);

            batcher.DrawText(font, Text, Content, Color, new Vect2(_scale, _scale), Align, Wrap);

            if (Clip)
                Clips.Pop(batcher);
        }

        foreach (var child in Children)
            child.Draw(batcher, frameTime);
    }

    private float ComputeScale(Font font, float width, float height)
    {
        float pitch = font.LineHeight + font.LineSpacing;

        float baseScale = Sizing switch
        {
            LabelSizing.Relative => pitch > 0f ? height * RelativeSize / pitch : 1f,
            LabelSizing.Fit => FitScale(font, width, height, pitch),
            _ => Scale,
        };

        float s = baseScale + ScaleOffset;
        return FinalScale(baseScale);
    }

    private float FinalScale(float baseScale)
    {
        float s = baseScale + ScaleOffset;
        return SnapScale ? MathF.Max(1f, MathF.Round(s)) : MathF.Max(0.1f, s);
    }

    private float FitScale(Font font, float width, float height, float pitch)
    {
        if (string.IsNullOrEmpty(Text) || pitch <= 0f || width <= 0f || height <= 0f)
            return 1f;

        bool Fits(float s)
        {
            // Wrapping scales linearly with size, so measure in unscaled font units.
            var (lines, maxLineWidth) = MeasureLayout(font, Text, width / s, Wrap);
            return lines * pitch * s <= height && maxLineWidth * s <= width;
        }

        if (SnapScale)
        {
            for (int s = (int)MathF.Floor(MaxScale); s > 1; s--)
            {
                if (Fits(s))
                    return s;
            }
            return 1f;
        }

        float lo = MinScale;
        float hi = MaxScale;
        if (Fits(hi))
            return hi;
        if (!Fits(lo))
            return lo;

        for (int i = 0; i < 20; i++)
        {
            float mid = (lo + hi) * 0.5f;
            if (Fits(mid))
                lo = mid;
            else
                hi = mid;
        }
        return lo;
    }

    // Mirrors the engine's wrapping (SpriteBatcher.DrawText) so Fit predicts the real layout.
    private static (int Lines, float MaxWidth) MeasureLayout(
        Font font,
        string text,
        float maxWidth,
        TextWrapMode mode
    )
    {
        int lines = 0;
        float maxW = 0f;

        foreach (string paragraph in text.Replace("\r", "").Split('\n'))
        {
            if (mode == TextWrapMode.None)
            {
                lines++;
                maxW = MathF.Max(maxW, font.MeasureWidth(paragraph));
            }
            else if (mode == TextWrapMode.Word)
            {
                float space = Advance(font, ' ');
                float cursor = 0f;
                float visible = 0f;
                bool hasLine = false;

                foreach (string word in paragraph.Split(' '))
                {
                    if (word.Length == 0)
                        continue;

                    float wordWidth = font.MeasureWidth(word);

                    if (hasLine && cursor + wordWidth > maxWidth)
                    {
                        lines++;
                        maxW = MathF.Max(maxW, visible);
                        cursor = 0f;
                        visible = 0f;
                    }

                    hasLine = true;
                    visible = cursor + wordWidth;
                    cursor = visible + space;
                }

                if (hasLine || string.IsNullOrWhiteSpace(paragraph))
                {
                    lines++;
                    maxW = MathF.Max(maxW, visible);
                }
            }
            else
            {
                if (paragraph.Length == 0)
                {
                    lines++;
                    continue;
                }

                float lineWidth = 0f;
                foreach (char c in paragraph)
                {
                    float cw = Advance(font, c);
                    if (lineWidth > 0f && lineWidth + cw > maxWidth)
                    {
                        lines++;
                        maxW = MathF.Max(maxW, lineWidth);
                        lineWidth = 0f;
                    }
                    lineWidth += cw;
                }

                lines++;
                maxW = MathF.Max(maxW, lineWidth);
            }
        }

        return (lines, maxW);
    }

    private static float Advance(Font font, char c) =>
        c == '\t' ? font.GetGlyph(' ').Advance * 4f : font.GetGlyph(c).Advance;

    public override float MeasureHeight(float parentWidth)
    {
        if (Sizing != LabelSizing.Fixed)
            return -1f; // Relative and Fit depend on the container height

        Font font = Font ?? Game.Instance?.Font;
        if (font is null)
            return -1f;

        float vertical = Padding.Y + Padding.W;
        if (string.IsNullOrEmpty(Text))
            return vertical;

        float width = Size.X * parentWidth - Padding.X - Padding.Z;
        float s = FinalScale(Scale);
        float pitch = font.LineHeight + font.LineSpacing;

        var (lines, _) = MeasureLayout(font, Text, width / s, Wrap);
        return lines * pitch * s + vertical;
    }

    public static string Lorem =
        "lorem ipsum dolor sit amet consectetur"
        + "adipiscing elit fuga sed eiusmod eligendi duis"
        + "nulla velit maxime aute at odio ducimus et repellendus"
        + "deleniti anim proident qui excepteur elit quis commodo"
        + "omnis dolore temporibus quo voluptas accusamus exercitation"
        + "ut magna cumque cupiditate non sunt magna sed sint ad ullamco in ut";
}
